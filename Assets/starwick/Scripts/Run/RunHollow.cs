using UnityEngine;

namespace Starwick
{
    public class RunHollow : MonoBehaviour
    {
        public float BaseOrbitSpeed = 42f;
        public float OrbitPerChain = 11f;

        public float OrbitSpeed { get; private set; }
        public float CurlAmount { get; private set; }

        Transform core;
        Material coreMat;
        Material haloMat;
        Light light;
        Spring3 follow;
        float angle;
        float excite;
        Color glow;

        public void Init(Color g, Vector3 startPos)
        {
            glow = g;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            var coreGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreGo.name = "RunHollowCore";
            var c = coreGo.GetComponent<Collider>();
            if (c != null) Destroy(c);
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localScale = Vector3.one * 0.5f;
            coreMat = new Material(unlit);
            coreMat.color = glow;
            coreMat.SetColor("_BaseColor", glow);
            coreGo.GetComponent<MeshRenderer>().material = coreMat;
            core = coreGo.transform;

            var haloGo = new GameObject("Halo");
            haloGo.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Torus(0.55f, 0.03f, 40, 8);
            var hmr = haloGo.AddComponent<MeshRenderer>();
            haloMat = new Material(unlit);
            haloMat.color = glow;
            haloMat.SetColor("_BaseColor", glow);
            hmr.material = haloMat;
            haloGo.transform.SetParent(core, false);
            haloGo.transform.localRotation = Quaternion.Euler(70f, 0f, 20f);

            var lgo = new GameObject("HollowLight");
            lgo.transform.SetParent(core, false);
            light = lgo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = glow;
            light.range = 10f;
            light.intensity = 3.5f;

            var moteGo = new GameObject("Motes");
            moteGo.transform.SetParent(core, false);
            var ps = moteGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.4f);
            main.startColor = glow;
            main.maxParticles = 18;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;
            var pr = ps.GetComponent<ParticleSystemRenderer>();
            pr.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = ProcTex.SoftDot(64) };
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            ps.Emit(10);

            follow.value = startPos;
            transform.position = startPos;
        }

        public void React()
        {
            excite = 1f;
        }

        public void Step(Vector3 wick, Vector3 lead, int chain, float pressure, float dt)
        {
            float curl = Mathf.Clamp01(pressure);
            CurlAmount = curl;
            OrbitSpeed = BaseOrbitSpeed + chain * OrbitPerChain + excite * 200f;
            angle += OrbitSpeed * dt;

            Vector3 leadDir = lead - wick;
            leadDir.y = 0f;
            if (leadDir.sqrMagnitude > 0.0001f) leadDir.Normalize();
            else leadDir = Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, leadDir);
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
            right.Normalize();

            float radius = Mathf.Lerp(2.4f, 0.7f, curl);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 basePos = wick
                + leadDir * Mathf.Lerp(1.6f, 0.25f, curl)
                + Vector3.up * Mathf.Lerp(1.3f, 0.45f, curl);
            Vector3 target = basePos
                + right * (Mathf.Cos(rad) * radius)
                + Vector3.up * (Mathf.Sin(rad) * radius * 0.4f);

            follow.Step(target, 50f, 9f, dt);
            transform.position = follow.value;

            Color c = Color.Lerp(glow, glow * 0.35f, curl);
            if (coreMat != null) { coreMat.color = c; coreMat.SetColor("_BaseColor", c); }
            if (haloMat != null) { haloMat.color = c; haloMat.SetColor("_BaseColor", c); }
            if (light != null) light.intensity = Mathf.Lerp(3.5f, 1.2f, curl) + excite * 2.5f;
            if (core != null) core.localScale = Vector3.one * Mathf.Lerp(0.5f, 0.36f, curl);

            if (excite > 0f) excite = Mathf.Max(0f, excite - dt * 1.5f);
        }
    }
}
