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
    internal class CNRDLCPackedBlob
    {
        public string encoding = "";
        public string dataBase64 = "";
        public int count;
        public int rawBytes;
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
        public CNRDLCPackedBlob[] opaquePacked = new CNRDLCPackedBlob[0];
        public CNRDLCPackedBlob[] cutoutPacked = new CNRDLCPackedBlob[0];
        public CNRDLCPackedBlob[] transparentPacked = new CNRDLCPackedBlob[0];
        public CNRDLCPackedBlob collisionBoxesPacked;
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
    internal class CNRDLCMapLoader : MonoBehaviour
    {
        internal const string Format = "cnr-dlc-map";
        internal const int FormatVersion = 3;
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
            // An official DLC keeps a stable cache filename across updates. Drop any
            // in-memory package before activating so a replaced file can never inherit
            // geometry/atlas data from the previous download at the same path.
            _prepared = null;
            _preparedAtlasPng = null;
            _preparedPath = "";
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
                if (map.version != LegacyFormatVersion && map.version != PackedLegacyFormatVersion && map.version != FormatVersion)
                {
                    reason = "DLC map format " + map.version + " is unsupported (expected " + LegacyFormatVersion + ", " + PackedLegacyFormatVersion + " or " + FormatVersion + ").";
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

                if ((map.version == PackedLegacyFormatVersion || map.version == FormatVersion) && !HydratePackedMap(map, out reason))
                    return false;

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
                chunk.collision = new CNRDLCMeshData[0];
            }
            return true;
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
                bool isRaw = f32Raw || q10Raw;
                bool quantizedQuads = q10Raw || q10Lz4;
                if (!f32Raw && !f32Lz4 && !q10Raw && !q10Lz4)
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
                            data.uv[i] = br.ReadUInt16() / QuantizedUvScale;
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
                    if (chunk.decodedCollisionBoxes != null && chunk.decodedCollisionBoxes.Length > 0)
                        collisionParts += BuildCollisionBoxes(chunkRoot, chunk.decodedCollisionBoxes);
                    else
                        collisionParts += BuildCollisionParts(chunkRoot, chunk.collision);

                    // Chunked meshes keep creation bounded; construction stays synchronous here
                    // because this legacy C# compiler cannot yield inside a try/catch body.
                }

                // Keep donor collision alive until the DLC collision is complete. If the
                // custom build fails, the bootstrap scene remains a safe fallback instead of
                // dropping the player into an empty world.
                StripBootstrapGeometry();

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
            // DLC maps must not depend on the donor scene's lights. Prefer truly unlit
            // shaders for every render bucket, with NGUI's bundled shader as a fallback
            // because it is guaranteed to exist in this legacy build.
            Shader opaqueShader = Shader.Find("Unlit/Texture");
            if (opaqueShader == null) opaqueShader = Shader.Find("Diffuse");
            if (opaqueShader == null) opaqueShader = Shader.Find("VertexLit");
            if (opaqueShader == null) opaqueShader = Shader.Find("Unlit/Transparent Colored");

            Shader cutoutShader = Shader.Find("Unlit/Transparent Cutout");
            if (cutoutShader == null) cutoutShader = Shader.Find("Transparent/Cutout/Diffuse");
            if (cutoutShader == null) cutoutShader = opaqueShader;

            Shader transparentShader = Shader.Find("Unlit/Transparent");
            if (transparentShader == null) transparentShader = Shader.Find("Unlit/Transparent Colored");
            if (transparentShader == null) transparentShader = Shader.Find("Transparent/Diffuse");
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

        private static int BuildCollisionBoxes(GameObject parent, float[][] boxes)
        {
            if (parent == null || boxes == null) return 0;
            int made = 0;
            for (int i = 0; i < boxes.Length; i++)
            {
                float[] b = boxes[i];
                if (b == null || b.Length < 6) continue;
                float sx = b[3] - b[0], sy = b[4] - b[1], sz = b[5] - b[2];
                if (sx <= 0f || sy <= 0f || sz <= 0f) continue;

                BoxCollider bc = parent.AddComponent<BoxCollider>();
                bc.center = new Vector3((b[0] + b[3]) * 0.5f, (b[1] + b[4]) * 0.5f, (b[2] + b[5]) * 0.5f);
                bc.size = new Vector3(sx, sy, sz);
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
            // Some legacy fallback shaders are lit. Supplying normals keeps those
            // fallbacks usable even if the preferred unlit shader was stripped.
            mesh.RecalculateNormals();
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
            if (_atlasTexture != null) { Destroy(_atlasTexture); _atlasTexture = null; }
            if (_opaqueMaterial != null) { Destroy(_opaqueMaterial); _opaqueMaterial = null; }
            if (_cutoutMaterial != null) { Destroy(_cutoutMaterial); _cutoutMaterial = null; }
            if (_transparentMaterial != null) { Destroy(_transparentMaterial); _transparentMaterial = null; }
        }
    }
}
