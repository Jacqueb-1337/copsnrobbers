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
        public const  string Version = "0.2.2";
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
        // Only SkinnedMeshRenderers (actual character mesh) are touched — NOT
        // MeshRenderers, which would include the capsule collider visual.
        private SkinnedMeshRenderer[] _playerSMRs;
        private bool[]                _smrWasEnabled;

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

        // ─── Freecam movement ─────────────────────────────────────────────────

        // Enable (or restore) only SkinnedMeshRenderers on the local player.
        private void ShowPlayerBody(bool show)
        {
            if (show)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("ExampleCharacter");
                if (player == null) { FreecamModEntry.Log("Freecam: player GO not found"); return; }

                _playerSMRs    = player.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                _smrWasEnabled = new bool[_playerSMRs.Length];
                for (int i = 0; i < _playerSMRs.Length; i++)
                {
                    _smrWasEnabled[i]       = _playerSMRs[i].enabled;
                    _playerSMRs[i].enabled  = true;
                }
                FreecamModEntry.Log("Freecam: enabled " + _playerSMRs.Length + " SMRs");
            }
            else
            {
                if (_playerSMRs != null)
                {
                    for (int i = 0; i < _playerSMRs.Length; i++)
                    {
                        if (_playerSMRs[i] != null)
                            _playerSMRs[i].enabled = _smrWasEnabled[i];
                    }
                }
                _playerSMRs    = null;
                _smrWasEnabled = null;
            }
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
