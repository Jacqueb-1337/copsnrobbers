using System.Collections;

namespace CNRLanServer;

/// <summary>
/// A room on the game server. Mirrors the customProperties expected by
/// CNRRoomInfo.Update() (keys: "map", "version", "mode") and the standard
/// Photon room properties (maxPlayers, isOpen, isVisible).
/// </summary>
public class RoomInfo
{
    public string  Name        { get; }
    public string  Map         { get; set; } = "FreeRun3_1";
    public string  Mode        { get; set; } = "0"; // 0=TDM 1=Stronghold 2=KC
    public string  Version     { get; set; } = "";  // must match client's version
    public byte    MaxPlayers  { get; set; } = 10;
    public bool    IsOpen      { get; set; } = true;
    public bool    IsVisible   { get; set; } = true;
    public bool    IsActive    => Players.Count > 0 || CreatedAt + TimeSpan.FromMinutes(5) > DateTime.UtcNow;

    public List<PlayerSession> Players { get; } = new();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    private int _nextActorNr = 1;

    public RoomInfo(string name) => Name = name;

    public int AssignActorNr()
    {
        int nr = _nextActorNr++;
        return nr;
    }

    public int PlayerCount => Players.Count;

    /// <summary>Builds a Hashtable with this room's custom properties for the game-list event.</summary>
    public Hashtable GetCustomPropertiesHashtable()
    {
        var ht = new Hashtable
        {
            ["map"]     = Map,
            ["mode"]    = Mode,
            ["version"] = Version
        };
        return ht;
    }

    /// <summary>Builds the full Photon game-props Hashtable (for OpJoin response).
    /// Uses the byte keys expected by client RoomInfo.CacheProperties:
    ///   255=maxPlayers, 253=isOpen, 254=isVisible, 252=playerCount
    /// String keys for custom props are merged in by MergeStringKeys.
    /// </summary>
    public Hashtable GetGamePropsHashtable()
    {
        var ht = GetCustomPropertiesHashtable(); // string keys: map, version, mode
        ht[(byte)255] = MaxPlayers;           // maxPlayers — byte.MaxValue key
        ht[(byte)253] = IsOpen;               // isOpen
        ht[(byte)254] = IsVisible;            // isVisible
        ht[(byte)252] = (byte)PlayerCount;    // playerCount (byte)
        ht[(byte)251] = false;                // removedFromList
        return ht;
    }
}

/// <summary>
/// Tracks all rooms across both master and game server.
/// Thread-safe (all public methods lock on _lock).
/// </summary>
public class RoomManager
{
    private readonly Dictionary<string, RoomInfo> _rooms = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public RoomInfo? GetRoom(string name)
    {
        lock (_lock)
        {
            _rooms.TryGetValue(name, out var r);
            return r;
        }
    }

    public RoomInfo GetOrCreateRoom(string name)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(name, out var r))
            {
                r = new RoomInfo(name);
                _rooms[name] = r;
                Console.WriteLine($"[RoomMgr] Created room '{name}'");
            }
            return r;
        }
    }

    public void DeleteRoom(string name)
    {
        lock (_lock)
        {
            _rooms.Remove(name);
            Console.WriteLine($"[RoomMgr] Deleted room '{name}'");
        }
    }

    public List<RoomInfo> GetVisibleRooms()
    {
        lock (_lock)
        {
            return _rooms.Values
                .Where(r => r.IsVisible && r.IsOpen && r.IsActive)
                .ToList();
        }
    }

    public void Prune()
    {
        lock (_lock)
        {
            var toRemove = _rooms.Values
                .Where(r => !r.IsActive)
                .Select(r => r.Name)
                .ToList();
            foreach (var n in toRemove)
            {
                _rooms.Remove(n);
                Console.WriteLine($"[RoomMgr] Pruned stale room '{n}'");
            }
        }
    }
}
