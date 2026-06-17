using UnityEngine;
using UnityEngine.Rendering;

namespace Starwick
{
    public class GroundRealm : MonoBehaviour
    {
        public int Seed = 1337;
        public float Size = 180f;
        public int Res = 120;

        public Vector3[] Sites { get; private set; }

        MeshFilter mf;
        MeshRenderer mr;
        Material mat;

        public static float HeightFor(int seed, float x, float z)
        {
            float ox = Mathf.Repeat(seed * 12.9898f, 1000f);
            float oz = Mathf.Repeat(seed * 78.233f, 1000f);
            float h = (Mathf.PerlinNoise((x + ox) * 0.012f, (z + oz) * 0.012f) - 0.5f) * 2f * 7f;
            h += (Mathf.PerlinNoise((x + ox) * 0.045f, (z + oz) * 0.045f) - 0.5f) * 2f * 2.2f;
            return h;
        }

        public static Vector3[] SitesFor(int seed)
        {
            int count = 3;
            var s = new Vector3[count];
            float baseAng = Mathf.Repeat(seed * 0.0173f, Mathf.PI * 2f);
            for (int i = 0; i < count; i++)
            {
                float ang = baseAng + (i / (float)count) * Mathf.PI * 2f;
                float r = 30f + (i % 2) * 9f;
                float x = Mathf.Cos(ang) * r;
                float z = Mathf.Sin(ang) * r;
                s[i] = new Vector3(x, HeightFor(seed, x, z), z);
            }
            return s;
        }

        public float Height(float x, float z) => HeightFor(Seed, x, z);

        void Awake()
        {
            Seed = Roster.Current.Seed;
            Build();
            ConfigureAtmosphere();
        }

        public void RebuildFor(int seed)
        {
            Seed = seed;
            Build();
            ConfigureAtmosphere();
        }

        void Build()
        {
            int n = Res;
            int side = n + 1;
            var verts = new Vector3[side * side];
            var uvs = new Vector2[side * side];
            var tris = new int[n * n * 6];
            float step = Size / n;
            float half = Size * 0.5f;

            for (int z = 0; z <= n; z++)
                for (int x = 0; x <= n; x++)
                {
                    float wx = x * step - half;
                    float wz = z * step - half;
                    int idx = z * side + x;
                    verts[idx] = new Vector3(wx, Height(wx, wz), wz);
                    uvs[idx] = new Vector2((float)x / n, (float)z / n);
                }

            int t = 0;
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    int a = z * side + x;
                    int b = (z + 1) * side + x;
                    int c = (z + 1) * side + x + 1;
                    int d = z * side + x + 1;
                    tris[t++] = a; tris[t++] = b; tris[t++] = d;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }

            var m = new Mesh();
            m.indexFormat = IndexFormat.UInt32;
            m.vertices = verts;
            m.uv = uvs;
            m.triangles = tris;
            m.RecalculateNormals();
            m.RecalculateBounds();

            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
            if (mf.sharedMesh != null) Destroy(mf.sharedMesh);
            mf.sharedMesh = m;

            if (mr == null)
            {
                mr = gameObject.AddComponent<MeshRenderer>();
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetFloat("_Smoothness", 0.36f);
                mat.SetFloat("_Metallic", 0f);
                mat.EnableKeyword("_EMISSION");
                mr.material = mat;
            }

            var ground = Roster.Current.Ground;
            mat.SetColor("_BaseColor", ground);
            mat.SetColor("_EmissionColor", ground * 0.5f);

            Sites = SitesFor(Seed);
        }

        void ConfigureAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.12f);
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 175f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Roster.Current.Ambient;
        }
    }
}
