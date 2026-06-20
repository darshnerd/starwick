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
        public int TrackRenderQueue => mat != null ? mat.renderQueue : 0;
        public int SparkBudget => sparks != null ? sparks.main.maxParticles : 0;

        readonly Dictionary<int, GameObject> active = new Dictionary<int, GameObject>();
        readonly Queue<GameObject> pool = new Queue<GameObject>();
        readonly List<int> tmpRemove = new List<int>();
        Material mat;
        Material seamMat;
        ParticleSystem sparks;

        static readonly Color SeamWarm = new Color(1.9f, 1.6f, 1.0f, 1f);
        static readonly Color SeamCool = new Color(0.55f, 0.78f, 1.7f, 1f);

        void Awake()
        {
            Track = new RunTrack(Seed);

            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetColor("_BaseColor", new Color(0.10f, 0.13f, 0.27f, 0.5f));
            mat.SetFloat("_Smoothness", 0.55f);
            mat.SetFloat("_Metallic", 0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.10f, 0.14f, 0.32f));

            seamMat = new Material(Shader.Find("Sprites/Default"));

            BuildSparks();
            Advance(0f);
        }

        void BuildSparks()
        {
            var go = new GameObject("EdgeSparks");
            go.transform.SetParent(transform, false);
            sparks = go.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 1.6f;
            main.startSpeed = 0.6f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(2.2f, 1.8f, 1.1f, 1f), new Color(0.7f, 0.9f, 2.0f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;
            var emission = sparks.emission;
            emission.enabled = true;
            emission.rateOverTime = 14f;
            var shape = sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f, 0.4f, ChunkLength);
            var r = sparks.GetComponent<ParticleSystemRenderer>();
            r.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = ProcTex.SoftDot(64) };
            r.renderMode = ParticleSystemRenderMode.Billboard;
            sparks.Play();
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

            if (sparks != null)
            {
                var fr = Track.Sample(distance + ChunkLength * 0.4f);
                sparks.transform.position = fr.Center + fr.Up * 0.3f;
                sparks.transform.rotation = Quaternion.LookRotation(fr.Tangent, fr.Up);
                var shape = sparks.shape;
                shape.scale = new Vector3(Track.Width(distance) + 2f, 0.4f, ChunkLength * 0.8f);
            }
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
            parts.Center = NewSeam(go.transform, 0.26f, SeamWarm);
            parts.Left = NewSeam(go.transform, 0.16f, SeamCool);
            parts.Right = NewSeam(go.transform, 0.16f, SeamCool);
            return go;
        }

        void BuildMesh(GameObject go, int index)
        {
            int n = SamplesPerChunk;
            float d0 = index * ChunkLength;
            float step = ChunkLength / n;

            var verts = new Vector3[(n + 1) * 3];
            var norms = new Vector3[(n + 1) * 3];
            var cols = new Color[(n + 1) * 3];
            var tris = new int[n * 12];

            var parts = go.GetComponent<ChunkParts>();
            var centerPts = new Vector3[n + 1];
            var leftPts = new Vector3[n + 1];
            var rightPts = new Vector3[n + 1];

            for (int i = 0; i <= n; i++)
            {
                float d = d0 + i * step;
                var fr = Track.Sample(d);
                Vector3 c = fr.Center;
                Vector3 right = fr.Right;
                Vector3 up = fr.Up;
                float w = Track.Width(d) * 0.5f;

                Vector3 lp = c - right * w;
                Vector3 rp = c + right * w;
                verts[i * 3] = lp;
                verts[i * 3 + 1] = c;
                verts[i * 3 + 2] = rp;
                norms[i * 3] = up;
                norms[i * 3 + 1] = up;
                norms[i * 3 + 2] = up;

                float shade = 0.6f + 0.4f * Mathf.PerlinNoise(d * 0.05f, 0f);
                var edge = new Color(0.16f * shade, 0.2f * shade, 0.34f * shade, 0.5f);
                var midc = new Color(0.3f, 0.34f, 0.5f, 0.62f);
                cols[i * 3] = edge;
                cols[i * 3 + 1] = midc;
                cols[i * 3 + 2] = edge;

                Vector3 lift = up * 0.05f;
                centerPts[i] = c + lift;
                leftPts[i] = lp + lift;
                rightPts[i] = rp + lift;
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
            m.normals = norms;
            m.colors = cols;
            m.triangles = tris;
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
