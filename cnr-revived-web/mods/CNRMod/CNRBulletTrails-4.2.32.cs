using System.Collections.Generic;
using UnityEngine;

namespace CNRMods
{
    // Draws a short-lived hitscan-style tracer for each real traveling Bullet.
    // The endpoint is calculated from the Bullet's actual spawn position/direction
    // using the same raycast mask as vanilla Bullet.Update(). The visible line starts
    // at the weapon model's muzzle instead, so first-person tracers appear to leave
    // the gun rather than the center of the screen.
    public class CNRBulletTrailSystem : MonoBehaviour
    {
        private const float TracerLifetime = 0.20f;
        private const float TracerStartAlpha = 0.60f;
        private const float TracerStartWidth = 0.032f;
        private const float TracerEndWidth = 0.010f;
        private const float TemplateScanInterval = 1.00f;
        private const float LiveFallbackScanInterval = 0.40f;
        private const float WeaponMuzzleSettleTime = 0.30f;
        private const int VanillaBulletRayMask = 19;

        private static Material _tracerMaterial;
        private static Texture2D _tracerTexture;
        private static readonly HashSet<int> _templateIds = new HashSet<int>();
        private static readonly List<WeaponScript> _weaponScripts = new List<WeaponScript>();
        private static readonly List<WeaponSync> _weaponSyncs = new List<WeaponSync>();

        private float _nextTemplateScan;
        private float _nextLiveScan;

        private void Start()
        {
            EnsureMaterial();
            RefreshWeaponSourcesAndTemplates();
            DecorateLiveBullets();
        }

        private void Update()
        {
            if (Time.time >= _nextTemplateScan)
            {
                _nextTemplateScan = Time.time + TemplateScanInterval;
                RefreshWeaponSourcesAndTemplates();
            }

            // Fallback for a projectile prefab that appeared after our template scan.
            // Normally CNRBulletRayTracer is inherited by the bullet at Instantiate(),
            // so the ray is resolved in the same frame the shot is fired.
            if (Time.time >= _nextLiveScan)
            {
                _nextLiveScan = Time.time + LiveFallbackScanInterval;
                DecorateLiveBullets();
            }
        }

        private static void RefreshWeaponSourcesAndTemplates()
        {
            _weaponScripts.Clear();
            _weaponSyncs.Clear();

            UnityEngine.Object[] weaponObjects = UnityEngine.Object.FindObjectsOfType(typeof(WeaponScript));
            for (int i = 0; i < weaponObjects.Length; i++)
            {
                WeaponScript weapon = weaponObjects[i] as WeaponScript;
                if (weapon == null) continue;
                _weaponScripts.Add(weapon);
                if (weapon.gameObject.GetComponent<CNRBulletMuzzleState>() == null)
                    weapon.gameObject.AddComponent<CNRBulletMuzzleState>();

                try { CNRVanillaHeadshotBalance.ConfigureWeapon(weapon); }
                catch { }

                try
                {
                    if (weapon.machineGun != null && weapon.machineGun.bullet != null)
                        ConfigureTemplate(weapon.machineGun.bullet.gameObject);
                }
                catch { }

                try
                {
                    if (weapon.ShotGun != null && weapon.ShotGun.bullet != null)
                        ConfigureTemplate(weapon.ShotGun.bullet.gameObject);
                }
                catch { }

                try
                {
                    if (weapon.knife != null && weapon.knife.bullet != null)
                        ConfigureTemplate(weapon.knife.bullet.gameObject);
                }
                catch { }
            }

            // Other players fire their replicated visual bullets through WeaponSync.
            // Decorating those templates gives remote shots the same per-pellet tracer.
            UnityEngine.Object[] syncObjects = UnityEngine.Object.FindObjectsOfType(typeof(WeaponSync));
            for (int i = 0; i < syncObjects.Length; i++)
            {
                WeaponSync sync = syncObjects[i] as WeaponSync;
                if (sync == null) continue;
                _weaponSyncs.Add(sync);

                try
                {
                    if (sync.bullet != null) ConfigureTemplate(sync.bullet);
                }
                catch { }
            }
        }

        private static void ConfigureTemplate(GameObject go)
        {
            if (go == null) return;

            // Register before AddComponent: Awake executes immediately on the template,
            // while an instantiated clone receives a new instance ID and emits a tracer.
            _templateIds.Add(go.GetInstanceID());
            RemoveLegacyTrail(go);

            if (go.GetComponent<CNRBulletRayTracer>() == null)
                go.AddComponent<CNRBulletRayTracer>();
            if (go.GetComponent<CNRDamageNumberProbe>() == null)
                go.AddComponent<CNRDamageNumberProbe>();
        }

        private static void DecorateLiveBullets()
        {
            UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(Bullet));
            for (int i = 0; i < objects.Length; i++)
            {
                Bullet bullet = objects[i] as Bullet;
                if (bullet == null || bullet.gameObject == null) continue;
                if (_templateIds.Contains(bullet.gameObject.GetInstanceID())) continue;

                RemoveLegacyTrail(bullet.gameObject);
                if (bullet.gameObject.GetComponent<CNRBulletRayTracer>() == null)
                    bullet.gameObject.AddComponent<CNRBulletRayTracer>();
                if (bullet.gameObject.GetComponent<CNRDamageNumberProbe>() == null)
                    bullet.gameObject.AddComponent<CNRDamageNumberProbe>();
            }
        }

        private static void RemoveLegacyTrail(GameObject go)
        {
            if (go == null) return;

            try
            {
                TrailRenderer trail = go.GetComponent<TrailRenderer>();
                if (trail != null) UnityEngine.Object.Destroy(trail);
            }
            catch { }

            try
            {
                Transform oldAnchor = go.transform.Find("CNR_BulletTrailAnchor");
                if (oldAnchor != null) UnityEngine.Object.Destroy(oldAnchor.gameObject);
            }
            catch { }
        }

        public static bool IsTemplate(GameObject go)
        {
            return go != null && _templateIds.Contains(go.GetInstanceID());
        }

        public static void EmitTracer(GameObject projectile)
        {
            if (projectile == null || IsTemplate(projectile)) return;

            try
            {
                Bullet bullet = projectile.GetComponent<Bullet>();
                if (bullet == null) return;

                Vector3 rayStart = projectile.transform.position;
                Vector3 rayDirection = projectile.transform.forward;
                if (rayDirection.sqrMagnitude < 0.0001f) return;
                rayDirection.Normalize();

                // Knife/melee attacks also instantiate Bullet objects in vanilla. They
                // should never get firearm tracers, even when the live-bullet fallback
                // discovers them after they have spawned.
                if (IsMeleeShot(bullet, rayStart, rayDirection)) return;

                // Vanilla Bullet.Update advances by speed * deltaTime * 10 until life
                // expires. Raycasting that full possible travel gives the same first
                // collision for normal static geometry and player hitboxes.
                float maxDistance = Mathf.Max(1f, Mathf.Abs((float)bullet.speed) * Mathf.Max(0.01f, bullet.life) * 10f);
                Vector3 endpoint = rayStart + rayDirection * maxDistance;
                RaycastHit hitInfo = default(RaycastHit);
                if (Physics.Raycast(rayStart, rayDirection, out hitInfo, maxDistance, VanillaBulletRayMask))
                    endpoint = hitInfo.point;

                Vector3 visualStart = ResolveVisualMuzzle(rayStart, rayDirection);
                CreateLine(visualStart, endpoint);
            }
            catch { }
        }

        private static bool IsMeleeShot(Bullet bullet, Vector3 projectileStart, Vector3 shotDirection)
        {
            // Local melee is definitive: only the selected/active local WeaponScript is enabled,
            // and vanilla marks projectiles spawned by it with shooter="player". This avoids the
            // old muzzle-distance guess accidentally decorating a ballistic knife/melee swing.
            if (bullet != null && bullet.shooter == "player")
            {
                for (int i = 0; i < _weaponScripts.Count; i++)
                {
                    WeaponScript local = _weaponScripts[i];
                    if (local == null || !local.gameObject.activeInHierarchy) continue;
                    try
                    {
                        if (local.transform.root != null && local.transform.root.tag == "Player" &&
                            local.GunType == WeaponScript.gunType.KNIFE)
                            return true;
                    }
                    catch { }
                }
            }

            // Remote/fallback path: retain source matching for old replicated melee bullets.
            float bestScore = float.MaxValue;
            WeaponScript bestWeapon = null;
            for (int i = 0; i < _weaponScripts.Count; i++)
            {
                WeaponScript weapon = _weaponScripts[i];
                if (weapon == null || weapon.firePoint == null || !weapon.gameObject.activeInHierarchy) continue;
                float score = SourceScore(weapon.firePoint, projectileStart, shotDirection);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestWeapon = weapon;
                }
            }
            return bestScore <= 1.0f && bestWeapon != null && bestWeapon.GunType == WeaponScript.gunType.KNIFE;
        }

        private static Vector3 ResolveVisualMuzzle(Vector3 projectileStart, Vector3 shotDirection)
        {
            float bestScore = float.MaxValue;
            WeaponScript bestWeapon = null;
            WeaponSync bestSync = null;

            // Instantiate() runs before vanilla restores firePoint.rotation, so during
            // this Awake both firePoint position and direction identify the firing gun.
            for (int i = 0; i < _weaponScripts.Count; i++)
            {
                WeaponScript weapon = _weaponScripts[i];
                if (weapon == null || weapon.firePoint == null || !weapon.gameObject.activeInHierarchy) continue;

                float score = SourceScore(weapon.firePoint, projectileStart, shotDirection);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestWeapon = weapon;
                    bestSync = null;
                }
            }

            for (int i = 0; i < _weaponSyncs.Count; i++)
            {
                WeaponSync sync = _weaponSyncs[i];
                if (sync == null || sync.firePoint == null || !sync.gameObject.activeInHierarchy) continue;

                float score = SourceScore(sync.firePoint, projectileStart, shotDirection);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestWeapon = null;
                    bestSync = sync;
                }
            }

            // A valid firing source should be essentially coincident with the projectile
            // spawn. Reject a loose match rather than drawing from another player's gun.
            if (bestScore > 1.0f) return projectileStart;
            if (bestWeapon != null) return GetWeaponMuzzle(bestWeapon, projectileStart, shotDirection);
            if (bestSync != null) return GetSyncMuzzle(bestSync, projectileStart, shotDirection);
            return projectileStart;
        }

        private static float SourceScore(Transform firePoint, Vector3 projectileStart, Vector3 shotDirection)
        {
            Vector3 delta = firePoint.position - projectileStart;
            float distanceScore = delta.sqrMagnitude;
            float dot = Vector3.Dot(firePoint.forward.normalized, shotDirection);
            float directionPenalty = Mathf.Max(0f, 1f - dot) * 0.10f;
            return distanceScore + directionPenalty;
        }

        private static Vector3 GetWeaponMuzzle(WeaponScript weapon, Vector3 fallback, Vector3 shotDirection)
        {
            // During the take-in animation the rendered gun can swing through extreme
            // positions/rotations. For the first few tenths of a second after enable,
            // use a stable first-person muzzle anchor instead of sampling that moving
            // model. The real projectile endpoint remains unchanged.
            try
            {
                CNRBulletMuzzleState muzzleState = weapon.GetComponent<CNRBulletMuzzleState>();
                if (muzzleState != null && Time.time - muzzleState.EnabledAt < WeaponMuzzleSettleTime &&
                    weapon.transform.root != null && weapon.transform.root.tag == "Player")
                    return GetStableFirstPersonMuzzle(fallback);
            }
            catch { }

            try
            {
                if (weapon.GunType == WeaponScript.gunType.MACHINE_GUN && weapon.machineGun != null && weapon.machineGun.muzzleFlash != null)
                    return weapon.machineGun.muzzleFlash.transform.position;
            }
            catch { }

            try
            {
                if (weapon.GunType == WeaponScript.gunType.SHOTGUN && weapon.ShotGun != null && weapon.ShotGun.smoke != null)
                    return weapon.ShotGun.smoke.transform.position;
            }
            catch { }

            Vector3 boundsMuzzle;
            if (TryGetFrontOfRenderedWeapon(weapon.gameObject, shotDirection, out boundsMuzzle))
                return boundsMuzzle;

            // Last-resort local approximation. The real endpoint is still exact; this
            // only moves the cosmetic start toward the visible bottom-right weapon.
            try
            {
                if (weapon.transform.root != null && weapon.transform.root.tag == "Player")
                    return GetStableFirstPersonMuzzle(fallback);
            }
            catch { }

            return fallback;
        }

        private static Vector3 GetStableFirstPersonMuzzle(Vector3 fallback)
        {
            try
            {
                if (Camera.main == null) return fallback;
                float depth = Camera.main.nearClipPlane + 0.45f;
                return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.64f, Screen.height * 0.42f, depth));
            }
            catch { return fallback; }
        }

        private static Vector3 GetSyncMuzzle(WeaponSync sync, Vector3 fallback, Vector3 shotDirection)
        {
            try
            {
                if (sync.muzzleFlash != null) return sync.muzzleFlash.transform.position;
            }
            catch { }

            Vector3 boundsMuzzle;
            if (TryGetFrontOfRenderedWeapon(sync.gameObject, shotDirection, out boundsMuzzle))
                return boundsMuzzle;
            return fallback;
        }

        private static bool TryGetFrontOfRenderedWeapon(GameObject root, Vector3 forward, out Vector3 point)
        {
            point = Vector3.zero;
            if (root == null) return false;

            try
            {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                if (renderers == null || renderers.Length == 0) return false;

                bool haveBounds = false;
                Bounds bounds = default(Bounds);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null) continue;
                    if (!haveBounds)
                    {
                        bounds = renderer.bounds;
                        haveBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
                if (!haveBounds) return false;

                Vector3 dir = forward.normalized;
                Vector3 ext = bounds.extents;
                float projectedExtent = Mathf.Abs(dir.x) * ext.x + Mathf.Abs(dir.y) * ext.y + Mathf.Abs(dir.z) * ext.z;
                point = bounds.center + dir * projectedExtent;
                return true;
            }
            catch { return false; }
        }

        private static void CreateLine(Vector3 start, Vector3 end)
        {
            if ((end - start).sqrMagnitude < 0.0001f) return;

            EnsureMaterial();
            GameObject lineObject = new GameObject("CNR_BulletTracerLine");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            if (line == null)
            {
                UnityEngine.Object.Destroy(lineObject);
                return;
            }

            if (_tracerMaterial != null) line.sharedMaterial = _tracerMaterial;
            line.useWorldSpace = true;
            line.SetVertexCount(2);
            line.SetWidth(TracerStartWidth, TracerEndWidth);
            Color tracerColor = new Color(1f, 0.82f, 0.38f, TracerStartAlpha);
            line.SetColors(tracerColor, tracerColor);
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = true;

            CNRBulletTracerLifetime lifetime = lineObject.AddComponent<CNRBulletTracerLifetime>();
            if (lifetime != null)
            {
                lifetime.Lifetime = TracerLifetime;
                lifetime.StartAlpha = TracerStartAlpha;
                lifetime.Line = line;
            }
            else UnityEngine.Object.Destroy(lineObject, TracerLifetime);
        }

        private static void EnsureMaterial()
        {
            if (_tracerMaterial != null) return;

            Shader shader = Shader.Find("Particles/Alpha Blended");
            if (shader == null) shader = Shader.Find("Particles/Additive");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) return;

            _tracerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _tracerTexture.name = "CNR_BulletTracerTexture";
            _tracerTexture.SetPixel(0, 0, Color.white);
            _tracerTexture.Apply(false, true);
            _tracerTexture.hideFlags = HideFlags.HideAndDontSave;

            _tracerMaterial = new Material(shader);
            _tracerMaterial.name = "CNR_BulletTracerMaterial";
            _tracerMaterial.mainTexture = _tracerTexture;
            // Per-tracer opacity/fade is driven through LineRenderer vertex colors.
            // Keep the shared material fully opaque so one tracer never changes another.
            _tracerMaterial.color = new Color(1f, 0.82f, 0.38f, 1f);
            _tracerMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    // Replaces vanilla Bullet on the local player's ballistic weapon templates. Vanilla
    // treats every EnemyHeadTag hit as exactly 1000 damage, regardless of the weapon's
    // actual bulletDamage. This keeps vanilla movement/hit routing but applies a per-weapon
    // multiplier to the real rolled body damage instead.
    public static class CNRVanillaHeadshotBalance
    {
        public static void ConfigureWeapon(WeaponScript weapon)
        {
            if (weapon == null || weapon.transform == null || weapon.transform.root == null) return;
            if (weapon.transform.root.tag != "Player") return;

            string weaponName = weapon.weaponName ?? "";
            if (weapon.GunType == WeaponScript.gunType.MACHINE_GUN && weapon.machineGun != null && weapon.machineGun.bullet != null)
                weapon.machineGun.bullet = PrepareTemplate(weapon.machineGun.bullet, weaponName);
            else if (weapon.GunType == WeaponScript.gunType.SHOTGUN && weapon.ShotGun != null && weapon.ShotGun.bullet != null)
                weapon.ShotGun.bullet = PrepareTemplate(weapon.ShotGun.bullet, weaponName);
            else if (weapon.GunType == WeaponScript.gunType.KNIFE && weapon.knife != null && weapon.knife.bullet != null)
                weapon.knife.bullet = PrepareTemplate(weapon.knife.bullet, weaponName);
        }

        private static Transform PrepareTemplate(Transform original, string weaponName)
        {
            if (original == null) return null;
            if (original.GetComponent<CNRBussinBullet>() != null) return original;

            CNRBalancedVanillaBullet existing = original.GetComponent<CNRBalancedVanillaBullet>();
            if (existing != null && existing.WeaponName == weaponName) return original;

            GameObject template = UnityEngine.Object.Instantiate(original.gameObject) as GameObject;
            if (template == null) return original;
            template.name = "CNR_Headshot_" + SanitizeName(weaponName) + "_BulletTemplate";
            template.transform.position = new Vector3(0f, -10000f, 0f);

            CNRBalancedVanillaBullet clonedBalanced = template.GetComponent<CNRBalancedVanillaBullet>();
            if (clonedBalanced != null)
            {
                clonedBalanced.WeaponName = weaponName;
                return template.transform;
            }

            Bullet source = template.GetComponent<Bullet>();
            if (source == null)
            {
                UnityEngine.Object.Destroy(template);
                return original;
            }

            int speed = source.speed;
            float life = source.life;
            int damage = source.damage;
            int impactForce = source.impactForce;
            bool impactHoles = source.impactHoles;
            bool knifeHoles = source.knifeHoles;
            bool doDamage = source.doDamage;
            List<GameObject> impactObjects = source.impactObjects;
            Transform bloodParticleEffect = source.bloodParticleEffect;
            string onlinePlayerTag = source.onlinePlayerTag;
            float bulletDamage = source.bulletDamage;
            string shooter = source.shooter;

            UnityEngine.Object.DestroyImmediate(source);
            CNRBalancedVanillaBullet custom = template.AddComponent<CNRBalancedVanillaBullet>();
            custom.speed = speed;
            custom.life = life;
            custom.damage = damage;
            custom.impactForce = impactForce;
            custom.impactHoles = impactHoles;
            custom.knifeHoles = knifeHoles;
            custom.doDamage = doDamage;
            custom.impactObjects = impactObjects;
            custom.bloodParticleEffect = bloodParticleEffect;
            custom.onlinePlayerTag = onlinePlayerTag;
            custom.bulletDamage = bulletDamage;
            custom.shooter = shooter;
            custom.WeaponName = weaponName;

            ModEntry.Log("HeadshotBalance: " + weaponName + " x" + GetMultiplier(weaponName).ToString("0.00"));
            return template.transform;
        }

        public static float GetMultiplier(string weaponName)
        {
            switch (weaponName)
            {
                case "Deagle": return 1.90f;
                case "GLOCK21": return 1.75f;

                case "MP5KA5": return 1.50f;
                case "UZI": return 1.50f;
                case "RAZER": return 1.50f;
                case "SantaGun": return 1.50f;

                case "MP5KA4": return 1.60f;
                case "STW-25": return 1.60f;
                case "G36K": return 1.60f;
                case "G36K1": return 1.60f;
                case "CandyRifle": return 1.60f;
                case "AUG": return 1.55f;
                case "M1Carbine": return 1.75f;
                case "TeslaP1": return 1.50f;

                case "M249": return 1.35f;
                case "M134": return 1.25f;

                case "Blaser R93": return 1.50f;
                case "ChristmasSniper": return 1.50f;
                case "FRF2": return 1.50f;

                case "M87T": return 1.25f;
                case "M3": return 1.25f;

                case "BallisticKnife": return 1.50f;
                case "GingerbreadKnife": return 1.50f;
            }
            return 1.50f;
        }

        private static string SanitizeName(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return "Unknown";
            return weaponName.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        }
    }

    public class CNRBalancedVanillaBullet : Bullet
    {
        public string WeaponName = "";

        private Vector3 _velocity;
        private Vector3 _newPos;
        private Vector3 _oldPos;
        private bool _hasHit;
        private bool _armed;

        public override void Start()
        {
            _armed = shooter == "player";
            if (!_armed) return;

            _newPos = transform.position;
            _oldPos = _newPos;
            _velocity = speed * transform.forward;
            UnityEngine.Object.Destroy(gameObject, life);
        }

        public override void Update()
        {
            if (!_armed || _hasHit) return;

            _newPos += _velocity * Time.deltaTime * 10f;
            Vector3 direction = _newPos - _oldPos;
            float magnitude = direction.magnitude;
            if (magnitude > 0f)
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(_oldPos, direction, out hitInfo, magnitude, 19))
                {
                    _newPos = hitInfo.point;
                    _hasHit = true;
                    Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

                    if (hitInfo.rigidbody != null)
                        hitInfo.rigidbody.AddForce(transform.forward * impactForce, ForceMode.Impulse);

                    string tag = hitInfo.transform != null ? hitInfo.transform.tag : "";
                    if (PlayerPrefs.GetInt("GameQualityLevel", 3) == 3 && impactHoles && impactObjects != null)
                    {
                        if (tag == "City" && impactObjects.Count > 0)
                            UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point, hitRotation);
                        else if (IsEnemyHitTag(tag) && impactObjects.Count > 1)
                            UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, hitRotation);
                    }

                    if (knifeHoles && impactObjects != null)
                    {
                        if (tag == "City" && impactObjects.Count > 0)
                            UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point,
                                hitRotation * Quaternion.Euler(0f, 90f, 0f));
                        else if (IsEnemyHitTag(tag) && impactObjects.Count > 1)
                            UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, hitRotation);
                    }

                    if (tag == "EnemyTag")
                    {
                        if (shooter == "player")
                            hitInfo.transform.SendMessageUpwards("decreaseBlood", bulletDamage, SendMessageOptions.DontRequireReceiver);
                        hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", true, SendMessageOptions.DontRequireReceiver);
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessage("OnDamaged", bulletDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "EnemyHeadTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                        {
                            int headDamage = Mathf.Max(1, Mathf.RoundToInt(bulletDamage * CNRVanillaHeadshotBalance.GetMultiplier(WeaponName)));
                            hitInfo.transform.SendMessageUpwards("OnDamaged", headDamage, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else if (tag == "EnemyBodyTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessageUpwards("OnDamaged", bulletDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "EnemyFootTag")
                    {
                        if (onlinePlayerTag == string.Empty)
                            hitInfo.transform.SendMessageUpwards("OnDamaged", bulletDamage * 0.7f, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (tag == "Player")
                    {
                        hitInfo.transform.SendMessage("PlayerDamage", bulletDamage, SendMessageOptions.DontRequireReceiver);
                    }
                    else if (hitInfo.transform != null)
                    {
                        hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", false, SendMessageOptions.DontRequireReceiver);
                    }

                    UnityEngine.Object.Destroy(gameObject, 1f);
                }
            }

            _oldPos = transform.position;
            transform.position = _newPos;
        }

        private static bool IsEnemyHitTag(string tag)
        {
            return tag == "EnemyTag" || tag == "EnemyHeadTag" || tag == "EnemyBodyTag" || tag == "EnemyFootTag";
        }
    }

    // Tracks when a weapon object becomes active so its take-in animation cannot throw
    // the cosmetic tracer muzzle wildly across the screen on an immediate shot.
    public class CNRBulletMuzzleState : MonoBehaviour
    {
        public float EnabledAt;

        private void OnEnable()
        {
            EnabledAt = Time.time;
        }
    }

    // Added to bullet templates. The template itself is registered by instance ID, while
    // an instantiated clone receives a new ID and emits exactly one tracer from Awake().
    public class CNRBulletRayTracer : MonoBehaviour
    {
        private void Awake()
        {
            if (CNRBulletTrailSystem.IsTemplate(gameObject)) return;
            CNRBulletTrailSystem.EmitTracer(gameObject);
        }
    }

    public class CNRBulletTracerLifetime : MonoBehaviour
    {
        public float Lifetime = 1.00f;
        public float StartAlpha = 0.60f;
        public LineRenderer Line;

        private float _bornAt;

        private void Start()
        {
            _bornAt = Time.time;
            UnityEngine.Object.Destroy(gameObject, Mathf.Max(0.01f, Lifetime));
        }

        private void Update()
        {
            if (Line == null) return;
            float duration = Mathf.Max(0.01f, Lifetime);
            float t = Mathf.Clamp01((Time.time - _bornAt) / duration);
            float alpha = Mathf.Lerp(StartAlpha, 0f, t);
            Color color = new Color(1f, 0.82f, 0.38f, alpha);
            Line.SetColors(color, color);
        }
    }
}
