// CNRModManager.cs  -  Mod Manager for Cops N Robbers
// Compiled by: .\build_mod.ps1 -ModFile CNRModManager.cs
//
// Two entry points:
//   CNRModManager.ModManagerEntry.Load()   - called by CNRMod's DLL scanner
//   CNRMods.ModEntry.Load() (shim below)   - called by game bootstrap when CNRMod absent

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// SHIM — MainMenuDirector.LoadMods() looks for CNRMods.ModEntry.Load().
// When CNRMod.dll is absent this satisfies the game's bootstrap call.
// When CNRMod IS present the shim detects it via AppDomain and does nothing
// (CNRMod handles loading and will call ModManagerEntry.Load() via its scanner).
// ─────────────────────────────────────────────────────────────────────────────
namespace CNRMods
{
    public class ModEntry   // intentional name match with CNRMod's ModEntry
    {
        public static void Load()
        {
            // Detect whether the real CNRMod (not our own shim) is loaded.
            // CNRMod's DLL scanner only calls the FIRST Load() it finds in the assembly,
            // so we cannot rely on it finding ModManagerEntry.Load() separately.
            // Always forward here — the _loaded guard prevents double-init.
            bool cnrModPresent = false;
            try
            {
                Assembly myAsm = typeof(CNRModManager.ModManagerEntry).Assembly;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType("CNRMods.ModEntry");
                    if (t == null) continue;
                    if (t.Assembly == myAsm) continue;   // that's just our own shim
                    cnrModPresent = true;
                    break;
                }
            }
            catch { }

            if (cnrModPresent)
                CNRModManager.ModManagerEntry.Load();         // CNRMod present: normal load, no extra DLL scan
            else
                CNRModManager.ModManagerEntry.BootstrapLoad(); // standalone: load + scan other mods
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MOD MANAGER
// ─────────────────────────────────────────────────────────────────────────────
namespace CNRModManager
{
    // ─────────────────────────────────────────────────────────────────────────
    // ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────────
    public static class ModManagerEntry
    {
        public  const string Version        = "1.5.4";
        private const string LogPath        = "/storage/emulated/0/CNRMods/modmanager.log";
        public  const string ModsDir        = "/storage/emulated/0/CNRMods";
        public  const string DefaultRepoUrl = "https://play.jacqueb.me/mods/repo.json";
        public  static bool  IsOpen         = false;
        public  static bool  IsLoaded       = false;

        private static bool      _loaded       = false;
        private static FieldInfo _ecoOpenFI    = null;   // CNRMods.EconomyHook.ModManagerOpen
        private static bool      _ecoFIChecked = false;

        // Sets EconomyHook.ModManagerOpen in CNRMod via reflection.
        // Called whenever IsOpen changes so CNRMod can hide its IMGUI buttons.
        internal static void SetEcoHookOpen(bool open)
        {
            if (!_ecoFIChecked)
            {
                _ecoFIChecked = true;
                try
                {
                    Assembly myAsm = typeof(ModManagerEntry).Assembly;
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (asm == myAsm) continue;
                        Type t = asm.GetType("CNRMods.EconomyHook");
                        if (t == null) continue;
                        _ecoOpenFI = t.GetField("ModManagerOpen",
                            BindingFlags.Public | BindingFlags.Static);
                        break;
                    }
                }
                catch { }
            }
            try { if (_ecoOpenFI != null) _ecoOpenFI.SetValue(null, open); } catch { }
        }

        // Called by CNRMod's multiplayer dialog "Open Mod Manager" button.
        public static void OpenWindow()
        {
            try
            {
                // Find the ModManagerHook MonoBehaviour instance and call its OpenWindow()
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType("CNRModManager.ModManagerHook");
                    if (t == null) continue;
                    UnityEngine.Object[] hooks = UnityEngine.Object.FindObjectsOfType(t);
                    if (hooks != null && hooks.Length > 0)
                    {
                        MethodInfo m = t.GetMethod("OpenWindowPublic",
                            BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                        if (m != null) { m.Invoke(hooks[0], null); }
                    }
                    break;
                }
            }
            catch { }
        }

        // Called by CNRMod's DLL scanner
        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            RegisterWithCNRMod();
            Log("=== CNRModManager v" + Version + " Load() ===");
            Spawn();
            IsLoaded = true;
        }

        // Called by CNRMods shim when CNRMod.dll is absent
        public static void BootstrapLoad()
        {
            if (_loaded) return;
            _loaded = true;
            Log("=== CNRModManager v" + Version + " BootstrapLoad() (no CNRMod) ===");
            Spawn();
            ScanAndLoadMods();
            IsLoaded = true;
        }

        private static void Spawn()
        {
            try
            {
                GameObject go = new GameObject("CNRModManager_Root");
                go.AddComponent<ModManagerHook>();
                GameObject.DontDestroyOnLoad(go);
            }
            catch (Exception ex) { Log("Spawn error: " + ex); }
        }

        // Fallback DLL loader — mirrors CNRMod's LoadExternalMods
        private static void ScanAndLoadMods()
        {
            try
            {
                if (!Directory.Exists(ModsDir)) return;

                List<string> alreadyLoaded = new List<string>();
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    alreadyLoaded.Add(asm.GetName().Name.ToLower());

                foreach (string path in Directory.GetFiles(ModsDir, "*.dll"))
                {
                    string fname = Path.GetFileName(path);
                    if (fname.Equals("CNRMod.dll",        StringComparison.OrdinalIgnoreCase)) continue;
                    if (fname.Equals("CNRModManager.dll", StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        Assembly asm = Assembly.Load(File.ReadAllBytes(path));
                        if (alreadyLoaded.Contains(asm.GetName().Name.ToLower()))
                        { Log("ScanMods: skip (loaded) " + fname); continue; }
                        bool found = false;
                        foreach (Type t in asm.GetTypes())
                        {
                            MethodInfo m = t.GetMethod("Load",
                                BindingFlags.Public | BindingFlags.Static,
                                null, Type.EmptyTypes, null);
                            if (m != null) { m.Invoke(null, null); found = true; break; }
                        }
                        Log("ScanMods: " + fname + (found ? " OK" : " (no Load())"));
                    }
                    catch (Exception ex) { Log("ScanMods err " + fname + ": " + ex.Message); }
                }
            }
            catch (Exception ex) { Log("ScanMods: " + ex.Message); }
        }

        private static void RegisterWithCNRMod()
        {
            try
            {
                Assembly myAsm = typeof(ModManagerEntry).Assembly;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null || me.Assembly == myAsm) continue;
                    MethodInfo reg = me.GetMethod("RegisterMod",
                        BindingFlags.Public | BindingFlags.Static, null,
                        new Type[] { typeof(string), typeof(string) }, null);
                    if (reg != null) reg.Invoke(null, new object[] { "CNRModManager", Version });
                    break;
                }
            }
            catch { }
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); } catch { }
            try { Debug.Log("[CNRModManager] " + msg); } catch { }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MONOBEHAVIOUR HOOK — GUI, scene detection, HTTP
    // ─────────────────────────────────────────────────────────────────────────
    public class ModManagerHook : MonoBehaviour
    {
        // Exposed for CNRMod to gate multiplayer: true when any required mod is missing or below minVersion.
        public static bool HasRequiredPending = false;

        // Layout / scaling
        private const float REF_W = 600f;

        // Window state
        private bool    _showWindow  = false;
        private int     _tab         = 0;   // 0=Installed 1=Browse 2=Repos
        private Vector2 _scroll      = Vector2.zero;
        private float   _lastToggle  = 0f;

        // ── Swipe-to-scroll ──────────────────────────────────────────────────
        // Same pattern as CNRSkinGridUI in CNRMod.  _scroll.y is modified
        // directly in Update() (screen-space drag converted to GUI units via sc).
        // DrawWindow() consumes pointer events during a drag so buttons don't
        // fire accidentally at the end of a swipe.
        private bool  _swipeActive   = false;
        private bool  _swipeDragging = false;
        private float _swipeStartY   = 0f;
        private float _swipePrevY    = 0f;
        private const float SWIPE_DEAD_PX = 12f;  // screen-pixels before swipe commits

        // Scene
        private string _scene    = "";
        private bool   _patched  = false;
        private bool   _nguiBlocked = false;

        // NGUI caches
        private UICamera[] _nguiCameras = null;
        private static Font _font = null;

        // Multiplayer button intercept
        private GameObject _goMpBtn = null;

        // ── Installed tab data ────────────────────────────────────────────────
        private struct InstalledMod
        {
            public string filename;
            public string displayName;
            public string version;
        }
        private List<InstalledMod> _installedMods = new List<InstalledMod>();

        // ── Browse tab data ───────────────────────────────────────────────────
        private struct ModVersion
        {
            public string version;
            public string url;
            public string changelog;
        }
        private struct ModDependency
        {
            public string kind;
            public string id;
            public string filename;
            public string url;
            public string minVersion;
            public bool latestOnly;
        }
        private struct RepoMod
        {
            public string id;
            public string name;
            public string description;
            public string latestVersion;
            public string minVersion;
            public string filename;
            public string latestUrl;
            public bool   latestOnly;
            public List<ModDependency> dependencies;
            public List<ModVersion> versions;
        }
        private List<RepoMod> _browseMods  = new List<RepoMod>();
        private string _browseStatus       = "";
        private bool   _browseFetching     = false;
        private string _statusMsg          = "";   // shared download / action feedback
        private int    _detailModIdx       = -1;    // -1=list view, >=0=detail for that index

        // ── Auto-update / update check ────────────────────────────────────────
        private const string PREF_AU_PREFIX  = "CNRModMgr_AU_"; // auto-update per mod (int 0/1, default 1)
        private const string PREF_IV_PREFIX  = "CNRModMgr_IV_"; // last-installed version per mod

        private List<RepoMod> _pendingUpdates        = new List<RepoMod>(); // outdated mods w/ au=true
        private bool          _updateBannerDismissed = false;
        private bool          _autoFetchDone         = false;  // startup repo fetch, once per session

        // ── Toast notification ────────────────────────────────────────────────
        private string _toastMsg   = "";
        private float  _toastUntil = 0f;

        // ── Batch update ──────────────────────────────────────────────────────
        private bool          _batchRunning      = false;
        private int           _batchTotal        = 0;
        private int           _batchDone         = 0;
        private List<RepoMod> _batchQueue        = new List<RepoMod>();
        private bool          _batchNeedsRestart = false;

        // ── Repos tab data ────────────────────────────────────────────────────
        private List<string> _repos      = new List<string>();
        private string       _newRepoInput = "";
        private const string PREF_REPOS    = "CNRModMgr_Repos";

        // ── Per-mod config panel ──────────────────────────────────────────────
        private int            _configModIdx       = -1;    // index into _installedMods; -1 = not open
        private Type           _configType         = null;
        private List<string[]> _configEntries      = new List<string[]>();
        private bool           _configDirty        = false;
        private bool           _configNeedsRestart = false;
        private readonly Dictionary<string, Type> _configTypeCache   = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string>          _configTypeChecked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ─────────────────────────────────────────────────────────────────────
        private void Start()
        {
            _scene = Application.loadedLevelName ?? "";
            if (_scene == "MainMenu") PatchMenu();
            LoadRepoList();
            // Silent background fetch on startup to detect available updates
            if (!_autoFetchDone && _repos.Count > 0)
            {
                _autoFetchDone = true;
                StartCoroutine(FetchBrowse(_repos[0]));
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            _scene       = Application.loadedLevelName ?? "";
            _patched     = false;
            _nguiCameras = null;
            _nguiBlocked = false;
            _showWindow  = false;
            _goMpBtn     = null;
            ModManagerEntry.IsOpen = false;
            ModManagerEntry.SetEcoHookOpen(false);
            if (_scene == "MainMenu") PatchMenu();
        }

        private void PatchMenu()
        {
            if (_patched) return;
            _patched = true;
            _goMpBtn = null;
            if (_font == null)
            {
                UILabel[] lbls = (UILabel[])(object)FindObjectsOfType(typeof(UILabel));
                foreach (UILabel lbl in lbls)
                    if (lbl.font != null && lbl.font.dynamicFont != null)
                    { _font = lbl.font.dynamicFont; break; }
            }
            ModManagerEntry.Log("PatchMenu scene=" + _scene + " font=" + (_font != null ? "ok" : "null"));
            RefreshInstalledMods();
            if (_browseMods.Count > 0)
                CheckForUpdates();
            else if (!_browseFetching)
                StartFetchBrowse();
        }

        private void TryInterceptMpButton()
        {
            if (_goMpBtn != null) return;
            try
            {
                MonoBehaviour[] all = (MonoBehaviour[])(object)FindObjectsOfType(typeof(MonoBehaviour));
                foreach (MonoBehaviour mb in all)
                {
                    if (mb == null || mb.GetType().Name != "UIButtonEventKit") continue;
                    FieldInfo fi = mb.GetType().GetField("buttonName",
                        BindingFlags.Instance | BindingFlags.Public);
                    if (fi == null) continue;
                    int bval;
                    try { bval = Convert.ToInt32(fi.GetValue(mb)); }
                    catch { continue; }
                    if (bval != 12) continue; // 12 = Multiplayer button
                    _goMpBtn = ((Component)(object)mb).gameObject;
                    ((Behaviour)(object)mb).enabled = false; // disable original handler
                    var interceptor = _goMpBtn.AddComponent<MgrMpInterceptor>();
                    interceptor.hook = this;
                    ModManagerEntry.Log("TryInterceptMpButton: intercepted GotoHall on " + _goMpBtn.name);
                    break;
                }
            }
            catch (Exception ex) { ModManagerEntry.Log("TryInterceptMpButton err: " + ex.Message); }
        }

        internal void OnMpButtonClick()
        {
            bool cnrOk = File.Exists("/sdcard/CNRMods/CNRMod.dll");
            bool stgOk = File.Exists("/sdcard/CNRMods/CNRSettingsMod.dll");
            bool mgrOk = File.Exists("/sdcard/CNRMods/CNRModManager.dll");
            if (cnrOk && stgOk && mgrOk)
            {
                Application.LoadLevel("MultiPlayerSelect");
                return;
            }
            // Missing mods — open mod manager window so user can install
            ModManagerEntry.Log("Mp blocked: cnr=" + cnrOk + " stg=" + stgOk + " mgr=" + mgrOk);
            OpenWindow();
        }

        private void Update()
        {
            SetNguiBlocking(_showWindow);
            if (_scene == "MainMenu" && _patched && _goMpBtn == null)
                TryInterceptMpButton();

            // Swipe-to-scroll: track drag in screen space, convert to GUI units
            if (_showWindow)
            {
                float sc = Screen.width / REF_W;
                if (Input.GetMouseButtonDown(0))
                {
                    _swipeActive   = true;
                    _swipeDragging = false;
                    _swipeStartY   = Input.mousePosition.y;
                    _swipePrevY    = Input.mousePosition.y;
                }
                else if (Input.GetMouseButton(0) && _swipeActive)
                {
                    float dy = Input.mousePosition.y - _swipePrevY;
                    _swipePrevY = Input.mousePosition.y;
                    if (!_swipeDragging && Mathf.Abs(Input.mousePosition.y - _swipeStartY) > SWIPE_DEAD_PX)
                    {
                        _swipeDragging        = true;
                        GUIUtility.hotControl = 0;  // cancel any button that captured on MouseDown
                    }
                    if (_swipeDragging)
                        _scroll.y = Mathf.Max(0f, _scroll.y + dy / sc);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _swipeActive = false;
                    // _swipeDragging cleared in DrawWindow when EventType.MouseUp is consumed
                }
            }
        }

        private void SetNguiBlocking(bool block)
        {
            if (block == _nguiBlocked) return;
            bool rescan = _nguiCameras == null;
            if (!rescan)
            {
                for (int i = 0; i < _nguiCameras.Length; i++)
                {
                    if (_nguiCameras[i] == null) { rescan = true; break; }
                }
            }
            if (rescan)
                _nguiCameras = (UICamera[])(object)FindObjectsOfType(typeof(UICamera));
            if (_nguiCameras != null)
                foreach (UICamera cam in _nguiCameras)
                    if (cam != null) cam.enabled = !block;
            _nguiBlocked = block;
        }

        private void OnGUI()
        {
            if (_scene != "MainMenu" || !_patched) return;

            float sc = Screen.width / REF_W;
            GUIUtility.ScaleAroundPivot(new Vector2(sc, sc), Vector2.zero);
            float vw = REF_W;
            float vh = Screen.height / sc;

            if (_showWindow)
            {
                DrawWindow(vw, vh);
                DrawToast(vw, vh);
                // Eat all pointer events so NGUI doesn't see them
                if (Event.current.isMouse || Event.current.isKey)
                    Event.current.Use();
                return;
            }

            // ── "Mod Manager" button — top-right ──────────────────────────────
            float btnW = 110f, btnH = 26f;
            float btnX = vw - btnW - 6f;
            float btnY = 6f;
            int    updCnt  = _pendingUpdates != null ? _pendingUpdates.Count : 0;
            string mmLabel = updCnt > 0 ? "Mods (" + updCnt + " \u25cf)" : "Mod Manager";
            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), mmLabel,
                MakeBtnStyle(13, updCnt > 0 ? new Color(1f, 0.85f, 0.3f) : new Color(0.4f, 0.8f, 1f))))
            {
                if (Time.unscaledTime - _lastToggle > 0.3f)
                {
                    _lastToggle = Time.unscaledTime;
                    OpenWindow();
                }
            }
            DrawToast(vw, vh);
        }

        private void OpenWindow()
        {
            _showWindow    = true;
            ModManagerEntry.IsOpen = true;
            ModManagerEntry.SetEcoHookOpen(true);
            _scroll        = Vector2.zero;
            _swipeActive   = false;
            _swipeDragging = false;
            _statusMsg  = "";
            RefreshInstalledMods();
            if (_tab == 1 && _browseMods.Count == 0 && !_browseFetching)
                StartFetchBrowse();
        }

        // Called via reflection by ModManagerEntry.OpenWindow()
        public void OpenWindowPublic() { OpenWindow(); }

        // ─────────────────────────────────────────────────────────────────────
        // INSTALLED TAB
        // ─────────────────────────────────────────────────────────────────────
        private void RefreshInstalledMods()
        {
            _installedMods.Clear();
            _configTypeCache.Clear();
            _configTypeChecked.Clear();
            _configModIdx = -1;
            try
            {
                if (!Directory.Exists(ModManagerEntry.ModsDir)) return;
                foreach (string path in Directory.GetFiles(ModManagerEntry.ModsDir, "*.dll"))
                {
                    string fn  = Path.GetFileName(path);
                    string dn  = Path.GetFileNameWithoutExtension(fn);
                    string ver = GetInstalledVersion(fn);
                    if (string.IsNullOrEmpty(ver)) ver = "?";
                    InstalledMod im;
                    im.filename    = fn;
                    im.displayName = dn;
                    im.version     = ver;
                    _installedMods.Add(im);
                }
            }
            catch (Exception ex) { ModManagerEntry.Log("RefreshInstalled: " + ex.Message); }
        }

        private string GetRegisteredVersion(string name)
        {
            try
            {
                Assembly myAsm = typeof(ModManagerEntry).Assembly;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type me = asm.GetType("CNRMods.ModEntry");
                    if (me == null || me.Assembly == myAsm) continue;
                    FieldInfo fi = me.GetField("RegisteredMods",
                        BindingFlags.Public | BindingFlags.Static);
                    if (fi == null) continue;
                    System.Collections.IDictionary dict =
                        fi.GetValue(null) as System.Collections.IDictionary;
                    if (dict != null && dict.Contains(name))
                    {
                        object v = dict[name];
                        return v != null ? v.ToString() : null;
                    }
                }
            }
            catch { }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // REPOS
        // ─────────────────────────────────────────────────────────────────────
        private void LoadRepoList()
        {
            _repos.Clear();
            string saved = PlayerPrefs.GetString(PREF_REPOS, "");
            if (!string.IsNullOrEmpty(saved))
                foreach (string r in saved.Split('|'))
                    if (!string.IsNullOrEmpty(r)) _repos.Add(r);
            if (!_repos.Contains(ModManagerEntry.DefaultRepoUrl))
                _repos.Insert(0, ModManagerEntry.DefaultRepoUrl);
        }

        private void SaveRepoList()
        {
            PlayerPrefs.SetString(PREF_REPOS, string.Join("|", _repos.ToArray()));
            PlayerPrefs.Save();
        }

        // ─────────────────────────────────────────────────────────────────────
        // BROWSE / HTTP
        // ─────────────────────────────────────────────────────────────────────
        private void StartFetchBrowse()
        {
            if (_browseFetching) return;
            string repoUrl = _repos.Count > 0 ? _repos[0] : ModManagerEntry.DefaultRepoUrl;
            StartCoroutine(FetchBrowse(repoUrl));
        }

        private IEnumerator FetchBrowse(string url)
        {
            _browseFetching = true;
            _browseStatus   = "Fetching...";
            _browseMods.Clear();
            ModManagerEntry.Log("FetchBrowse: " + url);
            WWW www = new WWW(url);
            yield return www;
            _browseFetching = false;
            if (!string.IsNullOrEmpty(www.error))
            {
                string err = www.error;
                if (err.Contains("UnknownHost") || err.Contains("Unable to resolve") || err.Contains("No address"))
                    _browseStatus = "Connection error — check your internet connection.";
                else
                    _browseStatus = "Connection error: " + err;
                ModManagerEntry.Log("FetchBrowse err: " + www.error);
                yield break;
            }
            ParseRepoJson(www.text);
        }

        private void ParseRepoJson(string json)
        {
            try
            {
                int modsIdx = json.IndexOf("\"mods\"");
                if (modsIdx < 0) { _browseStatus = "Invalid repo: no 'mods' key"; return; }
                int arrStart = json.IndexOf('[', modsIdx);
                int arrEnd   = json.LastIndexOf(']');
                if (arrStart < 0 || arrEnd <= arrStart) { _browseStatus = "Invalid repo format"; return; }
                string arr = json.Substring(arrStart + 1, arrEnd - arrStart - 1);

                int depth = 0, objStart = -1;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] == '{')
                    {
                        if (depth == 0) objStart = i;
                        depth++;
                    }
                    else if (arr[i] == '}')
                    {
                        depth--;
                        if (depth == 0 && objStart >= 0)
                        {
                            string obj = arr.Substring(objStart, i - objStart + 1);
                            RepoMod mod;
                            mod.id            = ParseJsonStr(obj, "id");
                            mod.name          = ParseJsonStr(obj, "name");
                            mod.description   = ParseJsonStr(obj, "description");
                            string latVer     = ParseJsonStr(obj, "latestVersion");
                            mod.latestVersion = !string.IsNullOrEmpty(latVer) ? latVer : ParseJsonStr(obj, "version");
                            mod.minVersion    = ParseJsonStr(obj, "minVersion");
                            mod.filename      = ParseJsonStr(obj, "filename");
                            string latUrl     = ParseJsonStr(obj, "latestUrl");
                            mod.latestUrl     = !string.IsNullOrEmpty(latUrl) ? latUrl : ParseJsonStr(obj, "url");
                            mod.latestOnly    = ParseJsonBool(obj, "latestOnly");
                            mod.dependencies  = ParseModDependencies(obj);
                            mod.versions      = ParseModVersions(obj);
                            if (mod.versions.Count == 0 && !string.IsNullOrEmpty(mod.latestVersion))
                            {
                                ModVersion fallback;
                                fallback.version   = mod.latestVersion;
                                fallback.url       = mod.latestUrl;
                                fallback.changelog = "";
                                mod.versions.Add(fallback);
                            }
                            if (!string.IsNullOrEmpty(mod.filename)) _browseMods.Add(mod);
                            objStart = -1;
                        }
                    }
                }
                _browseStatus = _browseMods.Count > 0 ? "" : "No mods found in repo";
                ModManagerEntry.Log("ParseRepo: " + _browseMods.Count + " mods");
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                _browseStatus = "Parse error";
                ModManagerEntry.Log("ParseRepo err: " + ex.Message);
            }
        }

        private static string ParseJsonStr(string json, string key)
        {
            string k = "\"" + key + "\"";
            int ki = json.IndexOf(k);
            if (ki < 0) return "";
            int colon = json.IndexOf(':', ki + k.Length);
            if (colon < 0) return "";
            int vi = json.IndexOf('"', colon + 1);
            if (vi < 0) return "";
            int ei = json.IndexOf('"', vi + 1);
            if (ei < 0) return "";
            return json.Substring(vi + 1, ei - vi - 1).Replace("\\/", "/");
        }

        private static bool ParseJsonBool(string json, string key)
        {
            string k = "\"" + key + "\"";
            int ki = json.IndexOf(k);
            if (ki < 0) return false;
            int colon = json.IndexOf(':', ki + k.Length);
            if (colon < 0) return false;
            int vi = colon + 1;
            while (vi < json.Length && (json[vi] == ' ' || json[vi] == '\r' || json[vi] == '\n')) vi++;
            return vi < json.Length && json[vi] == 't';
        }

        private static List<ModVersion> ParseModVersions(string json)
        {
            var list = new List<ModVersion>();
            int idx = json.IndexOf("\"versions\"");
            if (idx < 0) return list;
            int arrStart = json.IndexOf('[', idx);
            int arrEnd   = FindArrayEnd(json, arrStart);
            if (arrStart < 0 || arrEnd <= arrStart) return list;
            string arr   = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
            int depth = 0, objStart = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == '{')      { if (depth == 0) objStart = i; depth++; }
                else if (arr[i] == '}')
                {
                    depth--;
                    if (depth == 0 && objStart >= 0)
                    {
                        string o = arr.Substring(objStart, i - objStart + 1);
                        ModVersion mv;
                        mv.version   = ParseJsonStr(o, "version");
                        mv.url       = ParseJsonStr(o, "url");
                        mv.changelog = ParseJsonStr(o, "changelog");
                        if (!string.IsNullOrEmpty(mv.version)) list.Add(mv);
                        objStart = -1;
                    }
                }
            }
            return list;
        }

        private static List<ModDependency> ParseModDependencies(string json)
        {
            var list = new List<ModDependency>();
            int idx = json.IndexOf("\"dependencies\"");
            if (idx < 0) return list;
            int arrStart = json.IndexOf('[', idx);
            int arrEnd = FindArrayEnd(json, arrStart);
            if (arrStart < 0 || arrEnd <= arrStart) return list;
            string arr = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
            int depth = 0, objStart = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == '{')
                {
                    if (depth == 0) objStart = i;
                    depth++;
                }
                else if (arr[i] == '}')
                {
                    depth--;
                    if (depth == 0 && objStart >= 0)
                    {
                        string o = arr.Substring(objStart, i - objStart + 1);
                        ModDependency dep;
                        dep.kind = ParseJsonStr(o, "kind");
                        if (string.IsNullOrEmpty(dep.kind)) dep.kind = "mod";
                        dep.id = ParseJsonStr(o, "id");
                        dep.filename = ParseJsonStr(o, "filename");
                        dep.url = ParseJsonStr(o, "url");
                        dep.minVersion = ParseJsonStr(o, "minVersion");
                        dep.latestOnly = ParseJsonBool(o, "latestOnly");
                        if (!string.IsNullOrEmpty(dep.id) || !string.IsNullOrEmpty(dep.filename) || !string.IsNullOrEmpty(dep.url))
                            list.Add(dep);
                        objStart = -1;
                    }
                }
            }
            return list;
        }

        private static int FindArrayEnd(string s, int start)
        {
            if (start < 0 || start >= s.Length || s[start] != '[') return -1;
            int depth = 0;
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == '[') depth++;
                else if (s[i] == ']') { depth--; if (depth == 0) return i; }
            }
            return -1;
        }

        private IEnumerator DownloadMod(string displayName, string filename, string url, string version)
        {
            _statusMsg = "Downloading " + displayName + "...";
            string dest = Path.Combine(ModManagerEntry.ModsDir, filename);
            ModManagerEntry.Log("Download: " + url + " -> " + dest);
            WWW www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                _statusMsg = "Download error: " + www.error;
                ModManagerEntry.Log("Download err: " + www.error);
                yield break;
            }
            try
            {
                if (!Directory.Exists(ModManagerEntry.ModsDir))
                    Directory.CreateDirectory(ModManagerEntry.ModsDir);
                File.WriteAllBytes(dest, www.bytes);
                ModManagerEntry.Log("Download OK: " + dest + " (" + www.bytes.Length + " bytes)");
                if (!string.IsNullOrEmpty(version))
                {
                    PlayerPrefs.SetString(PREF_IV_PREFIX + filename, version);
                    PlayerPrefs.SetInt(PREF_AU_PREFIX + filename, 1);
                    PlayerPrefs.Save();
                }
                RefreshInstalledMods();
                _batchNeedsRestart = true;
                _statusMsg  = "";
                _toastMsg   = displayName + " installed — restart game to apply!";
                _toastUntil = Time.unscaledTime + 4f;
            }
            catch (Exception ex)
            {
                _statusMsg = "Save error: " + ex.Message;
                ModManagerEntry.Log("Download save err: " + ex.Message);
            }
        }

        private IEnumerator InstallRepoMod(RepoMod mod)
        {
            yield return StartCoroutine(InstallDependencies(mod));
            yield return StartCoroutine(DownloadMod(mod.name, mod.filename, mod.latestUrl, mod.latestVersion));
        }

        private IEnumerator InstallDependencies(RepoMod mod)
        {
            if (mod.dependencies == null || mod.dependencies.Count == 0) yield break;
            for (int i = 0; i < mod.dependencies.Count; i++)
            {
                ModDependency dep = mod.dependencies[i];
                if (string.Equals(dep.kind, "file", StringComparison.OrdinalIgnoreCase))
                    yield return StartCoroutine(DownloadDependencyFile(mod, dep));
                else
                    yield return StartCoroutine(DownloadDependencyMod(mod, dep));
            }
        }

        private IEnumerator DownloadDependencyFile(RepoMod owner, ModDependency dep)
        {
            string destName = !string.IsNullOrEmpty(dep.filename) ? dep.filename : Path.GetFileName(dep.url);
            if (string.IsNullOrEmpty(destName) || string.IsNullOrEmpty(dep.url)) yield break;
            string dest = Path.Combine(ModManagerEntry.ModsDir, destName);
            if (File.Exists(dest))
            {
                ModManagerEntry.Log("Dependency file already present: " + destName);
                yield break;
            }
            _statusMsg = "Downloading dependency " + destName + "...";
            ModManagerEntry.Log("Dependency file download: " + dep.url + " -> " + dest);
            WWW www = new WWW(dep.url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                _statusMsg = "Dependency download error: " + www.error;
                ModManagerEntry.Log("Dependency file err: " + www.error);
                yield break;
            }
            try
            {
                if (!Directory.Exists(ModManagerEntry.ModsDir))
                    Directory.CreateDirectory(ModManagerEntry.ModsDir);
                File.WriteAllBytes(dest, www.bytes);
                ModManagerEntry.Log("Dependency file OK: " + dest + " (" + www.bytes.Length + " bytes)");
            }
            catch (Exception ex)
            {
                _statusMsg = "Dependency save error: " + ex.Message;
                ModManagerEntry.Log("Dependency file save err: " + ex.Message);
            }
        }

        private IEnumerator DownloadDependencyMod(RepoMod owner, ModDependency dep)
        {
            RepoMod target;
            if (!FindRepoMod(dep, out target))
            {
                ModManagerEntry.Log("Dependency mod not found for " + owner.filename + " dep id=" + dep.id);
                yield break;
            }
            string targetVersion = SelectDependencyVersion(target, dep);
            string targetUrl = ResolveDependencyUrl(target, targetVersion);
            string destPath = Path.Combine(ModManagerEntry.ModsDir, target.filename);
            if (File.Exists(destPath))
            {
                string iv = GetInstalledVersion(target.filename);
                string effectiveMin = !string.IsNullOrEmpty(dep.minVersion) ? dep.minVersion
                                    : (dep.latestOnly ? target.latestVersion ?? "" : "");
                if (string.IsNullOrEmpty(effectiveMin) || (!string.IsNullOrEmpty(iv) && CompareVersions(iv, effectiveMin) >= 0))
                {
                    ModManagerEntry.Log("Dependency mod already installed: " + target.filename);
                    yield break;
                }
            }
            yield return StartCoroutine(DownloadMod(target.name, target.filename, targetUrl, targetVersion));
        }

        private bool FindRepoMod(ModDependency dep, out RepoMod found)
        {
            for (int i = 0; i < _browseMods.Count; i++)
            {
                RepoMod rm = _browseMods[i];
                if (!string.IsNullOrEmpty(dep.id) && rm.id.Equals(dep.id, StringComparison.OrdinalIgnoreCase))
                {
                    found = rm;
                    return true;
                }
                if (!string.IsNullOrEmpty(dep.filename) && rm.filename.Equals(dep.filename, StringComparison.OrdinalIgnoreCase))
                {
                    found = rm;
                    return true;
                }
            }
            found = default(RepoMod);
            return false;
        }

        private string SelectDependencyVersion(RepoMod mod, ModDependency dep)
        {
            if (dep.latestOnly || string.IsNullOrEmpty(dep.minVersion)) return mod.latestVersion;
            string best = "";
            for (int i = 0; i < mod.versions.Count; i++)
            {
                ModVersion mv = mod.versions[i];
                if (string.IsNullOrEmpty(mv.version)) continue;
                if (CompareVersions(mv.version, dep.minVersion) < 0) continue;
                if (string.IsNullOrEmpty(best) || CompareVersions(mv.version, best) > 0)
                    best = mv.version;
            }
            if (string.IsNullOrEmpty(best)) best = mod.latestVersion;
            return best;
        }

        private string ResolveDependencyUrl(RepoMod mod, string version)
        {
            if (!string.IsNullOrEmpty(version))
            {
                for (int i = 0; i < mod.versions.Count; i++)
                    if (mod.versions[i].version == version && !string.IsNullOrEmpty(mod.versions[i].url))
                        return mod.versions[i].url;
            }
            return mod.latestUrl;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DRAWING
        // ─────────────────────────────────────────────────────────────────────
        // AUTO-UPDATE HELPERS
        private bool GetAutoUpdate(string filename)
        {
            return PlayerPrefs.GetInt(PREF_AU_PREFIX + filename, 1) != 0;
        }

        private void SetAutoUpdate(string filename, bool val)
        {
            PlayerPrefs.SetInt(PREF_AU_PREFIX + filename, val ? 1 : 0);
            PlayerPrefs.Save();
        }

        private string GetInstalledVersion(string filename)
        {
            // 1. CNRMod's RegisteredMods reflection dict — live running version, always beats cache
            string dn  = Path.GetFileNameWithoutExtension(filename);
            string reg = GetRegisteredVersion(dn);
            if (!string.IsNullOrEmpty(reg))
            {
                // Keep PlayerPrefs in sync so future lookups (before next load) are accurate
                PlayerPrefs.SetString(PREF_IV_PREFIX + filename, reg);
                return reg;
            }

            // 2. Scan the DLL assembly itself for a public static/const Version string.
            // Manual file replacement does not update PlayerPrefs, so the DLL must beat
            // the cached "last installed by Mod Manager" version.
            try
            {
                string dllPath = Path.Combine(ModManagerEntry.ModsDir, filename);
                if (File.Exists(dllPath))
                {
                    Assembly asm = Assembly.Load(File.ReadAllBytes(dllPath));
                    foreach (Type t in asm.GetTypes())
                    {
                        // const string Version = "x.y.z"  (IsLiteral = true for const)
                        FieldInfo fi = t.GetField("Version",
                            BindingFlags.Public | BindingFlags.Static);
                        if (fi != null && fi.FieldType == typeof(string))
                        {
                            object v = fi.GetValue(null);
                            if (v != null && !string.IsNullOrEmpty(v.ToString()))
                            {
                                string dllVersion = v.ToString();
                                PlayerPrefs.SetString(PREF_IV_PREFIX + filename, dllVersion);
                                return dllVersion;
                            }
                        }
                    }
                }
            }
            catch { }

            // 3. PlayerPrefs is only a last-resort fallback for older mods that do not
            // expose a Version field and have not registered with CNRMod yet.
            string stored = PlayerPrefs.GetString(PREF_IV_PREFIX + filename, "");
            if (!string.IsNullOrEmpty(stored)) return stored;

            return "";
        }

        private string GetMinVersionFromRepo(string filename)
        {
            foreach (RepoMod rm in _browseMods)
                if (rm.filename.Equals(filename, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(rm.minVersion)) return rm.minVersion;
                    if (rm.latestOnly) return rm.latestVersion ?? "";
                    return "";
                }
            return "";
        }

        private string GetLatestVersionFromRepo(string filename)
        {
            foreach (RepoMod rm in _browseMods)
                if (rm.filename.Equals(filename, StringComparison.OrdinalIgnoreCase))
                    return rm.latestVersion;
            return "";
        }

        private void CheckForUpdates()
        {
            _pendingUpdates.Clear();
            foreach (RepoMod mod in _browseMods)
            {
                // Effective minimum: explicit minVersion, or latestVersion when latestOnly=true
                string effectiveMin = !string.IsNullOrEmpty(mod.minVersion) ? mod.minVersion
                                    : (mod.latestOnly ? mod.latestVersion ?? "" : "");
                bool installed = File.Exists(Path.Combine(ModManagerEntry.ModsDir, mod.filename));
                // Required but not installed at all — must download
                if (!installed)
                {
                    if (!string.IsNullOrEmpty(effectiveMin))
                        _pendingUpdates.Add(mod);
                    continue;
                }
                string iv = GetInstalledVersion(mod.filename);
                // Always queue if below effective minimum (regardless of auto-update setting)
                bool belowMin = !string.IsNullOrEmpty(effectiveMin)
                                && !string.IsNullOrEmpty(iv)
                                && CompareVersions(iv, effectiveMin) < 0;
                bool outdated = !string.IsNullOrEmpty(iv) && iv != mod.latestVersion;
                if (belowMin || (outdated && GetAutoUpdate(mod.filename)))
                    _pendingUpdates.Add(mod);
            }
            _updateBannerDismissed = false;
            // Update the multiplayer gate flag used by CNRMod
            HasRequiredPending = false;
            foreach (RepoMod mod in _pendingUpdates)
            {
                string effectiveMin3 = !string.IsNullOrEmpty(mod.minVersion) ? mod.minVersion
                                     : (mod.latestOnly ? mod.latestVersion ?? "" : "");
                bool notInstalled = !File.Exists(Path.Combine(ModManagerEntry.ModsDir, mod.filename));
                string iv3 = GetInstalledVersion(mod.filename);
                bool belowMin3 = !string.IsNullOrEmpty(effectiveMin3)
                                 && !string.IsNullOrEmpty(iv3)
                                 && CompareVersions(iv3, effectiveMin3) < 0;
                if (notInstalled || belowMin3) { HasRequiredPending = true; break; }
            }
            ModManagerEntry.Log("CheckForUpdates: " + _pendingUpdates.Count + " update(s)  HasRequiredPending=" + HasRequiredPending);
        }

        // Returns -1 / 0 / +1  (a < b / a == b / a > b)  for semver strings.
        private static int CompareVersions(string a, string b)
        {
            if (a == b) return 0;
            string[] pa = a.TrimStart('v').Split('.');
            string[] pb = b.TrimStart('v').Split('.');
            int len = Math.Max(pa.Length, pb.Length);
            for (int i = 0; i < len; i++)
            {
                int na, nb;
                na = i < pa.Length && int.TryParse(pa[i], out na) ? na : 0;
                nb = i < pb.Length && int.TryParse(pb[i], out nb) ? nb : 0;
                if (na != nb) return na < nb ? -1 : 1;
            }
            return 0;
        }

        private void StartBatchUpdate()
        {
            if (_batchRunning) return;
            _batchQueue.Clear();
            foreach (RepoMod m in _pendingUpdates) _batchQueue.Add(m);
            _batchTotal        = _batchQueue.Count;
            _batchDone         = 0;
            _batchNeedsRestart = false;
            StartCoroutine(BatchUpdateCoroutine());
        }

        private IEnumerator BatchUpdateCoroutine()
        {
            _batchRunning = true;
            for (int i = 0; i < _batchQueue.Count; i++)
            {
                RepoMod mod = _batchQueue[i];
                yield return StartCoroutine(InstallRepoMod(mod));
                _batchDone++;
            }
            _batchRunning          = false;
            _batchNeedsRestart     = true;
            _updateBannerDismissed = true;
            _pendingUpdates.Clear();
            _statusMsg  = "";
            _toastMsg   = "All mods updated \u2014 restart game to apply!";
            _toastUntil = Time.unscaledTime + 6f;
        }

        private void DrawToast(float vw, float vh)
        {
            if (string.IsNullOrEmpty(_toastMsg)) return;
            if (Time.unscaledTime >= _toastUntil) { _toastMsg = ""; return; }
            float alpha = Mathf.Clamp01((_toastUntil - Time.unscaledTime) / 0.8f);
            float th = 40f, tw = Mathf.Min(vw - 40f, 400f);
            float tx = (vw - tw) * 0.5f, ty = vh - th - 24f;
            Color prev = GUI.color;
            GUI.color = new Color(0.06f, 0.38f, 0.14f, alpha * 0.95f);
            GUI.DrawTexture(new Rect(tx, ty, tw, th), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUIStyle tst = MakeLabelStyle(13, Color.white);
            tst.alignment = TextAnchor.MiddleCenter;
            tst.wordWrap  = false;
            GUI.Label(new Rect(tx, ty, tw, th), _toastMsg, tst);
            GUI.color = prev;
        }

        private void DrawWindow(float vw, float vh)
        {
            // Dim overlay
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;
            // (NGUI cameras are disabled while open, so no blocker button needed —
            //  a full-screen invisible Button drawn first would steal all hotControl and
            //  prevent every real button in the window from ever receiving clicks.)

            float winW = Mathf.Min(560f, vw - 20f);
            float winH = vh - 40f;
            float winX = (vw - winW) * 0.5f;
            float winY = 20f;

            // Window background
            GUI.color = new Color(0.10f, 0.12f, 0.18f, 1f);
            GUI.DrawTexture(new Rect(winX, winY, winW, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Title
            GUIStyle titleSt = MakeLabelStyle(16, Color.white);
            titleSt.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(winX + 10f, winY + 5f, winW - 80f, 26f),
                "Mod Manager  v" + ModManagerEntry.Version, titleSt);

            // Close button
            if (GUI.Button(new Rect(winX + winW - 68f, winY + 5f, 60f, 24f),
                "Close", MakeBtnStyle(14, new Color(1f, 0.4f, 0.4f))))
            {
                _showWindow            = false;
                ModManagerEntry.IsOpen = false;
                ModManagerEntry.SetEcoHookOpen(false);
                _detailModIdx          = -1;
                return;
            }

            // Tab row
            float tabY = winY + 36f;
            float tabH = 26f;
            float tabW = winW / 3f;
            string[] tabNames = { "Installed", "Browse", "Repos" };
            for (int i = 0; i < 3; i++)
            {
                bool active = _tab == i;
                GUIStyle tabSt = MakeBtnStyle(13,
                    active ? new Color(0.4f, 0.8f, 1f) : new Color(0.55f, 0.55f, 0.55f));
                if (active) tabSt.fontStyle = FontStyle.Bold;
                if (GUI.Button(new Rect(winX + tabW * i, tabY, tabW, tabH), tabNames[i], tabSt))
                {
                    if (_tab != i) { _tab = i; _scroll = Vector2.zero; _statusMsg = ""; _detailModIdx = -1; }
                    if (_tab == 1 && _browseMods.Count == 0 && !_browseFetching)
                        StartFetchBrowse();
                }
            }

            // Consume pointer events during an active swipe so no button fires at drag-end
            if (_swipeDragging && (Event.current.type == EventType.MouseDown ||
                                   Event.current.type == EventType.MouseUp))
            {
                if (Event.current.type == EventType.MouseUp) _swipeDragging = false;
                Event.current.Use();
            }

            // Content area
            float cY = tabY + tabH + 6f;
            float cH = winH - (cY - winY) - 10f;
            GUILayout.BeginArea(new Rect(winX + 6f, cY, winW - 12f, cH));
            // Invisible scrollbar — swipe is the only scroll mechanism
            _scroll = GUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none, GUIStyle.none);

            if      (_tab == 0) DrawInstalledTab();
            else if (_tab == 1) DrawBrowseTab();
            else                DrawReposTab();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawUpdateBanner()
        {
            if (_pendingUpdates.Count > 0 && !_batchRunning && !_updateBannerDismissed)
            {
                int reqCount  = 0;
                int optCount  = 0;
                foreach (RepoMod pu in _pendingUpdates)
                {
                    bool notInstalled = !File.Exists(Path.Combine(ModManagerEntry.ModsDir, pu.filename));
                    string iv2 = GetInstalledVersion(pu.filename);
                    string effectiveMin2 = !string.IsNullOrEmpty(pu.minVersion) ? pu.minVersion
                                         : (pu.latestOnly ? pu.latestVersion ?? "" : "");
                    bool belowMin2 = !string.IsNullOrEmpty(effectiveMin2)
                                     && !string.IsNullOrEmpty(iv2)
                                     && CompareVersions(iv2, effectiveMin2) < 0;
                    if (notInstalled || belowMin2) reqCount++; else optCount++;
                }
                string bannerText = reqCount > 0 && optCount == 0
                    ? reqCount + " required mod(s) to install"
                    : reqCount > 0
                        ? _pendingUpdates.Count + " mod(s) need attention"
                        : _pendingUpdates.Count + " update(s) available";

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUIStyle bannerSt = MakeLabelStyle(13, reqCount > 0 ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.85f, 0.3f));
                bannerSt.fontStyle = FontStyle.Bold;
                GUILayout.Label(bannerText, bannerSt);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Install All", MakeBtnStyle(13, new Color(0.3f, 0.9f, 0.5f)),
                    GUILayout.Height(26f), GUILayout.Width(90f)))
                    StartBatchUpdate();
                if (reqCount == 0)
                {
                    if (GUILayout.Button("x", MakeBtnStyle(13, new Color(0.6f, 0.6f, 0.6f)),
                        GUILayout.Height(26f), GUILayout.Width(26f)))
                        _updateBannerDismissed = true;
                }
                GUILayout.EndHorizontal();
                foreach (RepoMod pu in _pendingUpdates)
                {
                    bool notInstalled2 = !File.Exists(Path.Combine(ModManagerEntry.ModsDir, pu.filename));
                    string rowLabel = (notInstalled2 ? "  + " : "  \u2191 ") + pu.name + "  \u2192  v" + pu.latestVersion;
                    GUILayout.Label(rowLabel, MakeLabelStyle(11, new Color(0.7f, 0.7f, 0.75f)));
                }
                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }
            if (_batchRunning)
            {
                GUILayout.Label("Updating... (" + _batchDone + " / " + _batchTotal + ")",
                    MakeLabelStyle(13, new Color(0.4f, 0.8f, 1f)));
                GUILayout.Space(4f);
            }
            if (_batchNeedsRestart && !_batchRunning)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("All mods updated!", MakeLabelStyle(13, new Color(0.4f, 1f, 0.5f)));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Restart Game", MakeBtnStyle(13, new Color(1f, 0.4f, 0.4f)),
                    GUILayout.Height(26f), GUILayout.Width(90f)))
                    RestartApp();
                GUILayout.EndHorizontal();
                GUILayout.Space(4f);
            }
        }

        private void DrawInstalledTab()
        {
            // If a mod's config panel is open, show it instead of the list
            if (_configModIdx >= 0 && _configModIdx < _installedMods.Count)
            {
                DrawConfigPanel();
                return;
            }

            DrawUpdateBanner();
            GUILayout.Label("DLLs in /sdcard/CNRMods/",
                MakeLabelStyle(12, new Color(0.6f, 0.6f, 0.7f)));
            GUILayout.Space(4f);
            if (GUILayout.Button("Refresh", MakeBtnStyle(12, new Color(0.5f, 0.8f, 0.5f)),
                GUILayout.Height(24f), GUILayout.Width(80f)))
                RefreshInstalledMods();
            GUILayout.Space(6f);

            if (_installedMods.Count == 0)
            {
                GUILayout.Label("No DLLs found.", MakeLabelStyle(13, new Color(0.8f, 0.8f, 0.8f)));
            }
            else
            {
                for (int imIdx = 0; imIdx < _installedMods.Count; imIdx++)
                {
                    InstalledMod m = _installedMods[imIdx];
                    string latestVer  = GetLatestVersionFromRepo(m.filename);
                    string minVer     = GetMinVersionFromRepo(m.filename);
                    string instdVer   = GetInstalledVersion(m.filename);
                    bool belowMin     = !string.IsNullOrEmpty(minVer)
                                       && !string.IsNullOrEmpty(instdVer)
                                       && CompareVersions(instdVer, minVer) < 0;
                    bool hasUpdate    = !string.IsNullOrEmpty(latestVer)
                                       && !string.IsNullOrEmpty(instdVer)
                                       && instdVer != latestVer;
                    bool core = m.filename.Equals("CNRMod.dll", StringComparison.OrdinalIgnoreCase);

                    GUILayout.BeginVertical(GUI.skin.box);

                    // Row 1: name, version, update badge, action
                    GUILayout.BeginHorizontal();
                    GUIStyle ns = MakeLabelStyle(13, Color.white);
                    ns.fontStyle = FontStyle.Bold;
                    GUILayout.Label(m.displayName, ns, GUILayout.Width(160f));
                    GUILayout.Label("v" + m.version,
                        MakeLabelStyle(12, new Color(0.7f, 0.9f, 0.7f)), GUILayout.Width(60f));
                    GUILayout.FlexibleSpace();
                    if (hasUpdate || belowMin)
                        GUILayout.Label(
                            belowMin ? "v" + latestVer + " REQUIRED" : "v" + latestVer + " avail",
                            MakeLabelStyle(11, belowMin ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.75f, 0.3f)));
                    if (core)
                    {
                        GUILayout.Label("(core)", MakeLabelStyle(11, new Color(0.55f, 0.55f, 0.55f)),
                            GUILayout.Width(50f));
                    }
                    else
                    {
                        string fn = m.filename;
                        if (GUILayout.Button("Remove",
                            MakeBtnStyle(11, new Color(1f, 0.4f, 0.4f)),
                            GUILayout.Height(22f), GUILayout.Width(70f)))
                        {
                            try
                            {
                                File.Delete(Path.Combine(ModManagerEntry.ModsDir, fn));
                                RefreshInstalledMods();
                                _batchNeedsRestart = true;
                                _statusMsg = fn + " removed. Restart app.";
                            }
                            catch (Exception ex) { _statusMsg = "Remove failed: " + ex.Message; }
                        }
                    }
                    GUILayout.EndHorizontal();

                    // Row 2: auto-update checkbox (non-core only)
                    if (!core)
                    {
                        bool au    = GetAutoUpdate(m.filename);
                        bool newAu = GUILayout.Toggle(au, " Auto-update",
                            MakeToggleStyle(11, new Color(0.6f, 0.6f, 0.65f)));
                        if (newAu != au) { SetAutoUpdate(m.filename, newAu); CheckForUpdates(); }
                    }

                    // Row 3: settings button (shown if mod exposes GetModConfig)
                    Type cfgType = GetCachedConfigType(m.filename);
                    if (cfgType != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        int captureIdx = imIdx;
                        if (GUILayout.Button("\u2699 Settings",
                            MakeBtnStyle(11, new Color(0.6f, 0.8f, 1f)),
                            GUILayout.Height(22f), GUILayout.Width(90f)))
                        {
                            _configModIdx       = captureIdx;
                            _configType         = cfgType;
                            _configEntries      = LoadConfigEntries(cfgType);
                            _configDirty        = false;
                            _configNeedsRestart = false;
                            _scroll             = Vector2.zero;
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(3f);
                }
            }

            if (!string.IsNullOrEmpty(_statusMsg))
            {
                GUILayout.Space(6f);
                GUILayout.Label(_statusMsg, MakeLabelStyle(12, new Color(1f, 0.9f, 0.5f)));
            }
        }

        private void DrawBrowseTab()
        {
            // Detail view for a selected mod
            if (_detailModIdx >= 0 && _detailModIdx < _browseMods.Count)
            {
                DrawModDetail(_browseMods[_detailModIdx]);
                return;
            }

            GUILayout.BeginHorizontal();
            string repoLabel = _repos.Count > 0 ? _repos[0] : "(none)";
            GUILayout.Label("Repo: " + repoLabel, MakeLabelStyle(11, new Color(0.55f, 0.55f, 0.65f)));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", MakeBtnStyle(12, new Color(0.5f, 0.8f, 0.5f)),
                GUILayout.Height(24f), GUILayout.Width(70f)))
            {
                _browseMods.Clear();
                _browseStatus = "";
                StartFetchBrowse();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);

            DrawUpdateBanner();

            if (_browseFetching)
            {
                GUILayout.Label("Fetching mod list...", MakeLabelStyle(13, new Color(0.7f, 0.9f, 1f)));
                return;
            }
            if (!string.IsNullOrEmpty(_browseStatus))
            {
                GUILayout.Label(_browseStatus, MakeLabelStyle(13, new Color(1f, 0.6f, 0.4f)));
                return;
            }
            if (_browseMods.Count == 0)
            {
                GUILayout.Label("No mods yet. Press Refresh.",
                    MakeLabelStyle(13, new Color(0.8f, 0.8f, 0.8f)));
                return;
            }

            for (int bi = 0; bi < _browseMods.Count; bi++)
            {
                RepoMod mod = _browseMods[bi];
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUIStyle ns = MakeLabelStyle(14, Color.white);
                ns.fontStyle = FontStyle.Bold;
                GUILayout.Label(mod.name, ns);
                GUILayout.FlexibleSpace();
                GUILayout.Label("v" + mod.latestVersion, MakeLabelStyle(12, new Color(0.7f, 0.9f, 0.7f)));
                GUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(mod.description))
                {
                    string brief = mod.description.Length > 85
                        ? mod.description.Substring(0, 82) + "..."
                        : mod.description;
                    GUILayout.Label(brief, MakeLabelStyle(12, new Color(0.8f, 0.8f, 0.8f)));
                }

                bool installed    = File.Exists(Path.Combine(ModManagerEntry.ModsDir, mod.filename));
                string installedVer = installed ? GetInstalledVersion(mod.filename) : "";
                bool isUpToDate   = installed && !string.IsNullOrEmpty(installedVer)
                                    && installedVer == mod.latestVersion;

                // Auto-update toggle row (only when installed)
                if (installed)
                {
                    bool au = GetAutoUpdate(mod.filename);
                    GUILayout.BeginHorizontal();
                    bool newAu = GUILayout.Toggle(au, " Auto-update",
                        MakeToggleStyle(11, new Color(0.65f, 0.65f, 0.7f)));
                    if (newAu != au) { SetAutoUpdate(mod.filename, newAu); CheckForUpdates(); }
                    if (!string.IsNullOrEmpty(installedVer))
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("v" + installedVer + " installed",
                            MakeLabelStyle(11, isUpToDate
                                ? new Color(0.4f, 1f, 0.5f)
                                : new Color(1f, 0.75f, 0.3f)));
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                int captureIdx = bi;
                if (GUILayout.Button("Details", MakeBtnStyle(12, new Color(0.55f, 0.7f, 1f)),
                    GUILayout.Height(24f), GUILayout.Width(66f)))
                {
                    _detailModIdx = captureIdx;
                    _scroll       = Vector2.zero;
                }
                string capName = mod.name;
                string capFile = mod.filename;
                string capUrl  = mod.latestUrl;
                string capVer  = mod.latestVersion;
                if (isUpToDate)
                {
                    GUI.enabled = false;
                    GUILayout.Button("Up to date",
                        MakeBtnStyle(12, new Color(0.4f, 0.55f, 0.4f)),
                        GUILayout.Height(24f), GUILayout.Width(78f));
                    GUI.enabled = true;
                }
                else
                {
                    string instLabel = installed ? "Update" : "Install";
                    Color  instCol   = installed ? new Color(0.4f, 0.8f, 1f) : new Color(0.3f, 0.9f, 0.5f);
                    if (GUILayout.Button(instLabel, MakeBtnStyle(12, instCol),
                        GUILayout.Height(24f), GUILayout.Width(68f)))
                        StartCoroutine(InstallRepoMod(mod));
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }

            if (!string.IsNullOrEmpty(_statusMsg))
            {
                GUILayout.Space(4f);
                GUILayout.Label(_statusMsg, MakeLabelStyle(12, new Color(1f, 0.9f, 0.5f)));
            }
        }

        private void DrawModDetail(RepoMod mod)
        {
            if (GUILayout.Button("< Back", MakeBtnStyle(13, new Color(0.55f, 0.7f, 1f)),
                GUILayout.Height(26f), GUILayout.Width(80f)))
            {
                _detailModIdx = -1;
                _scroll       = Vector2.zero;
                return;
            }
            GUILayout.Space(8f);

            // Title + installed badge
            GUILayout.BeginHorizontal();
            GUIStyle titleSt = MakeLabelStyle(16, Color.white);
            titleSt.fontStyle = FontStyle.Bold;
            GUILayout.Label(mod.name, titleSt);
            GUILayout.FlexibleSpace();
            bool installedMod = File.Exists(Path.Combine(ModManagerEntry.ModsDir, mod.filename));
            if (installedMod)
                GUILayout.Label("Installed", MakeLabelStyle(13, new Color(0.4f, 1f, 0.5f)),
                    GUILayout.Width(72f));
            GUILayout.EndHorizontal();

            // Auto-update toggle
            if (installedMod)
            {
                bool au = GetAutoUpdate(mod.filename);
                bool newAu = GUILayout.Toggle(au, " Auto-update when updates are available",
                    MakeToggleStyle(12, new Color(0.65f, 0.65f, 0.7f)));
                if (newAu != au) { SetAutoUpdate(mod.filename, newAu); CheckForUpdates(); }
            }
            GUILayout.Space(6f);

            // Full description
            if (!string.IsNullOrEmpty(mod.description))
            {
                GUIStyle descSt = MakeLabelStyle(13, new Color(0.85f, 0.85f, 0.85f));
                descSt.wordWrap = true;
                GUILayout.Label(mod.description, descSt);
                GUILayout.Space(12f);
            }

            // Version history
            GUIStyle secHdr = MakeLabelStyle(13, new Color(0.45f, 0.75f, 1f));
            secHdr.fontStyle = FontStyle.Bold;
            GUILayout.Label("Version History", secHdr);
            GUILayout.Space(4f);

            if (mod.versions == null || mod.versions.Count == 0)
            {
                GUILayout.Label("No version history available.",
                    MakeLabelStyle(12, new Color(0.6f, 0.6f, 0.6f)));
            }
            else
            {
                for (int vi = 0; vi < mod.versions.Count; vi++)
                {
                    ModVersion ver    = mod.versions[vi];
                    bool isLatest     = ver.version == mod.latestVersion;

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();

                    Color verCol = isLatest ? new Color(0.4f, 1f, 0.5f) : new Color(0.8f, 0.8f, 0.8f);
                    GUIStyle verSt = MakeLabelStyle(13, verCol);
                    if (isLatest) verSt.fontStyle = FontStyle.Bold;
                    GUILayout.Label("v" + ver.version + (isLatest ? "  (latest)" : ""), verSt);
                    GUILayout.FlexibleSpace();

                    bool belowMin = !string.IsNullOrEmpty(mod.minVersion)
                        && CompareVersions(ver.version, mod.minVersion) < 0;

                    if (mod.latestOnly && !isLatest)
                    {
                        GUILayout.Label("(latest only)",
                            MakeLabelStyle(11, new Color(0.5f, 0.5f, 0.55f)));
                    }
                    else if (belowMin)
                    {
                        GUILayout.Label("< min v" + mod.minVersion,
                            MakeLabelStyle(11, new Color(0.8f, 0.3f, 0.3f)));
                    }
                    else
                    {
                        string detInstalledVer = GetInstalledVersion(mod.filename);
                        bool isThisVerInstalled = isLatest && installedMod
                            && !string.IsNullOrEmpty(detInstalledVer)
                            && detInstalledVer == mod.latestVersion;
                        string vUrl = ver.url;
                        string vTag = ver.version;
                        if (isThisVerInstalled)
                        {
                            GUI.enabled = false;
                            GUILayout.Button("Up to date",
                                MakeBtnStyle(12, new Color(0.4f, 0.55f, 0.4f)),
                                GUILayout.Height(24f), GUILayout.Width(82f));
                            GUI.enabled = true;
                        }
                        else
                        {
                            string btnLbl = isLatest ? (installedMod ? "Update" : "Install") : "Download";
                            Color  btnClr = isLatest
                                ? (installedMod ? new Color(0.4f, 0.8f, 1f) : new Color(0.3f, 0.9f, 0.5f))
                                : new Color(0.65f, 0.65f, 0.75f);
                            if (GUILayout.Button(btnLbl, MakeBtnStyle(12, btnClr),
                                GUILayout.Height(24f), GUILayout.Width(82f)))
                                StartCoroutine(InstallRepoMod(mod));
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (!string.IsNullOrEmpty(ver.changelog))
                    {
                        GUIStyle clSt = MakeLabelStyle(11, new Color(0.7f, 0.7f, 0.72f));
                        clSt.wordWrap = true;
                        GUILayout.Label(ver.changelog, clSt);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(3f);
                }
            }

            if (!string.IsNullOrEmpty(_statusMsg))
            {
                GUILayout.Space(6f);
                GUILayout.Label(_statusMsg, MakeLabelStyle(12, new Color(1f, 0.9f, 0.5f)));
            }
        }

        private void DrawReposTab()
        {
            GUILayout.Label("Mod Repositories", MakeLabelStyle(13, new Color(0.6f, 0.6f, 0.7f)));
            GUILayout.Space(6f);

            bool changed = false;
            for (int i = 0; i < _repos.Count; i++)
            {
                bool isDefault = _repos[i] == ModManagerEntry.DefaultRepoUrl;
                GUILayout.BeginHorizontal();
                string prefix = isDefault ? "[Default] " : "";
                Color  col    = isDefault ? new Color(0.5f, 0.9f, 1f) : Color.white;
                GUILayout.Label(prefix + _repos[i], MakeLabelStyle(11, col));
                GUILayout.FlexibleSpace();
                if (!isDefault)
                {
                    if (GUILayout.Button("Remove", MakeBtnStyle(11, new Color(1f, 0.4f, 0.4f)),
                        GUILayout.Height(22f), GUILayout.Width(70f)))
                    {
                        _repos.RemoveAt(i);
                        changed = true;
                        GUILayout.EndHorizontal();
                        break;
                    }
                    if (GUILayout.Button("Use", MakeBtnStyle(11, new Color(0.4f, 0.8f, 1f)),
                        GUILayout.Height(22f), GUILayout.Width(46f)))
                    {
                        string r = _repos[i];
                        _repos.RemoveAt(i);
                        _repos.Insert(0, r);
                        changed = true;
                        _browseMods.Clear();
                        _browseStatus = "";
                        GUILayout.EndHorizontal();
                        break;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            if (changed) SaveRepoList();

            GUILayout.Space(10f);
            GUILayout.Label("Add repo URL:", MakeLabelStyle(12, new Color(0.6f, 0.6f, 0.7f)));
            GUILayout.BeginHorizontal();
            GUIStyle inputSt = new GUIStyle(GUI.skin.textField);
            if (_font != null) inputSt.font = _font;
            inputSt.fontSize = 12;
            _newRepoInput = GUILayout.TextField(_newRepoInput, inputSt, GUILayout.Height(26f));
            if (GUILayout.Button("Add", MakeBtnStyle(12, new Color(0.3f, 0.9f, 0.5f)),
                GUILayout.Height(26f), GUILayout.Width(50f)))
            {
                string t = _newRepoInput.Trim();
                if (!string.IsNullOrEmpty(t) && !_repos.Contains(t))
                {
                    _repos.Add(t);
                    SaveRepoList();
                    _newRepoInput = "";
                }
            }
            GUILayout.EndHorizontal();
        }

        // ─────────────────────────────────────────────────────────────────────
        // STYLE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        // Restart the game process on Android (re-launches via PackageManager intent).
        // Falls back to Application.Quit() if the JNI call fails (e.g. editor / WSA).
        private static void RestartApp()
        {
            try
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                string pkg      = activity.Call<string>("getPackageName");
                var pm          = activity.Call<AndroidJavaObject>("getPackageManager");
                var intent      = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", pkg);
                intent.Call<AndroidJavaObject>("addFlags", unchecked((int)0x08000000)); // FLAG_ACTIVITY_CLEAR_TOP
                activity.Call("startActivity", intent);
                var proc = new AndroidJavaClass("android.os.Process");
                proc.CallStatic("killProcess", proc.CallStatic<int>("myPid"));
            }
            catch { Application.Quit(); }
        }

        private GUIStyle MakeBtnStyle(int size, Color col)
        {
            GUIStyle s = new GUIStyle(GUI.skin.button);
            if (_font != null) s.font = _font;
            s.fontSize = size;
            s.normal.textColor = col;
            s.hover.textColor  = Color.white;
            s.active.textColor = Color.white;
            return s;
        }

        private GUIStyle MakeLabelStyle(int size, Color col)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            if (_font != null) s.font = _font;
            s.fontSize = size;
            s.normal.textColor = col;
            return s;
        }

        private GUIStyle MakeToggleStyle(int size, Color col)
        {
            GUIStyle s = new GUIStyle(GUI.skin.toggle);
            if (_font != null) s.font = _font;
            s.fontSize = size;
            s.normal.textColor   = col;
            s.onNormal.textColor = col;
            return s;
        }

        // ─────────────────────────────────────────────────────────────────────
        // MOD CONFIG PANEL
        // ─────────────────────────────────────────────────────────────────────
        private Type FindConfigType(string filename)
        {
            string asmBase = Path.GetFileNameWithoutExtension(filename);
            Assembly myAsm = typeof(ModManagerEntry).Assembly;
            // 1. Scan already-loaded assemblies that match this mod's filename.
            //    ReflectionTypeLoadException handler ensures missing-dep assemblies don't abort the scan.
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm == myAsm) continue;
                if (!asm.GetName().Name.Equals(asmBase, StringComparison.OrdinalIgnoreCase)) continue;
                Type[] types = null;
                try { types = asm.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException rtle) { types = rtle.Types; }
                catch { continue; }
                if (types == null) continue;
                foreach (Type t in types)
                {
                    if (t == null) continue;
                    try
                    {
                        MethodInfo mi = t.GetMethod("GetModConfig",
                            BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                        if (mi != null)
                        {
                            ModManagerEntry.Log("FindConfigType: found GetModConfig on " + t.FullName + " in " + asm.GetName().Name);
                            return t;
                        }
                    }
                    catch { }
                }
            }
            // 2. Fallback: load from disk
            try
            {
                string path = Path.Combine(ModManagerEntry.ModsDir, filename);
                if (!File.Exists(path)) return null;
                Assembly asm2 = Assembly.Load(File.ReadAllBytes(path));
                Type[] types2 = null;
                try { types2 = asm2.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException rtle) { types2 = rtle.Types; }
                catch { return null; }
                if (types2 == null) return null;
                foreach (Type t in types2)
                {
                    if (t == null) continue;
                    MethodInfo mi = t.GetMethod("GetModConfig",
                        BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (mi != null)
                    {
                        ModManagerEntry.Log("FindConfigType(disk): found GetModConfig on " + t.FullName);
                        return t;
                    }
                }
            }
            catch (Exception ex) { ModManagerEntry.Log("FindConfigType disk err " + filename + ": " + ex.Message); }
            ModManagerEntry.Log("FindConfigType: no GetModConfig found for " + filename);
            return null;
        }

        private Type GetCachedConfigType(string filename)
        {
            if (!_configTypeChecked.Contains(filename))
            {
                _configTypeChecked.Add(filename);
                _configTypeCache[filename] = FindConfigType(filename);
            }
            Type t2;
            _configTypeCache.TryGetValue(filename, out t2);
            return t2;
        }

        private List<string[]> LoadConfigEntries(Type t)
        {
            var result = new List<string[]>();
            try
            {
                MethodInfo mi = t.GetMethod("GetModConfig",
                    BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (mi == null) return result;
                string[][] rows = mi.Invoke(null, null) as string[][];
                if (rows == null) return result;
                foreach (string[] row in rows)
                {
                    if (row == null || row.Length < 4) continue;
                    string[] copy = new string[5];
                    copy[0] = row.Length > 0 ? (row[0] ?? "") : "";
                    copy[1] = row.Length > 1 ? (row[1] ?? "") : "";
                    copy[2] = row.Length > 2 ? (row[2] ?? "string") : "string";
                    copy[3] = row.Length > 3 ? (row[3] ?? "") : "";
                    copy[4] = row.Length > 4 ? (row[4] ?? "") : "";
                    result.Add(copy);
                }
            }
            catch (Exception ex) { ModManagerEntry.Log("LoadConfigEntries: " + ex.Message); }
            return result;
        }

        private void SaveConfigEntries()
        {
            if (_configType == null) return;
            try
            {
                MethodInfo mi = _configType.GetMethod("SetModConfig",
                    BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] { typeof(string[][]) }, null);
                if (mi == null) { ModManagerEntry.Log("SaveConfigEntries: SetModConfig not found"); return; }
                mi.Invoke(null, new object[] { _configEntries.ToArray() });
            }
            catch (Exception ex) { ModManagerEntry.Log("SaveConfigEntries: " + ex.Message); }
        }

        private void DrawConfigPanel()
        {
            if (GUILayout.Button("< Back", MakeBtnStyle(13, new Color(0.55f, 0.7f, 1f)),
                GUILayout.Height(26f), GUILayout.Width(80f)))
            {
                _configModIdx       = -1;
                _configType         = null;
                _configEntries.Clear();
                _configDirty        = false;
                _configNeedsRestart = false;
                _scroll             = Vector2.zero;
                return;
            }
            GUILayout.Space(8f);

            string modName = (_configModIdx >= 0 && _configModIdx < _installedMods.Count)
                ? _installedMods[_configModIdx].displayName : "Mod";
            GUIStyle titleSt = MakeLabelStyle(15, Color.white);
            titleSt.fontStyle = FontStyle.Bold;
            GUILayout.Label("Settings \u2014 " + modName, titleSt);
            GUILayout.Space(10f);

            GUIStyle inputSt = new GUIStyle(GUI.skin.textField);
            if (_font != null) inputSt.font = _font;
            inputSt.fontSize = 12;

            foreach (string[] entry in _configEntries)
            {
                string type = entry[2].ToLower();
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(entry[1], MakeLabelStyle(13, new Color(0.85f, 0.9f, 1f)));
                if (!string.IsNullOrEmpty(entry[4]))
                {
                    GUIStyle descSt = MakeLabelStyle(11, new Color(0.55f, 0.55f, 0.62f));
                    descSt.wordWrap = true;
                    GUILayout.Label(entry[4], descSt);
                }
                if (type == "bool")
                {
                    bool cur  = entry[3].ToLower() == "true" || entry[3] == "1";
                    bool next = GUILayout.Toggle(cur, cur ? " Enabled" : " Disabled",
                        MakeToggleStyle(12, new Color(0.75f, 0.85f, 0.75f)));
                    if (next != cur) { entry[3] = next ? "true" : "false"; _configDirty = true; }
                }
                else
                {
                    string next = GUILayout.TextField(entry[3], inputSt, GUILayout.Height(26f));
                    if (next != entry[3]) { entry[3] = next; _configDirty = true; }
                }
                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }

            GUILayout.Space(8f);
            if (_configNeedsRestart)
            {
                GUIStyle restSt = MakeLabelStyle(13, new Color(1f, 0.85f, 0.3f));
                restSt.fontStyle = FontStyle.Bold;
                GUILayout.Label("Changes saved \u2014 restart game to apply!", restSt);
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Restart Game", MakeBtnStyle(13, new Color(1f, 0.4f, 0.4f)),
                    GUILayout.Height(28f), GUILayout.Width(100f)))
                    RestartApp();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = _configDirty;
                if (GUILayout.Button("Save", MakeBtnStyle(13, new Color(0.3f, 0.9f, 0.5f)),
                    GUILayout.Height(28f), GUILayout.Width(80f)))
                {
                    SaveConfigEntries();
                    _configDirty        = false;
                    _configNeedsRestart = true;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }
    }

    public class MgrMpInterceptor : MonoBehaviour
    {
        public ModManagerHook hook;
        private void OnClick()
        {
            if (hook != null) hook.OnMpButtonClick();
        }
    }
}
