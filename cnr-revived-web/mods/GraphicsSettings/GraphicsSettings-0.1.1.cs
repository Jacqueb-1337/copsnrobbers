// GraphicsSettings v0.1.1 - independent CNR mod; CNRSettingsMod >=3.1.110 required.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GraphicsSettings
{
    public static class GraphicsSettingsEntry
    {
        public const string Version = "0.1.1";
        public static void Load()
        {
            if (GameObject.Find("GraphicsSettings") != null) return;
            GameObject go = new GameObject("GraphicsSettings");
            go.AddComponent<GraphicsSettingsHook>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }

    public class GraphicsSettingsHook : MonoBehaviour
    {
        const string Prefix = "CNRGraphics_";
        const int MaxMipmapBytes = 32 * 1024 * 1024;
        const int MaxMipmapTextures = 32;
        static GraphicsSettingsHook instance;
        bool ready, warned, prefsDirty;
        float nextRegister, nextWork, sceneReadyTime, nextSave;
        int currentScene = -1;
        int textureLimit, filtering, fpsLimit, lodCreated, lodEligible, mipBuilt, mipSkipped, mipBytes;
        float lodBias, ambient, dimmer;
        bool autoLod, mipmaps, showStats;
        Color originalAmbient;
        float originalLodBias;
        int originalTextureLimit, originalTargetFPS;
        bool capturedGlobals, capturedAmbient;
        MeshFilter[] pendingMeshes;
        MeshRenderer[] pendingRenderers;
        int meshCursor, textureCursor, textureMaterialCursor;
        readonly Dictionary<Texture2D, Texture2D> generatedTextures = new Dictionary<Texture2D, Texture2D>();
        readonly Dictionary<Material, Texture> replacedTextures = new Dictionary<Material, Texture>();
        readonly Dictionary<Texture2D, FilterMode> replacedFilters = new Dictionary<Texture2D, FilterMode>();
        readonly List<GameObject> createdLodObjects = new List<GameObject>();
        sealed class LodPair { public MeshRenderer full, reduced; public bool useReduced; }
        readonly List<LodPair> createdPairs = new List<LodPair>();
        readonly List<Mesh> createdMeshes = new List<Mesh>();
        static MethodInfo section, label, hint, toggle;

        static void Log(string message) { try { Debug.Log("[GraphicsSettings] " + message); } catch {} }

        void Awake()
        {
            instance = this;
            lodBias = PlayerPrefs.GetFloat(Prefix + "lodBias", 1f);
            textureLimit = PlayerPrefs.GetInt(Prefix + "textureLimit", -1);
            filtering = PlayerPrefs.GetInt(Prefix + "filter", 1);
            fpsLimit = PlayerPrefs.GetInt(Prefix + "fps", 0);
            autoLod = PlayerPrefs.GetInt(Prefix + "autoLod", 0) != 0;
            mipmaps = PlayerPrefs.GetInt(Prefix + "mipmaps", 0) != 0;
            ambient = PlayerPrefs.GetFloat(Prefix + "ambient", 1f);
            dimmer = PlayerPrefs.GetFloat(Prefix + "dimmer", 0f);
            showStats = PlayerPrefs.GetInt(Prefix + "stats", 1) != 0;
            Log("Loaded " + GraphicsSettingsEntry.Version + "; waiting for CNRSettingsMod 3.1.110");
        }

        void Update()
        {
            float now = Time.realtimeSinceStartup;
            if (prefsDirty && now >= nextSave) { PlayerPrefs.Save(); prefsDirty = false; nextSave = now + 3f; }
            if (!ready)
            {
                if (now >= nextRegister)
                {
                    nextRegister = now + 1f;
                    TryRegister();
                }
                return; // no graphics changes when CNRSettingsMod is missing/outdated
            }
            if (Application.loadedLevel != currentScene)
            {
                OnSceneChanged();
            }
            if (now < sceneReadyTime || now < nextWork) return;
            nextWork = now + 0.20f;
            if (autoLod) { ProcessOneMesh(); UpdateLodSwitchers(); }
            if (mipmaps) ProcessOneTexture();
        }

        void TryRegister()
        {
            if (GameObject.Find("CNRSettingsMod") == null) return;
            Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < all.Length; i++)
            {
                Type entry = all[i].GetType("CNRSettingsMod.SettingsModEntry", false);
                if (entry == null) continue;
                FieldInfo versionField = entry.GetField("Version", BindingFlags.Public | BindingFlags.Static);
                string version = versionField == null ? "" : Convert.ToString(versionField.GetRawConstantValue());
                if (VersionCompare(version, "3.1.110") < 0)
                {
                    if (!warned) Log("Disabled: requires CNRSettingsMod >= 3.1.110; found " + version);
                    warned = true;
                    return;
                }
                Type tabs = all[i].GetType("CNRSettingsMod.SettingsExternalTabs", false);
                Type ui = all[i].GetType("CNRSettingsMod.SettingsUI", false);
                if (tabs == null || ui == null) return;
                MethodInfo addTab = tabs.GetMethod("RegisterTab", new Type[] { typeof(string), typeof(string), typeof(Action<float>) });
                if (addTab == null) return;
                try
                {
                    section = ui.GetMethod("Section", BindingFlags.Public | BindingFlags.Static);
                    label = ui.GetMethod("Label", BindingFlags.Public | BindingFlags.Static);
                    hint = ui.GetMethod("Hint", BindingFlags.Public | BindingFlags.Static);
                    toggle = ui.GetMethod("Toggle", BindingFlags.Public | BindingFlags.Static);
                    addTab.Invoke(null, new object[] { "GraphicsSettings", "GRAPHICS", new Action<float>(DrawTab) });
                    ready = true;
                    originalLodBias = QualitySettings.lodBias;
                    originalTextureLimit = QualitySettings.masterTextureLimit;
                    originalTargetFPS = Application.targetFrameRate;
                    capturedGlobals = true;
                    ApplyGlobal();
                    OnSceneChanged();
                    Log("Registered GRAPHICS tab with settings v" + version);
                }
                catch (Exception ex) { Log("Registration failed: " + ex.Message); }
                return;
            }
        }

        static int VersionCompare(string a, string b)
        {
            string[] aa = a.Split('.'), bb = b.Split('.');
            for (int i = 0; i < 3; i++)
            {
                int x = 0, y = 0;
                if (i < aa.Length) int.TryParse(aa[i], out x);
                if (i < bb.Length) int.TryParse(bb[i], out y);
                if (x != y) return x.CompareTo(y);
            }
            return 0;
        }

        void OnLevelWasLoaded(int level)
        {
            if (ready) OnSceneChanged();
        }

        void OnSceneChanged()
        {
            RestoreTextures();
            RemoveLods();
            pendingMeshes = null;
            pendingRenderers = null;
            meshCursor = textureCursor = textureMaterialCursor = 0;
            lodEligible = lodCreated = mipBuilt = mipSkipped = mipBytes = 0;
            currentScene = Application.loadedLevel;
            originalAmbient = RenderSettings.ambientLight;
            capturedAmbient = true;
            ApplyAmbient();
            if (filtering != 1) ApplyTextureFilters();
            sceneReadyTime = Time.realtimeSinceStartup + 1.5f;
            nextWork = sceneReadyTime;
            Log("Scene " + Application.loadedLevelName + " loaded; autoLOD=" + autoLod + ", mipmaps=" + mipmaps);
        }

        void ApplyGlobal()
        {
            QualitySettings.lodBias = Mathf.Clamp(lodBias, 0.25f, 3f);
            QualitySettings.masterTextureLimit = textureLimit < 0 ? originalTextureLimit : Mathf.Clamp(textureLimit, 0, 2);
            Application.targetFrameRate = fpsLimit == 0 ? originalTargetFPS : fpsLimit;
        }

        void ApplyAmbient()
        {
            if (!capturedAmbient) return;
            RenderSettings.ambientLight = new Color(
                Mathf.Clamp01(originalAmbient.r * ambient),
                Mathf.Clamp01(originalAmbient.g * ambient),
                Mathf.Clamp01(originalAmbient.b * ambient),
                originalAmbient.a);
        }

        static void Section(string s) { if (section != null) section.Invoke(null, new object[] { s }); else GUILayout.Label(s); }
        static void Label(string s) { if (label != null) label.Invoke(null, new object[] { s }); else GUILayout.Label(s); }
        static void Hint(string s) { if (hint != null) hint.Invoke(null, new object[] { s }); else GUILayout.Label(s); }
        static bool Toggle(string s, bool v) { return toggle != null ? (bool)toggle.Invoke(null, new object[] { s, v }) : GUILayout.Toggle(v, s); }

        void DrawTab(float width)
        {
            float oldLod = lodBias, oldAmbient = ambient, oldDimmer = dimmer;
            int oldLimit = textureLimit, oldFiltering = filtering, oldFps = fpsLimit;
            bool oldAutoLod = autoLod, oldMipmaps = mipmaps, oldStats = showStats;
            Section("Geometry");
            float newBias = Slider("LOD bias", lodBias, 0.25f, 3f, "0.00");
            if (Mathf.Abs(newBias - lodBias) >= 0.001f) { lodBias = newBias; PlayerPrefs.SetFloat(Prefix + "lodBias", lodBias); ApplyGlobal(); }
            Hint("Higher values preserve full detail farther away. Camera zoom automatically increases on-screen LOD.");
            bool newAuto = Toggle("Experimental automatic mesh LOD", autoLod);
            if (newAuto != autoLod)
            {
                autoLod = newAuto;
                PlayerPrefs.SetInt(Prefix + "autoLod", autoLod ? 1 : 0);
                if (!autoLod) RemoveLods();
                else { pendingMeshes = null; meshCursor = 0; }
            }
            Hint("Converts eligible scenery meshes only. Does not alter physics, players or network objects. Starts OFF.");
            Section("Textures");
            int newLimit = Choose("Texture resolution", textureLimit + 1, new string[] { "Game default", "Full", "Half", "Quarter" }) - 1;
            if (newLimit != textureLimit) { textureLimit = newLimit; PlayerPrefs.SetInt(Prefix + "textureLimit", textureLimit); ApplyGlobal(); }
            Hint("Unity texture limit only affects textures with mip levels.");
            int newFilter = Choose("Texture filtering", filtering, new string[] { "Point", "Bilinear", "Trilinear" });
            if (newFilter != filtering) { filtering = newFilter; PlayerPrefs.SetInt(Prefix + "filter", filtering); ApplyTextureFilters(); }
            bool newMip = Toggle("Experimental generate missing mipmaps", mipmaps);
            if (newMip != mipmaps)
            {
                mipmaps = newMip;
                PlayerPrefs.SetInt(Prefix + "mipmaps", mipmaps ? 1 : 0);
                if (!mipmaps) RestoreTextures();
                else { pendingRenderers = null; textureCursor = textureMaterialCursor = 0; }
            }
            Hint("Rebuilds eligible map textures with mip levels. GPU readback may hitch. Capped at 32 textures / 32 MiB per map. Starts OFF.");
            Section("Lighting & display");
            float newAmb = Slider("Ambient light", ambient, 0f, 2f, "0.00");
            if (Mathf.Abs(newAmb - ambient) >= 0.001f) { ambient = newAmb; PlayerPrefs.SetFloat(Prefix + "ambient", ambient); ApplyAmbient(); }
            Hint("Adjusts lit surfaces only. Does not alter game lighting simulation.");
            float newDim = Slider("Screen dimming", dimmer, 0f, 0.7f, "0.00");
            if (Mathf.Abs(newDim - dimmer) >= 0.001f) { dimmer = newDim; PlayerPrefs.SetFloat(Prefix + "dimmer", dimmer); }
            Hint("Gamma correction needs a compatible full-screen color grading shader; not implemented yet.");
            Section("Performance");
            int selectedFPS = fpsLimit == 30 ? 1 : fpsLimit == 60 ? 2 : fpsLimit == 90 ? 3 : fpsLimit == 120 ? 4 : 0;
            int fpsIdx = Choose("Frame rate cap", selectedFPS, new string[] { "Game default", "30", "60", "90", "120" });
            int fps = new int[] { 0, 30, 60, 90, 120 }[fpsIdx];
            if (fps != fpsLimit) { fpsLimit = fps; PlayerPrefs.SetInt(Prefix + "fps", fps); ApplyGlobal(); }
            bool newStats = Toggle("Show generator statistics", showStats);
            if (newStats != showStats) { showStats = newStats; PlayerPrefs.SetInt(Prefix + "stats", showStats ? 1 : 0); }
            if (showStats)
            {
                Label("LOD meshes: " + lodCreated + " / " + lodEligible);
                Label("Mipmaps: " + mipBuilt + " rebuilt, " + mipSkipped + " skipped (" + (mipBytes / 1048576f).ToString("0.0") + " MiB)");
            }
            if (GUILayout.Button("Restore graphics defaults"))
            {
                lodBias = originalLodBias; ambient = 1f; dimmer = 0f; textureLimit = -1; filtering = 1; fpsLimit = 0;
                autoLod = mipmaps = false;
                RemoveLods(); RestoreTextures(); ApplyGlobal(); ApplyAmbient();
                PlayerPrefs.DeleteKey(Prefix + "lodBias"); PlayerPrefs.DeleteKey(Prefix + "ambient");
                PlayerPrefs.DeleteKey(Prefix + "dimmer"); PlayerPrefs.DeleteKey(Prefix + "textureLimit");
                PlayerPrefs.DeleteKey(Prefix + "filter"); PlayerPrefs.DeleteKey(Prefix + "fps");
                PlayerPrefs.DeleteKey(Prefix + "autoLod"); PlayerPrefs.DeleteKey(Prefix + "mipmaps");
            }
            if (oldLod != lodBias || oldAmbient != ambient || oldDimmer != dimmer ||
                oldLimit != textureLimit || oldFiltering != filtering || oldFps != fpsLimit ||
                oldAutoLod != autoLod || oldMipmaps != mipmaps || oldStats != showStats)
                prefsDirty = true; // save only on actual setting changes
        }

        static float Slider(string name, float current, float min, float max, string format)
        {
            Label(name + ": " + current.ToString(format));
            float w = GUILayout.HorizontalSlider(current, min, max, GUILayout.MinWidth(100f));
            return Mathf.Round(w * 20f) / 20f;
        }

        static int Choose(string name, int selected, string[] choices)
        {
            Label(name);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < choices.Length; i++)
                if (GUILayout.Toggle(i == selected, choices[i], "Button", GUILayout.MinWidth(35f))) selected = i;
            GUILayout.EndHorizontal();
            return selected;
        }

        void OnGUI()
        {
            if (!ready || dimmer <= 0.01f || Event.current.type != EventType.Repaint) return;
            Color prior = GUI.color;
            int priorDepth = GUI.depth;
            GUI.depth = -900;
            GUI.color = new Color(0f, 0f, 0f, dimmer);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prior;
            GUI.depth = priorDepth;
        }

        // Old NGUI fonts and menus use shared atlas textures. Changing filter
        // or rebuilding mip levels on these can make glyphs render as blocks.
        static bool IsUiAtlasTexture(Material mat, Texture2D texture)
        {
            string names = ((mat == null ? "" : mat.name) + " " + (texture == null ? "" : texture.name)).ToLowerInvariant();
            return names.Contains("font") || names.Contains("atlas") ||
                   names.Contains("ngui") || names.Contains("menu") ||
                   names.Contains("bitmap") || names.Contains("uiwidget");
        }

        void ApplyTextureFilters()
        {
            // Operate only on textures used by world MeshRenderers, not UI atlases.
            MeshRenderer[] renderers = (MeshRenderer[])UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer));
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer r = renderers[i];
                if (r == null || IsGameplayOrUI(r.transform)) continue;
                Material[] mats = r.sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material mat = mats[j];
                    if (mat == null || !mat.HasProperty("_MainTex")) continue;
                    Texture2D t = mat.mainTexture as Texture2D;
                    if (t == null || IsUiAtlasTexture(mat, t)) continue;
                    if (!replacedFilters.ContainsKey(t)) replacedFilters.Add(t, t.filterMode);
                    t.filterMode = (FilterMode)filtering;
                }
            }
        }

        void ProcessOneTexture()
        {
            if (mipBuilt >= MaxMipmapTextures || mipBytes >= MaxMipmapBytes) return;
            if (pendingRenderers == null)
            {
                pendingRenderers = (MeshRenderer[])UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer));
                textureCursor = textureMaterialCursor = 0;
            }
            // Only one expensive texture generation per work tick, bounded scans.
            int checkedCount = 0;
            while (textureCursor < pendingRenderers.Length && checkedCount++ < 12)
            {
                MeshRenderer renderer = pendingRenderers[textureCursor];
                if (renderer == null || !renderer.enabled || IsGameplayOrUI(renderer.transform))
                {
                    textureCursor++; textureMaterialCursor = 0;
                    continue;
                }
                Material[] mats = renderer.sharedMaterials;
                while (textureMaterialCursor < mats.Length)
                {
                    Material mat = mats[textureMaterialCursor++];
                    if (mat == null || !mat.HasProperty("_MainTex")) continue;
                    Texture2D source = mat.mainTexture as Texture2D;
                    if (source == null || IsUiAtlasTexture(mat, source) || source.mipmapCount > 1 || source.width < 64 || source.height < 64) continue;
                    if (source.width > 1024 || source.height > 1024) { mipSkipped++; continue; }
                    Texture2D replacement;
                    if (!generatedTextures.TryGetValue(source, out replacement))
                    {
                        int bytes = source.width * source.height * 4 * 4 / 3;
                        if (mipBytes + bytes > MaxMipmapBytes) return;
                        replacement = GenerateMipmappedCopy(source);
                        if (replacement == null) { mipSkipped++; continue; }
                        generatedTextures[source] = replacement;
                        mipBytes += bytes;
                        mipBuilt++;
                    }
                    if (!replacedTextures.ContainsKey(mat)) replacedTextures.Add(mat, source);
                    mat.mainTexture = replacement;
                    if (mipBuilt >= MaxMipmapTextures) return;
                    return;
                }
                textureCursor++;
                textureMaterialCursor = 0;
            }
        }

        Texture2D GenerateMipmappedCopy(Texture2D original)
        {
            Texture2D tex = null;
            RenderTexture target = null, prior = RenderTexture.active;
            try
            {
                target = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(original, target);
                RenderTexture.active = target;
                tex = new Texture2D(original.width, original.height, TextureFormat.RGBA32, true);
                tex.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
                tex.Apply(true, false);
                tex.wrapMode = original.wrapMode;
                tex.filterMode = (FilterMode)filtering;
                tex.anisoLevel = original.anisoLevel;
                tex.name = "GraphicsSettings_mip_" + original.name;
                return tex;
            }
            catch (Exception ex)
            {
                Log("Mip copy skipped for " + original.name + ": " + ex.Message);
                if (tex != null) UnityEngine.Object.Destroy(tex);
                return null;
            }
            finally
            {
                RenderTexture.active = prior;
                if (target != null) RenderTexture.ReleaseTemporary(target);
            }
        }

        void RestoreTextures()
        {
            foreach (KeyValuePair<Material, Texture> item in replacedTextures)
                if (item.Key != null) item.Key.mainTexture = item.Value;
            replacedTextures.Clear();
            foreach (KeyValuePair<Texture2D, FilterMode> item in replacedFilters)
                if (item.Key != null) item.Key.filterMode = item.Value;
            replacedFilters.Clear();
            foreach (KeyValuePair<Texture2D, Texture2D> item in generatedTextures)
                if (item.Value != null) UnityEngine.Object.Destroy(item.Value);
            generatedTextures.Clear();
            pendingRenderers = null;
            textureCursor = textureMaterialCursor = mipBuilt = mipBytes = 0;
        }

        void ProcessOneMesh()
        {
            if (pendingMeshes == null)
            {
                pendingMeshes = (MeshFilter[])UnityEngine.Object.FindObjectsOfType(typeof(MeshFilter));
                meshCursor = 0;
            }
            int checkedCount = 0;
            while (meshCursor < pendingMeshes.Length && checkedCount++ < 12)
            {
                MeshFilter mf = pendingMeshes[meshCursor++];
                if (mf == null || IsGameplayOrUI(mf.transform)) continue;
                MeshRenderer renderer = mf.GetComponent<MeshRenderer>();
                if (renderer == null || !renderer.enabled || mf.GetComponent<LODGroup>() != null) continue;
                Mesh source = mf.sharedMesh;
                if (source == null || source.subMeshCount != 1 || source.vertexCount < 200 || source.vertexCount > 12000) continue;
                lodEligible++;
                Mesh low = MeshReduction.TryReduce(source);
                if (low == null) continue;
                try
                {
                    GameObject child = new GameObject("GraphicsSettings_LOD1");
                    child.transform.parent = mf.transform;
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;
                    child.transform.localScale = Vector3.one;
                    child.layer = mf.gameObject.layer;
                    MeshFilter filter = child.AddComponent<MeshFilter>();
                    filter.sharedMesh = low;
                    MeshRenderer lowRenderer = child.AddComponent<MeshRenderer>();
                    lowRenderer.sharedMaterials = renderer.sharedMaterials;
                    lowRenderer.enabled = false;
                    createdMeshes.Add(low);
                    createdPairs.Add(new LodPair { full = renderer, reduced = lowRenderer });
                    createdLodObjects.Add(child);
                    lodCreated++;
                    return;
                }
                catch (Exception ex)
                {
                    Log("LOD setup skipped: " + ex.Message);
                    UnityEngine.Object.Destroy(low);
                }
            }
        }

        // Unity 4.6's LODGroup lacks SetLODs. Select meshes ourselves using
        // apparent height, so reduced FOV (scope/zoom) automatically picks detail.
        void UpdateLodSwitchers()
        {
            if (createdPairs.Count == 0) return;
            Camera cam = Camera.main;
            if (cam == null)
            {
                Camera[] cameras = Camera.allCameras;
                for (int c = 0; c < cameras.Length; c++)
                {
                    Camera candidate = cameras[c];
                    if (candidate == null || !candidate.enabled) continue;
                    string name = candidate.name.ToLowerInvariant();
                    if (name.Contains("ui") || name.Contains("kamcord") || name.Contains("ngui")) continue;
                    if (cam == null || candidate.depth > cam.depth) cam = candidate;
                }
            }
            if (cam == null) return;
            float halfFov = cam.fieldOfView * Mathf.PI / 360f;
            float tanHalfFov = Mathf.Tan(halfFov);
            if (tanHalfFov <= 0.001f) return;
            Vector3 eye = cam.transform.position;
            Vector3 forward = cam.transform.forward;
            for (int i = 0; i < createdPairs.Count; i++)
            {
                LodPair pair = createdPairs[i];
                if (pair.full == null || pair.reduced == null) continue;
                Bounds b = pair.full.bounds;
                float depth = Vector3.Dot(b.center - eye, forward);
                // Never degrade geometry behind or extremely close to the viewer.
                float apparent = depth <= 0.01f ? 10f : b.size.magnitude / (depth * 2f * tanHalfFov);
                float threshold = (pair.useReduced ? 0.14f : 0.11f) / Mathf.Clamp(lodBias, 0.25f, 3f);
                bool low = apparent < threshold && depth > cam.nearClipPlane;
                if (pair.useReduced == low) continue;
                pair.useReduced = low;
                pair.full.enabled = !low;
                pair.reduced.enabled = low;
            }
        }

        static bool IsGameplayOrUI(Transform t)
        {
            while (t != null)
            {
                GameObject go = t.gameObject;
                if (go.GetComponent<PhotonView>() != null ||
                    go.GetComponent<Rigidbody>() != null ||
                    go.GetComponent<CharacterController>() != null ||
                    go.GetComponent<Camera>() != null ||
                    go.GetComponent<Animation>() != null ||
                    go.GetComponent<Animator>() != null) return true;
                if (go.name.StartsWith("Panel(") || go.name.StartsWith("UI") ||
                    go.name.StartsWith("NGUI") || go.name.IndexOf("Player") >= 0 ||
                    go.name.IndexOf("Weapon") >= 0) return true;
                t = t.parent;
            }
            return false;
        }

        void RemoveLods()
        {
            for (int i = 0; i < createdPairs.Count; i++)
            {
                if (createdPairs[i].full != null) createdPairs[i].full.enabled = true;
                if (createdPairs[i].reduced != null) createdPairs[i].reduced.enabled = false;
            }
            for (int i = 0; i < createdLodObjects.Count; i++)
                if (createdLodObjects[i] != null) UnityEngine.Object.Destroy(createdLodObjects[i]);
            for (int i = 0; i < createdMeshes.Count; i++)
                if (createdMeshes[i] != null) UnityEngine.Object.Destroy(createdMeshes[i]);
            createdPairs.Clear(); createdLodObjects.Clear(); createdMeshes.Clear();
            pendingMeshes = null;
            meshCursor = lodCreated = lodEligible = 0;
        }

        void OnDestroy()
        {
            RestoreTextures();
            RemoveLods();
            if (prefsDirty) PlayerPrefs.Save();
            if (capturedAmbient) RenderSettings.ambientLight = originalAmbient;
            if (capturedGlobals)
            {
                QualitySettings.lodBias = originalLodBias;
                QualitySettings.masterTextureLimit = originalTextureLimit;
                Application.targetFrameRate = originalTargetFPS;
            }
            instance = null;
        }
    }
}
