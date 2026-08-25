using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace CNRMods
{
    [Serializable]
    internal class CNRDLCMapAtlas
    {
        public int width;
        public int height;
        public string pngBase64 = "";
    }

    [Serializable]
    internal class CNRDLCMeshData
    {
        public float[] vertices = new float[0];
        public float[] uv = new float[0];
        public int[] triangles = new int[0];
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
    }

    // Dedicated map path for exported/baked DLC maps. This intentionally does not
    // share the legacy donor-object cloning pipeline in MapLoader.
    internal class CNRDLCMapLoader : MonoBehaviour
    {
        internal const string Format = "cnr-dlc-map";
        internal const int FormatVersion = 1;
        internal const string PrefActive = "CNRMod_DLCMapActive";
        internal const string PrefPath = "CNRMod_DLCMapPath";
        internal const string PrefId = "CNRMod_DLCMapId";
        internal const string PrefUrl = "CNRMod_DLCMapURL";
        internal const string BootstrapScene = "FreeRun3_1";

        private const int MaxAtlasBytes = 32 * 1024 * 1024;
        private const int MaxVerticesPerPart = 65000;

        private static CNRDLCMapFile _prepared;
        private static byte[] _preparedAtlasPng;
        private static string _preparedPath = "";
        private static GameObject _mapRoot;
        private static Texture2D _atlasTexture;
        private static Material _opaqueMaterial;
        private static Material _cutoutMaterial;
        private static Material _transparentMaterial;

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

        internal static bool ActivateFile(string path, string url, string id, out string reason)
        {
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

                if (_prepared != null && string.Equals(_preparedPath, path, StringComparison.OrdinalIgnoreCase))
                    return true;

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
                if (map.version != FormatVersion)
                {
                    reason = "DLC map format " + map.version + " is unsupported (expected " + FormatVersion + ").";
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
            if (cc != null) cc.enabled = false;

            try
            {
                if (_mapRoot != null) Destroy(_mapRoot);
                ReleaseRenderResources();
                StripBootstrapGeometry();

                _atlasTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                _atlasTexture.name = "CNRDLCMapAtlas";
                if (!_atlasTexture.LoadImage(_preparedAtlasPng)) throw new Exception("atlas PNG could not be decoded");
                _atlasTexture.filterMode = FilterMode.Point;
                _atlasTexture.wrapMode = TextureWrapMode.Clamp;

                BuildMaterials();
                _mapRoot = new GameObject("CNRDLCMapRoot");
                _mapRoot.transform.position = OriginVector(_prepared.origin);

                int renderParts = 0;
                int collisionParts = 0;
                for (int i = 0; i < _prepared.chunks.Length; i++)
                {
                    CNRDLCMapChunk chunk = _prepared.chunks[i];
                    GameObject chunkRoot = new GameObject("Chunk_" + chunk.x + "_" + chunk.y + "_" + chunk.z);
                    chunkRoot.transform.parent = _mapRoot.transform;
                    chunkRoot.transform.localPosition = new Vector3(chunk.x, chunk.y, chunk.z) * _prepared.blockScale;
                    chunkRoot.transform.localRotation = Quaternion.identity;
                    chunkRoot.transform.localScale = Vector3.one * _prepared.blockScale;
                    chunkRoot.isStatic = true;

                    renderParts += BuildRenderParts(chunkRoot, chunk.opaque, _opaqueMaterial, "Opaque");
                    renderParts += BuildRenderParts(chunkRoot, chunk.cutout, _cutoutMaterial, "Cutout");
                    renderParts += BuildRenderParts(chunkRoot, chunk.transparent, _transparentMaterial, "Transparent");
                    collisionParts += BuildCollisionParts(chunkRoot, chunk.collision);

                    // Chunked meshes keep creation bounded; construction stays synchronous here
                    // because this legacy C# compiler cannot yield inside a try/catch body.
                }

                ModEntry.Log("DLCMap built: chunks=" + _prepared.chunks.Length + " renderParts=" + renderParts + " collisionParts=" + collisionParts);
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

        private static void BuildMaterials()
        {
            Shader opaqueShader = Shader.Find("Unlit/Texture");
            if (opaqueShader == null) opaqueShader = Shader.Find("Diffuse");
            Shader cutoutShader = Shader.Find("Transparent/Cutout/Diffuse");
            if (cutoutShader == null) cutoutShader = Shader.Find("Unlit/Transparent");
            if (cutoutShader == null) cutoutShader = opaqueShader;
            Shader transparentShader = Shader.Find("Unlit/Transparent");
            if (transparentShader == null) transparentShader = Shader.Find("Transparent/Diffuse");
            if (transparentShader == null) transparentShader = cutoutShader;

            _opaqueMaterial = new Material(opaqueShader); _opaqueMaterial.name = "CNRDLCMap Opaque"; _opaqueMaterial.mainTexture = _atlasTexture;
            _cutoutMaterial = new Material(cutoutShader); _cutoutMaterial.name = "CNRDLCMap Cutout"; _cutoutMaterial.mainTexture = _atlasTexture;
            _transparentMaterial = new Material(transparentShader); _transparentMaterial.name = "CNRDLCMap Transparent"; _transparentMaterial.mainTexture = _atlasTexture;
        }

        private static int BuildRenderParts(GameObject parent, CNRDLCMeshData[] parts, Material material, string label)
        {
            if (parts == null || material == null) return 0;
            int made = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                CNRDLCMeshData data = parts[i];
                if (data == null || data.vertices == null || data.vertices.Length == 0) continue;
                Mesh mesh = MakeMesh(data, true);
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
                Mesh mesh = MakeMesh(data, false);
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

        private static Mesh MakeMesh(CNRDLCMeshData data, bool withUv)
        {
            int vc = data.vertices.Length / 3;
            Vector3[] vertices = new Vector3[vc];
            for (int i = 0; i < vc; i++)
                vertices[i] = new Vector3(data.vertices[i * 3], data.vertices[i * 3 + 1], data.vertices[i * 3 + 2]);

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
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 OriginVector(float[] raw)
        {
            if (raw == null || raw.Length < 3) return Vector3.zero;
            return new Vector3(raw[0], raw[1], raw[2]);
        }

        private static void RemapVanillaSpawnPoints()
        {
            if (_prepared == null) return;

            List<Vector3> targets = new List<Vector3>();
            Vector3 origin = OriginVector(_prepared.origin);
            if (_prepared.spawns != null)
            {
                for (int i = 0; i < _prepared.spawns.Length; i++)
                {
                    float[] s = _prepared.spawns[i];
                    if (s == null || s.Length < 3) continue;
                    targets.Add(origin + new Vector3(s[0], s[1], s[2]) * _prepared.blockScale);
                }
            }
            if (targets.Count == 0) targets.Add(origin + new Vector3(0f, 3f, 0f));

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

            int remappedNamedSpawns = RemapNamedSceneSpawns(targets);
            ModEntry.Log("DLCMap: remapped vanilla spawn list(s)=" + remappedMenus + " namedSpawns=" + remappedNamedSpawns + " points=" + targets.Count);
        }

        private static int RemapNamedSceneSpawns(List<Vector3> targets)
        {
            if (targets == null || targets.Count == 0) return 0;
            int moved = 0;
            int targetIndex = 0;

            // CNR's actual mobile multiplayer path uses PlayerLogic.RandomPosition(),
            // which resolves these GameObjects by name. Move the existing donor scene
            // markers before PlayerLogic.Start() so both initial spawn and respawns use
            // DLC coordinates without any post-spawn teleport.
            for (int team = 1; team <= 2; team++)
            {
                for (int i = 1; i <= 16; i++)
                    moved += MoveNamedSpawn("Spawn_" + team + "_" + i, targets, ref targetIndex);
            }

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

        private static Vector3 FirstSpawn()
        {
            Vector3 origin = OriginVector(_prepared.origin);
            if (_prepared.spawns != null && _prepared.spawns.Length > 0 && _prepared.spawns[0] != null && _prepared.spawns[0].Length >= 3)
            {
                float[] s = _prepared.spawns[0];
                return origin + new Vector3(s[0], s[1], s[2]) * _prepared.blockScale;
            }
            return origin + new Vector3(0f, 3f, 0f);
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
            if (_atlasTexture != null) { Destroy(_atlasTexture); _atlasTexture = null; }
            if (_opaqueMaterial != null) { Destroy(_opaqueMaterial); _opaqueMaterial = null; }
            if (_cutoutMaterial != null) { Destroy(_cutoutMaterial); _cutoutMaterial = null; }
            if (_transparentMaterial != null) { Destroy(_transparentMaterial); _transparentMaterial = null; }
        }
    }
}
