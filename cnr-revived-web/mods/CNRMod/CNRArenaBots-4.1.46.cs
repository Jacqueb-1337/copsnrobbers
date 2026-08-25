using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace CNRMods
{
    internal class CNRArenaBotState
    {
        public string Id;
        public TeamType Team;
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
        public float NextThinkAt;
        public float LastMovedAt;
        public Vector3 LastMoveSample;
        public string LastTargetId = "";
        public Vector3 LastKnownTargetPosition;
        public float LastTargetVisibleAt;
        public bool HasLastKnownTarget;
        public Vector3 WanderDirection;
        public Vector3 WanderTarget;
        public float WanderUntil;
        public float NextWanderAt;
        public List<Vector3> NavPath;
        public int NavPathIndex;
        public float NextNavRepathAt;
        public Vector3 NavTarget;
        public Vector3 NavWaypoint;
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
        private const float BOT_GROUND_EXTRA_LIFT = 0.22f;
        private const float BOT_VISION_HALF_ANGLE = 55f;
        private const float WANDER_SPEED = 1.85f;
        private const float NAV_REPATH_INTERVAL = 0.75f;
        private const float NAV_TARGET_REPATH_DISTANCE = 1.0f;
        private const float NAV_HEIGHT_TOLERANCE = 1.15f;
        private const float NAV_CLIMB_RATE = 3.8f;
        private const float NAV_DESCEND_RATE = 7.0f;

        private static CNRArenaBotManager _instance;
        private static string _pendingPackedState;
        private static int _pendingPackedStateSender;
        private static bool _pendingClear;

        private readonly List<CNRArenaBotState> _bots = new List<CNRArenaBotState>();
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

            // Keep the current target only while it is still a valid enemy. Its live
            // position is consulted solely to test visibility; when occluded we pursue
            // the last position actually seen, never the target's current x-ray position.
            if (!string.IsNullOrEmpty(targetId) && TryResolveEnemyTarget(bot, targetId, out targetInfo, out targetBot))
            {
                Vector3 actual = targetBot != null ? targetBot.Position : targetInfo.mPosition;
                float actualDist = Vector3.Distance(bot.Position, actual);
                visible = CanSeeTarget(bot, actual, targetId, actualDist + 1f);
                if (visible)
                {
                    targetPos = actual;
                    bot.LastKnownTargetPosition = actual;
                    bot.LastTargetVisibleAt = now;
                    bot.HasLastKnownTarget = true;
                }
            }
            else
            {
                targetId = "";
                bot.LastTargetId = "";
                bot.HasLastKnownTarget = false;
                targetInfo = null;
                targetBot = null;
            }

            // A visible enemy is allowed to replace a hidden remembered target. This
            // keeps bots responsive to somebody who actually walks into view instead of
            // tunnel-visioning through a wall toward stale information.
            if (!visible)
            {
                PlayerInfo visibleInfo;
                CNRArenaBotState visibleBot;
                FindNearestVisibleEnemy(bot, out visibleInfo, out visibleBot);
                if (visibleBot != null || visibleInfo != null)
                {
                    targetBot = visibleBot;
                    targetInfo = visibleInfo;
                    targetId = visibleBot != null ? visibleBot.Id : visibleInfo.mId;
                    targetPos = visibleBot != null ? visibleBot.Position : visibleInfo.mPosition;
                    bot.LastTargetId = targetId;
                    bot.LastKnownTargetPosition = targetPos;
                    bot.LastTargetVisibleAt = now;
                    bot.HasLastKnownTarget = true;
                    visible = true;
                }
                else if (bot.HasLastKnownTarget && !string.IsNullOrEmpty(bot.LastTargetId))
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

            // Choose a tactical destination first, then let the shared zombie A* grid
            // decide how to reach it. This prevents the previous hybrid behavior where
            // retreat/strafe movement bypassed the nav path and could make bots orbit or
            // oscillate around a waypoint.
            if (!visible || dist > 14f)
            {
                moveTarget = targetPos;
                wantsMove = true;
                bot.Status = PlayerStatus.walk;
            }
            else if (dist < 3.2f)
            {
                moveTarget = bot.Position - dir * 4.0f;
                wantsMove = true;
                bot.Status = PlayerStatus.walk;
            }
            else
            {
                float sideSign = ((int.Parse(bot.Id) & 1) == 0) ? 1f : -1f;
                Vector3 side = new Vector3(-dir.z, 0f, dir.x) * sideSign;
                moveTarget = bot.Position + side * 4.0f;
                wantsMove = true;
                bot.Status = PlayerStatus.fire;
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

            // When pathing around the map, face the path rather than constantly twisting
            // toward a target through walls. At combat range the bot still faces the
            // visible enemy while strafing/backing away.
            Vector3 facingDir = visible && dist <= 14f ? dir : (move.sqrMagnitude > 0.01f ? move.normalized : dir);
            FaceDirection(bot, facingDir);
            bot.GunRotation = Quaternion.identity;
            bot.FirePoint = bot.Position + new Vector3(0f, 1.15f, 0f) + facingDir * 0.45f;

            if (move.sqrMagnitude > 0.01f)
                MoveBot(bot, move.normalized, speed * THINK_INTERVAL);

            if (visible && dist <= 22f && now >= bot.NextShotAt)
            {
                bot.Status = PlayerStatus.fire;
                bot.NextShotAt = now + UnityEngine.Random.Range(0.36f, 0.62f);

                // Bots should feel fallible rather than hitscan-perfect. Accuracy falls
                // off with range and gets a small penalty while actively moving.
                float rangeT = Mathf.InverseLerp(3f, 22f, dist);
                float hitChance = Mathf.Lerp(0.84f, 0.52f, rangeT);
                if (move.sqrMagnitude > 0.05f) hitChance -= 0.06f;
                hitChance = Mathf.Clamp(hitChance, 0.42f, 0.88f);

                if (UnityEngine.Random.value <= hitChance)
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

            bot.NavWaypoint = waypoint;
            Vector3 toWaypoint = waypoint - fromGround;
            toWaypoint.y = 0f;
            if (toWaypoint.sqrMagnitude <= 0.0001f) return false;
            direction = toWaypoint.normalized;
            return true;
        }

        private void MoveBot(CNRArenaBotState bot, Vector3 desired, float amount)
        {
            if (bot == null) return;
            desired.y = 0f;
            if (desired.sqrMagnitude <= 0.0001f) return;
            desired.Normalize();

            Vector3 move = desired;
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
            }
            else if (Time.realtimeSinceStartup - bot.LastMovedAt > 1.5f)
            {
                bot.NavPath = null;
                bot.NavPathIndex = 0;
                bot.NextNavRepathAt = 0f;
                if (!navReady)
                {
                    Vector3 side = new Vector3(-move.z, 0f, move.x).normalized;
                    bot.Position += side * 0.75f;
                }
                bot.LastMoveSample = bot.Position;
                bot.LastMovedAt = Time.realtimeSinceStartup;
            }
        }

        private static void ClearTargetMemory(CNRArenaBotState bot)
        {
            if (bot == null) return;
            bot.LastTargetId = "";
            bot.LastKnownTargetPosition = Vector3.zero;
            bot.LastTargetVisibleAt = 0f;
            bot.HasLastKnownTarget = false;
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

        private static Vector3 GetBotForward(CNRArenaBotState bot)
        {
            float yaw = bot.BodyRotation.eulerAngles.y - 90f;
            float rad = yaw * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;
        }

        private static void FaceDirection(CNRArenaBotState bot, Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f) return;
            direction.Normalize();
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            bot.BodyRotation = Quaternion.Euler(0f, yaw + 90f, 0f);
        }

        private void WanderBot(CNRArenaBotState bot)
        {
            float now = Time.realtimeSinceStartup;
            if (bot == null) return;

            if (now >= bot.NextWanderAt || bot.WanderDirection.sqrMagnitude < 0.01f)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                bot.WanderDirection = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 candidate = bot.Position + bot.WanderDirection * UnityEngine.Random.Range(4f, 10f);
                candidate.y = bot.Position.y;

                Vector3 snapped;
                if (_arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready)
                {
                    float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
                    Vector3 candidateGround = candidate;
                    candidateGround.y -= bodyOffset;
                    float currentGroundY = bot.Position.y - bodyOffset;
                    if (CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(candidateGround, 16, currentGroundY, NAV_HEIGHT_TOLERANCE, out snapped))
                    {
                        snapped.y += bodyOffset;
                        bot.WanderTarget = snapped;
                    }
                    else
                    {
                        bot.WanderUntil = now;
                        bot.NextWanderAt = now + 0.35f;
                        bot.Status = PlayerStatus.idle;
                        return;
                    }
                }
                else
                    bot.WanderTarget = candidate;

                bot.NavPath = null;
                bot.NavPathIndex = 0;
                bot.NextNavRepathAt = 0f;
                bot.WanderUntil = now + UnityEngine.Random.Range(2.5f, 5.5f);
                bot.NextWanderAt = bot.WanderUntil + UnityEngine.Random.Range(0.35f, 1.25f);
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
            p.mPosition = bot.Position;
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
            for (int i = 0; i < players.Length; i++)
            {
                NetPlayerController npc = players[i];
                if (npc == null || npc.pInfo == null || !IsBotId(npc.pInfo.mId)) continue;
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
            if (attackerBot != null && !IsFreeForAllMode() && attackerBot.Team == bot.Team) return;
            if (attackerBot == null && attackerActorId > 0)
            {
                PlayerInfo attackerInfo = FindHumanByActor(attackerActorId);
                if (attackerInfo != null && !IsFreeForAllMode() && attackerInfo.mTeam == bot.Team) return;
            }

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
            CNRArenaBotManager.ReportHit(BotId, damage);
        }
    }
}
