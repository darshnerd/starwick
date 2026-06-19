using UnityEngine;

namespace Starwick
{
    public class FlowSky : MonoBehaviour
    {
        public int StarCount = 520;
        public float Radius = 150f;
        public float Brightness { get; private set; } = 1f;

        ParticleSystem stars;
        ParticleSystem nebula;
        Material starMat;

        bool fogWas;
        FogMode fogModeWas;
        Color fogColorWas;
        float fogStartWas;
        float fogEndWas;
        bool fogCaptured;

        void Awake()
        {
            stars = BuildLayer("FlowStars", ProcTex.SoftDot(64), Radius,
                new Color(1.9f, 1.8f, 1.6f, 1f), new Color(1.3f, 1.5f, 2.1f, 1f),
                0.5f, 1.7f, StarCount);
            nebula = BuildLayer("FlowNebula", ProcTex.Nebula(128, 1337), Radius * 0.82f,
                new Color(0.55f, 0.4f, 1.1f, 0.4f), new Color(0.3f, 0.55f, 1.1f, 0.42f),
                26f, 52f, 28);
            starMat = stars.GetComponent<ParticleSystemRenderer>().material;
        }

        ParticleSystem BuildLayer(string n, Texture2D tex, float radius, Color a, Color b,
            float minSize, float maxSize, int count)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = count + 8;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            shape.radiusThickness = 1f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = tex };
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Emit(count);
            return ps;
        }

        public void SetBrightness(float progress)
        {
            float level = Mathf.Lerp(0.7f, 1.5f, Mathf.Clamp01(progress));
            Brightness = level;
            if (starMat != null) starMat.color = new Color(level, level, level, 1f);
        }

        void OnEnable()
        {
            if (!fogCaptured)
            {
                fogWas = RenderSettings.fog;
                fogModeWas = RenderSettings.fogMode;
                fogColorWas = RenderSettings.fogColor;
                fogStartWas = RenderSettings.fogStartDistance;
                fogEndWas = RenderSettings.fogEndDistance;
                fogCaptured = true;
            }
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.07f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 210f;
        }

        void RestoreFog()
        {
            if (!fogCaptured) return;
            RenderSettings.fog = fogWas;
            RenderSettings.fogMode = fogModeWas;
            RenderSettings.fogColor = fogColorWas;
            RenderSettings.fogStartDistance = fogStartWas;
            RenderSettings.fogEndDistance = fogEndWas;
        }

        void OnDisable() => RestoreFog();
        void OnDestroy() => RestoreFog();
    }
}
