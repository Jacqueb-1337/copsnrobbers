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
        private bool _wasActive;
        private FieldInfo _timeoutCountsField;

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

        private bool IsAuthority()
        {
            try { return PhotonNetwork.isMasterClient; }
            catch { return false; }
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
            PlayerInfo targetInfo;
            CNRArenaBotState targetBot;
            FindNearestEnemy(bot, out targetInfo, out targetBot);
            Vector3 targetPos;
            string targetId;
            if (targetBot != null) { targetPos = targetBot.Position; targetId = targetBot.Id; }
            else if (targetInfo != null) { targetPos = targetInfo.mPosition; targetId = targetInfo.mId; }
            else
            {
                bot.Status = PlayerStatus.idle;
                bot.LastTargetId = "";
                return;
            }

            bot.LastTargetId = targetId;
            Vector3 flat = targetPos - bot.Position;
            flat.y = 0f;
            float dist = flat.magnitude;
            if (dist < 0.05f) return;
            Vector3 dir = flat / dist;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            bot.BodyRotation = Quaternion.Euler(0f, yaw, 0f);
            bot.GunRotation = Quaternion.Euler(0f, yaw, 0f);
            bot.FirePoint = bot.Position + new Vector3(0f, 1.15f, 0f) + dir * 0.45f;

            bool visible = HasLineOfSight(bot.Position + new Vector3(0f, 1.15f, 0f), targetPos + new Vector3(0f, 0.9f, 0f), targetId, dist + 1f);
            float speed = 3.4f;
            Vector3 move = Vector3.zero;

            if (!visible || dist > 14f)
            {
                move = dir;
                bot.Status = PlayerStatus.walk;
            }
            else if (dist < 3.2f)
            {
                move = -dir;
                bot.Status = PlayerStatus.walk;
            }
            else
            {
                // Same tactical idea as the improved single-player AI: keep pressure
                // while strafing instead of standing still and trading shots.
                float sideSign = ((int.Parse(bot.Id) & 1) == 0) ? 1f : -1f;
                move = new Vector3(-dir.z, 0f, dir.x) * sideSign * 0.65f;
                bot.Status = PlayerStatus.fire;
            }

            if (move.sqrMagnitude > 0.01f)
                MoveBot(bot, move.normalized, speed * THINK_INTERVAL);

            if (visible && dist <= 22f && Time.realtimeSinceStartup >= bot.NextShotAt)
            {
                bot.Status = PlayerStatus.fire;
                bot.NextShotAt = Time.realtimeSinceStartup + UnityEngine.Random.Range(0.32f, 0.55f);
                int damage = UnityEngine.Random.Range(12, 23);
                if (targetBot != null)
                    ApplyDamageToBot(targetBot.Id, damage, bot.Id, 0);
                else
                    DamageHuman(targetInfo, damage, bot.Id);
            }
        }

        private void MoveBot(CNRArenaBotState bot, Vector3 desired, float amount)
        {
            Vector3 origin = bot.Position + new Vector3(0f, 0.9f, 0f);
            RaycastHit wall;
            Vector3 move = desired;
            if (Physics.Raycast(origin, desired, out wall, 1.25f, -21))
            {
                Vector3 side = new Vector3(-desired.z, 0f, desired.x);
                if ((int.Parse(bot.Id) & 1) != 0) side = -side;
                move = side;
            }

            Vector3 next = bot.Position + move * amount;
            RaycastHit ground;
            if (Physics.Raycast(next + new Vector3(0f, 4f, 0f), Vector3.down, out ground, 9f, -21))
                next.y = ground.point.y + 0.05f;

            bot.Position = next;
            float moved = Vector3.Distance(bot.LastMoveSample, bot.Position);
            if (moved > 0.30f)
            {
                bot.LastMoveSample = bot.Position;
                bot.LastMovedAt = Time.realtimeSinceStartup;
            }
            else if (Time.realtimeSinceStartup - bot.LastMovedAt > 1.5f)
            {
                // Stuck recovery carried over from the improved AI behavior.
                Vector3 side = new Vector3(-move.z, 0f, move.x);
                bot.Position += side * 1.5f;
                bot.LastMoveSample = bot.Position;
                bot.LastMovedAt = Time.realtimeSinceStartup;
            }
        }

        private void FindNearestEnemy(CNRArenaBotState bot, out PlayerInfo bestInfo, out CNRArenaBotState bestBot)
        {
            bestInfo = null;
            bestBot = null;
            float best = float.MaxValue;

            if (_mgr.myPlayerInfo != null)
                ConsiderHumanTarget(bot, _mgr.myPlayerInfo, ref bestInfo, ref best);
            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    PlayerInfo p = others[i];
                    if (p == null || IsBotId(p.mId)) continue;
                    ConsiderHumanTarget(bot, p, ref bestInfo, ref best);
                }
            }

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState b = _bots[i];
                if (b == bot || b.Team == bot.Team || b.Status == PlayerStatus.dead || b.Hp <= 0) continue;
                float d = (b.Position - bot.Position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestInfo = null;
                    bestBot = b;
                }
            }
        }

        private static void ConsiderHumanTarget(CNRArenaBotState bot, PlayerInfo p, ref PlayerInfo best, ref float bestDist)
        {
            if (p == null || p.mId == "null" || p.mTeam == TeamType.Nil || p.mTeam == bot.Team) return;
            if (p.mConnnectStatus != ConnectStatus.InGame || p.mStatus == PlayerStatus.dead || p.mHp <= 0) return;
            float d = (p.mPosition - bot.Position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = p; }
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
            List<Vector3> candidates = new List<Vector3>();
            if (_mgr != null)
            {
                PlayerInfo mine = _mgr.myPlayerInfo;
                if (mine != null && mine.mTeam == team && mine.mConnnectStatus == ConnectStatus.InGame) candidates.Add(mine.mPosition);
                PlayerInfo[] others = _mgr.otherPlayersInfoList;
                if (others != null)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        PlayerInfo p = others[i];
                        if (p == null || IsBotId(p.mId) || p.mTeam != team || p.mConnnectStatus != ConnectStatus.InGame) continue;
                        candidates.Add(p.mPosition);
                    }
                }
            }
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState b = _bots[i];
                if (b != ignore && b.Team == team && b.Hp > 0) candidates.Add(b.Position);
            }

            Vector3 basePos = candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : new Vector3(0f, 2f, 0f);
            Vector3 pos = basePos + new Vector3(UnityEngine.Random.Range(-3.5f, 3.5f), 0f, UnityEngine.Random.Range(-3.5f, 3.5f));
            RaycastHit hit;
            if (Physics.Raycast(pos + new Vector3(0f, 6f, 0f), Vector3.down, out hit, 15f, -21)) pos.y = hit.point.y + 0.05f;
            return pos;
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

        private static void ApplyBotToPlayerInfo(CNRArenaBotState bot, PlayerInfo p)
        {
            p.mId = bot.Id;
            p.mNickName = GetBotName(bot);
            p.mTeam = bot.Team;
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
                    .Append(b.BodyRotation.eulerAngles.y.ToString("R", CultureInfo.InvariantCulture));
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
                    b.BodyRotation = Quaternion.Euler(0f, yaw, 0f); b.GunRotation = b.BodyRotation;
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
            if (attackerBot != null && attackerBot.Team == bot.Team) return;
            if (attackerBot == null && attackerActorId > 0)
            {
                PlayerInfo attackerInfo = FindHumanByActor(attackerActorId);
                if (attackerInfo != null && attackerInfo.mTeam == bot.Team) return;
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
                SendKillCredit(attackerActorId, bot.Id, damage);
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
