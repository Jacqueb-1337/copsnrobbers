using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CNRMods
{
    /// <summary>
    /// Custom Maps Mod — extends the WW room creation map selector with extra scene slots.
    /// The right-arrow on the last standard map (FreeRun12_1) now continues into custom maps
    /// instead of wrapping back to FreeRun3_1. The left-arrow on FreeRun3_1 wraps to the last
    /// custom map. Custom map names display as an orange OnGUI overlay on the create-room panel.
    ///
    /// How it works (no Harmony needed):
    ///   NGUI buttons call methods via UIButtonMessage.target.SendMessage(functionName).
    ///   We find all UIButtonMessage components with functionName=="ToWWNextMap" / "ToWWPreMap"
    ///   and redirect their target to this MonoBehaviour, which provides replacement handlers
    ///   covering the full extended map list.
    ///
    /// Adding / removing custom maps:
    ///   Edit CUSTOM_MAPS and CUSTOM_NAMES below. Scene names must be real Unity scenes
    ///   compiled into the APK (e.g. FreeRun13_1, FreeRun14_1, FreeRun15_1 …).
    /// </summary>
    public static class CustomMapsEntry
    {
        public static void Initialize()
        {
            try
            {
                var go = new GameObject("CNRCustomMaps");
                go.AddComponent<CustomMapsHook>();
                GameObject.DontDestroyOnLoad(go);
                ModEntry.Log("CustomMaps mod initialized");
            }
            catch (Exception ex)
            {
                ModEntry.Log("CustomMaps init failed: " + ex);
            }
        }
    }

    public class CustomMapsHook : MonoBehaviour
    {
        // ── Standard maps (the 10 maps already in the game's selector, index matches mWWMapTexture[]) ──
        static readonly string[] STANDARD_MAPS =
        {
            "FreeRun3_1",   // mWWMapTexture[0]  "Death Platform"
            "FreeRun4_1",   // mWWMapTexture[1]  "Warehouse"
            "FreeRun5_1",   // mWWMapTexture[2]  "Snow"
            "FreeRun6_1",   // mWWMapTexture[3]  "Dusty"
            "FreeRun7_1",   // mWWMapTexture[4]  "Knife Factory"
            "FreeRun8_1",   // mWWMapTexture[5]  "Desert A"
            "FreeRun9_1",   // mWWMapTexture[6]  "Desert B"
            "FreeRun10_1",  // mWWMapTexture[7]  "Chess Hero"
            "FreeRun11_1",  // mWWMapTexture[8]  "Maze Village"
            "FreeRun12_1",  // mWWMapTexture[9]  "Christmas Town"
        };

        // ── Extra scene slots to unlock — add more here as needed ──
        static readonly string[] CUSTOM_MAPS =
        {
            "FreeRun13_1",
            "FreeRun14_1",
            "FreeRun15_1",
        };

        // ── Display names shown in the orange overlay while on a custom map ──
        static readonly Dictionary<string, string> CUSTOM_NAMES = new Dictionary<string, string>
        {
            { "FreeRun13_1", "[MOD] Arena" },
            { "FreeRun14_1", "[MOD] Open Field" },
            { "FreeRun15_1", "[MOD] Rooftop" },
        };

        // combined, built in Awake
        string[] _allMaps;

        // Track whether we've hooked the buttons in the current scene load
        bool _hooked = false;

        // ── Lifecycle ────────────────────────────────────────────────────────────────

        void Awake()
        {
            var list = new List<string>(STANDARD_MAPS);
            list.AddRange(CUSTOM_MAPS);
            _allMaps = list.ToArray();
        }

        void OnLevelWasLoaded(int level)
        {
            _hooked = false;
            if (Application.loadedLevelName == "MultiplayerSelect")
                StartCoroutine(HookButtons());
        }

        IEnumerator HookButtons()
        {
            // Give NGUI a moment to finish initialising before we iterate components
            yield return new WaitForSeconds(0.25f);

            var allMsgs = (UIButtonMessage[])GameObject.FindObjectsOfType(typeof(UIButtonMessage));
            int hooked = 0;
            foreach (var msg in allMsgs)
            {
                if (msg.functionName == "ToWWNextMap" || msg.functionName == "ToWWPreMap")
                {
                    msg.target = ((Component)this).gameObject;
                    hooked++;
                    ModEntry.Log("Hooked UIButtonMessage: " + msg.functionName +
                                 " on " + ((Component)msg).gameObject.name);
                }
            }
            _hooked = hooked > 0;
            ModEntry.Log("Button hook complete — hooked " + hooked + " button(s)");
        }

        // ── Navigation handlers (called by NGUI via SendMessage) ──────────────────

        /// <summary>Called when the player taps the right-arrow on the map selector.</summary>
        public void ToWWNextMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            int idx = Array.IndexOf(_allMaps, msd.mCurWWMapSelect);
            if (idx < 0)                     idx = 0;                       // unknown → first
            else if (idx >= _allMaps.Length - 1) idx = 0;                   // last → wrap to first
            else                             idx++;

            ApplyMap(msd, idx);
        }

        /// <summary>Called when the player taps the left-arrow on the map selector.</summary>
        public void ToWWPreMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            int idx = Array.IndexOf(_allMaps, msd.mCurWWMapSelect);
            if (idx <= 0) idx = _allMaps.Length - 1;   // first/unknown → wrap to last
            else          idx--;

            ApplyMap(msd, idx);
        }

        // ── Internal helpers ──────────────────────────────────────────────────────

        void ApplyMap(MultiplayerSelectDirector msd, int idx)
        {
            string scene = _allMaps[idx];
            msd.mCurWWMapSelect = scene;

            int stdIdx = Array.IndexOf(STANDARD_MAPS, scene);
            if (stdIdx >= 0)
            {
                // Standard map — use its own preview texture and let the game's
                // mode-checkbox logic run normally.
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[stdIdx];
                msd.mWWMapUITexture.MarkAsChanged();
                msd.WWResetModeCheckBox();
            }
            else
            {
                // Custom map — borrow the last standard texture as a placeholder,
                // then manually set checkboxes (TDM + KC = yes, Stronghold = no).
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
                msd.mWWMapUITexture.MarkAsChanged();

                msd.mModeCheckBoxSH.SetActive(false);
                msd.mModeCheckBoxTDM.SetActive(true);

                // Switch away from Stronghold unconditionally — it's not supported on
                // custom maps and the SH checkbox is now hidden, so TDM is the safe default.
                msd.SwitchToMode(GrowthGameModeTag.tTeamDeathMatch);
            }

            ModEntry.Log("Map changed to: " + scene);
        }

        // ── HUD overlay ──────────────────────────────────────────────────────────

        /// <summary>
        /// Draws an orange name label on the map preview area when a custom map is active,
        /// and a small warning banner to remind the host that all players need this mod.
        /// </summary>
        void OnGUI()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;
            if (msd.mCurWWSubScene != MSD_SubSceneInWorldWide.RoomCreate) return;

            string scene = msd.mCurWWMapSelect;
            if (!CUSTOM_NAMES.ContainsKey(scene)) return;   // standard map — nothing to overlay

            string displayName = CUSTOM_NAMES[scene];

            // ── Map name ──
            var nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            float sw = Screen.width, sh = Screen.height;
            float bw = 280f, bh = 40f;
            float bx = sw * 0.5f - bw * 0.5f;
            float by = sh * 0.34f;

            GUI.color = new Color(1f, 0.55f, 0.05f, 0.95f);
            GUI.Label(new Rect(bx, by, bw, bh), displayName, nameStyle);

            // ── Mod-required notice ──
            var noteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
            };
            GUI.color = new Color(1f, 1f, 0.3f, 0.85f);
            GUI.Label(new Rect(bx, by + bh, bw, 28f), "Requires mod on all clients", noteStyle);

            GUI.color = Color.white;
        }
    }
}
