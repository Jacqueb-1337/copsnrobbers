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
            // 5 -- top-level panels / misc (game owns their scale -- no prefSZ)
            new HudDragItem {
                displayName    = "Toolbar",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(ToolBar)" },
                prefPX = "CNRMod_PX_ToolBar", prefPY = "CNRMod_PY_ToolBar", prefSZ = null },
            // 6
            new HudDragItem {
                displayName    = "Chat bar",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(ChatBar)" },
                prefPX = "CNRMod_PX_ChatBar", prefPY = "CNRMod_PY_ChatBar", prefSZ = null },
            // 7 -- Panel(LeftMenu): contains HP blood display, health pack btn, player list btn
            new HudDragItem {
                displayName    = "HP panel",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(LeftMenu)" },
                prefPX = "CNRMod_PX_HP",  prefPY = "CNRMod_PY_HP",  prefSZ = null },
            // 8 -- Panel(Top): whole top bar (cop+robber resource/objective bars, team head icons, schedule)
            new HudDragItem {
                displayName    = "Top panel",
                parentName     = null,
                nameCandidates = new string[]{ "Panel(Top)" },
                prefPX = "CNRMod_PX_TS1", prefPY = "CNRMod_PY_TS1", prefSZ = null },
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
        private bool   _inGameScene  = false;
        private string _sceneName    = "";
        private bool   _showSettings = false;
        private bool   _btnPatched   = false;

        // -- Sliderotate reflection cache --------------------------------------
        private MonoBehaviour _sliderotate;
        private FieldInfo     _fiSensX, _fiSensY;
        private FieldInfo     _fiCannotRotate; // bool cannotRotate on Sliderotate
        private GameObject   _aimBtn          = null;
        private bool         _unscopeOnFire   = true;   // default: game behaviour (unscope after fire)
        private float        _diagTimer       = 0f;    // diagnostics: log every N seconds

        // -- Camera / FOV scope detection -------------------------------------
        private Camera _mainCam    = null;
        private float  _defaultFov = 60f;
        private float  _scopedFov  = -1f; // captured on first scope-in

        // -- Settings values ---------------------------------------------------
        private float _sensNormal    = 3.2f;
        private float _adsMultiplier = 0.5f; // 0.0-1.0: multiply normal sens when scoped
        private bool  _isAiming      = false;
        private bool  _prevIsAiming  = false; // track transitions to avoid writing every frame
        private float _keepScopeTimer = -1f;  // >0 while we're trying to re-scope after fire
        private float _lastFireTime   = -999f; // Time.time when fire last detected
        private int   _prevFpsOnFire  = -1;    // previous value of FpsOnFire pref (toggled on each press)

        // -- IMGUI state -------------------------------------------------------
        private Rect    _winRect;
        private Vector2 _scroll;
        private const float REF_W = 600f;

        // -- Pause-menu sprites (MenuSystem atlas, cached on first in-game scene) --
        private static Texture2D _spPanelBack   = null;
        private static Texture2D _spButtonNull  = null;
        private static Texture2D _spPropKuang   = null;  // checkbox unchecked bg
        private static Texture2D _spSelectKuang = null;  // checkbox checked (checkmark)
        private static Texture2D _spSliderB     = null;
        private static Texture2D _spSliderThumb = null;
        private static bool      _menuSpsCached = false;
        private static Font      _gameFont      = null;

        // -- HUD drag editor ---------------------------------------------------
        private bool    _hudEditMode    = false;
        private int     _draggingIdx    = -1;
        private Vector2 _dragOffset     = Vector2.zero;
        private int     _resizingIdx    = -1;
        private Vector2 _resizeTouchStart = Vector2.zero;
        private float   _resizeStartRatio = 1f;
        private bool    _suppressSaveIdx = false;
        private int     _suppressSaveFor = -1;
        private Camera  _nguiCam        = null;
        private UICamera _nguiUICam      = null;
        private bool    _editResizeMode = false;  // false=drag  true=resize; double-tap to toggle
        private float   _lastTapTime    = 0f;
        private int     _lastTapIdx     = -1;
        private const float DOUBLE_TAP_THRESH = 0.35f;
        // Enforced scales applied every LateUpdate to prevent game resets
        private float[] _savedScales = new float[DRAG_COUNT];

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
            StartCoroutine(DumpSpritesDelayed());
        }

        private IEnumerator DumpSpritesDelayed()
        {
            yield return null;
            yield return null;
            DumpAllSprites();
            if (_inGameScene && !_menuSpsCached) CacheMenuSystemSprites();
        }

        private void UpdateScene(string scene)
        {
            _sceneName        = scene ?? "";
            _inGameScene      = IsGameScene(scene);
            _btnPatched       = false;
            _showSettings     = false;
            _hudEditMode      = false;
            _draggingIdx      = -1;
            _resizingIdx      = -1;
            _suppressSaveIdx  = false;
            _suppressSaveFor  = -1;
            _isAiming         = false;
            _prevIsAiming     = false;
            _keepScopeTimer   = -1f;
            _lastFireTime     = -999f;
            _prevFpsOnFire    = -1;
            _sliderotate      = null;
            _allSliderotates.Clear();
            _mainCam          = null;
            _scopedFov        = -1f;
            if (_nguiUICam != null) { _nguiUICam.enabled = true; _nguiUICam = null; }
            _nguiCam          = null;
            _pausePanelRef    = null;
            _pauseUIPanel     = null;
            _wasPauseVisible  = false;
            for (int i = 0; i < DRAG_COUNT; i++) { _dragGOs[i] = null; _dragOrigPos[i] = Vector3.zero; /* keep _dragOrigScale so captured baseline survives multiple ApplyHUDOnLoad runs */ }
            for (int i = 0; i < _visGOs.Length;  i++) _visGOs[i]  = null;
            LoadPrefs();
            if (_inGameScene) StartCoroutine(ApplyHUDOnLoad());
        }

        private IEnumerator ApplyHUDOnLoad()
        {
            // Wait long enough for the game's NGUI auto-scaler to run and settle
            // before we snapshot baseline scales and apply our ratios.
            for (int f = 0; f < 20; f++) yield return null;
            ReCacheHUD();
            CacheNguiCam();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) PlayerPrefs.Save();
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
            // Write desired scales here so UIPanel.LateUpdate (runs after Update) sees the transform change.
            // LateUpdate will also write to beat UIRoot, but UIPanel has already processed by then.
            EnforceScales();
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

        private bool _spriteDumped = false;
        public void OpenSettings()
        {
            _showSettings = !_showSettings;
            if (_showSettings)
            {
                ReCacheHUD(); CacheNguiCam();
                if (!_spriteDumped) { _spriteDumped = true; DumpAllSprites(); }
                // Block NGUI from processing touches while settings are open
                if (_nguiUICam == null && _nguiCam != null) _nguiUICam = _nguiCam.GetComponent<UICamera>();
                if (_nguiUICam == null) { UICamera[] cams = (UICamera[])FindObjectsOfType(typeof(UICamera)); if (cams.Length > 0) _nguiUICam = cams[0]; }
                if (_nguiUICam != null) _nguiUICam.enabled = false;
            }
            else
            {
                if (_nguiUICam != null) _nguiUICam.enabled = true;
            }
        }

        // =====================================================================
        // Camera / sensitivity
        // =====================================================================
        private void EnforceScales()
        {
            if (!_inGameScene) return;
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                if (_dragGOs[i] == null || _savedScales[i] < 0f) continue;
                if (_dragOrigScale[i] == Vector3.zero) continue;
                float tx = _dragOrigScale[i].x * _savedScales[i];
                float ty = _dragOrigScale[i].y * _savedScales[i];
                Vector3 cur = _dragGOs[i].transform.localScale;
                if (Mathf.Abs(cur.x / tx - 1f) > 0.0001f || Mathf.Abs(cur.y / ty - 1f) > 0.0001f)
                {
                    _dragGOs[i].transform.localScale = new Vector3(tx, ty, cur.z);
                    if (_hudEditMode) ForceNGUIRebuild(i);
                }
            }
        }

        private static FieldInfo _fiWidgetPanel   = null;
        private static FieldInfo _fiRebuildAll    = null;

        private static UIPanel GetWidgetPanel(UIWidget w)
        {
            if (_fiWidgetPanel == null)
                _fiWidgetPanel = typeof(UIWidget).GetField("mPanel", BindingFlags.Instance | BindingFlags.NonPublic);
            return (_fiWidgetPanel != null) ? (UIPanel)_fiWidgetPanel.GetValue(w) : null;
        }

        private static void SetRebuildAll(UIPanel p)
        {
            if (_fiRebuildAll == null)
                _fiRebuildAll = typeof(UIPanel).GetField("mRebuildAll", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_fiRebuildAll != null) _fiRebuildAll.SetValue(p, true);
        }

        private void ForceNGUIRebuild(int i)
        {
            if (_dragGOs[i] == null) return;
            UIWidget[] ws = _dragGOs[i].GetComponentsInChildren<UIWidget>(true);
            UIPanel fallback = NGUITools.FindInParents<UIPanel>(_dragGOs[i].gameObject);
            System.Collections.Generic.HashSet<UIPanel> panels = new System.Collections.Generic.HashSet<UIPanel>();
            for (int j = 0; j < ws.Length; j++)
            {
                ws[j].MarkAsChangedLite();
                UIPanel wp = GetWidgetPanel(ws[j]);
                if (wp == null) wp = fallback;
                if (wp != null) panels.Add(wp);
            }
            foreach (UIPanel p in panels) { SetRebuildAll(p); if (ws.Length > 0) p.AddWidget(ws[0]); }
            if (panels.Count == 0 && fallback != null) { SetRebuildAll(fallback); if (ws.Length > 0) fallback.AddWidget(ws[0]); }
        }

        private void LateUpdate()
        {
            if (!_inGameScene) return;
            // Beat UIRoot (which also runs in LateUpdate and resets scales).
            EnforceScales();
            if (_hudEditMode) return;
            ApplySensitivity();
        }

        private void ApplySensitivity()
        {
            _diagTimer += Time.deltaTime;
            bool doLog = _diagTimer >= 3f;
            if (doLog) _diagTimer = 0f;

            if (_sliderotate == null) CacheSliderotate();
            if (_sliderotate == null || _fiSensX == null)
            {
                if (doLog) SettingsModEntry.Log("DIAG: Sliderotate NOT found - skipping sens");
                return;
            }

            if (_aimBtn == null) CacheAimBtn();
            if (_mainCam == null) CacheMainCam();

            // Detect fire button press: FpsOnFire pref is toggled (0↔1) on every press.
            // Any value change means the fire button was just tapped.
            int curFpsOnFire = PlayerPrefs.GetInt("FpsOnFire", 0);
            if (_prevFpsOnFire != -1 && curFpsOnFire != _prevFpsOnFire)
                _lastFireTime = Time.time;
            _prevFpsOnFire = curFpsOnFire;
            // When scoped, game uses TweenFOV to zoom camera down from ~60 to ~20-30.
            bool isNowScoped = false;
            float currentFov = -1f;
            if (_mainCam != null)
            {
                currentFov = _mainCam.fieldOfView;
                // Only update defaultFov while not scoped (so it captures the true unscoped fov)
                if (currentFov > _defaultFov - 2f) _defaultFov = currentFov;
                isNowScoped = currentFov < _defaultFov - 5f;
                // Capture the scoped FOV for potential keep-scope use
                if (isNowScoped && (_scopedFov < 0f || currentFov < _scopedFov))
                    _scopedFov = currentFov;
            }

            float aimedSens = _sensNormal * _adsMultiplier;
            if (doLog) SettingsModEntry.Log("DIAG: isAiming=" + _isAiming
                + " isNowScoped=" + isNowScoped
                + " fov=" + currentFov.ToString("F1")
                + " defaultFov=" + _defaultFov.ToString("F1")
                + " aimBtn=" + (_aimBtn != null ? _aimBtn.activeSelf.ToString() : "null")
                + " sr_enabled=" + (_sliderotate != null ? ((Behaviour)_sliderotate).enabled.ToString() : "null")
                + " cannotRot=" + (_fiCannotRotate != null ? _fiCannotRotate.GetValue(_sliderotate).ToString() : "null")
                + " sensX=" + _fiSensX.GetValue(_sliderotate)
                + " normal=" + _sensNormal + " mult=" + _adsMultiplier + " aimed=" + aimedSens.ToString("F2"));

            bool wasAiming = _isAiming;
            _isAiming = isNowScoped;

            // ---- Keep-scope: if scope just ended due to firing, re-press AimBtn ----
            // Only trigger if the fire button was pressed recently (< 0.35s ago).
            // This lets manual aim-button taps pass through unchanged.
            if (wasAiming && !_isAiming && !_unscopeOnFire)
            {
                float timeSinceFire = Time.time - _lastFireTime;
                if (timeSinceFire < 0.35f)
                {
                    _keepScopeTimer = 1.5f;
                    SettingsModEntry.Log("KeepScope: fire-triggered unscope (dt=" + timeSinceFire.ToString("F3") + "s), starting re-scope");
                }
                else
                {
                    SettingsModEntry.Log("KeepScope: manual unscope (dt=" + timeSinceFire.ToString("F3") + "s), NOT re-scoping");
                }
            }
            if (_keepScopeTimer > 0f && !_isAiming)
            {
                _keepScopeTimer -= Time.deltaTime;
                // Simulate pressing the Aim button every frame until re-scoped or timed out
                PlayerPrefs.SetInt("OnAim", 1);
            }
            else if (_isAiming && _keepScopeTimer > 0f)
            {
                // Successfully re-scoped
                SettingsModEntry.Log("KeepScope: re-scope SUCCESS");
                _keepScopeTimer = -1f;
            }

            // Sliderotate only reads PlayerPrefs in Start(), NOT in Update().
            // Write directly to sensitivityX/Y via reflection -- takes effect immediately.
            // Only write on state transition so we don't fight whatever else may set the field.
            if (_isAiming != wasAiming)
            {
                float newSens = _isAiming ? aimedSens : _sensNormal;
                WriteAllSens(newSens);
                SettingsModEntry.Log("SensChange: scoped=" + _isAiming + " fov=" + currentFov.ToString("F1") + " -> sensitivityX=" + newSens.ToString("F2") + " (" + _allSliderotates.Count + " instances)");
            }
            _prevIsAiming = _isAiming;
        }

        // (fire detection now uses FpsOnFire pref polling in ApplySensitivity)

        public void ToggleAiming() { _isAiming = !_isAiming; }

        // All Sliderotate instances — patch every one of them
        private System.Collections.Generic.List<MonoBehaviour> _allSliderotates
            = new System.Collections.Generic.List<MonoBehaviour>();

        private void WriteAllSens(float value)
        {
            if (_fiSensX == null) return;
            foreach (MonoBehaviour sr in _allSliderotates)
            {
                if (sr == null) continue;
                _fiSensX.SetValue(sr, value);
                _fiSensY.SetValue(sr, value);
            }
        }

        private void CacheSliderotate()
        {
            _allSliderotates.Clear();
            UnityEngine.Object[] all = Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
            foreach (UnityEngine.Object obj in all)
            {
                MonoBehaviour mb = obj as MonoBehaviour;
                if (mb == null) continue;
                if (string.Compare(mb.GetType().Name, "Sliderotate", System.StringComparison.OrdinalIgnoreCase) != 0) continue;
                _allSliderotates.Add(mb);
                SettingsModEntry.Log("SR[" + _allSliderotates.Count + "] GO="
                    + mb.gameObject.name
                    + " active=" + mb.gameObject.activeInHierarchy
                    + " enabled=" + ((Behaviour)mb).enabled);
            }
            if (_allSliderotates.Count > 0)
            {
                _sliderotate = _allSliderotates[0];
                Type t = _sliderotate.GetType();
                _fiSensX       = t.GetField("sensitivityX",  BindingFlags.Instance | BindingFlags.NonPublic);
                _fiSensY       = t.GetField("sensitivityY",  BindingFlags.Instance | BindingFlags.NonPublic);
                _fiCannotRotate = t.GetField("cannotRotate", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            SettingsModEntry.Log("CacheSliderotate: total=" + _allSliderotates.Count
                + " fiSensX=" + (_fiSensX != null));
            CacheAimBtn();
            CacheMainCam();
        }

        private void CacheMainCam()
        {
            // Try Camera.main (requires "MainCamera" tag)
            Camera cam = Camera.main;
            if (cam != null)
            {
                _mainCam = cam;
                _defaultFov = cam.fieldOfView;
                _scopedFov = -1f;
                SettingsModEntry.Log("CacheMainCam: " + cam.gameObject.name + " fov=" + cam.fieldOfView);
                return;
            }
            // Fallback: find first non-UI camera in the active scene
            Camera[] allCams = (Camera[])UnityEngine.Object.FindObjectsOfType(typeof(Camera));
            foreach (Camera c in allCams)
            {
                string n = c.gameObject.name.ToLower();
                if (n.Contains("ngui") || n.Contains("ui") || n.Contains("menu")) continue;
                _mainCam = c;
                _defaultFov = c.fieldOfView;
                _scopedFov = -1f;
                SettingsModEntry.Log("CacheMainCam fallback: " + c.gameObject.name + " fov=" + c.fieldOfView);
                return;
            }
            SettingsModEntry.Log("CacheMainCam: NOT found");
        }

        private void CacheAimBtn()
        {
            // Prefer the drag-cached GO; fall back to FindObjectsOfTypeAll which
            // finds inactive objects too (unlike GameObject.Find).
            if (_dragGOs[4] != null) { _aimBtn = _dragGOs[4]; return; }
            UnityEngine.Object[] all = Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (UnityEngine.Object obj in all)
            {
                GameObject go = obj as GameObject;
                if (go != null && go.name == "Image Button(Aim)")
                { _aimBtn = go; SettingsModEntry.Log("CacheAimBtn found: " + go.name); return; }
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
            // Re-enable UICamera (was disabled when settings opened before entering edit mode)
            if (_nguiUICam != null) { _nguiUICam.enabled = true; _nguiUICam = null; }
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
        private IEnumerator ResetHUDViaEditMode()
        {
            // Close settings and wait for input to clear before entering edit mode
            _showSettings = false;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            // Enter edit mode — this calls ReCacheHUD and populates _dragGOs
            EnterHudEditMode();
            yield return null;
            // Apply factory reset to all live items
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPX);
                PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPY);
                if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                _savedScales[i] = -1f;
                if (_dragGOs[i] != null)
                {
                    if (_dragOrigPos[i] != Vector3.zero)
                        _dragGOs[i].transform.position = _dragOrigPos[i];
                    _dragGOs[i].transform.localScale =
                        (_dragOrigScale[i] != Vector3.zero) ? _dragOrigScale[i] : Vector3.one;
                }
                _dragOrigScale[i] = Vector3.zero; // force fresh recapture on next scene load
            }
            PlayerPrefs.Save();
            // Let NGUI process the repositioned transforms for one frame before re-enabling buttons
            yield return null;
            yield return null;
            // Exit edit mode so the game returns to normal play
            ExitHudEditMode();
        }

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
                for (int i = 0; i < DRAG_COUNT; i++)
                {
                    if (_dragGOs[i] == null) continue;
                    Rect full = GetHandleRect(i);
                    if (!full.Contains(guiPos)) continue;

                    // Double-tap on same handle toggles move / resize mode globally
                    if (_lastTapIdx == i && (Time.time - _lastTapTime) < DOUBLE_TAP_THRESH)
                    {
                        _editResizeMode = !_editResizeMode;
                        _lastTapIdx = -1;
                    }
                    else
                    {
                        _lastTapIdx  = i;
                        _lastTapTime = Time.time;
                    }

                    if (_editResizeMode)
                    {
                        _resizingIdx      = i;
                        _draggingIdx      = -1;
                        _resizeTouchStart = guiPos;
                        _resizeStartRatio = (_savedScales[i] >= 0f) ? _savedScales[i] : 1f;
                    }
                    else
                    {
                        _draggingIdx = i;
                        _resizingIdx = -1;
                        // Store offset from the item's CENTER so the item stays under the finger
                        _dragOffset  = guiPos - RectCenter(full);
                    }
                    break;
                }
            }
            else if (held)
            {
                if (_draggingIdx >= 0)
                    MoveDragItem(_draggingIdx, guiPos - _dragOffset);
                else if (_resizingIdx >= 0)
                {
                    // Update _savedScales directly; LateUpdate applies origScale*ratio.
                    // This beats NGUI UIRoot which also runs in LateUpdate after us.
                    float refDist = Mathf.Max(56f, Screen.width * 0.08f);
                    float delta = (guiPos.x - _resizeTouchStart.x)
                                - (guiPos.y - _resizeTouchStart.y); // guiPos Y flipped: up = negative, so subtract
                    float dragRatio = 1f + delta / refDist;
                    // clamp so button can't go below 15% of original
                    dragRatio = Mathf.Max(dragRatio, 0.15f);
                    _savedScales[_resizingIdx] = _resizeStartRatio * dragRatio;
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
                    PlayerPrefs.Save();
                    _resizingIdx  = -1;
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
            float baseSz = Mathf.Max(56f, Screen.width * 0.08f);
            // Scale the handle to reflect the current saved ratio so user sees live size feedback.
            float ratio = (_savedScales[i] > 0f && DRAG_ITEMS[i].prefSZ != null) ? _savedScales[i] : 1f;
            // Clamp so handle stays usable even if ratio is extreme
            float sz = Mathf.Clamp(baseSz * ratio, 24f, Screen.width * 0.7f);
            if (_nguiCam != null)
            {
                Vector3 sp = _nguiCam.WorldToScreenPoint(_dragGOs[i].transform.position);
                return new Rect(sp.x - sz * 0.5f, (Screen.height - sp.y) - sz * 0.5f, sz, sz);
            }
            float x = (i % 2 == 0) ? 20f : Screen.width  - sz - 20f;
            float y = (i < 2)      ? 20f : Screen.height - sz - 20f;
            return new Rect(x, y, sz, sz);
        }

        private void MoveDragItem(int i, Vector2 guiCenter)
        {
            if (_dragGOs[i] == null || _nguiCam == null) return;
            // guiCenter is the desired center in GUI coords (Y=0 at top)
            float depth = _nguiCam.WorldToScreenPoint(_dragGOs[i].transform.position).z;
            Vector3 world = _nguiCam.ScreenToWorldPoint(new Vector3(guiCenter.x, Screen.height - guiCenter.y, depth));
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
            if (DRAG_ITEMS[i].prefSZ == null) return;
            if (_savedScales[i] < 0f) return;
            PlayerPrefs.SetFloat(DRAG_ITEMS[i].prefSZ, _savedScales[i]);
            SettingsModEntry.Log("SAVE ratio[" + i + "] " + DRAG_ITEMS[i].displayName + " = " + _savedScales[i].ToString("F4") + " (base x=" + _dragOrigScale[i].x.ToString("F5") + " y=" + _dragOrigScale[i].y.ToString("F5") + ")");
        }

        private void LoadDragScale(int i)
        {
            if (_dragGOs[i] == null || DRAG_ITEMS[i].prefSZ == null) return;
            if (!PlayerPrefs.HasKey(DRAG_ITEMS[i].prefSZ)) return;
            if (_dragOrigScale[i] == Vector3.zero) return;
            float ratio = PlayerPrefs.GetFloat(DRAG_ITEMS[i].prefSZ);
            float tx = _dragOrigScale[i].x * ratio;
            float ty = _dragOrigScale[i].y * ratio;
            Vector3 orig = _dragGOs[i].transform.localScale;
            _dragGOs[i].transform.localScale = new Vector3(tx, ty, orig.z);
            _savedScales[i] = ratio;
            SettingsModEntry.Log("LOAD ratio[" + i + "] " + DRAG_ITEMS[i].displayName + " ratio=" + ratio.ToString("F4") + " target=(" + tx.ToString("F5") + "," + ty.ToString("F5") + ")");
        }

        // =====================================================================
        // IMGUI
        // =====================================================================
        private void OnGUI()
        {
            if (_sceneName == "MainMenu" && !_showSettings && !_hudEditMode)
            {
                // Main menu overlay -- reload button (bottom-right)
                float sc = Screen.width / REF_W;
                GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);
                float vh2 = Screen.height / sc;
                GUIStyle rb = new GUIStyle(GUI.skin.button);
                rb.fontSize = 10; rb.fontStyle = FontStyle.Bold;
                rb.normal.textColor = new Color(0.4f, 1f, 0.6f);
                if (GUI.Button(new Rect(REF_W - 10f - 120f, vh2 - 34f, 120f, 26f), "Reload Mods", rb))
                {
                    DumpAllSprites();
                    ReloadExternalMods();
                }
                return;
            }
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

                float w = Mathf.Min(vw * 0.96f, 420f);
                float h = Mathf.Min(vh * 0.92f, 525f);
                _winRect = new Rect((vw - w) * 0.5f, (vh - h) * 0.5f, w, h);

                GUIStyle winBg = new GUIStyle(GUI.skin.window);
                winBg.normal.background   = (_spPanelBack != null)
                    ? _spPanelBack
                    : MakeTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
                winBg.onNormal.background = winBg.normal.background;
                winBg.fontSize            = 15;
                _winRect = GUI.Window(9902, _winRect, DrawSettingsWindow, "  [CNR Mod]  Settings", winBg);
            }

            // Consume all input events so nothing passes through to NGUI
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                Event.current.Use();
        }

        private void DrawSettingsWindow(int id)
        {
            float pw = _winRect.width - 28f;
            GUIStyle vScroll = new GUIStyle(GUI.skin.verticalScrollbar); vScroll.fixedWidth = 30f;
            _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUIStyle.none, vScroll,
                GUILayout.Width(_winRect.width - 4f));
            GUILayout.Space(6f);

            // ---- Sensitivity ------------------------------------------------
            SectionHeader("Sensitivity");
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Normal  [" + _sensNormal.ToString("F1") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newSens = DrawSlider(_sensNormal, 1f, 7f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newSens - _sensNormal) > 0.05f)
            {
                _sensNormal = Mathf.Round(newSens * 10f) / 10f;
                PlayerPrefs.SetFloat("Sensitivity", _sensNormal);
                if (_sliderotate != null && _fiSensX != null)
                    WriteAllSens(_sensNormal);
            }
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Aimed   [" + Mathf.RoundToInt(_adsMultiplier * 100f) + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newAimed = DrawSlider(_adsMultiplier, 0.1f, 1.0f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newAimed - _adsMultiplier) > 0.005f)
            {
                _adsMultiplier = Mathf.Round(newAimed * 20f) / 20f; // snap to 5% steps
                PlayerPrefs.SetFloat("CNRMod_AimedMult", _adsMultiplier);
            }
            GUILayout.Label("  % of normal sens while scoped (e.g. 50% = half speed)", HintStyle());
            GUILayout.Space(10f);

            // ---- AWP / ADS --------------------------------------------------
            SectionHeader("AWP / ADS");
            GUILayout.Space(4f);
            {
                GUILayout.Space(2f);
                bool clicked = GUILayout.Button(GUIContent.none, GhostBtnStyle(), GUILayout.Height(34f));
                Rect rk = GUILayoutUtility.GetLastRect();
                Texture2D chkTex = _unscopeOnFire
                    ? (_spSelectKuang ?? MakeTex(2, 2, Color.white))
                    : (_spPropKuang   ?? MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)));
                GUI.DrawTexture(new Rect(rk.x + 3f, rk.y + 2f, 30f, 30f), chkTex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(rk.x + 39f, rk.y, rk.width - 42f, rk.height), "Unscope after firing", LabelStyle());
                if (clicked)
                {
                    _unscopeOnFire = !_unscopeOnFire;
                    PlayerPrefs.SetInt("CNRMod_UnscopeOnFire", _unscopeOnFire ? 1 : 0);
                    if (_unscopeOnFire) _keepScopeTimer = -1f; // cancel any pending re-scope
                }
            }
            GUILayout.Label("  ON (default) = AWP un-scopes after each shot\n  OFF = stays scoped until you tap aim again", HintStyle());
            GUILayout.Space(14f);

            // ---- HUD Visibility ---------------------------------------------
            SectionHeader("HUD Visibility");
            GUILayout.Space(4f);
            for (int i = 0; i < VIS_ITEMS.Length; i++)
            {
                GUILayout.Space(2f);
                bool clicked = GUILayout.Button(GUIContent.none, GhostBtnStyle(), GUILayout.Height(34f));
                Rect r = GUILayoutUtility.GetLastRect();
                Texture2D chkTex = _visOn[i]
                    ? (_spSelectKuang ?? MakeTex(2, 2, Color.white))
                    : (_spPropKuang   ?? MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)));
                GUI.DrawTexture(new Rect(r.x + 3f, r.y + 2f, 30f, 30f), chkTex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(r.x + 39f, r.y, r.width - 42f, r.height), VIS_ITEMS[i].displayName, LabelStyle());
                if (clicked)
                {
                    _visOn[i] = !_visOn[i];
                    PlayerPrefs.SetInt(VIS_ITEMS[i].prefKey, _visOn[i] ? 1 : 0);
                    if (_visGOs[i] != null) _visGOs[i].SetActive(_visOn[i]);
                }
            }
            GUILayout.Space(10f);

            // ---- HUD Layout -------------------------------------------------
            SectionHeader("HUD Layout");
            GUILayout.Space(4f);
            GUILayout.Label("  Drag/resize all HUD buttons: fire, jump, reload, record, aim, toolbar, chat, healthbar, team scores, pause, player list, health pack, ammo buy, HUD hide.  Center of handle = drag.  Edge = resize.  RST = reset.", HintStyle());
            GUILayout.Space(6f);

            if (GUILayout.Button("Edit Layout  (drag mode)", BtnStyle(20, new Color(0.9f, 0.9f, 1f))))
                StartCoroutine(EnterEditModeNextFrame());

            GUILayout.Space(14f);

            // ---- Reset All HUD ----------------------------------------------
            if (GUILayout.Button("Reset All HUD to Defaults", BtnStyle(18, new Color(1f, 0.45f, 0.45f))))
            {
                if (_inGameScene)
                    StartCoroutine(ResetHUDViaEditMode());
                else
                {
                    // Not in game scene yet — just wipe prefs; defaults load on next scene entry
                    for (int i = 0; i < DRAG_COUNT; i++)
                    {
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPX);
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPY);
                        if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                        _savedScales[i]   = -1f;
                        _dragOrigScale[i] = Vector3.zero;
                    }
                    PlayerPrefs.Save();
                }
            }
            GUILayout.Space(6f);

            // ---- Close & Save -----------------------------------------------
            if (GUILayout.Button("  Close & Save  ", BtnStyle(22, Color.white)))
            {
                _showSettings = false;
                if (_nguiUICam != null) _nguiUICam.enabled = true;
                PlayerPrefs.Save();
            }

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
            string modeLabel = _editResizeMode ? "[RESIZE]  double-tap handle to switch" : "[MOVE]  double-tap handle to switch";
            GUI.Label(new Rect(12f, 8f, vw - 170f, 48f), "HUD Edit  --  " + modeLabel, banner);

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
                new Color(1f,   0.3f, 0.3f, 0.09f),   // 0  fire     red
                new Color(0.3f, 0.85f,1f,   0.09f),   // 1  jump     cyan
                new Color(1f,   0.70f,0.1f, 0.09f),   // 2  reload   orange
                new Color(0.85f,0.3f, 1f,   0.09f),   // 3  record   purple
                new Color(0.5f, 0.9f, 1f,   0.09f),   // 4  aim      sky
                new Color(0.35f,1f,   0.35f,0.09f),   // 5  toolbar  green
                new Color(1f,   0.5f, 0.9f, 0.09f),   // 6  chatbar  pink
                new Color(1f,   1f,   0.3f, 0.09f),   // 7  healthbar yellow
                new Color(0.3f, 0.6f, 1f,   0.09f),   // 8  team1    blue
                new Color(1f,   0.4f, 0.4f, 0.09f),   // 9  team2    red2
                new Color(0.8f, 0.8f, 0.8f, 0.09f),   // 10 pause    grey
                new Color(0.4f, 1f,   0.8f, 0.09f),   // 11 plist    teal
                new Color(0.6f, 1f,   0.4f, 0.09f),   // 12 hpack    lime
                new Color(1f,   0.7f, 0.2f, 0.09f),   // 13 ammo     amber
                new Color(0.9f, 0.5f, 1f,   0.09f),   // 14 hudhide  violet
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
                Rect rr = new Rect(vRect.xMax - 24f, vRect.yMax - 16f, 24f, 16f);
                GUIStyle rs = new GUIStyle(GUI.skin.button);
                rs.fontSize = 9;
                rs.fontStyle = FontStyle.Bold;
                rs.normal.textColor = new Color(1f, 0.3f, 0.3f);
                if (GUI.Button(rr, "RST", rs))
                {
                    if (_dragGOs[i] != null)
                    {
                        _dragGOs[i].transform.position   = _dragOrigPos[i];
                        _dragGOs[i].transform.localScale = (_dragOrigScale[i] != Vector3.zero) ? _dragOrigScale[i] : Vector3.one;
                        _savedScales[i] = -1f;  // disable scale enforcement; let game auto-scaler own it
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPX);
                        PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefPY);
                        if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                        PlayerPrefs.Save();
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
            s.fontSize = 15; s.normal.textColor = Color.white;
            if (_gameFont != null) s.font = _gameFont;
            return s;
        }
        private static GUIStyle HintStyle()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 11; s.wordWrap = true;
            s.normal.textColor = new Color(0.72f, 0.72f, 0.72f);
            if (_gameFont != null) s.font = _gameFont;
            return s;
        }
        private static GUIStyle GhostBtnStyle()
        {
            GUIStyle s = new GUIStyle();
            s.normal.background = null;
            s.hover.background  = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.08f));
            s.active.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.16f));
            return s;
        }
        private static GUIStyle BtnStyle(int fontSize = 20, Color textColor = default(Color))
        {
            if (textColor == default(Color)) textColor = Color.white;
            GUIStyle s = new GUIStyle(GUI.skin.button);
            s.fontSize    = fontSize;
            s.fixedHeight = fontSize < 15 ? 33f : 39f;
            s.normal.textColor  = textColor;
            s.hover.textColor   = textColor;
            s.active.textColor  = textColor;
            if (_gameFont != null) s.font = _gameFont;
            if (_spButtonNull != null)
            {
                s.normal.background  = _spButtonNull;
                s.hover.background   = _spButtonNull;
                s.active.background  = _spButtonNull;
            }
            return s;
        }
        // Draw a slider using invisible IMGUI input + manual texture paint
        private static float DrawSlider(float val, float min, float max)
        {
            const float thumbW = 30f;
            const float height = 33f;
            // Invisible styles -- input only, no drawing
            GUIStyle invisBg    = new GUIStyle();
            GUIStyle invisThumb = new GUIStyle();
            invisBg.fixedHeight    = height;
            invisThumb.fixedWidth  = thumbW;
            invisThumb.fixedHeight = height;
            float newVal = GUILayout.HorizontalSlider(val, min, max, invisBg, invisThumb, GUILayout.Height(height));
            if (Event.current.type == EventType.Repaint)
            {
                Rect r = GUILayoutUtility.GetLastRect();
                // Track
                float trackH = 10f;
                Rect track = new Rect(r.x + thumbW * 0.5f, r.y + (r.height - trackH) * 0.5f,
                                      r.width - thumbW, trackH);
                Texture2D trackTex = _spSliderB ?? MakeTex(2, 2, new Color(0.20f, 0.22f, 0.28f, 1f));
                GUI.DrawTexture(track, trackTex, ScaleMode.StretchToFill);
                // Thumb
                float t = (val - min) / Mathf.Max(max - min, 0.001f);
                float thumbX = r.x + t * (r.width - thumbW);
                Rect thumb = new Rect(thumbX, r.y + (r.height - thumbW) * 0.5f, thumbW, thumbW);
                Texture2D thumbTex = _spSliderThumb ?? MakeTex(2, 2, new Color(0.55f, 0.55f, 0.60f, 1f));
                GUI.DrawTexture(thumb, thumbTex, ScaleMode.ScaleToFit);
            }
            return newVal;
        }
        private static void SectionHeader(string title)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold; s.fontSize = 16;
            s.normal.textColor = new Color(1f, 0.85f, 0.3f);
            if (_gameFont != null) s.font = _gameFont;
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
                    if (_dragOrigScale[i] == Vector3.zero)
                        _dragOrigScale[i] = found.transform.localScale;
                    LoadDragPos(i);
                    LoadDragScale(i);
                    SettingsModEntry.Log("DRAG[" + i + "] " + item.displayName + " -> " + found.name);
                }
                else
                    SettingsModEntry.Log("DRAG[" + i + "] " + item.displayName + " NOT FOUND");
            }

            // Keep _aimBtn reference up to date from drag cache
            if (_dragGOs[4] != null) _aimBtn = _dragGOs[4];
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
            _sensNormal     = PlayerPrefs.GetFloat("Sensitivity",       3.2f);
            // Guard against corruption: if a previous crash left a very low value in
            // "Sensitivity" (from a scoped-in write), clamp it back to minimum 1.0 and
            // restore the pref so Sliderotate starts correctly next time.
            if (_sensNormal < 1.0f)
            {
                _sensNormal = 3.2f;
                PlayerPrefs.SetFloat("Sensitivity", _sensNormal);
                SettingsModEntry.Log("LoadPrefs: corrected corrupted Sensitivity pref -> 3.2");
            }
            _adsMultiplier  = PlayerPrefs.GetFloat("CNRMod_AimedMult",  0.5f);
            _unscopeOnFire  = PlayerPrefs.GetInt("CNRMod_UnscopeOnFire", 1) == 1;
            for (int i = 0; i < VIS_ITEMS.Length; i++)
                _visOn[i] = PlayerPrefs.GetInt(VIS_ITEMS[i].prefKey, 1) == 1;
            // Pre-populate _savedScales so LateUpdate can enforce them immediately.
            // Use -1f as sentinel for "no saved scale" since valid scales can be tiny (< 0.01).
            // Also nuke any stale scale prefs for game-owned panels (prefSZ == null items).
            string[] stalePanelKeys = new string[]{ "CNRMod_SZ_ToolBar", "CNRMod_SZ_ChatBar", "CNRMod_SZ_HP", "CNRMod_SZ_TS1" };
            foreach (string k in stalePanelKeys) PlayerPrefs.DeleteKey(k);
            // Migration: if scale version key absent, wipe all saved scales (clears stale data from old code).
            if (!PlayerPrefs.HasKey("CNRMod_ScaleVer") || PlayerPrefs.GetInt("CNRMod_ScaleVer") < 3)
            {
                SettingsModEntry.Log("LoadPrefs: migrating scale prefs to ratio-based (v3)");
                for (int i = 0; i < DRAG_COUNT; i++)
                    if (DRAG_ITEMS[i].prefSZ != null) PlayerPrefs.DeleteKey(DRAG_ITEMS[i].prefSZ);
                PlayerPrefs.SetInt("CNRMod_ScaleVer", 3);
                PlayerPrefs.Save();
            }
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                if (DRAG_ITEMS[i].prefSZ != null && PlayerPrefs.HasKey(DRAG_ITEMS[i].prefSZ))
                {
                    _savedScales[i] = PlayerPrefs.GetFloat(DRAG_ITEMS[i].prefSZ);
                    SettingsModEntry.Log("PREFS ratio[" + i + "] " + DRAG_ITEMS[i].displayName + " = " + _savedScales[i].ToString("F4"));
                }
                else
                    _savedScales[i] = -1f;
            }
        }

        // =====================================================================
        // Misc helpers
        // =====================================================================
        private void ReloadExternalMods()
        {
            SettingsModEntry.Log("ReloadExternalMods: delegating to MainMenuDirector.LoadMods()");
            try { MainMenuDirector.LoadMods(); }
            catch (Exception ex) { SettingsModEntry.Log("ReloadExternalMods err: " + ex.Message); }
        }

        private void CacheMenuSystemSprites()
        {
            UISprite[] all = (UISprite[])FindObjectsOfType(typeof(UISprite));
            UIAtlas atlas = null;
            foreach (UISprite s in all)
            {
                if (s.atlas != null && s.atlas.name == "MenuSystem") { atlas = s.atlas; break; }
            }
            if (atlas == null) { SettingsModEntry.Log("CacheMenuSprites: MenuSystem not found"); return; }
            _spPanelBack   = ExtractSprite(atlas, "PanelBack");
            _spButtonNull  = ExtractSprite(atlas, "ButtonNull_2");
            _spPropKuang   = ExtractSprite(atlas, "PropKuang");
            _spSelectKuang = ExtractSprite(atlas, "SelectKuang");
            _spSliderB     = ExtractSprite(atlas, "SliderB");
            _spSliderThumb = ExtractSprite(atlas, "SliderThumb");
            _menuSpsCached = true;
            if (_gameFont == null)
            {
                UILabel[] lbls = (UILabel[])FindObjectsOfType(typeof(UILabel));
                foreach (UILabel lbl in lbls)
                    if (lbl.font != null && lbl.font.dynamicFont != null)
                    { _gameFont = lbl.font.dynamicFont; break; }
            }
            SettingsModEntry.Log("CacheMenuSprites: Panel=" + (_spPanelBack!=null) +
                " Btn=" + (_spButtonNull!=null) + " Chk=" + (_spPropKuang!=null) +
                " Sel=" + (_spSelectKuang!=null) + " SlB=" + (_spSliderB!=null) +
                " SlT=" + (_spSliderThumb!=null) + " Font=" + (_gameFont!=null));
        }

        private static Texture2D ExtractSprite(UIAtlas atlas, string spName)
        {
            UIAtlas.Sprite sp = atlas.GetSprite(spName);
            if (sp == null) { SettingsModEntry.Log("ExtractSprite: not found: " + spName); return null; }
            Rect outer = sp.outer;
            int texW = atlas.texture.width;
            int texH = atlas.texture.height;
            int px, py, pw, ph;
            if (atlas.coordinates == UIAtlas.Coordinates.Pixels)
            {
                px = Mathf.RoundToInt(outer.x);     py = Mathf.RoundToInt(outer.y);
                pw = Mathf.RoundToInt(outer.width); ph = Mathf.RoundToInt(outer.height);
            }
            else // TexCoords: 0-1 normalized, Y=0 at bottom
            {
                px = Mathf.RoundToInt(outer.x * texW);      py = Mathf.RoundToInt((1f - outer.y - outer.height) * texH);
                pw = Mathf.RoundToInt(outer.width  * texW); ph = Mathf.RoundToInt(outer.height * texH);
            }
            if (pw <= 0 || ph <= 0) return null;
            // Y in NGUI Pixels is from top; GL ReadPixels uses Y from bottom
            float uScale = (float)pw / texW;
            float vScale = (float)ph / texH;
            float uOff   = (float)px / texW;
            float vOff   = 1f - (float)(py + ph) / texH;
            RenderTexture rt = RenderTexture.GetTemporary(pw, ph, 0, RenderTextureFormat.ARGB32);
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = atlas.texture;
            mat.SetTextureOffset("_MainTex", new Vector2(uOff, vOff));
            mat.SetTextureScale("_MainTex",  new Vector2(uScale, vScale));
            Graphics.Blit(atlas.texture, rt, mat);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D result = new Texture2D(pw, ph, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, pw, ph), 0, 0);
            result.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.Destroy(mat);
            SettingsModEntry.Log("ExtractSprite: " + spName + " " + pw + "x" + ph);
            return result;
        }

        private void DumpAllSprites()
        {
            // One-time dump: log every UISprite in the scene so we can identify
            // the atlas + sprite names used by the gun shop card background/border.
            SettingsModEntry.Log("=== SPRITE DUMP START ===");
            UISprite[] all = (UISprite[])FindObjectsOfType(typeof(UISprite));
            System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> byAtlas =
                new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            foreach (UISprite s in all)
            {
                string atlasName = (s.atlas != null) ? s.atlas.name : "null";
                string spName = s.spriteName ?? "null";
                string goPath = GetPath(s.transform);
                if (!byAtlas.ContainsKey(atlasName))
                    byAtlas[atlasName] = new System.Collections.Generic.List<string>();
                string entry = spName + "  [" + goPath + "]";
                if (!byAtlas[atlasName].Contains(entry))
                    byAtlas[atlasName].Add(entry);
            }
            foreach (var kv in byAtlas)
            {
                SettingsModEntry.Log("ATLAS: " + kv.Key);
                foreach (string e in kv.Value)
                    SettingsModEntry.Log("  sprite: " + e);
            }
            SettingsModEntry.Log("=== SPRITE DUMP END ===");
        }

        private static string GetPath(Transform t)
        {
            if (t == null) return "";
            string path = t.name;
            Transform p = t.parent;
            int depth = 0;
            while (p != null && depth < 5) { path = p.name + "/" + path; p = p.parent; depth++; }
            return path;
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

