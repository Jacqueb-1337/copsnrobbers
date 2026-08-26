using UnityEngine;

namespace CNRMods
{
    internal partial class CNRArenaBotState
    {
        public Vector3 LastObservedTargetPosition;
        public float LastObservedTargetAt;
        public Vector3 TargetVelocityEstimate;
        public Vector3 SearchAnchor;
        public int SearchStep;
        public float LastHeardThreatAt;
        public float LastSoundRouteAt;
    }

    public partial class CNRArenaBotManager
    {
        private const float BOT_GUNFIRE_HEARING_RADIUS = 30f;
        private const float BOT_SOUND_REDIRECT_STALE_SECONDS = 1.15f;
        private const float BOT_SOUND_REROUTE_INTERVAL = 1.35f;
        private const float BOT_SOUND_REROUTE_DISTANCE = 5.0f;
        private const int LOST_TARGET_SEARCH_STEPS = 3;

        private void ObserveVisibleTargetMotion(CNRArenaBotState bot, string targetId,
            Vector3 targetPosition, float now, bool changed)
        {
            if (bot == null) return;
            if (changed)
            {
                bot.TargetVelocityEstimate = Vector3.zero;
                bot.LastObservedTargetPosition = targetPosition;
                bot.LastObservedTargetAt = now;
            }
            else
            {
                float dt = now - bot.LastObservedTargetAt;
                if (dt >= 0.05f && dt <= 1.0f)
                {
                    Vector3 measured = (targetPosition - bot.LastObservedTargetPosition) / dt;
                    measured.y = 0f;
                    float max = BOT_RUN_SPEED * 1.35f;
                    if (measured.magnitude > max) measured = measured.normalized * max;
                    bot.TargetVelocityEstimate = Vector3.Lerp(bot.TargetVelocityEstimate, measured, 0.45f);
                    bot.LastObservedTargetPosition = targetPosition;
                    bot.LastObservedTargetAt = now;
                }
                else if (dt > 1.0f)
                {
                    bot.LastObservedTargetPosition = targetPosition;
                    bot.LastObservedTargetAt = now;
                }
            }

            bot.SearchAnchor = targetPosition;
            bot.SearchStep = 0;
        }

        private static void ResetExtendedPerception(CNRArenaBotState bot)
        {
            if (bot == null) return;
            bot.LastObservedTargetPosition = Vector3.zero;
            bot.LastObservedTargetAt = 0f;
            bot.TargetVelocityEstimate = Vector3.zero;
            bot.SearchAnchor = Vector3.zero;
            bot.SearchStep = 0;
            bot.LastHeardThreatAt = 0f;
            bot.LastSoundRouteAt = 0f;
        }

        private bool TryHearHostileGunfire(CNRArenaBotState bot, out string targetId,
            out Vector3 cuePosition, out float score)
        {
            targetId = "";
            cuePosition = Vector3.zero;
            score = -1f;
            if (bot == null || _mgr == null) return false;

            PlayerInfo mine = _mgr.myPlayerInfo;
            ConsiderAudibleHuman(bot, mine, ref targetId, ref cuePosition, ref score);

            PlayerInfo[] others = _mgr.otherPlayersInfoList;
            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    PlayerInfo p = others[i];
                    if (p == null || IsBotId(p.mId)) continue;
                    ConsiderAudibleHuman(bot, p, ref targetId, ref cuePosition, ref score);
                }
            }

            for (int i = 0; i < _bots.Count; i++)
            {
                CNRArenaBotState other = _bots[i];
                if (other == null || other == bot || other.Status != PlayerStatus.fire ||
                    other.Hp <= 0 || other.Status == PlayerStatus.dead) continue;
                if (!IsFreeForAllMode() && other.Team == bot.Team) continue;

                Vector3 delta = other.Position - bot.Position;
                delta.y = 0f;
                float distance = delta.magnitude;
                if (distance > BOT_GUNFIRE_HEARING_RADIUS) continue;
                float audible = 1f - distance / BOT_GUNFIRE_HEARING_RADIUS;
                if (audible <= score) continue;
                score = audible;
                targetId = other.Id;
                cuePosition = BuildSoundCue(other.Position, distance);
            }

            return !string.IsNullOrEmpty(targetId);
        }

        private void ConsiderAudibleHuman(CNRArenaBotState bot, PlayerInfo candidate,
            ref string targetId, ref Vector3 cuePosition, ref float score)
        {
            if (!IsValidHumanEnemy(bot, candidate) || candidate.mStatus != PlayerStatus.fire) return;
            Vector3 delta = candidate.mPosition - bot.Position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance > BOT_GUNFIRE_HEARING_RADIUS) return;
            float audible = 1f - distance / BOT_GUNFIRE_HEARING_RADIUS;
            if (audible <= score) return;
            score = audible;
            targetId = candidate.mId;
            cuePosition = BuildSoundCue(candidate.mPosition, distance);
        }

        private static Vector3 BuildSoundCue(Vector3 actual, float distance)
        {
            float t = Mathf.Clamp01(distance / BOT_GUNFIRE_HEARING_RADIUS);
            float error = Mathf.Lerp(0.8f, 4.0f, t) * UnityEngine.Random.Range(0.65f, 1.0f);
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return actual + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * error;
        }

        private void SetAudibleThreat(CNRArenaBotState bot, string targetId, Vector3 cuePosition,
            float soundScore, float now)
        {
            if (bot == null || string.IsNullOrEmpty(targetId)) return;
            bool changed = bot.LastTargetId != targetId;
            Vector3 previousCue = bot.LastKnownTargetPosition;
            if (changed)
            {
                bot.TargetVelocityEstimate = Vector3.zero;
                bot.LastObservedTargetPosition = cuePosition;
                bot.LastObservedTargetAt = 0f;
            }
            else
            {
                // Repeated gunfire samples contain intentional positional error. Smooth the
                // cue instead of making the navigation destination jump on every sample.
                cuePosition = Vector3.Lerp(previousCue, cuePosition, 0.30f);
            }

            Vector3 cueDelta = cuePosition - previousCue;
            cueDelta.y = 0f;
            bool routeMissing = bot.NavPath == null || bot.NavPath.Count == 0;
            bool rerouteReady = now - bot.LastSoundRouteAt >= BOT_SOUND_REROUTE_INTERVAL;
            bool shouldReroute = changed ||
                (rerouteReady && (routeMissing ||
                    cueDelta.sqrMagnitude >= BOT_SOUND_REROUTE_DISTANCE * BOT_SOUND_REROUTE_DISTANCE));

            bot.LastTargetId = targetId;
            bot.LastKnownTargetPosition = cuePosition;
            // Keep this just outside HasFreshVisibleTarget's 0.35s window. Hearing gives
            // a search cue, never fake visual confirmation for engagement coordination.
            bot.LastTargetVisibleAt = now - 0.40f;
            bot.LastHeardThreatAt = now;
            bot.HasLastKnownTarget = true;
            bot.TargetCertainty = Mathf.Max(bot.TargetCertainty, 0.10f + soundScore * 0.08f);
            bot.ReactionReadyAt = changed
                ? now + UnityEngine.Random.Range(0.30f, 0.52f)
                : bot.ReactionReadyAt;
            bot.CurrentTargetScore = Mathf.Max(bot.CurrentTargetScore, 0.18f + soundScore * 0.18f);
            bot.SearchAnchor = cuePosition;
            bot.SearchStep = 0;
            bot.Behavior = CNRArenaBotBehavior.Search;
            bot.TacticalTarget = bot.Position;
            bot.TacticalScore = -1f;
            bot.TacticalCommittedUntil = 0f;

            // Hearing used to clear NavPath every time sustained gunfire refreshed the cue.
            // That recreated the old A* storm during firefights. Only rebuild when the
            // source changes or the smoothed cue has moved far enough, and never faster
            // than the explicit sound reroute interval.
            if (shouldReroute)
            {
                bot.NavPath = null;
                bot.NavPathIndex = 0;
                bot.NextNavRepathAt = 0f;
                bot.LastSoundRouteAt = now;
            }
            ReportCommanderContact(bot, cuePosition, 0.50f);
        }

        private bool TryAdvanceLostTargetSearch(CNRArenaBotState bot, float now, out Vector3 searchPoint)
        {
            searchPoint = bot != null ? bot.Position : Vector3.zero;
            if (bot == null || bot.SearchStep >= LOST_TARGET_SEARCH_STEPS) return false;
            if (now - bot.LastTargetVisibleAt >= TARGET_MEMORY_SECONDS) return false;

            Vector3 baseDirection = bot.TargetVelocityEstimate;
            baseDirection.y = 0f;
            if (baseDirection.sqrMagnitude < 0.20f * 0.20f)
                baseDirection = GetBotForward(bot);
            else
                baseDirection.Normalize();

            float unseen = Mathf.Max(0f, now - bot.LastTargetVisibleAt);
            Vector3 predicted = bot.SearchAnchor + bot.TargetVelocityEstimate * Mathf.Min(1.15f, unseen);

            float angle;
            float distance;
            if (bot.SearchStep == 0) { angle = 0f; distance = 4.5f; }
            else if (bot.SearchStep == 1) { angle = 55f; distance = 5.5f; }
            else { angle = -55f; distance = 5.5f; }

            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * baseDirection;
            Vector3 candidate = predicted + direction * distance;
            candidate.y = bot.LastKnownTargetPosition.y;

            float bodyOffset = GetBodyGroundOffset() + BOT_GROUND_EXTRA_LIFT;
            Vector3 groundCandidate = candidate;
            groundCandidate.y -= bodyOffset;
            Vector3 snapped;
            if (_arenaNavScene == _scene && CNRZombieMod.ZombieNavGrid.Ready &&
                CNRZombieMod.ZombieNavGrid.TrySnapToWalkableNearHeight(
                    groundCandidate, 7, groundCandidate.y, NAV_HEIGHT_TOLERANCE, out snapped))
            {
                candidate = snapped;
                candidate.y += bodyOffset;
            }

            bot.SearchStep++;
            bot.LastKnownTargetPosition = candidate;
            bot.NavPath = null;
            bot.NavPathIndex = 0;
            bot.NextNavRepathAt = 0f;
            searchPoint = candidate;
            return true;
        }
    }
}
