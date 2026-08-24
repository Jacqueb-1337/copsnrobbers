using System;
using UnityEngine;

namespace CNRMods
{
    [Serializable]
    internal class CNRMatchSettingsData
    {
        public int Schema = 1;
        public string Mode = "tdm";
        public int RoundSeconds = 600;
        public int KcKillLimit = 0;              // 0 = vanilla automatic-by-player-count rule
        public int StrongholdResources = 900;
        public int CtfScoreLimit = 30;
        public int CtfFlagReturnSeconds = 30;
        public int ZombieStartDelaySeconds = 8;
        public int ZombieInterRoundSeconds = 10;
        public int ZombieMaxPerRound = 75;

        public CNRMatchSettingsData Clone()
        {
            CNRMatchSettingsData d = new CNRMatchSettingsData();
            d.Schema = Schema;
            d.Mode = Mode;
            d.RoundSeconds = RoundSeconds;
            d.KcKillLimit = KcKillLimit;
            d.StrongholdResources = StrongholdResources;
            d.CtfScoreLimit = CtfScoreLimit;
            d.CtfFlagReturnSeconds = CtfFlagReturnSeconds;
            d.ZombieStartDelaySeconds = ZombieStartDelaySeconds;
            d.ZombieInterRoundSeconds = ZombieInterRoundSeconds;
            d.ZombieMaxPerRound = ZombieMaxPerRound;
            return d;
        }
    }

    // Single source of truth for host-edited, pre-join queued, and active match rules.
    // The UI only edits Host. A room join only promotes Pending after preflight succeeds.
    internal static class CNRMatchSettings
    {
        internal const string PropSettings = "cnrs";
        internal const int CurrentSchema = 1;

        private static CNRMatchSettingsData _host = NewDefaults("tdm");
        private static CNRMatchSettingsData _pending;
        private static CNRMatchSettingsData _active;
        private static string _pendingRoom = "";

        internal static CNRMatchSettingsData Host { get { return _host; } }
        internal static CNRMatchSettingsData Pending { get { return _pending; } }
        internal static CNRMatchSettingsData Active
        {
            get { return _active != null ? _active : _host; }
        }

        internal static CNRMatchSettingsData NewDefaults(string mode)
        {
            CNRMatchSettingsData d = new CNRMatchSettingsData();
            d.Mode = NormalizeMode(mode);
            return d;
        }

        internal static void SyncHostModeFromRoomUi(MultiplayerSelectDirector msd)
        {
            string selected = CNRMatchMetadata.GetSelectedGameMode(msd);
            if (!string.IsNullOrEmpty(selected)) _host.Mode = selected;
        }

        internal static bool SelectMode(MultiplayerSelectDirector msd, string mode, out string reason)
        {
            reason = "";
            if (msd == null) { reason = "Room creation UI is not ready."; return false; }
            mode = NormalizeMode(mode);
            GrowthGameModeTag vanilla;
            if (!TryGetVanillaMode(mode, out vanilla)) { reason = "Unknown game mode."; return false; }
            if (!msd.isCurMapSupportThisMode(vanilla))
            {
                reason = "The selected map does not support " + CNRMatchMetadata.GetDisplayMode(mode) + ".";
                return false;
            }

            CtfMode.PendingCtf = mode == "ctf";
            CtfMode.IsCtfRoom = false;
            ZombieMode.PendingZombie = mode == "zombies";
            ZombieMode.IsZombieRoom = false;
            msd.SwitchToMode(vanilla);
            _host.Mode = mode;
            return true;
        }

        internal static bool IsModeAvailable(MultiplayerSelectDirector msd, string mode)
        {
            GrowthGameModeTag vanilla;
            return msd != null && TryGetVanillaMode(NormalizeMode(mode), out vanilla) && msd.isCurMapSupportThisMode(vanilla);
        }

        internal static bool TryGetVanillaMode(string mode, out GrowthGameModeTag vanilla)
        {
            mode = NormalizeMode(mode);
            if (mode == "tdm") { vanilla = GrowthGameModeTag.tTeamDeathMatch; return true; }
            if (mode == "stronghold" || mode == "ctf") { vanilla = GrowthGameModeTag.tStronghold; return true; }
            if (mode == "kc" || mode == "zombies") { vanilla = GrowthGameModeTag.tKillingCompetition; return true; }
            vanilla = GrowthGameModeTag.tTeamDeathMatch;
            return false;
        }

        internal static string PackHost(MultiplayerSelectDirector msd)
        {
            SyncHostModeFromRoomUi(msd);
            Sanitize(_host);
            return Pack(_host);
        }

        internal static string Pack(CNRMatchSettingsData d)
        {
            if (d == null) return "";
            Sanitize(d);
            return d.Schema + "|" + NormalizeMode(d.Mode) + "|" + d.RoundSeconds + "|" + d.KcKillLimit + "|" +
                d.StrongholdResources + "|" + d.CtfScoreLimit + "|" + d.CtfFlagReturnSeconds + "|" +
                d.ZombieStartDelaySeconds + "|" + d.ZombieInterRoundSeconds + "|" + d.ZombieMaxPerRound;
        }

        internal static bool TryUnpack(string packed, out CNRMatchSettingsData data, out string reason)
        {
            data = null;
            reason = "";
            if (string.IsNullOrEmpty(packed)) { reason = "Room did not advertise match settings."; return false; }
            try
            {
                string[] p = packed.Split('|');
                if (p.Length < 10) { reason = "Room match settings are incomplete."; return false; }
                CNRMatchSettingsData d = new CNRMatchSettingsData();
                if (!int.TryParse(p[0], out d.Schema) || d.Schema != CurrentSchema)
                {
                    reason = "Unsupported match-settings schema " + p[0] + ".";
                    return false;
                }
                d.Mode = NormalizeMode(p[1]);
                GrowthGameModeTag ignored;
                if (!TryGetVanillaMode(d.Mode, out ignored)) { reason = "Room advertised an unknown game mode."; return false; }
                if (!int.TryParse(p[2], out d.RoundSeconds) ||
                    !int.TryParse(p[3], out d.KcKillLimit) ||
                    !int.TryParse(p[4], out d.StrongholdResources) ||
                    !int.TryParse(p[5], out d.CtfScoreLimit) ||
                    !int.TryParse(p[6], out d.CtfFlagReturnSeconds) ||
                    !int.TryParse(p[7], out d.ZombieStartDelaySeconds) ||
                    !int.TryParse(p[8], out d.ZombieInterRoundSeconds) ||
                    !int.TryParse(p[9], out d.ZombieMaxPerRound))
                {
                    reason = "Room match settings contain invalid values.";
                    return false;
                }
                if (!ValidateRanges(d, out reason)) return false;
                data = d;
                return true;
            }
            catch (Exception ex)
            {
                reason = "Could not parse room match settings: " + ex.Message;
                return false;
            }
        }

        internal static bool QueueFromRoom(RoomInfo room, out string reason)
        {
            reason = "";
            if (room == null) { reason = "Room information is unavailable."; return false; }
            string canonical = NormalizeMode(CNRCompatibility.GetRoomProp(room, CNRMatchMetadata.PropGameMode));
            string raw = CNRCompatibility.GetRoomProp(room, PropSettings);

            // Truly old/vanilla rooms have no CNR metadata at all. They remain joinable with defaults.
            bool isCnrRoom = !string.IsNullOrEmpty(CNRCompatibility.GetRoomProp(room, "cnrp")) ||
                             !string.IsNullOrEmpty(CNRCompatibility.GetRoomProp(room, "cnrm"));
            CNRMatchSettingsData parsed;
            if (string.IsNullOrEmpty(raw) && !isCnrRoom)
            {
                parsed = NewDefaults(string.IsNullOrEmpty(canonical) ? "tdm" : canonical);
            }
            else if (!TryUnpack(raw, out parsed, out reason))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(canonical) && parsed.Mode != canonical)
            {
                reason = "Bad match state. Match settings do not agree with the advertised game mode.";
                return false;
            }

            _pending = parsed.Clone();
            _pendingRoom = room.name ?? "";
            ModEntry.Log("MatchSettings: queued room=" + _pendingRoom + " settings=" + Pack(_pending));
            return true;
        }

        internal static void QueueHostForCreatedRoom(string roomName)
        {
            _pending = _host.Clone();
            _pendingRoom = roomName ?? "";
            ModEntry.Log("MatchSettings: queued host settings room=" + _pendingRoom + " settings=" + Pack(_pending));
        }

        internal static void PromotePending(string roomName)
        {
            if (_pending == null) return;
            if (!string.IsNullOrEmpty(_pendingRoom) && !string.IsNullOrEmpty(roomName) &&
                !string.Equals(_pendingRoom, roomName, StringComparison.Ordinal)) return;
            _active = _pending.Clone();
            _pending = null;
            _pendingRoom = "";
            CtfMode.WinScore = _active.CtfScoreLimit;
            ModEntry.Log("MatchSettings: activated " + Pack(_active));
        }

        internal static void ClearPending()
        {
            _pending = null;
            _pendingRoom = "";
        }

        internal static void ClearActive()
        {
            _active = null;
            CtfMode.WinScore = 30;
        }

        internal static string NormalizeMode(string mode)
        {
            mode = (mode ?? "").Trim().ToLowerInvariant();
            if (mode == "teamdeathmatch" || mode == "team deathmatch") return "tdm";
            if (mode == "killingcompetition" || mode == "killing competition") return "kc";
            if (mode == "capturetheflag" || mode == "capture the flag") return "ctf";
            return mode;
        }

        internal static bool ValidateRanges(CNRMatchSettingsData d, out string reason)
        {
            reason = "";
            if (d.RoundSeconds < 60 || d.RoundSeconds > 3600) { reason = "Round time must be between 1 and 60 minutes."; return false; }
            if (d.KcKillLimit != 0 && (d.KcKillLimit < 5 || d.KcKillLimit > 500)) { reason = "Kill limit must be Auto or 5-500."; return false; }
            if (d.StrongholdResources < 100 || d.StrongholdResources > 5000) { reason = "Stronghold resources must be 100-5000."; return false; }
            if (d.CtfScoreLimit < 1 || d.CtfScoreLimit > 100) { reason = "CTF score limit must be 1-100."; return false; }
            if (d.CtfFlagReturnSeconds < 5 || d.CtfFlagReturnSeconds > 180) { reason = "Flag return time must be 5-180 seconds."; return false; }
            if (d.ZombieStartDelaySeconds < 0 || d.ZombieStartDelaySeconds > 60) { reason = "Zombie start delay must be 0-60 seconds."; return false; }
            if (d.ZombieInterRoundSeconds < 0 || d.ZombieInterRoundSeconds > 60) { reason = "Zombie inter-round delay must be 0-60 seconds."; return false; }
            if (d.ZombieMaxPerRound < 5 || d.ZombieMaxPerRound > 150) { reason = "Zombie cap must be 5-150."; return false; }
            return true;
        }

        internal static void Sanitize(CNRMatchSettingsData d)
        {
            if (d == null) return;
            d.Schema = CurrentSchema;
            d.Mode = NormalizeMode(d.Mode);
            d.RoundSeconds = Mathf.Clamp(d.RoundSeconds, 60, 3600);
            if (d.KcKillLimit != 0) d.KcKillLimit = Mathf.Clamp(d.KcKillLimit, 5, 500);
            d.StrongholdResources = Mathf.Clamp(d.StrongholdResources, 100, 5000);
            d.CtfScoreLimit = Mathf.Clamp(d.CtfScoreLimit, 1, 100);
            d.CtfFlagReturnSeconds = Mathf.Clamp(d.CtfFlagReturnSeconds, 5, 180);
            d.ZombieStartDelaySeconds = Mathf.Clamp(d.ZombieStartDelaySeconds, 0, 60);
            d.ZombieInterRoundSeconds = Mathf.Clamp(d.ZombieInterRoundSeconds, 0, 60);
            d.ZombieMaxPerRound = Mathf.Clamp(d.ZombieMaxPerRound, 5, 150);
        }
    }

    // Applies the already-validated pending rules only after the room has been joined.
    public class CNRMatchSettingsRuntimeHook : MonoBehaviour
    {
        private CNRMultiplayerManager _manager;
        private bool _managerInitialised;

        internal static float CtfFlagReturnSeconds
        {
            get { return Mathf.Max(5f, CNRMatchSettings.Active.CtfFlagReturnSeconds); }
        }

        internal static int ZombieMaxPerRound
        {
            get { return Mathf.Clamp(CNRMatchSettings.Active.ZombieMaxPerRound, 5, 150); }
        }

        internal static float ZombieStartDelay
        {
            get { return Mathf.Clamp(CNRMatchSettings.Active.ZombieStartDelaySeconds, 0, 60); }
        }

        internal static float ZombieInterRoundDelay
        {
            get { return Mathf.Clamp(CNRMatchSettings.Active.ZombieInterRoundSeconds, 0, 60); }
        }

        void OnLevelWasLoaded(int level)
        {
            _manager = null;
            _managerInitialised = false;
            if (Application.loadedLevelName == "MainMenu" || Application.loadedLevelName == "MultiplayerSelect") return;
            try
            {
                if (PhotonNetwork.room != null) CNRMatchSettings.PromotePending(PhotonNetwork.room.name);
            }
            catch { }
        }

        void LateUpdate()
        {
            try
            {
                if (PhotonNetwork.room == null)
                {
                    if (Application.loadedLevelName == "MainMenu" || Application.loadedLevelName == "MultiplayerSelect")
                        CNRMatchSettings.ClearActive();
                    _manager = null;
                    _managerInitialised = false;
                    return;
                }

                if (CNRMatchSettings.Pending != null)
                    CNRMatchSettings.PromotePending(PhotonNetwork.room.name);

                CNRMultiplayerManager mgr = CNRMultiplayerManager.mInstance;
                if (mgr == null) return;
                if (_manager != mgr)
                {
                    _manager = mgr;
                    _managerInitialised = false;
                }
                ApplyToManager(mgr);
            }
            catch (Exception ex)
            {
                ModEntry.Log("MatchSettings runtime error: " + ex.Message);
            }
        }

        private void ApplyToManager(CNRMultiplayerManager mgr)
        {
            CNRMatchSettingsData s = CNRMatchSettings.Active;
            if (s == null) return;

            // Vanilla recomputes this from a hard-coded 600 every Update. LateUpdate
            // replaces only the resulting value, leaving its network elapsed-time sync intact.
            float elapsed = 0f;
            try { elapsed = mgr.myPlayerInfo != null ? mgr.myPlayerInfo.mGameRoundTime : 0f; } catch { }
            mgr.mGameRoundTimeRest = Mathf.Max(0f, s.RoundSeconds - elapsed);

            if (mgr.myModeInfo == null) return;
            if (!_managerInitialised)
            {
                if (mgr.myModeInfo.mCurMode == GrowthGameModeTag.tStronghold && mgr.myModeInfo.mStrongholdInfo != null && s.Mode != "ctf")
                {
                    mgr.myModeInfo.mStrongholdInfo.copResource = s.StrongholdResources;
                    mgr.myModeInfo.mStrongholdInfo.robberResource = s.StrongholdResources;
                }
                _managerInitialised = true;
                ModEntry.Log("MatchSettings: applied runtime rules mode=" + s.Mode);
            }

            if (mgr.myModeInfo.mCurMode == GrowthGameModeTag.tKillingCompetition && mgr.myModeInfo.mKillingCompetitionInfo != null && s.Mode != "zombies")
            {
                if (s.KcKillLimit > 0) mgr.myModeInfo.mKillingCompetitionInfo.MAXKilling = s.KcKillLimit;
            }

            if (s.Mode == "ctf") CtfMode.WinScore = s.CtfScoreLimit;
        }
    }
}
