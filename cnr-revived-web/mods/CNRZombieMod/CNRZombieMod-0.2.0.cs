// CNRZombieMod.cs — v0.2.0
// Zombie multiplayer test mod for Cops N Robbers.
//
// Reuses the game's own singleplayer AI infrastructure:
//   - Enemy prefab:       grabbed from Game.mInstance.enemyBot or
//                         SingleEnemyManager.mInstance.knifeEnemy via reflection
//                         and cached via DontDestroyOnLoad.  The user must load
//                         any Kill-mode / SingleMode scene at least once per
//                         session so the reference is available.
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
        public const  string Version      = "0.2.0";
        public const  byte   ZOMBIE_EVENT = 198;   // Photon custom event code (≠ CNRMod's 199)
        private const string LogPath      = "/storage/emulated/0/CNRMods/zombiemod.log";

        public static void Load()
        {
            try
            {
                var go = new GameObject("CNRZombieMod_Root");
                go.AddComponent<ZombieHook>();
                UnityEngine.Object.DontDestroyOnLoad(go);
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
            try { File.AppendAllText(LogPath,
                      "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); }
            catch { }
            try { Debug.Log("[ZombieMod] " + msg); } catch { }
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
            if (ev.Code == ZombieModEntry.ZOMBIE_EVENT && _hook != null)
                _hook.OnZombieEvent(ev);
            _orig.OnEvent(ev);
        }
        public void DebugReturn(DebugLevel l, string m) { _orig.DebugReturn(l, m); }
        public void OnOperationResponse(OperationResponse r) { _orig.OnOperationResponse(r); }
        public void OnStatusChanged(StatusCode c) { _orig.OnStatusChanged(c); }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZOMBIE HOOK — persistent MonoBehaviour (DontDestroyOnLoad root)
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieHook : MonoBehaviour
    {
        // ── Tunables ────────────────────────────────────────────────────────
        private const int   ZOMBIE_COUNT   = 5;
        private const float SPAWN_RADIUS   = 10f;   // metres around local player
        private const float CHASE_SPEED    = 2.8f;
        private const float BROADCAST_SECS = 0.30f; // position sync interval
        private const int   STRIDE         = 5;     // floats per zombie: id,x,y,z,rotY

        // ── Cached enemy prefab (grabbed from any scene that has one) ────────
        // We try Game.mInstance.enemyBot and SingleEnemyManager.mInstance.knifeEnemy
        // via reflection, stored across scene loads via DontDestroyOnLoad.
        private static UnityEngine.Object _cachedPrefab = null;   // Transform or GameObject

        // ── Per-scene state ──────────────────────────────────────────────────
        private bool  _masterSpawned;
        private float _broadcastTimer;
        private readonly Dictionary<byte, ZombieDriver> _drivers
            = new Dictionary<byte, ZombieDriver>();
        private readonly Dictionary<byte, ZombieProxy>  _proxies
            = new Dictionary<byte, ZombieProxy>();

        // ── Photon proxy ─────────────────────────────────────────────────────
        private bool   _proxyInstalled;
        private object _lastPeer;

        // ── HUD ──────────────────────────────────────────────────────────────
        private string _hud = "";

        // ─────────────────────────────────────────────────────────────────────
        // Unity messages
        // ─────────────────────────────────────────────────────────────────────

        void Start() { TryInstallProxy(); }

        void OnLevelWasLoaded(int level)
        {
            TryCachePrefab();       // grab enemy prefab whenever a scene has one
            _masterSpawned = false;
            ClearAll();
            _hud = "";
        }

        void OnLeftRoom()         { _masterSpawned = false; ClearAll(); }
        void OnDisconnectedFromPhoton()
        {
            _masterSpawned = false; _proxyInstalled = false; _lastPeer = null; ClearAll();
        }

        void Update()
        {
            // Keep Photon proxy installed even if peer re-connects
            object peer = GetNetworkingPeer();
            if (!ReferenceEquals(peer, _lastPeer)) { _proxyInstalled = false; _lastPeer = peer; }
            if (!_proxyInstalled) TryInstallProxy();

            string scene = Application.loadedLevelName;
            if (!IsGameScene(scene)) return;
            if (!IsInRoom())         return;

            if (IsMasterClient())
            {
                if (!_masterSpawned && GameObject.Find("ExampleCharacter") != null)
                    SpawnZombies();

                if (_masterSpawned)
                {
                    _broadcastTimer -= Time.deltaTime;
                    if (_broadcastTimer <= 0f) { _broadcastTimer = BROADCAST_SECS; Broadcast(); }
                }
            }
        }

        void OnGUI()
        {
            if (!string.IsNullOrEmpty(_hud))
                GUI.Label(new Rect(8f, 150f, 400f, 24f), _hud);

            if (_cachedPrefab == null)
                GUI.Label(new Rect(8f, 174f, 480f, 24f),
                    "[ZombieMod] No enemy prefab cached — visit a FreeRun (Kill) or Single Mode scene first");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Try to grab the enemy prefab reference from any available source.
        // Called every scene load so we opportunistically cache it.
        // ─────────────────────────────────────────────────────────────────────
        private void TryCachePrefab()
        {
            if (_cachedPrefab != null) return;  // already have it

            try
            {
                // Source 1: Game.mInstance.enemyBot  (Transform prefab ref, Kill mode)
                Type gameType = FindType("Game");
                if (gameType != null)
                {
                    FieldInfo miField = gameType.GetField("mInstance",
                        BindingFlags.Public | BindingFlags.Static);
                    object gameInst = miField != null ? miField.GetValue(null) : null;
                    if (gameInst != null)
                    {
                        FieldInfo ebField = gameType.GetField("enemyBot",
                            BindingFlags.Public | BindingFlags.Instance);
                        UnityEngine.Object eb = ebField != null
                            ? (UnityEngine.Object)ebField.GetValue(gameInst) : null;
                        if (eb != null) { _cachedPrefab = eb; ZombieModEntry.Log("Prefab cached from Game.enemyBot"); return; }

                        // Also try enemyBot1 … enemyBot4
                        foreach (string fn in new[] { "enemyBot1","enemyBot2","enemyBot3","enemyBot4" })
                        {
                            FieldInfo f = gameType.GetField(fn, BindingFlags.Public | BindingFlags.Instance);
                            UnityEngine.Object v = f != null ? (UnityEngine.Object)f.GetValue(gameInst) : null;
                            if (v != null) { _cachedPrefab = v; ZombieModEntry.Log("Prefab cached from Game." + fn); return; }
                        }
                    }
                }

                // Source 2: SingleEnemyManager.mInstance.knifeEnemy  (singleplayer scenes)
                Type semType = FindType("SingleEnemyManager");
                if (semType != null)
                {
                    FieldInfo miField = semType.GetField("mInstance",
                        BindingFlags.Public | BindingFlags.Static);
                    object semInst = miField != null ? miField.GetValue(null) : null;
                    if (semInst != null)
                    {
                        foreach (string fn in new[] { "knifeEnemy","gunEnemy","snipeEnemy","grenadeEnemy" })
                        {
                            FieldInfo f = semType.GetField(fn, BindingFlags.Public | BindingFlags.Instance);
                            UnityEngine.Object v = f != null ? (UnityEngine.Object)f.GetValue(semInst) : null;
                            if (v != null) { _cachedPrefab = v; ZombieModEntry.Log("Prefab cached from SingleEnemyManager." + fn); return; }
                        }
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("TryCachePrefab err: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Master: spawn N enemy GOs using the cached game prefab.
        // Immediately disables SingleEnemyLogic (before its Start() fires) so
        // it never accesses SingleEnemyManager.mInstance or PlayerLogic.mInstance.
        // ZombieDriver is added as the replacement state machine.
        // ─────────────────────────────────────────────────────────────────────
        private void SpawnZombies()
        {
            _masterSpawned = true;

            if (_cachedPrefab == null)
            {
                ZombieModEntry.Log("SpawnZombies: no prefab cached — skipping");
                _hud = "[ZombieMod] No enemy prefab — visit Kill mode first";
                return;
            }

            GameObject ec = GameObject.Find("ExampleCharacter");
            Vector3 origin = ec != null ? ec.transform.position : Vector3.zero;

            for (int i = 0; i < ZOMBIE_COUNT; i++)
            {
                float angle = (Mathf.PI * 2f / ZOMBIE_COUNT) * i;
                Vector3 pos = origin + new Vector3(Mathf.Sin(angle) * SPAWN_RADIUS, 0f, Mathf.Cos(angle) * SPAWN_RADIUS);

                GameObject enemyGO = null;
                try
                {
                    // Instantiate from the cached prefab (Transform or GameObject ref both work)
                    UnityEngine.Object inst = UnityEngine.Object.Instantiate(
                        _cachedPrefab, pos, Quaternion.identity);
                    // Normalize to a GameObject
                    if (inst is GameObject)
                        enemyGO = (GameObject)inst;
                    else if (inst is Component)
                        enemyGO = ((Component)inst).gameObject;

                    if (enemyGO == null) { ZombieModEntry.Log("SpawnZombies: Instantiate returned non-GO"); continue; }

                    enemyGO.name = "ZombieEnemy_" + (i + 1);

                    // ── Disable SingleEnemyLogic BEFORE its Start() fires ──────
                    // (Start() fires next frame, so we have this frame to disable it)
                    DisableComponent(enemyGO, "SingleEnemyLogic");

                    // ── Create the 4 temp transform GOs that SingleEnemyLogic
                    //    would look for (safe to create even though it's disabled,
                    //    in case something else references them) ──────────────────
                    CreateTempTransform("enemyTempGenerateTransform" + i, pos);
                    CreateTempTransform("enemyTempPatrolTransform"   + i, pos);
                    CreateTempTransform("enemyTempRushTransform"     + i, pos);
                    CreateTempTransform("enemyTempAttackTransform"   + i, pos);

                    // ── Add our driver (reads SingleEnemyAI, drives it) ─────────
                    ZombieDriver drv = enemyGO.AddComponent<ZombieDriver>();
                    drv.ZombieId    = (byte)(i + 1);
                    drv.ChaseSpeed  = CHASE_SPEED;
                    _drivers[(byte)(i + 1)] = drv;
                }
                catch (Exception ex) { ZombieModEntry.Log("SpawnZombies[" + i + "] err: " + ex.Message); }
            }

            ZombieModEntry.Log("Master: spawned " + ZOMBIE_COUNT + " zombies near " + origin);
            _broadcastTimer = 0f;
            _hud = "[ZombieMod] Master — " + ZOMBIE_COUNT + " zombies running";
        }

        private static void DisableComponent(GameObject go, string typeName)
        {
            Type t = FindType(typeName);
            if (t == null) return;
            Component c = go.GetComponent(t);
            if (c != null) ((Behaviour)c).enabled = false;
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
            if (_drivers.Count == 0) return;

            float[] data = new float[_drivers.Count * STRIDE];
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
                idx++;
            }

            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["zd"] = data;

            try
            {
                object peer = GetNetworkingPeer();
                if (peer == null) return;
                MethodInfo raise = peer.GetType().GetMethod("OpRaiseEvent",
                    new Type[] { typeof(byte), typeof(System.Collections.Hashtable), typeof(bool), typeof(byte) });
                if (raise != null)
                    raise.Invoke(peer, new object[] { ZombieModEntry.ZOMBIE_EVENT, ht, false, (byte)0 });
            }
            catch (Exception ex) { ZombieModEntry.Log("Broadcast err: " + ex.Message); }
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
                if (IsMasterClient()) return;  // master drives its own GOs

                if (!ev.Parameters.ContainsKey((byte)245)) return;
                var ht = ev.Parameters[(byte)245] as System.Collections.Hashtable;
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

        private ZombieProxy GetOrCreateProxy(byte id)
        {
            ZombieProxy p;
            if (_proxies.TryGetValue(id, out p) && p != null && p.gameObject != null)
                return p;

            GameObject go = null;
            if (_cachedPrefab != null)
            {
                try
                {
                    UnityEngine.Object inst = UnityEngine.Object.Instantiate(_cachedPrefab);
                    go = inst is GameObject ? (GameObject)inst
                       : inst is Component  ? ((Component)inst).gameObject : null;
                    if (go != null)
                    {
                        go.name = "ZombieProxy_" + id;
                        // Disable AI — non-master client just interpolates
                        DisableComponent(go, "SingleEnemyLogic");
                        DisableComponent(go, "SingleEnemyAI");
                    }
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
            _proxies[id] = p;
            return p;
        }

        private void ClearAll()
        {
            foreach (var kv in _drivers)
                if (kv.Value != null && kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            _drivers.Clear();

            foreach (var kv in _proxies)
                if (kv.Value != null && kv.Value.gameObject != null)
                    UnityEngine.Object.Destroy(kv.Value.gameObject);
            _proxies.Clear();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Photon helpers (same pattern as CNRMod)
        // ─────────────────────────────────────────────────────────────────────
        private bool IsInRoom()
        {
            try { Type t = GetPNType(); if (t == null) return false;
                  PropertyInfo pi = t.GetProperty("inRoom", BindingFlags.Public | BindingFlags.Static);
                  return pi != null && (bool)pi.GetValue(null, null); }
            catch { return false; }
        }
        private bool IsMasterClient()
        {
            try { Type t = GetPNType(); if (t == null) return false;
                  PropertyInfo pi = t.GetProperty("isMasterClient", BindingFlags.Public | BindingFlags.Static);
                  return pi != null && (bool)pi.GetValue(null, null); }
            catch { return false; }
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
    public class ZombieDriver : MonoBehaviour
    {
        public byte  ZombieId;
        public float ChaseSpeed = 2.8f;

        private Component  _singleEnemyAI; // SingleEnemyAI instance (may be null if not on GO)
        private bool       _hasAstar;
        private bool       _aiReady;

        // Reflected fields/props on SingleEnemyAI / AIPath
        private FieldInfo  _fTarget;       // AIPath.target     (Transform)
        private FieldInfo  _fSpeed;        // AIPath.speed      (float)
        private FieldInfo  _fCanSearch;    // AIPath.canSearch  (bool)
        private FieldInfo  _fCanMove;      // AIPath.canMove    (bool)

        // Body/hand Animation components (for playing walk animation)
        private Animation _bodyAnim;
        private Animation _handAnim;

        void Start()
        {
            try
            {
                Type aiType = FindType("SingleEnemyAI");
                if (aiType != null)
                {
                    _singleEnemyAI = GetComponent(aiType);
                    if (_singleEnemyAI != null)
                    {
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

                // Check if A* is present in this scene
                _hasAstar = CheckAstar();

                if (_singleEnemyAI != null && _hasAstar)
                {
                    // Let AIPath run movement; we just set the target each frame
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
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver.Start err: " + ex.Message); }
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

                Transform playerTr = GetNearestPlayer();
                if (playerTr == null) return;

                if (_aiReady && _singleEnemyAI != null)
                {
                    // Tell SingleEnemyAI / AIPath to path toward the player
                    if (_fTarget != null) _fTarget.SetValue(_singleEnemyAI, playerTr);
                    if (_fSpeed  != null) _fSpeed.SetValue(_singleEnemyAI, ChaseSpeed);
                    // SingleEnemyAI.Update() handles movement via CharacterController / NavController
                }
                else
                {
                    // Direct movement fallback
                    Vector3 dir = playerTr.position - transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        dir.Normalize();
                        transform.position += dir * ChaseSpeed * Time.deltaTime;
                        transform.rotation  = Quaternion.LookRotation(dir);
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("ZombieDriver.Update err: " + ex.Message); }
        }

        // ─── helpers ───────────────────────────────────────────────────────

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
            // Local player
            GameObject ec = GameObject.FindWithTag("Player");
            if (ec == null) ec = GameObject.Find("ExampleCharacter");

            // Remote players — find via NetPlayerController type
            Type npcType = FindType("NetPlayerController");
            if (npcType == null) return ec != null ? ec.transform : null;

            UnityEngine.Object[] remotes = FindObjectsOfType(npcType);
            Vector3 pos = transform.position;
            Transform best = ec != null ? ec.transform : null;
            float bestSq = best != null ? (best.position - pos).sqrMagnitude : float.MaxValue;

            for (int i = 0; i < remotes.Length; i++)
            {
                Component c = remotes[i] as Component;
                if (c == null) continue;
                float sq = (c.transform.position - pos).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = c.transform; }
            }
            return best;
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
    // Receives target position from ZombieHook.OnZombieEvent and interpolates.
    // Also plays the walk animation and shows a Z-label above the enemy.
    // ─────────────────────────────────────────────────────────────────────────
    public class ZombieProxy : MonoBehaviour
    {
        public byte ZombieId;

        private float _tx, _ty, _tz, _tRotY;
        private bool  _hasTarget;

        private Animation _bodyAnim;
        private Animation _handAnim;

        private const float INTERP_K = 7f;

        void Start()
        {
            _bodyAnim = FindAnim("weaponControl/EnemyAnimation");
            _handAnim = FindAnim("weaponControl/1_3/handrightup/handright_Animation");
            if (_bodyAnim == null) _bodyAnim = FindAnim("GameObject/EnemyAnimation");
            if (_bodyAnim == null) _bodyAnim = FindAnim("EnemyAnimation");

            if (_bodyAnim != null) _bodyAnim.Play("walkway");
            if (_handAnim != null) _handAnim.Play("walkway");
        }

        public void SetTarget(float x, float y, float z, float rotY)
        {
            if (!_hasTarget)
            {
                transform.position = new Vector3(x, y, z);
                transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                _hasTarget = true;
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

        void OnGUI()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(transform.position + new Vector3(0f, 1.4f, 0f));
            if (sp.z <= 0f) return;
            GUI.color = Color.green;
            GUI.Label(new Rect(sp.x - 28f, Screen.height - sp.y - 10f, 56f, 20f), "Z" + ZombieId);
            GUI.color = Color.white;
        }

        private Animation FindAnim(string path)
        {
            Transform t = transform.Find(path);
            return t != null ? t.gameObject.GetComponent<Animation>() : null;
        }
    }
}
