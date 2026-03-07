// CNRSceneDumperMod.cs -- Scene object/texture palette dumper for map making
// Entry point: CNRSceneDumperMod.SceneDumperEntry.Load() -- called by IPRedirectMod DLL scanner
//
// On each game scene load (after 4s settle delay) outputs to:
//   /sdcard/CNRMods/scene_dumps/<scene_name>/
//     objects.json        -- full hierarchy: path, mesh, material, bounds, color
//     overhead.png        -- 1024x1024 top-down XZ footprint, each object colored by material
//     textures/           -- one PNG per unique material texture

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace CNRSceneDumperMod
{
    // -------------------------------------------------------------------------
    // Entry point
    // -------------------------------------------------------------------------
    public static class SceneDumperEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/scene_dumps/dumper.log";

        public static void Load()
        {
            try
            {
                GameObject go = new GameObject("CNRSceneDumper");
                go.AddComponent<SceneDumperHook>();
                GameObject.DontDestroyOnLoad(go);
                Log("CNRSceneDumperMod loaded");
            }
            catch (Exception ex) { Log("Load() error: " + ex); }
        }

        public static void Log(string msg)
        {
            try
            {
                Directory.CreateDirectory("/storage/emulated/0/CNRMods/scene_dumps");
                File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n");
            }
            catch { }
            try { Debug.Log("[CNRDumper] " + msg); } catch { }
        }
    }

    // -------------------------------------------------------------------------
    // Main hook
    // -------------------------------------------------------------------------
    public class SceneDumperHook : MonoBehaviour
    {
        private static readonly string[] GAME_SCENES = {
            "CopsAndRobbers", "CNRMultiplayer", "BlankScene", "MultiPlayer",
            "GameScene", "Level", "Map", "Stage", "Room"
        };

        private void Start()            { OnLevelWasLoaded(0); }
        private void OnLevelWasLoaded(int _) { StartCoroutine(DumpAfterDelay()); }

        private IEnumerator DumpAfterDelay()
        {
            // Give the scene 4 seconds to fully spawn game objects
            yield return new WaitForSeconds(4f);
            string scene = Application.loadedLevelName ?? "unknown";
            SceneDumperEntry.Log("Starting dump for scene: " + scene);
            StartCoroutine(DumpScene(scene));
        }

        // =====================================================================
        // Master dump coroutine
        // =====================================================================
        private IEnumerator DumpScene(string sceneName)
        {
            string safe  = SafeName(sceneName);
            string dir   = "/storage/emulated/0/CNRMods/scene_dumps/" + safe;
            string texDir = dir + "/textures";
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(texDir);

            // Collect all renderers in the scene
            Renderer[] renderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
            SceneDumperEntry.Log("Found " + renderers.Length + " renderers");

            // ---- 1. Collect object info, compute world bounds ---------------
            var entries  = new List<ObjEntry>();
            var matColors   = new Dictionary<string, Color32>();   // matName → color
            var matTextures = new Dictionary<string, Material>(); // matName → first material that has a mainTexture

            Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
            bool first = true;

            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                string path    = GetPath(r.gameObject);
                string matName = r.sharedMaterial != null ? r.sharedMaterial.name : "none";
                string meshName = "none";
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) meshName = mf.sharedMesh.name;

                Bounds b    = r.bounds;
                Color32 col = new Color32(180, 180, 180, 255);
                if (r.sharedMaterial != null)
                {
                    Color c = r.sharedMaterial.color;
                    col = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), 255);

                    if (!matColors.ContainsKey(matName))
                    {
                        // material.color is almost always white for textured mats;
                        // sample center pixel of the texture if available for a better swatch
                        Texture2D t2d = r.sharedMaterial.mainTexture as Texture2D;
                        if (t2d != null)
                        {
                            // Store the whole material so MakeReadable can use its exact shader
                            matTextures[matName] = r.sharedMaterial;
                            // Try fast CPU sample first
                            try
                            {
                                Color sc = t2d.GetPixel(t2d.width / 2, t2d.height / 2);
                                col = new Color32((byte)(sc.r*255),(byte)(sc.g*255),(byte)(sc.b*255),255);
                            }
                            catch { /* not CPU readable — will stay white, fixed at palette gen */ }
                        }
                        matColors[matName] = col;
                    }
                }

                entries.Add(new ObjEntry {
                    path = path, mesh = meshName, mat = matName, col = col,
                    cx = r.transform.position.x,
                    cy = r.transform.position.y,
                    cz = r.transform.position.z,
                    sx = r.transform.lossyScale.x,
                    sy = r.transform.lossyScale.y,
                    sz = r.transform.lossyScale.z,
                    rx = r.transform.eulerAngles.x,
                    ry = r.transform.eulerAngles.y,
                    rz = r.transform.eulerAngles.z
                });

                // Still use bounds for overhead image centering
                if (first) { sceneBounds = r.bounds; first = false; }
                else sceneBounds.Encapsulate(r.bounds);
            }

            yield return null; // breathe

            // ---- 2. Write objects.json -------------------------------------
            WriteJson(entries, dir + "/objects.json");
            SceneDumperEntry.Log("Wrote objects.json (" + entries.Count + " objects)");
            yield return null;

            // ---- 3. Save per-material textures ----------------------------
            int texSaved = 0;
            foreach (var kv in matTextures)
            {
                string texFile = texDir + "/" + SafeName(kv.Key) + ".png";
                Material srcMat = kv.Value;
                Texture2D srcTex = srcMat != null ? srcMat.mainTexture as Texture2D : null;
                if (srcTex != null)
                {
                    // Must wait for end of frame so GPU has a valid render context
                    yield return new WaitForEndOfFrame();
                    Texture2D readable = MakeReadable(srcTex, srcMat);
                    if (readable != null)
                    {
                        try
                        {
                            byte[] png = readable.EncodeToPNG();
                            File.WriteAllBytes(texFile, png);
                            texSaved++;
                            // Update palette color by sampling center of the actual readable texture
                            Color sc = readable.GetPixel(readable.width / 2, readable.height / 2);
                            matColors[kv.Key] = new Color32(
                                (byte)(sc.r * 255), (byte)(sc.g * 255), (byte)(sc.b * 255), 255);
                        }
                        catch (Exception ex) { SceneDumperEntry.Log("tex save err " + kv.Key + ": " + ex.Message); }
                        Destroy(readable);
                    }
                    else
                    {
                        SceneDumperEntry.Log("tex unreadable: " + kv.Key + " fmt=" + srcTex.format + " size=" + srcTex.width + "x" + srcTex.height);
                    }
                }
            }
            SceneDumperEntry.Log("Saved " + texSaved + " textures");

            // ---- 4. Generate overhead PNG ----------------------------------
            const int IMG = 1024;
            Texture2D overhead = GenerateOverhead(entries, sceneBounds, IMG, matColors);
            try { File.WriteAllBytes(dir + "/overhead.png", overhead.EncodeToPNG()); }
            catch (Exception ex) { SceneDumperEntry.Log("overhead save err: " + ex.Message); }
            Destroy(overhead);
            yield return null;

            // ---- 5. Generate material palette PNG -------------------------
            Texture2D palette = GeneratePalette(matColors, IMG);
            try { File.WriteAllBytes(dir + "/material_palette.png", palette.EncodeToPNG()); }
            catch (Exception ex) { SceneDumperEntry.Log("palette save err: " + ex.Message); }
            Destroy(palette);

            SceneDumperEntry.Log("DUMP COMPLETE -> " + dir);
        }

        // =====================================================================
        // Overhead PNG:  top-down XZ view, each object footprint colored by mat
        // =====================================================================
        private Texture2D GenerateOverhead(List<ObjEntry> entries, Bounds scene, int size, Dictionary<string, Color32> matColors)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // Fill background dark grey
            Color32[] bg = new Color32[size * size];
            Color32 bgCol = new Color32(30, 30, 35, 255);
            for (int i = 0; i < bg.Length; i++) bg[i] = bgCol;
            tex.SetPixels32(bg);

            float worldW = Mathf.Max(scene.size.x, 1f);
            float worldD = Mathf.Max(scene.size.z, 1f);
            float margin = 0.95f;
            float scale  = size * margin / Mathf.Max(worldW, worldD);
            float offX   = (size - worldW * scale) * 0.5f - scene.min.x * scale;
            float offZ   = (size - worldD * scale) * 0.5f - scene.min.z * scale;

            // Sort back-to-front by Y so taller objects draw on top
            entries.Sort((a, b) => a.cy.CompareTo(b.cy));

            foreach (ObjEntry e in entries)
            {
                // Skip tiny/invisible objects (sub-pixel or zero size in XZ)
                if (e.sx < 0.05f && e.sz < 0.05f) continue;

                Color32 col = matColors.ContainsKey(e.mat) ? matColors[e.mat] : new Color32(150, 150, 150, 255);
                // Slightly darken by height so elevated objects stand out
                float brightness = Mathf.Clamp01(0.5f + e.cy * 0.04f);
                col = Tint(col, brightness);

                int px0 = Mathf.RoundToInt((e.cx - e.sx * 0.5f) * scale + offX);
                int pz0 = Mathf.RoundToInt((e.cz - e.sz * 0.5f) * scale + offZ);
                int px1 = Mathf.RoundToInt((e.cx + e.sx * 0.5f) * scale + offX);
                int pz1 = Mathf.RoundToInt((e.cz + e.sz * 0.5f) * scale + offZ);

                px0 = Mathf.Clamp(px0, 0, size - 1);
                pz0 = Mathf.Clamp(pz0, 0, size - 1);
                px1 = Mathf.Clamp(px1, 0, size - 1);
                pz1 = Mathf.Clamp(pz1, 0, size - 1);

                // Fill interior
                for (int pz = pz0; pz <= pz1; pz++)
                    for (int px = px0; px <= px1; px++)
                        tex.SetPixel(px, pz, col);

                // Draw 1px outline slightly brighter
                Color32 outline = Tint(col, 1.4f);
                for (int px = px0; px <= px1; px++) { tex.SetPixel(px, pz0, outline); tex.SetPixel(px, pz1, outline); }
                for (int pz = pz0; pz <= pz1; pz++) { tex.SetPixel(px0, pz, outline); tex.SetPixel(px1, pz, outline); }
            }

            tex.Apply();
            return tex;
        }

        // =====================================================================
        // Material palette PNG: grid of color swatches, 8 per row
        // =====================================================================
        private Texture2D GeneratePalette(Dictionary<string, Color32> matColors, int imgSize)
        {
            const int COLS      = 8;
            const int SWATCH_W  = 80;
            const int SWATCH_H  = 40;
            const int LABEL_H   = 12; // pixels reserved for label (not actual text — just darker strip)

            int colCount = matColors.Count;
            int rows     = Mathf.CeilToInt((float)colCount / COLS);
            int w        = COLS * SWATCH_W;
            int h        = rows * (SWATCH_H + LABEL_H);

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[w * h];
            // Background
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(20, 20, 25, 255);
            tex.SetPixels32(pixels);

            int idx = 0;
            foreach (var kv in matColors)
            {
                int col = idx % COLS;
                int row = idx / COLS;
                int x0  = col * SWATCH_W;
                int y0  = (rows - 1 - row) * (SWATCH_H + LABEL_H); // flip Y

                // Swatch body
                for (int py = LABEL_H; py < SWATCH_H + LABEL_H; py++)
                    for (int px = 1; px < SWATCH_W - 1; px++)
                        tex.SetPixel(x0 + px, y0 + py, kv.Value);

                // Darker label strip at bottom with index number encoded as brightness bands
                // (full text rendering is not available without a font asset)
                // Instead: encode the index as a small brightness pattern
                byte marker = (byte)(40 + (idx % 10) * 15);
                for (int px = 0; px < SWATCH_W; px++)
                    tex.SetPixel(x0 + px, y0, new Color32(marker, marker, marker, 255));

                idx++;
            }

            tex.Apply();

            // Also dump a text legend alongside the JSON
            return tex;
        }

        // =====================================================================
        // JSON writer
        // =====================================================================
        private void WriteJson(List<ObjEntry> entries, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 0; i < entries.Count; i++)
            {
                ObjEntry e = entries[i];
                sb.Append("  {");
                sb.Append("\"path\":\"")  .Append(Escape(e.path)).Append("\",");
                sb.Append("\"mesh\":\"")  .Append(Escape(e.mesh)).Append("\",");
                sb.Append("\"mat\":\"")   .Append(Escape(e.mat)).Append("\",");
                sb.Append("\"color\":[")  .Append(e.col.r).Append(",").Append(e.col.g).Append(",").Append(e.col.b).Append("],");
                sb.Append("\"pos\":[").Append(F(e.cx)).Append(",").Append(F(e.cy)).Append(",").Append(F(e.cz)).Append("],");
                sb.Append("\"rot\":[").Append(F(e.rx)).Append(",").Append(F(e.ry)).Append(",").Append(F(e.rz)).Append("],");
                sb.Append("\"size\":[").Append(F(e.sx)).Append(",").Append(F(e.sy)).Append(",").Append(F(e.sz)).Append("]");
                sb.Append("}");
                if (i < entries.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("]");
            File.WriteAllText(path, sb.ToString());
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private static Texture2D MakeReadable(Texture2D src, Material srcMat = null)
        {
            if (src == null) return null;

            // -- Method 1: direct CPU copy (works if texture has Read/Write enabled)
            try
            {
                Color[] pixels = src.GetPixels();  // throws if not readable
                // GetPixels succeeded — texture is CPU readable, encode directly
                Texture2D dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
                dst.SetPixels(pixels);
                dst.Apply();
                return dst;
            }
            catch { /* not CPU readable — fall through to camera method */ }

            // -- Method 2: Graphics.Blit with the game's own material
            // Camera.Render() approach failed — quad culling issues.
            // Blit is direct: GPU decompresses ETC2/PVRTC → ARGB32 RT in one call.
            // The game's own material is used so the shader can handle the format.
            try
            {
                int w = Mathf.Min(src.width,  512);
                int h = Mathf.Min(src.height, 512);

                RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
                rt.Create();

                if (srcMat != null)
                {
                    Material blitMat = new Material(srcMat);
                    blitMat.mainTexture = src;
                    Graphics.Blit(src, rt, blitMat);
                    Destroy(blitMat);
                }
                else
                {
                    Graphics.Blit(src, rt);
                }

                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dst.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                dst.Apply();
                RenderTexture.active = prev;
                rt.Release();
                Destroy(rt);
                return dst;
            }
            catch (Exception ex)
            {
                SceneDumperEntry.Log("MakeReadable blit err: " + ex.Message);
                return null;
            }
        }

        private static Color32 Tint(Color32 c, float brightness)
        {
            return new Color32(
                (byte)Mathf.Clamp(c.r * brightness, 0, 255),
                (byte)Mathf.Clamp(c.g * brightness, 0, 255),
                (byte)Mathf.Clamp(c.b * brightness, 0, 255),
                255);
        }

        private static string GetPath(GameObject go)
        {
            var parts = new System.Collections.Generic.List<string>();
            Transform t = go.transform;
            int limit = 20;
            while (t != null && limit-- > 0) { parts.Insert(0, t.name); t = t.parent; }
            return string.Join("/", parts.ToArray());
        }

        private static string SafeName(string s)
        {
            if (s == null) return "unknown";
            var sb = new StringBuilder();
            foreach (char c in s)
                sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            return sb.ToString();
        }
        private static string F(float v)
        {
            return v.ToString("F2", CultureInfo.InvariantCulture);
        }
        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        // =====================================================================
        // Data struct
        // =====================================================================
        private struct ObjEntry
        {
            public string path, mesh, mat;
            public Color32 col;
            public float cx, cy, cz;   // transform.position
            public float sx, sy, sz;   // transform.lossyScale
            public float rx, ry, rz;   // transform.eulerAngles
        }
    }

}
