using UnityEngine;

namespace Starwick
{
    public class CosmosFx : MonoBehaviour
    {
        public int StarCount = 700;
        public int TwinkleCount = 200;
        public int NebulaCount = 28;
        public float Radius = 120f;

        ParticleSystem stars;
        ParticleSystem twinkle;
        ParticleSystem nebula;

        public int ActiveStars => stars != null ? stars.particleCount : 0;
        public int ActiveNebula => nebula != null ? nebula.particleCount : 0;

        void Awake()
        {
            var dot = ProcTex.SoftDot(64);
            var cloud = ProcTex.Nebula(128, 4242);
            var shader = Shader.Find("Sprites/Default");

            stars = NewLayer("Stars", StarCount, Radius, 0.12f, 0.7f,
                new Color(2.0f, 1.85f, 1.6f, 1f), new Color(1.35f, 1.55f, 2.2f, 1f), dot, shader, false);
            stars.Emit(StarCount);

            twinkle = NewLayer("Twinkle", TwinkleCount, Radius, 0.5f, 1.3f,
                new Color(2.6f, 2.4f, 2.0f, 1f), new Color(1.7f, 2.0f, 2.8f, 1f), dot, shader, false);
            MakeTwinkle(twinkle, TwinkleCount);

            nebula = NewLayer("Nebula", NebulaCount, Radius * 0.78f, 40f, 95f,
                new Color(0.75f, 0.35f, 1.0f, 0.32f), new Color(0.22f, 0.5f, 0.95f, 0.34f), cloud, shader, true);
            nebula.Emit(NebulaCount);
        }

        void MakeTwinkle(ParticleSystem ps, int count)
        {
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 6f);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = count / 4f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(grad);

            ps.Play();
        }

        ParticleSystem NewLayer(string layerName, int count, float radius, float minSize, float maxSize,
            Color colorA, Color colorB, Texture2D tex, Shader shader, bool randomRotation)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            if (randomRotation)
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.283f);
            main.maxParticles = count + 300;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.radiusThickness = 1f;

            var material = new Material(shader);
            material.mainTexture = tex;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }
    }
}
