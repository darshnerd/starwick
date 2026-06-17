using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public static class ProcMesh
    {
        public static Mesh Torus(float radius, float tube, int seg, int sides)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i <= seg; i++)
            {
                float u = (float)i / seg * Mathf.PI * 2f;
                var center = new Vector3(Mathf.Cos(u) * radius, 0f, Mathf.Sin(u) * radius);
                for (int j = 0; j <= sides; j++)
                {
                    float v = (float)j / sides * Mathf.PI * 2f;
                    var normal = new Vector3(
                        Mathf.Cos(u) * Mathf.Cos(v),
                        Mathf.Sin(v),
                        Mathf.Sin(u) * Mathf.Cos(v));
                    verts.Add(center + normal * tube);
                }
            }
            for (int i = 0; i < seg; i++)
            {
                for (int j = 0; j < sides; j++)
                {
                    int a = i * (sides + 1) + j;
                    int b = (i + 1) * (sides + 1) + j;
                    int c = (i + 1) * (sides + 1) + j + 1;
                    int d = i * (sides + 1) + j + 1;
                    tris.Add(a); tris.Add(b); tris.Add(d);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }
            var m = new Mesh();
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        public static Mesh Octahedron(float size)
        {
            var v = new Vector3[]
            {
                new Vector3(0f, size, 0f),
                new Vector3(0f, -size, 0f),
                new Vector3(size, 0f, 0f),
                new Vector3(-size, 0f, 0f),
                new Vector3(0f, 0f, size),
                new Vector3(0f, 0f, -size),
            };
            int[] t =
            {
                0, 2, 4, 0, 4, 3, 0, 3, 5, 0, 5, 2,
                1, 4, 2, 1, 3, 4, 1, 5, 3, 1, 2, 5,
            };
            var m = new Mesh();
            m.vertices = v;
            m.triangles = t;
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
