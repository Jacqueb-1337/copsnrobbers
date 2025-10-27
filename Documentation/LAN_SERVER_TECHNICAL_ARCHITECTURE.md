# LAN Server Technical Architecture & Implementation Plan

## Document Purpose
Complete technical specification for building a drop-in replacement for Photon Cloud networking that works on local WiFi networks without internet connectivity.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         Game (APK)                              │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  UI Layer (Server Browser, Matchmaking)                   │ │
│  │  ┌──────────────────────────────────────────────────────┐ │ │
│  │  │ Network Layer (Patched)                              │ │ │
│  │  │  - LanNetworkManager (replaces PhotonNetwork)        │ │ │
│  │  │  - BroadcastListener (UDP 5056)                      │ │ │
│  │  │  - RpcInterceptor (hooks game RPCs)                  │ │ │
│  │  │  - StateSerializer (OnPhotonSerializeView)           │ │ │
│  │  │  - TcpClient (connect to LAN server)                 │ │ │
│  │  └──────────────────────────────────────────────────────┘ │ │
│  │  ┌──────────────────────────────────────────────────────┐ │ │
│  │  │ Gameplay Layer (Unmodified)                          │ │ │
│  │  │  - PlayerController, WeaponController, etc.          │ │ │
│  │  │  - Calls RPC/Serialize but doesn't know about LAN    │ │ │
│  │  └──────────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────┬──────────────────────┬────────────────────┘
                       │                      │
        TCP Port 5055  │                      │ UDP Port 5056
        (Gameplay)     │                      │ (Discovery)
                       │                      │
        ┌──────────────▼──────────────────────▼────────────┐
        │         LAN Server (PC)                          │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ TCP Server                                  │ │
        │  │  - Accept connections on port 5055         │ │
        │  │  - Handle messages from all 4 clients      │ │
        │  │  - Message thread pool (handle 100+ msg/s) │ │
        │  └─────────────────────────────────────────────┘ │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ Room Manager                                │ │
        │  │  - Create/join/leave rooms                 │ │
        │  │  - Maintain player list per room           │ │
        │  │  - Track room properties                   │ │
        │  └─────────────────────────────────────────────┘ │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ Message Router                              │ │
        │  │  - Parse operation codes                   │ │
        │  │  - Route to room manager / RPC executor     │ │
        │  │  - Broadcast to appropriate clients        │ │
        │  └─────────────────────────────────────────────┘ │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ RPC Executor                                │ │
        │  │  - Deserialize RPC parameters              │ │
        │  │  - Queue for broadcast to room             │ │
        │  │  - Handle buffered RPCs for latecomers     │ │
        │  └─────────────────────────────────────────────┘ │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ UDP Broadcaster                             │ │
        │  │  - Send "I'm here" beacon every 1 second   │ │
        │  │  - Advertise: name, players, port          │ │
        │  └─────────────────────────────────────────────┘ │
        │  ┌─────────────────────────────────────────────┐ │
        │  │ State Synchronizer                          │ │
        │  │  - Aggregate player state updates          │ │
        │  │  - Forward to all other players in room    │ │
        │  │  - Don't send back to sender               │ │
        │  └─────────────────────────────────────────────┘ │
        └─────────────────────────────────────────────────────┘
```

---

## Client (APK) Implementation Details

### Phase 1: Disable Cloud Connection

**File**: `Assets/Scripts/Network/PhotonInitializer.cs` (NEW - create)

```csharp
using UnityEngine;
using Photon;

public class PhotonInitializer : MonoBehaviour
{
    void Start()
    {
        // Prevent Photon from connecting to cloud
        PhotonNetwork.autoJoinLobby = false;
        
        // Initialize LAN network manager instead
        LanNetworkManager.Instance.Initialize();
        
        // Show server browser UI
        ServerBrowserUI.Instance.ShowDialog();
    }
}
```

**What to Modify**:
1. Find existing `PhotonNetwork.ConnectUsingSettings()` calls
2. Replace with `LanNetworkManager.Connect(serverIP, serverPort)`
3. Stub out any cloud-specific code

### Phase 2: Broadcast Listener

**File**: `Assets/Scripts/Network/BroadcastListener.cs` (NEW)

```csharp
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

public class BroadcastListener : MonoBehaviour
{
    private static BroadcastListener instance;
    private UdpClient udpClient;
    private Thread listenerThread;
    private bool isRunning = false;
    
    public struct DiscoveredServer
    {
        public string name;
        public string ipAddress;
        public int tcpPort;
        public int currentPlayers;
        public int maxPlayers;
        public System.DateTime lastSeen;
    }
    
    public Dictionary<string, DiscoveredServer> DiscoveredServers { get; private set; }
    
    void Start()
    {
        instance = this;
        DiscoveredServers = new Dictionary<string, DiscoveredServer>();
        
        // Start listening for broadcasts
        udpClient = new UdpClient(5056);
        isRunning = true;
        
        listenerThread = new Thread(ListenForBroadcasts);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }
    
    void ListenForBroadcasts()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 5056);
        
        while (isRunning)
        {
            try
            {
                // Timeout after 1 second (non-blocking)
                udpClient.Client.ReceiveTimeout = 1000;
                byte[] data = udpClient.Receive(ref remoteEP);
                
                if (data.Length > 20)
                {
                    // Parse: [CNRS header][version][players][max][port][name...]
                    if (data[0] == 'C' && data[1] == 'N' && data[2] == 'R' && data[3] == 'S')
                    {
                        int version = System.BitConverter.ToInt16(data, 4);
                        int currentPlayers = System.BitConverter.ToInt16(data, 6);
                        int maxPlayers = System.BitConverter.ToInt16(data, 8);
                        int tcpPort = System.BitConverter.ToInt16(data, 10);
                        
                        string serverName = System.Text.Encoding.UTF8.GetString(data, 12, data.Length - 12);
                        string senderIP = remoteEP.Address.ToString();
                        
                        lock (DiscoveredServers)
                        {
                            string key = senderIP + ":" + tcpPort;
                            DiscoveredServers[key] = new DiscoveredServer
                            {
                                name = serverName,
                                ipAddress = senderIP,
                                tcpPort = tcpPort,
                                currentPlayers = currentPlayers,
                                maxPlayers = maxPlayers,
                                lastSeen = System.DateTime.Now
                            };
                        }
                    }
                }
            }
            catch (SocketException)
            {
                // Timeout is expected, just retry
            }
        }
    }
    
    void OnDestroy()
    {
        isRunning = false;
        if (listenerThread != null && listenerThread.IsAlive)
            listenerThread.Join(1000);
        if (udpClient != null)
            udpClient.Close();
    }
}
```

### Phase 3: Network Manager (Replaces PhotonNetwork)

**File**: `Assets/Scripts/Network/LanNetworkManager.cs` (NEW)

```csharp
using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

public class LanNetworkManager : MonoBehaviour
{
    public static LanNetworkManager Instance { get; private set; }
    
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private int localPlayerID = -1;
    private string connectedServerIP;
    private int connectedServerPort;
    
    // Event callbacks (mimics PhotonNetwork)
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action<Room> OnJoinedRoom;
    public static event Action OnLeftRoom;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void Initialize()
    {
        // Setup complete
    }
    
    public void Connect(string serverIP, int serverPort)
    {
        connectedServerIP = serverIP;
        connectedServerPort = serverPort;
        
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIP, serverPort);
            networkStream = tcpClient.GetStream();
            
            // Send handshake
            byte[] handshake = CreateHandshakeMessage("Player_" + UnityEngine.Random.Range(1000, 9999));
            networkStream.Write(handshake, 0, handshake.Length);
            networkStream.Flush();
            
            // Receive player ID
            byte[] response = new byte[10];
            int read = networkStream.Read(response, 0, 10);
            
            if (response[0] == 0x01) // Connect response
            {
                localPlayerID = BitConverter.ToInt16(response, 3);
                Debug.Log("Connected! Assigned Player ID: " + localPlayerID);
                OnConnected?.Invoke();
                
                // Start receiving messages
                StartCoroutine(ReceiveMessages());
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect: " + e.Message);
        }
    }
    
    private byte[] CreateHandshakeMessage(string playerName)
    {
        List<byte> msg = new List<byte>();
        msg.Add(0x01); // OpCode: Connect
        
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(playerName);
        msg.AddRange(BitConverter.GetBytes((short)(nameBytes.Length + 5)));
        msg.Add(0x01); // Protocol version
        msg.AddRange(BitConverter.GetBytes((int)DateTime.Now.Ticks));
        msg.AddRange(nameBytes);
        
        return msg.ToArray();
    }
    
    public void CreateRoom(string roomName, int maxPlayers = 4)
    {
        byte[] msg = new byte[256];
        msg[0] = 0x02; // OpCode: CreateRoom
        
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(roomName);
        msg[3] = (byte)maxPlayers;
        msg[4] = 1; // isOpen
        msg[5] = 1; // isVisible
        
        System.Array.Copy(nameBytes, 0, msg, 6, nameBytes.Length);
        
        short length = (short)(3 + nameBytes.Length);
        System.Array.Copy(BitConverter.GetBytes(length), 0, msg, 1, 2);
        
        networkStream.Write(msg, 0, 6 + nameBytes.Length);
        networkStream.Flush();
    }
    
    public void JoinRoom(string roomName)
    {
        // Similar to CreateRoom but with different OpCode (0x05)
    }
    
    public void LeaveRoom()
    {
        byte[] msg = new byte[3];
        msg[0] = 0x06; // OpCode: LeaveRoom
        msg[1] = 0;
        msg[2] = 0;
        
        networkStream.Write(msg, 0, 3);
        networkStream.Flush();
    }
    
    private System.Collections.IEnumerator ReceiveMessages()
    {
        byte[] buffer = new byte[8192];
        
        while (tcpClient != null && tcpClient.Connected)
        {
            try
            {
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                
                if (bytesRead > 0)
                {
                    ProcessMessage(buffer, bytesRead);
                }
                else
                {
                    // Server disconnected
                    OnDisconnected?.Invoke();
                    yield break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving: " + e.Message);
                yield break;
            }
            
            yield return null;
        }
    }
    
    private void ProcessMessage(byte[] data, int length)
    {
        if (length < 3) return;
        
        byte opCode = data[0];
        short payloadLength = BitConverter.ToInt16(data, 1);
        
        switch (opCode)
        {
            case 0x01: // Connect response (already handled)
                break;
            case 0x03: // RPC Call
                OnRPCReceived(data, 3, payloadLength);
                break;
            case 0x04: // State Sync
                OnStateSyncReceived(data, 3, payloadLength);
                break;
            case 0x07: // Player List Update
                OnPlayerListUpdated(data, 3, payloadLength);
                break;
        }
    }
    
    private void OnRPCReceived(byte[] data, int offset, short length)
    {
        // Parse and execute RPC
        int viewID = BitConverter.ToInt32(data, offset);
        byte target = data[offset + 4];
        short methodID = BitConverter.ToInt16(data, offset + 5);
        
        // Find PhotonView with this ID
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            // Deserialize and execute...
            pv.ExecuteRPC(methodID, data, offset + 7);
        }
    }
    
    private void OnStateSyncReceived(byte[] data, int offset, short length)
    {
        // Parse state data and apply to networked objects
    }
    
    private void OnPlayerListUpdated(byte[] data, int offset, short length)
    {
        // Update UI with current players in room
    }
    
    public int GetLocalPlayerID()
    {
        return localPlayerID;
    }
}
```

### Phase 4: RPC Interceptor

**File**: `Assets/Scripts/Network/RpcInterceptor.cs` (NEW)

```csharp
using UnityEngine;
using System.Collections.Generic;

public class RpcInterceptor : MonoBehaviour
{
    // Intercept photonView.RPC() calls and send to LAN server instead of Photon
    
    public static void SendRPC(PhotonView photonView, string methodName, 
                               RpcTarget target, params object[] parameters)
    {
        if (photonView == null) return;
        
        // Serialize RPC call
        byte[] rpcMessage = SerializeRPC(photonView.ViewID, methodName, parameters);
        
        // Send to LAN server
        if (LanNetworkManager.Instance != null)
        {
            // Queue message for sending
            MessageQueue.Instance.Enqueue(rpcMessage);
        }
    }
    
    private static byte[] SerializeRPC(int viewID, string methodName, object[] parameters)
    {
        List<byte> msg = new List<byte>();
        
        msg.Add(0x03); // OpCode: RPC Call
        
        // Placeholder for length
        int lengthPos = msg.Count;
        msg.AddRange(new byte[2]);
        
        // View ID
        msg.AddRange(System.BitConverter.GetBytes(viewID));
        
        // Target
        msg.Add(0x00); // AllBuffered
        
        // Method ID (hash of method name)
        msg.AddRange(System.BitConverter.GetBytes(GetMethodHash(methodName)));
        
        // Parameter count and data
        msg.Add((byte)parameters.Length);
        
        foreach (object param in parameters)
        {
            SerializeParameter(msg, param);
        }
        
        // Update length
        short length = (short)(msg.Count - 3);
        msg[lengthPos] = (byte)(length & 0xFF);
        msg[lengthPos + 1] = (byte)((length >> 8) & 0xFF);
        
        return msg.ToArray();
    }
    
    private static void SerializeParameter(List<byte> msg, object param)
    {
        if (param == null)
        {
            msg.Add(0x00); // Null type
        }
        else if (param is int)
        {
            msg.Add(0x03); // Int type
            msg.AddRange(System.BitConverter.GetBytes((int)param));
        }
        else if (param is float)
        {
            msg.Add(0x05); // Float type
            msg.AddRange(System.BitConverter.GetBytes((float)param));
        }
        else if (param is Vector3)
        {
            msg.Add(0x07); // Vector3 type
            Vector3 v = (Vector3)param;
            msg.AddRange(System.BitConverter.GetBytes(v.x));
            msg.AddRange(System.BitConverter.GetBytes(v.y));
            msg.AddRange(System.BitConverter.GetBytes(v.z));
        }
        else if (param is string)
        {
            msg.Add(0x09); // String type
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes((string)param);
            msg.AddRange(System.BitConverter.GetBytes((short)strBytes.Length));
            msg.AddRange(strBytes);
        }
    }
    
    private static short GetMethodHash(string methodName)
    {
        // Simple hash of method name (must match server's hashing)
        return (short)(methodName.GetHashCode());
    }
}
```

### Phase 5: Message Queue (Thread-Safe)

**File**: `Assets/Scripts/Network/MessageQueue.cs` (NEW)

```csharp
using UnityEngine;
using System.Collections.Generic;

public class MessageQueue : MonoBehaviour
{
    public static MessageQueue Instance { get; private set; }
    
    private Queue<byte[]> outgoing = new Queue<byte[]>();
    private NetworkStream networkStream;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    
    public void Enqueue(byte[] message)
    {
        lock (outgoing)
        {
            outgoing.Enqueue(message);
        }
    }
    
    void Update()
    {
        // Send queued messages
        lock (outgoing)
        {
            while (outgoing.Count > 0)
            {
                byte[] msg = outgoing.Dequeue();
                if (networkStream != null && networkStream.CanWrite)
                {
                    networkStream.Write(msg, 0, msg.Length);
                    networkStream.Flush();
                }
            }
        }
    }
}
```

---

## Server Implementation Details

### Server Architecture (C# or Python)

**Language Choice Recommendation**: C# for parity with game code, or Python for simplicity

**Threading Model**:
```
Main Thread
  └─ TCP Server (listening on port 5055)
     ├─ Client Handler Thread 1 (for player 1)
     ├─ Client Handler Thread 2 (for player 2)
     ├─ Client Handler Thread 3 (for player 3)
     └─ Client Handler Thread 4 (for player 4)
  
  └─ UDP Broadcaster (port 5056)
     └─ Broadcasts every 1 second
  
  └─ Room Manager (shared state)
     ├─ Room list
     ├─ Player list
     └─ Game state
```

### Core Server Class (C#)

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

public class LanGameServer
{
    private TcpListener tcpListener;
    private Dictionary<int, ConnectedClient> clients = new Dictionary<int, ConnectedClient>();
    private Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    private int nextPlayerID = 1;
    private int nextRoomID = 1;
    
    private UdpClient udpBroadcaster;
    private bool isRunning = true;
    
    private const int TCP_PORT = 5055;
    private const int UDP_PORT = 5056;
    private const int MAX_PLAYERS = 8;
    
    public void Start()
    {
        // Start TCP server
        tcpListener = new TcpListener(IPAddress.Any, TCP_PORT);
        tcpListener.Start(5);
        
        Thread tcpThread = new Thread(AcceptConnections);
        tcpThread.IsBackground = true;
        tcpThread.Start();
        
        Console.WriteLine("LAN Server started on port " + TCP_PORT);
        
        // Start UDP broadcaster
        udpBroadcaster = new UdpClient();
        Thread broadcasterThread = new Thread(BroadcastServerAvailability);
        broadcasterThread.IsBackground = true;
        broadcasterThread.Start();
        
        Console.WriteLine("UDP Broadcaster started on port " + UDP_PORT);
    }
    
    private void AcceptConnections()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                int playerID = nextPlayerID++;
                
                ConnectedClient cc = new ConnectedClient(playerID, client, this);
                lock (clients)
                {
                    clients[playerID] = cc;
                }
                
                Thread clientThread = new Thread(() => HandleClient(cc));
                clientThread.IsBackground = true;
                clientThread.Start();
                
                Console.WriteLine("Player " + playerID + " connected");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error accepting connection: " + e.Message);
            }
        }
    }
    
    private void HandleClient(ConnectedClient client)
    {
        NetworkStream stream = client.TcpClient.GetStream();
        byte[] buffer = new byte[8192];
        
        try
        {
            while (client.IsConnected && isRunning)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                if (bytesRead > 0)
                {
                    ProcessClientMessage(client, buffer, bytesRead);
                }
                else
                {
                    // Client disconnected
                    DisconnectClient(client);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error handling client: " + e.Message);
            DisconnectClient(client);
        }
    }
    
    private void ProcessClientMessage(ConnectedClient client, byte[] data, int length)
    {
        if (length < 3) return;
        
        byte opCode = data[0];
        short payloadLength = BitConverter.ToInt16(data, 1);
        
        switch (opCode)
        {
            case 0x01: // Connect (already handled in handshake)
                break;
            case 0x02: // CreateRoom
                HandleCreateRoom(client, data, 3);
                break;
            case 0x03: // RPC Call
                HandleRPC(client, data, 3);
                break;
            case 0x04: // State Sync
                HandleStateSync(client, data, 3);
                break;
            case 0x05: // Join Room
                HandleJoinRoom(client, data, 3);
                break;
            case 0x06: // Leave Room
                HandleLeaveRoom(client);
                break;
        }
    }
    
    private void HandleCreateRoom(ConnectedClient client, byte[] data, int offset)
    {
        int maxPlayers = data[offset];
        bool isOpen = data[offset + 1] == 1;
        bool isVisible = data[offset + 2] == 1;
        
        // Extract room name...
        
        Room room = new Room
        {
            RoomID = nextRoomID++,
            Name = "Game",
            MaxPlayers = Math.Min(maxPlayers, MAX_PLAYERS),
            IsOpen = isOpen,
            IsVisible = isVisible,
            Owner = client.PlayerID,
            CreatedTime = DateTime.Now
        };
        
        lock (rooms)
        {
            rooms[room.RoomID] = room;
        }
        
        client.RoomID = room.RoomID;
        room.Players.Add(client.PlayerID);
        
        // Send response
        BroadcastToRoom(room.RoomID, CreateRoomCreatedMessage(room));
    }
    
    private void HandleRPC(ConnectedClient client, byte[] data, int offset)
    {
        if (client.RoomID == -1) return;
        
        // Extract RPC data and broadcast to room
        Room room = GetRoom(client.RoomID);
        if (room != null)
        {
            BroadcastToRoomExcept(room, client.PlayerID, data);
        }
    }
    
    private void HandleStateSync(ConnectedClient client, byte[] data, int offset)
    {
        if (client.RoomID == -1) return;
        
        // Broadcast player state to others in room
        Room room = GetRoom(client.RoomID);
        if (room != null)
        {
            BroadcastToRoomExcept(room, client.PlayerID, data);
        }
    }
    
    private void BroadcastToRoom(int roomID, byte[] message)
    {
        Room room = GetRoom(roomID);
        if (room == null) return;
        
        foreach (int playerID in room.Players)
        {
            if (clients.ContainsKey(playerID))
            {
                clients[playerID].SendMessage(message);
            }
        }
    }
    
    private void BroadcastToRoomExcept(Room room, int excludePlayerID, byte[] message)
    {
        foreach (int playerID in room.Players)
        {
            if (playerID != excludePlayerID && clients.ContainsKey(playerID))
            {
                clients[playerID].SendMessage(message);
            }
        }
    }
    
    private void BroadcastServerAvailability()
    {
        while (isRunning)
        {
            try
            {
                IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, UDP_PORT);
                
                // Build broadcast message
                List<byte> msg = new List<byte>();
                msg.AddRange(new byte[] { (byte)'C', (byte)'N', (byte)'R', (byte)'S' });
                msg.AddRange(BitConverter.GetBytes((short)1)); // version
                
                int currentPlayers = clients.Count;
                msg.AddRange(BitConverter.GetBytes((short)currentPlayers));
                msg.AddRange(BitConverter.GetBytes((short)MAX_PLAYERS));
                msg.AddRange(BitConverter.GetBytes((short)TCP_PORT));
                
                string serverName = "Local Server";
                msg.AddRange(System.Text.Encoding.UTF8.GetBytes(serverName));
                
                udpBroadcaster.Send(msg.ToArray(), msg.Count, broadcastEP);
                
                Thread.Sleep(1000); // Broadcast every second
            }
            catch (Exception e)
            {
                Console.WriteLine("Broadcast error: " + e.Message);
            }
        }
    }
    
    private void DisconnectClient(ConnectedClient client)
    {
        client.IsConnected = false;
        
        // Remove from room
        if (client.RoomID != -1)
        {
            Room room = GetRoom(client.RoomID);
            if (room != null)
            {
                room.Players.Remove(client.PlayerID);
                
                // Notify others
                BroadcastToRoom(room.RoomID, CreatePlayerLeftMessage(client.PlayerID));
                
                // Delete room if empty
                if (room.Players.Count == 0)
                {
                    lock (rooms)
                    {
                        rooms.Remove(room.RoomID);
                    }
                }
            }
        }
        
        // Remove from clients list
        lock (clients)
        {
            clients.Remove(client.PlayerID);
        }
        
        client.Dispose();
        Console.WriteLine("Player " + client.PlayerID + " disconnected");
    }
    
    private Room GetRoom(int roomID)
    {
        lock (rooms)
        {
            if (rooms.ContainsKey(roomID))
                return rooms[roomID];
        }
        return null;
    }
}

public class ConnectedClient
{
    public int PlayerID { get; private set; }
    public int RoomID { get; set; } = -1;
    public TcpClient TcpClient { get; private set; }
    public bool IsConnected { get; set; } = true;
    private Queue<byte[]> outgoingMessages = new Queue<byte[]>();
    private Thread sendThread;
    
    public ConnectedClient(int playerID, TcpClient tcpClient, LanGameServer server)
    {
        PlayerID = playerID;
        TcpClient = tcpClient;
        
        sendThread = new Thread(SendMessagesThread);
        sendThread.IsBackground = true;
        sendThread.Start();
    }
    
    public void SendMessage(byte[] message)
    {
        lock (outgoingMessages)
        {
            outgoingMessages.Enqueue(message);
        }
    }
    
    private void SendMessagesThread()
    {
        while (IsConnected)
        {
            lock (outgoingMessages)
            {
                while (outgoingMessages.Count > 0)
                {
                    byte[] msg = outgoingMessages.Dequeue();
                    try
                    {
                        TcpClient.GetStream().Write(msg, 0, msg.Length);
                        TcpClient.GetStream().Flush();
                    }
                    catch
                    {
                        IsConnected = false;
                    }
                }
            }
            
            Thread.Sleep(10);
        }
    }
    
    public void Dispose()
    {
        TcpClient?.Close();
        TcpClient?.Dispose();
    }
}

public class Room
{
    public int RoomID { get; set; }
    public string Name { get; set; }
    public int MaxPlayers { get; set; }
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; }
    public int Owner { get; set; }
    public DateTime CreatedTime { get; set; }
    public List<int> Players { get; set; } = new List<int>();
}
```

---

## Testing Strategy

### Unit Tests
- [ ] Message serialization/deserialization
- [ ] Room create/join/leave logic
- [ ] Player list management

### Integration Tests
- [ ] Single client connection
- [ ] Two clients in same room
- [ ] RPC routing
- [ ] State sync

### End-to-End Tests
- [ ] 4 players on same WiFi
- [ ] Full game loop (matchmaking, gameplay, scoring)
- [ ] Disconnection and reconnection

---

## Deployment Checklist

**Server Setup**:
- [ ] Compile server executable
- [ ] Create config file with server name and max players
- [ ] Test on Windows PC on LAN
- [ ] Verify UDP broadcast reaches devices
- [ ] Verify TCP port 5055 accepts connections

**APK Patching**:
- [ ] Compile modified APK with LAN network code
- [ ] Test on Android phone on same WiFi
- [ ] Verify server discovery works
- [ ] Verify connection and gameplay

**Firewall Configuration**:
- [ ] Open Windows Defender Firewall for port 5055 (TCP)
- [ ] Open port 5056 (UDP)
- [ ] Or disable firewall for testing

---

## Timeline Estimate

| Phase | Task | Hours |
|-------|------|-------|
| 1 | Photon protocol analysis | 8 |
| 2 | Binary protocol design | 4 |
| 3 | Server core implementation | 12 |
| 4 | Client network code | 10 |
| 5 | RPC/State sync implementation | 8 |
| 6 | Testing and debugging | 12 |
| **Total** | | **54** |

---

## Known Risks & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Photon protocol incompatibility | High | Critical | Reverse-engineer with dnSpy first |
| RPC parameter mismatch | High | Critical | Extensive testing with logging |
| Latency/sync issues | Medium | High | Implement lag compensation |
| Firewall blocks connections | Medium | Medium | Document firewall setup |
| APK patch doesn't compile | Low | High | Test patching process early |
| Server crashes under load | Low | High | Add error handling and logging |

