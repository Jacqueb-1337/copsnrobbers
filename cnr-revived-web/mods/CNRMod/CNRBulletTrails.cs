using UnityEngine;

namespace CNRMods
{
    // Adds a purely visual tracer to the game's existing traveling Bullet objects.
    // Damage ownership is intentionally untouched: authoritative bullets still deal
    // damage and remote/non-authoritative bullets keep their vanilla onlinePlayerTag.
    public class CNRBulletTrailSystem : MonoBehaviour
    {
        private const float TrailLifetime = 0.12f;
        private const float TrailStartWidth = 0.045f;
        private const float TrailEndWidth = 0.008f;
        private const float TemplateScanInterval = 0.50f;
        private const float LiveFallbackScanInterval = 0.05f;

        private static Material _trailMaterial;
        private static Texture2D _trailTexture;
        private float _nextTemplateScan;
        private float _nextLiveScan;

        private void Start()
        {
            EnsureMaterial();
            DecorateWeaponTemplates();
            DecorateLiveBullets();
        }

        private void Update()
        {
            // Put the TrailRenderer on the weapon's bullet template whenever possible.
            // The instantiated projectile is then born with the trail, which matters
            // because vanilla bullets can cross a large part of the map in one frame.
            if (Time.time >= _nextTemplateScan)
            {
                _nextTemplateScan = Time.time + TemplateScanInterval;
                DecorateWeaponTemplates();
            }

            // Fallback for bullets created from a template we did not discover. This is
            // intentionally throttled so the visual feature does not add a full scene
            // FindObjectsOfType allocation every rendered frame.
            if (Time.time >= _nextLiveScan)
            {
                _nextLiveScan = Time.time + LiveFallbackScanInterval;
                DecorateLiveBullets();
            }
        }

        private static void DecorateWeaponTemplates()
        {
            UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(WeaponScript));
            for (int i = 0; i < objects.Length; i++)
            {
                WeaponScript weapon = objects[i] as WeaponScript;
                if (weapon == null) continue;

                try
                {
                    if (weapon.machineGun != null && weapon.machineGun.bullet != null)
                        ConfigureTrail(weapon.machineGun.bullet.gameObject);
                }
                catch { }

                try
                {
                    if (weapon.ShotGun != null && weapon.ShotGun.bullet != null)
                        ConfigureTrail(weapon.ShotGun.bullet.gameObject);
                }
                catch { }
            }
        }

        private static void DecorateLiveBullets()
        {
            UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(Bullet));
            for (int i = 0; i < objects.Length; i++)
            {
                Bullet bullet = objects[i] as Bullet;
                if (bullet == null || bullet.gameObject == null) continue;
                ConfigureTrail(bullet.gameObject);
            }
        }

        private static void ConfigureTrail(GameObject go)
        {
            if (go == null || go.GetComponent<CNRBulletTrailMarker>() != null) return;

            try
            {
                TrailRenderer trail = go.GetComponent<TrailRenderer>();
                if (trail == null) trail = go.AddComponent<TrailRenderer>();
                if (trail == null) return;

                EnsureMaterial();
                if (_trailMaterial != null) trail.sharedMaterial = _trailMaterial;
                trail.time = TrailLifetime;
                trail.startWidth = TrailStartWidth;
                trail.endWidth = TrailEndWidth;
                trail.autodestruct = false;
                trail.enabled = true;

                // Add the marker last. If an old Unity prefab refuses one of the trail
                // assignments, the fallback scan gets another chance instead of marking
                // a half-configured projectile as complete.
                go.AddComponent<CNRBulletTrailMarker>();
            }
            catch { }
        }

        private static void EnsureMaterial()
        {
            if (_trailMaterial != null) return;

            Shader shader = Shader.Find("Particles/Additive");
            if (shader == null) shader = Shader.Find("Particles/Alpha Blended");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) return;

            _trailTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _trailTexture.name = "CNR_BulletTrailTexture";
            _trailTexture.SetPixel(0, 0, Color.white);
            _trailTexture.Apply(false, true);
            _trailTexture.hideFlags = HideFlags.HideAndDontSave;

            _trailMaterial = new Material(shader);
            _trailMaterial.name = "CNR_BulletTrailMaterial";
            _trailMaterial.mainTexture = _trailTexture;
            _trailMaterial.color = new Color(1f, 0.82f, 0.38f, 0.95f);
            _trailMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    // Marker component prevents us from repeatedly reconfiguring the same template or
    // projectile. Clones inherit it together with the TrailRenderer.
    public class CNRBulletTrailMarker : MonoBehaviour
    {
    }
}
