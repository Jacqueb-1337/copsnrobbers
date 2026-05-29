using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace CNRMods
{
    // CNRSkinDumperMod — dumps the roleModel mesh + baked texture when Brigand
    // (or any target skin) is displayed in the UIStoreDirector skin shop.
    //
    // Output is written to /sdcard/CNRMods/skin_dump/ :
    //   roleModel_<skinName>.obj          — all submeshes as a single OBJ
    //   roleModel_<skinName>_<part>.png   — baked texture per unique material
    //
    // Install:
    //   adb push CNRSkinDumperMod.dll /sdcard/CNRMods/CNRSkinDumperMod.dll
    //   (mod loader auto-loads any .dll in /sdcard/CNRMods/ on startup)
    //
    // Load entry-point called by MainMenuDirector_LoadMods_patch (mod loader).
    public static class CNRSkinDumperMod_Entry
    {
        public static void Load()
        {
            var go = new GameObject("CNRSkinDumperMod");
            go.AddComponent<CNRSkinDumper>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("[SkinDumper] loaded");
        }
    }

    public class CNRSkinDumper : MonoBehaviour
    {
        const string OUT_DIR    = "/sdcard/CNRMods/skin_dump/";
        const string TARGET_SKIN = "Brigand";   // mNameDisplay to wait for; set "" to dump whatever is shown

        bool _dumped = false;

        void Start()
        {
            if (!Directory.Exists(OUT_DIR))
                Directory.CreateDirectory(OUT_DIR);
            StartCoroutine(WaitAndDump());
        }

        IEnumerator WaitAndDump()
        {
            Debug.Log("[SkinDumper] waiting for StoreScene ...");

            // Wait until UIStoreDirector is available
            while (UIStoreDirector.mInstance == null)
                yield return new WaitForSeconds(0.5f);

            Debug.Log("[SkinDumper] UIStoreDirector found, watching for skin: " +
                      (TARGET_SKIN == "" ? "<any>" : TARGET_SKIN));

            UIStoreDirector store = UIStoreDirector.mInstance;

            // Poll until the target skin is selected
            while (!_dumped)
            {
                yield return new WaitForSeconds(0.3f);

                if (store == null) { store = UIStoreDirector.mInstance; continue; }

                // Read curSkinId via reflection (private field)
                int curId = -1;
                try
                {
                    var fi = typeof(UIStoreDirector).GetField("curSkinId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fi != null) curId = (int)fi.GetValue(store);
                }
                catch { }

                if (curId < 0 || store.gSkinItemInfo == null || curId >= store.gSkinItemInfo.Length)
                    continue;

                string displayName = store.gSkinItemInfo[curId].mNameDisplay ?? "";

                bool isTarget = TARGET_SKIN == "" ||
                                displayName.IndexOf(TARGET_SKIN, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!isTarget) continue;

                // Find roleModel
                GameObject role = null;
                try
                {
                    var fi = typeof(UIStoreDirector).GetField("roleModel",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    // roleModel is public in the decompiled source
                    if (fi != null) role = (GameObject)fi.GetValue(store);
                }
                catch { }

                // Fallback: it's a public field, try direct access via reflection on the instance
                if (role == null)
                {
                    var prop = typeof(UIStoreDirector).GetField("roleModel",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.FlattenHierarchy);
                    if (prop != null) role = (GameObject)prop.GetValue(store);
                }

                if (role == null)
                {
                    Debug.Log("[SkinDumper] roleModel not found on UIStoreDirector. Trying body1...");
                    // Fallback: grab body1-6 via reflection
                    DumpBodyParts(store, displayName);
                    _dumped = true;
                    continue;
                }

                Debug.Log("[SkinDumper] Found roleModel: " + role.name +
                          " for skin: " + displayName + " — dumping ...");
                DumpRoleModel(role, displayName);
                _dumped = true;
            }

            Debug.Log("[SkinDumper] dump complete. Files in " + OUT_DIR);
        }

        // -----------------------------------------------------------------------
        //  Dump roleModel: walk every Renderer child, export each Mesh as OBJ
        //  and each unique Texture2D as PNG.
        // -----------------------------------------------------------------------
        void DumpRoleModel(GameObject root, string skinName)
        {
            string safeName = MakeSafe(skinName);

            // Collect all renderers (MeshRenderer + SkinnedMeshRenderer)
            Component[] components = root.GetComponentsInChildren(typeof(Renderer));

            var objBuilder   = new StringBuilder();
            int vertexOffset = 0;
            int partIndex    = 0;

            foreach (Component comp in components)
            {
                Renderer r = comp as Renderer;
                if (r == null) continue;

                string partName = r.gameObject.name;

                // --- Dump texture ---
                if (r.material != null && r.material.mainTexture != null)
                {
                    Texture2D srcTex = r.material.mainTexture as Texture2D;
                    if (srcTex != null)
                    {
                        string texFile = OUT_DIR + safeName + "_tex_" + MakeSafe(partName) + ".png";
                        SaveTexture(srcTex, texFile);
                        Debug.Log("[SkinDumper]   texture -> " + texFile);
                    }
                }

                // --- Extract mesh ---
                Mesh mesh = GetMesh(r);
                if (mesh == null) { Debug.Log("[SkinDumper]   " + partName + ": no mesh"); continue; }

                // Write OBJ group for this part
                objBuilder.AppendLine("g " + MakeSafe(partName));

                Vector3[] verts = mesh.vertices;
                Vector2[] uvs   = mesh.uv;
                Vector3[] norms = mesh.normals;

                // Transform vertices to world space
                Transform t = r.gameObject.transform;
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 wv = t.TransformPoint(verts[i]);
                    objBuilder.AppendLine("v " + F(wv.x) + " " + F(wv.y) + " " + F(wv.z));
                }
                for (int i = 0; i < uvs.Length; i++)
                    objBuilder.AppendLine("vt " + F(uvs[i].x) + " " + F(uvs[i].y));
                for (int i = 0; i < norms.Length; i++)
                {
                    Vector3 wn = t.TransformDirection(norms[i]);
                    objBuilder.AppendLine("vn " + F(wn.x) + " " + F(wn.y) + " " + F(wn.z));
                }

                bool hasUv  = uvs  != null && uvs.Length  == verts.Length;
                bool hasNrm = norms != null && norms.Length == verts.Length;

                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    int[] tris = mesh.GetTriangles(sub);
                    for (int ti = 0; ti < tris.Length; ti += 3)
                    {
                        int a = tris[ti]     + vertexOffset + 1;
                        int b = tris[ti + 1] + vertexOffset + 1;
                        int c = tris[ti + 2] + vertexOffset + 1;
                        if (hasUv && hasNrm)
                            objBuilder.AppendLine("f " + a+"/"+a+"/"+a + " " + b+"/"+b+"/"+b + " " + c+"/"+c+"/"+c);
                        else if (hasUv)
                            objBuilder.AppendLine("f " + a+"/"+a + " " + b+"/"+b + " " + c+"/"+c);
                        else
                            objBuilder.AppendLine("f " + a + " " + b + " " + c);
                    }
                }

                vertexOffset += verts.Length;
                partIndex++;
                Debug.Log("[SkinDumper]   " + partName + ": " + verts.Length + " verts, " + (mesh.triangles.Length/3) + " tris");
            }

            // Write OBJ file
            string objPath = OUT_DIR + safeName + "_roleModel.obj";
            File.WriteAllText(objPath, objBuilder.ToString());
            Debug.Log("[SkinDumper] OBJ written -> " + objPath);

            // Also write a summary of all part names and their local positions
            var summary = new StringBuilder();
            summary.AppendLine("skin: " + skinName);
            summary.AppendLine("roleModel: " + root.name);
            summary.AppendLine("parts:");
            WriteHierarchy(root.transform, summary, 0);
            File.WriteAllText(OUT_DIR + safeName + "_hierarchy.txt", summary.ToString());
            Debug.Log("[SkinDumper] hierarchy -> " + OUT_DIR + safeName + "_hierarchy.txt");
        }

        // Fallback when roleModel field not found — dump body1-6
        void DumpBodyParts(UIStoreDirector store, string skinName)
        {
            string safeName = MakeSafe(skinName);
            string[] fields = { "body1","body2","body3","body4","body5","body6" };
            var objBuilder  = new StringBuilder();
            int vOff        = 0;

            foreach (string fieldName in fields)
            {
                var fi = typeof(UIStoreDirector).GetField(fieldName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (fi == null) continue;
                GameObject go = (GameObject)fi.GetValue(store);
                if (go == null) continue;

                // Dump texture
                Renderer r = go.GetComponent<Renderer>();
                if (r == null) r = go.GetComponentInChildren<Renderer>();
                if (r != null && r.material != null && r.material.mainTexture != null)
                {
                    Texture2D t2 = r.material.mainTexture as Texture2D;
                    if (t2 != null) SaveTexture(t2, OUT_DIR + safeName + "_tex_" + fieldName + ".png");
                }

                // Dump mesh
                Component[] renderers = go.GetComponentsInChildren(typeof(Renderer));
                foreach (Component comp in renderers)
                {
                    Renderer rr = comp as Renderer;
                    if (rr == null) continue;
                    Mesh mesh = GetMesh(rr);
                    if (mesh == null) continue;

                    objBuilder.AppendLine("g " + fieldName + "_" + rr.gameObject.name);
                    Vector3[] verts = mesh.vertices;
                    Vector2[] uvs   = mesh.uv;
                    Transform t     = rr.gameObject.transform;
                    for (int i = 0; i < verts.Length; i++)
                    { Vector3 w = t.TransformPoint(verts[i]); objBuilder.AppendLine("v " + F(w.x) + " " + F(w.y) + " " + F(w.z)); }
                    for (int i = 0; i < uvs.Length; i++)
                        objBuilder.AppendLine("vt " + F(uvs[i].x) + " " + F(uvs[i].y));
                    int[] tris = mesh.triangles;
                    bool hasUv = uvs != null && uvs.Length == verts.Length;
                    for (int ti = 0; ti < tris.Length; ti += 3)
                    {
                        int a=tris[ti]+vOff+1, b=tris[ti+1]+vOff+1, c=tris[ti+2]+vOff+1;
                        if (hasUv) objBuilder.AppendLine("f " + a+"/"+a+" " + b+"/"+b+" " + c+"/"+c);
                        else       objBuilder.AppendLine("f " + a + " " + b + " " + c);
                    }
                    vOff += verts.Length;
                }
            }

            string objPath = OUT_DIR + safeName + "_bodyparts.obj";
            File.WriteAllText(objPath, objBuilder.ToString());
            Debug.Log("[SkinDumper] body-parts OBJ -> " + objPath);
        }

        // -----------------------------------------------------------------------
        //  Helpers
        // -----------------------------------------------------------------------

        static Mesh GetMesh(Renderer r)
        {
            var smr = r as SkinnedMeshRenderer;
            if (smr != null)
            {
                // Bake the skinned mesh into its current pose
                Mesh baked = new Mesh();
                smr.BakeMesh(baked);
                return baked;
            }
            var mf = r.GetComponent<MeshFilter>();
            if (mf != null) return mf.sharedMesh ?? mf.mesh;
            return null;
        }

        static void SaveTexture(Texture2D src, string path)
        {
            try
            {
                // Try to read via RenderTexture blit (works for non-readable textures)
                RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(src, rt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D readable = new Texture2D(src.width, src.height, TextureFormat.ARGB32, false);
                readable.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
                readable.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);

                byte[] png = readable.EncodeToPNG();
                File.WriteAllBytes(path, png);
                UnityEngine.Object.Destroy(readable);
            }
            catch (Exception ex)
            {
                Debug.Log("[SkinDumper] SaveTexture failed for " + path + ": " + ex.Message);
            }
        }

        static void WriteHierarchy(Transform t, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            string pos    = t.localPosition.x.ToString("F4") + "," +
                            t.localPosition.y.ToString("F4") + "," +
                            t.localPosition.z.ToString("F4");
            string rot    = t.localEulerAngles.x.ToString("F2") + "," +
                            t.localEulerAngles.y.ToString("F2") + "," +
                            t.localEulerAngles.z.ToString("F2");
            string scale  = t.localScale.x.ToString("F4") + "," +
                            t.localScale.y.ToString("F4") + "," +
                            t.localScale.z.ToString("F4");

            // Check for renderer / mesh
            string extras = "";
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
            {
                Mesh m = null;
                var smr = r as SkinnedMeshRenderer;
                if (smr != null) m = smr.sharedMesh;
                else { var mf = r.GetComponent<MeshFilter>(); if (mf != null) m = mf.sharedMesh; }
                if (m != null) extras += " [Mesh:" + m.name + " " + m.vertexCount + "v " + (m.triangles.Length/3) + "t]";
                if (r.material != null) extras += " [Mat:" + r.material.name + "]";
            }

            sb.AppendLine(indent + t.gameObject.name +
                          "  pos=(" + pos + ") rot=(" + rot + ") scale=(" + scale + ")" + extras);

            for (int i = 0; i < t.childCount; i++)
                WriteHierarchy(t.GetChild(i), sb, depth + 1);
        }

        static string MakeSafe(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }

        static string F(float v) { return v.ToString("F6"); }
    }
}
