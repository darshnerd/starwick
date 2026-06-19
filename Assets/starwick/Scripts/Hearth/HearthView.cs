using UnityEngine;

namespace Starwick
{
    public class HearthView : MonoBehaviour
    {
        public Transform[] Nodes { get; private set; }
        public bool Reacted { get; private set; }
        public int PulseNode => pulseIndex;

        Material[] mats;
        Vector3[] baseScale;
        int pulseIndex = -1;
        float reactClock;
        ParticleSystem motes;

        public static readonly HearthState.Node[] Order =
        {
            HearthState.Node.Starwell,
            HearthState.Node.HollowGrove,
            HearthState.Node.LoomGate,
            HearthState.Node.MemoryArchive,
            HearthState.Node.ConstellariumChamber,
            HearthState.Node.RainikyoCore,
        };

        public HearthState.Node PulseNodeId =>
            pulseIndex >= 0 && pulseIndex < Order.Length ? Order[pulseIndex] : HearthState.Node.RainikyoCore;

        public int MoteCount => motes != null ? motes.particleCount : 0;

        static readonly Color Restored = new Color(2.6f, 2.0f, 1.1f, 1f);
        static readonly Color Dormant = new Color(0.18f, 0.22f, 0.4f, 1f);
        static readonly Color Affordable = new Color(0.6f, 0.7f, 1.4f, 1f);

        public void Build()
        {
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            Nodes = new Transform[Order.Length];
            mats = new Material[Order.Length];
            baseScale = new Vector3[Order.Length];

            for (int i = 0; i < Order.Length; i++)
            {
                bool restored = HearthState.IsRestored(Order[i]);
                float ang = i / (float)Order.Length * Mathf.PI * 2f;
                bool core = Order[i] == HearthState.Node.RainikyoCore;
                Vector3 pos = core
                    ? new Vector3(0f, 1.5f, 0f)
                    : new Vector3(Mathf.Sin(ang) * 7f, 1f, Mathf.Cos(ang) * 7f);

                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "Hearth_" + Order[i];
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
                go.transform.SetParent(transform, false);
                go.transform.position = pos;
                go.transform.localScale = Vector3.one * (core ? 1.8f : 1.1f);

                var c = restored ? Restored : Dormant;
                if (core && restored) c = Restored * 1.4f;
                var m = new Material(unlit);
                m.color = c;
                m.SetColor("_BaseColor", c);
                go.GetComponent<MeshRenderer>().material = m;

                if (restored)
                {
                    var lgo = new GameObject("HearthLight");
                    lgo.transform.SetParent(go.transform, false);
                    var light = lgo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.8f, 0.5f);
                    light.range = 9f;
                    light.intensity = 3f;
                }

                Nodes[i] = go.transform;
                mats[i] = m;
                baseScale[i] = go.transform.localScale;
            }

            motes = BuildMotes();
            pulseIndex = NextAffordable();
        }

        ParticleSystem BuildMotes()
        {
            var go = new GameObject("StarlightMotes");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 1.6f;
            main.startSpeed = 2.4f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(2.6f, 2.0f, 1.1f, 1f), new Color(2.0f, 1.5f, 0.7f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 5f;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            var tex = ProcTex.SoftDot(64);
            r.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = tex };
            r.renderMode = ParticleSystemRenderMode.Billboard;
            return ps;
        }

        int NextAffordable()
        {
            for (int i = 0; i < Order.Length; i++)
                if (HearthState.CanRestore(Order[i])) return i;
            return -1;
        }

        public void React(RunResults r)
        {
            Reacted = true;
            reactClock = 0f;

            for (int i = 0; i < Order.Length; i++)
            {
                bool restored = HearthState.IsRestored(Order[i]);
                bool core = Order[i] == HearthState.Node.RainikyoCore;
                var c = restored ? (core ? Restored * 1.4f : Restored) : Dormant;
                if (mats[i] != null) { mats[i].color = c; mats[i].SetColor("_BaseColor", c); }
            }

            pulseIndex = NextAffordable();

            if (motes != null)
            {
                Vector3 target = pulseIndex >= 0 && Nodes[pulseIndex] != null
                    ? Nodes[pulseIndex].position
                    : Vector3.up * 1.5f;
                motes.transform.position = target;
                int n = 20 + Mathf.Clamp(r != null ? r.GatesRelit * 6 : 0, 0, 60);
                motes.Emit(n);
            }
        }

        void Update()
        {
            if (pulseIndex < 0 || Nodes == null || pulseIndex >= Nodes.Length || Nodes[pulseIndex] == null) return;
            reactClock += Time.deltaTime;
            float w = 0.5f + 0.5f * Mathf.Sin(reactClock * 3f);
            Nodes[pulseIndex].localScale = baseScale[pulseIndex] * (1f + 0.18f * w);
            if (mats[pulseIndex] != null)
            {
                var c = Color.Lerp(Dormant, Affordable, w);
                mats[pulseIndex].color = c;
                mats[pulseIndex].SetColor("_BaseColor", c);
            }
        }

        public float Brightness(int i)
        {
            if (mats == null || i < 0 || i >= mats.Length) return 0f;
            var c = mats[i].GetColor("_BaseColor");
            return Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        }
    }
}
