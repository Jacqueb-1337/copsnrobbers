using System;
using System.Collections.Generic;
using UnityEngine;

namespace CNRMods
{
    internal partial class CNRArenaBotState
    {
        public float NextStrategicReportAt;
        public int StrategicOrderRevision = -1;
        public Vector3 StrategicTarget;
    }

    internal sealed class CNRArenaStrategicHotspot
    {
        public TeamType KnowledgeTeam;
        public Vector3 Position;
        public float Weight;
        public float LastUpdatedAt;
    }

    internal sealed class CNRArenaSquadOrder
    {
        public int SquadId;
        public TeamType Team;
        public Vector3 Destination;
        public float Score;
        public float CommittedUntil;
        public float LastAssignedAt;
        public int Revision;
    }

    public partial class CNRArenaBotManager
    {
        // The commander is deliberately much slower than the individual brains. It only
        // answers "where should this squad be?"; perception/aim/movement remain 10 Hz.
        private const float COMMANDER_INTERVAL = 0.65f;
        private const float COMMANDER_CONTACT_LIFETIME = 14.0f;
        private const float COMMANDER_CONTACT_MERGE_RADIUS = 6.0f;
        private const float COMMANDER_REPORT_INTERVAL = 0.80f;
        private const float COMMANDER_ORDER_MIN_SECONDS = 6.0f;
        private const float COMMANDER_ORDER_MAX_SECONDS = 9.0f;
        private const float COMMANDER_SWITCH_ADVANTAGE = 1.45f;
        private const float COMMANDER_POST_COMMIT_ADVANTAGE = 1.16f;
        private const float COMMANDER_SLOT_SPACING = 2.75f;
        private const int COMMANDER_MAX_HOTSPOTS = 12;
        private const int COMMANDER_MAX_STRATEGIC_NODES = 28;

        private readonly List<CNRArenaStrategicHotspot> _commanderHotspots =
            new List<CNRArenaStrategicHotspot>();
        private readonly Dictionary<int, CNRArenaSquadOrder> _commanderOrders =
            new Dictionary<int, CNRArenaSquadOrder>();
        private readonly List<Vector3> _commanderNodes = new List<Vector3>();

        private float _nextCommanderAt;
        private string _commanderScene = "";
        private int _commanderRevision;
        private Vector3 _commanderCenter;
        private Vector3 _commanderCopSpawn;
        private Vector3 _commanderRobberSpawn;
        private bool _commanderHasCopSpawn;
        private bool _commanderHasRobberSpawn;
        private bool _commanderUpdateRobbers;

        private void ResetCommanderState()
        {
            _commanderHotspots.Clear();
            _commanderOrders.Clear();
            _commanderNodes.Clear();
            _commanderScene = "";
            _nextCommanderAt = 0f;
            _commanderRevision = 0;
            _commanderCenter = Vector3.zero;
            _commanderHasCopSpawn = false;
            _commanderHasRobberSpawn = false;
            _commanderUpdateRobbers = false;
        }

        private void UpdateArenaCommander(CNRMatchSettingsData rules)
        {
            if (rules == null || rules.Mode != "tdm" || IsFreeForAllMode())
            {
                if (_commanderOrders.Count > 0 || _commanderHotspots.Count > 0)
                {
                    _commanderOrders.Clear();
                    _commanderHotspots.Clear();
                }
                return;
            }

            if (_arenaNavScene != _scene || !CNRZombieMod.ZombieNavGrid.Ready) return;
            EnsureCommanderStrategicNodes();
            if (_commanderNodes.Count == 0) return;

            float now = Time.realtimeSinceStartup;
            if (now < _nextCommanderAt) return;
            _nextCommanderAt = now + COMMANDER_INTERVAL;

            PruneCommanderHotspots(now);
            // Split Commander work across ticks so both teams do not rescore their full
            // strategic node/hotspot sets on the same rendered frame.
            if (_commanderUpdateRobbers)
                AssignCommanderOrdersForTeam(TeamType.Robber, now);
            else
                AssignCommanderOrdersForTeam(TeamType.Cop, now);
            _commanderUpdateRobbers = !_commanderUpdateRobbers;
            PruneUnusedCommanderOrders();
        }

        private void EnsureCommanderStrategicNodes()
        {
            if (_commanderScene == _scene && _commanderNodes.Count > 0) return;

            _commanderNodes.Clear();
            _commanderOrders.Clear();
            _commanderHotspots.Clear();
            _commanderScene = _scene;
            _commanderCenter = FindArenaNavCenter();

            Vector3 spawn;
            _commanderHasCopSpawn = TryGetTeamSpawnCentroid(TeamType.Cop, out spawn);
            if (_commanderHasCopSpawn) _commanderCopSpawn = spawn;
            _commanderHasRobberSpawn = TryGetTeamSpawnCentroid(TeamType.Robber, out spawn);
            if (_commanderHasRobberSpawn) _commanderRobberSpawn = spawn;

            AddCommanderNode(_commanderCenter);
            if (_commanderHasCopSpawn) AddCommanderNode(_commanderCopSpawn);
            if (_commanderHasRobberSpawn) AddCommanderNode(_commanderRobberSpawn);

            // A small deterministic sector set is enough for strategy. These are projected
            // onto the already-baked nav grid once per scene; no A* runs here.
            float[] radii = new float[] { 10f, 20f, 31f };
            for (int r = 0; r < radii.Length; r++)
            {
                int directions = r == 2 ? 8 : 6;
                for (int i = 0; i < directions; i++)
                {
                    float angle = (Mathf.PI * 2f * i / directions) + (r * 0.31f);
                    Vector3 candidate = _commanderCenter +
                        new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radii[r];
                    candidate.y = _commanderCenter.y;
                    AddCommanderNode(candidate);
                    if (_commanderNodes.Count >= COMMANDER_MAX_STRATEGIC_NODES) break;
                }
                if (_commanderNodes.Count >= COMMANDER_MAX_STRATEGIC_NODES) break;
            }

            // Team spawn transforms themselves often sit on useful lanes/doorways. Keep a
            // few of them so the commander can stage a squad on either side of the map.
            AddSpawnNodes(TeamType.Cop, 4);
            AddSpawnNodes(TeamType.Robber, 4);
        }

        private bool TryGetTeamSpawnCentroid(TeamType team, out Vector3 centroid)
        {
            centroid = Vector3.zero;
            string prefix = team == TeamType.Cop ? "Spawn_1_" : "Spawn_2_";
            int count = 0;
            for (int i = 1; i <= 16; i++)
            {
                GameObject go = GameObject.Find(prefix + i);
                if (go == null) continue;
                centroid += go.transform.position;
                count++;
            }
            if (count <= 0) return false;
            centroid /= count;
            Vector3 projected;
            if (TryProjectCommanderBodyPoint(centroid, out projected)) centroid = projected;
            return true;
        }

        private void AddSpawnNodes(TeamType team, int maxCount)
        {
            string prefix = team == TeamType.Cop ? "Spawn_1_" : "Spawn_2_";
            int added = 0;
            for (int i = 1; i <= 16 && added < maxCount; i++)
            {
                GameObject go = GameObject.Find(prefix + i);
                if (go == null) continue;
                int before = _commanderNodes.Count;
                AddCommanderNode(go.transform.position);
                if (_commanderNodes.Count > before) added++;
            }
        }

        private void AddCommanderNode(Vector3 candidate)
        {
            if (_commanderNodes.Count >= COMMANDER_MAX_STRATEGIC_NODES) return;
            Vector3 projected;
            if (!TryProjectCommanderBodyPoint(candidate, out projected)) return;

            for (int i = 0; i < _commanderNodes.Count; i++)
            {
                Vector3 delta = _commanderNodes[i] - projected;
                if (delta.sqrMagnitude < 4.0f * 4.0f) return;
            }
            _commanderNodes.Add(projected);
        }

        private bool TryProjectCommanderBodyPoint(Vector3 candidate, out Vector3 bodyPoint)
        {
            bodyPoint = candidate;
            if (_arenaNavScene != _scene || !CNRZombieMod.ZombieNavGrid.Ready) return false;

            float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
            Vector3 groundCandidate = candidate;
            groundCandidate.y -= bodyOffset;
            Vector3 snapped;
            if (!CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                groundCandidate, 12, groundCandidate.y, 2.0f, out snapped))
            {
                // Spawn transforms and map-center samples are sometimes floor anchors rather
                // than body roots. Try their original height once before rejecting the node.
                groundCandidate = candidate;
                if (!CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                    groundCandidate, 12, groundCandidate.y, 2.0f, out snapped)) return false;
            }
            bodyPoint = snapped;
            bodyPoint.y += bodyOffset;
            return true;
        }

        private void ReportCommanderContact(CNRArenaBotState reporter, Vector3 hostilePosition, float weight)
        {
            if (reporter == null || IsFreeForAllMode()) return;
            CNRMatchSettingsData rules = CNRMatchSettings.Active;
            if (rules == null || rules.Mode != "tdm") return;

            float now = Time.realtimeSinceStartup;
            if (now < reporter.NextStrategicReportAt) return;
            reporter.NextStrategicReportAt = now + COMMANDER_REPORT_INTERVAL;

            // Keep contacts as reported body-space positions. Projection is only needed
            // once a squad actually consumes an order, not every time a sighting is stored.
            float mergeSq = COMMANDER_CONTACT_MERGE_RADIUS * COMMANDER_CONTACT_MERGE_RADIUS;
            CNRArenaStrategicHotspot nearest = null;
            float nearestSq = float.MaxValue;
            for (int i = 0; i < _commanderHotspots.Count; i++)
            {
                CNRArenaStrategicHotspot spot = _commanderHotspots[i];
                if (spot.KnowledgeTeam != reporter.Team) continue;
                Vector3 delta = spot.Position - hostilePosition;
                float d = delta.sqrMagnitude;
                if (d <= mergeSq && d < nearestSq)
                {
                    nearest = spot;
                    nearestSq = d;
                }
            }

            if (nearest != null)
            {
                nearest.Position = Vector3.Lerp(nearest.Position, hostilePosition, 0.42f);
                nearest.Weight = Mathf.Clamp(nearest.Weight + weight * 0.35f, 0.35f, 3.0f);
                nearest.LastUpdatedAt = now;
                return;
            }

            CNRArenaStrategicHotspot created = new CNRArenaStrategicHotspot();
            created.KnowledgeTeam = reporter.Team;
            created.Position = hostilePosition;
            created.Weight = Mathf.Clamp(weight, 0.35f, 3.0f);
            created.LastUpdatedAt = now;
            _commanderHotspots.Add(created);

            while (_commanderHotspots.Count > COMMANDER_MAX_HOTSPOTS)
            {
                int remove = 0;
                float weakest = float.MaxValue;
                for (int i = 0; i < _commanderHotspots.Count; i++)
                {
                    float age = now - _commanderHotspots[i].LastUpdatedAt;
                    float value = _commanderHotspots[i].Weight - age * 0.12f;
                    if (value < weakest) { weakest = value; remove = i; }
                }
                _commanderHotspots.RemoveAt(remove);
            }
        }

        private void PruneCommanderHotspots(float now)
        {
            for (int i = _commanderHotspots.Count - 1; i >= 0; i--)
                if (now - _commanderHotspots[i].LastUpdatedAt > COMMANDER_CONTACT_LIFETIME)
                    _commanderHotspots.RemoveAt(i);
        }

        private void AssignCommanderOrdersForTeam(TeamType team, float now)
        {
            int[] seenSquads = new int[8];
            int seenCount = 0;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState bot = _bots[i];
                if (bot == null || bot.Team != team || bot.SquadId < 0) continue;
                bool seen = false;
                for (int s = 0; s < seenCount; s++)
                    if (seenSquads[s] == bot.SquadId) { seen = true; break; }
                if (seen) continue;
                if (seenCount < seenSquads.Length) seenSquads[seenCount++] = bot.SquadId;

                Vector3 squadCenter;
                if (!TryGetAliveSquadCentroid(bot.SquadId, team, out squadCenter)) continue;

                Vector3 bestDestination;
                float bestScore;
                if (!TryChooseCommanderDestination(team, bot.SquadId, squadCenter, now,
                    out bestDestination, out bestScore)) continue;

                CNRArenaSquadOrder current;
                bool hasCurrent = _commanderOrders.TryGetValue(bot.SquadId, out current);
                if (hasCurrent)
                {
                    float currentScore = ScoreCommanderDestination(team, bot.SquadId,
                        squadCenter, current.Destination, now);
                    float threshold = now < current.CommittedUntil
                        ? COMMANDER_SWITCH_ADVANTAGE
                        : COMMANDER_POST_COMMIT_ADVANTAGE;
                    if (currentScore > 0.01f && bestScore <= currentScore * threshold)
                    {
                        current.Score = currentScore;
                        continue;
                    }
                }

                if (!hasCurrent)
                {
                    current = new CNRArenaSquadOrder();
                    current.SquadId = bot.SquadId;
                    current.Team = team;
                    _commanderOrders[bot.SquadId] = current;
                }

                current.Destination = bestDestination;
                current.Score = bestScore;
                current.LastAssignedAt = now;
                current.CommittedUntil = now + UnityEngine.Random.Range(
                    COMMANDER_ORDER_MIN_SECONDS, COMMANDER_ORDER_MAX_SECONDS);
                current.Revision = ++_commanderRevision;
            }
        }

        private bool TryGetAliveSquadCentroid(int squadId, TeamType team, out Vector3 center)
        {
            center = Vector3.zero;
            int count = 0;
            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState bot = _bots[i];
                if (bot == null || bot.Team != team || bot.SquadId != squadId ||
                    bot.Status == PlayerStatus.dead || bot.Hp <= 0) continue;
                center += bot.Position;
                count++;
            }
            if (count <= 0) return false;
            center /= count;
            return true;
        }

        private bool TryChooseCommanderDestination(TeamType team, int squadId, Vector3 squadCenter,
            float now, out Vector3 destination, out float score)
        {
            destination = squadCenter;
            score = -1f;

            for (int i = 0; i < _commanderNodes.Count; i++)
            {
                Vector3 node = _commanderNodes[i];
                float candidateScore = ScoreCommanderDestination(team, squadId, squadCenter, node, now);
                if (candidateScore > score)
                {
                    score = candidateScore;
                    destination = node;
                }
            }

            // Recent actual contacts are candidates themselves, so a squad can reinforce a
            // fight even if it occurs between the cached sector nodes.
            for (int i = 0; i < _commanderHotspots.Count; i++)
            {
                CNRArenaStrategicHotspot spot = _commanderHotspots[i];
                if (spot.KnowledgeTeam != team) continue;
                // Hotspots are scoring hints, not path endpoints yet. Avoid repeatedly
                // scanning nearby nav cells for every squad during every Commander pass.
                float candidateScore = ScoreCommanderDestination(team, squadId, squadCenter, spot.Position, now);
                if (candidateScore > score)
                {
                    score = candidateScore;
                    destination = spot.Position;
                }
            }
            return score >= 0f;
        }

        private float ScoreCommanderDestination(TeamType team, int squadId, Vector3 squadCenter,
            Vector3 candidate, float now)
        {
            float travelDistance = Vector3.Distance(squadCenter, candidate);
            float travelScore = 1f / (1f + travelDistance * 0.035f);
            float centerScore = 1f / (1f + Vector3.Distance(_commanderCenter, candidate) * 0.045f);
            float pressure = GetCommanderHotspotPressure(team, candidate, now);
            float frontline = GetCommanderFrontlineScore(team, candidate);
            float spacing = GetCommanderSquadSpacingScore(team, squadId, candidate);

            // With no contact, squads naturally spread through useful midfield sectors.
            // Once somebody reports a fight, contact pressure dominates the strategic score.
            return 0.30f * travelScore + 0.22f * centerScore + 0.28f * frontline +
                0.34f * spacing + 2.10f * pressure;
        }

        private float GetCommanderHotspotPressure(TeamType team, Vector3 candidate, float now)
        {
            float pressure = 0f;
            for (int i = 0; i < _commanderHotspots.Count; i++)
            {
                CNRArenaStrategicHotspot spot = _commanderHotspots[i];
                if (spot.KnowledgeTeam != team) continue;
                float age = now - spot.LastUpdatedAt;
                float freshness = Mathf.Clamp01(1f - age / COMMANDER_CONTACT_LIFETIME);
                float distance = Vector3.Distance(candidate, spot.Position);
                float proximity = 1f / (1f + distance * 0.12f);
                pressure += spot.Weight * freshness * proximity;
            }
            return Mathf.Min(2.5f, pressure);
        }

        private float GetCommanderFrontlineScore(TeamType team, Vector3 candidate)
        {
            if (!_commanderHasCopSpawn || !_commanderHasRobberSpawn) return 0.65f;
            Vector3 own = team == TeamType.Cop ? _commanderCopSpawn : _commanderRobberSpawn;
            Vector3 enemy = team == TeamType.Cop ? _commanderRobberSpawn : _commanderCopSpawn;
            Vector3 axis = enemy - own;
            axis.y = 0f;
            float length = axis.magnitude;
            if (length < 1f) return 0.65f;
            axis /= length;
            Vector3 fromOwn = candidate - own;
            fromOwn.y = 0f;
            float progress = Mathf.Clamp01(Vector3.Dot(fromOwn, axis) / length);

            // Prefer the middle/forward two-thirds, not the enemy's literal spawn point.
            float centered = 1f - Mathf.Abs(progress - 0.62f) / 0.62f;
            return Mathf.Clamp01(centered);
        }

        private float GetCommanderSquadSpacingScore(TeamType team, int squadId, Vector3 candidate)
        {
            float nearest = 30f;
            foreach (KeyValuePair<int, CNRArenaSquadOrder> pair in _commanderOrders)
            {
                CNRArenaSquadOrder order = pair.Value;
                if (order == null || order.Team != team || order.SquadId == squadId) continue;
                nearest = Mathf.Min(nearest, Vector3.Distance(candidate, order.Destination));
            }
            return Mathf.Clamp01((nearest - 5f) / 15f);
        }

        private void PruneUnusedCommanderOrders()
        {
            if (_commanderOrders.Count == 0) return;
            List<int> remove = null;
            foreach (KeyValuePair<int, CNRArenaSquadOrder> pair in _commanderOrders)
            {
                bool exists = false;
                for (int i = 0; i < _bots.Count; i++)
                {
                    CNRArenaBotState bot = _bots[i];
                    if (bot != null && bot.SquadId == pair.Key) { exists = true; break; }
                }
                if (exists) continue;
                if (remove == null) remove = new List<int>();
                remove.Add(pair.Key);
            }
            if (remove == null) return;
            for (int i = 0; i < remove.Count; i++) _commanderOrders.Remove(remove[i]);
        }

        private bool TryGetCommanderPatrolTarget(CNRArenaBotState bot, out Vector3 target)
        {
            target = bot != null ? bot.Position : Vector3.zero;
            if (bot == null || bot.SquadId < 0 || IsFreeForAllMode()) return false;
            CNRMatchSettingsData rules = CNRMatchSettings.Active;
            if (rules == null || rules.Mode != "tdm") return false;

            CNRArenaSquadOrder order;
            if (!_commanderOrders.TryGetValue(bot.SquadId, out order) || order == null) return false;

            if (bot.StrategicOrderRevision != order.Revision)
            {
                Vector3 squadCenter;
                if (!TryGetAliveSquadCentroid(bot.SquadId, bot.Team, out squadCenter))
                    squadCenter = bot.Position;

                Vector3 forward = order.Destination - squadCenter;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.01f) forward = GetBotForward(bot);
                else forward.Normalize();
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                if (right.sqrMagnitude > 0.001f) right.Normalize();

                float slotOffset = 0f;
                if (bot.SquadSize == 2)
                    slotOffset = (bot.SquadSlot == 0 ? -0.5f : 0.5f) * COMMANDER_SLOT_SPACING;
                else if (bot.SquadSize >= 3)
                    slotOffset = (bot.SquadSlot - 1) * COMMANDER_SLOT_SPACING;

                Vector3 candidate = order.Destination + right * slotOffset;
                Vector3 projected;
                bot.StrategicTarget = TryProjectCommanderBodyPoint(candidate, out projected)
                    ? projected
                    : order.Destination;
                bot.StrategicOrderRevision = order.Revision;
            }

            target = bot.StrategicTarget;
            return true;
        }
    }
}
