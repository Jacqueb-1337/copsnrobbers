using System;
using UnityEngine;

namespace CNRMods
{
    /// <summary>
    /// Core single-player AI upgrade. Keeps the game's original SingleEnemyLogic
    /// for mission scripting, damage, weapons, rewards, and death handling, while
    /// supervising target selection and movement with faster perception and more
    /// deliberate ranged behavior inspired by the later 5.x enemy architecture.
    /// </summary>
    public class CNRSinglePlayerAIHook : MonoBehaviour
    {
        private float _nextScanAt;
        private string _lastScene = "";
        private int _attachLogBudget = 12;

        public static bool IsSinglePlayerScene()
        {
            string scene = Application.loadedLevelName ?? "";
            return scene.StartsWith("SingleMode_", StringComparison.Ordinal);
        }

        void OnLevelWasLoaded(int level)
        {
            _lastScene = Application.loadedLevelName ?? "";
            _nextScanAt = 0f;
        }

        void Update()
        {
            if (!IsSinglePlayerScene()) return;

            string scene = Application.loadedLevelName ?? "";
            if (_lastScene != scene)
            {
                _lastScene = scene;
                _nextScanAt = 0f;
            }

            if (Time.realtimeSinceStartup < _nextScanAt) return;
            _nextScanAt = Time.realtimeSinceStartup + 0.35f;
            AttachBrains();
        }

        private void AttachBrains()
        {
            UnityEngine.Object[] enemies;
            try { enemies = UnityEngine.Object.FindObjectsOfType(typeof(SingleEnemyLogic)); }
            catch { return; }

            for (int i = 0; i < enemies.Length; i++)
            {
                SingleEnemyLogic logic = enemies[i] as SingleEnemyLogic;
                if (logic == null) continue;
                GameObject go = logic.gameObject;
                if (go == null) continue;
                if (go.GetComponent<SingleEnemyAI>() == null) continue;
                if (go.GetComponent<CNRSingleEnemyBrain>() != null) continue;

                go.AddComponent<CNRSingleEnemyBrain>();
                if (_attachLogBudget > 0)
                {
                    _attachLogBudget--;
                    ModEntry.Log("SinglePlayerAI: attached to " + go.name);
                }
            }
        }
    }

    public class CNRSingleEnemyBrain : MonoBehaviour
    {
        private const float THINK_INTERVAL = 0.08f;
        private const float MEMORY_SECONDS = 6.0f;
        private const float ALERT_INTERVAL = 1.75f;
        private const float STUCK_SAMPLE_INTERVAL = 0.75f;
        private const float STUCK_DISTANCE_EPSILON = 0.12f;
        private const float STUCK_RECOVERY_SECONDS = 1.5f;

        private SingleEnemyLogic _logic;
        private SingleEnemyAI _ai;
        private Transform _player;
        private GameObject _memoryTargetObject;
        private Transform _memoryTarget;

        private float _nextThinkAt;
        private float _lastSeenAt = -999f;
        private Vector3 _lastSeenPos;
        private float _nextAlertAt;
        private float _nextRepositionAt;
        private float _nextStuckSampleAt;
        private float _stuckFor;
        private Vector3 _lastStuckSamplePos;
        private int _strafeSign = 1;
        private int _logBudget = 4;

        void Awake()
        {
            _logic = GetComponent<SingleEnemyLogic>();
            _ai = GetComponent<SingleEnemyAI>();
            _lastStuckSamplePos = transform.position;

            _memoryTargetObject = new GameObject("CNR_AI_Target_" + gameObject.GetInstanceID());
            _memoryTargetObject.hideFlags = HideFlags.HideInHierarchy;
            _memoryTarget = _memoryTargetObject.transform;
            _memoryTarget.position = transform.position;
        }

        void Start()
        {
            EnsurePlayer();
            ApplyBaselineTuning();
        }

        void OnDestroy()
        {
            if (_memoryTargetObject != null)
                UnityEngine.Object.Destroy(_memoryTargetObject);
        }

        void LateUpdate()
        {
            if (!CNRSinglePlayerAIHook.IsSinglePlayerScene()) return;
            if (_logic == null || _ai == null || _logic.bDead) return;
            if (PlayerLogic.mInstance != null && PlayerLogic.mInstance.bDied) return;
            if (Time.realtimeSinceStartup < _nextThinkAt) return;

            _nextThinkAt = Time.realtimeSinceStartup + THINK_INTERVAL;
            if (!EnsurePlayer()) return;

            ApplyBaselineTuning();
            Think();
            CheckStuckRecovery();
        }

        private bool EnsurePlayer()
        {
            if (_player != null) return true;
            if (_logic != null && _logic.playerTransform != null)
            {
                _player = _logic.playerTransform;
                return true;
            }

            GameObject player = null;
            try { player = GameObject.FindWithTag("Player"); } catch { }
            if (player == null) return false;
            _player = player.transform;
            if (_logic != null) _logic.playerTransform = _player;
            return true;
        }

        private void ApplyBaselineTuning()
        {
            if (_logic == null) return;

            // These are intentionally modest. The goal is responsiveness, not
            // turning every enemy into a speed-hacked bullet sponge.
            if (_logic.patrolSpeed < 2.0f) _logic.patrolSpeed = 2.0f;
            if (_logic.AttackMoveSpeed < 2.8f) _logic.AttackMoveSpeed = 2.8f;
            if (_logic.RushSpeed < 4.2f) _logic.RushSpeed = 4.2f;
            if (_logic.EscapeSpeed < 4.4f) _logic.EscapeSpeed = 4.4f;
        }

        private void Think()
        {
            Vector3 enemyEye = transform.position + new Vector3(0f, 1.2f, 0f);
            Vector3 playerAim = _player.position + new Vector3(0f, 0.9f, 0f);
            float distance = Vector3.Distance(transform.position, _player.position);
            bool visible = HasLineOfSight(enemyEye, playerAim, distance + 2f);

            if (visible)
            {
                _lastSeenAt = Time.realtimeSinceStartup;
                _lastSeenPos = _player.position;

                if (Time.realtimeSinceStartup >= _nextAlertAt)
                {
                    _nextAlertAt = Time.realtimeSinceStartup + ALERT_INTERVAL;
                    AlertNearbyEnemies();
                }

                HandleVisiblePlayer(distance);
                return;
            }

            HandleLostPlayer();
        }

        private bool HasLineOfSight(Vector3 origin, Vector3 target, float maxDistance)
        {
            Vector3 delta = target - origin;
            float len = delta.magnitude;
            if (len <= 0.01f) return true;

            RaycastHit hit;
            if (!Physics.Raycast(origin, delta / len, out hit, Mathf.Min(maxDistance, 65f), -21))
                return false;

            if (hit.collider == null) return false;
            if (hit.collider.transform == _player) return true;
            if (hit.collider.transform.IsChildOf(_player)) return true;
            if (hit.collider.gameObject.tag == "Player") return true;
            return false;
        }

        private void HandleVisiblePlayer(float distance)
        {
            switch (_logic.enemyType)
            {
                case EnemyType.knifeEnemy:
                    HandleKnife(distance);
                    break;
                case EnemyType.gunEnemy:
                    HandleGun(distance);
                    break;
                case EnemyType.sniperEnemy:
                    HandleSniper(distance);
                    break;
                case EnemyType.granadeEnemy:
                    HandleGrenade(distance);
                    break;
            }
        }

        private void HandleKnife(float distance)
        {
            if (distance <= 2.1f)
            {
                SetAction(EnemyAction.attack);
                _ai.canSearch = false;
            }
            else
            {
                SetAction(EnemyAction.rush);
                _ai.target = _player;
                _ai.canSearch = true;
                _ai.speed = Mathf.Max(_ai.speed, _logic.RushSpeed);
            }
        }

        private void HandleGun(float distance)
        {
            if (distance > 20f)
            {
                SetAction(EnemyAction.rush);
                _ai.target = _player;
                _ai.canSearch = true;
                _ai.speed = Mathf.Max(_ai.speed, _logic.RushSpeed);
                return;
            }

            SetAction(EnemyAction.attack);

            if (Time.realtimeSinceStartup >= _nextRepositionAt)
            {
                _nextRepositionAt = Time.realtimeSinceStartup + 1.1f + UnityEngine.Random.Range(0f, 0.55f);
                _strafeSign = -_strafeSign;
                Vector3 away = HorizontalDirection(_player.position, transform.position);
                Vector3 side = new Vector3(-away.z, 0f, away.x) * _strafeSign;

                if (distance < 6.5f)
                    SetMoveTarget(transform.position + away * 5.5f + side * 2.0f, _logic.AttackMoveSpeed);
                else
                    SetMoveTarget(transform.position + side * 4.0f + away * 0.75f, _logic.AttackMoveSpeed);
            }
        }

        private void HandleSniper(float distance)
        {
            SetAction(EnemyAction.attack);

            if (distance < 18f)
            {
                Vector3 away = HorizontalDirection(_player.position, transform.position);
                Vector3 side = new Vector3(-away.z, 0f, away.x) * _strafeSign;
                SetMoveTarget(transform.position + away * 8f + side * 2.5f, Mathf.Max(3.2f, _logic.AttackMoveSpeed));
                if (Time.realtimeSinceStartup >= _nextRepositionAt)
                {
                    _nextRepositionAt = Time.realtimeSinceStartup + 1.4f;
                    _strafeSign = -_strafeSign;
                }
            }
            else
            {
                // A sniper with a clean sight line should stop wandering randomly.
                _ai.canSearch = false;
            }
        }

        private void HandleGrenade(float distance)
        {
            if (distance > 26f)
            {
                SetAction(EnemyAction.rush);
                _ai.target = _player;
                _ai.canSearch = true;
                _ai.speed = Mathf.Max(_ai.speed, _logic.RushSpeed);
                return;
            }

            SetAction(EnemyAction.attack);

            if (Time.realtimeSinceStartup >= _nextRepositionAt)
            {
                _nextRepositionAt = Time.realtimeSinceStartup + 1.5f + UnityEngine.Random.Range(0f, 0.7f);
                _strafeSign = -_strafeSign;
                Vector3 away = HorizontalDirection(_player.position, transform.position);
                Vector3 side = new Vector3(-away.z, 0f, away.x) * _strafeSign;

                if (distance < 10f)
                    SetMoveTarget(transform.position + away * 6.5f + side * 2f, _logic.AttackMoveSpeed);
                else
                    SetMoveTarget(transform.position + side * 4.5f, _logic.AttackMoveSpeed);
            }
        }

        private void HandleLostPlayer()
        {
            float sinceSeen = Time.realtimeSinceStartup - _lastSeenAt;
            if (sinceSeen <= MEMORY_SECONDS)
            {
                SetAction(EnemyAction.search);
                _memoryTarget.position = _lastSeenPos;
                _ai.target = _memoryTarget;
                _ai.canSearch = true;
                _ai.speed = Mathf.Max(_ai.speed, _logic.RushSpeed * 0.9f);
                return;
            }

            // End the vanilla warning/search loops instead of letting an enemy
            // repeatedly stare at a wall forever after losing sight of the player.
            if (_logic.enemyAction == EnemyAction.search ||
                _logic.enemyAction == EnemyAction.warning ||
                _logic.enemyAction == EnemyAction.rush ||
                _logic.enemyAction == EnemyAction.attack)
            {
                SetAction(EnemyAction.patrol);
                _ai.canSearch = true;
            }
        }

        private void SetAction(EnemyAction action)
        {
            if (_logic.enemyAction == action) return;
            _logic.enemyAction = action;

            if (action == EnemyAction.attack) _logic.attackTime = 0f;
            else if (action == EnemyAction.rush) _logic.rushTime = 0f;
            else if (action == EnemyAction.search) _logic.searchTime = 0f;
            else if (action == EnemyAction.warning) _logic.warningTime = 0f;
            else if (action == EnemyAction.escape) _logic.escapeTime = 0f;
            else if (action == EnemyAction.patrol) _logic.patrolTime = 0f;
            else if (action == EnemyAction.idle) _logic.idleTime = 0f;
        }

        private void SetMoveTarget(Vector3 position, float speed)
        {
            _memoryTarget.position = position;
            _ai.target = _memoryTarget;
            _ai.canSearch = true;
            _ai.speed = speed;
        }

        private static Vector3 HorizontalDirection(Vector3 from, Vector3 to)
        {
            Vector3 v = to - from;
            v.y = 0f;
            if (v.sqrMagnitude < 0.001f) return Vector3.forward;
            return v.normalized;
        }

        private void AlertNearbyEnemies()
        {
            Collider[] around;
            try { around = Physics.OverlapSphere(transform.position, 28f); }
            catch { return; }

            for (int i = 0; i < around.Length; i++)
            {
                Collider col = around[i];
                if (col == null || col.gameObject == gameObject) continue;
                if (col.gameObject.tag != "singleEnemy") continue;

                SingleEnemyLogic other = col.gameObject.GetComponent<SingleEnemyLogic>();
                SingleEnemyAI otherAi = col.gameObject.GetComponent<SingleEnemyAI>();
                if (other == null || otherAi == null || other.bDead) continue;

                if (other.enemyAction == EnemyAction.idle ||
                    other.enemyAction == EnemyAction.patrol ||
                    other.enemyAction == EnemyAction.warning)
                {
                    other.enemyAction = EnemyAction.rush;
                    other.rushTime = 0f;
                    otherAi.target = _player;
                    otherAi.canSearch = true;
                    otherAi.speed = Mathf.Max(otherAi.speed, other.RushSpeed);
                }
            }
        }

        private void CheckStuckRecovery()
        {
            if (Time.realtimeSinceStartup < _nextStuckSampleAt) return;
            _nextStuckSampleAt = Time.realtimeSinceStartup + STUCK_SAMPLE_INTERVAL;

            bool shouldMove = _ai.canSearch && _ai.target != null &&
                              Vector3.Distance(transform.position, _ai.target.position) > 2f;
            float moved = Vector3.Distance(transform.position, _lastStuckSamplePos);
            _lastStuckSamplePos = transform.position;

            if (!shouldMove || moved > STUCK_DISTANCE_EPSILON)
            {
                _stuckFor = 0f;
                return;
            }

            _stuckFor += STUCK_SAMPLE_INTERVAL;
            if (_stuckFor < STUCK_RECOVERY_SECONDS) return;
            _stuckFor = 0f;
            _strafeSign = -_strafeSign;

            Vector3 side = transform.right * (_strafeSign * 3.5f);
            Vector3 forward = transform.forward * 2.0f;
            SetMoveTarget(transform.position + side + forward, Mathf.Max(_logic.RushSpeed, 4.2f));

            if (_logBudget > 0)
            {
                _logBudget--;
                ModEntry.Log("SinglePlayerAI: stuck recovery on enemy " + _logic.enemyId);
            }
        }
    }
}
