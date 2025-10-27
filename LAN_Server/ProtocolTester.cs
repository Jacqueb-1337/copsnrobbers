using System;
using System.Collections;
using System.IO;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// Test utility to validate message parsing and serialization
    /// </summary>
    public class ProtocolTester
    {
        public static void RunTests()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Protocol Parser & Serializer Tests               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            TestSerializeDeserializeBasicTypes();
            TestSerializeDeserializeCollections();
            TestAuthenticateMessage();
            TestCreateGameMessage();
            TestAppStatsResponse();

            Console.WriteLine("\n✅ All tests completed!\n");
        }

        private static void TestSerializeDeserializeBasicTypes()
        {
            Console.WriteLine("Test 1: Serialize/Deserialize Basic Types");
            Console.WriteLine("──────────────────────────────────────────");

            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.PeerCount, 4 },
                { PhotonProtocol.ParameterCode.RoomName, "MyRoom" },
                { PhotonProtocol.ParameterCode.Broadcast, true }
            };

            // Serialize
            byte[] data = PhotonMessageSerializer.SerializeEvent(PhotonProtocol.EventCode.AppStats, parameters);
            Console.WriteLine("  Serialized: {0} bytes", data.Length);

            // Deserialize
            var message = PhotonMessageParser.ParseMessage(data, data.Length);
            if (message != null)
            {
                Console.WriteLine("  Deserialized: EventCode=0x{0:X2}, Params={1}", message.OperationCode, message.Parameters.Count);
                
                int? count = message.GetInt(PhotonProtocol.ParameterCode.PeerCount);
                string? room = message.GetString(PhotonProtocol.ParameterCode.RoomName);
                bool? broadcast = message.GetBool(PhotonProtocol.ParameterCode.Broadcast);
                
                Console.WriteLine("    PeerCount: {0}", count);
                Console.WriteLine("    RoomName: {0}", room);
                Console.WriteLine("    Broadcast: {0}", broadcast);
                Console.WriteLine("  ✅ PASS\n");
            }
            else
            {
                Console.WriteLine("  ❌ FAIL - Parse failed\n");
            }
        }

        private static void TestSerializeDeserializeCollections()
        {
            Console.WriteLine("Test 2: Serialize/Deserialize Collections (Hashtable)");
            Console.WriteLine("────────────────────────────────────────────────────");

            var gameProps = new Hashtable
            {
                { "MaxPlayers", 4 },
                { "GameMode", "CopsNRobbers" },
                { "IsOpen", true }
            };

            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.GameProperties, gameProps }
            };

            // Serialize
            byte[] data = PhotonMessageSerializer.SerializeEvent(
                PhotonProtocol.EventCode.PropertiesChanged, parameters);
            Console.WriteLine("  Serialized: {0} bytes", data.Length);

            // Deserialize
            var message = PhotonMessageParser.ParseMessage(data, data.Length);
            if (message != null)
            {
                var table = message.GetHashtable(PhotonProtocol.ParameterCode.GameProperties);
                if (table != null)
                {
                    Console.WriteLine("  Deserialized Hashtable: {0} entries", table.Count);
                    foreach (DictionaryEntry entry in table)
                    {
                        Console.WriteLine("    {0}: {1}", entry.Key, entry.Value);
                    }
                    Console.WriteLine("  ✅ PASS\n");
                }
                else
                {
                    Console.WriteLine("  ❌ FAIL - Hashtable not found\n");
                }
            }
            else
            {
                Console.WriteLine("  ❌ FAIL - Parse failed\n");
            }
        }

        private static void TestAuthenticateMessage()
        {
            Console.WriteLine("Test 3: Authenticate Message (Client → Server)");
            Console.WriteLine("───────────────────────────────────────────────");

            // Simulate client sending Authenticate
            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.ApplicationId, "myAppId123" },
                { PhotonProtocol.ParameterCode.AppVersion, "3.0.2" },
                { PhotonProtocol.ParameterCode.UserId, "player@example.com" }
            };

            byte[] data = PhotonMessageSerializer.SerializeEvent(
                PhotonProtocol.OperationCode.Authenticate, parameters);
            Console.WriteLine("  Message: {0} bytes", data.Length);

            // Parse it
            var message = PhotonMessageParser.ParseMessage(data, data.Length);
            if (message != null)
            {
                Console.WriteLine("  OpCode: 0x{0:X2} (Authenticate)", message.OperationCode);
                Console.WriteLine("  AppID: {0}", message.GetString(PhotonProtocol.ParameterCode.ApplicationId));
                Console.WriteLine("  Version: {0}", message.GetString(PhotonProtocol.ParameterCode.AppVersion));
                Console.WriteLine("  UserID: {0}", message.GetString(PhotonProtocol.ParameterCode.UserId));
                Console.WriteLine("  ✅ PASS\n");
            }
            else
            {
                Console.WriteLine("  ❌ FAIL\n");
            }
        }

        private static void TestCreateGameMessage()
        {
            Console.WriteLine("Test 4: CreateGame Message (Client → Server)");
            Console.WriteLine("──────────────────────────────────────────────");

            var parameters = new Dictionary<byte, object>
            {
                { PhotonProtocol.ParameterCode.RoomName, "Game_001" },
                { PhotonProtocol.ParameterCode.GameProperties, new Hashtable
                {
                    { "MaxPlayers", 4 },
                    { "MapName", "level_01" }
                }}
            };

            byte[] data = PhotonMessageSerializer.SerializeEvent(
                PhotonProtocol.OperationCode.CreateGame, parameters);
            Console.WriteLine("  Message: {0} bytes", data.Length);

            var message = PhotonMessageParser.ParseMessage(data, data.Length);
            if (message != null)
            {
                Console.WriteLine("  OpCode: 0x{0:X2} (CreateGame)", message.OperationCode);
                Console.WriteLine("  RoomName: {0}", message.GetString(PhotonProtocol.ParameterCode.RoomName));
                
                var props = message.GetHashtable(PhotonProtocol.ParameterCode.GameProperties);
                if (props != null)
                {
                    Console.WriteLine("  GameProperties:");
                    foreach (DictionaryEntry entry in props)
                    {
                        Console.WriteLine("    {0}: {1}", entry.Key, entry.Value);
                    }
                }
                
                Console.WriteLine("  ✅ PASS\n");
            }
            else
            {
                Console.WriteLine("  ❌ FAIL\n");
            }
        }

        private static void TestAppStatsResponse()
        {
            Console.WriteLine("Test 5: AppStats Response (Server → Client)");
            Console.WriteLine("───────────────────────────────────────────");

            var gameState = new GameServerState
            {
                AllPlayers = new System.Collections.Generic.Dictionary<int, GamePlayer>
                {
                    { 1, new GamePlayer { PlayerName = "Player1" } },
                    { 2, new GamePlayer { PlayerName = "Player2" } }
                },
                Rooms = new System.Collections.Generic.Dictionary<string, GameRoom>
                {
                    { "room1", new GameRoom { RoomName = "Room1" } }
                }
            };

            byte[] data = PhotonMessageSerializer.CreateAppStatsResponse(gameState);
            Console.WriteLine("  Message: {0} bytes", data.Length);

            var message = PhotonMessageParser.ParseMessage(data, data.Length);
            if (message != null)
            {
                Console.WriteLine("  EventCode: 0x{0:X2} (AppStats)", message.OperationCode);
                Console.WriteLine("  PeerCount: {0}", message.GetInt(PhotonProtocol.ParameterCode.PeerCount));
                Console.WriteLine("  GameCount: {0}", message.GetInt(PhotonProtocol.ParameterCode.GameCount));
                Console.WriteLine("  ✅ PASS\n");
            }
            else
            {
                Console.WriteLine("  ❌ FAIL\n");
            }
        }
    }
}
