// CNRSettingsMod.cs -- In-game settings/HUD mod for Cops N Robbers
// Entry point: CNRSettingsMod.SettingsModEntry.Load() -- called by IPRedirectMod DLL scanner

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CNRSettingsMod
{
    // Visibility toggle descriptor (whole panels)
    internal struct HudVisItem
    {
        public string displayName;
        public string goName;
        public string prefKey;
    }

    // Draggable element descriptor (individual buttons or panels)
    internal struct HudDragItem
    {
        public string   displayName;
        public string   parentName;      // non-null: find as child of this GO
        public string[] nameCandidates;  // try each until one is found
        public string   prefPX;
        public string   prefPY;
        public string   prefSZ;          // uniform scale pref key (optional)
    }

    // -------------------------------------------------------------------------
    // Entry point
    // -------------------------------------------------------------------------
    public static class SettingsModEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/settings.log";

        public static void Load()
        {
            try
            {
                GameObject go = new GameObject("CNRSettingsMod");
                go.AddComponent<SettingsModHook>();
                GameObject.DontDestroyOnLoad(go);
                Log("CNRSettingsMod loaded");
            }
            catch (Exception ex) { Log("Load() error: " + ex); }
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); } catch { }
            try { Debug.Log("[CNRSettings] " + msg); } catch { }
        }
    }

    // -------------------------------------------------------------------------
    // Main hook MonoBehaviour
    // -------------------------------------------------------------------------
    public class SettingsModHook : MonoBehaviour
    {
        // -- HUD visibility toggles (whole panels) ----------------------------
        private static readonly HudVisItem[] VIS_ITEMS = new HudVisItem[]
        {
            new HudVisItem { displayName="Touch controls", goName="Panel(Control)",    prefKey="CNRMod_HUD_Controls" },
            new HudVisItem { displayName="Toolbar",        goName="Panel(ToolBar)",    prefKey="CNRMod_HUD_ToolBar"  },
            new HudVisItem { displayName="Chat bar",       goName="Panel(ChatBar)",    prefKey="CNRMod_HUD_ChatBar"  },
            new HudVisItem { displayName="ADS/Aim button", goName="Image Button(Aim)", prefKey="CNRMod_HUD_Aim"      },
        };
        private bool[]       _visOn  = new bool[]       { true, true, true, true };
        private GameObject[] _visGOs = new GameObject[4];

        // -- Draggable HUD items (all repositionable / resizable) -------------
        // Index order must stay stable (prefs are keyed by prefPX/PY/SZ strings)
        private static readonly HudDragItem[] DRAG_ITEMS = new HudDragItem[]
        {
            // 0 -- Panel(Control) children
            new HudDragItem {
                displayName    = "Fire",
                parentName     = "Panel(Control)",
                nameCandidates = new string[]{ "FireButton", "Image Button(Shoot)", "Button(Shoot)", "Image Button(Fire)", "Button(Fire)" },
                prefPX = "CNRMod_PX_Fire",   prefPY = "CNRMod_PY_Fire",   prefSZ = "CNRMod_SZ_Fire"   },
            // 1
            new HudDragItem {
                displayName    = "Jump",
                parentName     = "Panel(Control)",
                nameCandidates = new string[]{ "Image Button(Jump)", "Button(Jump)" },
                prefPX = "CNRMod_PX_Jump",   prefPY = "CNRMod_PY_Jump",   prefSZ = "CNRMod_SZ_Jump"   },
            // 2
            new HudDragItem {
                displayName    = "Reload",
                parentName     = "Panel(Control)",
                nameCandidates = new string[]{ "Image Button(Reload)", "Button(Reload)" },
                prefPX = "CNRMod_PX_Reload", prefPY = "CNRMod_PY_Reload", prefSZ = "CNRMod_SZ_Reload" },
            // 3
            new HudDragItem {
                displayName    = "Record",
                parentName     = "Panel(Control)",
                nameCandidates = new string[]{ "Image Button(Record)", "Button(Record)", "Image Button(Video)", "Button(Video)", "Image Button(Screenshot)" },
                prefPX = "CNRMod_PX_Record", prefPY = "CNRMod_PY_Record", prefSZ = "CNRMod_SZ_Record" },
            // 4
            new HudDragItem {
                displayName    = "ADS/Aim",
                parentName     = "Panel(Control)",
                nameCandidates = new string[]{ "Image Button(Aim)" },
                prefPX = "CNRMod_PX_Aim",    prefPY = "CNRMod_PY_Aim",    prefSZ = "CNRMod_SZ_Aim"    },
            // 5 -- top-level panels / misc
            new HudDragItem {
                displayName    = "Toolbar",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(ToolBar)" },
                prefPX = "CNRMod_PX_ToolBar", prefPY = "CNRMod_PY_ToolBar", prefSZ = "CNRMod_SZ_ToolBar" },
            // 6
            new HudDragItem {
                displayName    = "Chat bar",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(ChatBar)" },
                prefPX = "CNRMod_PX_ChatBar", prefPY = "CNRMod_PY_ChatBar", prefSZ = "CNRMod_SZ_ChatBar" },
            // 7 -- Panel(LeftMenu): contains HP blood display, health pack btn, player list btn
            new HudDragItem {
                displayName    = "HP panel",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(LeftMenu)" },
                prefPX = "CNRMod_PX_HP",  prefPY = "CNRMod_PY_HP",  prefSZ = "CNRMod_SZ_HP"  },
            // 8 -- Panel(Top): whole top bar (cop+robber resource/objective bars, team head icons, schedule)
            new HudDragItem {
                displayName    = "Top panel",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(Top)" },
                prefPX = "CNRMod_PX_TS1", prefPY = "CNRMod_PY_TS1", prefSZ = "CNRMod_SZ_TS1" },
            // 9 -- Gun icon / switch button (moves only the gun sprite, not arrows or ammo)
            new HudDragItem {
                displayName    = "Gun icon",
                parentName     = "Panel(RightMenu)",
                nameCandidates = new string[]{ "SwitchButton" },
                prefPX = "CNRMod_PX_TS2", prefPY = "CNRMod_PY_TS2", prefSZ = "CNRMod_SZ_TS2" },
            // 10 -- Pause button (independent from weapon group)
            new HudDragItem {
                displayName    = "Pause btn",
                parentName     = null,
                nameCandidates = new string[]{ "Image Button(Pause)" },
                prefPX = "CNRMod_PX_Pause", prefPY = "CNRMod_PY_Pause", prefSZ = "CNRMod_SZ_Pause" },
            // 11 -- Player list / leaderboard button in Panel(LeftMenu)
            new HudDragItem {
                displayName    = "Player list",
                parentName     = "Panel(LeftMenu)",
                nameCandidates = new string[]{ "Image Button(InnerLeaderboard)" },
                prefPX = "CNRMod_PX_PList", prefPY = "CNRMod_PY_PList", prefSZ = "CNRMod_SZ_PList" },
            // 12 -- Health pack / medikit button in Panel(LeftMenu)
            new HudDragItem {
                displayName    = "Health pack",
                parentName     = "Panel(LeftMenu)",
                nameCandidates = new string[]{ "Image Button(MediKit)" },
                prefPX = "CNRMod_PX_HPack", prefPY = "CNRMod_PY_HPack", prefSZ = "CNRMod_SZ_HPack" },
            // 13 -- Ammo buy button (independent from weapon group)
            new HudDragItem {
                displayName    = "Ammo buy",
                parentName     = null,
                nameCandidates = new string[]{ "Image Button(AddBullet)" },
                prefPX = "CNRMod_PX_Ammo", prefPY = "CNRMod_PY_Ammo", prefSZ = "CNRMod_SZ_Ammo" },
            // 14 -- Prev gun arrow inside Panel(RightMenu)
            new HudDragItem {
                displayName    = "Prev gun",
                parentName     = "Panel(RightMenu)",
                nameCandidates = new string[]{ "Image Button(LeftSwitch)" },
                prefPX = "CNRMod_PX_PrevG", prefPY = "CNRMod_PY_PrevG", prefSZ = "CNRMod_SZ_PrevG" },
            // 15 -- Next gun arrow inside Panel(RightMenu)
            new HudDragItem {
                displayName    = "Next gun",
                parentName     = "Panel(RightMenu)",
                nameCandidates = new string[]{ "Image Button(RightSwitch)" },
                prefPX = "CNRMod_PX_NextG", prefPY = "CNRMod_PY_NextG", prefSZ = "CNRMod_SZ_NextG" },
            // 16 -- Ammo count label inside Panel(RightMenu)
            new HudDragItem {
                displayName    = "Ammo count",
                parentName     = "Panel(RightMenu)",
                nameCandidates = new string[]{ "Bullets" },
                prefPX = "CNRMod_PX_WpSel", prefPY = "CNRMod_PY_WpSel", prefSZ = "CNRMod_SZ_WpSel" },
        };
        private const int DRAG_COUNT = 17;
        private GameObject[] _dragGOs      = new GameObject[DRAG_COUNT];
        private Vector3[]    _dragOrigPos  = new Vector3[DRAG_COUNT];
        private Vector3[]    _dragOrigScale = new Vector3[DRAG_COUNT];

        // -- Scene state -------------------------------------------------------
        private bool _inGameScene  = false;
        private bool _showSettings = false;
        private bool _btnPatched   = false;

        // -- Sliderotate reflection cache --------------------------------------
        private MonoBehaviour _sliderotate;
        private FieldInfo _fiSensX, _fiSensY;
        private FieldInfo _fiRotX,  _fiRotY;
        private FieldInfo _fiMinY,  _fiMaxY;
        private FieldInfo _fiCamT;
        private float _prevRotX, _prevRotY;

        // Scoped weapon type int values: STW_25=3 M87T=4 AWP=7 ChristmasSniper=16 FRF2=25
        private static readonly int[] SCOPED_WEAPONS = { 3, 4, 7, 16, 25 };

        // -- Settings values ---------------------------------------------------
        private float _sensNormal = 3.2f;
        private float _sensScoped = 0.4f;
        private bool  _noAccel    = false;
        private float _maxDelta   = 15f;

        // -- IMGUI state -------------------------------------------------------
        private Rect    _winRect;
        private Vector2 _scroll;
        private const float REF_W = 600f;
        // Touch scroll tracking for settings panel
        private int   _scrollTouchId   = -1;
        private float _scrollTouchLastY = 0f;

        // -- HUD drag editor ---------------------------------------------------
        private bool    _hudEditMode    = false;
        private int     _draggingIdx    = -1;    // index being dragged (-1 = none)
        private Vector2 _dragOffset     = Vector2.zero;
        private int     _resizingIdx    = -1;    // index being resized (-1 = none)
        private float   _resizeStartDist = 0f;   // screen-pixel dist from center when resize began
        private Vector3 _resizeStartScale = Vector3.one;
        private bool    _suppressSaveIdx = false; // don't save on next touch-up (after reset)
        private int     _suppressSaveFor = -1;
        private Camera  _nguiCam        = null;

        // -- Pause panel polling -----------------------------------------------
        private GameObject _pausePanelRef;
        private UIPanel    _pauseUIPanel;
        private bool       _wasPauseVisible = false;

        // =====================================================================
        // Lifecycle
        // =====================================================================
        private void OnEnable()  { UIMenuDirector.OnGamePaused += OnGamePaused; }
        private void OnDisable() { UIMenuDirector.OnGamePaused -= OnGamePaused; }
        private void OnDestroy() { UIMenuDirector.OnGamePaused -= OnGamePaused; }
        private void Start()     { UpdateScene(Application.loadedLevelName); }

        private void OnLevelWasLoaded(int level)
        {
            UpdateScene(Application.loadedLevelName);
            SettingsModEntry.Log("scene=" + Application.loadedLevelName + " inGame=" + _inGameScene);
        }

        private void UpdateScene(string scene)
        {
            _inGameScene      = IsGameScene(scene);
            _btnPatched       = false;
            _showSettings     = false;
            _hudEditMode      = false;
            _draggingIdx      = -1;
            _resizingIdx      = -1;
            _suppressSaveIdx  = false;
            _suppressSaveFor  = -1;
            _sliderotate      = null;
            _nguiCam          = null;
            _pausePanelRef    = null;
            _pauseUIPanel     = null;
            _wasPauseVisible  = false;
            for (int i = 0; i < DRAG_COUNT; i++) { _dragGOs[i] = null; _dragOrigPos[i] = Vector3.zero; _dragOrigScale[i] = Vector3.zero; }
            for (int i = 0; i < _visGOs.Length;  i++) _visGOs[i]  = null;
            LoadPrefs();
            if (_inGameScene) StartCoroutine(ApplyHUDOnLoad());
        }

        private IEnumerator ApplyHUDOnLoad()
        {
            yield return null; yield return null;
            ReCacheHUD();
            CacheNguiCam();
        }

        // =====================================================================
        // Pause panel hook
        // =====================================================================
        private void OnGamePaused()
        {
            if (_inGameScene && !_btnPatched) StartCoroutine(PatchAfterFrame());
        }

        private void Update()
        {
            if (!_inGameScene) return;

            if (_pausePanelRef == null)
            {
                _pausePanelRef = GameObject.Find("Panel(Pause)");
                if (_pausePanelRef != null)
                    _pauseUIPanel = _pausePanelRef.GetComponent<UIPanel>();
            }

            // Only poll pause visibility when NOT in edit mode (panel is hidden then)
            if (_pausePanelRef != null && !_hudEditMode)
            {
                bool nowVisible = _pauseUIPanel != null
                    ? _pauseUIPanel.alpha > 0.5f
                    : _pausePanelRef.activeSelf;

                if (nowVisible && !_wasPauseVisible && !_btnPatched)
                    StartCoroutine(PatchAfterFrame());
                if (!nowVisible && _wasPauseVisible)
                    _btnPatched = false;
                _wasPauseVisible = nowVisible;
            }

            if (_hudEditMode) HandleHudDrag();
            if (_showSettings && !_hudEditMode) HandleSettingsScroll();
        }

        private void HandleSettingsScroll()
        {
            if (Input.touchCount == 0) { _scrollTouchId = -1; return; }
            float scale = Screen.width / REF_W;
            for (int ti = 0; ti < Input.touchCount; ti++)
            {
                Touch t = Input.GetTouch(ti);
                // Convert screen pos to scaled GUI space (Y flipped)
                Vector2 guiPos = new Vector2(t.position.x / scale, (Screen.height - t.position.y) / scale);
                if (t.phase == TouchPhase.Began)
                {
                    // Start tracking only if touch is inside the settings window
                    if (_winRect.Contains(guiPos))
                    {
                        _scrollTouchId    = t.fingerId;
                        _scrollTouchLastY = t.position.y;
                    }
                }
                else if (t.fingerId == _scrollTouchId &&
                         (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
                {
                    float delta = _scrollTouchLastY - t.position.y; // positive = scroll down
                    _scroll.y      = Mathf.Max(0f, _scroll.y + delta);
                    _scrollTouchLastY = t.position.y;
                }
                else if (t.fingerId == _scrollTouchId &&
                         (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
                    _scrollTouchId = -1;
            }
        }

        private IEnumerator PatchAfterFrame()
        {
            yield return null;
            PatchRestartButton();
        }

        private void PatchRestartButton()
        {
            GameObject pausePanel = GameObject.Find("Panel(Pause)");
            MonoBehaviour[] comps = pausePanel != null
                ? pausePanel.GetComponentsInChildren<MonoBehaviour>(true)
                : (MonoBehaviour[])(object)UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));

            foreach (MonoBehaviour mb in comps)
            {
                if (mb.GetType().Name != "UIButtonEventKit") continue;
                FieldInfo fi = mb.GetType().GetField("buttonName", BindingFlags.Instance | BindingFlags.Public);
                int val = fi != null ? (int)(object)fi.GetValue(mb) : -1;
                UILabel[] dbg = ((Component)mb).GetComponentsInChildren<UILabel>(true);
                string lbl = (dbg != null && dbg.Length > 0) ? dbg[0].text : "";
                SettingsModEntry.Log("btn=" + val + " lbl='" + lbl + "' go=" + ((Component)mb).gameObject.name);
            }

            MonoBehaviour target = null;
            foreach (MonoBehaviour mb in comps)
            {
                if (mb.GetType().Name != "UIButtonEventKit") continue;
                FieldInfo fi = mb.GetType().GetField("buttonName", BindingFlags.Instance | BindingFlags.Public);
                if (fi == null) continue;
                int val = (int)(object)fi.GetValue(mb);
                if (val == 49) { target = mb; break; }
                if (val == 4 && target == null) target = mb;
            }
            if (target == null)
            {
                foreach (MonoBehaviour mb in comps)
                {
                    if (mb.GetType().Name != "UIButtonEventKit") continue;
                    UILabel[] lbls = ((Component)mb).GetComponentsInChildren<UILabel>(true);
                    if (lbls == null) continue;
                    foreach (UILabel l in lbls)
                        if (l.text.ToLower().Contains("new game")) { target = mb; break; }
                    if (target != null) break;
                }
            }

            if (target == null) { _btnPatched = false; return; }

            ((Behaviour)target).enabled = false;
            UILabel[] ren = ((Component)target).GetComponentsInChildren<UILabel>(true);
            if (ren != null && ren.Length > 0) ren[0].text = "Settings";
            SettingsBtnClick proxy = ((Component)target).gameObject.GetComponent<SettingsBtnClick>()
                ?? ((Component)target).gameObject.AddComponent<SettingsBtnClick>();
            proxy.hook = this;
            _btnPatched = true;
            SettingsModEntry.Log("patched -> Settings  go=" + ((Component)target).gameObject.name);
        }

        public void OpenSettings()
        {
            _showSettings = !_showSettings;
            if (_showSettings) { ReCacheHUD(); CacheNguiCam(); }
        }

        // =====================================================================
        // Camera / sensitivity
        // =====================================================================
        private void LateUpdate()
        {
            // Lock camera while in HUD edit mode
            if (!_inGameScene || _hudEditMode) return;
            ApplySensitivity();
        }

        private void ApplySensitivity()
        {
            if (_sliderotate == null) CacheSliderotate();
            if (_sliderotate == null || _fiSensX == null) return;

            float eff = IsCurrentWeaponScoped() ? _sensNormal * _sensScoped : _sensNormal;
            if (Mathf.Abs((float)_fiSensX.GetValue(_sliderotate) - eff) > 0.02f)
            {
                _fiSensX.SetValue(_sliderotate, eff);
                _fiSensY.SetValue(_sliderotate, eff);
            }

            if (_noAccel && _fiRotX != null && _fiRotY != null)
            {
                float rotX = (float)_fiRotX.GetValue(_sliderotate);
                float rotY = (float)_fiRotY.GetValue(_sliderotate);
                float dx = Mathf.DeltaAngle(_prevRotX, rotX);
                float dy = rotY - _prevRotY;
                bool clamped = false;
                if (Mathf.Abs(dx) > _maxDelta) { rotX = _prevRotX + Mathf.Sign(dx) * _maxDelta; clamped = true; }
                if (Mathf.Abs(dy) > _maxDelta)
                {
                    float mn = _fiMinY != null ? (float)_fiMinY.GetValue(_sliderotate) : -35f;
                    float mx = _fiMaxY != null ? (float)_fiMaxY.GetValue(_sliderotate) :  35f;
                    rotY = Mathf.Clamp(_prevRotY + Mathf.Sign(dy) * _maxDelta, mn, mx);
                    clamped = true;
                }
                if (clamped)
                {
                    _fiRotX.SetValue(_sliderotate, rotX);
                    _fiRotY.SetValue(_sliderotate, rotY);
                    (_sliderotate as Component).transform.localEulerAngles = new Vector3(0f, rotX, 0f);
                    if (_fiCamT != null)
                    {
                        Transform ct = (Transform)_fiCamT.GetValue(_sliderotate);
                        if (ct != null) ct.localEulerAngles = new Vector3(-rotY, 0f, 0f);
                    }
                    rotX = (float)_fiRotX.GetValue(_sliderotate);
                    rotY = (float)_fiRotY.GetValue(_sliderotate);
                }
                _prevRotX = rotX; _prevRotY = rotY;
            }
        }

        private void CacheSliderotate()
        {
            UnityEngine.Object[] all = Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
            foreach (UnityEngine.Object obj in all)
            {
                MonoBehaviour mb = obj as MonoBehaviour;
                if (mb == null || mb.GetType().Name != "Sliderotate" || !mb.gameObject.activeInHierarchy) continue;
                _sliderotate = mb;
                Type t = mb.GetType();
                _fiSensX = t.GetField("sensitivityX",    BindingFlags.Instance | BindingFlags.NonPublic);
                _fiSensY = t.GetField("sensitivityY",    BindingFlags.Instance | BindingFlags.NonPublic);
                _fiRotX  = t.GetField("rotationX",       BindingFlags.Instance | BindingFlags.NonPublic);
                _fiRotY  = t.GetField("rotationY",       BindingFlags.Instance | BindingFlags.NonPublic);
                _fiMinY  = t.GetField("minimumY",        BindingFlags.Instance | BindingFlags.NonPublic);
                _fiMaxY  = t.GetField("maximumY",        BindingFlags.Instance | BindingFlags.NonPublic);
                _fiCamT  = t.GetField("cameratransform", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_fiRotX != null) _prevRotX = (float)_fiRotX.GetValue(_sliderotate);
                if (_fiRotY != null) _prevRotY = (float)_fiRotY.GetValue(_sliderotate);
                break;
            }
        }

        private void CacheNguiCam()
        {
            if (_nguiCam != null) return;
            string[] paths = { "InGameMenu-Online/Camera", "InGameMenu-Local/Camera", "InGameMenu/Camera" };
            foreach (string p in paths)
            {
                GameObject g = GameObject.Find(p);
                if (g != null) { _nguiCam = g.GetComponent<Camera>(); if (_nguiCam != null) break; }
            }
        }

        // =====================================================================
        // HUD edit mode -- enter / exit
        // =====================================================================
        private void EnterHudEditMode()
        {
            // Re-find all drag GOs fresh so positions are captured in current
            // on-screen/expanded state rather than whatever state they were in at scene load
            for (int i = 0; i < DRAG_COUNT; i++) _dragGOs[i] = null;
            ReCacheHUD();
            CacheNguiCam();
            _hudEditMode  = true;
            _showSettings = false;
            // Hide the pause panel so its NGUI buttons cannot be tapped
            if (_pausePanelRef != null) _pausePanelRef.SetActive(false);
            // Lock camera rotation
            if (_sliderotate != null) ((Behaviour)_sliderotate).enabled = false;
            // Disable UIButton components on touch controls so they don't fire game actions
            SetControlPanelButtonsEnabled(false);
        }

        private void ExitHudEditMode()
        {
            for (int i = 0; i < DRAG_COUNT; i++) { SaveDragPos(i); SaveDragScale(i); }
            PlayerPrefs.Save();
            _draggingIdx  = -1;
            _resizingIdx  = -1;
            _hudEditMode  = false;
            // Restore pause panel
            if (_pausePanelRef != null) _pausePanelRef.SetActive(true);
            // Restore camera
            if (_sliderotate != null) ((Behaviour)_sliderotate).enabled = true;
            // Re-enable touch control buttons
            SetControlPanelButtonsEnabled(true);
        }

        private void SetControlPanelButtonsEnabled(bool enabled)
        {
            GameObject panel = GameObject.Find("Panel(Control)");
            if (panel == null) return;
            MonoBehaviour[] mbs = panel.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour mb in mbs)
            {
                string n = mb.GetType().Name;
                if (n == "UIButton" || n == "UIButtonEventKit" || n == "UIButtonMessage"
                    || n == "UIButtonKeys" || n == "UIButtonScale")
                    ((Behaviour)mb).enabled = enabled;
            }
        }

        // Wait several frames before entering edit mode:
        // - 2 frames so the "Edit Layout" tap doesn't bleed through to NGUI below
        // - extra frames so any slide/tween animations have finished and all panels
        //   are at their final on-screen positions before we snapshot _dragOrigPos
        private IEnumerator EnterEditModeNextFrame()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            EnterHudEditMode();
        }

        // =====================================================================
        // Drag / resize handling (called from Update while in edit mode)
        // =====================================================================
        private void HandleHudDrag()
        {
            Vector2 inputPos = Vector2.zero;
            bool down = false, held = false, up = false;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                inputPos = t.position;
                down = t.phase == TouchPhase.Began;
                held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                up   = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            }
            else
            {
                inputPos = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
                up   = Input.GetMouseButtonUp(0);
            }

            // Input Y=0 at bottom; GUI Y=0 at top
            Vector2 guiPos = new Vector2(inputPos.x, Screen.height - inputPos.y);

            if (down)
            {
                // Check each item -- edge zone = resize, inner zone = drag
                for (int i = 0; i < DRAG_COUNT; i++)
                {
                    if (_dragGOs[i] == null) continue;
                    Rect full  = GetHandleRect(i);
                    Rect inner = ShrinkRect(full, 0.30f); // inner 70% = drag zone
                    if (!full.Contains(guiPos)) continue;
                    if (inner.Contains(guiPos))
                    {
                        // Start drag from center zone
                        _draggingIdx = i;
                        _resizingIdx = -1;
                        _dragOffset  = guiPos - new Vector2(full.x, full.y);
                    }
                    else
                    {
                        // Start resize from edge zone
                        _resizingIdx      = i;
                        _draggingIdx      = -1;
                        _resizeStartDist  = Vector2.Distance(guiPos, RectCenter(full));
                        _resizeStartScale = _dragGOs[i].transform.localScale;
                    }
                    break;
                }
            }
            else if (held)
            {
                if (_draggingIdx >= 0)
                    MoveDragItem(_draggingIdx, guiPos - _dragOffset);
                else if (_resizingIdx >= 0 && _resizeStartDist > 1f)
                {
                    Rect full  = GetHandleRect(_resizingIdx);
                    float dist = Vector2.Distance(guiPos, RectCenter(full));
                    float ratio = dist / _resizeStartDist;
                    // Clamp to minimum: scale can't drop below 0.25 of original
                    ratio = Mathf.Max(ratio, 0.25f);
                    Vector3 ns = _resizeStartScale * ratio;
                    // Also enforce absolute minimum handle of ~50px screen width
                    if (_nguiCam != null)
                    {
                        // Estimate resulting screen size via world-unit size
                        float origSz = _dragGOs[_resizingIdx].transform.localScale.magnitude;
                        if (origSz < 0.001f) origSz = 1f;
                        float minRatio = 50f / (Screen.width * 0.15f);
                        ratio = Mathf.Max(ratio, minRatio);
                        ns = _resizeStartScale * ratio;
                    }
                    _dragGOs[_resizingIdx].transform.localScale = ns;
                }
            }
            else if (up)
            {
                if (_draggingIdx >= 0)
                {
                    if (!(_suppressSaveIdx && _suppressSaveFor == _draggingIdx))
                        SaveDragPos(_draggingIdx);
                    _draggingIdx = -1;
                }
                if (_resizingIdx >= 0)
                {
                    SaveDragScale(_resizingIdx);
                    _resizingIdx = -1;
                }
                _suppressSaveIdx = false;
                _suppressSaveFor = -1;
            }
        }

        private static Rect ShrinkRect(Rect r, float borderFraction)
        {
            float bx = r.width  * borderFraction;
            float by = r.height * borderFraction;
            return new Rect(r.x + bx, r.y + by, r.width - bx * 2f, r.height - by * 2f);
        }
        private static Vector2 RectCenter(Rect r)
        {
            return new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);
        }

        private Rect GetHandleRect(int i)
        {
            if (_dragGOs[i] == null) return new Rect(-9999, -9999, 1, 1);
            float sz = Mathf.Max(80f, Screen.width * 0.12f);
            if (_nguiCam != null)
            {
                Vector3 sp = _nguiCam.WorldToScreenPoint(_dragGOs[i].transform.position);
                return new Rect(sp.x - sz * 0.5f, (Screen.height - sp.y) - sz * 0.5f, sz, sz);
            }
            float x = (i % 2 == 0) ? 20f : Screen.width  - sz - 20f;
            float y = (i < 2)      ? 20f : Screen.height - sz - 20f;
            return new Rect(x, y, sz, sz);
        }

        private void MoveDragItem(int i, Vector2 guiTopLeft)
        {
            if (_dragGOs[i] == null || _nguiCam == null) return;
            float sz = Mathf.Max(80f, Screen.width * 0.12f);
            float gcx = guiTopLeft.x + sz * 0.5f;
            float gcy = guiTopLeft.y + sz * 0.5f;
            float depth = _nguiCam.WorldToScreenPoint(_dragGOs[i].transform.position).z;
            Vector3 world = _nguiCam.ScreenToWorldPoint(new Vector3(gcx, Screen.height - gcy, depth));
            _dragGOs[i].transform.position = new Vector3(world.x, world.y, _dragGOs[i].transform.position.z);
        }

        private void SaveDragPos(int i)
        {
            if (_dragGOs[i] == null) return;
            Vector3 lp = _dragGOs[i].transform.localPosition;
            PlayerPrefs.SetFloat(DRAG_ITEMS[i].prefPX, lp.x);
            PlayerPrefs.SetFloat(DRAG_ITEMS[i].prefPY, lp.y);
        }

        private void LoadDragPos(int i)
        {
            if (_dragGOs[i] == null || !PlayerPrefs.HasKey(DRAG_ITEMS[i].prefPX)) return;
            float px = PlayerPrefs.GetFloat(DRAG_ITEMS[i].prefPX);
            float py = PlayerPrefs.GetFloat(DRAG_ITEMS[i].prefPY);
            Vector3 lp = _dragGOs[i].transform.localPosition;
            _dragGOs[i].transform.localPosition = new Vector3(px, py, lp.z);
        }

        private void SaveDragScale(int i)
        {
            if (_dragGOs[i] == null || DRAG_ITEMS[i].prefSZ == null) return;
            PlayerPrefs.SetFloat(DRAG_ITEMS[i].prefSZ, _dragGOs[i].transform.localScale.x);
        }

        private void LoadDragScale(int i)
        {
            if (_dragGOs[i] == null || DRAG_ITEMS[i].prefSZ == null) return;
            if (!PlayerPrefs.HasKey(DRAG_ITEMS[i].prefSZ)) return;
            float s = PlayerPrefs.GetFloat(DRAG_ITEMS[i].prefSZ);
            if (s < 0.01f) return;
            Vector3 orig = _dragGOs[i].transform.localScale;
            _dragGOs[i].transform.localScale = new Vector3(s, s, orig.z);
        }

        // =====================================================================
        // IMGUI
        // =====================================================================
        private void OnGUI()
        {
            if (!_inGameScene) return;
            if (!_showSettings && !_hudEditMode) return;

            float scale = Screen.width / REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
            float vw = REF_W;
            float vh = Screen.height / scale;

            if (_hudEditMode)
            {
                // In edit mode the pause panel is hidden; no fullscreen blocker needed
                // (blocker would swallow Finish / RST button clicks)
                DrawHudEditOverlay(vw, vh);
            }
            else
            {
                // Invisible full-screen button -- blocks NGUI click-through on settings panel
                GUI.Button(new Rect(0, 0, vw, vh), GUIContent.none, GUIStyle.none);
                // Heavy dim overlay
                GUI.color = new Color(0f, 0f, 0f, 0.88f);
                GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
                GUI.color = Color.white;

                float w = Mathf.Min(vw * 0.96f, 560f);
                float h = Mathf.Min(vh * 0.92f, 700f);
                _winRect = new Rect((vw - w) * 0.5f, (vh - h) * 0.5f, w, h);

                GUIStyle winBg = new GUIStyle(GUI.skin.window);
                winBg.normal.background   = MakeTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
                winBg.onNormal.background = winBg.normal.background;
                winBg.fontSize            = 20;
                _winRect = GUI.Window(9902, _winRect, DrawSettingsWindow, "  [CNR Mod]  Settings", winBg);
            }

            // Consume all input events so nothing passes through to NGUI
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                Event.current.Use();
        }

        private void DrawSettingsWindow(int id)
        {
            float pw = _winRect.width - 28f;
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Width(_winRect.width - 4f));
            GUILayout.Space(6f);

            // ---- Sensitivity ------------------------------------------------
            SectionHeader("Sensitivity");
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Normal  [" + _sensNormal.ToString("F1") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newSens = GUILayout.HorizontalSlider(_sensNormal, 1f, 7f, GUILayout.Height(36f));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newSens - _sensNormal) > 0.05f)
            {
                _sensNormal = Mathf.Round(newSens * 10f) / 10f;
                PlayerPrefs.SetFloat("Sensitivity", _sensNormal);
                if (_sliderotate != null && _fiSensX != null)
                {
                    _fiSensX.SetValue(_sliderotate, _sensNormal);
                    _fiSensY.SetValue(_sliderotate, _sensNormal);
                }
            }
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scoped  [x" + _sensScoped.ToString("F2") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newScope = GUILayout.HorizontalSlider(_sensScoped, 0.05f, 1.5f, GUILayout.Height(36f));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newScope - _sensScoped) > 0.005f)
            {
                _sensScoped = Mathf.Round(newScope * 100f) / 100f;
                PlayerPrefs.SetFloat("CNRMod_ScopeSens", _sensScoped);
            }
            GUILayout.Label("  Multiplier when holding AWP / M87T / STW-25 / FRF2", HintStyle());
            GUILayout.Space(14f);

            // ---- Camera -----------------------------------------------------
            SectionHeader("Camera");
            GUILayout.Space(4f);

            bool newNoAccel = GUILayout.Toggle(_noAccel, " Disable swipe acceleration  (cap rotation per frame)", ToggleStyle());
            if (newNoAccel != _noAccel) { _noAccel = newNoAccel; PlayerPrefs.SetInt("CNRMod_NoAccel", _noAccel ? 1 : 0); }

            if (_noAccel)
            {
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Max deg/frame  [" + _maxDelta.ToString("F0") + "]", LabelStyle(), GUILayout.Width(pw * 0.44f));
                float newDelta = GUILayout.HorizontalSlider(_maxDelta, 2f, 50f, GUILayout.Height(36f));
                GUILayout.EndHorizontal();
                if (Mathf.Abs(newDelta - _maxDelta) > 0.5f) { _maxDelta = Mathf.Round(newDelta); PlayerPrefs.SetFloat("CNRMod_MaxDeltaDeg", _maxDelta); }
                GUILayout.Label("  10-20 is comfortable for touch.", HintStyle());
            }
            GUILayout.Space(14f);

            // ---- HUD Visibility ---------------------------------------------
            SectionHeader("HUD Visibility");
            GUILayout.Space(4f);
            for (int i = 0; i < VIS_ITEMS.Length; i++)
            {
                bool newVis = GUILayout.Toggle(_visOn[i], "  " + VIS_ITEMS[i].displayName, ToggleStyle());
                if (newVis != _visOn[i])
                {
                    _visOn[i] = newVis;
                    PlayerPrefs.SetInt(VIS_ITEMS[i].prefKey, newVis ? 1 : 0);
                    if (_visGOs[i] != null) _visGOs[i].SetActive(newVis);
                }
            }
            GUILayout.Space(10f);

            // ---- HUD Layout -------------------------------------------------
            SectionHeader("HUD Layout");
            GUILayout.Space(4f);
            GUILayout.Label("  Drag/resize all HUD buttons: fire, jump, reload, record, aim, toolbar, chat, healthbar, team scores, pause, player list, health pack, ammo buy, HUD hide.  Center of handle = drag.  Edge = resize.  RST = reset.", HintStyle());
            GUILayout.Space(6f);

            GUIStyle editBtn = new GUIStyle(GUI.skin.button);
            editBtn.fontSize    = 20;
            editBtn.fixedHeight = 48f;
            editBtn.normal.textColor = new Color(0.9f, 0.9f, 1f);
            if (GUILayout.Button("Edit Layout  (drag mode)", editBtn))
                StartCoroutine(EnterEditModeNextFrame());

            GUILayout.Space(14f);

            // ---- Reset All HUD ----------------------------------------------
            GUIStyle resetAllBtn = new GUIStyle(GUI.skin.button);
            resetAllBtn.fontSize    = 18;
            resetAllBtn.fixedHeight = 44f;
            resetAllBtn.normal.textColor = new Color(1f, 0.45f, 0.45f);
            if (GUILayout.Button("Reset All HUD to Defaults", resetAllBtn))
            {
                for (int i = 0; i < DRAG_COUNT; i++)
                {
                    PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPX);
                    PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPY);
                    if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                    if (_dragGOs[i] != null)
                    {
                        if (_dragOrigPos[i]   != Vector3.zero) _dragGOs[i].transform.position   = _dragOrigPos[i];
                        if (_dragOrigScale[i] != Vector3.zero) _dragGOs[i].transform.localScale  = _dragOrigScale[i];
                    }
                }
                PlayerPrefs.Save();
            }
            GUILayout.Space(6f);

            // ---- Close & Save -----------------------------------------------
            GUIStyle closeBtn = new GUIStyle(GUI.skin.button);
            closeBtn.fontSize    = 22;
            closeBtn.fixedHeight = 52f;
            if (GUILayout.Button("  Close & Save  ", closeBtn)) { _showSettings = false; PlayerPrefs.Save(); }

            GUILayout.Space(8f);
            GUILayout.EndScrollView();
        }

        private void DrawHudEditOverlay(float vw, float vh)
        {
            // Top banner
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, vw, 64f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle banner = new GUIStyle(GUI.skin.label);
            banner.fontSize  = 18;
            banner.fontStyle = FontStyle.Bold;
            banner.alignment = TextAnchor.MiddleLeft;
            banner.normal.textColor = new Color(1f, 0.9f, 0.2f);
            GUI.Label(new Rect(12f, 8f, vw - 170f, 48f), "HUD Layout Edit  --  center=drag  edge=resize  R=reset", banner);

            // Finish button (top-right)
            GUIStyle finBtn = new GUIStyle(GUI.skin.button);
            finBtn.fontSize  = 20;
            finBtn.fontStyle = FontStyle.Bold;
            finBtn.normal.textColor = new Color(0.3f, 1f, 0.4f);
            if (GUI.Button(new Rect(vw - 158f, 8f, 150f, 48f), "Finish", finBtn))
                ExitHudEditMode();

            // Per-item tint colours -- alpha kept low so actual buttons are visible
            Color[] colours = new Color[]
            {
                new Color(1f,   0.3f, 0.3f, 0.28f),   // 0  fire     red
                new Color(0.3f, 0.85f,1f,   0.28f),   // 1  jump     cyan
                new Color(1f,   0.70f,0.1f, 0.28f),   // 2  reload   orange
                new Color(0.85f,0.3f, 1f,   0.28f),   // 3  record   purple
                new Color(0.5f, 0.9f, 1f,   0.28f),   // 4  aim      sky
                new Color(0.35f,1f,   0.35f,0.28f),   // 5  toolbar  green
                new Color(1f,   0.5f, 0.9f, 0.28f),   // 6  chatbar  pink
                new Color(1f,   1f,   0.3f, 0.28f),   // 7  healthbar yellow
                new Color(0.3f, 0.6f, 1f,   0.28f),   // 8  team1    blue
                new Color(1f,   0.4f, 0.4f, 0.28f),   // 9  team2    red2
                new Color(0.8f, 0.8f, 0.8f, 0.28f),   // 10 pause    grey
                new Color(0.4f, 1f,   0.8f, 0.28f),   // 11 plist    teal
                new Color(0.6f, 1f,   0.4f, 0.28f),   // 12 hpack    lime
                new Color(1f,   0.7f, 0.2f, 0.28f),   // 13 ammo     amber
                new Color(0.9f, 0.5f, 1f,   0.28f),   // 14 hudhide  violet
            };
            // Bright border colours (opaque) matching tint hue
            Color[] borders = new Color[]
            {
                new Color(1f,   0.3f, 0.3f, 0.90f),
                new Color(0.3f, 0.85f,1f,   0.90f),
                new Color(1f,   0.70f,0.1f, 0.90f),
                new Color(0.85f,0.3f, 1f,   0.90f),
                new Color(0.5f, 0.9f, 1f,   0.90f),
                new Color(0.35f,1f,   0.35f,0.90f),
                new Color(1f,   0.5f, 0.9f, 0.90f),
                new Color(1f,   1f,   0.3f, 0.90f),
                new Color(0.3f, 0.6f, 1f,   0.90f),
                new Color(1f,   0.4f, 0.4f, 0.90f),
                new Color(0.8f, 0.8f, 0.8f, 0.90f),
                new Color(0.4f, 1f,   0.8f, 0.90f),
                new Color(0.6f, 1f,   0.4f, 0.90f),
                new Color(1f,   0.7f, 0.2f, 0.90f),
                new Color(0.9f, 0.5f, 1f,   0.90f),
            };

            float scale = Screen.width / REF_W;

            for (int i = 0; i < DRAG_COUNT; i++)
            {
                if (_dragGOs[i] == null) continue;

                Rect real  = GetHandleRect(i);
                Rect vRect = new Rect(real.x / scale, real.y / scale,
                                      real.width / scale, real.height / scale);
                float bw = 3f; // border width

                bool isActive = (_draggingIdx == i || _resizingIdx == i);
                Color fill   = isActive ? new Color(1f, 1f, 1f, 0.18f) : colours[i % colours.Length];
                Color border = borders[i % borders.Length];

                // Semi-transparent fill
                GUI.color = fill;
                GUI.DrawTexture(vRect, Texture2D.whiteTexture);

                // Solid border rectangle
                GUI.color = border;
                GUI.DrawTexture(new Rect(vRect.x,              vRect.y,              vRect.width, bw),           Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(vRect.x,              vRect.yMax - bw,      vRect.width, bw),           Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(vRect.x,              vRect.y,              bw, vRect.height),          Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(vRect.xMax - bw,      vRect.y,              bw, vRect.height),          Texture2D.whiteTexture);

                // Resize mode: draw thicker inner guide line to indicate edge zone
                if (_resizingIdx == i)
                {
                    float eb = vRect.width * 0.30f;
                    GUI.color = new Color(1f, 1f, 0f, 0.70f);
                    GUI.DrawTexture(new Rect(vRect.x + eb, vRect.y + eb, vRect.width - eb*2f, 1.5f), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(vRect.x + eb, vRect.yMax - eb, vRect.width - eb*2f, 1.5f), Texture2D.whiteTexture);
                }

                GUI.color = Color.white;

                // Label -- outlined dark shadow for readability
                GUIStyle hs = new GUIStyle(GUI.skin.label);
                hs.fontSize  = Mathf.Max(12, (int)(vRect.height * 0.22f));
                hs.fontStyle = FontStyle.Bold;
                hs.alignment = TextAnchor.MiddleCenter;
                hs.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
                GUI.Label(new Rect(vRect.x + 1f, vRect.y + 1f, vRect.width, vRect.height - 24f), DRAG_ITEMS[i].displayName, hs);
                hs.normal.textColor = Color.white;
                GUI.Label(new Rect(vRect.x, vRect.y, vRect.width, vRect.height - 24f), DRAG_ITEMS[i].displayName, hs);

                // Reset button -- bottom-right corner; suppresses drag-save on this frame
                Rect rr = new Rect(vRect.xMax - 36f, vRect.yMax - 26f, 36f, 26f);
                GUIStyle rs = new GUIStyle(GUI.skin.button);
                rs.fontSize = 12;
                rs.fontStyle = FontStyle.Bold;
                rs.normal.textColor = new Color(1f, 0.3f, 0.3f);
                if (GUI.Button(rr, "RST", rs))
                {
                    if (_dragGOs[i] != null)
                    {
                        if (_dragOrigPos[i]   != Vector3.zero) _dragGOs[i].transform.position   = _dragOrigPos[i];
                        if (_dragOrigScale[i] != Vector3.zero) _dragGOs[i].transform.localScale  = _dragOrigScale[i];
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPX);
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPY);
                        if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                        // Suppress the drag-save that will fire on touch-up this same frame
                        _suppressSaveIdx = true;
                        _suppressSaveFor = i;
                        _draggingIdx     = -1;
                        _resizingIdx     = -1;
                    }
                }
            }
        }

        // =====================================================================
        // Style helpers
        // =====================================================================
        private static GUIStyle LabelStyle()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 20; s.normal.textColor = Color.white; return s;
        }
        private static GUIStyle HintStyle()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 16; s.wordWrap = true;
            s.normal.textColor = new Color(0.7f, 0.7f, 0.7f); return s;
        }
        private static GUIStyle ToggleStyle()
        {
            GUIStyle s = new GUIStyle(GUI.skin.toggle);
            s.fontSize = 20; s.normal.textColor = Color.white; return s;
        }
        private static void SectionHeader(string title)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold; s.fontSize = 22;
            s.normal.textColor = new Color(1f, 0.85f, 0.3f);
            GUILayout.Label("--  " + title + "  --", s);
        }
        private static Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D t = new Texture2D(w, h);
            t.SetPixels(pix); t.Apply(); return t;
        }

        // =====================================================================
        // HUD cache
        // =====================================================================
        private void ReCacheHUD()
        {
            // Visibility GOs
            for (int i = 0; i < VIS_ITEMS.Length; i++)
            {
                if (_visGOs[i] == null) _visGOs[i] = GameObject.Find(VIS_ITEMS[i].goName);
                if (_visGOs[i] != null) _visGOs[i].SetActive(_visOn[i]);
            }

            // Log Panel(Control) children for discovery
            GameObject controlPanel = GameObject.Find("Panel(Control)");
            if (controlPanel != null)
                foreach (Transform child in controlPanel.transform)
                    SettingsModEntry.Log("Panel(Control) child: [" + child.name + "]");

            // Broad scene scan -- log all root GOs and immediate children
            // so we can identify unknown HUD element names from the log
            LogSceneHud();

            // Drag GOs
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                if (_dragGOs[i] != null) continue;
                HudDragItem item = DRAG_ITEMS[i];
                GameObject found = null;

                if (item.parentName != null)
                {
                    GameObject parent = GameObject.Find(item.parentName);
                    if (parent != null)
                    {
                        foreach (string n in item.nameCandidates)
                        {
                            Transform t = parent.transform.Find(n);
                            if (t != null) { found = t.gameObject; break; }
                        }
                        if (found == null)
                            foreach (string n in item.nameCandidates)
                            {
                                found = FindChildRecursive(parent, n);
                                if (found != null) break;
                            }
                    }
                }
                else
                {
                    foreach (string n in item.nameCandidates)
                    {
                        found = GameObject.Find(n);
                        if (found != null) break;
                    }
                }

                _dragGOs[i] = found;
                if (found != null)
                {
                    if (_dragOrigPos[i]   == Vector3.zero) _dragOrigPos[i]   = found.transform.position;
                    if (_dragOrigScale[i] == Vector3.zero) _dragOrigScale[i] = found.transform.localScale;
                    LoadDragPos(i);
                    LoadDragScale(i);
                    SettingsModEntry.Log("DRAG[" + i + "] " + item.displayName + " -> " + found.name);
                }
                else
                    SettingsModEntry.Log("DRAG[" + i + "] " + item.displayName + " NOT FOUND");
            }
        }

        private void LogSceneHud()
        {
            // Deep-walk every InGameMenu* object so we can read all NGUI panel / button names
            string[] roots = { "InGameMenu-Online", "InGameMenu-Local", "InGameMenu" };
            foreach (string rootName in roots)
            {
                GameObject root = GameObject.Find(rootName);
                if (root == null) continue;
                SettingsModEntry.Log("=== Deep scan: " + rootName + " ===");
                LogChildrenRecursive(root.transform, 0, 6); // max depth 6
            }
        }

        private static void LogChildrenRecursive(Transform t, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            string indent = new string(' ', depth * 2);
            foreach (Transform child in t)
            {
                SettingsModEntry.Log(indent + "[" + child.name + "]");
                LogChildrenRecursive(child, depth + 1, maxDepth);
            }
        }

        private static GameObject FindChildRecursive(GameObject parent, string name)
        {
            Transform[] all = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
                if (t.name == name) return t.gameObject;
            return null;
        }

        // =====================================================================
        // Persistence
        // =====================================================================
        private void LoadPrefs()
        {
            _sensNormal = PlayerPrefs.GetFloat("Sensitivity",        3.2f);
            _sensScoped = PlayerPrefs.GetFloat("CNRMod_ScopeSens",   0.4f);
            _noAccel    = PlayerPrefs.GetInt  ("CNRMod_NoAccel",     0) == 1;
            _maxDelta   = PlayerPrefs.GetFloat("CNRMod_MaxDeltaDeg", 15f);
            for (int i = 0; i < VIS_ITEMS.Length; i++)
                _visOn[i] = PlayerPrefs.GetInt(VIS_ITEMS[i].prefKey, 1) == 1;
        }

        // =====================================================================
        // Misc helpers
        // =====================================================================
        private bool IsCurrentWeaponScoped()
        {
            if ((object)PlayerLogic.mInstance == null) return false;
            int wt = (int)PlayerLogic.mInstance.mWeaponType;
            foreach (int s in SCOPED_WEAPONS) if (s == wt) return true;
            return false;
        }

        private static bool IsGameScene(string scene)
        {
            if (scene == null) return false;
            return scene.StartsWith("FreeRun") || scene.StartsWith("SingleMode");
        }
    }

    // Proxy component on the patched "Settings" button GO
    public class SettingsBtnClick : MonoBehaviour
    {
        public SettingsModHook hook;
        private void OnClick() { if (hook != null) hook.OpenSettings(); }
    }
}
