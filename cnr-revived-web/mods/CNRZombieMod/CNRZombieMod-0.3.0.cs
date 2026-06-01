// CNRZombieMod.cs — v0.3.0
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
        public const  string Version      = "0.3.0";
        public const  byte   ZOMBIE_EVENT = 198;   // Photon custom event code (≠ CNRMod's 199)
        private const string LogPath      = "/storage/emulated/0/CNRMods/zombiemod.log";

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

        // ── Persistent template GO ────────────────────────────────────────────
        // When we first find a source enemy prefab in any scene, we immediately
        // Instantiate it, disable+deactivate it, and DontDestroyOnLoad it.
        // This avoids stale UnityEngine.Object refs: scene-assigned Transform/
        // GameObject fields become null/invalid once their scene unloads, but
        // a DontDestroyOnLoad GO lives until the app quits.
        private static GameObject _templateGO = null;

        // ── Per-scene state ──────────────────────────────────────────────────
        private bool  _masterSpawned;
        private bool  _astarBuilt;     // true once we've built the runtime A* graph
        private float _broadcastTimer;
        private readonly Dictionary<byte, ZombieDriver> _drivers
            = new Dictionary<byte, ZombieDriver>();
        private readonly Dictionary<byte, ZombieProxy>  _proxies
            = new Dictionary<byte, ZombieProxy>();

        // ── Photon proxy ─────────────────────────────────────────────────────
        private bool   _proxyInstalled;
        private object _lastPeer;

        // ── HUD / diagnostics ────────────────────────────────────────────────
        private string _hud  = "";
        private float  _diagTimer = 0f;
        private const float DIAG_INTERVAL = 3f;
        private static readonly bool NAV_DEBUG_OVERLAY = true;
        private const int   NAV_DEBUG_NODE_STEP = 4;      // draw every Nth GridGraph node
        private const int   NAV_DEBUG_MAX_TILES = 1800;   // keep the overlay cheap on mobile
        private const float NAV_DEBUG_TILE_Y = 0.35f;
        private const float NAV_DEBUG_BLOCK_RADIUS = 90f;
        private const float NAV_DEBUG_MAX_STEP = 1.15f;
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

        // Freecam is parked for now. Leave the fields/methods below in place so it
        // can be re-enabled quickly after the nav debug overlay is sorted out.
        private static readonly bool FREECAM_ENABLED = false;
        private bool       _freecam;
        private Camera     _freecamCam;
        private Transform  _freecamOrigParent;
        private Vector3    _freecamOrigLocalPos;
        private Quaternion _freecamOrigLocalRot;
        private Vector3    _freecamPos;
        private Vector3    _freecamGuiMove;
        private Vector2    _freecamGuiLook;
        private bool       _freecamGuiFast;
        private float      _freecamYaw;
        private float      _freecamPitch;
        private const float FREECAM_SPEED = 12f;
        private const float FREECAM_FAST_MULT = 4f;
        private const float FREECAM_LOOK_SENS = 2.0f;

        // ─────────────────────────────────────────────────────────────────────
        // Unity messages
        // ─────────────────────────────────────────────────────────────────────

        void Start() { TryInstallProxy(); }

        void OnLevelWasLoaded(int level)
        {
            string scene = Application.loadedLevelName;
            if (FREECAM_ENABLED) DisableFreecam();

            // Always try to cache the prefab template on any scene load.
            // In singleplayer scenes, SingleEnemyManager.knifeEnemy is available —
            // that prefab has the full AI component set (Seeker + SingleEnemyAI).
            // TryCachePrefab will upgrade from a no-AI template if needed.
            TryCachePrefab();

            _masterSpawned = false;
            ClearAll();
            _hud = "";

            // Avoid live enemy component dumps during normal play. The old
            // scanner was useful for discovery, but it can stall the game hard
            // when singleplayer enemies include large weapon/model trees.
        }

        void OnLeftRoom()         { _masterSpawned = false; ClearAll(); }
        void OnDisconnectedFromPhoton()
        {
            _masterSpawned = false; _proxyInstalled = false; _lastPeer = null; ClearAll();
        }

        void Update()
        {
            try
            {
            // Keep Photon proxy installed even if peer re-connects
            object peer = GetNetworkingPeer();
            if (!ReferenceEquals(peer, _lastPeer)) { _proxyInstalled = false; _lastPeer = peer; }
            if (!_proxyInstalled) TryInstallProxy();

            string scene = Application.loadedLevelName;
            bool inRoom   = IsInRoom();
            bool isMaster = IsMasterClient();
            bool hasTemplate = _templateGO != null;
            GameObject ec = GameObject.Find("ExampleCharacter");  // only name, no tag

            if (FREECAM_ENABLED && Input.GetKeyDown(KeyCode.F6))
                ToggleFreecam();

            // Periodic diagnostic log + template retry
            _diagTimer -= Time.deltaTime;
            if (_diagTimer <= 0f)
            {
                _diagTimer = DIAG_INTERVAL;
                ZombieModEntry.Log(string.Format(
                    "Diag: scene={0} gameScene={1} inRoom={2} isMaster={3} hasTemplate={4} player={5} spawned={6}",
                    scene, IsGameScene(scene), inRoom, isMaster, hasTemplate,
                    ec != null ? ec.name : "null", _masterSpawned));
                // Retry caching the prefab if it failed on scene load
                // (SingleEnemyManager.mInstance may have been null at OnLevelWasLoaded time)
                if (_templateGO == null) TryCachePrefab();
            }

            // Persistent HUD showing current state
            _hud = string.Format("[ZMod] scene={0} room={1} master={2} tmpl={3} player={4} spawned={5} {6} {7}",
                scene, inRoom, isMaster, hasTemplate, ec != null ? "Y" : "N", _masterSpawned, _navDebugStatus,
                _freecam ? "freecam=ON" : "");

            if (!IsGameScene(scene)) return;
            if (!inRoom)             return;

            if (isMaster)
            {
                if (!_masterSpawned && ec != null)
                    SpawnZombies();

                if (_masterSpawned)
                {
                    _broadcastTimer -= Time.deltaTime;
                    if (_broadcastTimer <= 0f) { _broadcastTimer = BROADCAST_SECS; Broadcast(); }
                }
            }
            }
            catch (Exception ex) { ZombieModEntry.Log("Update err: " + ex.Message + "\n" + ex.StackTrace); }
        }

        void LateUpdate()
        {
            try
            {
                if (FREECAM_ENABLED && _freecam) UpdateFreecam();
            }
            catch (Exception ex) { ZombieModEntry.Log("Freecam err: " + ex.Message + "\n" + ex.StackTrace); }
        }

        void OnGUI()
        {
            GUI.Label(new Rect(8f, 150f, 600f, 24f), _hud);
            if (FREECAM_ENABLED && _freecam)
                GUI.Label(new Rect(8f, 174f, 760f, 24f), "[Freecam] F6 off | WASD move | Q/E down/up | Shift fast | RMB/arrows look");
            if (FREECAM_ENABLED) DrawFreecamGui();
        }

        private void ToggleFreecam()
        {
            if (_freecam) DisableFreecam();
            else EnableFreecam();
        }

        private void EnableFreecam()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                ZombieModEntry.Log("Freecam: no Camera.main found");
                return;
            }

            _freecamCam = cam;
            Transform t = cam.transform;
            _freecamOrigParent = t.parent;
            _freecamOrigLocalPos = t.localPosition;
            _freecamOrigLocalRot = t.localRotation;
            _freecamPos = t.position;

            Vector3 e = t.eulerAngles;
            _freecamYaw = e.y;
            _freecamPitch = e.x;
            if (_freecamPitch > 180f) _freecamPitch -= 360f;

            t.parent = null;
            _freecam = true;
            ZombieModEntry.Log("Freecam enabled at " + _freecamPos);
        }

        private void DisableFreecam()
        {
            if (!_freecam)
            {
                _freecamCam = null;
                return;
            }

            try
            {
                if (_freecamCam != null)
                {
                    Transform t = _freecamCam.transform;
                    if (_freecamOrigParent != null)
                    {
                        t.parent = _freecamOrigParent;
                        t.localPosition = _freecamOrigLocalPos;
                        t.localRotation = _freecamOrigLocalRot;
                    }
                }
            }
            catch (Exception ex) { ZombieModEntry.Log("Freecam restore err: " + ex.Message); }

            _freecam = false;
            _freecamCam = null;
            _freecamOrigParent = null;
            ZombieModEntry.Log("Freecam disabled");
        }

        private void UpdateFreecam()
        {
            if (_freecamCam == null)
            {
                DisableFreecam();
                return;
            }

            float lookX = 0f;
            float lookY = 0f;
            if (Input.GetMouseButton(1))
            {
                lookX += Input.GetAxis("Mouse X");
                lookY += Input.GetAxis("Mouse Y");
            }
            if (Input.GetKey(KeyCode.LeftArrow))  lookX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) lookX += 1f;
            if (Input.GetKey(KeyCode.UpArrow))    lookY += 1f;
            if (Input.GetKey(KeyCode.DownArrow))  lookY -= 1f;
            lookX += _freecamGuiLook.x;
            lookY += _freecamGuiLook.y;

            _freecamYaw += lookX * FREECAM_LOOK_SENS;
            _freecamPitch -= lookY * FREECAM_LOOK_SENS;
            _freecamPitch = Mathf.Clamp(_freecamPitch, -89f, 89f);

            Quaternion rot = Quaternion.Euler(_freecamPitch, _freecamYaw, 0f);
            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) move += Vector3.back;
            if (Input.GetKey(KeyCode.A)) move += Vector3.left;
            if (Input.GetKey(KeyCode.D)) move += Vector3.right;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) move += Vector3.down;
            move += _freecamGuiMove;

            float speed = FREECAM_SPEED;
            if (_freecamGuiFast || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speed *= FREECAM_FAST_MULT;

            if (move.sqrMagnitude > 0.001f)
                _freecamPos += rot * move.normalized * speed * Time.deltaTime;

            Transform t = _freecamCam.transform;
            t.position = _freecamPos;
            t.rotation = rot;
        }

        private void DrawFreecamGui()
        {
            if (GUI.Button(new Rect(8f, 198f, 126f, 38f), _freecam ? "Freecam ON" : "Freecam"))
                ToggleFreecam();

            _freecamGuiMove = Vector3.zero;
            _freecamGuiLook = Vector2.zero;
            _freecamGuiFast = false;
            if (!_freecam) return;

            float x = 8f;
            float y = 244f;
            float s = 48f;
            if (GUI.RepeatButton(new Rect(x + s, y, s, s), "W")) _freecamGuiMove += Vector3.forward;
            if (GUI.RepeatButton(new Rect(x, y + s, s, s), "A")) _freecamGuiMove += Vector3.left;
            if (GUI.RepeatButton(new Rect(x + s, y + s, s, s), "S")) _freecamGuiMove += Vector3.back;
            if (GUI.RepeatButton(new Rect(x + s * 2f, y + s, s, s), "D")) _freecamGuiMove += Vector3.right;
            if (GUI.RepeatButton(new Rect(x, y + s * 2f, s, s), "Q")) _freecamGuiMove += Vector3.down;
            if (GUI.RepeatButton(new Rect(x + s, y + s * 2f, s, s), "E")) _freecamGuiMove += Vector3.up;
            if (GUI.RepeatButton(new Rect(x + s * 2f, y + s * 2f, s, s), "Fast")) _freecamGuiFast = true;

            x = 178f;
            if (GUI.RepeatButton(new Rect(x, y, s, s), "<")) _freecamGuiLook.x -= 1f;
            if (GUI.RepeatButton(new Rect(x + s, y, s, s), "^")) _freecamGuiLook.y += 1f;
            if (GUI.RepeatButton(new Rect(x + s * 2f, y, s, s), ">")) _freecamGuiLook.x += 1f;
            if (GUI.RepeatButton(new Rect(x + s, y + s, s, s), "v")) _freecamGuiLook.y -= 1f;
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
            {
                Type aiCheck = FindType("SingleEnemyAI");
                if (aiCheck != null && _templateGO.GetComponent(aiCheck) != null)
                    return; // already have the best template (Knife_singleEnemy)
                // Template exists but has no AI (e.g. Enemy3). Destroy and rebuild.
                ZombieModEntry.Log("TryCachePrefab: upgrading template (no SingleEnemyAI on current)");
                UnityEngine.Object.Destroy(_templateGO);
                _templateGO = null;
            }

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
        // Prefers SingleEnemyManager.knifeEnemy (has Seeker+SingleEnemyAI) over
        // Game.mInstance.enemyBot (visual-only mesh, no AI components).
        private static UnityEngine.Object FindSourcePrefab()
        {
            // Source 1 (PREFERRED): SingleEnemyManager.mInstance.knifeEnemy
            // This prefab has the full component set: Seeker, SingleEnemyAI, SingleEnemyLogic.
            // Available in singleplayer scenes (SingleMode_1) and sometimes in multiplayer.
            Type semType = FindType("SingleEnemyManager");
            if (semType != null)
            {
                FieldInfo miField = semType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object semInst = miField != null ? miField.GetValue(null) : null;
                ZombieModEntry.Log("FindSourcePrefab: SingleEnemyManager.mInstance=" + (semInst != null ? semInst.ToString() : "null"));
                if (semInst != null)
                {
                    foreach (string fn in new[] { "knifeEnemy","gunEnemy","snipeEnemy","grenadeEnemy" })
                    {
                        FieldInfo f = semType.GetField(fn, BindingFlags.Public | BindingFlags.Instance);
                        if (f == null) continue;
                        UnityEngine.Object v = (UnityEngine.Object)f.GetValue(semInst);
                        ZombieModEntry.Log("FindSourcePrefab: SEM." + fn + "=" + (v != null ? v.name : "null"));
                        if (v != null) return v;
                    }
                }
            }

            // Source 2 (FALLBACK): Game.mInstance.enemyBot  (Kill Mode / multiplayer scenes)
            // Enemy3: visual mesh only — no Seeker/SingleEnemyAI.  Use only if SEM unavailable.
            Type gameType = FindType("Game");
            if (gameType != null)
            {
                FieldInfo miField = gameType.GetField("mInstance", BindingFlags.Public | BindingFlags.Static);
                object gameInst = miField != null ? miField.GetValue(null) : null;
                ZombieModEntry.Log("FindSourcePrefab: Game.mInstance=" + (gameInst != null ? gameInst.ToString() : "null"));
                if (gameInst != null)
                {
                    foreach (string fn in new[] { "enemyBot","enemyBot1","enemyBot2","enemyBot3","enemyBot4" })
                    {
                        FieldInfo f = gameType.GetField(fn, BindingFlags.Public | BindingFlags.Instance);
                        if (f == null) continue;
                        UnityEngine.Object v = (UnityEngine.Object)f.GetValue(gameInst);
                        ZombieModEntry.Log("FindSourcePrefab: Game." + fn + "=" + (v != null ? v.name : "null"));
                        if (v != null) return v;
                    }
                }
            }

            // Source 3 (LAST RESORT): Resources.Load by name — works if the
            // prefab was placed in a Resources/ folder in the original project.
            // Lets the mod work without ever visiting singleplayer.
            foreach (string rn in new[] { "Knife_singleEnemy", "knifeEnemy", "Enemy/Knife_singleEnemy" })
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
        private void SpawnZombies()
        {
            if (_templateGO == null)
            {
                ZombieModEntry.Log("SpawnZombies: no template GO — prefab not cached yet");
                _hud = "[ZombieMod] No enemy prefab — visit FreeRun (Kill Mode) first";
                return;
            }

            _masterSpawned = true;

            GameObject ec = GameObject.Find("ExampleCharacter");
            Vector3 origin = ec != null ? ec.transform.position : Vector3.zero;
            ZombieModEntry.Log("SpawnZombies: origin=" + origin + " template=" + _templateGO.name);

            // Build a runtime A* GridGraph if none exists in this (multiplayer) scene.
            // GridGraph scans downward raycasts: any flat physics surface becomes walkable.
            if (!_astarBuilt)
            {
                LogSceneLayers(origin);
                BuildAstarGraph(origin);
                _astarBuilt = true;
            }
            for (int i = 0; i < ZOMBIE_COUNT; i++)
            {
                float angle = (Mathf.PI * 2f / ZOMBIE_COUNT) * i;
                Vector3 pos = origin + new Vector3(Mathf.Sin(angle) * SPAWN_RADIUS, 0f, Mathf.Cos(angle) * SPAWN_RADIUS);

                GameObject enemyGO = null;
                try
                {
                    // Instantiate from the persistent DontDestroyOnLoad template GO
                    // (always valid, never stale)
                    enemyGO = (GameObject)UnityEngine.Object.Instantiate(
                        _templateGO, pos, Quaternion.identity);
                    enemyGO.SetActive(true);  // template is inactive; activate the clone

                    if (enemyGO == null) { ZombieModEntry.Log("SpawnZombies[" + i + "]: Instantiate returned null"); continue; }
                    ZombieModEntry.Log("SpawnZombies[" + i + "]: spawned " + enemyGO.name + " at " + pos);

                    enemyGO.name = "ZombieEnemy_" + (i + 1);

                    // ── Disable AI components BEFORE their Start() fires ─────
                    // Start() fires on the next frame; disabling here prevents:
                    //   SingleEnemyLogic.Start() — crashes (null SingleEnemyManager.mInstance)
                    //   SingleEnemyAI.Start()    — crashes (null AstarPath.active in MP)
                    // ZombieDriver re-enables SingleEnemyAI only when A* is confirmed present.
                    DisableComponent(enemyGO, "SingleEnemyLogic");
                    DisableComponent(enemyGO, "SingleEnemyAI");

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
            Component[] comps = go.GetComponentsInChildren(t, true);
            for (int i = 0; i < comps.Length; i++)
            {
                Behaviour b = comps[i] as Behaviour;
                if (b != null) b.enabled = false;
            }
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
            if (_templateGO != null)
            {
                try
                {
                    go = (GameObject)UnityEngine.Object.Instantiate(_templateGO);
                    go.SetActive(true);
                    go.name = "ZombieProxy_" + id;
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

            ClearNavDebug();

            _masterSpawned = false;
            _astarBuilt    = false;
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
                        SetMember(coll, collType, "heightCheck",           false); // keep nodes at flat walking Y; no terrain snapping
                        SetMember(coll, collType, "heightMask",            obstacleMask);
                        SetMember(coll, collType, "fromHeight",            100f);
                        SetMember(coll, collType, "unwalkableWhenNoGround",false);
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
                SetMember(grid, gridType, "maxClimb", NAV_DEBUG_MAX_STEP);
                // Erosion: after collision marks wall-interior nodes blocked, erode by 1 node
                // so adjacent nodes (character clearance zone) are also blocked.
                SetMember(grid, gridType, "erodeIterations", 1);

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
            if (!NAV_DEBUG_OVERLAY) return;
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

                UnityEngine.Object[] objs = FindObjectsOfType(typeof(MeshFilter));
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
            if (!NAV_DEBUG_OVERLAY || _navDebugRoot == null) return;
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
                    Component[] comps = FindObjectsOfType(logicType) as Component[];
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
                    Component[] comps = FindObjectsOfType(aiType) as Component[];
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
    public class ZombieDriver : MonoBehaviour
    {
        public byte  ZombieId;
        public float ChaseSpeed = 2.8f;

        private Component          _singleEnemyAI; // SingleEnemyAI instance (may be null if not on GO)
        private CharacterController _cc;            // for wall-respecting movement when no A* graph
        private bool                _hasAstar;
        private bool                _aiReady;

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

                // Check if A* is present in this scene
                _hasAstar = CheckAstar();

                if (_singleEnemyAI != null && _hasAstar)
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

                _cc = GetComponent<CharacterController>();
                ZombieModEntry.Log("ZombieDriver[" + ZombieId + "]: cc=" + (_cc != null) + " aiReady=" + _aiReady + " hasAstar=" + _hasAstar);

                // Attach debug HUD (billboard text + path line/dots)
                if (_singleEnemyAI != null)
                {
                    ZombieDebugHUD hud = gameObject.AddComponent<ZombieDebugHUD>();
                    hud.SrcAI = _singleEnemyAI;
                }
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
                    // Tell SingleEnemyAI / AIPath to path toward the player.
                    // Also force canSearch=true every frame — AIPath.SearchPath() sets it to
                    // false if target was null, and won't reset it on its own.
                    if (_fTarget    != null) _fTarget.SetValue(_singleEnemyAI, playerTr);
                    if (_fSpeed     != null) _fSpeed.SetValue(_singleEnemyAI, ChaseSpeed);
                    if (_fCanSearch != null) _fCanSearch.SetValue(_singleEnemyAI, true);
                    if (_fCanMove   != null) _fCanMove.SetValue(_singleEnemyAI, true);
                }
                else
                {
                    // Direct movement fallback — use CharacterController so walls are respected.
                    Vector3 dir = playerTr.position - transform.position;
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
            // Local player — no FindWithTag (tag may not be registered, throws UnityException)
            GameObject ec = GameObject.Find("ExampleCharacter");

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
            float gx = sp.x - 80f;
            float gy = Screen.height - sp.y - 70f;
            const float W = 160f, LH = 14f;

            GUIStyle bg = new GUIStyle(GUI.skin.label);
            bg.normal.background = Texture2D.whiteTexture;
            bg.normal.textColor  = Color.black;
            bg.fontSize = 10;
            bg.padding  = new RectOffset(2, 2, 1, 1);

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(gx - 2f, gy - 2f, W + 4f, LH * 5f + 4f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            string walkStr = walkable ? "<color=#00ff00>WALK</color>" : "<color=#ff4444>BLOCK</color>";
            GUIStyle rich = new GUIStyle(bg);
            rich.richText = true;
            rich.normal.background = null;
            rich.normal.textColor  = Color.white;
            rich.fontSize = 10;

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
}
