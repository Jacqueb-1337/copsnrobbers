// CNRFreecamMod.cs — v0.1.0
//
// Stand-alone freecam for Cops N Robbers (extracted from CNRZombieMod).
//
// Freecam only detaches Camera.main from the player rig — the player character
// stays exactly where they are with their skin, held weapon, and all game logic
// intact (you can still shoot, take damage, etc. from your real position).
//
// Features:
//   • F6 toggles freecam on/off.
//   • WASD/Q/E move the camera, RMB + drag or arrow keys rotate, Shift = fast.
//   • On-screen D-pad / look buttons for touch use.
//   • F key briefly shows which colliders are active on the local player
//     (useful for seeing CNRSettingsMod's legs/torso disable in action).
//
// Entry point: FreecamModEntry.Load() — found by CNRMod's DLL scanner.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRFreecamMod
{
    // ─────────────────────────────────────────────────────────────────────────
    // ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────────
    public class FreecamModEntry
    {
        public const  string Version = "0.3.3";
        private const string LogPath = "/storage/emulated/0/CNRMods/freecammod.log";
        private static bool  _loaded = false;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                var go = new GameObject("CNRFreecamMod_Root");
                go.AddComponent<FreecamHook>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                Log("=== CNRFreecamMod v" + Version + " loaded ===");

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null) continue;
                    MethodInfo reg = me.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    if (reg != null) reg.Invoke(null, new object[] { "CNRFreecamMod", Version });
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
            try { Debug.Log("[FreecamMod] " + msg); } catch { }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FREECAM HOOK
    // ─────────────────────────────────────────────────────────────────────────
    public class FreecamHook : MonoBehaviour
    {
        // ── Tunables ────────────────────────────────────────────────────────
        private const float FREECAM_SPEED     = 12f;
        private const float FREECAM_FAST_MULT = 4f;
        private const float FREECAM_LOOK_SENS = 2.0f;

        // ── Freecam state ────────────────────────────────────────────────────
        private bool       _freecam;
        // A dedicated second Camera is created for freecam. Camera.main is NEVER
        // moved — it stays on the player rig so that all game logic (fire direction,
        // raycasts, aim) uses the correct player-space origin.
        private GameObject _freecamGO;
        private Camera     _freecamCam;
        private Camera     _mainCam;
        private int        _mainOrigCullingMask;
        private Vector3    _freecamPos;
        private float      _freecamYaw;
        private float      _freecamPitch;

        // ── Player body renderer restore ───────────────────────────────────
        // The game hides the local player body via PlayerLocationSync.localObjectsToDeactivate:
        // each GO in that list (and ALL its children) is set inactive with SetActiveRecursively(false).
        // Primary fix: reflect into that list and recursively re-activate each subtree.
        // Fallback: old ancestor-walk approach (in case PLS isn't found).
        private GameObject[]          _localBodyGOs;   // items from PlayerLocationSync.localObjectsToDeactivate
        private SkinnedMeshRenderer[] _playerSMRs;     // fallback
        private bool[]                _smrWasEnabled;  // fallback
        private Transform[]           _activatedGOs;   // fallback

        // On-screen touch D-pad accumulators (reset each OnGUI frame)
        private Vector3 _guiMove = Vector3.zero;
        private Vector2 _guiLook = Vector2.zero;
        private bool    _guiFast;

        // ── Collider-status display ───────────────────────────────────────────
        // When F is pressed, briefly show which body-part colliders are disabled
        // (CNRSettingsMod disables legs/torso to prevent self-damage).
        private float _colliderDisplayTimer;
        private const float COLLIDER_DISPLAY_SECS = 3f;
        private string _colliderStatus = "";

        // ─────────────────────────────────────────────────────────────────────

        void OnDestroy()
        {
            try { DisableFreecam(); } catch { }
        }

        void Update()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.F6))
                    ToggleFreecam();

                // Joke: press F to flash collider status
                if (Input.GetKeyDown(KeyCode.F))
                {
                    RefreshColliderStatus();
                    _colliderDisplayTimer = COLLIDER_DISPLAY_SECS;
                }
                // G = dump all renderers in scene to log
                if (Input.GetKeyDown(KeyCode.G))
                    DumpSceneRenderers();
                if (_colliderDisplayTimer > 0f)
                    _colliderDisplayTimer -= Time.deltaTime;

                if (_freecam)
                    UpdateFreecam();
            }
            catch (Exception ex) { FreecamModEntry.Log("Update err: " + ex.Message); }
        }

        void LateUpdate()
        {
            try
            {
                if (_freecam) ApplyFreecam();
            }
            catch (Exception ex) { FreecamModEntry.Log("LateUpdate err: " + ex.Message); }
        }

        void OnGUI()
        {
            try { DrawGui(); }
            catch (Exception ex) { FreecamModEntry.Log("OnGUI err: " + ex.Message); }
        }

        // ─── Freecam enable / disable ─────────────────────────────────────────

        private void ToggleFreecam()
        {
            if (_freecam) DisableFreecam();
            else          EnableFreecam();
        }

        private void EnableFreecam()
        {
            Camera main = Camera.main;
            if (main == null) { FreecamModEntry.Log("Freecam: no Camera.main"); return; }

            _mainCam             = main;
            _mainOrigCullingMask = main.cullingMask;

            // Start freecam at the current camera world position/rotation
            _freecamPos   = main.transform.position;
            Vector3 e     = main.transform.eulerAngles;
            _freecamYaw   = e.y;
            _freecamPitch = e.x > 180f ? e.x - 360f : e.x;

            // Create a dedicated camera for the freecam view.
            // Camera.main is NOT moved so game fire/aim logic is unaffected.
            _freecamGO  = new GameObject("FreecamCamera");
            _freecamCam = _freecamGO.AddComponent<Camera>();
            _freecamCam.CopyFrom(main);
            _freecamCam.depth       = main.depth + 1;
            _freecamCam.cullingMask = ~0;
            _freecamGO.transform.position = _freecamPos;
            _freecamGO.transform.rotation = Quaternion.Euler(_freecamPitch, _freecamYaw, 0f);

            // Suppress Camera.main rendering so the scene isn't drawn twice.
            // Camera.main stays active so the game's input/raycast code works.
            main.cullingMask = 0;

            // Enable only SkinnedMeshRenderers (player body) — avoids turning on
            // capsule/debug MeshRenderers that would show a capsule placeholder.
            ShowPlayerBody(true);

            _freecam = true;
            FreecamModEntry.Log("Freecam enabled at " + _freecamPos);
        }

        private void DisableFreecam()
        {
            if (!_freecam) return;

            // Restore Camera.main rendering
            if (_mainCam != null) _mainCam.cullingMask = _mainOrigCullingMask;

            // Destroy the freecam camera
            if (_freecamGO != null) UnityEngine.Object.Destroy(_freecamGO);
            _freecamGO  = null;
            _freecamCam = null;
            _mainCam    = null;

            // Restore player body SkinnedMeshRenderers
            ShowPlayerBody(false);

            _freecam = false;
            FreecamModEntry.Log("Freecam disabled");
        }

        // ─── Player body show/hide ────────────────────────────────────────────

        // Primary approach: use PlayerLocationSync.localObjectsToDeactivate (the exact list
        // the game deactivated at Start) and recursively re-activate every GO in each subtree.
        // Because the game uses SetActiveRecursively(false), ALL descendants have activeSelf=false
        // — simply calling SetActive(true) on the root alone is not enough.
        private void ShowPlayerBody(bool show)
        {
            if (show)
            {
                _localBodyGOs = GetLocalBodyGOs();
                if (_localBodyGOs != null && _localBodyGOs.Length > 0)
                {
                    FreecamModEntry.Log("ShowPlayerBody: activating " + _localBodyGOs.Length + " localObjectsToDeactivate items");
                    for (int i = 0; i < _localBodyGOs.Length; i++)
                    {
                        if (_localBodyGOs[i] != null)
                        {
                            FreecamModEntry.Log("  -> " + _localBodyGOs[i].name);
                            ActivateSubtree(_localBodyGOs[i].transform);
                        }
                    }
                }
                else
                {
                    FreecamModEntry.Log("ShowPlayerBody: PLS not found, using SMR fallback");
                    ShowPlayerBodyFallback();
                }
            }
            else
            {
                // Restore via primary: deactivate each subtree recursively (mirrors SetActiveRecursively(false))
                if (_localBodyGOs != null)
                {
                    for (int i = 0; i < _localBodyGOs.Length; i++)
                        if (_localBodyGOs[i] != null)
                            DeactivateSubtree(_localBodyGOs[i].transform);
                    _localBodyGOs = null;
                }
                // Restore via fallback
                if (_playerSMRs != null)
                    for (int i = 0; i < _playerSMRs.Length; i++)
                        if (_playerSMRs[i] != null) _playerSMRs[i].enabled = _smrWasEnabled[i];
                if (_activatedGOs != null)
                    for (int i = _activatedGOs.Length - 1; i >= 0; i--)
                        if (_activatedGOs[i] != null) _activatedGOs[i].gameObject.SetActive(false);
                _playerSMRs = null; _smrWasEnabled = null; _activatedGOs = null;
            }
        }

        // Reflect into PlayerLocationSync to get the exact list of body GOs the game hid.
        // Returns null if the component or list cannot be found.
        private GameObject[] GetLocalBodyGOs()
        {
            Type plsType = null;
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                plsType = asm.GetType("PlayerLocationSync");
                if (plsType != null) break;
            }
            if (plsType == null) { FreecamModEntry.Log("GetLocalBodyGOs: PlayerLocationSync type not found"); return null; }

            UnityEngine.Object[] allPls = UnityEngine.Object.FindObjectsOfType(plsType);
            FreecamModEntry.Log("GetLocalBodyGOs: found " + allPls.Length + " PlayerLocationSync(s)");

            System.Reflection.FieldInfo localField = plsType.GetField("localObjectsToDeactivate");
            if (localField == null) { FreecamModEntry.Log("GetLocalBodyGOs: localObjectsToDeactivate field not found"); return null; }

            foreach (UnityEngine.Object obj in allPls)
            {
                Component comp = obj as Component;
                if (comp == null) continue;

                // Filter to local player only via photonView.isMine
                Component pv = comp.GetComponent("PhotonView");
                if (pv != null)
                {
                    System.Reflection.PropertyInfo isMineP = pv.GetType().GetProperty("isMine");
                    if (isMineP != null && !(bool)isMineP.GetValue(pv, null)) continue;
                }

                System.Collections.IList list = localField.GetValue(comp) as System.Collections.IList;
                if (list == null || list.Count == 0) continue;

                GameObject[] result = new GameObject[list.Count];
                for (int i = 0; i < list.Count; i++) result[i] = list[i] as GameObject;
                return result;
            }
            FreecamModEntry.Log("GetLocalBodyGOs: no local-player PLS with non-empty localObjectsToDeactivate found");
            return null;
        }

        // Recursively call SetActive(true) on a GO and all its descendants.
        // Needed because SetActiveRecursively(false) sets activeSelf=false on each node individually.
        private void ActivateSubtree(Transform t)
        {
            t.gameObject.SetActive(true);
            for (int i = 0; i < t.childCount; i++) ActivateSubtree(t.GetChild(i));
        }

        // Recursively call SetActive(false) bottom-up (mirrors SetActiveRecursively(false)).
        private void DeactivateSubtree(Transform t)
        {
            for (int i = 0; i < t.childCount; i++) DeactivateSubtree(t.GetChild(i));
            t.gameObject.SetActive(false);
        }

        // Fallback: find player by tag/name and activate inactive ancestor GOs up to root.
        private void ShowPlayerBodyFallback()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("ExampleCharacter");
            if (player == null) { FreecamModEntry.Log("ShowPlayerBodyFallback: player GO not found"); return; }
            Transform playerRoot = player.transform;

            SkinnedMeshRenderer[] allSMRs = player.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var toActivate = new System.Collections.Generic.List<Transform>();
            foreach (SkinnedMeshRenderer smr in allSMRs)
            {
                Transform t = smr.transform;
                while (t != null && t != playerRoot)
                {
                    if (!t.gameObject.activeSelf && !toActivate.Contains(t)) toActivate.Add(t);
                    t = t.parent;
                }
            }
            _activatedGOs = toActivate.ToArray();
            foreach (Transform t in _activatedGOs) t.gameObject.SetActive(true);

            _playerSMRs    = player.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            _smrWasEnabled = new bool[_playerSMRs.Length];
            for (int i = 0; i < _playerSMRs.Length; i++)
            {
                _smrWasEnabled[i]      = _playerSMRs[i].enabled;
                _playerSMRs[i].enabled = true;
            }
            FreecamModEntry.Log("ShowPlayerBodyFallback: activated " + _activatedGOs.Length +
                " GOs, enabled " + _playerSMRs.Length + " SMRs");
        }

        // ─── Freecam movement ─────────────────────────────────────────────────

        private void UpdateFreecam()
        {
            // Accumulate look input (mouse + on-screen buttons)
            float lookX = _guiLook.x;
            float lookY = _guiLook.y;
            if (Input.GetMouseButton(1))
            {
                lookX += Input.GetAxis("Mouse X");
                lookY += Input.GetAxis("Mouse Y");
            }
            if (Input.GetKey(KeyCode.LeftArrow))  lookX -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) lookX += 1f;
            if (Input.GetKey(KeyCode.UpArrow))    lookY += 1f;
            if (Input.GetKey(KeyCode.DownArrow))  lookY -= 1f;

            _freecamYaw   += lookX * FREECAM_LOOK_SENS;
            _freecamPitch -= lookY * FREECAM_LOOK_SENS;
            _freecamPitch  = Mathf.Clamp(_freecamPitch, -89f, 89f);

            // Accumulate move input
            Vector3 move = _guiMove;
            if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) move += Vector3.back;
            if (Input.GetKey(KeyCode.A)) move += Vector3.left;
            if (Input.GetKey(KeyCode.D)) move += Vector3.right;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))         move += Vector3.up;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))   move += Vector3.down;

            float speed = FREECAM_SPEED;
            if (_guiFast || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speed *= FREECAM_FAST_MULT;

            Quaternion rot = Quaternion.Euler(_freecamPitch, _freecamYaw, 0f);
            if (move.sqrMagnitude > 0.001f)
                _freecamPos += rot * move.normalized * speed * Time.deltaTime;
        }

        // Applied in LateUpdate so it runs after all player movement scripts
        private void ApplyFreecam()
        {
            if (_freecamCam == null || _freecamGO == null) { DisableFreecam(); return; }
            _freecamGO.transform.position = _freecamPos;
            _freecamGO.transform.rotation = Quaternion.Euler(_freecamPitch, _freecamYaw, 0f);
        }

        // ─── Scene renderer dump (G key) ─────────────────────────────────────

        private static string GetPath(Transform t)
        {
            if (t.parent == null) return t.gameObject.name;
            return GetPath(t.parent) + "/" + t.gameObject.name;
        }

        private void DumpSceneRenderers()
        {
            FreecamModEntry.Log("=== DumpSceneRenderers ===");

            // Part 1: ALL actively-rendering objects (FindObjectsOfType = scene only, active in hierarchy)
            UnityEngine.Object[] activeRens = (UnityEngine.Object[])FindObjectsOfType(typeof(Renderer));
            FreecamModEntry.Log("Active-in-hierarchy Renderers: " + activeRens.Length);
            foreach (UnityEngine.Object o in activeRens)
            {
                Renderer r = o as Renderer;
                if (r == null) continue;
                string renEnabled = r.enabled ? "ON" : "off";
                string layer = LayerMask.LayerToName(r.gameObject.layer);
                FreecamModEntry.Log(string.Format("  ACTIVE [{0}] {1} | ren={2} | layer={3}",
                    r.GetType().Name, GetPath(r.transform), renEnabled, layer));
            }

            // Part 2: Walk the ExampleCharacter hierarchy (all children, incl. inactive)
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("ExampleCharacter");
            FreecamModEntry.Log("ExampleCharacter found: " + (player != null ? player.name : "NULL"));
            if (player != null)
            {
                FreecamModEntry.Log("-- ExampleCharacter subtree (all Renderers incl. inactive) --");
                Renderer[] sub = player.GetComponentsInChildren<Renderer>(true);
                FreecamModEntry.Log("  Total Renderers under player root: " + sub.Length);
                foreach (Renderer r in sub)
                {
                    string goActive  = r.gameObject.activeInHierarchy ? "ACTIVE" : "inactive";
                    string renEnabled = r.enabled ? "ON" : "off";
                    FreecamModEntry.Log(string.Format("  [{0}] {1} | go={2} | ren={3}",
                        r.GetType().Name, GetPath(r.transform), goActive, renEnabled));
                }
            }

            // Part 3: Look for Cop_Fixed / Robber_Fixed body GOs (3rd-person body in multiplayer)
            FreecamModEntry.Log("-- Looking for 3rd-person body GOs --");
            string[] bodyNames = new string[] { "Cop_Fixed", "Robber_Fixed", "PlayerBody", "Body" };
            foreach (string bname in bodyNames)
            {
                GameObject bg = GameObject.Find(bname);
                FreecamModEntry.Log("  " + bname + ": " + (bg != null ? "FOUND active=" + bg.activeInHierarchy : "not found"));
            }

            FreecamModEntry.Log("=== End DumpSceneRenderers ===");
        }

        // ─── Collider status ──────────────────────────────────────────────────

        private void RefreshColliderStatus()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("ExampleCharacter");
            if (player == null) { _colliderStatus = "[ player not found ]"; return; }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== CNRSettingsMod Collider Status ===");

            Collider[] cols = player.GetComponentsInChildren<Collider>(true);
            foreach (Collider c in cols)
            {
                if (c is CharacterController) continue;
                string state = c.enabled ? "<color=lime>ENABLED</color>" : "<color=red>DISABLED</color>";
                sb.AppendLine("  [" + c.gameObject.name + "]  " + state);
            }

            // CharacterController
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                sb.AppendLine("  [CharacterController]  detectCollisions=" +
                    (cc.detectCollisions ? "<color=lime>true</color>" : "<color=red>false</color>"));

            _colliderStatus = sb.ToString();
        }

        // ─── GUI ──────────────────────────────────────────────────────────────

        private void DrawGui()
        {
            float ui = Mathf.Max(1f, Screen.height / 720f);

            // Freecam toggle button (always visible)
            if (GUI.Button(new Rect(8f * ui, 198f * ui, 126f * ui, 38f * ui),
                           _freecam ? "Freecam ON" : "Freecam"))
                ToggleFreecam();

            // Hint label
            if (!_freecam)
            {
                GUIStyle hint = new GUIStyle(GUI.skin.label);
                hint.fontSize = (int)(11f * ui);
                hint.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
                GUI.Label(new Rect(8f * ui, 238f * ui, 200f * ui, 20f * ui),
                    "F6 toggle  |  F = collider info", hint);
                goto colliderDisplay;
            }

            // ── Freecam D-pad (touch) ──────────────────────────────────────────
            _guiMove = Vector3.zero;
            _guiLook = Vector2.zero;
            _guiFast = false;

            float x = 8f * ui;
            float y = 244f * ui;
            float s = 48f * ui;
            if (GUI.RepeatButton(new Rect(x + s,         y,       s, s), "W")) _guiMove += Vector3.forward;
            if (GUI.RepeatButton(new Rect(x,             y + s,   s, s), "A")) _guiMove += Vector3.left;
            if (GUI.RepeatButton(new Rect(x + s,         y + s,   s, s), "S")) _guiMove += Vector3.back;
            if (GUI.RepeatButton(new Rect(x + s * 2f,    y + s,   s, s), "D")) _guiMove += Vector3.right;
            if (GUI.RepeatButton(new Rect(x,             y+s*2f,  s, s), "Q")) _guiMove += Vector3.down;
            if (GUI.RepeatButton(new Rect(x + s,         y+s*2f,  s, s), "E")) _guiMove += Vector3.up;
            if (GUI.RepeatButton(new Rect(x + s * 2f,   y+s*2f,  s, s), "Fast")) _guiFast = true;

            x = 178f * ui;
            if (GUI.RepeatButton(new Rect(x,             y,       s, s), "<"))  _guiLook.x -= 1f;
            if (GUI.RepeatButton(new Rect(x + s,         y,       s, s), "^"))  _guiLook.y += 1f;
            if (GUI.RepeatButton(new Rect(x + s * 2f,   y,       s, s), ">"))  _guiLook.x += 1f;
            if (GUI.RepeatButton(new Rect(x + s,         y + s,   s, s), "v"))  _guiLook.y -= 1f;

            // Position readout
            GUIStyle pos = new GUIStyle(GUI.skin.label);
            pos.fontSize = (int)(12f * ui);
            pos.normal.textColor = Color.white;
            GUI.Label(new Rect(8f * ui, (y + s * 4f), 320f * ui, 22f * ui),
                string.Format("pos ({0:F1}, {1:F1}, {2:F1})  yaw {3:F0}°",
                    _freecamPos.x, _freecamPos.y, _freecamPos.z, _freecamYaw), pos);


            // Freecam label header
            GUIStyle header = new GUIStyle(GUI.skin.label);
            header.fontSize  = (int)(14f * ui);
            header.fontStyle = FontStyle.Bold;
            header.normal.textColor = Color.cyan;
            GUI.Label(new Rect(8f * ui, 178f * ui, 300f * ui, 22f * ui),
                "[FREECAM]  F6 = exit  |  RMB/arrows = look  |  WASD Q/E = move", header);

            colliderDisplay:
            // ── Joke collider display ──────────────────────────────────────────
            if (_colliderDisplayTimer > 0f && _colliderStatus.Length > 0)
            {
                // Semi-transparent background box
                float alpha = Mathf.Min(1f, _colliderDisplayTimer);
                GUI.color = new Color(0f, 0f, 0f, 0.72f * alpha);
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - 220f * ui, Screen.height * 0.25f,
                    440f * ui, 320f * ui), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUIStyle cs = new GUIStyle(GUI.skin.label);
                cs.fontSize  = (int)(13f * ui);
                cs.richText  = true;
                cs.normal.textColor = Color.white;
                GUI.Label(new Rect(Screen.width * 0.5f - 210f * ui, Screen.height * 0.25f + 8f * ui,
                    420f * ui, 300f * ui), _colliderStatus, cs);
            }
        }
    }
}
