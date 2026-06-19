using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class RunTrack
    {
        public int Seed;

        readonly List<float[]> ramps = new List<float[]>();

        public RunTrack(int seed) { Seed = seed; }

        public static float HeightFor(int seed, float d)
        {
            float ox = Mathf.Repeat(seed * 0.3137f, 1000f);
            float h = (Mathf.PerlinNoise((d + ox) * 0.012f, ox * 0.01f) - 0.5f) * 2f * 5f;
            h += Mathf.Sin((d + ox) * 0.018f) * 3f;
            h += Mathf.Sin((d + ox) * 0.006f) * 3f;
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

        public static float BankFor(int seed, float d)
        {
            float c0 = CenterXFor(seed, d - 3f);
            float c1 = CenterXFor(seed, d + 3f);
            float turn = (c1 - c0) / 6f;
            return Mathf.Clamp(-turn * 26f, -24f, 24f);
        }

        public void AddRamp(float d, float amp, float width)
        {
            ramps.Add(new[] { d, amp, Mathf.Max(0.5f, width) });
        }

        float RampSum(float d)
        {
            float s = 0f;
            for (int i = 0; i < ramps.Count; i++)
            {
                var r = ramps[i];
                float x = (d - r[0]) / r[2];
                s += r[1] * Mathf.Exp(-x * x);
            }
            return s;
        }

        public float Height(float d) => HeightFor(Seed, d) + RampSum(d);
        public float Width(float d) => WidthFor(Seed, d);
        public float CenterX(float d) => CenterXFor(Seed, d);
        public float Bank(float d) => BankFor(Seed, d);

        public Vector3 Center(float d) => new Vector3(CenterX(d), Height(d), d);

        public Vector3 Tangent(float d)
        {
            Vector3 a = Center(d - 0.5f);
            Vector3 b = Center(d + 0.5f);
            var t = b - a;
            return t.sqrMagnitude > 0.0001f ? t.normalized : Vector3.forward;
        }

        public TrackFrame Sample(float d)
        {
            Vector3 c = Center(d);
            Vector3 tan = Tangent(d);
            Vector3 right = Vector3.Cross(Vector3.up, tan);
            right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
            return new TrackFrame { Center = c, Tangent = tan, Right = right };
        }

        public Vector3 SurfaceAt(float d, float lane)
        {
            var f = Sample(d);
            return f.Center + f.Right * lane;
        }
    }

    public struct TrackFrame
    {
        public Vector3 Center;
        public Vector3 Tangent;
        public Vector3 Right;
    }
}
