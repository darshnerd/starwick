using UnityEngine;

namespace Starwick
{
    public class Companion : MonoBehaviour
    {
        public string DisplayName = "Vesp";
        public Color Glow = new Color(1.7f, 0.75f, 2.3f, 1f);
        public float orbitRadius = 3.0f;
        public float orbitSpeed = 28f;
        public float distance = 9f;
        public float TapRadiusPixels = 110f;

        public int TapCount { get; private set; }

        Transform core;
        Transform halo;
        Material coreMat;
        Material haloMat;
        Spring3 follow;
        float angle;
        bool wasDown;
        float excite;

        void Awake()
        {
            Glow = Roster.Current.Glow;
            DisplayName = Roster.Current.Name;

            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            var coreGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreGo.name = "Hollow";
            var c = coreGo.GetComponent<Collider>();
            if (c != null) Destroy(c);
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localScale = Vector3.one * 0.5f;
            coreMat = new Material(unlit);
            coreMat.color = Glow;
            coreMat.SetColor("_BaseColor", Glow);
            coreGo.GetComponent<MeshRenderer>().material = coreMat;
            core = coreGo.transform;

            var haloGo = new GameObject("Halo");
            haloGo.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Torus(0.55f, 0.03f, 40, 8);
            var hmr = haloGo.AddComponent<MeshRenderer>();
            haloMat = new Material(unlit);
            haloMat.color = Glow;
            haloMat.SetColor("_BaseColor", Glow);
            hmr.material = haloMat;
            haloGo.transform.SetParent(core, false);
            haloGo.transform.localRotation = Quaternion.Euler(70f, 0f, 20f);
            halo = haloGo.transform;

            var moteGo = new GameObject("Motes");
            moteGo.transform.SetParent(core, false);
            var ps = moteGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            main.startColor = new Color(2.0f, 1.4f, 2.6f, 1f);
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.7f;
            var pr = ps.GetComponent<ParticleSystemRenderer>();
            var pm = new Material(Shader.Find("Sprites/Default"));
            pm.mainTexture = ProcTex.SoftDot(64);
            pr.material = pm;
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            ps.Emit(10);

            if (Sw.Cam != null)
                transform.position = Sw.Cam.transform.position + Sw.Cam.transform.forward * distance;
            follow.value = transform.position;
        }

        public void Retint()
        {
            Glow = Roster.Current.Glow;
            DisplayName = Roster.Current.Name;
            if (coreMat != null)
            {
                coreMat.color = Glow;
                coreMat.SetColor("_BaseColor", Glow);
            }
            if (haloMat != null)
            {
                haloMat.color = Glow;
                haloMat.SetColor("_BaseColor", Glow);
            }
        }

        void Update()
        {
            float t = Time.time;
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            var cam = Sw.Cam != null ? Sw.Cam.transform : null;

            if (cam != null)
            {
                Vector3 lead = LeadDirection(cam);
                Vector3 right = Vector3.Cross(Vector3.up, lead);
                if (right.sqrMagnitude < 0.0001f) right = cam.right;
                right.Normalize();

                angle += (orbitSpeed + excite * 220f) * dt;
                float rad = angle * Mathf.Deg2Rad;
                var basePos = cam.position + lead * distance + Vector3.up * 1.4f;
                var target = basePos
                    + right * (Mathf.Cos(rad) * orbitRadius)
                    + Vector3.up * (Mathf.Sin(rad) * orbitRadius * 0.4f + Mathf.Sin(t * 1.1f) * 0.5f);

                follow.Step(target, 45f, 9f, dt);
                transform.position = follow.value;

                Vector3 toCam = cam.position - core.position;
                if (toCam.sqrMagnitude > 0.0001f)
                    core.rotation = Quaternion.Slerp(core.rotation,
                        Quaternion.LookRotation(toCam.normalized, Vector3.up), dt * 3f);

                bool down = InputService.PointerDown;
                if (down && !wasDown && !InputService.UiBlocking)
                {
                    var sp = Sw.Cam.WorldToScreenPoint(transform.position);
                    if (sp.z > 0f && Vector2.Distance(InputService.PointerPosition, sp) < TapRadiusPixels)
                        React();
                }
                wasDown = down;
            }

            if (excite > 0f) excite = Mathf.Max(0f, excite - dt * 0.6f);

            float s = 0.5f * (1f + Mathf.Sin(t * 4f) * 0.05f + excite * 0.5f);
            if (core != null) core.localScale = Vector3.one * s;
            if (halo != null) halo.Rotate(0f, (70f + excite * 360f) * dt, 0f, Space.Self);
        }

        Vector3 LeadDirection(Transform cam)
        {
            if (Sw.Decor != null && !Sw.Decor.AllSitesLit)
            {
                Vector3 toObj = Sw.Decor.ObjectiveCenter - cam.position;
                toObj.y = 0f;
                if (toObj.sqrMagnitude > 0.5f) return toObj.normalized;
            }
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            return fwd.sqrMagnitude > 0.01f ? fwd.normalized : cam.forward;
        }

        public void React()
        {
            TapCount++;
            excite = 1f;
            if (Sw.Sfx != null) Sw.Sfx.Tick();
        }
    }
}
