// CNRSkinDumperMod.cs -- TEMPORARY debug mod.
// Dumps skin PNGs with UV wireframe baked in (one per player) + crisp raw PNG.
// Color key: HEAD=red  TRUNK=green  LEG_R=blue  LEG_L=cyan  HAND_L=yellow  HAND_R=magenta
// Output: /storage/emulated/0/CNRMods/skins/
// Log:    /storage/emulated/0/CNRMods/skin_dump.log

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CNRSkinDumperMod
{
    public static class SkinDumperEntry
    {
        private const string LogPath = "/storage/emulated/0/CNRMods/skin_dump.log";
        public  const string Version  = "1.0.0";

        public static void Load()
        {
            try
            {
                if ((SkinDumperHook)UnityEngine.Object.FindObjectOfType(typeof(SkinDumperHook)) != null) return;
                var go = new GameObject("CNRSkinDumperMod");
                go.AddComponent<SkinDumperHook>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                Log("CNRSkinDumperMod v" + Version + " loaded");
            }
            catch (Exception ex) { Log("Load error: " + ex); }
        }

        public static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); } catch { }
            try { Debug.Log("[SkinDumper] " + msg); } catch { }
        }
    }

    public class SkinDumperHook : MonoBehaviour
    {
        // Per-part display name and wire color (in-scene GL overlay AND baked UV lines)
        private static readonly string[] PartNames  = { "HEAD", "TRUNK", "LEG R", "LEG L", "HAND L", "HAND R" };
        private static readonly Color[]  PartColors = {
            new Color(1f,  0f,   0f,   1f),   // HEAD    red
            new Color(0f,  1f,   0f,   1f),   // TRUNK   green
            new Color(0f,  0.5f, 1f,   1f),   // LEG R   blue
            new Color(0f,  1f,   1f,   1f),   // LEG L   cyan
            new Color(1f,  1f,   0f,   1f),   // HAND L  yellow
            new Color(1f,  0f,   1f,   1f),   // HAND R  magenta
        };

        // GO name â†’ part index
        private static readonly Dictionary<string, int> GoNameToIdx =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "head_new",      0 },
            { "trunk_new",     1 },
            { "legright_new",  2 },
            { "legleft_new",   3 },
            { "handleft_new",  4 },
            { "handright_new", 5 },
        };

        // Child paths on other-player prefabs (from decompiled code)
        private static readonly string[] PartPaths = {
            "GameObject/EnemyAnimation/1_1/head_new",
            "GameObject/EnemyAnimation/1_2/trunk_new",
            "GameObject/EnemyAnimation/1_4/legright_new",
            "GameObject/EnemyAnimation/1_005/legleft_new",
            "GameObject/EnemyAnimation/1_006/handleft_new",
            "GameObject/1_3/handrightup/handright_Animation/handright_new",
        };

        // ---------------------------------------------------------------
        // In-scene GL bounding-box overlay
        // ---------------------------------------------------------------
        private struct PartInfo { public Renderer rend; public int idx; }
        private readonly List<PartInfo> _parts = new List<PartInfo>();
        private Material _wireMat;
        private float    _nextRefresh;

        private void Awake()
        {
            _wireMat = new Material(
                "Shader \"Hidden/CNRWire\" {" +
                "SubShader { Pass {" +
                "    ZWrite Off Cull Off Fog { Mode Off }" +
                "    BindChannels { Bind \"vertex\", vertex Bind \"color\", color }" +
                "} } }");
            _wireMat.hideFlags = HideFlags.HideAndDontSave;
        }

        private void Update()
        {
            if (Time.time >= _nextRefresh) { _nextRefresh = Time.time + 1.5f; RefreshParts(); }
        }

        private void RefreshParts()
        {
            _parts.Clear();
            var mgr = CNRMultiplayerManager.mInstance;
            if (mgr == null) return;

            var field = typeof(CNRMultiplayerManager).GetField(
                "otherPlayerObject", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var objs = (GameObject[])field.GetValue(mgr);
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] == null) continue;
                    for (int d = 0; d < PartPaths.Length; d++)
                    {
                        Transform t = objs[i].transform.Find(PartPaths[d]);
                        if (t == null) continue;
                        Renderer r = t.GetComponent<Renderer>() ?? t.gameObject.renderer;
                        if (r != null) _parts.Add(new PartInfo { rend = r, idx = d });
                    }
                }
            }

            if (mgr.myPlayerCharacterBody != null)
            {
                Renderer[] rends = mgr.myPlayerCharacterBody.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in rends)
                {
                    int idx; if (GoNameToIdx.TryGetValue(r.gameObject.name, out idx))
                        _parts.Add(new PartInfo { rend = r, idx = idx });
                }
            }

            var known = new HashSet<Renderer>();
            foreach (var p in _parts) if (p.rend != null) known.Add(p.rend);
            Renderer[] all = (Renderer[])FindObjectsOfType(typeof(Renderer));
            foreach (Renderer r in all)
            {
                if (known.Contains(r)) continue;
                int idx; if (GoNameToIdx.TryGetValue(r.gameObject.name, out idx))
                    _parts.Add(new PartInfo { rend = r, idx = idx });
            }
        }

        private void OnRenderObject()
        {
            if (_parts.Count == 0 || _wireMat == null) return;
            _wireMat.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            foreach (var p in _parts)
            {
                if (p.rend == null) continue;
                GL.Color(PartColors[p.idx]);
                DrawBounds(p.rend.bounds);
            }
            GL.End();
            GL.PopMatrix();
        }

        private static void DrawBounds(Bounds b)
        {
            Vector3 mi = b.min, ma = b.max;
            Ln(mi.x,mi.y,mi.z, ma.x,mi.y,mi.z); Ln(ma.x,mi.y,mi.z, ma.x,mi.y,ma.z);
            Ln(ma.x,mi.y,ma.z, mi.x,mi.y,ma.z); Ln(mi.x,mi.y,ma.z, mi.x,mi.y,mi.z);
            Ln(mi.x,ma.y,mi.z, ma.x,ma.y,mi.z); Ln(ma.x,ma.y,mi.z, ma.x,ma.y,ma.z);
            Ln(ma.x,ma.y,ma.z, mi.x,ma.y,ma.z); Ln(mi.x,ma.y,ma.z, mi.x,ma.y,mi.z);
            Ln(mi.x,mi.y,mi.z, mi.x,ma.y,mi.z); Ln(ma.x,mi.y,mi.z, ma.x,ma.y,mi.z);
            Ln(ma.x,mi.y,ma.z, ma.x,ma.y,ma.z); Ln(mi.x,mi.y,ma.z, mi.x,ma.y,ma.z);
        }
        private static void Ln(float x0,float y0,float z0, float x1,float y1,float z1)
        { GL.Vertex3(x0,y0,z0); GL.Vertex3(x1,y1,z1); }

        private void OnGUI()
        {
            if (_parts.Count == 0) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            var style = new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 18,
                                       alignment  = TextAnchor.MiddleCenter };
            foreach (var p in _parts)
            {
                if (p.rend == null) continue;
                Vector3 sp = cam.WorldToScreenPoint(p.rend.bounds.center);
                if (sp.z <= 0f) continue;
                style.normal.textColor = PartColors[p.idx];
                GUI.Label(new Rect(sp.x-60f, Screen.height-sp.y-24f, 120f, 48f),
                          PartNames[p.idx], style);
            }
        }

        // ---------------------------------------------------------------
        // PNG dump on room join
        // ---------------------------------------------------------------
        private void OnLevelWasLoaded(int level)
        {
            if (level > 0) StartCoroutine(DumpSkinsDelayed());
        }

        private IEnumerator DumpSkinsDelayed()
        {
            SkinDumperEntry.Log("Level loaded -- waiting 5s for players to sync...");
            yield return new WaitForSeconds(5f);
            DumpAll();
        }

        private struct RendIdx { public Renderer rend; public int idx; }

        private void DumpAll()
        {
            try
            {
                var mgr = CNRMultiplayerManager.mInstance;
                if (mgr == null) { SkinDumperEntry.Log("mInstance null -- skipping"); return; }

                string outDir = "/storage/emulated/0/CNRMods/skins/";
                Directory.CreateDirectory(outDir);
                int saved = 0;

                // --- Other players ---
                var field = typeof(CNRMultiplayerManager).GetField(
                    "otherPlayerObject", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var objs   = (GameObject[])field.GetValue(mgr);
                    var infos  = mgr.otherPlayersInfoList;
                    for (int i = 0; i < objs.Length; i++)
                    {
                        if (objs[i] == null) continue;
                        var    info = infos[i];
                        string nick = Safe(info != null ? info.mNickName : null, "player" + i);
                        string skin = Safe(info != null ? info.mSkinName  : null, "unknown");

                        var parts = new List<RendIdx>();
                        for (int d = 0; d < PartPaths.Length; d++)
                        {
                            Transform t = objs[i].transform.Find(PartPaths[d]);
                            if (t == null) continue;
                            Renderer r = t.GetComponent<Renderer>() ?? t.gameObject.renderer;
                            if (r != null) parts.Add(new RendIdx { rend = r, idx = d });
                        }
                        if (parts.Count == 0) { SkinDumperEntry.Log("No parts for " + nick); continue; }

                        Texture src = parts[0].rend.material != null ? parts[0].rend.material.mainTexture : null;
                        if (src == null) { SkinDumperEntry.Log("No texture for " + nick); continue; }

                        string bp = outDir + Sanitize(nick) + "_" + Sanitize(skin);
                        SaveAnnotated(src, parts, bp);
                        SkinDumperEntry.Log("Saved: " + bp + "_uvwire.png  (" + nick + " " + skin + ")");
                        saved++;
                    }
                }

                // --- Local player ---
                if (mgr.myPlayerInfo != null)
                {
                    string nick = Safe(mgr.myPlayerInfo.mNickName, "me");
                    string skin = Safe(mgr.myPlayerInfo.mSkinName,  "unknown");

                    // Get skin texture via HandSkinControl (always present in FP gameplay)
                    Texture src = null;
                    var hsc = (HandSkinControl)FindObjectOfType(typeof(HandSkinControl));
                    if (hsc != null && hsc.curHandSkinMaterial != null)
                        src = hsc.curHandSkinMaterial.mainTexture;

                    // Fallback: any known-part renderer in the scene
                    if (src == null)
                    {
                        Renderer[] allR = (Renderer[])FindObjectsOfType(typeof(Renderer));
                        foreach (Renderer r in allR)
                        {
                            int ix; if (!GoNameToIdx.TryGetValue(r.gameObject.name, out ix)) continue;
                            if (r.material != null && r.material.mainTexture != null)
                            { src = r.material.mainTexture; break; }
                        }
                    }

                    if (src == null)
                    {
                        SkinDumperEntry.Log("Local: no texture. HandSkinControl=" + (hsc != null ? "found" : "null"));
                    }
                    else
                    {
                        // Collect body-part renderers visible in scene.
                        // In first-person view only the hands are spawned for the local player,
                        // so fill any missing part indices from other-player GameObjects -- they
                        // share the same mesh topology; we only need the UVs, not their texture.
                        var parts    = new List<RendIdx>();
                        var foundIdx = new HashSet<int>();
                        Renderer[] allRends = (Renderer[])FindObjectsOfType(typeof(Renderer));
                        foreach (Renderer r in allRends)
                        {
                            int ix;
                            if (GoNameToIdx.TryGetValue(r.gameObject.name, out ix) && foundIdx.Add(ix))
                                parts.Add(new RendIdx { rend = r, idx = ix });
                        }

                        // Fill missing parts from other-player objects
                        if (foundIdx.Count < PartPaths.Length && field != null)
                        {
                            var fbObjs = (GameObject[])field.GetValue(mgr);
                            for (int i = 0; i < fbObjs.Length && foundIdx.Count < PartPaths.Length; i++)
                            {
                                if (fbObjs[i] == null) continue;
                                for (int d = 0; d < PartPaths.Length; d++)
                                {
                                    if (foundIdx.Contains(d)) continue;
                                    Transform t = fbObjs[i].transform.Find(PartPaths[d]);
                                    if (t == null) continue;
                                    Renderer r2 = t.GetComponent<Renderer>() ?? t.gameObject.renderer;
                                    if (r2 != null) { parts.Add(new RendIdx { rend = r2, idx = d }); foundIdx.Add(d); }
                                }
                            }
                        }

                        SkinDumperEntry.Log("Local parts found: " + parts.Count + "/" + PartPaths.Length +
                                            " (scene=" + foundIdx.Count + ")");

                        string bp = outDir + Sanitize(nick) + "_" + Sanitize(skin) + "_local";
                        SaveAnnotated(src, parts, bp);
                        SkinDumperEntry.Log("Saved local: " + bp + "_uvwire.png  (parts=" + parts.Count + ")");
                        saved++;
                    }
                }

                SkinDumperEntry.Log("Done. " + saved + " skin(s) saved.");
                SkinDumperEntry.Log("Color key: HEAD=red  TRUNK=green  LEG_R=blue  LEG_L=cyan  HAND_L=yellow  HAND_R=magenta");
            }
            catch (Exception ex) { SkinDumperEntry.Log("DumpAll error: " + ex); }
        }

        // Save <base>_raw.png  (4x upscaled, crisp, no annotations)
        // and  <base>_uvwire.png (4x upscaled, thin 1px boundary edges + small labels baked in).
        private static void SaveAnnotated(Texture src, List<RendIdx> parts, string basePath)
        {
            FilterMode oldFilter = src.filterMode;
            src.filterMode = FilterMode.Point;

            RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Point;
            tex.ReadPixels(new Rect(0f, 0f, src.width, src.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            src.filterMode = oldFilter;

            // Raw: 4x nearest-neighbour upscale, no annotations
            Texture2D rawUp = ScaleUp(tex, 4);
            File.WriteAllBytes(basePath + "_raw.png", rawUp.EncodeToPNG());
            Destroy(rawUp);

            // Annotated: upscale FIRST, then draw thin 1px lines/text on the big canvas.
            // Drawing on big canvas also gives 4x more UV pixel positions, which correctly
            // resolves boundary edges for small UV regions (head, trunk, legs).
            Texture2D big = ScaleUp(tex, 4);
            Destroy(tex);
            foreach (var pr in parts)
            {
                if (pr.rend == null) continue;
                Mesh mesh = GetMesh(pr.rend);
                if (mesh == null) continue;
                Color col = PartColors[pr.idx];
                DrawUVBoundary(big, mesh, col);
                Vector2 cen = GetUVCentroid(mesh);
                int cx = (int)(cen.x * (big.width  - 1));
                int cy = (int)(cen.y * (big.height - 1));
                DrawBitmapText(big, PartNames[pr.idx], cx, cy, col);
            }
            big.Apply();
            File.WriteAllBytes(basePath + "_uvwire.png", big.EncodeToPNG());
            Destroy(big);
        }

        private static Mesh GetMesh(Renderer r)
        {
            var smr = r as SkinnedMeshRenderer;
            if (smr != null) return smr.sharedMesh;
            var mf = r.GetComponent<MeshFilter>();
            return mf != null ? mf.sharedMesh : null;
        }

        private static Vector2 GetUVCentroid(Mesh mesh)
        {
            Vector2[] uvs = mesh.uv;
            if (uvs == null || uvs.Length == 0) return new Vector2(0.5f, 0.5f);
            float sx = 0f, sy = 0f;
            foreach (var uv in uvs) { sx += uv.x; sy += uv.y; }
            return new Vector2(sx / uvs.Length, sy / uvs.Length);
        }

        // Draw BOUNDARY edges only: UV edges belonging to exactly ONE triangle.
        private static void DrawUVBoundary(Texture2D tex, Mesh mesh, Color col)
        {
            Vector2[] uvs  = mesh.uv;
            int[]     tris = mesh.triangles;
            if (uvs == null || uvs.Length == 0 || tris == null) return;
            int w = tex.width, h = tex.height;
            var count  = new Dictionary<long, int>();
            var coords = new Dictionary<long, int[]>();
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                int ax=(int)(uvs[tris[i  ]].x*(w-1)), ay=(int)(uvs[tris[i  ]].y*(h-1));
                int bx=(int)(uvs[tris[i+1]].x*(w-1)), by=(int)(uvs[tris[i+1]].y*(h-1));
                int cx=(int)(uvs[tris[i+2]].x*(w-1)), cy=(int)(uvs[tris[i+2]].y*(h-1));
                CountEdge(count, coords, ax,ay, bx,by);
                CountEdge(count, coords, bx,by, cx,cy);
                CountEdge(count, coords, cx,cy, ax,ay);
            }
            foreach (var kv in count)
            {
                if (kv.Value != 1) continue;
                int[] e = coords[kv.Key];
                DrawLine(tex, e[0],e[1], e[2],e[3], col);
            }
        }

        private static long EdgeKey(int x0,int y0, int x1,int y1)
        {
            long p0=(long)x0*4096+y0, p1=(long)x1*4096+y1;
            if (p0>p1){long t=p0;p0=p1;p1=t;}
            return (p0<<24)|p1;
        }

        private static void CountEdge(Dictionary<long,int> count, Dictionary<long,int[]> coords,
                                       int x0,int y0, int x1,int y1)
        {
            long key = EdgeKey(x0,y0,x1,y1);
            if (!count.ContainsKey(key))
            {
                count[key] = 0;
                long p0=(long)x0*4096+y0, p1=(long)x1*4096+y1;
                coords[key] = (p0<=p1) ? new int[]{x0,y0,x1,y1} : new int[]{x1,y1,x0,y0};
            }
            count[key]++;
        }

        // Nearest-neighbour upscale (caller must Destroy result).
        private static Texture2D ScaleUp(Texture2D src, int factor)
        {
            int nw=src.width*factor, nh=src.height*factor;
            Texture2D dst = new Texture2D(nw, nh, TextureFormat.ARGB32, false);
            dst.filterMode = FilterMode.Point;
            Color[] sp = src.GetPixels();
            Color[] dp = new Color[nw*nh];
            for (int y=0;y<src.height;y++)
                for (int x=0;x<src.width;x++)
                {
                    Color c = sp[y*src.width+x];
                    for (int dy=0;dy<factor;dy++)
                        for (int dx=0;dx<factor;dx++)
                            dp[(y*factor+dy)*nw+(x*factor+dx)] = c;
                }
            dst.SetPixels(dp);
            dst.Apply();
            return dst;
        }

        // ---------------------------------------------------------------
        // 5x7 bitmap font (only chars needed for part labels).
        // byte[7]: one byte per row top-to-bottom, bit4=leftmost pixel.
        // ---------------------------------------------------------------
        private static readonly Dictionary<char,byte[]> Font5x7 = new Dictionary<char,byte[]>
        {
            { ' ', new byte[]{ 0x00,0x00,0x00,0x00,0x00,0x00,0x00 } },
            { 'A', new byte[]{ 0x0E,0x11,0x11,0x1F,0x11,0x11,0x11 } },
            { 'D', new byte[]{ 0x1E,0x11,0x11,0x11,0x11,0x11,0x1E } },
            { 'E', new byte[]{ 0x1F,0x10,0x10,0x1E,0x10,0x10,0x1F } },
            { 'G', new byte[]{ 0x0E,0x11,0x10,0x17,0x11,0x11,0x0E } },
            { 'H', new byte[]{ 0x11,0x11,0x11,0x1F,0x11,0x11,0x11 } },
            { 'K', new byte[]{ 0x11,0x12,0x14,0x18,0x14,0x12,0x11 } },
            { 'L', new byte[]{ 0x10,0x10,0x10,0x10,0x10,0x10,0x1F } },
            { 'N', new byte[]{ 0x11,0x19,0x15,0x13,0x11,0x11,0x11 } },
            { 'R', new byte[]{ 0x1E,0x11,0x11,0x1E,0x14,0x12,0x11 } },
            { 'T', new byte[]{ 0x1F,0x04,0x04,0x04,0x04,0x04,0x04 } },
            { 'U', new byte[]{ 0x11,0x11,0x11,0x11,0x11,0x11,0x0E } },
        };

        // Draw text centered at (cx,cy). Each char is 5x7px + 1px gap.
        // Texture2D y=0 is bottom; row 0 of font = top of char = highest y.
        private static void DrawBitmapText(Texture2D tex, string text, int cx, int cy, Color col)
        {
            const int CW=6, CH=7;
            int totalW = text.Length*CW-1;
            int startX  = cx - totalW/2;
            int startY  = cy + CH/2;
            int w=tex.width, h=tex.height;
            for (int ci=0;ci<text.Length;ci++)
            {
                byte[] rows;
                if (!Font5x7.TryGetValue(char.ToUpper(text[ci]), out rows)) continue;
                int ox = startX + ci*CW;
                for (int row=0;row<CH;row++)
                {
                    byte bits = rows[row];
                    int py = startY - row;
                    if (py<0||py>=h) continue;
                    for (int bit=0;bit<5;bit++)
                    {
                        if ((bits & (0x10>>bit))==0) continue;
                        int px = ox+bit;
                        if (px>=0&&px<w) tex.SetPixel(px, py, col);
                    }
                }
            }
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
        {
            int w=tex.width, h=tex.height;
            int dx=Math.Abs(x1-x0), sx=x0<x1?1:-1;
            int dy=Math.Abs(y1-y0), sy=y0<y1?1:-1;
            int err=(dx>dy?dx:-dy)/2;
            while(true)
            {
                if(x0>=0&&x0<w&&y0>=0&&y0<h) tex.SetPixel(x0,y0,col);
                if(x0==x1&&y0==y1) break;
                int e2=err;
                if(e2>-dx){err-=dy;x0+=sx;}
                if(e2< dy){err+=dx;y0+=sy;}
            }
        }

        private static string Safe(string s, string fallback)
        {
            return (s!=null&&s.Length>0)?s:fallback;
        }

        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
                if(c!='/'&&c!='\\'&&c!=':'&&c!='*'&&c!='?'&&c!='"'&&c!='<'&&c!='>'&&c!='|')
                    sb.Append(c);
            return sb.Length>0?sb.ToString():"unnamed";
        }
    }
}
