using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods
{
    [Serializable]
    internal class CNRDLCMapAtlasEntry
    {
        public string id = "";
        public int x;
        public int y;
        public int w;
        public int h;
    }

    [Serializable]
    internal class CNRDLCMapAtlas
    {
        public int width;
        public int height;
        public string pngBase64 = "";
        public CNRDLCMapAtlasEntry[] entries = new CNRDLCMapAtlasEntry[0];
    }

    [Serializable]
    internal class CNRDLCMeshData
    {
        public float[] vertices = new float[0];
        public float[] uv = new float[0];
        public int[] triangles = new int[0];
    }

    [Serializable]
    internal class CNRDLCPackedBlob
    {
        public string encoding = "";
        public string dataBase64 = "";
        public int count;
        public int rawBytes;
    }

    [Serializable]
    internal class CNRDLCTiledRenderGroup
    {
        public string texture = "";
        public CNRDLCPackedBlob[] packed = new CNRDLCPackedBlob[0];
        [NonSerialized] public CNRDLCMeshData[] decoded = new CNRDLCMeshData[0];
    }

    [Serializable]
    internal class CNRDLCMapChunk
    {
        public int x;
        public int y;
        public int z;
        public CNRDLCMeshData[] opaque = new CNRDLCMeshData[0];
        public CNRDLCMeshData[] cutout = new CNRDLCMeshData[0];
        public CNRDLCMeshData[] transparent = new CNRDLCMeshData[0];
        public CNRDLCMeshData[] collision = new CNRDLCMeshData[0];
        [NonSerialized] public float[][] decodedCollisionBoxes;
        [NonSerialized] public float[][] decodedBulletPassThroughBoxes;
        [NonSerialized] public float[][] decodedClimbableBoxes;
        [NonSerialized] public float[][] decodedWaterBoxes;
        public CNRDLCPackedBlob[] opaquePacked = new CNRDLCPackedBlob[0];
        public CNRDLCPackedBlob[] cutoutPacked = new CNRDLCPackedBlob[0];
        public CNRDLCPackedBlob[] transparentPacked = new CNRDLCPackedBlob[0];
        public CNRDLCTiledRenderGroup[] opaqueTiled = new CNRDLCTiledRenderGroup[0];
        public CNRDLCTiledRenderGroup[] cutoutTiled = new CNRDLCTiledRenderGroup[0];
        public CNRDLCTiledRenderGroup[] transparentTiled = new CNRDLCTiledRenderGroup[0];
        public CNRDLCPackedBlob collisionBoxesPacked;
        public CNRDLCPackedBlob bulletPassThroughBoxesPacked;
        public CNRDLCPackedBlob climbableBoxesPacked;
        public CNRDLCPackedBlob waterBoxesPacked;
    }

    [Serializable]
    internal class CNRDLCMapFile
    {
        public string format = "";
        public int version;
        public string id = "";
        public string name = "";
        public string source = "";
        public float blockScale = 1f;
        public float[] origin = new float[] { 0f, 0f, 0f };
        public CNRDLCMapAtlas atlas;
        public CNRDLCMapChunk[] chunks = new CNRDLCMapChunk[0];
        public float[][] spawns = new float[0][];
        public float[][] copSpawns = new float[0][];
        public float[][] robberSpawns = new float[0][];
    }

    // Dedicated map path for exported/baked DLC maps. This intentionally does not
    // share the legacy donor-object cloning pipeline in MapLoader.
    internal class CNRDLCProjectileBarrierIgnore : MonoBehaviour
    {
        private int _appliedBarrierCount = -1;

        void Start() { ApplyNow(); }
        void OnEnable() { _appliedBarrierCount = -1; }

        void Update()
        {
            if (_appliedBarrierCount != CNRDLCProjectilePassThrough.BarrierCount)
                ApplyNow();
        }

        internal void ApplyNow()
        {
            List<Collider> barriers = CNRDLCProjectilePassThrough.Barriers;
            if (barriers == null || barriers.Count == 0) return;

            Collider[] mine = gameObject.GetComponentsInChildren<Collider>(true);
            for (int m = 0; m < mine.Length; m++)
            {
                Collider projectileCollider = mine[m];
                if (projectileCollider == null || !projectileCollider.enabled || !projectileCollider.gameObject.activeInHierarchy) continue;
                for (int b = 0; b < barriers.Count; b++)
                {
                    Collider barrier = barriers[b];
                    if (barrier == null || !barrier.enabled || !barrier.gameObject.activeInHierarchy) continue;
                    try { Physics.IgnoreCollision(projectileCollider, barrier); }
                    catch { }
                }
            }
            _appliedBarrierCount = barriers.Count;
        }
    }

    internal class CNRDLCProjectilePassThrough : MonoBehaviour
    {
        // Dedicated high layer used only by invisible Minecraft barrier collision.
        // Vanilla Bullet raycasts use mask 19, so they never see this layer. Player
        // physics is explicitly enabled against it below. Rigidbody projectiles stay
        // on their original layers and ignore only these exact barrier colliders.
        internal const int BarrierLayer = 30;
        private static readonly List<Collider> _barriers = new List<Collider>();
        private static CNRDLCProjectilePassThrough _activeOwner;

        private float _nextLiveProjectileScanAt;
        private bool _projectilePrefabsConfigured;
        private Type _weaponScriptType;
        private Type _weaponSyncType;
        private Type _projectileType;

        internal static List<Collider> Barriers { get { return _barriers; } }
        internal static int BarrierCount { get { return _barriers.Count; } }

        internal static void RegisterBarrier(Collider collider)
        {
            if (collider != null && !_barriers.Contains(collider)) _barriers.Add(collider);
        }

        void Awake()
        {
            _activeOwner = this;
            _barriers.Clear();

            // Layer 30 is reserved by CNRMod for DLC barriers. Force physical
            // collision with normal scene layers; projectile-specific pass-through
            // is handled per collider and does not alter player collision.
            try
            {
                for (int layer = 0; layer < 32; layer++)
                    Physics.IgnoreLayerCollision(BarrierLayer, layer, false);
            }
            catch (Exception ex) { ModEntry.Log("DLCMap barrier layer setup warning: " + ex.Message); }
        }

        void Start()
        {
            ConfigurePlayerCollision();
            ConfigureProjectilePrefabs();
            ConfigureLiveProjectiles();
            _nextLiveProjectileScanAt = Time.time + 2.0f;
        }

        void Update()
        {
            if (Time.time < _nextLiveProjectileScanAt) return;
            _nextLiveProjectileScanAt = Time.time + 2.0f;
            ConfigureLiveProjectiles();
        }

        void OnDestroy()
        {
            if (_activeOwner == this)
            {
                _barriers.Clear();
                _activeOwner = null;
            }
        }

        private Type ResolveGameType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        private void ConfigurePlayerCollision()
        {
            try
            {
                UnityEngine.Object[] controllers = FindObjectsOfType(typeof(CharacterController));
                for (int i = 0; i < controllers.Length; i++)
                {
                    Component c = controllers[i] as Component;
                    if (c != null)
                        Physics.IgnoreLayerCollision(BarrierLayer, c.gameObject.layer, false);
                }
            }
            catch (Exception ex) { ModEntry.Log("DLCMap barrier/player collision warning: " + ex.Message); }
        }

        private void ConfigureProjectilePrefabs()
        {
            if (_projectilePrefabsConfigured) return;
            try
            {
                if (_weaponScriptType == null) _weaponScriptType = ResolveGameType("WeaponScript");
                if (_weaponSyncType == null) _weaponSyncType = ResolveGameType("WeaponSync");
                if (_projectileType == null) _projectileType = ResolveGameType("Projectile");

                if (_weaponScriptType != null)
                {
                    UnityEngine.Object[] scripts = Resources.FindObjectsOfTypeAll(_weaponScriptType);
                    for (int i = 0; i < scripts.Length; i++)
                    {
                        try
                        {
                            Component c = scripts[i] as Component;
                            if (c == null) continue;
                            FieldInfo glField = c.GetType().GetField("grenadeLauncher");
                            if (glField == null) continue;
                            object gl = glField.GetValue(c);
                            if (gl == null) continue;
                            FieldInfo projectileField = gl.GetType().GetField("projectile");
                            if (projectileField == null) continue;
                            Rigidbody projectile = projectileField.GetValue(gl) as Rigidbody;
                            if (projectile != null) EnsureProjectileIgnore(projectile.gameObject);
                        }
                        catch { }
                    }
                }

                if (_weaponSyncType != null)
                {
                    UnityEngine.Object[] syncs = Resources.FindObjectsOfTypeAll(_weaponSyncType);
                    for (int i = 0; i < syncs.Length; i++)
                    {
                        try
                        {
                            Component c = syncs[i] as Component;
                            if (c == null) continue;
                            FieldInfo projectileField = c.GetType().GetField("projectile");
                            if (projectileField == null) continue;
                            Rigidbody projectile = projectileField.GetValue(c) as Rigidbody;
                            if (projectile != null) EnsureProjectileIgnore(projectile.gameObject);
                        }
                        catch { }
                    }
                }

                _projectilePrefabsConfigured = true;
            }
            catch (Exception ex) { ModEntry.Log("DLCMap projectile/barrier prefab scan warning: " + ex.Message); }
        }

        private void ConfigureLiveProjectiles()
        {
            try
            {
                if (_projectileType == null) _projectileType = ResolveGameType("Projectile");
                if (_projectileType == null) return;

                // Very sparse safety fallback. Normal spawned projectiles inherit the
                // ignore component from their prefab, so there is no reason to rescan
                // every few frames.
                UnityEngine.Object[] live = FindObjectsOfType(_projectileType);
                for (int i = 0; i < live.Length; i++)
                {
                    Component c = live[i] as Component;
                    if (c != null) EnsureProjectileIgnore(c.gameObject);
                }
            }
            catch (Exception ex) { ModEntry.Log("DLCMap live projectile/barrier scan warning: " + ex.Message); }
        }

        private static void EnsureProjectileIgnore(GameObject go)
        {
            if (go == null) return;
            CNRDLCProjectileBarrierIgnore ignore = go.GetComponent<CNRDLCProjectileBarrierIgnore>();
            if (ignore != null) return;

            ignore = go.AddComponent<CNRDLCProjectileBarrierIgnore>();
            if (ignore != null && go.activeInHierarchy) ignore.ApplyNow();
        }
    }

    internal class CNRMinecraftClimbableVolume : MonoBehaviour
    {
    }

    internal class CNRMinecraftLadderController : MonoBehaviour
    {
        private const float MinecraftTicksPerSecond = 20f;
        private const float MaxHorizontalPerTick = 0.15f;
        private const float MaxDownPerTick = 0.15f;
        private const float ClimbUpPerTick = 0.20f;

        internal float blockScale = 1f;

        private readonly HashSet<Collider> _contacts = new HashSet<Collider>();
        private CharacterController _controller;
        private FPScontroller _fps;
        private Vector3 _lastPosition;
        private bool _hasLastPosition;
        private bool _wasOnClimbable;
        private float _verticalSpeed;
        private float _jumpClimbUntil;
        private float _climbIntentUntil;
        private float _contactGraceUntil;
        private readonly HashSet<int> _jumpTouchIds = new HashSet<int>();
        private GameObject _jumpButton;
        private Camera _jumpUiCamera;
        private int _contactLogBudget = 8;

        internal bool IsClimbing { get { return _wasOnClimbable || HasLiveContact(); } }

        void Awake()
        {
            CacheComponents();
            _lastPosition = transform.position;
            _hasLastPosition = true;
            DisableVanillaLadderState();
        }

        void OnEnable()
        {
            _contacts.Clear();
            _lastPosition = transform.position;
            _hasLastPosition = true;
            _wasOnClimbable = false;
            _verticalSpeed = 0f;
            _jumpClimbUntil = -1f;
            _climbIntentUntil = -1f;
            _contactGraceUntil = -1f;
            _jumpTouchIds.Clear();
            _jumpButton = null;
            _jumpUiCamera = null;
        }

        void OnDisable()
        {
            _contacts.Clear();
            _jumpTouchIds.Clear();
            RestoreNormalGravity();
            _wasOnClimbable = false;
        }

        internal void ResetForMap(float scale)
        {
            blockScale = scale > 0f ? scale : 1f;
            _contacts.Clear();
            _lastPosition = transform.position;
            _hasLastPosition = true;
            _wasOnClimbable = false;
            _verticalSpeed = 0f;
            _jumpClimbUntil = -1f;
            _climbIntentUntil = -1f;
            _contactGraceUntil = -1f;
            _jumpTouchIds.Clear();
            _jumpButton = null;
            _jumpUiCamera = null;
            DisableVanillaLadderState();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsClimbable(other)) return;
            _contacts.Add(other);
            if (_contactLogBudget-- > 0) ModEntry.Log("DLCMap: Minecraft climbable contact entered");
        }

        void OnTriggerStay(Collider other)
        {
            if (IsClimbable(other)) _contacts.Add(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null) _contacts.Remove(other);
        }

        void LateUpdate()
        {
            CacheComponents();
            DisableVanillaLadderState();

            Vector3 now = transform.position;
            if (!_hasLastPosition)
            {
                _lastPosition = now;
                _hasLastPosition = true;
                return;
            }

            bool liveContact = HasLiveContact();
            if (liveContact) _contactGraceUntil = Time.time + 0.12f;
            bool onClimbable = liveContact || (_wasOnClimbable && Time.time <= _contactGraceUntil);
            if (!onClimbable)
            {
                _jumpTouchIds.Clear();
                if (_wasOnClimbable)
                {
                    RestoreNormalGravity();
                    if (_fps != null && _fps.movement != null)
                        _fps.movement.velocity.y = _verticalSpeed;
                }
                _wasOnClimbable = false;
                _lastPosition = now;
                return;
            }

            float scale = blockScale > 0f ? blockScale : 1f;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float maxHorizontalSpeed = MaxHorizontalPerTick * MinecraftTicksPerSecond * scale;
            float maxDownSpeed = MaxDownPerTick * MinecraftTicksPerSecond * scale;
            float climbUpSpeed = ClimbUpPerTick * MinecraftTicksPerSecond * scale;

            if (!_wasOnClimbable)
            {
                _verticalSpeed = _fps != null && _fps.movement != null ? _fps.movement.velocity.y : 0f;
                if (_verticalSpeed < -maxDownSpeed) _verticalSpeed = -maxDownSpeed;
            }

            bool horizontalCollision = false;
            if (_fps != null && _fps.movement != null)
                horizontalCollision = (_fps.movement.collisionFlags & CollisionFlags.Sides) != 0;

            bool jumpPressed = DetectJumpClimbIntent();
            bool forwardClimb = DetectForwardClimbIntent();
            if (horizontalCollision || forwardClimb || jumpPressed)
                _climbIntentUntil = Time.time + 0.20f;

            if (Time.time <= _climbIntentUntil)
            {
                _verticalSpeed = climbUpSpeed;
            }
            else
            {
                float gravity = (_fps != null && _fps.movement != null) ? _fps.movement.gravity : 10f;
                _verticalSpeed -= gravity * dt;
                if (_verticalSpeed < -maxDownSpeed) _verticalSpeed = -maxDownSpeed;
            }

            bool sneaking = _fps != null && (_fps.crouch || _fps.prone);
            if (sneaking && _verticalSpeed < 0f) _verticalSpeed = 0f;

            if (_fps != null)
            {
                _fps.onLadder = false;
                _fps.grounded = false;
                if (_fps.movement != null)
                {
                    _fps.movement.enableGravity = false;
                    Vector3 v = _fps.movement.velocity;
                    v.x = Mathf.Clamp(v.x, -maxHorizontalSpeed, maxHorizontalSpeed);
                    v.y = 0f;
                    v.z = Mathf.Clamp(v.z, -maxHorizontalSpeed, maxHorizontalSpeed);
                    _fps.movement.velocity = v;
                }
            }

            Vector3 rawDelta = now - _lastPosition;
            float teleportGuard = Mathf.Max(1.5f * scale, 1.5f);
            if (rawDelta.sqrMagnitude <= teleportGuard * teleportGuard && _controller != null && _controller.enabled)
            {
                float maxHorizontalDelta = maxHorizontalSpeed * dt;
                Vector3 desiredDelta = new Vector3(
                    Mathf.Clamp(rawDelta.x, -maxHorizontalDelta, maxHorizontalDelta),
                    _verticalSpeed * dt,
                    Mathf.Clamp(rawDelta.z, -maxHorizontalDelta, maxHorizontalDelta));
                float maxVerticalCorrection = Mathf.Max(0.18f * scale, climbUpSpeed * dt * 2f);
                float correctionY = Mathf.Clamp(desiredDelta.y - rawDelta.y, -maxVerticalCorrection, maxVerticalCorrection);
                Vector3 correction = new Vector3(desiredDelta.x - rawDelta.x, correctionY, desiredDelta.z - rawDelta.z);
                if (correction.sqrMagnitude > 0.0000001f)
                    _controller.Move(correction);
            }

            _wasOnClimbable = true;
            _lastPosition = transform.position;
        }

        private bool DetectForwardClimbIntent()
        {
            try
            {
                VCAnalogJoystickBase stick = VCAnalogJoystickBase.GetInstance("stick");
                if (stick != null && stick.AxisY > 0.10f) return true;
            }
            catch { }

            if (_fps != null)
            {
                Vector3 localInput = transform.InverseTransformDirection(_fps.inputMoveDirection);
                if (localInput.z > 0.10f) return true;
            }

            try { return Input.GetAxis("Vertical") > 0.10f; }
            catch { return false; }
        }

        private bool DetectJumpClimbIntent()
        {
            bool held = _fps != null && _fps.inputJump;
            try { if (Input.GetButton("Jump")) held = true; }
            catch { }
            if (DetectHudJumpTouch()) held = true;

            try
            {
                if (PlayerPrefs.GetInt("OnJump", 0) == 1)
                {
                    _jumpClimbUntil = Time.time + 0.45f;
                    PlayerPrefs.SetInt("OnJump", 0);
                }
            }
            catch { }

            if (held) _jumpClimbUntil = Time.time + 0.12f;
            return held || Time.time <= _jumpClimbUntil;
        }

        private bool DetectHudJumpTouch()
        {
            try
            {
                CacheJumpUi();
                if (_jumpButton == null || _jumpUiCamera == null) return false;

                bool held = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _jumpTouchIds.Remove(touch.fingerId);
                        continue;
                    }

                    if (_jumpTouchIds.Contains(touch.fingerId))
                    {
                        held = true;
                        continue;
                    }

                    if (touch.phase != TouchPhase.Began) continue;
                    Ray ray = _jumpUiCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (!Physics.Raycast(ray, out hit, 100f) || hit.collider == null) continue;
                    Transform t = hit.collider.transform;
                    Transform jump = _jumpButton.transform;
                    while (t != null)
                    {
                        if (t == jump)
                        {
                            _jumpTouchIds.Add(touch.fingerId);
                            held = true;
                            break;
                        }
                        t = t.parent;
                    }
                }
                return held;
            }
            catch { return false; }
        }

        private void CacheJumpUi()
        {
            if (_jumpButton == null)
            {
                _jumpButton = GameObject.Find("Image Button(Jump)");
                if (_jumpButton == null) _jumpButton = GameObject.Find("Button(Jump)");
            }
            if (_jumpUiCamera != null || _jumpButton == null) return;

            UICamera[] uiCameras = (UICamera[])UnityEngine.Object.FindObjectsOfType(typeof(UICamera));
            Camera fallback = null;
            for (int i = 0; i < uiCameras.Length; i++)
            {
                if (uiCameras[i] == null) continue;
                Camera cam = uiCameras[i].GetComponent<Camera>();
                if (cam == null) continue;
                if (fallback == null) fallback = cam;
                if ((cam.cullingMask & (1 << _jumpButton.layer)) != 0)
                {
                    _jumpUiCamera = cam;
                    return;
                }
            }
            _jumpUiCamera = fallback;
        }

        private void CacheComponents()
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            if (_fps == null) _fps = GetComponent<FPScontroller>();
        }

        private void DisableVanillaLadderState()
        {
            LadderPlayer vanilla = GetComponent<LadderPlayer>();
            if (vanilla != null && vanilla.enabled) vanilla.enabled = false;
            if (_fps != null)
            {
                _fps.onLadder = false;
                if (!_wasOnClimbable && _fps.movement != null) _fps.movement.enableGravity = true;
            }
        }

        private void RestoreNormalGravity()
        {
            if (_fps == null) return;
            _fps.onLadder = false;
            if (_fps.movement != null) _fps.movement.enableGravity = true;
        }

        private bool HasLiveContact()
        {
            if (_contacts.Count == 0) return false;
            List<Collider> dead = null;
            foreach (Collider c in _contacts)
            {
                if (c != null && IsClimbable(c)) return true;
                if (dead == null) dead = new List<Collider>();
                dead.Add(c);
            }
            if (dead != null)
                for (int i = 0; i < dead.Count; i++) _contacts.Remove(dead[i]);
            return false;
        }

        private static bool IsClimbable(Collider other)
        {
            return other != null && other.GetComponent<CNRMinecraftClimbableVolume>() != null;
        }
    }

    internal static class CNRMinecraftWaterRegistry
    {
        private static float _blockScale = 1f;
        private static float _bucketSize = 4f;
        private static readonly Dictionary<long, List<Bounds>> _buckets = new Dictionary<long, List<Bounds>>();
        private static int _volumeCount;

        internal static int VolumeCount { get { return _volumeCount; } }

        internal static void Configure(float blockScale)
        {
            _blockScale = blockScale > 0f ? blockScale : 1f;
            _bucketSize = Mathf.Max(_blockScale * 4f, 0.25f);
            Clear();
        }

        internal static void Clear()
        {
            _buckets.Clear();
            _volumeCount = 0;
        }

        internal static void Register(Bounds bounds)
        {
            if (bounds.size.x <= 0f || bounds.size.y <= 0f || bounds.size.z <= 0f) return;

            int minX = Mathf.FloorToInt((bounds.min.x - 0.001f) / _bucketSize);
            int maxX = Mathf.FloorToInt((bounds.max.x + 0.001f) / _bucketSize);
            int minZ = Mathf.FloorToInt((bounds.min.z - 0.001f) / _bucketSize);
            int maxZ = Mathf.FloorToInt((bounds.max.z + 0.001f) / _bucketSize);
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    long key = BucketKey(x, z);
                    List<Bounds> list;
                    if (!_buckets.TryGetValue(key, out list))
                    {
                        list = new List<Bounds>();
                        _buckets[key] = list;
                    }
                    list.Add(bounds);
                }
            }
            _volumeCount++;
        }

        internal static bool TryGetSurface(Vector3 point, out float surfaceY)
        {
            surfaceY = float.MinValue;
            List<Bounds> list;
            if (!_buckets.TryGetValue(BucketKeyFor(point.x, point.z), out list)) return false;

            float edgeSlop = Mathf.Max(0.04f * _blockScale, 0.002f);
            float maxSeedDistance = 1.50f * _blockScale;

            // Find the water volume vertically nearest the player at this X/Z. This is
            // only a seed: deep pools are often exported as several stacked water boxes.
            int seedIndex = -1;
            float seedDistance = float.MaxValue;
            for (int i = 0; i < list.Count; i++)
            {
                Bounds b = list[i];
                if (point.x < b.min.x - edgeSlop || point.x > b.max.x + edgeSlop ||
                    point.z < b.min.z - edgeSlop || point.z > b.max.z + edgeSlop) continue;

                float distance = 0f;
                if (point.y < b.min.y) distance = b.min.y - point.y;
                else if (point.y > b.max.y) distance = point.y - b.max.y;
                if (distance < seedDistance)
                {
                    seedDistance = distance;
                    seedIndex = i;
                }
            }
            if (seedIndex < 0 || seedDistance > maxSeedDistance) return false;

            Bounds seed = list[seedIndex];
            float componentMin = seed.min.y;
            float componentMax = seed.max.y;
            // Minecraft water occupies almost a full block visually. Older exports stored
            // that exact ~8/9-block height for every submerged cell, leaving a small gap
            // between vertically adjacent gameplay boxes. Bridge that water-sized gap,
            // but keep a real one-block air gap disconnected.
            float connectSlop = Mathf.Max(0.15f * _blockScale, 0.01f);

            // Grow through vertically touching/overlapping water volumes at this X/Z.
            // The top of this connected component is the one real surface. An air gap
            // breaks the component, so stacked pools/fountains remain independent.
            bool expanded = true;
            while (expanded)
            {
                expanded = false;
                for (int i = 0; i < list.Count; i++)
                {
                    Bounds b = list[i];
                    if (point.x < b.min.x - edgeSlop || point.x > b.max.x + edgeSlop ||
                        point.z < b.min.z - edgeSlop || point.z > b.max.z + edgeSlop) continue;
                    if (b.max.y < componentMin - connectSlop || b.min.y > componentMax + connectSlop) continue;

                    if (b.min.y < componentMin)
                    {
                        componentMin = b.min.y;
                        expanded = true;
                    }
                    if (b.max.y > componentMax)
                    {
                        componentMax = b.max.y;
                        expanded = true;
                    }
                }
            }

            surfaceY = componentMax;
            return true;
        }

        internal static bool ContainsPoint(Vector3 point)
        {
            List<Bounds> list;
            if (!_buckets.TryGetValue(BucketKeyFor(point.x, point.z), out list)) return false;

            float edgeSlop = Mathf.Max(0.002f * _blockScale, 0.0005f);
            for (int i = 0; i < list.Count; i++)
            {
                Bounds b = list[i];
                if (point.x < b.min.x - edgeSlop || point.x > b.max.x + edgeSlop ||
                    point.y < b.min.y - edgeSlop || point.y > b.max.y + edgeSlop ||
                    point.z < b.min.z - edgeSlop || point.z > b.max.z + edgeSlop) continue;
                return true;
            }
            return false;
        }

        internal static bool Overlaps(Bounds query)
        {
            if (query.size.x <= 0f || query.size.y <= 0f || query.size.z <= 0f) return false;

            float edgeSlop = Mathf.Max(0.002f * _blockScale, 0.0005f);
            int minX = Mathf.FloorToInt((query.min.x - edgeSlop) / _bucketSize);
            int maxX = Mathf.FloorToInt((query.max.x + edgeSlop) / _bucketSize);
            int minZ = Mathf.FloorToInt((query.min.z - edgeSlop) / _bucketSize);
            int maxZ = Mathf.FloorToInt((query.max.z + edgeSlop) / _bucketSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    List<Bounds> list;
                    if (!_buckets.TryGetValue(BucketKey(x, z), out list)) continue;
                    for (int i = 0; i < list.Count; i++)
                    {
                        Bounds b = list[i];
                        if (query.max.x < b.min.x - edgeSlop || query.min.x > b.max.x + edgeSlop ||
                            query.max.y < b.min.y - edgeSlop || query.min.y > b.max.y + edgeSlop ||
                            query.max.z < b.min.z - edgeSlop || query.min.z > b.max.z + edgeSlop) continue;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool HasWaterAbove(Vector3 groundPoint, float minimumDepth)
        {
            List<Bounds> list;
            if (!_buckets.TryGetValue(BucketKeyFor(groundPoint.x, groundPoint.z), out list)) return false;

            float edgeSlop = Mathf.Max(0.04f * _blockScale, 0.002f);
            for (int i = 0; i < list.Count; i++)
            {
                Bounds b = list[i];
                if (groundPoint.x < b.min.x - edgeSlop || groundPoint.x > b.max.x + edgeSlop ||
                    groundPoint.z < b.min.z - edgeSlop || groundPoint.z > b.max.z + edgeSlop) continue;
                if (b.min.y > groundPoint.y + 0.50f) continue;
                if (b.max.y - groundPoint.y >= minimumDepth) return true;
            }
            return false;
        }

        private static long BucketKeyFor(float x, float z)
        {
            return BucketKey(Mathf.FloorToInt(x / _bucketSize), Mathf.FloorToInt(z / _bucketSize));
        }

        private static long BucketKey(int x, int z)
        {
            unchecked { return ((long)x << 32) ^ (uint)z; }
        }
    }

    internal class CNRMinecraftWaterController : MonoBehaviour
    {
        // Classic/pre-1.13 Minecraft feel: upright player, strong water slowdown,
        // slow idle sinking, and holding Jump swims upward with a small surface bob.
        // No modern crawl/swim pose or diving state.
        private const float HorizontalSpeedRatio = 0.51f;
        private const float SwimEnterSubmersion = 0.30f;
        private const float SwimExitSubmersion = -0.08f;
        private const float SurfaceHeadBand = 0.08f;
        private const float MaxSinkSpeed = 0.55f;
        private const float MaxRiseSpeed = 1.85f;
        private const float SurfaceGravity = 8.0f;
        private const float SinkResponse = 3.5f;
        private const float RiseResponse = 7.5f;

        private static bool _localPlayerInWater;
        internal static float LocalHorizontalSpeedMultiplier
        {
            get { return _localPlayerInWater ? HorizontalSpeedRatio : 1f; }
        }

        internal float blockScale = 1f;

        private readonly HashSet<int> _jumpTouchIds = new HashSet<int>();
        private CharacterController _controller;
        private FPScontroller _fps;
        private CNRMinecraftLadderController _ladder;
        private Vector3 _lastPosition;
        private bool _hasLastPosition;
        private bool _wasSwimming;
        private float _verticalSpeed;
        private float _jumpHeldUntil;
        private GameObject _jumpButton;
        private Camera _jumpUiCamera;

        void Awake()
        {
            CacheComponents();
            _lastPosition = transform.position;
            _hasLastPosition = true;
        }

        void OnEnable()
        {
            _jumpTouchIds.Clear();
            _lastPosition = transform.position;
            _hasLastPosition = true;
            _wasSwimming = false;
            _verticalSpeed = 0f;
            _jumpHeldUntil = -1f;
            _jumpButton = null;
            _jumpUiCamera = null;
            _localPlayerInWater = false;
        }

        void OnDisable()
        {
            _jumpTouchIds.Clear();
            RestoreNormalGravity();
            _wasSwimming = false;
            _localPlayerInWater = false;
        }

        internal void ResetForMap(float scale)
        {
            blockScale = scale > 0f ? scale : 1f;
            _jumpTouchIds.Clear();
            _lastPosition = transform.position;
            _hasLastPosition = true;
            _wasSwimming = false;
            _verticalSpeed = 0f;
            _jumpHeldUntil = -1f;
            _jumpButton = null;
            _jumpUiCamera = null;
            _localPlayerInWater = false;
            RestoreNormalGravity();
        }

        void LateUpdate()
        {
            CacheComponents();
            Vector3 now = transform.position;
            if (!_hasLastPosition)
            {
                _lastPosition = now;
                _hasLastPosition = true;
                return;
            }

            if (_controller == null)
            {
                LeaveWater(now);
                return;
            }

            Bounds body = _controller.bounds;
            float bodyHeight = Mathf.Max(body.size.y, 0.1f);

            // Swimming is deliberately based on direct hitbox/water overlap, not on a
            // reconstructed pool surface. Ignore only the lowest 10% of the controller
            // (the feet); touching even a sliver of water anywhere in the upper 90%
            // immediately hands vertical physics to this controller.
            float swimBottom = body.min.y + bodyHeight * 0.10f;
            Bounds swimBody = new Bounds(
                new Vector3(body.center.x, (swimBottom + body.max.y) * 0.5f, body.center.z),
                new Vector3(body.size.x, Mathf.Max(body.max.y - swimBottom, 0.01f), body.size.z));
            bool touchingWater = CNRMinecraftWaterRegistry.Overlaps(swimBody);
            _localPlayerInWater = touchingWater;
            if (!touchingWater)
            {
                LeaveWater(now);
                return;
            }

            float scale = blockScale > 0f ? blockScale : 1f;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);

            if (!_wasSwimming)
            {
                _verticalSpeed = (_fps != null && _fps.movement != null) ? _fps.movement.velocity.y : 0f;
                _verticalSpeed = Mathf.Clamp(_verticalSpeed, -MaxSinkSpeed * scale, MaxRiseSpeed * scale);
            }

            bool jumpHeld = DetectJumpIntent();
            if (jumpHeld)
            {
                _verticalSpeed = Mathf.MoveTowards(
                    _verticalSpeed,
                    MaxRiseSpeed * scale,
                    RiseResponse * scale * dt);
            }
            else
            {
                // No vanilla gravity while the upper 90% of the hitbox touches water.
                // Without upward input, settle downward at the intentionally slow water rate.
                _verticalSpeed = Mathf.MoveTowards(
                    _verticalSpeed,
                    -MaxSinkSpeed * scale,
                    SinkResponse * scale * dt);
            }

            if (_fps != null)
            {
                _fps.grounded = false;
                if (_fps.movement != null)
                {
                    _fps.movement.enableGravity = false;
                    Vector3 velocity = _fps.movement.velocity;
                    velocity.y = _verticalSpeed;
                    _fps.movement.velocity = velocity;
                }
            }

            // Vanilla CNR/legacy input already moved the CharacterController earlier in
            // the frame. Replace only the resulting vertical delta with our water delta.
            // This keeps horizontal collision/steering from the normal controller while
            // removing its gravity/jump arc inside sufficiently deep water.
            Vector3 rawDelta = now - _lastPosition;
            float teleportGuard = Mathf.Max(1.5f * scale, 1.5f);
            if (rawDelta.sqrMagnitude <= teleportGuard * teleportGuard && _controller.enabled)
            {
                float wantedY = _verticalSpeed * dt;
                // Vanilla movement already ran earlier this frame. Replace its complete
                // vertical delta while swimming; a capped correction lets vanilla gravity
                // win during deep falls and was the cause of the rapid sinking behavior.
                float correctionY = wantedY - rawDelta.y;
                if (Mathf.Abs(correctionY) > 0.00001f)
                    _controller.Move(new Vector3(0f, correctionY, 0f));
            }

            _wasSwimming = true;
            _lastPosition = transform.position;
        }

        private void LeaveWater(Vector3 now)
        {
            _jumpTouchIds.Clear();
            _localPlayerInWater = false;
            if (_wasSwimming)
            {
                RestoreNormalGravity();
                if (_fps != null && _fps.movement != null)
                {
                    Vector3 velocity = _fps.movement.velocity;
                    velocity.y = _verticalSpeed;
                    _fps.movement.velocity = velocity;
                }
            }
            _wasSwimming = false;
            _lastPosition = now;
        }

        private bool DetectJumpIntent()
        {
            bool held = _fps != null && _fps.inputJump;
            try { if (Input.GetButton("Jump")) held = true; }
            catch { }
            if (DetectHudJumpTouch()) held = true;

            try
            {
                if (PlayerPrefs.GetInt("OnJump", 0) == 1)
                {
                    _jumpHeldUntil = Time.time + 0.16f;
                    PlayerPrefs.SetInt("OnJump", 0);
                }
            }
            catch { }

            if (held) _jumpHeldUntil = Time.time + 0.12f;
            return held || Time.time <= _jumpHeldUntil;
        }

        private bool DetectHudJumpTouch()
        {
            try
            {
                CacheJumpUi();
                if (_jumpButton == null || _jumpUiCamera == null) return false;

                bool held = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _jumpTouchIds.Remove(touch.fingerId);
                        continue;
                    }
                    if (_jumpTouchIds.Contains(touch.fingerId))
                    {
                        held = true;
                        continue;
                    }
                    if (touch.phase != TouchPhase.Began) continue;

                    Ray ray = _jumpUiCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    if (!Physics.Raycast(ray, out hit, 100f) || hit.collider == null) continue;
                    Transform t = hit.collider.transform;
                    Transform jump = _jumpButton.transform;
                    while (t != null)
                    {
                        if (t == jump)
                        {
                            _jumpTouchIds.Add(touch.fingerId);
                            held = true;
                            break;
                        }
                        t = t.parent;
                    }
                }
                return held;
            }
            catch { return false; }
        }

        private void CacheJumpUi()
        {
            if (_jumpButton == null)
            {
                _jumpButton = GameObject.Find("Image Button(Jump)");
                if (_jumpButton == null) _jumpButton = GameObject.Find("Button(Jump)");
            }
            if (_jumpUiCamera != null || _jumpButton == null) return;

            UICamera[] uiCameras = (UICamera[])UnityEngine.Object.FindObjectsOfType(typeof(UICamera));
            Camera fallback = null;
            for (int i = 0; i < uiCameras.Length; i++)
            {
                if (uiCameras[i] == null) continue;
                Camera cam = uiCameras[i].GetComponent<Camera>();
                if (cam == null) continue;
                if (fallback == null) fallback = cam;
                if ((cam.cullingMask & (1 << _jumpButton.layer)) != 0)
                {
                    _jumpUiCamera = cam;
                    return;
                }
            }
            _jumpUiCamera = fallback;
        }

        private void CacheComponents()
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            if (_fps == null) _fps = GetComponent<FPScontroller>();
            if (_ladder == null) _ladder = GetComponent<CNRMinecraftLadderController>();
        }

        private bool TryGetWaterSurface(out float surfaceY)
        {
            surfaceY = float.MinValue;
            if (_controller == null) return false;
            return CNRMinecraftWaterRegistry.TryGetSurface(_controller.bounds.center, out surfaceY);
        }

        private void RestoreNormalGravity()
        {
            if (_fps != null && _fps.movement != null) _fps.movement.enableGravity = true;
        }

    }

    internal class CNRDLCChunkCollisionStreamer : MonoBehaviour
    {
        private sealed class Entry
        {
            internal GameObject root;
            internal Bounds worldBounds;
            internal bool active = true;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private CharacterController[] _anchors = new CharacterController[0];
        private float _loadRadius = 64f;
        private float _unloadRadius = 80f;
        private float _nextRefresh;
        private float _nextAnchorRefresh;

        internal void Configure(float blockScale)
        {
            float scale = blockScale > 0f ? blockScale : 1f;
            // Keep enough collision around every simulated CharacterController for long
            // sight lines/projectiles, while dropping distant map chunks out of PhysX.
            _loadRadius = 64f * scale;
            _unloadRadius = 80f * scale;
            _nextRefresh = 0f;
            _nextAnchorRefresh = 0f;
        }

        internal void Register(GameObject root, Bounds worldBounds)
        {
            if (root == null) return;
            Entry entry = new Entry();
            entry.root = root;
            entry.worldBounds = worldBounds;
            entry.active = root.activeSelf;
            _entries.Add(entry);
        }

        void Update()
        {
            if (Time.time < _nextRefresh) return;
            _nextRefresh = Time.time + 0.25f;

            if (Time.time >= _nextAnchorRefresh)
            {
                _nextAnchorRefresh = Time.time + 1f;
                _anchors = (CharacterController[])UnityEngine.Object.FindObjectsOfType(typeof(CharacterController));
            }

            bool haveAnchor = false;
            for (int i = 0; i < _anchors.Length; i++)
            {
                CharacterController anchor = _anchors[i];
                if (anchor != null && anchor.enabled && anchor.gameObject.activeInHierarchy)
                {
                    haveAnchor = true;
                    break;
                }
            }
            if (!haveAnchor) return;

            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry == null || entry.root == null) continue;
                float radius = entry.active ? _unloadRadius : _loadRadius;
                bool wanted = IsNearAnyAnchor(entry.worldBounds, radius);
                if (wanted == entry.active) continue;
                entry.root.SetActive(wanted);
                entry.active = wanted;
            }
        }

        private bool IsNearAnyAnchor(Bounds bounds, float radius)
        {
            float radiusSq = radius * radius;
            for (int i = 0; i < _anchors.Length; i++)
            {
                CharacterController anchor = _anchors[i];
                if (anchor == null || !anchor.enabled || !anchor.gameObject.activeInHierarchy) continue;
                Vector3 p = anchor.transform.position;
                float dx = p.x < bounds.min.x ? bounds.min.x - p.x : (p.x > bounds.max.x ? p.x - bounds.max.x : 0f);
                float dz = p.z < bounds.min.z ? bounds.min.z - p.z : (p.z > bounds.max.z ? p.z - bounds.max.z : 0f);
                if (dx * dx + dz * dz <= radiusSq) return true;
            }
            return false;
        }
    }

    internal class CNRDLCMapLoader : MonoBehaviour
    {
        internal const string Format = "cnr-dlc-map";
        internal const int FormatVersion = 4;
        internal const int PackedAtlasFormatVersion = 3;
        internal const int PackedLegacyFormatVersion = 2;
        internal const int LegacyFormatVersion = 1;
        internal const string PrefActive = "CNRMod_DLCMapActive";
        internal const string PrefPath = "CNRMod_DLCMapPath";
        internal const string PrefId = "CNRMod_DLCMapId";
        internal const string PrefUrl = "CNRMod_DLCMapURL";
        internal const string BootstrapScene = "FreeRun3_1";

        private const int MaxAtlasBytes = 32 * 1024 * 1024;
        private const int MaxVerticesPerPart = 65000;
        private const int MaxPackedCompressedBytes = 32 * 1024 * 1024;
        private const int MaxPackedInflatedBytes = 16 * 1024 * 1024;
        private const int MaxIndicesPerPart = 400000;
        private const int MaxCollisionBoxesPerChunk = 100000;
        private const float DefaultSpawnHeight = 50f;
        private const float QuantizedPositionScale = 1024f;
        private const float QuantizedUvScale = 65535f;
        private const float TiledUvScale = 1024f;
        private const float LegacyWaterDepthBias = 0.005f;
        private const int TiledRenderRegionSize = 64;

        private static CNRDLCMapFile _prepared;
        private static byte[] _preparedAtlasPng;
        private static string _preparedPath = "";
        private static GameObject _mapRoot;
        private static Texture2D _atlasTexture;
        private static Material _opaqueMaterial;
        private static Material _cutoutMaterial;
        private static Material _transparentMaterial;
        private static readonly Dictionary<string, Texture2D> _tiledTextures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Material> _tiledMaterials = new Dictionary<string, Material>();
        private static readonly Dictionary<string, CNRDLCMapAtlasEntry> _atlasEntriesById = new Dictionary<string, CNRDLCMapAtlasEntry>();
        private static Rect[] _waterAtlasUvRects = new Rect[0];
        private static CNRDLCChunkCollisionStreamer _collisionStreamer;
        private static int _legacyWaterBiasedQuads;
        private static int _collisionBoxesBeforeMerge;
        private static int _collisionBoxesAfterMerge;
        private static int _tiledRenderRegions;
        private static int _tiledRenderDrawCalls;

        internal static bool IsActive
        {
            get { return PlayerPrefs.GetInt(PrefActive, 0) == 1; }
        }

        internal static bool IsDlcMapJson(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return false;
            string f = ModEntry.ParseJsonStringValue(raw, "format");
            return string.Equals(f, Format, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsDlcMapFile(string path)
        {
            try { return File.Exists(path) && IsDlcMapJson(File.ReadAllText(path)); }
            catch { return false; }
        }

        internal static void SelectFile(string path, string url, string id)
        {
            // Browsing the create-room carousel must stay metadata-only. Preparing a
            // large DLC package deserializes/decompresses the entire map and can take
            // seconds on the Unity main thread. Existing join/bootstrap paths prepare it
            // once when the selected map is actually needed.
            _prepared = null;
            _preparedAtlasPng = null;
            _preparedPath = "";
            _waterAtlasUvRects = new Rect[0];
            _legacyWaterBiasedQuads = 0;
            PlayerPrefs.SetInt(PrefActive, 1);
            PlayerPrefs.SetString(PrefPath, path ?? "");
            PlayerPrefs.SetString(PrefId, id ?? "");
            PlayerPrefs.SetString(PrefUrl, url ?? "");
            PlayerPrefs.SetString("CNRMod_ActiveMapURL", url ?? "");
            PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
            PlayerPrefs.DeleteKey("CNRMod_DonorScene");
        }

        internal static bool ActivateFile(string path, string url, string id, out string reason)
        {
            // An official DLC keeps a stable cache filename across updates. Drop any
            // in-memory package before activating so a replaced file can never inherit
            // geometry/atlas data from the previous download at the same path.
            _prepared = null;
            _preparedAtlasPng = null;
            _preparedPath = "";
            _waterAtlasUvRects = new Rect[0];
            _legacyWaterBiasedQuads = 0;
            if (!PrepareFile(path, out reason)) return false;
            try
            {
                PlayerPrefs.SetInt(PrefActive, 1);
                PlayerPrefs.SetString(PrefPath, path ?? "");
                PlayerPrefs.SetString(PrefId, string.IsNullOrEmpty(id) ? (_prepared.id ?? "") : id);
                PlayerPrefs.SetString(PrefUrl, url ?? "");

                // Keep the URL for room-resource advertisement, but explicitly turn the
                // old custom-map cache path off so legacy MapLoader cannot race this one.
                PlayerPrefs.SetString("CNRMod_ActiveMapURL", url ?? "");
                PlayerPrefs.DeleteKey("CNRMod_MapCacheReady");
                PlayerPrefs.DeleteKey("CNRMod_DonorScene");
                PlayerPrefs.Save();
                ModEntry.Log("DLCMap: activated " + (id ?? "") + " path=" + path);
                return true;
            }
            catch (Exception ex)
            {
                reason = "Could not activate DLC map: " + ex.Message;
                return false;
            }
        }

        internal static void ClearActive()
        {
            _prepared = null;
            _preparedAtlasPng = null;
            _preparedPath = "";
            _waterAtlasUvRects = new Rect[0];
            _legacyWaterBiasedQuads = 0;
            CNRMinecraftWaterRegistry.Clear();
            try
            {
                PlayerPrefs.DeleteKey(PrefActive);
                PlayerPrefs.DeleteKey(PrefPath);
                PlayerPrefs.DeleteKey(PrefId);
                PlayerPrefs.DeleteKey(PrefUrl);
                PlayerPrefs.Save();
            }
            catch { }
        }

        internal static bool PrepareFile(string path, out string reason)
        {
            reason = "";
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    reason = "DLC map file is missing.";
                    return false;
                }

                // Do not reuse a prepared package solely because the cache path matches.
                // ContentManager intentionally overwrites a stable <mapId>.json path when a
                // DLC map is updated. Reusing by pathname can therefore resurrect the previous
                // map's geometry even though the file on disk has changed.
                string raw = File.ReadAllText(path);
                if (!IsDlcMapJson(raw))
                {
                    reason = "Map is not a CNR DLC map package.";
                    return false;
                }

                CNRDLCMapFile map = JsonReader.Deserialize<CNRDLCMapFile>(raw);
                if (map == null || !string.Equals(map.format, Format, StringComparison.OrdinalIgnoreCase))
                {
                    reason = "DLC map header is invalid.";
                    return false;
                }
                if (map.version != LegacyFormatVersion && map.version != PackedLegacyFormatVersion && map.version != PackedAtlasFormatVersion && map.version != FormatVersion)
                {
                    reason = "DLC map format " + map.version + " is unsupported (expected " + LegacyFormatVersion + " through " + FormatVersion + ").";
                    return false;
                }
                if (map.atlas == null || string.IsNullOrEmpty(map.atlas.pngBase64))
                {
                    reason = "DLC map does not contain an atlas.";
                    return false;
                }
                if (map.chunks == null || map.chunks.Length == 0)
                {
                    reason = "DLC map contains no geometry chunks.";
                    return false;
                }
                if (map.blockScale <= 0f || map.blockScale > 100f) map.blockScale = 1f;

                byte[] atlas = Convert.FromBase64String(map.atlas.pngBase64);
                if (atlas == null || atlas.Length == 0 || atlas.Length > MaxAtlasBytes)
                {
                    reason = "DLC map atlas is empty or too large.";
                    return false;
                }

                if ((map.version == PackedLegacyFormatVersion || map.version == PackedAtlasFormatVersion) && !HydratePackedMap(map, out reason))
                    return false;
                if (map.version == FormatVersion && !HydrateTiledMap(map, out reason))
                    return false;

                ApplyMinecraftCoordinateHandedness(map);

                for (int c = 0; c < map.chunks.Length; c++)
                {
                    CNRDLCMapChunk chunk = map.chunks[c];
                    if (chunk == null) { reason = "DLC map contains an empty chunk."; return false; }
                    if (!ValidateParts(chunk.opaque, "opaque", out reason)) return false;
                    if (!ValidateParts(chunk.cutout, "cutout", out reason)) return false;
                    if (!ValidateParts(chunk.transparent, "transparent", out reason)) return false;
                    if (!ValidateCollisionParts(chunk.collision, out reason)) return false;
                }

                _prepared = map;
                _waterAtlasUvRects = BuildWaterAtlasUvRects(map.atlas);
                _preparedAtlasPng = atlas;
                _preparedPath = path;
                ModEntry.Log("DLCMap: prebuilt " + map.chunks.Length + " chunks before join, atlas=" + atlas.Length + " bytes");
                return true;
            }
            catch (Exception ex)
            {
                reason = "DLC map could not be prepared: " + ex.Message;
                return false;
            }
        }

        private static bool HydratePackedMap(CNRDLCMapFile map, out string reason)
        {
            reason = "";
            if (map == null || map.chunks == null) { reason = "DLC map v2 chunks are missing."; return false; }
            for (int c = 0; c < map.chunks.Length; c++)
            {
                CNRDLCMapChunk chunk = map.chunks[c];
                if (chunk == null) { reason = "DLC map v2 contains an empty chunk."; return false; }
                CNRDLCMeshData[] decoded;
                if (!DecodePackedMeshes(chunk.opaquePacked, "opaque", out decoded, out reason)) return false;
                chunk.opaque = decoded;
                if (!DecodePackedMeshes(chunk.cutoutPacked, "cutout", out decoded, out reason)) return false;
                chunk.cutout = decoded;
                if (!DecodePackedMeshes(chunk.transparentPacked, "transparent", out decoded, out reason)) return false;
                chunk.transparent = decoded;
                float[][] decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.collisionBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedCollisionBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.bulletPassThroughBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedBulletPassThroughBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.climbableBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedClimbableBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.waterBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedWaterBoxes = decodedBoxes;
                chunk.collision = new CNRDLCMeshData[0];
            }
            return true;
        }

        private static bool HydrateTiledMap(CNRDLCMapFile map, out string reason)
        {
            reason = "";
            if (map == null || map.chunks == null) { reason = "DLC map v4 chunks are missing."; return false; }
            for (int c = 0; c < map.chunks.Length; c++)
            {
                CNRDLCMapChunk chunk = map.chunks[c];
                if (chunk == null) { reason = "DLC map v4 contains an empty chunk."; return false; }
                if (!HydrateTiledGroups(chunk.opaqueTiled, "opaque", out reason)) return false;
                if (!HydrateTiledGroups(chunk.cutoutTiled, "cutout", out reason)) return false;
                if (!HydrateTiledGroups(chunk.transparentTiled, "transparent", out reason)) return false;

                float[][] decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.collisionBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedCollisionBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.bulletPassThroughBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedBulletPassThroughBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.climbableBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedClimbableBoxes = decodedBoxes;
                if (!DecodeCollisionBoxes(chunk.waterBoxesPacked, out decodedBoxes, out reason)) return false;
                chunk.decodedWaterBoxes = decodedBoxes;

                chunk.opaque = new CNRDLCMeshData[0];
                chunk.cutout = new CNRDLCMeshData[0];
                chunk.transparent = new CNRDLCMeshData[0];
                chunk.collision = new CNRDLCMeshData[0];
            }
            return true;
        }

        private static bool HydrateTiledGroups(CNRDLCTiledRenderGroup[] groups, string kind, out string reason)
        {
            reason = "";
            if (groups == null) return true;
            for (int i = 0; i < groups.Length; i++)
            {
                CNRDLCTiledRenderGroup group = groups[i];
                if (group == null || string.IsNullOrEmpty(group.texture))
                {
                    reason = "DLC map " + kind + " tiled group is invalid.";
                    return false;
                }
                CNRDLCMeshData[] decoded;
                if (!DecodePackedMeshes(group.packed, kind + " tiled", out decoded, out reason)) return false;
                group.decoded = decoded;
            }
            return true;
        }

        private static void ApplyMinecraftCoordinateHandedness(CNRDLCMapFile map)
        {
            if (map == null || string.IsNullOrEmpty(map.source)
                || !map.source.StartsWith("minecraft-fabric-", StringComparison.OrdinalIgnoreCase)) return;

            // Minecraft and Unity use opposite coordinate handedness. A direct XYZ copy
            // mirrors Fabric exports. Convert map-local Z once after packed data has been
            // hydrated, and reverse triangle winding so normals/culling remain correct.
            if (map.chunks != null)
            {
                for (int c = 0; c < map.chunks.Length; c++)
                {
                    CNRDLCMapChunk chunk = map.chunks[c];
                    if (chunk == null) continue;
                    chunk.z = -chunk.z;
                    MirrorMeshPartsZ(chunk.opaque);
                    MirrorMeshPartsZ(chunk.cutout);
                    MirrorMeshPartsZ(chunk.transparent);
                    MirrorTiledGroupsZ(chunk.opaqueTiled);
                    MirrorTiledGroupsZ(chunk.cutoutTiled);
                    MirrorTiledGroupsZ(chunk.transparentTiled);
                    MirrorMeshPartsZ(chunk.collision);
                    MirrorBoxesZ(chunk.decodedCollisionBoxes);
                    MirrorBoxesZ(chunk.decodedBulletPassThroughBoxes);
                    MirrorBoxesZ(chunk.decodedClimbableBoxes);
                    MirrorBoxesZ(chunk.decodedWaterBoxes);
                }
            }

            MirrorSpawnListZ(map.spawns);
            MirrorSpawnListZ(map.copSpawns);
            MirrorSpawnListZ(map.robberSpawns);
            ModEntry.Log("DLCMap: applied Minecraft->Unity Z handedness conversion");
        }

        private static void MirrorMeshPartsZ(CNRDLCMeshData[] parts)
        {
            if (parts == null) return;
            for (int p = 0; p < parts.Length; p++)
            {
                CNRDLCMeshData data = parts[p];
                if (data == null) continue;
                if (data.vertices != null)
                    for (int i = 2; i < data.vertices.Length; i += 3) data.vertices[i] = -data.vertices[i];
                if (data.triangles != null)
                {
                    for (int i = 0; i + 2 < data.triangles.Length; i += 3)
                    {
                        int tmp = data.triangles[i + 1];
                        data.triangles[i + 1] = data.triangles[i + 2];
                        data.triangles[i + 2] = tmp;
                    }
                }
            }
        }

        private static void MirrorTiledGroupsZ(CNRDLCTiledRenderGroup[] groups)
        {
            if (groups == null) return;
            for (int i = 0; i < groups.Length; i++)
                if (groups[i] != null) MirrorMeshPartsZ(groups[i].decoded);
        }

        private static void MirrorBoxesZ(float[][] boxes)
        {
            if (boxes == null) return;
            for (int i = 0; i < boxes.Length; i++)
            {
                float[] b = boxes[i];
                if (b == null || b.Length < 6) continue;
                float oldMin = b[2];
                float oldMax = b[5];
                b[2] = -oldMax;
                b[5] = -oldMin;
            }
        }

        private static void MirrorSpawnListZ(float[][] spawns)
        {
            if (spawns == null) return;
            for (int i = 0; i < spawns.Length; i++)
            {
                float[] s = spawns[i];
                if (s != null && s.Length >= 3) s[2] = -s[2];
            }
        }

        private static bool DecodePackedMeshes(CNRDLCPackedBlob[] blobs, string kind, out CNRDLCMeshData[] meshes, out string reason)
        {
            reason = "";
            if (blobs == null || blobs.Length == 0) { meshes = new CNRDLCMeshData[0]; return true; }
            meshes = new CNRDLCMeshData[blobs.Length];
            for (int i = 0; i < blobs.Length; i++)
            {
                if (!DecodePackedMesh(blobs[i], kind, out meshes[i], out reason)) return false;
            }
            return true;
        }

        private static bool DecodePackedMesh(CNRDLCPackedBlob blob, string kind, out CNRDLCMeshData mesh, out string reason)
        {
            mesh = null;
            reason = "";
            try
            {
                if (blob == null || string.IsNullOrEmpty(blob.dataBase64))
                {
                    reason = "DLC map " + kind + " packed mesh encoding is invalid.";
                    return false;
                }

                bool f32Raw = blob.encoding == "cnrmesh-f32-u16-raw-v1";
                bool f32Lz4 = blob.encoding == "cnrmesh-f32-u16-lz4-v1";
                bool q10Raw = blob.encoding == "cnrmesh-q10-u16-quads-raw-v1";
                bool q10Lz4 = blob.encoding == "cnrmesh-q10-u16-quads-lz4-v1";
                bool tiledRaw = blob.encoding == "cnrmesh-q10-uvq10-quads-raw-v1";
                bool tiledLz4 = blob.encoding == "cnrmesh-q10-uvq10-quads-lz4-v1";
                bool isRaw = f32Raw || q10Raw || tiledRaw;
                bool quantizedQuads = q10Raw || q10Lz4 || tiledRaw || tiledLz4;
                bool tiledUv = tiledRaw || tiledLz4;
                if (!f32Raw && !f32Lz4 && !q10Raw && !q10Lz4 && !tiledRaw && !tiledLz4)
                {
                    reason = blob.encoding == "cnrmesh-f32-u16-gzip-v1"
                        ? "DLC map uses legacy gzip packing. Re-export this map with exporter format v3."
                        : "DLC map " + kind + " packed mesh encoding is invalid.";
                    return false;
                }

                byte[] packed = Convert.FromBase64String(blob.dataBase64);
                if (packed.Length == 0 || packed.Length > MaxPackedCompressedBytes)
                {
                    reason = "DLC map " + kind + " packed mesh is empty or too large.";
                    return false;
                }
                byte[] raw = isRaw ? packed : Lz4Decompress(packed, blob.rawBytes);
                if (raw.Length > MaxPackedInflatedBytes)
                {
                    reason = "DLC map " + kind + " packed mesh expands beyond the safety limit.";
                    return false;
                }

                using (MemoryStream ms = new MemoryStream(raw, false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    CNRDLCMeshData data = new CNRDLCMeshData();
                    if (quantizedQuads)
                    {
                        if (raw.Length < 4) { reason = "DLC map " + kind + " quantized mesh header is truncated."; return false; }
                        int vc = br.ReadInt32();
                        if (vc <= 0 || vc > MaxVerticesPerPart || (vc % 4) != 0)
                        {
                            reason = "DLC map " + kind + " quantized mesh vertex count is invalid.";
                            return false;
                        }
                        int ic = (vc / 4) * 6;
                        if (ic > MaxIndicesPerPart)
                        {
                            reason = "DLC map " + kind + " quantized mesh index count is invalid.";
                            return false;
                        }
                        long expected = 4L + (long)vc * 3L * 2L + (long)vc * 2L * 2L;
                        if (expected != raw.Length)
                        {
                            reason = "DLC map " + kind + " quantized mesh length is invalid.";
                            return false;
                        }

                        data.vertices = new float[vc * 3];
                        for (int i = 0; i < data.vertices.Length; i++)
                            data.vertices[i] = br.ReadInt16() / QuantizedPositionScale;
                        data.uv = new float[vc * 2];
                        for (int i = 0; i < data.uv.Length; i++)
                            data.uv[i] = tiledUv ? br.ReadInt16() / TiledUvScale : br.ReadUInt16() / QuantizedUvScale;
                        data.triangles = new int[ic];
                        int ti = 0;
                        for (int v = 0; v < vc; v += 4)
                        {
                            data.triangles[ti++] = v;
                            data.triangles[ti++] = v + 1;
                            data.triangles[ti++] = v + 2;
                            data.triangles[ti++] = v;
                            data.triangles[ti++] = v + 2;
                            data.triangles[ti++] = v + 3;
                        }
                    }
                    else
                    {
                        if (raw.Length < 12) { reason = "DLC map " + kind + " packed mesh header is truncated."; return false; }
                        int vc = br.ReadInt32();
                        int ic = br.ReadInt32();
                        int flags = br.ReadInt32();
                        if (vc <= 0 || vc > MaxVerticesPerPart || ic < 0 || ic > MaxIndicesPerPart || (ic % 3) != 0 || (flags & 1) == 0)
                        {
                            reason = "DLC map " + kind + " packed mesh counts are invalid.";
                            return false;
                        }
                        long expected = 12L + (long)vc * 3L * 4L + (long)vc * 2L * 4L + (long)ic * 2L;
                        if (expected != raw.Length)
                        {
                            reason = "DLC map " + kind + " packed mesh length is invalid.";
                            return false;
                        }
                        data.vertices = new float[vc * 3];
                        for (int i = 0; i < data.vertices.Length; i++) data.vertices[i] = br.ReadSingle();
                        data.uv = new float[vc * 2];
                        for (int i = 0; i < data.uv.Length; i++) data.uv[i] = br.ReadSingle();
                        data.triangles = new int[ic];
                        for (int i = 0; i < ic; i++)
                        {
                            int index = br.ReadUInt16();
                            if (index >= vc) { reason = "DLC map " + kind + " packed mesh has an out-of-range index."; return false; }
                            data.triangles[i] = index;
                        }
                    }
                    mesh = data;
                }
                return true;
            }
            catch (Exception ex)
            {
                reason = "DLC map " + kind + " packed mesh could not be decoded: " + ex.Message;
                return false;
            }
        }

        private static bool DecodeCollisionBoxes(CNRDLCPackedBlob blob, out float[][] boxes, out string reason)
        {
            boxes = new float[0][];
            reason = "";
            try
            {
                if (blob == null || string.IsNullOrEmpty(blob.dataBase64)) return true;

                bool f32Raw = blob.encoding == "cnrboxes-f32-raw-v1";
                bool f32Lz4 = blob.encoding == "cnrboxes-f32-lz4-v1";
                bool q10Raw = blob.encoding == "cnrboxes-q10-raw-v1";
                bool q10Lz4 = blob.encoding == "cnrboxes-q10-lz4-v1";
                bool isRaw = f32Raw || q10Raw;
                bool quantized = q10Raw || q10Lz4;
                if (!f32Raw && !f32Lz4 && !q10Raw && !q10Lz4)
                {
                    reason = blob.encoding == "cnrboxes-f32-gzip-v1"
                        ? "DLC map uses legacy gzip collision packing. Re-export this map with exporter format v3."
                        : "DLC map collision box encoding is invalid.";
                    return false;
                }

                byte[] packed = Convert.FromBase64String(blob.dataBase64);
                if (packed.Length == 0 || packed.Length > MaxPackedCompressedBytes)
                {
                    reason = "DLC map packed collision data is empty or too large.";
                    return false;
                }
                byte[] raw = isRaw ? packed : Lz4Decompress(packed, blob.rawBytes);
                if (raw.Length > MaxPackedInflatedBytes)
                {
                    reason = "DLC map packed collision data expands beyond the safety limit.";
                    return false;
                }
                if (raw.Length < 4) { reason = "DLC map packed collision header is truncated."; return false; }

                using (MemoryStream ms = new MemoryStream(raw, false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int count = br.ReadInt32();
                    long expected = 4L + (long)count * (quantized ? 12L : 24L);
                    if (count < 0 || count > MaxCollisionBoxesPerChunk || (blob.count > 0 && blob.count != count) || expected != raw.Length)
                    {
                        reason = "DLC map packed collision count is invalid.";
                        return false;
                    }
                    boxes = new float[count][];
                    for (int i = 0; i < count; i++)
                    {
                        float[] b = new float[6];
                        for (int j = 0; j < 6; j++)
                            b[j] = quantized ? br.ReadInt16() / QuantizedPositionScale : br.ReadSingle();
                        if (!(b[3] > b[0] && b[4] > b[1] && b[5] > b[2]))
                        {
                            reason = "DLC map packed collision contains an invalid box.";
                            return false;
                        }
                        boxes[i] = b;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                reason = "DLC map packed collision could not be decoded: " + ex.Message;
                return false;
            }
        }

        private static byte[] Lz4Decompress(byte[] compressed, int expectedLength)
        {
            if (compressed == null || compressed.Length == 0)
                throw new Exception("Packed DLC map LZ4 blob is empty.");
            if (expectedLength <= 0 || expectedLength > MaxPackedInflatedBytes)
                throw new Exception("Packed DLC map LZ4 output length is invalid.");

            byte[] output = new byte[expectedLength];
            int input = 0;
            int written = 0;
            while (input < compressed.Length)
            {
                int token = compressed[input++];
                int literalLength = token >> 4;
                if (literalLength == 15) literalLength += ReadLz4Length(compressed, ref input);
                if (literalLength < 0 || input > compressed.Length - literalLength || written > output.Length - literalLength)
                    throw new Exception("Packed DLC map LZ4 literal run is invalid.");
                Buffer.BlockCopy(compressed, input, output, written, literalLength);
                input += literalLength;
                written += literalLength;

                // A final literals-only sequence has no offset or match.
                if (input == compressed.Length) break;
                if (input > compressed.Length - 2)
                    throw new Exception("Packed DLC map LZ4 match offset is truncated.");

                int offset = compressed[input] | (compressed[input + 1] << 8);
                input += 2;
                if (offset <= 0 || offset > written)
                    throw new Exception("Packed DLC map LZ4 match offset is invalid.");

                int matchLength = token & 15;
                if (matchLength == 15) matchLength += ReadLz4Length(compressed, ref input);
                matchLength += 4;
                if (matchLength < 4 || written > output.Length - matchLength)
                    throw new Exception("Packed DLC map LZ4 match length is invalid.");

                int match = written - offset;
                for (int i = 0; i < matchLength; i++) output[written++] = output[match + i];
            }

            if (written != output.Length)
                throw new Exception("Packed DLC map LZ4 output length does not match its header.");
            return output;
        }

        private static int ReadLz4Length(byte[] data, ref int offset)
        {
            int total = 0;
            while (true)
            {
                if (offset >= data.Length) throw new Exception("Packed DLC map LZ4 length is truncated.");
                int value = data[offset++];
                if (total > MaxPackedInflatedBytes - value)
                    throw new Exception("Packed DLC map LZ4 length exceeds the safety limit.");
                total += value;
                if (value != 255) return total;
            }
        }

        private static bool ValidateParts(CNRDLCMeshData[] parts, string kind, out string reason)
        {
            reason = "";
            if (parts == null) return true;
            for (int i = 0; i < parts.Length; i++)
            {
                CNRDLCMeshData p = parts[i];
                if (p == null) continue;
                int vc = p.vertices == null ? 0 : p.vertices.Length / 3;
                if (p.vertices == null || p.vertices.Length % 3 != 0 || vc > MaxVerticesPerPart)
                {
                    reason = "DLC map " + kind + " mesh has invalid/oversized vertices.";
                    return false;
                }
                if (p.uv == null || p.uv.Length != vc * 2)
                {
                    reason = "DLC map " + kind + " mesh has invalid UVs.";
                    return false;
                }
                if (!ValidateTriangles(p.triangles, vc))
                {
                    reason = "DLC map " + kind + " mesh has invalid triangles.";
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateCollisionParts(CNRDLCMeshData[] parts, out string reason)
        {
            reason = "";
            if (parts == null) return true;
            for (int i = 0; i < parts.Length; i++)
            {
                CNRDLCMeshData p = parts[i];
                if (p == null) continue;
                int vc = p.vertices == null ? 0 : p.vertices.Length / 3;
                if (p.vertices == null || p.vertices.Length % 3 != 0 || vc > MaxVerticesPerPart || !ValidateTriangles(p.triangles, vc))
                {
                    reason = "DLC map collision mesh is invalid or oversized.";
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateTriangles(int[] tris, int vertexCount)
        {
            if (tris == null || tris.Length % 3 != 0) return false;
            for (int i = 0; i < tris.Length; i++)
                if (tris[i] < 0 || tris[i] >= vertexCount) return false;
            return true;
        }

        internal static bool TryBuildActiveInCurrentScene(out string reason)
        {
            reason = "";
            try
            {
                if (!IsActive) { reason = "DLC map is not active."; return false; }
                string scene = Application.loadedLevelName ?? "";
                if (scene != BootstrapScene && scene != "FreeRun5_1" && scene != "FreeRun8_1")
                {
                    reason = "Current scene is not a DLC bootstrap scene: " + scene;
                    return false;
                }

                CNRDLCMapLoader loader = UnityEngine.Object.FindObjectOfType(typeof(CNRDLCMapLoader)) as CNRDLCMapLoader;
                if (loader == null) { reason = "DLC map loader component is unavailable."; return false; }

                string path = PlayerPrefs.GetString(PrefPath, "");
                if (_prepared == null && !PrepareFile(path, out reason)) return false;
                RemapVanillaSpawnPoints();
                loader.StartCoroutine(loader.BuildScene());
                ModEntry.Log("DLCMap: late room activation building current scene=" + scene);
                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private void Awake()
        {
            if (IsActive && _prepared == null)
            {
                string path = PlayerPrefs.GetString(PrefPath, "");
                string reason;
                if (!string.IsNullOrEmpty(path) && !PrepareFile(path, out reason))
                    ModEntry.Log("DLCMap startup prepare failed: " + reason);
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            if (!IsActive) return;
            string scene = Application.loadedLevelName;
            if (scene != BootstrapScene && scene != "FreeRun5_1" && scene != "FreeRun8_1") return;

            // Repoint vanilla's authoritative spawn list immediately, before its normal
            // Photon player-instantiation path chooses a random spawn Transform.
            string reason;
            string path = PlayerPrefs.GetString(PrefPath, "");
            if (_prepared == null && !PrepareFile(path, out reason))
            {
                ModEntry.Log("DLCMap spawn remap aborted: " + reason);
                return;
            }
            RemapVanillaSpawnPoints();
            StartCoroutine(BuildScene());
        }

        private IEnumerator BuildScene()
        {
            yield return null;
            yield return null;

            string reason;
            string path = PlayerPrefs.GetString(PrefPath, "");
            if (_prepared == null && !PrepareFile(path, out reason))
            {
                ModEntry.Log("DLCMap scene build aborted: " + reason);
                yield break;
            }

            // Repeat after two frames in case the bootstrap's RoomMultiplayerMenu was
            // instantiated slightly after OnLevelWasLoaded. Any later respawn will then
            // still use the remapped vanilla list without a custom respawn hook.
            RemapVanillaSpawnPoints();

            GameObject player = GameObject.Find("ExampleCharacter");
            CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
            if (player != null)
            {
                LadderPlayer vanillaLadder = player.GetComponent<LadderPlayer>();
                if (vanillaLadder != null) vanillaLadder.enabled = false;

                FPScontroller fps = player.GetComponent<FPScontroller>();
                if (fps != null)
                {
                    fps.onLadder = false;
                    if (fps.movement != null) fps.movement.enableGravity = true;
                }

                CNRMinecraftLadderController minecraftLadder = player.GetComponent<CNRMinecraftLadderController>();
                bool addedMinecraftLadder = minecraftLadder == null;
                if (minecraftLadder == null) minecraftLadder = player.AddComponent<CNRMinecraftLadderController>();
                minecraftLadder.ResetForMap(_prepared.blockScale);
                if (addedMinecraftLadder)
                    ModEntry.Log("DLCMap: attached Minecraft-style climbable controller to local player");

                CNRMinecraftWaterController swimming = player.GetComponent<CNRMinecraftWaterController>();
                bool addedSwimming = swimming == null;
                if (swimming == null) swimming = player.AddComponent<CNRMinecraftWaterController>();
                swimming.ResetForMap(_prepared.blockScale);
                if (addedSwimming)
                    ModEntry.Log("DLCMap: attached Minecraft-style swimming controller to local player");
            }
            if (cc != null) cc.enabled = false;

            try
            {
                if (_mapRoot != null) Destroy(_mapRoot);
                ReleaseRenderResources();

                _atlasTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                _atlasTexture.name = "CNRDLCMapAtlas";
                if (!_atlasTexture.LoadImage(_preparedAtlasPng)) throw new Exception("atlas PNG could not be decoded");
                ApplyDefaultMinecraftTintFallback(_atlasTexture, _prepared.atlas);
                _atlasTexture.filterMode = FilterMode.Point;
                _atlasTexture.wrapMode = TextureWrapMode.Clamp;

                BuildMaterials();
                CNRMinecraftWaterRegistry.Configure(_prepared.blockScale);
                _mapRoot = new GameObject("CNRDLCMapRoot");
                _mapRoot.transform.position = OriginVector(_prepared.origin);
                _mapRoot.AddComponent<CNRDLCProjectilePassThrough>();
                _collisionStreamer = _mapRoot.AddComponent<CNRDLCChunkCollisionStreamer>();
                _collisionStreamer.Configure(_prepared.blockScale);
                ApplyReadabilityLighting(_mapRoot);

                _legacyWaterBiasedQuads = 0;
                _collisionBoxesBeforeMerge = 0;
                _collisionBoxesAfterMerge = 0;
                _tiledRenderRegions = 0;
                _tiledRenderDrawCalls = 0;
                int renderParts = 0;
                int collisionParts = 0;
                int bulletPassThroughParts = 0;
                int climbableParts = 0;
                int waterParts = 0;
                GameObject[] chunkRoots = new GameObject[_prepared.chunks.Length];

                // Register every water volume before any transparent mesh is built. Besides
                // making swimming queries immediately complete, this lets the legacy water
                // z-fighting repair determine which side of a coplanar face is actually water.
                for (int i = 0; i < _prepared.chunks.Length; i++)
                {
                    CNRDLCMapChunk chunk = _prepared.chunks[i];
                    GameObject chunkRoot = new GameObject("Chunk_" + chunk.x + "_" + chunk.y + "_" + chunk.z);
                    chunkRoot.transform.parent = _mapRoot.transform;
                    chunkRoot.transform.localPosition = new Vector3(chunk.x, chunk.y, chunk.z) * _prepared.blockScale;
                    chunkRoot.transform.localRotation = Quaternion.identity;
                    chunkRoot.transform.localScale = Vector3.one * _prepared.blockScale;
                    chunkRoot.isStatic = true;
                    chunkRoots[i] = chunkRoot;

                    if (chunk.decodedWaterBoxes != null && chunk.decodedWaterBoxes.Length > 0)
                        waterParts += BuildWaterBoxes(chunkRoot, chunk.decodedWaterBoxes);
                }

                if (_prepared.version == FormatVersion)
                {
                    renderParts += BuildTiledRenderRegions(_mapRoot, _prepared.chunks, _opaqueMaterial, "Opaque", 0);
                    renderParts += BuildTiledRenderRegions(_mapRoot, _prepared.chunks, _cutoutMaterial, "Cutout", 1);
                    renderParts += BuildTiledRenderRegions(_mapRoot, _prepared.chunks, _transparentMaterial, "Transparent", 2);
                }

                for (int i = 0; i < _prepared.chunks.Length; i++)
                {
                    CNRDLCMapChunk chunk = _prepared.chunks[i];
                    GameObject chunkRoot = chunkRoots[i];

                    if (_prepared.version != FormatVersion)
                    {
                        renderParts += BuildRenderParts(chunkRoot, chunk.opaque, _opaqueMaterial, "Opaque");
                        renderParts += BuildRenderParts(chunkRoot, chunk.cutout, _cutoutMaterial, "Cutout");
                        renderParts += BuildRenderParts(chunkRoot, chunk.transparent, _transparentMaterial, "Transparent");
                    }
                    if (chunk.decodedCollisionBoxes != null && chunk.decodedCollisionBoxes.Length > 0)
                        collisionParts += BuildCollisionBoxes(chunkRoot, chunk.decodedCollisionBoxes);
                    else
                        collisionParts += BuildCollisionParts(chunkRoot, chunk.collision);
                    if (chunk.decodedBulletPassThroughBoxes != null && chunk.decodedBulletPassThroughBoxes.Length > 0)
                        bulletPassThroughParts += BuildBulletPassThroughBoxes(chunkRoot, chunk.decodedBulletPassThroughBoxes);
                    if (chunk.decodedClimbableBoxes != null && chunk.decodedClimbableBoxes.Length > 0)
                        climbableParts += BuildClimbableBoxes(chunkRoot, chunk.decodedClimbableBoxes);

                    // Chunked meshes keep creation bounded; construction stays synchronous here
                    // because this legacy C# compiler cannot yield inside a try/catch body.
                }

                // Water volumes change the path cost map. Invalidate any nav grid that may
                // have baked before the DLC scene finished so the next bot/zombie bake sees
                // the merged water volumes exactly once.
                if (waterParts > 0) CNRZombieMod.ZombieNavGrid.Invalidate();

                // Keep donor collision alive until the DLC collision is complete. If the
                // custom build fails, the bootstrap scene remains a safe fallback instead of
                // dropping the player into an empty world.
                StripBootstrapGeometry();

                ModEntry.Log("DLCMap built: chunks=" + _prepared.chunks.Length + " renderParts=" + renderParts + " tiledRegions=" + _tiledRenderRegions + " tiledDrawCalls=" + _tiledRenderDrawCalls + " collisionParts=" + collisionParts + " collisionMerge=" + _collisionBoxesBeforeMerge + "->" + _collisionBoxesAfterMerge + " bulletPassThrough=" + bulletPassThroughParts + " climbables=" + climbableParts + " waterVolumes=" + waterParts + " legacyWaterBiasQuads=" + _legacyWaterBiasedQuads);
            }
            catch (Exception ex)
            {
                ModEntry.Log("DLCMap scene build failed: " + ex.Message);
            }
            finally
            {
                if (cc != null) cc.enabled = true;
            }
        }

        private static void ApplyReadabilityLighting(GameObject parent)
        {
            // The bootstrap scene can contain several directional/point lights. Leaving
            // those active makes DLC chunks receive wildly different totals depending on
            // where they happen to sit in the donor scene. Disable the inherited lights
            // first, then use one deliberately narrow-range lighting setup for the map.
            int disabledLights = 0;
            Light[] inheritedLights = (Light[])GameObject.FindObjectsOfType(typeof(Light));
            for (int i = 0; i < inheritedLights.Length; i++)
            {
                Light light = inheritedLights[i];
                if (light == null || !light.enabled) continue;
                light.enabled = false;
                disabledLights++;
            }

            // Keep enough ambient light that a face pointed away from the key never drops
            // near black. The directional contribution is intentionally modest so bright
            // textures such as stone cannot blow out while adjacent faces still separate.
            RenderSettings.ambientLight = new Color(0.44f, 0.44f, 0.44f, 1f);

            if (parent != null)
            {
                GameObject keyGo = new GameObject("CNRDLCMapKeyLight");
                keyGo.transform.parent = parent.transform;
                keyGo.transform.localPosition = Vector3.zero;
                keyGo.transform.localRotation = Quaternion.Euler(48f, -32f, 0f);
                Light key = keyGo.AddComponent<Light>();
                key.type = LightType.Directional;
                key.color = Color.white;
                key.intensity = 0.34f;
                key.shadows = LightShadows.None;
            }

            ModEntry.Log("DLCMap lighting: disabled inherited lights=" + disabledLights + " ambient=0.44 key=0.34");
        }

        private static void ApplyDefaultMinecraftTintFallback(Texture2D atlasTexture, CNRDLCMapAtlas atlas)
        {
            if (atlasTexture == null || atlas == null || atlas.entries == null || atlas.entries.Length == 0) return;

            int changed = 0;
            for (int i = 0; i < atlas.entries.Length; i++)
            {
                CNRDLCMapAtlasEntry entry = atlas.entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.id) || entry.w <= 0 || entry.h <= 0) continue;

                Color tint;
                if (!TryGetDefaultMinecraftTint(entry.id, out tint)) continue;

                // Exporter atlas coordinates are top-left based; Unity's GetPixels/SetPixels
                // use a bottom-left origin. Include the one-pixel atlas gutter so bilinear
                // sampling never pulls an untinted white edge into the block texture.
                int x0 = Mathf.Clamp(entry.x - 1, 0, atlasTexture.width);
                int x1 = Mathf.Clamp(entry.x + entry.w + 1, 0, atlasTexture.width);
                int top0 = Mathf.Clamp(entry.y - 1, 0, atlasTexture.height);
                int top1 = Mathf.Clamp(entry.y + entry.h + 1, 0, atlasTexture.height);
                int y0 = atlasTexture.height - top1;
                int w = x1 - x0;
                int h = top1 - top0;
                if (w <= 0 || h <= 0) continue;

                try
                {
                    Color[] pixels = atlasTexture.GetPixels(x0, y0, w, h);
                    for (int p = 0; p < pixels.Length; p++)
                    {
                        Color c = pixels[p];
                        c.r *= tint.r;
                        c.g *= tint.g;
                        c.b *= tint.b;
                        pixels[p] = c;
                    }
                    atlasTexture.SetPixels(x0, y0, w, h, pixels);
                    changed++;
                }
                catch (Exception ex)
                {
                    ModEntry.Log("DLCMap: default Minecraft tint fallback failed for " + entry.id + ": " + ex.Message);
                }
            }

            if (changed > 0)
            {
                atlasTexture.Apply(false, false);
                ModEntry.Log("DLCMap: applied default Minecraft tint fallback to " + changed + " atlas entries");
            }
        }

        private static bool TryGetDefaultMinecraftTint(string textureId, out Color tint)
        {
            tint = Color.white;
            if (string.IsNullOrEmpty(textureId)) return false;

            string id = textureId.ToLowerInvariant();
            int tintMarker = id.IndexOf("#tint_");
            if (tintMarker >= 0 && id.IndexOf("#tint_ffffff") < 0)
                return false; // A real tint was supplied and baked by the exporter.

            if (id.IndexOf("spruce_leaves") >= 0) tint = Rgb24(0x619961);
            else if (id.IndexOf("birch_leaves") >= 0) tint = Rgb24(0x80A755);
            else if (id.IndexOf("lily_pad") >= 0) tint = Rgb24(0x208030);
            else if (id.IndexOf("water_still") >= 0 || id.IndexOf("water_flow") >= 0) tint = Rgb24(0x3F76E4);
            else if (id.IndexOf("leaves") >= 0 || id.IndexOf("vine") >= 0) tint = Rgb24(0x77AB2F);
            else if (id.IndexOf("grass_block_top") >= 0 || id.IndexOf("grass_block_side_overlay") >= 0 ||
                     id.IndexOf("short_grass") >= 0 || id.IndexOf("tall_grass") >= 0 ||
                     id.IndexOf("fern") >= 0 || id.EndsWith("/grass") || id.EndsWith("/grass_block")) tint = Rgb24(0x91BD59);
            else return false;

            return true;
        }

        private static Color Rgb24(int rgb)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
        }

        private static void BuildMaterials()
        {
            // Prefer lit legacy shaders now that DLC maps provide their own controlled
            // ambient + directional lighting. This gives block faces visible contour and
            // recess depth. Unlit shaders remain the final fallback for stripped builds.
            Shader opaqueShader = Shader.Find("Diffuse");
            if (opaqueShader == null) opaqueShader = Shader.Find("VertexLit");
            if (opaqueShader == null) opaqueShader = Shader.Find("Unlit/Texture");
            if (opaqueShader == null) opaqueShader = Shader.Find("Unlit/Transparent Colored");

            Shader cutoutShader = Shader.Find("Transparent/Cutout/Diffuse");
            if (cutoutShader == null) cutoutShader = Shader.Find("Unlit/Transparent Cutout");
            if (cutoutShader == null) cutoutShader = opaqueShader;

            Shader transparentShader = Shader.Find("Transparent/Diffuse");
            if (transparentShader == null) transparentShader = Shader.Find("Unlit/Transparent");
            if (transparentShader == null) transparentShader = Shader.Find("Unlit/Transparent Colored");
            if (transparentShader == null) transparentShader = cutoutShader;

            _opaqueMaterial = new Material(opaqueShader); _opaqueMaterial.name = "CNRDLCMap Opaque"; _opaqueMaterial.mainTexture = _atlasTexture;
            _cutoutMaterial = new Material(cutoutShader); _cutoutMaterial.name = "CNRDLCMap Cutout"; _cutoutMaterial.mainTexture = _atlasTexture;
            _transparentMaterial = new Material(transparentShader); _transparentMaterial.name = "CNRDLCMap Transparent"; _transparentMaterial.mainTexture = _atlasTexture;

            if (_opaqueMaterial.HasProperty("_Color")) _opaqueMaterial.SetColor("_Color", Color.white);
            if (_cutoutMaterial.HasProperty("_Color")) _cutoutMaterial.SetColor("_Color", Color.white);
            if (_transparentMaterial.HasProperty("_Color")) _transparentMaterial.SetColor("_Color", Color.white);

            // Opaque and cutout surfaces must participate in the depth buffer even if an
            // old build only has a less-ideal fallback shader available. Without this,
            // back/far faces can blend through the face nearest the camera.
            _opaqueMaterial.renderQueue = 2000;
            _cutoutMaterial.renderQueue = 2450;
            _transparentMaterial.renderQueue = 3000;
            if (_opaqueMaterial.HasProperty("_ZWrite")) _opaqueMaterial.SetInt("_ZWrite", 1);
            if (_cutoutMaterial.HasProperty("_ZWrite")) _cutoutMaterial.SetInt("_ZWrite", 1);
            if (_transparentMaterial.HasProperty("_ZWrite")) _transparentMaterial.SetInt("_ZWrite", 0);
            if (_opaqueMaterial.HasProperty("_Mode")) _opaqueMaterial.SetFloat("_Mode", 0f);
            if (_cutoutMaterial.HasProperty("_Mode")) _cutoutMaterial.SetFloat("_Mode", 1f);
            if (_cutoutMaterial.HasProperty("_Cutoff")) _cutoutMaterial.SetFloat("_Cutoff", 0.5f);

            ModEntry.Log("DLCMap shaders: opaque=" + opaqueShader.name + " cutout=" + cutoutShader.name + " transparent=" + transparentShader.name);
        }

        private static int BuildTiledRenderRegions(GameObject parent, CNRDLCMapChunk[] chunks, Material baseMaterial, string label, int layer)
        {
            if (parent == null || chunks == null || baseMaterial == null) return 0;

            Dictionary<long, List<int>> regions = new Dictionary<long, List<int>>();
            for (int i = 0; i < chunks.Length; i++)
            {
                CNRDLCMapChunk chunk = chunks[i];
                if (chunk == null) continue;
                CNRDLCTiledRenderGroup[] groups = GetTiledGroupsForLayer(chunk, layer);
                if (groups == null || groups.Length == 0) continue;

                int rx = FloorDiv(chunk.x, TiledRenderRegionSize);
                int rz = FloorDiv(chunk.z, TiledRenderRegionSize);
                long key = unchecked(((long)rx << 32) ^ (uint)rz);
                List<int> indices;
                if (!regions.TryGetValue(key, out indices))
                {
                    indices = new List<int>();
                    regions[key] = indices;
                }
                indices.Add(i);
            }

            int made = 0;
            foreach (KeyValuePair<long, List<int>> pair in regions)
                made += BuildTiledRenderRegion(parent, chunks, pair.Value, baseMaterial, label, layer, pair.Key);
            return made;
        }

        private static CNRDLCTiledRenderGroup[] GetTiledGroupsForLayer(CNRDLCMapChunk chunk, int layer)
        {
            if (chunk == null) return new CNRDLCTiledRenderGroup[0];
            if (layer == 1) return chunk.cutoutTiled;
            if (layer == 2) return chunk.transparentTiled;
            return chunk.opaqueTiled;
        }

        private static int BuildTiledRenderRegion(GameObject parent, CNRDLCMapChunk[] chunks, List<int> indices, Material baseMaterial, string label, int layer, long regionKey)
        {
            if (indices == null || indices.Count == 0) return 0;

            GameObject renderRoot = new GameObject(label + "Region_" + regionKey);
            renderRoot.transform.parent = parent.transform;
            renderRoot.transform.localPosition = Vector3.zero;
            renderRoot.transform.localRotation = Quaternion.identity;
            renderRoot.transform.localScale = Vector3.one * (_prepared != null ? _prepared.blockScale : 1f);
            renderRoot.isStatic = true;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            Dictionary<string, List<int>> trianglesByTexture = new Dictionary<string, List<int>>();
            List<string> textureOrder = new List<string>();
            int made = 0;
            int batchIndex = 0;

            for (int c = 0; c < indices.Count; c++)
            {
                CNRDLCMapChunk chunk = chunks[indices[c]];
                if (chunk == null) continue;
                CNRDLCTiledRenderGroup[] groups = GetTiledGroupsForLayer(chunk, layer);
                if (groups == null) continue;
                Vector3 chunkOffset = new Vector3(chunk.x, chunk.y, chunk.z);

                for (int g = 0; g < groups.Length; g++)
                {
                    CNRDLCTiledRenderGroup group = groups[g];
                    if (group == null || string.IsNullOrEmpty(group.texture) || group.decoded == null) continue;

                    for (int p = 0; p < group.decoded.Length; p++)
                    {
                        CNRDLCMeshData data = group.decoded[p];
                        if (data == null || data.vertices == null || data.vertices.Length == 0) continue;
                        int vc = data.vertices.Length / 3;
                        if (vc <= 0 || vc > MaxVerticesPerPart) continue;

                        if (vertices.Count > 0 && vertices.Count + vc > MaxVerticesPerPart)
                        {
                            made += FlushTiledRegionBatch(renderRoot, vertices, uv, trianglesByTexture, textureOrder, baseMaterial, label, batchIndex++);
                            vertices.Clear();
                            uv.Clear();
                            trianglesByTexture.Clear();
                            textureOrder.Clear();
                        }

                        Vector3[] localVertices = new Vector3[vc];
                        for (int i = 0; i < vc; i++)
                            localVertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]) + chunkOffset;

                        if (layer == 2 && IsWaterTextureId(group.texture))
                            ApplyTiledWaterDepthBias(localVertices, data.triangles, renderRoot.transform);

                        int vertexBase = vertices.Count;
                        for (int i = 0; i < vc; i++)
                        {
                            vertices.Add(localVertices[i]);
                            uv.Add(new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]));
                        }

                        if (data.triangles != null && data.triangles.Length > 0)
                        {
                            List<int> groupTriangles;
                            if (!trianglesByTexture.TryGetValue(group.texture, out groupTriangles))
                            {
                                groupTriangles = new List<int>();
                                trianglesByTexture[group.texture] = groupTriangles;
                                textureOrder.Add(group.texture);
                            }
                            for (int i = 0; i < data.triangles.Length; i++)
                                groupTriangles.Add(vertexBase + data.triangles[i]);
                        }
                    }
                }
            }

            if (vertices.Count > 0)
                made += FlushTiledRegionBatch(renderRoot, vertices, uv, trianglesByTexture, textureOrder, baseMaterial, label, batchIndex);

            if (made == 0) Destroy(renderRoot);
            else _tiledRenderRegions++;
            return made;
        }

        private static int FlushTiledRegionBatch(GameObject parent, List<Vector3> vertices, List<Vector2> uv, Dictionary<string, List<int>> trianglesByTexture, List<string> textureOrder, Material baseMaterial, string label, int batchIndex)
        {
            if (parent == null || vertices == null || vertices.Count == 0 || textureOrder == null || textureOrder.Count == 0) return 0;

            Mesh mesh = new Mesh();
            mesh.name = "CNRDLCMapTiledRegionMesh";
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.subMeshCount = textureOrder.Count;
            Material[] materials = new Material[textureOrder.Count];
            for (int i = 0; i < textureOrder.Count; i++)
            {
                string texture = textureOrder[i];
                List<int> tris;
                if (!trianglesByTexture.TryGetValue(texture, out tris)) tris = new List<int>();
                mesh.SetTriangles(tris.ToArray(), i);
                materials[i] = GetTiledMaterial(texture, baseMaterial, label);
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject go = new GameObject(label + "RegionBatch_" + batchIndex);
            go.transform.parent = parent.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.isStatic = true;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = materials;
            mr.castShadows = false;
            mr.receiveShadows = false;
            _tiledRenderDrawCalls += textureOrder.Count;
            return 1;
        }

        private static int FloorDiv(int value, int divisor)
        {
            int q = value / divisor;
            int r = value % divisor;
            if (r != 0 && value < 0) q--;
            return q;
        }

        private static int BuildTiledRenderParts(GameObject parent, CNRDLCTiledRenderGroup[] groups, Material baseMaterial, string label)
        {
            if (parent == null || groups == null || baseMaterial == null) return 0;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int[]> subTriangles = new List<int[]>();
            List<Material> materials = new List<Material>();
            int made = 0;
            int batchIndex = 0;

            for (int g = 0; g < groups.Length; g++)
            {
                CNRDLCTiledRenderGroup group = groups[g];
                if (group == null || string.IsNullOrEmpty(group.texture) || group.decoded == null) continue;
                Material material = GetTiledMaterial(group.texture, baseMaterial, label);
                List<int> groupTriangles = new List<int>();

                for (int p = 0; p < group.decoded.Length; p++)
                {
                    CNRDLCMeshData data = group.decoded[p];
                    if (data == null || data.vertices == null || data.vertices.Length == 0) continue;
                    int vc = data.vertices.Length / 3;
                    if (vc <= 0 || vc > MaxVerticesPerPart) continue;

                    if (vertices.Count > 0 && vertices.Count + vc > MaxVerticesPerPart)
                    {
                        if (groupTriangles.Count > 0)
                        {
                            subTriangles.Add(groupTriangles.ToArray());
                            materials.Add(material);
                            groupTriangles.Clear();
                        }
                        made += FlushTiledRenderBatch(parent, vertices, uv, subTriangles, materials, label, batchIndex++);
                        vertices.Clear();
                        uv.Clear();
                        subTriangles.Clear();
                        materials.Clear();
                    }

                    Vector3[] localVertices = new Vector3[vc];
                    for (int i = 0; i < vc; i++)
                        localVertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

                    if (string.Equals(label, "Transparent", StringComparison.Ordinal) && IsWaterTextureId(group.texture))
                        ApplyTiledWaterDepthBias(localVertices, data.triangles, parent.transform);

                    int vertexBase = vertices.Count;
                    for (int i = 0; i < vc; i++)
                    {
                        vertices.Add(localVertices[i]);
                        uv.Add(new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]));
                    }
                    if (data.triangles != null)
                        for (int i = 0; i < data.triangles.Length; i++) groupTriangles.Add(vertexBase + data.triangles[i]);
                }

                if (groupTriangles.Count > 0)
                {
                    subTriangles.Add(groupTriangles.ToArray());
                    materials.Add(material);
                }
            }

            if (vertices.Count > 0)
                made += FlushTiledRenderBatch(parent, vertices, uv, subTriangles, materials, label, batchIndex);
            return made;
        }

        private static int FlushTiledRenderBatch(GameObject parent, List<Vector3> vertices, List<Vector2> uv, List<int[]> subTriangles, List<Material> materials, string label, int batchIndex)
        {
            if (vertices == null || vertices.Count == 0 || subTriangles == null || subTriangles.Count == 0) return 0;

            Mesh mesh = new Mesh();
            mesh.name = "CNRDLCMapTiledMesh";
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.subMeshCount = subTriangles.Count;
            for (int i = 0; i < subTriangles.Count; i++) mesh.SetTriangles(subTriangles[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject go = new GameObject(label + "Tiled_" + batchIndex);
            go.transform.parent = parent.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.isStatic = true;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = materials.ToArray();
            mr.castShadows = false;
            mr.receiveShadows = false;
            return 1;
        }

        private static bool IsWaterTextureId(string textureId)
        {
            return !string.IsNullOrEmpty(textureId) &&
                (textureId.IndexOf("water_still", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 textureId.IndexOf("water_flow", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static Material GetTiledMaterial(string textureId, Material baseMaterial, string label)
        {
            string key = label + "|" + textureId;
            Material cached;
            if (_tiledMaterials.TryGetValue(key, out cached) && cached != null) return cached;

            Texture2D texture = GetTiledTexture(textureId);
            Material material = new Material(baseMaterial);
            material.name = "CNRDLCMap " + label + " " + textureId;
            material.mainTexture = texture;
            _tiledMaterials[key] = material;
            return material;
        }

        private static Texture2D GetTiledTexture(string textureId)
        {
            Texture2D cached;
            if (_tiledTextures.TryGetValue(textureId, out cached) && cached != null) return cached;

            if (_atlasEntriesById.Count == 0 && _prepared != null && _prepared.atlas != null && _prepared.atlas.entries != null)
            {
                for (int i = 0; i < _prepared.atlas.entries.Length; i++)
                {
                    CNRDLCMapAtlasEntry entry = _prepared.atlas.entries[i];
                    if (entry != null && !string.IsNullOrEmpty(entry.id) && !_atlasEntriesById.ContainsKey(entry.id))
                        _atlasEntriesById.Add(entry.id, entry);
                }
            }

            CNRDLCMapAtlasEntry atlasEntry;
            if (!_atlasEntriesById.TryGetValue(textureId, out atlasEntry) || atlasEntry == null ||
                atlasEntry.w <= 0 || atlasEntry.h <= 0 || _atlasTexture == null || _prepared == null || _prepared.atlas == null)
                throw new Exception("DLC map tiled texture is missing from atlas: " + textureId);

            int yBottom = _prepared.atlas.height - atlasEntry.y - atlasEntry.h;
            Color[] pixels = _atlasTexture.GetPixels(atlasEntry.x, yBottom, atlasEntry.w, atlasEntry.h);
            Texture2D texture = new Texture2D(atlasEntry.w, atlasEntry.h, TextureFormat.ARGB32, false);
            texture.name = "CNRDLCMapTile " + textureId;
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            _tiledTextures[textureId] = texture;
            return texture;
        }

        private static void ApplyTiledWaterDepthBias(Vector3[] vertices, int[] triangles, Transform waterSpace)
        {
            if (vertices == null || vertices.Length < 4 || waterSpace == null) return;
            int quadCount = vertices.Length / 4;
            for (int q = 0; q < quadCount; q++)
            {
                int first = q * 4;
                int i0 = first, i1 = first + 1, i2 = first + 2;
                int triStart = q * 6;
                if (triangles != null && triStart + 2 < triangles.Length)
                {
                    int t0 = triangles[triStart], t1 = triangles[triStart + 1], t2 = triangles[triStart + 2];
                    if (t0 >= first && t0 < first + 4 && t1 >= first && t1 < first + 4 && t2 >= first && t2 < first + 4)
                    {
                        i0 = t0; i1 = t1; i2 = t2;
                    }
                }

                Vector3 normal = Vector3.Cross(vertices[i1] - vertices[i0], vertices[i2] - vertices[i0]);
                float length = normal.magnitude;
                if (length <= 0.000001f) continue;
                normal /= length;
                if (Mathf.Abs(normal.y) > 0.25f) continue;

                Vector3 faceCenter = Vector3.zero;
                for (int k = 0; k < 4; k++) faceCenter += vertices[first + k];
                faceCenter *= 0.25f;
                Vector3 axis = Mathf.Abs(normal.x) >= Mathf.Abs(normal.z) ? Vector3.right : Vector3.forward;
                const float probeDistance = 0.03f;
                bool plusWater = CNRMinecraftWaterRegistry.ContainsPoint(waterSpace.TransformPoint(faceCenter + axis * probeDistance));
                bool minusWater = CNRMinecraftWaterRegistry.ContainsPoint(waterSpace.TransformPoint(faceCenter - axis * probeDistance));
                if (plusWater == minusWater) continue;

                Vector3 offset = (plusWater ? axis : -axis) * LegacyWaterDepthBias;
                for (int k = 0; k < 4; k++) vertices[first + k] += offset;
                _legacyWaterBiasedQuads++;
            }
        }

        private static int BuildRenderParts(GameObject parent, CNRDLCMeshData[] parts, Material material, string label)
        {
            if (parts == null || material == null) return 0;
            int made = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                CNRDLCMeshData data = parts[i];
                if (data == null || data.vertices == null || data.vertices.Length == 0) continue;
                Mesh mesh = MakeMesh(data, true, string.Equals(label, "Transparent", StringComparison.Ordinal), parent.transform);
                GameObject go = new GameObject(label + "_" + i);
                go.transform.parent = parent.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.isStatic = true;
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.castShadows = false;
                mr.receiveShadows = false;
                made++;
            }
            return made;
        }

        private static int BuildCollisionBoxes(GameObject parent, float[][] boxes)
        {
            if (parent == null || boxes == null || boxes.Length == 0) return 0;

            _collisionBoxesBeforeMerge += boxes.Length;
            List<float[]> merged = MergeCollisionBoxes(boxes);
            _collisionBoxesAfterMerge += merged.Count;
            if (merged.Count == 0) return 0;

            GameObject collisionRoot = new GameObject("StaticCollision");
            collisionRoot.transform.parent = parent.transform;
            collisionRoot.transform.localPosition = Vector3.zero;
            collisionRoot.transform.localRotation = Quaternion.identity;
            collisionRoot.transform.localScale = Vector3.one;
            collisionRoot.isStatic = true;

            Vector3 boundsMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 boundsMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            int made = 0;
            for (int i = 0; i < merged.Count; i++)
            {
                float[] b = merged[i];
                float sx = b[3] - b[0], sy = b[4] - b[1], sz = b[5] - b[2];
                if (sx <= 0f || sy <= 0f || sz <= 0f) continue;

                BoxCollider bc = collisionRoot.AddComponent<BoxCollider>();
                bc.center = new Vector3((b[0] + b[3]) * 0.5f, (b[1] + b[4]) * 0.5f, (b[2] + b[5]) * 0.5f);
                bc.size = new Vector3(sx, sy, sz);
                boundsMin = Vector3.Min(boundsMin, new Vector3(b[0], b[1], b[2]));
                boundsMax = Vector3.Max(boundsMax, new Vector3(b[3], b[4], b[5]));
                made++;
            }

            if (made == 0)
            {
                Destroy(collisionRoot);
                return 0;
            }

            if (_collisionStreamer != null)
            {
                Vector3 localCenter = (boundsMin + boundsMax) * 0.5f;
                Vector3 localSize = boundsMax - boundsMin;
                Vector3 scale = parent.transform.lossyScale;
                Vector3 worldSize = new Vector3(Mathf.Abs(localSize.x * scale.x), Mathf.Abs(localSize.y * scale.y), Mathf.Abs(localSize.z * scale.z));
                _collisionStreamer.Register(collisionRoot, new Bounds(parent.transform.TransformPoint(localCenter), worldSize));
            }
            return made;
        }

        private static List<float[]> MergeCollisionBoxes(float[][] input)
        {
            List<float[]> boxes = new List<float[]>();
            for (int i = 0; i < input.Length; i++)
            {
                float[] b = input[i];
                if (b == null || b.Length < 6 || b[3] <= b[0] || b[4] <= b[1] || b[5] <= b[2]) continue;
                boxes.Add(new float[] { b[0], b[1], b[2], b[3], b[4], b[5] });
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int axis = 0; axis < 3; axis++)
                {
                    for (int i = 0; i < boxes.Count && !changed; i++)
                    {
                        for (int j = i + 1; j < boxes.Count; j++)
                        {
                            if (!CanMergeCollisionBoxes(boxes[i], boxes[j], axis)) continue;
                            float[] a = boxes[i];
                            float[] b = boxes[j];
                            a[axis] = Mathf.Min(a[axis], b[axis]);
                            a[axis + 3] = Mathf.Max(a[axis + 3], b[axis + 3]);
                            boxes.RemoveAt(j);
                            changed = true;
                            break;
                        }
                    }
                    if (changed) break;
                }
            }
            return boxes;
        }

        private static bool CanMergeCollisionBoxes(float[] a, float[] b, int axis)
        {
            const float epsilon = 0.0005f;
            int otherA = (axis + 1) % 3;
            int otherB = (axis + 2) % 3;
            if (Mathf.Abs(a[otherA] - b[otherA]) > epsilon || Mathf.Abs(a[otherA + 3] - b[otherA + 3]) > epsilon ||
                Mathf.Abs(a[otherB] - b[otherB]) > epsilon || Mathf.Abs(a[otherB + 3] - b[otherB + 3]) > epsilon) return false;

            return Mathf.Abs(a[axis + 3] - b[axis]) <= epsilon || Mathf.Abs(b[axis + 3] - a[axis]) <= epsilon;
        }

        private static int BuildBulletPassThroughBoxes(GameObject parent, float[][] boxes)
        {
            if (parent == null || boxes == null) return 0;

            GameObject go = new GameObject("BulletPassThroughCollision");
            go.transform.parent = parent.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.layer = CNRDLCProjectilePassThrough.BarrierLayer; // Dedicated DLC barrier layer; excluded by vanilla Bullet mask 19.
            go.isStatic = true;

            int made = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                float[] b = boxes[i];
                if (b == null || b.Length < 6) continue;
                float sx = b[3] - b[0], sy = b[4] - b[1], sz = b[5] - b[2];
                if (sx <= 0f || sy <= 0f || sz <= 0f) continue;

                BoxCollider bc = go.AddComponent<BoxCollider>();
                bc.center = new Vector3((b[0] + b[3]) * 0.5f, (b[1] + b[4]) * 0.5f, (b[2] + b[5]) * 0.5f);
                bc.size = new Vector3(sx, sy, sz);
                CNRDLCProjectilePassThrough.RegisterBarrier(bc);
                made++;
            }

            if (made == 0) Destroy(go);
            return made;
        }

        private static int BuildClimbableBoxes(GameObject parent, float[][] boxes)
        {
            if (parent == null || boxes == null) return 0;
            int made = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                float[] b = boxes[i];
                if (b == null || b.Length < 6) continue;

                // Exporter <= 0.1.4 stored the rendered/collision shape of a climbable
                // (for example a ladder's paper-thin slab). Expand to the enclosing
                // Minecraft block cells at load time so old maps get the same reliable
                // interaction volume as 0.1.5+ without needing to be re-exported.
                const float snapEpsilon = 0.001f;
                float minX = Mathf.Floor(b[0] + snapEpsilon);
                float minY = Mathf.Floor(b[1] + snapEpsilon);
                float minZ = Mathf.Floor(b[2] + snapEpsilon);
                float maxX = Mathf.Ceil(b[3] - snapEpsilon);
                float maxY = Mathf.Ceil(b[4] - snapEpsilon);
                float maxZ = Mathf.Ceil(b[5] - snapEpsilon);
                float sx = maxX - minX, sy = maxY - minY, sz = maxZ - minZ;
                if (sx <= 0f || sy <= 0f || sz <= 0f) continue;

                GameObject go = new GameObject("Climbable_" + i);
                go.transform.parent = parent.transform;
                go.transform.localPosition = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.isStatic = true;

                BoxCollider trigger = go.AddComponent<BoxCollider>();
                trigger.center = Vector3.zero;
                trigger.size = new Vector3(sx, sy, sz);
                trigger.isTrigger = true;
                go.AddComponent<CNRMinecraftClimbableVolume>();
                made++;
            }
            return made;
        }

        private static int BuildWaterBoxes(GameObject parent, float[][] boxes)
        {
            if (parent == null || boxes == null || boxes.Length == 0) return 0;

            // Water is gameplay metadata, not solid physics. Register the exporter's
            // already-merged bounds directly instead of creating trigger colliders. This
            // keeps large pools essentially free in PhysX and gives players/bots/nav the
            // same fast spatial lookup.
            Vector3 scale = parent.transform.lossyScale;
            int made = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                float[] b = boxes[i];
                if (b == null || b.Length < 6) continue;
                float sx = b[3] - b[0], sy = b[4] - b[1], sz = b[5] - b[2];
                if (sx <= 0f || sy <= 0f || sz <= 0f) continue;

                Vector3 localCenter = new Vector3((b[0] + b[3]) * 0.5f, (b[1] + b[4]) * 0.5f, (b[2] + b[5]) * 0.5f);
                Vector3 worldCenter = parent.transform.TransformPoint(localCenter);
                Vector3 worldSize = new Vector3(Mathf.Abs(sx * scale.x), Mathf.Abs(sy * scale.y), Mathf.Abs(sz * scale.z));
                CNRMinecraftWaterRegistry.Register(new Bounds(worldCenter, worldSize));
                made++;
            }
            return made;
        }

        private static int BuildCollisionParts(GameObject parent, CNRDLCMeshData[] parts)
        {
            if (parts == null) return 0;
            int made = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                CNRDLCMeshData data = parts[i];
                if (data == null || data.vertices == null || data.vertices.Length == 0) continue;
                Mesh mesh = MakeMesh(data, false, false, null);
                GameObject go = new GameObject("Collision_" + i);
                go.transform.parent = parent.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.isStatic = true;
                MeshCollider mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                made++;
            }
            return made;
        }

        private static Mesh MakeMesh(CNRDLCMeshData data, bool withUv, bool applyLegacyWaterDepthBias, Transform waterSpace)
        {
            int vc = data.vertices.Length / 3;
            Vector3[] vertices = new Vector3[vc];
            for (int i = 0; i < vc; i++)
                vertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

            if (withUv && applyLegacyWaterDepthBias)
                ApplyLegacyWaterDepthBias(vertices, data.uv, data.triangles, waterSpace);

            Mesh mesh = new Mesh();
            mesh.name = "CNRDLCMapMesh";
            mesh.vertices = vertices;
            if (withUv)
            {
                Vector2[] uv = new Vector2[vc];
                for (int i = 0; i < vc; i++) uv[i] = new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]);
                mesh.uv = uv;
            }
            mesh.triangles = data.triangles;
            // Some legacy fallback shaders are lit. Supplying normals keeps those
            // fallbacks usable even if the preferred unlit shader was stripped.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Rect[] BuildWaterAtlasUvRects(CNRDLCMapAtlas atlas)
        {
            if (atlas == null || atlas.width <= 0 || atlas.height <= 0 || atlas.entries == null)
                return new Rect[0];

            List<Rect> rects = new List<Rect>();
            float invW = 1f / atlas.width;
            float invH = 1f / atlas.height;
            const float padPixels = 0.75f;
            for (int i = 0; i < atlas.entries.Length; i++)
            {
                CNRDLCMapAtlasEntry entry = atlas.entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.id) || entry.w <= 0 || entry.h <= 0) continue;
                if (entry.id.IndexOf("water_still", StringComparison.OrdinalIgnoreCase) < 0 &&
                    entry.id.IndexOf("water_flow", StringComparison.OrdinalIgnoreCase) < 0) continue;

                float uMin = Mathf.Clamp01((entry.x - padPixels) * invW);
                float uMax = Mathf.Clamp01((entry.x + entry.w + padPixels) * invW);
                float vMin = Mathf.Clamp01(1f - (entry.y + entry.h + padPixels) * invH);
                float vMax = Mathf.Clamp01(1f - (entry.y - padPixels) * invH);
                rects.Add(new Rect(uMin, vMin, Mathf.Max(0f, uMax - uMin), Mathf.Max(0f, vMax - vMin)));
            }
            return rects.ToArray();
        }

        private static bool IsWaterAtlasUv(float u, float v)
        {
            if (_waterAtlasUvRects == null) return false;
            for (int i = 0; i < _waterAtlasUvRects.Length; i++)
            {
                Rect r = _waterAtlasUvRects[i];
                if (u >= r.xMin && u <= r.xMax && v >= r.yMin && v <= r.yMax) return true;
            }
            return false;
        }

        private static void ApplyLegacyWaterDepthBias(Vector3[] vertices, float[] rawUv, int[] triangles, Transform waterSpace)
        {
            if (vertices == null || rawUv == null || vertices.Length < 4 || rawUv.Length < vertices.Length * 2 ||
                _waterAtlasUvRects == null || _waterAtlasUvRects.Length == 0) return;

            int quadCount = vertices.Length / 4;
            for (int q = 0; q < quadCount; q++)
            {
                int first = q * 4;
                float u = 0f, v = 0f;
                for (int k = 0; k < 4; k++)
                {
                    u += rawUv[(first + k) * 2];
                    v += rawUv[(first + k) * 2 + 1];
                }
                u *= 0.25f;
                v *= 0.25f;
                if (!IsWaterAtlasUv(u, v)) continue;

                int i0 = first, i1 = first + 1, i2 = first + 2;
                int triStart = q * 6;
                if (triangles != null && triStart + 2 < triangles.Length)
                {
                    int t0 = triangles[triStart], t1 = triangles[triStart + 1], t2 = triangles[triStart + 2];
                    if (t0 >= first && t0 < first + 4 && t1 >= first && t1 < first + 4 && t2 >= first && t2 < first + 4)
                    {
                        i0 = t0;
                        i1 = t1;
                        i2 = t2;
                    }
                }

                Vector3 normal = Vector3.Cross(vertices[i1] - vertices[i0], vertices[i2] - vertices[i0]);
                float normalLength = normal.magnitude;
                if (normalLength <= 0.000001f) continue;
                normal /= normalLength;

                // Old exporters emitted vertical water faces exactly coplanar with solid
                // block faces. Both windings of each water face are present, so using the
                // triangle normal directly would push one copy inward and its reverse copy
                // outward. Probe the registered water volume on both sides instead and move
                // both windings toward the actual water interior.
                if (Mathf.Abs(normal.y) > 0.25f || waterSpace == null) continue;

                Vector3 faceCenter = Vector3.zero;
                for (int k = 0; k < 4; k++) faceCenter += vertices[first + k];
                faceCenter *= 0.25f;

                Vector3 axis = Mathf.Abs(normal.x) >= Mathf.Abs(normal.z) ? Vector3.right : Vector3.forward;
                const float probeDistance = 0.03f;
                bool plusWater = CNRMinecraftWaterRegistry.ContainsPoint(waterSpace.TransformPoint(faceCenter + axis * probeDistance));
                bool minusWater = CNRMinecraftWaterRegistry.ContainsPoint(waterSpace.TransformPoint(faceCenter - axis * probeDistance));
                if (plusWater == minusWater) continue;

                Vector3 inward = plusWater ? axis : -axis;
                Vector3 offset = inward * LegacyWaterDepthBias;
                for (int k = 0; k < 4; k++) vertices[first + k] += offset;
                _legacyWaterBiasedQuads++;
            }
        }

        private static Vector3 OriginVector(float[] raw)
        {
            if (raw == null || raw.Length < 3) return Vector3.zero;
            return new Vector3(raw[0], raw[1], raw[2]);
        }

        private static void RemapVanillaSpawnPoints()
        {
            if (_prepared == null) return;

            Vector3 origin = OriginVector(_prepared.origin);
            List<Vector3> targets = BuildSpawnTargets(_prepared.spawns, origin);
            List<Vector3> copTargets = BuildSpawnTargets(_prepared.copSpawns, origin);
            List<Vector3> robberTargets = BuildSpawnTargets(_prepared.robberSpawns, origin);

            // Generic/FFA spawn consumers can use all authored team markers when no
            // separate generic spawn was supplied. Otherwise keep the authored generic
            // list exact. A package with no markers at all retains the 50-block fallback.
            if (targets.Count == 0)
            {
                targets.AddRange(copTargets);
                targets.AddRange(robberTargets);
            }
            if (targets.Count == 0)
                targets.Add(origin + new Vector3(0f, DefaultSpawnHeight, 0f));
            if (copTargets.Count == 0) copTargets.AddRange(targets);
            if (robberTargets.Count == 0) robberTargets.AddRange(targets);

            RoomMultiplayerMenu[] menus = (RoomMultiplayerMenu[])Resources.FindObjectsOfTypeAll(typeof(RoomMultiplayerMenu));
            int remappedMenus = 0;
            for (int m = 0; m < menus.Length; m++)
            {
                RoomMultiplayerMenu menu = menus[m];
                if (menu == null) continue;
                if (menu.spawnPoints == null) menu.spawnPoints = new List<Transform>();

                Quaternion fallbackRotation = Quaternion.identity;
                for (int i = 0; i < menu.spawnPoints.Count; i++)
                {
                    if (menu.spawnPoints[i] != null)
                    {
                        fallbackRotation = menu.spawnPoints[i].rotation;
                        break;
                    }
                }

                while (menu.spawnPoints.Count < targets.Count)
                {
                    GameObject go = new GameObject("CNRDLCSpawn_" + menu.spawnPoints.Count);
                    go.transform.parent = menu.transform;
                    go.transform.localScale = Vector3.one;
                    go.transform.rotation = fallbackRotation;
                    menu.spawnPoints.Add(go.transform);
                }
                while (menu.spawnPoints.Count > targets.Count)
                    menu.spawnPoints.RemoveAt(menu.spawnPoints.Count - 1);

                for (int i = 0; i < targets.Count; i++)
                {
                    Transform spawn = menu.spawnPoints[i];
                    if (spawn == null)
                    {
                        GameObject go = new GameObject("CNRDLCSpawn_" + i);
                        go.transform.parent = menu.transform;
                        go.transform.localScale = Vector3.one;
                        go.transform.rotation = fallbackRotation;
                        spawn = go.transform;
                        menu.spawnPoints[i] = spawn;
                    }
                    spawn.position = targets[i];
                }
                remappedMenus++;
            }

            int remappedNamedSpawns = RemapNamedSceneSpawns(targets, copTargets, robberTargets);
            ModEntry.Log("DLCMap: remapped vanilla spawn list(s)=" + remappedMenus + " namedSpawns=" + remappedNamedSpawns + " generic=" + targets.Count + " cops=" + copTargets.Count + " robbers=" + robberTargets.Count);
        }

        private static List<Vector3> BuildSpawnTargets(float[][] source, Vector3 origin)
        {
            List<Vector3> result = new List<Vector3>();
            if (source == null) return result;
            for (int i = 0; i < source.Length; i++)
            {
                float[] s = source[i];
                if (s == null || s.Length < 3) continue;
                result.Add(origin + new Vector3(s[0], s[1], s[2]) * _prepared.blockScale);
            }
            return result;
        }

        private static int RemapNamedSceneSpawns(List<Vector3> targets, List<Vector3> copTargets, List<Vector3> robberTargets)
        {
            if (targets == null || targets.Count == 0) return 0;
            int moved = 0;
            int targetIndex = 0;
            int copIndex = 0;
            int robberIndex = 0;

            // PlayerLogic.RandomPosition() resolves Spawn_1_N for cops and Spawn_2_N
            // for robbers. Preserve those teams when the exported map contains armor-
            // stand spawn markers, while old maps keep using the generic spawn list.
            for (int i = 1; i <= 16; i++)
                moved += MoveNamedSpawn("Spawn_1_" + i, copTargets, ref copIndex);
            for (int i = 1; i <= 16; i++)
                moved += MoveNamedSpawn("Spawn_2_" + i, robberTargets, ref robberIndex);

            // Other vanilla modes use Spawn_1..Spawn_5 (some maps expose more).
            for (int i = 1; i <= 20; i++)
                moved += MoveNamedSpawn("Spawn_" + i, targets, ref targetIndex);

            // Cover the alternate legacy spawn helpers as well. These are harmless when
            // absent and make DLC maps work across more vanilla/custom game modes.
            for (int i = 1; i <= 20; i++)
            {
                moved += MoveNamedSpawn("SpawnList/Position" + i, targets, ref targetIndex);
                moved += MoveNamedSpawn("SpawnPosition/Position" + i, targets, ref targetIndex);
            }

            return moved;
        }

        private static int MoveNamedSpawn(string path, List<Vector3> targets, ref int targetIndex)
        {
            GameObject go = GameObject.Find(path);
            if (go == null) return 0;
            go.transform.position = targets[targetIndex % targets.Count];
            targetIndex++;
            return 1;
        }

        private static void StripBootstrapGeometry()
        {
            string[] preserve = new string[]
            {
                "Camera", "Light", "Sun", "Sky", "Fog", "Director", "Manager", "Controller",
                "Audio", "Sound", "Player", "Character", "Spawn", "Canvas", "EventSystem", "UI",
                "UIRoot", "NGUI", "_UIDrawCall", "UIPanel", "UICamera", "UISprite", "UILabel",
                "Photon", "CNRMod", "CNRDLCMap", "ExampleCharacter", "IsDied", "IsPause",
                "InGameMenu", "VCAnalog", "Joystick", "HUD", "Hud", "MainScene", "KamcordPrefab",
                "CNRSettings", "Environment", "Ambient", "Render", "Skybox", "Directional"
            };

            int cleared = 0;
            GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || go.transform.parent != null) continue;
                if (ShouldPreserve(go.name, preserve)) continue;
                if (go.GetComponent<PhotonView>() != null) continue;
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++) renderers[r].enabled = false;
                Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < colliders.Length; c++) colliders[c].enabled = false;
                cleared++;
            }
            ModEntry.Log("DLCMap: stripped bootstrap geometry roots=" + cleared);
        }

        private static bool ShouldPreserve(string name, string[] keys)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < keys.Length; i++)
                if (name.IndexOf(keys[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static void ReleaseRenderResources()
        {
            foreach (KeyValuePair<string, Material> kv in _tiledMaterials)
                if (kv.Value != null) Destroy(kv.Value);
            foreach (KeyValuePair<string, Texture2D> kv in _tiledTextures)
                if (kv.Value != null) Destroy(kv.Value);
            _tiledMaterials.Clear();
            _tiledTextures.Clear();
            _atlasEntriesById.Clear();

            if (_atlasTexture != null) { Destroy(_atlasTexture); _atlasTexture = null; }
            if (_opaqueMaterial != null) { Destroy(_opaqueMaterial); _opaqueMaterial = null; }
            if (_cutoutMaterial != null) { Destroy(_cutoutMaterial); _cutoutMaterial = null; }
            if (_transparentMaterial != null) { Destroy(_transparentMaterial); _transparentMaterial = null; }
        }
    }
}
