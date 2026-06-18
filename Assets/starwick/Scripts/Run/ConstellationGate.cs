using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class ConstellationGate
    {
        public enum Kind { SingleNode, Arc, Thread, Spiral, SplitChoice, Relight, Memory }

        public Kind Type;
        public float NodeRadius;
        public bool Complete { get; private set; }
        public int HitCount { get; private set; }
        public int PerfectCount { get; private set; }

        readonly List<Vector3> nodes;
        readonly bool[] hit;
        int nextIndex;

        public System.Action OnComplete;

        public int NodeCount => nodes.Count;

        public ConstellationGate(Kind type, List<Vector3> nodePositions, float radius)
        {
            Type = type;
            nodes = nodePositions;
            NodeRadius = radius;
            hit = new bool[nodes.Count];
        }

        public bool Test(Vector3 prev, Vector3 cur)
        {
            if (Complete || nextIndex >= nodes.Count) return false;

            Vector3 node = nodes[nextIndex];
            float dist = DistancePointToSegment(node, prev, cur);
            if (dist <= NodeRadius)
            {
                hit[nextIndex] = true;
                HitCount++;
                if (dist <= NodeRadius * 0.35f) PerfectCount++;
                nextIndex++;
                if (nextIndex >= nodes.Count)
                {
                    Complete = true;
                    OnComplete?.Invoke();
                }
                return true;
            }
            return false;
        }

        public Vector3 NextNode => nextIndex < nodes.Count ? nodes[nextIndex] : Vector3.zero;

        static float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector3.Distance(p, a + ab * t);
        }
    }
}
