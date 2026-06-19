using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public struct GateSpec
    {
        public float Distance;
        public ConstellationGate.Kind Kind;
        public int Nodes;
        public float Spacing;
        public float Lift;
        public float Lane;
        public string Segment;
    }

    public struct RampSpec
    {
        public float Distance;
        public float Amplitude;
        public float Width;
    }

    public static class RunRhythmPlan
    {
        public static List<GateSpec> Build(int seed, float runLength)
        {
            var list = new List<GateSpec>();
            float j = (Mathf.PerlinNoise(seed * 0.013f, 4.1f) - 0.5f) * 0.02f;

            for (int i = 0; i < 3; i++)
                list.Add(new GateSpec
                {
                    Distance = runLength * (0.14f + i * 0.10f + j),
                    Kind = ConstellationGate.Kind.Thread,
                    Nodes = 3, Spacing = 3f, Lift = 1.0f, Lane = 0f, Segment = "easy",
                });

            for (int i = 0; i < 2; i++)
                list.Add(new GateSpec
                {
                    Distance = runLength * (0.40f + i * 0.10f),
                    Kind = ConstellationGate.Kind.Arc,
                    Nodes = 3, Spacing = 3f, Lift = 3.6f, Lane = i == 0 ? 0f : 1.4f, Segment = "airtime",
                });

            list.Add(new GateSpec
            {
                Distance = runLength * 0.58f,
                Kind = ConstellationGate.Kind.SplitChoice,
                Nodes = 1, Spacing = 0f, Lift = 1.0f, Lane = 2.6f, Segment = "split",
            });

            for (int i = 0; i < 4; i++)
            {
                float lane = (Mathf.PerlinNoise(seed * 0.07f, i * 1.7f) - 0.5f) * 3f;
                list.Add(new GateSpec
                {
                    Distance = runLength * (0.64f + i * 0.06f),
                    Kind = ConstellationGate.Kind.Thread,
                    Nodes = 3, Spacing = 2.5f, Lift = 1.0f, Lane = lane, Segment = "dense",
                });
            }

            list.Add(new GateSpec
            {
                Distance = runLength * 0.90f,
                Kind = ConstellationGate.Kind.Relight,
                Nodes = 3, Spacing = 3f, Lift = 1.0f, Lane = 0f, Segment = "finale",
            });

            return list;
        }

        public static List<RampSpec> Ramps(int seed, float runLength)
        {
            return new List<RampSpec>
            {
                new RampSpec { Distance = runLength * 0.37f, Amplitude = 4f, Width = 6f },
                new RampSpec { Distance = runLength * 0.47f, Amplitude = 4f, Width = 6f },
            };
        }
    }
}
