using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    internal enum CNRArenaBotBehavior
    {
        Patrol,
        Search,
        Rush,
        Attack,
        Recover
    }

    internal class CNRArenaBotState
    {
        public string Id;
        public TeamType Team;
        public int SquadId = -1;
        public int SquadSlot;
        public int SquadSize = 1;
        public int Hp = 100;
        public int Kills;
        public int Deaths;
        public Vector3 Position;
        public Quaternion BodyRotation = Quaternion.identity;
        public Quaternion GunRotation = Quaternion.identity;
        public Vector3 FirePoint;
        public PlayerStatus Status = PlayerStatus.idle;
        public float RespawnAt;
        public float NextShotAt;
        public float FireStatusUntil;
        public float NextThinkAt;
        public float LastMovedAt;
        public Vector3 LastMoveSample;
        public string LastTargetId = "";
        public Vector3 LastKnownTargetPosition;
        public float LastTargetVisibleAt;
        public bool HasLastKnownTarget;
        public float TargetCertainty;
        public float ReactionReadyAt;
        public float CurrentTargetScore;
        public float TacticalScore = -1f;
        public float TacticalCommittedUntil;
        public Vector3 AimDirection;
        public float AimErrorDegrees;
        public float AimErrorTarget;
        public float NextAimErrorAt;
        public Vector3 WanderDirection;
        public Vector3 WanderTarget;
        public float WanderUntil;
        public float NextWanderAt;
        public List<Vector3> NavPath;
        public int NavPathIndex;
        public float NextNavRepathAt;
        public Vector3 NavTarget;
        public Vector3 NavWaypoint;
        public Vector3 FacingDirection;
        public Vector3 MoveVelocity;
        public Vector3 TacticalTarget;
        public float NextRepositionAt;
        public float NextAlertAt;
        public int StrafeSign = 1;
        public int StuckRecoveries;
        public float RecoverUntil;
        public CNRArenaBotBehavior Behavior = CNRArenaBotBehavior.Patrol;
    }

    // Host-authoritative virtual players for TDM and Killing Competition.
    // Bots occupy normal CNRMultiplayerManager PlayerInfo slots, so vanilla player
    // rendering, team counters, scoreboards and end-of-round rankings see them as
    // players. They deliberately do NOT consume Photon room slots, allowing a real
    // joiner to replace an AUTO bot without the room ever appearing full.
    public class CNRArenaBotManager : MonoBehaviour
    {
        private const int BOT_ID_BASE = 9001;
        private const int MAX_BOTS = 14;       // vanilla scoreboards expose 7 rows per team
        private const float RECONCILE_INTERVAL = 0.40f;
        private const float STATE_INTERVAL = 0.20f;
        private const float THINK_INTERVAL = 0.10f;
        private const float RESPAWN_SECONDS = 3.0f;
        private const float TARGET_MEMORY_SECONDS = 5.0f;
        private const float TARGET_REACHED_DISTANCE = 1.15f;
        private const float DEFAULT_BODY_GROUND_OFFSET = 0.85f;
        private const float NET_PLAYER_POSITION_Y_BIAS = 0.233f;
        private const float BOT_GROUND_EXTRA_LIFT = 0f;
        private const float BOT_VISION_HALF_ANGLE = 55f;
        private const float WANDER_SPEED = 1.85f;
        private const float NAV_REPATH_INTERVAL = 0.75f;
        private const float NAV_TARGET_REPATH_DISTANCE = 1.0f;
        private const float NAV_HEIGHT_TOLERANCE = 1.15f;
        private const float NAV_CLIMB_RATE = 3.8f;
        private const float NAV_DESCEND_RATE = 7.0f;
        private const float BOT_TURN_RATE = 300f;
        private const float BOT_FIRE_HALF_ANGLE = 12f;
        private const float BOT_ACCELERATION = 10.5f;
        private const float BOT_SEPARATION_RADIUS = 1.25f;
        private const float BOT_ALERT_RADIUS = 26f;
        private const float BOT_ALERT_INTERVAL = 1.0f;
        private const float BOT_STUCK_SECONDS = 1.25f;
        private const int SQUAD_MAX_SIZE = 3;
        private const float SQUAD_LANE_SPACING = 4.25f;
        private const float SQUAD_REPORT_ERROR = 0.85f;
        private const int NAV_SHORTCUT_LOOKAHEAD = 4;
        private const float TARGET_SWITCH_ADVANTAGE = 1.22f;
        private const float TARGET_CERTAINTY_FIRE = 0.62f;
        private const float TARGET_CERTAINTY_GAIN = 1.75f;
        private const float TARGET_CERTAINTY_LOSS = 0.32f;
        private const float TARGET_REACTION_MIN = 0.22f;
        private const float TARGET_REACTION_MAX = 0.48f;
        private const int TACTICAL_CANDIDATE_COUNT = 6;
        private const float TACTICAL_SWITCH_ADVANTAGE = 1.18f;
        private const float TACTICAL_REACHED_DISTANCE = 0.95f;
        private const float AIM_ERROR_CHANGE_RATE = 18f;
        private const float AIM_TRACK_RATE = 8.0f;
        private const float AIM_ERROR_REFRESH_MIN = 0.28f;
        private const float AIM_ERROR_REFRESH_MAX = 0.58f;

        private static CNRArenaBotManager _instance;
        private static string _pendingPackedState;
        private static int _pendingPackedStateSender;
        private static bool _pendingClear;

        private readonly List<CNRArenaBotState> _bots = new List<CNRArenaBotState>();
        private readonly Dictionary<string, NetPlayerController> _botVisuals = new Dictionary<string, NetPlayerController>();
        private readonly Dictionary<string, PlayerStatus> _humanLastStatus = new Dictionary<string, PlayerStatus>();
        private readonly Dictionary<string, string> _humanLastBotAttacker = new Dictionary<string, string>();
        private readonly Dictionary<string, float> _humanLastBotAttackAt = new Dictionary<string, float>();

        private CNRMultiplayerManager _mgr;
        private float _nextReconcileAt;
        private float _nextStateAt;
        private float _nextVisualBindAt;
        private string _scene = "";
        private string _arenaNavScene = "";
        private float _nextArenaNavBakeAt;
        private bool _wasActive;
        private FieldInfo _timeoutCountsField;
        private float _bodyGroundOffset = DEFAULT_BODY_GROUND_OFFSET;
        private float _nextBodyGroundSampleAt;

        void Awake()
        {
            _instance = this;
        }

        void OnLevelWasLoaded(int level)
        {
            _scene = Application.loadedLevelName ?? "";
            _mgr = null;
            _nextReconcileAt = 0f;
            _nextStateAt = 0f;
            _nextVisualBindAt = 0f;
            _arenaNavScene = "";
            _nextArenaNavBakeAt = 0f;
            _botVisuals.Clear();
            _humanLastStatus.Clear();
            _humanLastBotAttacker.Clear();
            _humanLastBotAttackAt.Clear();
            if (_scene == "MainMenu" || _scene == "MultiplayerSelect")
                ResetLocalBots();
        }

        void Update()
        {
            if (_pendingClear)
            {
                _pendingClear = false;
                ResetLocalBots();
            }

            if (!IsRoomActive())
            {
                if (_wasActive) ResetLocalBots();
                _wasActive = false;
                return;
            }

            _wasActive = true;
            _mgr = CNRMultiplayerManager.mInstance;
            if (_mgr == null) return;

            CNRMatchSettingsData rules = CNRMatchSettings.Active;
            if (!RulesEnableBots(rules))
            {
                if (_bots.Count > 0) ResetLocalBots();
                return;
            }

            if (IsAuthority())
            {
                EnsureArenaNavGrid();
                if (Time.realtimeSinceStartup >= _nextReconcileAt)
                {
                    _nextReconcileAt = Time.realtimeSinceStartup + RECONCILE_INTERVAL;
                    ReconcileRoster(rules);
                }
                if (AnyHumanInGame())
                {
                    SimulateBots();
                    DetectBotKillsOnHumans();
                }
                else
                {
                    HoldBotsForLobby();
                }
                if (Time.realtimeSinceStartup >= _nextStateAt)
                {
                    _nextStateAt = Time.realtimeSinceStartup + STATE_INTERVAL;
                    BroadcastState();
                }
            }
            else if (_pendingPackedState != null)
            {
                string packed = _pendingPackedState;
                int sender = _pendingPackedStateSender;
                _pendingPackedState = null;
                _pendingPackedStateSender = 0;
                if (IsCurrentMasterSender(sender)) ApplyPackedState(packed);
            }

            InjectPlayerInfos();
            if (Time.realtimeSinceStartup >= _nextVisualBindAt)
            {
                _nextVisualBindAt = Time.realtimeSinceStartup + 0.35f;
                BindDamageReceivers();
            }
        }

        private bool IsRoomActive()
        {
            try
            {
                if (PhotonNetwork.room == null) return false;
                if (Application.loadedLevelName == "MainMenu" || Application.loadedLevelName == "MultiplayerSelect") return false;
                return CNRMultiplayerManager.mInstance != null;
            }
            catch { return false; }
        }

        private static bool RulesEnableBots(CNRMatchSettingsData rules)
        {
            if (rules == null) return false;
            if (rules.Mode == "tdm") return rules.TdmBotsEnabled != 0;
            if (rules.Mode == "kc") return rules.KcBotsEnabled != 0;
            return false;
        }

        private static bool IsFreeForAllMode()
        {
            CNRMatchSettingsData rules = CNRMatchSettings.Active;
            return rules != null && rules.Mode == "kc";
        }

        private bool IsAuthority()
        {
            try { return PhotonNetwork.isMasterClient; }
            catch { return false; }
        }

        private bool EnsureArenaNavGrid()
        {
            if (_arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready) return true;
            float now = Time.realtimeSinceStartup;
            if (now < _nextArenaNavBakeAt) return false;
            _nextArenaNavBakeAt = now + 2f;

            try
            {
                Vector3 center = FindArenaNavCenter();
                CNRZombieMod.ZombieNavGrid.Bake(center);
                if (CNRZombieMod.ZombieNavGrid.Ready)
                {
                    _arenaNavScene = _scene;
                    ModEntry.Log("ArenaBots: shared nav grid baked for " + _scene + " center=" + center);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Log("ArenaBots nav bake failed: " + ex.Message);
            }
            return false;
        }

        private Vector3 FindArenaNavCenter()
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int team = 1; team <= 2; team++)
            {
                for (int i = 1; i <= 16; i++)
                {
                    GameObject spawn = GameObject.Find("Spawn_" + team + "_" + i);
                    if (spawn == null) continue;
                    sum += spawn.transform.position;
                    count++;
                }
            }
            if (count == 0)
            {
                for (int i = 1; i <= 8; i++)
                {
                    GameObject spawn = GameObject.Find("Spawn_" + i);
                    if (spawn == null) continue;
                    sum += spawn.transform.position;
                    count++;
                }
            }
            if (count > 0) return sum / count;

            if (_mgr != null && _mgr.myPlayerInfo != null &&
                _mgr.myPlayerInfo.mConnnectStatus == ConnectStatus.InGame)
                return _mgr.myPlayerInfo.mPosition;
            return Vector3.zero;
        }

        private static bool IsCurrentMasterSender(int senderId)
        {
            try
            {
                PhotonPlayer master = PhotonNetwork.masterClient;
                return master != null && master.ID == senderId;
            }
            catch { return false; }
        }

        void OnMasterClientSwitched(PhotonPlayer newMaster)
        {
            try
            {
                int newMasterId = newMaster != null ? newMaster.ID : 0;
                if (PhotonNetwork.isMasterClient)
                {
                    // A state packet already accepted from the former authority may be
                    // newer than the last frame we applied. Adopt it before taking over.
                    if (_pendingPackedState != null)
                    {
                        string packed = _pendingPackedState;
                        _pendingPackedState = null;
                        _pendingPackedStateSender = 0;
                        ApplyPackedState(packed);
                    }
                    _nextReconcileAt = 0f;
                    _nextStateAt = 0f;
                    ModEntry.Log("ArenaBots: authority handoff -> local master " + newMasterId);
                }
                else
                {
                    // Discard any packet that was queued across the authority boundary.
                    // Fresh state from the newly elected master will replace it immediately.
                    _pendingPackedState = null;
                    _pendingPackedStateSender = 0;
                    ModEntry.Log("ArenaBots: authority handoff -> remote master " + newMasterId);
                }
            }
            catch (Exception ex) { ModEntry.Log("ArenaBots authority handoff: " + ex.Message); }
        }

        private void ReconcileRoster(CNRMatchSettingsData rules)
        {
            int realCop, realRobber, realOtherCount;
            CountRealPlayers(out realCop, out realRobber, out realOtherCount);

            int desiredCop = 0;
            int desiredRobber = 0;
            if (rules.Mode == "tdm")
            {
                desiredCop = rules.TdmCopBotsAuto != 0 ? Mathf.Max(0, rules.TdmCopBots - realCop) : rules.TdmCopBots;
                desiredRobber = rules.TdmRobberBotsAuto != 0 ? Mathf.Max(0, rules.TdmRobberBots - realRobber) : rules.TdmRobberBots;
            }
            else if (rules.Mode == "kc")
            {
                // Kill Competition exposes one global bot pool. With Auto enabled,
                // KcBotCount is the desired total population (humans + bots).
                int realTotal = realCop + realRobber;
                int count = rules.KcBotsAuto != 0
                    ? Mathf.Max(0, rules.KcBotCount - realTotal)
                    : rules.KcBotCount;
                count = Mathf.Clamp(count, 0, MAX_BOTS);

                // The vanilla mode still stores an internal team value, so distribute
                // the global bot pool to keep those backing slots roughly balanced.
                int copTotal = realCop;
                int robberTotal = realRobber;
                for (int i = 0; i < count; i++)
                {
                    if (copTotal <= robberTotal) { desiredCop++; copTotal++; }
                    else { desiredRobber++; robberTotal++; }
                }
            }

            // Counts are bot counts when Auto is off. Do not shrink them as humans join.
            // With Auto on, the configured count is the desired total occupancy for that team.
            desiredCop = Mathf.Clamp(desiredCop, 0, 7);
            desiredRobber = Mathf.Clamp(desiredRobber, 0, 7);

            // otherPlayersInfoList has 20 slots. Keep enough room for every real remote player.
            int maxBotsBySlots = Mathf.Clamp(20 - realOtherCount, 0, MAX_BOTS);
            while (desiredCop + desiredRobber > maxBotsBySlots)
            {
                if (desiredCop > desiredRobber && desiredCop > 0) desiredCop--;
                else if (desiredRobber > 0) desiredRobber--;
                else break;
            }

            ReconcileTeam(TeamType.Cop, desiredCop);
            ReconcileTeam(TeamType.Robber, desiredRobber);
            RebuildSquadAssignments();
        }

        private bool AnyHumanInGame()
        {
            if (_mgr == null) return false;
            PlayerInfo mine = _mgr.myPlayerInfo;
            if (mine != null && !IsBotId(mine.mId) && mine.mConnnectStatus == ConnectStatus.InGame) return true;
            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others == null) return false;
            for (int i = 0; i < others.Length; i++)
            {
                PlayerInfo p = others[i];
                if (p != null && !IsBotId(p.mId) && p.mId != "null" && p.mConnnectStatus == ConnectStatus.InGame)
                    return true;
            }
            return false;
        }

        private void HoldBotsForLobby()
        {
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState bot = _bots[i];
                if (bot.Status != PlayerStatus.dead) bot.Status = PlayerStatus.idle;
                bot.LastTargetId = "";
            }
        }

        private void CountRealPlayers(out int cop, out int robber, out int otherReal)
        {
            cop = 0;
            robber = 0;
            otherReal = 0;
            if (_mgr == null) return;

            PlayerInfo mine = _mgr.myPlayerInfo;
            if (IsRealActivePlayer(mine))
            {
                if (mine.mTeam == TeamType.Cop) cop++;
                else if (mine.mTeam == TeamType.Robber) robber++;
            }

            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others == null) return;
            for (int i = 0; i < others.Length; i++)
            {
                PlayerInfo p = others[i];
                if (p == null || IsBotId(p.mId) || p.mId == "null") continue;
                otherReal++;
                if (!IsRealActivePlayer(p)) continue;
                if (p.mTeam == TeamType.Cop) cop++;
                else if (p.mTeam == TeamType.Robber) robber++;
            }
        }

        private static bool IsRealActivePlayer(PlayerInfo p)
        {
            return p != null && p.mId != "null" && !IsBotId(p.mId) &&
                p.mConnnectStatus == ConnectStatus.InGame && p.mTeam != TeamType.Nil;
        }

        private void ReconcileTeam(TeamType team, int desired)
        {
            int have = 0;
            for (int i = 0; i < _bots.Count; i++) if (_bots[i].Team == team) have++;

            while (have > desired)
            {
                for (int i = _bots.Count - 1; i >= 0; i--)
                {
                    if (_bots[i].Team != team) continue;
                    _bots.RemoveAt(i);
                    have--;
                    break;
                }
            }

            while (have < desired && _bots.Count < MAX_BOTS)
            {
                CNRArenaBotState bot = CreateBot(team);
                if (bot == null) break;
                _bots.Add(bot);
                have++;
            }
        }

        private void RebuildSquadAssignments()
        {
            if (IsFreeForAllMode())
            {
                for (int i = 0; i < _bots.Count; i++)
                {
                    _bots[i].SquadId = -1;
                    _bots[i].SquadSlot = 0;
                    _bots[i].SquadSize = 1;
                }
                return;
            }

            AssignTeamSquads(TeamType.Cop, 100);
            AssignTeamSquads(TeamType.Robber, 200);
        }

        private void AssignTeamSquads(TeamType team, int squadBase)
        {
            List<CNRArenaBotState> members = new List<CNRArenaBotState>();
            for (int i = 0; i < _bots.Count; i++)
                if (_bots[i] != null && _bots[i].Team == team) members.Add(_bots[i]);

            members.Sort(delegate(CNRArenaBotState a, CNRArenaBotState b)
            {
                return string.CompareOrdinal(a.Id, b.Id);
            });

            int cursor = 0;
            int group = 0;
            while (cursor < members.Count)
            {
                int size = Mathf.Min(SQUAD_MAX_SIZE, members.Count - cursor);
                int squadId = squadBase + group;
                for (int slot = 0; slot < size; slot++)
                {
                    CNRArenaBotState member = members[cursor + slot];
                    member.SquadId = squadId;
                    member.SquadSlot = slot;
                    member.SquadSize = size;
                }
                cursor += size;
                group++;
            }
        }

        private CNRArenaBotState CreateBot(TeamType team)
        {
            int id = FindFreeBotId();
            if (id <= 0) return null;
            CNRArenaBotState bot = new CNRArenaBotState();
            bot.Id = id.ToString(CultureInfo.InvariantCulture);
            bot.Team = team;
            bot.Position = FindSpawnPosition(team, null);
            bot.LastMoveSample = bot.Position;
            bot.LastMovedAt = Time.realtimeSinceStartup;
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            bot.WanderDirection = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            bot.WanderTarget = bot.Position;
            bot.WanderUntil = 0f;
            bot.NextWanderAt = 0f;
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
            bot.NavTarget = bot.Position;
            bot.NavWaypoint = bot.Position;
            bot.FacingDirection = bot.WanderDirection;
            bot.AimDirection = bot.WanderDirection;
            bot.StrafeSign = (id & 1) == 0 ? 1 : -1;
            bot.TacticalTarget = bot.Position;
            FaceDirection(bot, bot.WanderDirection);
            return bot;
        }

        private int FindFreeBotId()
        {
            for (int id = BOT_ID_BASE; id < BOT_ID_BASE + MAX_BOTS; id++)
            {
                bool used = false;
                for (int i = 0; i < _bots.Count; i++)
                    if (_bots[i].Id == id.ToString(CultureInfo.InvariantCulture)) { used = true; break; }
                if (!used) return id;
            }
            return -1;
        }

        private void SimulateBots()
        {
            if (_mgr == null) return;
            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState bot = _bots[i];
                if (bot.Status == PlayerStatus.dead || bot.Hp <= 0)
                {
                    bot.Status = PlayerStatus.dead;
                    if (bot.RespawnAt <= 0f) bot.RespawnAt = now + RESPAWN_SECONDS;
                    if (now >= bot.RespawnAt)
                    {
                        bot.Hp = 100;
                        bot.Status = PlayerStatus.idle;
                        bot.RespawnAt = 0f;
                        bot.Position = FindSpawnPosition(bot.Team, bot);
                        ClearTargetMemory(bot);
                        bot.LastMoveSample = bot.Position;
                        bot.LastMovedAt = now;
                        bot.NavPath = null;
                        bot.NavPathIndex = 0;
                        bot.NextNavRepathAt = 0f;
                        bot.NavTarget = bot.Position;
                        bot.NavWaypoint = bot.Position;
                        bot.WanderTarget = bot.Position;
                        bot.WanderUntil = 0f;
                        bot.NextWanderAt = 0f;
                        bot.MoveVelocity = Vector3.zero;
                        bot.TacticalTarget = bot.Position;
                        bot.RecoverUntil = 0f;
                        bot.StuckRecoveries = 0;
                        bot.Behavior = CNRArenaBotBehavior.Patrol;
                    }
                    continue;
                }

                if (now < bot.NextThinkAt) continue;
                bot.NextThinkAt = now + THINK_INTERVAL;
                ThinkBot(bot);
            }
        }

        private void ThinkBot(CNRArenaBotState bot)
        {
            float now = Time.realtimeSinceStartup;
            PlayerInfo targetInfo = null;
            CNRArenaBotState targetBot = null;
            Vector3 targetPos = Vector3.zero;
            string targetId = bot.LastTargetId;
            bool visible = false;

            // Resolve the current hostile first, then compare it against every other
            // visible hostile (human or bot). A new target must be meaningfully better
            // before we switch, otherwise two similar enemies make the bot flicker back
            // and forth every think tick.
            float currentScore = -1f;
            if (!string.IsNullOrEmpty(targetId) && TryResolveEnemyTarget(bot, targetId, out targetInfo, out targetBot))
            {
                Vector3 actual = targetBot != null ? targetBot.Position : targetInfo.mPosition;
                float actualDist = Vector3.Distance(bot.Position, actual);
                visible = CanSeeTarget(bot, actual, targetId, actualDist + 1f);
                if (visible)
                {
                    PlayerStatus targetStatus = targetBot != null ? targetBot.Status : targetInfo.mStatus;
                    currentScore = ScoreVisibleThreat(bot, actual, targetStatus, actualDist);
                    targetPos = actual;
                }
            }
            else if (!string.IsNullOrEmpty(targetId))
            {
                ClearTargetMemory(bot);
                targetId = "";
                targetInfo = null;
                targetBot = null;
            }

            PlayerInfo bestVisibleInfo;
            CNRArenaBotState bestVisibleBot;
            float bestVisibleScore;
            FindBestVisibleEnemy(bot, out bestVisibleInfo, out bestVisibleBot, out bestVisibleScore);
            if (bestVisibleBot != null || bestVisibleInfo != null)
            {
                string bestId = bestVisibleBot != null ? bestVisibleBot.Id : bestVisibleInfo.mId;
                bool sameTarget = visible && targetId == bestId;
                bool shouldSwitch = !visible || sameTarget ||
                    bestVisibleScore > currentScore * TARGET_SWITCH_ADVANTAGE;
                if (shouldSwitch)
                {
                    targetBot = bestVisibleBot;
                    targetInfo = bestVisibleInfo;
                    targetId = bestId;
                    targetPos = bestVisibleBot != null ? bestVisibleBot.Position : bestVisibleInfo.mPosition;
                    SetVisibleTarget(bot, targetId, targetPos, bestVisibleScore, now);
                    visible = true;
                    currentScore = bestVisibleScore;
                }
            }

            if (visible)
            {
                // Staying on the same enemy steadily improves identification certainty.
                // Switching to somebody else resets certainty/reaction time in
                // SetVisibleTarget, so seeing multiple enemies never means instant aim.
                SetVisibleTarget(bot, targetId, targetPos, currentScore, now);
                UpdateVisibleTargetCertainty(bot, targetPos, Vector3.Distance(bot.Position, targetPos));
            }
            else
            {
                bot.TargetCertainty = Mathf.MoveTowards(bot.TargetCertainty, 0f,
                    TARGET_CERTAINTY_LOSS * THINK_INTERVAL);

                if (bot.HasLastKnownTarget && !string.IsNullOrEmpty(bot.LastTargetId))
                {
                    float unseenFor = now - bot.LastTargetVisibleAt;
                    Vector3 toKnown = bot.LastKnownTargetPosition - bot.Position;
                    toKnown.y = 0f;
                    if (unseenFor >= TARGET_MEMORY_SECONDS || toKnown.magnitude <= TARGET_REACHED_DISTANCE)
                    {
                        ClearTargetMemory(bot);
                        WanderBot(bot);
                        return;
                    }
                    targetId = bot.LastTargetId;
                    targetPos = bot.LastKnownTargetPosition;
                    targetInfo = null;
                    targetBot = null;
                }
                else
                {
                    ClearTargetMemory(bot);
                    WanderBot(bot);
                    return;
                }
            }

            Vector3 flat = targetPos - bot.Position;
            flat.y = 0f;
            float dist = flat.magnitude;
            if (dist < 0.05f)
            {
                if (!visible) ClearTargetMemory(bot);
                WanderBot(bot);
                return;
            }
            Vector3 dir = flat / dist;
            float speed = 3.4f;
            Vector3 move = Vector3.zero;
            Vector3 moveTarget = bot.Position;
            bool wantsMove = false;

            if (visible && now >= bot.NextAlertAt)
            {
                bot.NextAlertAt = now + BOT_ALERT_INTERVAL;
                AlertNearbyBots(bot, targetId, targetPos);
            }

            // Merge the later SingleEnemyAI behavior with our global zombie A* route.
            // The brain chooses and HOLDS a tactical destination; A* only answers how to
            // reach it. Recomputing a strafe point from the current position every think
            // was the source of the constant orbit/spin behavior.
            if (bot.Behavior == CNRArenaBotBehavior.Recover && now < bot.RecoverUntil)
            {
                moveTarget = bot.TacticalTarget;
                wantsMove = true;
                bot.Status = PlayerStatus.walk;
            }
            else if (!visible)
            {
                bot.Behavior = CNRArenaBotBehavior.Search;
                moveTarget = targetPos;
                wantsMove = true;
                bot.Status = PlayerStatus.walk;
            }
            else if (dist > 20f)
            {
                bot.Behavior = CNRArenaBotBehavior.Rush;
                moveTarget = targetPos;
                wantsMove = true;
                bot.Status = PlayerStatus.walk;
            }
            else
            {
                bot.Behavior = CNRArenaBotBehavior.Attack;

                // Pick real combat positions instead of blindly alternating left/right.
                // The current position and held tactical destination are scored alongside
                // new ring candidates. Once a destination is chosen, hysteresis keeps it
                // until a replacement is substantially better.
                if (now >= bot.NextRepositionAt && now >= bot.TacticalCommittedUntil)
                {
                    Vector3 candidate;
                    float candidateScore;
                    if (TryChooseTacticalCombatPosition(bot, targetPos, targetId, out candidate, out candidateScore))
                    {
                        Vector3 heldPosition = bot.TacticalTarget;
                        float heldScore = bot.TacticalScore;
                        bool heldValid = bot.TacticalScore >= 0f &&
                            TryScoreTacticalPosition(bot, bot.TacticalTarget, targetPos, targetId,
                                out heldPosition, out heldScore);
                        bool shouldChange = !heldValid || candidateScore > heldScore * TACTICAL_SWITCH_ADVANTAGE;
                        if (shouldChange)
                        {
                            bot.TacticalTarget = candidate;
                            bot.TacticalScore = candidateScore;
                            bot.TacticalCommittedUntil = now + UnityEngine.Random.Range(1.45f, 2.25f);
                            bot.NavPath = null;
                            bot.NavPathIndex = 0;
                            bot.NextNavRepathAt = 0f;
                        }
                        else
                        {
                            bot.TacticalTarget = heldPosition;
                            bot.TacticalScore = heldScore;
                        }
                    }
                    bot.NextRepositionAt = now + UnityEngine.Random.Range(2.0f, 2.8f);
                }

                moveTarget = bot.TacticalTarget;
                Vector3 tacticalDelta = moveTarget - bot.Position;
                tacticalDelta.y = 0f;
                wantsMove = tacticalDelta.sqrMagnitude > TACTICAL_REACHED_DISTANCE * TACTICAL_REACHED_DISTANCE;
                bot.Status = wantsMove ? PlayerStatus.walk : PlayerStatus.idle;
            }

            if (wantsMove)
            {
                Vector3 navMove;
                if (TryGetNavigationDirection(bot, moveTarget, out navMove))
                    move = navMove;
                else if (_arenaNavScene != _scene || !CNRZombieMod.ZombieNavGrid.Ready)
                {
                    move = moveTarget - bot.Position;
                    move.y = 0f;
                    if (move.sqrMagnitude > 0.0001f) move.Normalize();
                }
            }

            if (move.sqrMagnitude > 0.01f)
                move = ApplyLocalSeparation(bot, move.normalized);

            // AIPath-style turning is gradual. In combat the body turns toward the enemy;
            // while searching/rushing through occluded geometry it follows the route.
            Vector3 facingDir = visible
                ? UpdateCombatAim(bot, targetPos, dist, move.sqrMagnitude > 0.01f, now)
                : (move.sqrMagnitude > 0.01f ? move.normalized : GetBotForward(bot));
            FaceDirection(bot, facingDir);
            Vector3 actualForward = GetBotForward(bot);
            if (visible)
            {
                float verticalAim = (targetPos.y + 0.9f) - (bot.Position.y + 1.15f);
                float pitch = Mathf.Atan2(verticalAim, Mathf.Max(0.05f, dist)) * Mathf.Rad2Deg;
                bot.GunRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
            else
            {
                bot.GunRotation = Quaternion.identity;
            }
            bot.FirePoint = bot.Position + new Vector3(0f, 1.15f, 0f) + actualForward * 0.45f;

            if (move.sqrMagnitude > 0.01f)
                MoveBot(bot, move.normalized, speed * THINK_INTERVAL);
            else
                bot.MoveVelocity = Vector3.MoveTowards(bot.MoveVelocity, Vector3.zero, BOT_ACCELERATION * THINK_INTERVAL);

            // Keep the fire state alive long enough to cross at least one authoritative
            // state broadcast. Otherwise a shot can begin and end between 0.20s packets,
            // so remote clients never see the bot actually fire.
            if (now < bot.FireStatusUntil)
                bot.Status = PlayerStatus.fire;

            // Seeing a target is intentionally wider than being aimed at it. A bot may
            // notice something near the edge of its 110-degree vision cone, but it cannot
            // shoot until its authoritative facing has turned to within 12 degrees.
            if (visible && dist <= 22f && now >= bot.NextShotAt &&
                bot.TargetCertainty >= TARGET_CERTAINTY_FIRE && now >= bot.ReactionReadyAt &&
                CanFireAtTarget(bot, targetPos, targetId, dist + 1f))
            {
                bot.Status = PlayerStatus.fire;
                bot.NextShotAt = now + UnityEngine.Random.Range(0.36f, 0.62f);
                bot.FireStatusUntil = now + 0.24f;

                // Damage now follows the bot's visible aim instead of an invisible hit
                // percentage. At distance the target subtends a smaller angle, so the same
                // amount of aim error naturally produces more misses.
                float aimError = Vector3.Angle(actualForward, dir);
                float effectiveError = aimError;
                float hitTolerance = GetAimHitTolerance(dist);

                // Small recoil kick changes the next aim destination rather than rolling
                // a totally independent accuracy result for every bullet.
                bot.AimErrorTarget = Mathf.Clamp(bot.AimErrorTarget + UnityEngine.Random.Range(-0.9f, 0.9f), -9f, 9f);

                if (effectiveError <= hitTolerance)
                {
                    int damage = UnityEngine.Random.Range(12, 23);
                    if (targetBot != null)
                        ApplyDamageToBot(targetBot.Id, damage, bot.Id, 0);
                    else if (targetInfo != null)
                        DamageHuman(targetInfo, damage, bot.Id);
                }
            }
        }

        private bool TryGetNavigationDirection(CNRArenaBotState bot, Vector3 target, out Vector3 direction)
        {
            direction = target - bot.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) direction.Normalize();
            else direction = GetBotForward(bot);

            if (_arenaNavScene != _scene || !CNRZombieMod.ZombieNavGrid.Ready) return false;

            // ZombieNavGrid stores ground heights, while arena PlayerInfo positions are
            // body-root positions. Query in ground-space so a nearby ceiling/upper floor
            // cannot look "closer" than the floor the bot is actually standing on.
            float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
            Vector3 fromGround = bot.Position;
            fromGround.y -= bodyOffset;
            Vector3 targetGround = target;
            targetGround.y -= bodyOffset;

            float now = Time.realtimeSinceStartup;
            Vector3 targetDelta = target - bot.NavTarget;
            targetDelta.y = 0f;
            bool repath = bot.NavPath == null || bot.NavPath.Count == 0 ||
                now >= bot.NextNavRepathAt || targetDelta.sqrMagnitude >= NAV_TARGET_REPATH_DISTANCE * NAV_TARGET_REPATH_DISTANCE;
            if (repath)
            {
                bot.NavPath = CNRZombieMod.ZombieNavGrid.Query(fromGround, targetGround);
                bot.NavPathIndex = 0;
                bot.NavTarget = target;
                int stagger = 0;
                int parsedId;
                if (int.TryParse(bot.Id, out parsedId)) stagger = Mathf.Abs(parsedId) & 3;
                bot.NextNavRepathAt = now + NAV_REPATH_INTERVAL + stagger * 0.06f;
            }

            Vector3 waypoint;
            if (bot.NavPath == null ||
                !CNRZombieMod.ZombieNavGrid.GetWaypoint(bot.NavPath, fromGround, ref bot.NavPathIndex, out waypoint))
                return false;

            TryShortcutNavigation(bot, fromGround, ref waypoint);
            bot.NavWaypoint = waypoint;
            Vector3 toWaypoint = waypoint - fromGround;
            toWaypoint.y = 0f;
            if (toWaypoint.sqrMagnitude <= 0.0001f) return false;
            direction = toWaypoint.normalized;
            return true;
        }

        private void TryShortcutNavigation(CNRArenaBotState bot, Vector3 fromGround, ref Vector3 waypoint)
        {
            // Equivalent to AIPath's forward-look behavior: keep A* as the safe global
            // route, but skip redundant grid corners when the intervening cells are all
            // directly traversable at the bot's current height.
            if (bot == null || bot.NavPath == null || bot.NavPath.Count == 0) return;
            int start = Mathf.Clamp(bot.NavPathIndex, 0, bot.NavPath.Count - 1);
            int max = Mathf.Min(bot.NavPath.Count - 1, start + NAV_SHORTCUT_LOOKAHEAD);
            for (int i = max; i > start; i--)
            {
                Vector3 candidate = bot.NavPath[i];
                if (!CanTravelDirectOnNav(fromGround, candidate)) continue;
                bot.NavPathIndex = i;
                waypoint = candidate;
                return;
            }
        }

        private bool CanTravelDirectOnNav(Vector3 fromGround, Vector3 toGround)
        {
            if (_arenaNavScene != _scene || !CNRZombieMod.ZombieNavGrid.Ready) return false;
            Vector3 flat = toGround - fromGround;
            flat.y = 0f;
            float distance = flat.magnitude;
            if (distance <= 0.55f) return true;

            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.45f));
            float lastY = fromGround.y;
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / (float)steps;
                Vector3 sample = Vector3.Lerp(fromGround, toGround, t);
                sample.y = lastY;
                Vector3 cell;
                if (!CNRZombieMod.ZombieNavGrid.TryGetWalkableAt(sample, lastY, NAV_HEIGHT_TOLERANCE, out cell)) return false;
                if (Mathf.Abs(cell.y - lastY) > NAV_HEIGHT_TOLERANCE) return false;
                lastY = cell.y;
            }
            return true;
        }

        private void MoveBot(CNRArenaBotState bot, Vector3 desired, float amount)
        {
            if (bot == null) return;
            desired.y = 0f;
            if (desired.sqrMagnitude <= 0.0001f) return;
            desired.Normalize();

            // AIPath accelerates a controller toward its desired velocity instead of
            // teleporting a fixed distance each decision tick. Preserve that feel in the
            // virtual bot simulation so corners, stairs and avoidance transitions are
            // smooth even though the authority is PlayerInfo-based.
            float requestedSpeed = amount / THINK_INTERVAL;
            Vector3 desiredVelocity = desired * requestedSpeed;
            bot.MoveVelocity = Vector3.MoveTowards(bot.MoveVelocity, desiredVelocity, BOT_ACCELERATION * THINK_INTERVAL);
            Vector3 move = bot.MoveVelocity;
            move.y = 0f;
            float moveSpeed = move.magnitude;
            if (moveSpeed <= 0.001f) return;
            move /= moveSpeed;
            amount = moveSpeed * THINK_INTERVAL;
            Vector3 next = bot.Position + move * amount;
            bool navReady = _arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready;

            if (navReady)
            {
                // Follow the same ground-height convention as ZombieDriver. Never fall
                // back to a generic downward ray while a nav grid is active: that ray was
                // able to select the top face of an invisible ceiling and teleport a bot
                // onto it. Invalid nav movement now means "repath", not "pick any floor".
                float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                float currentGroundY = bot.Position.y - bodyOffset;
                Vector3 navCell;
                if (!CNRZombieMod.ZombieNavGrid.TryGetWalkableAt(next, currentGroundY, NAV_HEIGHT_TOLERANCE, out navCell))
                {
                    bot.NavPath = null;
                    bot.NavPathIndex = 0;
                    bot.NextNavRepathAt = 0f;
                    return;
                }

                float dy = navCell.y - currentGroundY;
                float maxUp = NAV_CLIMB_RATE * THINK_INTERVAL;
                float maxDown = NAV_DESCEND_RATE * THINK_INTERVAL;
                next.y = bot.Position.y + Mathf.Clamp(dy, -maxDown, maxUp);
            }
            else
            {
                Vector3 origin = bot.Position + new Vector3(0f, 0.9f, 0f);
                RaycastHit wall;
                if (Physics.Raycast(origin, move, out wall, 1.25f, -21) &&
                    wall.collider != null && !wall.collider.isTrigger &&
                    !IsPlayerOrCharacterCollider(wall.collider.transform))
                {
                    Vector3 side = new Vector3(-move.z, 0f, move.x);
                    if ((int.Parse(bot.Id) & 1) != 0) side = -side;
                    move = side.normalized;
                    next = bot.Position + move * amount;
                }

                Vector3 grounded;
                if (TryProjectToWorldGround(next, out grounded))
                    next.y = grounded.y + GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                else
                    next.y = bot.Position.y;
            }

            bot.Position = next;
            float moved = Vector3.Distance(bot.LastMoveSample, bot.Position);
            if (moved > 0.30f)
            {
                bot.LastMoveSample = bot.Position;
                bot.LastMovedAt = Time.realtimeSinceStartup;
                bot.StuckRecoveries = 0;
            }
            else if (Time.realtimeSinceStartup - bot.LastMovedAt > BOT_STUCK_SECONDS)
            {
                RecoverStuckBot(bot, move, navReady);
                bot.LastMoveSample = bot.Position;
                bot.LastMovedAt = Time.realtimeSinceStartup;
            }
        }

        private void RecoverStuckBot(CNRArenaBotState bot, Vector3 forward, bool navReady)
        {
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
            bot.MoveVelocity = Vector3.zero;
            bot.StuckRecoveries++;

            if (bot.StuckRecoveries >= 3)
            {
                // Same spirit as the later AI: abandon a bad chase instead of endlessly
                // grinding against one corner. Patrol will choose a fresh route next tick.
                bot.StuckRecoveries = 0;
                ClearTargetMemory(bot);
                bot.Behavior = CNRArenaBotBehavior.Patrol;
                bot.NextWanderAt = 0f;
                return;
            }

            if (forward.sqrMagnitude <= 0.0001f) forward = GetBotForward(bot);
            forward.y = 0f;
            forward.Normalize();
            bot.StrafeSign = -bot.StrafeSign;
            Vector3 side = new Vector3(-forward.z, 0f, forward.x) * bot.StrafeSign;
            Vector3 candidate = bot.Position + side * 3.5f + forward * 1.75f;

            if (navReady)
            {
                float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                float currentGroundY = bot.Position.y - bodyOffset;
                Vector3 groundCandidate = candidate;
                groundCandidate.y -= bodyOffset;
                Vector3 snapped;
                if (CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                    groundCandidate, 7, currentGroundY, NAV_HEIGHT_TOLERANCE, out snapped))
                {
                    snapped.y += bodyOffset;
                    candidate = snapped;
                }
                else
                {
                    candidate = bot.Position;
                }
            }

            bot.TacticalTarget = candidate;
            bot.RecoverUntil = Time.realtimeSinceStartup + 0.8f;
            bot.Behavior = CNRArenaBotBehavior.Recover;
        }

        private static void ClearTargetMemory(CNRArenaBotState bot)
        {
            if (bot == null) return;
            bot.LastTargetId = "";
            bot.LastKnownTargetPosition = Vector3.zero;
            bot.LastTargetVisibleAt = 0f;
            bot.HasLastKnownTarget = false;
            bot.TargetCertainty = 0f;
            bot.ReactionReadyAt = 0f;
            bot.CurrentTargetScore = 0f;
            bot.TacticalTarget = bot.Position;
            bot.TacticalScore = -1f;
            bot.TacticalCommittedUntil = 0f;
            bot.NextRepositionAt = 0f;
            bot.AimDirection = GetBotForward(bot);
            bot.AimErrorDegrees = 0f;
            bot.AimErrorTarget = 0f;
            bot.NextAimErrorAt = 0f;
            bot.FireStatusUntil = 0f;
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
            bot.WanderUntil = 0f;
            bot.NextWanderAt = 0f;
        }

        private bool TryResolveEnemyTarget(CNRArenaBotState bot, string targetId, out PlayerInfo info, out CNRArenaBotState targetBot)
        {
            info = null;
            targetBot = null;
            if (string.IsNullOrEmpty(targetId)) return false;

            CNRArenaBotState b = FindBot(targetId);
            if (b != null)
            {
                if (b == bot || (!IsFreeForAllMode() && b.Team == bot.Team) || b.Status == PlayerStatus.dead || b.Hp <= 0) return false;
                targetBot = b;
                return true;
            }

            if (_mgr == null) return false;
            PlayerInfo mine = _mgr.myPlayerInfo;
            if (mine != null && mine.mId == targetId && IsValidHumanEnemy(bot, mine)) { info = mine; return true; }
            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others == null) return false;
            for (int i = 0; i < others.Length; i++)
            {
                PlayerInfo p = others[i];
                if (p != null && p.mId == targetId && !IsBotId(p.mId) && IsValidHumanEnemy(bot, p))
                {
                    info = p;
                    return true;
                }
            }
            return false;
        }

        private void FindBestVisibleEnemy(CNRArenaBotState bot, out PlayerInfo bestInfo, out CNRArenaBotState bestBot, out float bestScore)
        {
            bestInfo = null;
            bestBot = null;
            bestScore = -1f;

            if (_mgr != null && _mgr.myPlayerInfo != null)
                ConsiderVisibleHumanThreat(bot, _mgr.myPlayerInfo, ref bestInfo, ref bestBot, ref bestScore);

            PlayerInfo[] others = _mgr != null ? _mgr.otherPlayersInfoList : null;
            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    PlayerInfo p = others[i];
                    if (p == null || IsBotId(p.mId)) continue;
                    ConsiderVisibleHumanThreat(bot, p, ref bestInfo, ref bestBot, ref bestScore);
                }
            }

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState other = _bots[i];
                if (other == null || other == bot || other.Status == PlayerStatus.dead || other.Hp <= 0) continue;
                if (!IsFreeForAllMode() && other.Team == bot.Team) continue;

                Vector3 delta = other.Position - bot.Position;
                float dist = delta.magnitude;
                if (!CanSeeTarget(bot, other.Position, other.Id, dist + 1f)) continue;
                float score = ScoreVisibleThreat(bot, other.Position, other.Status, dist);
                if (score <= bestScore) continue;

                bestScore = score;
                bestInfo = null;
                bestBot = other;
            }
        }

        private void ConsiderVisibleHumanThreat(CNRArenaBotState bot, PlayerInfo candidate,
            ref PlayerInfo bestInfo, ref CNRArenaBotState bestBot, ref float bestScore)
        {
            if (!IsValidHumanEnemy(bot, candidate)) return;
            Vector3 delta = candidate.mPosition - bot.Position;
            float dist = delta.magnitude;
            if (!CanSeeTarget(bot, candidate.mPosition, candidate.mId, dist + 1f)) return;

            float score = ScoreVisibleThreat(bot, candidate.mPosition, candidate.mStatus, dist);
            if (score <= bestScore) return;
            bestScore = score;
            bestInfo = candidate;
            bestBot = null;
        }

        private float ScoreVisibleThreat(CNRArenaBotState observer, Vector3 position, PlayerStatus status, float distance)
        {
            Vector3 flat = position - observer.Position;
            flat.y = 0f;
            float facing = 1f;
            if (flat.sqrMagnitude > 0.001f)
            {
                float minDot = Mathf.Cos(BOT_VISION_HALF_ANGLE * Mathf.Deg2Rad);
                float dot = Vector3.Dot(GetBotForward(observer), flat.normalized);
                facing = Mathf.InverseLerp(minDot, 1f, dot);
            }

            float distanceScore = Mathf.Lerp(1f, 0.35f, Mathf.Clamp01(distance / 35f));
            float score = distanceScore * 0.72f + facing * 0.28f;
            if (status == PlayerStatus.fire) score += 0.08f;
            return score;
        }

        private void SetVisibleTarget(CNRArenaBotState bot, string targetId, Vector3 targetPosition,
            float score, float now)
        {
            bool changed = bot.LastTargetId != targetId;
            bot.LastTargetId = targetId;
            bot.LastKnownTargetPosition = targetPosition;
            bot.LastTargetVisibleAt = now;
            bot.HasLastKnownTarget = true;
            bot.CurrentTargetScore = score;

            if (!changed) return;

            // A new hostile must actually be noticed before the bot can shoot. Target
            // changes also invalidate the old combat destination so the new enemy does
            // not inherit a flank point that was chosen for somebody else.
            bot.TargetCertainty = 0.08f;
            bot.ReactionReadyAt = now + UnityEngine.Random.Range(TARGET_REACTION_MIN, TARGET_REACTION_MAX);
            bot.AimDirection = GetBotForward(bot);
            bot.AimErrorDegrees = UnityEngine.Random.Range(-7.5f, 7.5f);
            bot.AimErrorTarget = bot.AimErrorDegrees;
            bot.NextAimErrorAt = now;
            bot.TacticalTarget = bot.Position;
            bot.TacticalScore = -1f;
            bot.TacticalCommittedUntil = 0f;
            bot.NextRepositionAt = 0f;
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
        }

        private void UpdateVisibleTargetCertainty(CNRArenaBotState bot, Vector3 targetPosition, float distance)
        {
            Vector3 flat = targetPosition - bot.Position;
            flat.y = 0f;
            float centrality = 1f;
            if (flat.sqrMagnitude > 0.001f)
            {
                float minDot = Mathf.Cos(BOT_VISION_HALF_ANGLE * Mathf.Deg2Rad);
                centrality = Mathf.InverseLerp(minDot, 1f,
                    Vector3.Dot(GetBotForward(bot), flat.normalized));
            }
            float rangeFactor = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(distance / 30f));
            float gain = TARGET_CERTAINTY_GAIN * Mathf.Lerp(0.55f, 1f, centrality) * rangeFactor;
            bot.TargetCertainty = Mathf.MoveTowards(bot.TargetCertainty, 1f, gain * THINK_INTERVAL);
        }

        private void FindNearestVisibleEnemy(CNRArenaBotState bot, out PlayerInfo bestInfo, out CNRArenaBotState bestBot)
        {
            bestInfo = null;
            bestBot = null;
            float best = float.MaxValue;

            if (_mgr != null && _mgr.myPlayerInfo != null)
                ConsiderVisibleHumanTarget(bot, _mgr.myPlayerInfo, ref bestInfo, ref best);
            PlayerInfo[] others = _mgr != null ? _mgr.otherPlayersInfoList : null;
            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    PlayerInfo p = others[i];
                    if (p == null || IsBotId(p.mId)) continue;
                    ConsiderVisibleHumanTarget(bot, p, ref bestInfo, ref best);
                }
            }

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState b = _bots[i];
                if (b == bot || (!IsFreeForAllMode() && b.Team == bot.Team) || b.Status == PlayerStatus.dead || b.Hp <= 0) continue;
                Vector3 delta = b.Position - bot.Position;
                float d = delta.sqrMagnitude;
                if (d >= best) continue;
                float dist = Mathf.Sqrt(d);
                if (!CanSeeTarget(bot, b.Position, b.Id, dist + 1f)) continue;
                best = d;
                bestInfo = null;
                bestBot = b;
            }
        }

        private void ConsiderVisibleHumanTarget(CNRArenaBotState bot, PlayerInfo p, ref PlayerInfo best, ref float bestDist)
        {
            if (!IsValidHumanEnemy(bot, p)) return;
            Vector3 delta = p.mPosition - bot.Position;
            float d = delta.sqrMagnitude;
            if (d >= bestDist) return;
            float dist = Mathf.Sqrt(d);
            if (!CanSeeTarget(bot, p.mPosition, p.mId, dist + 1f)) return;
            bestDist = d;
            best = p;
        }

        private static bool IsValidHumanEnemy(CNRArenaBotState bot, PlayerInfo p)
        {
            if (p == null || p.mId == "null" || p.mTeam == TeamType.Nil) return false;
            if (!IsFreeForAllMode() && p.mTeam == bot.Team) return false;
            if (p.mConnnectStatus != ConnectStatus.InGame || p.mStatus == PlayerStatus.dead || p.mHp <= 0) return false;
            return true;
        }

        private void AlertNearbyBots(CNRArenaBotState source, string targetId, Vector3 seenPosition)
        {
            // Nearby teammates can react to a fight normally. Squadmates additionally
            // share a radio contact even when separated, but the report is deliberately
            // approximate and never replaces a hostile the receiver can already see.
            if (source == null || string.IsNullOrEmpty(targetId) || IsFreeForAllMode()) return;
            float now = Time.realtimeSinceStartup;
            float radiusSq = BOT_ALERT_RADIUS * BOT_ALERT_RADIUS;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState other = _bots[i];
                if (other == null || other == source || other.Team != source.Team || other.Status == PlayerStatus.dead) continue;

                bool squadMate = source.SquadId >= 0 && other.SquadId == source.SquadId;
                Vector3 delta = other.Position - source.Position;
                delta.y = 0f;
                if (!squadMate && delta.sqrMagnitude > radiusSq) continue;
                if (HasFreshVisibleTarget(other, now)) continue;

                Vector3 reportedPosition = seenPosition;
                if (squadMate)
                {
                    float reportAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                    float reportError = UnityEngine.Random.Range(0.20f, SQUAD_REPORT_ERROR);
                    reportedPosition += new Vector3(Mathf.Cos(reportAngle), 0f, Mathf.Sin(reportAngle)) * reportError;
                }

                other.LastTargetId = targetId;
                other.LastKnownTargetPosition = reportedPosition;
                other.LastTargetVisibleAt = now - (squadMate ? 0.45f : 0.25f);
                other.HasLastKnownTarget = true;
                other.TargetCertainty = squadMate ? 0.05f : 0.08f;
                other.ReactionReadyAt = now + UnityEngine.Random.Range(TARGET_REACTION_MIN, TARGET_REACTION_MAX);
                other.CurrentTargetScore = 0f;
                other.TacticalTarget = other.Position;
                other.TacticalScore = -1f;
                other.TacticalCommittedUntil = 0f;
                other.NextRepositionAt = 0f;
                other.Behavior = CNRArenaBotBehavior.Search;
                other.NavPath = null;
                other.NavPathIndex = 0;
                other.NextNavRepathAt = 0f;
            }
        }

        private bool HasFreshVisibleTarget(CNRArenaBotState bot, float now)
        {
            if (bot == null || !bot.HasLastKnownTarget || string.IsNullOrEmpty(bot.LastTargetId)) return false;
            if (now - bot.LastTargetVisibleAt > 0.35f) return false;

            PlayerInfo targetInfo;
            CNRArenaBotState targetBot;
            if (!TryResolveEnemyTarget(bot, bot.LastTargetId, out targetInfo, out targetBot)) return false;
            Vector3 targetPosition = targetBot != null ? targetBot.Position : targetInfo.mPosition;
            float distance = Vector3.Distance(bot.Position, targetPosition);
            return CanSeeTarget(bot, targetPosition, bot.LastTargetId, distance + 1f);
        }

        private Vector3 ApplyLocalSeparation(CNRArenaBotState bot, Vector3 desired)
        {
            // AIPath handled local collision through CharacterController. Arena bots are
            // virtual PlayerInfos, so emulate the useful part as a small separation force
            // while leaving static/global routing to ZombieNavGrid.
            Vector3 separation = Vector3.zero;
            float radiusSq = BOT_SEPARATION_RADIUS * BOT_SEPARATION_RADIUS;

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState other = _bots[i];
                if (other == null || other == bot || other.Status == PlayerStatus.dead) continue;
                Vector3 away = bot.Position - other.Position;
                away.y = 0f;
                float sq = away.sqrMagnitude;
                if (sq <= 0.0001f || sq >= radiusSq) continue;
                float dist = Mathf.Sqrt(sq);
                separation += (away / dist) * (1f - dist / BOT_SEPARATION_RADIUS);
            }

            if (_mgr != null)
            {
                PlayerInfo mine = _mgr.myPlayerInfo;
                if (mine != null && mine.mConnnectStatus == ConnectStatus.InGame)
                    AddHumanSeparation(bot, mine, ref separation, radiusSq);
                PlayerInfo[] others = _mgr.otherPlayersInfoList;
                if (others != null)
                    for (int i = 0; i < others.Length; i++)
                    {
                        PlayerInfo p = others[i];
                        if (p == null || IsBotId(p.mId)) continue;
                        AddHumanSeparation(bot, p, ref separation, radiusSq);
                    }
            }

            Vector3 result = desired + separation * 0.85f;
            result.y = 0f;
            if (result.sqrMagnitude <= 0.0001f) return desired;
            return result.normalized;
        }

        private static void AddHumanSeparation(CNRArenaBotState bot, PlayerInfo p, ref Vector3 separation, float radiusSq)
        {
            if (p == null || p.mId == "null" || p.mConnnectStatus != ConnectStatus.InGame || p.mStatus == PlayerStatus.dead) return;
            Vector3 away = bot.Position - p.mPosition;
            away.y = 0f;
            float sq = away.sqrMagnitude;
            if (sq <= 0.0001f || sq >= radiusSq) return;
            float dist = Mathf.Sqrt(sq);
            separation += (away / dist) * (1f - dist / BOT_SEPARATION_RADIUS);
        }

        private bool TryGetSquadLaneAnchor(CNRArenaBotState bot, Vector3 targetPosition, out Vector3 anchor)
        {
            anchor = bot != null ? bot.Position : targetPosition;
            if (bot == null || bot.SquadId < 0 || bot.SquadSize <= 1 || IsFreeForAllMode()) return false;

            Vector3 squadCenter = Vector3.zero;
            int activeMembers = 0;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState member = _bots[i];
                if (member == null || member.SquadId != bot.SquadId || member.Status == PlayerStatus.dead || member.Hp <= 0) continue;
                squadCenter += member.Position;
                activeMembers++;
            }
            if (activeMembers <= 0) return false;
            squadCenter /= activeMembers;

            Vector3 forward = targetPosition - squadCenter;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.01f)
            {
                forward = targetPosition - bot.Position;
                forward.y = 0f;
            }
            if (forward.sqrMagnitude <= 0.01f) return false;
            forward.Normalize();

            float lane = 0f;
            if (bot.SquadSize == 2)
                lane = bot.SquadSlot == 0 ? -0.65f : 0.65f;
            else if (bot.SquadSize > 2)
                lane = (bot.SquadSlot / (float)(bot.SquadSize - 1)) * 2f - 1f;

            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            anchor = targetPosition - forward * 9.0f + right * (lane * SQUAD_LANE_SPACING);
            anchor.y = targetPosition.y;
            return true;
        }

        private float GetSquadLaneScore(CNRArenaBotState bot, Vector3 position, Vector3 targetPosition)
        {
            Vector3 anchor;
            if (!TryGetSquadLaneAnchor(bot, targetPosition, out anchor)) return 1f;
            Vector3 delta = position - anchor;
            delta.y = 0f;
            return 1f / (1f + delta.magnitude * 0.18f);
        }

        private bool TryChooseTacticalCombatPosition(CNRArenaBotState bot, Vector3 targetPosition,
            string targetId, out Vector3 chosenPosition, out float chosenScore)
        {
            chosenPosition = bot.Position;
            chosenScore = -1f;

            Vector3 scored;
            float score;
            if (TryScoreTacticalPosition(bot, bot.Position, targetPosition, targetId, out scored, out score))
            {
                chosenPosition = scored;
                chosenScore = score;
            }

            if ((bot.TacticalTarget - bot.Position).sqrMagnitude > 0.35f * 0.35f &&
                TryScoreTacticalPosition(bot, bot.TacticalTarget, targetPosition, targetId, out scored, out score) &&
                score > chosenScore)
            {
                chosenPosition = scored;
                chosenScore = score;
            }

            // Always test the bot's assigned squad lane directly. Random ring samples can
            // still win if terrain makes the lane bad, but on open ground this gives a
            // squad a stable left/center/right frontage instead of bunching on one point.
            Vector3 laneAnchor;
            if (TryGetSquadLaneAnchor(bot, targetPosition, out laneAnchor) &&
                TryScoreTacticalPosition(bot, laneAnchor, targetPosition, targetId, out scored, out score) &&
                score > chosenScore)
            {
                chosenPosition = scored;
                chosenScore = score;
            }

            float baseAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            for (int i = 0; i < TACTICAL_CANDIDATE_COUNT; i++)
            {
                float ring = 7.0f + (i % 3) * 2.0f;
                float angle = baseAngle + ((Mathf.PI * 2f) * i / TACTICAL_CANDIDATE_COUNT);
                Vector3 candidate = targetPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ring;
                candidate.y = targetPosition.y;

                if (!TryScoreTacticalPosition(bot, candidate, targetPosition, targetId, out scored, out score))
                    continue;
                if (score <= chosenScore) continue;
                chosenPosition = scored;
                chosenScore = score;
            }

            return chosenScore >= 0f;
        }

        private bool TryScoreTacticalPosition(CNRArenaBotState bot, Vector3 candidateBody,
            Vector3 targetPosition, string targetId, out Vector3 snappedBody, out float score)
        {
            snappedBody = candidateBody;
            score = -1f;
            bool navReady = _arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready;
            float routeDistance = 0f;

            if (navReady)
            {
                float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                Vector3 fromGround = bot.Position;
                fromGround.y -= bodyOffset;
                Vector3 candidateGround = candidateBody;
                candidateGround.y -= bodyOffset;

                Vector3 snappedGround;
                if (!CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                    candidateGround, 7, candidateGround.y, NAV_HEIGHT_TOLERANCE * 1.5f, out snappedGround))
                    return false;

                snappedBody = snappedGround;
                snappedBody.y += bodyOffset;
                Vector3 routeFlat = snappedBody - bot.Position;
                routeFlat.y = 0f;
                if (routeFlat.sqrMagnitude > 0.75f * 0.75f)
                {
                    List<Vector3> path = CNRZombieMod.ZombieNavGrid.Query(fromGround, snappedGround);
                    if (path == null || path.Count < 2) return false;
                    Vector3 previous = fromGround;
                    for (int i = 0; i < path.Count; i++)
                    {
                        routeDistance += Vector3.Distance(previous, path[i]);
                        previous = path[i];
                    }
                }
            }
            else
            {
                Vector3 grounded;
                if (TryProjectToWorldGround(candidateBody, out grounded))
                    snappedBody.y = grounded.y + GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                routeDistance = Vector3.Distance(bot.Position, snappedBody);
            }

            Vector3 toTarget = targetPosition - snappedBody;
            toTarget.y = 0f;
            float targetDistance = toTarget.magnitude;
            if (targetDistance < 3.75f || targetDistance > 19.5f) return false;
            if (!HasLineOfSight(snappedBody + new Vector3(0f, 1.15f, 0f),
                targetPosition + new Vector3(0f, 0.9f, 0f), targetId, targetDistance + 1f))
                return false;

            float rangeScore = 1f - Mathf.Clamp01(Mathf.Abs(targetDistance - 9.0f) / 9.0f);
            float routeScore = 1f / (1f + routeDistance * 0.07f);
            float spacingScore = GetFriendlyTacticalSpacingScore(bot, snappedBody);
            float laneScore = GetSquadLaneScore(bot, snappedBody, targetPosition);

            Vector3 currentRadial = bot.Position - targetPosition;
            Vector3 candidateRadial = snappedBody - targetPosition;
            currentRadial.y = 0f;
            candidateRadial.y = 0f;
            float flankScore = 0.5f;
            if (currentRadial.sqrMagnitude > 0.01f && candidateRadial.sqrMagnitude > 0.01f)
            {
                float flankAngle = Vector3.Angle(currentRadial, candidateRadial) * Mathf.Deg2Rad;
                flankScore = Mathf.Abs(Mathf.Sin(flankAngle));
            }

            score = rangeScore * 0.32f + routeScore * 0.20f +
                spacingScore * 0.14f + flankScore * 0.12f + laneScore * 0.22f;
            return true;
        }

        private float GetFriendlyTacticalSpacingScore(CNRArenaBotState bot, Vector3 position)
        {
            if (IsFreeForAllMode()) return 1f;
            float nearest = 6f;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState other = _bots[i];
                if (other == null || other == bot || other.Team != bot.Team ||
                    other.Status == PlayerStatus.dead || other.Hp <= 0) continue;
                Vector3 delta = other.Position - position;
                delta.y = 0f;
                nearest = Mathf.Min(nearest, delta.magnitude);
            }
            return Mathf.Clamp01((nearest - 1.25f) / 4.0f);
        }

        private Vector3 UpdateCombatAim(CNRArenaBotState bot, Vector3 targetPosition,
            float distance, bool moving, float now)
        {
            Vector3 ideal = targetPosition - bot.Position;
            ideal.y = 0f;
            if (ideal.sqrMagnitude <= 0.0001f) return GetBotForward(bot);
            ideal.Normalize();

            if (bot.AimDirection.sqrMagnitude <= 0.0001f)
                bot.AimDirection = GetBotForward(bot);

            if (now >= bot.NextAimErrorAt)
            {
                float rangeT = Mathf.Clamp01((distance - 3f) / 20f);
                float spread = Mathf.Lerp(1.1f, 5.0f, rangeT);
                spread += (1f - bot.TargetCertainty) * 3.5f;
                if (moving) spread += 1.35f;
                bot.AimErrorTarget = UnityEngine.Random.Range(-spread, spread);
                bot.NextAimErrorAt = now + UnityEngine.Random.Range(AIM_ERROR_REFRESH_MIN, AIM_ERROR_REFRESH_MAX);
            }

            bot.AimErrorDegrees = Mathf.MoveTowards(bot.AimErrorDegrees, bot.AimErrorTarget,
                AIM_ERROR_CHANGE_RATE * THINK_INTERVAL);
            Vector3 desired = Quaternion.Euler(0f, bot.AimErrorDegrees, 0f) * ideal;
            desired.y = 0f;
            if (desired.sqrMagnitude > 0.0001f) desired.Normalize();

            Vector3 current = bot.AimDirection;
            current.y = 0f;
            if (current.sqrMagnitude <= 0.0001f) current = GetBotForward(bot);
            else current.Normalize();

            bot.AimDirection = Vector3.Slerp(current, desired,
                Mathf.Clamp01(AIM_TRACK_RATE * THINK_INTERVAL));
            bot.AimDirection.y = 0f;
            if (bot.AimDirection.sqrMagnitude <= 0.0001f) return ideal;
            bot.AimDirection.Normalize();
            return bot.AimDirection;
        }

        private static float GetAimHitTolerance(float distance)
        {
            float angularRadius = Mathf.Atan2(0.48f, Mathf.Max(1f, distance)) * Mathf.Rad2Deg;
            return Mathf.Clamp(angularRadius * 1.30f, 1.15f, 6.0f);
        }

        private bool CanSeeTarget(CNRArenaBotState bot, Vector3 target, string targetId, float maxDistance)
        {
            if (bot == null) return false;
            Vector3 toTarget = target - bot.Position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Vector3 forward = GetBotForward(bot);
                float minDot = Mathf.Cos(BOT_VISION_HALF_ANGLE * Mathf.Deg2Rad);
                if (Vector3.Dot(forward, toTarget.normalized) < minDot) return false;
            }
            return HasLineOfSight(bot.Position + new Vector3(0f, 1.15f, 0f),
                target + new Vector3(0f, 0.9f, 0f), targetId, maxDistance);
        }

        private bool CanFireAtTarget(CNRArenaBotState bot, Vector3 target, string targetId, float maxDistance)
        {
            if (bot == null) return false;
            Vector3 toTarget = target - bot.Position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.01f) return false;
            float minDot = Mathf.Cos(BOT_FIRE_HALF_ANGLE * Mathf.Deg2Rad);
            if (Vector3.Dot(GetBotForward(bot), toTarget.normalized) < minDot) return false;
            return HasLineOfSight(bot.Position + new Vector3(0f, 1.15f, 0f),
                target + new Vector3(0f, 0.9f, 0f), targetId, maxDistance);
        }

        private static Vector3 GetBotForward(CNRArenaBotState bot)
        {
            if (bot != null)
            {
                Vector3 facing = bot.FacingDirection;
                facing.y = 0f;
                if (facing.sqrMagnitude > 0.0001f) return facing.normalized;

                float yaw = bot.BodyRotation.eulerAngles.y - 90f;
                float rad = yaw * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;
            }
            return Vector3.forward;
        }

        private static void FaceDirection(CNRArenaBotState bot, Vector3 direction)
        {
            if (bot == null) return;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f) return;
            direction.Normalize();

            Vector3 current = bot.FacingDirection;
            current.y = 0f;
            if (current.sqrMagnitude <= 0.0001f) current = direction;
            else current.Normalize();

            // Match the later AIPath behavior: turn toward the requested heading instead
            // of snapping the authoritative quaternion to a new direction every think.
            // Vision uses this same vector, so what the bot can see and where its body is
            // actually turning can no longer diverge.
            float maxRadians = BOT_TURN_RATE * Mathf.Deg2Rad * THINK_INTERVAL;
            current = Vector3.RotateTowards(current, direction, maxRadians, 0f);
            current.y = 0f;
            if (current.sqrMagnitude <= 0.0001f) current = direction;
            current.Normalize();
            bot.FacingDirection = current;

            float yaw = Mathf.Atan2(current.x, current.z) * Mathf.Rad2Deg;
            // Local players publish CharacterBody.rotation, whose model-forward axis is
            // 90 degrees from gameplay forward. Preserve that exact vanilla convention.
            bot.BodyRotation = Quaternion.Euler(0f, yaw + 90f, 0f);
        }

        private bool TryChoosePatrolDestination(CNRArenaBotState bot, out Vector3 target, out float routeDistance)
        {
            target = bot != null ? bot.Position : Vector3.zero;
            routeDistance = 0f;
            if (bot == null) return false;

            bool navReady = _arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready;
            if (!navReady)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = UnityEngine.Random.Range(6f, 20f);
                target = bot.Position + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * distance;
                target.y = bot.Position.y;
                routeDistance = distance;
                return true;
            }

            float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
            Vector3 fromGround = bot.Position;
            fromGround.y -= bodyOffset;

            for (int attempt = 0; attempt < 12; attempt++)
            {
                // Patrol has deliberately mixed range. Most picks are useful medium
                // routes, with regular nearby checks and occasional long cross-map walks.
                float roll = UnityEngine.Random.value;
                float distance;
                if (roll < 0.32f) distance = UnityEngine.Random.Range(4f, 8f);
                else if (roll < 0.76f) distance = UnityEngine.Random.Range(9f, 18f);
                else distance = UnityEngine.Random.Range(20f, 34f);

                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector3 candidateGround = fromGround +
                    new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * distance;
                candidateGround.y = fromGround.y;

                Vector3 snapped;
                if (!CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                    candidateGround, 8, fromGround.y, NAV_HEIGHT_TOLERANCE, out snapped))
                    continue;

                Vector3 flat = snapped - fromGround;
                flat.y = 0f;
                if (flat.sqrMagnitude < 3f * 3f) continue;

                List<Vector3> path = CNRZombieMod.ZombieNavGrid.Query(fromGround, snapped);
                if (path == null || path.Count < 2) continue;

                float length = 0f;
                Vector3 previous = fromGround;
                for (int i = 0; i < path.Count; i++)
                {
                    length += Vector3.Distance(previous, path[i]);
                    previous = path[i];
                }
                if (length < 3f) continue;

                target = snapped;
                target.y += bodyOffset;
                routeDistance = length;
                return true;
            }
            return false;
        }

        private void WanderBot(CNRArenaBotState bot)
        {
            float now = Time.realtimeSinceStartup;
            if (bot == null) return;
            bot.Behavior = CNRArenaBotBehavior.Patrol;

            if (now >= bot.NextWanderAt || bot.WanderDirection.sqrMagnitude < 0.01f)
            {
                float routeDistance;
                Vector3 patrolTarget;
                if (!TryChoosePatrolDestination(bot, out patrolTarget, out routeDistance))
                {
                    bot.WanderUntil = now;
                    bot.NextWanderAt = now + UnityEngine.Random.Range(0.35f, 0.85f);
                    bot.Status = PlayerStatus.idle;
                    return;
                }

                bot.WanderTarget = patrolTarget;
                Vector3 initialDirection = patrolTarget - bot.Position;
                initialDirection.y = 0f;
                if (initialDirection.sqrMagnitude > 0.001f)
                    bot.WanderDirection = initialDirection.normalized;

                bot.NavPath = null;
                bot.NavPathIndex = 0;
                bot.NextNavRepathAt = 0f;
                // Commit to the chosen location long enough to actually reach it. Far
                // patrol picks used to time out after a few seconds and immediately choose
                // another heading, which looked like random left/right twitching.
                float travelBudget = routeDistance / Mathf.Max(0.1f, WANDER_SPEED);
                bot.WanderUntil = now + Mathf.Clamp(travelBudget * 1.8f + 3f, 5f, 32f);
                bot.NextWanderAt = bot.WanderUntil + UnityEngine.Random.Range(0.25f, 1.0f);
            }

            if (now > bot.WanderUntil)
            {
                bot.Status = PlayerStatus.idle;
                return;
            }

            Vector3 toTarget = bot.WanderTarget - bot.Position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.70f * 0.70f)
            {
                bot.WanderUntil = now;
                bot.NextWanderAt = now + UnityEngine.Random.Range(0.35f, 1.25f);
                bot.Status = PlayerStatus.idle;
                return;
            }

            Vector3 moveDir;
            if (!TryGetNavigationDirection(bot, bot.WanderTarget, out moveDir))
            {
                if (_arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready)
                {
                    bot.Status = PlayerStatus.idle;
                    bot.NextWanderAt = now + 0.35f;
                    return;
                }
                moveDir = toTarget.normalized;
            }
            moveDir = ApplyLocalSeparation(bot, moveDir);
            bot.WanderDirection = moveDir;
            FaceDirection(bot, moveDir);
            bot.GunRotation = Quaternion.identity;
            bot.FirePoint = bot.Position + new Vector3(0f, 1.15f, 0f) + moveDir * 0.45f;
            bot.Status = PlayerStatus.walk;
            MoveBot(bot, moveDir, WANDER_SPEED * THINK_INTERVAL);
        }

        private bool HasLineOfSight(Vector3 origin, Vector3 target, string targetId, float maxDistance)
        {
            Vector3 delta = target - origin;
            float len = delta.magnitude;
            if (len <= 0.05f) return true;
            RaycastHit hit;
            if (!Physics.Raycast(origin, delta / len, out hit, Mathf.Min(maxDistance, 65f), -21)) return true;
            if (hit.collider == null) return true;
            Transform t = hit.collider.transform;
            while (t != null)
            {
                NetPlayerController npc = t.GetComponent<NetPlayerController>();
                if (npc != null && npc.pInfo != null && npc.pInfo.mId == targetId) return true;
                if (t.gameObject.tag == "Player" && _mgr != null && _mgr.myPlayerInfo != null && _mgr.myPlayerInfo.mId == targetId) return true;
                t = t.parent;
            }
            return false;
        }

        private void DamageHuman(PlayerInfo target, int damage, string botId)
        {
            if (_mgr == null || target == null || string.IsNullOrEmpty(target.mId)) return;
            try
            {
                _humanLastBotAttacker[target.mId] = botId;
                _humanLastBotAttackAt[target.mId] = Time.realtimeSinceStartup;
                string payload = botId + "@" + damage;

                // sendMessageToPeersAdapt uses PhotonTargets.Others, so the authority
                // cannot target itself through that path. Apply host-local bot damage
                // directly and use the targeted RPC path for remote humans.
                if (_mgr.myPlayerInfo != null && target.mId == _mgr.myPlayerInfo.mId)
                {
                    if (PlayerLogic.mInstance != null)
                        PlayerLogic.mInstance.PlayerDamageStrOnline(payload);
                }
                else
                {
                    string[] peerIds = new string[] { target.mId };
                    _mgr.sendMessageToPeersAdapt(peerIds, "ExampleCharacter", "PlayerDamageStrOnline", payload, true);
                }
            }
            catch (Exception ex) { ModEntry.Log("ArenaBots human damage: " + ex.Message); }
        }

        private void DetectBotKillsOnHumans()
        {
            if (_mgr == null) return;
            CheckHumanDeath(_mgr.myPlayerInfo);
            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others == null) return;
            for (int i = 0; i < others.Length; i++)
                if (others[i] != null && !IsBotId(others[i].mId)) CheckHumanDeath(others[i]);
        }

        private void CheckHumanDeath(PlayerInfo p)
        {
            if (p == null || p.mId == "null" || IsBotId(p.mId)) return;
            PlayerStatus old;
            bool had = _humanLastStatus.TryGetValue(p.mId, out old);
            _humanLastStatus[p.mId] = p.mStatus;
            if (!had || old == PlayerStatus.dead || p.mStatus != PlayerStatus.dead) return;

            string botId;
            float at;
            if (!_humanLastBotAttacker.TryGetValue(p.mId, out botId) || !_humanLastBotAttackAt.TryGetValue(p.mId, out at)) return;
            if (Time.realtimeSinceStartup - at > 2.5f) return;
            CNRArenaBotState bot = FindBot(botId);
            if (bot == null) return;
            bot.Kills++;
            AddKcTeamKill(bot.Team);
        }

        private void AddKcTeamKill(TeamType team)
        {
            CNRMatchSettingsData rules = CNRMatchSettings.Active;
            if (rules == null || rules.Mode != "kc" || _mgr == null || _mgr.myModeInfo == null || _mgr.myModeInfo.mKillingCompetitionInfo == null) return;
            if (team == TeamType.Cop) _mgr.myModeInfo.mKillingCompetitionInfo.copKilling++;
            else if (team == TeamType.Robber) _mgr.myModeInfo.mKillingCompetitionInfo.robberKilling++;
        }

        private Vector3 FindSpawnPosition(TeamType team, CNRArenaBotState ignore)
        {
            // Vanilla CNR LocalOnline uses Spawn_1_1..4 for Cops and Spawn_2_1..4
            // for Robbers. Use the same scene spawn transforms instead of inventing a
            // position near an existing player, which can place bots inside geometry or
            // far away from the actual team spawn area. Scan a few extra indices so
            // custom maps can expose more spawn points while keeping the vanilla names.
            List<Vector3> candidates = new List<Vector3>();
            string prefix = team == TeamType.Cop ? "Spawn_1_" : "Spawn_2_";
            for (int i = 1; i <= 16; i++)
            {
                GameObject spawn = GameObject.Find(prefix + i);
                if (spawn != null) candidates.Add(spawn.transform.position);
            }

            if (candidates.Count > 0)
                return SnapBodyPositionToGround(candidates[UnityEngine.Random.Range(0, candidates.Count)]);

            // Compatibility fallback for maps that only expose generic multiplayer spawns.
            for (int i = 1; i <= 8; i++)
            {
                GameObject spawn = GameObject.Find("Spawn_" + i);
                if (spawn != null) candidates.Add(spawn.transform.position);
            }
            if (candidates.Count > 0)
                return SnapBodyPositionToGround(candidates[UnityEngine.Random.Range(0, candidates.Count)]);

            // Last resort: use a live teammate's exact authoritative position. Do not add
            // a random multi-metre offset because that was capable of putting bots outside
            // valid walkable space.
            if (_mgr != null)
            {
                PlayerInfo mine = _mgr.myPlayerInfo;
                if (mine != null && mine.mTeam == team && mine.mConnnectStatus == ConnectStatus.InGame)
                    return mine.mPosition;
                PlayerInfo[] others = _mgr.otherPlayersInfoList;
                if (others != null)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        PlayerInfo p = others[i];
                        if (p == null || IsBotId(p.mId) || p.mTeam != team || p.mConnnectStatus != ConnectStatus.InGame) continue;
                        return p.mPosition;
                    }
                }
            }

            ModEntry.Log("ArenaBots: no valid scene spawn found for " + team + "; using origin fallback");
            Vector3 fallback = new Vector3(0f, 2f, 0f);
            return SnapBodyPositionToGround(fallback);
        }

        private Vector3 SnapBodyPositionToGround(Vector3 position)
        {
            Vector3 grounded;
            float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
            if (_arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready)
            {
                if (CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(position, 6, position.y, NAV_HEIGHT_TOLERANCE, out grounded))
                {
                    position.x = grounded.x;
                    position.z = grounded.z;
                    position.y = grounded.y + bodyOffset;
                }
                else
                {
                    // Spawn transforms are floor anchors. If the height-aware nav lookup
                    // cannot find that floor, keep the anchor rather than raycasting onto
                    // an overhead/invisible ceiling.
                    position.y += bodyOffset;
                }
                return position;
            }

            if (TryProjectToWorldGround(position, out grounded))
                position.y = grounded.y + bodyOffset;
            return position;
        }

        private float GetBodyGroundOffset()
        {
            float now = Time.realtimeSinceStartup;
            if (now < _nextBodyGroundSampleAt) return _bodyGroundOffset;
            _nextBodyGroundSampleAt = now + 0.75f;

            // PlayerInfo.mPosition is the real CharacterBody position. Sample a live
            // human against world ground so virtual PlayerInfo uses the same vertical
            // convention on every map/prefab instead of guessing a model height.
            if (_mgr != null)
            {
                PlayerInfo p = _mgr.myPlayerInfo;
                if (p != null && !IsBotId(p.mId) && p.mConnnectStatus == ConnectStatus.InGame && p.mStatus != PlayerStatus.dead)
                {
                    Vector3 ground;
                    if (TryProjectToWorldGround(p.mPosition, out ground))
                    {
                        float offset = p.mPosition.y - ground.y;
                        if (offset >= 0.25f && offset <= 2.0f)
                        {
                            _bodyGroundOffset = offset;
                            return _bodyGroundOffset;
                        }
                    }
                }

                PlayerInfo[] others = _mgr.otherPlayersInfoList;
                if (others != null)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        p = others[i];
                        if (p == null || IsBotId(p.mId) || p.mConnnectStatus != ConnectStatus.InGame || p.mStatus == PlayerStatus.dead) continue;
                        Vector3 ground;
                        if (!TryProjectToWorldGround(p.mPosition, out ground)) continue;
                        float offset = p.mPosition.y - ground.y;
                        if (offset < 0.25f || offset > 2.0f) continue;
                        _bodyGroundOffset = offset;
                        return _bodyGroundOffset;
                    }
                }
            }
            return _bodyGroundOffset;
        }

        private static bool TryProjectToWorldGround(Vector3 position, out Vector3 groundPoint)
        {
            groundPoint = position;
            RaycastHit[] hits = Physics.RaycastAll(position + new Vector3(0f, 4f, 0f), Vector3.down, 10f, -21);
            if (hits == null || hits.Length == 0) return false;

            float bestDistance = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (IsPlayerOrCharacterCollider(hit.collider.transform)) continue;
                if (hit.normal.y < 0.25f) continue;
                if (hit.distance >= bestDistance) continue;
                bestDistance = hit.distance;
                groundPoint = hit.point;
                found = true;
            }
            return found;
        }

        private static bool IsPlayerOrCharacterCollider(Transform t)
        {
            while (t != null)
            {
                if (t.GetComponent<CharacterController>() != null) return true;
                if (t.GetComponent<NetPlayerController>() != null) return true;
                try { if (t.gameObject.tag == "Player") return true; } catch { }
                t = t.parent;
            }
            return false;
        }

        private void InjectPlayerInfos()
        {
            if (_mgr == null || _mgr.otherPlayersInfoList == null) return;
            PlayerInfo[] slots = _mgr.otherPlayersInfoList;

            // Remove stale bot entries first. Vanilla manager sees the slot go null-id
            // and destroys the associated remote player object on its next update.
            for (int i = 0; i < slots.Length; i++)
            {
                PlayerInfo p = slots[i];
                if (p == null || !IsBotId(p.mId)) continue;
                if (FindBot(p.mId) == null) slots[i] = new PlayerInfo();
            }

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState bot = _bots[i];
                int slot = FindSlotForBot(slots, bot.Id);
                if (slot < 0) continue;
                PlayerInfo p = slots[slot];
                if (p == null || p.mId == "null") p = new PlayerInfo(bot.Id, GetBotName(bot));
                ApplyBotToPlayerInfo(bot, p);
                slots[slot] = p;
                ResetVanillaTimeout(slot);
            }
        }

        private void ResetVanillaTimeout(int slot)
        {
            try
            {
                if (_timeoutCountsField == null)
                    _timeoutCountsField = typeof(CNRMultiplayerManager).GetField("otherPlayerTimeOutCount", BindingFlags.Instance | BindingFlags.NonPublic);
                int[] counts = _timeoutCountsField != null ? _timeoutCountsField.GetValue(_mgr) as int[] : null;
                if (counts != null && slot >= 0 && slot < counts.Length) counts[slot] = 0;
            }
            catch { }
        }

        private static int FindSlotForBot(PlayerInfo[] slots, string id)
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i] != null && slots[i].mId == id) return i;
            for (int i = 0; i < slots.Length; i++) if (slots[i] == null || slots[i].mId == "null") return i;
            return -1;
        }

        private void ApplyBotToPlayerInfo(CNRArenaBotState bot, PlayerInfo p)
        {
            p.mId = bot.Id;
            p.mNickName = GetBotName(bot);
            p.mTeam = bot.Team;
            if (IsFreeForAllMode() && _mgr != null && _mgr.myPlayerInfo != null)
            {
                // KC is FFA to the player even though the legacy mode still needs a
                // backing Cop/Robber value internally for its aggregate counters. Present
                // every bot as an enemy on each client instead of half green / half red.
                if (_mgr.myPlayerInfo.mTeam == TeamType.Cop) p.mTeam = TeamType.Robber;
                else if (_mgr.myPlayerInfo.mTeam == TeamType.Robber) p.mTeam = TeamType.Cop;
            }
            p.mConnnectStatus = ConnectStatus.InGame;
            p.mHp = bot.Hp;
            p.mKillNum = bot.Kills;
            p.mDeadNum = bot.Deaths;
            // NetPlayerController.SetPosition() adds +0.233m to all received player
            // positions. Bot.Position is already the authoritative body-world position,
            // so subtract that legacy display bias here and let vanilla add it back once.
            p.mPosition = bot.Position + new Vector3(0f, -NET_PLAYER_POSITION_Y_BIAS, 0f);
            p.mBodyRotation = bot.BodyRotation;
            p.mGunRotation = bot.GunRotation;
            p.mFirePointPos = bot.FirePoint;
            p.mStatus = bot.Status;
            p.mWeaponType = WeaponType.Deagle;
            p.mLv = 1;
            p.mSkinName = "Skin_1";
        }

        private void BindDamageReceivers()
        {
            NetPlayerController[] players;
            try { players = (NetPlayerController[])UnityEngine.Object.FindObjectsOfType(typeof(NetPlayerController)); }
            catch { return; }
            _botVisuals.Clear();
            for (int i = 0; i < players.Length; i++)
            {
                NetPlayerController npc = players[i];
                if (npc == null || npc.pInfo == null || !IsBotId(npc.pInfo.mId)) continue;
                _botVisuals[npc.pInfo.mId] = npc;
                AttachReceiver(npc.gameObject, npc.pInfo.mId);
                Collider[] cols = npc.GetComponentsInChildren<Collider>();
                for (int c = 0; c < cols.Length; c++)
                    if (cols[c] != null) AttachReceiver(cols[c].gameObject, npc.pInfo.mId);
            }
        }

        private static void AttachReceiver(GameObject go, string botId)
        {
            if (go == null) return;
            CNRArenaBotDamageReceiver r = go.GetComponent<CNRArenaBotDamageReceiver>();
            if (r == null) r = go.AddComponent<CNRArenaBotDamageReceiver>();
            r.BotId = botId;
        }

        private void BroadcastState()
        {
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["bot_state"] = PackState();
            ModEntry.RaiseFastEvent(ht);
        }

        private string PackState()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < _bots.Count; i++)
            {
                if (i > 0) sb.Append(';');
                CNRArenaBotState b = _bots[i];
                sb.Append(b.Id).Append(',').Append((int)b.Team).Append(',').Append(b.Hp).Append(',')
                    .Append(b.Kills).Append(',').Append(b.Deaths).Append(',').Append((int)b.Status).Append(',')
                    .Append(b.Position.x.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.Position.y.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.Position.z.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.BodyRotation.eulerAngles.y.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.HasLastKnownTarget ? b.LastTargetId : "").Append(',')
                    .Append(b.LastKnownTargetPosition.x.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.LastKnownTargetPosition.y.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(b.LastKnownTargetPosition.z.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append((b.HasLastKnownTarget ? Mathf.Max(0f, TARGET_MEMORY_SECONDS - (Time.realtimeSinceStartup - b.LastTargetVisibleAt)) : 0f).ToString("R", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private void ApplyPackedState(string packed)
        {
            List<CNRArenaBotState> incoming = new List<CNRArenaBotState>();
            if (!string.IsNullOrEmpty(packed))
            {
                string[] rows = packed.Split(';');
                for (int i = 0; i < rows.Length; i++)
                {
                    string[] p = rows[i].Split(',');
                    if (p.Length < 10) continue;
                    int team, hp, kills, deaths, status;
                    float x, y, z, yaw;
                    if (!int.TryParse(p[1], out team) || !int.TryParse(p[2], out hp) || !int.TryParse(p[3], out kills) ||
                        !int.TryParse(p[4], out deaths) || !int.TryParse(p[5], out status) ||
                        !float.TryParse(p[6], NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
                        !float.TryParse(p[7], NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
                        !float.TryParse(p[8], NumberStyles.Float, CultureInfo.InvariantCulture, out z) ||
                        !float.TryParse(p[9], NumberStyles.Float, CultureInfo.InvariantCulture, out yaw)) continue;
                    if (!IsBotId(p[0])) continue;
                    CNRArenaBotState b = new CNRArenaBotState();
                    b.Id = p[0]; b.Team = (TeamType)team; b.Hp = hp; b.Kills = kills; b.Deaths = deaths;
                    b.Status = (PlayerStatus)status; b.Position = new Vector3(x, y, z);
                    b.BodyRotation = Quaternion.Euler(0f, yaw, 0f); b.GunRotation = Quaternion.identity;
                    float facingYaw = (yaw - 90f) * Mathf.Deg2Rad;
                    b.FacingDirection = new Vector3(Mathf.Sin(facingYaw), 0f, Mathf.Cos(facingYaw)).normalized;
                    if (p.Length >= 15 && !string.IsNullOrEmpty(p[10]))
                    {
                        float lx, ly, lz, remaining;
                        if (float.TryParse(p[11], NumberStyles.Float, CultureInfo.InvariantCulture, out lx) &&
                            float.TryParse(p[12], NumberStyles.Float, CultureInfo.InvariantCulture, out ly) &&
                            float.TryParse(p[13], NumberStyles.Float, CultureInfo.InvariantCulture, out lz) &&
                            float.TryParse(p[14], NumberStyles.Float, CultureInfo.InvariantCulture, out remaining) && remaining > 0f)
                        {
                            b.LastTargetId = p[10];
                            b.LastKnownTargetPosition = new Vector3(lx, ly, lz);
                            b.HasLastKnownTarget = true;
                            b.LastTargetVisibleAt = Time.realtimeSinceStartup - Mathf.Max(0f, TARGET_MEMORY_SECONDS - Mathf.Min(TARGET_MEMORY_SECONDS, remaining));
                        }
                    }
                    incoming.Add(b);
                }
            }
            _bots.Clear();
            _bots.AddRange(incoming);
        }

        private void ApplyDamageToBot(string botId, int damage, string attackerBotId, int attackerActorId)
        {
            if (!IsAuthority()) return;
            CNRArenaBotState bot = FindBot(botId);
            if (bot == null || bot.Status == PlayerStatus.dead || bot.Hp <= 0) return;
            CNRArenaBotState attackerBot = !string.IsNullOrEmpty(attackerBotId) ? FindBot(attackerBotId) : null;
            PlayerInfo attackerInfo = null;
            if (attackerBot != null && !IsFreeForAllMode() && attackerBot.Team == bot.Team) return;
            if (attackerBot == null && attackerActorId > 0)
            {
                attackerInfo = FindHumanByActor(attackerActorId);
                if (attackerInfo != null && !IsFreeForAllMode() && attackerInfo.mTeam == bot.Team) return;
            }

            if (attackerBot != null)
                RegisterDamageThreat(bot, attackerBot.Id, attackerBot.Position);
            else if (attackerInfo != null)
                RegisterDamageThreat(bot, attackerInfo.mId, attackerInfo.mPosition);

            bot.Hp -= Mathf.Max(1, damage);
            if (bot.Hp > 0) return;
            bot.Hp = 0;
            bot.Status = PlayerStatus.dead;
            bot.Deaths++;
            bot.RespawnAt = Time.realtimeSinceStartup + RESPAWN_SECONDS;

            if (attackerBot != null)
            {
                attackerBot.Kills++;
                AddKcTeamKill(attackerBot.Team);
            }
            else if (attackerActorId > 0)
            {
                // RaiseCnrEvent is sent to other peers. If the master itself killed the
                // bot, sending bot_credit would never loop back to the local player, so
                // award it directly. Remote attackers still receive the targeted credit
                // through the normal event path.
                bool localAttacker = false;
                try { localAttacker = PhotonNetwork.player != null && PhotonNetwork.player.ID == attackerActorId; }
                catch { }
                if (localAttacker) AwardLocalBotKill(bot.Id, damage);
                else SendKillCredit(attackerActorId, bot.Id, damage);
            }
            BroadcastState();
        }

        private void RegisterDamageThreat(CNRArenaBotState bot, string attackerId, Vector3 attackerPosition)
        {
            if (bot == null || string.IsNullOrEmpty(attackerId)) return;
            float now = Time.realtimeSinceStartup;

            // Do not make damage magically interrupt a target the bot is actively seeing.
            // If its current target is hidden/stale, however, being shot is a strong enough
            // sensory cue to redirect the search toward the attacker.
            PlayerInfo currentInfo;
            CNRArenaBotState currentBot;
            if (!string.IsNullOrEmpty(bot.LastTargetId) &&
                TryResolveEnemyTarget(bot, bot.LastTargetId, out currentInfo, out currentBot))
            {
                Vector3 currentPosition = currentBot != null ? currentBot.Position : currentInfo.mPosition;
                float currentDistance = Vector3.Distance(bot.Position, currentPosition);
                if (bot.LastTargetId != attackerId &&
                    CanSeeTarget(bot, currentPosition, bot.LastTargetId, currentDistance + 1f))
                    return;
            }

            // A hit tells the victim where the threat roughly came from, not the attacker's
            // exact network coordinate. Add a little horizontal uncertainty so this never
            // becomes x-ray tracking through walls.
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float error = UnityEngine.Random.Range(0.6f, 2.2f);
            Vector3 cuePosition = attackerPosition +
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * error;

            bool changed = bot.LastTargetId != attackerId;
            bot.LastTargetId = attackerId;
            bot.LastKnownTargetPosition = cuePosition;
            bot.LastTargetVisibleAt = now;
            bot.HasLastKnownTarget = true;
            bot.TargetCertainty = Mathf.Max(bot.TargetCertainty, 0.30f);
            bot.ReactionReadyAt = changed
                ? now + UnityEngine.Random.Range(0.12f, 0.25f)
                : Mathf.Min(bot.ReactionReadyAt, now + 0.12f);
            bot.CurrentTargetScore = Mathf.Max(bot.CurrentTargetScore, 0.35f);
            bot.Behavior = CNRArenaBotBehavior.Search;
            bot.TacticalTarget = bot.Position;
            bot.TacticalScore = -1f;
            bot.TacticalCommittedUntil = 0f;
            bot.NextRepositionAt = 0f;
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
        }

        private PlayerInfo FindHumanByActor(int actorId)
        {
            string id = actorId.ToString(CultureInfo.InvariantCulture);
            if (_mgr != null && _mgr.myPlayerInfo != null && _mgr.myPlayerInfo.mId == id) return _mgr.myPlayerInfo;
            if (_mgr != null && _mgr.otherPlayersInfoList != null)
                for (int i = 0; i < _mgr.otherPlayersInfoList.Length; i++)
                    if (_mgr.otherPlayersInfoList[i] != null && _mgr.otherPlayersInfoList[i].mId == id) return _mgr.otherPlayersInfoList[i];
            return null;
        }

        private void SendKillCredit(int actorId, string botId, int damage)
        {
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["bot_credit"] = actorId.ToString(CultureInfo.InvariantCulture) + "|" + botId + "|" + damage.ToString(CultureInfo.InvariantCulture);
            ModEntry.RaiseCnrEvent(ht);
        }

        private CNRArenaBotState FindBot(string id)
        {
            for (int i = 0; i < _bots.Count; i++) if (_bots[i].Id == id) return _bots[i];
            return null;
        }

        private static string GetBotName(CNRArenaBotState bot)
        {
            int n;
            if (!int.TryParse(bot.Id, out n)) n = BOT_ID_BASE;
            n = (n - BOT_ID_BASE) + 1;
            if (IsFreeForAllMode()) return "[BOT] Player " + n;
            return bot.Team == TeamType.Cop ? "[BOT] Officer " + n : "[BOT] Robber " + n;
        }

        internal static bool IsBotId(string id)
        {
            int n;
            return int.TryParse(id, out n) && n >= BOT_ID_BASE && n < BOT_ID_BASE + MAX_BOTS;
        }

        public static void ReceiveState(string packed, int senderId)
        {
            if (!IsCurrentMasterSender(senderId)) return;
            if (_instance != null && _instance.IsAuthority()) return;
            _pendingPackedState = packed ?? "";
            _pendingPackedStateSender = senderId;
        }

        public static void ReceiveHit(string raw, int senderId)
        {
            if (_instance == null || !_instance.IsAuthority() || string.IsNullOrEmpty(raw)) return;
            string[] p = raw.Split('|');
            int damage;
            if (p.Length < 2 || !int.TryParse(p[1], out damage)) return;
            _instance.ApplyDamageToBot(p[0], damage, null, senderId);
        }

        public static void ReceiveCredit(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;
            string[] p = raw.Split('|');
            int actor, damage;
            if (p.Length < 3 || !int.TryParse(p[0], out actor) || !int.TryParse(p[2], out damage)) return;
            try
            {
                if (PhotonNetwork.player == null || PhotonNetwork.player.ID != actor) return;
                AwardLocalBotKill(p[1], damage);
            }
            catch (Exception ex) { ModEntry.Log("ArenaBots credit: " + ex.Message); }
        }

        private static void AwardLocalBotKill(string botId, int damage)
        {
            PlayerLogic logic = PlayerLogic.mInstance;
            CNRMultiplayerManager mgr = CNRMultiplayerManager.mInstance;
            if (logic == null || mgr == null) return;

            // Bot kills deliberately bypass vanilla AddOneKill. Vanilla would award
            // 3 XP + 3 coins and the CNR kill-delta hook would add another 3 XP.
            CNRXpRewardHook.SuppressBotKillBonus(1);
            logic.killedNum++;
            mgr.myPlayerInfo.mKillNum = logic.killedNum;

            try { mgr.BroadcastKillEventMessageOnlineAdapt(botId); } catch { }
            try { mgr.SendKillingCompetitionGetMessage(); } catch { }

            GrowthManagerKit.AddCharacterExp(1);
            bool hardline = CNRPerkSystem.HasPerk("hardline");
            if (hardline) GrowthManagerKit.AddCoins(1);

            try
            {
                if (_instance != null)
                {
                    CNRArenaBotState bot = _instance.FindBot(botId);
                    if (bot != null) CNRPerkSystem.TrySpawnKillDrops(bot.Position + Vector3.up * 0.25f);
                }
            }
            catch { }

            ModEntry.Log("ArenaBots: local bot kill " + botId + " reward=1xp coins=" + (hardline ? "1" : "0"));
        }

        public static void ReportHit(string botId, int damage)
        {
            if (string.IsNullOrEmpty(botId) || damage <= 0) return;
            if (_instance != null && _instance.IsAuthority())
            {
                int actor = 0;
                try { if (PhotonNetwork.player != null) actor = PhotonNetwork.player.ID; } catch { }
                _instance.ApplyDamageToBot(botId, damage, null, actor);
                return;
            }
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            ht["bot_hit"] = botId + "|" + damage.ToString(CultureInfo.InvariantCulture);
            ModEntry.RaiseCnrEvent(ht);
        }

        public static void ClearRoomState()
        {
            _pendingPackedState = null;
            _pendingPackedStateSender = 0;
            _pendingClear = true;
            if (_instance != null) _instance.ResetLocalBots();
        }

        private void ResetLocalBots()
        {
            if (_mgr == null) _mgr = CNRMultiplayerManager.mInstance;
            if (_mgr != null && _mgr.otherPlayersInfoList != null)
            {
                for (int i = 0; i < _mgr.otherPlayersInfoList.Length; i++)
                    if (_mgr.otherPlayersInfoList[i] != null && IsBotId(_mgr.otherPlayersInfoList[i].mId))
                        _mgr.otherPlayersInfoList[i] = new PlayerInfo();
            }
            _bots.Clear();
            _botVisuals.Clear();
            _humanLastStatus.Clear();
            _humanLastBotAttacker.Clear();
            _humanLastBotAttackAt.Clear();
        }
    }

    // Added to every collider on a vanilla remote-player prefab that represents a
    // virtual bot. The stock NetPlayerController still runs (for visuals), while this
    // receiver forwards actual player damage to the bot authority.
    public class CNRArenaBotDamageReceiver : MonoBehaviour
    {
        public string BotId = "";
        public void OnDamaged(int damage)
        {
            // Bot visuals do not use the vanilla EnemyTag hierarchy, so the generic
            // bullet probe cannot identify them as damage targets. This receiver is
            // invoked by the local player's real hit path; report the same damage here.
            CNRDamageNumbers.Report(transform, transform.position + Vector3.up * 0.9f, damage, false);
            CNRArenaBotManager.ReportHit(BotId, damage);
        }
    }
}
