using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Serializes game objects back into Photon binary protocol
    /// Used for sending responses and events to clients
    /// </summary>
    public class PhotonMessageSerializer
    {
        private static ushort _frameIdCounter = 0;

        /// <summary>
        /// Wrap a Photon operation in the UDP frame header
        /// </summary>
        public static byte[] WrapInPhotonFrame(byte[] operationData)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Photon UDP frame header
                writer.Write((byte)0xFF);                    // Marker
                writer.Write((byte)0xFF);                    // Marker
                writer.Write((ushort)_frameIdCounter++);     // Frame ID
                writer.Write((uint)operationData.Length);    // Payload length
                writer.Write((ushort)0);                     // Timestamp
                writer.Write((ushort)0);                     // Sequence
                writer.Write((byte)0);                       // Flags
                writer.Write((byte)0xFF);                    // Unknown marker (from client packets)
                
                // Append the actual operation data
                writer.Write(operationData);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serialize an event to send to clients
        /// </summary>
        public static byte[] SerializeEvent(byte eventCode, Dictionary<byte, object> parameters)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write event code
                writer.Write(eventCode);

                // Write parameters
                foreach (var kvp in parameters)
                {
                    writer.Write(kvp.Key);  // Parameter code
                    WriteValue(writer, kvp.Value);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serialize a single typed value
        /// </summary>
        private static void WriteValue(BinaryWriter writer, object? value)
        {
            if (value == null)
            {
                writer.Write(PhotonProtocol.TypeCode.Null);
                return;
            }

            switch (value)
            {
                case bool b:
                    writer.Write(PhotonProtocol.TypeCode.Bool);
                    writer.Write(b);
                    break;

                case byte b:
                    writer.Write(PhotonProtocol.TypeCode.Byte);
                    writer.Write(b);
                    break;

                case short s:
                    writer.Write(PhotonProtocol.TypeCode.Int16);
                    writer.Write(s);
                    break;

                case int i:
                    writer.Write(PhotonProtocol.TypeCode.Int);
                    writer.Write(i);
                    break;

                case long l:
                    writer.Write(PhotonProtocol.TypeCode.Long);
                    writer.Write(l);
                    break;

                case float f:
                    writer.Write(PhotonProtocol.TypeCode.Float);
                    writer.Write(f);
                    break;

                case double d:
                    writer.Write(PhotonProtocol.TypeCode.Double);
                    writer.Write(d);
                    break;

                case string s:
                    writer.Write(PhotonProtocol.TypeCode.String);
                    WriteString(writer, s);
                    break;

                case byte[] arr:
                    writer.Write(PhotonProtocol.TypeCode.ByteArray);
                    WriteByteArray(writer, arr);
                    break;

                case Hashtable table:
                    writer.Write(PhotonProtocol.TypeCode.Hashtable);
                    WriteHashtable(writer, table);
                    break;

                case Dictionary<object, object> dict:
                    writer.Write(PhotonProtocol.TypeCode.Dictionary);
                    WriteDictionary(writer, dict);
                    break;

                case object[] arr:
                    // Write arrays as count + individual items (Photon format)
                    writer.Write((byte)0x00); // Use custom marker for array
                    writer.Write((uint)arr.Length);
                    foreach (var item in arr)
                    {
                        WriteValue(writer, item);
                    }
                    break;

                default:
                    Console.WriteLine("⚠️  Unknown type for serialization: {0}", value.GetType().Name);
                    writer.Write(PhotonProtocol.TypeCode.Null);
                    break;
            }
        }

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)data.Length);
            writer.Write(data);
        }

        private static void WriteByteArray(BinaryWriter writer, byte[] value)
        {
            writer.Write((uint)value.Length);
            writer.Write(value);
        }

        private static void WriteHashtable(BinaryWriter writer, Hashtable table)
        {
            writer.Write((ushort)table.Count);

            foreach (DictionaryEntry entry in table)
            {
                WriteValue(writer, entry.Key);
                WriteValue(writer, entry.Value);
            }
        }

        private static void WriteDictionary(BinaryWriter writer, Dictionary<object, object> dict)
        {
            writer.Write((ushort)dict.Count);

            foreach (var kvp in dict)
            {
                WriteValue(writer, kvp.Key);
                WriteValue(writer, kvp.Value);
            }
        }

        /// <summary>
        /// Create AppStats response for Authenticate
        /// </summary>
        public static byte[] CreateAppStatsResponse(GameServerState state)
        {
            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.PeerCount, state.TotalPlayerCount },
                { PhotonProtocol.ParameterCode.GameCount, state.TotalRoomCount },
                { PhotonProtocol.ParameterCode.MasterPeerCount, 1 }  // Master server always has 1
            };

            return SerializeEvent(PhotonProtocol.EventCode.AppStats, parameters);
        }

        /// <summary>
        /// Create GameList response for JoinLobby
        /// </summary>
        public static byte[] CreateGameListResponse(GameServerState state)
        {
            var roomList = new object[state.Rooms.Count];
            int i = 0;

            foreach (var room in state.Rooms.Values)
            {
                // Create room info hashtable
                var roomInfo = new Hashtable
                {
                    { "name", room.RoomName },
                    { "playerCount", room.CurrentPlayerCount },
                    { "maxPlayers", room.MaxPlayers },
                    { "isOpen", room.IsOpen },
                    { "isVisible", room.IsVisible }
                };

                roomList[i++] = roomInfo;
            }

            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.GameList, roomList }
            };

            return SerializeEvent(PhotonProtocol.EventCode.GameList, parameters);
        }

        /// <summary>
        /// Create Join event when player enters room
        /// </summary>
        public static byte[] CreateJoinEvent(GameRoom room, GamePlayer joiningPlayer)
        {
            // Create actor list of all current players
            var actorList = new int[room.Players.Count];
            int i = 0;
            foreach (var player in room.Players.Values)
            {
                actorList[i++] = player.ActorNumber;
            }

            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.ActorNr, joiningPlayer.ActorNumber },
                { PhotonProtocol.ParameterCode.ActorList, actorList },
                { PhotonProtocol.ParameterCode.GameProperties, CreateGamePropertiesHashtable(room) }
            };

            return SerializeEvent(PhotonProtocol.EventCode.Join, parameters);
        }

        /// <summary>
        /// Create Leave event when player exits room
        /// </summary>
        public static byte[] CreateLeaveEvent(int departingActorNumber)
        {
            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.ActorNr, departingActorNumber }
            };

            return SerializeEvent(PhotonProtocol.EventCode.Leave, parameters);
        }

        /// <summary>
        /// Create PropertiesChanged event
        /// </summary>
        public static byte[] CreatePropertiesChangedEvent(
            Hashtable? gameProperties = null,
            Hashtable? playerProperties = null)
        {
            var parameters = new Dictionary<byte, object>();

            if (gameProperties != null && gameProperties.Count > 0)
            {
                parameters[PhotonProtocol.ParameterCode.GameProperties] = gameProperties;
            }

            if (playerProperties != null && playerProperties.Count > 0)
            {
                parameters[PhotonProtocol.ParameterCode.PlayerProperties] = playerProperties;
            }

            return SerializeEvent(PhotonProtocol.EventCode.PropertiesChanged, parameters);
        }

        private static Hashtable CreateGamePropertiesHashtable(GameRoom room)
        {
            return new Hashtable
            {
                { "MaxPlayers", room.MaxPlayers },
                { "IsOpen", room.IsOpen },
                { "IsVisible", room.IsVisible },
                { "GameMode", room.GameMode },
                { "MapName", room.MapName }
            };
        }

        /// <summary>
        /// Create AppStats event to signal server is ready
        /// </summary>
        public static byte[] CreateAppStatsEvent(GameServerState state)
        {
            var parameters = new Dictionary<byte, object>
            {
                { (byte)224, "CopsNRobbers" },              // ApplicationId
                { (byte)228, (int)state.Rooms.Count },      // GameCount
                { (byte)229, 1 },                           // PeerCount (1 = this peer connected)
                { (byte)227, 1 }                            // MasterPeerCount
            };

            return SerializeEvent(PhotonProtocol.EventCode.AppStats, parameters);
        }

        /// <summary>
        /// Create GameList event to send available rooms to client
        /// </summary>
        public static byte[] CreateGameListEvent(GameServerState state)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Event code for GameList (0xE6 = 230 in Photon protocol)
                writer.Write((byte)0xE6); // GameList event code

                // Write number of rooms
                writer.Write((byte)state.Rooms.Count);

                // Write each room's data
                foreach (var room in state.Rooms.Values)
                {
                    // Room name
                    byte[] nameBytes = Encoding.UTF8.GetBytes(room.RoomName);
                    writer.Write((ushort)nameBytes.Length);
                    writer.Write(nameBytes);

                    // Room properties
                    writer.Write((byte)room.Players.Count);      // Current players
                    writer.Write((byte)room.MaxPlayers);         // Max players
                    writer.Write(room.IsOpen ? (byte)1 : (byte)0); // Is open
                    writer.Write(room.IsVisible ? (byte)1 : (byte)0); // Is visible
                }

                return ms.ToArray();
            }
        }
    }
}
