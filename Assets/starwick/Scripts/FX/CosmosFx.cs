using UnityEngine;

namespace Starwick
{
    public class CosmosFx : MonoBehaviour
    {
        public int StarCount = 700;
        public float Radius = 120f;

        ParticleSystem ps;

        public int ActiveStars => ps != null ? ps.particleCount : 0;

        void Awake()
        {
            ps = gameObject.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.95f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1.7f, 1.6f, 1.4f), new Color(1.2f, 1.35f, 1.9f));
            main.maxParticles = StarCount + 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Radius;
            shape.radiusThickness = 1f;

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(shader);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Emit(StarCount);
        }
    }
}
