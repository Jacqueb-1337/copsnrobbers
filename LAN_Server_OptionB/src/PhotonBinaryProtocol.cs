using System.Collections;
using System.Text;

namespace CNRLanServer;

// ─────────────────────────────────────────────────────────────────────────────
// Photon TCP Binary Protocol — reverse-engineered from Photon3Unity3D.dll IL
//
// Wire layout for every message going out FROM the server:
//
//   [0]      0xF3              magic marker
//   [1..4]   total_len (BE)    includes this 9-byte header
//   [5]      0x00              channel (always 0 for us)
//   [6]      0x00              flags (bit7 = encrypted, we never encrypt)
//   [7]      0xF3              inner magic (MUST be 0xF3 — ReceiveIncomingCommands checks inbuff[0]==0xF3 or 0xF4)
//   [8]      msg_type          EgMessageType: 3=OpResp, 4=EventData, 1=ConnectInit
//   [9..]    payload           see per-type notes below
//
// Payload for OperationResponse (msg_type 3) — NO setType prefix:
//   opCode   (1 byte)
//   retCode  (int16 big-endian)
//   debug    (0x2A = null, or 0x73 + 2-byte-len + UTF8 bytes for non-null)
//   params   (parameter table — see SerializeParamTable)
//
// Payload for EventData (msg_type 4) — NO setType prefix:
//   eventCode  (1 byte)
//   params     (parameter table)
//
// Parameter table:
//   count (int16 big-endian)
//   then for each entry:
//     key   (1 byte)
//     value (type-byte + serialized data — see GpType enum)
//
// Init packet FROM CLIENT (first 48 bytes on connect):
//   [0]      0xFB
//   [1..4]   0x00 0x00 0x00 0x30   (length = 48)
//   [5..6]   0x00 0x01
//   [7..15]  0xF3 0x00 0x01 0x06 0x01 0x03 0x00 0x01 0x07
//   [16..47] AppID (32 bytes, zero-padded)
//
// Our init response to client (9 bytes):
//   0xF3 0x00 0x00 0x00 0x09 0x00 0x00 0xF3 0x01
// bytes[7]=0xF3 inner magic required by ReceiveIncomingCommands (inbuff[0] must be 0xF3)
// bytes[8]=0x01: b=1 → switch case 0 → InitCallback()   (null-stream special path, no deserialization)
// ALL outgoing frames (Op/Event) must also have frame[7]=0xF3 for the same reason (done in WrapTcpFrame).
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>GpType codes used by Photon's binary serialization.</summary>
public static class GpType
{
    public const byte Null        = 42;   // '*'
    public const byte Boolean     = 111;  // 'o'
    public const byte Byte        = 98;   // 'b'
    public const byte Short       = 107;  // 'k'
    public const byte Integer     = 105;  // 'i'
    public const byte Long        = 108;  // 'l'
    public const byte Float       = 102;  // 'f'
    public const byte Double      = 100;  // 'd'
    public const byte String      = 115;  // 's'
    public const byte StringArray = 97;   // 'a'
    public const byte ByteArray   = 120;  // 'x'
    public const byte IntArray    = 121;  // 'y' (stored as Array type with element 0x69)
    public const byte Hashtable   = 104;  // 'h'
    public const byte Dictionary  = 68;   // 'D'
    public const byte ObjectArray = 122;  // 'z'
    public const byte EventData   = 101;  // 'e'
    public const byte OpRequest   = 113;  // 'q'
    public const byte OpResponse  = 112;  // 'p'
    public const byte Array       = 121;  // 'y' (typed array)
}

/// <summary>
/// EgMessageType values for byte[8] of the TCP frame header (server→client direction).
/// Confirmed from binary switch table (Photon3Unity3D.dll RVA 0x7A7C IL_0144):
///   b=1 → switch case 0 → InitCallback()                   ← ConnectInitResponse
///   b=2 → switch case 1 → default/unexpected (unused)
///   b=3 → switch case 2 → DeserializeOperationResponse     ← OperationResponse
///   b=4 → switch case 3 → DeserializeEventData             ← EventData
///   b=7 → switch case 6 → DeserializeOperationResponse(internal, DH key)
/// Note: frame[7] (inBuff[0]) MUST be 0xF3 so ReceiveIncomingCommands adds it to incomingList.
/// </summary>
public static class EgMsgType
{
    public const byte ConnectInitResponse = 1;   // b=1 → switch case 0 → InitCallback()
    public const byte OperationResponse   = 3;   // b=3 → switch case 2 → DeserializeOperationResponse → OnOperationResponse
    public const byte EventData           = 4;   // b=4 → switch case 3 → DeserializeEventData → OnEvent
}

/// <summary>Photon operation codes (confirmed from Assembly-CSharp.dll decompilation).</summary>
public static class OpCode
{
    public const byte Join         = 255; // 0xFF — game server event only
    public const byte Leave        = 254; // 0xFE
    public const byte RaiseEvent   = 253; // 0xFD
    public const byte SetProperties= 252; // 0xFC
    public const byte GetProperties= 251; // 0xFB
    public const byte ChangeGroups = 248; // 0xF8
    public const byte FindFriends  = 224; // 0xE0
    public const byte CreateGame   = 227; // 0xE3
    public const byte JoinGame     = 226; // 0xE2
    public const byte JoinRandom   = 225; // 0xE1
    public const byte JoinLobby    = 229; // 0xE5
    public const byte LeaveLobby   = 228; // 0xE4
    public const byte Authenticate = 230; // 0xE6
    public const byte GetRegions   = 220; // 0xDC
    public const byte Ping         = 0;
}

/// <summary>Photon event codes.</summary>
public static class EvtCode
{
    public const byte Join        = 255; // 0xFF — someone joined a room
    public const byte Leave       = 254; // 0xFE
    public const byte PropsChanged= 253; // 0xFD
    public const byte RaiseEvent  = 0;   // varies — the evtCode IS the user code
    public const byte GameList    = 230; // 0xE6 — master→client: list of rooms
    public const byte GameListUpd = 229; // 0xE5 — incremental room list update
    public const byte AppStats    = 226; // 0xE2
}

/// <summary>Parameter keys used in Photon operations (confirmed from game source).</summary>
public static class ParamKey
{
    public const byte RoomName       = 255; // 0xFF
    public const byte ActorNr        = 254; // 0xFE
    public const byte TargetActorNr  = 253; // 0xFD
    public const byte ActorList      = 252; // 0xFC
    public const byte PlayerProps    = 249; // 0xF9
    public const byte GameProps      = 248; // 0xF8
    public const byte GameList       = 246; // 0xF6 — array of rooms
    public const byte RoomOptionFlags= 242; // 0xF2
    public const byte CleanupCache   = 241; // 0xF1
    public const byte MaxPlayers     = 239; // 0xEF
    public const byte IsOpen         = 237; // 0xED
    public const byte IsVisible      = 236; // 0xEC
    public const byte ExpectedUsers  = 235; // 0xEB
    public const byte GameServer     = 230; // 0xE6 — game server address "ip:port"
    public const byte AppId          = 224; // 0xE0
    public const byte AppVersion     = 220; // 0xDC
    public const byte userId         = 225; // 0xE1
    public const byte SecretOrToken  = 221; // 0xDD
    public const byte LobbyName      = 213; // 0xD5
    public const byte LobbyType      = 212; // 0xD4
    public const byte ClientKey      = 209; // 0xD1 — crypto
    public const byte EventCode      = 244; // 0xF4
    public const byte Data           = 245; // 0xF5
    public const byte Code           = 244; // 0xF4
    public const byte Position       = 247; // 0xF7
    public const byte ServerTime     = 228; // 0xE4
    public const byte RoomCount      = 227; // 0xE3
    public const byte PeerCount      = 226; // 0xE2
    public const byte MasterPeerCount= 225; // -- reused
    // custom room properties are sent inside GameProps hashtable
}

/// <summary>
/// Encodes and decodes Photon binary protocol messages.
/// All integers are big-endian (network byte order).
/// </summary>
public static class PhotonBinaryProtocol
{
    // ───────────────────────── Constants ──────────────────────────────────
    private const byte TcpMagic    = 0xF3;
    private const int  HeaderLen   = 9;
    private const byte InitMarker  = 0xFB;
    private const byte NullByte    = GpType.Null;

    // 9-byte "You're connected" response sent after receiving an init packet.
    // TConnect.Run() reads the 9-byte header, extracts payload length from bytes[1-4],
    // writes bytes[7..8] to opCollectionStream, then reads <payloadLen-9> body bytes
    // and appends them.  The resulting buffer is passed to ReceiveIncomingCommands()
    // which queues it for DispatchIncomingCommands → DeserializeMessageAndCallback().
    //
    // DeserializeMessageAndCallback layout:
    //   inBuff[0]       must be 0xF3 (243) or 0xFD (253) — outer magic check
    //   inBuff[1]&0x7F  messageType (V_0): switch(V_0 - 1):
    //       0 (V_0=1) → DeserializeOperationResponse  (null stream → ignored)
    //       1 (V_0=2) → EventData
    //       2 (V_0=3) → this.InitCallback()            ← WE WANT THIS
    //       3 (V_0=4) → second OperationResponse handler
    // bytes[7]=0xF3 is the inner magic marker; ReceiveIncomingCommands requires inbuff[0]=0xF3.
    // bytes[8]=0x01: b=1 → switch case 0 → InitCallback() (null stream special case, no deserialization).
    public static readonly byte[] InitResponse =
    {
        0xF3, 0x00, 0x00, 0x00, 0x09,  // first-byte marker + total length = 9
        0x00, 0x00,                     // channel, flags
        0xF3, 0x01                      // [7]=inner magic (required by ReceiveIncomingCommands), [8]=b=1 → InitCallback()
    };

    // ─────────────────────────── Read ─────────────────────────────────────

    /// <summary>
    /// Tries to read the size of the next complete frame from a partial buffer.
    /// Returns -1 if not enough bytes yet.
    /// Returns 0 if it is an init packet (caller should read exactly 48 bytes).
    /// Returns N > 0 if it is a regular message (caller should read exactly N bytes).
    /// </summary>
    public static int PeekFrameSize(byte[] buf, int available)
    {
        if (available < 1)
            return -1;

        if (buf[0] == 0xF0)
        {
            // Photon ping frame: exactly 5 bytes {0xF0, sentTime(4 bytes)}
            return available >= 5 ? 5 : -1;
        }

        // 0xFB is used for BOTH the init packet AND all regular TCP frames (TcpMagic == InitMarker == 0xFB).
        // 0xF3 and 0xF4 are also valid regular frame markers.
        // For all of these, read the actual frame length from bytes[1..4].
        if (buf[0] != 0xFB && buf[0] != 0xF3 && buf[0] != 0xF4)
            return -1; // unexpected byte — discard

        if (available < 5)
            return -1;

        int len = ReadInt32BE(buf, 1);
        if (len < 5) return -1; // sanity check
        return available >= len ? len : -1;
    }

    /// <summary>
    /// Parses a complete frame buffer (already validated to have the right length).
    /// Returns an IncomingMessage or null if frame is not a normal op.
    /// </summary>
    public static IncomingMessage? ParseFrame(byte[] frame)
    {
        if (frame[0] == 0xF0)
        {
            // Ping / keepalive frame from client
            return new IncomingMessage { IsPing = true, PingFrame = frame };
        }

        if (frame.Length < 9)
            return null;

        byte msgType = frame[8];

        // True init packet: 0xFB marker, length=48, msgType=0
        if (frame[0] == 0xFB && frame.Length == 48 && msgType == 0x00)
        {
            // Parse the AppID from bytes[16..47]
            string appId = Encoding.UTF8.GetString(frame, 16, 32).TrimEnd('\0');
            return new IncomingMessage { IsInit = true, AppId = appId };
        }

        using var ms = new MemoryStream(frame, 9, frame.Length - 9);
        return msgType switch
        {
            2 => new IncomingMessage { OpRequest = DeserializeOpRequest(ms) }, // OpRequest (client→server)
            3 => new IncomingMessage { OpRequest = DeserializeOpRequest(ms) }, // OpResponse internal
            7 => new IncomingMessage { OpRequest = DeserializeOpRequest(ms) }, // internal op
            _ => null
        };
    }

    // Handle a raw 0xF0 / ping frame (byte[0] == 0xF0)
    public static bool IsPingFrame(byte[] frame) =>
        frame.Length > 0 && frame[0] == 0xF0;

    /// <summary>
    /// Build a 9-byte ping response for TConnect.Run() which always reads 9-byte headers.
    /// Client ping:    {0xF0, sentTime[4 BE]}           — 5 bytes
    /// Server reply:   {0xF0, serverTime[4 BE], sentTime[4 BE]}  — 9 bytes
    /// TPeer.ReadPingResult() reads bytes[1..4] as serverTime and bytes[5..8] as sentTime.
    /// </summary>
    public static byte[] BuildPingResponse(byte[]? clientPingFrame)
    {
        var resp = new byte[9];
        resp[0] = 0xF0;
        int serverMs = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);
        resp[1] = (byte)(serverMs >> 24);
        resp[2] = (byte)(serverMs >> 16);
        resp[3] = (byte)(serverMs >> 8);
        resp[4] = (byte)(serverMs);
        // echo back client's sent-time (bytes 1–4 of the 5-byte ping)
        if (clientPingFrame != null && clientPingFrame.Length >= 5)
        {
            resp[5] = clientPingFrame[1];
            resp[6] = clientPingFrame[2];
            resp[7] = clientPingFrame[3];
            resp[8] = clientPingFrame[4];
        }
        return resp;
    }

    // ─────────────────────── Write helpers ────────────────────────────────

    /// <summary>Builds a complete TCP frame for an OperationResponse.</summary>
    public static byte[] BuildOpResponse(
        byte opCode,
        short returnCode,
        Dictionary<byte, object>? parameters,
        string? debugMsg = null)
    {
        using var payload = new MemoryStream(64);
        payload.WriteByte(opCode);
        WriteInt16BE(payload, returnCode);

        if (string.IsNullOrEmpty(debugMsg))
        {
            payload.WriteByte(NullByte);
        }
        else
        {
            // Write type byte then the string data
            WriteStringWithType(payload, debugMsg);
        }

        SerializeParamTable(payload, parameters);

        return WrapTcpFrame(EgMsgType.OperationResponse, payload.ToArray());
    }

    /// <summary>Builds a complete TCP frame for an EventData.</summary>
    public static byte[] BuildEvent(byte eventCode, Dictionary<byte, object>? parameters)
    {
        using var payload = new MemoryStream(64);
        payload.WriteByte(eventCode);
        SerializeParamTable(payload, parameters);
        return WrapTcpFrame(EgMsgType.EventData, payload.ToArray());
    }

    // ─────────────────────── Type serializers ─────────────────────────────

    /// <summary>Writes a value with its type prefix byte (setType = true mode).</summary>
    public static void WriteValueWithType(MemoryStream out_, object? value)
    {
        if (value == null)
        {
            out_.WriteByte(GpType.Null);
            return;
        }

        switch (value)
        {
            case bool b:
                out_.WriteByte(GpType.Boolean);
                out_.WriteByte(b ? (byte)1 : (byte)0);
                break;

            case byte bv:
                out_.WriteByte(GpType.Byte);
                out_.WriteByte(bv);
                break;

            case short sv:
                out_.WriteByte(GpType.Short);
                WriteInt16BE(out_, sv);
                break;

            case int iv:
                out_.WriteByte(GpType.Integer);
                WriteInt32BE(out_, iv);
                break;

            case long lv:
                out_.WriteByte(GpType.Long);
                WriteInt64BE(out_, lv);
                break;

            case float fv:
                out_.WriteByte(GpType.Float);
                WriteFloatBE(out_, fv);
                break;

            case double dv:
                out_.WriteByte(GpType.Double);
                WriteDoubleBE(out_, dv);
                break;

            case string sv:
                WriteStringWithType(out_, sv);
                break;

            case byte[] ba:
                out_.WriteByte(GpType.ByteArray);
                WriteInt32BE(out_, ba.Length);
                out_.Write(ba, 0, ba.Length);
                break;

            case string[] sa:
                out_.WriteByte(GpType.StringArray);
                WriteInt16BE(out_, (short)sa.Length);
                foreach (var s in sa)
                    WriteStringRaw(out_, s);
                break;

            case int[] ia:
                // Stored as typed Array: type byte 'y', count, element type 'i', then packed
                out_.WriteByte(GpType.Array);
                WriteInt16BE(out_, (short)ia.Length);
                out_.WriteByte(GpType.Integer); // element type
                foreach (var i in ia)
                    WriteInt32BE(out_, i);
                break;

            case Hashtable ht:
                out_.WriteByte(GpType.Hashtable);
                WriteInt16BE(out_, (short)ht.Count);
                foreach (DictionaryEntry entry in ht)
                {
                    WriteValueWithType(out_, entry.Key);
                    WriteValueWithType(out_, entry.Value);
                }
                break;

            case object[] oa:
                out_.WriteByte(GpType.ObjectArray);
                WriteInt16BE(out_, (short)oa.Length);
                foreach (var o in oa)
                    WriteValueWithType(out_, o);
                break;

            default:
                // Fallback: try to handle boxed primitives from interfaces
                throw new NotSupportedException($"Cannot serialize type {value.GetType().Name}");
        }
    }

    /// <summary>Writes a parameter table without the type prefix for the table itself.</summary>
    public static void SerializeParamTable(MemoryStream out_, Dictionary<byte, object>? table)
    {
        if (table == null || table.Count == 0)
        {
            WriteInt16BE(out_, 0);
            return;
        }
        WriteInt16BE(out_, (short)table.Count);
        foreach (var kv in table)
        {
            out_.WriteByte(kv.Key);
            WriteValueWithType(out_, kv.Value);
        }
    }

    // ─────────────────────── Type deserializers ───────────────────────────

    public static OpRequestData DeserializeOpRequest(MemoryStream ms)
    {
        byte opCode = (byte)ms.ReadByte();
        var parameters = DeserializeParamTable(ms);
        return new OpRequestData(opCode, parameters);
    }

    public static Dictionary<byte, object?> DeserializeParamTable(MemoryStream ms)
    {
        short count = ReadInt16BE(ms);
        var result = new Dictionary<byte, object?>(count < 0 ? 0 : count);
        for (int i = 0; i < count; i++)
        {
            byte key = (byte)ms.ReadByte();
            byte typeCode = (byte)ms.ReadByte();
            object? value = DeserializeValue(ms, typeCode);
            result[key] = value;
        }
        return result;
    }

    public static object? DeserializeValue(MemoryStream ms, byte type)
    {
        return type switch
        {
            GpType.Null      => null,
            GpType.Boolean   => ms.ReadByte() != 0,
            GpType.Byte      => (object)(byte)ms.ReadByte(),
            GpType.Short     => (object)ReadInt16BE(ms),
            GpType.Integer   => (object)ReadInt32BE(ms),
            GpType.Long      => (object)ReadInt64BE(ms),
            GpType.Float     => (object)ReadFloatBE(ms),
            GpType.Double    => (object)ReadDoubleBE(ms),
            GpType.String    => ReadStringRaw(ms),
            GpType.ByteArray => ReadByteArray(ms),
            GpType.Hashtable => ReadHashtable(ms),
            GpType.Array     => ReadTypedArray(ms),
            GpType.ObjectArray => ReadObjectArray(ms),
            GpType.Dictionary  => ReadDictionary(ms),
            GpType.StringArray => ReadStringArray(ms),
            _                => throw new NotSupportedException($"Unsupported GpType: {type}")
        };
    }

    // ─────────────────────── Primitive helpers ────────────────────────────

    private static void WriteInt16BE(MemoryStream s, short v)
    {
        s.WriteByte((byte)(v >> 8));
        s.WriteByte((byte)(v & 0xFF));
    }

    private static void WriteInt32BE(MemoryStream s, int v)
    {
        s.WriteByte((byte)(v >> 24));
        s.WriteByte((byte)(v >> 16));
        s.WriteByte((byte)(v >> 8));
        s.WriteByte((byte)(v & 0xFF));
    }

    private static void WriteInt64BE(MemoryStream s, long v)
    {
        for (int i = 56; i >= 0; i -= 8)
            s.WriteByte((byte)(v >> i));
    }

    private static void WriteFloatBE(MemoryStream s, float v)
    {
        byte[] b = BitConverter.GetBytes(v);
        if (BitConverter.IsLittleEndian) Array.Reverse(b);
        s.Write(b, 0, 4);
    }

    private static void WriteDoubleBE(MemoryStream s, double v)
    {
        byte[] b = BitConverter.GetBytes(v);
        if (BitConverter.IsLittleEndian) Array.Reverse(b);
        s.Write(b, 0, 8);
    }

    /// <summary>Writes 0x73 + 2-byte len + UTF8 bytes.</summary>
    private static void WriteStringWithType(MemoryStream s, string v)
    {
        s.WriteByte(GpType.String);
        WriteStringRaw(s, v);
    }

    /// <summary>Writes 2-byte BE length + UTF8 bytes (no type prefix).</summary>
    private static void WriteStringRaw(MemoryStream s, string v)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        WriteInt16BE(s, (short)bytes.Length);
        s.Write(bytes, 0, bytes.Length);
    }

    private static int ReadInt32BE(byte[] b, int offset) =>
        (b[offset] << 24) | (b[offset + 1] << 16) | (b[offset + 2] << 8) | b[offset + 3];

    private static short ReadInt16BE(MemoryStream ms)
    {
        int hi = ms.ReadByte();
        int lo = ms.ReadByte();
        return (short)((hi << 8) | lo);
    }

    private static int ReadInt32BE(MemoryStream ms)
    {
        int a = ms.ReadByte(), b = ms.ReadByte(), c = ms.ReadByte(), d = ms.ReadByte();
        return (a << 24) | (b << 16) | (c << 8) | d;
    }

    private static long ReadInt64BE(MemoryStream ms)
    {
        long v = 0;
        for (int i = 56; i >= 0; i -= 8)
            v |= (long)ms.ReadByte() << i;
        return v;
    }

    private static float ReadFloatBE(MemoryStream ms)
    {
        byte[] b = new byte[4];
        ms.Read(b, 0, 4);
        if (BitConverter.IsLittleEndian) Array.Reverse(b);
        return BitConverter.ToSingle(b, 0);
    }

    private static double ReadDoubleBE(MemoryStream ms)
    {
        byte[] b = new byte[8];
        ms.Read(b, 0, 8);
        if (BitConverter.IsLittleEndian) Array.Reverse(b);
        return BitConverter.ToDouble(b, 0);
    }

    private static string ReadStringRaw(MemoryStream ms)
    {
        short len = ReadInt16BE(ms);
        if (len <= 0) return string.Empty;
        byte[] b = new byte[len];
        ms.Read(b, 0, len);
        return Encoding.UTF8.GetString(b);
    }

    private static byte[] ReadByteArray(MemoryStream ms)
    {
        int len = ReadInt32BE(ms);
        byte[] b = new byte[len];
        ms.Read(b, 0, len);
        return b;
    }

    private static Hashtable ReadHashtable(MemoryStream ms)
    {
        short count = ReadInt16BE(ms);
        var ht = new Hashtable(count);
        for (int i = 0; i < count; i++)
        {
            byte keyType = (byte)ms.ReadByte();
            object? key = DeserializeValue(ms, keyType);
            byte valType = (byte)ms.ReadByte();
            object? val = DeserializeValue(ms, valType);
            if (key != null) ht[key] = val;
        }
        return ht;
    }

    private static Array ReadTypedArray(MemoryStream ms)
    {
        short count = ReadInt16BE(ms);
        byte elemType = (byte)ms.ReadByte();
        var arr = new object?[count];
        for (int i = 0; i < count; i++)
            arr[i] = DeserializeValue(ms, elemType);
        return arr;
    }

    private static object?[] ReadObjectArray(MemoryStream ms)
    {
        short count = ReadInt16BE(ms);
        var arr = new object?[count];
        for (int i = 0; i < count; i++)
        {
            byte t = (byte)ms.ReadByte();
            arr[i] = DeserializeValue(ms, t);
        }
        return arr;
    }

    private static Dictionary<object, object?> ReadDictionary(MemoryStream ms)
    {
        byte keyTypeCode = (byte)ms.ReadByte();
        byte valTypeCode = (byte)ms.ReadByte();
        bool keyHasType  = keyTypeCode == 0;
        bool valHasType  = valTypeCode == 0;

        short count = ReadInt16BE(ms);
        var dict = new Dictionary<object, object?>(count);
        for (int i = 0; i < count; i++)
        {
            byte kt = keyHasType ? (byte)ms.ReadByte() : keyTypeCode;
            object? k = DeserializeValue(ms, kt);
            byte vt = valHasType ? (byte)ms.ReadByte() : valTypeCode;
            object? v = DeserializeValue(ms, vt);
            if (k != null) dict[k] = v;
        }
        return dict;
    }

    private static string[] ReadStringArray(MemoryStream ms)
    {
        short count = ReadInt16BE(ms);
        var arr = new string[count];
        for (int i = 0; i < count; i++)
            arr[i] = ReadStringRaw(ms);
        return arr;
    }

    // ─────────────────────── Frame wrapper ────────────────────────────────

    private static byte[] WrapTcpFrame(byte msgType, byte[] payload)
    {
        int total = HeaderLen + payload.Length;
        byte[] frame = new byte[total];
        frame[0] = TcpMagic;
        frame[1] = (byte)(total >> 24);
        frame[2] = (byte)(total >> 16);
        frame[3] = (byte)(total >> 8);
        frame[4] = (byte)(total & 0xFF);
        frame[5] = 0x00; // channel
        frame[6] = 0x00; // flags (no encryption)
        frame[7] = TcpMagic; // inner magic marker — required: ReceiveIncomingCommands checks inbuff[0]==0xF3
        frame[8] = msgType;
        Buffer.BlockCopy(payload, 0, frame, HeaderLen, payload.Length);
        return frame;
    }
}

// ─────────────────────────── Data types ───────────────────────────────────────

public record OpRequestData(byte OpCode, Dictionary<byte, object?> Parameters);

public class IncomingMessage
{
    public bool IsInit   { get; init; }
    public bool IsPing   { get; init; }
    public byte[]? PingFrame { get; init; }   // raw 5-byte client ping, used to build the 9-byte server response
    public string? AppId { get; init; }
    public OpRequestData? OpRequest { get; init; }
}
