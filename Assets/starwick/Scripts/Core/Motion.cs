using UnityEngine;

namespace Starwick
{
    public struct Spring
    {
        public float value;
        public float velocity;

        public float Step(float target, float stiffness, float damping, float dt)
        {
            float force = (target - value) * stiffness - velocity * damping;
            velocity += force * dt;
            value += velocity * dt;
            return value;
        }
    }

    public struct Spring3
    {
        public Vector3 value;
        public Vector3 velocity;

        public Vector3 Step(Vector3 target, float stiffness, float damping, float dt)
        {
            Vector3 force = (target - value) * stiffness - velocity * damping;
            velocity += force * dt;
            value += velocity * dt;
            return value;
        }
    }

    public static class Ease
    {
        public static float OutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return 1f - u * u * u;
        }

        public static float InOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float OutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        public static float OutElastic(float t)
        {
            t = Mathf.Clamp01(t);
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }
}
