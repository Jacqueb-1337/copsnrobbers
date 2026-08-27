using System.Collections.Generic;
using UnityEngine;

namespace CNRMods
{
    // Client-only damage number bridge. Nothing is networked and the feature is OFF by default.
    public static class CNRDamageNumbers
    {
        public const string PrefKey = "CNRMod_DamageNumbers";
        private static CNRDamageNumberHud _hud;

        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(PrefKey, 0) == 1; }
        }

        public static void Report(Transform target, Vector3 point, int amount, bool headshot)
        {
            if (!Enabled || target == null || amount <= 0) return;

            if (_hud == null)
            {
                GameObject go = new GameObject("CNR_DamageNumbers");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _hud = go.AddComponent<CNRDamageNumberHud>();
            }

            Transform root = target.root != null ? target.root : target;
            _hud.Add(root.gameObject.GetInstanceID(), point, amount, headshot);
        }
    }

    public class CNRDamageNumberHud : MonoBehaviour
    {
        private sealed class Popup
        {
            public int TargetId;
            public int Amount;
            public Vector3 WorldPoint;
            public float StartedAt;
            public float LastHitAt;
            public bool Headshot;
        }

        private const float MergeWindow = 0.11f;
        private const float Lifetime = 0.82f;
        private readonly List<Popup> _popups = new List<Popup>();
        private GUIStyle _style;
        private GUIStyle _shadowStyle;

        public void Add(int targetId, Vector3 point, int amount, bool headshot)
        {
            if (!CNRDamageNumbers.Enabled || amount <= 0) return;

            // Shotguns/Bussin' create several pellets in the same instant. Merge them into
            // one readable number instead of painting five overlapping labels.
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                Popup existing = _popups[i];
                if (existing.TargetId == targetId && Time.time - existing.LastHitAt <= MergeWindow)
                {
                    existing.Amount += amount;
                    existing.WorldPoint = point;
                    existing.StartedAt = Time.time;
                    existing.LastHitAt = Time.time;
                    existing.Headshot = existing.Headshot || headshot;
                    return;
                }
            }

            Popup popup = new Popup();
            popup.TargetId = targetId;
            popup.Amount = amount;
            popup.WorldPoint = point;
            popup.StartedAt = Time.time;
            popup.LastHitAt = Time.time;
            popup.Headshot = headshot;
            _popups.Add(popup);
        }

        private void OnGUI()
        {
            if (!CNRDamageNumbers.Enabled)
            {
                if (_popups.Count > 0) _popups.Clear();
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;
            EnsureStyles();

            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                Popup popup = _popups[i];
                float age = Time.time - popup.StartedAt;
                if (age >= Lifetime)
                {
                    _popups.RemoveAt(i);
                    continue;
                }

                Vector3 world = popup.WorldPoint + Vector3.up * (0.20f + age * 0.55f);
                Vector3 screen = cam.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;

                float alpha = 1f - Mathf.Clamp01(age / Lifetime);
                string text = popup.Amount.ToString();
                Rect r = new Rect(screen.x - 55f, Screen.height - screen.y - 20f, 110f, 40f);

                _shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f * alpha);
                GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, _shadowStyle);
                _style.normal.textColor = popup.Headshot
                    ? new Color(1f, 0.82f, 0.25f, alpha)
                    : new Color(1f, 1f, 1f, alpha);
                GUI.Label(r, text, _style);
            }
        }

        private void EnsureStyles()
        {
            if (_style != null) return;
            _style = new GUIStyle(GUI.skin.label);
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = 22;
            _style.fontStyle = FontStyle.Bold;
            _shadowStyle = new GUIStyle(_style);
        }
    }

    // Inherited by real bullets from their templates. It only arms for bullets whose
    // vanilla shooter field is "player", so remote/incoming shots never make numbers.
    public class CNRDamageNumberProbe : MonoBehaviour
    {
        private Bullet _bullet;
        private Vector3 _lastPosition;
        private Vector3 _spawnPosition;
        private bool _armed;
        private bool _reported;

        private void Start()
        {
            if (!CNRDamageNumbers.Enabled)
            {
                enabled = false;
                return;
            }

            _bullet = GetComponent<Bullet>();
            if (_bullet == null || _bullet.shooter != "player")
            {
                enabled = false;
                return;
            }

            _lastPosition = transform.position;
            _spawnPosition = _lastPosition;
            _armed = true;
        }

        private void LateUpdate()
        {
            if (!_armed || _reported || _bullet == null) return;

            Vector3 current = transform.position;
            Vector3 delta = current - _lastPosition;
            float distance = delta.magnitude;
            if (distance > 0.0001f)
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(_lastPosition, delta / distance, out hit, distance + 0.08f, 19))
                {
                    string tag = hit.transform != null ? hit.transform.tag : "";
                    if (IsDamageTag(tag))
                    {
                        int damage = ResolveDamage(hit, tag);
                        CNRDamageNumbers.Report(hit.transform, hit.point, damage, tag == "EnemyHeadTag");
                        _reported = true;
                        enabled = false;
                        return;
                    }
                }
            }

            _lastPosition = current;
        }

        private int ResolveDamage(RaycastHit hit, string tag)
        {
            float damage = Mathf.Max(1f, _bullet.bulletDamage);
            CNRBussinBullet bussin = _bullet as CNRBussinBullet;
            if (bussin != null)
            {
                float traveled = Vector3.Distance(_spawnPosition, hit.point);
                damage *= CNRDLCWeaponSystem.GetBussinDamageScale(bussin.BussinLevel, traveled);
                if (tag == "EnemyHeadTag") damage *= CNRDLCWeaponSystem.BussinHeadshotMultiplier;
                else if (tag == "EnemyFootTag") damage *= 0.7f;
            }
            else
            {
                CNRBalancedVanillaBullet balanced = _bullet as CNRBalancedVanillaBullet;
                if (tag == "EnemyHeadTag" && balanced != null)
                    damage *= CNRVanillaHeadshotBalance.GetMultiplier(balanced.WeaponName);
                else if (tag == "EnemyFootTag")
                    damage *= 0.7f;
            }

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        private static bool IsDamageTag(string tag)
        {
            return tag == "EnemyTag" || tag == "EnemyHeadTag" ||
                   tag == "EnemyBodyTag" || tag == "EnemyFootTag" || tag == "Player";
        }
    }
}
