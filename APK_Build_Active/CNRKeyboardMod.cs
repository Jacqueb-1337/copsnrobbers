// CNRKeyboardMod.cs — Keyboard + mouse controls for Cops N Robbers on WSA
//
// Controls (active only during gameplay scenes):
//   WASD        — Move
//   Space       — Jump
//   Left click  — Fire (locks cursor first click)
//   Mouse       — Look (yaw + pitch)
//   Scroll up   — Next weapon  (1/2 keys also work natively)
//   R           — Reload       (already handled by game)
//   Escape      — Unlock cursor / pause
//
// Click anywhere in the game scene to lock the cursor and enable KB+Mouse.
// Press Escape to unlock.

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    // ── Entry point ───────────────────────────────────────────────────────────
    public static class KeyboardMouseEntry
    {
        public static void Initialize()
        {
            try
            {
                GameObject go = new GameObject("CNRKeyboardMod");
                go.AddComponent<KeyboardMouseHook>();
                GameObject.DontDestroyOnLoad(go);
                ModEntry.Log("KeyboardMouseEntry: initialized");
            }
            catch (Exception ex)
            {
                ModEntry.Log("KeyboardMouseEntry.Initialize() error: " + ex);
            }
        }
    }

    // ── Main hook ─────────────────────────────────────────────────────────────
    public class KeyboardMouseHook : MonoBehaviour
    {
        // ── Scene detection ──────────────────────────────────────────────────
        //   Gameplay scenes start with these prefixes.
        private static readonly string[] GAME_PREFIXES = { "FreeRun", "SingleMode" };

        private bool _inGameScene  = false;
        private bool _cursorLocked = false;

        // ── Joystick injection ────────────────────────────────────────────────
        //   VCAnalogJoystickBase._deltaPixels drives AxisX/AxisY.
        //   dragDeltaMagnitudeMaxPixels (default 60) is the full-stick magnitude.
        private MonoBehaviour _joystick      = null;
        private FieldInfo     _fiDeltaPixels = null;
        private float         _dragMax       = 60f;

        // ── Camera injection (Sliderotate) ────────────────────────────────────
        //   rotationX  → player yaw  (transform.localEulerAngles.y)
        //   rotationY  → camera pitch (cameratransform.localEulerAngles.x)
        private MonoBehaviour _sliderotate    = null;
        private FieldInfo     _fiRotationX    = null;
        private FieldInfo     _fiRotationY    = null;
        private FieldInfo     _fiCamTransform = null;
        private FieldInfo     _fiMinY         = null;
        private FieldInfo     _fiMaxY         = null;

        // ── Weapon scroll ─────────────────────────────────────────────────────
        private float _scrollAccum = 0f;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Start()
        {
            _inGameScene = IsGameScene(Application.loadedLevelName);
        }

        private void OnLevelWasLoaded(int level)
        {
            string scene = Application.loadedLevelName;
            _inGameScene  = IsGameScene(scene);
            // Reset cached references — scene objects are recreated
            _joystick     = null;
            _sliderotate  = null;
            SetCursorLocked(false);
            ModEntry.Log("KeyboardMod: scene=" + scene + " inGame=" + _inGameScene);
        }

        private void OnDestroy()
        {
            SetCursorLocked(false);
        }

        // ── Update ────────────────────────────────────────────────────────────
        private void Update()
        {
            if (!_inGameScene) return;

            // Escape: unlock cursor
            if (Input.GetKeyDown(KeyCode.Escape) && _cursorLocked)
            {
                SetCursorLocked(false);
                return;
            }

            // Any left click in an unlocked state: lock cursor + enable controls
            if (Input.GetMouseButtonDown(0) && !_cursorLocked)
            {
                SetCursorLocked(true);
                // Don't fire on the lock click itself
                return;
            }

            if (!_cursorLocked) return;

            // ── Jump ─────────────────────────────────────────────────────────
            if (Input.GetKeyDown(KeyCode.Space))
                PlayerPrefs.SetInt("OnJump", 1);

            // ── Weapon scroll (scroll wheel → simulate 1/2 key presses) ──────
            //   The game already handles KeyCode 49 (1) and KeyCode 50 (2).
            //   We accumulate scroll and send synthetic state changes.
            _scrollAccum += Input.GetAxis("Mouse ScrollWheel");
            if (_scrollAccum >= 0.1f)
            {
                _scrollAccum = 0f;
                // Simulate pressing "2" via PlayerPrefs weapon index (best-effort)
                int cur = PlayerPrefs.GetInt("WeaponIndex", 0);
                PlayerPrefs.SetInt("WeaponIndex", (cur + 1) % 2);
            }
            else if (_scrollAccum <= -0.1f)
            {
                _scrollAccum = 0f;
                int cur = PlayerPrefs.GetInt("WeaponIndex", 0);
                PlayerPrefs.SetInt("WeaponIndex", ((cur - 1) % 2 + 2) % 2);
            }

            // ── Fire (left mouse button) ──────────────────────────────────────
            if (Input.GetMouseButton(0)
                && (object)PlayerLogic.mInstance != null
                && !PlayerLogic.mInstance.bDied)
            {
                WeaponType wt = PlayerLogic.mInstance.mWeaponType;
                if (wt == WeaponType.BallisticKnife || wt == WeaponType.GingerbreadKnife)
                    PlayerLogic.mInstance.mStatus = PlayerStatus.knifeFire;
                else
                    PlayerLogic.mInstance.mStatus = PlayerStatus.fire;
            }

            // ── Movement (WASD → joystick axis injection) ─────────────────────
            InjectJoystick();
        }

        // ── LateUpdate ────────────────────────────────────────────────────────
        //   Runs AFTER all Update()s, so Sliderotate has already processed
        //   any touch. We add the mouse delta on top.
        private void LateUpdate()
        {
            if (!_inGameScene || !_cursorLocked) return;

            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            if (mx == 0f && my == 0f) return;

            InjectMouseLook(mx, my);
        }

        // ── Joystick injection ────────────────────────────────────────────────
        private void InjectJoystick()
        {
            float ax = 0f, ay = 0f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ax += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  ax -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    ay += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  ay -= 1f;

            // Normalise diagonals to prevent faster diagonal movement
            if (ax != 0f && ay != 0f) { ax *= 0.7071f; ay *= 0.7071f; }

            // Cache the joystick MonoBehaviour + reflection fields
            if (_joystick == null)
            {
                VCAnalogJoystickBase inst = VCAnalogJoystickBase.GetInstance("stick");
                if (inst == null) return;

                _joystick = (MonoBehaviour)(object)inst;

                // Walk up the inheritance chain to reach VCAnalogJoystickBase
                Type t = _joystick.GetType();
                while (t != null && t.Name != "VCAnalogJoystickBase")
                    t = t.BaseType;
                if (t == null) t = _joystick.GetType();

                _fiDeltaPixels = t.GetField("_deltaPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fiDragMax = t.GetField("dragDeltaMagnitudeMaxPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiDragMax != null) _dragMax = (float)fiDragMax.GetValue(_joystick);

                ModEntry.Log("KeyboardMod: joystick hooked, dragMax=" + _dragMax);
            }

            if (_fiDeltaPixels == null) return;

            // Set _deltaPixels so AxisX/AxisY return the desired normalised values
            Vector2 delta = new Vector2(ax * _dragMax, ay * _dragMax);
            _fiDeltaPixels.SetValue(_joystick, delta);
        }

        // ── Mouse look injection ──────────────────────────────────────────────
        private void InjectMouseLook(float mx, float my)
        {
            // Cache the Sliderotate component + reflection fields
            if (_sliderotate == null)
            {
                UnityEngine.Object[] all =
                    Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
                foreach (UnityEngine.Object obj in all)
                {
                    MonoBehaviour mb = obj as MonoBehaviour;
                    if (mb == null || mb.GetType().Name != "Sliderotate") continue;
                    if (!mb.gameObject.activeInHierarchy)               continue;

                    _sliderotate  = mb;
                    Type t        = mb.GetType();
                    _fiRotationX    = t.GetField("rotationX",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    _fiRotationY    = t.GetField("rotationY",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    _fiCamTransform = t.GetField("cameratransform",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    _fiMinY         = t.GetField("minimumY",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    _fiMaxY         = t.GetField("maximumY",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    ModEntry.Log("KeyboardMod: Sliderotate hooked");
                    break;
                }
            }

            if (_sliderotate == null || _fiRotationX == null) return;

            // Sensitivity matches the game's own slider (default 3.2)
            float sens = PlayerPrefs.GetFloat("Sensitivity", 3.2f);

            float rotX = (float)_fiRotationX.GetValue(_sliderotate);
            float rotY = (float)_fiRotationY.GetValue(_sliderotate);
            float minY = (_fiMinY != null) ? (float)_fiMinY.GetValue(_sliderotate) : -35f;
            float maxY = (_fiMaxY != null) ? (float)_fiMaxY.GetValue(_sliderotate) : 35f;

            rotX += mx * sens;
            rotY += my * sens;   // note: game renders as -rotY on X euler, so this is correct
            rotY  = Mathf.Clamp(rotY, minY, maxY);

            _fiRotationX.SetValue(_sliderotate, rotX);
            _fiRotationY.SetValue(_sliderotate, rotY);

            // Apply to transforms exactly as Sliderotate does
            (_sliderotate as Component).transform.localEulerAngles =
                new Vector3(0f, rotX, 0f);

            if (_fiCamTransform != null)
            {
                Transform camT = (Transform)_fiCamTransform.GetValue(_sliderotate);
                if (camT != null)
                    camT.localEulerAngles = new Vector3(-rotY, 0f, 0f);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void SetCursorLocked(bool locked)
        {
            _cursorLocked      = locked;
            Screen.lockCursor  = locked;
            Screen.showCursor  = !locked;
        }

        private static bool IsGameScene(string scene)
        {
            foreach (string p in GAME_PREFIXES)
                if (scene.StartsWith(p)) return true;
            return false;
        }

        // ── HUD overlay ───────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!_inGameScene) return;

            if (!_cursorLocked)
            {
                // Prompt bar at bottom of screen
                GUIStyle box = new GUIStyle(GUI.skin.box);
                box.fontSize  = 15;
                box.alignment = TextAnchor.MiddleCenter;
                GUI.color = new Color(1f, 1f, 1f, 0.80f);
                GUI.Box(new Rect(Screen.width * 0.25f, Screen.height - 38f,
                    Screen.width * 0.5f, 30f),
                    "[CNR Mod]  Click to enable Keyboard + Mouse  |  ESC = unlock", box);
            }
            else
            {
                // Crosshair
                float cx = Screen.width  * 0.5f;
                float cy = Screen.height * 0.5f;
                GUI.color = new Color(1f, 1f, 1f, 0.90f);
                // Vertical bar
                GUI.DrawTexture(new Rect(cx - 1f, cy - 10f,  2f, 20f), Texture2D.whiteTexture);
                // Horizontal bar
                GUI.DrawTexture(new Rect(cx - 10f, cy - 1f, 20f,  2f), Texture2D.whiteTexture);
                // Centre dot
                GUI.DrawTexture(new Rect(cx - 2f, cy - 2f,   4f,  4f), Texture2D.whiteTexture);

                // Control hint (bottom-left corner)
                GUIStyle hint = new GUIStyle(GUI.skin.box);
                hint.fontSize  = 13;
                hint.alignment = TextAnchor.UpperLeft;
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
                GUI.Box(new Rect(8f, Screen.height - 84f, 230f, 74f),
                    "WASD / Arrows : Move\n" +
                    "Space         : Jump\n" +
                    "LMB           : Fire\n" +
                    "Scroll / 1-2  : Weapon   ESC: Unlock", hint);
            }
        }
    }
}
