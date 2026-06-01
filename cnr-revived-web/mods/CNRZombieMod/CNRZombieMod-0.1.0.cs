// CNRZombieMod.cs — v0.1.0
// Zombie multiplayer test mod for Cops N Robbers.
//
// Architecture:
//   Master client only: runs simple chase/wander AI for N zombies, broadcasts
//     their positions via Photon RaiseEvent (event code 198) every ~0.3 s.
//   All clients: receive zombie state → create/update local ZombieProxy GOs
//     (green capsule placeholders) that interpolate smoothly to received positions.
//
// No A*/NavMesh required — movement is direct Vector3 with a downward raycast
// for ground-snapping.  Pathfinding can be added later once the scene graph
// is understood.
//
// Entry point: ZombieModEntry.Load() — found by CNRMod's DLL scanner.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ExitGames.Client.Photon;
using UnityEngine;

namespace CNRZombieMod
{
    // ─────────────────────────────────────────────────────────────────────────
    // ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieModEntry
    {
        public const  string Version      = "0.1.0";
        public const  byte   ZOMBIE_EVENT = 198;   // Photon custom event code (must differ from CNRMod's 199)
        private const string LogPath      = "/storage/emulated/0/CNRMods/zombiemod.log";

        public static void Load()
        {
            try
            {
                var go = new GameObject("CNRZombieMod_Root");
                go.AddComponent<ZombieHook>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                Log("=== CNRZombieMod v" + Version + " loaded ===");

                // Register with CNRMod so it appears in the mod list
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
            try { File.AppendAllText(LogPath,
                      "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[ZombieMod] " + msg); } catch { }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PHOTON EVENT PROXY
    // Wraps whichever listener is currently installed as
    //   NetworkingPeer.externalListener (may already be CNRMod's proxy).
    // Intercepts event code 198; all other traffic is forwarded unchanged.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombiePhotonProxy : IPhotonPeerListener
    {
        private readonly IPhotonPeerListener _orig;
        private readonly ZombieHook          _hook;

        public ZombiePhotonProxy(IPhotonPeerListener orig, ZombieHook hook)
        {
            _orig = orig;
            _hook = hook;
        }

        public void OnEvent(EventData ev)
        {
            if (ev.Code == ZombieModEntry.ZOMBIE_EVENT && _hook != null)
                _hook.OnZombieEvent(ev);
            _orig.OnEvent(ev);
        }

        public void DebugReturn(DebugLevel level, string message)
        { _orig.DebugReturn(level, message); }

        public void OnOperationResponse(OperationResponse resp)
        { _orig.OnOperationResponse(resp); }

        public void OnStatusChanged(StatusCode code)
        { _orig.OnStatusChanged(code); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE HOOK — central MonoBehaviour on persistent root GO
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieHook : MonoBehaviour
    {
        // ── Tunables ─────────────────────────────────────────────────────────
        private const int   ZOMBIE_COUNT    = 5;
        private const float CHASE_SPEED     = 2.8f;   // m/s
        private const float WANDER_SPEED    = 1.2f;   // m/s
        private const float CHASE_RANGE     = 35f;    // metres — start chasing below this dist
        private const float WANDER_CHANGE   = 4.5f;   // seconds between new wander direction
        private const float BROADCAST_SECS  = 0.30f;  // position sync interval (master → clients)
        private const float GRAVITY         = 12f;    // fake gravity when no ground below
        private const int   STRIDE          = 5;      // floats per zombie: id, x, y, z, rotY

        // ── Zombie state (master client only) ─────────────────────────────────
        private struct ZData
        {
            public byte  id;
            public float x, y, z;
            public float rotY;
            public float wanderAngle;
            public float wanderTimer;
            public float velY;
        }

        private ZData[] _z;
        private bool    _masterSpawned;
        private float   _broadcastTimer;

        // ── Per-client proxy map ───────────────────────────────────────────────
        private readonly Dictionary<byte, ZombieProxy> _proxies
            = new Dictionary<byte, ZombieProxy>();

        // ── Photon proxy install state ─────────────────────────────────────────
        private bool   _proxyInstalled;
        private object _lastPeer;

        // ── HUD message ───────────────────────────────────────────────────────
        private string _hud = "";

        // ─────────────────────────────────────────────────────────────────────
        // Unity Messages
        // ─────────────────────────────────────────────────────────────────────

        void Start()
        {
            TryInstallProxy();
        }

        void OnLevelWasLoaded(int level)
        {
            _masterSpawned  = false;
            ClearProxies();
            _hud = "";
        }

        void OnLeftRoom()
        {
            _masterSpawned = false;
            ClearProxies();
        }

        void OnDisconnectedFromPhoton()
        {
            _masterSpawned  = false;
            _proxyInstalled = false;
            _lastPeer       = null;
            ClearProxies();
        }

        void Update()
        {
            // Re-check proxy whenever the networkingPeer instance changes
            object peer = GetNetworkingPeer();
            if (!ReferenceEquals(peer, _lastPeer))
            {
                _proxyInstalled = false;
                _lastPeer       = peer;
            }
            if (!_proxyInstalled) TryInstallProxy();

            // Only run in multiplayer game scenes while in a Photon room
            string scene = Application.loadedLevelName;
            if (!IsGameScene(scene)) return;
            if (!IsInRoom())         return;

            if (IsMasterClient())
            {
                if (!_masterSpawned)
                {
                    // Wait until the local player GO exists before spawning
                    if (GameObject.Find("ExampleCharacter") != null)
                        SpawnZombies();
                }
                else
                {
                    RunAI();
                    _broadcastTimer -= Time.deltaTime;
                    if (_broadcastTimer <= 0f)
                    {
                        _broadcastTimer = BROADCAST_SECS;
                        Broadcast();
                    }
                }
            }
        }

        void OnGUI()
        {
            if (!string.IsNullOrEmpty(_hud))
                GUI.Label(new Rect(8f, 150f, 320f, 24f), _hud);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: Spawn zombie records at positions around the local player
        // ─────────────────────────────────────────────────────────────────────
        private void SpawnZombies()
        {
            _masterSpawned = true;
            GameObject ec = GameObject.Find("ExampleCharacter");
            float ox = ec != null ? ec.transform.position.x : 0f;
            float oy = ec != null ? ec.transform.position.y : 0f;
            float oz = ec != null ? ec.transform.position.z : 0f;

            _z = new ZData[ZOMBIE_COUNT];
            for (int i = 0; i < ZOMBIE_COUNT; i++)
            {
                float angle = (360f / ZOMBIE_COUNT) * i * Mathf.Deg2Rad;
                _z[i].id          = (byte)(i + 1);
                _z[i].x           = ox + Mathf.Sin(angle) * 9f;
                _z[i].y           = oy;
                _z[i].z           = oz + Mathf.Cos(angle) * 9f;
                _z[i].rotY        = UnityEngine.Random.Range(0f, 360f);
                _z[i].wanderAngle = UnityEngine.Random.Range(0f, 360f);
                _z[i].wanderTimer = UnityEngine.Random.Range(0f, WANDER_CHANGE);
                _z[i].velY        = 0f;
            }

            ZombieModEntry.Log("Master: spawned " + ZOMBIE_COUNT
                               + " zombies at (" + ox + "," + oy + "," + oz + ")");
            _broadcastTimer = 0f;   // trigger immediate broadcast
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: per-frame AI loop
        // ─────────────────────────────────────────────────────────────────────
        private void RunAI()
        {
            float dt = Time.deltaTime;
            List<Vector3> targets = GetPlayerPositions();

            for (int i = 0; i < _z.Length; i++)
            {
                ZData z = _z[i];

                // Find nearest player (horizontal distance)
                Vector3 zPos    = new Vector3(z.x, z.y, z.z);
                Vector3 nearest = NearestPos(zPos, targets);
                float   dist    = Vector3.Distance(zPos, nearest);
                bool    chasing = dist < CHASE_RANGE;

                float dx = 0f, dz = 0f;

                if (chasing)
                {
                    float nx  = nearest.x - z.x;
                    float nz2 = nearest.z - z.z;
                    float len = Mathf.Sqrt(nx * nx + nz2 * nz2);
                    if (len > 0.5f)
                    {
                        nx /= len; nz2 /= len;
                        dx      = nx;
                        dz      = nz2;
                        z.rotY  = Mathf.Atan2(nx, nz2) * Mathf.Rad2Deg;
                    }
                }
                else
                {
                    z.wanderTimer -= dt;
                    if (z.wanderTimer <= 0f)
                    {
                        z.wanderTimer = WANDER_CHANGE;
                        z.wanderAngle = UnityEngine.Random.Range(0f, 360f);
                        z.rotY        = z.wanderAngle;
                    }
                    float wa = z.wanderAngle * Mathf.Deg2Rad;
                    dx = Mathf.Sin(wa);
                    dz = Mathf.Cos(wa);
                }

                float speed = chasing ? CHASE_SPEED : WANDER_SPEED;
                z.x += dx * speed * dt;
                z.z += dz * speed * dt;

                // Ground-snap: cast downward from 2 m above current position
                RaycastHit hit;
                Vector3 castOrigin = new Vector3(z.x, z.y + 2f, z.z);
                if (Physics.Raycast(castOrigin, Vector3.down, out hit, 12f))
                {
                    z.y    = Mathf.Lerp(z.y, hit.point.y, dt * 10f);
                    z.velY = 0f;
                }
                else
                {
                    z.velY -= GRAVITY * dt;
                    z.y    += z.velY * dt;
                }

                _z[i] = z;

                // Update master's own visual proxy directly (no interpolation lag)
                ZombieProxy p = GetOrCreateProxy(z.id);
                p.SetImmediate(z.x, z.y, z.z, z.rotY);
            }

            _hud = "[ZombieMod] Master — " + ZOMBIE_COUNT + " zombies running";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: pack and RaiseEvent to all other clients
        // Format: float[] { id, x, y, z, rotY,  id, x, y, z, rotY, ... }
        //   wrapped in Hashtable["zd"] so Photon's serializer handles it.
        // ─────────────────────────────────────────────────────────────────────
        private void Broadcast()
        {
            if (_z == null) return;

            float[] data = new float[_z.Length * STRIDE];
            for (int i = 0; i < _z.Length; i++)
            {
                int b       = i * STRIDE;
                data[b]     = _z[i].id;
                data[b + 1] = _z[i].x;
                data[b + 2] = _z[i].y;
                data[b + 3] = _z[i].z;
                data[b + 4] = _z[i].rotY;
            }

            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["zd"] = data;

            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                // OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliably, byte channelId)
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] {
                        typeof(byte),
                        typeof(System.Collections.Hashtable),
                        typeof(bool),
                        typeof(byte)
                    });
                if (raise != null)
                    raise.Invoke(peer, new object[] { ZombieModEntry.ZOMBIE_EVENT, ht, false, (byte)0 });
                else
                    ZombieModEntry.Log("Broadcast: OpRaiseEvent(Hashtable) overload not found");
            }
            catch (Exception ex) { ZombieModEntry.Log("Broadcast err: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // All clients: receive zombie state from Photon proxy
        // ─────────────────────────────────────────────────────────────────────
        public void OnZombieEvent(EventData ev)
        {
            try
            {
                // Master already updates its proxies directly in RunAI()
                if (IsMasterClient()) return;

                if (!ev.Parameters.ContainsKey((byte)245)) return;
                System.Collections.Hashtable ht =
                    ev.Parameters[(byte)245] as System.Collections.Hashtable;
                if (ht == null || !ht.ContainsKey("zd")) return;

                float[] data = ht["zd"] as float[];
                if (data == null) return;

                int count = data.Length / STRIDE;
                for (int i = 0; i < count; i++)
                {
                    int  b  = i * STRIDE;
                    byte id = (byte)(int)data[b];
                    ZombieProxy p = GetOrCreateProxy(id);
                    p.SetTarget(data[b + 1], data[b + 2], data[b + 3], data[b + 4]);
                }

                _hud = "[ZombieMod] Client — " + count + " zombies synced";
            }
            catch (Exception ex) { ZombieModEntry.Log("OnZombieEvent err: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private ZombieProxy GetOrCreateProxy(byte id)
        {
            ZombieProxy p;
            if (_proxies.TryGetValue(id, out p) && p != null && p.gameObject != null)
                return p;

            // Spawn a green capsule as a placeholder visual
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "ZombieProxy_" + id;

            // Tint it green
            try { go.renderer.material.color = new Color(0.1f, 0.75f, 0.1f, 1f); }
            catch { }

            // Remove collider so we don't block bullets / player movement
            Collider col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);

            p          = go.AddComponent<ZombieProxy>();
            p.ZombieId = id;
            _proxies[id] = p;
            return p;
        }

        private void ClearProxies()
        {
            foreach (var kv in _proxies)
                if (kv.Value != null && kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            _proxies.Clear();
            _z = null;
        }

        private List<Vector3> GetPlayerPositions()
        {
            List<Vector3> list = new List<Vector3>();

            // Local player
            GameObject ec = GameObject.Find("ExampleCharacter");
            if (ec != null) list.Add(ec.transform.position);

            // Remote players via NetPlayerController
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("NetPlayerController");
                if (t == null) continue;
                UnityEngine.Object[] objs = FindObjectsOfType(t);
                for (int i = 0; i < objs.Length; i++)
                {
                    Component c = objs[i] as Component;
                    if (c != null) list.Add(c.transform.position);
                }
                break;
            }

            // Fallback: use world origin so zombies aren't NaN
            if (list.Count == 0) list.Add(Vector3.zero);
            return list;
        }

        private Vector3 NearestPos(Vector3 from, List<Vector3> targets)
        {
            Vector3 best   = targets[0];
            float   bestSq = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                float sq = (from - targets[i]).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = targets[i]; }
            }
            return best;
        }

        // Accepts any scene whose name starts with "FreeRun" or "CRScene"
        // (covers both vanilla and custom-map suffixed scenes like "FreeRun5_1")
        private static bool IsGameScene(string name)
        {
            return name.StartsWith("FreeRun") || name.StartsWith("CRScene");
        }

        private bool IsInRoom()
        {
            try
            {
                Type pnt = GetPNType();
                if (pnt == null) return false;
                PropertyInfo pi = pnt.GetProperty("inRoom",
                    BindingFlags.Public | BindingFlags.Static);
                return pi != null && (bool)pi.GetValue(null, null);
            }
            catch { return false; }
        }

        private bool IsMasterClient()
        {
            try
            {
                Type pnt = GetPNType();
                if (pnt == null) return false;
                PropertyInfo pi = pnt.GetProperty("isMasterClient",
                    BindingFlags.Public | BindingFlags.Static);
                return pi != null && (bool)pi.GetValue(null, null);
            }
            catch { return false; }
        }

        private Type GetPNType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("PhotonNetwork");
                if (t != null) return t;
            }
            return null;
        }

        private object GetNetworkingPeer()
        {
            try
            {
                Type pnt = GetPNType();
                if (pnt == null) return null;
                FieldInfo fi = pnt.GetField("networkingPeer",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                return fi != null ? fi.GetValue(null) : null;
            }
            catch { return null; }
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

                IPhotonPeerListener current = lf.GetValue(peer) as IPhotonPeerListener;
                if (current == null) return;

                // Already wrapped — nothing to do
                if (current is ZombiePhotonProxy) { _proxyInstalled = true; return; }

                lf.SetValue(peer, new ZombiePhotonProxy(current, this));
                _proxyInstalled = true;
                ZombieModEntry.Log("Proxy installed, wrapping " + current.GetType().Name);
            }
            catch (Exception ex) { ZombieModEntry.Log("TryInstallProxy err: " + ex.Message); }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE PROXY — one instance per zombie on every client
    // Receives target position from ZombieHook and interpolates toward it.
    // Also draws a small world-space label so you can see which zombie is which.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieProxy : MonoBehaviour
    {
        public byte ZombieId;

        private float _tx, _ty, _tz, _tRotY;
        private bool  _hasTarget;

        private const float INTERP_K = 7f;   // lerp factor for client-side smoothing

        // Called by master client each frame (immediate snap, no interpolation)
        public void SetImmediate(float x, float y, float z, float rotY)
        {
            transform.position = new Vector3(x, y, z);
            transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            _tx = x; _ty = y; _tz = z; _tRotY = rotY;
            _hasTarget = true;
        }

        // Called by non-master clients when a network packet arrives
        public void SetTarget(float x, float y, float z, float rotY)
        {
            if (!_hasTarget)
            {
                // First packet: snap directly so proxy doesn't lerp from world origin
                transform.position = new Vector3(x, y, z);
                transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                _hasTarget = true;
            }
            _tx = x; _ty = y; _tz = z; _tRotY = rotY;
        }

        void Update()
        {
            if (!_hasTarget) return;

            float t   = Mathf.Min(1f, INTERP_K * Time.deltaTime);
            Vector3 p = transform.position;
            transform.position = new Vector3(
                Mathf.Lerp(p.x, _tx, t),
                Mathf.Lerp(p.y, _ty, t),
                Mathf.Lerp(p.z, _tz, t));

            float curY = transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(curY, _tRotY, t), 0f);
        }

        void OnGUI()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos  = transform.position + new Vector3(0f, 1.4f, 0f);
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            if (screenPos.z <= 0f) return;   // behind camera

            GUI.color = Color.green;
            GUI.Label(new Rect(screenPos.x - 28f,
                               Screen.height - screenPos.y - 10f,
                               56f, 20f),
                      "Z" + ZombieId);
            GUI.color = Color.white;
        }
    }
}
