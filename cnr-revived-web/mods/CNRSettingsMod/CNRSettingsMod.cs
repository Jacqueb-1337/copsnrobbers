// CNRSettingsMod.cs -- In-game settings/HUD mod for Cops N Robbers
// Entry point: CNRSettingsMod.SettingsModEntry.Load() -- called by CNRMod DLL scanner

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
        public  const string Version = "3.1.46";

        public static void Load()
        {
            try
            {
                // Guard against duplicate instances: LoadMods() is called once per
                // MainMenuDirector.Awake(), which fires on every scene transition back
                // to the main menu.  Without this check each round trip adds another
                // SettingsModHook MonoBehaviour, and OwnJumpPhysics() gets called N
                // times per frame after N matches � multiplying jump height by N.
                if (GameObject.Find("CNRSettingsMod") != null)
                {
                    Log("CNRSettingsMod already running � skipping duplicate Load()");
                    return;
                }
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
        private const string HUD_CFG_PATH = "/storage/emulated/0/CNRMods/hud.cfg";

        // -- KBM keybinds ----------------------------------------------------
        private static readonly string[] KBM_BIND_NAMES = {
            "Fire", "Jump", "Forward", "Back", "Left", "Right", "Next gun", "Prev gun",
            "Aim", "Pause", "Reload", "Player list", "Chat" };
        private static readonly string[] KBM_PREF_KEYS = {
            "CNRMod_KB_Fire", "CNRMod_KB_Jump", "CNRMod_KB_Fwd", "CNRMod_KB_Back",
            "CNRMod_KB_Left", "CNRMod_KB_Right", "CNRMod_KB_NextWpn", "CNRMod_KB_PrevWpn",
            "CNRMod_KB_Aim", "CNRMod_KB_Pause", "CNRMod_KB_Reload", "CNRMod_KB_PList", "CNRMod_KB_Chat" };
        private static readonly KeyCode[] KBM_DEFAULTS = {
            KeyCode.Mouse0, KeyCode.Space, KeyCode.W, KeyCode.S,
            KeyCode.A, KeyCode.D, KeyCode.E, KeyCode.Q,
            KeyCode.Mouse1, KeyCode.P, KeyCode.R, KeyCode.Tab, KeyCode.T };
        private const int KBM_BIND_COUNT = 13;

        // -- Gamepad / controller keybinds -----------------------------------
        private static readonly string[] GP_BIND_NAMES = {
            "Fire", "Jump", "Next gun", "Prev gun", "Aim", "Pause", "Reload", "Player list", "Chat" };
        private static readonly string[] GP_PREF_KEYS = {
            "CNRMod_GP_Fire", "CNRMod_GP_Jump", "CNRMod_GP_NextWpn", "CNRMod_GP_PrevWpn",
            "CNRMod_GP_Aim", "CNRMod_GP_Pause", "CNRMod_GP_Reload", "CNRMod_GP_PList", "CNRMod_GP_Chat" };
        private static readonly KeyCode[] GP_DEFAULTS = {
            KeyCode.JoystickButton0,  // A / Cross      = Fire
            KeyCode.JoystickButton1,  // B / Circle     = Jump
            KeyCode.JoystickButton5,  // RB / R1        = Next gun
            KeyCode.JoystickButton4,  // LB / L1        = Prev gun
            KeyCode.JoystickButton9,  // R-stick click  = Aim
            KeyCode.JoystickButton7,  // Start/Options  = Pause
            KeyCode.JoystickButton2,  // X / Square     = Reload
            KeyCode.JoystickButton6,  // Back/Share     = Player list
            KeyCode.JoystickButton8,  // L-stick click  = Chat
        };
        private const int GP_BIND_COUNT = 9;

        private System.Collections.Generic.Dictionary<string,string> _hudCfg =
            new System.Collections.Generic.Dictionary<string,string>();
        private GameObject[] _dragGOs      = new GameObject[DRAG_COUNT];
        private Vector3[]    _dragOrigPos  = new Vector3[DRAG_COUNT];
        private Vector3[]    _dragOrigScale = new Vector3[DRAG_COUNT];

        // -- Scene state -------------------------------------------------------
        private bool   _inGameScene     = false;
        private string _sceneName       = "";
        private bool   _showSettings    = false;
        private bool   _btnPatched      = false;
        private bool   _menuBtnPatched  = false;
        private float  _lastToggleTime  = -10f;

        // -- Sliderotate reflection cache --------------------------------------
        private MonoBehaviour _sliderotate;
        private FieldInfo     _fiSensX, _fiSensY;
        private FieldInfo     _fiCannotRotate; // bool cannotRotate on Sliderotate
        private FieldInfo     _fiMinY, _fiMaxY; // float minimumY / maximumY on Sliderotate
        private const float   CAM_MIN_Y_DEFAULT = -35f;
        private const float   CAM_MAX_Y_DEFAULT =  35f;
        private const float   CAM_MIN_Y_WIDE    = -70f;
        private const float   CAM_MAX_Y_WIDE    =  70f;
        private bool          _wideCam          = false;
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
        private Vector2 _scrollKbm     = Vector2.zero;
        private Vector2 _scrollAccount = Vector2.zero;
        private Vector2 _scrollCtrl    = Vector2.zero;
        private float   _kbmDeadzone   = 0.05f; // keyboard/mouse inject: axis magnitude below this is zeroed
        private float   _touchDeadzone = 0.1f;  // touch joystick: normalised magnitude below this is zeroed
        // -- Gamepad / controller state ----------------------------------------
        private bool    _gamepadEnabled     = false;
        private float   _controllerDeadzone = 0.1f;
        private float   _controllerSens     = 1.5f;  // left-stick magnitude multiplier
        private float   _controllerCamSens    = 0.5f; // right-stick camera scale per frame
        private float   _controllerCamFalloff = 1.0f; // 1.0 = linear, >1 = slow start / fast end
        private float   _controllerAimMult    = 0.5f;
        private bool    _gpRightStickOk     = true;  // legacy, unused
        private KeyCode[] _gpKeys           = new KeyCode[GP_BIND_COUNT];
        private string[]  _gpAxisBinds      = new string[GP_BIND_COUNT]; // "axisName|+" or "axisName|-", else null
        private bool[]    _gpAxisPrevHeld   = new bool[GP_BIND_COUNT];   // for rising-edge detection
        private static readonly string[] GP_AXIS_PREF_KEYS = {
            "CNRMod_GPA_Fire", "CNRMod_GPA_Jump", "CNRMod_GPA_NextWpn", "CNRMod_GPA_PrevWpn",
            "CNRMod_GPA_Aim",  "CNRMod_GPA_Pause", "CNRMod_GPA_Reload", "CNRMod_GPA_PList", "CNRMod_GPA_Chat" };
        private int     _gpCaptureIdx       = -1;
        private int     _gpCaptureCooldown  = 0;
        private float[] _gpAxisBaseline     = null; // axis values at capture start (for delta detection)
        // -- Gamepad axis probe -----------------------------------------------
        private bool   _gpAxesProbed  = false;
        private string _gpLAxisX      = null;
        private string _gpLAxisY      = null;
        private string _gpRAxisX      = null;  // Unity axis name for right stick X (set by Detect)
        private string _gpRAxisY      = null;  // Unity axis name for right stick Y (set by Detect)
        private int    _gpStickDetect = 0;    // 0=idle, 1=LX, 2=LY, 3=RX, 4=RY
        private float[] _gpStickDetAxBase  = null; // Unity InputManager baseline at Detect press
        // -- APK version check ------------------------------------------------
        private string _apkVersionName    = null;  // null = not yet checked
        private bool   _apkNeedsUpdate    = true;  // assume needs update until CheckApkVersion confirms otherwise
        private bool   _apkUpdateDismissed = false; // user dismissed banner for this session
        private float[] _gpStickDetJoyBase = null; // JoyProxy snapshot at Detect press
        private int    _gpLStickJAX   = 0;    // JoyProxy axis for left stick X  (default AXIS_X=0)
        private int    _gpLStickJAY   = 1;    // JoyProxy axis for left stick Y  (default AXIS_Y=1)
        private int    _gpRStickJAX   = 11;   // JoyProxy axis for right stick X (default AXIS_Z=11)
        private int    _gpRStickJAY   = 14;   // JoyProxy axis for right stick Y (default AXIS_RZ=14)
        // Joystick MotionEvent proxy (intercepts Android axes Unity's InputManager doesn't expose)
        private JoyMotionProxy _joyProxy;
        private bool           _joyProxySetup;
        private float[]        _gpJoyBaseline = null; // joyProxy axis snapshot at capture start
        // Android MotionEvent axis IDs for Xbox-style gamepad
        private const int JA_RSX  = 11; // AXIS_Z      right stick X
        private const int JA_RSY  = 14; // AXIS_RZ     right stick Y
        private const int JA_HATX = 15; // AXIS_HAT_X  dpad X
        private const int JA_HATY = 16; // AXIS_HAT_Y  dpad Y
        private const int JA_LT   = 17; // AXIS_LTRIGGER
        private const int JA_RT   = 18; // AXIS_RTRIGGER
        private static readonly string[] GP_LAXIS_X = { "Horizontal", "Joystick Axis 1", "1st axis" };
        private static readonly string[] GP_LAXIS_Y = { "Vertical",   "Joystick Axis 2", "2nd axis" };
        // All axes to probe for button capture and live test
        private static readonly string[] GP_ALL_AXES = {
            "Horizontal", "Vertical",
            "Joystick Axis 1",  "Joystick Axis 2",  "Joystick Axis 3",  "Joystick Axis 4",
            "Joystick Axis 5",  "Joystick Axis 6",  "Joystick Axis 7",  "Joystick Axis 8",
            "Joystick Axis 9",  "Joystick Axis 10",
            "Rotate Camera Horizontal Buttons", "Rotate Camera Vertical Buttons" };
        private int     _activeTab  = 0;   // 0 = Settings  1 = KBM  2 = Account
        private string  _pinInput    = "";
        private string  _pinPassword = "";
        private string  _claimName  = "";
        private string  _claimPin   = "";
        private string  _claimPassword = "";
        private string  _accountMsg = "";
        private const float REF_W = 600f;

        // -- KBM state --------------------------------------------------------
        private bool     _kbmEnabled      = false;
        private bool     _cursorLocked    = false;
        private float    _mouseSensNgl    = 3.2f;  // mouse normal sens (separate from touch)
        private float    _mouseAdsMult    = 0.5f;  // mouse ads multiplier
        private float    _kbmScrollAccum  = 0f;
        private Vector3  _lastMousePos       = Vector3.zero;
        private bool     _lastMousePosValid  = false;
        private UICamera[] _uiCamCache       = new UICamera[0];
        private float    _uiCamCacheAge      = 0f;
        private KeyCode[] _kbKeys         = new KeyCode[KBM_BIND_COUNT];
        private int      _captureIdx      = -1;    // bind being captured; -1=none
        private int      _captureCooldown = 0;     // frames to skip before detecting input

        // -- KBM pointer capture (Android 8+ / API 26+) ----------------------
        private CapturedPointerListener _capListener   = null;
        private bool                    _captureActive = false;
        private bool                    _rmbWasHeld    = false;  // for RMB aim rising-edge detection

        // -- KBM debug overlay snapshot (populated each LateUpdate) ----------
        private string _dbgSource = "";
        private float  _dbgRawDx = 0f, _dbgRawDy = 0f;
        private float  _dbgAxX = 0f, _dbgAxY = 0f;
        private Vector3 _dbgMousePos = Vector3.zero;
        private int    _dbgFrame = 0;
        private int    _proxyFires = 0;         // total calls to WindowCallbackProxy.dispatchGenericMotionEvent
        private float  _dbgAbsX = 0f, _dbgAbsY = 0f;  // last abs XY reported by proxy
        private volatile int _gmlFires = 0;   // total calls to GmlProxy.onGenericMotion
        private float  _gmlAbsX = 0f, _gmlAbsY = 0f;  // last abs XY reported by GmlProxy
        private int    _injectFires = 0;   // times KbmInjectMouseLook ran past early-return
        private float  _dbgEulerY = 0f;    // character euler Y after last inject
        private HoverProxy           _hoverProxy = null;
        private volatile int         _hvrFires = 0;    // total calls to HoverProxy.onHover
        private float  _hvrAbsX = 0f, _hvrAbsY = 0f;  // last abs XY reported by HoverProxy
        private int    _winFocusFires = 0;             // onWindowFocusChanged(true) count

        // -- KBM Window.Callback intercept (AXIS_RELATIVE_X/Y velocity) -----
        private WindowCallbackProxy  _winProxy = null;
        private GmlProxy             _gmlProxy = null;
        private volatile float       _amlDx    = 0f;  // accumulated relative X since last drain
        private volatile float       _amlDy    = 0f;  // accumulated relative Y since last drain
        private bool                 _amlGotData = false;  // have we received any AML data yet?

        // -- KBM reflection (joystick) ----------------------------------------
        private MonoBehaviour _kbmJoystick      = null;
        private FieldInfo     _kbmFiDeltaPixels = null;
        private float         _kbmDragMax       = 60f;
        private CRWeaponManager _weaponManager  = null;

        // -- KBM reflection (Sliderotate rotation) ----------------------------
        private FieldInfo _fiRotationX    = null;
        private FieldInfo _fiRotationY    = null;
        private FieldInfo _fiCamTransform = null;

        // -- KBM chat cache ---------------------------------------------------
        private GameObject _chatBarGO      = null;
        private GameObject _chatInputGO    = null;
        private bool       _chatWasFocused = false;

        // -- Pause-menu sprites (bundled PNGs, loaded once on first in-game scene) --
        // Base64-encoded PNGs cropped from the MenuSystem atlas (2048x1024)
        private static readonly string _PanelBackB64 =
            "iVBORw0KGgoAAAANSUhEUgAAAA0AAAAHCAYAAADTcMcaAAAAGUlEQVR4nGNkZmJ6z0AiYCJVAwiMamKAAABsYAEEL63GFQAA" +
            "AABJRU5ErkJggg==";

        private static readonly string _ButtonNull_2B64 =
            "iVBORw0KGgoAAAANSUhEUgAAAYgAAACNCAYAAACt+e0hAAC6JElEQVR4nO39CZRdSXoeBn73vXy578g9E4nEvgMFFKpQqKqu" +
            "tbu6eqnu5tJskkORwxFljigfWjYtjazRmRmbtix5xmN5JFmWTIuiuLNJir1WdXXX2qgFBRQKW2FfM5H7vufLzHdjzh/LvXHj" +
            "Rtx7E02fmXOMH+ch37s3lj+2f4s//vDy+TzDQ3gID+EhPISHoEFdNVhOf/AQHsJDeAgP4SEQlHzgIYN4CA/hITyEhxADnz1k" +
            "EA/hITyEh/AQHPBQg3gID+EhPISHYIUy28O/+UWGpk6G+Ulg7A6wsgisrwILi8DSOrDmA80VQH0TUN0AbNoMbGoBykktAbC4" +
            "DixMA/kyoLOJuJAHgGFdq3AZQJ7SMuDWx0DHVqBnE6UDRhYYpoeBvTvFb4J5MNz9FDi4n36p57S/HqYRv+MQphL/l8B43SGY" +
            "Zei/EeBOOE8MAo2dQFNOpFkH41xWcFrRTuqD2XVgeR7oavJw+z5DcQno2wVUwUNR4jk1BwzfBJragdZuYHYauHkaePYlnXdT" +
            "2iQ/AhPXsB9WAQwNA+2dot4kmAXD7CjQ255c5iyAhki9DCVZ1yoDCh5QLftqZg6oqwfm54AzPxIt2nkE2LU1Xr4Yk/jzVTA+" +
            "n5r4xPGAhu0AY8BcP+99E8dPrwDNnUB5JXD5faB1MzBxH2jpARragbvngROfoXroQyMlYHIFuHcZOHpUtU3NA/f8mgNDfaQv" +
            "HhQY5gAUQOMUn68PUt7QNDAzBuzbrdqwUV+UePo4XnqajeAqSyrvAFaH+VwR85M+tQAWtLJtdYXlDM8AqyvAlo40HOzremoN" +
            "KBSAusQ2sL+GMf5J4UHGUIHINzwPFCqBFlqk2rv5deCf/G4uO4NAycMvPufjtd8BtrUAn84CKz5QUwG0NwKf/zrQvhWYHQdm" +
            "JwDfB7p3Apv3ARUesLgGlNaArmrg44tE/Bl21AL3GVDrAY1EPGRViwC6f5G+EYLEYlZx9i7Du38C/O1fYKgAcHMB2FYbJcIE" +
            "C2AYX2LYShQJHifW0QaZk8s10H6mTi4CHB/zeaz7wHB5GPDXgL29jBPPb/+BYKgHX2Sc6Ta0AkM3gdJBYMdjwHoR+OR1YMED" +
            "/vNfoBWzD+jsBW5fAXCPl0oQLiZb/WF7BxmwPAfMTwNtW4Bu6/wWeecBnD8PjFQAL/400BQpS6VjuDkLvPF7wOR94MDzDCe+" +
            "ADRLMh3tFzEWVG6TKufX7Xja+1JfDPozGqeb1oVu/v5X/wxoaAN+/u8DhQpg9C4w0Q9cnQV+8xdy2lwIx/4Pfx8YuQ781v+d" +
            "fuUkyV4Dmo4B02cc9dnb8sldhh19ivCI+U7z3kVoRsAwNghs6gIqPaASQI01pd4facIRc6RXz81nKp2rfSqPvo6SiKdvEH8f" +
            "4wBagzJGuABA3yqCZzRrfE4XavgYkMQyIsdJx9fHnSUEaz+5j3znPLk6BTQ1Uy32vpwCQ7NlLSi4Mibm1w4uNXkp81vv2zSG" +
            "o3A2CbeNltHfMkkj8nJFqrpCHAbBOIVt1fDxVhj+13/vxsKzubm+dIihd97Hs58DRvqBU5eAzz4N/Op/pCMsKr8yBVw5CXTt" +
            "AnbtEU/Hp4X0XN8C/ODfCE3i878ObCoIFhASk+gkL8LH2+8Iokad/tUvAB9eBb7/L4Gf+nvAkd5oRxMRnBgQkmIP1X9UDbRt" +
            "EMwODZu9LDUKws2EaflXEDp9gYhyiBnMSsmvypgcNK2vfADsOwGMDgJXqZ/2AB19QHuDGE4iGjTRiaGSpPvenwPP/zJwlEvZ" +
            "+sAzXBz2sSOiDZg4hXXfXWFoqRTymFikkWEPGJ4XtFvvK3Pxi/T31nzMDANLs8D6GvCZQNo2gfE6qVybVpAsFUbLCesXC/36" +
            "NMPCjNAS+oiKOgjV1SmGi28AH/0InJFR/5ZXA1/9UnTOrcNHmfxNfdZbSTOc5nifELfWrlqZZbzPo3BnycfoHaCsXCg8j+2y" +
            "t5uEh6JkBiEpSJaEz94B2nqBnryuHZhEPSSkl0eBfVwzpBp83FoAttPEsDII9Vz+LmwG1gaN9+Y6MHG2MRCG199huHMe+PXf" +
            "DGkIMcYOA49BxrjVYnutYOS3Fhgqa0jIUXWK54tgWAGwydkOEyxEk5TRcWBvm8lwVVk2RhplEDSvwv50gTlXbJpd2FdkZRBM" +
            "M00Y0svT2xjFlywwb74KHPsC0C3TeUsAxhhePQN89X+K2lQIKsvBrAziuV0MX+z0ceKngbH7wCMv6ZMxKt3dXWO4cw7Y95jg" +
            "9QREGG9eFOap4jL4QtnUDZz4KaCqFlicBXZxjpuT5ZXw3/4jcJPW5r1C8s4VgD0ngP7LwjzwhRdFaiLYpNZPSbJJ4/LO68Di" +
            "NPDiN2iyxIEk38paakPYgSQ5XD8FHHpeERoBpNxOrgkG19oMrDCg3RNkWi0sKq+qXkjlJOncGgQOdROBEYxNSetE+D99F/j+" +
            "N4H/6X81B5JhVDLLCYmfci0buAw8cli0TR/4IZLIOA76xKG/JKfORiYY9Q9J99DkCnMR+fKb7V0IOaDpMWD6I3zzmwx7ngYO" +
            "dppt0cGc9LZ0LgZugmrPZgD3g2fElP/k94G6FuArX7AtIiVtAv+HXxblPrYbuHcP+B/+jZk+TBv0RsNRYPashRDSiCxq0l2y" +
            "FEgMdXaM5kaUYE8KvYRrCVSSkFDNhW0j/ALWJWGMzg+7ZOqD8boqtHQXBoFt3ZTfxN82HlkJlAk2xh03G333NYYvv0zfc8Z7" +
            "kW+RGBpfX4K5/eAt4PPP62mjQkwcovXfLwHnfohInSR07OJqcxpB9rRnJsEXAhfpnMKk7qVoxDaTjo1Z6PXa0pm4qb8NUpNY" +
            "wK0lH3MTQHsvMD4ItHcDnUseMAb8yz9jePMc8J2RDTCIZ3cw/Fe/4YMYdkMHsKfFAziDIPC5BHPzI2DncaCnD1hiwBb+Or74" +
            "LgwDZ18FJoeBplaxIGoagF/4hkg/BIZ/8RtAWQE48DSw5bBgEJODQGufkJQaK5XZQy85anIi4jzeL8wq594EPvNzwJE+8AnR" +
            "lQ/3QWhxjk8BbB3Y3aYPpuj4U1cZZ2hVdYKpkRBJBJHaTPbrfAHo3Q8c7BNMg6QzIuiffz5OiEkK+P1/BXz3XeCv/jicND88" +
            "ybD/aXBiT5ItwfSoaOvgDeDwYSHxj5SAkdvAsZ0izSd3gaYOYmhmPyujnc8ZHJEwwaxFfSX5jfqLJjFnateB974ptLxv/DpJ" +
            "sGEf0LSiPIEE0/s5oP+HWICPqzcInyTCmCRZ2sAmjbrNKCRNnv8R8PgrQG2eWh5PY+5nfOt7DH/xp8CeDuAf/nfx9NHFmJOj" +
            "KLS2GP61h4CFTxzExGwX00wq4hmNz8CYMC+++KSqr4XEBEufKdzihJWEldpawSRqMknw4e9wDy7MM1hi6OYPdfNbmjSu12er" +
            "25XezGcjgLZ5lEWwiBJjMljVaW0/eR5o7iCNKsq07cSYJbQ3PibJc8JkEKY2YZZrpkkq04U7w8VBYF+3eDOjhEbOHBh+678B" +
            "rsyIPeMPZ+0MwroHQRvPB3vkxKP5ssyEqEupPeDeReDc22LPgexZM7F2eEIF8Ivo6WQY2QPUNAP9n4q3Py9NVVPw8Xv/CHjs" +
            "FaBvP7AyLwjzzk6guF/ZbG0NN/mvh75Khk27hCy97QhQI7UFXWuA3LStqgZ28J6KLhz6kLaSywHNXcDaKnDpbWDlAPDdfw70" +
            "HQS6dwubPk28mSXg2ingsS+JsvKB+kuYl6GCL3ofdYVw4f32f86wvAgsTAkit74u9h/qmoDN1UDF4dBuTbj3SObw4aeCaRLT" +
            "i0tkyhAGzJQATylm6pmUWDvkYxpX2hjfdlQwpait2+PDXKYTzv4f8jckcR7d6Vqk+qK2qeXusQyfk2mK4cfvAI8/q4i/Sie+" +
            "19QCbeTQEJvPLJCu7wUmFEHsv/qlEu5fLqGsArgwyHCo26bu61CSEthsHNeF85zI1yYSuJDsmAuM8u1tE5+gj3KNgD9hIYyi" +
            "L4U9PlpfX60w4ynGTppCKDCFwoggkFEiGjf7eejOM1wdBPbE+gYOadhGIM086rmNAbgk6TSimISbiYcX23x++rCNkLukeeZo" +
            "n22um3n1epOIvk27cjHZ5PlmjsG9JeD2aWAr1xaBZjoXTZNp3Mc//X8Dn0wA9eXAZw8AH75nL9G6dT01DFwalIj4HrDqCdGT" +
            "REvm4ae+AvzGvwD2HBTpQ0KuQQUp0B7nWORRQmaltSLwW/9XQjbH9xt+//8J7DgmpFhiDsRTFqdUma5OsD8ns89H74v2E/Go" +
            "j6QPO24b2a6FG46UpnxuCvjxWbKxgxORvbsEgSEmQhudhHvXTuDw58BtoTfOCOZQXQ3secIkVr4kyRM4fd3HyQ+B35ebQG99" +
            "4OPKKNCzA1zlu/oBsKUg9h6mRoR5q0uzTSsgrYcY1svcuylO2Cb5BjLj9lBiIlzBkO2jZ2NjoQZG+wLErzY3kHkGeGK/TeqJ" +
            "M86QMadJzvYyfvCWKR3ZFhHDZEn0t5hTeh4/+P/Rw4IlknQ+zd/TpqawSRNBFswBWk8y/J2/5+HXf1P0O2liydKxkoZ01hm2" +
            "OVJ8rI/CzzCLM9/wr7aX5990lCXShmWofCSM0If6gOH0dYZ//c+ADy6a9TBjX0x/bz7LYY8wTlugNsJ07G3WGVOaZhVuO8ff" +
            "hePChYWzTI5xmjZj1uua14b0lIAjUttjEncxF+N9ZOIjNOE44zDrc/22tVGUR4LC914T3+dmhahz5SrwR78vTQfjPn77vwMG" +
            "FoH/+LPAbz0D7G9xn3awahB+SZgfOv4u49ZfwSS09vseumTjooOsdcy0kL6GAZz8I+D6GPDf/4+APy3U93/774CKAjA7KP62" +
            "7hebn3cvArPDwEuPurlouOkaTtj5cWALuZFOE04ef0OddZfs8cOCMTTKfERwR2YFkSRGsrIGNDYDI5cFATlyVLhqUre9uB8Y" +
            "IIK6HaB949FG4PYdoKcI5ItAKylK0wJP4p8DjExdPt7+A+DSKDC7qtrs48y3gVc+CwxdB1amgfPEIHYBbAWoqaZ0AncxMMJd" +
            "loCIO0lC3rQnn0UXC73n7LgAVBL1l+lWyGuhDGA+MDUdMk0vsH+7pBn9t03i1yUoc/Lqz3zMgOHjs8BzR8N+si12wvXd0+Du" +
            "zTsfo7S6M0SIG+0xFZBDhVwQt0sAde/VD4H9T5ib8Qy0D1etlbG7l/GxvzVFVlMG4fdgtDFXD/jkeFqU9HErsEAkbdlC1sI5" +
            "GJq2RHkdYHwmin52ETi39JtGyCp4PwCbWoFHf0U8E30ctldY7m112sp2EYpFAz+XtmB+139H505boPmYeKk8DH/1baC6DmjY" +
            "KtZyst3eJcVnTe9qT5pGlFXjiY7t1mCskjSgLKA0b2E9uH0L+MGfCyX4C08C5U8Cf/bbwJ05oK3Vx6YVYKQIfONRypsD6/ZQ" +
            "1dUDfEvt8WVgEOQhQvb3pRVgqVIuMGISRUHM+KqTENrdwwZSElJtCenBCeHV9LOvAM3j5PVTwj/8fwHHnwEefRx47w1g4pYo" +
            "k/Ycfu5x0Wg2rhMZUUed0Y00adTWLJm6OmlvQRswmnyn3hZumWXfABoKYZcS46sbF+l3yBLvVwmcSTSlv5Wa1F2U9dDEbiOB" +
            "iueNLhjCcWoBKM4CFXlBmP/dbwNsnOEeGHq2irMQi6NCS2spAOdfBbYfBfZR/nFPmx5RokWER7wTjIOkZ7JSCFboBbjq04v2" +
            "EPqUZ4i0clfIvgr7yaZyZ1lEcfOQOdGXyHPlXWDLfqAs1rZo31F7Rq4Co7eAhgZoxs/oAqLHjNxGVsUk7CT3UOKJd4H17XQ2" +
            "ITpPqmgRaFIuabT0uTQGjNwCNu+01LO9HViuBYbuiN/j8q/VpBD+FkZEnWFG52O0T+2EIL6eQtzu0xwyCKX3zJfAZqeA6Wmg" +
            "/5plHOJ9GGXq8XbE54RNU3gQYLi4BvQWGBpS+oHq+LkTMgVNjgheaeatjeEUlpU075lDi0Lk+U054tv1MjxyZStqLrNZNXAT" +
            "R/1ddK1VSgG1ljz1fh64+wnwOnlGDgK7DwP/xdPAwB2gfzCHn3uUVoSHxud7MXN9AMun7czBySCaNgFHXhJWokv9wOO9ytxE" +
            "jEIG6SAPKQDXidB1ikVJdJOsN75EdmxBmAtOPAUcOwj0+wxvfAc48Rxw7Cgw7QMzo8D4iNAeDpDJyqcNgBwwJVjAD0jaXgBq" +
            "yEa/VXoyTQPdTcJdK7p5HZeefuYZgSfftCVJWhJJfVNWDQaV10P/+eE7WnLT0lc6OnDRQSMet52k2wqxB3DweWDkL4Ahn/rE" +
            "x51+4F06uNULPPkV4Panwlz1TJ9Qt/NZjmJIII3m+nWgpRc4Erh6mupsFGgPgjSIZqeqavYjEhafa5FEf5NgceRpebAu1r4Q" +
            "5yUpo375JW08fXv9/CzKynLwvBIeeun780ClD+4ZJtyHFSE18RN/d9BzLsaZKrwEmmzD96UXQdjWcLaYxDOXWXp0L3hhNorh" +
            "Iuu4Nwp0tes1MbC3v+uo197uKJh4mEwi+jw8B2S2PclfP6yfTIAH88BdvibMOi2EkxYIAfm8OnF2QVaNInwfPUCbTsRpihJp" +
            "5fOPHFbA8K//KbCrG/g7v6TXS3Moh+aI+cmuWcXXpZuBqnzfekNsvJdVAxfPALUNwIFngGPUyXtEmSSssh4PFc1A8RaQ6/Yw" +
            "c39A+Pmuu9trZRAkMR8giu8Di03AfR/ooR906tkHPn4HaOoEtu0QB5DIQ4n2GejU6t6a0KebPIuKZFgHcPay2GcgoH0GsjX3" +
            "esAXf07gN70IXL4MbNnDgKY2YGKEmyfoQNnnuYifk0SDwW9gyEkCQrZoMiuEnqrRjqcB79SekTCytArsLRdcV3hD6YfgogO0" +
            "kw69NQK7IwQr2plkRqPXn6wCa8vAkVZByEYOA1feBy6QSvcscKEdeL4XWKX+3Cvy5X3aFCZTGHneq7/uASNtanYN2L1Dmox8" +
            "F2OI9kNX5LyDbXLa6nQxB/U3jXnkBCG2St5hWcRIdjvNDdFnZOmsoFPsnLCHeLfLsa2Sy1Gc+I5rNQqoLm6N8xmazPZfuwS0" +
            "vQQcegz4+P1IPqGxqbK8wDxWGZFuXSYcsw9VOTbpPY7zjlZuQuZtTez/TW3A5HiCKSgLkRXl0wn7kxeALYcYDkRw8gymYJZp" +
            "RgIQ7raUryzm569L5vTXx9kVhtWJBTzR7OpHl4Dj6kdbWh10zy53Gh2U+CFqEub4v/X3pAYboRfmIT8bbJSBh3Nt6AowelN4" +
            "XObqgF/8rHxP1W7OiYl+m3HOXKQzt2T25AK4o5o0BqHLXXRKemoWKG8Q9a0x8Y48ncg577Hjwi7M968lkSXiSKr+0A1xoIqA" +
            "jsPTJnTHdqENtJILrR62oQYY2kPmAobWG0Q6GT64CrxMh+8aqoHDx4FV8sd6n1t1lXmnMfOUFynovNJsOUCBGqg9dVKqrnSY" +
            "A+jfflnbG7cYXtweJapEsPgpfwDvTQNH28VBNvKm+dwLwPUJ4AR1FICv/orItSLzza0ArJJxzYsYHZPEn3FTmE5s1MYaqeYM" +
            "B4P3IhRFIVFSFL8LiX1kI0jZFknyc126tJk4osSR1HMaj/DglEoX7rxwE9ORE8AnHwC+CLVxXzIEOtm/rRW4vwrsLgeuwueM" +
            "h+C9UWB3u5izkIyE5gIx5Hk5D+t1hvneD4HdBxPwDvEn7XFf4HhtY9g68UsDN+EnQaZ/EWirsUmbWl9amYOZTv/t0ig93icv" +
            "HBJaa6zEw9weDJz/yOgXWtxNAJm+JJCWSGYlWklrkVaac0HgQ5rxa3cYWOBtmLTCXYzP7PM0Ap2Wn8XKE9qDMJ+Sl9/+iK6Z" +
            "hKtepvnc3dZQGAn7tbEVePJngSpPuFSzBk864DGxKUrfON3WsQrrS+pZK4NQWS/OAh+/Chx9SRDQwWVxIG3XcaCjXEiwC7KQ" +
            "m2vA+H1gN7kgklS9Blz4EBgvAk8eBZ59WhBi6Slr7SyScr9zAThMJ7kpbs8eKQnOEkklKldGwWWAy2T4UepaUseGE2dVk6CJ" +
            "yJKRQh028lMnj1j4gjlE0xFxaZZPKY6RUJiENw312aYG4OwEsL2FVFGBITFQIlSbpNpDaRUhO5TAHNLZ4EYlFBOylO8+JEbM" +
            "yn0qO0kSFEQj2A6QUPbiV7A+NSaYgWTfZFLCx8In7xPaI5OeqE+RXztJHcRgOBKMO1LcluNOQsm1UaBFneaU+OSk5BiNqUSm" +
            "VB+4cj6xfybkYcf9VpOLvvCTnmU1hYi9vaM1LknUZE7x/PQhrTwa8sPGaKLvaZqSJh2CfLc4D9SFxqJIfs4cwvVSrdVDmnIW" +
            "ifrlvQmvTVwi9acJPK6+d0nxXgK+tjJsjCmJyZl9a2qiDEPaaZkGbZ/t+28Cu18ia4ysr6Axh0TGqePqarsrFpPM1toA/NTP" +
            "h94vNVXA5BLQ1iw2SEnqvTUEDNYApRLwzFYp4RGxLwD/4D8BzowCe9vDU72qbEGw4x3WukVoIeTVvQ0Mr15haOv18ei7b2up" +
            "FNFMU5nD5yVJmFWdik4QHso04QZbhwqY1Y5Uqc3jebkRnJP9MNYi+lBpKJsM6anAObttAtmlKyG1Mr7nQWEiWKrE41K7zYlo" +
            "I2YmLqZUHEIhERdbn4apP+VmP8XExfO1N75lpI32BwVAfFIGF+SnANo6gRXpQTE3w5l/rczXXc5wqz3eKu6lBw9XwLDH2ie2" +
            "vhTvX/2B8Lhi3Axi0zRs/aiXZUryJkTTEGEX88RSX+9OoP+6Zbyj40lMJi5FqvJM5u+aNxJuXgH2PQJUVIhDQ8RUY+2xCTo2" +
            "puRaz3p7XfPJJfnbNCW9vXobbeA5vmcl9PE5a19rUZyZ1EqFQCQsBFduAfvJ+iKFWiVoNm8FHmmg+S/rIROPBR9Rs9Dg+FgQ" +
            "h5lPFwvtfm2yHe3kx82As8PAJ6PiMXk3bWKAx4CrE8AHfwH01gPHmzyUMY+fk1hnQB2drWMenmrz0Bz0jYfzq8Dv/IWQqimS" +
            "6z0GXNber68A1fS9r4v/7tohTipfLDHcZwxTjGGclYSLENebpM0r+CvKob9FEgTls0oGlAfvwk8589Bt5IuWqX/0dB5uMWBw" +
            "CGhlQAsDehmwl3nYyjzUyrJrWI7/jtarylHlqmf6u2iaRcaCfhpnYgzc+VjCd9VvZpvTPnofeEYZSR+9zaqMUiw/hT0ZtNYn" +
            "PkOxZ8Ae2tHmZedEntFhYHYWmJ2xtM3D9tgYhN+vf2xrr95XOflR9XlobKd5b2uvuw9XYvPJNrdsY0dzyVU2LaTrlnzR9tOn" +
            "3ZyLOyjCZoWcY645ZZu/Em+KDbPvCHD8+di7IcZwckh4LvHnVVUZ54wqgxjfLqC+IRzjDc018535zOz7NHywgXQuumHWG12f" +
            "NxjDf/U/AlfPAdMzwI/fBD46B2zZJiJWNMq0s/Lv8322sbLgxQ0uYu+u4VkPva9skw5HodNRdgbByxRciTZFyXR0/7rwHqKI" +
            "3KTiUJGbNwHde4XUF+IiVHZh8w4dDFV5fQURjG9qSOwDkNq0V0qAlO7qKbGxzKqF7Le3DDh+DNiWC72WyCvpm+8BJ0doY1Js" +
            "eI1H6hHfVzT31Og7L4aXwt0+7vo/kZbcVl//Y+CpTnpPx8cI/5xWnl6uXr6qw/ZP7K3Y8KBzGZ6slzSshqAuVZarPrJJCp8e" +
            "+tB4xusw22/D0+yPeH/Hf+v/wme0T6ae0Z7TH7wunBxitFZ+1rQ9Lv1TH2lz2sccl2i+V46qNGTuEiZIsRMmxpdVV4M9/RJY" +
            "TR1Y1xaedvth+S5D+SpNhfMd20AbbOOhj1fSXFN9SnPNB7t7E6xZnNggM1BYh5qLrrUgfy/TcTaATU2AHX480jbSlJ/q9HCg" +
            "TD5bWnKsBce82nUQbGUBbJaOyvkZ+8rLuM5VGWlrH462m2No+2fDN7pu1fl5+lCgifPvAUVfuOZ/+B1xbcKXDgPbpMu6yksW" +
            "iXIn7bKtSQ8+EeVaD6VCBfo/ueMw02cxMbGQq5Df7ktHxXkDbrNfEfdBLMs9g68/D5RJ7yIFa/JTkOYTHWhR/41fZbi/BtT6" +
            "mm2Sjv+u+Bi5C+GhdPEaroH8vsUmMJVyVZpziJBQhNRVitAp702Nh+EWONCeADGwllgDbX9dEDW90EG78+eAX/6GdMvNBElm" +
            "iBBGgk1vNXChWrwHPj6YAnrJXkeePF0VwP0lS1k+bnC7sQe0dYAfwDj7QbD/km6aS9pDcLUhbgtWc8AsQ7kFUprXXhOBEb/+" +
            "WWDd4QpbppmCwvrVvKkEVpQY4DLTpe2/hHk/mGD8xP8z3Z4chxxw+BiPv5IvlKFEcVruCxYXmsOS9ldc9bnSJo2Jaz9DN4uZ" +
            "9dieqYVPavaqbI+rjiTTCgPu3BDmJfJCIS4fuA1T/3hArXRVXVhCP3x5MNFss/5bq+OqOhouCGl0Dev4mTg71vfjT4lwC/A1" +
            "E7c5v5OApYyT2/xq60taHzNr4nwW0dPxG8DWg8JVnqJL3DwDfOVrisbo4b9NU2CGdtC4EN0YZlh4bSVanjwflplBqHiVevhh" +
            "IiwN8h1NJwoDvjQvAvCdaIs2fEJ6MalzEWEcGRE4jr7VkEdT0I2Mh009OSa03VNjQHMbw+3zwPl5Ef6C1PkOObvoJGBxEdjV" +
            "rAJxCU8gc0gIfxHww3wnAtKJ+wr0habwTIaPb4j7HPghrJ37gBuXE1KTfUuWvZK0UaXbmbW8Bl4NzUIiwNZ2oLMTuH82hjcx" +
            "xfExYAdtFI2NAGO0lYoNLARHOxKZRRzs/RvmJYZP7tJffFnM0XUVVsSKo4MZceZgvle/pXshnR5aWHIwNxXrSETAPfsacPRl" +
            "2cfdvcDWncDSEr/Nab1Iu+JzQTlbpWTmBrPttrbY0rjyuQh3tD3xMlRMJj2tyThtRFa+p4icxBhv0N0kJhdnGoMh//hHgEsU" +
            "zFAC3TImy734KVDar9y4bW12tdfDpgS/IHv7zWB9TDIHUQ8Jru+PAQfafPk+y9z2MtRve27HnFblCoXDaBG08hnppTFG2nQn" +
            "sPspYopq34AgBxx9Ajj7oWMu23AQ6biuQHfUzInxm5PCOlJkXOurMLZhXDIZlMyBbh+7/gHQxYOORYEYwg6pYVQY5d6RRMA8" +
            "MXqHTA8XgC/+GvB0m4d98PDlwx73mCJvqOEbYiOZNn+JiVBojGbnRGMBsSVtIx7x0+McezazlBlOXsK7vU8ELOQtomh79K7g" +
            "wYvc1CS5E92gtCI7gvqKpzE/IcGMn8WI/g58tTu3ACdVSOpo28nd7kRbRB10EsdsoCus+u8k8OQdGu66+IHKVeDeitAOaXzD" +
            "MSXToYl3vK22emNpdx22j28DzZC8vJhG9D1FKN5DompFuXB5OveBWJC3rgKXz2sEL2nOJPWPbSyyaDjmGCS9E98HtWerwSVd" +
            "Wl3E/JqbgLZ28QnyGn3Utx353q3AvkMGMbL09TSt7ijw6AQAvsiZg405qfKSJPSsEn60j+oi/a7EUxFV9/Y5dQ2XTudcY8c2" +
            "sF5UmTamExJsOgC8r8VDeXAnpcc985Q3Y0/gcaeYwwkRAI6PgwPa9gJ1WyyMKcr0Q689Mge5T+nao7lqReoVTfrA9AjQ0ALs" +
            "7AEOPAps9k2ZgoKLqU4Qlnn1fkV2Aw0aDUyYj2FwHDy8eFVQnmgInbw8+HeEWWe4BJDDiPA2scUSsoE9TY3EMyzDJuky7W4F" +
            "0bnKT4fug1j0garFGVRxRiBsWstKoCWgXaUyCpUuG086Za12M9GKCsRKez0Mt68Du3aZuEtmMCzCPfCD3oTDux/GYv+E7bBL" +
            "kRvTHuwSWTJRdElhUUGDrNbEDMh8eKdc3PT24o7oeP7ouohTtTOQYaJqtQijbYFtO4HbKuSESFtoaMbakceBj0nyCiF3+Dj8" +
            "tVVU1TVi+bW/4GWS4PPeEvCFg3TF35kI3rQ3UVKutol94pYaaQrQQbFoDu+vof/1fD4+HBcX2ZTXCM3s0zvA0jTwhaM0Z6RH" +
            "0eIcsEhU6ojos8hClvOKdkpPvgn/2JNiE9J3jG9tFbC0DAwMxEwftO8SjyFmmy9pErqNadjMLArMuSN+XyoxfOtfA0sl4Bc+" +
            "q9Mb9oCCFLPksb2L/t5a7cHn4ruo99osw/f/EPil3yCHsDw3xYqdFw/4zIsoK5QLLZZiZsRouky3dTewuARcuBd5zpg8Ta2l" +
            "L9/jYfVqcuus90H8H59n+Oe/FrdzkY04vwL48nR0+J6l/n71orgH4pFeYU5QF/4om+Srn4LfNvXiLwp7882SiJIaxi7x+WGn" +
            "g+2hahTt8Pgz/Q7s6GDbAsHZJl/IdYk4hPZ0PZ8oiy5Covsd4sRZA2IWnmTflXK3nR8kFPVcYMAh5c+sARHUEbnpl9NuYhb+" +
            "7OTe2QyMqUNJ4SSPXxRk4u8iOLb+zQo5oIqMqiq6owliLrx0UOBGajbx1F4DD3EgiEAEOozjapPi1Xd99XjA3oMo792G1R/8" +
            "VXT8q8qBukZgaQEf3lnA6jJw65x4tf0RYHev4PueFvy7IRNh0nGQaRvqgR17gY9POfrfVV5GLSnyTswBmjNDi3R2wpaGwox7" +
            "QB3F198P3PgUmCXDA1LWlu0cjCfs+9co0uY8RYUTIYQNfPS20Xa1fjbCXaejPitIiTiSRtWbA/q2AWvrwGC/wQxcJ8I947mt" +
            "3o2sD8YdHwZuAo/z6BCVQFsXv1ntfYomfQvY9wpwuNJD4cUvY+2N74f2nOpKYEl3uTHbaNPoWIRB9N8G7lPTOzxx2YwsY3kd" +
            "+Nnv57LfB8Gz+spOpY7IAwXu+SfdqORF65wTSUG4IpC0FQiEL6wB534M/PTfBurla9p/6JcnWUcmgWsfAHWbKGa5YCD3zgNX" +
            "F4FjTzNslQHZWtvC/G7OrDpKqJFEZKi8fCJBsQ+mXkfIHGyDQCFFPORaAD8ILGYB7qPsCZ27aMT0VmczLPNtWZrKQrXQmCCj" +
            "6hy23q74pTDZwSUV6+8TFgyfyCFjFxAy5ekhYOwAhUDRLzaKQiilx8c1io9pljB/U5CcS1i9fDHeuRRzZYlmFsMTcge0YzMw" +
            "cEtEFb70jjj5//IJtQ9n64s0oiHfz8wDZz4C2juA0VHjnuSk8lzamHjGIw3z73Ghh9ZYR7V0b6yrBgrlwJTwY+NQYsDMHHD6" +
            "AwN3O9EmQYVyBxcK6jieel+EEqako/oiYNazEGGE3Y2COQ90nB3jr/LcuS3MFE88i0JNLdbe+b5ck1lw8RKeJwnIUaDhuPi2" +
            "0Owe6VgDRoXDw+QQ8PM/S7mFq+7a2feEC6yCRRtzMC0DtrbId0rGos+wIbzo9RhgZRBEWIgpzMsT0YNXge49wOYyYd5QcY+u" +
            "zwkPFApORVLt+BowWxC2ZN18Vl4AHn1ehDbQeXafpJH9K0BjJ7Dr8dDqQpf+0B0JIuY9cI3CgtdS2I1wEG5Jwhm9WlOXUsSm" +
            "Ou2F6KpV9DRp2Lkiv2uBhmnVITV9UpZoEY7n4G31wO7oE9YC5n6phPAQU3SRNkWCi+g4qbQ2qeJBF5stTdJzl9RipG1pBiYm" +
            "8fEcsDALvPGaCGh4iPZoYmWmEU0d9L52pXMFSYszHNon3EmDu53ho0lg8Drw9iXgCwc20rc2hiUX5ehIhnJc42n2uTgJ7h49" +
            "0lg9VHzuK2D+OlbpdPNU1MzmHnsdd9FP9L9wLNHxYKGUu6JLPCb+FISzUXhZjA0Bi8sJ2sNGrua01Webm/I93S0weA+rxCx3" +
            "HgDq6gVzs85b5tDmkoQnF4i8pMxNrgBXTgGHvyrmxJ99E9j3GdHWAAOKb/RAddkYVtQN1hzXpNVvZRCK1xMjWCmIu3/pGV0+" +
            "M3ID2LJbEGUKdElEm4wbZM6k36t0PwKF1OexmgQilHaN7j0wulxtXrV2i1P7dBBzVprpqf4t5KEp0zwtLzPRrbck9IkTykLb" +
            "mZDSTZVsMjGH+WWgUzsmTek+uCQ2mR9/heFIbYiRsAbaJVZlrqFwEv/6/wP8U+3i9fCvDzacE5dYj8lyYxqPPAAU6W0RpE+d" +
            "Qo8TBQr1rCRFF5gTxxVd06V5qe8284XeFy5C7CLS8veECL1AHmlPfl1cD3uQzmcFxDMrQ7AtWFvbzGe2fDreZjuBxzYx7DwB" +
            "/MkfAOyAjVik4akz7yRCk8bg9LQy3SOPAefOoJPvRNnShclXXv92BqKq46l/zwFbtwJ37nC3UF3Qipdjtkl7tmcfP92Ou3dk" +
            "8izMwTaXXPU62m9qMMTE+u8BPZv5yfuy8kqst7cD+ZwID7KoxDRzLdjWgGscTRDPSG985zWgvhJ44auC1py5BXTsAPZ3eMZa" +
            "cLVNq5M0NrrMJoMGZIqRFe05FEcVu3iAcxArLAzdTWLDalG4OpN/boMvNghbZWzmWR9YqRVaAxH9c2PAlTmgoxugI0WQhJw2" +
            "yxWtXpMdRnSf8g2WA7VNwFYtTbxz9OZSIDH1XZRPuFZqHUlawjwD5v3Q1Y02yLfvA/eYOv09YPvXo6YbUbKaDILDz2shpE/e" +
            "Ab7yMxAL9KwKUqbhteSLO19hui3bCG9UqdiXQMg2WxeeS1OJakbhs/SJ5IY0DUVfULY0QkOjg5IUlnsfcX6+QfeTaAdZJHHb" +
            "4s2uZdEcOkZXygZnfZLGwYafjXhmaYNtDDU4qzbQXVqXmc81D8z26P3tASeeRHldI1ZzZcCN61pZYRm0jkWE2YT2kItsdZW0" +
            "XycRP5smYKazlWHmj7aV/qc71QOXVnIhmhzHOnmqkVZTUYHC7sNYm58BPvrA0lcsw9w364/ieOkiMD0I/PqvhlcKHKOQ8wTB" +
            "ZnUWoUZKqxSbbkltACQAudkTgeHXNYhyitzMJMtPuGrAuQeRl9mJWM9QRNdBoUnsaAHKFqIxd+qDU62CQJdXM5z+JtC5E2h9" +
            "WmyuhqkFUuTJQc5YSu3a1xeapcJ4MySx+3yvQoSNTl5gykaspHDaD6mtVCe9Q4xpf2j154ByHpIkbAsxgjN3gOe3RiUGfdie" +
            "5wPqgfEFauJgThC985OlRRFN1tY2m8SfJEWngW3y2YhGEkNJI3omnqJf1mV30FwNz+YkSWpZ2mVjhLb2peFrr4u8545SpAcH" +
            "00vHLRQ00qV4M6/+3jYPbONglmfLtwHG/sH7PFKBnUCK323BGklgmmTrn9VuGrNK4clClPtdkmASPitKF3AOY9MAeQTV1iHX" +
            "0QnPK4OXzyFXU6+5vqYJYjaIp1MRHV446OGFg7a5lF1gCYCQjPRnAqzoZ7u1+sjSMZrM/Jx7EAqIQVCYC48cAIpAgcJRy/lO" +
            "ftYTq8Dh8ujgNlUDjW1AS4/QNpI6QG29hHsW0UlC5y4+OUk2JjI3hfsf0bRuUB6lZp59VH6Ec4qIr2d/CORfBp7pEfWvmppJ" +
            "RU6chU/1rkiSqKLPKGXoaJIkqSQxDPHuZsBMbfWZB6b0MtSdGLI8HrfdrC8rgWPOydYgx5Tmd/wci9l216JMep7lmSuNLLO3" +
            "V5gh5LjER2sjdZiEz3Mf5MrMGN3jm57GxEMn/qa0rN6paMjm2BjahrM+W71JDNySt0Me7BkZS2GEhiCWC++R2RSkk9Lz9BL3" +
            "vPL3HUFFfSN/VRodclxu9WBzjKKw3rsJHCCpdFMTsDAngsRZxyqL4GXmsYFFYFXNDjRheYJaJ2Ubieaq7OH90gWRQvReWgLW" +
            "CmojGPyuYTo0JmK2hwjRUL78hTBGU1LDi9pf8pBQ8dUV0Eb20tPA5XN0Y5NgOuJ+BVMid3WUuRhseIjn1IynvgZs5nsu4hkF" +
            "FTwuPVz4E84ckia9beLankdxFNeAJkES0wj/Uv/Zy2HauNqltch46ec5EiVjOdkMgmLTStSZUBJAVNwld//p7/QFlcYcXBqW" +
            "g/DYxoWCj8lnxDDNa0yTtSbzvb0vFFDEMWXY3Mhcjb9LIRibuwCvDOjvt+TX+1cxg+i4iEi9Jo56GUnr0CSAG5GYZZ4R0z3Q" +
            "hoMutEk8+7YAt/VQIoopy3Jnl4H7d7Ci4sBPjCYIK9jAvBNw+wKQI/M59d/0rCMwnm1+2BiegrR+9lJGQL6nTduAHmxkk1oG" +
            "siSo1gptr2OYW5IRBeHh6BFxXSQL3KRYoHXQh0wKU5LwELOKxq4RnVAr1Xjl+UlLs9vomL3wsbxTnKbu2aS8ssyFbxIRBVkH" +
            "meH0NDA/BbRTHfLUN+2LkA+xAgobFr/mNOg543v0nTiXYZt4pjRmw88sP8RbgJCKwo1ue1+IUOpJ9annvuMchSrblByZ5dpG" +
            "/YY8Uc6CDG0RnZQuApkkUdlwNyVgPAAhJ4kkKgTEL1tKqtv13PZuI4QyLY+r3whyqDl4HIvkFnhvwJE3xD8MP68e5fheBN4P" +
            "Q1VkE35suCDDMxcjdxHNhPpv3Y29j4blYUA/nU6gTYoFsVkt6Vs6uOZk2Cc7DooACpyGkFtxLG1SecYzik9EAeic2w42Ji2e" +
            "kQMQF+c4LZNl8vC9iV6ubg1CFUIxUFSlpG7fXg1vceuCx6OpXpcSxyZD0iqTeeJmnihGNGD6PoUNl7Zqhq7Dws2OGMlf/hBo" +
            "7gQ+z71LBFAHLOrSgdaOaN32waeDeuSPTHFQjtaKK4v5YaKgHMUckqQlvd5omjXJMMkHvNY60ZOkSBuIsaE2j0lbcPRMlE7A" +
            "jTZXy40rpzTOcHkdWF0EuhuYvOVNj12kl8ms5kkCGTkgIp2KfbmsC/DBxjJejp4+i9aHiBYxKCVPcoOlNghHiixMyPXOtZjj" +
            "9W8cPOMCJ4ZSaRW58kr4vZ1A/1CChBx/XvG5l1Gk+7kjgoEDuOujKsfVPmyQsWdgBMFzkwinaWXymWISzqYxS38xZz3kCr+4" +
            "RiEzTCFK5U3TCCw4U4GZwZxj8i9tB2RzfEphEBa71GAJGB8AtrWFGYmw03eSTMMYgWHj6F08nEUUlCnMpU7RqUtiCq3aXcJf" +
            "eFEwI99Q2WoSQ3AoZT7svGVtsf/MM4LZ8eZH9k6SFnO8DtcErpDERsjnZn9YBjMVxI1mpByLDX9xXkJEUTUnoQELalK7pbn9" +
            "OWCyDqgxwqlQf/kZ/dN3GePfkJjXXED08Y1rFh9EQrdJbp7lu9n/4Zh0Sm2a5kexSOc37O2N1p3GJP63AtEusfYELE1PAaV1" +
            "gP5G7ktWuJo4yWfN1cjly+GT6cVXZ4X0O5aNPnMKHSn1bFh7tuGf9E4vx3ZyOg1nE2zMIazjh38KTC8ArX+TRc6EhXkehPFn" +
            "mTN2gZAOOJP24BuHc5G8BeG+D8IkHkSIZqR57obxvkEKDo1GfjoStPZAiyEkXnS386UJcR4i9HwhF1dxB0O0Q2xEMToBSkEY" +
            "cPGbLkIalO89efWnuP4zjcNnaYPAZ0Y780G/Rcnif2J+bkKt0tnei9/hSeTwlr2Csw9sz8yyo0SaBIDQMUDvY7rMhY4ghm0h" +
            "CG8h1tNHn7jxso0fXXlpjoUNd1saFwFxaFVOqVd86E5lYhS072mvQ34aKoG6covEaUqTtjG1j7MdksZOr5O8kT4EPjotbO6J" +
            "9bLQNfLIYWD3YSxePQ8MjvIDpp8W9SCXZn4bLgYeie2ySduu9ykCkFNwkPm62zKUkYUJqXWu78MBz30DqMwBf/RvKVpE0vrW" +
            "68gKrn52gWP9xE072Q/K0bZOl5aApEYKcU2ha1rm7WjRCWja0GuVoTHu3AIa2oDtdcREktQoVav+3QtMFvvItTZI4RpUt+Su" +
            "w7AcyskiUFkh3KGXYldRmtKR7Z0L53i+0P1Wl1pFWXSoL2qPt6nJSWBKYFm1naSJn8xk+XsKNamu99RAbD7bCKGLKOv4bkQ7" +
            "MPOmgUtDc2mJ9jEhrbW7IOZ5nQ2vugLKjpwQQdU+0P3ppbkycneKWXcScczC7E3cWcpp8nj6wFupUAA+OS9xFmNIjIEON1L6" +
            "5DnrOXBxzYEskLQe08bVBLoKUl0xZnlnzc8sacx8Pn54GZijO+iPAgeeFldl2HMk0cPsaz7UrpPbK0bQWItt8kCvCu6WlUHQ" +
            "wbK1ElCm6Re0GGrLRHhml05yexa4ewF48TNiY7Jxq5A2xIVCCvm0SRo2Sj2L2tWTJCyb1BLWp0xV1B8UIoQG8vmtatGaizQL" +
            "2AbU47e+EUMNI6Gbg6efHDbxtC32tMVna/+Dgpt4RH7TqcmRGUda22LWGZgLZ70OVz+kL4aNE5ms/SbqFpqyPl7a3Jldwzrd" +
            "1UyHr0wX79iZGBdOaVJ0Gs4urcp8Fv8duLLOSgd0eaCL0oUehlnWiGuc1XMzEoGZ1pxLrvlkg7Q5lSRY2BgaLKf9FYEW+68D" +
            "YPjDfwuMLAOPyeszdmwXFrehYWBLexLeSUKejnNcePzej4H1NeAbL+jlWIDJsVTjSeYYGmL97qCsDKLOA+pzYuNThFSWhfoM" +
            "Htmj6XSy3BCek1IjpaNDiBRnJ8evWRXBzZTmEJc49Ma7nrkGcaOEXJSzIE1MFGrnwyXgqW6AHdwP0Abc+HwGCcUF0TStwROz" +
            "fS5V00X4shK7tPQbYRw6wTPr0/p8eNbSpqxj4lrsLuYflksLjsyO4vS7rc40wpSkoZkL0zZmTDMn+Li0BhygKJY6wbs3ApTL" +
            "S9yd7X4QImu+S5sfZr0u4u6aRybhSro9MSvRlb9b60QAw7WsuOt4aL8p+B7byNhnAbHvucqjMcRxEVqWAIoHR/epTw8DbQ3A" +
            "oUeA/UeAPhb2yWxb6HlJ6aNx3NKEnCQm6OGnn5b0VavPlpaH+qZ9CKXBEnMY1vNucJOapKRzg0BVt7htS0dLfa71A1ffA3Yc" +
            "A17YCRzpBWZGdOJom5DJXF/v/GRwTaJomaRI0hwcmADKCkBLg5jmTxJzoG8Xr1h6yCW9Jquzi9KLSNnMmTNPfCKLG/z0CePS" +
            "KGw4uiTQNKJr9mESc0iqPwkHs86sErKJv3h+8ipD+xbgUNVGBARVl4mTPq6udyIqcG3sPcOFZWBblZxHJqxuhCmb9WYREDYC" +
            "WaV+Pb2Ojw1sc9lGoMMxV+uDw7gKLW7OeReTskBBu2slsR0bBYbf/X1xg+pLX2fBlQREm276IgIDWUhoj/XGR4C/BnTuAJ4+" +
            "EFo7dDOrihdHsC0WdDNJYInjFf3r0jZMECEd2TYPjCSsiWi6SGi4rLGY8j6wtTOqfpT4qTthkiFvnJYu4NDzQInf/uNxM9Su" +
            "Y4x7vdg5ZNIEEL+XAgZhThpxSphQirtymmULIDTeeVeYkx55STzrDtpjC7an/iZNThsBFYVGXT9N/Fx5BVwuAQfyWSTDjWoD" +
            "aYQ7aVG7ntkYiUnMabOWTC5h3Pngb187cHfE0p40xubzC97+I3KNOrgfXl0D2MkPHafabe01IY1BCRDMITpPybGB7g6uDUww" +
            "6l0SDmljJ+vpbuJXnHLOlIp7kqCkrcGmAjBtcWGJlWF7nnUd6Hgk4WWOuW2+6czSmFe1nugb62HOpLXroj/RZ1TsvqfE/sGH" +
            "bwG3eoGj0lw+dhd4po+oh8cjOywcA9ZL4tzYWkD3kuaA1h7a33nkAHD9kjbWetvT2pMFJD6EW3AI3Sgrt0EGofq92VKV2OoS" +
            "knlNDtjbBrw3SCG5Gb8/tVyz1hGxF3Hfs0qOwg3Sbo4CVkpALq+/d0/SfjB89C4wcA/42t8Q9wcriJ8cTJLAN0JUXJPdTBOv" +
            "b3/e3KxOApeUH5ckyBxTFlloqg4/uO9jYxKmXocXSnI8rr6GCx30mPW51CgOSso62qqAu6OBh1pZJgIl8H37DrBjl5TIzl8G" +
            "42dUdCZoC3iWtXwXcc8B3c3AyLS6n5IDOXB0RU5Ba+XU5uSCtwkfSfhIGFY3tyXhZSsnYV47mYOZN6MAom1e2zQ9mzZEFwRF" +
            "pWf56+gB4OxFvXBpDJbMcnAyIw3R32dtY1zYo/n6bJ+gX28uALc/AeqbRUyuzX3RTXru8ZhnWMyD380hTOo5Mf+5K3kCPg0F" +
            "4P6tlLmi42mjiy6tP5qXj9SiDzZOgpvciKU9Anq+0U3q2QVg2g+9b0Zlwk1yPhC3LNcCzO3pBD7tB9p7GK6fBeYOAIfKGT4e" +
            "ArZ20cnoNMnK/G1v8D7JoeKmG7Mshr/4Q+DGHPBrvwz0+Tmw3jagX1wOkw30AZCDHsNN4lHIhVFbgxviXOAiEg868ZPyiVPN" +
            "1kUJ0vjESfiqGF4uLcoxaXlsGb0OT2xOwccHN+k6Uc3rhXbxpJghcHMRFAph2wwMkG+coJbPbvGALcKeysGP41n5+Rex8u6b" +
            "PHSwOLnuwt+UTvX3Gi5t1eKOXbpuMyblmSDrIru4NaRC+niJdtnS63iZz21gk0LNZ0ljmwRm+0xGYVvvLiLGgKnxsM0VHqqf" +
            "fwlLr/1A/B4w77hmcm652uBtoF3x/iG9lmi7qqJnD9CxFThSR3TPFLXDsSBBeDenTDlgUwEoVAJz8xY8NHwmiWnbGHeaUG1j" +
            "FMmCKQ/kyvchGDAdTRusJwtYlQsyyesOjNc+FaFq9YppY1qFnCCJrrNHHJr7zFFgj9xEeIIzBxvyaZBlstqIFQsGmYTaRzuA" +
            "IzUesL0LWDQ9bkxQEyuucqbiRLolxWiK3U6VMb/1vcIjBzTkBbGMtNOFp60MPV/0eZV1QWVhDrbnOoHw+YbcwR3mc4W7uqhd" +
            "3FQWjaApy2xuE9JlTEPzgG0d2rPQXMiZgwweGZV+bEQ9AzOmw2WXbljNPYuBtm2UM++qJ8cJYOpc4JqRbcFnWUd6O11zSn1P" +
            "ql8C7fU02O4Wsc+nZHzM5xJuK3MjdSjD0um3HHn1em3PhVBR+8oXBZV34mP7LcLCKMebe6PADKPrP4FHyVQKuQapL7obHQww" +
            "D+ykO5UrgRHN6aWlwkhn6yub8JKgEap0m9R5m40w+Oxg1SDK6GbCWWCBXLUK3Nwroq5y76SQ2SvPapLxeo37n8Wma/IpvY1J" +
            "GjY1VpURpqdvd8eBzTuBQ4/JU7wzE5Jb28p3SVTZcXrzpggieKjSJsGY+NokHhtR9sKLMejCDbrM5K8FTFySJGxXXldfRRcv" +
            "/U9mRz/FRPHOp8CJ/ZYos+evAvu2SRNWFO+q7XuxzAmL3ucMWKZP2gI0nyW0NZg3+nORj4fC5+Yzoxyr9iD7pWgwjBhudP+w" +
            "Mk+YeCbha0rxCQyA+vTyHfu8I+KXL5faNlA4/iSY72P93IfAnGOeBn+lih/RzrIyNS0d73Mb88lAGwamMVd8S0i41ojEOoir" +
            "BC6eBY4dFTRMldfRLvKIKAAy8intC5F2ODgbHzPSGtbXgRsDUdxpiFs6gKo5YECu4QpT6zb70dZWx5hH+sqmeYRvspuwUxiE" +
            "0pBnR4HxbtHPdaoSUlMCvEUjlGwrpoZLJd7IZNH/JhHXsANpyNQZh9YWoLMFoIjdPAXFJE+tz/Y7xEF4stgZ2nPb5RE4PRBW" +
            "DN+kOmwTmC4eUmW4JpMN/7RF5BoHc5yStBNX/cIbi8nxoH0f4eFjKYMHLhN1PbuPoZyPlUHs6fPpbQMvAUsn39aKVeJIVobo" +
            "InJJ7bVL5XnJJMKDoK6+Fe256gN9Oenp1tWIsp17sf6O2mj3LBqIWa7r3Qbm3ad3BMGzuZguLQH5FZG04KGiuh4L0xNgrZ3A" +
            "7FCCoCZ96yOMHAbD0PvC7B8LnrFnSfRD659Raf/Q9ozs5QE/+nOgpRdoY3KuUhuOPSLiqQzcFjelLUivtFWXMEdMTXZmxCOS" +
            "4ofTXdj3wnsNCOg2tkTYCCFPYyIylQzrnaLLZdQgpDso3f88Mgk0bBJmpCoVOzziuREOeHgT20YGNOm9uRhsi0BcpF4t58Oy" +
            "/K4rddEyskIUP107iuJDzGHjnNldl8lU3Ti5h9tGOExGZJPykjQKsw47I1JkWggNnpAsdFNsDIRjQ7xsVb5JYGSaBfd4pAsX" +
            "rva40rmZpBAasoBIvyfQMDxU7D2M4jjFOHLVpdqih3FIW1M2Rqb6T+ufwAPI6FddmKoE5s+fAu5PCzMJX/cJcyLmVaTct114" +
            "utpizv8kwSYJ9HzRdpLVg3SkEz9NY5IDHj0IXLgkJPv3zolk++SVZpfvGmXq3xMYpkqTeq9PkhCQBTL0i5Lc1Fwj9bf4gPdB" +
            "UHVrUjqq3yQiqKqQx2FspSzSsevZRherXXpTQAxBaZTq1jJyObXf3OQaDOWWCWsaESb7JwG2QWao168/Nwm+KRW7LgVKqjsN" +
            "LxfTiD4fkPdmB8ECd3YBN4YzlJ+Gh/4+zNP401/FyvwMVq6cF0QsgpvSXGz4mww0iQC52puU1iUkRb+vXDgtpV0Tp6Q+zwK2" +
            "uURcO4eqY89h+fInwP2ZMOYSl2gtdcySGUGaRcaK0jNHx9H8Hl/n4VvJ6JtzwNSDrKQs9CZJMw7Tq5sl6JqXZjojsKcXuHdD" +
            "M/v4IWPork8RjFzfLfQllTZuhAmmzTV54pkLZ2GQDV9R/UYyvZM5NnkbwMogyJReLS0cTXqQPB/cM2l+DHh8K7mtCSBLsNgy" +
            "tC12FQEyawfEO3JFCxgnXDP1tMIGTFLoqiSM4b3USeq+BaZVNyZNuiQJPw1cC0v/reLhZNUA4hDGCDKJk7mYbUTIxNHGZMlz" +
            "S14jqQExBwTzxQMmRhK8edJA5bOZjgROMyd/IAiXCL0bwa/wwhNYe/ODWJ6wbF2q1tO4tYUoXqZkupF+lc+HddFSLytp3JKY" +
            "VBJTIe3Ax/LsFKr2HcEyPgH6Z4yNQhfepveQ8TyDNrAGJvZqJvTrVx3mR2sdSe3UCbf+196eezMMx3i8FKmZjd4Xe1e2fpiY" +
            "j2pOnOhRqPy0eW3gEDG/2da9re1Jz5PG2gM6KoDte4FbV4ARaTJUWgRJcESU1fRLaIrzytEmzWuTmJByCnisC1iriWos65KI" +
            "L8ciugpkyd896ZB+iGXY6HGJgwolrgLd6Vsyeh0UsKoydooxRSWOfLdNKtuCTQLXQs6uQZFLcXi7nEs6Ub9zopM4Y1PlJkla" +
            "WdqRhaAzi/3aQsgmH4Q5mIQ2QbKniR+pX8LBrVglKVk976iUafUxzgEHtgBjQ8CYMqukMQcdxxRinChY2Nprm6+W9GWeDK5m" +
            "wbVabv7HPK60cj64gKW+JmBcxmQNNsNdRNeFr66h2doSLZPcjaMYZ11TKq1el46bzuFsQkC0jFkw7OC3i+aEhxYRraR5uqy3" +
            "yxbO3CbJW9pXJsdnXjfRO9JGcLbVJ9vG73Sx4N5ZhYp9R1AcHQRGyI4k7nLkOZUkrWgGbSVwjc7bWKgN2tSg9txdBMqrAfIW" +
            "5c8p/AmdtNaGgOzN5ZErK+OVsNTFE/6dl7fh0V9xwC3sqOZYWTq4pAYT3NJ7+NsFSUxHz2vDLantAne69EcMqK1ci7QaODcl" +
            "ScJZ25YVWEZpbiMQjkG8lxII8t5eYGhAmEMUXFAb2zQ5iWAWtSxaurv3RGTKWFtsi17HwcWIxTMiQhTcQNw1vUEgD5FqdduX" +
            "Wad5zsboF3lDWBR3Exhwx+URF12HyXPGhp+eT/fQMpgfuSHzUCSuE/BZCadRfo3ZB9E5OCTvf9muDro11wGz8xa3ZL09zIGb" +
            "C8x+kM+IsaTuRWSpR+v3VekRZYYaGVrGyvz7gXmJ/+9LBkFMkUww3EnNB6bpzm5v4+G+SUghjXKJbuErAEvlYl/D1mVR+7x9" +
            "sKPXXJoQPqe2knW0tAasF3RywVL9VfT64uWb712TUJ/0trKiz/LPPI7SB2fUOXtHWm8DeDou0+mqAarrgZuaz3hqmeFoCTNf" +
            "mlZl4pL2ziQSPwkTis4s8c3CEE24orsVRnPz/KRm998G5k1tw0/dPFdlXQXjMXc+/ziZMk1GrfepeE6n00mInwuYRNr4e5wx" +
            "eMcPoWZTC8ry5Zj59qsOnGzS/YMwZBs+LmnYdeeziZNNqzDwD5iDXq6tDlKly4Fxdf1ZhjYGTFXgTrIzHbOjQKp5+ZdCZAR7" +
            "kxSS3TkHkupyMUZXGrOv0vJmhDUlMFgEQqNddJFYsKbyRvq42ScAJ61VbqN1TUC7ZA72AU2TqvRw4+mdocxjdApdXE1JIAxa" +
            "C7ESbMTdRSSVxOKawKIscfo8OtHs2oCA0kdnxOa2U6LMzmyiuBpEsb4RmBIhKiLlbm0GeuoTNad1y1Wg8bTmuKaBDdeN5NXT" +
            "6wtH/ySVqZ7pY2phWKcvA6PKvKTj7SpP/04b/iLtE4+bMcJsoHDxgrvW7W032t9eiZrPP4fGrh4UKsLrmWJ10St5CNDEM1o2" +
            "+2tgDuZYJM0T2xxwrJuAOZjj7AE72qPPx5VBOYvgEZ0HtI5v+0D/iNgjJcgRudvRicKzx4GtTcCSznzc9CsEy7rkYOZzCWIJ" +
            "gs6GwKW1pwl1OaBVP4Mhgx1G9pYyButrYEITWS4AdbI8n1SRdcBfdzXejmApFqlUzxO/s3lyXkQDNuSJYFM8fJa20PXfSWnD" +
            "9kTvG1Z5zM00rYPJhhtc32lbMHEJM46nK5/2/dpwbFOYw40pYFszcod2wz971TIJ6d49cjfMII1nlmiyMLuNgovo2dKkgW2s" +
            "RP5R0BWQ5ljYxkoAWS12yed+5rkmvtPBK9HvKX0zVsT8+Ciq6htQqKwGYz78rW3ADeNSm0pg03NfxOTVi8AlOuJlw+cnGQdL" +
            "e2hPo7MKGFxyakJk0NuWWK6pAZjMQ9oFrotrTeNj41o3LBJ77e4dcWC1uAQsLwKdXcDj5Kq0ro3d1VEUm1qFNl4xE3ot5iSx" +
            "zHTts2cRRjTgextp4/CTrhezjOTy+HWjdA6CR3OVD2XQQ1Z6AA0iL19G3SXTJBS7JFNITBtFhphAdV0o8ZLsR2c+TTNbeJZR" +
            "LN7lyFWoNkjisCGuDbZ8Fg1hOKaRpBEv1/skQqW94yYsk0RJHG5PSeag57XVYY6fK61rnBPwyyTlbQSyaod6Wi81fd6R95pV" +
            "IzLbmbaozXRmXzryE+M/dRnL736IuTs3sLKwgOptO+N5ZoHJgTvA2P0UPHQcbHVrv4OwHo7+JoI9GF6Ya4NtTsuCa54wXFih" +
            "kDiGRM690TZCOMP+ffU/ALcuAEM3gaFbQG2zskBY6v+AoqfOaQfWZLTTmADGLPWlAPXn3p0bmDO2NFnzbJDJMHUHhMy3kF6O" +
            "c5PaD6KxCg2AyhKRVlN5lbVDS3xxJkmFwtOpWX4WpIpOn0G6unS7Ovwm0osopGFtpH2rS9rFFq8+QEmLXJfuzbTyb7ABGraH" +
            "pFC6lrU3VdVOU/tsOKYxk6Q2xbWiKGRhZno6m/Tn0gQ3whyy4qHAllYnwOYz22/G44WFgkXYj7siV8KaxNRG+F1tsr0X5Yrz" +
            "IebYaXlJ9b90D6vox6pNipaMxA1J88Ak4DJtzLXTJvDYJFUt7LY1nQ23MO9BGZYmfj2trsXZ5pyOn4drdDD8HWDrLqBjO7BW" +
            "BOobgB2xQJA6+MAttVGfNgeZJV3CPCCGevZGwhxyla/KM9tsGztb3iT8BUXkn7V43qRSrAxinUJ80JmHU0BzN7C/R4ZYphuJ" +
            "KCRCpERX8dEOiRppzEUcLYO0hctjwpNqbwfQvQ2okrfUxdOH33s0phbHw0bsTHz0yWkjgvqzHDbH7pM2y5QbQiVbvbYrDNMG" +
            "301cwrMmZjnms6wT16xXT582MZMIjFlO1oUj07WT37rN80gHl1ZkSyNx2NWG8u4tWH3rdAK+WRdndJ6otGXBfNHnWbSey+tA" +
            "WRkLzFrZ6jQEC37Lml1Qiz9zCW22PAbzUKdnE+sx50yS0ORi+PE0tDdE0YiJRmw5CDQ1ATOzwBEpxTJrnUlrWyufFhJfszbI" +
            "Ov9taROYTLWH8uPHsPrJGeGlY81jwyUND9FuRtFoC8y4eU4K2Y4bg1iSBlHuAwvTwJs/Brb+ZyJ2DPel1a+t2xC4pGzxnfpk" +
            "aA2YGgbuXgSmVoFf/anwQhY7vxMNpbxr8vyA8LRySSSuM4NmOvN5WFdc2jHL0dsKYEsdrXixVxC8K/HLjzaBoSlx4qaUreG6" +
            "GJgDE6Sb3npgaV7eKJW2UFz1u/BxEVWVRmcyCrIwBw12NiHfuRml4QsGg7W0uTJniXnj6OedzUBFJYq3rwGd5SKuzorNBXMj" +
            "jFevz5aXxa64PNsPDN0A2vqAndtdBCZtjNLmkEOTsOKopzHXrbqZLGG+Wes1/9rqU3nMvGH9xJtohEjzm20Abk4Am5qFEGum" +
            "tePBxEnjpiqgX/c/1Q/MGXsxAd1LYtouoS+FaawDxTfOZBA8tPftOaFVpp5MZ2DTQnvwW4CyLg/rsxQjSowf31veyB4Eeb2Q" +
            "l9Txl4DPPK60SH3AkiUZsedhIwjxfLQpPC5NSWSCrKwGyvLAoUPm/od7glXF9g50AqjS+XLcbZKZbRGY7bUtEtdzDe7MibsL" +
            "I3k8rgKrcOkBrsf3OCXLZIlP76uEdLPzKN++F7mjuzJIH7aJ7sXCFdj70ib5phGGpLplGTemUHr3gmNs9LqYgzmY+NLfHDA5" +
            "DQwMAHfmgUFiDimElkwrVklYZ/auNcJwccVHyfCoI613Vy/w/IvAc9tdGoatLr0fzO+2PHqaKBHOn6DQ6gbweP1Gf7XkgD32" +
            "vblk8GS4c1VnStpE5ht9u79ZXAOaLHQY7abzDxHmkICDnwXnJEjByxXuxJZWpZtJYw5G/xFhHwPWzzFurz/8mydS5otDg1gs" +
            "AmPjwP5W4AtPC4LPiwgunUhqgM2c5IYJSeDJwW2oAOxqBvbJ60GFNOCSwkIGINwP9bte7XiFJbmkueQBoliWY4sMj9Rkldxk" +
            "eRcppymRGuk3EUe/poWNcBEIW1+YErqDwCwARToTMKiHlkyS5Nz90WLY7MmRYKt1spnEyEVUTHzStAuTEGeQGF1lRu7oNfG0" +
            "lNtCLnbz0cN5iZJ5NI3nMeQikTUFbq3ySfIstDElW5/b0sKdrruA9eFbmheRfDepe/dQCHryaiqgrqUD650rWH4juMcyAT/t" +
            "O/fuMfvN7H/Xug/TXV4EmmvEHqAQDtXhUpaw75PSB07wDKHT9T4hf5m03Scyb5tQ4xJ0yMU0O8NifG9AakHVQKEauEOBGB8k" +
            "WF91BdDRGiX0Re2kNNOkfzJtEL0sd0gpH48CqyvAiS3xhk+qMw/y79kfATs+S0iJmqMDnkJgtXLD77aBtTOZ6DMbMSObJ5Cv" +
            "MYfMNtltkk/SwlHH/XVc3QQm/tucuLbAg9JLY1D5g5l9Y/aTbdHq9Xqx4IAslZinSYWeJayEjaDo313ExraYtWcUpoAfNLLh" +
            "YeY16mhuAQplCXd0JEvW+/gBULNc1xw1oMNDfscBlE5ezDhH5O1ZZJMJwjJY0gzqt5tp5S5FqUf+kSZU1ddicWEOpbnZ5Lqp" +
            "XqIwy661JZ9Frqw1x9cOKwvAxDqwuSGJFqh9nzRGmgbMUX6CMGbmj4RHseWRZQVXlbrqTWJ2LpCb1DUeGN2RWgRWJ4DVM8K9" +
            "eOPhvpnYFFaQUwxAj7MlnxVi6Ear29UG/PG3gdEbwNdejKYhUqVunCMh5edepIs3PKCRjIozKRJ0DujKAUO6a5xN4nAtdhtx" +
            "TeDW8vJ6cYG9WY6NgCVJA+YEYdyPe5E25bnRz4azi3g48Ai4O5O30hHDCA9y2SFpsrvyCT1CHMbMwmjTwAtVlGXTLz1JKLAw" +
            "z0NdaOzbjRmKyxTMJ5k2FpLBy6DhEHPwUN3UiiUKRJhIX9KIhl6XjXCqu74Nwai5HYwc1yNFu4LeybLbCkBtLXB1KsOcT8B1" +
            "j4fK2nosvHY3da1wiNyhYKtL/l7Vw6LI81yvvIy14gpmvv1WpG+YPHfxSJv0atTuYJmFL7WJnIgNtOgSIDIw4lTNglm+J60t" +
            "rTzCjWyK45b1O58Fp7RnFtrmS1PNDXXKWqs3YQpY9yDm13g4j2DaJn1UVfZPDrXI4W99BdjULW6e0993yQtlWJ380PMJBnaT" +
            "otnoaYU73GvngX/yF9qzIRGCKghEFXz0f57lncxfmdPes1h90bJsZau0ObCWHNghun1btiP20dPTXzr8Lg7A029ikPcu6vWb" +
            "7U+q30zHeAAuUb78PUuhBxgGnWOlt9lLSaO3QQgKgkR5YJ6XUqZtvCxjR/NgUW9jVpy07yUf5dXV2HTkhAMH1xxx91Fu314s" +
            "fvIR2J2llLQsA74e2GPbwNr1eajhtxbOj+BvoRylk5ej47+jwTE35N/7q2BXp1LwSsJXfK/t24qFt+9G5q19bMz1kq1v6596" +
            "Bh0/9zVUPPkIxi6ewvS33+Z1UQylC8uipL98Gzj5fWXNkHTjaB8vg3taqk8FwJrT1hKlcc0t2/Ms8yTDuHfUgU1kye+qz0Wr" +
            "zDke5uGi4RgDmzdpB9u4BlFRBtQSe5bCizIj8b0HdZ4lFaISypO0L+qrIjUpTd1/S5IDfxkWHnXdBHYfBG7J+Go1VC65kHZX" +
            "AP22I/lJzZZpt3cAF/XYRqZUZf4126dJJGMMqJkRjyInE3VpRcdRx8VDBzyUDjIeDTm4bEd778ZDT+vOR+GWab+nMzjda6bR" +
            "Q3i72mt7Jg6f+bHrMx11WNuRpBVlAUe6iyOY755AQ1uXmLO9dJWmJR5wxBXZVb7I4797JWVu6W1ILw/35WU0vodF+PjmqwwN" +
            "NcCXn5F3apha3dmBaNm0YX5z1mJH1nHMoi3Y1kz02ez3b0ff7cqhrKWA9fdXtShptr7V63aZCBnmJ8e51rD0I4rEKxr08RTQ" +
            "fwmo2wQstgCLS8CjL2o0iGzfZ6gPjQ5Q+ybyWtt4X9j2n5LAy9B/Rprgrg0dGHCLQoinzHHSHmHToF3zymXJkLXKvWNmeZ/Y" +
            "qnw+H3v/88cZ/vHX41yANo3XFxlWSRWQZw7WpP3ZfmuUqD5sks2EIcPWbm0VmxU3KDCIb+SJYCEZlmkKEBNBMRVFo/OxjmU8" +
            "nEdzJH/aJDE73tuAWmkjjjYTWNIAuwi2K69Zv8k8zXKkTR7KTm0j8rbfeh2mCc3E3cYEbHnMPjHbltTfZrkeZwr5nm0ovX8V" +
            "6M6h9cnPYfz9HwKDcn5z05ulfCK8e7cCp28Z+DiAHxrT54SRJzhUZiGgdHqPBAz4uOYL70UWeLklzR+zvDRGa6ZNGmv1zDYH" +
            "JA5BpG29vZZ5xe9z1sqN9bnZrpD2nBoEZseBpx4RQqqOw6UVoLmSLBFm+5VbalIdSc/+usDLQFtcaVzmsaz4qvERZRBzGBoF" +
            "hsinYFErox1YGQJ+45N4mRXlYAnhvu0TQ3EiSJNWlZHq01VgT7l+QZAweYVumKZULu1x9POaCkGob1LHoVqPyKh3CBhuSY8o" +
            "sjhPzgK1DcBOY/LQwpPdpuWNtN5Stg1M4uhiJDYJHxkZhwnxZ5Wf242Vu9eEfdFKQGzM1HgfEDcbpE1Ik3ik5XUxVxtjMPs1" +
            "qRxzzvrA3VWs370iL7T3MfbG6zImv0wS3KVhlFfnoaZhExb9W+nMgWDOxvC0PDwgmqVvCI8R9TyHXXSux2d8Tyec40nzJ+15" +
            "Epj4mnMwBSIypE0zoHnlC2mYCEIF0PnVRgz/8YxUAaJjT9Fvr94BcgXgQI8QQNcXgb494gKzECPxbT/Zmfhzy/hksnLY5qE2" +
            "l6qkhGlj7JlAtTFJW0ib2/rvjTAbdZ5DtIe8mPwVT8Rd0mn7sIeKIx7o/igbWBmE0/OJPKVoQBLwbC8AueACehF2vM3RNFIM" +
            "+6imUTodO5bQWBcR1d8JqJHMiD6bpVEyrFsOfK8H9Ns2t13gkq50CBe5Mx3fOLNIlxxcl56YoPeBh8oXDqO6thrL/Sq5fNdF" +
            "qj31Kwt/kyRHxJE2wlLjzph1IaXtKZoC1U+qW3DGwF1/69dfxOrSEmY/+FDz83ZrSXIJGPjZCDZtDJr97GBo9xkWSmeA+pxF" +
            "2rXh/SCEQHtGV0C21QHX5zADhjfeBn76OVdfaeU35tDw1BMora9j4ewZYNymxSSNtSmU6M9d79SzNIFAvuMRXD2UPe5h6D/M" +
            "ACUfN31gBxEKLc97HwAzc8D+Y8DteeD2WaBzJ7CDvL6CuW3iaKw3WmMiOiiw5JLCXW032rxkNC3WfrO9rnQu7S9rPhNf/bdN" +
            "wInn4Qec6ctCPG0FHRh00EDrJrWn/Z8Q6C8Gq0ZocbqNeNp6naH4LQ62KGTNjwksOS2FYEBO2tj1Os0JS4djNhoUzIITeVxE" +
            "Bj5ndKfZFh/cTcnsCwqLwEHltbFmXarW6txaiZrGZky+92H0oA3ZPnmfaPrdkPReIDc6sclhqUPhrdepcEwiFnqbbW0HvN6t" +
            "2mnGZII7/t6bmCWNSPei5CYwc59D4KnP13h7bNJ/spAhwAeGfckckvBN0s4c6dWYk3mJGAP9pWzXhQa9BTn89HMuvI1nK0Bx" +
            "eQmLs5NhlM5YO9MISwjFSDtSmJqT2NqAYf2kjw+v+7gwDZRFKI8I5T86C/TtBFo2iftgvvQMcLTTLD9n0TIRCiEkFNJaiJhK" +
            "baBrqHr5GqNpJKHKQj8ySfOICHHYUr5BLS9L2o3QMD2P1uZ6YJZoAzagQZBwyQ9WBBdsyEKl9uA6KFeQnlsU8gLynmrafM0W" +
            "msPkmLaJmFBGezswP8o55KD0kNxpLdMlJZl4uCa/tE+LnVlHO9IkN6POreUobN6GtXevWrWEWN3KHHSriIW+CTAeoVEDwq2t" +
            "DtVtXVg6dzUsgzbkiOiWXLhbcIO02dFMGbS1xdZP8bay03dkvS6io0lw9xlw3zhjYL0aU6u3OycGPXKyVEvbovrAbKupAZj1" +
            "JM09lxZpK8NCQKkvpqRNvr0OmNZveVEGflOyNcpYYlj+0TlLW9IIjGf9LryDzLVHV3USf3cxnKQ+jPZJRQVwkNrLTUMsECwp" +
            "VMbnXxKpOnzAo6sw+Ry1CwVBPTTug0xE66RHhKg1JIar7fpvDW9+y5znrjcVZJoeD8sTo8Ad5cv7/xtQG9T8Rjn92tQlgE07" +
            "+oqioNsKE0ep4pxSuVBdXAHuB2+lm2cFxWrNcfd1dbHPRtwH425+Zhrljmj7AOzKKNiCSEMMSu0z2D62K2Yi9fbmwPY1ud3c" +
            "qJ5Rm/ur3hazDYYrYwW52cp0d9aweuka2M5qw/VWd1nT6g7e+Vh+4yJYi4eaL+0L660HGvu2Y2VpDqzOaMOaNllsbbeNG7V1" +
            "0HSrFf/sYy3nhN7mNdq/Mtwvne555r+EeZOTdQz6YFO6+6XxWQFqjh1zzC8TL9f8tcznreWWeexyPXR8ZgF2dd7R7qR+Mfsn" +
            "yR3Yte5sczcXK6s+1d3a5fYa7ZPD3K1UpF+RWzNn7wCHWoQpmmQRFpilPbCdIm5zUEatJ9aO1o6WV54C29YElvfAWsW5afsa" +
            "dM05V98xo61Z+1j981Bz4BjY1dENjElWt+AH+JAXaTHcQuCfmmSvVKsGUUul2TLxEj1UlTOc/QBoOU77TpQ4PC3JVUV+76vG" +
            "pX5iYCnv9As6RAfbL7pQxggWz69DSwdat+0G9gHjf/a20Rlp0lOCtFdFUq7qLy2fsk5NLwszTGA7teBma/8Yw8L3LodoDjNM" +
            "U+C5IVle5MDrTzAmJH3Q5s5YWE406ohLGzPrdpk8wvd0EmZV00YjefSTt9Q+7lVnmhyM+ud8LJw/AzR52ql1s34Td1u7jLRN" +
            "LcDQsCXsQVx6do+lY0y4dOzjR1cY2rYAh6JhilPKdKVLw0VLT33VVg1cW8o4//XfdhgEww9+KJSn3b3AZ/Z4OL5Fbpx2eMCI" +
            "z8/rBKH9SauS87rp5ccw/dpHQVk83YCP8fxpeM0tYC0daNq8FYubJ7FKIWXuUnQ67TKg3Q3A4KxwTAhO6ZvtcB06hGV9e8a6" +
            "0MdbvFug64gDc1da35nf/xpB0XRaL4TrHU9oXXQwOcA7DgleTOpb1PZM3IZOWZNfMlkCtmnvLq8Bl98HXnp2I2TIYTJ44PwU" +
            "m4nuDhbcPprOHBBHPWeGMXZ6CDjeC+yuAq4uOMwFetkmUaeP4anRAuR7t6FEV19R56nyuG+uJ/YJgs3sLP2h1VnS0pHv8ce2" +
            "HTbbhDQnb0LfH9yM9q07MfonbyQT40hdLCMx0d95/L6P8MRCDvmnt6F0kiI+0elcLV/EpOT6LoH3uWtmZjVHGM/oRLW6uTIT" +
            "g0kDvWwqxsO2PQzf/wHQ+XmaQhslIOa4pJlx5fsdlajbfRC1TS1Y2D4Btr6OxdlpsA/vpuMdq1/8/TevA80F4Fc/i0CEa//Z" +
            "FzF5dwBrp2/wDRCfCTf2ch4UkIiXDLnSCky9ehpvX2Q89mVDm7j74VAbQ+7OCtidIb5upoaHuQBR6N2FtZYlcW6EhAly2Bie" +
            "DecLv6TaE6YQHYhortiYOxz9KDezeYh1i5AYcbIw4X8jZmABHkOvDDj6qyfgr63jk//5tKAZ6wzMfqNbCoOwfFM8g9TA9u3R" +
            "ZxfmgE9OA91tYh/Ud0rU2kZteU7orkR/gwMl9gUcjxqSRLBNMBdFgpTPf0pCcuqeJb+tjMAhPGzThIXo0nWt/ZI50ISijpqX" +
            "38n8TJtsba3A/XGDQdhAx8FGrG3tNvPq/eMqQ4NbA2Bbd8LndzLYJGbbb7ZBYhL2mboAiha+f0oyBycjUuXq/RKX6Ny42fpH" +
            "hkUIwnIYY0rEpK4R6F0AbplxjB5k8Rt55ZogAXtxXewPRA852uayhAjeertt/aW3Sx54K5RjYXIc5ZXVqKqjCS1SzXPfw3i5" +
            "VS/sQ0N1O0bOfQDcV3eAhwyZlLxf40E4c/B7PGw6+iQmv/0eht5+U3pekZeb+NtgGjDagU2PPIHT997DrRFg3zZgaQ54dLOw" +
            "VrQpqwC1l68bD8XBa8D2Os1bj8n9JzrfQs9XgZvGPZWVck1GHnvaV+mS3wBU7NyN4uVrMlyGPFtA66I6B+TKgDmKCizLIhyo" +
            "7AppU6MyFn9CbX4jwKMb0MFgD+vFFZTW1uNXdG6EQajb3Mh88Ml14Ze8PA80tAL7q0WzaDksSa8luj+J+vuXXzQNOGLyVr+w" +
            "D+XVdZj57qmwkpocak4c4N4X5ZW1WL1yI/FUI3lX6EGzo+AiVmlSTZpqp6udLjDMI7T5QTHmJ5eF542+SElDoEvDedEqJopa" +
            "efL30Li6pi6F2CS401oXvirPZk6xaRT6c/l3gmGYtAfeBpPIunDNxYkZuRnPyglEg6o2Mqy40WLyBeNV5UcuxNGZgo3I2xiG" +
            "2U8myPSKYZ8bs/cTLbKSL5iEsLtlZAwuocaVN4ff+pLPiWGNzR06iO2jtaVamg/6zTEy+8PSP/sbgflZrE/Nw+/qxvidO2Ck" +
            "Kc2uAYeawd2Qgp08gcvS1ATW5xdAAV30ssnKQ6GgGpV4t6cLWPcx8e33RJogbrxlvhIx3dcErCxh4rsfoLYA9DYBvduBW1cp" +
            "7EgeXUe3YP0Uhf8w8m6rEhH9Am1c64OheaBFic1avlLKyWpGQeo8eJs3o6K2DisU1Xd8LiyDC03EDCRzoO+exuzJjEZJYox7" +
            "o7BBIYTJfY0xYLFnGsvzM2C9sr3z4vmGTlLvq2f4YoePm/NAdw3w9MvAnbNAVT3wmUPA8pTQqskLkOIHPXGYzFo2qU4jJIH3" +
            "jXxXmUP7V5/DWP8tsP4B4YkQyevqFAF0fIlIhrh3VrR1LcJEcpYj7zaiGekOoz6TALuYSFSbKXuyD40d3Zig+4NPD1valGWA" +
            "k9KY75ImnIsIJpVpGT9rPeJeiPg+gQtnjRjpp2kj45OEn40x6XXb2iAZVLcn55gsh/YxYl5V0fGue/kw5i9fAPpd88Yxl4K6" +
            "0iBpLtraYhEI+nJoOfokJs68L/G0lWGW4zppLGFXJVBVDdydRutLz6C4MIe5t84JqZDykIv3lI9/8j2Gf/AlEuHzhh1b1LUK" +
            "X0Z51gUsM6SLS7uXErkiunLOsNlSJIfy3BbOzh6wsxz1O/ZjjkLnXzUuArKOn8SJTLy1kogEUW9tQCFGKlBG4c4X54Dzillq" +
            "7xMhYT1Se4npTMxbNPSs80a9j+JCWwbD94FhQrdFdhwJEUvAyiTwd6/Ey6sogFkZxO46hs2VfsA0/9YvAs1twKnvAJs6gasX" +
            "hUDX0ABU1wCf2W66o6URQHqfQ/kz27C6sgScHxX38aYSMpMYh1CS++S1TmLuWnCutFmZg54mLtWWPd2H9ZO63daU2uJtiUIW" +
            "omQjco6+L3jIHW+GPzQN3NbbaSvXhZ+QSJRRJbwrPMuktYFN40nDx2RiFuJpJUD6e3vbBOSAtuQNPPucUpvLlnwRoqzaIG3t" +
            "U0nMygUevONdqK5vxOLJKxnuB0gTOrR27KG9N2mH2EymtorwN/dAYvjTd4BfeZY3DN9+38fxJ4FX3xRhrb78ojBFizI1bZcu" +
            "DMplvcvAtt5d+XJis5iI30AWgcw2X5LWI4vmp7FssoXpzyjV28pMxdmYN858cQGP6PjQADBCV9Mo4IxaXJ74n161MwiriYnJ" +
            "PZyuGuFpcIiMfKQx7Ae2bALWV4CRO8BLh1R6w8ziRFwfCIbiu3rwryxgMwmJOnMqoGCsTrNu3zhwk1ZPvC43LqofZB2FHDp2" +
            "7MVAhEFkmUBaGpr0kcVEPv/kN98KnCXdcINEYQ0oLS2h8eBezDTcBD5RW8Fp2lEcx3I1+XR//QBf1wI0iWCSxpKhPYl4yrI3" +
            "0a1xWQm9BjI+kgBzQSb0mUt7cEnsMeZgwSWCZ/idnRrCQj15UZluh1mYoJ5OJzw+cHUZqM3xy4Fwg+aIZA7bPC5YVMDDL//t" +
            "GrDLJJaV0NAu9gKeeR7YyovMgx1oAC5TvBsVasSTgqAY56oTm9HYuRnD776vmXZcTN3QLuT3li89honvnQ6ZbCbmgGg5xLTI" +
            "s4fMnrEIA7DnDy5WUu+kF5a6jjMm8Mq0pLlKyT0aiM+EpHWQRUuJQsDGJFPgIAPpsdwDhPturwb2bAW+8lLYAY/1iT0HMiN+" +
            "9SmSlPJAlR5vVQ2gbaG7JrzulWt7b/tulutiHElMy5TkbAvURbCMiRF7R+pVHq1ffRIDH71jOXmh42Dn+MHzZUu+wRImz444" +
            "TnSYfaCXS1FdfeDsEma+dRm4ZXW/seCSRLh9/C9vanPdyRwUPrY5Yutr83uadqLXa/RJwBxsGogAYQpnOOs8jZz0PG1ckVxG" +
            "TKtwtdHxfa6ERW7ScfWBrV4zorJuARAS//xCSTIHDRpIbPbwP7/BgMu0eVTiY//Sr25Dz8/tw4mv7xEmJzrEf4liLgH5rVuA" +
            "bXqMBYHX8uUBDF9UzEFjUDsrgT3VaH7pEYt0H7ah5ZXjqG6V0uui1o/B6WeXMGE870m+E4FDixTMdJBBSznsLEdhxxahVnPm" +
            "oMDAhRgQmVadjMg1ZvG1bP9oY1thIe91WjyiBqn+6yTcAKuJqaeS4V/9PR/beEdrnNIHVucZivyQaw5tX38KY9+Um0324jcg" +
            "4ZrpEyTCVMjKTFxSm0uK8YCjzdjyyAnce+v7waXf0QGzEHlXWZFnLjU3Se013+l5QlzyTzejLF+O4jvDFhxtMaBs+Onl6vgy" +
            "GW/LQ81L+7H4+uUNSvm28uX3XeWAv47OY89ienQQK29dTxgzE8ifnua/i8CEddOZCxUF5IYP7Mxp6Wtylk3FaPvd7XPNpzgO" +
            "2TQnc8zMU8Y2IpIESeMh8pLecGkEONYRpv8P7zHMrgt39q58XrkRCbu5Gdqb7OqVdcDSvIicSfE0YhGDlTs4ubN6gsjWAJW9" +
            "m7Hy4T1jjck85C1E5WzxgHuWdlK9XDOTG5MRTzSj7VTGeNreA7Sw8BsAa+RaHQ8DlxgdSUqXlld7Q9Fc7zOMDMbDiZOJ6beu" +
            "b8DE9MR+aMwhBD+I5ipCPoz+6UnH4jZ/Jy2AtIXhyq8TK70c8ZsUXxGJSHYWbYSqTbZY2SanNRijVv7Wo5/hrmLsVrwcIr+d" +
            "scmcdPBGw0/DPdoevf0u5mnDV8JjlejafhBLczNYaR5NuH/ZVZcN3yjQJjU/VX3piqRdZhpbn5vpDEK+vxrdR05g9O4NjN65" +
            "htLgcEJoEw94pBE4p26N84LzMHShDAceOsEO6pQwwQ4+x0Nc8gc6UaKrRS+H9vdkqdwFJlNJYvxamuBQoF627e4Nc9ySBIrs" +
            "QK1+tI3C5QhC/o++C+xqAH7lM/Qzh5rP7sUCCQUkhZM0yh1RpAswZSbXq8Z54cJKzCGt70Yls/A8LL9/D3QtU7VtbZCJhpJy" +
            "IU3vM/m+oEnykf6zzO1IGQngJ6VzrD/F/7ICxXpfNEPL2OiocDQoe3Izd0Veun8buLrqGGNxfi0MeSSv203CO+0cRLI0woCF" +
            "Eujqks2xRrjKUenMZ+p+hzQwJVffkU+8rzKJkO3iDlUe98oQIZ75JI/YQ6PtuPPRO8iTh0eMU3s8UGA8nxnWeAOLNNjUdBHc" +
            "pLIkbrly9H/rXdG+nFSVA5WeV5KhTBszDr+T4MBLmUnCxUZYHZpDhYeGvp0YpKtCz+oxmWzCgLQhc+ZgCa9Mt0ul9rnbZbj0" +
            "wSAPQ10f9NODjGWyMBNpi/6d5iVNNX6Yy5wHojwy7YtgFGlzYSMQ4kEm9Y5feA75fBkG/+ANfOM5H298KNIUnukTzIHgvnGd" +
            "rYray+NN6czLJTQa7ed5FHOwMFblTUX2fn4tqxD06l8+iLLySkxd+Vi739kUoLR6lQcdrTWqzAx7XynNT8GegkkX430W+R05" +
            "yGn2sYW5EUOlTZwFIy3N8eACJAk+w3r/AKr3HQCmJMUPBApLe7k5SX7vlvMqEIY2yCBEfA6dOTC+E074qdgd3Zw4uBpsdobW" +
            "CQZUSYm/IpaezhTkZMz+OAGrcjJnESNIeDS5QKtDMoRNzx/D5F+dNqQxQ60/OwWf666m15bqK139d0mNUah5cRcW370hJoB2" +
            "5i56IUvYthj+sd8yTZuHLbtPYDB3EeskgVN5Ec8lV7+YeKdpNhJNfieCjeilESijTcsM069dEBfp0MGbi0pc1ImM5mQQ26A1" +
            "yj1Yo5VhRty1tTv6vFZuw1v7Xo1RZGGa/aSXb/aLuZi1uiOM3D7eIW5pYCvHNs9DWJTNG/rDt4C+PF/4A1eB33wB8HtzWFta" +
            "MuL4ZNWQLOvKRj8688KXXj4nEaBR7y8Wbnpv+trjaGzrQqm0iv6LH8Onm5cidclAlz0F4N6a2Cuj3xTZmO6ubi0AS2uCgOp3" +
            "hDCXx1WWdZgGDsEpZp1I8Pq6yzBz92L4O9j/8EITF11Axa8gBhhxfPp2L8Qz6foGpxeTyGNO3vB99JuNOYhQCcIP2p5bb0g0" +
            "iqSWhuKFd+QA/WZQZ50h5PW3Oz15mY5JeKMDNPHWmQhOIS5pk9yUJHRvHjNtHPeFy8QcmDhYR1xSTVAeK8bsYx1nizRDnhTL" +
            "0u6ZB+7+6esWAmoSKBN0/G3aiyNPrG0PQjAkUH/QRTojS7G+XgdDWaz/bRKpfMeZg60tLsleK4PiEU3bmL1Mr7o22HR0CQQ2" +
            "JmvLY/bLRt7bmHECA3KWJb4r5YXWZuXdEv79SeCXnwYYMYvaCpT6xx0xjYy6yBtqTxNwZiqKJ+UlE8DOJqC4IjynuDYA4Ngm" +
            "4MwE/uVbwJFtwJNbxJ5qUBNJ/MrF9GANqhubce/T01ibmwFmZWA4PnYaXqQdcDOMBPqtJPXpVbH+9A3bMtODUO8fva9d88cC" +
            "dHhROU20efD62sD8deDydMoeSNr8cgDRAXlDIVtWd3VGyxVPvOyb1D9zlOH/9orFbY7C41OFU2ZxdkITmoCyclU787AzIpuk" +
            "awM5kNbQA0gnEFaCp5dtpnHhZGEmWz20HX0cY39xyim9uidCQnt7PJT1tGP901FpYjEndFL7s0r7tmc5fpyfu3kGR/mT+lb/" +
            "rtV9qBpeRSXYaSPkt1Pi1YmgKZnqml4S8baNmVEf2Ycjp21dczEJ0uZHWjlJRMQsy8xnYyiu8YnrJTSklcih55eex/0P3xYX" +
            "vhC91cNZ2NpDh6Z4/Cx7v5c/3YVcvhxtfdvR/3s/Cp7/1SmGbXuAQ8E9IlrZLblkDavBQ/NnDmFttYj5t69Z6s84XgXb5VpI" +
            "mXsaEN0hU0ZeXdZl4ExRBcrlO5rukbrS1o4XN4eRBkHndwjUYbtWD2wRGLrGMEpCF6WjQyo8khBD0ffw929s4KCcnUEAkz6w" +
            "RG7NU4rR6rZb16KzTRoTbAQ264JLAlPK1J/b8LENTlI620Jz1W8pJ7YfoE9em4RswyUJV62cZooRY4YS0LxHMklBLo8nG4Nx" +
            "SZUkkkqHgciYI2r+4bf+mX3jwjf8vvNv/xSmBu9i8ttnjbmU005s6/sSrvZmmSNpoPUFDw+SVG4GIGJDZsjVBLMMGL+2g8y/" +
            "boEnzeOGceK8Yz+wqVbQN3FlMJ1mzonus276JwlU8t0eSdiDeR8y9L86BXzteJhDv6p4Wob/IatjU2xdyPFVQHsTtUDl1i38" +
            "IK5/a9zhnCHLoHnBQzHQOMnLqZQw6cnDfbE9TAl1Hrq+9CTmpyY1RmRApEwLQylIJsHHdiPzKwGMqcG9mAYhGARvc3i0pVgB" +
            "/P0Ldgbh3qQ27qR+87T49vQOYbEQT22miyyQxEySFm6S9JXEANIIuq1eR17ijBQZ0ohFE8UlDRg/RCcOYtn6zNUHaZKlKR1p" +
            "/RI7iMS4Y86+SqD3V15E/+9RhFablKeXb4O09pp9Ln/bDoYpZwF+l7RN0ssBR+uBs3Og2xPIGiduYxD5vceawXwfE/fvGN5U" +
            "8i+p8Ny8mBRbK6mttjmjEykXQZY+9GTOCFweE4i1yzWSiDKZTXi0UQXad84AfeFtSOX35uTtiSpdyljJvT46Jb33KLCL7PE+" +
            "0PNLz/ErTYdff1+g7byClcyaHrb/zS9j8NpF5MrK0Ny5GaO3r6H3wKO49bvf56HoxTWT0flKR2a/9JggZOemgYtXga+dAN67" +
            "C1Q3AI800f6DaEO8do3Q0p4CTYxKD8vkfUZB4sijaYcHXHcIYmbgYyLkOmlbSqBrswyDf/KeNp8ddChgqJYx4NqFVgcxE+LK" +
            "6hS/01U2O6irohnt25BGRuUXgOoWT4Q5u7BhLyZEGvQCHzyG1VnAk50XDnHSxLO9y0ro1G/zfVLeJEgqK2nhahMqYJy5nwwH" +
            "10lLfVLpt8ep9xT2gJ8WNZmai0lZJBZZxwq5wJRT2OQVOcENSSzWFp0YmvXQeYtqlCjM+LLBhM0QE/qNVhEmZvM4yoV/HqlH" +
            "z54jwB7g/h+IA4i05nfJgGhseBo3/uTbQtW19gNtWFehvKEZq+8OxW3CiYzBJoTo6XP8lPHmZ57DwPffBsYM4Sl2wlcvVy/P" +
            "sJvTbzLbqY1L/UR4g4f6E7sx9wFdz0rqvXjXI19X99aB9axg+fqq6HNub7eMHUnIweamuD1uF5eBRJr7772LXV/8GtafO4bx" +
            "PycnDgBHqoFPzFhHwrtldmwIm/cewfLcDKpqG7Fp81bOuHkdxMQinjOSKa0CH40AE+NCiKaIDXW+hwO95EEmmXrMacMQAKh8" +
            "MnsRbAMKzdXwqmuxOjEcnolIBX2Nh1D/8h7M9V8HLvsbcH+1rBfeBkP4MT2n9L0RAn3TfCOWFX1cpdwlGgOUN3pYXWJYXQIW" +
            "F91lOqO5htfuhMhdJjeCdeEhFaKbRUV3SckuCXwj3NKlUZg4mLgl1WkS37QBStICzLxJDMiob8GyDxQwBxNcjEWVqZ6HeBxp" +
            "EeM39EfqsKMnGNC0qjvLOISEZf2kIhghjnUvd2N+dBj4xOdRf3c689v6Q+sXQufsHAbOvo2mVw6h4xdOYPiPP8BOT9zcxSES" +
            "rM5cnOJZRUMzircUFdEJrk0wMZmmaz7J32vA3Pgo2EYuck/UhBO8WghmGWZfoytlbcIGw+K7M0AHBdLT3QW9kDES4aF+LZbw" +
            "6hmgaydQQREUCnJd7/TgNTWAfTSDoWufIsdv7xNQXlOPVc514vWOv3oO9X+jA4vzM6huakZ1XSPG7t4S/lazsi8PVAAja1Io" +
            "APaSiaUXmOkFLt8Tm9KEQ92RGh56nK1TBACSaAwhwhX3apCsmKMiqTNwYnbhdYVuZ6wgu4zeZptQZqMtGgRL2rWOXcJxAlNQ" +
            "VyCTgKCH79BOdEeukR3wsCrpCHdqSmCc1j2Irx5h+H98MZrr1gpw8S2gtgnI+8BRecLd3sAshMXVoWGT0vPayrL91vGySK7O" +
            "tPo7V/q0ATUJ0Ea1JxejseHjuYeAJhBJouSWsmQzj9m0DBs+Ko9eiZ43WvHu//hlrK4s4M7vvGe4mbpmpWVxcbOe2SATX01r" +
            "II2izwPu+PIMQ1he5bPdWHlnyH0+xZOeYJp7pU37suNr4pY2H9UzV79ayqcTwiTip0a/jeNKpO063W+0CjzWC3w6KUqvozWd" +
            "A7oi9cm/ndQXOrM0cXe1LQuh8/FHHwHbu4Fd3SL2XfCe9jrIrHKoEriwYq9rfx741A+CzkXr0+cOskOwTyXB6dxits3bYJqk" +
            "uWEDLxlPHSxt9n1xinqUQsmQuZMmg7yLutgD/IM37XsQVjuJ7ulFAiftJ26vBL72ssevCDzKd8nTTCw2Iq5/HA13cnV1L2dW" +
            "ZmJb3Gnl6PXbtASdIMo0ja6FbhJYo7xqVz4bLknfLYSC/zQWNU0Yssdw5qDKMPsmCz4uhmKmIWJM93qvwyuUibsVOETNP6HT" +
            "oa0sqsMXdy6kEmYNL8aw6fAhHlLeDK208s5gAhGThlpOEE085IdCPFA+ss8TEbOCi6m7iELKeOpAnikBc7Ctp2S4eg/48S3g" +
            "t18HXj0PrK4CmzlzcMyvgDmo52Ju//kZETkgBH1Nyb6MrAH5jgi+fE5ejsQcjgfMQatf2dwvSJdVlZ+vG/kh5kAQORVsCDUb" +
            "YQ75yGEs8LKIOXiusbGtd2PdcdfZBIZJDCgoI4lmGvUr5kCMwgSzza0e2n/maFBOOe315bXyYvHHUjSIF/Yz/PdfNU+lihAb" +
            "xVlgOSiQBeMT3lpnW+S2d6aUZcvrAYfL0bX3CPySj5FvnrbEXk+SeFWaJC5vw8MFtrroY4ukyTKeUk6S0m1pXe9saVlCJFKz" +
            "TebzpHHRn1uIgLVNSf1rk9DNvraddpa/iVDTogjuG5Hpub3X50yIiNmWCDPMMt4mjnp7lD1Z+LNzdxtL+JVofTahJSMEUrJl" +
            "XIJNTHPvJd5GtVYvjAKHRDxuC+S4q3T78Ucw+uk54Ko+v0WZRAJarJ6MrvYx9PzS07j/ByfxL99jeGov8Eiza+zVc+330Rp+" +
            "vqGqp5N7JpVuzAriTQG3jHqifWB7zjK8NyGrYGoDuY9EIT20riw70YzN+46guLgAL5fD8I1P4Z9WFxDpder9K09Vx9pttiEK" +
            "VNfAG1MY7ZcHXMiMzO+rZyh2e/gv3rJrENY9iEayQfp0TQPD7U+AvoOC09f6ekwPhQgdWoqwkRQJKZo3jRBuf+x5/u32uQ/4" +
            "Jrk7rQt0BuBKm8YkXETUldYlZZj5vQ3W5cLRVUeat45Mzy+LT2IONu3CLMdsS7gSViRR0jXTUKhwtDcgiJ6DETHHJp7Koxaj" +
            "hwIYepFD4ek2rJ4cS5iHCVqKzWFANZH6jmz7kQ1Usz9t2pr+Tg4XddJuDz2PPIH7pz8ErslySAolm3LBQ9mxRqz3z4YXIHGv" +
            "IEu/WNqk1uqBVuDyFLC30bz7QkZW7s9hZOKc3Pg2tQufu5sy55ygsBVkxggZNv3/l//0x7hyH6gvAIcbZUw3m2DXmQs0F7KR" +
            "l9G7M0t8TiytDctLfeT88DcizGhpqSO4Ad7dVwIMoSc4dGsbZ9cYUBRI7VmTh47PPoLGjl7k8jlU1ggjaHlVNW4vvQVc9C0H" +
            "erXyll20VZtH5O2mOTSsnpwSdLsHYPKyoOptHpbOMrAEDcJKQdQ0Gb4BLCyK3yWjS2gDyf3R0+hKl/pne29+F7/nJkb4FXn+" +
            "pXnr++jHc/zW69zIR8/PjLrFhp14njPan7PUredHSr+kfcx/G2mT5d9I4Bhp6WNxn234T7XPbKOJT5ifIqrqF4+ygFDpfcGi" +
            "v9ds7fLAcrYxTphX3TmUP90BttPD1sOPoebF3mj+2rQ5Kz8LKf08S30ozqnG+8i2Duiv6nf5l4SvNYCVV3BiwSoLYZ4lmabT" +
            "4y6jjF/96xqzKG7qRlf9Q8RkT6NIjz2Vot5I+32wJR1P1TYf4nbwHLp+4QRwrMbefwpf+YxIzcCooCuPH3HRBvl9WNR1V0VE" +
            "qM3xeww4jncZ2KDEbT1pjsvvVTQHPDC6jS4Yaw9sPdp3Yl6oueRaLz5KN0oYDcZNpVVjbaa34MPnNlBeXc9PUK+uklMxUFxa" +
            "wsrCAuq7+wSedPpZHidgHR5Ykz62aq04aAnlo/6q9rgKELwrAaWbgD8L+C3AwlkGv0EPl5TRxLS3juEf/oKPTc3AtfPiWV0L" +
            "0NEJNEVMTMAffyw0jqOHxOE8lll5tklTiv3o3z3pz613Q1b13CGhxMB8b0ufZD6xaEy7PSn92TZDtXSZ8NHLTmqjMquEZCDJ" +
            "3BBvm/ndlicrXjbQ09pOOGug3+YWuS5Uw0PXfHIyT0QTEpJUz5eeQFV9IwrllZgcvIvRb551tMdsS5a+U+kM3Lo9dD19DEN/" +
            "qmJ72cbcyKO/2yTNRhEpV7aRNKaYucksMy4Rk+BI+9s9xLAP5uArSbUvx2P6RCE0MZ8d51HXcWx7nmsFzV/ag4aOTkwPD2Lm" +
            "++RFBYsJ0CxPiMMinpIdv7hVQZOII94/enpX++X7iHZsH9/yp5qxOjsN/1KJX+Hd58TPBh6wLYeeJ58AK/lYmB7HLN3tSZdx" +
            "y0OHPI02n8tONGLnsWe5ezl9GFtH/xvvizkeceVlWlgSQ+PRf8c26eMaEmO0Se1hZFoWS4MwA5TtEkzkP/sWspuYOnuBzmbg" +
            "o5PAkvTHpZtBK6uBOrpnnAE3+4Ez94HFdaBUA9y+B7T1ikk44gvvs2qzIxPBNRgUWMrcd7BNkqQFmBRye6PPtfuUOSo2+zLt" +
            "Bj4AY9pPG2+2yawvnCRTFcXjdxF7F9gIftLiSFo05uJOYmrKBmowODXZqX+b5MG5+5axpXRqM9mTKm5sc5nCF/gYOP0h9r/8" +
            "NYzcuILJH18L0WrJofdLT6JAYT3Wfdz+3bfs3Z5o2rO1V+A8+CfyzEAEd88xL41xiJz8lUAhte/b8bhRBAauAR1dwD6+ORBN" +
            "R2a+cR8YuQF072Yo8R3inNg3UVG4KZrxPEPlZ9qw8mMyxYl1R+7QvERpn5r87lVMQkRx/f0zwI4W4ESfuQbMdSjKEvGUspic" +
            "5V+6H4IITmT+J60r4zn3Rov3lz4OxZMTOHkDaOsDdhWEFkHvSoFZ1FZnmN9rruJ7CKT11TS3oH37Xizum8D9Pz4lk1Co83A8" +
            "Kqvrefj9xdkp5MvKUNPUDEab48y2qS7rNRmA/tu40yn2nkrxPfgVQN1RD/OfMOSagMNf34VP/pfrKDvgbczEVF+fRwNyqG0B" +
            "Nu8BWjcL9YTu/KDzJldvALfHhVl0UwWwvA5cHxV3btBwNMaYQxLElaQQTLut7aPeuSCNQGchZjoRkzdC0d8gSxLDstVp4i/T" +
            "Trhw9nkU4DgjsrXNFXspiXHYFrT5Pkv7zPFw159/Qoa+MHFfk+GOiQmrUMnk+CKh/OlmYI9aMLKtzDZ3FI4UoI3h03/2l5yw" +
            "8QNlCiYYxu/eQs4rg8831yzZnSDMJvE2JhEucy4n5LEVE2EOUeTKKoAThxRziBdy8S7wxkfA3t3EGPIop/vkW6qEprIkmY90" +
            "XRfMIY77hxeJeoUeZe/dBrqriDl46Pj6UaAl71ijynPQ5rSh3uu/tfVGXE1nvJHvel8kjL/xoalT0tJ+cBfYs5OYQw7tP0O3" +
            "2Alv8DxHiUyUm3ifBbhRqBgN31yujF/IRfOItIjiwgKKy0vY9KU9cdrBrTHtmBkfxvi9Gxi5dQV3z592BCO1rUsWdXm2ZTNB" +
            "eUrVAfPEqGgv+Q7wyffoAi4fixfcE91+kppcIX3gxC5VI8PbZ4HFaWBLNfj9s90+8NRO4OxFYHIFaK0EWrS1R3vxb10EvnIQ" +
            "ltDYekuyqHFmOpOguyQK9czUQMxyXKpuEi76RLXd7OWh+vkOLL01krGNNvfKEN82aXeM12Om1dvn8vhSv81ybNqEWY7tvbZg" +
            "1H27MfyiuK6/b7quanVHvIHohHCYpviu4qJJDNCcDz68Y9WobW7D/Os8Qln4xvdx4/Q7YGeUr72tPNvvLJCmdSSAK7xCpH9D" +
            "vPqC0kPp+K7PuPWI4Fiv+NCJ5O2fOYTZoVHMjo1yezjfTCbbSreH6mc7sHR/VIwBnV6uBNpfOcwPyR1//RzYdAmn7jMc7wGe" +
            "pLMmZAf3gabOXlR/pRG3Tr8DnLe4JatzDRaG2PByH2Zfk/e28xhBmkbJIcudHqpvXb/DtUNET1wHIN4/0ZtD/ng9+vY/yjUB" +
            "soBMzQINDSJNcWxamH8apKOAMb/XB+dR2utjfnQIzBcqQFlFBeo3tSH/MxUY/d55mUesI7pbo7K6Bo3tvZibGsH68Lxoc2yZ" +
            "edqYW+aCTi+CMDUGEF+j/Qy69GmMob7TwywXNHy8eQXoqCbe5zvvHU1zc+GdQFrD2ABtpAg34bZaYZNcWAIW6PToqoiW+52L" +
            "wEVSV6WmNBaJ6GmTnN11Jj9L0jr0tNIjIwBb/TYObZ/IUaYWJbD5E/UxxhJlDh5w0MZIXBKSWW8W5mDLZ7bNBUn5bX8d2lsQ" +
            "RsM1Ri6JW/mLm+CS0pPmkmRU8j0xh90nXkT37oMxvPnp4DPK114HiW+3xwlYzYtdgkA4+zHruNmED0teChxHnlOmKr5kKVtq" +
            "YlNGOSvzwEe35A91fqPWw8CFa5gYGMXqWfKAkvXwOoGly6PIt9SFy6baQ1NHD2oamrH7F19G+ZPNON7j4bVPJd68T4DL/+xb" +
            "uPU7bwHndXcWDU9FfwhX0hBpX4ny1nqYfeNuaDZcca012zjbNAobTYjP1+CumMoctvzSCfTtOoKyQjnW18lwn4NHZ/Dui/Re" +
            "bUV4sCzof23dTjBM3r+D9fU1lHwfDW3i6rCVpQUszVIYb5Ve4Ddw5RPOUJs7u5H35P6PaULSaYMZK8oGseCJsg8oMgIxF9qH" +
            "GQVmT4syb5WAHfXAPu5K6G1Qg2B0OZCPN04Be46LQyyHnqWYPUDOB1p9oGsX8NF54IlHgO1yAAbgY7AfYHVif4SCQIUusRrS" +
            "G9Ye9Pw2KdhFyG3vXXUmMQPzt6kKA+u0WZMS62Xb8edw+/xbWjkuomJbFK5BdOGpP3OVYRJ5FxHPioNZZ8gQw6sjKVSzlI7V" +
            "ouB3TwBVz7Rh+ca4kIxkXza/sot7sS2/PWbBz7ycScMniGXjoXvHwUBbYGSeuhzOw8U3RhI0Rga6NnFm5K68TtPWdreWlK6F" +
            "Opgsb5K8iyBGOCwgCQitUzoG2CXL3l3H8MMB4K/OA1866IvF3gOs3C2GDgDBRU/SXZfm8uJ8eJPhPONhNtq37cZ4/x1UUaiS" +
            "R5bweX9FrO2Yi7GjPVQ57SmpA1O0GUFnR8akuYEg4tvv0mZdEB1Dcf0vMaAcOl46gJG/VJfqaHNoXw67nn4BQ1cvoryiEi3b" +
            "d6KsrAwVTzdj/7tTQLXPXevZRwHXsgMJyacHgiBYdJFS5+79/Pv85Ljhnu/x8u41nOYmqeJJ2zWMSTTNALVnV7QJruqCMBmo" +
            "rxhaIbZJ3yThfeXuW2ewPiroheNhRrqHliZEcVpEBNxBARIPU63SVU4GCaN+PTXEw7ugtQq4MA8crE2SuLIwiyTCZTIDWz4T" +
            "XOYIswxmTxMcv9eI+FklOblMOUxIWMwlTVomQIOHthf2Yuz9K5FNLns78BMsqCyquS1dgpZF0iERHhaGAWIqdEOrirNPO6d0" +
            "hkDM4KW3dNu36Mvyyiquri+x0YQ5oC8MA58tHmpb25DPlyOfBxh3ArARH3kqfsYikVL0uNUk5mj2V5a+1+uwlG2al7TwzPyS" +
            "lUWzOt0UKcomZjG3BuzsAPLMQ+5QHrVNLZidH09EsfpoazgWPR6au7eguLyAmoYm3PvkQ9R2dGKBUenKUQPp84eYWOaIpC5t" +
            "wbVeonOUuqaDvu3IobKpCsN/cdEikDHuEDKzcxhlFZWoadwEL1+Oa//i+5pJzIvWp/e7Tg/IPERjU+Oh+fBOVNU18L0I8poj" +
            "N/3wxraQFsy/rp3oD8bWpB+SoarbRI+WY+3sajT9aprAJyr3fekSy5EJ0x37P+/B0L1+4J/HtWiWfqNcfPILxcdFRIGfPwy8" +
            "egWgg46/fES84WcqAJy7Anxhr94RZhkm0XIR7I1oGCa4GFJWIqnfZWDTJlyEnEJGqO+uBWDUNetj5NwVtJ7YjfGr16RnlKtt" +
            "SczGxrhsoLfB1k9p+aXXySZ5MCjSFTIvMY112YfqJrII04ziPfhn5zLgb5sjMu09xjcQCW5/8gGK8FFwlcWZQxZi7+r7JGHF" +
            "xM3EOaG/SesZ0cIrEHNV6Zh1pnEgQ8fXDom28pMMF9Yxzd0dDPx7PRRaCoL41IhN1AUS7WuA3Z97AYXKWsxPjODe2Y+4ijI3" +
            "PhyWERN4HP2ku3zG+sSM+2T2l/nXpV2Iv6NrQB8R1pukuSbZZ3y8/buXsJs0mxfW0dDVC1/d28GdGZhWLoXdsLVNu9r0HjBx" +
            "7zrKHqvkTKe5awtKa+uaYcExJ1Yc/ZGPnqAv0vjE0idB2E9xUVDMoavvX8PRr74E4PWNhvu2Sfg2YhEnisQE1j8FvnUJePIA" +
            "cOaKuDlyU6VS/VxhOqKNihLeaPOWpVQaxdfFEPTJaSPiWYinVr/VXmgyiSTJOwshl+XcKWH87lX0fOMI8o+V4967H2luvzbp" +
            "VS8nQUKN5Enqj7QyjLzj8hMJ+6DlpUVmDX5m7s/Y6kxg2uazZrmhuEi3WS5hYuAO5t8cMeaaKjNNaMgyNzTNKdiMtdX1AMJN" +
            "RDBw8bDwYeieqcA31ovRrn6GNVI1CBYZJgfviUW5SAdVSZMYw/A3lRQugwUG5aQJd7Z56OiHIO6Tax0hhf6IuUbMofyxanRu" +
            "389n1eD181g9a4tN4WF3E8NtstG/eR+Tb4oQ8HTr/NmrwNE9wOgSMNwPvLBHn8vmfRZ6v/tYP72MdSxjiA4aBDHITDD7ICdd" +
            "erWyqcBIaHwXDU6bn5Z5TJswC8D8BR8nV+zMIZFBkJmS7vcu0wonExOZ06L7CqriKBJf3iueDfoMEyvArkagvBJYoNOP2tZ4" +
            "1HocHejgZjHy7Q9C9oq/KuRXOmScnDFIIra2xRyfqFkZahxfUxL1MfBHn6D75w+DkQeC9aJ4W9tc71zSq16nrQwNiBCSRFu0" +
            "MRizz5IWugtsxNLEm7ljJJFHB/n11wE3PnoH66eU2CVv12uzEF5nXQlzhyZitfIgoU3fB2lf0nOtjbQPQhu8RP3Nenh0T31N" +
            "Ja0XS91B2Ao6F6A4gIehP73glPxJFxHhnNKYYUaNOfI7ab7EBUY9ffOLHejadwSX/vmrjjhp0d9bKxVNE2N49jpwZRY46PND" +
            "5tizi96L+FQ8NPDtnOhr2mgvhmdWhDVfK7/Fw/aXnsSt99/X5ppDiyA8R7OsaVcaex8PS7fezTxEUpimotNDkbyPJoElHmsr" +
            "l/0k9ecOMbzc5eNJsc8SANmxijPAUrDBlTLJK6IX45zqB5ragV1axES6RrBGTmDytOtV+Ws9VD5Sj87t3CaFO793KuGeXLNu" +
            "F6HUFfCsErNB7NpywkwSsRmaBNGVP0lStaXRA1To0ouBt+3qUtUpAZ5u6bv+sx2YOzUiTjnGwE4c4v1nmxNGnbYTn9Y+sOFp" +
            "k1Rdv5MgaRyS0rv6IKG9qXg/AETs1VpZsefioBd9hIHNoZ1YgwDq4BJ6zPYkaECRcpLAAx7xBDG1ekBqdbflULalgPX+tfBw" +
            "WbAGRDr6n2ggWTztYyiB+m7VJA0OF/Gc7DNiDBVx5ho59U1xmyjtoKSDqk110jFDdxSIrVFz7WsQXB9suRjLuKp3CIyHcSfm" +
            "QNeyjJU8tB+lvT2RfOBNhmLJx395O+7mWu46ST0+AIxRBwjaHCIgGV2cxjoIHgUwIxc64lxr4L7THGQoiA/7gdoGYL+8lJyY" +
            "Azm89VH+OQ9tfbtRt6mD30zFO8QMO0AhpWP1uzi00ZbgmStKqKPMkZLBbRX3dUm0atLYNI+08BM6cTI1FC2fuvhFB4uvvGuR" +
            "zg2T/5teb5a8NlxceaWuWkyXeJJ/m88s9akQA0GoAZMh2AQIs7w0PLIQd0sdPICd8YxUfRq7JFN5gzTNrdv84SUuMVOEUDQo" +
            "24+uAvu2M/TxvQtjfgaXyqRpNBaGoBwNbEKXHumW366XxjzlM7LyBEH+zLklf7flsP3LT/KnqzuXUF5ZjZnhQeS4dcPHxNkB" +
            "3mfkQbqJLA827zPCi+qhv+uSucgxaP3aLqzMz2B+dFx4WCmtXZ1BUXSoR7ZfD1OivL9o34gELmX5qNL6ZdbYsG+Qm+LqwisK" +
            "00P18njqFrpCc+ZuCX92EThxgDxZ1Tvzql51xwf9lHuCy8DoeyEtoSonbCexJVgZRG8P8IXP2oY9DDqlI0Egt08kU9UaEwyO" +
            "SeA8HO+Nl7RFxoyhAyqFcnHjcHl1NQ/MRdIA2VL1G+3cEnwGlYwmeL2KemibkLby4gyEtqF4xMnYdZ36wTF98doWuUv6cqVV" +
            "eMjTx5F8rsXlaAuPyeMijiaBcDE6/dYybXYpgSIWOVMHl0aQRYMw0ql6IvUl4BvJby5GE48kCTkNiABZtAir66xGOLg8Qlqi" +
            "ckN11avPq5zY8K0F6orAF+Q8WeOHxDy0f3UXRr91PYVRusZE1HV2FFhZZHhym4mDBGXOiDEHlc4xB+iUWiydHstI3Kl8k25B" +
            "pDhL/QwNL5DPElBdVY+K6hrMdg5gnbwoe7dgzLsHpoe9qZKCSlHuU0k3UHEuQxCwmdF+rJ5bEfOYTprT/atlEhXiuCrPGtD5" +
            "2F5M99zHCt12x9e5bBfdvaKTA7UmbAaMecDbDjAyoRBQDLcmFUcqh1O3fBzfrvXhXaEyff0g8Dtn6b5uH8e2WsYAIT4i1KDH" +
            "90Q6d3gYfNfn86rH99EkVMyNnIMACkE92mKRi52YpGmxojoWzPs2AoSTpE8TPNTKTaCbP3ofh372K7j0P7yKSfg4eQ14ZXe8" +
            "HLsFLYOpYVWFukwiiibOZjpxMtMpEetucWQ/vmMGW5FH99Wl9jzuTFKd+nPGm+A2IWQlYknttUl+DnNJnZElcpGLTBcxadg0" +
            "ozQc9e+2cl2QpHkYZSXWmZQ+CbKMiUE4ieAEHnOudNF3Yj4w1G2rR0N7D5o6N2N5YQ53zn4IyJAKo38lQiwouDoL7Gkwiba7" +
            "jy7NAS1twOQYcJMOXKnFx11D4TIzJAgXrrWl/SU3b9rwHWKC+BLQae9yYH1tFRXV9RimgErqfI0HjE6SW5HhTac0sBktDj3t" +
            "pxG6tI+0AqzSCV+l0U2QNUNDk1yhiXiT6aIBGH7rSrhmC/I9uUVTP1CaLOdYStL9morZ4XHPO44LmY4prNcykL8NzgRE74UW" +
            "i1/jnqIe8kfyKJ0VUXcHuDVGCYV+yLApxMYgMCKHId8JlO57KEuQq617EF85wPB/+Wz88hHayFmZoj2ItBZnMR/oaV2Su/g+" +
            "DYZzd4DnOZdM0xSiQGNbn2lhmrgnm2Wi6V2SnRjE2hdb0NKzHStL8xj5ztXoVYG0p0GRFWmSRS6rd+Fpw8vswzR80+qx1UuQ" +
            "0470W/qM5i15YkTObMQ1R3fdLsk96erIpL7Xy9XvTDDTmfVpkmsquBhFgpYTw92SNicZbuAxZCs3rs1enQb28OvZoiKTt98D" +
            "IxVcXqW6KPf+7PhFNapra8DugtgvLEq6Sooxye2TgY2fNtDzwhNJ2dqDuxNcG8UpTKlaEt1ZLZglEc+IVULLQ4RbHdyrlVpk" +
            "zCVUzoOmHNfKvHI6TQ+UqEy60McRqgKUjbQP0swIKoHyugqs0nWoC5q1gEzqxISovGrZYWreVktmVKHMUi7vIlleEMXYdQ83" +
            "4987j++GX1zB6PfkiXQC0ri0vVI6rDcy7GnRLVS9DMV6D//1J551DyIh1IZOdNJU9I2AbiIRyJvqawhiMtN8F8zBfO85PrlA" +
            "miLmMB2pR6mUeggHEycbXi4wF7n8K/dVCDbvPYLF2Uls6t6CquP1Rr1SbXV6v7gWUzRN/UttVoIRwysf29VP6H9L35B0ZPal" +
            "PlwBczCZgtYOmvg2iZqYjwuvkraAOP6yfvLeUbcRxXxYNcnOOrYu7eWva66buKQJSVq/KPNSgH+2Mm9EhDdFXHywT0vAcHiY" +
            "syZVYBKfP/wUGBvkB8oxsiCuJiWrkjDqaMwBkpCT/ZzGZ6uH9oM7BVGPBOrT25uToTrkh8afx3eSsMdD5cFq8ZzGncJw0SHV" +
            "vFpfRhv0E8tEoK3nBURdlds9lNcJyb10kdzJHWNTkloAv7FQnu+hz0Ufq++viH2lWtlm5ZSjtA9iDrXyQ4yFMzuh9cTXoIa3" +
            "alfguan1ETE2YiAyDlbHI1swfOZayBwUA4NuzlFjQIKH0IDq9+dQt0+WmTAVEk5S64WLX0y6uMeubo2kNfNtRMJKeuaQfCMb" +
            "f9FFf19ufJOAHht+LmnYmF9Wydat5vPf82GZl//FD7Hlb4hAYGFqWfeYlMojMe9tI+a5f+/3UNvcjlket8DAw0zPXVtcTEBv" +
            "k4PZ1OREf0di1uvpXfVrEEx8PY1UqyNts+Q3bfZkp1WbhDENQ5YT3ExnOgyYeJvt3wgkScMJjCEy7hroAkPMVOcus70C+M41" +
            "4Mu7DamzLy/7yYz2a2Po4e9f5ARewLvjjF9mE22bhvyUD2zKCUl+mGFk9LqYK0QM6crNq/JqWF0jnNWYOx//UELv3L6bXzW8" +
            "fPmGqKZBatpV5qlsmSdyPkNviybBrwhivlpkKKnDzDaIaaxMK1PrszFJlOvkZrZOi8g9X+EU3EZIG/FM7H26DrwVJK5Be7R2" +
            "EAOqkPsx1cDwuXvaXSnaQcMaae6S/cRHnQ6g3QO69nvIF4CpwfRpbjUxvXKA4e+/EJ+ApKaQiWmR0yGXSSOJEWTNs1FIn+ih" +
            "z3YI6tL1ztSybZqUiyDY2ieetbzSh5aerbj6o3eBG2qhZoiXaC1Xqqq9Hmq661BV14iJ79zNyGCyMG0bg3Bpfl6Gcl0ampeS" +
            "xoWjLZ2rfhOyaIZpeGTF6SeBpPaH32ejCqvloJyHR/7uF7E0P4OBSx9j+VTR0mcKcnEGXK02ZhkKx/NYU3dUB/sNYT0RfElT" +
            "VBHCYy6dWjoyRRFzIEZIGqG6C8N2AVSQV2eA8nyC2itwQPkRD83tnZgaHcYqLX79sikjXhLv0DL5c02VbWx0q6bXyjbMSQa2" +
            "YGH6BVmeMlnRGR1iGroJkWsg2nfdfZbyq740QW+7+l6Q38kRZ5qBrQCjE8BcC51H8zBP+zj8uj7Bc/6b6xs2MSVJg1kWS9Z3" +
            "SYTVTGdRxRPLDdNEmYMXMIZOa3kpOJCa5mR2rnzAre/cxb2Lp7Dr+Sfk89xPYJoQF7YTcyCms6l7s6N/tO/K1a4jh7LHcyLS" +
            "Y2K9en6TUSaZS8zfrnaaOEqmZ8XDzJOER8K4VDlwbFEmx6xzwcTJNh/l30ibkvrOUm7iFBF1mqXlY85cItro8twsiotCHTl1" +
            "zxjjihzQkecbo1VPVwKHpUmEvIqIORAB3J7jEU+Fm6Y4CJbYXyTZkodgEDrV0Q/KBEP7V8RIiAjWaVK3ui1Q4Ur15nKofqZG" +
            "eBkV0pkDpVv9hGHkgyGskleTznhsbvuzcoOFPlRfr9ZO8khqlq6shFeFfLYm72ig52R7o0+DbJ9iDmTx4B/ZJn18g4B7kmrr" +
            "TMvc7Cac+Oaq3ERX/aMYGUUDnpSWEsl0ao+LNPNXZdkNQBkRRkr74Cam+PMw5nxWyMoETEnMpf4/SFm2MszJbZOajXbQ7UtL" +
            "mqubLY0VP4+P59LJZVw7+Z4lrUtSdLVD/F14ew4NXy+ime6ydZl5SCUliUUdJJoE6h/twuRHQymx9l342Uwxtja4pPoETYOf" +
            "zDbrt4FrPqTMy0hMMi1Pt/xJiyoGWQUis/9lvpQovxw6PHidAI+BR4uapMUKSeEj0m28bnFLW7T+vLwjOS9Xa/+fX8HdVYb5" +
            "SWB6RdwYSc+HVPRXMh3VAM0727FK10fmivySsKBMChu+1cPSSe3eDD6fXH0j06xZzn7oefjJZLEv5dUr2kJUTEvDN+sZN1NV" +
            "dlZgZbTITTSLt5ZF/6R5CuWkyYXGltCvk5FyI27ZDtpSpXnn6ecXJuTvCkn470hCrMZR7acVZWiZBcVEZP7wwr6wH9KYnJ4+" +
            "OBQo64loaLI9xNTkJVP0//xJhhX93MsEdV1SLNcEN9fQxqw/l+5gwQ1eGjLOgtIgbKTtm51JxNVsu7knqT6DsEXcJG35RT1l" +
            "lR7WSTKKNS2NECYxSZc24iJ60d8UDrtErn4nylF8f1UrTn7h0Sa18lcZJr+rDJAmEU8aS5MpuPpclWd4wjXLo62JjF6f+Gnz" +
            "Smfotv615I+NszAnlBWAdX4XEUs5aZzElBzjFRxEMt7rdu5hBsbtx1pZkfMzpgYX7pvq5qUg3X6P32nAZqRgsEQH5bTdZV60" +
            "Fx6kooImGKaWRkXxikkpoO83s2k/Nc9WY/H8knAllW2LO6PLtioTy4wiVLL9et2qnJsMK6WiIMrc1dUYKxVt1YyOSmWR9xPk" +
            "8wrpQjqj97FlDXryMyvR53ho6cj8RgxjxqBeRf1QnHqve2bJMDWq7Y10FWgj5odmAX7Fd0of2+SQSBYPbV/r4weM5+/Jyrmv" +
            "wkaF7CQGEdy1HBZIbq78lRbTI4qlWUgSIiZxEs+iKTfeoHRirKdTk8yUXM36wkVKoY9HT+m2fn1yuUaOoWx/jl8LuTJBh3uy" +
            "3NCWRnxCfCe/N4KK6lv85Hn/6YtGXH09b1JAPJMJS8qzJKVAq2ZljA+pubN6fCYjH7U9Bo60Thz1eaO0H+UX7mK0GgQu+rJM" +
            "Uv+ngbVTRvtqtQNVERxM3GzPDNwDxqi/l8JWLI+r/VGB6ZYvrpQmwTOM/yO9acjsMgUwYgwR/B1aIBHcSjnW1juvbVq2DXeR" +
            "buHTZdHeghyayAl6s/4sgqZG9OnsQ63mmaaC4ZEEr0LF9Kk7P+S77R6quiuw/O5K6ABAB71SBUJPpJmWZZtOJGSmCm6/k20h" +
            "uSvY/LYIDNxP2DjdPQXMXZ+V5rW0/qBDaGruuM7/MIx+dE+sRVke0WxGpq2CZGCathI7BqaB3cIZHMnXF6b+0RuQJK2nSYGu" +
            "zrAtNp042D5ZytbTh2lmUxZo0Je9Hsbu3rP0hUm44n2wfp+hnK5r7XO1NStDNfMT+Bj65jX0U8RNW9BKRURzcrE8UyXU7EpN" +
            "fY7hJO/e5ipxFsZMHh0mQTLxdrXPNj5pWoqErTls/sV9CX1jgBl3KLjX18BpQU+b1Acp9fV46Hh+m4OoJpXtmt8+984j5mDt" +
            "J9JWyP+dM2q9LrNuydAbZDsjoawRMo4YhUhgDvv009NAw/Py/vAKWz/pgkEaHTG0MNqb4KGA5Iax6aJqXm3Qz7B8rSiIKY03" +
            "bZ5HxtbWLg1mDCJM5VCbYl5IljVklkuMpmjp135Dy6eyaHyC/RuNEbXINtQmmNfuK4ap9W07UFGteR176f1uZxDOhUEn+8SJ" +
            "vEBlCb7rv/Vn6hSf7bue3pVXz2fLa0vrem7/NMTyRfNPqe93fbAzfuw9Dx1PH9rIi7VV/p4G5t71Mfs25dfKoP0BJOFn9ouZ" +
            "XsNlWX+u6tHDXQjCsWX/o0J3pAM9XMpIq9v1Xm+rawzMPtHz6W2L92vkU2fBaWceDTubMPAnl8N0NKPp6lInbsbcJdfPWL3M" +
            "3n+p/WG210PH8a3o6NutPU/qs7S+9PFjuriM9wcd9NLL0+JxVWkbodZxEvm6nt0p7NTrRigI9VGx+BPXdVh21/6dwfPWr/Ty" +
            "w6HcVXBHSruJSZGwQiEt9Oc6PnwvxvgQY7jEwjLUc36ftva7KM1cSnofMNvDjPHR8GVa3S3y40sX0k6VLhdtH+GyS1Jh/k7m" +
            "q9APzKXMH2IOleaalnWMyDbMpa1LrY3UjtvAyqDUFAmXJiUgupmE48KgcOPivi8cHOgSMFbI8YMFjMdLcZlAXOBSU7Pm19OY" +
            "pqekumxl28xA7vxbtFu6bDAn3ZqbujxMXzPLSDKZqIW5kb4z26H3ha1fWFQOGAOu/O57xt3GZjkuk5mrjiQcVR4jLw+mJ7/z" +
            "mDO6MOLF8wTBBMP6+p44hLvfPR+Nu7QuF6AibiTBknTmus0s5sGRwUxlbZv99/A3b2OYVuZuD/kGoPRR2txzmdYYF1iPdUuT" +
            "EjGAFqDtyc0Yo+suyQyyTZ68pb6kIa80YmPp0CRCVDB9A1kRZb0v113jHsVthtbIn6rJ72HswgDGbnNXqTC2QwykF4+Sgs3N" +
            "bD+8t8CaXz8rQhqxEnaswLQ8yeudE06175CXc7VcWlbUGR5iOJtk2kDblK69tRaXVDoQSAKZCr+hm5iCszASD5q/ZB4Mgk1a" +
            "2hHBO8kSI34Hq0mdlZROEK07PMxQn2iHsDfkxfTOD4CeduCZo4C/xuDLPaxUlTCGZJq90WQaG1mkJpj5XYRT/5tELM100Tpo" +
            "TtCiLa3pHgF6Oa76s7bRyBtMPJMxuMwxRl9EmIOJk8kcXOXov9OYgyWdTniIoHXmwoMpsbZZ+r7Nw53XL9i9jtTG45qHqoYC" +
            "2r+6G3f//QUHbvpcMU/76t8Zdw/mEhc/w2LiahtLbY5dywUup0nMl0kHF/PMzloggArvJE6gCh4n8i3HOlFcWMDcxHxINH0L" +
            "we2TRIoqmAdGzt4LDxnGDuzZ1oAdrq0AU1PAiS4tz21diHT1kbSnE+ej5CqaaRDg0jTjWOZ5zsjrhKhZOdoubX5WAd5WD2xC" +
            "Eum8TFarewrJv5NSS9M31BUDMSkraTXKvGMe9gz6XeZdMvFyADGnSHgez85wVMtJWJjXumINyJcBNTw0ix1Sz0Fs3Qw8cTRS" +
            "jYX4JjVkIxpCmk0szV7pYkBp9mwbcd0I3iJdvqDnSWKG4h1dy8ovX0ttq4v4ZH2ehItZn54mg23YWre24IioqmP+SePGYwSl" +
            "4at9HwvjCkXrjpa7fH8NNQ3NFvxsAkLyXGt8pAk9R3fG6zuY1kcWQpSQMmQOog4yv9O+Q6zeHDB5cgTjp4Yx98EccF4jTkSM" +
            "dCCTBmkZ/JCrlE4jYaodOAfluHHeXUnMIQccVNRUzyM/1nAhEg9Pq79WapTqYJkaukj7NbyCkBVpNEkXAsw5GY6zt9dDXVOd" +
            "4MhLUpMgBlCQJiVO+LV5EUQVUCDtPEoAUngq889C0kXPWeid9ixCvbV0Cp+cOrMlgTRJGiLybKgDvEpgaRaYIHOVA5yokhWJ" +
            "ovscp/0/booVew98N5x7OJmNsknJLkhbKFnfw7iLIQtkMY/I+CRBkaY0bZeqp4aYo2/U72g9O4QLSiylMqXbmZStnTlUPp0D" +
            "W2cofsg22I82Tc6EjTDkaJ01uyqw2FoEIgK8C6+kduoDkiyFR8q9w3DpD95xTA/bOJp/wzqmX53ENHfPYbi5BuxQRO+CaXI1" +
            "NVE6BEanbJO0MR2faL9SNXTKhenvCh4q2iFuBSPCr27OIg1n3LJ5OeNqWwJ4js1rekEml1WGO2D8a99xT/ME04hVj4fqrQUs" +
            "Da2JsxS2tpMZUEaa5RLuvPxep7WDryttvBdVtGSjDTyJjJxM5UyqLDKd59pgFhvYWw4cwPTgPTAKh6FAmmNyDYAvrWYR11q+" +
            "n2eEdFFfg+tmVGRnGUuK3un3MKi7sK0gX/A+17phImUdUagPCXw7rQ6oqAGWZyS+dUCO3Lutji0pGgQxhzikSfcmkjaNw5Ym" +
            "a102CVd/ZyN2SWqyDBAW4JJUpsa5adPNgifjIQhM1dqyaDQI50j43h66PKGvujxUVtejyK8szKL52CQTG87aswYXUzD7IhqY" +
            "bfGdlXDxR9LZykiaX6ZQkHO0RWca8nss+qxed5ImBhTo9GmPkjbpmY837gPv396AtMdjCKkyRRl2XMx2esChHEDSudHGyuM5" +
            "bNq8VRANndhF6knqdwmRkK5Gvk1qE9PQCrWgjT+6Apy8qdyE/Vi9hW66uCdnVO8B7XIMqf55Bw7zKhy/ow1k1ydPJk+G6KiT" +
            "2hwxHM4sLUTXxRxoH/mxdjS1q1vNjHbfYvDPWvqVDsGRlqG7zBIupPmQxsGbbuQpaQtfMfWIXKPPE412qDwRbUxmatHzWvqS" +
            "Pqr8JRmPbQZYnGbI2wPrccgQzdWUiMxOMid22sR0vbOVmUViTSvL9lx7H0Q/NNrovCPCdmAoTmjiJMDeL+UuphaYq1yglTXE" +
            "MPP6jHT3SyHGERqWNI5GGr7Ja5NAtQIj5aaBhTETsQhux5LufBbG2v5Tvej7lYPR57GIrXr71CILGct8bJQs2giPNArUbK+I" +
            "eHv0dQGfDe4lkXgqXCOXrxAh9ND8dJtl3Zh9FAoWSqArHM9h5/FjoUSt9RXtdw2dvRO1USYFfzPyB30V2M8t62UCyO1UbdP7" +
            "KIr7N3YYbaa5S7etdXlYu82EkFDQw2UAFRSdmQi52pCN4KD1lXnIUCOOtY9Wo3PfFmGLX5GfUbkPQOEkzD0D1UcRs5WspxVo" +
            "7duOoRufYvZHyrHdSxfOYn0n61DBsIK9APkxTWYqNlOsGLtAKRqup5EQHPJMRnON+rNOrjPinz5A0VM2FKzvS3sZfusZP7aI" +
            "iWYuTwILscu1N0IQzPRiAMK7pdMIY1ZIYkoPWo6tLEV8os9nZHtcUX1DsF15Kg3RRMUikWrNOpPAJPxSqqpWh7aSmLEhgTvL" +
            "tOXXCfJGcdQ2KBuA+mPVmBtc1i58F7D9/3QY9y5cwPqZklGPSxPIofOnezE5PMDj8WDFxhj0vzJfLYVW1vYYKCx0wFS0emXQ" +
            "M/HKxUTdQgsJoEMUSrtWKBvDQQBJkW9UCoh5VRa5publ6WBljySCTJIgxT6iyedr/vrKq8sWUj7Y0NTw5WOgj6N6Hx/rM2NA" +
            "TTWwVxGtfE64e5c0d1MCFRWWLsZxRks115I5tsZ36gcidjQ/VFlNsq3EZakdNH4lI/9WeYp6Wg/U5wm8J/WowjqkCa0G3tzd" +
            "2mLqK6Q9U/0j+1DRgoDmIr6RH8Mlji+/y2e3h7lpoKoOmB5imKcY7lWAXwD+8Rkv+53UIxPA1SlgrchwoFOeTtdQYE4iEe/Y" +
            "eGRJe6MqghOh+mIy63gQQr8Bzh+pP2lhuAhn+L5BMdXM+Bl1jGsTlwgUXUcYuTfaXEhmbZb+4lKWjcHpebS/fBLa6tmIYJCh" +
            "rdAWKtUnXQdn316yhPBmuPnWBeNwVJomxNC5az8a2jtx5cyp7It/QVK3i0rEMzfSTVtvtD9o/VY7GQP99XFtCRibBdqIIcqU" +
            "rTJGUj1d1jMGTBSBlzbLUkgZmdXCNPBgcNKMpdArl6epG6WQQWNOREhJr4qBxPpA4iq9wNTtdNE2R+fVo9IWHazdkiTY6gyG" +
            "gpy4I5o1SJwCom32ifE38NQx1mOD7AeSnKskI1Cn3xXhtF625IlDZHmZnvpqSpbNvdNsNMjTwndIgkaui/yuaoewpMvXWh9E" +
            "mYMs33bYrSTHy3UXSCwMS5LAJzyYKA7X1DBDZS2ZlgTToI3rUoIdycogOlqA3c3AyXPAmUXBcajQbZWmpJQu7eczNcAm7adJ" +
            "7mmQJAW4ymIbKMPGsOxMTBCKBFDmJO6TL79z5iAOWOx98nGMtF/B9Dtz0v5g4GBxa4uPD0theJZ3Cw/CnLNoDjo+8rvu6rfg" +
            "aZEtTRwocJzT2T0gvGZdH//jV4VNXQ9WFuBhahGi+tA061vHclwSc5dgNCHvI7G3nVFoflRVAlsqZYgkWQ1pCp2yzKPKvKNQ" +
            "GNHrk4HwKGzEqHa4jcl2jkok+GE3R0yphK6k5DHrA7/NrMRdW8l7KShrTy6m6UU0k6IHRhpF6t0Wxhr1JP7mCf2SEQpFnZeI" +
            "uIwmzMNaaUIkJkNMiN9Lb8PFE18V0+nQ+rLaCLCnGJRqo/LEit1HnUZ/kkLTJOVT3y35lgB/gPGjRwurFPIbWCU8VwA/EgYm" +
            "0zkIqiCHpx+JSj9M2xXfGMH4SSGZO0bTudLYGE6SeSRZcopLO67nLJk5xCSIaB3NTzbi9rmPUPxA4WZINvSxLnKbWUM9NxZh" +
            "cBjJ1ncZJ3OkfIcqnYnRSKlxxFJmrG6LhrlJ+qfHxsznHi3i7gTT8y3elgrDxEOEktDSx7I1uRXc+hFlNKIOFdaH8sfnhrTh" +
            "02loIj7dHvLtQInMScGZD9luVXCLh4oGYHUFYAOGdqFOEhvts/+Oru/olaQyRRvA1nLYLcNpBMCZg+zzzR68FoCROU8BSduR" +
            "+eBYYzR+65q2POGYl2bkU/NsgdlHFB+MpP5qLfoq7VG02Ew1RhkcxLkTvh9DZ1Bi0VMtOCR4B7khI10N1qyez2WOI1BCqJgT" +
            "uTqgvBZYjQQr3ICbaxRND2wNmB/2MHqPcZX3wQh/mvawkXwmmBKyhRhRa9VJQlUm+QmrcMLWOlm42OZoV8dGdLU86ro9dTl6" +
            "IlH00LLfw8SnLq8WBnxnRsb6cLx33krmnmjlfYC/Dqwr5/oxnZ7amGCWsl2TTD5XdnraHItsjIYqvNcEFCogLnOJXZ3pqiPK" +
            "CMurgNXRuKYozCUCKp71cP+dNPU8Ph+skcCt+UReWr80NMkCgo4DbRx6qNwGNLV6WCgA/gKQLwL5cg/5zdKWvABU1QMzIwzr" +
            "w0B1BeD3M6xMS8Kp++BHTBGyLhWcb1a5c4fEnebGKtmmud3BMtdf89HzbA73L5ptCNPVbQKa1oB+Iu5KqJwB6nd4mJ9iYNMy" +
            "OXn7rBhaHTEGfqBLhgCR5Zb1qPmq4cNDqij85Ryr0s4mVEhBhcqpBxrWgNn3Gcq6tLKCuWIDprmfyjlaYiLY3VTKPg7HT/M9" +
            "KJN9vqg3wS3MlvV42LTZwyidvFfrJQcUeoC1CbU/6dD8I3iIsxvkAs8P8zKG3DqwPBjmTWJJDg1Cl1RJYgDmhoB7t33cLQKL" +
            "QZz4eMOiZdjtwWHarFpIEqQQdp2IEt583pPUrr03QxGQpOBpMVho0i8Am7o9rkXdu6u7n4VQWQF0NIj5em/WQy4v/I/tniLi" +
            "992zsgKaiGVAtRwRCpu0Srbj4Xj7KOR4V5uHfjo9ShE9q4A5HqY/Q3/mPRzr9HDzNMOMLlkSo4zRgyTCLHE268xrC2Ndy6+8" +
            "k6h99Jgvfmqzhz17PYzcZpghwpSzhcCI1lFZ5XEL3GpREoDA08VD5ywwzAmOlocbv6VROO/h7us+/7tjRw6NncCZt33ep/UV" +
            "wJSSyvj+t+yTgoenXqzG/PQyLnxMh7u00O9BNM84rtb9N3L5jETsDIlzfY2HsjJgfR5o6qnA8lIRs2MMhUo67eqhohLcJTFf" +
            "DawsA0NDwNKyjEcUlC9jpRGQPUG/XpZvfDKUeR62tHu4P8ewShGaKR2de2oBxgeAxWU5TtR+3rdRfKn/RktAe9A4RaDlOrvI" +
            "0HoXKFHk0DJgfhVoqwGuXWYoqjWVFx7mZeXA2LLQont7PUxPMMwvA601QNchD+N3gaFxGQqCd5VgBDRWNPXm5TyqKQdqa4GK" +
            "Sg9DiwzrhLeqizFUFD3cpbMN1B/BeQzTwUAH2aZyoIxuSqV+4gfnXLRMy2Oja/xeCEsdZZrAJBnR7gM57PvMI7j0zjncVYIm" +
            "9S0t037ADxiqAfrY03eJSjkNYxEoywv+WcnjdmajvfbtCenlSYe+/FVg9j5w7xbTmIPWOURMqZExSDJX6J2YJJGnQZK0q9Wl" +
            "ThPSwtaIfrT+MH1zNVDHF0iU+JN029onruyLTYScx4ufmQOmSYrLAZV5chMDcjRCKW2o4VcMMr7gG+SduSY01eRw4GAOTeXA" +
            "wiRDGQX6ywFFrhUZ6fUTlHr/lHxOEGcWDZ91mlhpDKbcQyvdN0yTNQbymT4d9HR8Qml9T+8KHhorgMl+Qaj4+zU9gqx9bjQ3" +
            "KoGNuESYZue+HDrp7mMDr93cg0a1X6Q9dMzDZ37+i9j/mZfRs9lDm9ReeFGKqDLGifZTn2tG69bdeOSllzkh4lDwUMVdFIXU" +
            "b4MYcyAPn7I4Y1C4zS0yTM0DExPA4NUi+u8xTC4BVbUex219TeBI8XTmp4C6OvIgMk7L+iHzcnmH0MEoTvAIA+o/mWdsggmC" +
            "oLZxAocteQ6D/xW4h8xBG3S65IxfZ8ewtgK0tgCzy9KZh7bPiJmuy/rWGKZmGcamgKYq0Y6KWmCZ6lxlGJ9muHaOIiBLQleS" +
            "+WQI8YV1YF7ut1QWgHLZrwMjkjnQ3ODEn4Vn3fQNX6Jb9DGdCIOxCfuUhACmtDLrWEfnqaAPDjDXDvWH8ngq83g/LM4y/ODf" +
            "fYIblzVCJdsfZQ4G4oHgIU98y4B9fK9BygvLnoclX/GkdNrrjMXEVY81D3ODHvrvMNxdJeZgUWMSrquzV67b6B15bZJp5jLN" +
            "Z3rHKWlB49xKPdXKniICb9RPhH78HsP4PZF1NaJKeqgsF8WSB+UKvSwxLOU8sdY4ETPbrH3PAYtaWO1hOhlrAVoQ4/3AuLpW" +
            "MO9xSWuS+10LXMoLQFsrUFoDhsk7g5+89NC72cPkBMMizY4sDDkYgzBtWz2weZ+HcTqtXZKzx9xnoCgDzLEplxeX2FDwx7oK" +
            "YHEVmJFxeCoqtI35FBgiFZv6y5gnN25GGYYan2uX4sEl71xk2HbkPhanp3FfhRogRk5tWhVEqboK6NntYVPXTgxcPY184VMe" +
            "u4bDOsMqTYS8IFAkTK2WiOBq7Q07kxMNYi6zRdUtgtG2bxICxWrgeuthZR3oJ2mXgmTWgGsQ5HVS4oyLIhmI703tHnr2ALfP" +
            "MSGUcFFR1N23xcNQP4ua1wPmDAwPMqyux4kidSudrvUDU2wYIZfGrbgSX7edFBrDF2ZxUpCaih5aejzu2DI9zDC7LtZPFJRg" +
            "QvPA48LUguFeurLMMKIuPZRA/TFNF8lxzywxn1ZKAsVpamww+YDmKmBKapfEXH05dk31HkolkY8TT9eZp3zYn0KA8uDRWBMd" +
            "lGMRa0/Bw8HHPdz6WMzt5lqp6awKHFuagKkZj9etLBqVZQIXqmvR97A4qNEsmmPrLrMaYmlyFR7qy4AZapduNaG1kgPWuSIt" +
            "NI2qnO5EtNFgfcQcBsClGKE5uDZj9Q6ybQ4mqUIWxKQqJbI7BiGObQJeRjmqY2lSEVGnCR8ZBFOSFsSCVPoKWuDLwKq6rlCa" +
            "omhR0BhQsFtO51VZtGjKgfnIZpWdiVUWiDC4mJ94vl5koNsW9cVFanyuCM6gymX9xQVBVEjTWMyRykqEZCve+w+3NTqpbKu6" +
            "JifrU+qpoXbTIvuYNsvVotGJiIKI+6LRTiKgtDDLxbbPEk1i2ebicpomqP1W9QfjJPOYpiUbDhqzffP3z2OO33ss54eSzqjP" +
            "9tPpdGCsn+E2PuIS+8rSKuaICMksfIGVielDykExODsQhdZmD117PKwtAc1LwNwEUFwF6huBpQVtPpGppFJohLSQSYttJrMm" +
            "CdxFhuV5YGqC8aGjsc7nxU149WSr8YHuXR4ukQmMzEB3GdaJAUQETvGjjubkmlAIOjo8DJM9Xc7RlWDvTAOpeReNOXlvDdhS" +
            "AGoaPLT3FlC1vsb7gxgP/Z0aFoSPCNbm3R6868DsGrDCzXLKmcHDmi/6cHhCPpYqKOHHjzaWEbnw4JUBS0WgtREYnRZpODFU" +
            "JkaVl8wqBaCC7CnqNjdJcsg8ScxhTuWxrjnAq/CEVdLoPwo1xPFRZuvIlbBi3cwMAw2tHnLTjGt8/powiS0WpXCjk8h1yRyU" +
            "1kp/uVKp4j6F/aGIPbVtldQBWj95YRqdI/pMGkYJmFHanzSnq4PstWXg87e6Gqit9zA7JnwhNr5JTXccDDD0U7DHgDkYRFPa" +
            "M2UO4284iRDx5JBIOO/YRUqZWhmJzCKNmQkcSr6mvlrLVkxFMIZ6Ir66X76cQGQbpiLITLKmvSe1N5iIEq9CpYc1/RYqaWYh" +
            "mhyPyizNBNwIGqqMQV6fYZImXIWHxkrJvFYYFooeaklSlRr91UsMVy7expqSUmnGcEnZt/dlpE9CWFuT4YKdY6XNE5sWqH4X" +
            "yQnE2AeKVK73WRbQxrw8h/37PHx6LskVVmggs4G0b9TJtyg8fsHT2jJw+RLjzL+BzDsKZ89D11YP7dsbcPnkDF940e2+0Ali" +
            "E8Ujqq3gIvbSQhHTY6LrZqeFqYTn8jzkK2gTEdx0RRe7rK4Ck4MMdS1ikS9JYkLMiOzt5KpIeekvmdaqGrzgIkiah0RIcgUP" +
            "RTInEVOlxGUe6hqA9RnBZEbo5rnixvt5pER7B0BvAahroU30SsyOrKGyXggS+QIwPcb4EPd0i4ihdc3AiHKMkFItCVBNTcDk" +
            "lNa3eQ/ltKcgqRNXjvPA3BqwsMiwQHsWco7OVXqoo3hCZBSg/bhKcLMcrSdifKJvgc3twNyMWL807rq53ibPBmGxdSC8ygRe" +
            "q3mgpkrs1yyshsR6UxWwvMBQUetxh4nFOaEd8yWXk8xACZFJJIyEgwrBUKn9pGneHxf7wTSeOkNZWvfAlKmMq4CCeSmBlcyJ" +
            "8uA0d6tu3yzm9ijRDp8hflQ6hUGsLUJjDgrjaEdVVQDLscMadpDyAKqqPSzHTmi6eugngTSNQgANZq7Ww8wSBNHOeciXAyVb" +
            "yAKajOYmkxJkBV0SB3TlJOdt5eYT7eRtueD207S3o8SxNYql76FQ5tYygn3NiLSjfS8C40QZlMRIajsRHsnASpF8cmXogcKC" +
            "OjUpxoILMTvymFkpEf4JfZzz0FhNOChmaME5MLu52s029k6OHe0f0WGgAJTZhbS9wIynx+iJO1ZsbvXQ3AkszzJOZAsr4l6n" +
            "Krnnw8eUAQ2NObT1bMZkzyxGeMA+rUxKSuNdAK58ytA+tMLNLoTb5CJQIOlfjaukVqUVhuWCx0MfkPbCzTw0vAtCeyCgRV9T" +
            "BjS0eGho93gIfrL3EyMZuMG4KauhykNbr8fdXkvrwPCQKJeEDGoPmR/riPmREOFiDtQtlCcyTkLAoTlM3kYdJDHRnvQZhraG" +
            "OfT1erh/mXEpn6RUYmQ7tgJ1rR4m75OHGfULxf4R+zcLywzFksejIC/R2NBYcYpG5i+P7wcT/jEpn68p0cdE/ObUgfp1qTCs" +
            "MSxr87uqyuPMeG453PLhsiH5Ltimk0OzKBSArduEZjE3wXh7PI/xtV/RTPtEwjGF9hD8EhN7RmViLEcnBcPn9SuTj41JSFMx" +
            "CY29+z2M3mF8HL2cxz2RgguAaGzklCPt0rS4VMm9Mi6wSqcAks9aWjwM3GNYJvrjiwgWznNyzBFq42AtExsZTjqtu8dlh1ze" +
            "4x33/y/QuMlDRXU1xoeWBF45j9uTVzRTQ6K2I9tEUC5V66B9Ngk6cW9FS0Ogpws8jDL2nbmRpq2CmloPy8sanrZ2kqtlObBG" +
            "tmNaK5qQUFEuNkwXgvhARp3SQqUUzL+u8Vb9LIhmWGY+L5grjRml4YKlVifhW1wTG5SUd03TFikfX0Bav1ZXeKhuFDWQKWjN" +
            "Z6ikPaZqYH6BCBLLNs8VL3KNmW2cZV6+4Sr5NREcIuRcCCFzRYnaJ8xLFJmT3pO0S0SE4uxU1QDl1eVYWVjFghJoqBzFjOg/" +
            "z+MMir7qfSUQ8MLpYNyP3NJZjrKyAirr6sF8H3MTo9zryAaFPG24ClpOm8/jowy1NYJp6dtEao+f7ymTxKsk7ECZj+MXFf1T" +
            "zNs5D+V52mvQ8kQ8eNPG0wuzkVSe1/CVzJY8s/RwSkGJOpoRN1iBV0BCVRuVmE9WCTKRVZPZUvQJ9/HQxqOMhPQaMTdWLIdK" +
            "yWOJ8zhVL2mo0mQXrgFRp8eAO5ZLpcrLwKwM4iE8hIfwEB7C/76BGERCFI6H8BAewkN4CP97hocM4iE8hIfwEB6CFR4yiIfw" +
            "EB7CQ3gIVnjIIB7CQ3gID+EhwAQ6kfH/BU6rTazTktZxAAAAAElFTkSuQmCC";

        private static readonly string _PropKuangB64 =
            "iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAACx0lEQVR4nOWbz27TQBDGv/m8cUurNH9cRKNKVU9IVW88AGfu" +
            "PfAMqGfEDc5cy3tx4171xIGQpFErlNgeNGu3caoIOOLMT86uvV5Hnp2Z9eUb+fTxQ5HnuUAEUMV2IgAUIQQNeZ4zLwuIDSqg" +
            "dm9LkMa5Vs6UABGt3Fu3slqRNhPfXlfX0S4RDc1BWwUt1XoFRO2op6NdKMqiNA9KSBLrUMZhRWguQ1EUGP+cYDa7FdXSfmgf" +
            "1V5kRvYHAzw/HEKYPJoZrLFzxnmqk8lErq9vxru7z76JSGHPokWQQFlCptPJ+dnZy8MsGyobIRqa4cqEFs+yv9/9enX15f1o" +
            "NFre308T1U4rElpkKSHslbe3P5KLi7ef8zx/IyIxPx/mhCePxOxWLcLJydEiy45+AUdJi3YwM6wYjzs7eV6Eh89scwt+YrCh" +
            "MSRms1nIslEyn89DmrbD4MUC0u12ZTq9DyLKTcm4weCKOoy1MjZthcHAwhrtdOqvzAONtye2iDT9+xzCGYQzCGcQziCcQTiD" +
            "cAbhDMIZhDMIZxDOIJxBOINwBuEMwhmEMwhnEM4gnEE4g3AG4QzCGYQzCGcQziCcQTiDcAbhDMIZhDMIZxDOIJxBOINwBrFF" +
            "LKIu7c+E9UtdE2paZ3K+NP2Hf/o/iO+8XMZ6ho0q4I3SQ9OX9nq93ISa3W7XhlohPdzZqcSl/f5eAshGsfe6wbEYgCCT/Obm" +
            "e2or1Vb5cAidvK5zWJPTrhmsUDEl+d3d/NXl5bsrUgpT1qJFRPWzmvRbzpMkQVkWkphqvCY0zaFAer0eTk9PMqi+riojWuHc" +
            "NSxKj49f4ODgoC5dWbEq8jDBrRDD4QDDQV+jOr5dzm04J7pZYoJS4qg0PfzoRxErZnqs6EHb0Zimq4Uw+6zGwYK+6XjL9fYF" +
            "8mak7uMGpiohhGDbd12KF289mboFxBCuSvF+A0sQ+H33CsLTAAAAAElFTkSuQmCC";

        private static readonly string _SelectKuangB64 =
            "iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAADnElEQVR4nO2bS48bRRDH/1XdHnvsTZwYob1zgTwWDqzEMblG" +
            "uUXcc+PAFQ48VkJC8E34AEC+ANfA5pDEJIcgUJQIaYW0yS7xY8bdVahmbMKJAzMg283P8kMauWb+qqq2u6aKYlioAgABEiNE" +
            "1T5uDSICVYX3Do49fJSA319McHz8DLPZTKBij+0QTSZYqdvt8e7uLoZnh/AhRBwdHeHWrW91fP8uL0KJxaKsBZO9Vv7fQGqX" +
            "ETns77+jN268S8PhEJ6IUBRzOTy8zdNJ+O7ipct3ZtPZwDmWTZVqiAr3e4PJeHxvf3z/7pVr166LxMiemcFMqhpwae/NO198" +
            "/uVXDx8+GGVZFrHBlGXpLly4ePzJwUf09MmjK1WqisJXR5erVjGb9k3s48dPzne7fqMFF0Vw9j6fzvuWotW6RFgKrlAoSM2z" +
            "Jtb7zRZsZFkWLTUtm1eLMGPL0T8X30QE12hqgl/yv+Bth5EYjMRgJAYjMRiJ4ds0RgthISFmbmWjJSLEyqodFqyj4NNXzk2E" +
            "nfiWzAYEsEQ+c3qar5XgsDT02vc/7KEouyBI01ypXKpgdLPitzde/2l1jqb4pgZiJGLvo5SFf/Xrbz4792IynDkGNQxqJSCP" +
            "guc7g5OjDz68yVk3xBCcc3UJbi1CWjp+PnVuWHoXIQ2dzFZxic5stnaBaFmwbajJdiZi/mlYBxStbbVcRGUkBiMxGInBSAxG" +
            "YjASg5EYjMRgJAYjMRiJwUgMRmIwEoORGL5NY0QQsVuTTAqpqzT/GCa1bbXZXFvBGmIvj9E27Y4bNsOIEHKJKELsrZngaNUc" +
            "6rBTObvz63PztHdztQJcA8yzJtZsOna6qGInNs5C3+jbAJyzGnQkddBH773/cZTYaknGxKpT9YiE6lxrFNILZmHvW+12Ms9W" +
            "Ytdy0ZIFq1TNM+3ZtDBuwbP/iuA6vHWtfzkZicFIDEZiMBKDkRiMxGCkK5iQlmDdruGOv/2npcunDURYJ/mquXqTMQ1lWTqo" +
            "3Zh/6crqnz4To9PJ0Mt7U2ubtwPbMgLQ6w0mlTuXmj1bhYKYOj7Dgx/Hbx8cfIr5fDIAtdcq9F9j2mIUzvv5xDT1B76KXpth" +
            "8uwc8jzny3tv6eHh7as//3Lvav2VTZ1poWo9sqvvdDKMRjswbTa7ZANbFMJCT05PqtklG+exCZeN1foXVqkKIsrzPo9G53Fm" +
            "ZwCyUTybTrOBLZthsrGebaEWTbAoXqXxHwgVkAq7VdKFAAAAAElFTkSuQmCC";

        private static readonly string _SliderBB64 =
            "iVBORw0KGgoAAAANSUhEUgAAARgAAAAaCAYAAABsMUMzAAAA4UlEQVR4nO3csWnDQBSA4afwIHFGyACuXWYTZ7FU1gDWJik9" +
            "RVawAw8SDmSTXnfd91WSiitU/BwH9yIAAAC4m+fT9HgB6NiWaZ5PTxHx0mtRgNUtI+ItM78i4vn+FWCjn6p6z8y8HI8fn5n5" +
            "u3VFgKaqpmU5X9oOZtfiktkeAbpoG5ZdO38BGEJggGEEBhhGYIBhBAYYRmCAYQQGGEZggGEEBhhGYIBhBAYYpl1AuraLSevd" +
            "AYDN1qZcs6oOy3I2rgHoPa7h0HYw31W177o0QMStjbV79SeA3ozjBQAAiP/+ADQPMromoHVIAAAAAElFTkSuQmCC";

        private static readonly string _SliderThumbB64 =
            "iVBORw0KGgoAAAANSUhEUgAAABgAAAAwCAYAAAALiLqjAAACc0lEQVR4nO2YTWsTQRzGn3nZkO4m2ZhS0lhKqwQPfgQ9W6Ue" +
            "vfXox/Db+AGEIggKkr6akyiirVSLrW1emqRN0nSTNC87IzMaTRtlN0Jv+zvswu7M//nPw1yeP3DFEPV4vvzs8fz8zUeGESq3" +
            "2y1JiP6sSAK4DcAFIP5Rg/567wAoSSmRSEyG9/e+tfL5wyxXf/L53B3bji/Ozs4JJXCpAeqz2RtSSmkYBggBy+UOurncIbTA" +
            "7tcv9KhYwML9hzSVuo56vYahU/hCSkk557DtODY2VrG+tsIkQLVAIjGJpuPg1csXWHiwiFvpNESnBSGl3/Jg3ECfGFjJvMba" +
            "agaxmA11Gi2gfItEo3CcM7zZXMfn7W2clApgXP/2RAgXphnBRCSKD+/fIRqN6eKq7u8KQgitKhtFPH27j01rCSAd3Z0ndAJo" +
            "FbFElpGeS6ErmC6uuNCiEglTCXtqBpi6CwjHnwCLgDUPEDvL6BsxKD4ioCFqiQu4dUC0/Ano9c7PO3/5cLhiaCDgRWCRJ4FF" +
            "ngQWeRJY5ElgkSeBRZ4EFnkSWORJYJEno/lAqmhJABICSM9fPiAGQPhfV14QUMmEMQrK1GclQn0GEKrXcs5G0ikfLh4KhdAm" +
            "NsK1Xdw7eQKD+YuyEhRu7xxtqwtxLQ4qehBiKKMNiiu2dvZwXi0hyb/rjX4gkHBBUXLCIJRiJpVEp9PVmU8LWJaJft/Fx09b" +
            "KFeOYZkmWjAxLlwIFAtF3db0dPLPCarVmsjlC6hUjoVlmv87ShCUUj1KyBeOmEqGlmUKLVA/Pc02Gk6KMV4mhKiRw2DT2MMQ" +
            "ShkkemGn2VRDj+zYNozLD3hc+EGiGi1IAAAAAElFTkSuQmCC";

        private static Texture2D _spPanelBack   = null;
        private static Texture2D _spButtonNull  = null;
        private static Texture2D _spPropKuang   = null;  // checkbox unchecked bg
        private static Texture2D _spSelectKuang = null;  // checkbox checked (checkmark)
        private static Texture2D _spSliderB     = null;
        private static Texture2D _spSliderThumb = null;
        private static bool      _menuSpsCached = false;
        private static Font      _gameFont      = null;

        // Cached plain-color fallback textures (each created once on demand)
        private static Texture2D _texTabActiveBg  = null; // active tab highlight
        private static Texture2D _texGhostHover   = null; // ghost-button hover
        private static Texture2D _texGhostActive  = null; // ghost-button press
        private static Texture2D _texWipBg        = null; // WIP warning banner bg
        private static Texture2D _texApkBannerBg  = null; // APK update banner bg (red-orange)

        // Cached GUIStyles (nullified when sprites/font are first loaded)
        private static GUIStyle  _gsWinBg         = null;
        private static GUIStyle  _gsVScroll        = null;
        private static GUIStyle  _gsLabel          = null;
        private static GUIStyle  _gsHint           = null;
        private static GUIStyle  _gsSectionHdr     = null;
        private static GUIStyle  _gsTabActive      = null;
        private static GUIStyle  _gsTabIdle        = null;
        private static GUIStyle  _gsGhostBtn       = null;
        private static GUIStyle  _gsInvisBg        = null;
        private static GUIStyle  _gsInvisThumb     = null;
        private static GUIStyle  _gsWipBanner      = null;
        private static GUIStyle  _gsApkBanner      = null; // reuse layout, different bg colour
        private static GUIStyle  _gsKeyLabelCtrl   = null;  // key/axis badge in Controllers tab
        private static GUIStyle  _gsKeyLabelKbm    = null;  // key badge in KB+Mouse tab
        private static System.Collections.Generic.Dictionary<long, GUIStyle> _gsBtnCache
            = new System.Collections.Generic.Dictionary<long, GUIStyle>();

        // -- HUD drag editor ---------------------------------------------------
        private bool    _hudEditMode    = false;
        private bool    _hudEditNotInGameMsg = false;
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

        // Supplemental screen-space fire-button detection (see TouchFireDetect)
        private float _touchFireCooldown = 0f;
        private bool  _kbmFireWasHeld   = false;
        private JoyStickController _joyStickCtrl         = null;
        private bool               _joyStickCtrlSearched  = false; // true after FindObjectOfType ran once (prevents per-frame search in CNR mode where JoyStickController is absent)
        private System.Reflection.FieldInfo _fiFireHoldCount = null;
        private System.Reflection.FieldInfo _fiJumpIsJumping  = null;
        private System.Reflection.FieldInfo _fiJumpTime       = null;
        private System.Reflection.FieldInfo _fiJumpCharCtrl   = null;
        private System.Collections.Generic.HashSet<int> _jumpTouchIds =
            new System.Collections.Generic.HashSet<int>();
        // Tracks which touch finger IDs legitimately STARTED on the fire button this frame.
        // Used by SuppressFalseFire() to cancel fire events from camera-swipe touches.
        private System.Collections.Generic.HashSet<int> _fireTouchIds =
            new System.Collections.Generic.HashSet<int>();

        // Our own jump arc � replaces JoyStickController's clunky 5-segment arc.
        // The game permanently applies Physics.gravity (-9.81 m/s Y) to the
        // CharacterController via cc.Move in its own Update().  We add our Y on top
        // in LateUpdate.  Net Y per frame = (_ownJumpVelY + (-9.81)) * dt.
        //
        // As a result, we need _ownJumpVelY > 9.81 to rise and < 9.81 to fall.
        // Asymmetric gravity (faster fall) gives the classic snappy platformer feel.
        //
        // Tuned for ~1.3 m peak height, ~0.25 s rise, ~0.25 s fall (0.50 s total).
        // Compare: original arc was ~0.9 m peak, 1.0 s total, uniform/flat.
        // Peak formula: h = (JumpInitialVel - 9.81)� / (2 * |JumpAscendGrav|)
        //   JumpInitialVel=24 ? ~2.5 m,  =22 ? ~1.8 m,  =20 ? ~1.3 m,  =19 ? ~1.0 m,  =18 ? ~0.8 m
        private bool  _ownJumpActive  = false;
        private bool  _kbmJumpPending  = false;  // set on spacebar down, consumed in LateUpdate
        private float _ownJumpVelY   = 0f;
        private const float JumpInitialVel  = 19f;   // slightly above vanilla
        private const float JumpAscendGrav  = -41f;  // d/dt(_ownJumpVelY) while rising
        private const float JumpDescendGrav = -56f;  // d/dt(_ownJumpVelY) while falling

        // -- Touch camera (routes touch look through KbmInjectMouseLook, eliminating deadzone)
        private int   _touchCamFingerId  = -1;
        private float _touchCamPrevX     = 0f;
        private float _touchCamPrevY     = 0f;
        private bool  _prevBDied         = false;  // respawn detection: resets camera finger on revive

        // -- Fire-cooldown crosshair (KBM cursor-locked) ----------------------
        private bool      _weapOnCooldown   = false;   // set in LateUpdate, read in OnGUI
        private Component _wmComp           = null;    // cached UnityScript WeaponManager component
        private FieldInfo _fiWmSelWeapon    = null;    // WeaponManager.SelectedWeapon
        private FieldInfo _fiWmNextFireTime = null;    // WeaponScript.nextFireTime

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
        private void Start()
        {
            UpdateScene(Application.loadedLevelName);
            RegisterWithEconomyHook();
            KbmRegisterMouseListener();
            CheckApkVersion();
            SetImmersiveMode();
        }

        private static void SetImmersiveMode()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            try
            {
                // Do NOT use 'using' on up/ac — runOnUiThread posts the runnable
                // asynchronously to the Android UI thread, so ac would be disposed
                // before the lambda executes, causing a native JNI crash.
                AndroidJavaClass  up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject ac = up.GetStatic<AndroidJavaObject>("currentActivity");
                ac.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        AndroidJavaObject window    = ac.Call<AndroidJavaObject>("getWindow");
                        AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView");
                        // SYSTEM_UI_FLAG_IMMERSIVE_STICKY (0x1000) |
                        // SYSTEM_UI_FLAG_HIDE_NAVIGATION  (0x0002) |
                        // SYSTEM_UI_FLAG_FULLSCREEN        (0x0004) |
                        // SYSTEM_UI_FLAG_LAYOUT_STABLE     (0x0100) |
                        // SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION (0x0200) |
                        // SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN (0x0400)
                        int flags = 0x1000 | 0x0002 | 0x0004 | 0x0100 | 0x0200 | 0x0400;
                        decorView.Call("setSystemUiVisibility", flags);
                    }
                    catch (System.Exception ex)
                    {
                        SettingsModEntry.Log("SetImmersiveMode UI-thread error: " + ex.Message);
                    }
                }));
            }
            catch (System.Exception e)
            {
                SettingsModEntry.Log("SetImmersiveMode error: " + e.Message);
            }
        }

        private void CheckApkVersion()
        {
            try
            {
                using (AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject ac = up.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject pm = ac.Call<AndroidJavaObject>("getPackageManager"))
                using (AndroidJavaObject pi = pm.Call<AndroidJavaObject>("getPackageInfo", ac.Call<string>("getPackageName"), 0))
                {
                    _apkVersionName = pi.Get<string>("versionName") ?? "";
                    _apkNeedsUpdate = !_apkVersionName.Contains("-cnr");
                    SettingsModEntry.Log("APK versionName=" + _apkVersionName + " needsUpdate=" + _apkNeedsUpdate);
                }
            }
            catch (Exception ex)
            {
                SettingsModEntry.Log("CheckApkVersion err: " + ex.Message);
                _apkNeedsUpdate = true; // show banner anyway; better safe than silent
            }
        }

        private void RegisterWithEconomyHook()
        {
            Type eco = EcoType();
            if (eco == null) return;
            FieldInfo fPres = eco.GetField("SettingsModPresent", BindingFlags.Public | BindingFlags.Static);
            if (fPres != null) fPres.SetValue(null, true);
            FieldInfo fCb = eco.GetField("OnAccountButtonClicked", BindingFlags.Public | BindingFlags.Static);
            if (fCb != null) fCb.SetValue(null, (System.Action)OpenAccount);
            // Register our version with CNRMod's mod version registry
            try
            {
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null) continue;
                    MethodInfo regM = me.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    if (regM != null) regM.Invoke(null, new object[] { "CNRSettingsMod", SettingsModEntry.Version });
                    break;
                }
            }
            catch { }
            SettingsModEntry.Log("Registered with EconomyHook");
        }

        private void OnLevelWasLoaded(int level)
        {
            UpdateScene(Application.loadedLevelName);
            SetImmersiveMode();
            SettingsModEntry.Log("scene=" + Application.loadedLevelName + " inGame=" + _inGameScene);
            StartCoroutine(DumpSpritesDelayed());
        }

        private IEnumerator DumpSpritesDelayed()
        {
            yield return null;
            yield return null;
            if (!_menuSpsCached) CacheMenuSystemSprites();
        }

        private void UpdateScene(string scene)
        {
            _sceneName        = scene ?? "";
            _inGameScene      = IsGameScene(scene);
            _btnPatched       = false;
            _menuBtnPatched   = false;
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
            _kbmJoystick      = null;
            _weaponManager    = null;
            KbmSetCursorLocked(false);
            _mainCam          = null;
            _scopedFov        = -1f;
            if (_nguiUICam != null) { _nguiUICam.enabled = true; _nguiUICam = null; }
            _nguiCam          = null;
            _pausePanelRef    = null;
            _pauseUIPanel     = null;
            _wasPauseVisible  = false;
            _lastMousePosValid = false;
            _uiCamCache        = new UICamera[0];
            _uiCamCacheAge     = 0f;
            _chatBarGO        = null;
            _chatInputGO      = null;
            _chatWasFocused   = false;
            for (int i = 0; i < DRAG_COUNT; i++) { _dragGOs[i] = null; _dragOrigPos[i] = Vector3.zero; /* keep _dragOrigScale so captured baseline survives multiple ApplyHUDOnLoad runs */ }
            for (int i = 0; i < _visGOs.Length;  i++) _visGOs[i]  = null;
            _joyStickCtrl         = null;
            _joyStickCtrlSearched = false;
            _fiJumpIsJumping  = null;
            _fiJumpTime       = null;
            _fiJumpCharCtrl   = null;
            _jumpTouchIds.Clear();
            _fireTouchIds.Clear();
            _ownJumpActive = false;
            _ownJumpVelY   = 0f;
            _kbmFireWasHeld  = false;
            _prevBDied       = false;
            _weapOnCooldown  = false;
            _wmComp          = null;
            LoadPrefs();
            CheckApkVersion(); // re-run on every scene load so new instances (after in-app DLL reload) always have the result
            if (_inGameScene) StartCoroutine(ApplyHUDOnLoad());
            if (_inGameScene && _kbmEnabled) StartCoroutine(AutoLockAfterLoad());
            if (_sceneName == "MainMenu") StartCoroutine(PatchMainMenuAfterLoad());
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
            else SetImmersiveMode();
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
            // Gamepad axis capture polling � runs every frame, in all scenes.
            if (_gpCaptureIdx >= 0 && _gpCaptureCooldown <= 0)
                GpCaptureAxisPoll();
            else if (_gpCaptureCooldown > 0)
                _gpCaptureCooldown--;

            // Stick Axes detection polling (Detect button in Controllers tab).
            if (_gpStickDetect > 0) GpStickDetectPoll();

            // Set UICamera.useMouse � must run in ALL scenes (including main menu).
            // In KBM mode: always true so hardware mouse clicks reach NGUI everywhere.
            // In pure touch mode: leave at Android default (false).
            _uiCamCacheAge -= Time.deltaTime;
            if (_uiCamCache.Length == 0 || _uiCamCacheAge <= 0f)
            {
                _uiCamCache    = (UICamera[])FindObjectsOfType(typeof(UICamera));
                _uiCamCacheAge = 2f;
            }
            bool wantMouse = _kbmEnabled;
            foreach (UICamera cam in _uiCamCache)
                if (cam != null) cam.useMouse = wantMouse;

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

                if (nowVisible && !_wasPauseVisible)
                {
                    if (!_btnPatched) StartCoroutine(PatchAfterFrame());
                    if (_kbmEnabled && _cursorLocked) KbmSetCursorLocked(false);
                }
                if (!nowVisible && _wasPauseVisible)
                {
                    _btnPatched = false;
                    if (_kbmEnabled && !_showSettings) KbmSetCursorLocked(true);
                }
                _wasPauseVisible = nowVisible;
            }

            if (_hudEditMode) HandleHudDrag();
            // Write desired scales here so UIPanel.LateUpdate (runs after Update) sees the transform change.
            // LateUpdate will also write to beat UIRoot, but UIPanel has already processed by then.
            EnforceScales();

            // Run jump detection here too (in addition to LateUpdate) so that if our
            // Update() executes before JoyStickController.Update() we set OnJump=1 in
            // the same frame and the jump fires immediately on the first press.
            if (!_hudEditMode) TouchJumpDetect();

            // -- KBM input ----------------------------------------------------
            if (!_kbmEnabled) return;

            // Chat input: block all gameplay input while chat is focused
            bool chatFocused = UIInputForChat.current != null;
            if (!_chatWasFocused && chatFocused)
            {
                // Chat just opened � make sure cursor is released
                if (_cursorLocked) KbmSetCursorLocked(false);
            }
            if (_chatWasFocused && !chatFocused)
            {
                // Chat just closed � re-lock for gameplay
                if (!_showSettings) KbmSetCursorLocked(true);
            }
            _chatWasFocused = chatFocused;

            if (chatFocused)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) UICamera.selectedObject = null;
                return; // suppress all gameplay input
            }

            // On some Android phones right-click fires both Mouse1 and KeyCode.Escape
            // (the OS maps the right button to the Back action which Unity receives as Escape).
            // Guard: don't unlock on Escape if the aim key (Mouse1) is simultaneously held.
            bool aimKeyHeld = (_kbKeys[8] == KeyCode.Mouse1 && Input.GetMouseButton(1))
                           || (_capListener != null && _kbKeys[8] == KeyCode.Mouse1 && _capListener.rmbHeld);
            if (Input.GetKeyDown(KeyCode.Escape) && _cursorLocked && !aimKeyHeld) { KbmSetCursorLocked(false); return; }
            if (Input.GetMouseButtonDown(0) && !_cursorLocked && !_showSettings && !_wasPauseVisible) { KbmSetCursorLocked(true); return; }
            if (!_cursorLocked || _showSettings) return;

            // Jump � set pending flag; TriggerOwnJump fires in LateUpdate once _joyStickCtrl is ready.
            if (Input.GetKeyDown(_kbKeys[1])) _kbmJumpPending = true;

            // Weapon scroll + keybinds
            _kbmScrollAccum += Input.GetAxis("Mouse ScrollWheel");
            if (_kbmScrollAccum >= 0.1f
                || (_kbKeys[6] != KeyCode.None && Input.GetKeyDown(_kbKeys[6])))
            {
                _kbmScrollAccum = 0f;
                if (_dragGOs[15] != null)  // Index 15 = Next gun button
                {
                    _dragGOs[15].SendMessage("OnPress", true,  SendMessageOptions.DontRequireReceiver);
                    _dragGOs[15].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
                KbmSwitchWeapon(+1);
            }
            else if (_kbmScrollAccum <= -0.1f
                || (_kbKeys[7] != KeyCode.None && Input.GetKeyDown(_kbKeys[7])))
            {
                _kbmScrollAccum = 0f;
                if (_dragGOs[14] != null)  // Index 14 = Prev gun button
                {
                    _dragGOs[14].SendMessage("OnPress", true,  SendMessageOptions.DontRequireReceiver);
                    _dragGOs[14].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
                KbmSwitchWeapon(-1);
            }

            // Aim toggle (index 8)
            // When capture active, RMB arrives via rmbHeld � detect rising edge.
            // When not captured, fall back to Input.GetKeyDown.
            bool aimKeyDown;
            if (_capListener != null && _kbKeys[8] == KeyCode.Mouse1)
            {
                bool rmbNow = _capListener.rmbHeld;
                aimKeyDown = rmbNow && !_rmbWasHeld;
                _rmbWasHeld = rmbNow;
            }
            else
            {
                aimKeyDown = _kbKeys[8] != KeyCode.None && Input.GetKeyDown(_kbKeys[8]);
                _rmbWasHeld = false;
            }
            if (aimKeyDown) PlayerPrefs.SetInt("OnAim", 1);

            // Pause (index 9)
            if (_kbKeys[9] != KeyCode.None && Input.GetKeyDown(_kbKeys[9]))
                KbmPressPause();

            // Reload (index 10)
            if (_kbKeys[10] != KeyCode.None && Input.GetKeyDown(_kbKeys[10]))
                PlayerPrefs.SetInt("FpsReload", 1);

            // Player list (index 11)
            if (_kbKeys[11] != KeyCode.None && Input.GetKeyDown(_kbKeys[11]))
                KbmToggleLeaderboard();

            // Chat (index 12)
            if (_kbKeys[12] != KeyCode.None && Input.GetKeyDown(_kbKeys[12]))
                KbmHandleChat();

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

        private IEnumerator PatchMainMenuAfterLoad()
        {
            yield return null;
            yield return null;
            PatchMainMenuButton();
        }

        private void PatchMainMenuButton()
        {
            // Button hiding and anchor caching are now handled by IPRedirectMod (EconomyHook).
            _menuBtnPatched = true;
        }

        public void OpenAccount()
        {
            _activeTab = 2;
            _scrollAccount = Vector2.zero;
            _accountMsg = "";
            OpenSettings();
        }

        private bool _spriteDumped = false;
        public void OpenSettings()
        {
            if (Time.unscaledTime - _lastToggleTime < 0.5f) return;
            _lastToggleTime = Time.unscaledTime;
            _showSettings = !_showSettings;
            if (_showSettings)
            {
                ReCacheHUD(); CacheNguiCam();
                if (!_spriteDumped) { _spriteDumped = true; DumpAllSprites(); }
                // Ensure sprites/font are loaded (may not have been ready at scene-load time)
                if (!_menuSpsCached) CacheMenuSystemSprites();
                if (_gameFont == null)
                {
                    UILabel[] lbls = (UILabel[])FindObjectsOfType(typeof(UILabel));
                    foreach (UILabel lbl in lbls)
                        if (lbl.font != null && lbl.font.dynamicFont != null)
                        { _gameFont = lbl.font.dynamicFont; break; }
                }
                // Block NGUI from processing touches while settings are open
                if (_nguiUICam == null && _nguiCam != null) _nguiUICam = _nguiCam.GetComponent<UICamera>();
                if (_nguiUICam == null) { UICamera[] cams = (UICamera[])FindObjectsOfType(typeof(UICamera)); if (cams.Length > 0) _nguiUICam = cams[0]; }
                if (_nguiUICam != null) _nguiUICam.enabled = false;
                // Suspend KBM cursor lock so user can interact with the settings panel
                if (_kbmEnabled && _cursorLocked) KbmSetCursorLocked(false);
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
            // Supplemental fire-button touch detection (bypasses Physics.Raycast).
            TouchFireDetect();
            // KBM fire: must run in LateUpdate so it overrides JoyStickController.Update()
            // which resets mStatus to idle every frame when Input.touchCount == 0.
            KbmFireDetect();
            // Touch camera: route right-side drag through KbmInjectMouseLook so behaviour
            // matches mouse exactly (no Android touch-slop deadzone, same sensitivity slider).
            TouchCameraDetect();
            // Suppress false fire events caused by camera-swipe touches accidentally
            // triggering the NGUI fire button (vanilla bug: UIButtonEventKit.OnPress fires
            // when any touch enters the button's collider, including camera-drag touches).
            // Only active in CNR multiplayer touch mode.
            SuppressFalseFire();
            // Jump-on-press: NGUI's OnClick fires on finger-up, so holding the jump
            // button delays the jump until release.  This detects the touch directly
            // and sets OnJump=1 on Began so the jump triggers immediately, and again
            // each frame while held so the player auto-re-jumps on landing.
            TouchJumpDetect();
            KbmJumpDetect();
            OwnJumpPhysics();
            ApplySensitivity();
            // In KBM mode with cursor locked: prevent Sliderotate.Update() from running.
            // Sliderotate has an "else" (MouseY-only) branch that reads Input.GetAxis("Mouse Y")
            // and writes to transform.localEulerAngles every Update frame, even without touches.
            // This causes double-application of mouse input � our KbmInjectMouseLook also applies
            // the same delta in LateUpdate.  Blocking it via cannotRotate eliminates the conflict.
            if (_kbmEnabled && _cursorLocked)
            {
                if (_sliderotate == null) CacheSliderotate();
                if (_sliderotate != null && _fiCannotRotate != null)
                    _fiCannotRotate.SetValue(_sliderotate, true);
            }
            // Gamepad right stick also injects via KbmInjectMouseLook � suppress Sliderotate too.
            else if (_gamepadEnabled && _joyProxy != null && _inGameScene)
            {
                if (_sliderotate == null) CacheSliderotate();
                if (_sliderotate != null && _fiCannotRotate != null)
                    _fiCannotRotate.SetValue(_sliderotate, true);
            }
            // KBM mouse look.
            // Primary: pointer capture (API 26+) gives AXIS_RELATIVE on captured events.
            // Fallback 1: WindowCallbackProxy AXIS_RELATIVE (if proxy fires before Unity).
            // Fallback 2: Input.GetAxis (may be 0 on old Android builds).
            // Fallback 3: mousePosition delta (stops at screen edges, last resort).
            if (_kbmEnabled && _cursorLocked && !_showSettings)
            {
                float dx, dy;
                bool captureHandled = false;
                // Always drain capListener � _captureActive may lag behind the actual grant
                if (_capListener != null)
                {
                    dx = _capListener.DrainDx();
                    dy = _capListener.DrainDy();
                    // dy from MotionEvent: positive = down; Unity camera: positive = up
                    if (dx != 0f || dy != 0f)
                    {
                        _captureActive = true;  // self-correct the flag once we see real data
                        KbmInjectMouseLook(dx * 0.05f, -dy * 0.05f);
                        _lastMousePosValid = false;
                        captureHandled = true;
                        _dbgSource = "capListener"; _dbgRawDx = dx; _dbgRawDy = dy;
                    }
                }
                if (!captureHandled)
                {
                    // Drain WindowCallbackProxy accumulator
                    dx = _amlDx;  _amlDx = 0f;
                    dy = _amlDy;  _amlDy = 0f;
                    if (dx != 0f || dy != 0f)  // no _amlGotData gate: any non-zero accumulator fires
                    {
                        KbmInjectMouseLook(dx * 0.05f, -dy * 0.05f);
                        _lastMousePosValid = false;
                        _dbgSource = "aml"; _dbgRawDx = dx; _dbgRawDy = dy;
                    }
                    else
                    {
                        // Fallback 2: Unity axis
                        float axX = Input.GetAxis("Mouse X");
                        float axY = Input.GetAxis("Mouse Y");
                        _dbgAxX = axX; _dbgAxY = axY;
                        if (axX != 0f || axY != 0f)
                        {
                            KbmInjectMouseLook(axX * 3f, axY * 3f);
                            _lastMousePosValid = false;
                            _dbgSource = "GetAxis"; _dbgRawDx = axX; _dbgRawDy = axY;
                        }
                        else
                        {
                            // Fallback 3: mousePosition delta
                            Vector3 curPos = Input.mousePosition;
                            _dbgMousePos = curPos;
                            if (_lastMousePosValid)
                            {
                                float mx = (curPos.x - _lastMousePos.x) * 0.05f;
                                float my = (curPos.y - _lastMousePos.y) * 0.05f;
                                if (mx != 0f || my != 0f)
                                {
                                    KbmInjectMouseLook(mx, my);
                                    _dbgSource = "mousePos"; _dbgRawDx = mx; _dbgRawDy = my;
                                }
                                else { _dbgSource = "NONE"; _dbgRawDx = 0f; _dbgRawDy = 0f; }
                            }
                            _lastMousePos      = curPos;
                            _lastMousePosValid = true;
                        }
                    }
                }
                _dbgFrame = Time.frameCount;
            }
            else
            {
                _lastMousePosValid = false;
                _amlDx = 0f;
                _amlDy = 0f;
                if (_capListener != null) { _capListener.DrainDx(); _capListener.DrainDy(); }
            }
            // Joystick inject runs at the END of LateUpdate so it cannot interfere with
            // the mouse-look code above.  Movement uses the previous frame's inject (1-frame
            // lag is imperceptible for held keys) which is fine since we inject every frame.
            if (_kbmEnabled && _inGameScene) KbmInjectJoystick();
            // Touch-joystick deadzone: when NOT in KBM mode, post-process _deltaPixels to kill drift.
            // KBM mode handles its own deadzone inside KbmInjectJoystick.
            else if (_gamepadEnabled && _inGameScene) GamepadUpdate();
            else if (_inGameScene && _touchDeadzone > 0f)
                ApplyTouchJoystickDeadzone();
            // Update fire-cooldown state for crosshair coloring.
            if (_inGameScene) UpdateWeaponCooldown();
        }

        // Reads nextFireTime from the selected WeaponScript to know if the gun is still cooling down.
        private void UpdateWeaponCooldown()
        {
            if (_wmComp == null)
            {
                var wmGo = GameObject.FindWithTag("WeaponManager");
                if (wmGo != null) _wmComp = wmGo.GetComponent("WeaponManager");
            }
            if (_wmComp == null) { _weapOnCooldown = false; return; }

            if (_fiWmSelWeapon == null)
                _fiWmSelWeapon = _wmComp.GetType().GetField("SelectedWeapon");
            var selectedWeapon = _fiWmSelWeapon != null ? _fiWmSelWeapon.GetValue(_wmComp) : null;
            if (selectedWeapon == null) { _weapOnCooldown = false; return; }

            if (_fiWmNextFireTime == null)
                _fiWmNextFireTime = selectedWeapon.GetType().GetField("nextFireTime",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            if (_fiWmNextFireTime == null) { _weapOnCooldown = false; return; }

            float nft = (float)_fiWmNextFireTime.GetValue(selectedWeapon);
            _weapOnCooldown = Time.time < nft;
        }

        // Suppresses false fire events in CNR multiplayer touch mode.
        // The vanilla NGUI UICamera fires OnPress(true) on the fire button whenever
        // ANY touch enters its collider -- including camera-swipe touches that were
        // never intended as fire. This sets CRInput.m_bFire / CRJoyStickController.fireFlag
        // which causes other players to hear continuous gunfire (no damage occurs because
        // no hit-detection raycast is performed). Fix: track which touch IDs actually BEGAN
        // on the fire button; cancel any that drift far from it; reset fire flags when no
        // legitimate fire touch is active.
        private void SuppressFalseFire()
        {
            // Only relevant in CNR multiplayer touch mode.
            if ((object)CRInput.mInstance == null) return;
            if (_kbmEnabled && _cursorLocked) return;  // KBM: KbmFireDetect owns fire
            if (_gamepadEnabled) return;               // Gamepad: its own fire path

            if (_dragGOs[0] == null || _nguiCam == null)
            {
                _fireTouchIds.Clear();
                return;
            }

            if (Input.touchCount == 0)
            {
                _fireTouchIds.Clear();
                CRInput.mInstance.m_bFire = false;
                if ((object)CRJoyStickController.mInstance != null)
                    CRJoyStickController.mInstance.fireFlag = false;
                return;
            }

            // Project fire button to screen space.
            Vector3 worldCenter = _dragGOs[0].transform.position;
            Vector3 scrCenter   = _nguiCam.WorldToScreenPoint(worldCenter);
            float   halfW       = _dragGOs[0].transform.lossyScale.x * 0.5f;
            float   scrRadius   = 80f;
            if (halfW >= 0.001f)
            {
                Vector3 scrEdge = _nguiCam.WorldToScreenPoint(
                    worldCenter + _nguiCam.transform.right * halfW);
                scrRadius = Mathf.Max(80f, Mathf.Abs(scrEdge.x - scrCenter.x));
            }
            float pressRadius2 = scrRadius * scrRadius;
            float driftRadius2 = (scrRadius * 1.8f) * (scrRadius * 1.8f);

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _fireTouchIds.Remove(t.fingerId);
                    continue;
                }
                float dx = t.position.x - scrCenter.x;
                float dy = t.position.y - scrCenter.y;
                float d2 = dx * dx + dy * dy;
                // Claim touch if it started within the fire button bounds.
                if (t.phase == TouchPhase.Began && d2 <= pressRadius2)
                    _fireTouchIds.Add(t.fingerId);
                // A tracked fire touch that drifted far from the button is now a camera/movement drag.
                // Release the NGUI button so it stops being treated as pressed.
                if (_fireTouchIds.Contains(t.fingerId) && d2 > driftRadius2)
                {
                    _fireTouchIds.Remove(t.fingerId);
                    _dragGOs[0].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
            }

            // No tracked fire touch → no legitimate fire from touch input → suppress.
            if (_fireTouchIds.Count == 0)
            {
                CRInput.mInstance.m_bFire = false;
                if ((object)CRJoyStickController.mInstance != null)
                    CRJoyStickController.mInstance.fireFlag = false;
            }
        }

        // Supplemental fire-button touch detection.
        // The game detects the fire button via Physics.Raycast from the NGUI camera.
        // When we expand the camera rect (to make NGUI accept touches anywhere), the
        // camera's aspect ratio changes, which can shift the camera's coordinate mapping
        // so the Physics.Raycast misses the FireButton collider for the first touch.
        // This method independently checks whether any active touch lands within the
        // FireButton's projected screen bounds, and if so directly sets the fire status.
        // It runs in LateUpdate (after JoyStickController.Update) so it can't be
        // overridden by the walk/idle logic for the same frame.
        private void TouchFireDetect()
        {
            _touchFireCooldown -= Time.deltaTime;
            if (_touchFireCooldown < 0f) _touchFireCooldown = 0f;
            if (_touchFireCooldown > 0f) return;

            if (_dragGOs[0] == null || _nguiCam == null) return;

            if ((object)PlayerLogic.mInstance == null || PlayerLogic.mInstance.bDied) return;
            if (NGUI.mInstance != null && NGUI.mInstance.noClips) return;
            if (Input.touchCount == 0) return;

            // Cache JoyStickController (used to keep fireStatusHoldTimeCount > 0).
            if ((object)_joyStickCtrl == null)
            {
                _joyStickCtrl = (JoyStickController)UnityEngine.Object.FindObjectOfType(typeof(JoyStickController));
            }
            if (_fiFireHoldCount == null && (object)_joyStickCtrl != null)
            {
                _fiFireHoldCount = typeof(JoyStickController).GetField(
                    "fireStatusHoldTimeCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            // Project fire-button center and edge to screen space to get tap radius.
            Vector3 worldCenter = _dragGOs[0].transform.position;
            Vector3 scrCenter   = _nguiCam.WorldToScreenPoint(worldCenter);
            float   halfW       = _dragGOs[0].transform.lossyScale.x * 0.5f;
            if (halfW < 0.001f) return;
            Vector3 scrEdge = _nguiCam.WorldToScreenPoint(
                worldCenter + _nguiCam.transform.right * halfW);
            float scrRadius = Mathf.Abs(scrEdge.x - scrCenter.x);
            if (scrRadius < 30f) scrRadius = 30f;   // minimum 30 px tap area
            float scrRadius2 = scrRadius * scrRadius;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Began &&
                    t.phase != TouchPhase.Stationary &&
                    t.phase != TouchPhase.Moved) continue;
                float dx = t.position.x - scrCenter.x;
                float dy = t.position.y - scrCenter.y;
                if (dx * dx + dy * dy > scrRadius2) continue;

                WeaponType wt = PlayerLogic.mInstance.mWeaponType;
                PlayerLogic.mInstance.mStatus =
                    (wt == WeaponType.BallisticKnife || wt == WeaponType.GingerbreadKnife)
                    ? PlayerStatus.knifeFire : PlayerStatus.fire;

                // KEY FIX: keep JoyStickController.fireStatusHoldTimeCount > 0 so its
                // "if (!flag && holdCount <= 0) mStatus = walk" guard stays FALSE.
                if ((object)_joyStickCtrl != null && _fiFireHoldCount != null)
                    _fiFireHoldCount.SetValue(_joyStickCtrl, 0.235f);

                if ((object)UIMenuDirector.mInstance != null)
                    UIMenuDirector.mInstance.GenFireEvent();

                _touchFireCooldown = 0.2f;  // slightly less than 0.235f � expires first
                break;
            }
        }

        // KBM fire: mirrors what the on-screen Fire button does.
        // Single-player (FreeRun/SingleMode): sets PlayerLogic.mStatus and keeps
        //   JoyStickController.fireStatusHoldTimeCount alive so Update() won't reset to idle.
        // CNR multiplayer: sets CRInput.m_bFire + CRJoyStickController.fireFlag, exactly as
        //   UIButtonEventKit ? GenFireEvent ? CRUIEventInteract.OnFire() does � but null-safe.
        //   CRWeaponScript.LateUpdate() reads m_bFire and fires the active melee weapon.
        // NOTE: In CNR mode PlayerLogic.mInstance is null � DON'T use it as a gate for fire!
        private void KbmFireDetect()
        {
            if (!_kbmEnabled || !_cursorLocked)
            {
                _kbmFireWasHeld = false;
                return;
            }
            bool fireHeld = (_kbKeys[0] == KeyCode.Mouse0)
                ? (_capListener != null ? _capListener.lmbHeld : Input.GetMouseButton(0))
                : (_kbKeys[0] != KeyCode.None && Input.GetKey(_kbKeys[0]));

            // Cache JoyStickController and fireStatusHoldTimeCount field (single-player scenes).
            // IMPORTANT: Only search once � in CNR mode JoyStickController is absent so searching
            // every frame is O(all scene components) ? causes periodic GC stutter.
            if ((object)_joyStickCtrl == null && !_joyStickCtrlSearched)
            {
                _joyStickCtrlSearched = true;
                _joyStickCtrl = (JoyStickController)UnityEngine.Object.FindObjectOfType(typeof(JoyStickController));
            }
            if (_fiFireHoldCount == null && (object)_joyStickCtrl != null)
                _fiFireHoldCount = typeof(JoyStickController).GetField(
                    "fireStatusHoldTimeCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (!fireHeld)
            {
                if (_kbmFireWasHeld)
                {
                    // On release: let JoyStickController reset mStatus to idle/walk next frame.
                    if ((object)_joyStickCtrl != null && _fiFireHoldCount != null)
                        _fiFireHoldCount.SetValue(_joyStickCtrl, 0f);
                    // Release the NGUI button so it doesn't stay stuck in pressed state.
                    if (_dragGOs[0] != null)
                        _dragGOs[0].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
                _kbmFireWasHeld = false;
                return;
            }

            if (NGUI.mInstance != null && NGUI.mInstance.noClips) return;

            bool fireRisingEdge = !_kbmFireWasHeld;
            _kbmFireWasHeld = true;

            // ---- Single-player path (PlayerLogic / JoyStickController scenes) ----
            if ((object)PlayerLogic.mInstance != null && !PlayerLogic.mInstance.bDied)
            {
                WeaponType wt = PlayerLogic.mInstance.mWeaponType;
                PlayerLogic.mInstance.mStatus =
                    (wt == WeaponType.BallisticKnife || wt == WeaponType.GingerbreadKnife)
                    ? PlayerStatus.knifeFire : PlayerStatus.fire;
                // Keep fireStatusHoldTimeCount > 0 so JoyStickController.Update() doesn't
                // override mStatus to idle (it only overrides when holdCount <= 0).
                if ((object)_joyStickCtrl != null && _fiFireHoldCount != null)
                    _fiFireHoldCount.SetValue(_joyStickCtrl, 0.235f);
            }

            // ---- CNR multiplayer path ----
            // OnPress(true) on rising edge only; OnPress(false) is sent on release (above).
            // m_bFire/fireFlag are set every frame to keep auto-fire running while held.
            if (fireRisingEdge)
            {
                if (_dragGOs[0] != null)
                    _dragGOs[0].SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                if ((object)UIMenuDirector.mInstance != null)
                    UIMenuDirector.mInstance.GenFireEvent();
            }
            if ((object)CRInput.mInstance != null)
                CRInput.mInstance.m_bFire = true;
            if ((object)CRJoyStickController.mInstance != null)
                CRJoyStickController.mInstance.fireFlag = true;
        }

        // Jump-on-press touch detection.
        // NGUI fires OnClick on finger-up, so holding the jump button delays the jump
        // until release.  This method detects the touch directly and bypasses the
        // PlayerPrefs-based signalling entirely: when the jump button is held and the
        // player is grounded, we directly write JoyStickController's private isJumping
        // and jumpTime fields (the same fields its own Update() writes) so a jump fires
        // in the current frame regardless of script execution order.
        // While the player is in the air (isJumping=true), we keep OnJump=1 in prefs
        // so JoyStickController re-jumps the instant it detects landing.
        //
        // Hit-testing uses Physics.Raycast from the NGUI camera � exactly what UICamera
        // does internally � so the detection correctly respects the button's actual
        // collider size regardless of any HUD-editor rescaling.
        // Handles keyboard jump in KBM mode � mirrors TouchJumpDetect but for keyboard.
        // Runs in LateUpdate so _joyStickCtrl can be cached here before TriggerOwnJump.
        private void KbmJumpDetect()
        {
            if (!_kbmEnabled || !_cursorLocked || !_kbmJumpPending) return;
            if ((object)PlayerLogic.mInstance == null || PlayerLogic.mInstance.bDied) return;

            // Cache JoyStickController and its reflection fields (same as TouchJumpDetect).
            if ((object)_joyStickCtrl == null)
                _joyStickCtrl = (JoyStickController)UnityEngine.Object.FindObjectOfType(typeof(JoyStickController));
            if ((object)_joyStickCtrl != null)
            {
                if (_fiJumpIsJumping == null)
                    _fiJumpIsJumping = typeof(JoyStickController).GetField(
                        "isJumping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (_fiJumpCharCtrl == null)
                    _fiJumpCharCtrl = typeof(JoyStickController).GetField(
                        "charactercontroller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            _kbmJumpPending = false;
            TriggerOwnJump();
        }

        // Routes right-side touch drag through KbmInjectMouseLook so touch and mouse camera
        // behave identically: same sensitivity slider, no Android touch-slop deadzone.
        // Suppresses Sliderotate for the tracked finger by toggling cannotRotate for one frame.
        private void TouchCameraDetect()
        {
            // Respawn: if bDied just cleared, discard any stale tracked camera finger so the
            // first post-respawn touch doesn't compute a huge delta from a pre-death position.
            bool nowDied = (object)PlayerLogic.mInstance != null && PlayerLogic.mInstance.bDied;
            if (_prevBDied && !nowDied) _touchCamFingerId = -1;
            _prevBDied = nowDied;

            if (_kbmEnabled && _cursorLocked) return; // KBM mouse handles camera
            if (_sliderotate == null && Input.touchCount > 0) CacheSliderotate();
            if (_sliderotate == null) return;
            if (nowDied) return;

            float sw = Screen.width;
            float sh = Screen.height;

            bool trackedAlive = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    if (t.fingerId == _touchCamFingerId) { _touchCamFingerId = -1; }
                    continue;
                }

                // Claim a new camera finger: right half of screen only (x >= 50%), upper 72%.
                if (_touchCamFingerId == -1 && t.phase == TouchPhase.Began)
                {
                    if (t.position.x >= sw * 0.5f && t.position.y <= sh * 0.72f)
                    {
                        _touchCamFingerId = t.fingerId;
                        _touchCamPrevX    = t.position.x;
                        _touchCamPrevY    = t.position.y;
                    }
                }

                if (t.fingerId != _touchCamFingerId) continue;
                trackedAlive = true;

                float dx = t.position.x - _touchCamPrevX;
                float dy = t.position.y - _touchCamPrevY;
                _touchCamPrevX = t.position.x;
                _touchCamPrevY = t.position.y;

                if (dx == 0f && dy == 0f) continue;

                // Scale matches Sliderotate: raw pixel delta * 0.1, then * sensitivityX via KbmInjectMouseLook.
                // Touch.position.y is bottom-left origin: positive Y = finger moves UP = camera looks UP.
                // KbmInjectMouseLook: positive my ? rotY increases ? cam pitches up. Same sign, no inversion.
                float touchSens = _isAiming ? (_sensNormal * _adsMultiplier) : _sensNormal;
                KbmInjectMouseLook(dx * 0.1f, dy * 0.1f, touchSens);

                // Block Sliderotate from double-processing this touch this frame.
                if (_fiCannotRotate != null) _fiCannotRotate.SetValue(_sliderotate, true);
            }

            // Re-enable Sliderotate next frame if our finger lifted (or there never was one).
            if (!trackedAlive && _touchCamFingerId == -1 && _fiCannotRotate != null)
                _fiCannotRotate.SetValue(_sliderotate, false);
        }

        private void TouchJumpDetect()
        {
            if (_dragGOs[1] == null || _nguiCam == null) return;
            if ((object)PlayerLogic.mInstance == null || PlayerLogic.mInstance.bDied) return;
            if (Input.touchCount == 0) return;

            // Cache JoyStickController and its private fields.
            if ((object)_joyStickCtrl == null)
                _joyStickCtrl = (JoyStickController)UnityEngine.Object.FindObjectOfType(typeof(JoyStickController));
            if ((object)_joyStickCtrl != null)
            {
                if (_fiJumpIsJumping == null)
                    _fiJumpIsJumping = typeof(JoyStickController).GetField(
                        "isJumping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (_fiJumpTime == null)
                    _fiJumpTime = typeof(JoyStickController).GetField(
                        "jumpTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (_fiJumpCharCtrl == null)
                    _fiJumpCharCtrl = typeof(JoyStickController).GetField(
                        "charactercontroller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);

                // Remove ended/cancelled touches from the claimed set.
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _jumpTouchIds.Remove(t.fingerId);
                    continue;
                }

                if (t.phase != TouchPhase.Began &&
                    t.phase != TouchPhase.Stationary &&
                    t.phase != TouchPhase.Moved) continue;

                // For Stationary/Moved, only act if this finger began on the button.
                if (t.phase != TouchPhase.Began && !_jumpTouchIds.Contains(t.fingerId)) continue;

                // Raycast from the NGUI camera through the touch position.
                // This respects the button's actual BoxCollider size (including HUD rescaling).
                Ray ray = _nguiCam.ScreenPointToRay(t.position);
                RaycastHit hit;
                if (t.phase == TouchPhase.Began)
                {
                    // On Began we must raycast to confirm the touch started on the button.
                    if (!Physics.Raycast(ray, out hit, 100f)) continue;
                    Transform hitT = hit.collider.transform;
                    Transform jumpT = _dragGOs[1].transform;
                    bool onJumpButton = false;
                    while (hitT != null)
                    {
                        if (hitT == jumpT) { onJumpButton = true; break; }
                        hitT = hitT.parent;
                    }
                    if (!onJumpButton) continue;
                    _jumpTouchIds.Add(t.fingerId);
                }

                // Hand off to TriggerOwnJump which decides whether to start a new arc.
                TriggerOwnJump();
                break;
            }
        }

        // Starts our custom jump arc if the player is grounded and not already jumping.
        // Called from TouchJumpDetect when a claimed finger is on the button.
        // While in the air (_ownJumpActive=true) this is a no-op; when the player
        // lands and the finger is still held, TouchJumpDetect calls this again and
        // it re-triggers automatically (auto-re-jump on landing).
        private void TriggerOwnJump()
        {
            if (_ownJumpActive) return;
            if ((object)_joyStickCtrl == null || _fiJumpCharCtrl == null) return;
            CharacterController cc = (CharacterController)_fiJumpCharCtrl.GetValue(_joyStickCtrl);
            if (cc == null || !cc.isGrounded) return;

            // Prevent JoyStickController's own arc from ever starting.
            if (_fiJumpIsJumping != null) _fiJumpIsJumping.SetValue(_joyStickCtrl, false);
            PlayerPrefs.SetInt("OnJump", 0);

            _ownJumpVelY   = JumpInitialVel;
            _ownJumpActive = true;
        }

        // Applies our own jump Y velocity each LateUpdate, AFTER JoyStickController
        // has already called cc.Move(horizontal + gravity) in its Update().
        // Unity CharacterController.Move() calls in the same frame are additive, so
        // net Y displacement = (-9.81 + _ownJumpVelY) * dt.
        //
        // Arc: _ownJumpVelY starts at JumpInitialVel (24), decelerates at JumpAscendGrav
        // (-41 m/s�) until net Y crosses zero (peak � 2.5 m), then accelerates down via
        // JumpDescendGrav (-56 m/s�) for a snap-fast landing (~0.3 s fall).
        private void OwnJumpPhysics()
        {
            if (!_ownJumpActive) return;
            if ((object)_joyStickCtrl == null || _fiJumpCharCtrl == null)
            {
                _ownJumpActive = false;
                return;
            }
            CharacterController cc = (CharacterController)_fiJumpCharCtrl.GetValue(_joyStickCtrl);
            if (cc == null) { _ownJumpActive = false; return; }

            // Keep game's arc suppressed every frame.
            if (_fiJumpIsJumping != null) _fiJumpIsJumping.SetValue(_joyStickCtrl, false);
            PlayerPrefs.SetInt("OnJump", 0);

            // Add our Y displacement on top of the game's horizontal+gravity move.
            cc.Move(new Vector3(0f, _ownJumpVelY * Time.deltaTime, 0f));

            // Asymmetric gravity: decelerate quickly on the way up, fall snap-fast.
            float grav = (_ownJumpVelY > 0f) ? JumpAscendGrav : JumpDescendGrav;
            _ownJumpVelY += grav * Time.deltaTime;

            // End on confirmed landing (after our move so isGrounded reflects latest pos).
            if (cc.isGrounded && _ownJumpVelY < 0f)
            {
                _ownJumpActive = false;
                _ownJumpVelY   = 0f;
            }
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

            // Detect fire button press: FpsOnFire pref is toggled (0?1) on every press.
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

        private void ApplyWideCam()
        {
            if (_fiMinY == null || _fiMaxY == null) return;
            float minY = _wideCam ? CAM_MIN_Y_WIDE    : CAM_MIN_Y_DEFAULT;
            float maxY = _wideCam ? CAM_MAX_Y_WIDE    : CAM_MAX_Y_DEFAULT;
            foreach (MonoBehaviour sr in _allSliderotates)
            {
                if (sr == null) continue;
                _fiMinY.SetValue(sr, minY);
                _fiMaxY.SetValue(sr, maxY);
            }
            SettingsModEntry.Log("ApplyWideCam: wide=" + _wideCam + " minY=" + minY + " maxY=" + maxY);
        }

        // (fire detection now uses FpsOnFire pref polling in ApplySensitivity)

        public void ToggleAiming() { _isAiming = !_isAiming; }

        // All Sliderotate instances � patch every one of them
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
                // Prefer active+enabled; fall back to first found
                _sliderotate = null;
                foreach (MonoBehaviour mb in _allSliderotates)
                {
                    if (mb.gameObject.activeInHierarchy && ((Behaviour)mb).enabled)
                    { _sliderotate = mb; break; }
                }
                if (_sliderotate == null) _sliderotate = _allSliderotates[0];
                Type t = _sliderotate.GetType();
                _fiSensX        = t.GetField("sensitivityX",  BindingFlags.Instance | BindingFlags.NonPublic);
                _fiSensY        = t.GetField("sensitivityY",  BindingFlags.Instance | BindingFlags.NonPublic);
                _fiCannotRotate = t.GetField("cannotRotate",  BindingFlags.Instance | BindingFlags.NonPublic);
                _fiMinY         = t.GetField("minimumY",       BindingFlags.Instance | BindingFlags.NonPublic);
                _fiMaxY         = t.GetField("maximumY",       BindingFlags.Instance | BindingFlags.NonPublic);
                _fiRotationX    = t.GetField("rotationX",      BindingFlags.Instance | BindingFlags.NonPublic);
                _fiRotationY    = t.GetField("rotationY",      BindingFlags.Instance | BindingFlags.NonPublic);
                _fiCamTransform = t.GetField("cameratransform",BindingFlags.Instance | BindingFlags.NonPublic);
            }
            SettingsModEntry.Log("CacheSliderotate: total=" + _allSliderotates.Count
                + " fiSensX=" + (_fiSensX != null) + " fiMinY=" + (_fiMinY != null)
                + " fiRotX=" + (_fiRotationX != null));
            CacheAimBtn();
            CacheMainCam();
            // Re-apply any pending wide-cam setting now that Sliderotate is ready
            if (_wideCam) ApplyWideCam();
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

        // NGUI UICamera.Raycast() rejects touches whose ScreenToViewportPoint falls outside
        // [0,1].  If the HUD camera's rect is constrained to the original button region
        // (e.g. right ~65% of screen, with the left reserved for the virtual joystick),
        // buttons dragged into the other region never fire.  Expanding the rect to the full
        // screen costs nothing � the joystick uses VCTouchController, not NGUI, so it is
        // completely unaffected.
        private void ExpandNguiCamToFullScreen()
        {
            CacheNguiCam();
            if (_nguiCam == null) return;
            Rect r = _nguiCam.rect;
            if (r.x > 0.001f || r.y > 0.001f || r.width < 0.999f || r.height < 0.999f)
            {
                _nguiCam.rect = new Rect(0f, 0f, 1f, 1f);
                SettingsModEntry.Log("HUD: NGUI cam viewport expanded to full screen (was " + r + ")");
            }
            // Also ensure the UICamera component on this camera covers the full
            // eventReceiverMask (-1 = all layers) so widgets on any layer get events.
            UICamera uiCam = _nguiCam.GetComponent<UICamera>();
            if (uiCam != null && uiCam.eventReceiverMask != (LayerMask)(-1))
            {
                SettingsModEntry.Log("HUD: UICamera eventReceiverMask was " + (int)uiCam.eventReceiverMask + ", leaving as-is (not -1)");
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
            HudCfgSave();
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
            // Enter edit mode � this calls ReCacheHUD and populates _dragGOs
            EnterHudEditMode();
            yield return null;
            // Apply factory reset to all live items
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                HudCfgDelete(DRAG_ITEMS[i].prefPX);
                HudCfgDelete(DRAG_ITEMS[i].prefPY);
                if (DRAG_ITEMS[i].prefSZ != null) HudCfgDelete(DRAG_ITEMS[i].prefSZ);
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
            HudCfgSave();
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
                    HudCfgSave();
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
            HudCfgSetFloat(DRAG_ITEMS[i].prefPX, lp.x);
            HudCfgSetFloat(DRAG_ITEMS[i].prefPY, lp.y);
        }

        private void LoadDragPos(int i)
        {
            if (_dragGOs[i] == null || !HudCfgHasKey(DRAG_ITEMS[i].prefPX)) return;
            float px = HudCfgGetFloat(DRAG_ITEMS[i].prefPX);
            float py = HudCfgGetFloat(DRAG_ITEMS[i].prefPY);
            Vector3 lp = _dragGOs[i].transform.localPosition;
            _dragGOs[i].transform.localPosition = new Vector3(px, py, lp.z);
        }

        private void SaveDragScale(int i)
        {
            if (DRAG_ITEMS[i].prefSZ == null) return;
            if (_savedScales[i] < 0f) return;
            HudCfgSetFloat(DRAG_ITEMS[i].prefSZ, _savedScales[i]);
            SettingsModEntry.Log("SAVE ratio[" + i + "] " + DRAG_ITEMS[i].displayName + " = " + _savedScales[i].ToString("F4") + " (base x=" + _dragOrigScale[i].x.ToString("F5") + " y=" + _dragOrigScale[i].y.ToString("F5") + ")");
        }

        private void LoadDragScale(int i)
        {
            if (_dragGOs[i] == null || DRAG_ITEMS[i].prefSZ == null) return;
            if (!HudCfgHasKey(DRAG_ITEMS[i].prefSZ)) return;
            if (_dragOrigScale[i] == Vector3.zero) return;
            float ratio = HudCfgGetFloat(DRAG_ITEMS[i].prefSZ);
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
            // KBM debug overlay � disabled
            if (false && _kbmEnabled && _cursorLocked && !_showSettings)
            {
                GUIStyle dbgStyle = new GUIStyle(GUI.skin.box);
                dbgStyle.alignment = TextAnchor.UpperLeft;
                dbgStyle.fontSize  = 22;
                dbgStyle.normal.textColor = Color.yellow;
                string srName = _sliderotate != null ? ((Component)(_sliderotate as Component)).gameObject.name : "NULL";
                string dbgText =
                    "KBM DEBUG\n" +
                    "cap=" + _captureActive + " lock=" + _cursorLocked + " focusChg=" + _winFocusFires + "\n" +
                    "src=" + _dbgSource + " dx=" + _dbgRawDx.ToString("F2") + " dy=" + _dbgRawDy.ToString("F2") + "\n" +
                    "cap.fires=" + (_capListener != null ? _capListener.FireCount.ToString() : "?") + "\n" +
                    "inject.fires=" + _injectFires + " sr=" + srName + "\n" +
                    "eulerY=" + _dbgEulerY.ToString("F1") + "\n" +
                    "proxy.fires=" + _proxyFires + " gml.fires=" + _gmlFires + "\n" +
                    "frame=" + _dbgFrame;
                GUI.Box(new Rect(10, 10, 420, 260), dbgText, dbgStyle);
            }

            // -- Crosshair (KBM cursor-locked, not settings/hud-edit) ----------
            if (_kbmEnabled && _cursorLocked && !_showSettings && !_hudEditMode)
            {
                float cx = Screen.width  * 0.5f;
                float cy = Screen.height * 0.5f;
                GUI.color = _weapOnCooldown ? new Color(1f, 0.15f, 0.15f, 0.90f)
                                            : new Color(1f,  1f,   1f,   0.90f);
                GUI.DrawTexture(new Rect(cx - 1f,  cy - 10f, 2f,  20f), Texture2D.whiteTexture); // vertical bar
                GUI.DrawTexture(new Rect(cx - 10f, cy - 1f,  20f, 2f ), Texture2D.whiteTexture); // horizontal bar
                GUI.DrawTexture(new Rect(cx - 2f,  cy - 2f,  4f,  4f ), Texture2D.whiteTexture); // centre dot
                GUI.color = Color.white;
            }

            // ---- APK update overlay (main menu, outside settings panel) --------
            if (_apkNeedsUpdate && !_apkUpdateDismissed && !_showSettings && !_hudEditMode && _sceneName == "MainMenu")
            {
                if (_gsApkBanner == null)
                {
                    _gsApkBanner = new GUIStyle(GUI.skin.box);
                    _gsApkBanner.fontSize = 13;
                    _gsApkBanner.fontStyle = FontStyle.Bold;
                    _gsApkBanner.normal.textColor = new Color(1f, 0.85f, 0.2f);
                    _gsApkBanner.wordWrap = true;
                    _gsApkBanner.alignment = TextAnchor.MiddleCenter;
                    _gsApkBanner.normal.background = _texApkBannerBg ?? (_texApkBannerBg = MakeTex(2, 2, new Color(0.5f, 0.1f, 0f, 0.92f)));
                }
                float bScale = Screen.width / REF_W;
                GUIUtility.ScaleAroundPivot(new Vector2(bScale, bScale), Vector2.zero);
                float bvw = REF_W;
                float bvh = Screen.height / bScale;
                float bw = Mathf.Min(bvw * 0.88f, 400f);
                float bh = 90f;
                float bx = (bvw - bw) * 0.5f;
                float by = bvh - bh - 20f;
                string apkVer = string.IsNullOrEmpty(_apkVersionName) ? "unknown" : _apkVersionName;
                GUI.Box(new Rect(bx, by, bw, 54f), "APK update required (current: " + apkVer + ")\nGamepad R-stick, D-pad, triggers need the patched APK", _gsApkBanner);
                if (GUI.Button(new Rect(bx, by + 57f, bw * 0.65f - 4f, 28f), "Download APK"))
                    Application.OpenURL("https://play.jacqueb.me/releases/CopsNRobbers-v3.0.2-cnr1.apk");
                if (GUI.Button(new Rect(bx + bw * 0.65f + 4f, by + 57f, bw * 0.35f - 4f, 28f), "Dismiss"))
                    _apkUpdateDismissed = true;
                GUIUtility.ScaleAroundPivot(Vector2.one, Vector2.zero);
            }

            if (!_showSettings && !_hudEditMode)
            {
                return;
            }

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

                if (_gsWinBg == null)
                {
                    _gsWinBg = new GUIStyle(GUI.skin.window);
                    _gsWinBg.normal.background   = _spPanelBack != null
                        ? _spPanelBack
                        : MakeTex(2, 2, new Color(0.10f, 0.10f, 0.12f, 0.97f));
                    _gsWinBg.onNormal.background = _gsWinBg.normal.background;
                    _gsWinBg.fontSize            = 15;
                    if (_gameFont != null) _gsWinBg.font = _gameFont;
                }
                _winRect = GUI.Window(9902, _winRect, DrawSettingsWindow, "  [CNR Mod]  Settings", _gsWinBg);
            }

            // Consume all input events so nothing passes through to NGUI
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                Event.current.Use();
        }

        private void DrawSettingsWindow(int id)
        {
            float pw = _winRect.width - 28f;

            // ---- APK update banner ------------------------------------------
            if (_apkNeedsUpdate && !_apkUpdateDismissed)
            {
                if (_gsApkBanner == null)
                {
                    _gsApkBanner = new GUIStyle(GUI.skin.box);
                    _gsApkBanner.fontSize = 13;
                    _gsApkBanner.fontStyle = FontStyle.Bold;
                    _gsApkBanner.normal.textColor = new Color(1f, 0.85f, 0.2f);
                    _gsApkBanner.wordWrap = true;
                    _gsApkBanner.alignment = TextAnchor.MiddleCenter;
                    _gsApkBanner.normal.background = _texApkBannerBg ?? (_texApkBannerBg = MakeTex(2, 2, new Color(0.5f, 0.1f, 0f, 0.92f)));
                }
                string apkVer = string.IsNullOrEmpty(_apkVersionName) ? "unknown" : _apkVersionName;
                GUILayout.Box("?  APK update required (current: " + apkVer + ")\n   Gamepad axes (R-stick, D-pad, triggers) need the patched APK.", _gsApkBanner, GUILayout.ExpandWidth(true));
                GUILayout.Space(2f);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Download APK", GUILayout.Height(30f)))
                    Application.OpenURL("https://play.jacqueb.me/releases/CopsNRobbers-v3.0.2-cnr1.apk");
                GUILayout.Space(4f);
                if (GUILayout.Button("Dismiss", GUILayout.Width(80f), GUILayout.Height(30f)))
                    _apkUpdateDismissed = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }

            // ---- Tab bar ----
            GUILayout.Space(2f);
            float tbW = (pw - 18f) / 4f;
            GUILayout.BeginHorizontal();
            GUILayout.Space(2f);
            if (GUILayout.Button("Settings", TabBtnStyle(_activeTab == 0), GUILayout.Width(tbW), GUILayout.Height(34f)))
            { if (_activeTab != 0) { _activeTab = 0; _scroll = Vector2.zero; _hudEditNotInGameMsg = false; } }
            GUILayout.Space(4f);
            if (GUILayout.Button("KB+Mouse", TabBtnStyle(_activeTab == 1), GUILayout.Width(tbW), GUILayout.Height(34f)))
            { if (_activeTab != 1) { _activeTab = 1; _scrollKbm = Vector2.zero; } }
            GUILayout.Space(4f);
            if (GUILayout.Button("Controllers", TabBtnStyle(_activeTab == 3), GUILayout.Width(tbW), GUILayout.Height(34f)))
            { if (_activeTab != 3) { _activeTab = 3; _scrollCtrl = Vector2.zero; } }
            GUILayout.Space(4f);
            if (GUILayout.Button("Account", TabBtnStyle(_activeTab == 2), GUILayout.Width(tbW), GUILayout.Height(34f)))
            { if (_activeTab != 2) { _activeTab = 2; _scrollAccount = Vector2.zero; } }
            GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);

            if (_gsVScroll == null) { _gsVScroll = new GUIStyle(GUI.skin.verticalScrollbar); _gsVScroll.fixedWidth = 30f; }
            Vector2 sv = (_activeTab == 0) ? _scroll
                       : (_activeTab == 1) ? _scrollKbm
                       : (_activeTab == 3) ? _scrollCtrl
                       : _scrollAccount;
            sv = GUILayout.BeginScrollView(sv, false, true, GUIStyle.none, _gsVScroll,
                GUILayout.Width(_winRect.width - 4f),
                GUILayout.Height(_winRect.height - 104f));
            if (_activeTab == 0) _scroll = sv;
            else if (_activeTab == 1) _scrollKbm = sv;
            else if (_activeTab == 3) _scrollCtrl = sv;
            else _scrollAccount = sv;
            GUILayout.Space(6f);

            if (_activeTab == 0)
            {
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
            GUILayout.Space(14f);

            // ---- Touch Joystick Deadzone -----------------------------------
            SectionHeader("Touch Joystick");
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Deadzone  [" + (_touchDeadzone * 100f).ToString("F0") + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newTouchDz = DrawSlider(_touchDeadzone, 0f, 0.4f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newTouchDz - _touchDeadzone) > 0.005f)
            {
                _touchDeadzone = Mathf.Round(newTouchDz * 20f) / 20f;
                HudCfgSetFloat("CNRMod_TouchDeadzone", _touchDeadzone);
            }
            GUILayout.Label("  Inputs below this magnitude are ignored (avoids drift)", HintStyle());
            GUILayout.Space(14f);

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

            // ---- Camera Angles ----------------------------------------------
            SectionHeader("Camera Angles");
            GUILayout.Space(4f);
            {
                GUILayout.Space(2f);
                bool clicked = GUILayout.Button(GUIContent.none, GhostBtnStyle(), GUILayout.Height(34f));
                Rect rc = GUILayoutUtility.GetLastRect();
                Texture2D chkTex = _wideCam
                    ? (_spSelectKuang ?? MakeTex(2, 2, Color.white))
                    : (_spPropKuang   ?? MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)));
                GUI.DrawTexture(new Rect(rc.x + 3f, rc.y + 2f, 30f, 30f), chkTex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(rc.x + 39f, rc.y, rc.width - 42f, rc.height), "Wide camera angle", LabelStyle());
                if (clicked)
                {
                    _wideCam = !_wideCam;
                    HudCfgSetInt("CNRMod_WideCam", _wideCam ? 1 : 0);
                    HudCfgSave();
                    if (_sliderotate == null) CacheSliderotate();
                    ApplyWideCam();
                }
            }
            GUILayout.Label("  OFF (default) = game default \u00b135\u00b0\n  ON = extended \u00b170\u00b0 (much more look-up/down freedom)", HintStyle());
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
                    HudCfgSetInt(VIS_ITEMS[i].prefKey, _visOn[i] ? 1 : 0);
                    HudCfgSave();
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
            {
                if (_inGameScene)
                    StartCoroutine(EnterEditModeNextFrame());
                else
                    _hudEditNotInGameMsg = true;
            }
            if (_hudEditNotInGameMsg)
            {
                GUIStyle warnSt = HintStyle(); warnSt.normal.textColor = new Color(1f, 0.6f, 0.3f); warnSt.fontSize = 13;
                GUILayout.Label("  \u26a0 Join a game first, then open Settings to edit the HUD layout.", warnSt);
            }

            GUILayout.Space(14f);

            // ---- Reset All HUD ----------------------------------------------
            if (GUILayout.Button("Reset All HUD to Defaults", BtnStyle(18, new Color(1f, 0.45f, 0.45f))))
            {
                if (_inGameScene)
                    StartCoroutine(ResetHUDViaEditMode());
                else
                {
                    // Not in game scene yet � just wipe prefs; defaults load on next scene entry
                    for (int i = 0; i < DRAG_COUNT; i++)
                    {
                        HudCfgDelete(DRAG_ITEMS[i].prefPX);
                        HudCfgDelete(DRAG_ITEMS[i].prefPY);
                        if (DRAG_ITEMS[i].prefSZ != null) HudCfgDelete(DRAG_ITEMS[i].prefSZ);
                        _savedScales[i]   = -1f;
                        _dragOrigScale[i] = Vector3.zero;
                    }
                    HudCfgSave();
                }
            }
            GUILayout.Space(6f);

            } // end Settings tab
            else if (_activeTab == 1) { DrawKbmTabContent(pw); }
            else if (_activeTab == 3) { DrawControllersTabContent(pw); }
            else { DrawAccountTabContent(pw); }

            GUILayout.Space(6f);
            GUILayout.EndScrollView();

            // ---- Close & Save � pinned to bottom via FlexibleSpace ----------
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("  Close & Save  ", BtnStyle(22, Color.white), GUILayout.Height(38f)))
            {
                _showSettings = false;
                if (_nguiUICam != null) _nguiUICam.enabled = true;
                PlayerPrefs.Save();
                HudCfgSave();
            }
            GUILayout.Space(6f);

            // Keybind capture overlay � drawn on top of everything else
            if (_captureIdx >= 0) DrawCaptureOverlay();
            if (_gpCaptureIdx >= 0) DrawGpCaptureOverlay();
        }

        // =====================================================================
        // EconomyHook reflection bridge (IPRedirectMod.dll not in compile refs)
        // =====================================================================
        private static Type _ecoType     = null;
        private static bool _ecoSearched = false;
        private static Type EcoType()
        {
            if (_ecoSearched) return _ecoType;
            _ecoSearched = true;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            { _ecoType = asm.GetType("CNRMods.EconomyHook"); if (_ecoType != null) break; }
            return _ecoType;
        }
        private static bool EcoGetBool(string field)
        {
            Type t = EcoType(); if (t == null) return false;
            FieldInfo fi = t.GetField(field, BindingFlags.Public | BindingFlags.Static);
            return fi != null && (bool)fi.GetValue(null);
        }
        private static int EcoGetInt(string field)
        {
            Type t = EcoType(); if (t == null) return 0;
            FieldInfo fi = t.GetField(field, BindingFlags.Public | BindingFlags.Static);
            return fi != null ? (int)fi.GetValue(null) : 0;
        }
        private static string EcoGetString(string field)
        {
            Type t = EcoType(); if (t == null) return null;
            FieldInfo fi = t.GetField(field, BindingFlags.Public | BindingFlags.Static);
            return fi != null ? fi.GetValue(null) as string : null;
        }
        private static void EcoSetString(string field, string value)
        {
            Type t = EcoType(); if (t == null) return;
            FieldInfo fi = t.GetField(field, BindingFlags.Public | BindingFlags.Static);
            if (fi != null) fi.SetValue(null, value);
        }
        private static void EcoCallStatic(string method, object[] args)
        {
            Type t = EcoType(); if (t == null) return;
            System.Reflection.MethodInfo mi = t.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (mi != null) mi.Invoke(null, args);
        }
        // =====================================================================
        // Controllers tab UI
        // =====================================================================
        private void DrawControllersTabContent(float pw)
        {
            // ---- Connected Controllers ----------------------------------------
            SectionHeader("Connected Controllers");
            GUILayout.Space(4f);
            string[] joyNames = Input.GetJoystickNames();
            bool anyConnected = false;
            if (joyNames != null)
            {
                for (int ji = 0; ji < joyNames.Length; ji++)
                {
                    if (!string.IsNullOrEmpty(joyNames[ji]))
                    {
                        GUILayout.Label("  \u2022  " + joyNames[ji], LabelStyle());
                        anyConnected = true;
                    }
                }
            }
            if (!anyConnected)
            {
                GUIStyle noCtrl = HintStyle();
                noCtrl.normal.textColor = new Color(1f, 0.55f, 0.55f);
                GUILayout.Label("  No controller detected.  Pair via Bluetooth or USB.", noCtrl);
            }
            GUILayout.Space(14f);

            // ---- Axis Live Test --------------------------------------------
            if (!_joyProxySetup) SetupJoyProxy();
            SectionHeader("Axis Live Test");
            GUILayout.Space(4f);
            GUIStyle axStyle = HintStyle(); axStyle.fontSize = 11;
            if (_joyProxy != null && _joyProxy.HasData)
            {
                GUILayout.Label("  JoyProxy axes (move sticks / triggers / D-pad):", HintStyle());
                string[][] labeled = {
                    new string[]{ "0",  "L-stick X" }, new string[]{ "1",  "L-stick Y" },
                    new string[]{ "11", "R-stick X" }, new string[]{ "14", "R-stick Y" },
                    new string[]{ "15", "DPad X" },    new string[]{ "16", "DPad Y" },
                    new string[]{ "17", "L-Trigger" }, new string[]{ "18", "R-Trigger" } };
                foreach (var pair in labeled)
                {
                    int axId; if (!int.TryParse(pair[0], out axId)) continue;
                    float av = _joyProxy.Get(axId);
                    string lbl = "    " + pair[1].PadRight(14) + ": " + av.ToString("F2") + (Mathf.Abs(av) > 0.3f ? "  \u25c4" : "");
                    GUILayout.Label(lbl, axStyle, GUILayout.Height(17f));
                }
            }
            else
            {
                GUILayout.Label("  Connect a controller \u2014 values appear here once a stick or button is moved.", HintStyle());
            }
            // Unity InputManager axes (Horizontal / Vertical from left stick)
            GUILayout.Space(3f);
            GUIStyle axStyle2 = HintStyle(); axStyle2.fontSize = 10;
            foreach (string an in new string[]{ "Horizontal", "Vertical", "Rotate Camera Horizontal Buttons", "Rotate Camera Vertical Buttons" })
            {
                float av = TryGetAxisRaw(an);
                if (!float.IsNaN(av))
                {
                    string sn = an.Length > 22 ? an.Substring(0, 22) : an;
                    GUILayout.Label("    " + sn.PadRight(22) + ": " + av.ToString("F2") + (Mathf.Abs(av) > 0.1f ? "  \u25c4" : ""), axStyle2, GUILayout.Height(15f));
                }
            }
            GUILayout.Space(10f);

            // ---- Gamepad Enabled --------------------------------------------
            SectionHeader("Gamepad Input");
            GUILayout.Space(4f);
            {
                GUILayout.Space(2f);
                bool clicked = GUILayout.Button(GUIContent.none, GhostBtnStyle(), GUILayout.Height(34f));
                Rect rk = GUILayoutUtility.GetLastRect();
                Texture2D chkTex = _gamepadEnabled
                    ? (_spSelectKuang ?? MakeTex(2, 2, Color.white))
                    : (_spPropKuang   ?? MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)));
                GUI.DrawTexture(new Rect(rk.x + 3f, rk.y + 2f, 30f, 30f), chkTex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(rk.x + 39f, rk.y, rk.width - 42f, rk.height), "Gamepad controls enabled", LabelStyle());
                if (clicked)
                {
                    _gamepadEnabled = !_gamepadEnabled;
                    HudCfgSetInt("CNRMod_GamepadEnabled", _gamepadEnabled ? 1 : 0);
                    HudCfgSave();
                }
            }
            GUILayout.Label("  Left stick = move.  Right stick = camera look (assign axes in Stick Axes below).", HintStyle());
            GUILayout.Space(14f);

            // ---- Deadzone --------------------------------------------------
            SectionHeader("Deadzone");
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Left stick  [" + (_controllerDeadzone * 100f).ToString("F0") + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newCDz = DrawSlider(_controllerDeadzone, 0f, 0.4f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newCDz - _controllerDeadzone) > 0.005f)
            {
                _controllerDeadzone = Mathf.Round(newCDz * 20f) / 20f;
                HudCfgSetFloat("CNRMod_CtrlDeadzone", _controllerDeadzone);
            }
            GUILayout.Label("  Inputs below this magnitude are ignored (avoids stick drift)", HintStyle());
            GUILayout.Space(14f);

            // ---- Sensitivity -----------------------------------------------
            SectionHeader("Sensitivity");
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Left stick  [" + _controllerSens.ToString("F1") + "x]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newCS = DrawSlider(_controllerSens, 0.5f, 3f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newCS - _controllerSens) > 0.05f)
            {
                _controllerSens = Mathf.Round(newCS * 10f) / 10f;
                HudCfgSetFloat("CNRMod_CtrlSens", _controllerSens);
            }
            GUILayout.Label("  Movement scale (1.0 = normal speed)", HintStyle());
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Right stick  [" + _controllerCamSens.ToString("F3") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newRCS = DrawSlider(_controllerCamSens, 0.05f, 3.0f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newRCS - _controllerCamSens) > 0.001f)
            {
                _controllerCamSens = Mathf.Round(newRCS * 200f) / 200f;
                HudCfgSetFloat("CNRMod_CtrlCamSens", _controllerCamSens);
            }
            GUILayout.Label("  Right stick camera speed", HintStyle());
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Falloff  [" + _controllerCamFalloff.ToString("F2") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newFalloff = DrawSlider(_controllerCamFalloff, 1.0f, 3.0f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newFalloff - _controllerCamFalloff) > 0.01f)
            {
                _controllerCamFalloff = Mathf.Round(newFalloff * 20f) / 20f;
                HudCfgSetFloat("CNRMod_CtrlCamFalloff", _controllerCamFalloff);
            }
            GUILayout.Label("  1.0 = linear  2.0+ = slow at low deflection, same max speed", HintStyle());
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Aimed   [" + Mathf.RoundToInt(_controllerAimMult * 100f) + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newCA = DrawSlider(_controllerAimMult, 0.1f, 1.0f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newCA - _controllerAimMult) > 0.005f)
            {
                _controllerAimMult = Mathf.Round(newCA * 20f) / 20f;
                HudCfgSetFloat("CNRMod_CtrlAimMult", _controllerAimMult);
            }
            GUILayout.Label("  % of right-stick speed while scoped", HintStyle());
            GUILayout.Space(14f);

            // ---- Stick Axis Detection ----------------------------------------
            SectionHeader("Stick Axes");
            GUILayout.Space(4f);
            GUILayout.Label("  Tap Detect then slowly push the stick or axis you want to assign.", HintStyle());
            GUILayout.Space(4f);
            string[] stickLabels  = { "L-stick X", "L-stick Y", "R-stick X", "R-stick Y" };
            string[] stickAxisNames = {
                _gpLAxisX ?? ("JA:" + _gpLStickJAX),
                _gpLAxisY ?? ("JA:" + _gpLStickJAY),
                _gpRAxisX ?? ("JA:" + _gpRStickJAX),
                _gpRAxisY ?? ("JA:" + _gpRStickJAY)
            };
            for (int si = 0; si < 4; si++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(stickLabels[si] + "  [" + stickAxisNames[si] + "]", HintStyle(), GUILayout.ExpandWidth(true));
                if (_gpStickDetect == si + 1)
                {
                    if (GUILayout.Button("Cancel", BtnStyle(11, new Color(1f, 0.5f, 0.5f)), GUILayout.Width(pw * 0.24f), GUILayout.Height(26f)))
                        _gpStickDetect = 0;
                }
                else
                {
                    if (GUILayout.Button("Detect", BtnStyle(11, new Color(0.5f, 1f, 0.6f)), GUILayout.Width(pw * 0.24f), GUILayout.Height(26f)))
                    {
                        _gpStickDetect = si + 1;
                        // snapshot baselines so we can detect delta accurately
                        _gpStickDetAxBase = new float[GP_ALL_AXES.Length];
                        for (int ai = 0; ai < GP_ALL_AXES.Length; ai++)
                            _gpStickDetAxBase[ai] = TryGetAxisRaw(GP_ALL_AXES[ai]);
                        _gpStickDetJoyBase = new float[20];
                    }
                }
                GUILayout.EndHorizontal();
                if (_gpStickDetect == si + 1)
                {
                    // Show live values of all Unity axes so user can see what's moving
                    var sbAxes = new System.Text.StringBuilder("  >>> Move axis now:  ");
                    for (int ai = 0; ai < GP_ALL_AXES.Length; ai++)
                    {
                        float v = TryGetAxisRaw(GP_ALL_AXES[ai]);
                        if (!float.IsNaN(v) && Mathf.Abs(v) > 0.05f)
                            sbAxes.Append(GP_ALL_AXES[ai]).Append('=').Append(v.ToString("F2")).Append(' ');
                    }
                    GUILayout.Label(sbAxes.ToString(), HintStyle());
                }
                GUILayout.Space(2f);
            }
            // Detection is handled in Update() via GpStickDetectPoll().
            GUILayout.Space(14f);

            // ---- Button Bindings ------------------------------------------
            SectionHeader("Button Bindings");
            GUILayout.Label("  Tip: triggers and D-pad are axes � move them during Rebind to assign.", HintStyle());
            GUILayout.Space(4f);
            float colName = pw * 0.33f;
            float colKey  = pw * 0.30f;
            if (_gsKeyLabelCtrl == null)
            {
                _gsKeyLabelCtrl = new GUIStyle(GUI.skin.box);
                _gsKeyLabelCtrl.fontSize  = 13;
                _gsKeyLabelCtrl.alignment = TextAnchor.MiddleCenter;
                _gsKeyLabelCtrl.normal.textColor = new Color(1f, 0.9f, 0.4f);
                if (_gameFont != null) _gsKeyLabelCtrl.font = _gameFont;
            }
            for (int i = 0; i < GP_BIND_COUNT; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(32f));
                GUILayout.Label(GP_BIND_NAMES[i], LabelStyle(), GUILayout.Width(colName));
                bool hasAxis = !string.IsNullOrEmpty(_gpAxisBinds[i]);
                string dispLabel = hasAxis ? GpAxisBindLabel(_gpAxisBinds[i]) : GpBtnName(_gpKeys[i]);
                GUILayout.Label(dispLabel, _gsKeyLabelCtrl, GUILayout.Width(colKey), GUILayout.Height(30f));
                // [X] clears axis bind when set
                if (hasAxis)
                {
                    if (GUILayout.Button("X", BtnStyle(12, new Color(1f, 0.5f, 0.5f)), GUILayout.Width(26f), GUILayout.Height(28f)))
                    {
                        _gpAxisBinds[i] = null;
                        HudCfgSet(GP_AXIS_PREF_KEYS[i], "");
                        HudCfgSave();
                    }
                }
                if (GUILayout.Button("Rebind", BtnStyle(12, new Color(0.6f, 0.9f, 1f)), GUILayout.Height(28f)))
                {
                    if (!_joyProxySetup) SetupJoyProxy();
                    _gpCaptureIdx      = i;
                    _gpCaptureCooldown = 4;
                    // snapshot Unity InputManager axes baseline
                    _gpAxisBaseline = new float[GP_ALL_AXES.Length];
                    for (int ai = 0; ai < GP_ALL_AXES.Length; ai++)
                        _gpAxisBaseline[ai] = TryGetAxisRaw(GP_ALL_AXES[ai]);
                    // snapshot JoyProxy axes baseline
                    _gpJoyBaseline = _joyProxy != null ? _joyProxy.Snapshot() : new float[20];
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            GUILayout.Space(6f);
        }
        // =====================================================================
        // Account tab UI
        // =====================================================================
        private void DrawAccountTabContent(float pw)
        {
            SectionHeader("Your Account");
            GUILayout.Space(6f);

            // Show player ID
            string pid = PlayerPrefs.GetString("CNRMod_EcoPlayerId", "");
            string displayId = (pid.Length >= 8) ? pid.Substring(0, 8) + "..." : (pid.Length > 0 ? pid : "(not registered yet)");
            GUILayout.Label("Device ID:  " + displayId, LabelStyle());
            GUILayout.Space(4f);

            bool ready    = EcoGetBool("Ready");
            bool serverUp = EcoGetBool("ServerUp");
            string statusTxt = ready ? "Connected" : (serverUp ? "Syncing..." : "Offline");
            Color statusCol  = ready ? new Color(0.3f, 1f, 0.4f) : (serverUp ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.4f, 0.4f));
            GUIStyle stLbl = LabelStyle(); stLbl.normal.textColor = statusCol;
            GUILayout.Label("Server:  " + statusTxt, stLbl);
            GUILayout.Space(10f);

            // Balances
            if (ready)
            {
                GUILayout.Label("Coins:  " + EcoGetInt("ServerCoins"), LabelStyle());
                GUILayout.Label("Gems:   " + EcoGetInt("ServerGems"),  LabelStyle());
                GUILayout.Space(10f);
            }

            // ---- Set recovery credentials -----------------------------------
            SectionHeader("Set Recovery Credentials");
            GUILayout.Space(4f);
            GUILayout.Label("Set a password (6+ chars) + 4-8 digit PIN to recover your account on a new phone.", HintStyle());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", LabelStyle(), GUILayout.Width(80f));
            _pinPassword = GUILayout.PasswordField(_pinPassword, '*', 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", LabelStyle(), GUILayout.Width(80f));
            _pinInput = GUILayout.TextField(_pinInput, 8, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Save Credentials", BtnStyle(18, new Color(0.4f, 0.8f, 1f))))
            {
                if (_pinPassword.Length < 6)
                {
                    _accountMsg = "Password must be at least 6 characters.";
                }
                else if (_pinInput.Length < 4 || _pinInput.Length > 8)
                {
                    _accountMsg = "PIN must be 4-8 digits.";
                }
                else
                {
                    bool isNum = true;
                    for (int ci = 0; ci < _pinInput.Length; ci++)
                        if (_pinInput[ci] < '0' || _pinInput[ci] > '9') { isNum = false; break; }
                    if (isNum)
                    {
                        EcoCallStatic("RequestSetPin", new object[]{ _pinPassword, _pinInput });
                        _accountMsg = "Credentials saved!";
                        _pinPassword = ""; _pinInput = "";
                    }
                    else _accountMsg = "PIN must be digits only.";
                }
            }
            GUILayout.Space(10f);

            // ---- Transfer account -------------------------------------------
            SectionHeader("Link This Device to Existing Account");
            GUILayout.Space(4f);
            GUILayout.Label("Enter your Display Name, password, and PIN to link this device to your account.", HintStyle());
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", LabelStyle(), GUILayout.Width(80f));
            _claimName = GUILayout.TextField(_claimName, 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", LabelStyle(), GUILayout.Width(80f));
            _claimPassword = GUILayout.PasswordField(_claimPassword, '*', 32, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("PIN:", LabelStyle(), GUILayout.Width(80f));
            _claimPin = GUILayout.PasswordField(_claimPin, '*', 8, GUI.skin.textField, GUILayout.Width(pw - 90f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Link Device", BtnStyle(18, new Color(1f, 0.7f, 0.3f))))
            {
                if (_claimName.Length == 0)
                    _accountMsg = "Enter your display name.";
                else if (_claimPassword.Length < 6)
                    _accountMsg = "Password must be at least 6 characters.";
                else if (_claimPin.Length < 4)
                    _accountMsg = "Enter your PIN (4-8 digits).";
                else
                {
                    EcoCallStatic("RequestClaim", new object[]{ _claimName, _claimPassword, _claimPin });
                    EcoSetString("ClaimResultMsg", null); // clear stale result before new request
                    _accountMsg = "Linking device...";
                    _claimName = ""; _claimPassword = ""; _claimPin = "";
                }
            }
            GUILayout.Space(10f);

            // Pick up result from DoClaim coroutine running in CNRMod
            string claimResult = EcoGetString("ClaimResultMsg");
            if (claimResult != null)
            {
                _accountMsg = claimResult;
                EcoSetString("ClaimResultMsg", null);
            }

            // Status message
            if (_accountMsg.Length > 0)
            {
                GUIStyle msg = new GUIStyle(LabelStyle());
                msg.normal.textColor = new Color(1f, 0.9f, 0.4f);
                msg.wordWrap = true;
                GUILayout.Label(_accountMsg, msg);
            }
        }

        // =====================================================================
        // KBM tab UI
        // =====================================================================
        private void DrawKbmTabContent(float pw)
        {
            // ---- WIP banner -------------------------------------------------
            if (_gsWipBanner == null)
            {
                _gsWipBanner = new GUIStyle(GUI.skin.box);
                _gsWipBanner.fontSize = 13;
                _gsWipBanner.fontStyle = FontStyle.Bold;
                _gsWipBanner.normal.textColor = new Color(1f, 0.85f, 0.2f);
                _gsWipBanner.wordWrap = true;
                _gsWipBanner.alignment = TextAnchor.MiddleCenter;
                _gsWipBanner.normal.background = _texWipBg ?? (_texWipBg = MakeTex(2, 2, new Color(0.35f, 0.2f, 0f, 0.85f)));
            }
            GUILayout.Box("?  Camera mouse-look is not working yet on Android.\n   Other KBM features (keyboard, buttons) are functional.", _gsWipBanner, GUILayout.ExpandWidth(true));
            GUILayout.Space(8f);

            // ---- KBM Enabled ------------------------------------------------
            SectionHeader("Keyboard & Mouse");
            GUILayout.Space(4f);
            {
                GUILayout.Space(2f);
                bool clicked = GUILayout.Button(GUIContent.none, GhostBtnStyle(), GUILayout.Height(34f));
                Rect rk = GUILayoutUtility.GetLastRect();
                Texture2D chkTex = _kbmEnabled
                    ? (_spSelectKuang ?? MakeTex(2, 2, Color.white))
                    : (_spPropKuang   ?? MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f)));
                GUI.DrawTexture(new Rect(rk.x + 3f, rk.y + 2f, 30f, 30f), chkTex, ScaleMode.ScaleToFit);
                GUI.Label(new Rect(rk.x + 39f, rk.y, rk.width - 42f, rk.height), "KBM controls enabled", LabelStyle());
                if (clicked)
                {
                    _kbmEnabled = !_kbmEnabled;
                    HudCfgSetInt("CNRMod_KbmEnabled", _kbmEnabled ? 1 : 0);
                    HudCfgSave();
                    if (!_kbmEnabled) KbmSetCursorLocked(false);
                }
            }
            GUILayout.Label("  Click game view to lock cursor + enable input.\n  Press Esc to release cursor.", HintStyle());
            GUILayout.Space(14f);

            // ---- Mouse Sensitivity ------------------------------------------
            SectionHeader("Mouse Sensitivity");
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Normal  [" + _mouseSensNgl.ToString("F1") + "]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newMS = DrawSlider(_mouseSensNgl, 0.5f, 15f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newMS - _mouseSensNgl) > 0.05f)
            {
                _mouseSensNgl = Mathf.Round(newMS * 10f) / 10f;
                HudCfgSetFloat("CNRMod_MouseSens", _mouseSensNgl);
            }
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Aimed   [" + Mathf.RoundToInt(_mouseAdsMult * 100f) + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newMA = DrawSlider(_mouseAdsMult, 0.1f, 1.0f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newMA - _mouseAdsMult) > 0.005f)
            {
                _mouseAdsMult = Mathf.Round(newMA * 20f) / 20f;
                HudCfgSetFloat("CNRMod_MouseAdsMult", _mouseAdsMult);
            }
            GUILayout.Label("  % of normal mouse sens while scoped", HintStyle());
            GUILayout.Space(14f);

            // ---- Keyboard Deadzone -----------------------------------------
            SectionHeader("Keyboard Deadzone");
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Deadzone  [" + (_kbmDeadzone * 100f).ToString("F0") + "%]", LabelStyle(), GUILayout.Width(pw * 0.42f));
            float newKbmDz = DrawSlider(_kbmDeadzone, 0f, 0.4f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(newKbmDz - _kbmDeadzone) > 0.005f)
            {
                _kbmDeadzone = Mathf.Round(newKbmDz * 20f) / 20f;
                HudCfgSetFloat("CNRMod_KbmDeadzone", _kbmDeadzone);
            }
            GUILayout.Label("  Keyboard joystick inject: axis below this magnitude is zeroed", HintStyle());
            GUILayout.Space(14f);

            // ---- Keybinds ---------------------------------------------------
            SectionHeader("Keybinds");
            GUILayout.Space(4f);
            float colName = pw * 0.35f;
            float colKey  = pw * 0.34f;
            float colBtn  = pw * 0.27f;
            if (_gsKeyLabelKbm == null)
            {
                _gsKeyLabelKbm = new GUIStyle(GUI.skin.box);
                _gsKeyLabelKbm.fontSize  = 14;
                _gsKeyLabelKbm.alignment = TextAnchor.MiddleCenter;
                _gsKeyLabelKbm.normal.textColor = new Color(1f, 0.9f, 0.4f);
                if (_gameFont != null) _gsKeyLabelKbm.font = _gameFont;
            }
            for (int i = 0; i < KBM_BIND_COUNT; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(32f));
                GUILayout.Label(KBM_BIND_NAMES[i], LabelStyle(), GUILayout.Width(colName));
                GUILayout.Label(KbKeyName(_kbKeys[i]), _gsKeyLabelKbm,
                    GUILayout.Width(colKey), GUILayout.Height(30f));
                if (GUILayout.Button("Rebind", BtnStyle(13, new Color(0.6f, 0.9f, 1f)),
                    GUILayout.Width(colBtn), GUILayout.Height(30f)))
                {
                    _captureIdx      = i;
                    _captureCooldown = 4;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            GUILayout.Space(6f);
        }

        private void DrawCaptureOverlay()
        {
            if (_captureCooldown > 0) { _captureCooldown--; return; }

            // Overlay covers the window interior (coords relative to GUI.Window origin)
            float w = _winRect.width;
            float h = _winRect.height;
            GUI.color = new Color(0f, 0f, 0f, 0.84f);
            GUI.DrawTexture(new Rect(0f, 18f, w, h - 18f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle title = new GUIStyle(GUI.skin.label);
            title.fontSize  = 18; title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.normal.textColor = new Color(1f, 0.9f, 0.3f);
            if (_gameFont != null) title.font = _gameFont;
            GUIStyle sub = new GUIStyle(title);
            sub.fontSize = 14; sub.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            float cy = h * 0.40f;
            GUI.Label(new Rect(10f, cy,        w - 20f, 40f), "Binding:  " + KBM_BIND_NAMES[_captureIdx], title);
            GUI.Label(new Rect(10f, cy + 48f,  w - 20f, 28f), "Press any key  \u2014  Esc to cancel", sub);

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                if (e.keyCode == KeyCode.Escape)
                    _captureIdx = -1;
                else
                {
                    _kbKeys[_captureIdx] = e.keyCode;
                    HudCfgSetInt(KBM_PREF_KEYS[_captureIdx], (int)e.keyCode);
                    HudCfgSave();
                    _captureIdx = -1;
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDown)
            {
                KeyCode mk = e.button == 0 ? KeyCode.Mouse0
                           : e.button == 1 ? KeyCode.Mouse1 : KeyCode.Mouse2;
                _kbKeys[_captureIdx] = mk;
                HudCfgSetInt(KBM_PREF_KEYS[_captureIdx], (int)mk);
                HudCfgSave();
                _captureIdx = -1;
                e.Use();
            }
        }

        private static GUIStyle TabBtnStyle(bool active)
        {
            if (active  && _gsTabActive != null) return _gsTabActive;
            if (!active && _gsTabIdle   != null) return _gsTabIdle;
            GUIStyle s = new GUIStyle(GUI.skin.button);
            s.fontSize  = 16;
            s.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            s.normal.textColor = active ? new Color(0.25f, 0.85f, 1f)  : new Color(0.55f, 0.55f, 0.65f);
            s.hover.textColor  = active ? new Color(0.25f, 0.85f, 1f)  : Color.white;
            if (active)
            {
                Texture2D t = _texTabActiveBg ?? (_texTabActiveBg = MakeTex(2, 2, new Color(0.07f, 0.20f, 0.30f, 1f)));
                s.normal.background = t;
                s.hover.background  = t;
            }
            if (_gameFont != null) s.font = _gameFont;
            if (active) _gsTabActive = s; else _gsTabIdle = s;
            return s;
        }

        private static string KbKeyName(KeyCode kc)
        {
            switch (kc)
            {
                case KeyCode.Mouse0:  return "LMB";
                case KeyCode.Mouse1:  return "RMB";
                case KeyCode.Mouse2:  return "MMB";
                case KeyCode.None:    return "---";
                default:              return kc.ToString();
            }
        }

        private static string GpBtnName(KeyCode kc)
        {
            if (kc == KeyCode.None) return "(none)";
            int kcInt = (int)kc;
            // Any-joystick range: JoystickButton0-19 = 330-349
            if (kcInt >= 330 && kcInt <= 349) return GpBtnShort(kcInt - 330);
            // Joystick1Button0-19 = 350-369  (Unity captures these when physical pad detected)
            if (kcInt >= 350 && kcInt <= 369) return GpBtnShort(kcInt - 350);
            // Joystick2-8 (370-509) - unlikely but handle
            if (kcInt >= 370 && kcInt <= 509) return "JBtn" + ((kcInt - 350) % 20);
            return kc.ToString();
        }
        // "JA:17|+" ? "LT",  "JA:15|-" ? "DPad-L",  "Horizontal|-" ? "H-"
        private static string GpAxisBindLabel(string axisBind)
        {
            if (string.IsNullOrEmpty(axisBind)) return "";
            int pipe = axisBind.LastIndexOf('|');
            if (pipe < 0) return axisBind;
            string axName = axisBind.Substring(0, pipe);
            string dir    = axisBind.Substring(pipe + 1);
            if (axName.StartsWith("JA:"))
            {
                switch (axName)
                {
                    case "JA:11": return dir == "+" ? "RS-R" : "RS-L";
                    case "JA:14": return dir == "+" ? "RS-D" : "RS-U";
                    case "JA:15": return dir == "+" ? "DPad-R" : "DPad-L";
                    case "JA:16": return dir == "+" ? "DPad-D" : "DPad-U";
                    case "JA:17": return "LT";
                    case "JA:18": return "RT";
                    default:      return "JA" + axName.Substring(3) + dir;
                }
            }
            if (axName.StartsWith("Joystick Axis "))  axName = "Axis" + axName.Substring("Joystick Axis ".Length);
            else if (axName == "Horizontal")          axName = "H";
            else if (axName == "Vertical")            axName = "V";
            else if (axName.StartsWith("Rotate Camera ")) axName = "RotCam";
            return axName + (dir == "+" ? "+" : "-");
        }
        private static string GpBtnShort(int n)
        {
            switch (n)
            {
                case 0:  return "Btn0 A";
                case 1:  return "Btn1 B";
                case 2:  return "Btn2 X";
                case 3:  return "Btn3 Y";
                case 4:  return "Btn4 LB";
                case 5:  return "Btn5 RB";
                case 6:  return "Btn6 Bk";
                case 7:  return "Btn7 St";
                case 8:  return "Btn8 LS";
                case 9:  return "Btn9 RS";
                default: return "Btn" + n;
            }
        }

        private void DrawGpCaptureOverlay()
        {
            if (_gpCaptureCooldown > 0) return; // still counting down in Update()

            float w = _winRect.width;
            float h = _winRect.height;
            GUI.color = new Color(0f, 0f, 0f, 0.84f);
            GUI.DrawTexture(new Rect(0f, 18f, w, h - 18f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle title = new GUIStyle(GUI.skin.label);
            title.fontSize  = 18; title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.normal.textColor = new Color(1f, 0.9f, 0.3f);
            if (_gameFont != null) title.font = _gameFont;
            GUIStyle sub = new GUIStyle(title);
            sub.fontSize = 14; sub.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            GUIStyle note = new GUIStyle(sub);
            note.fontSize = 12; note.normal.textColor = new Color(0.65f, 0.65f, 0.65f);

            float cy = h * 0.30f;
            GUI.Label(new Rect(10f, cy,        w - 20f, 40f), "Binding:  " + GP_BIND_NAMES[_gpCaptureIdx], title);
            GUI.Label(new Rect(10f, cy + 48f,  w - 20f, 28f), "Press a button  OR  move an axis (trigger / D-pad)", sub);
            GUI.Label(new Rect(10f, cy + 80f,  w - 20f, 22f), "Tap Cancel below  \u2014  or Esc on keyboard", note);

            // On-screen Cancel button (tap-friendly for gamepad users without keyboard)
            if (GUI.Button(new Rect(w * 0.5f - 64f, cy + 112f, 128f, 40f), "Cancel", BtnStyle(15, new Color(1f, 0.4f, 0.4f))))
            {
                _gpCaptureIdx = -1;
                return;
            }

            // ---- Poll axes for threshold-based bindings (triggers, D-pad, sticks) ----
            // Detection is now handled in Update() via GpCaptureAxisPoll() for reliability.

            // ---- Key / button press ----------------------------------------
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                int kcInt = (int)e.keyCode;
                if (e.keyCode == KeyCode.Escape)
                {
                    _gpCaptureIdx = -1;
                }
                else
                {
                    // Normalize Joystick1Button0-19 (350-369) ? JoystickButton0-19 (330-349)
                    KeyCode toStore = (kcInt >= 350 && kcInt <= 369) ? (KeyCode)(kcInt - 20) : e.keyCode;
                    _gpKeys[_gpCaptureIdx] = toStore;
                    // Clear axis bind for this slot (key bind takes priority)
                    _gpAxisBinds[_gpCaptureIdx] = null;
                    HudCfgSet(GP_AXIS_PREF_KEYS[_gpCaptureIdx], "");
                    HudCfgSetInt(GP_PREF_KEYS[_gpCaptureIdx], (int)toStore);
                    HudCfgSave();
                    _gpCaptureIdx = -1;
                }
                e.Use();
            }
        }

        // =====================================================================
        // KBM helpers
        // =====================================================================
        private void KbmSetCursorLocked(bool locked)
        {
            _cursorLocked      = locked;
            _lastMousePosValid = false;
            _amlDx = 0f; _amlDy = 0f;
            if (!locked && _winProxy   != null) _winProxy.ResetAbsPos();
            if (!locked && _gmlProxy   != null) _gmlProxy.ResetAbsPos();
            if (!locked && _hoverProxy != null) _hoverProxy.ResetAbsPos();
            // Release Sliderotate when cursor is unlocked so touch camera works normally
            if (!locked && _sliderotate != null && _fiCannotRotate != null)
                _fiCannotRotate.SetValue(_sliderotate, false);
            Screen.showCursor  = !locked;
            SetAndroidPointerIcon(!locked);
            SetPointerCapture(locked);  // request/release pointer capture tied to lock state
        }

        private void SetPointerCapture(bool capture)
        {
            AndroidJavaClass  player   = null;
            AndroidJavaObject activity = null, window = null, decor = null;
            try
            {
                player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                window   = activity.Call<AndroidJavaObject>("getWindow");
                decor    = window.Call<AndroidJavaObject>("getDecorView");
                // requestPointerCapture / setOnCapturedPointerListener MUST be called on the
                // Android UI thread (ViewRootImpl is only available there).
                // Use the DecorView (FrameLayout) � NOT findFocus() � because SurfaceView
                // (Unity's render surface) does not support requestPointerCapture().
                // Captured events are delivered to the view that requested capture �
                // our OnCapturedPointerListener on the DecorView will receive them.
                if (capture)
                {
                    // Do NOT set _captureActive=true here � the runnable checks hasCap
                    // asynchronously and sets it only if capture is actually granted.
                    // Until then, Input.GetAxis("Mouse X/Y") runs as the primary path.
                    _captureActive = false;
                    if (_capListener != null) _capListener.Reset();
                    var r = new PointerCaptureRunnable(decor, true, _capListener, this);
                    activity.Call("runOnUiThread", r);
                    decor = null;  // ownership transferred to runnable
                    SettingsModEntry.Log("KBM: requestPointerCapture posted to UI thread (decor)");
                }
                else
                {
                    _captureActive = false;
                    if (_capListener != null) _capListener.Reset();
                    var r = new PointerCaptureRunnable(decor, false, null, this);
                    activity.Call("runOnUiThread", r);
                    decor = null;
                    SettingsModEntry.Log("KBM: releasePointerCapture posted to UI thread");
                }
            }
            catch (Exception ex)
            {
                _captureActive = false;
                SettingsModEntry.Log("KBM: SetPointerCapture(" + capture + ") err: " + ex.Message);
            }
            finally
            {
                if (decor    != null) decor.Dispose();
                if (window   != null) window.Dispose();
                if (activity != null) activity.Dispose();
                if (player   != null) player.Dispose();
            }
        }

        // Hide or show the hardware mouse cursor using Android's PointerIcon API (API 24+).
        // Safe to call from Unity's main thread; falls back silently on older API levels.
        private void SetAndroidPointerIcon(bool visible)
        {
            AndroidJavaClass  player   = null;
            AndroidJavaObject activity = null, window = null, decor = null;
            try
            {
                player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                window   = activity.Call<AndroidJavaObject>("getWindow");
                decor    = window.Call<AndroidJavaObject>("getDecorView");
                if (!visible)
                {
                    // PointerIcon.TYPE_NULL (0) = hidden cursor � available API 24+
                    AndroidJavaClass  piClass  = new AndroidJavaClass("android.view.PointerIcon");
                    AndroidJavaObject nullIcon = piClass.CallStatic<AndroidJavaObject>("getSystemIcon", activity, 0);
                    decor.Call("setPointerIcon", nullIcon);
                    nullIcon.Dispose();
                    piClass.Dispose();
                }
                else
                {
                    decor.Call("setPointerIcon", (AndroidJavaObject)null);
                }
            }
            catch (Exception ex)
            {
                SettingsModEntry.Log("SetAndroidPointerIcon(" + visible + ") err: " + ex.Message);
            }
            finally
            {
                if (decor    != null) decor.Dispose();
                if (window   != null) window.Dispose();
                if (activity != null) activity.Dispose();
                if (player   != null) player.Dispose();
            }
        }

        // Wrap the Activity's Window.Callback so we intercept dispatchGenericMotionEvent
        // as a fallback when pointer capture is unavailable.
        // Also registers the CapturedPointerListener for pointer-capture-based input.
        private void KbmRegisterMouseListener()
        {
            AndroidJavaClass  player   = null;
            AndroidJavaObject activity = null, window = null, orig = null, decor = null;
            try
            {
                player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                window   = activity.Call<AndroidJavaObject>("getWindow");
                decor    = window.Call<AndroidJavaObject>("getDecorView");

                // Create CapturedPointerListener; it gets registered on the focused view
                // inside SetPointerCapture(true) (must be the same view that holds capture).
                _capListener = new CapturedPointerListener();
                SettingsModEntry.Log("KBM: CapturedPointerListener created");

                // Also install Window.Callback proxy as fallback (skip if already installed by JoyProxy path)
                if (_winProxy == null)
                {
                    orig = window.Call<AndroidJavaObject>("getCallback");
                    _winProxy = new WindowCallbackProxy(orig, this);
                    orig = null; // ownership transferred to _winProxy
                    window.Call("setCallback", _winProxy);
                    SettingsModEntry.Log("KBM: WindowCallbackProxy installed");
                }
                else
                {
                    SettingsModEntry.Log("KBM: WindowCallbackProxy already installed (by JoyProxy)");
                }

                // View.OnGenericMotionListener on DecorView (catches ACTION_SCROLL etc.).
                _gmlProxy = new GmlProxy(this);
                AndroidJavaObject gmlDecorRef = window.Call<AndroidJavaObject>("getDecorView");
                activity.Call("runOnUiThread", new GmlSetRunnable(gmlDecorRef, _gmlProxy, this));
                SettingsModEntry.Log("KBM: GmlProxy posted to UI thread");

                // *** PRIMARY INPUT PATH ***
                // ACTION_HOVER_MOVE goes through ViewRootImpl.dispatchHoverEvent(), NOT
                // dispatchGenericMotionEvent. So Window.Callback and OnGenericMotionListener
                // are both structurally unreachable for mouse hover.
                // Solution: View.OnHoverListener on the actual Unity render surface
                // (mUnityPlayer or its first child SurfaceView).
                _hoverProxy = new HoverProxy(this);
                try
                {
                    AndroidJavaObject mup = activity.Get<AndroidJavaObject>("mUnityPlayer");
                    AndroidJavaObject hvrTarget;
                    try
                    {
                        int cc = mup.Call<int>("getChildCount");
                        hvrTarget = cc > 0 ? mup.Call<AndroidJavaObject>("getChildAt", 0) : mup;
                    }
                    catch { hvrTarget = mup; }
                    activity.Call("runOnUiThread", new HoverSetRunnable(hvrTarget, _hoverProxy, this));
                    SettingsModEntry.Log("KBM: HoverProxy posted to UI thread (mUnityPlayer surface)");
                }
                catch (Exception hEx)
                {
                    SettingsModEntry.Log("KBM: HoverProxy reg err: " + hEx.Message);
                    // Fallback: try DecorView
                    AndroidJavaObject hvrDecor = window.Call<AndroidJavaObject>("getDecorView");
                    activity.Call("runOnUiThread", new HoverSetRunnable(hvrDecor, _hoverProxy, this));
                    SettingsModEntry.Log("KBM: HoverProxy posted to UI thread (decor fallback)");
                }
            }
            catch (Exception ex)
            {
                SettingsModEntry.Log("KBM: mouse listener reg failed: " + ex.Message);
            }
            finally
            {
                if (decor    != null) decor.Dispose();
                if (window   != null) window.Dispose();
                if (activity != null) activity.Dispose();
                if (player   != null) player.Dispose();
            }
        }

        private IEnumerator AutoLockAfterLoad()
        {
            for (int f = 0; f < 30; f++) yield return null;
            if (_kbmEnabled && _inGameScene && !_showSettings) KbmSetCursorLocked(true);
        }

        private void KbmPressPause()
        {
            GameObject pauseBtn = GameObject.Find("Image Button(Pause)");
            if (pauseBtn != null) pauseBtn.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        }

        private void KbmToggleLeaderboard()
        {
            TapGO(_dragGOs[11]); // Image Button(InnerLeaderboard)
        }

        private void KbmHandleChat()
        {
            if (_chatBarGO == null) _chatBarGO = GameObject.Find("Panel(ChatBar)");
            if (_chatInputGO == null && _chatBarGO != null)
            {
                UIInputForChat uic = _chatBarGO.GetComponentInChildren<UIInputForChat>();
                if (uic != null) _chatInputGO = ((Component)(object)uic).gameObject;
            }
            bool barVisible = _chatBarGO != null && _chatBarGO.transform.localPosition.y >= -10f;
            if (!barVisible)
            {
                GameObject bottomUpBtn = GameObject.Find("Image Button(BottomUp)");
                if (bottomUpBtn != null) bottomUpBtn.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
            }
            StartCoroutine(KbmFocusChatNextFrame());
        }

        private IEnumerator KbmFocusChatNextFrame()
        {
            yield return null;
            yield return null;
            if (_chatInputGO != null) UICamera.selectedObject = _chatInputGO;
            KbmSetCursorLocked(false);
        }

        // Simulate a full NGUI finger tap on a weapon switch button.
        // A real tap fires: OnPress(true) ? OnPress(false) ? OnClick.
        // OnClick is what UIButtonEventKit handles to actually perform the switch.
        private void TapGO(GameObject go)
        {
            if (go == null) return;
            go.SendMessage("OnPress", true,  SendMessageOptions.DontRequireReceiver);
            go.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
            go.SendMessage("OnClick",        SendMessageOptions.DontRequireReceiver);
            // Clear NGUI focus so the button doesn't stay "hot" � prevents the next
            // unrelated input (jump, fire, etc.) from replaying on this button.
            UICamera.hoveredObject  = null;
            UICamera.selectedObject = null;
        }

        private void KbmSwitchWeapon(int dir)
        {
            // dir > 0 = next gun (_dragGOs[15] = Image Button(RightSwitch))
            // dir < 0 = prev gun (_dragGOs[14] = Image Button(LeftSwitch))
            TapGO(dir > 0 ? _dragGOs[15] : _dragGOs[14]);
        }


        private void KbmInjectJoystick()
        {
            float ax = 0f, ay = 0f;
            if (Input.GetKey(_kbKeys[5]) || Input.GetKey(KeyCode.RightArrow)) ax += 1f;
            if (Input.GetKey(_kbKeys[4]) || Input.GetKey(KeyCode.LeftArrow))  ax -= 1f;
            if (Input.GetKey(_kbKeys[2]) || Input.GetKey(KeyCode.UpArrow))    ay += 1f;
            if (Input.GetKey(_kbKeys[3]) || Input.GetKey(KeyCode.DownArrow))  ay -= 1f;

            // No diagonal normalization: inject each axis at full magnitude.
            // The game's joystick reads components independently, so normalizing
            // would reduce forward speed to 70% when also strafing.

            if (_kbmJoystick == null)
            {
                VCAnalogJoystickBase inst = VCAnalogJoystickBase.GetInstance("stick");
                if (inst == null) return;
                _kbmJoystick = (MonoBehaviour)(object)inst;
                Type t = _kbmJoystick.GetType();
                while (t != null && t.Name != "VCAnalogJoystickBase") t = t.BaseType;
                if (t == null) t = _kbmJoystick.GetType();
                _kbmFiDeltaPixels = t.GetField("_deltaPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fiMax = t.GetField("dragDeltaMagnitudeMaxPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiMax != null) _kbmDragMax = (float)fiMax.GetValue(_kbmJoystick);
                SettingsModEntry.Log("KBM: joystick hooked, dragMax=" + _kbmDragMax);
            }

            if (_kbmFiDeltaPixels == null) return;
            float injectX = ax * _kbmDragMax;
            float injectY = ay * _kbmDragMax;

            // Apply deadzone: if keyboard magnitude (0-1) is below threshold, zero it.
            float mag = Mathf.Sqrt(ax * ax + ay * ay);
            if (mag > 0f && mag < _kbmDeadzone)
            {
                injectX = 0f;
                injectY = 0f;
            }
            // JoyStickController.Update() has three branches: |AxisY| > |AxisX| (forward-dominant),
            // |AxisY| < |AxisX| (strafe-dominant), and both < 0.05 (idle).  The equal-axis
            // diagonal case (e.g. W+A with both = �1) hits none of them and skips the moveSpeed
            // multiplication entirely, making diagonal movement almost stationary.
            // Fix: when strafing diagonally, nudge |AxisX| just below |AxisY| so the
            // forward-dominant branch fires and full moveSpeed is applied.
            if (ax != 0f && ay != 0f)
                injectX *= 0.999f;
            _kbmFiDeltaPixels.SetValue(_kbmJoystick, new Vector2(injectX, injectY));
        }

        private void ApplyTouchJoystickDeadzone()
        {
            // Cache joystick + _deltaPixels field the same way KbmInjectJoystick does.
            if (_kbmJoystick == null)
            {
                VCAnalogJoystickBase inst = VCAnalogJoystickBase.GetInstance("stick");
                if (inst == null) return;
                _kbmJoystick = (MonoBehaviour)(object)inst;
                Type t = _kbmJoystick.GetType();
                while (t != null && t.Name != "VCAnalogJoystickBase") t = t.BaseType;
                if (t == null) t = _kbmJoystick.GetType();
                _kbmFiDeltaPixels = t.GetField("_deltaPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fiMax = t.GetField("dragDeltaMagnitudeMaxPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiMax != null) _kbmDragMax = (float)fiMax.GetValue(_kbmJoystick);
            }
            if (_kbmFiDeltaPixels == null || _kbmDragMax <= 0f) return;
            Vector2 dp = (Vector2)_kbmFiDeltaPixels.GetValue(_kbmJoystick);
            // Compare normalised magnitude against deadzone threshold
            float normalised = dp.magnitude / _kbmDragMax;
            if (normalised > 0f && normalised < _touchDeadzone)
                _kbmFiDeltaPixels.SetValue(_kbmJoystick, Vector2.zero);
        }

        // Returns NaN if the axis name doesn't exist in the game's InputManager.
        private static float TryGetAxisRaw(string name)
        {
            try { return Input.GetAxisRaw(name); }
            catch (System.ArgumentException) { return float.NaN; }
        }

        // True if the axis binding for slot idx is currently beyond threshold.
        // Called from Update() every frame while a Rebind capture is active.
        // Checks JoyProxy axes (delta from baseline) then Unity InputManager axes.
        private void GpCaptureAxisPoll()
        {
            int idx = _gpCaptureIdx;
            if (idx < 0) return;

            // --- JoyProxy path (all axes: L-stick, R-stick, triggers, dpad) ---
            if (_joyProxy != null && _gpJoyBaseline != null)
            {
                float[] joyNow = _joyProxy.Snapshot();
                int[] joyAxes = { 0, 1, 11, 14, 15, 16, 17, 18 };
                foreach (int axId in joyAxes)
                {
                    if (axId >= joyNow.Length) continue;
                    float bl = axId < _gpJoyBaseline.Length ? _gpJoyBaseline[axId] : 0f;
                    float delta = joyNow[axId] - bl;
                    if (Mathf.Abs(delta) > 0.30f)
                    {
                        string dir = delta > 0f ? "+" : "-";
                        _gpAxisBinds[idx] = "JA:" + axId + "|" + dir;
                        HudCfgSet(GP_AXIS_PREF_KEYS[idx], _gpAxisBinds[idx]);
                        _gpKeys[idx] = KeyCode.None;
                        HudCfgSetInt(GP_PREF_KEYS[idx], (int)KeyCode.None);
                        HudCfgSave();
                        _gpCaptureIdx = -1;
                        return;
                    }
                }
            }

            // --- Unity InputManager fallback (Horizontal / Vertical from L-stick) ---
            if (_gpAxisBaseline != null)
            {
                for (int ai = 0; ai < GP_ALL_AXES.Length; ai++)
                {
                    string an = GP_ALL_AXES[ai];
                    float cur = TryGetAxisRaw(an);
                    if (float.IsNaN(cur)) continue;
                    float baseline = float.IsNaN(_gpAxisBaseline[ai]) ? 0f : _gpAxisBaseline[ai];
                    float delta = cur - baseline;
                    if (Mathf.Abs(delta) > 0.40f)
                    {
                        string dir = delta > 0f ? "+" : "-";
                        _gpAxisBinds[idx] = an + "|" + dir;
                        HudCfgSet(GP_AXIS_PREF_KEYS[idx], _gpAxisBinds[idx]);
                        _gpKeys[idx] = KeyCode.None;
                        HudCfgSetInt(GP_PREF_KEYS[idx], (int)KeyCode.None);
                        HudCfgSave();
                        _gpCaptureIdx = -1;
                        return;
                    }
                }
            }
        }

        // Called from Update() every frame while the Stick Axes "Detect" mode is active.
        // Polls all GP_ALL_AXES via Unity InputManager (delta-from-baseline) and saves the
        // axis name string directly. JoyProxy path is kept as dead code for now.
        private void GpStickDetectPoll()
        {
            int slot = _gpStickDetect; // 1=LX, 2=LY, 3=RX, 4=RY
            if (slot <= 0) return;

            // --- JoyProxy path (delta from baseline, works for all axes) ---
            if (_joyProxy != null && _gpStickDetJoyBase != null)
            {
                float[] joyNow = _joyProxy.Snapshot();
                int[] excludeLX = { _gpLStickJAY, _gpRStickJAX, _gpRStickJAY };
                int[] excludeLY = { _gpLStickJAX, _gpRStickJAX, _gpRStickJAY };
                int[] excludeRX = { _gpLStickJAX, _gpLStickJAY, _gpRStickJAY };
                int[] excludeRY = { _gpLStickJAX, _gpLStickJAY, _gpRStickJAX };
                int[] excludes  = slot == 1 ? excludeLX
                                : slot == 2 ? excludeLY
                                : slot == 3 ? excludeRX
                                : excludeRY;
                int[] candidates = { 0, 1, 11, 14, 15, 16, 17, 18 };
                foreach (int axId in candidates)
                {
                    bool skip = false;
                    foreach (int ex in excludes) if (ex == axId) { skip = true; break; }
                    if (skip) continue;
                    float bl = axId < _gpStickDetJoyBase.Length ? _gpStickDetJoyBase[axId] : 0f;
                    float delta = joyNow[axId] - bl;
                    if (Mathf.Abs(delta) > 0.30f)
                    {
                        switch (slot)
                        {
                            case 1: _gpLStickJAX = axId; HudCfgSetInt("CNRMod_LStickJAX", axId); break;
                            case 2: _gpLStickJAY = axId; HudCfgSetInt("CNRMod_LStickJAY", axId); break;
                            case 3: _gpRStickJAX = axId; HudCfgSetInt("CNRMod_RStickJAX", axId); break;
                            case 4: _gpRStickJAY = axId; HudCfgSetInt("CNRMod_RStickJAY", axId); break;
                        }
                        HudCfgSave();
                        _gpStickDetect = 0;
                        return;
                    }
                }
            }

            // --- Unity InputManager fallback (L-stick only: maps to JA:0/JA:1) ---
            if (_gpStickDetAxBase != null)
            {
                // Find axis with largest delta above threshold
                int bestAi = -1;
                float bestDelta = 0.35f;
                for (int ai = 0; ai < GP_ALL_AXES.Length; ai++)
                {
                    float cur = TryGetAxisRaw(GP_ALL_AXES[ai]);
                    if (float.IsNaN(cur)) continue;
                    float bl = float.IsNaN(_gpStickDetAxBase[ai]) ? 0f : _gpStickDetAxBase[ai];
                    float d = Mathf.Abs(cur - bl);
                    if (d > bestDelta) { bestDelta = d; bestAi = ai; }
                }
                if (bestAi >= 0)
                {
                    string axName = GP_ALL_AXES[bestAi];
                    switch (slot)
                    {
                        case 1: _gpLAxisX = axName; HudCfgSet("CNRMod_LAxisX", axName); break;
                        case 2: _gpLAxisY = axName; HudCfgSet("CNRMod_LAxisY", axName); break;
                        case 3: _gpRAxisX = axName; HudCfgSet("CNRMod_RAxisX", axName); break;
                        case 4: _gpRAxisY = axName; HudCfgSet("CNRMod_RAxisY", axName); break;
                    }
                    HudCfgSave();
                    _gpStickDetect = 0;
                }
            }
        }

        // Maps known Unity InputManager axis names to Android MotionEvent axis constants.
        // No longer used by detection (detection now saves axis names directly); kept for
        // legacy JoyProxy runtime path only.
        private static int UnityAxisNameToJoystickAxis(string name)
        {
            if (name == "Horizontal") return 0;  // AXIS_X  = left stick X
            if (name == "Vertical")   return 1;  // AXIS_Y  = left stick Y
            return -1;
        }

        private bool GpAxisHeld(int idx)
        {
            if (string.IsNullOrEmpty(_gpAxisBinds[idx])) return false;
            int pipe = _gpAxisBinds[idx].LastIndexOf('|');
            if (pipe < 0) return false;
            string axName = _gpAxisBinds[idx].Substring(0, pipe);
            char   dir    = _gpAxisBinds[idx][pipe + 1];
            float  v;
            if (axName.StartsWith("JA:"))
            {
                int axId;
                v = (int.TryParse(axName.Substring(3), out axId) && _joyProxy != null)
                    ? _joyProxy.Get(axId) : 0f;
            }
            else
            {
                v = TryGetAxisRaw(axName);
                if (float.IsNaN(v)) return false;
            }
            return dir == '+' ? (v > 0.5f) : (v < -0.5f);
        }

        // Null-safe read from JoyMotionProxy axes.
        private float JoyRaw(int axis) { return _joyProxy != null ? _joyProxy.Get(axis) : 0f; }

        // Ensure Window.Callback proxy is installed (needed even when KBM is off,
        // so WindowCallbackProxy.dispatchGenericMotionEvent can feed _joyProxy).
        private void EnsureWindowCallback()
        {
            if (_winProxy != null) return;
            AndroidJavaClass  player   = null;
            AndroidJavaObject activity = null, window = null, orig = null;
            try
            {
                player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                window   = activity.Call<AndroidJavaObject>("getWindow");
                orig     = window.Call<AndroidJavaObject>("getCallback");
                _winProxy = new WindowCallbackProxy(orig, this);
                orig = null; // ownership transferred to _winProxy
                window.Call("setCallback", _winProxy);
                SettingsModEntry.Log("JoyProxy: EnsureWindowCallback installed");
            }
            catch (Exception ex)
            {
                SettingsModEntry.Log("EnsureWindowCallback: " + ex.Message);
            }
            finally
            {
                if (orig     != null) orig.Dispose();
                if (window   != null) window.Dispose();
                if (activity != null) activity.Dispose();
                if (player   != null) player.Dispose();
            }
        }

        // Register JoyMotionProxy as OnGenericMotionListener on Unity's render surface.
        // Also ensures WindowCallbackProxy is installed as a reliable feed-through path.
        // Returns false so Unity still receives all joystick events normally.
        private void SetupJoyProxy()
        {
            if (_joyProxySetup) return;
            _joyProxySetup = true;
            _joyProxy = new JoyMotionProxy(); // create before EnsureWindowCallback so Feed() works
            EnsureWindowCallback(); // feed path via Window.Callback
            AndroidJavaClass  player   = null;
            AndroidJavaObject activity = null, window = null;
            try
            {
                player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                window   = activity.Call<AndroidJavaObject>("getWindow");
                // Joystick events are dispatched to the focused view (mUnityPlayer / SurfaceView),
                // NOT to DecorView. Register on the same target as HoverProxy.
                AndroidJavaObject regTarget = null;
                try
                {
                    AndroidJavaObject mup = activity.Get<AndroidJavaObject>("mUnityPlayer");
                    try
                    {
                        int cc = mup.Call<int>("getChildCount");
                        if (cc > 0)
                        {
                            regTarget = mup.Call<AndroidJavaObject>("getChildAt", 0);
                            mup.Dispose();
                        }
                        else regTarget = mup; // use mUnityPlayer itself
                    }
                    catch { regTarget = mup; }
                }
                catch
                {
                    regTarget = window.Call<AndroidJavaObject>("getDecorView"); // last resort
                }
                activity.Call("runOnUiThread", new JoyProxySetRunnable(regTarget, _joyProxy));
                // regTarget ownership transferred to runnable
            }
            catch (Exception ex)
            {
                SettingsModEntry.Log("SetupJoyProxy: " + ex.Message);
            }
            finally
            {
                if (window   != null) window.Dispose();
                if (activity != null) activity.Dispose();
                if (player   != null) player.Dispose();
            }
        }

        private void ProbeGpAxes()        {
            _gpAxesProbed = true;
            foreach (string n in GP_LAXIS_X) { float v = TryGetAxisRaw(n); if (!float.IsNaN(v)) { _gpLAxisX = n; break; } }
            foreach (string n in GP_LAXIS_Y) { float v = TryGetAxisRaw(n); if (!float.IsNaN(v)) { _gpLAxisY = n; break; } }
            // R-stick axes come from user config (Detect buttons in Controllers tab) � loaded from prefs.
            SettingsModEntry.Log("GP axes probed: LX=" + (_gpLAxisX ?? "?") + " LY=" + (_gpLAxisY ?? "?")
                + "  RX=" + (_gpRAxisX ?? "?") + " RY=" + (_gpRAxisY ?? "?"));
        }

        private void GamepadUpdate()
        {
            if (!_joyProxySetup) SetupJoyProxy();
            if (!_gpAxesProbed) ProbeGpAxes();

            // ---- Movement inject (left stick ? VCAnalogJoystickBase) --------
            if (_kbmJoystick == null)
            {
                VCAnalogJoystickBase inst = VCAnalogJoystickBase.GetInstance("stick");
                if (inst == null) return;
                _kbmJoystick = (MonoBehaviour)(object)inst;
                Type t = _kbmJoystick.GetType();
                while (t != null && t.Name != "VCAnalogJoystickBase") t = t.BaseType;
                if (t == null) t = _kbmJoystick.GetType();
                _kbmFiDeltaPixels = t.GetField("_deltaPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo fiMax = t.GetField("dragDeltaMagnitudeMaxPixels",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiMax != null) _kbmDragMax = (float)fiMax.GetValue(_kbmJoystick);
                SettingsModEntry.Log("GamepadUpdate: joystick hooked, dragMax=" + _kbmDragMax);
            }
            if (_kbmFiDeltaPixels != null)
            {
                float ax, ay;
                // Prefer JoyProxy AXIS_X/Y (bypasses Unity InputManager limitations)
                if (_joyProxy != null && _joyProxy.HasData)
                {
                    ax =  _joyProxy.Get(_gpLStickJAX);
                    ay = -_joyProxy.Get(_gpLStickJAY); // Y is inverted: push up = positive Y in Unity
                }
                else if (_gpLAxisX != null && _gpLAxisY != null)
                {
                    ax = TryGetAxisRaw(_gpLAxisX);
                    ay = TryGetAxisRaw(_gpLAxisY);
                    if (float.IsNaN(ax)) ax = 0f;
                    if (float.IsNaN(ay)) ay = 0f;
                }
                else { ax = 0f; ay = 0f; }
                float mag = Mathf.Sqrt(ax * ax + ay * ay);
                if (mag > 0f && mag < _controllerDeadzone) { ax = 0f; ay = 0f; }
                float injectX = ax * _kbmDragMax * _controllerSens;
                float injectY = ay * _kbmDragMax * _controllerSens;
                if (Mathf.Abs(injectX) > _kbmDragMax) injectX = Mathf.Sign(injectX) * _kbmDragMax;
                if (Mathf.Abs(injectY) > _kbmDragMax) injectY = Mathf.Sign(injectY) * _kbmDragMax;
                if (ax != 0f && ay != 0f) injectX *= 0.999f; // diagonal fix
                _kbmFiDeltaPixels.SetValue(_kbmJoystick, new Vector2(injectX, injectY));
            }

            // ---- Right stick ? camera (Unity axis names set by Detect) --------
            if (_gpRAxisX != null || _joyProxy != null)
            {
                if (_sliderotate == null) CacheSliderotate();
                if (_sliderotate != null)
                {
                    float rx, ry;
                    if (_gpRAxisX != null)
                    {
                        rx =  TryGetAxisRaw(_gpRAxisX); if (float.IsNaN(rx)) rx = 0f;
                        string ryName = _gpRAxisY ?? _gpRAxisX;
                        ry = -(TryGetAxisRaw(ryName)); if (float.IsNaN(ry)) ry = 0f;
                    }
                    else
                    {
                        rx =  JoyRaw(_gpRStickJAX);
                        ry = -JoyRaw(_gpRStickJAY);
                    }
                    // invert: stick up = look up
                    float rmag = Mathf.Sqrt(rx * rx + ry * ry);
                    if (rmag > _controllerDeadzone)
                    {
                        // Apply power curve to radial magnitude so the direction is preserved
                        // and full deflection (rmag=1) still produces the same max speed.
                        // falloff 1.0 = linear (identical to old rx*camSens); 2.0+ = slow start.
                        float curved = Mathf.Pow(rmag, _controllerCamFalloff);
                        float camSens = _isAiming ? (_controllerCamSens * _controllerAimMult) : _controllerCamSens;
                        KbmInjectMouseLook(rx / rmag * curved * camSens, ry / rmag * curved * camSens);
                    }
                }
            }

            // When no finger is on the screen, clear NGUI's hover target every frame.
            // A real touchscreen tap sets UICamera.hoveredObject to the tapped button and NGUI
            // never clears it on lift � so the next controller input (jump, fire, etc.) blindly
            // re-fires on that button.  Clearing here when touchCount==0 breaks that stale link.
            if (Input.touchCount == 0)
            {
                UICamera.hoveredObject  = null;
                UICamera.selectedObject = null;
            }

            // Snapshot current axis-held state before running actions (for rising-edge detection).
            // Done here � before the PlayerLogic guard � so CNR-mode actions can read it too.
            bool[] axisNow = new bool[GP_BIND_COUNT];
            for (int bi = 0; bi < GP_BIND_COUNT; bi++) axisNow[bi] = GpAxisHeld(bi);

            // ---- Fire (index 0, held) � works in both single-player AND CNR mode ----
            // OnPress(true) only on rising edge, OnPress(false) on falling edge � prevents
            // the NGUI button from getting stuck in a permanently-pressed state.
            // m_bFire/fireFlag are set every frame while held so auto-fire weapons keep firing.
            bool fireDown = (_gpKeys[0] != KeyCode.None && Input.GetKeyDown(_gpKeys[0])) || (axisNow[0] && !_gpAxisPrevHeld[0]);
            bool fireUp   = (_gpKeys[0] != KeyCode.None && Input.GetKeyUp(_gpKeys[0]))   || (!axisNow[0] && _gpAxisPrevHeld[0]);
            bool fireHeld = (_gpKeys[0] != KeyCode.None && Input.GetKey(_gpKeys[0]))     || axisNow[0];
            if (fireDown && _dragGOs[0] != null)
                _dragGOs[0].SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
            if (fireUp && _dragGOs[0] != null)
                _dragGOs[0].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
            if (fireHeld)
            {
                if ((object)UIMenuDirector.mInstance != null)
                    UIMenuDirector.mInstance.GenFireEvent();
                if ((object)CRInput.mInstance != null)
                    CRInput.mInstance.m_bFire = true;
                if ((object)CRJoyStickController.mInstance != null)
                    CRJoyStickController.mInstance.fireFlag = true;
            }

            // ---- Weapon switch (index 2 = next, 3 = prev, down) � works in CNR mode ----
            // Click the on-screen button + call KbmSwitchWeapon directly (same belt-and-suspenders
            // pattern as fire: SendMessage hits any handlers on the GO; KbmSwitchWeapon sets
            // CRInput.m_bSwitch + m_PropIconName which is the real CNR weapon-swap mechanism).
            if ((_gpKeys[2] != KeyCode.None && Input.GetKeyDown(_gpKeys[2])) || (axisNow[2] && !_gpAxisPrevHeld[2]))
            {
                if (_dragGOs[15] != null)  // Index 15 = Next gun button
                {
                    _dragGOs[15].SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                    _dragGOs[15].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
                KbmSwitchWeapon(+1);
            }
            if ((_gpKeys[3] != KeyCode.None && Input.GetKeyDown(_gpKeys[3])) || (axisNow[3] && !_gpAxisPrevHeld[3]))
            {
                if (_dragGOs[14] != null)  // Index 14 = Prev gun button
                {
                    _dragGOs[14].SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
                    _dragGOs[14].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
                }
                KbmSwitchWeapon(-1);
            }

            // Actions below require single-player (PlayerLogic) to be active.
            if ((object)PlayerLogic.mInstance == null || PlayerLogic.mInstance.bDied)
            {
                for (int bi = 0; bi < GP_BIND_COUNT; bi++) _gpAxisPrevHeld[bi] = axisNow[bi];
                return;
            }

            // ---- Jump (index 1, down) ----------------------------------------
            bool jumpDown = (_gpKeys[1] != KeyCode.None && Input.GetKeyDown(_gpKeys[1]))
                         || (axisNow[1] && !_gpAxisPrevHeld[1]);
            if (jumpDown)
            {
                if ((object)_joyStickCtrl == null)
                    _joyStickCtrl = (JoyStickController)UnityEngine.Object.FindObjectOfType(typeof(JoyStickController));
                if ((object)_joyStickCtrl != null)
                {
                    if (_fiJumpIsJumping == null)
                        _fiJumpIsJumping = typeof(JoyStickController).GetField("isJumping",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (_fiJumpCharCtrl == null)
                        _fiJumpCharCtrl = typeof(JoyStickController).GetField("charactercontroller",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                TriggerOwnJump();
            }

            // ---- Aim (index 4, down) -----------------------------------------
            if ((_gpKeys[4] != KeyCode.None && Input.GetKeyDown(_gpKeys[4])) || (axisNow[4] && !_gpAxisPrevHeld[4]))
                PlayerPrefs.SetInt("OnAim", 1);

            // ---- Pause (index 5, down) ---------------------------------------
            if ((_gpKeys[5] != KeyCode.None && Input.GetKeyDown(_gpKeys[5])) || (axisNow[5] && !_gpAxisPrevHeld[5]))
                KbmPressPause();

            // ---- Reload (index 6, down) --------------------------------------
            if ((_gpKeys[6] != KeyCode.None && Input.GetKeyDown(_gpKeys[6])) || (axisNow[6] && !_gpAxisPrevHeld[6]))
                PlayerPrefs.SetInt("FpsReload", 1);

            // ---- Player list (index 7, down) ---------------------------------
            if ((_gpKeys[7] != KeyCode.None && Input.GetKeyDown(_gpKeys[7])) || (axisNow[7] && !_gpAxisPrevHeld[7]))
                KbmToggleLeaderboard();

            // ---- Chat (index 8, down) ----------------------------------------
            if ((_gpKeys[8] != KeyCode.None && Input.GetKeyDown(_gpKeys[8])) || (axisNow[8] && !_gpAxisPrevHeld[8]))
                KbmHandleChat();

            // Update prev-held state for next frame's rising-edge detection
            for (int bi = 0; bi < GP_BIND_COUNT; bi++) _gpAxisPrevHeld[bi] = axisNow[bi];
        }

        private float _kbmInjectLogTimer = 0f;
        // sensOverride >= 0: use that value directly instead of the KBM mouse sensitivity.
        // Touch camera passes _sensNormal so it obeys the touch sensitivity slider, not the KBM one.
        private void KbmInjectMouseLook(float mx, float my, float sensOverride = -1f)
        {
            if (_sliderotate == null) CacheSliderotate();
            // Re-cache if we've latched onto an inactive GO (e.g. stale prefab or respawn)
            if (_sliderotate != null && !((Component)(_sliderotate as Component)).gameObject.activeInHierarchy)
            {
                _sliderotate = null;
                CacheSliderotate();
            }
            if (_sliderotate == null || _fiRotationX == null) return;

            float sens = sensOverride >= 0f ? sensOverride
                       : (_isAiming ? (_mouseSensNgl * _mouseAdsMult) : _mouseSensNgl);

            // Mirror what Sliderotate.Update() does: base horizontal from transform, NOT the
            // private rotationX field.  The field is only updated when a touch is active;
            // reading it when no touch has occurred gives 0 (stale), causing a snap to 0�.
            Component srComp = _sliderotate as Component;
            float rotX = srComp.transform.localEulerAngles.y + mx * sens;
            float rotY = (float)_fiRotationY.GetValue(_sliderotate) + my * sens;
            float minY = (_fiMinY != null) ? (float)_fiMinY.GetValue(_sliderotate) : -35f;
            float maxY = (_fiMaxY != null) ? (float)_fiMaxY.GetValue(_sliderotate) : 35f;
            rotY = Mathf.Clamp(rotY, minY, maxY);

            _injectFires++;
            _fiRotationX.SetValue(_sliderotate, rotX);
            _fiRotationY.SetValue(_sliderotate, rotY);
            srComp.transform.localEulerAngles = new Vector3(0f, rotX, 0f);
            _dbgEulerY = srComp.transform.localEulerAngles.y;
            if (_fiCamTransform != null)
            {
                Transform camT = (Transform)_fiCamTransform.GetValue(_sliderotate);
                if (camT != null) camT.localEulerAngles = new Vector3(-rotY, 0f, 0f);
            }

            // Diagnostic: log once per second so we can confirm injection is reaching here
            _kbmInjectLogTimer -= Time.deltaTime;
            if (_kbmInjectLogTimer <= 0f)
            {
                _kbmInjectLogTimer = 1f;
                Transform camT2 = _fiCamTransform != null ? (Transform)_fiCamTransform.GetValue(_sliderotate) : null;
                SettingsModEntry.Log("KBM inject: mx=" + mx.ToString("F2") + " my=" + my.ToString("F2")
                    + " sens=" + sens.ToString("F2")
                    + " rotX=" + rotX.ToString("F1") + " eulerY=" + srComp.transform.localEulerAngles.y.ToString("F1")
                    + " rotY=" + rotY.ToString("F1")
                    + " GO=" + srComp.gameObject.name
                    + " active=" + srComp.gameObject.activeInHierarchy
                    + " cam=" + (camT2 != null ? camT2.gameObject.name : "NULL"));
            }
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
                        HudCfgDelete(DRAG_ITEMS[i].prefPX);
                        HudCfgDelete(DRAG_ITEMS[i].prefPY);
                        if (DRAG_ITEMS[i].prefSZ != null) HudCfgDelete(DRAG_ITEMS[i].prefSZ);
                        HudCfgSave();
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
            if (_gsLabel != null) return _gsLabel;
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 15; s.normal.textColor = Color.white;
            if (_gameFont != null) s.font = _gameFont;
            return _gsLabel = s;
        }
        private static GUIStyle HintStyle()
        {
            if (_gsHint != null) return _gsHint;
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 11; s.wordWrap = true;
            s.normal.textColor = new Color(0.72f, 0.72f, 0.72f);
            if (_gameFont != null) s.font = _gameFont;
            return _gsHint = s;
        }
        private static GUIStyle GhostBtnStyle()
        {
            if (_gsGhostBtn != null) return _gsGhostBtn;
            GUIStyle s = new GUIStyle();
            s.normal.background = null;
            s.hover.background  = _texGhostHover  ?? (_texGhostHover  = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.08f)));
            s.active.background = _texGhostActive ?? (_texGhostActive = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.16f)));
            return _gsGhostBtn = s;
        }
        private static GUIStyle BtnStyle(int fontSize = 20, Color textColor = default(Color))
        {
            if (textColor == default(Color)) textColor = Color.white;
            // 20-bit packed key: upper 8=fontSize, lower 12=quantised RGB
            long key = ((long)fontSize << 24)
                     | ((long)(textColor.r * 15f + 0.5f) << 16)
                     | ((long)(textColor.g * 15f + 0.5f) << 8)
                     |  (long)(textColor.b * 15f + 0.5f);
            GUIStyle cached;
            if (_gsBtnCache.TryGetValue(key, out cached)) return cached;
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
            _gsBtnCache[key] = s;
            return s;
        }
        // Draw a slider using invisible IMGUI input + manual texture paint
        private static float DrawSlider(float val, float min, float max)
        {
            const float thumbW = 30f;
            const float height = 33f;
            // Invisible styles -- input only, no drawing (cached to avoid per-frame allocation)
            if (_gsInvisBg == null)    { _gsInvisBg    = new GUIStyle(); _gsInvisBg.fixedHeight    = height; }
            if (_gsInvisThumb == null) { _gsInvisThumb = new GUIStyle(); _gsInvisThumb.fixedWidth  = thumbW; _gsInvisThumb.fixedHeight = height; }
            float newVal = GUILayout.HorizontalSlider(val, min, max, _gsInvisBg, _gsInvisThumb, GUILayout.Height(height));
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
            if (_gsSectionHdr == null)
            {
                _gsSectionHdr = new GUIStyle(GUI.skin.label);
                _gsSectionHdr.fontStyle = FontStyle.Bold; _gsSectionHdr.fontSize = 16;
                _gsSectionHdr.normal.textColor = new Color(1f, 0.85f, 0.3f);
                if (_gameFont != null) _gsSectionHdr.font = _gameFont;
            }
            GUILayout.Label("--  " + title + "  --", _gsSectionHdr);
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

            // Camera viewport expansion is intentionally NOT done here.
            // Expanding the NGUI camera rect (e.g. to full screen) changes the camera's
            // projection matrix, which causes Camera.ScreenPointToRay() to produce a
            // different ray for the same screen position.  JoyStickController uses that
            // ray with Physics.Raycast to detect the FireButton collider � when the rect
            // is expanded the ray misses and one-finger fire stops working entirely.
            // Consequence: NGUI buttons dragged to the left side of the screen (outside
            // the original camera viewport) will not respond to touches.  Acceptable
            // trade-off; the drag editor will constrain buttons to the right half.
            CacheNguiCam();
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

        // =====================================================================
        // JoyMotionProxy � View.OnGenericMotionEventListener on the DecorView.
        // Intercepts SOURCE_JOYSTICK MotionEvents to read right stick / trigger /
        // dpad axes that are NOT mapped in the game's InputManager and therefore
        // inaccessible via Input.GetAxisRaw().  Returns false so Unity still receives
        // the event and processes button presses normally.
        // =====================================================================
        private class JoyMotionProxy : AndroidJavaProxy
        {
            private readonly object _lk = new object();
            private readonly float[] _ax = new float[20];
            private bool _hasData;

            internal JoyMotionProxy() : base("android.view.View$OnGenericMotionListener") { }

            // Called on Android UI thread by View.dispatchGenericMotionEvent.
            bool onGenericMotion(AndroidJavaObject v, AndroidJavaObject e)
            {
                try
                {
                    int src = e.Call<int>("getSource");
                    // Accept SOURCE_JOYSTICK (0x01000010) or SOURCE_GAMEPAD class bit (0x00000400)
                    if ((src & 0x01000010) != 0 || (src & 0x00000400) != 0)
                    {
                        lock (_lk)
                        {
                            _ax[0]  = e.Call<float>("getAxisValue", 0);  // AXIS_X      left stick X
                            _ax[1]  = e.Call<float>("getAxisValue", 1);  // AXIS_Y      left stick Y
                            _ax[11] = e.Call<float>("getAxisValue", 11); // AXIS_Z      right stick X
                            _ax[14] = e.Call<float>("getAxisValue", 14); // AXIS_RZ     right stick Y
                            _ax[15] = e.Call<float>("getAxisValue", 15); // AXIS_HAT_X  dpad X
                            _ax[16] = e.Call<float>("getAxisValue", 16); // AXIS_HAT_Y  dpad Y
                            _ax[17] = e.Call<float>("getAxisValue", 17); // AXIS_LTRIGGER
                            _ax[18] = e.Call<float>("getAxisValue", 18); // AXIS_RTRIGGER
                            _hasData = true;
                        }
                    }
                }
                catch { }
                return false; // don't consume: Unity still sees the event
            }

            internal float Get(int axis)
            {
                if (!_hasData || axis < 0 || axis >= _ax.Length) return 0f;
                lock (_lk) { return _ax[axis]; }
            }
            internal bool HasData { get { return _hasData; } }
            internal float[] Snapshot()
            {
                var copy = new float[_ax.Length];
                lock (_lk) { System.Array.Copy(_ax, copy, _ax.Length); }
                return copy;
            }

            // Secondary feed path: called from WindowCallbackProxy.dispatchGenericMotionEvent
            // so we get joystick data even if setOnGenericMotionListener was overwritten.
            internal void Feed(AndroidJavaObject ev)
            {
                try
                {
                    int src = ev.Call<int>("getSource");
                    // Accept SOURCE_JOYSTICK (0x01000010) or SOURCE_GAMEPAD class bit (0x00000400)
                    if ((src & 0x01000010) == 0 && (src & 0x00000400) == 0) return;
                    lock (_lk)
                    {
                        _ax[0]  = ev.Call<float>("getAxisValue", 0);
                        _ax[1]  = ev.Call<float>("getAxisValue", 1);
                        _ax[11] = ev.Call<float>("getAxisValue", 11);
                        _ax[14] = ev.Call<float>("getAxisValue", 14);
                        _ax[15] = ev.Call<float>("getAxisValue", 15);
                        _ax[16] = ev.Call<float>("getAxisValue", 16);
                        _ax[17] = ev.Call<float>("getAxisValue", 17);
                        _ax[18] = ev.Call<float>("getAxisValue", 18);
                        _hasData = true;
                    }
                }
                catch { }
            }
        }

        private class JoyProxySetRunnable : AndroidJavaProxy
        {
            private readonly AndroidJavaObject _view;
            private readonly JoyMotionProxy    _proxy;
            internal JoyProxySetRunnable(AndroidJavaObject view, JoyMotionProxy proxy)
                : base("java.lang.Runnable") { _view = view; _proxy = proxy; }
            public void run()
            {
                try   { _view.Call("setOnGenericMotionListener", _proxy);
                        SettingsModEntry.Log("JoyProxy: registered on " + _view.Call<AndroidJavaObject>("getClass").Call<string>("getSimpleName")); }
                catch (Exception ex) { SettingsModEntry.Log("JoyProxy reg: " + ex.Message); }
                finally { _view.Dispose(); }
            }
        }

        // =====================================================================
        // HoverProxy � View.OnHoverListener on Unity's render surface.
        // ACTION_HOVER_MOVE goes through ViewRootImpl.dispatchHoverEvent(), NOT
        // dispatchGenericMotionEvent � so Window.Callback and OnGenericMotionListener
        // are both dead for mouse hover.  OnHoverListener on the actual hovered view
        // (mUnityPlayer or its SurfaceView child) intercepts before Unity sees it.
        // Returns false so Unity still processes hover (needed for NGUI + cursor pos).
        // =====================================================================
        private class HoverProxy : AndroidJavaProxy
        {
            private const int ACTION_HOVER_MOVE = 7;
            private readonly SettingsModHook _host;
            private float _lastAbsX = float.MinValue;
            private float _lastAbsY = float.MinValue;

            public HoverProxy(SettingsModHook host)
                : base("android.view.View$OnHoverListener") { _host = host; }

            public void ResetAbsPos() { _lastAbsX = float.MinValue; _lastAbsY = float.MinValue; }

            bool onHover(AndroidJavaObject view, AndroidJavaObject ev)
            {
                _host._hvrFires++;
                try
                {
                    int action = ev.Call<int>("getActionMasked");
                    if (action == ACTION_HOVER_MOVE)
                    {
                        // AXIS_RELATIVE only non-zero when pointer capture active
                        float rdx = ev.Call<float>("getAxisValue", 27);
                        float rdy = ev.Call<float>("getAxisValue", 28);
                        if (rdx != 0f || rdy != 0f)
                        {
                            _host._amlDx += rdx;
                            _host._amlDy += rdy;
                            _host._amlGotData = true;
                        }
                        else
                        {
                            float ax = ev.Call<float>("getX");
                            float ay = ev.Call<float>("getY");
                            _host._hvrAbsX = ax;
                            _host._hvrAbsY = ay;
                            if (_lastAbsX != float.MinValue)
                            {
                                float ddx = ax - _lastAbsX;
                                float ddy = ay - _lastAbsY;
                                if (ddx != 0f || ddy != 0f)
                                {
                                    _host._amlDx += ddx;
                                    _host._amlDy += ddy;
                                    _host._amlGotData = true;
                                }
                            }
                            _lastAbsX = ax;
                            _lastAbsY = ay;
                        }
                    }
                }
                catch { }
                return false; // let Unity process hover (NGUI + cursor tracking)
            }
        }

        private class HoverSetRunnable : AndroidJavaProxy
        {
            private readonly AndroidJavaObject _view;
            private readonly HoverProxy        _proxy;
            private readonly SettingsModHook   _host;

            public HoverSetRunnable(AndroidJavaObject view, HoverProxy proxy, SettingsModHook host)
                : base("java.lang.Runnable") { _view = view; _proxy = proxy; _host = host; }

            public void run()
            {
                try
                {
                    _view.Call("setOnHoverListener", _proxy);
                    string cls = _view.Call<AndroidJavaObject>("getClass").Call<string>("getName");
                    SettingsModEntry.Log("KBM: HoverProxy installed on " + cls);
                }
                catch (Exception ex) { SettingsModEntry.Log("KBM: HoverProxy install err: " + ex.Message); }
                finally { _view.Dispose(); }
            }
        }

        // =====================================================================
        // GmlProxy � View.OnGenericMotionListener on DecorView.
        // Fires for ALL mouse generic motion events (hover + captured move) at the
        // view level, independently of whether Window.Callback fires.
        // Returns false so Unity still receives every event.
        // =====================================================================
        private class GmlProxy : AndroidJavaProxy
        {
            private const int AXIS_RELATIVE_X = 27;
            private const int AXIS_RELATIVE_Y = 28;
            private const int ACTION_HOVER_MOVE = 7;
            private const int ACTION_MOVE       = 2;

            private readonly SettingsModHook _host;
            private float _lastAbsX = float.MinValue;
            private float _lastAbsY = float.MinValue;

            public GmlProxy(SettingsModHook host)
                : base("android.view.View$OnGenericMotionListener") { _host = host; }

            public void ResetAbsPos() { _lastAbsX = float.MinValue; _lastAbsY = float.MinValue; }

            bool onGenericMotion(AndroidJavaObject view, AndroidJavaObject ev)
            {
                _host._gmlFires++;
                try
                {
                    int action = ev.Call<int>("getActionMasked");
                    if (action == ACTION_HOVER_MOVE || action == ACTION_MOVE)
                    {
                        // Primary: AXIS_RELATIVE (only non-zero when pointer capture is active)
                        float rdx = ev.Call<float>("getAxisValue", AXIS_RELATIVE_X);
                        float rdy = ev.Call<float>("getAxisValue", AXIS_RELATIVE_Y);
                        if (rdx != 0f || rdy != 0f)
                        {
                            _host._amlDx += rdx;
                            _host._amlDy += rdy;
                            _host._amlGotData = true;
                        }
                        else
                        {
                            // Fallback: absolute position delta (bounded by screen edge)
                            float ax = ev.Call<float>("getX");
                            float ay = ev.Call<float>("getY");
                            _host._gmlAbsX = ax;
                            _host._gmlAbsY = ay;
                            if (_lastAbsX != float.MinValue)
                            {
                                float ddx = ax - _lastAbsX;
                                float ddy = ay - _lastAbsY;
                                if (ddx != 0f || ddy != 0f)
                                {
                                    _host._amlDx += ddx;
                                    _host._amlDy += ddy;
                                    _host._amlGotData = true;
                                }
                            }
                            _lastAbsX = ax;
                            _lastAbsY = ay;
                        }
                    }
                }
                catch { }
                return false; // do not consume � Unity must still receive the event
            }
        }

        // Runnable that installs GmlProxy on DecorView from the UI thread.
        // Owns and disposes 'view'.
        private class GmlSetRunnable : AndroidJavaProxy
        {
            private readonly AndroidJavaObject _view;
            private readonly GmlProxy          _proxy;
            private readonly SettingsModHook   _host;

            public GmlSetRunnable(AndroidJavaObject view, GmlProxy proxy, SettingsModHook host)
                : base("java.lang.Runnable") { _view = view; _proxy = proxy; _host = host; }

            public void run()
            {
                try
                {
                    _view.Call("setOnGenericMotionListener", _proxy);
                    string cls = _view.Call<AndroidJavaObject>("getClass").Call<string>("getName");
                    SettingsModEntry.Log("KBM: GmlProxy installed on " + cls);
                }
                catch (Exception ex) { SettingsModEntry.Log("KBM: GmlProxy install err: " + ex.Message); }
                finally { _view.Dispose(); }
            }
        }

        // =====================================================================
        // WindowCallbackProxy � wraps Activity Window.Callback to intercept
        // dispatchGenericMotionEvent before any view sees it.  All other methods
        // are forwarded to the original callback so game input is unaffected.
        // AXIS_RELATIVE_X (27) / AXIS_RELATIVE_Y (28) give hardware mouse velocity
        // on ACTION_HOVER_MOVE (7) / ACTION_MOVE (2) without pointer capture.
        // =====================================================================
        // Runnable posted to Android's UI thread to call requestPointerCapture /
        // setOnCapturedPointerListener on the correct thread.  Owns and disposes 'view'.
        private class PointerCaptureRunnable : AndroidJavaProxy
        {
            private readonly AndroidJavaObject          _view;
            private readonly bool                       _capture;
            private readonly CapturedPointerListener    _listener;
            private readonly SettingsModHook            _host;

            public PointerCaptureRunnable(AndroidJavaObject view, bool capture,
                                          CapturedPointerListener listener, SettingsModHook host)
                : base("java.lang.Runnable")
            {
                _view     = view;
                _capture  = capture;
                _listener = listener;
                _host     = host;
            }

            public void run()
            {
                try
                {
                    string cls = _view.Call<AndroidJavaObject>("getClass")
                                      .Call<string>("getName");
                    if (_capture)
                    {
                        // Try on passed view (DecorView)
                        if (_listener != null)
                            _view.Call("setOnCapturedPointerListener", _listener);
                        _view.Call("requestPointerCapture");
                        bool hasCap = _view.Call<bool>("hasPointerCapture");
                        SettingsModEntry.Log("KBM: decor capture hasCap=" + hasCap + " view=" + cls);

                        // requestPointerCapture() requires the calling View to have input
                        // focus.  On Unity, mUnityPlayer (not DecorView) holds focus.
                        // Try the currently focused view and mUnityPlayer explicitly.
                        AndroidJavaClass  pl2 = null;
                        AndroidJavaObject ac2 = null, wn2 = null, fv = null, mup = null;
                        try
                        {
                            pl2 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                            ac2 = pl2.GetStatic<AndroidJavaObject>("currentActivity");
                            wn2 = ac2.Call<AndroidJavaObject>("getWindow");
                            // Try window's focused view
                            fv = wn2.Call<AndroidJavaObject>("getCurrentFocus");
                            if (fv != null)
                            {
                                if (_listener != null) fv.Call("setOnCapturedPointerListener", _listener);
                                fv.Call("requestPointerCapture");
                                bool hasFv = fv.Call<bool>("hasPointerCapture");
                                string fvCls = fv.Call<AndroidJavaObject>("getClass").Call<string>("getName");
                                SettingsModEntry.Log("KBM: focused capture hasCap=" + hasFv + " view=" + fvCls);
                            }
                            else
                            {
                                SettingsModEntry.Log("KBM: getCurrentFocus() returned null");
                            }
                            // Also try mUnityPlayer directly
                            mup = ac2.Get<AndroidJavaObject>("mUnityPlayer");
                            if (mup != null)
                            {
                                if (_listener != null) mup.Call("setOnCapturedPointerListener", _listener);
                                mup.Call("requestPointerCapture");
                                bool hasMup = mup.Call<bool>("hasPointerCapture");
                                string mupCls = mup.Call<AndroidJavaObject>("getClass").Call<string>("getName");
                                SettingsModEntry.Log("KBM: mUnityPlayer capture hasCap=" + hasMup + " view=" + mupCls);
                            }
                        }
                        catch (Exception ce) { SettingsModEntry.Log("KBM: focused/mup capture err: " + ce.Message); }
                        finally
                        {
                            if (mup != null) mup.Dispose();
                            if (fv  != null) fv.Dispose();
                            if (wn2 != null) wn2.Dispose();
                            if (ac2 != null) ac2.Dispose();
                            if (pl2 != null) pl2.Dispose();
                        }
                        _host._captureActive = true;
                    }
                    else
                    {
                        _view.Call("releasePointerCapture");
                        _host._captureActive = false;
                        SettingsModEntry.Log("KBM: UI release done view=" + cls);
                    }
                }
                catch (Exception ex)
                {
                    SettingsModEntry.Log("KBM: PointerCaptureRunnable err: " + ex.Message);
                }
                finally
                {
                    _view.Dispose();
                }
            }
        }

        private class WindowCallbackProxy : AndroidJavaProxy
        {
            private const int ACTION_HOVER_MOVE = 7;
            private const int ACTION_MOVE       = 2;
            private const int AXIS_RELATIVE_X   = 27;
            private const int AXIS_RELATIVE_Y   = 28;

            private readonly AndroidJavaObject _orig;
            private readonly SettingsModHook   _host;
            private float _lastAbsX = float.MinValue;
            private float _lastAbsY = float.MinValue;

            public WindowCallbackProxy(AndroidJavaObject orig, SettingsModHook host)
                : base("android.view.Window$Callback") { _orig = orig; _host = host; }

            // Intercept mouse movement; forward event to original for view dispatch.
            public bool dispatchGenericMotionEvent(AndroidJavaObject ev)
            {
                _host._proxyFires++;  // diagnostic: counts whether this JNI proxy is invoked at all
                // Feed joystick axes to JoyProxy regardless of action type
                if (_host._joyProxy != null) _host._joyProxy.Feed(ev);
                try
                {
                    int action = ev.Call<int>("getActionMasked");
                    if (action == ACTION_HOVER_MOVE || action == ACTION_MOVE)
                    {
                        // Primary: AXIS_RELATIVE (only available when pointer capture is active)
                        float rdx = ev.Call<float>("getAxisValue", AXIS_RELATIVE_X);
                        float rdy = ev.Call<float>("getAxisValue", AXIS_RELATIVE_Y);
                        if (rdx != 0f || rdy != 0f)
                        {
                            _host._amlDx += rdx;
                            _host._amlDy += rdy;
                            _host._amlGotData = true;
                        }
                        else
                        {
                            // Fallback: absolute position delta.
                            // Works even without pointer capture; cursor is hidden (TYPE_NULL)
                            // but Android still updates getX/getY as the physical mouse moves.
                            float ax = ev.Call<float>("getX");
                            float ay = ev.Call<float>("getY");
                            _host._dbgAbsX = ax;  // diagnostic: track last reported cursor position
                            _host._dbgAbsY = ay;
                            if (_lastAbsX != float.MinValue)
                            {
                                float ddx = ax - _lastAbsX;
                                float ddy = ay - _lastAbsY;
                                if (ddx != 0f || ddy != 0f)
                                {
                                    _host._amlDx += ddx;
                                    _host._amlDy += ddy;
                                    _host._amlGotData = true;
                                }
                            }
                            _lastAbsX = ax;
                            _lastAbsY = ay;
                        }
                    }
                }
                catch { }
                return _orig.Call<bool>("dispatchGenericMotionEvent", ev);
            }

            // Called when cursor is unlocked so the next lock doesn't produce a delta jump.
            public void ResetAbsPos() { _lastAbsX = float.MinValue; _lastAbsY = float.MinValue; }

            // Forward all other Window.Callback methods to the original.
            public bool dispatchKeyEvent(AndroidJavaObject ev)
            { return _orig.Call<bool>("dispatchKeyEvent", ev); }
            public bool dispatchKeyShortcutEvent(AndroidJavaObject ev)
            { return _orig.Call<bool>("dispatchKeyShortcutEvent", ev); }
            public bool dispatchTouchEvent(AndroidJavaObject ev)
            { return _orig.Call<bool>("dispatchTouchEvent", ev); }
            public bool dispatchTrackballEvent(AndroidJavaObject ev)
            { return _orig.Call<bool>("dispatchTrackballEvent", ev); }
            public bool dispatchPopulateAccessibilityEvent(AndroidJavaObject ev)
            { return _orig.Call<bool>("dispatchPopulateAccessibilityEvent", ev); }
            public void onWindowFocusChanged(bool hasFocus)
            {
                _orig.Call("onWindowFocusChanged", hasFocus);
                if (hasFocus) _host._winFocusFires++;
                if (hasFocus && _host._kbmEnabled && _host._cursorLocked)
                {
                    AndroidJavaClass  pl = null;
                    AndroidJavaObject ac = null, wn = null, dc = null, fv = null, mup = null;
                    try
                    {
                        pl  = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        ac  = pl.GetStatic<AndroidJavaObject>("currentActivity");
                        wn  = ac.Call<AndroidJavaObject>("getWindow");
                        // Try focused view first (mUnityPlayer holds focus in Unity)
                        fv  = wn.Call<AndroidJavaObject>("getCurrentFocus");
                        if (fv != null)
                        {
                            if (_host._capListener != null)
                                fv.Call("setOnCapturedPointerListener", _host._capListener);
                            fv.Call("requestPointerCapture");
                            SettingsModEntry.Log("KBM: focusChange capture -> focused view");
                        }
                        mup = ac.Get<AndroidJavaObject>("mUnityPlayer");
                        if (mup != null)
                        {
                            if (_host._capListener != null)
                                mup.Call("setOnCapturedPointerListener", _host._capListener);
                            mup.Call("requestPointerCapture");
                            SettingsModEntry.Log("KBM: focusChange capture -> mUnityPlayer");
                        }
                        dc  = wn.Call<AndroidJavaObject>("getDecorView");
                        if (_host._capListener != null)
                            dc.Call("setOnCapturedPointerListener", _host._capListener);
                        dc.Call("requestPointerCapture");
                        _host._captureActive = true;
                        SettingsModEntry.Log("KBM: focusChange capture -> decor (winFocusFires=" + _host._winFocusFires + ")");
                    }
                    catch (Exception ex) { SettingsModEntry.Log("KBM: focus-capture err: " + ex.Message); }
                    finally
                    {
                        if (mup != null) mup.Dispose();
                        if (fv  != null) fv.Dispose();
                        if (dc  != null) dc.Dispose();
                        if (wn  != null) wn.Dispose();
                        if (ac  != null) ac.Dispose();
                        if (pl  != null) pl.Dispose();
                    }
                }
            }
            public void onAttachedToWindow()                 { _orig.Call("onAttachedToWindow"); }
            public void onDetachedFromWindow()               { _orig.Call("onDetachedFromWindow"); }
            public void onContentChanged()                   { _orig.Call("onContentChanged"); }
            public void onWindowAttributesChanged(AndroidJavaObject p) { _orig.Call("onWindowAttributesChanged", p); }
        }

        // =====================================================================
        // Captured pointer listener (Android 8+ / API 26+)
        // Implements View.OnCapturedPointerListener via AndroidJavaProxy.
        // Called on the Android UI thread; drain methods are called from Unity game thread.
        // ARM word-sized reads/writes are atomic, so no lock needed for these simple fields.
        // =====================================================================
        private class CapturedPointerListener : AndroidJavaProxy
        {
            // Raw accumulated relative mouse delta since last drain.
            private volatile float _dx;
            private volatile float _dy;
            private volatile bool  _loggedFirst;
            // Mouse button state (set/cleared from Android UI thread, read on game thread).
            public volatile bool lmbHeld;
            public volatile bool rmbHeld;

            // Android MotionEvent constants
            private const int AXIS_RELATIVE_X    = 27;
            private const int AXIS_RELATIVE_Y    = 28;
            private const int ACTION_DOWN         = 0;   // primary button pressed (LMB)
            private const int ACTION_UP           = 1;   // primary button released (LMB)
            private const int ACTION_MOVE         = 2;
            private const int ACTION_HOVER_MOVE   = 7;
            private const int ACTION_BUTTON_PRESS   = 11;
            private const int ACTION_BUTTON_RELEASE  = 12;
            private const int BUTTON_PRIMARY     = 1;
            private const int BUTTON_SECONDARY   = 2;

            public CapturedPointerListener()
                : base("android.view.View$OnCapturedPointerListener") { }

            public int  FireCount;
            public void Reset() { _dx = 0f; _dy = 0f; lmbHeld = false; rmbHeld = false; _loggedFirst = false; FireCount = 0; }

            // Called by Unity game thread each frame to atomically read+clear delta.
            public float DrainDx() { float v = _dx; _dx = 0f; return v; }
            public float DrainDy() { float v = _dy; _dy = 0f; return v; }

            // Java callback: View.OnCapturedPointerListener.onCapturedPointer(View, MotionEvent)
            bool onCapturedPointer(AndroidJavaObject view, AndroidJavaObject e)
            {
                FireCount++;
                try
                {
                    int action = e.Call<int>("getActionMasked");
                    if (!_loggedFirst)
                    {
                        _loggedFirst = true;
                        float rx = e.Call<float>("getAxisValue", AXIS_RELATIVE_X);
                        float ry = e.Call<float>("getAxisValue", AXIS_RELATIVE_Y);
                        UnityEngine.Debug.Log("KBM: onCapturedPointer FIRST action=" + action
                            + " rx=" + rx + " ry=" + ry);
                    }
                    if (action == ACTION_MOVE || action == ACTION_HOVER_MOVE)
                    {
                        _dx += e.Call<float>("getAxisValue", AXIS_RELATIVE_X);
                        _dy += e.Call<float>("getAxisValue", AXIS_RELATIVE_Y);
                        // Sync button state from live button mask � catches presses that
                        // arrived as ACTION_DOWN rather than ACTION_BUTTON_PRESS.
                        int bs = e.Call<int>("getButtonState");
                        lmbHeld = (bs & BUTTON_PRIMARY)   != 0;
                        rmbHeld = (bs & BUTTON_SECONDARY) != 0;
                    }
                    else if (action == ACTION_DOWN)  { lmbHeld = true; }
                    else if (action == ACTION_UP)    { lmbHeld = false; }
                    else if (action == ACTION_BUTTON_PRESS)
                    {
                        int btn = e.Call<int>("getActionButton");
                        if (btn == BUTTON_PRIMARY)    lmbHeld = true;
                        if (btn == BUTTON_SECONDARY)  rmbHeld = true;
                    }
                    else if (action == ACTION_BUTTON_RELEASE)
                    {
                        int btn = e.Call<int>("getActionButton");
                        if (btn == BUTTON_PRIMARY)    lmbHeld = false;
                        if (btn == BUTTON_SECONDARY)  rmbHeld = false;
                    }
                }
                catch (Exception) { }
                return true;
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
            HudCfgLoad();
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
            _wideCam        = HudCfgGetInt("CNRMod_WideCam", 0) == 1;
            _kbmEnabled     = HudCfgGetInt("CNRMod_KbmEnabled", 0) == 1;
            _mouseSensNgl   = HudCfgGetFloat("CNRMod_MouseSens", 3.2f);
            _mouseAdsMult   = HudCfgGetFloat("CNRMod_MouseAdsMult", 0.5f);
            _kbmDeadzone   = HudCfgGetFloat("CNRMod_KbmDeadzone",   0.05f);
            _touchDeadzone = HudCfgGetFloat("CNRMod_TouchDeadzone", 0.1f);
            _gamepadEnabled      = HudCfgGetInt("CNRMod_GamepadEnabled", 0) == 1;
            _controllerDeadzone  = HudCfgGetFloat("CNRMod_CtrlDeadzone",  0.1f);
            _controllerSens      = HudCfgGetFloat("CNRMod_CtrlSens",      1.5f);
            _controllerCamSens    = HudCfgGetFloat("CNRMod_CtrlCamSens",     0.5f);
            _controllerCamFalloff = HudCfgGetFloat("CNRMod_CtrlCamFalloff", 1.0f);
            _controllerAimMult    = HudCfgGetFloat("CNRMod_CtrlAimMult",    0.5f);
            for (int ki = 0; ki < KBM_BIND_COUNT; ki++)
                _kbKeys[ki] = (KeyCode)HudCfgGetInt(KBM_PREF_KEYS[ki], (int)KBM_DEFAULTS[ki]);
            for (int gi = 0; gi < GP_BIND_COUNT; gi++)
            {
                _gpKeys[gi]      = (KeyCode)HudCfgGetInt(GP_PREF_KEYS[gi], (int)GP_DEFAULTS[gi]);
                string axBind    = HudCfgGet(GP_AXIS_PREF_KEYS[gi], "");
                _gpAxisBinds[gi] = axBind.Length > 0 ? axBind : null;
            }
            _gpLStickJAX = HudCfgGetInt("CNRMod_LStickJAX", 0);
            _gpLStickJAY = HudCfgGetInt("CNRMod_LStickJAY", 1);
            _gpRStickJAX = HudCfgGetInt("CNRMod_RStickJAX", 11);
            _gpRStickJAY = HudCfgGetInt("CNRMod_RStickJAY", 14);
            string rax = HudCfgGet("CNRMod_RAxisX", ""); if (rax.Length > 0) _gpRAxisX = rax;
            string ray = HudCfgGet("CNRMod_RAxisY", ""); if (ray.Length > 0) _gpRAxisY = ray;
            string lax = HudCfgGet("CNRMod_LAxisX", ""); if (lax.Length > 0) _gpLAxisX = lax;
            string lay = HudCfgGet("CNRMod_LAxisY", ""); if (lay.Length > 0) _gpLAxisY = lay;
            for (int i = 0; i < VIS_ITEMS.Length; i++)
                _visOn[i] = HudCfgGetInt(VIS_ITEMS[i].prefKey, 1) == 1;
            // Pre-populate _savedScales so LateUpdate can enforce them immediately.
            // Use -1f as sentinel for "no saved scale" since valid scales can be tiny (< 0.01).
            // Also nuke any stale scale prefs for game-owned panels (prefSZ == null items).
            string[] stalePanelKeys = new string[]{ "CNRMod_SZ_ToolBar", "CNRMod_SZ_ChatBar", "CNRMod_SZ_HP", "CNRMod_SZ_TS1" };
            foreach (string k in stalePanelKeys) { PlayerPrefs.DeleteKey(k); HudCfgDelete(k); }
            // Migration: if scale version key absent, wipe all saved scales (clears stale data from old code).
            if (!HudCfgHasKey("CNRMod_ScaleVer") || HudCfgGetInt("CNRMod_ScaleVer") < 3)
            {
                SettingsModEntry.Log("LoadPrefs: migrating scale prefs to ratio-based (v3)");
                for (int i = 0; i < DRAG_COUNT; i++)
                    if (DRAG_ITEMS[i].prefSZ != null) HudCfgDelete(DRAG_ITEMS[i].prefSZ);
                HudCfgSetInt("CNRMod_ScaleVer", 3);
                HudCfgSave();
            }
            for (int i = 0; i < DRAG_COUNT; i++)
            {
                if (DRAG_ITEMS[i].prefSZ != null && HudCfgHasKey(DRAG_ITEMS[i].prefSZ))
                {
                    _savedScales[i] = HudCfgGetFloat(DRAG_ITEMS[i].prefSZ);
                    SettingsModEntry.Log("PREFS ratio[" + i + "] " + DRAG_ITEMS[i].displayName + " = " + _savedScales[i].ToString("F4"));
                }
                else
                    _savedScales[i] = -1f;
            }
        }

        // =====================================================================
        // HUD config file helpers (/sdcard/CNRMods/hud.cfg)
        // =====================================================================
        private void HudCfgLoad()
        {
            _hudCfg.Clear();
            if (!File.Exists(HUD_CFG_PATH)) return;
            try
            {
                foreach (string line in File.ReadAllLines(HUD_CFG_PATH))
                {
                    int eq = line.IndexOf('=');
                    if (eq > 0) _hudCfg[line.Substring(0, eq).Trim()] = line.Substring(eq + 1);
                }
                SettingsModEntry.Log("HudCfg: loaded " + _hudCfg.Count + " entries");
            }
            catch (System.Exception ex) { SettingsModEntry.Log("HudCfg load error: " + ex.Message); }
        }

        private void HudCfgSave()
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(HUD_CFG_PATH);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var sb = new System.Text.StringBuilder();
                foreach (var kv in _hudCfg) sb.AppendLine(kv.Key + "=" + kv.Value);
                File.WriteAllText(HUD_CFG_PATH, sb.ToString());
            }
            catch (System.Exception ex) { SettingsModEntry.Log("HudCfg save error: " + ex.Message); }
        }

        private bool  HudCfgHasKey(string key) { return _hudCfg.ContainsKey(key); }
        private void  HudCfgDelete(string key)  { _hudCfg.Remove(key); }

        private float HudCfgGetFloat(string key, float def = 0f)
        {
            string v; float r;
            return (_hudCfg.TryGetValue(key, out v) &&
                    float.TryParse(v, System.Globalization.NumberStyles.Float,
                                   System.Globalization.CultureInfo.InvariantCulture, out r)) ? r : def;
        }
        private void HudCfgSetFloat(string key, float val)
        {
            _hudCfg[key] = val.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        }

        private int  HudCfgGetInt(string key, int def = 0)
        {
            string v; int r;
            return (_hudCfg.TryGetValue(key, out v) && int.TryParse(v, out r)) ? r : def;
        }
        private void HudCfgSetInt(string key, int val) { _hudCfg[key] = val.ToString(); }
        private string HudCfgGet(string key, string def = "") { string v; return _hudCfg.TryGetValue(key, out v) ? v : def; }
        private void   HudCfgSet(string key, string val)       { _hudCfg[key] = val; }

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
            if (_menuSpsCached) return;

            // Load sprites bundled as embedded PNGs (no UIAtlas extraction needed)
            // PanelBack
            _spPanelBack = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spPanelBack.LoadImage(System.Convert.FromBase64String(_PanelBackB64));
            _spPanelBack.Apply();

            // ButtonNull_2
            _spButtonNull = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spButtonNull.LoadImage(System.Convert.FromBase64String(_ButtonNull_2B64));
            _spButtonNull.Apply();

            // PropKuang
            _spPropKuang = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spPropKuang.LoadImage(System.Convert.FromBase64String(_PropKuangB64));
            _spPropKuang.Apply();

            // SelectKuang
            _spSelectKuang = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spSelectKuang.LoadImage(System.Convert.FromBase64String(_SelectKuangB64));
            _spSelectKuang.Apply();

            // SliderB
            _spSliderB = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spSliderB.LoadImage(System.Convert.FromBase64String(_SliderBB64));
            _spSliderB.Apply();

            // SliderThumb
            _spSliderThumb = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            _spSliderThumb.LoadImage(System.Convert.FromBase64String(_SliderThumbB64));
            _spSliderThumb.Apply();
            _menuSpsCached = true;

            // Nullify cached styles so they rebuild next frame with correct font + textures
            _gsWinBg = _gsVScroll = _gsLabel = _gsHint = _gsSectionHdr = null;
            _gsTabActive = _gsTabIdle = _gsGhostBtn = _gsInvisBg = _gsInvisThumb = _gsWipBanner = null;
            _gsKeyLabelCtrl = _gsKeyLabelKbm = null;
            _gsBtnCache.Clear();

            // Find game font from any live UILabel
            if (_gameFont == null)
            {
                UILabel[] lbls = (UILabel[])FindObjectsOfType(typeof(UILabel));
                foreach (UILabel lbl in lbls)
                    if (lbl.font != null && lbl.font.dynamicFont != null)
                    { _gameFont = lbl.font.dynamicFont; break; }
            }
            SettingsModEntry.Log("LoadBundledSprites: Panel=" + (_spPanelBack!=null) +
                " Btn=" + (_spButtonNull!=null) + " Chk=" + (_spPropKuang!=null) +
                " Sel=" + (_spSelectKuang!=null) + " SlB=" + (_spSliderB!=null) +
                " SlT=" + (_spSliderThumb!=null) + " Font=" + (_gameFont!=null));
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

