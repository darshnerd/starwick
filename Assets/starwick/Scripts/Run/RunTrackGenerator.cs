using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Starwick
{
    public class ChunkParts : MonoBehaviour
    {
        public LineRenderer Center;
        public LineRenderer Left;
        public LineRenderer Right;
    }

    public class RunTrackGenerator : MonoBehaviour
    {
        public int Seed = 20240618;
        public float ChunkLength = 100f;
        public int SamplesPerChunk = 48;
        public int ChunksAhead = 8;
        public int ChunksBehind = 3;

        public RunTrack Track { get; private set; }
        public int ActiveChunks => active.Count;
        public int LastChunkVertexCount { get; private set; }

        readonly Dictionary<int, GameObject> active = new Dictionary<int, GameObject>();
        readonly Queue<GameObject> pool = new Queue<GameObject>();
        readonly List<int> tmpRemove = new List<int>();
        Material mat;
        Material seamMat;

        static readonly Color SeamWarm = new Color(1.5f, 1.3f, 0.8f, 1f);
        static readonly Color SeamCool = new Color(0.45f, 0.62f, 1.35f, 1f);

        void Awake()
        {
            Track = new RunTrack(Seed);
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.07f, 0.08f, 0.14f));
            mat.SetFloat("_Smoothness", 0.4f);
            mat.SetFloat("_Metallic", 0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.04f, 0.05f, 0.11f));
            seamMat = new Material(Shader.Find("Sprites/Default"));
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

        LineRenderer NewSeam(Transform parent, float width, Color c)
        {
            var go = new GameObject("Seam");
            go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = seamMat;
            lr.useWorldSpace = true;
            lr.widthMultiplier = width;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;
            lr.startColor = c;
            lr.endColor = c;
            lr.textureMode = LineTextureMode.Stretch;
            return lr;
        }

        GameObject NewChunk()
        {
            var go = new GameObject("TrackChunk");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            var parts = go.AddComponent<ChunkParts>();
            parts.Center = NewSeam(go.transform, 0.22f, SeamWarm);
            parts.Left = NewSeam(go.transform, 0.13f, SeamCool);
            parts.Right = NewSeam(go.transform, 0.13f, SeamCool);
            return go;
        }

        void BuildMesh(GameObject go, int index)
        {
            int n = SamplesPerChunk;
            float d0 = index * ChunkLength;
            float step = ChunkLength / n;

            var verts = new Vector3[(n + 1) * 3];
            var cols = new Color[(n + 1) * 3];
            var tris = new int[n * 12];

            var parts = go.GetComponent<ChunkParts>();
            var centerPts = new Vector3[n + 1];
            var leftPts = new Vector3[n + 1];
            var rightPts = new Vector3[n + 1];

            for (int i = 0; i <= n; i++)
            {
                float d = d0 + i * step;
                Vector3 c = Track.Center(d);
                Vector3 tan = Track.Tangent(d);
                Vector3 right = Vector3.Cross(Vector3.up, tan).normalized;
                right = Quaternion.AngleAxis(Track.Bank(d) * 1.25f, tan) * right;
                float w = Track.Width(d) * 0.5f;

                Vector3 lp = c - right * w;
                Vector3 rp = c + right * w;
                verts[i * 3] = lp;
                verts[i * 3 + 1] = c;
                verts[i * 3 + 2] = rp;

                float shade = 0.6f + 0.4f * Mathf.PerlinNoise(d * 0.05f, 0f);
                float crest = Mathf.Clamp01((c.y - Track.Center(d - 6f).y) * 0.4f + 0.5f);
                var edge = new Color(0.16f * shade, 0.2f * shade, 0.34f * shade, 1f);
                var mid = Color.Lerp(edge, new Color(0.3f, 0.34f, 0.5f, 1f), crest);
                cols[i * 3] = edge;
                cols[i * 3 + 1] = mid;
                cols[i * 3 + 2] = edge;

                Vector3 up = Vector3.up * 0.04f;
                centerPts[i] = c + up;
                leftPts[i] = lp + up;
                rightPts[i] = rp + up;
            }

            int t = 0;
            for (int i = 0; i < n; i++)
            {
                int a = i * 3;
                int b = (i + 1) * 3;
                tris[t++] = a; tris[t++] = b; tris[t++] = a + 1;
                tris[t++] = a + 1; tris[t++] = b; tris[t++] = b + 1;
                tris[t++] = a + 1; tris[t++] = b + 1; tris[t++] = a + 2;
                tris[t++] = a + 2; tris[t++] = b + 1; tris[t++] = b + 2;
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
            LastChunkVertexCount = verts.Length;

            SetSeam(parts.Center, centerPts);
            SetSeam(parts.Left, leftPts);
            SetSeam(parts.Right, rightPts);
        }

        void SetSeam(LineRenderer lr, Vector3[] pts)
        {
            if (lr == null) return;
            lr.positionCount = pts.Length;
            lr.SetPositions(pts);
        }
    }
}
