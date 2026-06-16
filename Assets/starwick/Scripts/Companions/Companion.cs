using UnityEngine;

namespace Starwick
{
    public class Companion : MonoBehaviour
    {
        public string DisplayName = "Vesp";
        public Color Glow = new Color(1.5f, 0.65f, 2.0f, 1f);
        public float orbitRadius = 3.0f;
        public float orbitSpeed = 16f;
        public float bob = 0.6f;
        public float distance = 9f;
        public float TapRadiusPixels = 90f;

        public int TapCount { get; private set; }

        float angle;
        bool wasDown;
        float motifBase = 0.32f;
        float swell;

        void Awake()
        {
            var moteGo = new GameObject("VespMote");
            moteGo.transform.SetParent(transform, false);

            var ps = moteGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.6f);
            main.startColor = Glow;
            main.maxParticles = 16;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.mainTexture = ProcTex.SoftDot(64);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Emit(7);
        }

        void Update()
        {
            angle += orbitSpeed * Time.deltaTime;
            var cam = Sw.Cam != null ? Sw.Cam.transform : null;
            if (cam != null)
            {
                float rad = angle * Mathf.Deg2Rad;
                var basePos = cam.position + cam.forward * distance;
                var offset = cam.right * (Mathf.Cos(rad) * orbitRadius)
                           + cam.up * (Mathf.Sin(rad) * orbitRadius * 0.4f + Mathf.Sin(Time.time * 1.3f) * bob);
                transform.position = Vector3.Lerp(transform.position, basePos + offset, Time.deltaTime * 2f);

                bool down = InputService.PointerDown;
                if (down && !wasDown)
                {
                    var sp = cam.GetComponent<Camera>() != null ? Sw.Cam.WorldToScreenPoint(transform.position) : Vector3.zero;
                    if (sp.z > 0f && Vector2.Distance(InputService.PointerPosition, sp) < TapRadiusPixels)
                        React();
                }
                wasDown = down;
            }

            if (swell > 0f) swell = Mathf.Max(0f, swell - Time.deltaTime * 0.6f);
            if (Sw.Motif != null) Sw.Motif.volume = motifBase + swell * 0.35f;
        }

        public void React()
        {
            TapCount++;
            swell = 1f;
        }
    }
}
