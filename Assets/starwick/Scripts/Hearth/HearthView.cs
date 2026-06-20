using UnityEngine;

namespace Starwick
{
    public class HearthView : MonoBehaviour
    {
        public Transform[] Nodes { get; private set; }
        public bool Reacted { get; private set; }
        public int PulseNode => pulseIndex;
        public int StructureCount => structures;

        Material[] mats;
        Vector3[] baseScale;
        bool[] hasStructure;
        int structures;
        int pulseIndex = -1;
        int pulsing = -1;
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
        static readonly Color Spire = new Color(2.2f, 1.7f, 0.9f, 1f);

        static Color BaseColor(int i)
        {
            bool restored = HearthState.IsRestored(Order[i]);
            bool core = Order[i] == HearthState.Node.RainikyoCore;
            if (!restored) return Dormant;
            return core ? Restored * 1.4f : Restored;
        }

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);

            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            Nodes = new Transform[Order.Length];
            mats = new Material[Order.Length];
            baseScale = new Vector3[Order.Length];
            hasStructure = new bool[Order.Length];
            structures = 0;
            pulsing = -1;

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
                go.transform.localPosition = pos;
                go.transform.localScale = Vector3.one * (core ? 1.8f : 1.1f);

                var c = BaseColor(i);
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

                if (restored) GrowStructure(i);
            }

            motes = BuildMotes();
            pulseIndex = NextAffordable();
        }

        void GrowStructure(int i)
        {
            if (hasStructure[i] || Nodes[i] == null) return;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            var go = new GameObject("Spire");
            go.transform.SetParent(Nodes[i], false);
            go.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            go.transform.localScale = new Vector3(0.7f, 2.4f, 0.7f);
            go.AddComponent<MeshFilter>().sharedMesh = ProcMesh.Octahedron(0.6f);
            var mr = go.AddComponent<MeshRenderer>();
            var m = new Material(unlit);
            m.color = Spire;
            m.SetColor("_BaseColor", Spire);
            mr.material = m;
            hasStructure[i] = true;
            structures++;
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

        void ResetNode(int i)
        {
            if (i < 0 || Nodes == null || i >= Nodes.Length || Nodes[i] == null) return;
            Nodes[i].localScale = baseScale[i];
            if (mats[i] != null)
            {
                var c = BaseColor(i);
                mats[i].color = c;
                mats[i].SetColor("_BaseColor", c);
            }
        }

        public void React(RunResults r)
        {
            Reacted = true;
            reactClock = 0f;

            for (int i = 0; i < Order.Length; i++)
            {
                bool restored = HearthState.IsRestored(Order[i]);
                var c = BaseColor(i);
                if (mats[i] != null) { mats[i].color = c; mats[i].SetColor("_BaseColor", c); }
                if (restored) GrowStructure(i);
            }

            ResetNode(pulsing);
            pulsing = -1;
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
            if (Nodes == null) return;
            if (pulseIndex != pulsing)
            {
                ResetNode(pulsing);
                pulsing = pulseIndex;
            }
            if (pulseIndex < 0 || pulseIndex >= Nodes.Length || Nodes[pulseIndex] == null) return;

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
