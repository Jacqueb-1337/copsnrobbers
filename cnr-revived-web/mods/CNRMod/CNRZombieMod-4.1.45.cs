// CNRZombieMod.cs — v0.3.0
// Zombie multiplayer test mod for Cops N Robbers.
//
// Reuses the game's own singleplayer AI infrastructure:
//   - Enemy prefab:       grabbed from Game.mInstance.enemyBot3 via reflection
//                         and cached via DontDestroyOnLoad.  The user must load
//                         a scene that exposes the enemy prefab reference at
//                         least once per session so the reference is available.
//   - SingleEnemyAI:      the game's AIPath-based enemy movement controller.
//                         Added to / already on the spawned enemy GO.  Provides
//                         full A* pathfinding when AstarPath.active != null in
//                         the current scene, otherwise falls back to direct
//                         vector movement.
//   - SingleEnemyLogic:   the full state machine (idle→patrol→rush→attack→escape)
//                         is disabled before its Start() fires (to avoid null
//                         singleton crashes in multiplayer scenes).  ZombieDriver
//                         replaces it with a simple chase-only state for the test.
//
// Sync: master client broadcasts enemy transforms via Photon event 198 (float[]
//   packed: id,x,y,z,rotY per enemy) every ~0.3 s.
//   Non-master clients receive and lerp their local copies (AI disabled).
//
// Entry point: ZombieModEntry.Load() — found by CNRMod's DLL scanner.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRZombieMod
{
    // ─────────────────────────────────────────────────────────────────────────
    // ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieModEntry
    {
        public const  string Version = "0.9.0";
        public const  byte   ZOMBIE_EVENT = 196;       // zombie simulation/state; keep separate from CNRMod fast visual sync 198
        public const  byte   ZOMBIE_MAP_EVENT = 195;   // overlay map request/snapshot/state
        private const string LogPath      = "/storage/emulated/0/CNRMods/zombiemod.log";
        private static readonly string[] QuietPrefixes = new string[] {
            "ZAI[", "NavDebug", "EnemyRendererDump", "DumpEnemy3", "TryCachePrefab",
            "FindSourcePrefab", "Broadcast:", "RaiseZombieEvent:", "OnZombieEvent:",
            "ApplyZombieDamageRequest:", "ZombieDamage:", "ReportZombieDamage:",
            "ApplyPlayerDamageToPeer:", "ZombieDamagePeer:", "GetNearestPlayerTarget:",
            "AddRemotePlayerTargets:", "ZombieProxy[", "ZombieDriver[", "PrepareZombieVisuals:",
            "EnemyOverlay:", "ZombieSkin:", "ApplyZombieHealthSync:"
        };

        private static bool _loaded = false;  // singleton guard

        public static void Load()
        {
            if (_loaded) return;   // mod loader may call Load() more than once (e.g. scene reload)
            _loaded = true;
            try
            {
                var go = new GameObject("CNRZombieMod_Root");
                go.AddComponent<ZombieHook>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                try
                {
                    string dir = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(LogPath, "");
                }
                catch { }
                Log("=== CNRZombieMod v" + Version + " loaded ===");

                // Register with CNRMod so it appears in the mod manager list
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null) continue;
                    MethodInfo reg = me.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    if (reg != null) reg.Invoke(null, new object[] { "CNRZombieMod", Version });
                    break;
                }
            }
            catch (Exception ex) { Log("Load error: " + ex); }
        }

        public static void Log(string msg)
        {
            if (!ShouldLog(msg)) return;
            try { File.AppendAllText(LogPath,
                      "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[ZombieMod] " + msg); } catch { }
        }

        private static bool ShouldLog(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return false;
            if (msg.IndexOf("err", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("GAME OVER", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("DOWNED", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("end round", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("begin round", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("started", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("loaded", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (msg.IndexOf("forced to cops", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            for (int i = 0; i < QuietPrefixes.Length; i++)
            {
                if (msg.StartsWith(QuietPrefixes[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PHOTON EVENT PROXY
    // Wraps whichever IPhotonPeerListener is installed as
    //   NetworkingPeer.externalListener (may already be CNRMod's proxy).
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombiePhotonProxy : IPhotonPeerListener
    {
        private readonly IPhotonPeerListener _orig;
        private readonly ZombieHook          _hook;

        public ZombiePhotonProxy(IPhotonPeerListener orig, ZombieHook hook)
        { _orig = orig; _hook = hook; }

        public void OnEvent(EventData ev)
        {
            if (_hook != null)
            {
                if (ev.Code == ZombieModEntry.ZOMBIE_EVENT)
                    _hook.OnZombieEvent(ev);
                else if (ev.Code == ZombieModEntry.ZOMBIE_MAP_EVENT)
                    _hook.OnZombieMapEvent(ev);
            }
            _orig.OnEvent(ev);
        }
        public void DebugReturn(DebugLevel l, string m) { _orig.DebugReturn(l, m); }
        public void OnOperationResponse(OperationResponse r) { _orig.OnOperationResponse(r); }
        public void OnStatusChanged(StatusCode c) { _orig.OnStatusChanged(c); }
    }

    // Destroys per-instance materials/meshes created for a zombie GO when the
    // GO dies. Without this, every spawned zombie leaked its cloned materials
    // (Unity never garbage-collects instantiated Materials/Meshes on its own).
    public class ZombieAssetCleanup : MonoBehaviour
    {
        private readonly List<UnityEngine.Object> _owned = new List<UnityEngine.Object>();

        public void Own(UnityEngine.Object o)
        {
            if (o != null) _owned.Add(o);
        }

        void OnDestroy()
        {
            for (int i = 0; i < _owned.Count; i++)
            {
                if (_owned[i] != null)
                {
                    try { UnityEngine.Object.Destroy(_owned[i]); } catch { }
                }
            }
            _owned.Clear();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE HOOK — persistent MonoBehaviour (DontDestroyOnLoad root)
    // ─────────────────────────────────────────────────────────────────────────
    [Serializable]
    public class ZombieMapConfig
    {
        public int version;
        public string scene;
        public ZombieSpawnPoint[] zombieSpawns;
        public ZombieWallBuy[] wallBuys;
        public ZombieDoor[] doors;
        public ZombieBarrier[] barriers;
        public ZombieCustomObject[] customObjects;
    }

    [Serializable]
    public class ZombieSpawnPoint
    {
        public string id;
        public string zone;
        public float[] pos;
        public float minDistance;
        public float maxDistance;
    }

    [Serializable]
    public class ZombieWallBuy
    {
        public string id;
        public string weaponId;
        public string runtimeName;
        public string displayName;
        public string slot;
        public int price;
        public int ammoPrice;
        public float[] pos;
        public float[] rot;
        public float interactRadius;
        public float scale;
    }

    [Serializable]
    public class ZombieDoor
    {
        public string id;
        public string displayName;
        public int price;
        public float[] pos;
        public float[] rot;
        public float[] size;
        public float interactRadius;
        public bool initiallyOpen;
    }

    [Serializable]
    public class ZombieBarrier
    {
        public string id;
        public float[] pos;
        public float[] rot;
        public float[] size;
        public int maxBoards;
        public int repairReward;
        public float interactRadius;
    }

    [Serializable]
    public class ZombieCustomObject
    {
        public string id;
        public string assetUrl;
        public float[] pos;
        public float[] rot;
        public float[] scale;
        public bool collidable = true;
        public string interactionType;
        public string interactionId;
        public int price;
        public float interactRadius;
        public float upgradeSeconds;
    }

    [Serializable]
    public class ZombieObjectAsset
    {
        public int version;
        public string id;
        public string name;
        public ZombieObjectMesh[] meshes;
    }

    [Serializable]
    public class ZombieObjectMesh
    {
        public string name;
        public float[] vertices;
        public float[] normals;
        public float[] uv;
        public int[] triangles;
        public string materialMode;
        public string materialName;
        public string textureUrl;
        public string textureData;
        public float[] color;
        public bool collidable = true;
    }

    public class ZombieWeaponRegistration
    {
        public string Id;
        public string DisplayName;
        public string Slot;
        public string RuntimeName;

        public ZombieWeaponRegistration(string id, string displayName, string slot, string runtimeName)
        {
            Id = id;
            DisplayName = displayName;
            Slot = slot;
            RuntimeName = runtimeName;
        }
    }

    // Public extension point for future DLC/mod weapon assemblies. A future gun mod only
    // needs to register the runtime WeaponScript name and the Zombies slot it belongs to;
    // map JSON can keep referring to a stable weaponId.
    public static class ZombieWeaponRegistry
    {
        private static readonly Dictionary<string, ZombieWeaponRegistration> _registered
            = new Dictionary<string, ZombieWeaponRegistration>(StringComparer.OrdinalIgnoreCase);

        public static void Register(string id, string displayName, string slot, string runtimeName)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(runtimeName)) return;
            _registered[id] = new ZombieWeaponRegistration(id, displayName, slot, runtimeName);
        }

        public static bool TryGet(string id, out ZombieWeaponRegistration registration)
        {
            registration = null;
            if (string.IsNullOrEmpty(id)) return false;
            return _registered.TryGetValue(id, out registration);
        }
    }

    public static class ZombieBalance
    {
        // This is intentionally gentler than the classic Treyarch health curve. Difficulty
        // should come from the whole system (more zombies, faster movement, perks/weapon-upgrade choices),
        // not runaway bullet-sponge health before those player-side systems exist.
        public static float ExpectedPlayerPower(int round)
        {
            if (round <= 1) return 1f;
            if (round <= 5) return Mathf.Lerp(1f, 1.25f, (round - 1) / 4f);
            if (round <= 10) return Mathf.Lerp(1.25f, 1.65f, (round - 5) / 5f);
            if (round <= 15) return Mathf.Lerp(1.65f, 2.15f, (round - 10) / 5f);
            if (round <= 20) return Mathf.Lerp(2.15f, 2.75f, (round - 15) / 5f);
            if (round <= 25) return Mathf.Lerp(2.75f, 3.30f, (round - 20) / 5f);
            if (round <= 30) return Mathf.Lerp(3.30f, 3.80f, (round - 25) / 5f);
            return Mathf.Min(6f, 3.80f + (round - 30) * 0.07f);
        }

        public static float HealthForRound(int round)
        {
            round = Mathf.Max(1, round);
            float power = ExpectedPlayerPower(round);
            // Preserve 100 HP at round 1. From there HP tracks expected weapon/perk power
            // slightly below one-for-one, with a small round-pressure term.
            float hp = 100f + (power - 1f) * 95f + (round - 1) * 5f;
            return Mathf.Max(100f, Mathf.Round(hp));
        }

        public static float ChaseSpeedForRound(int round)
        {
            round = Mathf.Max(1, round);
            return Mathf.Min(4.65f, 2.80f + (round - 1) * 0.075f);
        }

        public static int AttackDamageForRound(int round)
        {
            round = Mathf.Max(1, round);
            return Mathf.Min(20, 8 + Mathf.FloorToInt((round - 1) / 3f));
        }

        public static float AttackCooldownForRound(int round)
        {
            round = Mathf.Max(1, round);
            return Mathf.Max(0.90f, 1.50f - (round - 1) * 0.025f);
        }
    }

    public class ZombieHook : MonoBehaviour
    {
        // ── Tunables ────────────────────────────────────────────────────────
        private const int   STARTING_POINTS = 500;
        private const string ENEMY_DUMP_DIR = "/storage/emulated/0/CNRMods/Enemy3Dump";
        private const int   BASE_ZOMBIES_PER_ROUND = 6;
        private static int MAX_ZOMBIES_PER_ROUND { get { return CNRMods.CNRMatchSettingsRuntimeHook.ZombieMaxPerRound; } }
        private const int   MAX_ACTIVE_ZOMBIES = 12;
        private const float ZOMBIES_PER_ROUND_MULT = 0.75f;
        private static float ROUND_START_DELAY { get { return CNRMods.CNRMatchSettingsRuntimeHook.ZombieStartDelay; } }
        private static float INTER_ROUND_DELAY { get { return CNRMods.CNRMatchSettingsRuntimeHook.ZombieInterRoundDelay; } }
        private const float BASE_SPAWN_INTERVAL = 2.5f;
        private const float SPAWN_INTERVAL_DECAY = 0.08f;
        private const float MIN_SPAWN_INTERVAL = 0.35f;
        private const int   POINTS_PER_KILL = 100;
        private const int   POINTS_PER_ROUND_BONUS = 5;
        private const int   END_ROUND_BONUS = 200;
        private const float SPAWN_RADIUS   = 10f;   // metres around local player
        private const float CHASE_SPEED    = 2.8f;
        private const float BROADCAST_SECS = 0.30f; // position sync interval
        private const int   STRIDE         = 5;     // floats per zombie: id,x,y,z,rotY
        private const int   PHASE_WAITING  = 0;
        private const int   PHASE_ACTIVE   = 1;
        private const int   PHASE_INTER    = 2;
        private const int   PHASE_GAMEOVER = 3;

        // ── Persistent template GO ────────────────────────────────────────────
        // When we first find a source enemy prefab in any scene, we immediately
        // Instantiate it, disable+deactivate it, and DontDestroyOnLoad it.
        // This avoids stale UnityEngine.Object refs: scene-assigned Transform/
        // GameObject fields become null/invalid once their scene unloads, but
        // a DontDestroyOnLoad GO lives until the app quits.
        private static GameObject _templateGO = null;
        private static bool _enemyModelAutoDumped = false;

        // ── Per-scene state ──────────────────────────────────────────────────
        private bool  _masterSpawned;
        private bool  _astarBuilt;     // true once we've built the runtime A* graph
        private bool  _modeStarted;
        private bool  _zombieSessionActive;
        private bool  _loggedWaitingForHostStart;
        private bool  _lastAuthority;
        private bool  _haveAuthorityState;
        private int   _authorityRefreshFrame = -1;
        private float _broadcastTimer;
        private float _spawnTimer;
        private int   _nextZombieId = 1;
        private int   _round;
        private int   _phase = PHASE_WAITING;
        private float _phaseTimer;
        private int   _spawnQueue;
        private int   _zombiesTotal;
        private int   _zombiesKilled;
        private int   _zombiesRemaining;
        private int   _points;

        // Zombies map metadata / interaction runtime.  Map files are separate from the
        // normal custom-map JSON so vanilla scenes can receive Zombies-only annotations.
        private const string ZOMBIE_MAP_BASE_URL = "https://play.jacqueb.me/maps/zombies";
        private const string ZOMBIE_MAP_LOCAL_DIR = "/storage/emulated/0/CNRMods/zombie-maps";
        private const float DEFAULT_INTERACT_RADIUS = 2.6f;
        private const float SPAWN_MIN_DISTANCE = 8f;
        private const float SPAWN_MAX_DISTANCE = 42f;
        private const float SPAWN_IDEAL_DISTANCE = 18f;
        private ZombieMapConfig _zombieMapConfig;
        private string _zombieMapRaw = "";
        private float _zombieMapRequestTimer;
        private readonly HashSet<string> _openedDoors = new HashSet<string>();
        private readonly Dictionary<string, int> _barrierBoards = new Dictionary<string, int>();
        private readonly List<GameObject> _overlayObjects = new List<GameObject>();
        private readonly Dictionary<string, string> _customAssetJsonCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _zombieMapScene = "";
        private bool _zombieMapLoading;
        private bool _zombieMapLoaded;
        private bool _zombieMapFromBuiltin;
        private bool _zombieMapRuntimeReady;
        private readonly List<ZombieWallBuyHologram> _wallBuyHolograms
            = new List<ZombieWallBuyHologram>();
        private readonly Dictionary<int, float> _spawnLastUsed
            = new Dictionary<int, float>();
        private ZombieWallBuyHologram _nearWallBuy;
        private bool _interactionRequested;
        private GUIStyle _interactionStyle;
        private GUIStyle _interactionSubStyle;

        // Zombies owns a transient runtime inventory.  We never write CurWeaponEquiped_*;
        // the player's normal loadout is untouched when the match ends.
        private object _weaponManager;
        private Type _weaponManagerType;
        private Type _weaponScriptType;
        private System.Collections.IList _weaponList;
        private System.Collections.IList _weaponTotal;
        private readonly List<object> _vanillaWeaponList = new List<object>();
        private object _vanillaSelectedWeapon;
        private int _vanillaWeaponIndex = -1;
        private bool _vanillaCanSwitch = true;
        private bool _zombieInventoryApplied;
        private readonly string[] _primarySlots = new string[2];
        private string _meleeSlot = "";
        private string _grenadeSlot = "";
        private bool _zombieInventoryReady;
        private const int MAX_ZOMBIE_WEAPON_STARS = 3;
        private const float WEAPON_DAMAGE_PER_STAR = 0.25f;
        private const float WEAPON_MAG_PER_STAR = 0.25f;
        private const float WEAPON_RESERVE_PER_STAR = 0.35f;
        private const int DEFAULT_UPGRADE_COST = 5000;
        private const float DEFAULT_UPGRADE_SECONDS = 4.0f;
        private readonly Dictionary<string, int> _weaponStars = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _weaponBaseClip = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _weaponBaseReserve = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private bool _weaponUpgradeLocked;
        private string _weaponUpgradeName = "";
        private string _weaponUpgradeSlot = "";
        private float _weaponUpgradeCompleteAt;
        private float _zombieWeaponDiagTimer;
        private ZombieCustomObject _nearUpgradeObject;
        private string _modeMessage = "";
        private float  _modeMessageTimer;
        private readonly Dictionary<byte, ZombieDriver> _drivers
            = new Dictionary<byte, ZombieDriver>();
        private readonly Dictionary<byte, ZombieProxy>  _proxies
            = new Dictionary<byte, ZombieProxy>();
        private readonly Dictionary<byte, float> _zombieHealth
            = new Dictionary<byte, float>();
        private readonly Dictionary<byte, int> _zombieVariant
            = new Dictionary<byte, int>();
        private readonly Dictionary<byte, string> _zombieLastAttacker
            = new Dictionary<byte, string>();
        private readonly HashSet<string> _downedPlayers
            = new HashSet<string>();
        private Transform _spectateTarget;
        private Vector3 _spectateCamLocalPos;
        private Quaternion _spectateCamLocalRot;
        private Transform _spectateCamParent;
        private bool _localModelHidden;
        private readonly List<Collider> _hiddenLocalColliders = new List<Collider>();
        private readonly Dictionary<string, List<Collider>> _hiddenPeerColliders
            = new Dictionary<string, List<Collider>>();
        private readonly Dictionary<string, List<Renderer>> _hiddenPeerRenderers
            = new Dictionary<string, List<Renderer>>();

        // ── Zombie variant skins ─────────────────────────────────────────────
        // zombie1.png = most common, zombie5.png = rarest.
        // Weights are relative integers; a variant is picked by summing them
        // and sampling uniformly over [0, total).
        private static readonly int[] VARIANT_WEIGHTS = { 35, 25, 20, 12, 8 }; // zombie1..5
        private const string SKIN_BASE_URL = "https://play.jacqueb.me/skins/zombie";
        // When non-empty, ALL zombies use this single URL instead of the variant system.
        // Set to "" to restore normal weighted-variant behaviour.
        private const string ZOMBIE_SKIN_TEST_URL = "";
        private static readonly Texture2D[] _skinTextures = new Texture2D[5]; // index 0..4 = zombie1..5
        private static Texture2D _skinTestTexture = null;
        private static bool _skinsLoadStarted = false;
        private static bool _enemyRendererDumped = false;

        private struct AtlasRemap
        {
            public string Part;
            public Rect Src;
            public Rect Dst;

            public AtlasRemap(string part, float sx, float sy, float sw, float sh, float dx, float dy, float dw, float dh)
            {
                Part = part;
                Src = new Rect(sx, sy, sw, sh);
                Dst = new Rect(dx, dy, dw, dh);
            }
        }

        private static readonly AtlasRemap[] ENEMY_BASE_ATLAS = new AtlasRemap[] {
            new AtlasRemap("blood", 0,0,64,32, 48,0,10,1),
            new AtlasRemap(null,17,13,8,5, 0,0,8,5), new AtlasRemap(null,7,13,8,5, 8,0,8,5),
            new AtlasRemap(null,1,7,8,5, 16,0,8,5), new AtlasRemap(null,47,13,8,5, 24,0,8,5),
            new AtlasRemap(null,37,13,8,5, 32,0,8,5), new AtlasRemap(null,27,13,8,5, 40,0,8,5),
            new AtlasRemap(null,47,19,8,6, 0,10,8,6), new AtlasRemap(null,37,19,8,6, 8,10,8,6),
            new AtlasRemap(null,1,12,4,6, 16,10,4,6), new AtlasRemap(null,57,19,4,6, 20,10,4,6),
            new AtlasRemap(null,11,7,4,5, 24,10,4,5), new AtlasRemap(null,57,13,4,5, 28,10,4,5),
            new AtlasRemap(null,7,18,4,7, 0,16,4,7), new AtlasRemap(null,19,18,4,7, 4,16,4,7),
            new AtlasRemap(null,19,25,4,7, 8,16,4,7), new AtlasRemap(null,7,25,4,7, 12,16,4,7),
            new AtlasRemap(null,49,25,4,7, 16,16,4,7), new AtlasRemap(null,13,18,4,7, 20,16,4,7),
            new AtlasRemap(null,1,25,4,7, 24,16,4,7), new AtlasRemap(null,1,18,4,7, 28,16,4,7),
            new AtlasRemap(null,37,25,4,7, 32,16,4,7), new AtlasRemap(null,31,18,4,7, 36,16,4,7),
            new AtlasRemap(null,25,25,4,7, 40,16,4,7), new AtlasRemap(null,13,25,4,7, 44,16,4,7),
            new AtlasRemap(null,43,25,4,7, 48,16,4,7), new AtlasRemap(null,25,18,4,7, 52,16,4,7),
            new AtlasRemap(null,31,25,4,7, 56,16,4,7), new AtlasRemap(null,55,25,4,7, 60,16,4,7),
            new AtlasRemap(null,43,4,4,3, 0,30,4,2), new AtlasRemap(null,13,4,4,3, 4,30,4,2),
            new AtlasRemap(null,37,4,4,3, 8,30,4,2), new AtlasRemap(null,1,4,4,3, 12,30,4,2),
            new AtlasRemap(null,25,4,4,3, 16,30,4,2), new AtlasRemap(null,7,4,4,3, 20,30,4,2),
            new AtlasRemap(null,31,4,4,3, 24,30,4,2), new AtlasRemap(null,19,4,4,3, 28,30,4,2),
        };

        private static readonly AtlasRemap[] ENEMY_LAYER_ATLAS = new AtlasRemap[] {
            new AtlasRemap(null,17,13,8,5, 0,5,8,5), new AtlasRemap(null,7,13,8,5, 8,5,8,5),
            new AtlasRemap(null,1,7,8,5, 16,5,8,5), new AtlasRemap(null,47,13,8,5, 24,5,8,5),
            new AtlasRemap(null,37,13,8,5, 32,5,8,5), new AtlasRemap(null,27,13,8,5, 40,5,8,5),
            new AtlasRemap(null,47,19,8,6, 32,10,8,6), new AtlasRemap(null,37,19,8,6, 40,10,8,6),
            new AtlasRemap(null,1,12,4,6, 48,10,4,6), new AtlasRemap(null,57,19,4,6, 52,10,4,6),
            new AtlasRemap(null,11,7,4,5, 56,10,4,5), new AtlasRemap(null,57,13,4,5, 60,10,4,5),
            new AtlasRemap(null,7,18,4,7, 0,23,4,7), new AtlasRemap(null,19,18,4,7, 4,23,4,7),
            new AtlasRemap(null,19,25,4,7, 8,23,4,7), new AtlasRemap(null,7,25,4,7, 12,23,4,7),
            new AtlasRemap(null,49,25,4,7, 16,23,4,7), new AtlasRemap(null,13,18,4,7, 20,23,4,7),
            new AtlasRemap(null,1,25,4,7, 24,23,4,7), new AtlasRemap(null,1,18,4,7, 28,23,4,7),
            new AtlasRemap(null,37,25,4,7, 32,23,4,7), new AtlasRemap(null,31,18,4,7, 36,23,4,7),
            new AtlasRemap(null,25,25,4,7, 40,23,4,7), new AtlasRemap(null,13,25,4,7, 44,23,4,7),
            new AtlasRemap(null,43,25,4,7, 48,23,4,7), new AtlasRemap(null,25,18,4,7, 52,23,4,7),
            new AtlasRemap(null,31,25,4,7, 56,23,4,7), new AtlasRemap(null,55,25,4,7, 60,23,4,7),
            new AtlasRemap(null,43,4,4,3, 32,30,4,2), new AtlasRemap(null,13,4,4,3, 36,30,4,2),
            new AtlasRemap(null,37,4,4,3, 40,30,4,2), new AtlasRemap(null,1,4,4,3, 44,30,4,2),
            new AtlasRemap(null,25,4,4,3, 48,30,4,2), new AtlasRemap(null,7,4,4,3, 52,30,4,2),
            new AtlasRemap(null,31,4,4,3, 56,30,4,2), new AtlasRemap(null,19,4,4,3, 60,30,4,2),
        };
        // ── Photon proxy ─────────────────────────────────────────────────────
        private bool   _proxyInstalled;
        private object _lastPeer;

        // ── HUD / diagnostics ────────────────────────────────────────────────
        private string _hud  = "";
        private float  _diagTimer = 0f;
        private const float DIAG_INTERVAL = 3f;
        private GUIStyle _styleBig;
        private GUIStyle _styleSmall;
        private GUIStyle _styleDowned;
        private GUIStyle _styleDownedSub;
        // Cached lookups (GameObject.Find / reflection are too slow for per-frame use)
        private GameObject _localPlayerGO;
        private float      _localPlayerFindTimer;
        private string     _cachedPeerId;
        private float      _peerIdCacheTimer;
        private bool  _navDebugEnabled = false;
        private const int   NAV_DEBUG_NODE_STEP = 4;      // draw every Nth GridGraph node
        private const int   NAV_DEBUG_MAX_TILES = 1800;   // keep the overlay cheap on mobile
        private const float NAV_DEBUG_TILE_Y = 0.35f;
        private const float NAV_DEBUG_BLOCK_RADIUS = 90f;
        private const float NAV_DEBUG_MAX_STEP = 1.15f;
        private const float NAV_PATH_MAX_CLIMB = 0.45f;
        private const float NAV_DEBUG_SAMPLE_STEP = 1.0f;
        private const int   NAV_DEBUG_SAMPLE_HALF = 55;   // 111x111 samples around player
        private const float NAV_DEBUG_WALKABLE_NORMAL_Y = 0.72f; // ~44 degrees max slope
        private const int   NAV_DEBUG_MAX_TOP_FACES = 700;
        private const int   NAV_DEBUG_MAX_SIDE_FACES = 900;
        private const int   NAV_DEBUG_MAX_MESH_TOP_TRIS = 4500;
        private const int   NAV_DEBUG_MAX_MESH_SIDE_TRIS = 4500;
        private const int   NAV_DEBUG_MAX_MESH_MARKERS = 600;
        private const int   NAV_DEBUG_MAX_TEXTURED_MARKERS = 1400;
        private const int   NAV_DEBUG_MAX_COLLISION_TRIS = 12000;
        private GameObject  _navDebugRoot;
        private string      _navDebugStatus = "";

        // ── Death / anti-respawn ─────────────────────────────────────────────
        private bool        _localPlayerDowned;
        private FieldInfo   _fPlMInstance;
        private FieldInfo   _fPlBlood;
        private FieldInfo   _fPlBDied;
        private FieldInfo   _fPlMStatus;
        private FieldInfo   _fPlGoDied;
        private FieldInfo   _fPlKilledNum;
        private List<string> _killFeed      = new List<string>();
        private float        _killFeedTimer;
        private const float  KILL_FEED_SECS = 5f;

        // ─────────────────────────────────────────────────────────────────────
        // Unity messages
        // ─────────────────────────────────────────────────────────────────────

        void Start()
        {
            TryInstallProxy();
            if (!_skinsLoadStarted)
            {
                _skinsLoadStarted = true;
                StartCoroutine(LoadZombieSkins());
            }
        }

        private System.Collections.IEnumerator LoadZombieSkins()
        {
            // Test mode: load a single skin for all variants
            if (ZOMBIE_SKIN_TEST_URL.Length > 0)
            {
                WWW www = new WWW(ZOMBIE_SKIN_TEST_URL);
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    _skinTestTexture = www.texture;
                    ConfigureSkinTexture(_skinTestTexture);
                    ZombieModEntry.Log("ZombieSkin: TEST skin loaded from " + ZOMBIE_SKIN_TEST_URL);
                }
                else
                {
                    ZombieModEntry.Log("ZombieSkin: TEST skin FAILED: " + www.error);
                }
                yield break;
            }

            for (int i = 0; i < 5; i++)
            {
                int idx = i; // capture for closure
                string url = SKIN_BASE_URL + (idx + 1) + ".png";
                WWW www = new WWW(url);
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    _skinTextures[idx] = www.texture;
                    ConfigureSkinTexture(_skinTextures[idx]);
                    ZombieModEntry.Log("ZombieSkin: loaded variant " + (idx + 1) + " from " + url);
                }
                else
                {
                    ZombieModEntry.Log("ZombieSkin: failed to load variant " + (idx + 1) + ": " + www.error);
                }
            }
        }

        // Returns a 0-based variant index (0..4) chosen by weighted random.
        private static int PickZombieVariant()
        {
            int total = 0;
            for (int i = 0; i < VARIANT_WEIGHTS.Length; i++) total += VARIANT_WEIGHTS[i];
            int roll = UnityEngine.Random.Range(0, total);
            int cum = 0;
            for (int i = 0; i < VARIANT_WEIGHTS.Length; i++)
            {
                cum += VARIANT_WEIGHTS[i];
                if (roll < cum) return i;
            }
            return 0;
        }

        // Applies the skin texture for the given 0-based variant to all body
        // renderers on the zombie GO (skips weapon renderers).
        private static void ApplyZombieSkin(GameObject go, int variant)
        {
            // Test mode overrides all variants with a single texture
            Texture2D tex = (ZOMBIE_SKIN_TEST_URL.Length > 0)
                ? _skinTestTexture
                : _skinTextures[variant];
            if (tex == null) return; // not loaded yet — keep default
            ZombieAssetCleanup cleanup = go.GetComponent<ZombieAssetCleanup>();
            if (cleanup == null) cleanup = go.AddComponent<ZombieAssetCleanup>();
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || r.transform == null) continue;
                if (LooksLikeHeldWeapon(r.transform)) continue; // skip weapons
                // Clone from sharedMaterial (cloning via .material would instantiate
                // an extra throwaway copy first). Track the clone for destruction.
                Material m = new Material(r.sharedMaterial != null ? r.sharedMaterial : r.material);
                m.mainTexture = tex;
                m.mainTextureOffset = Vector2.zero;
                m.mainTextureScale = Vector2.one;
                if (LooksLikeEnemyOverlay(r.transform))
                {
                    ConfigureOverlayMaterial(m);
                    try { r.enabled = true; } catch { }
                }
                r.material = m;
                cleanup.Own(m);
            }
        }

        private static void ConfigureSkinTexture(Texture2D tex)
        {
            if (tex == null) return;
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 0;
            tex.wrapMode = TextureWrapMode.Clamp;
        }

        private static void ConfigureOverlayMaterial(Material m)
        {
            if (m == null) return;
            try
            {
                Shader s = Shader.Find("Transparent/Cutout/Diffuse");
                if (s == null) s = Shader.Find("Mobile/Transparent/Cutout");
                if (s == null) s = Shader.Find("Transparent/Diffuse");
                if (s != null) m.shader = s;
            }
            catch { }
            try { m.SetInt("_Cull", 0); } catch { }
            try { m.SetInt("_ZWrite", 0); } catch { }
            try { m.SetFloat("_Cutoff", 0.01f); } catch { }
            try { m.EnableKeyword("_ALPHATEST_ON"); } catch { }
            try { m.color = Color.white; } catch { }
            try { m.SetColor("_Color", Color.white); } catch { }
            try { m.renderQueue = 3000; } catch { }
            try { m.mainTextureOffset = Vector2.zero; } catch { }
            try { m.mainTextureScale = Vector2.one; } catch { }
        }

        private static bool LooksLikeEnemyHealthBar(Transform t)
        {
            while (t != null)
            {
                string n = t.name ?? "";
                if (n.IndexOf("bloodbar", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (n.IndexOf("bloodposition", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (n.Equals("blood", StringComparison.OrdinalIgnoreCase)) return true;
                t = t.parent;
            }
            return false;
        }

        private static bool LooksLikeEnemyOverlay(Transform t)
        {
            return TransformPath(t).IndexOf("__EnemyOverlay", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsEnemyBodyPartName(string name)
        {
            return string.Equals(name, "1_1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "1_2", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "1_3", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "1_4", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "1_005", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "1_006", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnemySkinPartName(string name)
        {
            return IsEnemyBodyPartName(name) || string.Equals(name, "blood", StringComparison.OrdinalIgnoreCase);
        }

        void OnLevelWasLoaded(int level)
        {
            string scene = Application.loadedLevelName;
            // Always try to cache the prefab template on any scene load.
            // Enemy3 is the preferred source prefab for this mod.
            // TryCachePrefab will upgrade from a no-AI template if needed.
            TryCachePrefab();

            _masterSpawned = false;
            _haveAuthorityState = false;
            _lastAuthority = false;
            _localPlayerGO = null;
            _cachedPeerId = null;
            ClearAll();
            ClearZombieMapRuntime();
            _hud = "";
            if (IsGameScene(scene) && IsZombieRoomSelected())
                StartCoroutine(LoadZombieMapConfig(scene));

            // Avoid live enemy component dumps during normal play. The old
            // scanner was useful for discovery, but it can stall the game hard
            // when singleplayer enemies include large weapon/model trees.
        }

        void OnLeftRoom()
        {
            _masterSpawned = false; _haveAuthorityState = false; _lastAuthority = false;
            _localPlayerGO = null; _cachedPeerId = null; ClearAll(); ClearZombieMapRuntime();
        }
        void OnDisconnectedFromPhoton()
        {
            _masterSpawned = false; _proxyInstalled = false; _lastPeer = null;
            _haveAuthorityState = false; _lastAuthority = false; ClearAll(); ClearZombieMapRuntime();
        }
        void Update()
        {
            try
            {
            // Keep Photon proxy installed even if peer re-connects
            object peer = GetNetworkingPeer();
            if (!ReferenceEquals(peer, _lastPeer)) { _proxyInstalled = false; _lastPeer = peer; }
            if (!_proxyInstalled) TryInstallProxy();

            // Tick lookup caches
            if (_localPlayerFindTimer > 0f) _localPlayerFindTimer -= Time.deltaTime;
            if (_peerIdCacheTimer > 0f) _peerIdCacheTimer -= Time.deltaTime;

            string scene = Application.loadedLevelName;
            bool inRoom   = IsInRoom();
            bool isAuthority = inRoom && IsMasterClientNow();
            bool hasTemplate = _templateGO != null;
            GameObject ec = GetLocalPlayerGO();

            if (Input.GetKeyDown(KeyCode.F12))
                ToggleNavDebug(ec != null ? ec.transform.position : Vector3.zero);
            if (Input.GetKeyDown(KeyCode.F7))
                DumpEnemy3Model();

            // Periodic diagnostic log + template retry + HUD diag string rebuild
            _diagTimer -= Time.deltaTime;
            if (_diagTimer <= 0f)
            {
                _diagTimer = DIAG_INTERVAL;
                bool photonMaster = IsMasterClient();
                ZombieModEntry.Log(string.Format(
                    "Diag: scene={0} gameScene={1} inRoom={2} photonMaster={3} zombieAuthority={4} hasTemplate={5} player={6} spawned={7} {8}",
                    scene, IsGameScene(scene), inRoom, photonMaster, isAuthority, hasTemplate,
                    ec != null ? ec.name : "null", _masterSpawned, GetAuthorityDebug()));
                // Retry caching the prefab if it failed on scene load
                // (SingleEnemyManager.mInstance may have been null at OnLevelWasLoaded time)
                if (_templateGO == null) TryCachePrefab();
                // Building this string per frame was pure GC churn — diag cadence is enough.
                _hud = string.Format("[ZMod] room={0} authority={1} photonMaster={2} tmpl={3} navdbg={4} {5}",
                    inRoom, isAuthority, photonMaster, hasTemplate, _navDebugEnabled ? "ON" : "OFF",
                    _navDebugStatus);
            }

            if (_modeMessageTimer > 0f) _modeMessageTimer -= Time.deltaTime;

            if (!IsGameScene(scene)) return;
            if (!inRoom)             return;
            if (!IsZombieSessionActive())
            {
                if (HasZombieRuntimeState())
                {
                    ZombieModEntry.Log("ZombieMode: clearing stale runtime in non-zombie room");
                    ClearAll();
                    ClearZombieMapRuntime();
                }
                return;
            }
            CachePlayerLogicFields();
            ForceCopTeamIfNeeded();
            EnsureZombieMapRuntime();
            EnforceZombieInventory();
            DiagnoseZombieWeaponFireState();
            UpdateWeaponUpgradeLock();
            UpdateZombieInteraction(ec);
            if (_phase != PHASE_GAMEOVER)
                ResetVanillaRoundOverState(false);

            if (!_haveAuthorityState || _lastAuthority != isAuthority)
            {
                ZombieModEntry.Log("AuthorityChanged: " + (_haveAuthorityState ? _lastAuthority.ToString() : "unset") +
                    " -> " + isAuthority + " " + GetAuthorityDebug());
                ClearAll();
                _lastAuthority = isAuthority;
                _haveAuthorityState = true;
            }

            if (isAuthority)
            {
                if (!_modeStarted && ec != null)
                {
                    if (IsLocalPlayerPastTeamSelection())
                    {
                        _loggedWaitingForHostStart = false;
                        StartZombieMode(ec.transform.position);
                    }
                    else if (!_loggedWaitingForHostStart)
                    {
                        _loggedWaitingForHostStart = true;
                        ZombieModEntry.Log("ZombieMode: waiting for host to finish team selection before round countdown");
                    }
                }

                if (_modeStarted)
                    UpdateMasterMode(Time.deltaTime, ec);

                _broadcastTimer -= Time.deltaTime;
                if (_broadcastTimer <= 0f) { _broadcastTimer = BROADCAST_SECS; Broadcast(); }
            }
            else
            {
                UpdateDeathTracking(Time.deltaTime);
            }
            }
            catch (Exception ex) { ZombieModEntry.Log("Update err: " + ex.Message + "\n" + ex.StackTrace); }
        }

        void LateUpdate() { }

        void OnGUI()
        {
            string scene = Application.loadedLevelName;
            if (!IsGameScene(scene)) return;
            if (!IsInRoom()) return;
            if (!IsZombieSessionActive()) return;
            DrawZombieHud();
            DrawZombieInteractionHud();
            GUI.Label(new Rect(8f, 150f, 760f, 24f), _hud);
        }

        private bool IsZombieSessionActive()
        {
            return CNRMods.ZombieMode.IsZombieRoom || _zombieSessionActive;
        }

        private bool IsZombieRoomSelected()
        {
            return CNRMods.ZombieMode.IsZombieRoom || CNRMods.ZombieMode.PendingZombie;
        }

        private bool HasZombieRuntimeState()
        {
            return _zombieSessionActive || _modeStarted || _drivers.Count > 0 || _proxies.Count > 0 ||
                   _zombieInventoryApplied || _zombieInventoryReady || _overlayObjects.Count > 0;
        }

        private void MarkZombieSessionActive()
        {
            _zombieSessionActive = true;
            try { CNRMods.ZombieMode.IsZombieRoom = true; } catch { }
        }

        private void ClearZombieMapRuntime()
        {
            RestoreVanillaInventory();
            for (int i = 0; i < _overlayObjects.Count; i++)
            {
                if (_overlayObjects[i] != null)
                    try { UnityEngine.Object.Destroy(_overlayObjects[i]); } catch { }
            }
            _overlayObjects.Clear();
            _wallBuyHolograms.Clear();
            _nearWallBuy = null;
            _nearUpgradeObject = null;
            _zombieMapConfig = null;
            _zombieMapRaw = "";
            _zombieMapScene = "";
            _zombieMapLoading = false;
            _zombieMapLoaded = false;
            _zombieMapFromBuiltin = false;
            _zombieMapRuntimeReady = false;
            _zombieMapRequestTimer = 0f;
            _openedDoors.Clear();
            _barrierBoards.Clear();
            _spawnLastUsed.Clear();
            _weaponManager = null;
            _weaponManagerType = null;
            _weaponScriptType = null;
            _weaponList = null;
            _weaponTotal = null;
            _zombieInventoryReady = false;
            _primarySlots[0] = null;
            _primarySlots[1] = null;
            _meleeSlot = "";
            _grenadeSlot = "";
            _weaponStars.Clear();
            _weaponBaseClip.Clear();
            _weaponBaseReserve.Clear();
            _weaponUpgradeLocked = false;
            _weaponUpgradeName = "";
            _weaponUpgradeSlot = "";
            _nearUpgradeObject = null;
        }

        private IEnumerator LoadZombieMapConfig(string scene)
        {
            if (_zombieMapLoading || string.IsNullOrEmpty(scene)) yield break;
            _zombieMapLoading = true;
            _zombieMapScene = scene;
            List<string> lookupKeys = GetZombieMapLookupKeys(scene);

            if (!IsMasterClientNow())
            {
                string builtRaw;
                string builtKey;
                if (TryGetBuiltinZombieMap(lookupKeys, out builtKey, out builtRaw))
                {
                    _zombieMapFromBuiltin = true;
                    ApplyZombieMapRaw(scene, builtRaw);
                    ZombieModEntry.Log("ZombieMap: client loaded built-in overlay key=" + builtKey + " scene=" + scene);
                }
                _zombieMapLoading = false;
                RequestZombieMapSnapshot();
                yield break;
            }

            string raw = null;
            _zombieMapFromBuiltin = false;
            try
            {
                for (int i = 0; i < lookupKeys.Count && string.IsNullOrEmpty(raw); i++)
                {
                    string localPath = Path.Combine(ZOMBIE_MAP_LOCAL_DIR, lookupKeys[i] + ".json");
                    if (File.Exists(localPath))
                    {
                        raw = File.ReadAllText(localPath);
                        ZombieModEntry.Log("ZombieMap: loaded local overlay key=" + lookupKeys[i] + " scene=" + scene);
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieMap local read err: " + ex.Message); }

            if (string.IsNullOrEmpty(raw))
            {
                string builtKey;
                if (TryGetBuiltinZombieMap(lookupKeys, out builtKey, out raw))
                {
                    _zombieMapFromBuiltin = true;
                    ZombieModEntry.Log("ZombieMap: loaded built-in overlay key=" + builtKey + " scene=" + scene);
                }
            }

            if (string.IsNullOrEmpty(raw))
            {
                for (int i = 0; i < lookupKeys.Count && string.IsNullOrEmpty(raw); i++)
                {
                    WWW www = new WWW(ZOMBIE_MAP_BASE_URL + "/configs/" + lookupKeys[i] + ".json");
                    yield return www;
                    if (string.IsNullOrEmpty(www.error) && !string.IsNullOrEmpty(www.text))
                    {
                        raw = www.text;
                        ZombieModEntry.Log("ZombieMap: loaded hosted overlay key=" + lookupKeys[i] + " scene=" + scene);
                    }
                }
                if (string.IsNullOrEmpty(raw))
                    ZombieModEntry.Log("ZombieMap: no local/built-in/hosted overlay for " + scene + " (fallback spawning remains active)");
            }

            ApplyZombieMapRaw(scene, raw);
            _zombieMapLoading = false;
            SendZombieMapSnapshot();
        }

        private List<string> GetZombieMapLookupKeys(string scene)
        {
            List<string> keys = new List<string>();
            AppendZombieMapKey(keys, scene);
            try { AppendZombieMapKey(keys, PlayerPrefs.GetString("CNRMod_CustomMapName", "")); } catch { }
            try
            {
                string url = PlayerPrefs.GetString("CNRMod_ActiveMapURL", "");
                if (!string.IsNullOrEmpty(url))
                {
                    int q = url.IndexOf('?');
                    if (q >= 0) url = url.Substring(0, q);
                    int slash = Math.Max(url.LastIndexOf('/'), url.LastIndexOf('\\'));
                    string file = slash >= 0 ? url.Substring(slash + 1) : url;
                    int dot = file.LastIndexOf('.');
                    if (dot > 0) file = file.Substring(0, dot);
                    AppendZombieMapKey(keys, file);
                }
            }
            catch { }

            if (string.Equals(scene, "FreeRun5_1", StringComparison.OrdinalIgnoreCase) ||
                ContainsKeyPart(keys, "snow"))
                AppendZombieMapKey(keys, "SnowMap3_Zombies");

            return keys;
        }

        private static void AppendZombieMapKey(List<string> keys, string raw)
        {
            string safe = MakeSafe(raw);
            if (string.IsNullOrEmpty(safe)) return;
            for (int i = 0; i < keys.Count; i++)
                if (string.Equals(keys[i], safe, StringComparison.OrdinalIgnoreCase)) return;
            keys.Add(safe);
        }

        private static bool ContainsKeyPart(List<string> keys, string part)
        {
            if (keys == null || string.IsNullOrEmpty(part)) return false;
            for (int i = 0; i < keys.Count; i++)
                if (!string.IsNullOrEmpty(keys[i]) && keys[i].IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static bool TryGetBuiltinZombieMap(List<string> keys, out string key, out string raw)
        {
            key = "";
            raw = null;
            if (keys == null) return false;
            for (int i = 0; i < keys.Count; i++)
            {
                if (ZombieBuiltinContent.TryGetMap(keys[i], out raw))
                {
                    key = keys[i];
                    return true;
                }
            }
            return false;
        }

        private void ApplyZombieMapRaw(string scene, string raw)
        {
            DestroyZombieOverlayObjects();
            _zombieMapScene = scene ?? "";
            _zombieMapRaw = raw ?? "";
            _zombieMapConfig = null;
            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    _zombieMapConfig = JsonReader.Deserialize<ZombieMapConfig>(raw);
                    if (_zombieMapConfig != null && string.IsNullOrEmpty(_zombieMapConfig.scene))
                        _zombieMapConfig.scene = scene;
                }
                catch (Exception ex)
                {
                    ZombieModEntry.Log("ZombieMap parse err: " + ex.Message);
                    _zombieMapConfig = null;
                }
            }
            _zombieMapLoaded = true;
            _zombieMapRuntimeReady = false;
        }

        private void DestroyZombieOverlayObjects()
        {
            for (int i = 0; i < _overlayObjects.Count; i++)
            {
                if (_overlayObjects[i] != null)
                    try { UnityEngine.Object.Destroy(_overlayObjects[i]); } catch { }
            }
            _overlayObjects.Clear();
            _wallBuyHolograms.Clear();
            _nearWallBuy = null;
            _nearUpgradeObject = null;
            _zombieMapRuntimeReady = false;
        }

        private void EnsureZombieMapRuntime()
        {
            if (!EnsureZombieInventory()) return;

            string scene = Application.loadedLevelName;
            if (!_zombieMapLoaded && !_zombieMapLoading)
            {
                if (IsMasterClientNow())
                    StartCoroutine(LoadZombieMapConfig(scene));
                else
                {
                    _zombieMapRequestTimer -= Time.deltaTime;
                    if (_zombieMapRequestTimer <= 0f)
                    {
                        _zombieMapRequestTimer = 2f;
                        RequestZombieMapSnapshot();
                    }
                }
                return;
            }

            if (!_zombieMapLoaded || _zombieMapRuntimeReady) return;
            _zombieMapRuntimeReady = true;
            if (_zombieMapConfig == null) return;

            if (_zombieMapConfig.wallBuys != null)
            {
                for (int i = 0; i < _zombieMapConfig.wallBuys.Length; i++)
                    CreateWallBuyHologram(_zombieMapConfig.wallBuys[i]);
            }
            MaterializeFutureOverlayGeometry();
        }

        private void RequestZombieMapSnapshot()
        {
            if (!IsInRoom() || IsMasterClientNow()) return;
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["mapreq"] = Application.loadedLevelName;
            RaiseZombieMapEvent(ht, true);
        }

        private void SendZombieMapSnapshot()
        {
            if (!IsInRoom() || !IsMasterClientNow() || !_zombieMapLoaded) return;
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["mapscene"] = _zombieMapScene ?? Application.loadedLevelName;
            ht["mapjson"] = _zombieMapRaw ?? "";
            ht["doors"] = SerializeOpenedDoors();
            ht["barriers"] = SerializeBarrierBoards();
            RaiseZombieMapEvent(ht, true);
        }

        private void RaiseZombieMapEvent(System.Collections.Hashtable ht, bool reliable)
        {
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] { typeof(byte), typeof(System.Collections.Hashtable), typeof(bool), typeof(byte) });
                if (raise != null)
                    raise.Invoke(peer, new object[] { ZombieModEntry.ZOMBIE_MAP_EVENT, ht, reliable, (byte)0 });
            }
            catch (Exception ex) { ZombieModEntry.Log("RaiseZombieMapEvent err: " + ex.Message); }
        }

        public void OnZombieMapEvent(EventData ev)
        {
            try
            {
                System.Collections.Hashtable ht = ExtractZombiePayload(ev);
                if (ht == null) return;
                if (!HasZombieMapPayload(ht)) return;

                if (ht.ContainsKey("mapreq") && IsMasterClientNow())
                {
                    if (!IsZombieSessionActive()) return;
                    string requestedScene = Convert.ToString(ht["mapreq"]);
                    if (string.Equals(requestedScene, Application.loadedLevelName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (_zombieMapLoaded) SendZombieMapSnapshot();
                        else if (!_zombieMapLoading) StartCoroutine(LoadZombieMapConfig(requestedScene));
                    }
                    return;
                }

                if (!ht.ContainsKey("mapscene")) return;
                string scene = Convert.ToString(ht["mapscene"]);
                if (!string.Equals(scene, Application.loadedLevelName, StringComparison.OrdinalIgnoreCase)) return;
                MarkZombieSessionActive();

                if (ht.ContainsKey("mapjson") && !IsMasterClientNow())
                {
                    _zombieMapFromBuiltin = false;
                    ApplyZombieMapRaw(scene, Convert.ToString(ht["mapjson"]));
                }
                if (ht.ContainsKey("doors")) ApplyOpenedDoors(Convert.ToString(ht["doors"]));
                if (ht.ContainsKey("barriers")) ApplyBarrierBoards(Convert.ToString(ht["barriers"]));
                EnsureZombieMapRuntime();
            }
            catch (Exception ex) { ZombieModEntry.Log("OnZombieMapEvent err: " + ex.Message); }
        }

        private bool HasZombieMapPayload(System.Collections.Hashtable ht)
        {
            if (ht == null) return false;
            return ht.ContainsKey("mapreq") || ht.ContainsKey("mapscene") ||
                   ht.ContainsKey("mapjson") || ht.ContainsKey("doors") || ht.ContainsKey("barriers");
        }

        private string SerializeOpenedDoors()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (string id in _openedDoors)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.Append(id);
            }
            return sb.ToString();
        }

        private void ApplyOpenedDoors(string raw)
        {
            _openedDoors.Clear();
            if (string.IsNullOrEmpty(raw)) return;
            string[] ids = raw.Split('|');
            for (int i = 0; i < ids.Length; i++) if (!string.IsNullOrEmpty(ids[i])) _openedDoors.Add(ids[i]);
        }

        private string SerializeBarrierBoards()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<string, int> kv in _barrierBoards)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.Append(kv.Key).Append('=').Append(kv.Value);
            }
            return sb.ToString();
        }

        private void ApplyBarrierBoards(string raw)
        {
            _barrierBoards.Clear();
            if (string.IsNullOrEmpty(raw)) return;
            string[] entries = raw.Split('|');
            for (int i = 0; i < entries.Length; i++)
            {
                int eq = entries[i].IndexOf('=');
                if (eq <= 0) continue;
                int boards;
                if (int.TryParse(entries[i].Substring(eq + 1), out boards))
                    _barrierBoards[entries[i].Substring(0, eq)] = boards;
            }
        }

        private void MaterializeFutureOverlayGeometry()
        {
            // Doors/barriers are already part of the synchronized schema so later interaction
            // work does not require a protocol change. For now render their blocking geometry.
            if (_zombieMapConfig.doors != null)
            {
                for (int i = 0; i < _zombieMapConfig.doors.Length; i++)
                {
                    ZombieDoor d = _zombieMapConfig.doors[i];
                    if (d == null || d.initiallyOpen || _openedDoors.Contains(d.id) || !ValidVec3(d.pos)) continue;
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = "ZombieDoor_" + (d.id ?? i.ToString());
                    go.transform.position = ToVec3(d.pos);
                    go.transform.localScale = ValidVec3(d.size) ? ToVec3(d.size) : new Vector3(2f, 3f, 0.4f);
                    if (ValidVec3(d.rot)) go.transform.eulerAngles = ToVec3(d.rot);
                    _overlayObjects.Add(go);
                }
            }
            if (_zombieMapConfig.barriers != null)
            {
                for (int i = 0; i < _zombieMapConfig.barriers.Length; i++)
                {
                    ZombieBarrier b = _zombieMapConfig.barriers[i];
                    if (b == null || !ValidVec3(b.pos)) continue;
                    int boards = b.maxBoards > 0 ? b.maxBoards : 5;
                    int syncedBoards;
                    if (_barrierBoards.TryGetValue(b.id ?? "", out syncedBoards)) boards = syncedBoards;
                    if (boards <= 0) continue;
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = "ZombieBarrier_" + (b.id ?? i.ToString());
                    go.transform.position = ToVec3(b.pos);
                    go.transform.localScale = ValidVec3(b.size) ? ToVec3(b.size) : new Vector3(2f, 2.5f, 0.25f);
                    if (ValidVec3(b.rot)) go.transform.eulerAngles = ToVec3(b.rot);
                    _overlayObjects.Add(go);
                }
            }
            if (_zombieMapConfig.customObjects != null)
            {
                for (int i = 0; i < _zombieMapConfig.customObjects.Length; i++)
                {
                    ZombieCustomObject obj = _zombieMapConfig.customObjects[i];
                    if (obj == null || !ValidVec3(obj.pos)) continue;
                    if (string.IsNullOrEmpty(obj.assetUrl)) BuildZombiePlaceholderObject(obj, i);
                    else StartCoroutine(LoadZombieCustomObject(obj));
                }
            }
        }

        private void BuildZombiePlaceholderObject(ZombieCustomObject placement, int index)
        {
            try
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "ZombieObject_" + (!string.IsNullOrEmpty(placement.id) ? placement.id : index.ToString());
                go.transform.position = ToVec3(placement.pos);
                if (ValidVec3(placement.rot)) go.transform.eulerAngles = ToVec3(placement.rot);
                go.transform.localScale = ValidVec3(placement.scale) ? ToVec3(placement.scale) : new Vector3(1.25f, 1.75f, 1.25f);
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Diffuse"));
                    rend.material.color = new Color(0.45f, 0.1f, 0.9f, 0.85f);
                }
                if (!placement.collidable)
                {
                    Collider col = go.GetComponent<Collider>();
                    if (col != null) UnityEngine.Object.Destroy(col);
                }
                _overlayObjects.Add(go);
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieObject placeholder err: " + ex.Message); }
        }

        private IEnumerator LoadZombieCustomObject(ZombieCustomObject placement)
        {
            if (placement == null || string.IsNullOrEmpty(placement.assetUrl)) yield break;
            string url = ResolveZombieAssetUrl(placement.assetUrl);
            string raw = null;
            if (!_customAssetJsonCache.TryGetValue(url, out raw))
            {
                if (ZombieBuiltinContent.TryGetAsset(placement.assetUrl, out raw))
                {
                    _customAssetJsonCache[url] = raw;
                }
                else
                {
                    WWW www = new WWW(url);
                    yield return www;
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        ZombieModEntry.Log("ZombieObject download err " + url + ": " + www.error);
                        yield break;
                    }
                    raw = www.text;
                    if (string.IsNullOrEmpty(raw)) yield break;
                    _customAssetJsonCache[url] = raw;
                }
            }

            ZombieObjectAsset asset = null;
            try { asset = JsonReader.Deserialize<ZombieObjectAsset>(raw); }
            catch (Exception ex) { ZombieModEntry.Log("ZombieObject parse err " + url + ": " + ex.Message); }
            if (asset == null || asset.meshes == null || asset.meshes.Length == 0) yield break;
            BuildZombieCustomObject(placement, asset);
        }

        private static string ResolveZombieAssetUrl(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            string s = raw.Trim();
            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return s;
            if (s.StartsWith("/")) return "https://play.jacqueb.me" + s;
            return ZOMBIE_MAP_BASE_URL + "/assets/" + s;
        }

        private void BuildZombieCustomObject(ZombieCustomObject placement, ZombieObjectAsset asset)
        {
            try
            {
                GameObject root = new GameObject("ZombieObject_" + (!string.IsNullOrEmpty(placement.id) ? placement.id : asset.id));
                root.transform.position = ToVec3(placement.pos);
                if (ValidVec3(placement.rot)) root.transform.eulerAngles = ToVec3(placement.rot);
                root.transform.localScale = ValidVec3(placement.scale) ? ToVec3(placement.scale) : Vector3.one;
                ZombieAssetCleanup cleanup = root.AddComponent<ZombieAssetCleanup>();

                int totalVertices = 0;
                int meshLimit = Math.Min(asset.meshes.Length, 64);
                for (int i = 0; i < meshLimit; i++)
                {
                    ZombieObjectMesh src = asset.meshes[i];
                    if (src == null || src.vertices == null || src.vertices.Length < 9 || src.triangles == null || src.triangles.Length < 3) continue;
                    int vertexCount = src.vertices.Length / 3;
                    if (totalVertices + vertexCount > 250000)
                    {
                        ZombieModEntry.Log("ZombieObject asset vertex cap reached (250k): " + (asset.id ?? "unknown"));
                        break;
                    }
                    totalVertices += vertexCount;
                    if (vertexCount > 65000)
                    {
                        ZombieModEntry.Log("ZombieObject mesh skipped (>65k vertices): " + (src.name ?? i.ToString()));
                        continue;
                    }

                    Vector3[] vertices = new Vector3[vertexCount];
                    for (int v = 0; v < vertexCount; v++)
                        vertices[v] = new Vector3(src.vertices[v * 3], src.vertices[v * 3 + 1], src.vertices[v * 3 + 2]);

                    Mesh mesh = new Mesh();
                    mesh.name = !string.IsNullOrEmpty(src.name) ? src.name : "ZombieObjectMesh_" + i;
                    mesh.vertices = vertices;
                    mesh.triangles = src.triangles;
                    if (src.uv != null && src.uv.Length >= vertexCount * 2)
                    {
                        Vector2[] uv = new Vector2[vertexCount];
                        for (int v = 0; v < vertexCount; v++) uv[v] = new Vector2(src.uv[v * 2], src.uv[v * 2 + 1]);
                        mesh.uv = uv;
                    }
                    if (src.normals != null && src.normals.Length >= vertexCount * 3)
                    {
                        Vector3[] normals = new Vector3[vertexCount];
                        for (int v = 0; v < vertexCount; v++) normals[v] = new Vector3(src.normals[v * 3], src.normals[v * 3 + 1], src.normals[v * 3 + 2]);
                        mesh.normals = normals;
                    }
                    else mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    cleanup.Own(mesh);

                    GameObject part = new GameObject(mesh.name);
                    part.transform.parent = root.transform;
                    part.transform.localPosition = Vector3.zero;
                    part.transform.localRotation = Quaternion.identity;
                    part.transform.localScale = Vector3.one;
                    MeshFilter mf = part.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;
                    MeshRenderer mr = part.AddComponent<MeshRenderer>();
                    Material mat = CreateZombieObjectMaterial(src, cleanup);
                    if (mat != null) mr.material = mat;
                    if (!string.IsNullOrEmpty(src.textureUrl)) StartCoroutine(LoadZombieObjectTexture(mr, src.textureUrl, cleanup));

                    if (placement.collidable && src.collidable)
                    {
                        MeshCollider mc = part.AddComponent<MeshCollider>();
                        mc.sharedMesh = mesh;
                    }
                }
                _overlayObjects.Add(root);
            }
            catch (Exception ex) { ZombieModEntry.Log("BuildZombieCustomObject err: " + ex.Message); }
        }

        private Material CreateZombieObjectMaterial(ZombieObjectMesh src, ZombieAssetCleanup cleanup)
        {
            Material mat = null;
            try
            {
                if (src != null && string.Equals(src.materialMode, "vanilla", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(src.materialName))
                {
                    Material found = FindSceneMaterial(src.materialName);
                    if (found != null) mat = new Material(found);
                }
                if (mat == null)
                {
                    Shader sh = Shader.Find("Mobile/Diffuse");
                    if (sh == null) sh = Shader.Find("Diffuse");
                    if (sh == null) sh = Shader.Find("Unlit/Texture");
                    if (sh != null) mat = new Material(sh);
                }
                if (mat == null) return null;

                if (src != null && src.color != null && src.color.Length >= 3 && mat.HasProperty("_Color"))
                {
                    float div = (src.color[0] > 1f || src.color[1] > 1f || src.color[2] > 1f) ? 255f : 1f;
                    float a = src.color.Length >= 4 ? src.color[3] / (src.color[3] > 1f ? 255f : 1f) : 1f;
                    mat.color = new Color(src.color[0] / div, src.color[1] / div, src.color[2] / div, a);
                }
                if (src != null && !string.IsNullOrEmpty(src.textureData))
                {
                    string data = src.textureData;
                    int comma = data.IndexOf(',');
                    if (data.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0) data = data.Substring(comma + 1);
                    byte[] bytes = Convert.FromBase64String(data);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.wrapMode = TextureWrapMode.Repeat;
                        tex.filterMode = FilterMode.Bilinear;
                        mat.mainTexture = tex;
                        cleanup.Own(tex);
                    }
                    else UnityEngine.Object.Destroy(tex);
                }
                cleanup.Own(mat);
            }
            catch (Exception ex) { ZombieModEntry.Log("CreateZombieObjectMaterial err: " + ex.Message); }
            return mat;
        }

        private IEnumerator LoadZombieObjectTexture(Renderer renderer, string rawUrl, ZombieAssetCleanup cleanup)
        {
            if (renderer == null || string.IsNullOrEmpty(rawUrl)) yield break;
            string url = rawUrl.Trim();
            if (url.StartsWith("/")) url = "https://play.jacqueb.me" + url;
            else if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = ZOMBIE_MAP_BASE_URL + "/assets/" + url;
            WWW www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error) || renderer == null) yield break;
            Texture2D tex = www.texture;
            if (tex == null) yield break;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            renderer.material.mainTexture = tex;
            if (cleanup != null) cleanup.Own(tex);
        }

        private static Material FindSceneMaterial(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Renderer[] renderers = (Renderer[])UnityEngine.Object.FindObjectsOfType(typeof(Renderer));
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || r.sharedMaterial == null) continue;
                string n = r.sharedMaterial.name ?? "";
                int inst = n.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
                if (inst >= 0) n = n.Substring(0, inst);
                if (string.Equals(n.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase)) return r.sharedMaterial;
            }
            return null;
        }

        private static bool ValidVec3(float[] v) { return v != null && v.Length >= 3; }
        private static Vector3 ToVec3(float[] v) { return new Vector3(v[0], v[1], v[2]); }

        private void CreateWallBuyHologram(ZombieWallBuy buy)
        {
            if (buy == null || !ValidVec3(buy.pos)) return;
            string runtimeName = ResolveWallBuyRuntimeName(buy);
            object weapon = FindWeaponByName(runtimeName);
            Component wc = weapon as Component;
            if (wc == null)
            {
                CreateWallBuyFallbackHologram(buy, runtimeName);
                ZombieModEntry.Log("WallBuy: source weapon missing for " + runtimeName + ", used fallback hologram");
                return;
            }

            try
            {
                bool sourceWasActive = wc.gameObject.activeSelf;
                if (sourceWasActive) wc.gameObject.SetActive(false);
                GameObject go = null;
                try { go = (GameObject)UnityEngine.Object.Instantiate(wc.gameObject, ToVec3(buy.pos), Quaternion.identity); }
                finally { if (sourceWasActive) wc.gameObject.SetActive(true); }
                if (go == null) return;
                go.name = "ZombieWallBuy_" + (buy.id ?? runtimeName);
                DisableHologramGameplay(go);
                if (ValidVec3(buy.rot)) go.transform.eulerAngles = ToVec3(buy.rot);
                float holoScale = buy.scale > 0.01f ? buy.scale : 0.7f;
                go.transform.localScale = go.transform.localScale * holoScale;
                SetLayerRecursively(go.transform, 0);
                ZombieWallBuyHologram holo = go.AddComponent<ZombieWallBuyHologram>();
                holo.Data = buy;
                holo.Hook = this;
                holo.BaseY = go.transform.position.y;
                TintHologram(go, holo);
                go.SetActive(true);
                _wallBuyHolograms.Add(holo);
                _overlayObjects.Add(go);
            }
            catch (Exception ex) { ZombieModEntry.Log("CreateWallBuyHologram err: " + ex.Message); }
        }

        private void CreateWallBuyFallbackHologram(ZombieWallBuy buy, string runtimeName)
        {
            try
            {
                GameObject root = new GameObject("ZombieWallBuy_" + (buy.id ?? runtimeName));
                root.transform.position = ToVec3(buy.pos);
                if (ValidVec3(buy.rot)) root.transform.eulerAngles = ToVec3(buy.rot);
                float holoScale = buy.scale > 0.01f ? buy.scale : 0.7f;

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "WeaponMarker";
                body.transform.parent = root.transform;
                body.transform.localPosition = Vector3.zero;
                body.transform.localRotation = Quaternion.Euler(0f, 45f, 20f);
                body.transform.localScale = new Vector3(1.25f, 0.35f, 0.35f) * holoScale;
                Collider col = body.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.Destroy(col);
                Renderer rend = body.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Diffuse"));
                    rend.material.color = new Color(0.20f, 0.85f, 1f, 0.72f);
                }

                GameObject label = new GameObject("Label");
                label.transform.parent = root.transform;
                label.transform.localPosition = new Vector3(0f, 0.65f * holoScale, 0f);
                label.transform.localRotation = Quaternion.identity;
                TextMesh tm = label.AddComponent<TextMesh>();
                tm.text = ResolveWallBuyDisplayName(buy);
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.characterSize = 0.22f * holoScale;
                tm.color = new Color(0.20f, 0.85f, 1f, 0.95f);

                ZombieWallBuyHologram holo = root.AddComponent<ZombieWallBuyHologram>();
                holo.Data = buy;
                holo.Hook = this;
                holo.BaseY = root.transform.position.y;
                _wallBuyHolograms.Add(holo);
                _overlayObjects.Add(root);
            }
            catch (Exception ex) { ZombieModEntry.Log("WallBuy fallback err: " + ex.Message); }
        }

        private static void DisableHologramGameplay(GameObject go)
        {
            Behaviour[] behaviours = go.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++) behaviours[i].enabled = false;
            Collider[] cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
            Rigidbody[] bodies = go.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++) { bodies[i].isKinematic = true; bodies[i].useGravity = false; }
            AudioSource[] audio = go.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < audio.Length; i++) audio[i].enabled = false;
        }

        private static void SetLayerRecursively(Transform t, int layer)
        {
            if (t == null) return;
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursively(t.GetChild(i), layer);
        }

        private static void TintHologram(GameObject go, ZombieWallBuyHologram owner)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                try
                {
                    Material source = renderers[i].sharedMaterial;
                    if (source == null) continue;
                    Material m = new Material(source);
                    if (m.HasProperty("_Color")) m.color = new Color(0.20f, 0.85f, 1f, 0.72f);
                    renderers[i].material = m;
                    owner.Own(m);
                }
                catch { }
            }
        }

        private string ResolveWallBuyRuntimeName(ZombieWallBuy buy)
        {
            if (buy == null) return "";
            ZombieWeaponRegistration reg;
            if (ZombieWeaponRegistry.TryGet(buy.weaponId, out reg) && reg != null && !string.IsNullOrEmpty(reg.RuntimeName))
                return reg.RuntimeName;
            return !string.IsNullOrEmpty(buy.runtimeName) ? buy.runtimeName : buy.weaponId;
        }

        private string ResolveWallBuyDisplayName(ZombieWallBuy buy)
        {
            if (buy == null) return "Weapon";
            ZombieWeaponRegistration reg;
            if (ZombieWeaponRegistry.TryGet(buy.weaponId, out reg) && reg != null && !string.IsNullOrEmpty(reg.DisplayName))
                return reg.DisplayName;
            if (!string.IsNullOrEmpty(buy.displayName)) return buy.displayName;
            return ResolveWallBuyRuntimeName(buy);
        }

        private string ResolveWallBuySlot(ZombieWallBuy buy, object weapon)
        {
            ZombieWeaponRegistration reg;
            if (buy != null && ZombieWeaponRegistry.TryGet(buy.weaponId, out reg) && reg != null && !string.IsNullOrEmpty(reg.Slot))
                return reg.Slot.ToLowerInvariant();
            if (buy != null && !string.IsNullOrEmpty(buy.slot) && !string.Equals(buy.slot, "auto", StringComparison.OrdinalIgnoreCase))
                return buy.slot.ToLowerInvariant();
            return InferWeaponSlot(weapon);
        }

        private void UpdateZombieInteraction(GameObject player)
        {
            _nearWallBuy = null;
            _nearUpgradeObject = null;
            if (player == null || !_zombieMapRuntimeReady || _weaponUpgradeLocked) { _interactionRequested = false; return; }
            float bestSq = float.MaxValue;
            Vector3 p = player.transform.position;
            for (int i = 0; i < _wallBuyHolograms.Count; i++)
            {
                ZombieWallBuyHologram h = _wallBuyHolograms[i];
                if (h == null || h.Data == null) continue;
                float radius = h.Data.interactRadius > 0.1f ? h.Data.interactRadius : DEFAULT_INTERACT_RADIUS;
                float sq = (h.transform.position - p).sqrMagnitude;
                if (sq <= radius * radius && sq < bestSq) { bestSq = sq; _nearWallBuy = h; _nearUpgradeObject = null; }
            }
            if (_zombieMapConfig != null && _zombieMapConfig.customObjects != null)
            {
                for (int i = 0; i < _zombieMapConfig.customObjects.Length; i++)
                {
                    ZombieCustomObject o = _zombieMapConfig.customObjects[i];
                    if (o == null || !ValidVec3(o.pos) || !IsUpgradeInteraction(o.interactionType)) continue;
                    float radius = o.interactRadius > 0.1f ? o.interactRadius : DEFAULT_INTERACT_RADIUS;
                    float sq = (ToVec3(o.pos) - p).sqrMagnitude;
                    if (sq <= radius * radius && sq < bestSq) { bestSq = sq; _nearUpgradeObject = o; _nearWallBuy = null; }
                }
            }

            bool input = _interactionRequested || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0);
            _interactionRequested = false;
            if (!input) return;
            if (_nearWallBuy != null) TryPurchaseWallBuy(_nearWallBuy.Data);
            else if (_nearUpgradeObject != null) TryStartWeaponUpgrade(_nearUpgradeObject);
        }

        private void DrawZombieInteractionHud()
        {
            if (!_weaponUpgradeLocked && (_nearWallBuy == null || _nearWallBuy.Data == null) && _nearUpgradeObject == null) return;
            string label;
            string sub;
            if (_weaponUpgradeLocked)
            {
                float remain = Mathf.Max(0f, _weaponUpgradeCompleteAt - Time.realtimeSinceStartup);
                label = "UPGRADING " + (_weaponUpgradeName ?? "WEAPON").ToUpperInvariant() + "...";
                sub = remain.ToString("F1") + "s - weapons locked";
            }
            else if (_nearUpgradeObject != null)
            {
                string runtime = GetSelectedWeaponName();
                int star = GetZombieWeaponStar(runtime);
                int cost = _nearUpgradeObject.price > 0 ? _nearUpgradeObject.price : DEFAULT_UPGRADE_COST;
                bool primary = IsPrimaryInventoryWeapon(runtime);
                if (!primary) { label = "WEAPON UPGRADE"; sub = "Hold a primary weapon"; }
                else if (star >= MAX_ZOMBIE_WEAPON_STARS) { label = runtime.ToUpperInvariant() + " - MAX " + star + " STARS"; sub = "Fully upgraded"; }
                else { label = "UPGRADE " + runtime.ToUpperInvariant() + "  " + star + " > " + (star + 1) + " STARS  -  " + cost; sub = _points >= cost ? "Tap to upgrade" : "Not enough points"; }
            }
            else
            {
                ZombieWallBuy buy = _nearWallBuy.Data;
                string runtime = ResolveWallBuyRuntimeName(buy);
                bool owned = ZombieInventoryContains(runtime);
                int cost = owned ? (buy.ammoPrice > 0 ? buy.ammoPrice : Math.Max(1, buy.price / 3)) : buy.price;
                label = owned ? "REFILL " + ResolveWallBuyDisplayName(buy).ToUpperInvariant() + " AMMO  -  " + cost : "BUY " + ResolveWallBuyDisplayName(buy).ToUpperInvariant() + "  -  " + cost;
                sub = _points >= cost ? "Tap to purchase" : "Not enough points";
            }

            if (_interactionStyle == null)
            {
                _interactionStyle = new GUIStyle(GUI.skin.button);
                _interactionStyle.alignment = TextAnchor.MiddleCenter;
                _interactionStyle.fontSize = Mathf.Max(14, Screen.height / 45);
                _interactionStyle.fontStyle = FontStyle.Bold;
                _interactionSubStyle = new GUIStyle(GUI.skin.label);
                _interactionSubStyle.alignment = TextAnchor.MiddleCenter;
                _interactionSubStyle.fontSize = Mathf.Max(11, Screen.height / 60);
            }
            float w = Mathf.Min(580f, Screen.width * 0.68f);
            float h = Mathf.Max(48f, Screen.height * 0.065f);
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.70f;
            if (_weaponUpgradeLocked) GUI.Box(new Rect(x, y, w, h), label, _interactionStyle);
            else if (GUI.Button(new Rect(x, y, w, h), label, _interactionStyle)) _interactionRequested = true;
            GUI.Label(new Rect(x, y + h, w, 24f), sub, _interactionSubStyle);
        }

        private void TryPurchaseWallBuy(ZombieWallBuy buy)
        {
            if (buy == null || !EnsureZombieInventory()) return;
            string runtime = ResolveWallBuyRuntimeName(buy);
            object weapon = FindWeaponByName(runtime);
            if (weapon == null) { ShowModeMessage("Weapon unavailable", 2f); return; }

            bool owned = ZombieInventoryContains(runtime);
            int cost = owned ? (buy.ammoPrice > 0 ? buy.ammoPrice : Math.Max(1, buy.price / 3)) : buy.price;
            if (_points < cost) { ShowModeMessage("Not enough points", 2f); return; }

            if (owned)
            {
                _points -= cost;
                RefillWeaponAmmo(weapon, ResolveWallBuySlot(buy, weapon));
                ShowModeMessage(ResolveWallBuyDisplayName(buy) + " ammo refilled", 2f);
                return;
            }

            string slot = ResolveWallBuySlot(buy, weapon);
            if (slot == "melee")
                _meleeSlot = runtime;
            else if (slot == "grenade")
                _grenadeSlot = runtime;
            else
            {
                if (string.IsNullOrEmpty(_primarySlots[0])) _primarySlots[0] = runtime;
                else if (string.IsNullOrEmpty(_primarySlots[1])) _primarySlots[1] = runtime;
                else
                {
                    string selected = GetSelectedWeaponName();
                    if (string.Equals(selected, _primarySlots[1], StringComparison.OrdinalIgnoreCase)) _primarySlots[1] = runtime;
                    else _primarySlots[0] = runtime;
                }
            }

            _points -= cost;
            _weaponStars[runtime] = 1;
            RebuildZombieInventory(runtime);
            RefillWeaponAmmo(weapon, slot);
            ShowModeMessage("Bought " + ResolveWallBuyDisplayName(buy), 2f);
        }

        private bool EnsureZombieInventory()
        {
            if (_zombieInventoryReady && _weaponManager != null && _weaponList != null && _weaponTotal != null) return true;
            try
            {
                _weaponManagerType = FindType("WeaponManager");
                _weaponScriptType = FindType("WeaponScript");
                if (_weaponManagerType == null || _weaponScriptType == null) return false;
                GameObject go = null;
                try { go = GameObject.FindWithTag("WeaponManager"); } catch { }
                if (go == null) go = GameObject.Find("WeaponManager");
                if (go == null) return false;
                _weaponManager = go.GetComponent(_weaponManagerType);
                if (_weaponManager == null) return false;
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo all = _weaponManagerType.GetField("allWeapons", f);
                FieldInfo total = _weaponManagerType.GetField("allWeaponsTotal", f);
                _weaponList = all != null ? all.GetValue(_weaponManager) as System.Collections.IList : null;
                _weaponTotal = total != null ? total.GetValue(_weaponManager) as System.Collections.IList : null;
                if (_weaponList == null || _weaponTotal == null) return false;
                CaptureVanillaInventory();

                _primarySlots[0] = "Deagle";
                _primarySlots[1] = null;
                _meleeSlot = GetStrongestOwnedKnifeName();
                _grenadeSlot = "";
                _weaponStars.Clear();
                _weaponStars["Deagle"] = 1;
                if (!string.IsNullOrEmpty(_meleeSlot)) _weaponStars[_meleeSlot] = 1;
                _zombieInventoryReady = true;
                RebuildZombieInventory("Deagle");
                object deagle = FindWeaponByName("Deagle");
                if (deagle != null) RefillWeaponAmmo(deagle, "primary");
                object knife = FindWeaponByName(_meleeSlot);
                if (knife != null) RefillWeaponAmmo(knife, "melee");
                return true;
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("EnsureZombieInventory err: " + ex.Message);
                return false;
            }
        }

        private void EnforceZombieInventory()
        {
            if (!_zombieInventoryReady || _weaponUpgradeLocked) return;
            if (_weaponManager == null || _weaponList == null || _weaponTotal == null) return;
            try
            {
                bool dirty = false;
                int expected = 0;
                if (!string.IsNullOrEmpty(_primarySlots[0]) && FindWeaponByName(_primarySlots[0]) != null) expected++;
                if (!string.IsNullOrEmpty(_primarySlots[1]) && FindWeaponByName(_primarySlots[1]) != null) expected++;
                if (!string.IsNullOrEmpty(_meleeSlot) && FindWeaponByName(_meleeSlot) != null) expected++;
                if (!string.IsNullOrEmpty(_grenadeSlot) && FindWeaponByName(_grenadeSlot) != null) expected++;

                if (_weaponList.Count != expected) dirty = true;
                for (int i = 0; i < _weaponList.Count && !dirty; i++)
                {
                    string runtime = GetWeaponName(_weaponList[i]);
                    if (!ZombieInventoryContains(runtime)) dirty = true;
                }

                string selectedName = GetSelectedWeaponName();
                if (!string.IsNullOrEmpty(selectedName) && !ZombieInventoryContains(selectedName))
                    dirty = true;

                if (!dirty) return;
                string keepSelected = ZombieInventoryContains(selectedName) ? selectedName : _primarySlots[0];
                if (string.IsNullOrEmpty(keepSelected)) keepSelected = _meleeSlot;
                ZombieModEntry.Log("ZombieInventory: enforcing zombie weapon list count=" + _weaponList.Count + " expected=" + expected + " selected=" + selectedName);
                RebuildZombieInventory(keepSelected);
            }
            catch (Exception ex) { ZombieModEntry.Log("EnforceZombieInventory err: " + ex.Message); }
        }

        private void CaptureVanillaInventory()
        {
            if (_zombieInventoryApplied || _weaponList == null || _weaponManager == null || _weaponManagerType == null) return;
            try
            {
                _vanillaWeaponList.Clear();
                for (int i = 0; i < _weaponList.Count; i++)
                    _vanillaWeaponList.Add(_weaponList[i]);
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo idx = _weaponManagerType.GetField("index", f);
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                FieldInfo canSwitch = _weaponManagerType.GetField("canSwitch", f);
                _vanillaWeaponIndex = idx != null ? Convert.ToInt32(idx.GetValue(_weaponManager)) : -1;
                _vanillaSelectedWeapon = selected != null ? selected.GetValue(_weaponManager) : null;
                _vanillaCanSwitch = canSwitch == null || Convert.ToBoolean(canSwitch.GetValue(_weaponManager));
            }
            catch (Exception ex) { ZombieModEntry.Log("CaptureVanillaInventory err: " + ex.Message); }
        }

        private void RestoreVanillaInventory()
        {
            if (!_zombieInventoryApplied || _weaponList == null || _weaponManager == null || _weaponManagerType == null) return;
            try
            {
                if (_weaponTotal != null)
                {
                    for (int i = 0; i < _weaponTotal.Count; i++)
                    {
                        Component c = _weaponTotal[i] as Component;
                        if (c != null) c.gameObject.SetActive(false);
                    }
                }

                _weaponList.Clear();
                for (int i = 0; i < _vanillaWeaponList.Count; i++)
                    if (_vanillaWeaponList[i] != null && !_weaponList.Contains(_vanillaWeaponList[i]))
                        _weaponList.Add(_vanillaWeaponList[i]);

                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo idx = _weaponManagerType.GetField("index", f);
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                FieldInfo canSwitch = _weaponManagerType.GetField("canSwitch", f);

                int restoreIndex = _vanillaWeaponIndex;
                if (restoreIndex < 0 || restoreIndex >= _weaponList.Count) restoreIndex = _weaponList.Count > 0 ? 0 : -1;
                object restoreWeapon = _vanillaSelectedWeapon;
                if (restoreWeapon == null || !_weaponList.Contains(restoreWeapon))
                    restoreWeapon = restoreIndex >= 0 ? _weaponList[restoreIndex] : null;

                if (idx != null && restoreIndex >= 0) idx.SetValue(_weaponManager, restoreIndex);
                if (selected != null && restoreWeapon != null) selected.SetValue(_weaponManager, restoreWeapon);
                if (canSwitch != null) canSwitch.SetValue(_weaponManager, _vanillaCanSwitch);

                Component chosen = restoreWeapon as Component;
                if (chosen != null)
                {
                    chosen.gameObject.SetActive(true);
                    chosen.gameObject.SendMessage("selectWeapon", SendMessageOptions.DontRequireReceiver);
                    GameObject ng = GameObject.Find("UI Root (3D)");
                    if (ng != null) ng.SendMessage("receiveGunName", GetWeaponName(restoreWeapon), SendMessageOptions.DontRequireReceiver);
                }
                _zombieInventoryApplied = false;
                ZombieModEntry.Log("ZombieInventory: restored vanilla weapon list count=" + _weaponList.Count);
            }
            catch (Exception ex) { ZombieModEntry.Log("RestoreVanillaInventory err: " + ex.Message); }
            _vanillaWeaponList.Clear();
            _vanillaSelectedWeapon = null;
            _vanillaWeaponIndex = -1;
        }

        private string GetStrongestOwnedKnifeName()
        {
            string best = "BallisticKnife";
            int bestLevel = PlayerPrefs.GetInt("BallisticKnife", 1);
            if (_weaponTotal == null) return best;
            for (int i = 0; i < _weaponTotal.Count; i++)
            {
                object w = _weaponTotal[i];
                if (InferWeaponSlot(w) != "melee") continue;
                string name = GetWeaponName(w);
                if (string.IsNullOrEmpty(name)) continue;
                int def = string.Equals(name, "BallisticKnife", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                int level = PlayerPrefs.GetInt(name, def);
                if (level <= 0) continue;
                if (level > bestLevel || (level == bestLevel && string.Equals(name, "GingerbreadKnife", StringComparison.OrdinalIgnoreCase)))
                {
                    best = name;
                    bestLevel = level;
                }
            }
            return best;
        }

        private string InferWeaponSlot(object weapon)
        {
            if (weapon == null) return "primary";
            try
            {
                FieldInfo gunType = weapon.GetType().GetField("GunType", BindingFlags.Public | BindingFlags.Instance);
                string gt = gunType != null && gunType.GetValue(weapon) != null ? gunType.GetValue(weapon).ToString() : "";
                if (string.Equals(gt, "KNIFE", StringComparison.OrdinalIgnoreCase)) return "melee";
                string n = GetWeaponName(weapon);
                if (string.Equals(n, "M67", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n, "MilkBomb", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(n, "GingerbreadBomb", StringComparison.OrdinalIgnoreCase)) return "grenade";
            }
            catch { }
            return "primary";
        }

        private object FindWeaponByName(string runtimeName)
        {
            if (_weaponTotal == null || string.IsNullOrEmpty(runtimeName)) return null;
            for (int i = 0; i < _weaponTotal.Count; i++)
            {
                object w = _weaponTotal[i];
                if (string.Equals(GetWeaponName(w), runtimeName, StringComparison.OrdinalIgnoreCase)) return w;
            }
            return null;
        }

        private static string GetWeaponName(object weapon)
        {
            if (weapon == null) return "";
            try
            {
                FieldInfo fi = weapon.GetType().GetField("weaponName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return fi != null ? Convert.ToString(fi.GetValue(weapon)) : "";
            }
            catch { return ""; }
        }

        private bool ZombieInventoryContains(string runtimeName)
        {
            if (string.IsNullOrEmpty(runtimeName)) return false;
            return string.Equals(_primarySlots[0], runtimeName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(_primarySlots[1], runtimeName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(_meleeSlot, runtimeName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(_grenadeSlot, runtimeName, StringComparison.OrdinalIgnoreCase);
        }

        private string GetSelectedWeaponName()
        {
            if (_weaponManager == null) return "";
            try
            {
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return selected != null ? GetWeaponName(selected.GetValue(_weaponManager)) : "";
            }
            catch { return ""; }
        }

        private static bool IsUpgradeInteraction(string type)
        {
            if (string.IsNullOrEmpty(type)) return false;
            return string.Equals(type, "upgrade", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(type, "weapon_upgrade", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(type, "gun_upgrade", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(type, "forge", StringComparison.OrdinalIgnoreCase);
        }

        private int GetZombieWeaponStar(string runtimeName)
        {
            if (string.IsNullOrEmpty(runtimeName)) return 1;
            int star;
            if (!_weaponStars.TryGetValue(runtimeName, out star))
            {
                star = 1;
                _weaponStars[runtimeName] = star;
            }
            return Mathf.Clamp(star, 1, MAX_ZOMBIE_WEAPON_STARS);
        }

        private bool IsPrimaryInventoryWeapon(string runtimeName)
        {
            if (string.IsNullOrEmpty(runtimeName)) return false;
            return string.Equals(_primarySlots[0], runtimeName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(_primarySlots[1], runtimeName, StringComparison.OrdinalIgnoreCase);
        }

        private void TryStartWeaponUpgrade(ZombieCustomObject machine)
        {
            if (machine == null || _weaponUpgradeLocked || !EnsureZombieInventory()) return;
            string runtime = GetSelectedWeaponName();
            if (!IsPrimaryInventoryWeapon(runtime)) { ShowModeMessage("Hold a primary weapon", 2f); return; }
            int star = GetZombieWeaponStar(runtime);
            if (star >= MAX_ZOMBIE_WEAPON_STARS) { ShowModeMessage(runtime + " is fully upgraded", 2f); return; }
            int cost = machine.price > 0 ? machine.price : DEFAULT_UPGRADE_COST;
            if (_points < cost) { ShowModeMessage("Not enough points", 2f); return; }

            string slotToken = "";
            if (string.Equals(_primarySlots[0], runtime, StringComparison.OrdinalIgnoreCase)) { _primarySlots[0] = null; slotToken = "p0"; }
            else if (string.Equals(_primarySlots[1], runtime, StringComparison.OrdinalIgnoreCase)) { _primarySlots[1] = null; slotToken = "p1"; }
            if (slotToken.Length == 0) return;

            _points -= cost;
            _weaponUpgradeName = runtime;
            _weaponUpgradeSlot = slotToken;
            _weaponUpgradeLocked = true;
            _weaponUpgradeCompleteAt = Time.realtimeSinceStartup + (machine.upgradeSeconds > 0.1f ? machine.upgradeSeconds : DEFAULT_UPGRADE_SECONDS);
            _nearUpgradeObject = null;
            _nearWallBuy = null;
            ForceUnarmedWeaponState();
            ShowModeMessage("Upgrading " + runtime + "...", 2f);
        }

        private void UpdateWeaponUpgradeLock()
        {
            if (!_weaponUpgradeLocked) return;
            ForceUnarmedWeaponState();
            if (Time.realtimeSinceStartup < _weaponUpgradeCompleteAt) return;

            string runtime = _weaponUpgradeName;
            if (_weaponUpgradeSlot == "p0") _primarySlots[0] = runtime;
            else if (_weaponUpgradeSlot == "p1") _primarySlots[1] = runtime;
            int nextStar = Mathf.Min(MAX_ZOMBIE_WEAPON_STARS, GetZombieWeaponStar(runtime) + 1);
            _weaponStars[runtime] = nextStar;
            _weaponUpgradeLocked = false;
            _weaponUpgradeName = "";
            _weaponUpgradeSlot = "";
            RebuildZombieInventory(runtime);
            object weapon = FindWeaponByName(runtime);
            if (weapon != null) RefillWeaponAmmo(weapon, "primary");
            ShowModeMessage(runtime + " upgraded to " + nextStar + " stars - ammo refilled", 3f);
        }

        private void ForceUnarmedWeaponState()
        {
            if (_weaponList == null || _weaponManager == null || _weaponManagerType == null) return;
            try
            {
                object sentinel = FindWeaponByName(_weaponUpgradeName);
                if (sentinel == null && _weaponList.Count > 0) sentinel = _weaponList[0];
                if (sentinel == null && _weaponTotal != null && _weaponTotal.Count > 0) sentinel = _weaponTotal[0];

                if (_weaponTotal != null)
                {
                    for (int i = 0; i < _weaponTotal.Count; i++)
                    {
                        Component c = _weaponTotal[i] as Component;
                        if (c != null) c.gameObject.SetActive(false);
                    }
                }

                // WeaponManager.Update() indexes allWeapons[index] before checking Count,
                // so never leave this list empty. One inactive sentinel keeps index valid,
                // makes Count < 2 (blocking vanilla swaps), and renders/fires nothing.
                _weaponList.Clear();
                if (sentinel != null) _weaponList.Add(sentinel);
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo idx = _weaponManagerType.GetField("index", f);
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                FieldInfo canSwitch = _weaponManagerType.GetField("canSwitch", f);
                if (_weaponList.Count > 0)
                {
                    if (idx != null) idx.SetValue(_weaponManager, 0);
                    if (selected != null) selected.SetValue(_weaponManager, _weaponList[0]);
                }
                if (canSwitch != null) canSwitch.SetValue(_weaponManager, false);
                if (PlayerLogic.mInstance != null) PlayerLogic.mInstance.mWeaponType = WeaponType.Nil;

                Type nguiType = FindType("NGUI");
                if (nguiType != null)
                {
                    FieldInfo inst = nguiType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    object ngui = inst != null ? inst.GetValue(null) : null;
                    FieldInfo cur = nguiType.GetField("curWeapon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (ngui != null && cur != null) cur.SetValue(ngui, "");
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ForceUnarmedWeaponState err: " + ex.Message); }
        }

        private int ScaleZombieWeaponDamage(int rawDamage)
        {
            if (rawDamage <= 0 || rawDamage >= 9000) return rawDamage;
            string runtime = GetSelectedWeaponName();
            if (string.IsNullOrEmpty(runtime)) return rawDamage;
            int star = GetZombieWeaponStar(runtime);
            float normalized = rawDamage;
            int profileLevel = GetPermanentDamageLevel(runtime);
            float baseAverage = GetVanillaDamageAverage(runtime, 1);
            float profileAverage = GetVanillaDamageAverage(runtime, profileLevel);
            if (baseAverage > 0.01f && profileAverage > 0.01f)
                normalized = rawDamage * (baseAverage / profileAverage);
            float starMult = 1f + (star - 1) * WEAPON_DAMAGE_PER_STAR;
            return Mathf.Max(1, Mathf.RoundToInt(normalized * starMult));
        }

        private static int GetPermanentDamageLevel(string runtime)
        {
            string key = null;
            if (runtime == "MP5KA4") key = "AK";
            else if (runtime == "STW-25") key = "M4";
            else if (runtime == "Deagle") key = "Deagle";
            else if (runtime == "M87T") key = "Rifle";
            else if (runtime == "GLOCK21") key = "GLOCK21";
            else if (runtime == "MP5KA5") key = "MP5KA5";
            else if (runtime == "UZI") key = "MP5KA5"; // vanilla damage code uses this key
            else if (runtime == "G36K") key = "G36K";
            else if (runtime == "AUG") key = "AUG";
            else if (runtime == "M3") key = "M3";
            else if (runtime == "M134") key = "M134";
            else if (runtime == "G36K1") key = "G36K1";
            else if (runtime == "RAZER") key = "RAZER";
            else if (runtime == "M1Carbine") key = "M1Carbine";
            else if (runtime == "TeslaP1") key = "TeslaP1";
            if (string.IsNullOrEmpty(key)) return 1;
            return Mathf.Clamp(PlayerPrefs.GetInt(key, 1), 1, 3);
        }

        private static float GetVanillaDamageAverage(string runtime, int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            if (runtime == "Deagle") return level == 1 ? 20f : (level == 2 ? 27.5f : 35f);
            if (runtime == "MP5KA4") return level == 1 ? 17f : (level == 2 ? 21f : 26.5f);
            if (runtime == "STW-25") return level == 1 ? 16f : (level == 2 ? 21f : 25f);
            if (runtime == "GLOCK21") return level == 1 ? 17.5f : (level == 2 ? 25f : 35f);
            if (runtime == "G36K") return level == 1 ? 21f : (level == 2 ? 27.5f : 40f);
            if (runtime == "MP5KA5") return level == 1 ? 12f : (level == 2 ? 16f : 26.5f);
            if (runtime == "UZI") return level == 1 ? 10.5f : (level == 2 ? 16f : 23f);
            if (runtime == "AUG" || runtime == "M134") return level == 1 ? 30f : (level == 2 ? 33.5f : 42.5f);
            if (runtime == "G36K1") return level == 1 ? 25f : (level == 2 ? 29f : 33f);
            if (runtime == "RAZER") return level == 1 ? 12f : (level == 2 ? 16f : 26.5f);
            if (runtime == "M1Carbine") return level == 1 ? 17.5f : (level == 2 ? 22.5f : 29f);
            if (runtime == "TeslaP1") return level == 1 ? 22.5f : (level == 2 ? 27f : 32f);
            if (runtime == "M87T") return level == 1 ? 9.5f : (level == 2 ? 11f : 14f);
            if (runtime == "M3") return level == 1 ? 10.5f : (level == 2 ? 12f : 16.5f);
            return 0f;
        }

        private void RebuildZombieInventory(string selectName)
        {
            if (_weaponList == null || _weaponTotal == null || _weaponManager == null) return;
            try
            {
                for (int i = 0; i < _weaponList.Count; i++)
                {
                    Component old = _weaponList[i] as Component;
                    if (old != null) old.gameObject.SetActive(false);
                }
                _weaponList.Clear();
                AddInventoryWeapon(_primarySlots[0]);
                AddInventoryWeapon(_primarySlots[1]);
                AddInventoryWeapon(_meleeSlot);
                AddInventoryWeapon(_grenadeSlot);
                if (_weaponList.Count == 0) return;

                int selectedIndex = 0;
                for (int i = 0; i < _weaponList.Count; i++)
                {
                    Component c = _weaponList[i] as Component;
                    if (c != null) c.gameObject.SetActive(false);
                    if (string.Equals(GetWeaponName(_weaponList[i]), selectName, StringComparison.OrdinalIgnoreCase)) selectedIndex = i;
                }
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo idx = _weaponManagerType.GetField("index", f);
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                FieldInfo canSwitch = _weaponManagerType.GetField("canSwitch", f);
                if (idx != null) idx.SetValue(_weaponManager, selectedIndex);
                if (selected != null) selected.SetValue(_weaponManager, _weaponList[selectedIndex]);
                if (canSwitch != null) canSwitch.SetValue(_weaponManager, !_weaponUpgradeLocked);
                SelectZombieWeaponViaVanilla(selectedIndex);
                GameObject ng = GameObject.Find("UI Root (3D)");
                if (ng != null) ng.SendMessage("receiveGunName", GetWeaponName(_weaponList[selectedIndex]), SendMessageOptions.DontRequireReceiver);
                ApplySelectedZombieWeaponState(_weaponList[selectedIndex]);
                _zombieInventoryApplied = true;
            }
            catch (Exception ex) { ZombieModEntry.Log("RebuildZombieInventory err: " + ex.Message); }
        }

        private void SelectZombieWeaponViaVanilla(int selectedIndex)
        {
            try
            {
                if (_weaponManager == null || _weaponList == null || selectedIndex < 0 || selectedIndex >= _weaponList.Count) return;
                Component chosen = _weaponList[selectedIndex] as Component;
                if (chosen == null) return;

                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo idx = _weaponManagerType.GetField("index", f);
                FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                int oldIndex = idx != null ? Convert.ToInt32(idx.GetValue(_weaponManager)) : selectedIndex;
                object oldWeapon = selected != null ? selected.GetValue(_weaponManager) : null;
                Component oldComp = oldWeapon as Component;

                if (oldComp == null || oldComp == chosen || oldIndex == selectedIndex)
                {
                    for (int i = 0; i < _weaponList.Count; i++)
                    {
                        Component c = _weaponList[i] as Component;
                        if (c != null && c != chosen) c.gameObject.SetActive(false);
                    }
                    chosen.gameObject.SetActive(true);
                    chosen.gameObject.SendMessage("selectWeapon", SendMessageOptions.DontRequireReceiver);
                    if (idx != null) idx.SetValue(_weaponManager, selectedIndex);
                    if (selected != null) selected.SetValue(_weaponManager, chosen);
                    return;
                }

                if (idx != null) idx.SetValue(_weaponManager, oldIndex);
                if (selected != null) selected.SetValue(_weaponManager, oldComp);
                MonoBehaviour mb = _weaponManager as MonoBehaviour;
                if (mb != null)
                {
                    object routine = _weaponManagerType.InvokeMember("SwitchWeapons",
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null, _weaponManager, new object[] { oldComp.gameObject, chosen.gameObject });
                    IEnumerator enumerator = routine as IEnumerator;
                    if (enumerator != null) mb.StartCoroutine(enumerator);
                    else chosen.gameObject.SetActive(true);
                }
                else
                {
                    oldComp.gameObject.SetActive(false);
                    chosen.gameObject.SetActive(true);
                    chosen.gameObject.SendMessage("selectWeapon", SendMessageOptions.DontRequireReceiver);
                }
                if (idx != null) idx.SetValue(_weaponManager, selectedIndex);
                if (selected != null) selected.SetValue(_weaponManager, chosen);
            }
            catch (Exception ex) { ZombieModEntry.Log("SelectZombieWeaponViaVanilla err: " + ex.Message); }
        }

        private void ApplySelectedZombieWeaponState(object weapon)
        {
            try
            {
                string runtime = GetWeaponName(weapon);
                Type nguiType = FindType("NGUI");
                if (nguiType != null)
                {
                    FieldInfo inst = nguiType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    object ngui = inst != null ? inst.GetValue(null) : null;
                    FieldInfo cur = nguiType.GetField("curWeapon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (ngui != null && cur != null) cur.SetValue(ngui, runtime);
                }
                if (PlayerLogic.mInstance != null)
                    PlayerLogic.mInstance.mWeaponType = RuntimeNameToWeaponType(runtime);
                if (CNRMultiplayerManager.mInstance != null && CNRMultiplayerManager.mInstance.myPlayerInfo != null)
                    CNRMultiplayerManager.mInstance.myPlayerInfo.mWeaponType = RuntimeNameToWeaponType(runtime);

                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo canFire = weapon.GetType().GetField("canFire", f);
                FieldInfo noBullets = weapon.GetType().GetField("noBullets", f);
                FieldInfo isReload = weapon.GetType().GetField("isReload", f);
                FieldInfo singleFire = weapon.GetType().GetField("singleFire", f);
                FieldInfo fire = weapon.GetType().GetField("fire", f);
                if (canFire != null) canFire.SetValue(weapon, true);
                if (noBullets != null) noBullets.SetValue(weapon, false);
                if (isReload != null) isReload.SetValue(weapon, false);
                if (singleFire != null) singleFire.SetValue(weapon, true);
                if (fire != null) fire.SetValue(weapon, false);
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplySelectedZombieWeaponState err: " + ex.Message); }
        }

        private void DiagnoseZombieWeaponFireState()
        {
            if (_zombieWeaponDiagTimer > 0f) { _zombieWeaponDiagTimer -= Time.deltaTime; return; }
            if (PlayerPrefs.GetInt("FpsOnFire", 0) != 1) return;
            _zombieWeaponDiagTimer = 0.5f;
            try
            {
                object weapon = null;
                if (_weaponManager != null && _weaponManagerType != null)
                {
                    BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                    FieldInfo selected = _weaponManagerType.GetField("SelectedWeapon", f);
                    weapon = selected != null ? selected.GetValue(_weaponManager) : null;
                }
                Component c = weapon as Component;
                string runtime = GetWeaponName(weapon);
                string details = "ZombieWeaponFireDiag: selected=" + (string.IsNullOrEmpty(runtime) ? "(none)" : runtime);
                details += " active=" + (c != null ? c.gameObject.activeSelf.ToString() : "null");
                details += " enabled=" + (c != null ? ((Behaviour)c).enabled.ToString() : "null");
                details += " listCount=" + (_weaponList != null ? _weaponList.Count.ToString() : "null");
                details += " index=" + GetWeaponManagerIndex();
                details += " FpsOnFire=1";
                if (weapon != null)
                {
                    details += " GunType=" + GetFieldString(weapon, "GunType");
                    details += " canFire=" + GetFieldString(weapon, "canFire");
                    details += " noBullets=" + GetFieldString(weapon, "noBullets");
                    details += " isReload=" + GetFieldString(weapon, "isReload");
                    details += " singleFire=" + GetFieldString(weapon, "singleFire");
                    details += " ammo=" + GetWeaponAmmoDebug(weapon);
                }
                ZombieModEntry.Log(details);
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieWeaponFireDiag err: " + ex.Message); }
        }

        private int GetWeaponManagerIndex()
        {
            try
            {
                if (_weaponManager == null || _weaponManagerType == null) return -1;
                FieldInfo idx = _weaponManagerType.GetField("index", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return idx != null ? Convert.ToInt32(idx.GetValue(_weaponManager)) : -1;
            }
            catch { return -1; }
        }

        private static string GetFieldString(object obj, string name)
        {
            try
            {
                if (obj == null) return "null";
                FieldInfo fi = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object v = fi != null ? fi.GetValue(obj) : null;
                return v != null ? v.ToString() : "null";
            }
            catch { return "err"; }
        }

        private static string GetWeaponAmmoDebug(object weapon)
        {
            try
            {
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                int gunType = GetIntField(weapon, "GunType", -1);
                if (gunType == 0)
                {
                    FieldInfo machine = weapon.GetType().GetField("machineGun", f);
                    object mg = machine != null ? machine.GetValue(weapon) : null;
                    return "mg " + GetFieldString(mg, "bulletsLeft") + "/" + GetFieldString(mg, "bulletsPerClip") + " clips=" + GetFieldString(mg, "clips");
                }
                if (gunType == 2)
                {
                    FieldInfo shotgun = weapon.GetType().GetField("ShotGun", f);
                    object sg = shotgun != null ? shotgun.GetValue(weapon) : null;
                    return "sg " + GetFieldString(sg, "bulletsLeft") + "/" + GetFieldString(sg, "bulletsPerClip") + " clips=" + GetFieldString(sg, "clips");
                }
                if (gunType == 1)
                {
                    FieldInfo launcher = weapon.GetType().GetField("grenadeLauncher", f);
                    object gl = launcher != null ? launcher.GetValue(weapon) : null;
                    return "gl ammoCount=" + GetFieldString(gl, "ammoCount");
                }
                return "knife";
            }
            catch { return "err"; }
        }

        private static WeaponType RuntimeNameToWeaponType(string runtime)
        {
            if (runtime == "Deagle") return WeaponType.Deagle;
            if (runtime == "M67") return WeaponType.M67;
            if (runtime == "M87T") return WeaponType.M87T;
            if (runtime == "MP5KA4") return WeaponType.MP5KA4;
            if (runtime == "RPG") return WeaponType.RPG;
            if (runtime == "STW-25") return WeaponType.STW_25;
            if (runtime == "BallisticKnife") return WeaponType.BallisticKnife;
            if (runtime == "Blaser R93") return WeaponType.AWP;
            if (runtime == "G36K") return WeaponType.G36K;
            if (runtime == "GLOCK21") return WeaponType.GLOCK21;
            if (runtime == "MP5KA5") return WeaponType.MP5KA5;
            if (runtime == "UZI") return WeaponType.UZI;
            if (runtime == "MilkBomb") return WeaponType.MilkBomb;
            if (runtime == "M249") return WeaponType.M249;
            if (runtime == "CandyRifle") return WeaponType.CandyRifle;
            if (runtime == "ChristmasSniper") return WeaponType.ChristmasSniper;
            if (runtime == "GingerbreadBomb") return WeaponType.GingerbreadBomb;
            if (runtime == "GingerbreadKnife") return WeaponType.GingerbreadKnife;
            if (runtime == "SantaGun") return WeaponType.SantaGun;
            if (runtime == "AUG") return WeaponType.AUG;
            if (runtime == "M3") return WeaponType.M3;
            if (runtime == "M134") return WeaponType.M134;
            if (runtime == "G36K1") return WeaponType.G36K1;
            if (runtime == "RAZER") return WeaponType.RAZER;
            if (runtime == "FRF2") return WeaponType.FRF2;
            if (runtime == "M1Carbine") return WeaponType.M1Carbine;
            if (runtime == "MiniCannon") return WeaponType.MiniCannon;
            if (runtime == "TeslaP1") return WeaponType.TeslaP1;
            return WeaponType.Deagle;
        }

        private void AddInventoryWeapon(string runtimeName)
        {
            if (string.IsNullOrEmpty(runtimeName)) return;
            object w = FindWeaponByName(runtimeName);
            if (w != null && !_weaponList.Contains(w)) _weaponList.Add(w);
        }

        private static int GetIntField(object obj, string field, int fallback)
        {
            if (obj == null) return fallback;
            FieldInfo fi = obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null) return fallback;
            try { return Convert.ToInt32(fi.GetValue(obj)); } catch { return fallback; }
        }

        private static void SetIntField(object obj, string field, int value)
        {
            if (obj == null) return;
            FieldInfo fi = obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi != null) try { fi.SetValue(obj, value); } catch { }
        }

        private static bool TryGetZombieBaseAmmo(string runtime, out int clip, out int reserve)
        {
            clip = 0; reserve = 0;
            if (runtime == "MP5KA4") { clip = 40; reserve = 120; }
            else if (runtime == "STW-25") { clip = 50; reserve = 150; }
            else if (runtime == "Deagle") { clip = 10; reserve = 60; }
            else if (runtime == "M87T") { clip = 10; reserve = 80; }
            else if (runtime == "GLOCK21") { clip = 20; reserve = 60; }
            else if (runtime == "MP5KA5") { clip = 50; reserve = 200; }
            else if (runtime == "UZI") { clip = 60; reserve = 240; }
            else if (runtime == "G36K") { clip = 50; reserve = 150; }
            else if (runtime == "AUG") { clip = 50; reserve = 150; }
            else if (runtime == "M3") { clip = 7; reserve = 28; }
            else if (runtime == "M134") { clip = 80; reserve = 240; }
            else if (runtime == "G36K1") { clip = 50; reserve = 150; }
            else if (runtime == "RAZER") { clip = 50; reserve = 150; }
            else if (runtime == "M1Carbine") { clip = 50; reserve = 150; }
            else if (runtime == "TeslaP1") { clip = 40; reserve = 160; }
            return clip > 0;
        }

        private void GetOrCaptureZombieBaseAmmo(string runtime, int fallbackClip, int fallbackReserve, out int clip, out int reserve)
        {
            clip = Math.Max(1, fallbackClip);
            reserve = Math.Max(clip, fallbackReserve);
            if (string.IsNullOrEmpty(runtime)) return;
            int cachedClip, cachedReserve;
            if (_weaponBaseClip.TryGetValue(runtime, out cachedClip) && _weaponBaseReserve.TryGetValue(runtime, out cachedReserve))
            {
                clip = Math.Max(1, cachedClip);
                reserve = Math.Max(clip, cachedReserve);
                return;
            }
            _weaponBaseClip[runtime] = clip;
            _weaponBaseReserve[runtime] = reserve;
        }

        private void RefillWeaponAmmo(object weapon, string slot)
        {
            if (weapon == null || string.Equals(slot, "melee", StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                string runtime = GetWeaponName(weapon);
                int star = GetZombieWeaponStar(runtime);
                float magMult = 1f + (star - 1) * WEAPON_MAG_PER_STAR;
                float reserveMult = 1f + (star - 1) * WEAPON_RESERVE_PER_STAR;
                int baseClip, baseReserve;
                bool knownBase = TryGetZombieBaseAmmo(runtime, out baseClip, out baseReserve);
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                int gunType = GetIntField(weapon, "GunType", 0); // 0 machine gun, 1 launcher, 2 shotgun, 3 knife

                if (gunType == 0)
                {
                    FieldInfo machine = weapon.GetType().GetField("machineGun", f);
                    object mg = machine != null ? machine.GetValue(weapon) : null;
                    if (mg != null)
                    {
                        int fallbackClip = Math.Max(1, GetIntField(mg, "bulletsPerClip", 30));
                        int fallbackReserve = Math.Max(fallbackClip, GetIntField(mg, "clips", fallbackClip * 3));
                        if (!knownBase) GetOrCaptureZombieBaseAmmo(runtime, fallbackClip, fallbackReserve, out baseClip, out baseReserve);
                        int mag = Mathf.Max(1, Mathf.CeilToInt(baseClip * magMult));
                        int reserve = Mathf.Max(mag, Mathf.CeilToInt(baseReserve * reserveMult));
                        SetIntField(mg, "bulletsPerClip", mag);
                        SetIntField(mg, "bulletsLeft", mag);
                        SetIntField(mg, "clips", reserve);
                    }
                }
                else if (gunType == 2)
                {
                    FieldInfo shotgun = weapon.GetType().GetField("ShotGun", f);
                    object sg = shotgun != null ? shotgun.GetValue(weapon) : null;
                    if (sg != null)
                    {
                        int fallbackClip = Math.Max(1, GetIntField(sg, "bulletsPerClip", 8));
                        int fallbackReserve = Math.Max(fallbackClip, GetIntField(sg, "clips", fallbackClip * 3));
                        if (!knownBase) GetOrCaptureZombieBaseAmmo(runtime, fallbackClip, fallbackReserve, out baseClip, out baseReserve);
                        int mag = Mathf.Max(1, Mathf.CeilToInt(baseClip * magMult));
                        int reserve = Mathf.Max(mag, Mathf.CeilToInt(baseReserve * reserveMult));
                        SetIntField(sg, "bulletsPerClip", mag);
                        SetIntField(sg, "bulletsLeft", mag);
                        SetIntField(sg, "clips", reserve);
                    }
                }
                else if (gunType == 1)
                {
                    FieldInfo launcher = weapon.GetType().GetField("grenadeLauncher", f);
                    object gl = launcher != null ? launcher.GetValue(weapon) : null;
                    if (gl != null)
                    {
                        int fallback = Math.Max(1, GetIntField(gl, "ammoCount", string.Equals(slot, "grenade", StringComparison.OrdinalIgnoreCase) ? 3 : 6));
                        if (!knownBase) GetOrCaptureZombieBaseAmmo(runtime, 1, fallback, out baseClip, out baseReserve);
                        SetIntField(gl, "ammoCount", Mathf.Max(1, Mathf.CeilToInt(baseReserve * reserveMult)));
                    }
                }
                RefillBowAmmoContainer(weapon, runtime, slot, reserveMult);

                FieldInfo noBullets = weapon.GetType().GetField("noBullets", f);
                FieldInfo reloadFlag = weapon.GetType().GetField("reloadFlag", f);
                FieldInfo isReload = weapon.GetType().GetField("isReload", f);
                if (noBullets != null) noBullets.SetValue(weapon, false);
                if (reloadFlag != null) reloadFlag.SetValue(weapon, true);
                if (isReload != null) isReload.SetValue(weapon, false);
                ZombieModEntry.Log("ZombieWeaponAmmo: " + runtime + " star=" + star + " magMult=" + magMult.ToString("F2") + " reserveMult=" + reserveMult.ToString("F2"));
            }
            catch (Exception ex) { ZombieModEntry.Log("RefillWeaponAmmo err: " + ex.Message); }
        }

        private void RefillBowAmmoContainer(object weapon, string runtime, string slot, float reserveMult)
        {
            try
            {
                BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo bowField = weapon.GetType().GetField("bow", f);
                object bow = bowField != null ? bowField.GetValue(weapon) : null;
                if (bow == null) return;
                int baseClip, baseReserve;
                bool knownBase = TryGetZombieBaseAmmo(runtime, out baseClip, out baseReserve);
                int fallback = string.Equals(slot, "grenade", StringComparison.OrdinalIgnoreCase) ? 3 : 100;
                int reserve = knownBase ? baseReserve : fallback;
                SetIntField(bow, "ammoCount", Mathf.Max(1, Mathf.CeilToInt(reserve * reserveMult)));
            }
            catch (Exception ex) { ZombieModEntry.Log("RefillBowAmmoContainer err: " + ex.Message); }
        }

        private void StartZombieMode(Vector3 origin)
        {
            if (_templateGO == null)
            {
                ZombieModEntry.Log("StartZombieMode: no template GO - prefab not cached yet");
                _hud = "[ZombieMod] No enemy prefab - visit FreeRun first";
                return;
            }

            if (!_astarBuilt)
            {
                LogSceneLayers(origin);
                BuildAstarGraph(origin);
                ZombieNavGrid.Bake(origin);
                _astarBuilt = true;
            }

            ResetVanillaRoundOverState(true);
            _modeStarted = true;
            MarkZombieSessionActive();
            _masterSpawned = true;
            _nextZombieId = 1;
            _round = 0;
            _points = STARTING_POINTS;
            _zombiesTotal = 0;
            _zombiesKilled = 0;
            _zombiesRemaining = 0;
            _spawnQueue = 0;
            _phase = PHASE_WAITING;
            _phaseTimer = ROUND_START_DELAY;
            _localPlayerDowned = false;
            _killFeed.Clear();
            CachePlayerLogicFields();
            ShowModeMessage("ZOMBIES - ROUND 1 IN " + Mathf.CeilToInt(_phaseTimer), 3f);
            ZombieModEntry.Log("ZombieMode: started, first round in " + ROUND_START_DELAY + "s");
        }

        private void UpdateMasterMode(float dt, GameObject localPlayer)
        {
            if (_phase == PHASE_GAMEOVER) return;

            UpdateDeathTracking(dt);
            if (_phase != PHASE_GAMEOVER && IsAllPlayersDown())
                TriggerGameOver();

                if (_phase == PHASE_WAITING)
            {
                _phaseTimer -= dt;
                if (_phaseTimer <= 0f)
                    BeginRound(_round + 1);
            }
            else if (_phase == PHASE_ACTIVE)
            {
                // Detect vanilla-killed zombies: SingleEnemyLogic.decreaseBlood destroys the
                // GO after ~3s. When that happens ZombieDriver becomes Unity-null.
                List<byte> vanillaDead = null;
                foreach (var kv in _drivers)
                    if (kv.Value == null) { if (vanillaDead == null) vanillaDead = new List<byte>(); vanillaDead.Add(kv.Key); }
                if (vanillaDead != null)
                    for (int vi = 0; vi < vanillaDead.Count; vi++) KillZombie(vanillaDead[vi], null);

                UpdateSpawning(dt, localPlayer);
                if (_zombiesRemaining <= 0 && _spawnQueue <= 0 && _drivers.Count == 0)
                    EndRound();
            }
            else if (_phase == PHASE_INTER)
            {
                _phaseTimer -= dt;
                if (_phaseTimer <= 0f)
                    BeginRound(_round + 1);
            }
        }

        private void BeginRound(int round)
        {
            _downedPlayers.Clear();
            RestoreAllPeerPlayerObjects();
            ReviveLocalPlayer();  // un-down the player before each new round
            _round = round;
            _phase = PHASE_ACTIVE;
            _phaseTimer = 0f;
            _spawnTimer = 0f;
            _zombiesKilled = 0;
            _zombiesTotal = Mathf.Min(
                Mathf.FloorToInt(BASE_ZOMBIES_PER_ROUND + round * ZOMBIES_PER_ROUND_MULT),
                MAX_ZOMBIES_PER_ROUND);
            _zombiesRemaining = _zombiesTotal;
            _spawnQueue = _zombiesTotal;
            ShowModeMessage("ROUND " + _round, 3f);
            ZombieModEntry.Log("ZombieMode: begin round=" + _round + " total=" + _zombiesTotal +
                " hp=" + ZombieBalance.HealthForRound(_round) +
                " speed=" + ZombieBalance.ChaseSpeedForRound(_round).ToString("F2") +
                " dmg=" + ZombieBalance.AttackDamageForRound(_round) +
                " cd=" + ZombieBalance.AttackCooldownForRound(_round).ToString("F2") +
                " expectedPower=" + ZombieBalance.ExpectedPlayerPower(_round).ToString("F2"));
        }

        private void EndRound()
        {
            _phase = PHASE_INTER;
            _phaseTimer = INTER_ROUND_DELAY;
            _points += END_ROUND_BONUS;
            ShowModeMessage("ROUND " + _round + " COMPLETE  +" + END_ROUND_BONUS, 4f);
            ZombieModEntry.Log("ZombieMode: end round=" + _round + " points=" + _points);
        }

        // ── Death system helpers ─────────────────────────────────────────────

        private void CachePlayerLogicFields()
        {
            if (_fPlBlood != null) return;  // already cached
            try
            {
                Type plType = FindType("PlayerLogic");
                if (plType == null) { ZombieModEntry.Log("CachePlayerLogicFields: PlayerLogic type not found"); return; }
                _fPlMInstance  = plType.GetField("mInstance",  BindingFlags.Public | BindingFlags.Static);
                _fPlBlood      = plType.GetField("blood",       BindingFlags.Public | BindingFlags.Instance);
                _fPlBDied      = plType.GetField("bDied",       BindingFlags.Public | BindingFlags.Instance);
                _fPlMStatus    = plType.GetField("mStatus",     BindingFlags.Public | BindingFlags.Instance);
                _fPlGoDied     = plType.GetField("goDied",      BindingFlags.Public | BindingFlags.Instance);
                _fPlKilledNum  = plType.GetField("killedNum",   BindingFlags.Public | BindingFlags.Instance);
                ZombieModEntry.Log("CachePlayerLogicFields: mInstance=" + (_fPlMInstance != null) +
                    " blood=" + (_fPlBlood != null) + " bDied=" + (_fPlBDied != null) +
                    " mStatus=" + (_fPlMStatus != null) + " goDied=" + (_fPlGoDied != null) +
                    " killedNum=" + (_fPlKilledNum != null));
            }
            catch (Exception ex) { ZombieModEntry.Log("CachePlayerLogicFields err: " + ex.Message); }
        }

        private void ForceCopTeamIfNeeded()
        {
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType == null) return;
                FieldInfo fiInst = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = fiInst != null ? fiInst.GetValue(null) : null;
                if (mgrInst == null) return;

                FieldInfo fiPlayer = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                object playerInfo = fiPlayer != null ? fiPlayer.GetValue(mgrInst) : null;
                if (playerInfo == null) return;

                Type playerType = playerInfo.GetType();
                FieldInfo fiTeam = playerType.GetField("mTeam", BindingFlags.Public | BindingFlags.Instance);
                if (fiTeam == null) return;

                object teamValue = fiTeam.GetValue(playerInfo);
                int teamInt = teamValue != null ? Convert.ToInt32(teamValue) : 0;
                if (teamInt == 1) return;

                fiTeam.SetValue(playerInfo, Enum.ToObject(fiTeam.FieldType, 1));

                MethodInfo synTeam = mgrType.GetMethod("SynTeamNumber", BindingFlags.Public | BindingFlags.Instance);
                if (synTeam != null) synTeam.Invoke(mgrInst, null);
                ZombieModEntry.Log("ForceCopTeam: local player forced to cops");
            }
            catch (Exception ex) { ZombieModEntry.Log("ForceCopTeamIfNeeded err: " + ex.Message); }
        }

        private bool IsLocalPlayerPastTeamSelection()
        {
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType == null) return false;
                FieldInfo fiInst = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = fiInst != null ? fiInst.GetValue(null) : null;
                if (mgrInst == null) return false;

                FieldInfo fiPlayer = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                object playerInfo = fiPlayer != null ? fiPlayer.GetValue(mgrInst) : null;
                if (playerInfo == null) return false;

                Type playerType = playerInfo.GetType();
                FieldInfo fiTeam = playerType.GetField("mTeam", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo fiConn = playerType.GetField("mConnnectStatus", BindingFlags.Public | BindingFlags.Instance);
                if (fiTeam == null || fiConn == null) return false;

                object teamValue = fiTeam.GetValue(playerInfo);
                object connValue = fiConn.GetValue(playerInfo);
                int teamInt = teamValue != null ? Convert.ToInt32(teamValue) : 0;
                int connInt = connValue != null ? Convert.ToInt32(connValue) : 0;

                // TeamType.Cop == 1 and ConnectStatus.InGame == 5 in the base game.
                return teamInt == 1 && connInt == 5;
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("IsLocalPlayerPastTeamSelection err: " + ex.Message);
                return false;
            }
        }

        private void UpdateDeathTracking(float dt)
        {
            _killFeedTimer -= dt;

            if (_fPlMInstance == null || _fPlBlood == null || _fPlBDied == null) return;
            object plInst = _fPlMInstance.GetValue(null);
            if (plInst == null) return;

            if (!_localPlayerDowned)
            {
                // Detect vanilla death: bDied went true AND blood ≤ 0
                bool bDied = (bool)_fPlBDied.GetValue(plInst);
                int  blood = (int) _fPlBlood.GetValue(plInst);
                if (bDied && blood <= 0)
                {
                    _localPlayerDowned = true;
                    ZombieModEntry.Log("ZombieMod: local player DOWNED by zombie");
                    AddKillFeedEntry("ZOMBIE killed you");
                    ReportPlayerDowned();
                    HideLocalPlayerModel();
                    // AttachSpectatorCamera();
                    // Suppress the vanilla single-player death panel — it shows two blank
                    // buttons in FreeRun, and clicking the respawn button crashes the game
                    // because SingleModeRespawnControl.mInstance is null in this scene.
                    SuppressVanillaDiedPanel();
                    // In solo play there is exactly one human — game over immediately.
                    // In multi, compare downed count vs room size (simple solo check for now).
                    if (IsAllPlayersDown())
                        TriggerGameOver();
                }
            }
            else
            {
                // Keep the player permanently dead until round ends / next round begins.
                // This overrides the 5-second vanilla waitForGeneratePlayer respawn.
                _fPlBlood.SetValue(plInst, 0);
                _fPlBDied.SetValue(plInst, true);
                HideLocalPlayerModel();
                // AttachSpectatorCamera();
                // Keep suppressing the panel every tick — the vanilla respawn coroutine
                // (waitForGeneratePlayer) can re-show it after a countdown.
                SuppressVanillaDiedPanel();
            }
        }

        private void SuppressVanillaDiedPanel()
        {
            try
            {
                Type t = FindType("UISingleGameSceneDirector");
                if (t == null) return;
                FieldInfo fiInst = t.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object inst = fiInst != null ? fiInst.GetValue(null) : null;
                if (inst == null) return;
                FieldInfo fiDied = t.GetField("diedPanel", BindingFlags.Public | BindingFlags.Instance);
                if (fiDied == null) return;
                GameObject dp = fiDied.GetValue(inst) as GameObject;
                if (dp != null && dp.activeSelf)
                {
                    dp.SetActive(false);
                    ZombieModEntry.Log("ZombieMod: suppressed vanilla diedPanel");
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("SuppressVanillaDiedPanel err: " + ex.Message); }
        }

        private void ReviveLocalPlayer()
        {
            if (!_localPlayerDowned) return;
            _localPlayerDowned = false;  // clear flag before writes so UpdateDeathTracking won't fight us
            string localId = GetLocalPeerId();
            if (!string.IsNullOrEmpty(localId)) _downedPlayers.Remove(localId);

            if (_fPlMInstance == null || _fPlBlood == null || _fPlBDied == null) return;
            object plInst = _fPlMInstance.GetValue(null);
            if (plInst == null) return;

            try
            {
                _fPlBlood.SetValue(plInst, 100);
                _fPlBDied.SetValue(plInst, false);

                // Set mStatus back to idle (PlayerStatus.idle == 1)
                if (_fPlMStatus != null) _fPlMStatus.SetValue(plInst, 1);

                // Re-enable CharacterController (disabled on death)
                Component plComp = plInst as Component;
                if (plComp != null)
                {
                    CharacterController cc = plComp.gameObject.GetComponent<CharacterController>();
                    if (cc != null) ((Collider)(object)cc).enabled = true;
                }

                // Hide the death overlay (goDied.renderer.enabled = false)
                if (_fPlGoDied != null)
                {
                    GameObject goDied = _fPlGoDied.GetValue(plInst) as GameObject;
                    if (goDied != null)
                    {
                        Renderer r = goDied.GetComponent<Renderer>();
                        if (r != null) r.enabled = false;
                    }
                }

                RestoreLocalPlayerModel();
                // RestoreSpectatorCamera();
                ZombieModEntry.Log("ZombieMod: local player revived for next round");
            }
            catch (Exception ex) { ZombieModEntry.Log("ReviveLocalPlayer err: " + ex.Message); }
        }

        private void ReportPlayerDowned()
        {
            try
            {
                string localId = GetLocalPeerId();
                if (string.IsNullOrEmpty(localId)) return;
                var ht = new System.Collections.Hashtable();
                ht["pd"] = localId;
                RaiseZombieEvent(ht, true);
            }
            catch (Exception ex) { ZombieModEntry.Log("ReportPlayerDowned err: " + ex.Message); }
        }

        private void ResetVanillaRoundOverState(bool clearScores)
        {
            try
            {
                Type mgr = FindType("CNRMultiplayerManager");
                if (mgr == null) return;
                BindingFlags instFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo fiMgr = mgr.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = fiMgr != null ? fiMgr.GetValue(null) : null;
                if (mgrInst == null) return;

                FieldInfo fiRoundFlag = mgr.GetField("roundOverChkStatusFlag", instFlags);
                if (fiRoundFlag != null && Convert.ToInt32(fiRoundFlag.GetValue(mgrInst)) != 0)
                    fiRoundFlag.SetValue(mgrInst, 0);

                FieldInfo fiResult = mgr.GetField("isClacedResult", instFlags);
                if (fiResult != null && (bool)fiResult.GetValue(mgrInst))
                    fiResult.SetValue(mgrInst, false);

                FieldInfo fiVerify = mgr.GetField("shVerifyTimeCount", instFlags);
                if (fiVerify != null) fiVerify.SetValue(mgrInst, 0f);

                FieldInfo fiMode = mgr.GetField("myModeInfo", instFlags);
                object modeInfo = fiMode != null ? fiMode.GetValue(mgrInst) : null;
                if (modeInfo == null) return;
                FieldInfo fiKci = modeInfo.GetType().GetField("mKillingCompetitionInfo", instFlags);
                object kci = fiKci != null ? fiKci.GetValue(modeInfo) : null;
                if (kci == null) return;

                FieldInfo fiCop = kci.GetType().GetField("copKilling", instFlags);
                FieldInfo fiRobber = kci.GetType().GetField("robberKilling", instFlags);
                FieldInfo fiMax = kci.GetType().GetField("MAXKilling", instFlags);
                if (fiCop == null || fiRobber == null || fiMax == null) return;

                int cop = Convert.ToInt32(fiCop.GetValue(kci));
                int robber = Convert.ToInt32(fiRobber.GetValue(kci));
                int max = Convert.ToInt32(fiMax.GetValue(kci));
                if (clearScores || cop >= max || robber >= max)
                {
                    if (cop != 0 || robber != 0)
                        ZombieModEntry.Log("ZombieMode: cleared stale vanilla round score " + cop + "-" + robber);
                    fiCop.SetValue(kci, 0);
                    fiRobber.SetValue(kci, 0);
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ResetVanillaRoundOverState err: " + ex.Message); }
        }

        private void TriggerGameOver()
        {
            _phase = PHASE_GAMEOVER;
            var ht = new System.Collections.Hashtable();
            ht["gd"] = 1;
            RaiseZombieEvent(ht, true);
            ShowModeMessage("GAME OVER", 999f);
            ZombieModEntry.Log("ZombieMod: GAME OVER — all players downed");
            // Force the local player's team score to MAXKilling so the vanilla TDM
            // round-over check fires on the next frame and pops the scoreboard.
            // The scoreboard reads killedNum per player, so zombie kills are already
            // reflected there (we increment killedNum in KillZombie).
            try
            {
                Type mgr = FindType("CNRMultiplayerManager");
                if (mgr == null) { ZombieModEntry.Log("TriggerGameOver: CNRMultiplayerManager not found"); return; }
                FieldInfo fiMgr = mgr.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = fiMgr != null ? fiMgr.GetValue(null) : null;
                if (mgrInst == null) { ZombieModEntry.Log("TriggerGameOver: CNRMultiplayerManager.mInstance null"); return; }

                // Get myModeInfo
                FieldInfo fiMode = mgr.GetField("myModeInfo", BindingFlags.Public | BindingFlags.Instance);
                if (fiMode == null) { ZombieModEntry.Log("TriggerGameOver: myModeInfo field not found"); return; }
                object modeInfo = fiMode.GetValue(mgrInst);
                if (modeInfo == null) { ZombieModEntry.Log("TriggerGameOver: myModeInfo null"); return; }

                // Get mKillingCompetitionInfo
                FieldInfo fiKCI = modeInfo.GetType().GetField("mKillingCompetitionInfo",
                    BindingFlags.Public | BindingFlags.Instance);
                if (fiKCI == null) { ZombieModEntry.Log("TriggerGameOver: mKillingCompetitionInfo field not found"); return; }
                object kci = fiKCI.GetValue(modeInfo);
                if (kci == null) { ZombieModEntry.Log("TriggerGameOver: mKillingCompetitionInfo null"); return; }

                // Read MAXKilling
                FieldInfo fiMax = kci.GetType().GetField("MAXKilling",
                    BindingFlags.Public | BindingFlags.Instance);
                int maxKilling = fiMax != null ? (int)fiMax.GetValue(kci) : 40;

                // Determine player's team and set that team's score to max
                FieldInfo fiMyInfo = mgr.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                object myInfo = fiMyInfo != null ? fiMyInfo.GetValue(mgrInst) : null;
                bool isCop = true;  // default cops
                if (myInfo != null)
                {
                    FieldInfo fiTeam = myInfo.GetType().GetField("mTeam",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (fiTeam != null)
                    {
                        object teamVal = fiTeam.GetValue(myInfo);
                        // TeamType.Cop == 1, TeamType.Robber == 2
                        isCop = (teamVal != null && (int)teamVal == 1);
                    }
                }

                string teamField = isCop ? "copKilling" : "robberKilling";
                FieldInfo fiScore = kci.GetType().GetField(teamField,
                    BindingFlags.Public | BindingFlags.Instance);
                if (fiScore != null)
                {
                    fiScore.SetValue(kci, maxKilling);
                    ZombieModEntry.Log("TriggerGameOver: set " + teamField + "=" + maxKilling
                        + " (MAXKilling) to trigger scoreboard");
                }
                else
                {
                    ZombieModEntry.Log("TriggerGameOver: " + teamField + " field not found");
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("TriggerGameOver err: " + ex.Message + "\n" + ex.StackTrace); }
        }

        private void AddKillFeedEntry(string msg)
        {
            _killFeed.Add(msg);
            _killFeedTimer = KILL_FEED_SECS;
            if (_killFeed.Count > 4) _killFeed.RemoveAt(0);
            TryPushIntoJoinFeed(msg);
        }

        private void TryPushIntoJoinFeed(string msg)
        {
            try
            {
                Type mgrType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    mgrType = asm.GetType("CRMultiplayerManager");
                    if (mgrType != null) break;
                }
                if (mgrType == null) return;

                FieldInfo instField = mgrType.GetField("mInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (instField == null) return;
                object mgr = instField.GetValue(null);
                if (mgr == null) return;

                FieldInfo msgField = mgrType.GetField("mMessageList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (msgField == null) return;
                var list = msgField.GetValue(mgr) as List<string>;
                if (list == null) return;

                list.Add(msg);
                if (list.Count > 20) list.RemoveRange(0, list.Count - 20);
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("TryPushIntoJoinFeed err: " + ex.Message);
            }
        }

        // ── End death system helpers ─────────────────────────────────────────

        private void UpdateSpawning(float dt, GameObject localPlayer)
        {
            if (_spawnQueue <= 0 || _drivers.Count >= MAX_ACTIVE_ZOMBIES) return;
            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            // Spawn near a random alive player so remote players get pressure too,
            // instead of every zombie spawning around the host.
            Vector3 origin = GetRandomAlivePlayerPosition(localPlayer);
            SpawnZombie(origin);
            _spawnQueue--;

            float interval = BASE_SPAWN_INTERVAL / (1f + _round * SPAWN_INTERVAL_DECAY);
            _spawnTimer = Mathf.Max(MIN_SPAWN_INTERVAL, interval);
        }

        private Vector3 GetRandomAlivePlayerPosition(GameObject localPlayer)
        {
            List<Vector3> alive = new List<Vector3>();
            string localId = GetLocalPeerId();
            if (localPlayer != null && !IsPeerDowned(localId))
                alive.Add(localPlayer.transform.position);
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                FieldInfo fiInst = mgrType != null ? mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static) : null;
                object mgrInst = fiInst != null ? fiInst.GetValue(null) : null;
                if (mgrInst != null)
                {
                    BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                    FieldInfo objField = mgrType.GetField("otherPlayerObject", flags);
                    FieldInfo infoField = mgrType.GetField("otherPlayersInfoList", flags);
                    Array objects = objField != null ? objField.GetValue(mgrInst) as Array : null;
                    Array infos = infoField != null ? infoField.GetValue(mgrInst) as Array : null;
                    if (objects != null && infos != null)
                    {
                        int count = Math.Min(objects.Length, infos.Length);
                        for (int i = 0; i < count; i++)
                        {
                            GameObject go = objects.GetValue(i) as GameObject;
                            object info = infos.GetValue(i);
                            if (go == null || info == null) continue;
                            FieldInfo idField = info.GetType().GetField("mId", flags);
                            string peerId = idField != null ? Convert.ToString(idField.GetValue(info)) : null;
                            if (string.IsNullOrEmpty(peerId) || peerId == "null") continue;
                            if (IsPeerDowned(peerId)) continue;
                            alive.Add(go.transform.position);
                        }
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("GetRandomAlivePlayerPosition err: " + ex.Message); }

            if (alive.Count == 0)
                return localPlayer != null ? localPlayer.transform.position : Vector3.zero;
            return alive[UnityEngine.Random.Range(0, alive.Count)];
        }

        private void ShowModeMessage(string message, float secs)
        {
            _modeMessage = message;
            _modeMessageTimer = secs;
        }

        private string PhaseName()
        {
            if (_phase == PHASE_ACTIVE) return "ACTIVE";
            if (_phase == PHASE_INTER) return "NEXT";
            if (_phase == PHASE_GAMEOVER) return "GAME OVER";
            return "STARTING";
        }

        private void DrawZombieHud()
        {
            if (!_modeStarted && !IsInRoom()) return;

            float ui = Mathf.Max(1f, Screen.height / 720f);

            // GUIStyle allocation per OnGUI call is expensive on mobile — build once.
            if (_styleBig == null)
            {
                _styleBig = new GUIStyle(GUI.skin.label);
                _styleBig.fontSize = (int)(22f * ui);
                _styleBig.fontStyle = FontStyle.Bold;
                _styleBig.alignment = TextAnchor.MiddleCenter;
                _styleBig.normal.textColor = Color.white;

                _styleSmall = new GUIStyle(GUI.skin.label);
                _styleSmall.fontSize = (int)(14f * ui);
                _styleSmall.normal.textColor = Color.white;

                _styleDowned = new GUIStyle(GUI.skin.label);
                _styleDowned.fontSize  = (int)(28f * ui);
                _styleDowned.fontStyle = FontStyle.Bold;
                _styleDowned.alignment = TextAnchor.MiddleCenter;
                _styleDowned.normal.textColor = new Color(1f, 0.2f, 0.2f, 1f);

                _styleDownedSub = new GUIStyle(GUI.skin.label);
                _styleDownedSub.fontSize  = (int)(16f * ui);
                _styleDownedSub.alignment = TextAnchor.MiddleCenter;
                _styleDownedSub.normal.textColor = Color.white;
            }
            GUIStyle big = _styleBig;
            GUIStyle small = _styleSmall;

            string top = "ROUND " + Mathf.Max(1, _round) + "  " + PhaseName();
            if (_phase == PHASE_WAITING || _phase == PHASE_INTER)
                top += " " + Mathf.CeilToInt(Mathf.Max(0f, _phaseTimer));
            GUI.Label(new Rect(Screen.width * 0.5f - 170f * ui, 12f * ui, 340f * ui, 32f * ui), top, big);

            GUI.Label(new Rect(20f * ui, Screen.height - 92f * ui, 280f * ui, 24f * ui),
                "ZOMBIE POINTS: " + _points, small);

            GUI.Label(new Rect(Screen.width - 250f * ui, 18f * ui, 230f * ui, 24f * ui),
                "ZOMBIES: " + _zombiesRemaining + "  KILLS: " + _zombiesKilled, small);
            if (_spawnQueue > 0)
                GUI.Label(new Rect(Screen.width - 250f * ui, 42f * ui, 230f * ui, 24f * ui),
                    "SPAWN QUEUE: " + _spawnQueue, small);

            if (_modeMessageTimer > 0f && _modeMessage != null && _modeMessage.Length > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 220f * ui, Screen.height * 0.28f, 440f * ui, 42f * ui), _modeMessage, big);

            // DOWNED overlay
            if (_localPlayerDowned)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 200f * ui, Screen.height * 0.5f - 24f * ui,
                    400f * ui, 50f * ui), "YOU WERE DOWNED", _styleDowned);
                GUI.Label(new Rect(Screen.width * 0.5f - 200f * ui, Screen.height * 0.5f + 30f * ui,
                    400f * ui, 28f * ui),
                    _phase == PHASE_GAMEOVER ? "GAME OVER" : "Waiting for round end...", _styleDownedSub);
            }

        }

        private void ToggleNavDebug(Vector3 center)
        {
            _navDebugEnabled = !_navDebugEnabled;
            if (_navDebugEnabled)
            {
                GameObject ec = GameObject.Find("ExampleCharacter");
                if (ec != null) center = ec.transform.position;
                ZombieModEntry.Log("NavDebug: F12 enabled");
                BuildBlockFaceOverlay(center);
            }
            else
            {
                ZombieModEntry.Log("NavDebug: F12 disabled");
                ClearNavDebug();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Try to grab the enemy prefab reference from any available source,
        // then IMMEDIATELY instantiate it into a DontDestroyOnLoad holder so the
        // reference is valid forever regardless of scene unloads.
        // ─────────────────────────────────────────────────────────────────────
        private void TryCachePrefab()
        {
            // If we already have a template that includes SingleEnemyAI (the full AI prefab),
            // we're done — no need to upgrade.
            if (_templateGO != null)
                return;

            string scene = Application.loadedLevelName;
            ZombieModEntry.Log("TryCachePrefab: scene=" + scene);

            try
            {
                UnityEngine.Object source = FindSourcePrefab();
                if (source == null)
                {
                    ZombieModEntry.Log("TryCachePrefab: no source found in " + scene);
                    return;
                }

                // Instantiate immediately so the ref isn't scene-lifetime-bound
                UnityEngine.Object inst = UnityEngine.Object.Instantiate(source);
                GameObject go = inst is GameObject ? (GameObject)inst
                             : inst is Component   ? ((Component)inst).gameObject : null;
                if (go == null) { ZombieModEntry.Log("TryCachePrefab: Instantiate returned non-GO"); return; }

                // Disable SingleEnemyLogic — its Start() crashes in multiplayer (no
                // SingleEnemyManager.mInstance).  SingleEnemyAI stays enabled so clones
                // can use A* once the graph is built.
                DisableComponent(go, "SingleEnemyLogic");
                DisableComponent(go, "SingleEnemyAI");
                PrepareZombieVisuals(go);
                go.SetActive(false);
                go.name = "ZombiePrefabTemplate";
                UnityEngine.Object.DontDestroyOnLoad(go);

                _templateGO = go;
                Type aiType = FindType("SingleEnemyAI");
                bool hasAI = aiType != null && (go.GetComponent(aiType) != null || go.GetComponentInChildren(aiType) != null);
                ZombieModEntry.Log("TryCachePrefab: template GO created from " + source.name + " hasAI=" + hasAI);
            }
            catch (Exception ex) { ZombieModEntry.Log("TryCachePrefab err: " + ex.Message + "\n" + ex.StackTrace); }
        }

        // Scan known fields for an enemy prefab reference.
        // Prefers Game.mInstance.enemyBot3 (Enemy3) over the other enemy bot
        // fields. This mod no longer uses knifeEnemy at all.
        private static UnityEngine.Object FindSourcePrefab()
        {
            // Source 1 (PREFERRED): Game.mInstance.enemyBot3
            // This is the enemy3 prefab the zombie logic is built around.
            Type gameType = FindType("Game");
            if (gameType != null)
            {
                FieldInfo miField = gameType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object gameInst = miField != null ? miField.GetValue(null) : null;
                ZombieModEntry.Log("FindSourcePrefab: Game.mInstance=" + (gameInst != null ? gameInst.ToString() : "null"));
                if (gameInst != null)
                {
                    foreach (string fn in new[] { "enemyBot3", "enemyBot", "enemyBot1", "enemyBot2", "enemyBot4" })
                    {
                        FieldInfo f = gameType.GetField(fn, BindingFlags.Public | BindingFlags.Instance);
                        if (f == null) continue;
                        UnityEngine.Object v = (UnityEngine.Object)f.GetValue(gameInst);
                        ZombieModEntry.Log("FindSourcePrefab: Game." + fn + "=" + (v != null ? v.name : "null"));
                        if (v != null) return v;
                    }
                }
            }

            // Source 2 (LAST RESORT): Resources.Load by name — works if the
            // prefab was placed in a Resources/ folder in the original project.
            // Lets the mod work without ever visiting singleplayer.
            foreach (string rn in new string[0])
            {
                UnityEngine.Object v = Resources.Load(rn);
                ZombieModEntry.Log("FindSourcePrefab: Resources.Load(" + rn + ")=" + (v != null ? v.name : "null"));
                if (v != null) return v;
            }

            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: spawn N enemy GOs using the cached game prefab.
        // Immediately disables SingleEnemyLogic (before its Start() fires) so
        // it never accesses SingleEnemyManager.mInstance or PlayerLogic.mInstance.
        // ZombieDriver is added as the replacement state machine.
        // ─────────────────────────────────────────────────────────────────────
        private void DumpEnemy3Model()
        {
            try
            {
                GameObject root = FindEnemy3DumpRoot();
                if (root == null)
                {
                    ZombieModEntry.Log("DumpEnemy3Model: no Enemy3 root found");
                    return;
                }

                Directory.CreateDirectory(ENEMY_DUMP_DIR);
                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string baseName = "Enemy3_" + stamp;
                string objPath = Path.Combine(ENEMY_DUMP_DIR, baseName + ".obj");
                string txtPath = Path.Combine(ENEMY_DUMP_DIR, baseName + "_hierarchy.txt");
                DumpEnemy3Obj(root, objPath, txtPath);
                ZombieModEntry.Log("DumpEnemy3Model: wrote " + objPath + " and " + txtPath);
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("DumpEnemy3Model err: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private GameObject FindEnemy3DumpRoot()
        {
            try
            {
                GameObject[] all = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                for (int i = 0; i < all.Length; i++)
                {
                    GameObject go = all[i] as GameObject;
                    if (go == null) continue;
                    string n = go.name ?? "";
                    if (n.IndexOf("ZombieEnemy_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("Enemy3", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        n.IndexOf("ZombiePrefabTemplate", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (go.GetComponentsInChildren<Renderer>(true).Length > 0)
                            return go;
                    }
                }
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("FindEnemy3DumpRoot err: " + ex.Message);
            }

            if (_templateGO != null)
                return _templateGO;

            return null;
        }

        private void DumpEnemy3Obj(GameObject root, string objPath, string hierarchyPath)
        {
            Component[] components = root.GetComponentsInChildren(typeof(Renderer), true);
            var obj = new System.Text.StringBuilder();
            var hierarchy = new System.Text.StringBuilder();
            int vOffset = 0;
            int dumped = 0;

            obj.AppendLine("# Enemy3 dump from CNRZombieMod");
            obj.AppendLine("# root=" + root.name);

            WriteEnemyHierarchy(root.transform, hierarchy, 0);

            for (int i = 0; i < components.Length; i++)
            {
                Renderer r = components[i] as Renderer;
                if (r == null || !ShouldDumpEnemyRenderer(r)) continue;

                Mesh mesh = GetDumpMesh(r);
                if (mesh == null || mesh.vertexCount == 0) continue;

                Vector3[] verts = mesh.vertices;
                Vector2[] uvs = mesh.uv;
                Vector3[] norms = mesh.normals;

                obj.AppendLine("g " + MakeSafe(r.gameObject.name));

                Transform t = r.transform;
                for (int vi = 0; vi < verts.Length; vi++)
                {
                    Vector3 w = t.TransformPoint(verts[vi]);
                    obj.AppendLine("v " + F(w.x) + " " + F(w.y) + " " + F(w.z));
                }

                for (int ui = 0; ui < verts.Length; ui++)
                {
                    if (uvs != null && ui < uvs.Length)
                        obj.AppendLine("vt " + F(uvs[ui].x) + " " + F(uvs[ui].y));
                    else
                        obj.AppendLine("vt 0.000000 0.000000");
                }

                for (int ni = 0; ni < verts.Length; ni++)
                {
                    if (norms != null && ni < norms.Length)
                    {
                        Vector3 n = t.TransformDirection(norms[ni]);
                        obj.AppendLine("vn " + F(n.x) + " " + F(n.y) + " " + F(n.z));
                    }
                    else
                    {
                        obj.AppendLine("vn 0.000000 1.000000 0.000000");
                    }
                }

                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    int[] tris = mesh.GetTriangles(sub);
                    for (int ti = 0; ti < tris.Length; ti += 3)
                    {
                        int a = tris[ti] + vOffset + 1;
                        int b = tris[ti + 1] + vOffset + 1;
                        int c = tris[ti + 2] + vOffset + 1;
                        obj.AppendLine("f " + a + "/" + a + "/" + a + " " + b + "/" + b + "/" + b + " " + c + "/" + c + "/" + c);
                    }
                }

                vOffset += verts.Length;
                dumped++;
                ZombieModEntry.Log("DumpEnemy3Obj: " + r.gameObject.name + " verts=" + verts.Length + " tris=" + (mesh.triangles.Length / 3));
            }

            File.WriteAllText(objPath, obj.ToString());
            File.WriteAllText(hierarchyPath, hierarchy.ToString());
            ZombieModEntry.Log("DumpEnemy3Obj: dumped renderers=" + dumped);
        }

        private static Mesh GetDumpMesh(Renderer r)
        {
            SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
            if (smr != null)
            {
                Mesh baked = new Mesh();
                smr.BakeMesh(baked);
                return baked;
            }

            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null)
                return mf.sharedMesh != null ? mf.sharedMesh : mf.mesh;

            return null;
        }

        private static bool ShouldDumpEnemyRenderer(Renderer r)
        {
            if (r == null) return false;
            if (!r.enabled) return false;
            string n = r.gameObject != null ? (r.gameObject.name ?? "") : "";
            string l = n.ToLowerInvariant();
            if (l.IndexOf("health") >= 0 ||
                l.IndexOf("hp") >= 0 ||
                l.IndexOf("bar") >= 0 ||
                l.IndexOf("label") >= 0 ||
                l.IndexOf("name") >= 0 ||
                l.IndexOf("canvas") >= 0 ||
                l.IndexOf("ui") >= 0 ||
                l.IndexOf("icon") >= 0 ||
                l.IndexOf("shadow") >= 0 ||
                l.IndexOf("effect") >= 0 ||
                l.IndexOf("particle") >= 0 ||
                l.IndexOf("weapon") >= 0 ||
                l.IndexOf("gun") >= 0 ||
                l.IndexOf("knife") >= 0 ||
                l.IndexOf("ammo") >= 0)
                return false;
            return r.GetComponent<Canvas>() == null && r.GetComponent<RectTransform>() == null;
        }

        private static void WriteEnemyHierarchy(Transform t, System.Text.StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            string extras = "";
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
            {
                Mesh m = GetDumpMesh(r);
                if (m != null)
                    extras = " [mesh:" + m.name + " " + m.vertexCount + "v " + (m.triangles.Length / 3) + "t]";
                if (r.material != null)
                    extras += " [mat:" + r.material.name + "]";
            }

            sb.AppendLine(indent + t.gameObject.name + extras);
            for (int i = 0; i < t.childCount; i++)
                WriteEnemyHierarchy(t.GetChild(i), sb, depth + 1);
        }

        private static string MakeSafe(string s)
        {
            if (string.IsNullOrEmpty(s)) return "part";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            return sb.ToString();
        }

        private static string F(float v)
        {
            return v.ToString("F6");
        }

        // Allocates a free zombie id (1..250), wrapping and skipping ids still
        // in use. Returns 0 when every id is taken (caller should skip spawn).
        private byte AllocZombieId()
        {
            for (int i = 0; i < 250; i++)
            {
                if (_nextZombieId > 250 || _nextZombieId < 1) _nextZombieId = 1;
                byte id = (byte)_nextZombieId++;
                if (!_drivers.ContainsKey(id) && !_zombieHealth.ContainsKey(id)) return id;
            }
            return 0;
        }

        private List<Vector3> GetAlivePlayerPositionsForSpawning()
        {
            List<Vector3> alive = new List<Vector3>();
            GameObject local = GetLocalPlayerGO();
            string localId = GetLocalPeerId();
            if (local != null && !IsPeerDowned(localId)) alive.Add(local.transform.position);
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                FieldInfo fiInst = mgrType != null ? mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static) : null;
                object mgrInst = fiInst != null ? fiInst.GetValue(null) : null;
                if (mgrInst != null)
                {
                    BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                    FieldInfo of = mgrType.GetField("otherPlayerObject", f);
                    FieldInfo inf = mgrType.GetField("otherPlayersInfoList", f);
                    Array objects = of != null ? of.GetValue(mgrInst) as Array : null;
                    Array infos = inf != null ? inf.GetValue(mgrInst) as Array : null;
                    if (objects != null && infos != null)
                    {
                        int count = Math.Min(objects.Length, infos.Length);
                        for (int i = 0; i < count; i++)
                        {
                            GameObject go = objects.GetValue(i) as GameObject;
                            object info = infos.GetValue(i);
                            if (go == null || info == null) continue;
                            FieldInfo idf = info.GetType().GetField("mId", f);
                            string peer = idf != null ? Convert.ToString(idf.GetValue(info)) : "";
                            if (string.IsNullOrEmpty(peer) || IsPeerDowned(peer)) continue;
                            alive.Add(go.transform.position);
                        }
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("GetAlivePlayerPositionsForSpawning err: " + ex.Message); }
            return alive;
        }

        private bool TryChooseConfiguredSpawn(out Vector3 pos)
        {
            pos = Vector3.zero;
            if (_zombieMapConfig == null || _zombieMapConfig.zombieSpawns == null || _zombieMapConfig.zombieSpawns.Length == 0)
                return false;
            List<Vector3> players = GetAlivePlayerPositionsForSpawning();
            if (players.Count == 0) return false;

            int bestIndex = -1;
            float bestScore = float.MaxValue;
            for (int i = 0; i < _zombieMapConfig.zombieSpawns.Length; i++)
            {
                ZombieSpawnPoint sp = _zombieMapConfig.zombieSpawns[i];
                if (sp == null || !ValidVec3(sp.pos)) continue;
                Vector3 p = ToVec3(sp.pos);
                float nearest = float.MaxValue;
                for (int j = 0; j < players.Count; j++)
                {
                    float d = Vector3.Distance(p, players[j]);
                    if (d < nearest) nearest = d;
                }
                float min = sp.minDistance > 0f ? sp.minDistance : SPAWN_MIN_DISTANCE;
                float max = sp.maxDistance > min ? sp.maxDistance : SPAWN_MAX_DISTANCE;
                if (nearest < min || nearest > max) continue;

                float score = Mathf.Abs(nearest - SPAWN_IDEAL_DISTANCE);
                float last;
                if (_spawnLastUsed.TryGetValue(i, out last))
                {
                    float age = Time.time - last;
                    if (age < 4f) score += (4f - age) * 5f;
                }
                if (IsSpawnVisibleToAnyPlayer(p, players)) score += 12f;
                score += UnityEngine.Random.Range(0f, 1.5f);
                if (score < bestScore) { bestScore = score; bestIndex = i; }
            }
            if (bestIndex < 0) return false;

            pos = ToVec3(_zombieMapConfig.zombieSpawns[bestIndex].pos);
            _spawnLastUsed[bestIndex] = Time.time;
            Vector3 nav;
            if (ZombieNavGrid.Ready && ZombieNavGrid.TrySnapToWalkable(pos, 24, out nav))
            {
                pos.x = nav.x;
                pos.z = nav.z;
                pos.y = RootYForGround(_templateGO != null ? _templateGO.GetComponent<CharacterController>() : null, nav.y);
            }
            return true;
        }

        private static bool IsSpawnVisibleToAnyPlayer(Vector3 spawn, List<Vector3> players)
        {
            Vector3 from = spawn + Vector3.up * 1.1f;
            for (int i = 0; i < players.Count; i++)
            {
                Vector3 to = players[i] + Vector3.up * 1.1f;
                if (!Physics.Linecast(from, to)) return true;
            }
            return false;
        }

        internal bool RelocateZombieForRecovery(ZombieDriver driver, string reason)
        {
            if (driver == null || !IsMasterClientNow()) return false;
            Vector3 pos;
            if (!TryChooseConfiguredSpawn(out pos))
            {
                GameObject local = GetLocalPlayerGO();
                Vector3 origin = GetRandomAlivePlayerPosition(local);
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float radius = UnityEngine.Random.Range(SPAWN_RADIUS * 0.85f, SPAWN_RADIUS * 1.2f);
                pos = origin + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                Vector3 nav;
                if (ZombieNavGrid.Ready && ZombieNavGrid.TrySnapToWalkable(pos, 24, out nav))
                {
                    pos.x = nav.x;
                    pos.z = nav.z;
                    pos.y = RootYForGround(driver.GetComponent<CharacterController>(), nav.y);
                }
            }
            driver.TeleportForRecovery(pos);
            ZombieModEntry.Log("ZombieRecovery: id=" + driver.ZombieId + " reason=" + reason + " -> " + pos);
            return true;
        }

        private void SpawnZombie(Vector3 origin)
        {
            if (_templateGO == null) return;

            byte id = AllocZombieId();
            if (id == 0) return;  // all 250 ids in use
            Vector3 pos;
            if (!TryChooseConfiguredSpawn(out pos))
            {
                // No configured spawn markers (or none are currently usable): retain the
                // original radial implementation as the compatibility fallback.
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float radius = UnityEngine.Random.Range(SPAWN_RADIUS * 0.75f, SPAWN_RADIUS * 1.25f);
                pos = origin + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                Vector3 navPos;
                if (ZombieNavGrid.Ready && ZombieNavGrid.TrySnapToWalkable(pos, 24, out navPos))
                {
                    pos.x = navPos.x;
                    pos.z = navPos.z;
                    pos.y = RootYForGround(_templateGO.GetComponent<CharacterController>(), navPos.y);
                }
            }

            try
            {
                int variant = PickZombieVariant();
                GameObject enemyGO = (GameObject)UnityEngine.Object.Instantiate(_templateGO, pos, Quaternion.identity);
                enemyGO.SetActive(true);
                enemyGO.name = "ZombieEnemy_" + id + "_v" + (variant + 1);
                PrepareZombieVisuals(enemyGO);
                ApplyZombieSkin(enemyGO, variant);

                DisableComponent(enemyGO, "SingleEnemyLogic");
                DisableComponent(enemyGO, "SingleEnemyAI");

                // Only the attack transform is used (ZombieDriver's move target);
                // SingleEnemyLogic is disabled so the other three were never read.
                CreateTempTransform("enemyTempAttackTransform"   + id, pos);

                    ZombieDriver drv = enemyGO.AddComponent<ZombieDriver>();
                    drv.ZombieId = id;
                    drv.ChaseSpeed = ZombieBalance.ChaseSpeedForRound(_round);
                    drv.AttackDamage = ZombieBalance.AttackDamageForRound(_round);
                    drv.AttackCooldown = ZombieBalance.AttackCooldownForRound(_round);
                    drv.MaxHealth = ZombieBalance.HealthForRound(_round);
                    drv.Hook = this;
                    _zombieHealth[id] = drv.MaxHealth;
                    _zombieVariant[id] = variant;
                    drv.ApplySharedHealth(drv.MaxHealth);

                    _drivers[id] = drv;
                ZombieModEntry.Log("SpawnZombie: id=" + id + " round=" + _round + " pos=" + pos);
                if (!_enemyModelAutoDumped)
                {
                    _enemyModelAutoDumped = true;
                    DumpEnemy3Model();
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("SpawnZombie err: " + ex.Message); }
        }

        private static float RootYForGround(CharacterController cc, float groundY)
        {
            if (cc != null)
                return groundY + Mathf.Max(0.05f, cc.height * 0.5f - cc.center.y + 0.05f);
            return groundY + 0.1f;
        }

        private static void DisableComponent(GameObject go, string typeName)
        {
            Type t = FindType(typeName);
            if (t == null) return;
            Component[] comps = go.GetComponentsInChildren(t, true);
            for (int i = 0; i < comps.Length; i++)
            {
                Behaviour b = comps[i] as Behaviour;
                if (b != null) b.enabled = false;
            }
        }

        // Called by ZombieDriver.OnDestroy (vanilla kill) or ClearAll.
        internal void OnZombieVanillaDeath(byte id)
        {
            if (!_drivers.ContainsKey(id)) return;  // already counted
            KillZombie(id, null);
        }

        private void KillZombie(byte id, GameObject go)
        {
            string attackerId;
            _zombieLastAttacker.TryGetValue(id, out attackerId);
            KillZombie(id, go, attackerId);
        }

        private void KillZombie(byte id, GameObject go, string attackerId)
        {
            Vector3 dropPos = Vector3.zero;
            bool haveDropPos = false;
            ZombieDriver drv;
            if (_drivers.TryGetValue(id, out drv))
            {
                if (drv != null)
                {
                    dropPos = drv.transform.position + Vector3.up * 0.25f;
                    haveDropPos = true;
                }
                _drivers.Remove(id);
            }
            if (!haveDropPos && go != null)
            {
                dropPos = go.transform.position + Vector3.up * 0.25f;
                haveDropPos = true;
            }
            _zombieHealth.Remove(id);
            _zombieVariant.Remove(id);
            _zombieLastAttacker.Remove(id);

            // Only destroy if caller passed a live GO (vanilla system already destroys it).
            if (go != null) UnityEngine.Object.Destroy(go);

            // Clean up this id's move-target GO so the hierarchy doesn't grow forever.
            GameObject tempTarget = GameObject.Find("enemyTempAttackTransform" + id);
            if (tempTarget != null) UnityEngine.Object.Destroy(tempTarget);

            _zombiesKilled++;
            _zombiesRemaining = Mathf.Max(0, _zombiesRemaining - 1);
            int award = POINTS_PER_KILL + Mathf.Max(0, _round - 1) * POINTS_PER_ROUND_BONUS;
            bool localAward = string.IsNullOrEmpty(attackerId) ||
                string.Equals(attackerId, GetLocalPeerId(), StringComparison.Ordinal);
            if (localAward)
            {
                _points += award;
                AddLocalZombieKillStat("KillZombie");
                if (haveDropPos) CNRMods.CNRPerkSystem.TrySpawnKillDrops(dropPos);
                ShowModeMessage("+" + award + " ZOMBIE KILL", 1.5f);
            }
            ZombieModEntry.Log("ZombieKilled: id=" + id + " round=" + _round +
                " points=" + _points + " remaining=" + _zombiesRemaining);
            if (!string.IsNullOrEmpty(attackerId))
            {
                var ht = new System.Collections.Hashtable();
                if (!string.Equals(attackerId, GetLocalPeerId(), StringComparison.Ordinal))
                    ht["kc"] = attackerId;
                ht["zk"] = (int)id;
                RaiseZombieEvent(ht, true);
            }
        }

        internal static void PrepareZombieVisuals(GameObject go)
        {
            if (go == null) return;
            int hidden = 0;
            int disabled = 0;

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || r.transform == null) continue;
                if (!LooksLikeHeldWeapon(r.transform)) continue;
                r.enabled = false;
                hidden++;
            }

            BuildEnemyOverlay(go);
            RemapEnemyAtlas(go);

            if (!_enemyRendererDumped)
            {
                _enemyRendererDumped = true;
                DumpEnemyRenderers(go);
            }

            Behaviour[] behaviours = go.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour b = behaviours[i];
                if (b == null || b is Animation || b is Animator || b is ZombieDriver || b is ZombieProxy) continue;
                string n = b.GetType().Name.ToLowerInvariant();
                if (n.IndexOf("weapon") < 0 && n.IndexOf("gun") < 0 &&
                    n.IndexOf("shoot") < 0 && n.IndexOf("fire") < 0 &&
                    n.IndexOf("bullet") < 0 && n.IndexOf("launcher") < 0)
                    continue;
                b.enabled = false;
                disabled++;
            }

            if (hidden > 0 || disabled > 0)
                ZombieModEntry.Log("PrepareZombieVisuals: hiddenWeaponRenderers=" + hidden +
                    " disabledWeaponBehaviours=" + disabled + " go=" + go.name);
        }

        private static void RemapEnemyAtlas(GameObject go)
        {
            if (go == null || go.transform.Find("__EnemyAtlasRemapped") != null) return;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || r.transform == null) continue;
                if (LooksLikeHeldWeapon(r.transform) || LooksLikeEnemyOverlay(r.transform)) continue;
                string name = r.gameObject != null ? r.gameObject.name : "";
                if (!IsEnemySkinPartName(name)) continue;

                Mesh mesh = CloneRendererMesh(r);
                if (mesh == null) continue;
                RemapMeshUv(mesh, ENEMY_BASE_ATLAS, name);
            }

            GameObject marker = new GameObject("__EnemyAtlasRemapped");
            marker.transform.parent = go.transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localRotation = Quaternion.identity;
            marker.transform.localScale = Vector3.one;
            marker.hideFlags = HideFlags.HideInHierarchy;
        }

        private static void BuildEnemyOverlay(GameObject go)
        {
            if (go == null) return;

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            int created = 0;
            int bodyCandidates = 0;
            int skippedSpecial = 0;
            int skippedNonBody = 0;
            int skippedExisting = 0;
            int skippedNoMesh = 0;
            int skippedNoUvRemap = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer src = renderers[i];
                if (src == null || src.transform == null) continue;
                if (LooksLikeHeldWeapon(src.transform) || LooksLikeEnemyHealthBar(src.transform) ||
                    LooksLikeEnemyOverlay(src.transform))
                {
                    skippedSpecial++;
                    continue;
                }
                string name = src.gameObject != null ? src.gameObject.name : "";
                if (!IsEnemyBodyPartName(name))
                {
                    skippedNonBody++;
                    continue;
                }
                bodyCandidates++;
                if (src.transform.Find("__EnemyOverlay") != null)
                {
                    skippedExisting++;
                    continue;
                }

                Mesh srcMesh = GetRendererMesh(src);
                if (srcMesh == null)
                {
                    skippedNoMesh++;
                    continue;
                }

                Mesh overlayMesh = (Mesh)UnityEngine.Object.Instantiate(srcMesh);
                if (!RemapMeshUv(overlayMesh, ENEMY_LAYER_ATLAS, name))
                {
                    skippedNoUvRemap++;
                    continue;
                }

                GameObject overlay = new GameObject("__EnemyOverlay");
                overlay.transform.parent = src.transform;
                overlay.transform.localPosition = Vector3.zero;
                overlay.transform.localRotation = Quaternion.identity;
                overlay.transform.localScale = Vector3.one * 1.04f;
                overlay.hideFlags = HideFlags.HideInHierarchy;

                MeshFilter mf = overlay.AddComponent<MeshFilter>();
                mf.mesh = overlayMesh;
                MeshRenderer mr = overlay.AddComponent<MeshRenderer>();
                Material mat = src.material != null ? new Material(src.material) : new Material(Shader.Find("Diffuse"));
                ConfigureOverlayMaterial(mat);
                mr.material = mat;
                try { mr.receiveShadows = false; } catch { }
                created++;
            }

            ZombieModEntry.Log("EnemyOverlay: root=" + go.name +
                " renderers=" + renderers.Length +
                " bodyCandidates=" + bodyCandidates +
                " created=" + created +
                " skippedSpecial=" + skippedSpecial +
                " skippedNonBody=" + skippedNonBody +
                " skippedExisting=" + skippedExisting +
                " skippedNoMesh=" + skippedNoMesh +
                " skippedNoUvRemap=" + skippedNoUvRemap);
        }

        private static bool RemapMeshUv(Mesh mesh, AtlasRemap[] remaps, string partName)
        {
            if (mesh == null || remaps == null) return false;
            Vector2[] uv = mesh.uv;
            if (uv == null || uv.Length == 0) return false;
            bool changed = false;
            for (int i = 0; i < uv.Length; i++)
            {
                Vector2 next = RemapEnemyUv(uv[i], remaps, partName);
                if (next != uv[i]) changed = true;
                uv[i] = next;
            }
            mesh.uv = uv;
            return changed;
        }

        private static Vector2 RemapEnemyUv(Vector2 uv, AtlasRemap[] remaps, string partName)
        {
            float px = uv.x * 64f;
            float py = (1f - uv.y) * 32f;
            const float eps = 0.03f;
            for (int i = 0; i < remaps.Length; i++)
            {
                AtlasRemap r = remaps[i];
                if (!string.IsNullOrEmpty(r.Part) &&
                    !string.Equals(r.Part, partName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (px < r.Src.x - eps || px > r.Src.x + r.Src.width + eps ||
                    py < r.Src.y - eps || py > r.Src.y + r.Src.height + eps)
                    continue;

                float lx = r.Src.width != 0f ? (px - r.Src.x) / r.Src.width : 0f;
                float ly = r.Src.height != 0f ? (py - r.Src.y) / r.Src.height : 0f;
                float dx = r.Dst.x + lx * r.Dst.width;
                float dy = r.Dst.y + ly * r.Dst.height;
                return new Vector2(dx / 64f, 1f - (dy / 32f));
            }
            return uv;
        }

        private static Mesh CloneRendererMesh(Renderer r)
        {
            if (r == null) return null;
            SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
            if (smr != null && smr.sharedMesh != null)
            {
                Mesh clone = (Mesh)UnityEngine.Object.Instantiate(smr.sharedMesh);
                smr.sharedMesh = clone;
                return clone;
            }
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh src = mf.sharedMesh != null ? mf.sharedMesh : mf.mesh;
                if (src == null) return null;
                Mesh clone = (Mesh)UnityEngine.Object.Instantiate(src);
                mf.mesh = clone;
                return clone;
            }
            return null;
        }

        private static Mesh GetRendererMesh(Renderer r)
        {
            if (r == null) return null;
            SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
            if (smr != null && smr.sharedMesh != null) return smr.sharedMesh;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null) return mf.sharedMesh != null ? mf.sharedMesh : mf.mesh;
            return null;
        }

        private static void DumpEnemyRenderers(GameObject go)
        {
            if (go == null) return;
            try
            {
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                ZombieModEntry.Log("EnemyRendererDump: count=" + renderers.Length + " root=" + go.name);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer r = renderers[i];
                    if (r == null) continue;
                    Mesh mesh = GetRendererMesh(r);
                    string type = r.GetType().Name;
                    string path = TransformPath(r.transform);
                    int verts = mesh != null ? mesh.vertexCount : 0;
                    int tris = mesh != null ? mesh.triangles.Length / 3 : 0;
                    int mats = r.sharedMaterials != null ? r.sharedMaterials.Length : 0;
                    ZombieModEntry.Log("EnemyRendererDump: type=" + type +
                        " name=" + r.gameObject.name +
                        " path=" + path +
                        " verts=" + verts +
                        " tris=" + tris +
                        " mats=" + mats +
                        " skinned=" + (r is SkinnedMeshRenderer));
                }
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("EnemyRendererDump err: " + ex.Message);
            }
        }

        private static bool LooksLikeHeldWeapon(Transform t)
        {
            string path = TransformPath(t).ToLowerInvariant();
            if (path.IndexOf("__enemyoverlay") >= 0) return false;

            // Match any weapon slot under weaponcontrol (1_0, 1_1, 1_2, 1_3, etc.)
            // but NOT the hand bones themselves.
            int wci = path.IndexOf("weaponcontrol/");
            if (wci >= 0)
            {
                string sub = path.Substring(wci + "weaponcontrol/".Length);
                if (sub.Length >= 2 && sub[0] == '1' && sub[1] == '_' && path.IndexOf("hand") < 0)
                    return true;
            }

            // Hand paths are safe UNLESS they also contain weapon-related keywords.
            if (path.IndexOf("hand") >= 0 &&
                path.IndexOf("bullet") < 0 && path.IndexOf("gun") < 0 &&
                path.IndexOf("qiang") < 0 && path.IndexOf("weapon") < 0 &&
                path.IndexOf("rifle") < 0 && path.IndexOf("pistol") < 0 &&
                path.IndexOf("knife") < 0)
                return false;

            // Any GO that is a grandchild+ of an EnemyAnimation/1_X bone is a weapon attachment
            // e.g. EnemyAnimation/1_3/M87T/Z.  Direct bone children like EnemyAnimation/1_1
            // have no further slash after the bone segment and are body parts — leave visible.
            int eai = path.IndexOf("/enemyanimation/1_");
            if (eai >= 0)
            {
                int boneStart = eai + "/enemyanimation/1_".Length;
                int nextSlash = path.IndexOf('/', boneStart);
                if (nextSlash >= 0) return true;
            }

            return path.IndexOf("weapon") >= 0 || path.IndexOf("gun") >= 0 ||
                   path.IndexOf("rifle") >= 0 || path.IndexOf("pistol") >= 0 ||
                   path.IndexOf("snipe") >= 0 || path.IndexOf("grenade") >= 0 ||
                   path.IndexOf("knife") >= 0 || path.IndexOf("muzzle") >= 0 ||
                   path.IndexOf("firepoint") >= 0 || path.IndexOf("bullet") >= 0 ||
                   path.IndexOf("launcher") >= 0 || path.IndexOf("qiang") >= 0;
        }

        private static string TransformPath(Transform t)
        {
            if (t == null) return "";
            string s = t.name;
            Transform p = t.parent;
            while (p != null)
            {
                s = p.name + "/" + s;
                p = p.parent;
            }
            return s;
        }

        private static void CreateTempTransform(string name, Vector3 pos)
        {
            // Don't double-create
            if (GameObject.Find(name) != null) return;
            GameObject g = new GameObject(name);
            g.transform.position = pos;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: pack & RaiseEvent to other clients.
        // Reads positions directly from the driver's own GO transform
        // (ZombieDriver / SingleEnemyAI move the GO; no separate state struct needed).
        // ─────────────────────────────────────────────────────────────────────
        private void Broadcast()
        {
            float[] data = new float[_drivers.Count * STRIDE];
            float[] hpData = new float[_drivers.Count * 2];
            int idx = 0;
            foreach (var kv in _drivers)
            {
                ZombieDriver drv = kv.Value;
                if (drv == null || drv.gameObject == null) continue;
                Vector3 p  = drv.transform.position;
                float rotY = drv.transform.eulerAngles.y;
                int   b    = idx * STRIDE;
                data[b]     = kv.Key;
                data[b + 1] = p.x;
                data[b + 2] = p.y;
                data[b + 3] = p.z;
                data[b + 4] = rotY;

                float hp;
                if (drv.TryGetSharedHealth(out hp))
                {
                    _zombieHealth[kv.Key] = hp;
                }
                else if (!_zombieHealth.TryGetValue(kv.Key, out hp))
                {
                    hp = ZombieBalance.HealthForRound(_round);
                }
                int h = idx * 2;
                hpData[h] = kv.Key;
                hpData[h + 1] = hp;
                idx++;
            }

            if (idx * STRIDE != data.Length)
            {
                float[] trimmed = new float[idx * STRIDE];
                Array.Copy(data, trimmed, trimmed.Length);
                data = trimmed;
            }
            if (idx * 2 != hpData.Length)
            {
                float[] trimmedHp = new float[idx * 2];
                Array.Copy(hpData, trimmedHp, trimmedHp.Length);
                hpData = trimmedHp;
            }

            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["zd"] = data;
            ht["zhp"] = hpData;
            float[] variantData = new float[idx * 2];
            for (int i = 0; i < idx; i++)
            {
                byte id = (byte)data[i * STRIDE];
                int variant;
                if (!_zombieVariant.TryGetValue(id, out variant)) variant = 0;
                variantData[i * 2] = id;
                variantData[i * 2 + 1] = variant;
            }
            ht["zv"] = variantData;
            ht["zs"] = new float[] {
                _round, _phase, _phaseTimer, _zombiesTotal,
                _zombiesKilled, _zombiesRemaining, _spawnQueue
            };
            ht["zpdown"] = GetDownedPeerListString();

            // Positions/state are re-sent every BROADCAST_SECS anyway, so a dropped
            // packet self-heals; unreliable avoids Photon resend queue backlog that
            // caused rubber-banding for clients on weak connections.
            RaiseZombieEvent(ht, false);
        }

        private void RaiseZombieEvent(System.Collections.Hashtable ht, bool reliable)
        {
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] { typeof(byte), typeof(System.Collections.Hashtable), typeof(bool), typeof(byte) });
                if (raise != null)
                {
                    raise.Invoke(peer, new object[] { ZombieModEntry.ZOMBIE_EVENT, ht, reliable, (byte)0 });
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("RaiseZombieEvent err: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // All clients: receive zombie state
        // Non-master clients: instantiate the same enemy prefab but with
        // SingleEnemyLogic AND SingleEnemyAI disabled, and add ZombieProxy
        // which lerps to the received transform.
        // ─────────────────────────────────────────────────────────────────────
        public void OnZombieEvent(EventData ev)
        {
            try
            {
                bool authority = IsMasterClientNow();
                var ht = ExtractZombiePayload(ev);
                if (ht == null) return;
                if (!HasZombieEventPayload(ht)) return;
                MarkZombieSessionActive();

                if (ht.ContainsKey("zpdmg"))
                {
                    ApplyZombiePlayerDamageEvent(ht["zpdmg"]);
                }

                if (authority)
                {
                    if (ht.ContainsKey("zh"))
                    {
                        ApplyZombieDamageRequest(ht["zh"]);
                    }
                    if (ht.ContainsKey("pd"))
                    {
                        ApplyPlayerDownRequest(ht["pd"]);
                    }
                    if (ht.ContainsKey("kc"))
                    {
                        ApplyKillCredit(ht["kc"], ht.ContainsKey("zk") ? ht["zk"] : null);
                    }
                    if (ht.ContainsKey("zk"))
                    {
                        ApplyZombieKillSync(ht["zk"]);
                    }
                    if (ht.ContainsKey("zhp"))
                    {
                        ApplyZombieHealthSync(NormalizeFloatArray(ht["zhp"]));
                    }
                    if (ht.ContainsKey("gd"))
                    {
                        ApplyGameOverSync(ht["gd"]);
                    }
                    return;  // authority only consumes damage requests; sync is local
                }

                if (ht.ContainsKey("zs"))
                {
                    ApplySyncedModeState(NormalizeFloatArray(ht["zs"]));
                }
                if (ht.ContainsKey("kc"))
                {
                    ApplyKillCredit(ht["kc"], ht.ContainsKey("zk") ? ht["zk"] : null);
                }
                if (ht.ContainsKey("zpdown"))
                {
                    ApplyDownedPeerList(ht["zpdown"]);
                }
                if (ht.ContainsKey("zk"))
                {
                    ApplyZombieKillSync(ht["zk"]);
                }
                if (ht.ContainsKey("zv"))
                {
                    ApplyZombieVariantSync(NormalizeFloatArray(ht["zv"]));
                }
                if (ht.ContainsKey("zhp"))
                {
                    ApplyZombieHealthSync(NormalizeFloatArray(ht["zhp"]));
                }
                if (ht.ContainsKey("gd"))
                {
                    ApplyGameOverSync(ht["gd"]);
                }
                if (ht.ContainsKey("zpdmg"))
                {
                    return;
                }
                if (!ht.ContainsKey("zd")) return;
                float[] data = NormalizeFloatArray(ht["zd"]);
                if (data == null) return;

                int count = data.Length / STRIDE;
                HashSet<byte> seen = new HashSet<byte>();
                for (int i = 0; i < count; i++)
                {
                    int  b  = i * STRIDE;
                    byte id = (byte)(int)data[b];
                    seen.Add(id);
                    ZombieProxy p = GetOrCreateProxy(id);
                    p.SetTarget(data[b + 1], data[b + 2], data[b + 3], data[b + 4]);
                }

                RemoveMissingProxies(seen);

                _hud = "[ZombieMod] Client — " + count + " zombies synced";
            }
            catch (Exception ex) { ZombieModEntry.Log("OnZombieEvent err: " + ex.Message); }
        }

        private bool HasZombieEventPayload(System.Collections.Hashtable ht)
        {
            if (ht == null) return false;
            return ht.ContainsKey("zh") || ht.ContainsKey("pd") || ht.ContainsKey("kc") ||
                   ht.ContainsKey("zk") || ht.ContainsKey("zhp") || ht.ContainsKey("gd") ||
                   ht.ContainsKey("zs") || ht.ContainsKey("zpdown") || ht.ContainsKey("zv") ||
                   ht.ContainsKey("zd") || ht.ContainsKey("zpdmg");
        }

        private System.Collections.Hashtable ExtractZombiePayload(EventData ev)
        {
            try
            {
                if (ev == null || ev.Parameters == null) return null;
                if (ev.Parameters.ContainsKey((byte)245))
                {
                    var payload = ev.Parameters[(byte)245] as System.Collections.Hashtable;
                    if (payload != null) return payload;
                    var dict = ev.Parameters[(byte)245] as System.Collections.IDictionary;
                    if (dict != null)
                    {
                        var flat = new System.Collections.Hashtable();
                        foreach (var de in dict)
                        {
                            object key = GetDictKey(de);
                            object value = GetDictValue(de);
                            if (key != null) flat[key] = value;
                        }
                        return flat;
                    }
                }
                foreach (var de in ev.Parameters)
                {
                    var ht = de.Value as System.Collections.Hashtable;
                    if (ht != null) return ht;
                    var dict = de.Value as System.Collections.IDictionary;
                    if (dict != null)
                    {
                        var flat = new System.Collections.Hashtable();
                        foreach (var inner in dict)
                        {
                            object key = GetDictKey(inner);
                            object value = GetDictValue(inner);
                            if (key != null) flat[key] = value;
                        }
                        return flat;
                    }
                }
            }
            catch { }
            return null;
        }

        private object GetDictKey(object entry)
        {
            try
            {
                var t = entry.GetType();
                PropertyInfo p = t.GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return p.GetValue(entry, null);
                FieldInfo f = t.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(entry);
            }
            catch { }
            return null;
        }

        private object GetDictValue(object entry)
        {
            try
            {
                var t = entry.GetType();
                PropertyInfo p = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) return p.GetValue(entry, null);
                FieldInfo f = t.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(entry);
            }
            catch { }
            return null;
        }

        private void ApplyZombieDamageRequest(object raw)
        {
            byte id = 0;
            int dmg = 0;
            string attacker = null;

            System.Collections.IDictionary dict = raw as System.Collections.IDictionary;
            if (dict != null)
            {
                if (dict.Contains("id")) id = (byte)Convert.ToInt32(dict["id"]);
                if (dict.Contains("dmg")) dmg = Convert.ToInt32(dict["dmg"]);
                if (dict.Contains("att")) attacker = Convert.ToString(dict["att"]);
            }
            else
            {
                int[] req = raw as int[];
                if (req == null && raw is Array)
                {
                    Array a = raw as Array;
                    if (a != null && a.Length >= 2)
                    {
                        req = new int[2];
                        req[0] = Convert.ToInt32(a.GetValue(0));
                        req[1] = Convert.ToInt32(a.GetValue(1));
                    }
                }
                if (req == null || req.Length < 2) return;
                id = (byte)Mathf.Clamp(req[0], 0, 250);
                dmg = Mathf.Max(1, req[1]);
            }

            if (dmg <= 0) return;
            ZombieModEntry.Log("ApplyZombieDamageRequest: id=" + id + " dmg=" + dmg +
                " attacker=" + (attacker ?? "null"));
            ApplyZombieDamage(id, dmg, attacker);
        }

        private void ApplyZombieDamage(byte id, int damage, string attackerId)
        {
            ZombieDriver drv;
            if (!_drivers.TryGetValue(id, out drv) || drv == null || drv.gameObject == null) return;
            float hp;
            if (!_zombieHealth.TryGetValue(id, out hp)) hp = ZombieBalance.HealthForRound(_round);
            hp -= Mathf.Max(1, damage);
            _zombieHealth[id] = hp;
            if (!string.IsNullOrEmpty(attackerId))
                _zombieLastAttacker[id] = attackerId;
            drv.ApplySharedHealth(hp);
            // Coalesce: schedule a prompt broadcast instead of a full re-send per hit.
            if (_broadcastTimer > 0.05f) _broadcastTimer = 0.05f;
            if (hp <= 0f)
                KillZombie(id, drv.gameObject, attackerId);
        }

        private void ApplyZombieHealthSync(float[] data)
        {
            if (data == null || data.Length < 2) return;
            try
            {
                for (int i = 0; i + 1 < data.Length; i += 2)
                {
                    byte id = (byte)Mathf.Clamp(Mathf.RoundToInt(data[i]), 0, 250);
                    float hp = data[i + 1];
                    _zombieHealth[id] = hp;

                    ZombieDriver drv;
                    if (_drivers.TryGetValue(id, out drv) && drv != null)
                        drv.ApplySharedHealth(hp);

                    ZombieProxy proxy;
                    if (_proxies.TryGetValue(id, out proxy) && proxy != null)
                        proxy.ApplySharedHealth(hp);

                    if (hp > 0f) continue;

                    if (_proxies.TryGetValue(id, out proxy) && proxy != null && proxy.gameObject != null)
                        UnityEngine.Object.Destroy(proxy.gameObject);
                    _proxies.Remove(id);
                    ZombieModEntry.Log("ApplyZombieHealthSync: removed dead proxy id=" + id);
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyZombieHealthSync err: " + ex.Message); }
        }

        private float[] NormalizeFloatArray(object value)
        {
            if (value == null) return null;
            float[] floats = value as float[];
            if (floats != null) return floats;
            Array array = value as Array;
            if (array == null) return null;
            float[] result = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                object item = array.GetValue(i);
                if (item == null)
                {
                    result[i] = 0f;
                    continue;
                }
                if (item is float) result[i] = (float)item;
                else if (item is int) result[i] = (int)item;
                else if (item is short) result[i] = (short)item;
                else if (item is byte) result[i] = (byte)item;
                else if (item is double) result[i] = (float)(double)item;
                else
                {
                    float parsed;
                    if (float.TryParse(item.ToString(), out parsed)) result[i] = parsed;
                }
            }
            return result;
        }

        public void ReportZombieDamage(byte zombieId, int damage)
        {
            if (damage <= 0) return;
            int scaledDamage = ScaleZombieWeaponDamage(damage);
            var ht = new System.Collections.Hashtable();
            ht["zh"] = new System.Collections.Hashtable {
                { "id", (int)zombieId },
                { "dmg", scaledDamage },
                { "att", GetLocalPeerId() ?? "" }
            };
            ZombieModEntry.Log("ReportZombieDamage: id=" + zombieId + " raw=" + damage + " scaled=" + scaledDamage +
                " peer=" + (GetLocalPeerId() ?? "null"));
            RaiseZombieEvent(ht, true);
        }

        public void ApplyLocalPlayerDamage(int damage)
        {
            if (damage <= 0) return;
            try
            {
                Type plType = FindType("PlayerLogic");
                if (plType == null) return;
                FieldInfo instField = plType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                MethodInfo dmgMethod = plType.GetMethod("PlayerDamage", BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[] { typeof(int) }, null);
                if (instField == null || dmgMethod == null) return;
                object plInst = instField.GetValue(null);
                if (plInst != null)
                    dmgMethod.Invoke(plInst, new object[] { damage });
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyLocalPlayerDamage err: " + ex.Message); }
        }

        private void ApplyZombiePlayerDamageEvent(object raw)
        {
            try
            {
                System.Collections.IDictionary dict = raw as System.Collections.IDictionary;
                if (dict == null) return;
                string peerId = dict.Contains("peer") ? Convert.ToString(dict["peer"]) : "";
                int damage = dict.Contains("dmg") ? Convert.ToInt32(dict["dmg"]) : 0;
                if (damage <= 0 || string.IsNullOrEmpty(peerId)) return;
                string localId = GetLocalPeerId();
                if (string.IsNullOrEmpty(localId) || !string.Equals(localId, peerId, StringComparison.Ordinal)) return;
                ZombieModEntry.Log("ZombieDamageEvent: applying local dmg=" + damage + " peer=" + peerId);
                ApplyLocalPlayerDamage(damage);
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyZombiePlayerDamageEvent err: " + ex.Message); }
        }

        public void ApplyPlayerDamageToPeer(string peerId, int damage)
        {
            if (damage <= 0 || string.IsNullOrEmpty(peerId))
                return;
            try
            {
                string localId = GetLocalPeerId();
                ZombieModEntry.Log("ApplyPlayerDamageToPeer: peer=" + peerId + " dmg=" + damage +
                    " local=" + (localId ?? "null"));
                if (!string.IsNullOrEmpty(localId) && string.Equals(localId, peerId, StringComparison.Ordinal))
                {
                    ApplyLocalPlayerDamage(damage);
                    return;
                }

                var ht = new System.Collections.Hashtable();
                ht["zpdmg"] = new System.Collections.Hashtable {
                    { "peer", peerId },
                    { "dmg", damage }
                };
                RaiseZombieEvent(ht, true);
                ZombieModEntry.Log("ZombieDamagePeerEvent: peer=" + peerId + " dmg=" + damage);
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyPlayerDamageToPeer err: " + ex.Message); }
        }

        private void ApplyPlayerDownRequest(object raw)
        {
            string peerId = raw as string;
            if (string.IsNullOrEmpty(peerId)) return;
            MarkZombieSessionActive();
            string localId = GetLocalPeerId();
            if (!string.IsNullOrEmpty(localId) && string.Equals(localId, peerId, StringComparison.Ordinal))
            {
                if (!_localPlayerDowned)
                {
                    _localPlayerDowned = true;
                    ZombieModEntry.Log("PlayerDownSync: local peer downed " + peerId);
                    HideLocalPlayerModel();
                    SuppressVanillaDiedPanel();
                }
            }
            if (_downedPlayers.Add(peerId))
            {
                ZombieModEntry.Log("PlayerDownSync: " + peerId + " downedCount=" + _downedPlayers.Count);
                HidePeerPlayerObject(peerId);
            }
            if (IsAllPlayersDown())
                TriggerGameOver();
        }

        private bool IsAllPlayersDown()
        {
            try
            {
                int roomSize = 0;
                try
                {
                    if (PhotonNetwork.playerList != null)
                        roomSize = PhotonNetwork.playerList.Length;
                }
                catch { }
                if (roomSize <= 0)
                {
                    try { roomSize = PhotonNetwork.room.playerCount; } catch { }
                }

                int downedCount = _downedPlayers.Count;
                string localId = GetLocalPeerId();
                if (_localPlayerDowned && !string.IsNullOrEmpty(localId) && !_downedPlayers.Contains(localId))
                    downedCount++;

                return roomSize > 0 && downedCount >= roomSize;
            }
            catch { return false; }
        }

        internal bool IsPeerDowned(string peerId)
        {
            if (string.IsNullOrEmpty(peerId)) return false;
            string localId = GetLocalPeerId();
            if (!string.IsNullOrEmpty(localId) && string.Equals(localId, peerId, StringComparison.Ordinal))
            {
                if (_localPlayerDowned) return true;
                return IsLocalPlayerActuallyDowned();
            }
            return _downedPlayers.Contains(peerId);
        }

        private bool IsLocalPlayerActuallyDowned()
        {
            try
            {
                if (_fPlMInstance == null || _fPlBlood == null || _fPlBDied == null)
                    return _localPlayerDowned;

                object plInst = _fPlMInstance.GetValue(null);
                if (plInst == null) return _localPlayerDowned;

                bool bDied = (bool)_fPlBDied.GetValue(plInst);
                int blood = Convert.ToInt32(_fPlBlood.GetValue(plInst));
                return bDied && blood <= 0;
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("IsLocalPlayerActuallyDowned err: " + ex.Message);
                return _localPlayerDowned;
            }
        }

        private void ApplyGameOverSync(object raw)
        {
            try
            {
                if (_phase == PHASE_GAMEOVER) return;
                if (!_modeStarted || _phase != PHASE_ACTIVE)
                {
                    ZombieModEntry.Log("ZombieMod: ignored stale gameover sync outside active round");
                    return;
                }
                _phase = PHASE_GAMEOVER;
                _phaseTimer = 0f;
                _localPlayerDowned = true;
                if (_fPlMInstance != null && _fPlBlood != null && _fPlBDied != null)
                {
                    object plInst = _fPlMInstance.GetValue(null);
                    if (plInst != null)
                    {
                        _fPlBlood.SetValue(plInst, 0);
                        _fPlBDied.SetValue(plInst, true);
                    }
                }
                SuppressVanillaDiedPanel();
                ShowModeMessage("GAME OVER", 999f);
                ZombieModEntry.Log("ZombieMod: applied gameover sync");
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyGameOverSync err: " + ex.Message); }
        }

        private void ApplyZombieKillSync(object raw)
        {
            try
            {
                int idInt = Convert.ToInt32(raw);
                byte id = (byte)Mathf.Clamp(idInt, 0, 250);
                ZombieModEntry.Log("ApplyZombieKillSync: id=" + id);
                ZombieProxy p;
                if (_proxies.TryGetValue(id, out p) && p != null && p.gameObject != null)
                    UnityEngine.Object.Destroy(p.gameObject);
                _proxies.Remove(id);
                ZombieModEntry.Log("ZombieMod: synced kill remove id=" + id);
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyZombieKillSync err: " + ex.Message); }
        }

        private void ApplyZombieVariantSync(float[] data)
        {
            if (data == null || data.Length < 2) return;
            try
            {
                for (int i = 0; i + 1 < data.Length; i += 2)
                {
                    byte id = (byte)Mathf.Clamp(Mathf.RoundToInt(data[i]), 0, 250);
                    int variant = Mathf.Clamp(Mathf.RoundToInt(data[i + 1]), 0, 4);
                    _zombieVariant[id] = variant;

                    // Only (re)apply the skin when it actually changed — re-applying
                    // every broadcast leaked a fresh Material set per zombie per 0.3s
                    // and was the main cause of client FPS collapse at high rounds.
                    ZombieProxy proxy;
                    if (_proxies.TryGetValue(id, out proxy) && proxy != null &&
                        proxy.gameObject != null && proxy.AppliedVariant != variant)
                    {
                        ApplyZombieSkin(proxy.gameObject, variant);
                        proxy.AppliedVariant = variant;
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyZombieVariantSync err: " + ex.Message); }
        }

        private void HideLocalPlayerModel()
        {
            try
            {
                GameObject ec = GameObject.Find("ExampleCharacter");
                if (ec == null) return;
                Renderer[] rs = ec.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rs.Length; i++)
                {
                    if (rs[i] != null) rs[i].enabled = false;
                }
                Collider[] cols = ec.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < cols.Length; i++)
                {
                    Collider c = cols[i];
                    if (c == null || !c.enabled) continue;
                    if (!_hiddenLocalColliders.Contains(c))
                        _hiddenLocalColliders.Add(c);
                    c.enabled = false;
                }
                _localModelHidden = true;
            }
            catch (Exception ex) { ZombieModEntry.Log("HideLocalPlayerModel err: " + ex.Message); }
        }

        private void RestoreLocalPlayerModel()
        {
            try
            {
                if (!_localModelHidden && _hiddenLocalColliders.Count == 0) return;
                GameObject ec = GameObject.Find("ExampleCharacter");
                if (ec == null) return;
                Renderer[] rs = ec.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rs.Length; i++)
                {
                    if (rs[i] != null) rs[i].enabled = true;
                }
                for (int i = _hiddenLocalColliders.Count - 1; i >= 0; i--)
                {
                    Collider c = _hiddenLocalColliders[i];
                    if (c != null) c.enabled = true;
                }
                _hiddenLocalColliders.Clear();
                _localModelHidden = false;
            }
            catch (Exception ex) { ZombieModEntry.Log("RestoreLocalPlayerModel err: " + ex.Message); }
        }

        private void HidePeerPlayerObject(string peerId)
        {
            try
            {
                GameObject go = FindRemotePlayerObject(peerId);
                if (go == null) return;

                List<Renderer> renderers;
                if (!_hiddenPeerRenderers.TryGetValue(peerId, out renderers))
                {
                    renderers = new List<Renderer>();
                    _hiddenPeerRenderers[peerId] = renderers;
                }
                Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rs.Length; i++)
                {
                    Renderer r = rs[i];
                    if (r == null || !r.enabled) continue;
                    if (!renderers.Contains(r)) renderers.Add(r);
                    r.enabled = false;
                }

                List<Collider> colliders;
                if (!_hiddenPeerColliders.TryGetValue(peerId, out colliders))
                {
                    colliders = new List<Collider>();
                    _hiddenPeerColliders[peerId] = colliders;
                }
                Collider[] cols = go.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < cols.Length; i++)
                {
                    Collider c = cols[i];
                    if (c == null || !c.enabled) continue;
                    if (!colliders.Contains(c)) colliders.Add(c);
                    c.enabled = false;
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("HidePeerPlayerObject err: " + ex.Message); }
        }

        private void RestoreAllPeerPlayerObjects()
        {
            try
            {
                foreach (var kv in _hiddenPeerRenderers)
                {
                    List<Renderer> list = kv.Value;
                    for (int i = 0; list != null && i < list.Count; i++)
                        if (list[i] != null) list[i].enabled = true;
                }
                foreach (var kv in _hiddenPeerColliders)
                {
                    List<Collider> list = kv.Value;
                    for (int i = 0; list != null && i < list.Count; i++)
                        if (list[i] != null) list[i].enabled = true;
                }
                _hiddenPeerRenderers.Clear();
                _hiddenPeerColliders.Clear();
            }
            catch (Exception ex) { ZombieModEntry.Log("RestoreAllPeerPlayerObjects err: " + ex.Message); }
        }

        private GameObject FindRemotePlayerObject(string peerId)
        {
            try
            {
                if (string.IsNullOrEmpty(peerId)) return null;
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType == null) return null;
                FieldInfo fiInst = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = fiInst != null ? fiInst.GetValue(null) : null;
                if (mgrInst == null) return null;

                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo objField = mgrType.GetField("otherPlayerObject", flags);
                FieldInfo infoField = mgrType.GetField("otherPlayersInfoList", flags);
                Array objects = objField != null ? objField.GetValue(mgrInst) as Array : null;
                Array infos = infoField != null ? infoField.GetValue(mgrInst) as Array : null;
                if (objects == null || infos == null) return null;

                int count = Math.Min(objects.Length, infos.Length);
                for (int i = 0; i < count; i++)
                {
                    object info = infos.GetValue(i);
                    if (info == null) continue;
                    FieldInfo idField = info.GetType().GetField("mId", flags);
                    string id = idField != null ? Convert.ToString(idField.GetValue(info)) : null;
                    if (!string.Equals(id, peerId, StringComparison.Ordinal)) continue;
                    return objects.GetValue(i) as GameObject;
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("FindRemotePlayerObject err: " + ex.Message); }
            return null;
        }

        private void ApplyKillCredit(object raw, object zombieRaw)
        {
            string peerId = raw as string;
            if (string.IsNullOrEmpty(peerId)) return;
            string localId = GetLocalPeerId();
            if (!string.Equals(peerId, localId, StringComparison.Ordinal)) return;
            try
            {
                int award = POINTS_PER_KILL + Mathf.Max(0, _round - 1) * POINTS_PER_ROUND_BONUS;
                _points += award;
                AddLocalZombieKillStat("KillCredit");
                byte zombieId;
                Vector3 dropPos;
                if (TryParseZombieId(zombieRaw, out zombieId) && TryFindZombieDropPosition(zombieId, out dropPos))
                    CNRMods.CNRPerkSystem.TrySpawnKillDrops(dropPos + Vector3.up * 0.25f);
                ZombieModEntry.Log("KillCredit: awarded to local peer " + peerId + " points=" + _points);
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyKillCredit err: " + ex.Message); }
        }

        private static bool TryParseZombieId(object raw, out byte id)
        {
            id = 0;
            if (raw == null) return false;
            try
            {
                int n = Convert.ToInt32(raw);
                if (n < 0 || n > 255) return false;
                id = (byte)n;
                return true;
            }
            catch { return false; }
        }

        private bool TryFindZombieDropPosition(byte id, out Vector3 pos)
        {
            pos = Vector3.zero;
            try
            {
                ZombieDriver drv;
                if (_drivers.TryGetValue(id, out drv) && drv != null)
                {
                    pos = drv.transform.position;
                    return true;
                }
                ZombieProxy proxy;
                if (_proxies.TryGetValue(id, out proxy) && proxy != null)
                {
                    pos = proxy.transform.position;
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void AddLocalZombieKillStat(string reason)
        {
            try
            {
                CachePlayerLogicFields();
                int next = -1;
                if (_fPlMInstance != null && _fPlKilledNum != null)
                {
                    object plInst = _fPlMInstance.GetValue(null);
                    if (plInst != null)
                    {
                        int cur = Convert.ToInt32(_fPlKilledNum.GetValue(plInst));
                        next = cur + 1;
                        _fPlKilledNum.SetValue(plInst, next);
                    }
                }

                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType != null)
                {
                    FieldInfo fiMgr = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    object mgrInst = fiMgr != null ? fiMgr.GetValue(null) : null;
                    FieldInfo fiPlayer = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                    object info = mgrInst != null && fiPlayer != null ? fiPlayer.GetValue(mgrInst) : null;
                    if (info != null)
                    {
                        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                        FieldInfo killField = info.GetType().GetField("mKillNum", flags);
                        if (killField != null)
                        {
                            int value = next >= 0 ? next : Convert.ToInt32(killField.GetValue(info)) + 1;
                            killField.SetValue(info, value);
                        }
                    }
                }
                ZombieModEntry.Log("ZombieKillStat: " + reason + " localKills=" + next);
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieKillStat err: " + ex.Message); }
        }

        private string GetDownedPeerListString()
        {
            try
            {
                string localId = GetLocalPeerId();
                if (_localPlayerDowned && !string.IsNullOrEmpty(localId))
                    _downedPlayers.Add(localId);
                if (_downedPlayers.Count == 0) return "";
                string[] ids = new string[_downedPlayers.Count];
                int i = 0;
                foreach (string id in _downedPlayers)
                    ids[i++] = id;
                return string.Join(",", ids);
            }
            catch { return ""; }
        }

        private void ApplyDownedPeerList(object raw)
        {
            try
            {
                string list = raw as string;
                if (list == null) return;

                HashSet<string> synced = new HashSet<string>();
                if (list.Length > 0)
                {
                    string[] ids = list.Split(',');
                    for (int i = 0; i < ids.Length; i++)
                    {
                        string id = ids[i];
                        if (string.IsNullOrEmpty(id)) continue;
                        synced.Add(id);
                        if (_downedPlayers.Add(id))
                            HidePeerPlayerObject(id);
                    }
                }

                string localId = GetLocalPeerId();
                if (!string.IsNullOrEmpty(localId) && synced.Contains(localId))
                {
                    _localPlayerDowned = true;
                    HideLocalPlayerModel();
                    SuppressVanillaDiedPanel();
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ApplyDownedPeerList err: " + ex.Message); }
        }

        private string DumpKeys(System.Collections.IDictionary dict)
        {
            if (dict == null) return "(null)";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (var de in dict)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append(GetDictKey(de)).Append("=").Append(DescribeValue(GetDictValue(de)));
            }
            return sb.ToString();
        }

        private string DumpValue(object value)
        {
            return DescribeValue(value);
        }

        private string DescribeValue(object value)
        {
            if (value == null) return "null";
            float[] f = value as float[];
            if (f != null) return "float[" + f.Length + "]";
            System.Collections.IDictionary ht = value as System.Collections.IDictionary;
            if (ht != null) return "dict[" + ht.Count + "]";
            string s = value as string;
            if (s != null) return "str[" + s.Length + "]";
            return value.GetType().Name;
        }

        private void RemoveMissingProxies(HashSet<byte> seen)
        {
            if (seen == null) return;
            List<byte> remove = null;
            foreach (var kv in _proxies)
            {
                if (!seen.Contains(kv.Key))
                {
                    if (remove == null) remove = new List<byte>();
                    remove.Add(kv.Key);
                }
            }
            if (remove == null) return;
            for (int i = 0; i < remove.Count; i++)
            {
                byte id = remove[i];
                ZombieProxy p;
                if (_proxies.TryGetValue(id, out p) && p != null && p.gameObject != null)
                    UnityEngine.Object.Destroy(p.gameObject);
                _proxies.Remove(id);
            }
        }

        private void ApplySyncedModeState(float[] state)
        {
            if (state == null || state.Length < 7) return;
            int syncedRound = Mathf.RoundToInt(state[0]);
            int syncedPhase = Mathf.RoundToInt(state[1]);
            bool newActiveRound = syncedPhase == PHASE_ACTIVE && syncedRound > _round;
            bool roundJustEnded = syncedPhase == PHASE_INTER && _phase == PHASE_ACTIVE && _modeStarted;
            _modeStarted = true;
            _round = syncedRound;
            _phase = syncedPhase;
            _phaseTimer = state[2];
            _zombiesTotal = Mathf.RoundToInt(state[3]);
            _zombiesKilled = Mathf.RoundToInt(state[4]);
            _zombiesRemaining = Mathf.RoundToInt(state[5]);
            _spawnQueue = Mathf.RoundToInt(state[6]);
            if (roundJustEnded)
            {
                // Master adds this bonus in EndRound(); clients mirror it here so
                // everyone's points stay in step across rounds.
                _points += END_ROUND_BONUS;
                ShowModeMessage("ROUND " + _round + " COMPLETE  +" + END_ROUND_BONUS, 4f);
            }
            if (newActiveRound)
            {
                _downedPlayers.Clear();
                RestoreAllPeerPlayerObjects();
                ReviveLocalPlayer();
                ShowModeMessage("ROUND " + _round, 3f);
            }
        }

        private ZombieProxy GetOrCreateProxy(byte id)
        {
            ZombieProxy p;
            if (_proxies.TryGetValue(id, out p) && p != null && p.gameObject != null)
                return p;

            int appliedVariant = -1;
            GameObject go = null;
            if (_templateGO != null)
            {
                try
                {
                    go = (GameObject)UnityEngine.Object.Instantiate(_templateGO);
                    go.SetActive(true);
                    go.name = "ZombieProxy_" + id;
                    PrepareZombieVisuals(go);
                    int variant;
                    if (TryGetZombieVariant(id, out variant))
                    {
                        ApplyZombieSkin(go, variant);
                        appliedVariant = variant;
                    }
                    // AI is already disabled on the template; nothing extra needed
                }
                catch { go = null; }
            }

            if (go == null)
            {
                // Fallback: plain capsule (no game model available yet)
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "ZombieProxy_" + id;
                try { go.renderer.material.color = new Color(0.1f, 0.75f, 0.1f, 1f); } catch { }
                Collider col = go.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.Destroy(col);
            }

            p = go.AddComponent<ZombieProxy>();
            p.ZombieId = id;
            p.MaxHealth = ZombieBalance.HealthForRound(_round);
            p.Hook = this;
            p.AppliedVariant = appliedVariant;
            _proxies[id] = p;
            return p;
        }

        private void ClearAll()
        {
            RestoreLocalPlayerModel();
            RestoreAllPeerPlayerObjects();
            if (_weaponUpgradeLocked)
            {
                if (_weaponUpgradeSlot == "p0") _primarySlots[0] = _weaponUpgradeName;
                else if (_weaponUpgradeSlot == "p1") _primarySlots[1] = _weaponUpgradeName;
                _weaponUpgradeLocked = false;
                _weaponUpgradeName = "";
                _weaponUpgradeSlot = "";
            }
            RestoreVanillaInventory();
            _nearUpgradeObject = null;
            foreach (var kv in _drivers)
                if (kv.Value != null && kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            _drivers.Clear();
            _zombieHealth.Clear();
            _zombieVariant.Clear();
            _zombieLastAttacker.Clear();
            _downedPlayers.Clear();

            foreach (var kv in _proxies)
                if (kv.Value != null && kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            _proxies.Clear();

            ClearNavDebug();

            _masterSpawned = false;
            _modeStarted   = false;
            _zombieSessionActive = false;
            _astarBuilt    = false;
            _localPlayerDowned = false;
            _loggedWaitingForHostStart = false;
            _round = 0;
            _points = STARTING_POINTS;
            _phase = PHASE_WAITING;
            _phaseTimer = 0f;
            _spawnTimer = 0f;
            _broadcastTimer = 0f;
            _nextZombieId = 1;
            _spawnQueue = 0;
            _zombiesTotal = 0;
            _zombiesKilled = 0;
            _zombiesRemaining = 0;
            _killFeed.Clear();
            _killFeedTimer = 0f;
            _modeMessage = "";
            _modeMessageTimer = 0f;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Photon helpers (same pattern as CNRMod)
        // ─────────────────────────────────────────────────────────────────────
        private bool IsInRoom()
        {
            // This PUN build has no PhotonNetwork.inRoom — check room != null instead.
            try { Type t = GetPNType(); if (t == null) return false;
                  PropertyInfo pi = t.GetProperty("room", BindingFlags.Public | BindingFlags.Static);
                  return pi != null && pi.GetValue(null, null) != null; }
            catch { return false; }
        }
        private bool IsMasterClient()
        {
            try { Type t = GetPNType(); if (t == null) return false;
                  PropertyInfo pi = t.GetProperty("isMasterClient", BindingFlags.Public | BindingFlags.Static);
                  return pi != null && (bool)pi.GetValue(null, null); }
            catch { return false; }
        }
        private string GetAppServerId()
        {
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType == null) return null;
                FieldInfo instField = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = instField != null ? instField.GetValue(null) : null;
                if (mgrInst == null) return null;
                RefreshCnrServerId(mgrInst);
                FieldInfo serverField = mgrType.GetField("serverId", BindingFlags.Public | BindingFlags.Instance);
                if (serverField == null) return null;
                return serverField.GetValue(mgrInst) as string;
            }
            catch { return null; }
        }
        internal GameObject GetLocalPlayerGO()
        {
            if (_localPlayerGO == null)
            {
                if (_localPlayerFindTimer > 0f) return null;
                _localPlayerGO = GameObject.Find("ExampleCharacter");
                _localPlayerFindTimer = 0.5f;  // retry at most twice a second
            }
            return _localPlayerGO;
        }

        internal string GetLocalPeerId()
        {
            if (_cachedPeerId != null && _peerIdCacheTimer > 0f) return _cachedPeerId;
            try
            {
                Type mgrType = FindType("CNRMultiplayerManager");
                if (mgrType == null) return _cachedPeerId;
                FieldInfo instField = mgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object mgrInst = instField != null ? instField.GetValue(null) : null;
                if (mgrInst == null) return _cachedPeerId;
                FieldInfo playerField = mgrType.GetField("myPlayerInfo", BindingFlags.Public | BindingFlags.Instance);
                object playerInfo = playerField != null ? playerField.GetValue(mgrInst) : null;
                if (playerInfo == null) return _cachedPeerId;
                Type playerInfoType = playerInfo.GetType();
                FieldInfo idField = playerInfoType.GetField("mId", BindingFlags.Public | BindingFlags.Instance);
                if (idField == null) return _cachedPeerId;
                _cachedPeerId = idField.GetValue(playerInfo) as string;
                _peerIdCacheTimer = 1f;
                return _cachedPeerId;
            }
            catch { return _cachedPeerId; }
        }

        internal bool TryGetZombieVariant(byte id, out int variant)
        {
            if (!_zombieVariant.TryGetValue(id, out variant))
            {
                variant = 0;
                return false;
            }
            return true;
        }
        private bool IsAppServerAuthority()
        {
            // Match CNRMod CTF and vanilla Stronghold/KillingCompetition:
            // the serverId player is the authoritative simulation owner.
            string serverPeerId = GetAppServerId();
            string localId = GetLocalPeerId();
            if (IsValidPeerId(serverPeerId) && IsValidPeerId(localId))
                return string.Equals(serverPeerId, localId, StringComparison.Ordinal);

            return false;
        }
        private bool IsValidPeerId(string id)
        {
            return !string.IsNullOrEmpty(id) && !string.Equals(id, "null", StringComparison.OrdinalIgnoreCase);
        }
        private void RefreshCnrServerId(object mgrInst)
        {
            try
            {
                if (mgrInst == null) return;
                int frame = Time.frameCount;
                if (_authorityRefreshFrame == frame) return;
                _authorityRefreshFrame = frame;
                MethodInfo mi = mgrInst.GetType().GetMethod("UpdateServerIdOnline",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null) mi.Invoke(mgrInst, null);
            }
            catch { }
        }
        private string GetRoomDebug()
        {
            try
            {
                Type t = GetPNType();
                if (t == null) return "pn=null";
                PropertyInfo roomPi = t.GetProperty("room", BindingFlags.Public | BindingFlags.Static);
                object room = roomPi != null ? roomPi.GetValue(null, null) : null;
                if (room == null) return "room=null";
                PropertyInfo namePi = room.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo countPi = room.GetType().GetProperty("playerCount", BindingFlags.Public | BindingFlags.Instance);
                string name = namePi != null ? Convert.ToString(namePi.GetValue(room, null)) : "?";
                string count = countPi != null ? Convert.ToString(countPi.GetValue(room, null)) : "?";
                return "room=" + name + " players=" + count;
            }
            catch (Exception ex)
            {
                return "roomErr=" + ex.Message;
            }
        }
        private string GetAuthorityDebug()
        {
            return GetRoomDebug() + " photonMaster=" + IsMasterClient() +
                " serverId=" + (GetAppServerId() ?? "null") +
                " localId=" + (GetLocalPeerId() ?? "null") +
                " authority=" + IsAppServerAuthority();
        }
        private bool _authCached;
        private int  _authCacheFrame = -1;
        public bool IsMasterClientNow()
        {
            // Reflection-heavy; cache per frame (called from Update + every Photon event).
            if (_authCacheFrame == Time.frameCount) return _authCached;
            _authCacheFrame = Time.frameCount;
            _authCached = IsAppServerAuthority();
            return _authCached;
        }
        private object GetNetworkingPeer()
        {
            try { Type t = GetPNType(); if (t == null) return null;
                  FieldInfo fi = t.GetField("networkingPeer",
                      BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                  return fi != null ? fi.GetValue(null) : null; }
            catch { return null; }
        }
        private Type _pnTypeCache;
        private Type GetPNType()
        {
            if (_pnTypeCache != null) return _pnTypeCache;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            { Type t = asm.GetType("PhotonNetwork"); if (t != null) { _pnTypeCache = t; return t; } }
            return null;
        }

        private void TryInstallProxy()
        {
            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                FieldInfo lf = peer.GetType().GetField("externalListener",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (lf == null) return;
                IPhotonPeerListener cur = lf.GetValue(peer) as IPhotonPeerListener;
                if (cur == null) return;
                if (cur is ZombiePhotonProxy) { _proxyInstalled = true; return; }
                lf.SetValue(peer, new ZombiePhotonProxy(cur, this));
                _proxyInstalled = true;
                ZombieModEntry.Log("Proxy installed, wrapping " + cur.GetType().Name);
            }
            catch (Exception ex) { ZombieModEntry.Log("TryInstallProxy err: " + ex.Message); }
        }

        // Probe which physics layers have colliders near the player so we can
        // configure the A* obstacle mask correctly.
        private static void LogSceneLayers(Vector3 origin)
        {
            try
            {
                Collider[] hits = Physics.OverlapSphere(origin, 30f, -1);
                var layers = new System.Collections.Generic.Dictionary<int, int>();
                foreach (Collider c in hits)
                {
                    int l = c.gameObject.layer;
                    if (!layers.ContainsKey(l)) layers[l] = 0;
                    layers[l]++;
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder("LayerScan:");
                foreach (var kv in layers)
                    sb.Append(" L").Append(kv.Key).Append("(").Append(kv.Value).Append(")");
                ZombieModEntry.Log(sb.ToString());
            }
            catch (Exception ex) { ZombieModEntry.Log("LayerScan err: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────        // Runtime A* graph builder.
        // Creates an AstarPath singleton and adds a GridGraph centered on the
        // player spawn point.  GridGraph scans using downward Physics raycasts
        // against ALL layers, so every flat floor/rooftop/platform collider
        // automatically becomes a walkable node.
        // ─────────────────────────────────────────────────────────────────────────
        private void BuildAstarGraph(Vector3 center)
        {
            try
            {
                Type astarType = FindType("AstarPath");
                // GridGraph may live in the Pathfinding namespace depending on A*PP version
                Type gridType  = FindType("GridGraph") ?? FindType("Pathfinding.GridGraph");
                if (astarType == null) { ZombieModEntry.Log("BuildAstar: AstarPath type not found"); BuildBlockFaceOverlay(center); return; }
                if (gridType  == null) { ZombieModEntry.Log("BuildAstar: GridGraph type not found");  BuildBlockFaceOverlay(center); return; }

                // If AstarPath.active already exists (e.g. Kill Mode scene), leave it.
                FieldInfo activeF = astarType.GetField("active", BindingFlags.Public | BindingFlags.Static);
                if (activeF != null && activeF.GetValue(null) != null)
                {
                    ZombieModEntry.Log("BuildAstar: AstarPath.active already exists, skipping");
                    BuildBlockFaceOverlay(center);
                    return;
                }

                // Find the actual floor Y by looking for the flattest world-space BoxCollider
                // (same logic as the nav debug: zero-thickness box = the floor quad).
                // We use this only for logging; the graph center stays at player Y so the
                // obstacle capsule sits above the floor, not inside it.
                float floorY = center.y;
                Collider[] nearby = Physics.OverlapSphere(center, 200f, -1);
                float bestThickness = float.MaxValue;
                foreach (Collider c in nearby)
                {
                    if (!IsWorldBlockCollider(c)) continue;
                    BoxCollider bc = c as BoxCollider;
                    if (bc == null) continue;
                    float wy = bc.bounds.size.y;
                    if (wy < bestThickness)
                    {
                        bestThickness = wy;
                        floorY = bc.bounds.max.y;
                    }
                }
                ZombieModEntry.Log("BuildAstar: detected floorY=" + floorY.ToString("F2") + " playerY=" + center.y.ToString("F2"));
                // Graph center uses player Y so the A* nodes are at walking height,
                // not at the floor surface (which would put the obstacle capsule inside the floor).
                Vector3 graphCenter = center;

                // Create the singleton.  AstarPath.Awake() sets AstarPath.active = this.
                GameObject astarGO = new GameObject("ZombieAstar");
                UnityEngine.Object.DontDestroyOnLoad(astarGO);
                Component astar = astarGO.AddComponent(astarType);
                ZombieModEntry.Log("BuildAstar: AstarPath component created");

                // Get astarData (holds the graphs array)
                FieldInfo dataF = astarType.GetField("astarData",
                    BindingFlags.Public | BindingFlags.Instance);
                if (dataF == null) dataF = astarType.GetField("data",
                    BindingFlags.Public | BindingFlags.Instance);
                object data = dataF != null ? dataF.GetValue(astar) : null;
                if (data == null) { ZombieModEntry.Log("BuildAstar: astarData field not found"); BuildBlockFaceOverlay(center); return; }

                // Add a GridGraph
                MethodInfo addGraph = data.GetType().GetMethod("AddGraph",
                    new[] { typeof(Type) });
                if (addGraph == null) { ZombieModEntry.Log("BuildAstar: AddGraph method not found"); BuildBlockFaceOverlay(center); return; }
                object grid = addGraph.Invoke(data, new object[] { gridType });
                if (grid == null) { ZombieModEntry.Log("BuildAstar: AddGraph returned null"); BuildBlockFaceOverlay(center); return; }

                // Configure: 240×240 nodes at 0.5 m/node = 120 m square coverage.
                SetGridField(grid, gridType, "center", graphCenter);
                if (!TrySetGridDimensions(grid, gridType, 240, 240, 0.5f))
                {
                    SetGridField(grid, gridType, "width",    240);
                    SetGridField(grid, gridType, "depth",    240);
                    SetGridField(grid, gridType, "nodeSize", 0.5f);
                }

                // BUG FIX: GridGraph.collision is null before Scan() — GetValue returns null,
                // so all subsequent SetMember calls are silently skipped.  Scan() then creates
                // a fresh GraphCollision with defaults (mask=0 → no layers → all nodes walkable).
                // Fix: create and populate the GraphCollision ourselves, then assign it to the
                // grid BEFORE Scan() is called, so Scan() uses our configured instance.
                Type collType = FindType("GraphCollision") ?? FindType("Pathfinding.GraphCollision");
                if (collType != null)
                {
                    FieldInfo collF = gridType.GetField("collision", BindingFlags.Public | BindingFlags.Instance);
                    if (collF != null)
                    {
                        object coll = Activator.CreateInstance(collType);
                        UnityEngine.LayerMask obstacleMask = (UnityEngine.LayerMask)(1 << 0);
                        // heightCheck=true: ray from above snaps nodes to ground surface.
                        // unwalkableWhenNoGround=true: nodes over voids (stairs edges, platform edges) → blocked.
                        // collisionCheck=true: capsule overlap check marks nodes inside/near walls → blocked.
                        // mask/heightMask = Layer 0 only (all scene geo is on Layer 0).
                        SetMember(coll, collType, "heightCheck",           true);
                        SetMember(coll, collType, "heightMask",            obstacleMask);
                        SetMember(coll, collType, "fromHeight",            12f);
                        SetMember(coll, collType, "unwalkableWhenNoGround",true);
                        SetMember(coll, collType, "collisionCheck",        true);
                        SetMember(coll, collType, "mask",                  obstacleMask);
                        // Capsule: worldRadius = diameter * nodeSize * 0.5 = 1.6*0.5*0.5 = 0.4m
                        // collisionOffset lifts capsule base so it doesn't clip the floor.
                        // With nodes snapped to floorY≈0.62, capsule base = 0.62+0.45-0.4 = 0.67 > 0.62 ✓
                        SetMember(coll, collType, "diameter",              1.6f);
                        SetMember(coll, collType, "collisionOffset",       0.45f);
                        SetMember(coll, collType, "thickRaycast",          false);
                        collF.SetValue(grid, coll);
                        ZombieModEntry.Log("BuildAstar: GraphCollision pre-created and assigned");
                    }
                    else ZombieModEntry.Log("BuildAstar: 'collision' field not found on GridGraph");
                }
                else ZombieModEntry.Log("BuildAstar: GraphCollision type not found");

                SetMember(grid, gridType, "maxSlope", 45f);
                SetMember(grid, gridType, "maxClimb", NAV_PATH_MAX_CLIMB);
                SetMember(grid, gridType, "maxClimbAxis", 1);
                SetMember(grid, gridType, "cutCorners", false);
                // Erosion can erase narrow stairs. Clearance is handled by the collision capsule.
                SetMember(grid, gridType, "erodeIterations", 0);

                // Scan — synchronous, blocks briefly but bakes the full navmesh
                MethodInfo scan = astarType.GetMethod("Scan",
                    BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
                if (scan != null)
                {
                    scan.Invoke(astar, null);
                    ZombieModEntry.Log("BuildAstar: scan complete, center=" + center);
                    BuildBlockFaceOverlay(center);
                }
                else
                {
                    ZombieModEntry.Log("BuildAstar: Scan() method not found");
                    BuildBlockFaceOverlay(center);
                }
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("BuildAstar err: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void BuildNavDebugOverlay(object grid, Vector3 center)
        {
            if (!_navDebugEnabled) return;
            ClearNavDebug();

            if (grid == null)
            {
                ZombieModEntry.Log("NavDebug: no GridGraph found, using block face overlay");
                BuildBlockFaceOverlay(center);
                return;
            }

            try
            {
                Type gridType = grid.GetType();
                int width = ToInt(GetMember(grid, gridType, "width"), 160);
                int depth = ToInt(GetMember(grid, gridType, "depth"), 160);
                Array nodes = GetMember(grid, gridType, "nodes") as Array;
                if (nodes == null)
                {
                    ZombieModEntry.Log("NavDebug: GridGraph.nodes not found, using block face overlay");
                    BuildBlockFaceOverlay(center);
                    return;
                }

                _navDebugRoot = new GameObject("ZombieNavDebugOverlay");
                UnityEngine.Object.DontDestroyOnLoad(_navDebugRoot);
                Material walkMat = CreateNavDebugMaterial("ZombieNavDebug_Walkable", new Color(0.05f, 1f, 0.2f, 0.35f));
                Material blockMat = CreateNavDebugMaterial("ZombieNavDebug_Blocked", new Color(1f, 0.08f, 0.04f, 0.55f));

                int made = 0;
                int walkableCount = 0;
                int blockedCount = 0;
                for (int z = 0; z < depth && made < NAV_DEBUG_MAX_TILES; z += NAV_DEBUG_NODE_STEP)
                {
                    for (int x = 0; x < width && made < NAV_DEBUG_MAX_TILES; x += NAV_DEBUG_NODE_STEP)
                    {
                        int idx = z * width + x;
                        if (idx < 0 || idx >= nodes.Length) continue;
                        object node = nodes.GetValue(idx);
                        if (node == null) continue;

                        Vector3 pos;
                        if (!TryGetNodePosition(node, out pos)) continue;
                        bool walkable = GetNodeWalkable(node);
                        CreateNavDebugTile(pos, walkable ? walkMat : blockMat, walkable);
                        if (walkable) walkableCount++; else blockedCount++;
                        made++;
                    }
                }

                ZombieModEntry.Log("NavDebug: drew " + made + " sampled GridGraph nodes walkable=" +
                    walkableCount + " blocked=" + blockedCount + " step=" + NAV_DEBUG_NODE_STEP);
                _navDebugStatus = "navdbg=grid:" + made + " W" + walkableCount + " B" + blockedCount;

                if (made < 50 || walkableCount == 0)
                {
                    ZombieModEntry.Log("NavDebug: GridGraph overlay too small/blocked, switching to block face overlay");
                    BuildBlockFaceOverlay(center);
                }
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("NavDebug err: " + ex.Message + " -- using block face overlay");
                BuildBlockFaceOverlay(center);
            }
        }

        private void BuildBlockFaceOverlay(Vector3 center)
        {
            if (!_navDebugEnabled) return;
            BuildCollisionFaceOverlay(center);
        }

        private void BuildCollisionFaceOverlay(Vector3 center)
        {
            try
            {
                ClearNavDebug();
                _navDebugRoot = new GameObject("ZombieNavDebugOverlay_CollisionFaces");
                UnityEngine.Object.DontDestroyOnLoad(_navDebugRoot);
                Material walkMat = CreateNavDebugMaterial("ZombieNavCollision_Walkable", new Color(0.0f, 0.85f, 0.18f, 1f));
                Material blockMat = CreateNavDebugMaterial("ZombieNavCollision_Blocked", new Color(1f, 0.05f, 0.0f, 1f));

                Collider[] cols = Physics.OverlapSphere(center, NAV_DEBUG_BLOCK_RADIUS + 40f, -1);
                int scanned = 0;
                int skipped = 0;
                int green = 0;
                int red = 0;

                System.Text.StringBuilder colDump = new System.Text.StringBuilder();
                for (int i = 0; i < cols.Length; i++)
                {
                    Collider col = cols[i];
                    string cname = col != null && col.gameObject != null ? col.gameObject.name : "null";
                    string ctype = col != null ? col.GetType().Name : "null";
                    if (!IsWorldBlockCollider(col))
                    {
                        colDump.Append("[SKIP:" + cname + "(" + ctype + ")] ");
                        skipped++; continue;
                    }
                    Bounds b = col.bounds;
                    if (b.min.y > center.y + 35f || b.max.y < center.y - 35f)
                    {
                        colDump.Append("[YRANGE:" + cname + " bminY=" + b.min.y.ToString("F1") + " bmaxY=" + b.max.y.ToString("F1") + "] ");
                        skipped++; continue;
                    }
                    colDump.Append("[OK:" + cname + "(" + ctype + ") sizeY=" + b.size.y.ToString("F1") + "] ");
                    scanned++;

                    MeshCollider mc = col as MeshCollider;
                    if (mc != null && mc.sharedMesh != null)
                    {
                        AddMeshColliderMarkers(mc, center, walkMat, blockMat, ref green, ref red);
                        continue;
                    }

                    BoxCollider bc = col as BoxCollider;
                    if (bc != null)
                    {
                        AddBoxColliderMarkers(bc, walkMat, blockMat, ref green, ref red);
                        continue;
                    }

                    // Unknown collider types can still be the main flat floor.
                    // Draw only a thin top marker for flat bounds; never draw side
                    // walls from bounds because cutout/compound colliders lie there.
                    if (b.size.y <= NAV_DEBUG_MAX_STEP)
                        AddBoundsTopOnlyMarker(b, walkMat, ref green);
                    else
                        skipped++;
                }
                ZombieModEntry.Log("NavDebug colliders: " + colDump.ToString());

                ZombieModEntry.Log("NavDebug: collision faces colliders=" + scanned +
                    " green=" + green + " red=" + red + " skipped=" + skipped);
                _navDebugStatus = "navdbg=coll G" + green + " R" + red;
            }
            catch (Exception ex) { ZombieModEntry.Log("NavDebug collision err: " + ex.Message); }
        }

        private void AddMeshColliderMarkers(MeshCollider mc, Vector3 center, Material walkMat, Material blockMat, ref int green, ref int red)
        {
            Mesh mesh = mc.sharedMesh;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            HashSet<string> seenPanels = new HashSet<string>();

            for (int t = 0; t + 2 < tris.Length; t += 3)
            {
                Vector3 a = mc.transform.TransformPoint(verts[tris[t]]);
                Vector3 b = mc.transform.TransformPoint(verts[tris[t + 1]]);
                Vector3 c = mc.transform.TransformPoint(verts[tris[t + 2]]);
                Vector3 triCenter = (a + b + c) / 3f;
                if (Mathf.Abs(triCenter.y - center.y) > 35f) continue;

                Vector3 n = Vector3.Cross(b - a, c - a);
                if (n.sqrMagnitude < 0.0001f) continue;
                n.Normalize();

                // Pure-horizontal faces (floor) skip the exposure check — their center may be
                // occluded by an object sitting on top (e.g. a raised platform over a large floor quad).
                // Use Abs() to also catch meshes with flipped winding order (normal pointing down).
                bool pureFloor = Mathf.Abs(n.y) >= 0.99f;
                if (Mathf.Abs(n.y) >= NAV_DEBUG_WALKABLE_NORMAL_Y && (pureFloor || IsExposedWalkableCollision(mc, triCenter)))
                {
                    if (green < NAV_DEBUG_MAX_COLLISION_TRIS &&
                        CreateCollisionFacePanel(a, b, c, n, walkMat, true, seenPanels))
                        green++;
                }
                else if (Mathf.Abs(n.y) < NAV_DEBUG_WALKABLE_NORMAL_Y)
                {
                    if (red < NAV_DEBUG_MAX_COLLISION_TRIS &&
                        CreateCollisionFacePanel(a, b, c, n, blockMat, false, seenPanels))
                        red++;
                }
            }
        }

        private bool IsExposedWalkableCollision(Collider expected, Vector3 triCenter)
        {
            RaycastHit hit;
            if (!Physics.Raycast(triCenter + Vector3.up * 0.4f, Vector3.down, out hit, 0.9f, -1))
                return false;
            if (hit.collider == null || !IsWorldBlockCollider(hit.collider))
                return false;
            if (Mathf.Abs(hit.point.y - triCenter.y) > 0.25f)
                return false;
            if (hit.collider == expected)
                return true;
            Transform a = hit.collider.transform;
            Transform b = expected != null ? expected.transform : null;
            return a != null && b != null && (a.IsChildOf(b) || b.IsChildOf(a));
        }

        private bool CreateCollisionFacePanel(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Material mat, bool walkable, HashSet<string> seenPanels)
        {
            float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            float maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            float minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            float maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
            float minZ = Mathf.Min(a.z, Mathf.Min(b.z, c.z));
            float maxZ = Mathf.Max(a.z, Mathf.Max(b.z, c.z));

            Vector3 pos;
            Vector3 scale;
            string axis;
            float t = 0.035f;

            if (walkable)
            {
                pos = new Vector3((minX + maxX) * 0.5f, (a.y + b.y + c.y) / 3f + t, (minZ + maxZ) * 0.5f);
                scale = new Vector3(Mathf.Max(0.08f, maxX - minX), t, Mathf.Max(0.08f, maxZ - minZ));
                axis = "Y";
            }
            else if (Mathf.Abs(normal.x) >= Mathf.Abs(normal.z))
            {
                float sign = normal.x >= 0f ? 1f : -1f;
                pos = new Vector3((a.x + b.x + c.x) / 3f + sign * t, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
                scale = new Vector3(t, Mathf.Max(0.08f, maxY - minY), Mathf.Max(0.08f, maxZ - minZ));
                axis = "X";
            }
            else
            {
                float sign = normal.z >= 0f ? 1f : -1f;
                pos = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (a.z + b.z + c.z) / 3f + sign * t);
                scale = new Vector3(Mathf.Max(0.08f, maxX - minX), Mathf.Max(0.08f, maxY - minY), t);
                axis = "Z";
            }

            string key = axis + ":" +
                Mathf.Round(pos.x * 10f) + ":" + Mathf.Round(pos.y * 10f) + ":" + Mathf.Round(pos.z * 10f) + ":" +
                Mathf.Round(scale.x * 10f) + ":" + Mathf.Round(scale.y * 10f) + ":" + Mathf.Round(scale.z * 10f);
            if (seenPanels.Contains(key)) return false;
            seenPanels.Add(key);

            CreateNavDebugBox(walkable ? "NavCollisionWalkPanel" : "NavCollisionBlockPanel", pos, scale, mat);
            return true;
        }

        private void AddBoxColliderMarkers(BoxCollider bc, Material walkMat, Material blockMat, ref int green, ref int red)
        {
            Transform tr = bc.transform;
            Vector3 c = bc.center;
            Vector3 h = bc.size * 0.5f;
            Bounds wb = bc.bounds;
            if (wb.size.y < 0.05f)
            {
                // Zero world-space thickness (transform Y scale ≈ 0): local offsets are useless.
                // Fall back to world-space bounds so the marker appears above the floor.
                AddBoundsTopOnlyMarker(wb, walkMat, ref green);
            }
            else
            {
                AddColliderFaceMarker(tr, c + new Vector3(0f, h.y, 0f), new Vector3(bc.size.x, 0.04f, bc.size.z), tr.up, walkMat, true, ref green);
                AddColliderFaceMarker(tr, c + new Vector3(-h.x, 0f, 0f), new Vector3(0.035f, bc.size.y, bc.size.z), -tr.right, blockMat, false, ref red);
                AddColliderFaceMarker(tr, c + new Vector3(h.x, 0f, 0f), new Vector3(0.035f, bc.size.y, bc.size.z), tr.right, blockMat, false, ref red);
                AddColliderFaceMarker(tr, c + new Vector3(0f, 0f, -h.z), new Vector3(bc.size.x, bc.size.y, 0.035f), -tr.forward, blockMat, false, ref red);
                AddColliderFaceMarker(tr, c + new Vector3(0f, 0f, h.z), new Vector3(bc.size.x, bc.size.y, 0.035f), tr.forward, blockMat, false, ref red);
            }
        }

        private void AddColliderFaceMarker(Transform tr, Vector3 localCenter, Vector3 localScale, Vector3 normal, Material mat, bool walkable, ref int count)
        {
            if (walkable && Mathf.Abs(normal.y) < NAV_DEBUG_WALKABLE_NORMAL_Y) return;
            if (!walkable && Mathf.Abs(normal.y) >= NAV_DEBUG_WALKABLE_NORMAL_Y) return;
            if (count >= NAV_DEBUG_MAX_TEXTURED_MARKERS) { count++; return; }
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = walkable ? "NavCollisionWalkFace" : "NavCollisionBlockFace";
            go.transform.parent = _navDebugRoot.transform;
            go.transform.position = tr.TransformPoint(localCenter) + normal.normalized * 0.025f;
            go.transform.rotation = tr.rotation;
            Vector3 s = Vector3.Scale(localScale, AbsVector(tr.lossyScale));
            go.transform.localScale = new Vector3(Mathf.Max(0.05f, s.x), Mathf.Max(0.05f, s.y), Mathf.Max(0.05f, s.z));
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
            Collider col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
            count++;
        }

        private void AddBoundsColliderMarkers(Bounds b, Material walkMat, Material blockMat, ref int green, ref int red)
        {
            if (green < NAV_DEBUG_MAX_TEXTURED_MARKERS)
                CreateNavDebugBox("NavCollisionBoundsTop", new Vector3(b.center.x, b.max.y + 0.08f, b.center.z),
                    new Vector3(Mathf.Max(0.1f, b.size.x), 0.10f, Mathf.Max(0.1f, b.size.z)), walkMat);
            green++;
            if (b.size.y > NAV_DEBUG_MAX_STEP && red < NAV_DEBUG_MAX_TEXTURED_MARKERS)
                red += CreateNavDebugSideFaces(b, blockMat, NAV_DEBUG_MAX_TEXTURED_MARKERS - red);
        }

        private void AddBoundsTopOnlyMarker(Bounds b, Material walkMat, ref int green)
        {
            if (green < NAV_DEBUG_MAX_TEXTURED_MARKERS)
                CreateNavDebugBox("NavCollisionBoundsTopOnly", new Vector3(b.center.x, b.max.y + 0.04f, b.center.z),
                    new Vector3(Mathf.Max(0.1f, b.size.x), 0.035f, Mathf.Max(0.1f, b.size.z)), walkMat);
            green++;
        }

        private static Vector3 AbsVector(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private void BuildTexturedFaceOverlay(Vector3 center)
        {
            try
            {
                ClearNavDebug();
                _navDebugRoot = new GameObject("ZombieNavDebugOverlay_TexturedFaces");
                UnityEngine.Object.DontDestroyOnLoad(_navDebugRoot);
                Material walkMat = CreateNavDebugMaterial("ZombieNavTextured_Walkable", new Color(0.0f, 0.85f, 0.18f, 1f));
                Material blockMat = CreateNavDebugMaterial("ZombieNavTextured_Blocked", new Color(1f, 0.05f, 0.0f, 1f));

                MeshFilter[] objs = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
                int scanned = 0;
                int skipped = 0;
                int green = 0;
                int red = 0;
                int markers = 0;

                for (int i = 0; i < objs.Length; i++)
                {
                    MeshFilter mf = objs[i] as MeshFilter;
                    if (mf == null || mf.sharedMesh == null || mf.gameObject == null) { skipped++; continue; }
                    if (!IsWorldMeshObject(mf.gameObject)) { skipped++; continue; }

                    Renderer rr = mf.gameObject.GetComponent<Renderer>();
                    if (rr == null || !RendererHasTexturedMaterial(rr)) { skipped++; continue; }
                    Bounds rb = rr.bounds;
                    if (Vector3.Distance(new Vector3(rb.center.x, center.y, rb.center.z), center) > NAV_DEBUG_BLOCK_RADIUS + 120f)
                    { skipped++; continue; }
                    if (rb.min.y > center.y + 50f || rb.max.y < center.y - 50f)
                    { skipped++; continue; }

                    Mesh mesh = mf.sharedMesh;
                    Vector3[] verts = mesh.vertices;
                    Material[] mats = rr.sharedMaterials;
                    int subCount = Math.Max(1, mesh.subMeshCount);
                    scanned++;

                    for (int sm = 0; sm < subCount; sm++)
                    {
                        Material smat = (mats != null && sm < mats.Length) ? mats[sm] : null;
                        if (!IsTexturedMaterial(smat)) continue;

                        int[] tris;
                        try { tris = mesh.GetTriangles(sm); }
                        catch { tris = mesh.triangles; }

                        for (int t = 0; t + 2 < tris.Length; t += 3)
                        {
                            Vector3 a = mf.transform.TransformPoint(verts[tris[t]]);
                            Vector3 b = mf.transform.TransformPoint(verts[tris[t + 1]]);
                            Vector3 c = mf.transform.TransformPoint(verts[tris[t + 2]]);
                            Vector3 triCenter = (a + b + c) / 3f;
                            if (Mathf.Abs(triCenter.y - center.y) > 20f) continue;

                            Vector3 nrm = Vector3.Cross(b - a, c - a);
                            if (nrm.sqrMagnitude < 0.0001f) continue;
                            nrm.Normalize();
                            float up = Mathf.Abs(nrm.y); // winding may be reversed

                            if (up >= NAV_DEBUG_WALKABLE_NORMAL_Y && green < NAV_DEBUG_MAX_MESH_TOP_TRIS)
                            {
                                if (markers < NAV_DEBUG_MAX_TEXTURED_MARKERS)
                                {
                                    CreateTexturedFaceMarker((a + b + c) / 3f + Vector3.up * 0.18f, walkMat, true);
                                    markers++;
                                }
                                green++;
                            }
                            else if (up < NAV_DEBUG_WALKABLE_NORMAL_Y && red < NAV_DEBUG_MAX_MESH_SIDE_TRIS)
                            {
                                Vector3 offset = new Vector3(nrm.x, 0f, nrm.z);
                                if (offset.sqrMagnitude < 0.0001f) offset = Vector3.up;
                                else offset.Normalize();
                                if (markers < NAV_DEBUG_MAX_TEXTURED_MARKERS)
                                {
                                    CreateTexturedFaceMarker((a + b + c) / 3f + offset * 0.20f, blockMat, false);
                                    markers++;
                                }
                                red++;
                            }
                        }
                    }
                }

                ZombieModEntry.Log("NavDebug: textured faces scanned=" + scanned +
                    " green=" + green + " red=" + red + " markers=" + markers + " skipped=" + skipped);
                _navDebugStatus = "navdbg=tex G" + green + " R" + red + " M" + markers;
            }
            catch (Exception ex) { ZombieModEntry.Log("NavDebug textured err: " + ex.Message); }
        }

        private void BuildSampledWalkOverlay(Vector3 center)
        {
            try
            {
                ClearNavDebug();
                _navDebugRoot = new GameObject("ZombieNavDebugOverlay_SampledWalk");
                UnityEngine.Object.DontDestroyOnLoad(_navDebugRoot);
                Material walkMat = CreateNavDebugMaterial("ZombieNavSample_Walkable", new Color(0.0f, 0.85f, 0.18f, 1f));
                Material blockMat = CreateNavDebugMaterial("ZombieNavSample_BlockedEdge", new Color(1f, 0.05f, 0.0f, 1f));

                int n = NAV_DEBUG_SAMPLE_HALF * 2 + 1;
                bool[,] ok = new bool[n, n];
                Vector3[,] pos = new Vector3[n, n];
                List<Vector3> greenVerts = new List<Vector3>();
                List<int> greenIdx = new List<int>();
                List<Vector3> redVerts = new List<Vector3>();
                List<int> redIdx = new List<int>();
                int green = 0;
                int red = 0;
                int miss = 0;

                for (int gz = 0; gz < n; gz++)
                {
                    for (int gx = 0; gx < n; gx++)
                    {
                        float x = (gx - NAV_DEBUG_SAMPLE_HALF) * NAV_DEBUG_SAMPLE_STEP;
                        float z = (gz - NAV_DEBUG_SAMPLE_HALF) * NAV_DEBUG_SAMPLE_STEP;
                        RaycastHit hit;
                        if (TryFindGroundHit(center + new Vector3(x, 0f, z), center.y, out hit))
                        {
                            ok[gx, gz] = true;
                            pos[gx, gz] = hit.point;
                            AddTopSampleQuad(greenVerts, greenIdx, hit.point);
                            green++;
                        }
                        else miss++;
                    }
                }

                for (int gz = 0; gz < n; gz++)
                {
                    for (int gx = 0; gx < n; gx++)
                    {
                        if (!ok[gx, gz]) continue;
                        if (gx + 1 < n && IsBlockedNeighbor(ok, pos, gx, gz, gx + 1, gz))
                        {
                            AddEdgeQuad(redVerts, redIdx, pos[gx, gz],
                                ok[gx + 1, gz] ? pos[gx + 1, gz] : pos[gx, gz] + new Vector3(NAV_DEBUG_SAMPLE_STEP, 0f, 0f));
                            red++;
                        }
                        if (gz + 1 < n && IsBlockedNeighbor(ok, pos, gx, gz, gx, gz + 1))
                        {
                            AddEdgeQuad(redVerts, redIdx, pos[gx, gz],
                                ok[gx, gz + 1] ? pos[gx, gz + 1] : pos[gx, gz] + new Vector3(0f, 0f, NAV_DEBUG_SAMPLE_STEP));
                            red++;
                        }
                    }
                }

                CreateNavDebugMesh("NavSampleWalkableMesh", greenVerts, greenIdx, walkMat);
                CreateNavDebugMesh("NavSampleBlockedEdgeMesh", redVerts, redIdx, blockMat);
                ZombieModEntry.Log("NavDebug: sampled walk green=" + green + " redEdges=" + red + " miss=" + miss);
                _navDebugStatus = "navdbg=sample G" + green + " R" + red;
            }
            catch (Exception ex) { ZombieModEntry.Log("NavDebug sample err: " + ex.Message); }
        }

        private void BuildMeshFaceOverlay(Vector3 center, Material topMat, Material sideMat,
            out int topTris, out int sideTris)
        {
            topTris = 0;
            sideTris = 0;

            MeshFilter[] filters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            if (filters == null) return;

            List<Vector3> topVerts = new List<Vector3>();
            List<int> topIdx = new List<int>();
            List<Vector3> sideVerts = new List<Vector3>();
            List<int> sideIdx = new List<int>();
            int scanned = 0;
            int skipped = 0;
            int markers = 0;

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter mf = filters[i];
                if (mf == null || mf.sharedMesh == null || mf.gameObject == null) { skipped++; continue; }
                if (!IsWorldMeshObject(mf.gameObject)) { skipped++; continue; }

                Renderer rr = mf.gameObject.GetComponent<Renderer>();
                Bounds rb = rr != null ? rr.bounds : mf.sharedMesh.bounds;
                if (rr == null)
                {
                    Vector3 wc = mf.transform.TransformPoint(rb.center);
                    rb = new Bounds(wc, Vector3.Scale(rb.size, mf.transform.lossyScale));
                }
                if (Vector3.Distance(new Vector3(rb.center.x, center.y, rb.center.z), center) > NAV_DEBUG_BLOCK_RADIUS + 20f)
                { skipped++; continue; }
                if (rb.min.y > center.y + 4f || rb.max.y < center.y - 12f)
                { skipped++; continue; }

                Mesh mesh = mf.sharedMesh;
                Vector3[] verts = mesh.vertices;
                int[] tris = mesh.triangles;
                scanned++;

                for (int t = 0; t + 2 < tris.Length; t += 3)
                {
                    Vector3 a = mf.transform.TransformPoint(verts[tris[t]]);
                    Vector3 b = mf.transform.TransformPoint(verts[tris[t + 1]]);
                    Vector3 c = mf.transform.TransformPoint(verts[tris[t + 2]]);
                    Vector3 n = Vector3.Cross(b - a, c - a);
                    if (n.sqrMagnitude < 0.0001f) continue;
                    n.Normalize();
                    float triYMax = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
                    float triYMin = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
                    if (triYMin > center.y + 8f || triYMax < center.y - 10f) continue;

                    if (Mathf.Abs(n.y) > 0.55f && topTris < NAV_DEBUG_MAX_MESH_TOP_TRIS)
                    {
                        AddDebugTriangle(topVerts, topIdx, a + Vector3.up * 0.18f, b + Vector3.up * 0.18f, c + Vector3.up * 0.18f, true);
                        if (markers < NAV_DEBUG_MAX_MESH_MARKERS)
                        {
                            CreateMeshFaceMarker("NavMeshTopMarker", (a + b + c) / 3f + Vector3.up * 0.28f, topMat, true);
                            markers++;
                        }
                        topTris++;
                    }
                    else if (Mathf.Abs(n.y) < 0.35f && (triYMax - triYMin) > 0.20f && sideTris < NAV_DEBUG_MAX_MESH_SIDE_TRIS)
                    {
                        AddDebugTriangle(sideVerts, sideIdx, a + n * 0.18f, b + n * 0.18f, c + n * 0.18f, true);
                        if (markers < NAV_DEBUG_MAX_MESH_MARKERS)
                        {
                            CreateMeshFaceMarker("NavMeshSideMarker", (a + b + c) / 3f + n * 0.28f, sideMat, false);
                            markers++;
                        }
                        sideTris++;
                    }
                }

                if (topTris >= NAV_DEBUG_MAX_MESH_TOP_TRIS && sideTris >= NAV_DEBUG_MAX_MESH_SIDE_TRIS)
                    break;
            }

            CreateNavDebugMesh("NavMeshTopWalkable", topVerts, topIdx, topMat);
            CreateNavDebugMesh("NavMeshSideBlocked", sideVerts, sideIdx, sideMat);
            ZombieModEntry.Log("NavDebug: mesh faces scanned=" + scanned + " topTris=" + topTris +
                " sideTris=" + sideTris + " markers=" + markers + " skippedMeshes=" + skipped);
        }

        private void CreateMeshFaceMarker(string name, Vector3 pos, Material mat, bool top)
        {
            float s = top ? 0.75f : 0.45f;
            Vector3 scale = top ? new Vector3(s, 0.12f, s) : new Vector3(0.35f, 0.75f, 0.35f);
            CreateNavDebugBox(name, pos, scale, mat);
        }

        private void CreateTexturedFaceMarker(Vector3 pos, Material mat, bool walkable)
        {
            Vector3 scale = walkable ? new Vector3(0.75f, 0.10f, 0.75f) : new Vector3(0.42f, 0.80f, 0.42f);
            CreateNavDebugBox(walkable ? "NavTextureWalkMarker" : "NavTextureBlockMarker", pos, scale, mat);
        }

        private static void AddDebugTriangle(List<Vector3> verts, List<int> idx, Vector3 a, Vector3 b, Vector3 c, bool doubleSided)
        {
            int start = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c);
            idx.Add(start); idx.Add(start + 1); idx.Add(start + 2);
            if (doubleSided)
            {
                idx.Add(start + 2); idx.Add(start + 1); idx.Add(start);
            }
        }

        private void CreateNavDebugMesh(string name, List<Vector3> verts, List<int> idx, Material mat)
        {
            if (verts.Count == 0 || idx.Count == 0) return;
            GameObject go = new GameObject(name);
            go.transform.parent = _navDebugRoot.transform;
            Mesh mesh = new Mesh();
            mesh.name = name + "_Mesh";
            mesh.vertices = verts.ToArray();
            mesh.triangles = idx.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
        }

        private static bool IsWorldBlockCollider(Collider c)
        {
            if (c == null || c.isTrigger) return false;
            GameObject go = c.gameObject;
            if (go == null) return false;
            string n = go.name;
            if (n == null) n = "";
            if (n.IndexOf("Zombie") >= 0 || n.IndexOf("NavDebug") >= 0 ||
                n.IndexOf("ExampleCharacter") >= 0 || n.IndexOf("Enemy") >= 0 ||
                n.IndexOf("Player") >= 0 || n.IndexOf("Camera") >= 0)
                return false;
            if (go.GetComponent<CharacterController>() != null) return false;
            return true;
        }

        private static bool HasUsableMesh(GameObject go)
        {
            if (go == null) return false;
            MeshFilter mf = go.GetComponent<MeshFilter>();
            return mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount >= 3;
        }

        private static bool IsWorldMeshObject(GameObject go)
        {
            if (go == null) return false;
            string n = go.name;
            if (n == null) n = "";
            if (n.IndexOf("Zombie") >= 0 || n.IndexOf("NavDebug") >= 0 ||
                n.IndexOf("ExampleCharacter") >= 0 || n.IndexOf("Enemy") >= 0 ||
                n.IndexOf("Player") >= 0 || n.IndexOf("Camera") >= 0 ||
                n.IndexOf("_UIDrawCall") >= 0 || n.IndexOf("Weapon") >= 0 ||
                n.IndexOf("hand") >= 0 || n.IndexOf("Hand") >= 0 ||
                n.IndexOf("Muzzle") >= 0 || n.IndexOf("FirePoint") >= 0)
                return false;
            if (go.GetComponent<CharacterController>() != null) return false;
            Renderer r = go.GetComponent<Renderer>();
            if (r == null) return false;
            Bounds b = r.bounds;
            if (b.size.x < 0.2f || b.size.y < 0.05f || b.size.z < 0.2f) return false;
            return true;
        }

        private static bool RendererHasTexturedMaterial(Renderer r)
        {
            if (r == null) return false;
            Material[] mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) return false;
            for (int i = 0; i < mats.Length; i++)
                if (IsTexturedMaterial(mats[i])) return true;
            return false;
        }

        private static bool IsTexturedMaterial(Material m)
        {
            if (m == null) return false;
            try
            {
                if (m.mainTexture != null) return true;
                if (m.HasProperty("_MainTex") && m.GetTexture("_MainTex") != null) return true;
            }
            catch { }
            string n = m.name;
            if (n == null) return false;
            if (n.IndexOf("Default") >= 0 || n.IndexOf("Font") >= 0 ||
                n.IndexOf("MenuSystem") >= 0 || n.IndexOf("Unlit") >= 0)
                return false;
            // Many old Unity mobile materials expose texture-backed atlas
            // materials by name even when mainTexture is not readable here.
            return true;
        }

        private int CreateNavDebugSideFaces(Bounds b, Material mat, int maxFaces)
        {
            int made = 0;
            float y = b.center.y;
            float h = Mathf.Max(0.1f, b.size.y);
            float t = 0.07f;
            if (made < maxFaces)
            {
                CreateNavDebugBox("NavSideBlocked", new Vector3(b.min.x - t, y, b.center.z),
                    new Vector3(t, h, Mathf.Max(0.15f, b.size.z)), mat);
                made++;
            }
            if (made < maxFaces)
            {
                CreateNavDebugBox("NavSideBlocked", new Vector3(b.max.x + t, y, b.center.z),
                    new Vector3(t, h, Mathf.Max(0.15f, b.size.z)), mat);
                made++;
            }
            if (made < maxFaces)
            {
                CreateNavDebugBox("NavSideBlocked", new Vector3(b.center.x, y, b.min.z - t),
                    new Vector3(Mathf.Max(0.15f, b.size.x), h, t), mat);
                made++;
            }
            if (made < maxFaces)
            {
                CreateNavDebugBox("NavSideBlocked", new Vector3(b.center.x, y, b.max.z + t),
                    new Vector3(Mathf.Max(0.15f, b.size.x), h, t), mat);
                made++;
            }
            return made;
        }

        private void CreateNavDebugBox(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.parent = _navDebugRoot.transform;
            go.transform.position = pos;
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.Destroy(c);
        }

        private static bool IsBlockedNeighbor(bool[,] ok, Vector3[,] pos, int ax, int az, int bx, int bz)
        {
            if (!ok[bx, bz]) return true;
            if (Mathf.Abs(pos[bx, bz].y - pos[ax, az].y) > NAV_DEBUG_MAX_STEP)
                return true;
            return HasWallBetween(pos[ax, az], pos[bx, bz]);
        }

        private static bool HasWallBetween(Vector3 a, Vector3 b)
        {
            Vector3 start = a + new Vector3(0f, 0.75f, 0f);
            Vector3 end = b + new Vector3(0f, 0.75f, 0f);
            Vector3 dir = end - start;
            float dist = dir.magnitude;
            if (dist < 0.05f) return false;
            RaycastHit hit;
            if (!Physics.Raycast(start, dir / dist, out hit, dist, -1)) return false;
            return IsWorldBlockCollider(hit.collider);
        }

        private static void AddTopSampleQuad(List<Vector3> verts, List<int> idx, Vector3 p)
        {
            float h = NAV_DEBUG_SAMPLE_STEP * 0.46f;
            float y = p.y + 0.10f;
            Vector3 a = new Vector3(p.x - h, y, p.z - h);
            Vector3 b = new Vector3(p.x + h, y, p.z - h);
            Vector3 c = new Vector3(p.x + h, y, p.z + h);
            Vector3 d = new Vector3(p.x - h, y, p.z + h);
            AddQuad(verts, idx, a, b, c, d, true);
        }

        private static void AddEdgeQuad(List<Vector3> verts, List<int> idx, Vector3 a, Vector3 b)
        {
            Vector3 mid = (a + b) * 0.5f;
            bool xEdge = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.z - a.z);
            float half = NAV_DEBUG_SAMPLE_STEP * 0.46f;
            float y0 = Mathf.Min(a.y, b.y) + 0.08f;
            float y1 = Mathf.Max(a.y, b.y) + 1.55f;

            Vector3 p0, p1, p2, p3;
            if (xEdge)
            {
                p0 = new Vector3(mid.x, y0, mid.z - half);
                p1 = new Vector3(mid.x, y0, mid.z + half);
                p2 = new Vector3(mid.x, y1, mid.z + half);
                p3 = new Vector3(mid.x, y1, mid.z - half);
            }
            else
            {
                p0 = new Vector3(mid.x - half, y0, mid.z);
                p1 = new Vector3(mid.x + half, y0, mid.z);
                p2 = new Vector3(mid.x + half, y1, mid.z);
                p3 = new Vector3(mid.x - half, y1, mid.z);
            }
            AddQuad(verts, idx, p0, p1, p2, p3, true);
        }

        private static void AddQuad(List<Vector3> verts, List<int> idx, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool doubleSided)
        {
            int start = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
            idx.Add(start); idx.Add(start + 1); idx.Add(start + 2);
            idx.Add(start); idx.Add(start + 2); idx.Add(start + 3);
            if (doubleSided)
            {
                idx.Add(start + 2); idx.Add(start + 1); idx.Add(start);
                idx.Add(start + 3); idx.Add(start + 2); idx.Add(start);
            }
        }

        private static bool TryFindGroundHit(Vector3 sample, float playerY, out RaycastHit bestHit)
        {
            bestHit = new RaycastHit();
            Vector3 start = new Vector3(sample.x, playerY + 10f, sample.z);
            RaycastHit[] hits = Physics.RaycastAll(start, Vector3.down, 24f, -1);
            bool found = false;
            float bestScore = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit h = hits[i];
                if (h.collider == null) continue;
                if (Mathf.Abs(h.normal.y) < 0.35f) continue; // floor-ish surfaces, even if wound downward
                float dy = Mathf.Abs(h.point.y - playerY);
                if (dy > 7f) continue; // ignore invisible ceilings/low skybox lids

                // Prefer hits closest to the current player floor, then flatter surfaces.
                float score = dy - h.normal.y * 0.25f;
                if (!found || score < bestScore)
                {
                    found = true;
                    bestScore = score;
                    bestHit = h;
                }
            }

            return found;
        }

        private void CreateNavDebugTile(Vector3 pos, Material mat, bool lowProfile)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = lowProfile ? "NavWalkable" : "NavBlocked";
            go.transform.parent = _navDebugRoot.transform;
            go.transform.position = pos + new Vector3(0f, NAV_DEBUG_TILE_Y, 0f);
            float h = lowProfile ? 0.22f : 0.8f;
            go.transform.localScale = new Vector3(NAV_DEBUG_NODE_STEP * 0.9f, h, NAV_DEBUG_NODE_STEP * 0.9f);
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.Destroy(c);
        }

        private void CreateNavDebugBeacon(Vector3 pos, string name)
        {
            if (!_navDebugEnabled || _navDebugRoot == null) return;
            try
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.parent = _navDebugRoot.transform;
                go.transform.position = pos + new Vector3(0f, 3f, 0f);
                go.transform.localScale = new Vector3(1f, 6f, 1f);
                Renderer r = go.GetComponent<Renderer>();
                if (r != null)
                    r.material = CreateNavDebugMaterial("ZombieNavDebug_Beacon", new Color(1f, 0f, 1f, 0.85f));
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.Destroy(c);
                ZombieModEntry.Log("NavDebug: beacon created at " + pos);
            }
            catch (Exception ex) { ZombieModEntry.Log("NavDebug beacon err: " + ex.Message); }
        }

        private void ClearNavDebug()
        {
            if (_navDebugRoot != null)
                UnityEngine.Object.Destroy(_navDebugRoot);
            _navDebugRoot = null;
            _navDebugStatus = "";
        }

        private static Material CreateNavDebugMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Self-Illumin/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Diffuse");
            if (shader == null) shader = Shader.Find("Transparent/Diffuse");
            Material mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);
            if (mat.HasProperty("_Emission")) mat.SetColor("_Emission", color);
            if (mat.HasProperty("_EmisColor")) mat.SetColor("_EmisColor", color);
            return mat;
        }

        private static object FindFirstGridGraph(object astar, Type astarType)
        {
            if (astar == null || astarType == null) return null;
            object data = GetMember(astar, astarType, "astarData");
            if (data == null) data = GetMember(astar, astarType, "data");
            if (data == null) return null;

            Array graphs = GetMember(data, data.GetType(), "graphs") as Array;
            if (graphs == null) return null;
            foreach (object graph in graphs)
            {
                if (graph == null) continue;
                if (graph.GetType().Name.IndexOf("GridGraph") >= 0)
                    return graph;
            }
            return null;
        }

        private static bool TryGetNodePosition(object node, out Vector3 pos)
        {
            pos = Vector3.zero;
            if (node == null) return false;
            object raw = GetMember(node, node.GetType(), "position");
            if (raw == null) raw = GetMember(node, node.GetType(), "Position");
            if (raw == null) return false;

            if (raw is Vector3)
            {
                pos = (Vector3)raw;
                return true;
            }

            Type t = raw.GetType();
            object xo = GetMember(raw, t, "x");
            object yo = GetMember(raw, t, "y");
            object zo = GetMember(raw, t, "z");
            if (xo == null || yo == null || zo == null) return false;

            float factor = 0.001f;
            FieldInfo pf = t.GetField("PrecisionFactor", BindingFlags.Public | BindingFlags.Static);
            if (pf != null) factor = Convert.ToSingle(pf.GetValue(null));
            else
            {
                FieldInfo fp = t.GetField("FloatPrecision", BindingFlags.Public | BindingFlags.Static);
                if (fp != null)
                {
                    float precision = Convert.ToSingle(fp.GetValue(null));
                    if (precision > 0.0001f) factor = 1f / precision;
                }
            }

            pos = new Vector3(Convert.ToSingle(xo) * factor,
                              Convert.ToSingle(yo) * factor,
                              Convert.ToSingle(zo) * factor);
            return true;
        }

        private static bool GetNodeWalkable(object node)
        {
            object v = GetMember(node, node.GetType(), "Walkable");
            if (v == null) v = GetMember(node, node.GetType(), "walkable");
            if (v is bool) return (bool)v;
            return true;
        }

        private static object GetMember(object obj, Type t, string name)
        {
            if (obj == null || t == null) return null;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo f = t.GetField(name, flags);
            if (f != null) return f.GetValue(obj);
            PropertyInfo p = t.GetProperty(name, flags);
            if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(obj, null);
            return null;
        }

        private static bool SetMember(object obj, Type t, string name, object val)
        {
            if (obj == null || t == null) return false;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo f = t.GetField(name, flags);
            if (f != null)
            {
                f.SetValue(obj, CoerceValue(val, f.FieldType));
                return true;
            }
            PropertyInfo p = t.GetProperty(name, flags);
            if (p != null && p.CanWrite && p.GetIndexParameters().Length == 0)
            {
                p.SetValue(obj, CoerceValue(val, p.PropertyType), null);
                return true;
            }
            return false;
        }

        private static object CoerceValue(object val, Type targetType)
        {
            if (val == null || targetType == null) return val;
            if (targetType.IsAssignableFrom(val.GetType())) return val;
            if (targetType == typeof(int) && val is UnityEngine.LayerMask)
                return ((UnityEngine.LayerMask)val).value;
            if (targetType == typeof(UnityEngine.LayerMask) && val is int)
                return (UnityEngine.LayerMask)(int)val;
            try { return Convert.ChangeType(val, targetType); }
            catch { return val; }
        }

        private static bool TrySetGridDimensions(object grid, Type gridType, int width, int depth, float nodeSize)
        {
            MethodInfo mi = gridType.GetMethod("SetDimensions",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(int), typeof(int), typeof(float) }, null);
            if (mi == null) return false;
            mi.Invoke(grid, new object[] { width, depth, nodeSize });
            ZombieModEntry.Log("BuildAstar: SetDimensions(" + width + "," + depth + "," + nodeSize + ")");
            return true;
        }

        private static int ToInt(object v, int fallback)
        {
            if (v == null) return fallback;
            try { return Convert.ToInt32(v); }
            catch { return fallback; }
        }

        private static void SetGridField(object obj, Type t, string name, object val)
        {
            if (!SetMember(obj, t, name, val))
                ZombieModEntry.Log("BuildAstar: field/property '" + name + "' not found on " + t.Name);
        }

        // ─────────────────────────────────────────────────────────────────────────        // Singleplayer enemy scan — waits 4 s for SingleEnemyManager to spawn
        // enemies, then logs every component on every enemy GO + children.
        // ─────────────────────────────────────────────────────────────────────
        private System.Collections.IEnumerator ScanEnemiesDelayed()
        {
            yield return new WaitForSeconds(4f);
            try
            {
                // Search directly by SingleEnemyLogic and SingleEnemyAI component types
                // so we always find the right GO regardless of its name.
                Type logicType = FindType("SingleEnemyLogic");
                Type aiType    = FindType("SingleEnemyAI");
                bool found = false;

                if (logicType != null)
                {
                    Component[] comps = Resources.FindObjectsOfTypeAll(logicType) as Component[];
                    ZombieModEntry.Log("EnemyScan: found " + comps.Length + " SingleEnemyLogic instances");
                    int n = 0;
                    foreach (Component c in comps)
                    {
                        found = true;
                        DumpComponents(c.gameObject, "");
                        if (++n >= 3) break;
                    }
                }
                else ZombieModEntry.Log("EnemyScan: SingleEnemyLogic type not found");

                if (aiType != null)
                {
                    Component[] comps = Resources.FindObjectsOfTypeAll(aiType) as Component[];
                    ZombieModEntry.Log("EnemyScan: found " + comps.Length + " SingleEnemyAI instances");
                    int n = 0;
                    foreach (Component c in comps)
                    {
                        found = true;
                        DumpComponents(c.gameObject, "");
                        if (++n >= 3) break;
                    }
                }
                else ZombieModEntry.Log("EnemyScan: SingleEnemyAI type not found");

                if (!found)
                    ZombieModEntry.Log("EnemyScan: no live enemies found after 4s — waiting for a wave to spawn");
            }
            catch (Exception ex) { ZombieModEntry.Log("EnemyScan err: " + ex.Message); }
        }

        private static void DumpComponents(GameObject go, string indent)
        {
            Component[] comps = go.GetComponents<Component>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(indent).Append("GO: ").Append(go.name).Append(" [");
            foreach (Component c in comps)
                sb.Append(c.GetType().Name).Append(", ");
            sb.Append("]");
            ZombieModEntry.Log(sb.ToString());
            for (int i = 0; i < go.transform.childCount; i++)
                DumpComponents(go.transform.GetChild(i).gameObject, indent + "  ");
        }

        // ─────────────────────────────────────────────────────────────────────
        private static bool IsGameScene(string s) { return s.StartsWith("FreeRun") || s.StartsWith("CRScene"); }

        private static Type FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            { Type t = asm.GetType(name); if (t != null) return t; }
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE DRIVER — runs on the enemy GO on the master client only.
    // Uses the game's SingleEnemyAI component (which extends AIPath) for
    // pathfinding movement.  Falls back to direct transform movement when
    // AstarPath.active is null (i.e. the current map has no A* navmesh).
    // ─────────────────────────────────────────────────────────────────────────
    internal static class ZombieNavGrid
    {
        private const float CELL = 0.5f;
        private const int   N = 240;
        private const float HALF = CELL * N * 0.5f;
        private const float RADIUS = 0.45f;
        private const float HEIGHT = 1.8f;
        private const float MAX_CLIMB = 1.05f;
        private const float LOW_OBSTACLE_IGNORE = 0.45f;
        private const float MAX_SURFACE_ABOVE_BAKE = 6.0f;
        private const float REACH = 0.65f;
        private const int   MAX_LINK_CELLS = 4;
        private const float MAX_LINK_CLIMB = 2.25f;
        private const float MAX_LINK_SLOPE = 0.9f;
        private const float PARTIAL_REACH_CELLS = 6.0f;
        private const float ENDPOINT_HEIGHT_TOLERANCE = 1.45f;

        private static bool _ready;
        private static int _walkableCount;
        private static float _minX;
        private static float _minZ;
        private static readonly byte[] _walk = new byte[N * N];
        private static readonly float[] _height = new float[N * N];
        private static readonly float[] _g = new float[N * N];
        private static readonly int[] _parent = new int[N * N];
        private static readonly int[] _gen = new int[N * N];
        private static readonly int[] _closed = new int[N * N];
        private static int _curGen = 1;

        public static bool Ready { get { return _ready; } }
        public static int WalkableCount { get { return _walkableCount; } }

        public static bool TrySnapToWalkable(Vector3 pos, int maxR, out Vector3 world)
        {
            world = pos;
            if (!_ready) return false;
            int cell = NearestWalkable(Cx(pos.x), Cz(pos.z), maxR);
            if (cell < 0) return false;
            world = new Vector3(Wx(cell % N), _height[cell], Wz(cell / N));
            return true;
        }

        public static bool TrySnapToWalkableNearHeight(Vector3 pos, int maxR, float referenceY, float maxDy, out Vector3 world)
        {
            world = pos;
            if (!_ready) return false;
            int cell = NearestWalkableNearHeight(Cx(pos.x), Cz(pos.z), maxR, referenceY, maxDy);
            if (cell < 0) return false;
            world = new Vector3(Wx(cell % N), _height[cell], Wz(cell / N));
            return true;
        }

        public static bool TryGetWalkableAt(Vector3 pos, float referenceY, float maxDy, out Vector3 world)
        {
            world = pos;
            if (!_ready) return false;
            int cx = Cx(pos.x), cz = Cz(pos.z);
            if (!Ok(cx, cz)) return false;
            int cell = Idx(cx, cz);
            if (_walk[cell] == 0 || Mathf.Abs(_height[cell] - referenceY) > maxDy) return false;
            world = new Vector3(Wx(cx), _height[cell], Wz(cz));
            return true;
        }

        public static void Bake(Vector3 center)
        {
            _minX = center.x - HALF;
            _minZ = center.z - HALF;
            Array.Clear(_walk, 0, _walk.Length);
            Array.Clear(_height, 0, _height.Length);

            int walk = 0, blocked = 0, noGround = 0;
            for (int z = 0; z < N; z++)
            {
                for (int x = 0; x < N; x++)
                {
                    int idx = Idx(x, z);
                    RaycastHit hit;
                    if (!TryFindGround(new Vector3(Wx(x), center.y, Wz(z)), center.y, out hit))
                    {
                        noGround++;
                        continue;
                    }
                    _height[idx] = hit.point.y;
                    if (IsBodyBlocked(hit.point))
                    {
                        blocked++;
                        continue;
                    }
                    _walk[idx] = 1;
                    walk++;
                }
            }

            _walkableCount = walk;
            _ready = walk > 0;
            ZombieModEntry.Log("ZombieNavGrid: baked walk=" + walk + " blocked=" + blocked +
                " noGround=" + noGround + " cell=" + CELL + " maxClimb=" + MAX_CLIMB);
        }

        public static List<Vector3> Query(Vector3 from, Vector3 to)
        {
            return Query(from, to, false, 0);
        }

        public static List<Vector3> Query(Vector3 from, Vector3 to, bool debug, byte debugId)
        {
            if (!_ready)
            {
                if (debug) ZombieModEntry.Log("ZAI[" + debugId + "] Query: grid not ready");
                return null;
            }
            int rawStartX = Cx(from.x), rawStartZ = Cz(from.z);
            int rawEndX = Cx(to.x), rawEndZ = Cz(to.z);
            int start = NearestWalkableNearHeight(rawStartX, rawStartZ, 24, from.y, ENDPOINT_HEIGHT_TOLERANCE);
            if (start < 0) start = NearestWalkable(rawStartX, rawStartZ, 24);
            int end = NearestWalkableNearHeight(rawEndX, rawEndZ, 24, to.y, ENDPOINT_HEIGHT_TOLERANCE);
            if (end < 0) end = NearestWalkable(rawEndX, rawEndZ, 24);
            if (debug)
                ZombieModEntry.Log("ZAI[" + debugId + "] Query: from=" + from + " to=" + to +
                    " rawStart=(" + rawStartX + "," + rawStartZ + ") rawEnd=(" + rawEndX + "," + rawEndZ + ")" +
                    " start=" + start + " startY=" + (start >= 0 ? _height[start].ToString("F2") : "n/a") +
                    " end=" + end + " endY=" + (end >= 0 ? _height[end].ToString("F2") : "n/a"));
            if (start < 0 || end < 0)
            {
                if (debug) ZombieModEntry.Log("ZAI[" + debugId + "] Query: no nearest walkable start/end");
                return null;
            }

            int gen = ++_curGen;
            if (_curGen == int.MaxValue)
            {
                Array.Clear(_gen, 0, _gen.Length);
                Array.Clear(_closed, 0, _closed.Length);
                _curGen = 1;
                gen = 1;
            }

            MinHeap open = new MinHeap(4096);
            _g[start] = 0f;
            _parent[start] = -1;
            _gen[start] = gen;
            open.Push(start, Heuristic(start, end));

            bool found = false;
            int best = start;
            float bestH = Heuristic(start, end);
            int expanded = 0;
            while (open.Count > 0)
            {
                int cur = open.Pop();
                if (_closed[cur] == gen) continue;
                _closed[cur] = gen;
                expanded++;
                float curH = Heuristic(cur, end);
                if (curH < bestH)
                {
                    bestH = curH;
                    best = cur;
                }
                if (cur == end) { found = true; break; }

                int cx = cur % N;
                int cz = cur / N;
                for (int dz = -1; dz <= 1; dz++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dz == 0) continue;
                        for (int step = 1; step <= MAX_LINK_CELLS; step++)
                        {
                            int nx = cx + dx * step, nz = cz + dz * step;
                            if (!Ok(nx, nz)) break;
                            int ni = Idx(nx, nz);
                            if (_walk[ni] == 0) continue;
                            if (!CanConnect(cur, ni, dx * step, dz * step)) continue;
                            if (step == 1 && dx != 0 && dz != 0)
                            {
                                int a = Idx(cx + dx, cz);
                                int b = Idx(cx, cz + dz);
                                if (_walk[a] == 0 || _walk[b] == 0) continue;
                                if (!CanConnect(cur, a, dx, 0) || !CanConnect(cur, b, 0, dz)) continue;
                            }

                            float stepCost = Mathf.Sqrt((dx * step) * (dx * step) + (dz * step) * (dz * step));
                            float ng = _g[cur] + stepCost;
                            if (_gen[ni] != gen || ng < _g[ni])
                            {
                                _gen[ni] = gen;
                                _g[ni] = ng;
                                _parent[ni] = cur;
                                open.Push(ni, ng + Heuristic(ni, end));
                            }
                        }
                    }
                }
            }
            bool usePartial = !found && best != start && bestH <= PARTIAL_REACH_CELLS;
            List<Vector3> result = found ? Reconstruct(end, gen) : (usePartial ? Reconstruct(best, gen) : null);
            if (debug)
                ZombieModEntry.Log("ZAI[" + debugId + "] Query: found=" + found +
                    " partial=" + usePartial + " expanded=" + expanded +
                    " points=" + (result != null ? result.Count : 0) +
                    " bestH=" + bestH.ToString("F2"));
            return result;
        }

        public static bool GetWaypoint(List<Vector3> path, Vector3 pos, ref int idx, out Vector3 wp)
        {
            wp = pos;
            if (path == null || path.Count == 0) return false;
            if (idx < 0) idx = 0;
            if (idx >= path.Count) idx = path.Count - 1;
            while (idx < path.Count - 1)
            {
                Vector3 p = path[idx];
                float dx = p.x - pos.x, dz = p.z - pos.z;
                if (dx * dx + dz * dz <= REACH * REACH) idx++;
                else break;
            }
            wp = path[idx];
            return true;
        }

        private static List<Vector3> Reconstruct(int end, int gen)
        {
            List<int> cells = new List<int>();
            int cur = end, guard = 0;
            while (cur >= 0 && guard++ < N * N)
            {
                cells.Add(cur);
                cur = (_gen[cur] == gen) ? _parent[cur] : -1;
            }
            cells.Reverse();

            List<Vector3> pts = new List<Vector3>();
            int lastDx = 99, lastDz = 99;
            for (int i = 0; i < cells.Count; i++)
            {
                int c = cells[i];
                int dx = 0, dz = 0;
                if (i + 1 < cells.Count)
                {
                    dx = (cells[i + 1] % N) - (c % N);
                    dz = (cells[i + 1] / N) - (c / N);
                }
                if (i == 0 || i == cells.Count - 1 || dx != lastDx || dz != lastDz)
                    pts.Add(new Vector3(Wx(c % N), _height[c], Wz(c / N)));
                lastDx = dx;
                lastDz = dz;
            }
            return pts;
        }

        private static bool CanConnect(int a, int b, int dx, int dz)
        {
            float dh = Mathf.Abs(_height[a] - _height[b]);
            if (dh <= MAX_CLIMB) return true;
            float horiz = CELL * Mathf.Sqrt(dx * dx + dz * dz);
            if (horiz <= 0.01f) return false;
            return dh <= MAX_LINK_CLIMB && dh <= horiz * MAX_LINK_SLOPE;
        }

        private static float Heuristic(int a, int b)
        {
            int ax = a % N, az = a / N, bx = b % N, bz = b / N;
            int dx = Mathf.Abs(ax - bx), dz = Mathf.Abs(az - bz);
            return Mathf.Max(dx, dz) + 0.4142f * Mathf.Min(dx, dz);
        }

        private static int NearestWalkable(int cx, int cz, int maxR)
        {
            if (Ok(cx, cz) && _walk[Idx(cx, cz)] != 0) return Idx(cx, cz);
            for (int r = 1; r <= maxR; r++)
                for (int dz = -r; dz <= r; dz++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;
                        int x = cx + dx, z = cz + dz;
                        if (Ok(x, z) && _walk[Idx(x, z)] != 0) return Idx(x, z);
                    }
            return -1;
        }

        private static int NearestWalkableNearHeight(int cx, int cz, int maxR, float y, float maxDy)
        {
            int best = -1;
            float bestScore = 999999f;
            for (int r = 0; r <= maxR; r++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (r != 0 && Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;
                        int x = cx + dx, z = cz + dz;
                        if (!Ok(x, z)) continue;
                        int idx = Idx(x, z);
                        if (_walk[idx] == 0) continue;
                        float dy = Mathf.Abs(_height[idx] - y);
                        if (dy > maxDy) continue;
                        float score = r * 10f + dy;
                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = idx;
                        }
                    }
                }
                if (best >= 0) return best;
            }
            return -1;
        }

        private static bool TryFindGround(Vector3 sample, float playerY, out RaycastHit bestHit)
        {
            bestHit = new RaycastHit();
            bool found = false;
            float bestY = -999999f;
            ConsiderGroundHits(Physics.RaycastAll(new Vector3(sample.x, playerY + 14f, sample.z), Vector3.down, 30f, -1),
                playerY, ref found, ref bestY, ref bestHit);
            ConsiderGroundHits(Physics.RaycastAll(new Vector3(sample.x, playerY - 14f, sample.z), Vector3.up, 30f, -1),
                playerY, ref found, ref bestY, ref bestHit);
            return found;
        }

        private static void ConsiderGroundHits(RaycastHit[] hits, float playerY, ref bool found, ref float bestY, ref RaycastHit bestHit)
        {
            if (hits == null) return;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit h = hits[i];
                if (h.collider == null || !IsWorldBlockCollider(h.collider)) continue;
                if (Mathf.Abs(h.normal.y) < 0.35f) continue;
                if (h.point.y < playerY - 4f || h.point.y > playerY + MAX_SURFACE_ABOVE_BAKE) continue;
                if (IsBodyBlocked(h.point)) continue;
                if (!found || h.point.y > bestY + 0.05f)
                {
                    found = true;
                    bestY = h.point.y;
                    bestHit = h;
                }
            }
        }

        private static bool IsBodyBlocked(Vector3 ground)
        {
            if (IsBodySphereBlocked(new Vector3(ground.x, ground.y + LOW_OBSTACLE_IGNORE + RADIUS, ground.z), ground.y))
                return true;
            if (IsBodySphereBlocked(new Vector3(ground.x, ground.y + 1.15f, ground.z), ground.y))
                return true;
            if (IsBodySphereBlocked(new Vector3(ground.x, ground.y + HEIGHT - RADIUS, ground.z), ground.y))
                return true;
            return false;
        }

        private static bool IsBodySphereBlocked(Vector3 center, float groundY)
        {
            Collider[] hits = Physics.OverlapSphere(center, RADIUS, -1);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider c = hits[i];
                if (!IsWorldBlockCollider(c)) continue;
                if (c.bounds.max.y <= groundY + LOW_OBSTACLE_IGNORE + 0.05f) continue;
                return true;
            }
            return false;
        }

        private static bool IsWorldBlockCollider(Collider c)
        {
            if (c == null || c.isTrigger) return false;
            GameObject go = c.gameObject;
            if (go == null) return false;
            string n = go.name;
            if (n == null) n = "";
            if (n.IndexOf("Zombie") >= 0 || n.IndexOf("NavDebug") >= 0 ||
                n.IndexOf("ExampleCharacter") >= 0 || n.IndexOf("Enemy") >= 0 ||
                n.IndexOf("Player") >= 0 || n.IndexOf("Camera") >= 0)
                return false;
            if (go.GetComponent<CharacterController>() != null) return false;
            return true;
        }

        private static int Cx(float wx) { return Mathf.FloorToInt((wx - _minX) / CELL); }
        private static int Cz(float wz) { return Mathf.FloorToInt((wz - _minZ) / CELL); }
        private static float Wx(int cx) { return _minX + cx * CELL + CELL * 0.5f; }
        private static float Wz(int cz) { return _minZ + cz * CELL + CELL * 0.5f; }
        private static int Idx(int cx, int cz) { return cz * N + cx; }
        private static bool Ok(int cx, int cz) { return cx >= 0 && cx < N && cz >= 0 && cz < N; }

        private class MinHeap
        {
            private int[] _items;
            private float[] _f;
            public int Count;
            public MinHeap(int cap)
            {
                _items = new int[cap];
                _f = new float[cap];
            }
            public void Push(int item, float f)
            {
                if (Count >= _items.Length)
                {
                    Array.Resize(ref _items, _items.Length * 2);
                    Array.Resize(ref _f, _f.Length * 2);
                }
                int i = Count++;
                _items[i] = item;
                _f[i] = f;
                while (i > 0)
                {
                    int p = (i - 1) >> 1;
                    if (_f[p] <= _f[i]) break;
                    Swap(i, p);
                    i = p;
                }
            }
            public int Pop()
            {
                int result = _items[0];
                Count--;
                if (Count > 0)
                {
                    _items[0] = _items[Count];
                    _f[0] = _f[Count];
                    int i = 0;
                    for (;;)
                    {
                        int l = i * 2 + 1, r = l + 1, s = i;
                        if (l < Count && _f[l] < _f[s]) s = l;
                        if (r < Count && _f[r] < _f[s]) s = r;
                        if (s == i) break;
                        Swap(i, s);
                        i = s;
                    }
                }
                return result;
            }
            private void Swap(int a, int b)
            {
                int ti = _items[a]; _items[a] = _items[b]; _items[b] = ti;
                float tf = _f[a]; _f[a] = _f[b]; _f[b] = tf;
            }
        }
    }

    public class ZombieWallBuyHologram : MonoBehaviour
    {
        public ZombieWallBuy Data;
        public ZombieHook Hook;
        public float BaseY;
        private float _phase;
        private readonly List<UnityEngine.Object> _owned = new List<UnityEngine.Object>();

        public void Own(UnityEngine.Object obj) { if (obj != null) _owned.Add(obj); }

        void Start() { _phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f); }

        void Update()
        {
            transform.Rotate(Vector3.up, 32f * Time.deltaTime, Space.World);
            Vector3 p = transform.position;
            p.y = BaseY + Mathf.Sin(Time.time * 1.7f + _phase) * 0.16f;
            transform.position = p;
        }

        void OnDestroy()
        {
            for (int i = 0; i < _owned.Count; i++)
                if (_owned[i] != null) try { UnityEngine.Object.Destroy(_owned[i]); } catch { }
            _owned.Clear();
        }
    }

    public class ZombieDriver : MonoBehaviour
    {
        public byte  ZombieId;
        public float ChaseSpeed = 2.8f;
        public float MaxHealth = 100f;
        public int AttackDamage = 8;
        public float AttackCooldown = 1.5f;
        public ZombieHook Hook;

        private const float CLIMB_RATE = 3.8f;
        private const float DESCEND_RATE = 7.0f;
        private const float STUCK_MIN_MOVE = 0.04f;
        private const float STUCK_TIME = 1.15f;
        private const float RECYCLE_DISTANCE = 55f;
        private const float RECYCLE_DELAY = 4f;

        private Component          _singleEnemyAI; // SingleEnemyAI instance (may be null if not on GO)
        private Component          _singleEnemyLogic; // SingleEnemyLogic instance for visible health state
        private CharacterController _cc;            // for wall-respecting movement when no A* graph
        private bool                _hasAstar;
        private bool                _aiReady;
        private bool                _useCustomNav;
        private Transform           _moveTarget;
        private List<Vector3>       _path;
        private int                 _pathIdx;
        private float               _pathTimer;
        private float               _velY;
        private int                 _lastLoggedPathIdx = -999;
        private string              _lastMoveDecision = "";
        private float               _stuckTimer;
        private int                 _stuckRecoveries;
        private float               _farFromPlayersTimer;
        private Vector3             _lastMovePos;
        private float               _unstickTimer;
        private Vector3             _unstickDir;
        private static int          _debugZombieId = -1;

        // Reflected fields/props on SingleEnemyAI / AIPath
        private FieldInfo  _fTarget;       // AIPath.target     (Transform)
        private FieldInfo  _fSpeed;        // AIPath.speed      (float)
        private FieldInfo  _fCanSearch;    // AIPath.canSearch  (bool)
        private FieldInfo  _fCanMove;      // AIPath.canMove    (bool)
        private FieldInfo  _fLogicBlood;   // SingleEnemyLogic.blood
        private FieldInfo  _fLogicBDied;   // SingleEnemyLogic.bDied
        private static Type _singleEnemyLogicType;
        private Transform  _bloodTransform;

        // Melee attack
        private const float ATTACK_RANGE    = 1.5f;
        private const float TARGET_REFRESH  = 0.3f;  // seconds between target re-scans
        private float       _attackCd;
        private float       _targetTimer;
        private PlayerTarget _cachedTarget;
        private static FieldInfo  _sfPlayerLogicInst;
        private static MethodInfo _smPlayerDamage;
        // Reflection caches for CNRMultiplayerManager target scanning (shared by all drivers)
        private static Type      _sMgrType;
        private static FieldInfo _sfMgrInst;
        private static FieldInfo _sfOtherObjects;
        private static FieldInfo _sfOtherInfos;
        private static FieldInfo _sfInfoId;

        // Body/hand Animation components (for playing walk animation)
        private Animation _bodyAnim;
        private Animation _handAnim;

        void OnDestroy()
        {
            try { if (Hook != null) Hook.OnZombieVanillaDeath(ZombieId); }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver.OnDestroy err: " + ex.Message); }
        }

        void Start()
        {
            try
            {
                Type aiType = FindType("SingleEnemyAI");
                if (_debugZombieId < 0)
                {
                    _debugZombieId = UnityEngine.Random.Range(1, 6);
                    ZombieModEntry.Log("ZAI: verbose debug zombie selected id=" + _debugZombieId);
                }
                Dbg("Start: spawned at " + transform.position);
                if (aiType != null)
                {
                    // Enemy3's SingleEnemyAI may be on a child GO, not the root
                    _singleEnemyAI = GetComponent(aiType) ?? GetComponentInChildren(aiType);
                    if (_singleEnemyAI != null)
                    {
                        ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: found SingleEnemyAI on " + _singleEnemyAI.gameObject.name);
                        _fTarget    = aiType.GetField("target",    BindingFlags.Public | BindingFlags.Instance)
                                   ?? aiType.BaseType.GetField("target",    BindingFlags.Public | BindingFlags.Instance);
                        _fSpeed     = aiType.GetField("speed",     BindingFlags.Public | BindingFlags.Instance)
                                   ?? aiType.BaseType.GetField("speed",     BindingFlags.Public | BindingFlags.Instance);
                        _fCanSearch = aiType.GetField("canSearch", BindingFlags.Public | BindingFlags.Instance)
                                   ?? aiType.BaseType.GetField("canSearch", BindingFlags.Public | BindingFlags.Instance);
                        _fCanMove   = aiType.GetField("canMove",   BindingFlags.Public | BindingFlags.Instance)
                                   ?? aiType.BaseType.GetField("canMove",   BindingFlags.Public | BindingFlags.Instance);
                    }
                }

                try
                {
                    Type logicType = FindType("SingleEnemyLogic");
                    if (logicType != null)
                    {
                        _singleEnemyLogicType = logicType;
                        _singleEnemyLogic = GetComponent(logicType) ?? GetComponentInChildren(logicType);
                    }
                    if (_singleEnemyLogic == null)
                    {
                        logicType = FindType("enemyLogic");
                        if (logicType != null)
                        {
                            _singleEnemyLogicType = logicType;
                            _singleEnemyLogic = GetComponent(logicType) ?? GetComponentInChildren(logicType);
                        }
                    }
                    if (logicType != null)
                    {
                        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        _fLogicBlood = logicType.GetField("blood", flags) ??
                                       logicType.GetField("Blood", flags);
                        _fLogicBDied = logicType.GetField("bDied", flags) ??
                                      logicType.GetField("bDead", flags);
                        ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: logic refs type=" + logicType.Name +
                            " logic=" + (_singleEnemyLogic != null) + " blood=" + (_fLogicBlood != null) +
                            " bDied=" + (_fLogicBDied != null));
                    }
                }
                catch (Exception ex) { ZombieModEntry.Log("ZombieDriver logic reflect err: " + ex.Message); }
                _bloodTransform = FindBloodTransform(transform);
                ApplySharedHealth(MaxHealth);

                // Check if A* is present in this scene
                _hasAstar = CheckAstar();
                _useCustomNav = ZombieNavGrid.Ready;
                Dbg("Start: hasAstar=" + _hasAstar + " customNavReady=" + _useCustomNav +
                    " hasSingleEnemyAI=" + (_singleEnemyAI != null));

                if (_useCustomNav)
                {
                    if (_singleEnemyAI != null) SetAI(false, 0f);
                    _aiReady = false;
                    ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: custom grid pathfinding active");
                }
                else if (_singleEnemyAI != null && _hasAstar)
                {
                    // Set target BEFORE re-enabling so when SingleEnemyAI.Start() fires
                    // next frame and immediately calls SearchPath(), target is non-null.
                    // If target is null at that point, AIPath sets canSearch=false permanently.
                    Transform initialTarget = GetNearestPlayer();
                    if (_fTarget != null && initialTarget != null)
                        _fTarget.SetValue(_singleEnemyAI, initialTarget);
                    // Re-enable the component (disabled on template to prevent its Start() from
                    // crashing on null singletons; A* is now live so it's safe to run).
                    ((Behaviour)_singleEnemyAI).enabled = true;
                    SetAI(true, ChaseSpeed);
                    _aiReady = true;
                    ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: AIPath pathfinding active");
                }
                else if (_singleEnemyAI != null && !_hasAstar)
                {
                    // Disable AIPath movement; we drive transform directly
                    SetAI(false, 0f);
                    _aiReady = false;
                    ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: no AstarPath — direct movement");
                }
                else
                {
                    _aiReady = false;
                    ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: SingleEnemyAI not found on GO — direct movement");
                }

                // Locate animation components (enemy prefab has these under weaponControl/)
                _bodyAnim = FindAnimation("weaponControl/EnemyAnimation");
                _handAnim = FindAnimation("weaponControl/1_3/handrightup/handright_Animation");
                // Fallback paths (if model root differs)
                if (_bodyAnim == null) _bodyAnim = FindAnimation("GameObject/EnemyAnimation");
                if (_bodyAnim == null) _bodyAnim = FindAnimation("EnemyAnimation");

                if (_bodyAnim != null) _bodyAnim.Play("walkway");
                if (_handAnim != null) _handAnim.Play("walkway");

                // Cache PlayerLogic.PlayerDamage for melee attacks (static, only need once)
                if (_sfPlayerLogicInst == null)
                {
                    try
                    {
                        Type plType = FindType("PlayerLogic");
                        if (plType != null)
                        {
                            _sfPlayerLogicInst = plType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                            _smPlayerDamage    = plType.GetMethod("PlayerDamage", BindingFlags.Public | BindingFlags.Instance,
                                                    null, new Type[] { typeof(int) }, null);
                            ZombieModEntry.Log("ZombieDriver: PlayerLogic reflection cached inst=" + (_sfPlayerLogicInst != null) +
                                " dmg=" + (_smPlayerDamage != null));
                        }
                    }
                    catch (Exception ex) { ZombieModEntry.Log("ZombieDriver: PlayerLogic reflect err: " + ex.Message); }
                }

                _cc = GetComponent<CharacterController>();
                GameObject mt = GameObject.Find("enemyTempAttackTransform" + ZombieId);
                _moveTarget = mt != null ? mt.transform : null;
                _lastMovePos = transform.position;
                _unstickTimer = 0f;
                _unstickDir = Vector3.zero;
                if (_cc != null)
                {
                    _cc.stepOffset = Mathf.Max(_cc.stepOffset, 1.05f);
                    _cc.slopeLimit = Mathf.Max(_cc.slopeLimit, 60f);
                }
                ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: cc=" + (_cc != null) + " aiReady=" + _aiReady + " hasAstar=" + _hasAstar);
                Dbg("Start: cc=" + (_cc != null) + " aiReady=" + _aiReady);

                // Attach debug HUD (billboard text + path line/dots)
                // Keep runtime debug visuals disabled in normal play.
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver.Start err: " + ex.Message); }
        }

        internal void TeleportForRecovery(Vector3 pos)
        {
            try
            {
                if (_cc != null) _cc.enabled = false;
                transform.position = pos;
                if (_cc != null) _cc.enabled = true;
                _path = null;
                _pathIdx = 0;
                _pathTimer = 0f;
                _velY = 0f;
                _stuckTimer = 0f;
                _stuckRecoveries = 0;
                _farFromPlayersTimer = 0f;
                _lastMovePos = pos;
                _cachedTarget = null;
                _targetTimer = 0f;
                if (_moveTarget != null) _moveTarget.position = pos;
            }
            catch (Exception ex) { ZombieModEntry.Log("TeleportForRecovery err: " + ex.Message); }
        }

        internal void ApplySharedHealth(float hp)
        {
            try
            {
                int roundedHp = Mathf.Max(0, Mathf.RoundToInt(hp));
                if (_singleEnemyLogic != null)
                {
                    if (_fLogicBlood != null)
                    {
                        if (_fLogicBlood.FieldType == typeof(float))
                            _fLogicBlood.SetValue(_singleEnemyLogic, (float)roundedHp);
                        else
                            _fLogicBlood.SetValue(_singleEnemyLogic, roundedHp);
                    }
                    if (_fLogicBDied != null) _fLogicBDied.SetValue(_singleEnemyLogic, hp <= 0f);
                }
                ApplyBloodBar(hp);
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver[" + ZombieId + "] ApplySharedHealth err: " + ex.Message); }
        }

        internal bool TryGetSharedHealth(out float hp)
        {
            hp = MaxHealth;
            try
            {
                if (_singleEnemyLogic == null || _fLogicBlood == null) return false;
                object raw = _fLogicBlood.GetValue(_singleEnemyLogic);
                if (raw == null) return false;
                hp = Convert.ToSingle(raw);
                return true;
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("ZombieDriver[" + ZombieId + "] TryGetSharedHealth err: " + ex.Message);
                return false;
            }
        }

        private void ApplyBloodBar(float hp)
        {
            if (_bloodTransform == null)
                _bloodTransform = FindBloodTransform(transform);
            if (_bloodTransform == null) return;

            float num = Mathf.Clamp01(hp / Mathf.Max(1f, MaxHealth));
            Vector3 lp = _bloodTransform.localPosition;
            Vector3 ls = _bloodTransform.localScale;
            _bloodTransform.localPosition = new Vector3((0f - (1f - num)) / 2f, lp.y, lp.z);
            _bloodTransform.localScale = new Vector3(num, ls.y, ls.z);
        }

        private static Transform FindBloodTransform(Transform root)
        {
            if (root == null) return null;
            Transform direct = root.Find("BloodBarSingle/blood");
            if (direct != null) return direct;
            direct = root.Find("BloodBar/blood");
            if (direct != null) return direct;
            return FindBloodTransformRecursive(root);
        }

        private static Transform FindBloodTransformRecursive(Transform t)
        {
            if (t == null) return null;
            string n = t.name ?? "";
            Transform parent = t.parent;
            string pn = parent != null && parent.name != null ? parent.name : "";
            if (string.Equals(n, "blood", StringComparison.OrdinalIgnoreCase) &&
                pn.IndexOf("BloodBar", StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform found = FindBloodTransformRecursive(t.GetChild(i));
                if (found != null) return found;
            }
            return null;
        }

        void Update()
        {
            try
            {
                // Safety guard: if there's no A* navmesh in this scene, keep canSearch/canMove
                // forced to false every frame.  SingleEnemyAI.Start() may reset them to their
                // AIPath defaults (true) if it runs after our Start(); this ensures they stay
                // off before SingleEnemyAI.Update() tries CalculateVelocity() and NPEs on
                // the null AstarPath singleton.
                if (!_aiReady && _singleEnemyAI != null)
                {
                    if (_fCanSearch != null) _fCanSearch.SetValue(_singleEnemyAI, false);
                    if (_fCanMove   != null) _fCanMove.SetValue(_singleEnemyAI, false);
                }

                if (IsZombieDowned())
                    return;

                // Re-scan for the nearest player only a few times per second —
                // scanning does GameObject.Find + reflection, far too heavy per frame.
                _targetTimer -= Time.deltaTime;
                if (_cachedTarget == null || _cachedTarget.Transform == null || _targetTimer <= 0f)
                {
                    _cachedTarget = GetNearestPlayerTarget();
                    _targetTimer = TARGET_REFRESH;
                }
                PlayerTarget target = _cachedTarget;
                if (target == null || target.Transform == null) return;
                Transform playerTr = target.Transform;
                float targetDistance = Vector3.Distance(transform.position, playerTr.position);
                Vector3 cur = transform.position;
                bool invalidPosition = float.IsNaN(cur.x) || float.IsNaN(cur.y) || float.IsNaN(cur.z) ||
                    float.IsInfinity(cur.x) || float.IsInfinity(cur.y) || float.IsInfinity(cur.z) || cur.y < -50f;
                if (invalidPosition && Hook != null && Hook.RelocateZombieForRecovery(this, "out-of-map")) return;
                if (targetDistance > RECYCLE_DISTANCE) _farFromPlayersTimer += Time.deltaTime;
                else _farFromPlayersTimer = 0f;
                if (_farFromPlayersTimer >= RECYCLE_DELAY && Hook != null && Hook.RelocateZombieForRecovery(this, "too-far"))
                {
                    _farFromPlayersTimer = 0f;
                    return;
                }
                Vector3 chasePoint = GetChasePoint(playerTr);

                if (_useCustomNav && ZombieNavGrid.Ready)
                {
                    DriveCustomNav(playerTr, chasePoint);
                }
                else if (_aiReady && _singleEnemyAI != null)
                {
                    // Tell SingleEnemyAI / AIPath to path toward the player.
                    // Also force canSearch=true every frame — AIPath.SearchPath() sets it to
                    // false if target was null, and won't reset it on its own.
                    if (_moveTarget != null)
                    {
                        _moveTarget.position = chasePoint;
                        if (_fTarget != null) _fTarget.SetValue(_singleEnemyAI, _moveTarget);
                    }
                    else if (_fTarget != null) _fTarget.SetValue(_singleEnemyAI, playerTr);
                    if (_fSpeed     != null) _fSpeed.SetValue(_singleEnemyAI, ChaseSpeed);
                    if (_fCanSearch != null) _fCanSearch.SetValue(_singleEnemyAI, true);
                    if (_fCanMove   != null) _fCanMove.SetValue(_singleEnemyAI, true);
                }
                else
                {
                    // Direct movement fallback — use CharacterController so walls are respected.
                    Vector3 dir = chasePoint - transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        dir.Normalize();
                        Vector3 move = dir * ChaseSpeed * Time.deltaTime;
                        move.y -= 9.8f * Time.deltaTime; // gravity
                        if (_cc != null)
                            _cc.Move(move);
                        else
                            transform.position += move;
                        transform.rotation = Quaternion.LookRotation(dir);
                    }
                    else if (_cc != null)
                    {
                        // Still apply gravity even when on top of target
                        _cc.Move(new Vector3(0f, -9.8f * Time.deltaTime, 0f));
                    }
                }

                // Proximity melee attack — playerTr is guaranteed non-null here
                _attackCd -= Time.deltaTime;
                if (_attackCd <= 0f && Hook != null)
                {
                    float dist = Vector3.Distance(transform.position, playerTr.position);
                    if (dist < ATTACK_RANGE)
                    {
                        Hook.ApplyPlayerDamageToPeer(target.PeerId, AttackDamage);
                        _attackCd = AttackCooldown;
                        Dbg("Decision: unarmed-attack target=" + playerTr.name +
                            " damage=" + AttackDamage + " range=" + dist.ToString("F2"));
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver.Update err: " + ex.Message); }
        }

        private Vector3 GetChasePoint(Transform playerTr)
        {
            Vector3 targetPos = playerTr != null ? playerTr.position : transform.position;
            Vector3 delta = targetPos - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.0001f)
                delta = transform.forward;
            if (delta.sqrMagnitude < 0.0001f)
                delta = Vector3.forward;
            delta.Normalize();
            float radius = Mathf.Clamp(ATTACK_RANGE * 0.9f, 0.75f, 1.35f);
            Vector3 point = targetPos - delta * radius;
            point.y = targetPos.y;
            return point;
        }

        private bool IsZombieDowned()
        {
            try
            {
                if (_singleEnemyLogic == null) return false;
                if (_fLogicBDied != null)
                {
                    object died = _fLogicBDied.GetValue(_singleEnemyLogic);
                    if (died is bool && (bool)died) return true;
                }
                if (_fLogicBlood != null)
                {
                    object blood = _fLogicBlood.GetValue(_singleEnemyLogic);
                    if (blood is int && (int)blood <= 0) return true;
                }
            }
            catch { }
            return false;
        }

        // ─── helpers ───────────────────────────────────────────────────────

        private void DriveCustomNav(Transform playerTr, Vector3 chasePoint)
        {
            _pathTimer -= Time.deltaTime;
                if (_unstickTimer > 0f)
                {
                    _unstickTimer -= Time.deltaTime;
                    if (_unstickTimer <= 0f)
                    {
                        _pathTimer = 0f;
                        _path = null;
                    }
                    Vector3 wander = _unstickDir;
                    wander.y = 0f;
                    if (wander.sqrMagnitude < 0.0001f) wander = transform.right;
                    wander.Normalize();
                    Vector3 beforeWander = transform.position;
                    Vector3 wanderMove = wander * (ChaseSpeed * 0.8f) * Time.deltaTime;
                    wanderMove.y -= 9.8f * Time.deltaTime;
                    CollisionFlags wanderFlags = 0;
                    if (_cc != null) wanderFlags = _cc.Move(wanderMove);
                    else transform.position += wanderMove;
                    transform.rotation = Quaternion.LookRotation(wander);
                    DetectStuck(beforeWander, wanderMove, wanderFlags, chasePoint);
                    DbgDecision("unstick-wander dir=" + wander + " remaining=" + _unstickTimer.ToString("F2"));
                    return;
                }

                if (_path == null || _pathTimer <= 0f)
                {
                    Dbg("Decision: repath from=" + transform.position + " target=" + chasePoint +
                        " oldPath=" + (_path != null ? _path.Count : 0));
                _path = ZombieNavGrid.Query(transform.position, chasePoint, IsDebugZombie(), ZombieId);
                _pathIdx = 0;
                _pathTimer = 0.75f;
                Dbg("Decision: repath result points=" + (_path != null ? _path.Count : 0));
            }

            Vector3 wp;
            if (!ZombieNavGrid.GetWaypoint(_path, transform.position, ref _pathIdx, out wp))
            {
                DbgDecision("no-waypoint path=" + (_path != null ? _path.Count : 0) + " idx=" + _pathIdx);
                if (SnapToNav("no-waypoint"))
                    return;
                ApplyGravityOnly();
                return;
            }
            if (_pathIdx != _lastLoggedPathIdx)
            {
                _lastLoggedPathIdx = _pathIdx;
                Dbg("Decision: waypoint idx=" + _pathIdx + " wp=" + wp +
                    " pos=" + transform.position + " heightDelta=" + (wp.y - transform.position.y).ToString("F2"));
            }

            Vector3 dir = new Vector3(wp.x - transform.position.x, 0f, wp.z - transform.position.z);
            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                Vector3 before = transform.position;
                float dy = wp.y - transform.position.y;
                Vector3 move = dir * ChaseSpeed * Time.deltaTime;
                if (dy > 0.08f)
                {
                    _velY = 0f;
                    move.y = Mathf.Min(dy, CLIMB_RATE * Time.deltaTime);
                }
                else
                {
                    _velY -= 9.8f * Time.deltaTime;
                    float fall = Mathf.Max(_velY * Time.deltaTime, -DESCEND_RATE * Time.deltaTime);
                    move.y = Mathf.Clamp(dy, fall, CLIMB_RATE * Time.deltaTime);
                }
                CollisionFlags flags = 0;
                if (_cc != null)
                {
                    flags = _cc.Move(move);
                    if ((flags & CollisionFlags.Below) != 0) _velY = 0f;
                }
                else transform.position += move;
                transform.rotation = Quaternion.LookRotation(dir);
                DetectStuck(before, move, flags, wp);
                DbgDecision("move dir=" + dir + " wp=" + wp + " dy=" + dy.ToString("F2") +
                    " velY=" + _velY.ToString("F2") + " flags=" + ((int)flags));
            }
            else
            {
                DbgDecision("at-waypoint idx=" + _pathIdx + " wp=" + wp);
                ApplyGravityOnly();
            }
        }

        private void ApplyGravityOnly()
        {
            _velY -= 9.8f * Time.deltaTime;
            if (_cc != null)
            {
                CollisionFlags flags = _cc.Move(new Vector3(0f, _velY * Time.deltaTime, 0f));
                if ((flags & CollisionFlags.Below) != 0) _velY = 0f;
            }
            DbgDecision("gravity-only velY=" + _velY.ToString("F2"));
        }

        private void DetectStuck(Vector3 before, Vector3 requestedMove, CollisionFlags flags, Vector3 wp)
        {
            Vector3 after = transform.position;
            Vector3 actual = after - before;
            float actualPlanar = new Vector2(actual.x, actual.z).magnitude;
            float requestedPlanar = new Vector2(requestedMove.x, requestedMove.z).magnitude;
            bool hitSide = (flags & CollisionFlags.Sides) != 0;

            if (requestedPlanar > 0.03f && (actualPlanar < STUCK_MIN_MOVE || hitSide))
                _stuckTimer += Time.deltaTime;
            else
            {
                _stuckTimer = 0f;
                if (actualPlanar > STUCK_MIN_MOVE * 2f) _stuckRecoveries = 0;
            }

            _lastMovePos = after;
            if (_stuckTimer < STUCK_TIME) return;

            Dbg("Decision: stuck recovery idx=" + _pathIdx + " pos=" + after +
                " wp=" + wp + " actualPlanar=" + actualPlanar.ToString("F3") +
                " flags=" + ((int)flags));
            _stuckTimer = 0f;
            _stuckRecoveries++;
            if (_stuckRecoveries >= 3 && Hook != null && Hook.RelocateZombieForRecovery(this, "repeated-stuck"))
            {
                _stuckRecoveries = 0;
                return;
            }
            _pathTimer = 0f;
            _pathIdx = 0;
            _unstickDir = RandomHorizontalDirection();
            _unstickTimer = UnityEngine.Random.Range(1.5f, 2.8f);
            if (!TryShiftMoveTarget(transform.position, _unstickDir))
                SnapToNav("stuck");
        }

        private bool SnapToNav(string reason)
        {
            Vector3 navPos;
            if (!ZombieNavGrid.TrySnapToWalkable(transform.position, 30, out navPos))
                return false;

            Vector3 p = transform.position;
            p.x = navPos.x;
            p.z = navPos.z;
            p.y = RootYForGround(navPos.y);
            transform.position = p;
            _velY = 0f;
            _pathTimer = 0.75f;
            Dbg("Decision: snap-to-nav reason=" + reason + " pos=" + p + " ground=" + navPos.y);
            return true;
        }

        private static Vector3 RandomHorizontalDirection()
        {
            Vector3 v = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            if (v.sqrMagnitude < 0.01f) v = Vector3.right;
            return v.normalized;
        }

        private bool TryShiftMoveTarget(Vector3 origin, Vector3 dir)
        {
            if (_moveTarget == null) return false;
            Vector3 point = origin + dir * UnityEngine.Random.Range(2.0f, 4.0f);
            point.y = origin.y;
            _moveTarget.position = point;
            return true;
        }

        private float RootYForGround(float groundY)
        {
            if (_cc != null)
                return groundY + Mathf.Max(0.05f, _cc.height * 0.5f - _cc.center.y + 0.05f);
            return groundY + 0.1f;
        }

        private bool IsDebugZombie()
        {
            return _debugZombieId == ZombieId;
        }

        private void Dbg(string msg)
        {
            if (IsDebugZombie())
                ZombieModEntry.Log("ZAI[" + ZombieId + "] " + msg);
        }

        private void DbgDecision(string msg)
        {
            if (!IsDebugZombie()) return;
            if (_lastMoveDecision == msg) return;
            _lastMoveDecision = msg;
            // ZombieModEntry.Log("ZAI[" + ZombieId + "] Decision: " + msg);  // silenced — high-frequency spam
        }

        private void SetAI(bool on, float speed)
        {
            if (_fCanSearch != null) _fCanSearch.SetValue(_singleEnemyAI, on);
            if (_fCanMove   != null) _fCanMove.SetValue(_singleEnemyAI, on);
            if (_fSpeed     != null) _fSpeed.SetValue(_singleEnemyAI, speed);
        }

        private static bool CheckAstar()
        {
            Type t = FindType("AstarPath");
            if (t == null) return false;
            FieldInfo fi = t.GetField("active", BindingFlags.Public | BindingFlags.Static);
            return fi != null && fi.GetValue(null) != null;
        }

        private Transform GetNearestPlayer()
        {
            PlayerTarget target = GetNearestPlayerTarget();
            return target != null ? target.Transform : null;
        }

        internal PlayerTarget GetNearestPlayerTarget()
        {
            try
            {
                Vector3 pos = transform.position;
                PlayerTarget best = null;
                float bestSq = float.MaxValue;

                GameObject ec = Hook != null ? Hook.GetLocalPlayerGO() : GameObject.Find("ExampleCharacter");
                string localPeer = Hook != null ? Hook.GetLocalPeerId() : null;
                if (ec != null && (Hook == null || !Hook.IsPeerDowned(localPeer)))
                {
                    float sq = (ec.transform.position - pos).sqrMagnitude;
                    best = new PlayerTarget(ec.transform, localPeer, sq);
                    bestSq = sq;
                }

                AddRemotePlayerTargets(pos, ref best, ref bestSq);

                if (best == null || best.Transform == null)
                    return null;
                return best;
            }
            catch (Exception ex) { ZombieModEntry.Log("GetNearestPlayerTarget err: " + ex.Message); return null; }
        }

        private void AddRemotePlayerTargets(Vector3 zombiePos, ref PlayerTarget best, ref float bestSq)
        {
            try
            {
                if (Hook == null) return;
                if (_sMgrType == null)
                {
                    _sMgrType = FindType("CNRMultiplayerManager");
                    if (_sMgrType == null) return;
                    BindingFlags f = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                    _sfMgrInst      = _sMgrType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                    _sfOtherObjects = _sMgrType.GetField("otherPlayerObject", f);
                    _sfOtherInfos   = _sMgrType.GetField("otherPlayersInfoList", f);
                }
                object mgrInst = _sfMgrInst != null ? _sfMgrInst.GetValue(null) : null;
                if (mgrInst == null) return;

                Array objects = _sfOtherObjects != null ? _sfOtherObjects.GetValue(mgrInst) as Array : null;
                Array infos = _sfOtherInfos != null ? _sfOtherInfos.GetValue(mgrInst) as Array : null;
                if (objects == null || infos == null) return;

                int count = Math.Min(objects.Length, infos.Length);
                for (int i = 0; i < count; i++)
                {
                    GameObject go = objects.GetValue(i) as GameObject;
                    object info = infos.GetValue(i);
                    if (go == null || info == null) continue;

                    if (_sfInfoId == null)
                        _sfInfoId = info.GetType().GetField("mId",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    string peerId = _sfInfoId != null ? Convert.ToString(_sfInfoId.GetValue(info)) : null;
                    if (string.IsNullOrEmpty(peerId) || peerId == "null") continue;
                    if (Hook.IsPeerDowned(peerId)) continue;

                    float sq = (go.transform.position - zombiePos).sqrMagnitude;
                    if (sq < bestSq)
                    {
                        bestSq = sq;
                        best = new PlayerTarget(go.transform, peerId, sq);
                    }
                }
            }
            catch (Exception ex)
            {
                ZombieModEntry.Log("AddRemotePlayerTargets err: " + ex.Message);
            }
        }

        private Animation FindAnimation(string path)
        {
            Transform t = transform.Find(path);
            return t != null ? t.gameObject.GetComponent<Animation>() : null;
        }

        private static Type FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            { Type t = asm.GetType(name); if (t != null) return t; }
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE PROXY — on the enemy GO on non-master clients.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieProxy : MonoBehaviour
    {
        public byte ZombieId;
        public float MaxHealth = 100f;
        public ZombieHook Hook;
        public int AppliedVariant = -1;  // last skin variant applied to this GO

        private float _tx, _ty, _tz, _tRotY;
        private bool  _hasTarget;

        private Animation _bodyAnim;
        private Animation _handAnim;
        private Component _singleEnemyLogic;
        private FieldInfo _fLogicBlood;
        private FieldInfo _fLogicBDied;
        private Transform _bloodTransform;
        private bool _reportedLocalDeath;
        private int _lastDamageReportFrame = -1;

        private const float INTERP_K = 7f;
        void Start()
        {
            _bodyAnim = FindAnim("weaponControl/EnemyAnimation");
            _handAnim = FindAnim("weaponControl/1_3/handrightup/handright_Animation");
            if (_bodyAnim == null) _bodyAnim = FindAnim("GameObject/EnemyAnimation");
            if (_bodyAnim == null) _bodyAnim = FindAnim("EnemyAnimation");

            if (_bodyAnim != null) _bodyAnim.Play("walkway");
            if (_handAnim != null) _handAnim.Play("walkway");

            try
            {
                Type logicType = FindType("SingleEnemyLogic");
                if (logicType != null)
                {
                    _singleEnemyLogic = GetComponent(logicType) ?? GetComponentInChildren(logicType);
                }
                if (_singleEnemyLogic == null)
                {
                    logicType = FindType("enemyLogic");
                    if (logicType != null)
                        _singleEnemyLogic = GetComponent(logicType) ?? GetComponentInChildren(logicType);
                }
                if (logicType != null)
                {
                    BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                    _fLogicBlood = logicType.GetField("blood", flags) ??
                                   logicType.GetField("Blood", flags);
                    _fLogicBDied = logicType.GetField("bDied", flags) ??
                                  logicType.GetField("bDead", flags);
                    ZombieModEntry.Log("ZombieProxy[" + ZombieId + "]: logicType=" + logicType.Name +
                        " logic=" + (_singleEnemyLogic != null) + " bloodField=" + (_fLogicBlood != null) +
                        " diedField=" + (_fLogicBDied != null));
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieProxy[" + ZombieId + "] logic reflect err: " + ex.Message); }
            _bloodTransform = FindBloodTransform(transform);
            ApplySharedHealth(MaxHealth);
        }

        public void SetTarget(float x, float y, float z, float rotY)
        {
            if (!_hasTarget)
            {
                transform.position = new Vector3(x, y, z);
                transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                _hasTarget = true;
            }
            else
            {
                // Teleport instead of sliding when the target jumped far
                // (id reuse, respawn, or a long packet gap).
                float dx = x - transform.position.x;
                float dz = z - transform.position.z;
                if (dx * dx + dz * dz > 64f)  // > 8 m
                {
                    transform.position = new Vector3(x, y, z);
                    transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                }
            }
            _tx = x; _ty = y; _tz = z; _tRotY = rotY;
        }

        void Update()
        {
            if (!_hasTarget) return;

            float   t = Mathf.Min(1f, INTERP_K * Time.deltaTime);
            Vector3 p = transform.position;
            transform.position = Vector3.Lerp(p, new Vector3(_tx, _ty, _tz), t);
            transform.rotation = Quaternion.Euler(0f,
                Mathf.LerpAngle(transform.eulerAngles.y, _tRotY, t), 0f);
        }

        void OnDestroy()
        {
            if (_reportedLocalDeath || Hook == null || !_hasTarget) return;
            if (IsLocallyDead())
                ReportLocalDeath("destroyed-dead");
        }

        private void ReportLocalDeath(string reason)
        {
            if (_reportedLocalDeath || Hook == null) return;
            _reportedLocalDeath = true;
            ZombieModEntry.Log("ZombieProxy[" + ZombieId + "]: local death detected reason=" + reason +
                " peer=" + (Hook.GetLocalPeerId() ?? "null"));
            Hook.ReportZombieDamage(ZombieId, 9999);
            try { UnityEngine.Object.Destroy(gameObject); } catch { }
        }

        public void decreaseBlood(int nblood)
        {
            ReportDamage(nblood, "decreaseBlood");
        }

        public void OnDamaged(int damage)
        {
            ReportDamage(damage, "OnDamaged");
        }

        public void ReceiveDamage(int damageBlood)
        {
            ReportDamage(damageBlood, "ReceiveDamage");
        }

        private void ReportDamage(int damage, string source)
        {
            if (Hook == null || damage <= 0) return;
            if (_lastDamageReportFrame == Time.frameCount) return;
            _lastDamageReportFrame = Time.frameCount;
            ZombieModEntry.Log("ZombieProxy[" + ZombieId + "]: damage source=" + source + " dmg=" + damage);
            Hook.ReportZombieDamage(ZombieId, damage);
        }

        internal void ApplySharedHealth(float hp)
        {
            try
            {
                int roundedHp = Mathf.Max(0, Mathf.RoundToInt(hp));
                if (_singleEnemyLogic != null)
                {
                    if (_fLogicBlood != null)
                    {
                        if (_fLogicBlood.FieldType == typeof(float))
                            _fLogicBlood.SetValue(_singleEnemyLogic, (float)roundedHp);
                        else
                            _fLogicBlood.SetValue(_singleEnemyLogic, roundedHp);
                    }
                    if (_fLogicBDied != null) _fLogicBDied.SetValue(_singleEnemyLogic, hp <= 0f);
                }
                ApplyBloodBar(hp);
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieProxy[" + ZombieId + "] ApplySharedHealth err: " + ex.Message); }
        }

        private void ApplyBloodBar(float hp)
        {
            if (_bloodTransform == null)
                _bloodTransform = FindBloodTransform(transform);
            if (_bloodTransform == null) return;

            float num = Mathf.Clamp01(hp / Mathf.Max(1f, MaxHealth));
            Vector3 lp = _bloodTransform.localPosition;
            Vector3 ls = _bloodTransform.localScale;
            _bloodTransform.localPosition = new Vector3((0f - (1f - num)) / 2f, lp.y, lp.z);
            _bloodTransform.localScale = new Vector3(num, ls.y, ls.z);
        }

        private static Transform FindBloodTransform(Transform root)
        {
            if (root == null) return null;
            Transform direct = root.Find("BloodBarSingle/blood");
            if (direct != null) return direct;
            direct = root.Find("BloodBar/blood");
            if (direct != null) return direct;
            return FindBloodTransformRecursive(root);
        }

        private static Transform FindBloodTransformRecursive(Transform t)
        {
            if (t == null) return null;
            string n = t.name ?? "";
            Transform parent = t.parent;
            string pn = parent != null && parent.name != null ? parent.name : "";
            if (string.Equals(n, "blood", StringComparison.OrdinalIgnoreCase) &&
                pn.IndexOf("BloodBar", StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform found = FindBloodTransformRecursive(t.GetChild(i));
                if (found != null) return found;
            }
            return null;
        }

        private bool IsLocallyDead()
        {
            try
            {
                if (_singleEnemyLogic == null) return false;
                if (_fLogicBDied != null)
                {
                    object died = _fLogicBDied.GetValue(_singleEnemyLogic);
                    if (died is bool && (bool)died) return true;
                }
                if (_fLogicBlood != null)
                {
                    object blood = _fLogicBlood.GetValue(_singleEnemyLogic);
                    if (blood is int && (int)blood <= 0) return true;
                    if (blood is float && (float)blood <= 0f) return true;
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieProxy[" + ZombieId + "] IsLocallyDead err: " + ex.Message); }
            return false;
        }

        private Animation FindAnim(string path)
        {
            Transform t = transform.Find(path);
            return t != null ? t.gameObject.GetComponent<Animation>() : null;
        }

        private static Type FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            { Type t = asm.GetType(name); if (t != null) return t; }
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE DEBUG HUD — billboarded text + path visualiser (master client only)
    // Shows above each enemy:
    //   • surface collider name + Y under feet
    //   • whether that surface node is walkable
    //   • target position
    //   • canSearch / canMove / path waypoint count
    // Also draws dots + lines along the current A* vector path.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieDebugHUD : MonoBehaviour
    {
        public Component SrcAI;  // the SingleEnemyAI / AIPath component

        // reflected fields we read each frame
        private FieldInfo _fTarget;
        private FieldInfo _fCanSearch;
        private FieldInfo _fCanMove;
        private FieldInfo _fPath;       // AIPath.path (Pathfinding.Path)
        private FieldInfo _fVectorPath; // Path.vectorPath (List<Vector3>)
        private bool      _reflected;

        // path-dot GameObjects pool
        private readonly List<GameObject> _dots = new List<GameObject>();
        private readonly List<GameObject> _lines = new List<GameObject>();
        private Material  _dotMat;
        private Material  _lineMat;

        void Start()
        {
            try
            {
                if (SrcAI == null) return;
                Type aiType = SrcAI.GetType();
                // walk up to AIPath base if needed
                _fTarget    = GetField(aiType, "target");
                _fCanSearch = GetField(aiType, "canSearch");
                _fCanMove   = GetField(aiType, "canMove");
                _fPath      = GetField(aiType, "path");

                if (_fPath != null)
                {
                    object pathObj = _fPath.GetValue(SrcAI);
                    Type pathType  = pathObj != null ? pathObj.GetType() : null;
                    if (pathType == null)
                    {
                        // resolve from assemblies
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        { var t2 = asm.GetType("Pathfinding.Path"); if (t2 != null) { pathType = t2; break; } }
                    }
                    if (pathType != null)
                        _fVectorPath = pathType.GetField("vectorPath", BindingFlags.Public | BindingFlags.Instance);
                }

                Shader sh = Shader.Find("Particles/Additive")
                         ?? Shader.Find("Legacy Shaders/Particles/Additive")
                         ?? Shader.Find("Unlit/Color");
                _dotMat  = new Material(sh);
                _dotMat.SetInt("_ZTest", 8);  // 8 = CompareFunction.Always → renders through walls
                _dotMat.renderQueue = 5000;
                _lineMat = new Material(sh);
                _lineMat.SetInt("_ZTest", 8);
                _lineMat.renderQueue = 5000;
                _reflected = true;
            }
            catch { }
        }

        private static FieldInfo GetField(Type t, string name)
        {
            while (t != null)
            {
                FieldInfo fi = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null) return fi;
                t = t.BaseType;
            }
            return null;
        }

        void Update()
        {
            if (!_reflected || SrcAI == null) { ClearDots(); return; }
            try
            {
                // Rebuild path visualisation
                List<Vector3> waypoints = null;
                if (_fPath != null)
                {
                    object pathObj = _fPath.GetValue(SrcAI);
                    if (pathObj != null && _fVectorPath != null)
                        waypoints = _fVectorPath.GetValue(pathObj) as List<Vector3>;
                }
                UpdatePathVis(waypoints);
            }
            catch { ClearDots(); }
        }

        void OnGUI()
        {
            if (!_reflected || SrcAI == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos = transform.position + Vector3.up * 2.0f;
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z <= 0f) return;

            // ── gather data ──────────────────────────────────────────────
            bool canSearch = _fCanSearch != null && (bool)_fCanSearch.GetValue(SrcAI);
            bool canMove   = _fCanMove   != null && (bool)_fCanMove.GetValue(SrcAI);

            Transform tgt = _fTarget != null ? _fTarget.GetValue(SrcAI) as Transform : null;
            string tgtStr = tgt != null
                ? string.Format("({0:F1},{1:F1},{2:F1})", tgt.position.x, tgt.position.y, tgt.position.z)
                : "null";

            // surface under feet
            Vector3 feet = transform.position;
            string surfStr  = "none";
            float  surfY    = feet.y;
            bool   walkable = false;
            RaycastHit hit;
            if (Physics.Raycast(feet + Vector3.up * 0.1f, Vector3.down, out hit, 2f))
            {
                surfStr  = hit.collider.name;
                surfY    = hit.point.y;

                // ask A* if the node at feet is walkable
                try
                {
                    Type astarType = null;
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    { var tt = asm.GetType("AstarPath"); if (tt != null) { astarType = tt; break; } }
                    if (astarType != null)
                    {
                        FieldInfo activeF = astarType.GetField("active", BindingFlags.Public | BindingFlags.Static);
                        object    active  = activeF != null ? activeF.GetValue(null) : null;
                        if (active != null)
                        {
                            MethodInfo getNear = astarType.GetMethod("GetNearest",
                                new[] { typeof(Vector3) });
                            if (getNear != null)
                            {
                                object nr = getNear.Invoke(active, new object[] { feet });
                                if (nr != null)
                                {
                                    // NNInfo.node
                                    FieldInfo nodeF = nr.GetType().GetField("node",
                                        BindingFlags.Public | BindingFlags.Instance);
                                    object node = nodeF != null ? nodeF.GetValue(nr) : null;
                                    if (node != null)
                                    {
                                        PropertyInfo walkableProp = node.GetType().GetProperty("Walkable",
                                            BindingFlags.Public | BindingFlags.Instance);
                                        if (walkableProp != null)
                                            walkable = (bool)walkableProp.GetValue(node, null);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            int wpCount = 0;
            try
            {
                if (_fPath != null)
                {
                    object pathObj = _fPath.GetValue(SrcAI);
                    if (pathObj != null && _fVectorPath != null)
                    {
                        var vp = _fVectorPath.GetValue(pathObj) as System.Collections.IList;
                        if (vp != null) wpCount = vp.Count;
                    }
                }
            }
            catch { }

            // ── render HUD ───────────────────────────────────────────────
            float hudScale = Mathf.Max(1f, Screen.height / 720f);
            float gx = sp.x - 80f * hudScale;
            float gy = Screen.height - sp.y - 70f * hudScale;
            float W = 160f * hudScale, LH = 14f * hudScale;

            GUIStyle bg = new GUIStyle(GUI.skin.label);
            bg.normal.background = Texture2D.whiteTexture;
            bg.normal.textColor  = Color.black;
            bg.fontSize = (int)(10f * hudScale);
            bg.padding  = new RectOffset(2, 2, 1, 1);

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(gx - 2f, gy - 2f, W + 4f, LH * 5f + 4f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            string walkStr = walkable ? "<color=#00ff00>WALK</color>" : "<color=#ff4444>BLOCK</color>";
            GUIStyle rich = new GUIStyle(bg);
            rich.richText = true;
            rich.normal.background = null;
            rich.normal.textColor  = Color.white;
            rich.fontSize = (int)(10f * hudScale);

            GUI.Label(new Rect(gx, gy,           W, LH), "surf: " + surfStr + " y=" + surfY.ToString("F2") + " " + walkStr, rich);
            GUI.Label(new Rect(gx, gy + LH,      W, LH), "tgt: " + tgtStr, rich);
            GUI.Label(new Rect(gx, gy + LH * 2f, W, LH),
                "search:" + (canSearch ? "<color=#00ff00>Y</color>" : "<color=#ff4444>N</color>") +
                " move:" + (canMove ? "<color=#00ff00>Y</color>" : "<color=#ff4444>N</color>"), rich);
            GUI.Label(new Rect(gx, gy + LH * 3f, W, LH), "waypts:" + wpCount, rich);
            GUI.Label(new Rect(gx, gy + LH * 4f, W, LH),
                string.Format("pos:({0:F1},{1:F1},{2:F1})", feet.x, feet.y, feet.z), rich);
        }

        // ── path visualisation ─────────────────────────────────────────────

        private void UpdatePathVis(List<Vector3> waypoints)
        {
            int needed = waypoints != null ? waypoints.Count : 0;

            // Grow dot pool
            while (_dots.Count < needed)
            {
                GameObject d = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                d.name = "ZPathDot";
                UnityEngine.Object.Destroy(d.GetComponent<Collider>());
                d.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                Renderer r = d.GetComponent<Renderer>();
                if (r != null && _dotMat != null) r.material = _dotMat;
                _dots.Add(d);
            }
            // Grow line pool (segment between consecutive waypoints = thin cylinder)
            while (_lines.Count < needed - 1)
            {
                GameObject l = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                l.name = "ZPathLine";
                UnityEngine.Object.Destroy(l.GetComponent<Collider>());
                Renderer r = l.GetComponent<Renderer>();
                if (r != null && _lineMat != null) r.material = _lineMat;
                _lines.Add(l);
            }

            // Position / activate dots
            for (int i = 0; i < _dots.Count; i++)
            {
                bool active = i < needed;
                _dots[i].SetActive(active);
                if (active)
                {
                    _dots[i].transform.position = waypoints[i] + Vector3.up * 0.05f;
                    Renderer r = _dots[i].GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.material.color = (i == 0)
                            ? new Color(0f, 1f, 0f)   // green = start
                            : new Color(1f, 0.6f, 0f); // orange = waypoints
                    }
                }
            }

            // Position / activate line segments
            for (int i = 0; i < _lines.Count; i++)
            {
                bool active = i < needed - 1;
                _lines[i].SetActive(active);
                if (active)
                {
                    Vector3 a = waypoints[i]     + Vector3.up * 0.05f;
                    Vector3 b = waypoints[i + 1] + Vector3.up * 0.05f;
                    Vector3 mid = (a + b) * 0.5f;
                    float   len = Vector3.Distance(a, b);
                    _lines[i].transform.position   = mid;
                    _lines[i].transform.up         = (b - a).normalized;
                    _lines[i].transform.localScale  = new Vector3(0.06f, len * 0.5f, 0.06f);
                    Renderer r = _lines[i].GetComponent<Renderer>();
                    if (r != null) r.material.color = new Color(1f, 1f, 0f, 0.8f); // yellow
                }
            }
        }

        private void ClearDots()
        {
            foreach (var d in _dots)  if (d != null) UnityEngine.Object.Destroy(d);
            foreach (var l in _lines) if (l != null) UnityEngine.Object.Destroy(l);
            _dots.Clear();
            _lines.Clear();
        }

        void OnDestroy() { ClearDots(); }
    }

    internal sealed class PlayerTarget
    {
        public readonly Transform Transform;
        public readonly string PeerId;
        public readonly float DistanceSq;

        public PlayerTarget(Transform transform, string peerId, float distanceSq)
        {
            Transform = transform;
            PeerId = peerId;
            DistanceSq = distanceSq;
        }
    }
}
