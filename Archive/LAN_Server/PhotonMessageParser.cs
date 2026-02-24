using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Binary message parser for Photon protocol
    /// Deserializes incoming packets into operation/event structures
    /// </summary>
    public class PhotonMessageParser
    {
        /// <summary>
        /// Parses a raw byte buffer into a Photon message
        /// Handles Photon UDP frame headers (FF-FF prefix)
        /// </summary>
        public static PhotonMessage? ParseMessage(byte[] buffer, int length)
        {
            if (length < 1)
                return null;

            try
            {
                using (var reader = new BinaryReader(new MemoryStream(buffer, 0, length)))
                {
                    int startPos = 0;
                    
                    // Check for Photon UDP frame header (FF-FF)
                    if (buffer[0] == 0xFF && length > 1 && buffer[1] == 0xFF)
                    {
                        // Skip Photon UDP frame header structure:
                        // 0-1: FF FF (frame marker)
                        // 2-3: Frame ID (ushort)
                        // 4-7: Payload length (uint)
                        // 8-9: Timestamp (ushort)
                        // 10-11: Sequence (ushort)
                        // 12: Flags/Reliability
                        // 13: ??? (FF seems to be a marker)
                        // 14+: Actual operation/parameters
                        
                        if (length < 15)
                            return null; // Frame header incomplete
                        
                        startPos = 14;
                        reader.BaseStream.Seek(startPos, SeekOrigin.Begin);
                        Console.WriteLine("   📋 Frame header detected, skipping to byte 14");
                    }
                    
                    byte opCode = reader.ReadByte();
                    Console.WriteLine("   📋 OpCode at position {0}: 0x{1:X2}", startPos, opCode);
                    
                    var message = new PhotonMessage { OperationCode = opCode };

                    // Read parameters (key-value pairs)
                    while (reader.BaseStream.Position < length)
                    {
                        byte paramCode = reader.ReadByte();
                        object? value = ReadValue(reader);
                        
                        if (value != null)
                        {
                            message.Parameters[paramCode] = value;
                        }
                    }

                    return message;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error parsing message: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Reads a typed value from the stream
        /// Type is inferred from the TypeCode prefix
        /// </summary>
        private static object? ReadValue(BinaryReader reader)
        {
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return null;

            byte typeCode = reader.ReadByte();

            return typeCode switch
            {
                PhotonProtocol.TypeCode.Null => null,
                PhotonProtocol.TypeCode.Bool => reader.ReadBoolean(),
                PhotonProtocol.TypeCode.Byte => reader.ReadByte(),
                PhotonProtocol.TypeCode.Int16 => reader.ReadInt16(),
                PhotonProtocol.TypeCode.Int => reader.ReadInt32(),
                PhotonProtocol.TypeCode.Long => reader.ReadInt64(),
                PhotonProtocol.TypeCode.Float => reader.ReadSingle(),
                PhotonProtocol.TypeCode.Double => reader.ReadDouble(),
                PhotonProtocol.TypeCode.String => ReadString(reader),
                PhotonProtocol.TypeCode.ByteArray => ReadByteArray(reader),
                PhotonProtocol.TypeCode.Vector2 => ReadVector2(reader),
                PhotonProtocol.TypeCode.Vector3 => ReadVector3(reader),
                PhotonProtocol.TypeCode.Quaternion => ReadQuaternion(reader),
                PhotonProtocol.TypeCode.Hashtable => ReadHashtable(reader),
                PhotonProtocol.TypeCode.Dictionary => ReadDictionary(reader),
                _ => null
            };
        }

        private static string ReadString(BinaryReader reader)
        {
            ushort length = reader.ReadUInt16();
            if (length == 0)
                return string.Empty;
            
            byte[] data = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(data);
        }

        private static byte[] ReadByteArray(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            if (length == 0)
                return Array.Empty<byte>();
            
            return reader.ReadBytes((int)length);
        }

        private static (float x, float y) ReadVector2(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return (x, y);
        }

        private static (float x, float y, float z) ReadVector3(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return (x, y, z);
        }

        private static (float x, float y, float z, float w) ReadQuaternion(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            return (x, y, z, w);
        }

        private static Hashtable ReadHashtable(BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            var table = new Hashtable();

            for (int i = 0; i < count; i++)
            {
                // Read key
                object? key = ReadValue(reader);
                
                // Read value
                object? value = ReadValue(reader);
                
                if (key != null && value != null)
                {
                    table[key] = value;
                }
            }

            return table;
        }

        private static Dictionary<object, object> ReadDictionary(BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            var dict = new Dictionary<object, object>();

            for (int i = 0; i < count; i++)
            {
                // Read key
                object? key = ReadValue(reader);
                
                // Read value
                object? value = ReadValue(reader);
                
                if (key != null && value != null)
                {
                    dict[key] = value;
                }
            }

            return dict;
        }
    }

    /// <summary>
    /// Represents a parsed Photon message
    /// </summary>
    public class PhotonMessage
    {
        public byte OperationCode { get; set; }
        public Dictionary<byte, object> Parameters { get; set; } = new();

        public override string ToString()
        {
            return $"OpCode: 0x{OperationCode:X2}, Params: {Parameters.Count}";
        }

        public object? GetParameter(byte code)
        {
            Parameters.TryGetValue(code, out var value);
            return value;
        }

        public string? GetString(byte code)
        {
            var value = GetParameter(code);
            return value as string;
        }

        public int? GetInt(byte code)
        {
            var value = GetParameter(code);
            if (value is int i)
                return i;
            return null;
        }

        public bool? GetBool(byte code)
        {
            var value = GetParameter(code);
            if (value is bool b)
                return b;
            return null;
        }

        public Hashtable? GetHashtable(byte code)
        {
            var value = GetParameter(code);
            return value as Hashtable;
        }
    }
}
