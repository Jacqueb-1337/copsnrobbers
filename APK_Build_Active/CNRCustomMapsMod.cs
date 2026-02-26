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

        // ── Extra scene slots — display name + which real scene to actually load ──
        // FreeRun13_1+ exist in the APK but lack multiplayer game controller scripts,
        // so we keep the custom name for the UI/room-list while loading a working base scene.
        static readonly string[] CUSTOM_MAPS =
        {
            "FreeRun13_1",
            "FreeRun14_1",
            "FreeRun15_1",
        };

        // Display names shown in the orange overlay
        static readonly Dictionary<string, string> CUSTOM_NAMES = new Dictionary<string, string>
        {
            { "FreeRun13_1", "[MOD] Map 11" },
            { "FreeRun14_1", "[MOD] Map 12" },
            { "FreeRun15_1", "[MOD] Map 13" },
        };

        // Which real (working) scene to actually LoadLevel for each custom slot.
        // Until proper scene injection is implemented, reuse existing multiplayer scenes.
        static readonly Dictionary<string, string> CUSTOM_SCENE_LOAD = new Dictionary<string, string>
        {
            { "FreeRun13_1", "FreeRun3_1"  },
            { "FreeRun14_1", "FreeRun5_1"  },
            { "FreeRun15_1", "FreeRun8_1"  },
        };

        // combined, built in Awake
        string[] _allMaps;

        // Track whether we've hooked the buttons in the current scene load
        bool _hooked = false;
        MSD_SubSceneInWorldWide _lastSubScene = (MSD_SubSceneInWorldWide)(-1);
        int _virtualIdx = 0; // tracks position in _allMaps independently of mCurWWMapSelect

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
            _lastSubScene = (MSD_SubSceneInWorldWide)(-1);
        }

        void Update()
        {
            if (_hooked) return;
            if (Application.loadedLevelName != "MultiplayerSelect") return;

            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            if (msd.mCurWWSubScene != _lastSubScene)
            {
                ModEntry.Log("CustomMaps Update: mCurWWSubScene=" + msd.mCurWWSubScene);
                _lastSubScene = msd.mCurWWSubScene;
            }

            if (msd.mCurWWSubScene != MSD_SubSceneInWorldWide.RoomCreate) return;

            StartCoroutine(HookButtons());
            _hooked = true;
        }

        IEnumerator HookButtons()
        {
            yield return new WaitForSeconds(0.1f);

            // Resources.FindObjectsOfTypeAll includes inactive GameObjects (NGUI hides sub-panels)
            var allBehaviours = (MonoBehaviour[])Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
            ModEntry.Log("HookButtons: scanning " + allBehaviours.Length + " behaviours (incl. inactive)");
            int hooked = 0;
            foreach (var comp in allBehaviours)
            {
                if (comp.GetType().Name != "MapSelectButtonEvent") continue;

                System.Reflection.FieldInfo btnField = comp.GetType().GetField("buttonName",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (btnField == null) continue;

                string btnName = btnField.GetValue(comp).ToString();
                ModEntry.Log("  MapSelectButtonEvent: " + btnName + " on " + comp.gameObject.name);

                if (btnName == "WWMapNext" || btnName == "WWMapPre")
                {
                    // Silence the original handler
                    object nilVal = System.Enum.Parse(btnField.FieldType, "Nil");
                    btnField.SetValue(comp, nilVal);

                    // Add our handler
                    var nav = comp.gameObject.AddComponent<MapNavButton>();
                    nav.isNext = (btnName == "WWMapNext");
                    nav.hook   = this;
                    hooked++;
                    ModEntry.Log("  -> HOOKED as " + (nav.isNext ? "Next" : "Pre"));
                }
            }
            _hooked = hooked > 0;
            // Sync virtual index to match current game-selected map
            if (_hooked)
            {
                var msd = MultiplayerSelectDirector.mInstance;
                if (msd != null)
                {
                    int existingIdx = Array.IndexOf(STANDARD_MAPS, msd.mCurWWMapSelect);
                    if (existingIdx >= 0) _virtualIdx = existingIdx;
                }
            }
            ModEntry.Log("Button hook complete — hooked " + hooked + " button(s)");
        }

        // ── Photon callbacks ──────────────────────────────────────────────────

        /// <summary>
        /// Photon calls OnJoinedRoom on all MonoBehaviours via SendMessage.
        /// We use this to log the scene that's about to load, and apply any redirect
        /// if the custom scene turns out not to exist in this APK build.
        /// </summary>
        void OnJoinedRoom()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            string scene = msd.mCurWWMapSelect;
            ModEntry.Log("OnJoinedRoom: mCurWWMapSelect=" + scene);

            if (CUSTOM_NAMES.ContainsKey(scene))
            {
                ModEntry.Log("Custom map room joined — LoadLevel(" + scene + ") about to fire");
                // Start a watchdog: if scene hasn't changed in 5s, the level doesn't exist — redirect
                StartCoroutine(LoadLevelWatchdog(scene));
            }
        }

        IEnumerator LoadLevelWatchdog(string expectedScene)
        {
            yield return new WaitForSeconds(5f);
            // If we're still in MultiplayerSelect, the LoadLevel either failed or didn't run
            if (Application.loadedLevelName == "MultiplayerSelect")
            {
                ModEntry.Log("WATCHDOG: still on MultiplayerSelect after 5s — '" + expectedScene + "' may not exist in this APK build. Redirecting to FreeRun3_1");
                var msd = MultiplayerSelectDirector.mInstance;
                if (msd != null)
                {
                    msd.mCurWWMapSelect = "FreeRun3_1";
                    Application.LoadLevel("FreeRun3_1");
                }
            }
        }

        // ── Navigation handlers ───────────────────────────────────────────────

        /// <summary>Called by MapNavButton when the player taps the right-arrow on the map selector.</summary>
        public void OnNextMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            _virtualIdx = (_virtualIdx >= _allMaps.Length - 1) ? 0 : _virtualIdx + 1;
            ApplyMap(msd, _virtualIdx);
        }

        /// <summary>Called by MapNavButton when the player taps the left-arrow on the map selector.</summary>
        public void OnPreMap()
        {
            var msd = MultiplayerSelectDirector.mInstance;
            if (msd == null) return;

            _virtualIdx = (_virtualIdx <= 0) ? _allMaps.Length - 1 : _virtualIdx - 1;
            ApplyMap(msd, _virtualIdx);
        }

        // ── Internal helpers ──────────────────────────────────────────────────────

        void ApplyMap(MultiplayerSelectDirector msd, int idx)
        {
            string scene = _allMaps[idx];

            int stdIdx = Array.IndexOf(STANDARD_MAPS, scene);
            if (stdIdx >= 0)
            {
                // Standard map — set normally and let game handle it
                PlayerPrefs.SetString("CNRMod_CustomMapName", "");
                msd.mCurWWMapSelect = scene;
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[stdIdx];
                msd.mWWMapUITexture.MarkAsChanged();
                msd.WWResetModeCheckBox();
            }
            else
            {
                // Custom map — use a working base scene for LoadLevel but keep display name
                string loadScene = CUSTOM_SCENE_LOAD.ContainsKey(scene) ? CUSTOM_SCENE_LOAD[scene] : "FreeRun3_1";
                msd.mCurWWMapSelect = loadScene;

                // Show the last standard texture as placeholder preview
                msd.mWWMapUITexture.mainTexture = (Texture)(object)msd.mWWMapTexture[STANDARD_MAPS.Length - 1];
                msd.mWWMapUITexture.MarkAsChanged();

                msd.mModeCheckBoxSH.SetActive(false);
                msd.mModeCheckBoxTDM.SetActive(true);
                msd.SwitchToMode(GrowthGameModeTag.tTeamDeathMatch);

                // Store the custom display name in PlayerPrefs so OnGUI can show it
                // even after mCurWWMapSelect is overwritten with the real scene name
                PlayerPrefs.SetString("CNRMod_CustomMapName", CUSTOM_NAMES[scene]);
            }

            ModEntry.Log("Map changed to: " + scene + " (loads: " + msd.mCurWWMapSelect + ")");
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

            // Show custom map name overlay when a custom slot is active
            // (mCurWWMapSelect holds the real scene now, so we use the cached pref)
            string displayName = PlayerPrefs.GetString("CNRMod_CustomMapName", "");
            if (string.IsNullOrEmpty(displayName)) return;

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

    /// <summary>Attached to the map arrow button GameObjects to intercept clicks.</summary>
    public class MapNavButton : MonoBehaviour
    {
        public bool isNext;
        public CustomMapsHook hook;

        void OnClick()
        {
            if (hook == null) return;
            if (isNext) hook.OnNextMap();
            else        hook.OnPreMap();
        }
    }
}
