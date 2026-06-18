using UnityEngine;

namespace Starwick
{
    public class RunTrack
    {
        public int Seed;

        public RunTrack(int seed) { Seed = seed; }

        public static float HeightFor(int seed, float d)
        {
            float ox = Mathf.Repeat(seed * 0.3137f, 1000f);
            float h = (Mathf.PerlinNoise((d + ox) * 0.012f, ox * 0.01f) - 0.5f) * 2f * 5f;
            h += Mathf.Sin((d + ox) * 0.018f) * 3f;
            return h;
        }

        public static float WidthFor(int seed, float d)
        {
            float ox = Mathf.Repeat(seed * 0.731f, 1000f);
            return 6f + Mathf.PerlinNoise((d + ox) * 0.01f, 7.3f) * 4f;
        }

        public static float CenterXFor(int seed, float d)
        {
            float ox = Mathf.Repeat(seed * 0.911f, 1000f);
            return Mathf.Sin((d + ox) * 0.009f) * 6f;
        }

        public float Height(float d) => HeightFor(Seed, d);
        public float Width(float d) => WidthFor(Seed, d);
        public float CenterX(float d) => CenterXFor(Seed, d);

        public Vector3 Center(float d) => new Vector3(CenterX(d), Height(d), d);

        public Vector3 Tangent(float d)
        {
            Vector3 a = Center(d - 0.5f);
            Vector3 b = Center(d + 0.5f);
            var t = b - a;
            return t.sqrMagnitude > 0.0001f ? t.normalized : Vector3.forward;
        }
    }
}
