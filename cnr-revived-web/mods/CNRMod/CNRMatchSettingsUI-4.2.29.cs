using System;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    // Match Settings is intentionally built from the room-creation screen's own
    // NGUI donors. CNR runs a very old NGUI build where transform scale is the
    // widget size, so cloning native controls in-place is far more reliable than
    // constructing modern-style width/height widgets.
    public class CNRMatchSettingsUIHook : MonoBehaviour
    {
        private const string GearName = "CNRMatchSettings_Gear";
        private const string OverlayName = "CNRMatchSettings_Overlay";

        private GameObject _gear;
        private Texture2D _settingsIcon;
        private Texture2D _gearHoverTexture;
        private Texture2D _gearActiveTexture;
        private GUIStyle _gearIconStyle;
        private Camera _roomUiCamera;
        private GameObject _overlay;
        private GameObject _content;
        private GameObject _roomPanel;
        private MultiplayerSelectDirector _msd;
        private Texture2D _backdropTexture;
        private GameObject _appliedIndicator;
        private readonly System.Collections.Generic.List<GameObject> _hiddenRoomRoots = new System.Collections.Generic.List<GameObject>();
        private CNRMatchSettingsData _draft;
        private int _tab;
        private string _lastMode = "";
        private string _lastMap = "";
        private string _lastMapKey = "";
        private float _scanAt;
        private int _baseDepth = 200;

        void OnLevelWasLoaded(int level)
        {
            if (_backdropTexture != null) UnityEngine.Object.Destroy(_backdropTexture);
            if (_settingsIcon != null) UnityEngine.Object.Destroy(_settingsIcon);
            if (_gearHoverTexture != null) UnityEngine.Object.Destroy(_gearHoverTexture);
            if (_gearActiveTexture != null) UnityEngine.Object.Destroy(_gearActiveTexture);
            _backdropTexture = null;
            _settingsIcon = null;
            _gearHoverTexture = null;
            _gearActiveTexture = null;
            _gearIconStyle = null;
            _roomUiCamera = null;
            _gear = null;
            _overlay = null;
            _content = null;
            _appliedIndicator = null;
            _hiddenRoomRoots.Clear();
            _draft = null;
            _roomPanel = null;
            _msd = null;
            _lastMode = "";
            _lastMap = "";
            _lastMapKey = "";
            _scanAt = 0f;
        }

        void Update()
        {
            if (Application.loadedLevelName != "MultiplayerSelect") return;
            if (Time.realtimeSinceStartup < _scanAt) return;
            _scanAt = Time.realtimeSinceStartup + 0.10f;

            MultiplayerSelectDirector msd = MultiplayerSelectDirector.mInstance;
            if (msd == null || msd.mRoomCreatePanel == null) return;
            _msd = msd;
            _roomPanel = msd.mRoomCreatePanel;

            bool mapChanged = CNRMatchSettings.ObserveHostMap(msd);
            CNRMatchSettings.SyncHostModeFromRoomUi(msd);
            string mapKey = CNRMatchSettings.GetHostMapKey(msd);
            if (string.IsNullOrEmpty(_lastMapKey)) _lastMapKey = mapKey;
            if (mapChanged || _lastMapKey != mapKey)
            {
                _lastMapKey = mapKey;
                _draft = CNRMatchSettings.Host.Clone();
                if (_overlay != null) RebuildContent();
            }

            if (_gear == null && _roomPanel.activeInHierarchy) TryInstallGear();
            if (_gear != null) _gear.SetActive(_roomPanel.activeInHierarchy && _overlay == null);
            if (CNRMatchSettings.HostHasCustomSettings && _appliedIndicator == null) EnsureAppliedIndicator();
            if (_appliedIndicator != null)
                _appliedIndicator.SetActive(_roomPanel.activeInHierarchy && _overlay == null && CNRMatchSettings.HostHasCustomSettings);

            if (_overlay != null)
            {
                string mode = CNRMatchMetadata.GetSelectedGameMode(msd);
                string map = mapKey;
                if (mode != _lastMode || map != _lastMap)
                {
                    _lastMode = mode;
                    _lastMap = map;
                    CNRMatchSettings.SyncHostModeFromRoomUi(msd);
                    if (_draft != null) _draft.Mode = mode;
                    RebuildContent();
                }
            }
        }


        private void TryInstallGear()
        {
            if (_roomPanel == null || !_roomPanel.activeInHierarchy) return;
            try
            {
                Transform existing = FindChildByName(_roomPanel.transform, GearName);
                if (existing != null)
                {
                    _gear = existing.gameObject;
                    return;
                }

                GameObject donor = FindNamedObject("WWMaxNumAddButton");
                if (donor == null) donor = FindNamedObject("WWMapPreButton");
                if (donor == null)
                {
                    ModEntry.Log("MatchSettingsUI: no native room control available for settings icon");
                    return;
                }

                bool wasActive = donor.activeSelf;
                if (!wasActive) donor.SetActive(true);
                _gear = Instantiate(donor) as GameObject;
                if (!wasActive) donor.SetActive(false);
                if (_gear == null) return;

                _gear.name = GearName;
                _gear.transform.parent = donor.transform.parent;
                _gear.transform.localScale = donor.transform.localScale;
                _gear.transform.localRotation = donor.transform.localRotation;
                _gear.transform.localPosition = new Vector3(-0.455f, 0.455f, -1f);
                SetLayerRecursive(_gear.transform, donor.layer);

                StripNativeActionComponents(_gear.transform);
                ModeSelectUiUtil.DestroyAllInChildren<UILocalize>(_gear.transform);
                ModeSelectUiUtil.DestroyAllInChildren<TweenPosition>(_gear.transform);

                UISprite background = FindSpriteByName(_gear.transform, "Background");
                int iconDepth = background != null ? background.depth + 2 : _baseDepth + 2;
                Transform iconParent = background != null ? background.transform.parent : _gear.transform;
                Vector3 iconPos = background != null ? background.transform.localPosition : Vector3.zero;
                Quaternion iconRot = background != null ? background.transform.localRotation : Quaternion.identity;
                float iconH = background != null ? Mathf.Max(42f, Mathf.Abs(background.transform.localScale.y)) : 52f;

                UISprite[] sprites = _gear.GetComponentsInChildren<UISprite>(true);
                for (int i = 0; i < sprites.Length; i++) if (sprites[i] != null) sprites[i].enabled = false;
                UILabel[] labels = _gear.GetComponentsInChildren<UILabel>(true);
                for (int i = 0; i < labels.Length; i++) if (labels[i] != null) labels[i].enabled = false;

                _settingsIcon = LoadSharedSettingsIcon();
                if (_settingsIcon == null)
                {
                    ModEntry.Log("MatchSettingsUI: settings icon asset missing");
                    Destroy(_gear);
                    _gear = null;
                    return;
                }

                GameObject iconGo = new GameObject("CNRMatchSettings_SettingsIcon");
                iconGo.layer = donor.layer;
                iconGo.transform.parent = iconParent;
                iconGo.transform.localPosition = iconPos;
                iconGo.transform.localRotation = iconRot;
                const float aspect = 769f / 693f;
                iconGo.transform.localScale = new Vector3(iconH * aspect, iconH, 1f);
                UITexture icon = iconGo.AddComponent<UITexture>();
                icon.mainTexture = _settingsIcon;
                icon.depth = iconDepth;
                icon.pivot = UIWidget.Pivot.Center;
                icon.color = Color.white;
                icon.MarkAsChanged();

                BoxCollider bc = _gear.GetComponent<BoxCollider>();
                if (bc == null) bc = _gear.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
                bc.size = new Vector3(58f, 58f, 1f);

                CNRMatchSettingsClick click = _gear.GetComponent<CNRMatchSettingsClick>();
                if (click == null) click = _gear.AddComponent<CNRMatchSettingsClick>();
                click.Owner = this;
                click.Action = "open";

                _gear.SetActive(true);
                ModEntry.Log("MatchSettingsUI: native NGUI bare settings icon installed");
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI gear error: " + ex.Message);
            }
        }

        // The settings icon is now a native NGUI widget. Keeping IMGUI out of this
        // screen avoids the camera-projection failure that made 4.1.40+ disappear.
        void OnGUI() { }

        private Texture2D LoadSharedSettingsIcon()
        {
            try
            {
                FieldInfo f = typeof(EconomyHook).GetField("_SettingsIconB64",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                string b64 = f != null ? f.GetValue(null) as string : null;
                if (string.IsNullOrEmpty(b64)) return null;
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.name = "CNRMatchSettings_SharedSettingsIcon";
                if (!tex.LoadImage(Convert.FromBase64String(b64)))
                {
                    UnityEngine.Object.Destroy(tex);
                    return null;
                }
                tex.Apply();
                return tex;
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI settings icon load: " + ex.Message);
                return null;
            }
        }

        private static Texture2D MakeGearHighlight(Color color)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.name = "CNRMatchSettings_GearHighlight";
            tex.SetPixel(0, 0, color);
            tex.SetPixel(1, 0, color);
            tex.SetPixel(0, 1, color);
            tex.SetPixel(1, 1, color);
            tex.Apply();
            return tex;
        }

        private Camera FindRoomUiCamera()
        {
            try
            {
                UICamera[] uiCameras = (UICamera[])UnityEngine.Object.FindObjectsOfType(typeof(UICamera));
                Camera fallback = null;
                for (int i = 0; i < uiCameras.Length; i++)
                {
                    if (uiCameras[i] == null) continue;
                    Camera cam = uiCameras[i].GetComponent<Camera>();
                    if (cam == null) continue;
                    if (fallback == null) fallback = cam;
                    if (_roomPanel != null && (cam.cullingMask & (1 << _roomPanel.layer)) != 0)
                        return cam;
                }
                return fallback;
            }
            catch { return null; }
        }

        private bool TryGetGearGuiRect(out Rect rect)
        {
            rect = new Rect(0f, 0f, 0f, 0f);
            if (_gear == null) return false;
            if (_roomUiCamera == null) _roomUiCamera = FindRoomUiCamera();
            if (_roomUiCamera == null) return false;

            // Project the exact NGUI anchor into screen pixels, so the icon stays in the
            // same place the SET control occupied on every aspect ratio/resolution.
            Vector3 center = _roomUiCamera.WorldToScreenPoint(_gear.transform.position);
            Vector3 down = _roomUiCamera.WorldToScreenPoint(_gear.transform.TransformPoint(new Vector3(0f, -26f, 0f)));
            Vector3 up = _roomUiCamera.WorldToScreenPoint(_gear.transform.TransformPoint(new Vector3(0f, 26f, 0f)));
            float h = Mathf.Abs(up.y - down.y);
            if (h < 8f || center.z < 0f) return false;

            const float iconAspect = 769f / 693f; // shared settings.png dimensions
            float w = h * iconAspect;
            rect = new Rect(center.x - w * 0.5f, Screen.height - center.y - h * 0.5f, w, h);
            return true;
        }

        internal void HandleAction(string action)
        {
            if (string.IsNullOrEmpty(action)) return;
            if (action == "open") { Open(); return; }
            if (action == "close") { Close(); return; }
            if (action == "save")
            {
                if (_draft != null) CNRMatchSettings.CommitHostDraft(_draft, _msd);
                Close();
                EnsureAppliedIndicator();
                return;
            }
            if (action == "reset")
            {
                CNRMatchSettings.ResetHostToDefaults(_msd);
                _draft = CNRMatchSettings.Host.Clone();
                if (_overlay != null) RebuildContent();
                EnsureAppliedIndicator();
                if (_appliedIndicator != null) _appliedIndicator.SetActive(false);
                return;
            }

            if (action.StartsWith("tab:"))
            {
                int next;
                if (int.TryParse(action.Substring(4), out next))
                {
                    _tab = Mathf.Clamp(next, 0, 3);
                    RefreshTabVisuals();
                    RebuildContent();
                }
                return;
            }

            if (action.StartsWith("mode:"))
            {
                string reason;
                string mode = action.Substring(5);
                if (CNRMatchSettings.SelectMode(_msd, mode, out reason))
                {
                    _lastMode = CNRMatchMetadata.GetSelectedGameMode(_msd);
                    CNRMatchSettings.SyncHostModeFromRoomUi(_msd);
                    if (_draft != null) _draft.Mode = _lastMode;
                    RebuildContent();
                }
                else if (!string.IsNullOrEmpty(reason))
                {
                    ModEntry.Log("MatchSettingsUI mode: " + reason);
                }
                return;
            }

            if (action.StartsWith("adj:"))
            {
                string[] p = action.Split(':');
                int delta;
                if (p.Length == 3 && int.TryParse(p[2], out delta))
                {
                    Adjust(p[1], delta);
                    RebuildContent();
                }
            }
        }

        private void Open()
        {
            if (_overlay != null || _msd == null || _roomPanel == null || !_roomPanel.activeInHierarchy) return;
            try
            {
                RaiseDepthAboveRoomUi();

                _overlay = new GameObject(OverlayName);
                _overlay.layer = _roomPanel.layer;
                _overlay.transform.parent = _roomPanel.transform;
                _overlay.transform.localPosition = new Vector3(0f, 0f, -10f);
                _overlay.transform.localRotation = Quaternion.identity;
                _overlay.transform.localScale = Vector3.one;
                // Stay in the room panel's existing NGUI draw list. Nested UIPanels are
                // sorted independently in this old NGUI build and allowed room widgets to
                // draw through the overlay, then disturbed label ordering after close.

                CreateOverlayBackdrop();
                _draft = CNRMatchSettings.Host.Clone();

                MakeNativeLabel(_overlay.transform, "MATCH SETTINGS", new Vector3(0f, 0.455f, -2f), _baseDepth + 20, 1.18f);

                GameObject tab0 = MakeNativeButton(_overlay.transform, "GENERAL", new Vector3(-0.33f, 0.335f, -2f), 220f, 112f, "tab:0", _baseDepth + 20, 0.72f);
                GameObject tab1 = MakeNativeButton(_overlay.transform, "MODE", new Vector3(-0.11f, 0.335f, -2f), 220f, 112f, "tab:1", _baseDepth + 20, 0.72f);
                GameObject tab2 = MakeNativeButton(_overlay.transform, "RULES", new Vector3(0.11f, 0.335f, -2f), 220f, 112f, "tab:2", _baseDepth + 20, 0.72f);
                GameObject tab3 = MakeNativeButton(_overlay.transform, "DEV", new Vector3(0.33f, 0.335f, -2f), 220f, 112f, "tab:3", _baseDepth + 20, 0.72f);
                if (tab0 != null) tab0.name = "CNRMatchSettings_Tab_0";
                if (tab1 != null) tab1.name = "CNRMatchSettings_Tab_1";
                if (tab2 != null) tab2.name = "CNRMatchSettings_Tab_2";
                if (tab3 != null) tab3.name = "CNRMatchSettings_Tab_3";

                MakeNativeButton(_overlay.transform, "BACK", new Vector3(0.12f, -0.405f, -2f), 230f, 112f, "close", _baseDepth + 20, 0.78f);
                MakeNativeButton(_overlay.transform, "SAVE", new Vector3(0.345f, -0.405f, -2f), 230f, 112f, "save", _baseDepth + 20, 0.78f);

                _content = new GameObject("CNRMatchSettings_Content");
                _content.layer = _roomPanel.layer;
                _content.transform.parent = _overlay.transform;
                _content.transform.localPosition = Vector3.zero;
                _content.transform.localRotation = Quaternion.identity;
                _content.transform.localScale = Vector3.one;

                _lastMode = CNRMatchMetadata.GetSelectedGameMode(_msd);
                _lastMap = CNRMatchSettings.GetHostMapKey(_msd);
                _lastMapKey = _lastMap;
                RefreshTabVisuals();
                RebuildContent();
                HideRoomCreateUiForOverlay();
                ModEntry.Log("MatchSettingsUI: opened mode=" + _lastMode);
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI open error: " + ex.Message);
                Close();
            }
        }

        private void Close()
        {
            try
            {
                if (_overlay != null) _overlay.SetActive(false);
                RestoreRoomCreateUiAfterOverlay();
                if (_overlay != null) UnityEngine.Object.Destroy(_overlay);
                if (_backdropTexture != null) UnityEngine.Object.Destroy(_backdropTexture);
                _backdropTexture = null;
                _overlay = null;
                _content = null;
                _draft = null;
                CNRMatchSettings.SyncHostModeFromRoomUi(_msd);
                if (_gear != null && _roomPanel != null && _roomPanel.activeInHierarchy) _gear.SetActive(true);
                ModEntry.Log("MatchSettingsUI: closed mode=" + CNRMatchMetadata.GetSelectedGameMode(_msd));
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI close error: " + ex.Message);
            }
        }

        private void HideRoomCreateUiForOverlay()
        {
            _hiddenRoomRoots.Clear();
            if (_roomPanel == null || _overlay == null) return;

            Transform panel = _roomPanel.transform;
            for (int i = 0; i < panel.childCount; i++)
            {
                Transform child = panel.GetChild(i);
                if (child == null || child.gameObject == _overlay) continue;
                if (!child.gameObject.activeSelf) continue;

                _hiddenRoomRoots.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
            ModEntry.Log("MatchSettingsUI: hid " + _hiddenRoomRoots.Count + " room-create roots behind overlay");
        }

        private void RestoreRoomCreateUiAfterOverlay()
        {
            for (int i = 0; i < _hiddenRoomRoots.Count; i++)
            {
                GameObject go = _hiddenRoomRoots[i];
                if (go != null) go.SetActive(true);
            }
            _hiddenRoomRoots.Clear();
        }

        private void CreateOverlayBackdrop()
        {
            if (_overlay == null || _roomPanel == null) return;
            try
            {
                // Draw an opaque surface for the settings screen. Original room-create
                // roots are disabled after the overlay has finished cloning its native
                // donor visuals, so they cannot bleed through regardless of NGUI depth.
                GameObject bg = new GameObject("CNRMatchSettings_Backdrop");
                bg.layer = _roomPanel.layer;
                bg.transform.parent = _overlay.transform;
                bg.transform.localPosition = new Vector3(0f, 0f, -1.5f);
                bg.transform.localRotation = Quaternion.identity;
                bg.transform.localScale = new Vector3(1600f, 900f, 1f);
                _backdropTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                _backdropTexture.name = "CNRMatchSettings_BackdropTexture";
                _backdropTexture.SetPixel(0, 0, new Color(0.16f, 0.16f, 0.16f, 1f));
                _backdropTexture.Apply();
                UITexture widget = bg.AddComponent<UITexture>();
                widget.mainTexture = _backdropTexture;
                widget.depth = _baseDepth + 5;
                widget.pivot = UIWidget.Pivot.Center;

                // Do not disable or rewrite any room-create controls. This collider simply
                // consumes clicks while the settings overlay is open.
                GameObject blocker = new GameObject("CNRMatchSettings_InputBlocker");
                blocker.layer = _roomPanel.layer;
                blocker.transform.parent = _overlay.transform;
                blocker.transform.localPosition = new Vector3(0f, 0f, -1f);
                blocker.transform.localRotation = Quaternion.identity;
                blocker.transform.localScale = Vector3.one;
                BoxCollider bc = blocker.AddComponent<BoxCollider>();
                bc.size = new Vector3(1600f, 900f, 1f);
                blocker.AddComponent<CNRMatchSettingsBlocker>();
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI backdrop error: " + ex.Message);
            }
        }

        private void EnsureAppliedIndicator()
        {
            if (_appliedIndicator != null || _msd == null || _roomPanel == null || _msd.mWWMapUITexture == null) return;
            try
            {
                Transform mapTransform = _msd.mWWMapUITexture.transform;
                Transform parent = mapTransform.parent != null ? mapTransform.parent : _roomPanel.transform;

                _appliedIndicator = new GameObject("CNRMatchSettings_AppliedIndicator");
                _appliedIndicator.layer = _roomPanel.layer;
                _appliedIndicator.transform.parent = parent;
                _appliedIndicator.transform.localPosition = mapTransform.localPosition + new Vector3(0f, -0.205f, -2f);
                _appliedIndicator.transform.localRotation = Quaternion.identity;
                _appliedIndicator.transform.localScale = Vector3.one;

                UILabel label = MakeNativeLabel(_appliedIndicator.transform, "CUSTOM SETTINGS APPLIED", new Vector3(-0.065f, 0f, -1f), _baseDepth + 15, 0.62f);
                if (label != null) label.color = new Color(1f, 0.52f, 0.08f, 1f);
                MakeNativeButton(_appliedIndicator.transform, "RESET", new Vector3(0.19f, 0f, -1f), 155f, 74f, "reset", _baseDepth + 15, 0.58f);
                _appliedIndicator.SetActive(false);
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI indicator error: " + ex.Message);
            }
        }

        private static bool IsRoomBackground(string name)
        {
            return name == "Sprite (4)" || name == "Sprite (5)" || name == "Sprite (BlackStrip)";
        }

        private void RefreshTabVisuals()
        {
            if (_overlay == null) return;
            for (int i = 0; i < 4; i++)
            {
                Transform t = FindChildByName(_overlay.transform, "CNRMatchSettings_Tab_" + i);
                if (t == null) continue;
                UISprite bg = FindSpriteByName(t, "Background");
                if (bg != null) bg.alpha = i == _tab ? 1f : 0.60f;
            }
        }

        private void RebuildContent()
        {
            if (_content == null || _msd == null) return;
            for (int i = _content.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_content.transform.GetChild(i).gameObject);

            CNRMatchSettings.SyncHostModeFromRoomUi(_msd);
            if (_draft == null) _draft = CNRMatchSettings.Host.Clone();
            CNRMatchSettingsData s = _draft;
            string mode = CNRMatchMetadata.GetSelectedGameMode(_msd);
            if (!string.IsNullOrEmpty(mode)) s.Mode = mode;

            if (_tab == 0)
            {
                MakeNativeLabel(_content.transform, "GENERAL", new Vector3(-0.29f, 0.225f, -2f), _baseDepth + 30, 0.92f);
                float y = 0.105f;
                AddValueRow("Round Time", FormatMinutes(s.RoundSeconds), "round", 60, ref y);
                MakeNativeLabel(_content.transform, "Required room resources are verified before joining.", new Vector3(0f, -0.155f, -2f), _baseDepth + 30, 0.68f);
                MakeNativeLabel(_content.transform, "Map: " + (_msd.mCurWWMapSelect ?? "Unknown"), new Vector3(0f, -0.235f, -2f), _baseDepth + 30, 0.68f);
            }
            else if (_tab == 1)
            {
                MakeNativeLabel(_content.transform, "GAME MODE", new Vector3(-0.27f, 0.225f, -2f), _baseDepth + 30, 0.92f);
                string[] modes = new string[] { "tdm", "stronghold", "kc", "ctf", "zombies" };
                float y = 0.145f;
                for (int i = 0; i < modes.Length; i++)
                {
                    if (!CNRMatchSettings.IsModeAvailable(_msd, modes[i])) continue;
                    MakeModeRow(modes[i], new Vector3(-0.13f, y, -2f));
                    y -= 0.105f;
                }
            }
            else if (_tab == 2)
            {
                MakeNativeLabel(_content.transform, "RULES - " + CNRMatchMetadata.GetDisplayMode(mode), new Vector3(-0.22f, 0.225f, -2f), _baseDepth + 30, 0.88f);
                float y = 0.105f;
                if (mode == "stronghold")
                {
                    AddValueRow("Starting Resources", s.StrongholdResources.ToString(), "stronghold", 100, ref y);
                }
                else if (mode == "kc")
                {
                    AddValueRow("Kill Limit", s.KcKillLimit == 0 ? "AUTO" : s.KcKillLimit.ToString(), "kc", 10, ref y);
                    AddValueRow("Bots", s.KcBotsEnabled != 0 ? "ON" : "OFF", "kcbots", 1, ref y);
                    if (s.KcBotsEnabled != 0)
                    {
                        AddBotTeamRow("Bot Count", s.KcBotCount, s.KcBotsAuto != 0, "kcbotcount", "kcbotauto", ref y);
                        MakeNativeLabel(_content.transform, "AUTO makes the count the total match target; humans replace bots.", new Vector3(0f, y - 0.01f, -2f), _baseDepth + 30, 0.58f);
                    }
                }
                else if (mode == "ctf")
                {
                    AddValueRow("Score Limit", s.CtfScoreLimit.ToString(), "ctfscore", 1, ref y);
                    AddValueRow("Flag Return", s.CtfFlagReturnSeconds + " sec", "ctfreturn", 5, ref y);
                }
                else if (mode == "zombies")
                {
                    AddValueRow("First Round Delay", s.ZombieStartDelaySeconds + " sec", "zstart", 1, ref y);
                    AddValueRow("Inter-Round Delay", s.ZombieInterRoundSeconds + " sec", "zinter", 1, ref y);
                    AddValueRow("Max Zombies", s.ZombieMaxPerRound.ToString(), "zmax", 5, ref y);
                }
                else
                {
                    AddValueRow("Bots", s.TdmBotsEnabled != 0 ? "ON" : "OFF", "tdmbots", 1, ref y);
                    if (s.TdmBotsEnabled != 0)
                    {
                        AddBotTeamRow("Cop Bots", s.TdmCopBots, s.TdmCopBotsAuto != 0, "tdmcopbots", "tdmcopauto", ref y);
                        AddBotTeamRow("Robber Bots", s.TdmRobberBots, s.TdmRobberBotsAuto != 0, "tdmrobberbots", "tdmrobberauto", ref y);
                        MakeNativeLabel(_content.transform, "AUTO makes the count the team target; humans replace bots.", new Vector3(0f, y - 0.01f, -2f), _baseDepth + 30, 0.58f);
                    }
                }
            }
            else
            {
                MakeNativeLabel(_content.transform, "DEV", new Vector3(-0.29f, 0.225f, -2f), _baseDepth + 30, 0.92f);
                float y = 0.105f;
                AddValueRow("Bussin' Velocity", (s.BussinVelocityTenths / 10f).ToString("0.0") + "x", "bussinvelocity", 1, ref y);
                MakeNativeLabel(_content.transform, "0.0x disables Bussin' recoil velocity. Default is 1.0x.", new Vector3(0f, -0.075f, -2f), _baseDepth + 30, 0.62f);
            }
        }

        private void AddValueRow(string name, string value, string key, int step, ref float y)
        {
            MakeNativeLabel(_content.transform, name, new Vector3(-0.245f, y, -2f), _baseDepth + 31, 0.74f);
            MakeSmallButton(_content.transform, "-", new Vector3(0.055f, y, -2f), "adj:" + key + ":" + (-step), _baseDepth + 32);
            MakeNativeLabel(_content.transform, value, new Vector3(0.205f, y, -2f), _baseDepth + 33, 0.74f);
            MakeSmallButton(_content.transform, "+", new Vector3(0.355f, y, -2f), "adj:" + key + ":" + step, _baseDepth + 32);
            y -= 0.115f;
        }

        private void AddBotTeamRow(string name, int count, bool autoEnabled, string countKey, string autoKey, ref float y)
        {
            MakeNativeLabel(_content.transform, name, new Vector3(-0.285f, y, -2f), _baseDepth + 31, 0.70f);
            MakeSmallButton(_content.transform, "-", new Vector3(-0.005f, y, -2f), "adj:" + countKey + ":-1", _baseDepth + 32);
            MakeNativeLabel(_content.transform, count.ToString(), new Vector3(0.105f, y, -2f), _baseDepth + 33, 0.74f);
            MakeSmallButton(_content.transform, "+", new Vector3(0.215f, y, -2f), "adj:" + countKey + ":1", _baseDepth + 32);
            MakeAutoCheckbox(_content.transform, autoEnabled, new Vector3(0.405f, y, -2f), "adj:" + autoKey + ":1", _baseDepth + 34);
            y -= 0.115f;
        }

        private void Adjust(string key, int delta)
        {
            if (_draft == null) _draft = CNRMatchSettings.Host.Clone();
            CNRMatchSettingsData s = _draft;
            if (key == "round") s.RoundSeconds += delta;
            else if (key == "stronghold") s.StrongholdResources += delta;
            else if (key == "kc")
            {
                if (s.KcKillLimit == 0 && delta > 0) s.KcKillLimit = 20;
                else
                {
                    s.KcKillLimit += delta;
                    if (s.KcKillLimit < 5) s.KcKillLimit = 0;
                }
            }
            else if (key == "ctfscore") s.CtfScoreLimit += delta;
            else if (key == "ctfreturn") s.CtfFlagReturnSeconds += delta;
            else if (key == "zstart") s.ZombieStartDelaySeconds += delta;
            else if (key == "zinter") s.ZombieInterRoundSeconds += delta;
            else if (key == "zmax") s.ZombieMaxPerRound += delta;
            else if (key == "tdmbots") s.TdmBotsEnabled = delta > 0 ? 1 : 0;
            else if (key == "kcbots") s.KcBotsEnabled = delta > 0 ? 1 : 0;
            else if (key == "kcbotcount") s.KcBotCount += delta;
            else if (key == "kcbotauto") s.KcBotsAuto = s.KcBotsAuto == 0 ? 1 : 0;
            else if (key == "tdmcopbots") s.TdmCopBots += delta;
            else if (key == "tdmrobberbots") s.TdmRobberBots += delta;
            else if (key == "tdmcopauto") s.TdmCopBotsAuto = s.TdmCopBotsAuto == 0 ? 1 : 0;
            else if (key == "tdmrobberauto") s.TdmRobberBotsAuto = s.TdmRobberBotsAuto == 0 ? 1 : 0;
            else if (key == "bussinvelocity") s.BussinVelocityTenths += delta;
            CNRMatchSettings.Sanitize(s);
        }

        private GameObject MakeNativeButton(Transform parent, string text, Vector3 pos, float bgWidth, float bgHeight, string action, int depth, float labelScale)
        {
            GameObject donor = FindNamedObject("WWRoomStartButton");
            if (donor == null) donor = FindNamedObject("WWRoomCreateBackButton");
            if (donor == null) return null;

            GameObject go = CreateFreshControlRoot(donor, parent, "CNRMatchSettings_Button_" + text, pos);
            UISprite bg = CloneSpriteVisual(donor.transform, "Background", go.transform, depth, bgWidth, bgHeight);
            UILabel label = CloneLabelVisual(donor.transform, go.transform, text, depth + 2, labelScale);
            if (label != null) label.transform.localPosition = new Vector3(0f, 0f, -1f);

            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(bgWidth, bgHeight, 1f);
            CNRMatchSettingsClick click = go.AddComponent<CNRMatchSettingsClick>();
            click.Owner = this;
            click.Action = action;
            go.SetActive(true);
            return go;
        }

        private GameObject MakeSmallButton(Transform parent, string text, Vector3 pos, string action, int depth)
        {
            GameObject donor = text == "-" ? FindNamedObject("WWMaxNumSubButton") : FindNamedObject("WWMaxNumAddButton");
            if (donor == null) donor = FindNamedObject("WWMaxNumAddButton");
            if (donor == null) return null;

            GameObject go = CreateFreshControlRoot(donor, parent, "CNRMatchSettings_Small_" + text, pos);
            CloneSpriteVisual(donor.transform, "Background", go.transform, depth, 42f, 42f);
            CloneLabelVisual(donor.transform, go.transform, text, depth + 2, 1f);

            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(48f, 48f, 1f);
            CNRMatchSettingsClick click = go.AddComponent<CNRMatchSettingsClick>();
            click.Owner = this;
            click.Action = action;
            go.SetActive(true);
            return go;
        }

        private GameObject MakeAutoCheckbox(Transform parent, bool selected, Vector3 pos, string action, int depth)
        {
            GameObject donor = FindModeRowDonor();
            if (donor == null) return MakeNativeButton(parent, selected ? "[X] AUTO" : "[ ] AUTO", pos, 118f, 46f, action, depth, 0.68f);

            GameObject go = CreateFreshControlRoot(donor, parent, "CNRMatchSettings_Auto", pos);
            UISprite bg = CloneSpriteVisual(donor.transform, "Background", go.transform, depth, -1f, -1f);
            UISprite check = CloneSpriteVisual(donor.transform, "Checkmark", go.transform, depth + 1, -1f, -1f);
            UILabel label = CloneLabelVisual(donor.transform, go.transform, "AUTO", depth + 2, 0.78f);
            if (check != null) check.alpha = selected ? 1f : 0f;
            if (label != null) label.text = "AUTO";

            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.center = Vector3.zero;
            bc.size = new Vector3(150f, 56f, 1f);
            CNRMatchSettingsClick click = go.AddComponent<CNRMatchSettingsClick>();
            click.Owner = this;
            click.Action = action;
            go.SetActive(true);
            return go;
        }

        private UILabel MakeNativeLabel(Transform parent, string text, Vector3 pos, int depth, float scaleMultiplier)
        {
            UILabel donor = _msd != null ? _msd.mMaxNumLabel : null;
            if (donor == null)
            {
                Transform direct = FindChildByName(_roomPanel != null ? _roomPanel.transform : null, "MaxNumLabel");
                if (direct != null) donor = direct.GetComponent<UILabel>();
            }
            if (donor == null) return null;

            GameObject go = (GameObject)UnityEngine.Object.Instantiate(donor.gameObject);
            go.name = "CNRMatchSettings_Label";
            go.layer = _roomPanel.layer;
            go.transform.parent = parent;
            go.transform.localPosition = pos;
            go.transform.localRotation = donor.transform.localRotation;
            go.transform.localScale = donor.transform.localScale * scaleMultiplier;
            SetLayerRecursive(go.transform, _roomPanel.layer);
            KeepOnlyVisualComponents(go.transform);

            UILabel label = go.GetComponent<UILabel>();
            if (label == null) label = go.GetComponentInChildren<UILabel>();
            if (label != null)
            {
                label.text = text;
                label.lineWidth = 0;
                label.maxLineCount = 1;
                label.depth = depth;
                label.pivot = UIWidget.Pivot.Center;
            }
            go.SetActive(true);
            return label;
        }

        private void MakeModeRow(string mode, Vector3 pos)
        {
            GameObject donor = FindModeRowDonor();
            if (donor == null) return;

            GameObject go = CreateFreshControlRoot(donor, _content.transform, "CNRMatchSettings_Mode_" + mode, pos);
            UISprite bg = CloneSpriteVisual(donor.transform, "Background", go.transform, _baseDepth + 35, -1f, -1f);
            UISprite check = CloneSpriteVisual(donor.transform, "Checkmark", go.transform, _baseDepth + 36, -1f, -1f);
            UILabel lbl = CloneLabelVisual(donor.transform, go.transform, CNRMatchMetadata.GetDisplayMode(mode), _baseDepth + 37, 1f);

            // Give the cloned virtual radio a predictable full-row hit target. The
            // original donors use different collider offsets/sizes depending on mode.
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.center = Vector3.zero;
            bc.size = new Vector3(420f, 72f, 1f);

            CNRMatchSettingsModeClick click = go.AddComponent<CNRMatchSettingsModeClick>();
            click.Owner = this;
            click.Mode = mode;
            click.Check = check;
            click.Label = lbl;
            go.SetActive(true);
        }

        private GameObject CreateFreshControlRoot(GameObject donor, Transform parent, string name, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.layer = _roomPanel.layer;
            go.transform.parent = parent;
            go.transform.localPosition = pos;
            go.transform.localRotation = donor != null ? donor.transform.localRotation : Quaternion.identity;
            go.transform.localScale = donor != null ? donor.transform.localScale : Vector3.one;
            return go;
        }

        private UISprite CloneSpriteVisual(Transform donorRoot, string childName, Transform parent, int depth, float width, float height)
        {
            Transform source = FindChildByName(donorRoot, childName);
            if (source == null) return null;
            UISprite donorSprite = source.GetComponent<UISprite>();
            if (donorSprite == null) return null;

            GameObject go = (GameObject)UnityEngine.Object.Instantiate(source.gameObject);
            go.name = childName;
            go.layer = _roomPanel.layer;
            go.transform.parent = parent;
            go.transform.localPosition = source.localPosition;
            go.transform.localRotation = source.localRotation;
            go.transform.localScale = source.localScale;
            SetLayerRecursive(go.transform, _roomPanel.layer);
            KeepOnlyVisualComponents(go.transform);

            UISprite sprite = go.GetComponent<UISprite>();
            if (sprite != null)
            {
                if (width > 0f && height > 0f) sprite.transform.localScale = new Vector3(width, height, 1f);
                sprite.depth = depth;
            }
            go.SetActive(true);
            return sprite;
        }

        private UILabel CloneLabelVisual(Transform donorRoot, Transform parent, string text, int depth, float scaleMultiplier)
        {
            UILabel donorLabel = ModeSelectUiUtil.FindInChildren<UILabel>(donorRoot);
            if (donorLabel == null) return null;

            GameObject go = (GameObject)UnityEngine.Object.Instantiate(donorLabel.gameObject);
            go.name = "Label";
            go.layer = _roomPanel.layer;
            go.transform.parent = parent;
            go.transform.localPosition = donorLabel.transform.localPosition;
            go.transform.localRotation = donorLabel.transform.localRotation;
            go.transform.localScale = donorLabel.transform.localScale * scaleMultiplier;
            SetLayerRecursive(go.transform, _roomPanel.layer);
            KeepOnlyVisualComponents(go.transform);

            UILabel label = go.GetComponent<UILabel>();
            if (label != null)
            {
                label.text = text;
                label.depth = depth;
            }
            go.SetActive(true);
            return label;
        }

        private static void KeepOnlyVisualComponents(Transform root)
        {
            if (root == null) return;
            MonoBehaviour[] mbs = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < mbs.Length; i++)
            {
                MonoBehaviour mb = mbs[i];
                if (mb == null) continue;
                if (mb is UIWidget) continue;
                UnityEngine.Object.DestroyImmediate(mb);
            }
            Collider[] cols = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
                if (cols[i] != null) UnityEngine.Object.DestroyImmediate(cols[i]);
        }

        private GameObject FindModeRowDonor()
        {
            if (_roomPanel == null) return null;
            Transform[] all = _roomPanel.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].name == "CnrBtn_Mode") return all[i].gameObject;
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].GetComponent<ModeSelectCheckBox>() != null) return all[i].gameObject;
            return null;
        }

        private GameObject FindNamedObject(string name)
        {
            Transform t = FindChildByName(_roomPanel != null ? _roomPanel.transform : null, name);
            return t != null ? t.gameObject : null;
        }

        private static UISprite FindSpriteByName(Transform root, string name)
        {
            if (root == null) return null;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t == null || t.name != name) continue;
                UISprite s = t.GetComponent<UISprite>();
                if (s != null) return s;
            }
            return null;
        }

        private static void StripNativeActionComponents(Transform root)
        {
            if (root == null) return;
            MapSelectButtonEvent[] mapEvents = root.GetComponentsInChildren<MapSelectButtonEvent>(true);
            for (int i = 0; i < mapEvents.Length; i++) UnityEngine.Object.DestroyImmediate(mapEvents[i]);
            CnrModeButton[] customModes = root.GetComponentsInChildren<CnrModeButton>(true);
            for (int i = 0; i < customModes.Length; i++) UnityEngine.Object.DestroyImmediate(customModes[i]);
            ModeSelectCheckBox[] vanillaModes = root.GetComponentsInChildren<ModeSelectCheckBox>(true);
            for (int i = 0; i < vanillaModes.Length; i++) UnityEngine.Object.DestroyImmediate(vanillaModes[i]);
            UICheckbox[] boxes = root.GetComponentsInChildren<UICheckbox>(true);
            for (int i = 0; i < boxes.Length; i++) UnityEngine.Object.DestroyImmediate(boxes[i]);
        }

        private void RaiseDepthAboveRoomUi()
        {
            if (_roomPanel == null) return;
            int maxDepth = 0;
            UIWidget[] widgets = _roomPanel.GetComponentsInChildren<UIWidget>(true);
            for (int i = 0; i < widgets.Length; i++)
                if (widgets[i] != null && widgets[i].depth > maxDepth) maxDepth = widgets[i].depth;
            _baseDepth = Mathf.Max(200, maxDepth + 20);
        }


        private static string FormatMinutes(int seconds)
        {
            int mins = Mathf.Max(1, seconds / 60);
            return mins + " min";
        }

        private static Transform FindChildByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildByName(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            if (root == null) return;
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++) SetLayerRecursive(root.GetChild(i), layer);
        }
    }

    public class CNRMatchSettingsClick : MonoBehaviour
    {
        public CNRMatchSettingsUIHook Owner;
        public string Action;
        void OnClick() { if (Owner != null) Owner.HandleAction(Action); }
    }

    public class CNRMatchSettingsModeClick : MonoBehaviour
    {
        public CNRMatchSettingsUIHook Owner;
        public string Mode;
        public UISprite Check;
        public UILabel Label;

        void Update()
        {
            MultiplayerSelectDirector msd = MultiplayerSelectDirector.mInstance;
            bool selected = msd != null && CNRMatchMetadata.GetSelectedGameMode(msd) == Mode;
            if (Check != null) Check.alpha = selected ? 1f : 0f;
            if (Label != null && Label.text != CNRMatchMetadata.GetDisplayMode(Mode)) Label.text = CNRMatchMetadata.GetDisplayMode(Mode);
        }

        void OnClick()
        {
            if (Owner != null) Owner.HandleAction("mode:" + Mode);
        }
    }

    public class CNRMatchSettingsBlocker : MonoBehaviour
    {
        void OnClick() { }
    }
}