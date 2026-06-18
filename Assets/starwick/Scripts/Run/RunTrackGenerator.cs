using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Starwick
{
    public class RunTrackGenerator : MonoBehaviour
    {
        public int Seed = 20240618;
        public float ChunkLength = 100f;
        public int SamplesPerChunk = 48;
        public int ChunksAhead = 8;
        public int ChunksBehind = 3;

        public RunTrack Track { get; private set; }
        public int ActiveChunks => active.Count;

        readonly Dictionary<int, GameObject> active = new Dictionary<int, GameObject>();
        readonly Queue<GameObject> pool = new Queue<GameObject>();
        readonly List<int> tmpRemove = new List<int>();
        Material mat;

        void Awake()
        {
            Track = new RunTrack(Seed);
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.07f, 0.08f, 0.14f));
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Metallic", 0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.03f, 0.04f, 0.09f));
            Advance(0f);
        }

        public void Advance(float distance)
        {
            int current = Mathf.FloorToInt(distance / ChunkLength);
            int from = current - ChunksBehind;
            int to = current + ChunksAhead;

            tmpRemove.Clear();
            foreach (var kv in active)
                if (kv.Key < from || kv.Key > to) tmpRemove.Add(kv.Key);
            for (int i = 0; i < tmpRemove.Count; i++)
            {
                Recycle(active[tmpRemove[i]]);
                active.Remove(tmpRemove[i]);
            }

            for (int i = from; i <= to; i++)
                if (i >= 0 && !active.ContainsKey(i))
                    active[i] = BuildChunk(i);
        }

        GameObject BuildChunk(int index)
        {
            GameObject go = pool.Count > 0 ? pool.Dequeue() : NewChunk();
            BuildMesh(go, index);
            go.SetActive(true);
            return go;
        }

        void Recycle(GameObject go)
        {
            go.SetActive(false);
            pool.Enqueue(go);
        }

        GameObject NewChunk()
        {
            var go = new GameObject("TrackChunk");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            return go;
        }

        void BuildMesh(GameObject go, int index)
        {
            int n = SamplesPerChunk;
            float d0 = index * ChunkLength;
            float step = ChunkLength / n;

            var verts = new Vector3[(n + 1) * 2];
            var cols = new Color[(n + 1) * 2];
            var tris = new int[n * 6];

            for (int i = 0; i <= n; i++)
            {
                float d = d0 + i * step;
                Vector3 c = Track.Center(d);
                Vector3 tan = Track.Tangent(d);
                Vector3 right = Vector3.Cross(Vector3.up, tan).normalized;
                float w = Track.Width(d) * 0.5f;
                verts[i * 2] = c - right * w;
                verts[i * 2 + 1] = c + right * w;
                float shade = 0.6f + 0.4f * Mathf.PerlinNoise(d * 0.05f, 0f);
                var col = new Color(0.12f * shade, 0.14f * shade, 0.22f * shade, 1f);
                cols[i * 2] = col;
                cols[i * 2 + 1] = col;
            }

            int t = 0;
            for (int i = 0; i < n; i++)
            {
                int a = i * 2;
                tris[t++] = a; tris[t++] = a + 2; tris[t++] = a + 1;
                tris[t++] = a + 1; tris[t++] = a + 2; tris[t++] = a + 3;
            }

            var mf = go.GetComponent<MeshFilter>();
            var m = mf.sharedMesh;
            if (m == null) { m = new Mesh(); mf.sharedMesh = m; }
            m.Clear();
            m.indexFormat = IndexFormat.UInt16;
            m.vertices = verts;
            m.colors = cols;
            m.triangles = tris;
            m.RecalculateNormals();
            m.RecalculateBounds();
        }
    }
}
