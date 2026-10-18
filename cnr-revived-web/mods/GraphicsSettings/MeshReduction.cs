// Bounded runtime vertex-clustering simplification for experimental world geometry LOD.
// Conservative: single-submesh meshes only, preserve UV/normal seams and vertex colors.
// No collider or network object is altered by this code.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsSettings
{
    internal static class MeshReduction
    {
        struct Cell : IEquatable<Cell>
        {
            public int x, y, z, nx, ny, nz, u, v;
            public bool Equals(Cell b)
            {
                return x == b.x && y == b.y && z == b.z &&
                    nx == b.nx && ny == b.ny && nz == b.nz && u == b.u && v == b.v;
            }
            public override bool Equals(object o) { return o is Cell && Equals((Cell)o); }
            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + x; h = h * 31 + y; h = h * 31 + z;
                    h = h * 31 + nx; h = h * 31 + ny; h = h * 31 + nz;
                    h = h * 31 + u; h = h * 31 + v;
                    return h;
                }
            }
        }

        struct Accumulator
        {
            public Vector3 position, normal;
            public Vector2 uv;
            public Vector4 tangent;
            public Color color;
            public int count;
        }

        internal static Mesh TryReduce(Mesh original)
        {
            if (original == null || original.subMeshCount != 1) return null;
            try
            {
                Vector3[] vertices = original.vertices;
                int[] indices = original.triangles;
                if (vertices == null || indices == null ||
                    vertices.Length < 200 || vertices.Length > 12000 || indices.Length < 600 || indices.Length > 60000)
                    return null;

                Bounds bounds = original.bounds;
                Vector3 size = bounds.size;
                float maxAxis = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                if (maxAxis < 0.01f) return null;
                float step = maxAxis / 12f;

                Vector3[] normals = original.normals;
                Vector2[] uv = original.uv;
                Vector4[] tangents = original.tangents;
                Color[] colors = original.colors;
                bool hasNormals = normals != null && normals.Length == vertices.Length;
                bool hasUV = uv != null && uv.Length == vertices.Length;
                bool hasTangents = tangents != null && tangents.Length == vertices.Length;
                bool hasColors = colors != null && colors.Length == vertices.Length;

                Dictionary<Cell, int> cells = new Dictionary<Cell, int>(vertices.Length);
                List<Accumulator> sums = new List<Accumulator>();
                int[] mapping = new int[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 p = vertices[i];
                    Vector3 n = hasNormals ? normals[i] : Vector3.up;
                    Vector2 t = hasUV ? uv[i] : Vector2.zero;
                    // Preserve sharp edges and approximate texture seams. If a mesh
                    // relies on tiled UV coordinates, its seam groups will stay split.
                    Cell cell = new Cell {
                        x = Mathf.FloorToInt((p.x - bounds.min.x) / step),
                        y = Mathf.FloorToInt((p.y - bounds.min.y) / step),
                        z = Mathf.FloorToInt((p.z - bounds.min.z) / step),
                        nx = Mathf.RoundToInt(n.x * 2f),
                        ny = Mathf.RoundToInt(n.y * 2f),
                        nz = Mathf.RoundToInt(n.z * 2f),
                        u = Mathf.FloorToInt(t.x * 8f),
                        v = Mathf.FloorToInt(t.y * 8f)
                    };
                    int result;
                    if (!cells.TryGetValue(cell, out result))
                    {
                        result = sums.Count;
                        cells.Add(cell, result);
                        sums.Add(new Accumulator());
                    }
                    Accumulator a = sums[result];
                    a.position += p;
                    a.normal += n;
                    a.uv += t;
                    if (hasTangents) a.tangent += tangents[i];
                    if (hasColors) a.color += colors[i];
                    a.count++;
                    sums[result] = a;
                    mapping[i] = result;
                }

                // Avoid creating a fake LOD when clustering cannot reduce enough
                // geometry to compensate for an extra mesh and renderer.
                if (sums.Count >= vertices.Length * 0.8f) return null;

                List<int> reducedIndices = new List<int>(indices.Length);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    int a = mapping[indices[i]], b = mapping[indices[i + 1]], c = mapping[indices[i + 2]];
                    if (a == b || b == c || a == c) continue;
                    reducedIndices.Add(a); reducedIndices.Add(b); reducedIndices.Add(c);
                }
                if (reducedIndices.Count < 3 || reducedIndices.Count >= indices.Length * 0.8f) return null;

                Vector3[] outVertices = new Vector3[sums.Count];
                Vector3[] outNormals = new Vector3[sums.Count];
                Vector2[] outUV = new Vector2[sums.Count];
                Vector4[] outTangents = new Vector4[sums.Count];
                Color[] outColors = new Color[sums.Count];
                for (int i = 0; i < sums.Count; i++)
                {
                    Accumulator a = sums[i];
                    float inv = 1f / a.count;
                    outVertices[i] = a.position * inv;
                    outNormals[i] = (a.normal * inv).normalized;
                    outUV[i] = a.uv * inv;
                    if (hasTangents) outTangents[i] = a.tangent * inv;
                    if (hasColors) outColors[i] = a.color * inv;
                }
                Mesh resultMesh = new Mesh();
                resultMesh.name = "GraphicsSettings_LOD_" + original.name;
                resultMesh.vertices = outVertices;
                resultMesh.uv = outUV;
                if (hasNormals) resultMesh.normals = outNormals;
                if (hasTangents) resultMesh.tangents = outTangents;
                if (hasColors) resultMesh.colors = outColors;
                resultMesh.triangles = reducedIndices.ToArray();
                if (!hasNormals) resultMesh.RecalculateNormals();
                resultMesh.RecalculateBounds();
                return resultMesh;
            }
            catch (Exception)
            {
                // A substantial fraction of shipped Unity mesh assets are not
                // CPU-readable. Never force them through the wrapper's render path.
                return null;
            }
        }
    }
}
