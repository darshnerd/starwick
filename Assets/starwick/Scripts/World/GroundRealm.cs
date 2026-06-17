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

        public static float HeightFor(int seed, float x, float z)
        {
            float ox = Mathf.Repeat(seed * 12.9898f, 1000f);
            float oz = Mathf.Repeat(seed * 78.233f, 1000f);
            float h = (Mathf.PerlinNoise((x + ox) * 0.012f, (z + oz) * 0.012f) - 0.5f) * 2f * 7f;
            h += (Mathf.PerlinNoise((x + ox) * 0.045f, (z + oz) * 0.045f) - 0.5f) * 2f * 2.2f;
            return h;
        }

        public float Height(float x, float z) => HeightFor(Seed, x, z);

        void Awake()
        {
            BuildMesh();
            BuildSites();
            ConfigureAtmosphere();
        }

        void BuildSites()
        {
            int count = 3;
            Sites = new Vector3[count];
            float baseAng = Mathf.Repeat(Seed * 0.0173f, Mathf.PI * 2f);
            for (int i = 0; i < count; i++)
            {
                float ang = baseAng + (i / (float)count) * Mathf.PI * 2f;
                float r = 30f + (i % 2) * 9f;
                float x = Mathf.Cos(ang) * r;
                float z = Mathf.Sin(ang) * r;
                Sites[i] = new Vector3(x, Height(x, z), z);
            }
        }

        void BuildMesh()
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

            gameObject.AddComponent<MeshFilter>().sharedMesh = m;
            var mr = gameObject.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.06f, 0.07f, 0.13f));
            mat.SetFloat("_Smoothness", 0.18f);
            mat.SetFloat("_Metallic", 0f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.02f, 0.03f, 0.09f));
            mr.material = mat;
        }

        void ConfigureAtmosphere()
        {
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.07f, 0.08f, 0.16f);
        }
    }
}
