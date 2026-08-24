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
        private GameObject _overlay;
        private GameObject _content;
        private GameObject _roomPanel;
        private MultiplayerSelectDirector _msd;
        private Texture2D _settingsIcon;
        private Texture2D _backdropTexture;
        private GameObject _appliedIndicator;
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
            _backdropTexture = null;
            _gear = null;
            _overlay = null;
            _content = null;
            _appliedIndicator = null;
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
            EnsureAppliedIndicator();
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
            if (_roomPanel == null) return;
            try
            {
                Transform existing = FindChildByName(_roomPanel.transform, GearName);
                if (existing != null)
                {
                    _gear = existing.gameObject;
                    return;
                }

                RaiseDepthAboveRoomUi();
                _settingsIcon = FindSettingsIcon();

                GameObject donor = FindNamedObject("WWMaxNumAddButton");
                if (donor == null) donor = FindNamedObject("WWMapPreButton");
                if (donor == null)
                {
                    ModEntry.Log("MatchSettingsUI: no native donor available for gear");
                    return;
                }

                _gear = CreateFreshControlRoot(donor, _roomPanel.transform, GearName, new Vector3(-0.455f, 0.455f, -1f));
                CloneSpriteVisual(donor.transform, "Background", _gear.transform, _baseDepth + 10, 52f, 52f);

                if (_settingsIcon != null)
                {
                    GameObject iconGo = new GameObject("GearIcon");
                    iconGo.layer = _gear.layer;
                    iconGo.transform.parent = _gear.transform;
                    iconGo.transform.localPosition = new Vector3(0f, 0f, -2f);
                    iconGo.transform.localRotation = Quaternion.identity;
                    iconGo.transform.localScale = new Vector3(34f, 34f, 1f);
                    UITexture icon = iconGo.AddComponent<UITexture>();
                    icon.mainTexture = _settingsIcon;
                    icon.depth = _baseDepth + 12;
                }
                else
                {
                    CloneLabelVisual(donor.transform, _gear.transform, "SET", _baseDepth + 12, 0.70f);
                }

                BoxCollider bc = _gear.AddComponent<BoxCollider>();
                bc.size = new Vector3(58f, 58f, 1f);

                CNRMatchSettingsClick click = _gear.AddComponent<CNRMatchSettingsClick>();
                click.Owner = this;
                click.Action = "open";
                _gear.SetActive(true);
                ModEntry.Log("MatchSettingsUI: native gear installed");
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI gear error: " + ex.Message);
            }
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
                    _tab = Mathf.Clamp(next, 0, 2);
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
                UIPanel overlayPanel = _overlay.AddComponent<UIPanel>();
                overlayPanel.showInPanelTool = false;

                CreateOverlayBackdrop();
                _draft = CNRMatchSettings.Host.Clone();

                MakeNativeLabel(_overlay.transform, "MATCH SETTINGS", new Vector3(0f, 0.455f, -2f), _baseDepth + 20, 1.18f);

                GameObject tab0 = MakeNativeButton(_overlay.transform, "GENERAL", new Vector3(-0.225f, 0.335f, -2f), 290f, 112f, "tab:0", _baseDepth + 20, 0.78f);
                GameObject tab1 = MakeNativeButton(_overlay.transform, "MODE", new Vector3(0f, 0.335f, -2f), 290f, 112f, "tab:1", _baseDepth + 20, 0.78f);
                GameObject tab2 = MakeNativeButton(_overlay.transform, "RULES", new Vector3(0.225f, 0.335f, -2f), 290f, 112f, "tab:2", _baseDepth + 20, 0.78f);
                if (tab0 != null) tab0.name = "CNRMatchSettings_Tab_0";
                if (tab1 != null) tab1.name = "CNRMatchSettings_Tab_1";
                if (tab2 != null) tab2.name = "CNRMatchSettings_Tab_2";

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

        private void CreateOverlayBackdrop()
        {
            if (_overlay == null || _roomPanel == null) return;
            try
            {
                // Draw our own opaque NGUI surface above the create-room widgets. The
                // underlying hierarchy remains active and unchanged, but it is no longer
                // visually competing with the settings controls.
                GameObject bg = new GameObject("CNRMatchSettings_Backdrop");
                bg.layer = _roomPanel.layer;
                bg.transform.parent = _overlay.transform;
                bg.transform.localPosition = new Vector3(0f, 0f, -1.5f);
                bg.transform.localRotation = Quaternion.identity;
                bg.transform.localScale = new Vector3(1600f, 900f, 1f);
                _backdropTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                _backdropTexture.name = "CNRMatchSettings_BackdropTexture";
                _backdropTexture.SetPixel(0, 0, new Color(0.16f, 0.16f, 0.16f, 0.98f));
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
            for (int i = 0; i < 3; i++)
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
            else
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
                    MakeNativeLabel(_content.transform, "Team Deathmatch uses the General rules.", new Vector3(0f, 0.055f, -2f), _baseDepth + 30, 0.78f);
                }
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

        private Texture2D FindSettingsIcon()
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int a = 0; a < assemblies.Length; a++)
                {
                    Type[] types;
                    try { types = assemblies[a].GetTypes(); }
                    catch (ReflectionTypeLoadException rtle) { types = rtle.Types; }
                    if (types == null) continue;

                    for (int t = 0; t < types.Length; t++)
                    {
                        Type type = types[t];
                        if (type == null) continue;
                        FieldInfo f = type.GetField("_SettingsIconB64", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (f == null || f.FieldType != typeof(string)) continue;
                        string b64 = f.GetValue(null) as string;
                        if (string.IsNullOrEmpty(b64)) continue;

                        Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                        if (tex.LoadImage(Convert.FromBase64String(b64)))
                        {
                            tex.Apply();
                            ModEntry.Log("MatchSettingsUI: using shared settings gear asset from " + type.FullName);
                            return tex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettingsUI settings icon lookup: " + ex.Message);
            }
            return null;
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