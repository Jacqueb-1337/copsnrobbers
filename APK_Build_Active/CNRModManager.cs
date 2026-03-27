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
        public  const string Version        = "1.0.0";
        private const string LogPath        = "/storage/emulated/0/CNRMods/modmanager.log";
        public  const string ModsDir        = "/storage/emulated/0/CNRMods";
        public  const string DefaultRepoUrl = "https://play.jacqueb.me/mods/repo.json";

        private static bool _loaded = false;

        // Called by CNRMod's DLL scanner
        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            RegisterWithCNRMod();
            Log("=== CNRModManager v" + Version + " Load() ===");
            Spawn();
        }

        // Called by CNRMods shim when CNRMod.dll is absent
        public static void BootstrapLoad()
        {
            if (_loaded) return;
            _loaded = true;
            Log("=== CNRModManager v" + Version + " BootstrapLoad() (no CNRMod) ===");
            Spawn();
            ScanAndLoadMods();
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
        // Layout / scaling
        private const float REF_W = 600f;

        // Window state
        private bool    _showWindow  = false;
        private int     _tab         = 0;   // 0=Installed 1=Browse 2=Repos
        private Vector2 _scroll      = Vector2.zero;
        private float   _lastToggle  = 0f;

        // Scene
        private string _scene    = "";
        private bool   _patched  = false;
        private bool   _nguiBlocked = false;

        // NGUI caches
        private UICamera[] _nguiCameras = null;
        private static Font _font = null;

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
        private struct RepoMod
        {
            public string id;
            public string name;
            public string description;
            public string latestVersion;
            public string filename;
            public string latestUrl;
            public bool   latestOnly;
            public List<ModVersion> versions;
        }
        private List<RepoMod> _browseMods  = new List<RepoMod>();
        private string _browseStatus       = "";
        private bool   _browseFetching     = false;
        private string _statusMsg          = "";   // shared download / action feedback
        private int    _detailModIdx       = -1;    // -1=list view, >=0=detail for that index

        // ── Repos tab data ────────────────────────────────────────────────────
        private List<string> _repos      = new List<string>();
        private string       _newRepoInput = "";
        private const string PREF_REPOS    = "CNRModMgr_Repos";

        // ─────────────────────────────────────────────────────────────────────
        private void Start()
        {
            _scene = Application.loadedLevelName ?? "";
            if (_scene == "MainMenu") PatchMenu();
            LoadRepoList();
        }

        private void OnLevelWasLoaded(int level)
        {
            _scene       = Application.loadedLevelName ?? "";
            _patched     = false;
            _nguiCameras = null;
            _nguiBlocked = false;
            _showWindow  = false;
            if (_scene == "MainMenu") PatchMenu();
        }

        private void PatchMenu()
        {
            if (_patched) return;
            _patched = true;
            if (_font == null)
            {
                UILabel[] lbls = (UILabel[])(object)FindObjectsOfType(typeof(UILabel));
                foreach (UILabel lbl in lbls)
                    if (lbl.font != null && lbl.font.dynamicFont != null)
                    { _font = lbl.font.dynamicFont; break; }
            }
            ModManagerEntry.Log("PatchMenu scene=" + _scene + " font=" + (_font != null ? "ok" : "null"));
        }

        private void Update()
        {
            SetNguiBlocking(_showWindow);
        }

        private void SetNguiBlocking(bool block)
        {
            if (block == _nguiBlocked) return;
            if (_nguiCameras == null)
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
                // Eat all pointer events so NGUI doesn't see them
                if (Event.current.isMouse || Event.current.isKey)
                    Event.current.Use();
                return;
            }

            // ── "Mod Manager" button — top-right ──────────────────────────────
            float btnW = 110f, btnH = 26f;
            float btnX = vw - btnW - 6f;
            float btnY = 6f;
            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Mod Manager",
                MakeBtnStyle(13, new Color(0.4f, 0.8f, 1f))))
            {
                if (Time.unscaledTime - _lastToggle > 0.3f)
                {
                    _lastToggle = Time.unscaledTime;
                    OpenWindow();
                }
            }
        }

        private void OpenWindow()
        {
            _showWindow = true;
            _scroll     = Vector2.zero;
            _statusMsg  = "";
            RefreshInstalledMods();
            if (_tab == 1 && _browseMods.Count == 0 && !_browseFetching)
                StartFetchBrowse();
        }

        // ─────────────────────────────────────────────────────────────────────
        // INSTALLED TAB
        // ─────────────────────────────────────────────────────────────────────
        private void RefreshInstalledMods()
        {
            _installedMods.Clear();
            try
            {
                if (!Directory.Exists(ModManagerEntry.ModsDir)) return;
                foreach (string path in Directory.GetFiles(ModManagerEntry.ModsDir, "*.dll"))
                {
                    string fn  = Path.GetFileName(path);
                    string dn  = Path.GetFileNameWithoutExtension(fn);
                    string ver = GetRegisteredVersion(dn);
                    if (ver == null) ver = "?";
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
                _browseStatus = "Error: " + www.error;
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
                            mod.filename      = ParseJsonStr(obj, "filename");
                            string latUrl     = ParseJsonStr(obj, "latestUrl");
                            mod.latestUrl     = !string.IsNullOrEmpty(latUrl) ? latUrl : ParseJsonStr(obj, "url");
                            mod.latestOnly    = ParseJsonBool(obj, "latestOnly");
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

        private IEnumerator DownloadMod(string displayName, string filename, string url)
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
                _statusMsg = displayName + " installed! Restart the app to load.";
                ModManagerEntry.Log("Download OK: " + dest + " (" + www.bytes.Length + " bytes)");
                RefreshInstalledMods();
            }
            catch (Exception ex)
            {
                _statusMsg = "Save error: " + ex.Message;
                ModManagerEntry.Log("Download save err: " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DRAWING
        // ─────────────────────────────────────────────────────────────────────
        private void DrawWindow(float vw, float vh)
        {
            // Dim overlay
            GUI.color = new Color(0f, 0f, 0f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, vw, vh), Texture2D.whiteTexture);
            GUI.color = Color.white;
            // Eat taps behind window
            if (GUI.Button(new Rect(0, 0, vw, vh), GUIContent.none, GUIStyle.none)) { }

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
                _showWindow   = false;
                _detailModIdx = -1;
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

            // Content area
            float cY = tabY + tabH + 6f;
            float cH = winH - (cY - winY) - 10f;
            GUILayout.BeginArea(new Rect(winX + 6f, cY, winW - 12f, cH));
            _scroll = GUILayout.BeginScrollView(_scroll);

            if      (_tab == 0) DrawInstalledTab();
            else if (_tab == 1) DrawBrowseTab();
            else                DrawReposTab();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawInstalledTab()
        {
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
                foreach (InstalledMod m in _installedMods)
                {
                    GUILayout.BeginHorizontal();
                    GUIStyle ns = MakeLabelStyle(13, Color.white);
                    ns.fontStyle = FontStyle.Bold;
                    GUILayout.Label(m.displayName, ns, GUILayout.Width(190f));
                    GUILayout.Label("v" + m.version,
                        MakeLabelStyle(12, new Color(0.7f, 0.9f, 0.7f)), GUILayout.Width(70f));
                    GUILayout.FlexibleSpace();
                    bool core = m.filename.Equals("CNRMod.dll",        StringComparison.OrdinalIgnoreCase) ||
                                m.filename.Equals("CNRModManager.dll", StringComparison.OrdinalIgnoreCase);
                    if (core)
                    {
                        GUILayout.Label("(core)", MakeLabelStyle(11, new Color(0.55f, 0.55f, 0.55f)),
                            GUILayout.Width(60f));
                    }
                    else
                    {
                        string fn = m.filename;   // capture for closure
                        if (GUILayout.Button("Remove",
                            MakeBtnStyle(11, new Color(1f, 0.4f, 0.4f)),
                            GUILayout.Height(22f), GUILayout.Width(70f)))
                        {
                            try
                            {
                                File.Delete(Path.Combine(ModManagerEntry.ModsDir, fn));
                                RefreshInstalledMods();
                                _statusMsg = fn + " removed. Restart app.";
                            }
                            catch (Exception ex) { _statusMsg = "Remove failed: " + ex.Message; }
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);
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

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                bool installed = File.Exists(Path.Combine(ModManagerEntry.ModsDir, mod.filename));
                if (installed)
                    GUILayout.Label("Installed", MakeLabelStyle(12, new Color(0.4f, 1f, 0.5f)),
                        GUILayout.Width(68f));
                int captureIdx = bi;
                if (GUILayout.Button("Details", MakeBtnStyle(12, new Color(0.55f, 0.7f, 1f)),
                    GUILayout.Height(24f), GUILayout.Width(66f)))
                {
                    _detailModIdx = captureIdx;
                    _scroll       = Vector2.zero;
                }
                string instLabel = installed ? "Update" : "Install";
                Color  instCol   = installed ? new Color(0.4f, 0.8f, 1f) : new Color(0.3f, 0.9f, 0.5f);
                string capName   = mod.name;
                string capFile   = mod.filename;
                string capUrl    = mod.latestUrl;
                if (GUILayout.Button(instLabel, MakeBtnStyle(12, instCol),
                    GUILayout.Height(24f), GUILayout.Width(68f)))
                    StartCoroutine(DownloadMod(capName, capFile, capUrl));
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

                    if (mod.latestOnly && !isLatest)
                    {
                        GUILayout.Label("(latest only)",
                            MakeLabelStyle(11, new Color(0.5f, 0.5f, 0.55f)));
                    }
                    else
                    {
                        string btnLbl = isLatest ? (installedMod ? "Update" : "Install") : "Download";
                        Color  btnClr = isLatest
                            ? (installedMod ? new Color(0.4f, 0.8f, 1f) : new Color(0.3f, 0.9f, 0.5f))
                            : new Color(0.65f, 0.65f, 0.75f);
                        string vUrl = ver.url;
                        string vTag = ver.version;
                        if (GUILayout.Button(btnLbl, MakeBtnStyle(12, btnClr),
                            GUILayout.Height(24f), GUILayout.Width(82f)))
                            StartCoroutine(DownloadMod(mod.name + " v" + vTag, mod.filename, vUrl));
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
    }
}
