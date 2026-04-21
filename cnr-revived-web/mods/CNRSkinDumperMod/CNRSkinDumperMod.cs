// CNRSkinDumperMod.cs -- TEMPORARY debug mod: dumps skin PNGs for all players in room on join.
// Delete this mod when skin support work is complete.
// Skins saved to: /storage/emulated/0/CNRMods/skins/<nick>_<skinName>.png
// Log at:         /storage/emulated/0/CNRMods/skin_dump.log

using System;
using System.Collections;
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
                if (UnityEngine.Object.FindObjectOfType<SkinDumperHook>() != null) return;
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
        // Fired by Unity whenever a level finishes loading.
        private void OnLevelWasLoaded(int level)
        {
            // Level 0 = main menu; anything higher = a game map.
            if (level > 0)
                StartCoroutine(DumpSkinsDelayed());
        }

        // Wait a few seconds so that all players have spawned and PlayerInfo has been synced.
        private IEnumerator DumpSkinsDelayed()
        {
            SkinDumperEntry.Log("Level loaded -- waiting 5s for players to sync...");
            yield return new WaitForSeconds(5f);
            DumpAll();
        }

        private void DumpAll()
        {
            try
            {
                var mgr = CNRMultiplayerManager.mInstance;
                if (mgr == null) { SkinDumperEntry.Log("CNRMultiplayerManager.mInstance is null -- skipping"); return; }

                // Grab private otherPlayerObject[] via reflection.
                var field = typeof(CNRMultiplayerManager).GetField(
                    "otherPlayerObject", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null) { SkinDumperEntry.Log("Could not find otherPlayerObject field"); return; }

                var otherObjs  = (GameObject[])field.GetValue(mgr);
                var otherInfos = mgr.otherPlayersInfoList;

                string outDir = "/storage/emulated/0/CNRMods/skins/";
                Directory.CreateDirectory(outDir);

                int saved = 0;

                // --- Other players ---
                for (int i = 0; i < otherObjs.Length; i++)
                {
                    if (otherObjs[i] == null) continue;

                    var info     = otherInfos[i];
                    string nick  = Safe(info != null ? info.mNickName : null, "player" + i);
                    string skin  = Safe(info != null ? info.mSkinName  : null, "unknown");

                    // Known child path from decompiled code.
                    Transform trunkT = otherObjs[i].transform.Find("GameObject/EnemyAnimation/1_2/trunk_new");
                    if (trunkT == null)
                    {
                        SkinDumperEntry.Log("No trunk child for slot " + i + " (" + nick + ")");
                        continue;
                    }

                    Renderer rend = trunkT.gameObject.renderer;
                    if (rend == null || rend.material == null || rend.material.mainTexture == null)
                    {
                        SkinDumperEntry.Log("No texture on renderer for " + nick);
                        continue;
                    }

                    string path = outDir + Sanitize(nick) + "_" + Sanitize(skin) + ".png";
                    SaveTexturePng(rend.material.mainTexture, path);
                    SkinDumperEntry.Log("Saved: " + path + "  (nick=" + nick + " skin=" + skin + ")");
                    saved++;
                }

                // --- Local (own) player ---
                if (mgr.myPlayerCharacterBody != null && mgr.myPlayerInfo != null)
                {
                    string nick = Safe(mgr.myPlayerInfo.mNickName, "me");
                    string skin = Safe(mgr.myPlayerInfo.mSkinName,  "unknown");

                    // Try both EnemyAnimation (some modes) and without it.
                    string[] candidatePaths = new[]
                    {
                        "GameObject/EnemyAnimation/1_2/trunk_new",
                        "EnemyAnimation/1_2/trunk_new",
                        "1_2/trunk_new",
                    };

                    bool found = false;
                    foreach (string cp in candidatePaths)
                    {
                        Transform t = mgr.myPlayerCharacterBody.Find(cp);
                        if (t == null) continue;
                        Renderer r = t.gameObject.renderer;
                        if (r == null || r.material == null || r.material.mainTexture == null) continue;

                        string path = outDir + Sanitize(nick) + "_" + Sanitize(skin) + "_local.png";
                        SaveTexturePng(r.material.mainTexture, path);
                        SkinDumperEntry.Log("Saved local: " + path);
                        saved++;
                        found = true;
                        break;
                    }

                    if (!found)
                        SkinDumperEntry.Log("Local player skin (not found via renderer): " + nick + " -> " + skin);
                }

                SkinDumperEntry.Log("Done. " + saved + " skin(s) saved to " + outDir);
            }
            catch (Exception ex)
            {
                SkinDumperEntry.Log("DumpAll error: " + ex);
            }
        }

        // Copy any Texture (including non-readable bundled textures) to a new Texture2D via
        // RenderTexture blit, then encode as PNG and write to disk.
        private static void SaveTexturePng(Texture src, string filePath)
        {
            RenderTexture rt   = RenderTexture.GetTemporary(src.width, src.height, 0);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex2d = new Texture2D(src.width, src.height, TextureFormat.ARGB32, false);
            tex2d.ReadPixels(new Rect(0f, 0f, src.width, src.height), 0, 0);
            tex2d.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            byte[] png = tex2d.EncodeToPNG();
            File.WriteAllBytes(filePath, png);
            Destroy(tex2d);
        }

        private static string Safe(string s, string fallback)
        {
            return (s != null && s.Length > 0) ? s : fallback;
        }

        // Strip chars that are illegal in Android file paths.
        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c != '/' && c != '\\' && c != ':' && c != '*' &&
                    c != '?' && c != '"'  && c != '<' && c != '>'  && c != '|')
                    sb.Append(c);
            }
            return sb.Length > 0 ? sb.ToString() : "unnamed";
        }
    }
}
