using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class RunDirector : MonoBehaviour
    {
        public float RunLength = 300f;

        public WickFlowMotor Motor { get; private set; }
        public RunTrackGenerator Track { get; private set; }
        public FlowCameraRig Camera { get; private set; }
        public ComboSystem Combo { get; private set; }
        public bool Running { get; private set; }
        public bool RelightFired { get; private set; }
        public RunResults Results { get; private set; }
        public RunHUD Hud;

        public int GlyphLineCount => gateLines.Count;
        public float HarmonyLevel => harmonySrc != null ? harmonySrc.volume : 0f;
        public float WickLightIntensity => wickLight != null ? wickLight.intensity : 0f;
        public float WickDim { get; private set; }

        readonly List<ConstellationGate> gates = new List<ConstellationGate>();
        readonly List<Transform[]> gateNodes = new List<Transform[]>();
        readonly List<Material[]> gateNodeMats = new List<Material[]>();
        readonly List<LineRenderer> gateLines = new List<LineRenderer>();
        readonly List<float> gateEndDist = new List<float>();
        readonly List<float> gateMinApproach = new List<float>();
        readonly List<bool> gateResolved = new List<bool>();
        Transform wickVisual;
        TrailRenderer trail;
        Light wickLight;
        Material wickMat;
        AudioSource harmonySrc;
        Vector3 prevPos;
        int relitCount;
        float runClock;

        static readonly Color WickWarm = new Color(2.9f, 2.1f, 1.2f, 1f);
        static readonly Color WickDark = new Color(0.7f, 0.6f, 0.95f, 1f);
        static readonly Color LightWarm = new Color(1f, 0.8f, 0.5f);
        static readonly Color LightCold = new Color(0.4f, 0.45f, 0.85f);
        static readonly Color NodeBright = new Color(2.8f, 2.2f, 1.2f, 1f);
        static readonly Color NodeCool = new Color(0.55f, 0.72f, 1.6f, 1f);
        static readonly Color NodeRelight = new Color(1.7f, 0.7f, 0.35f, 1f);

        public void Begin(int seed)
        {
            var tGo = new GameObject("RunTrack");
            tGo.transform.SetParent(transform);
            tGo.SetActive(false);
            Track = tGo.AddComponent<RunTrackGenerator>();
            Track.Seed = seed;
            tGo.SetActive(true);

            foreach (var r in RunRhythmPlan.Ramps(seed, RunLength))
                Track.Track.AddRamp(r.Distance, r.Amplitude, r.Width);

            var mGo = new GameObject("Wick");
            mGo.transform.SetParent(transform);
            Motor = mGo.AddComponent<WickFlowMotor>();
            Motor.GroundAt = d => Track.Track.Height(d);
            Motor.WorldAt = (d, lane, h) => { var p = Track.Track.SurfaceAt(d, lane); p.y = h; return p; };
            Motor.Height = Track.Track.Height(0f);

            var cGo = new GameObject("FlowCam");
            cGo.transform.SetParent(transform);
            Camera = cGo.AddComponent<FlowCameraRig>();
            Camera.Target = null;

            var skyGo = new GameObject("FlowSky");
            skyGo.transform.SetParent(Camera.VisualCam, false);
            skyGo.AddComponent<FlowSky>();

            harmonySrc = gameObject.AddComponent<AudioSource>();
            harmonySrc.clip = ProcAudio.Pad(8);
            harmonySrc.loop = true;
            harmonySrc.spatialBlend = 0f;
            harmonySrc.playOnAwake = false;
            harmonySrc.volume = 0f;
            harmonySrc.Play();

            Combo = new ComboSystem();
            gates.Clear();
            gateNodes.Clear();
            gateNodeMats.Clear();
            gateLines.Clear();
            gateEndDist.Clear();
            gateMinApproach.Clear();
            gateResolved.Clear();
            relitCount = 0;
            runClock = 0f;
            WickDim = 0f;

            BuildWickVisual();
            PlaceGates(seed);

            prevPos = Motor.Position;
            if (wickVisual != null) wickVisual.position = prevPos;
            Results = null;
            RelightFired = false;
            Running = true;
        }

        void BuildWickVisual()
        {
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "WickVisual";
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one * 0.7f;
            wickMat = new Material(unlit);
            wickMat.color = WickWarm;
            wickMat.SetColor("_BaseColor", WickWarm);
            go.GetComponent<MeshRenderer>().material = wickMat;

            trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.7f;
            trail.startWidth = 0.55f;
            trail.endWidth = 0.02f;
            trail.numCapVertices = 4;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(2.2f, 1.6f, 2.6f, 0.9f);
            trail.endColor = new Color(0.4f, 0.3f, 0.8f, 0f);

            var lgo = new GameObject("WickLight");
            lgo.transform.SetParent(go.transform, false);
            wickLight = lgo.AddComponent<Light>();
            wickLight.type = LightType.Point;
            wickLight.color = LightWarm;
            wickLight.range = 14f;
            wickLight.intensity = 5f;

            wickVisual = go.transform;
        }

        Transform MakeGlyphNode(Sprite dotSprite, Vector3 pos, Color c, out Material mat)
        {
            var go = new GameObject("GateGlyph");
            go.transform.SetParent(transform);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 1.6f;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = QuadMesh();
            var mr = go.AddComponent<MeshRenderer>();
            mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = dotSprite.texture;
            mat.color = c;
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.transform;
        }

        static Mesh quadMesh;
        static Mesh QuadMesh()
        {
            if (quadMesh != null) return quadMesh;
            quadMesh = new Mesh();
            quadMesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)
            };
            quadMesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            quadMesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateBounds();
            return quadMesh;
        }

        void PlaceGates(int seed)
        {
            var dotTex = ProcTex.SoftDot(64);
            var dotSprite = Sprite.Create(dotTex, new Rect(0, 0, dotTex.width, dotTex.height), new Vector2(0.5f, 0.5f));
            var lineMat = new Material(Shader.Find("Sprites/Default"));

            foreach (var spec in RunRhythmPlan.Build(seed, RunLength))
            {
                bool relight = spec.Kind == ConstellationGate.Kind.Relight;
                var nodes = new List<Vector3>();
                float end = spec.Distance;
                for (int i = 0; i < spec.Nodes; i++)
                {
                    float gd = spec.Distance + i * spec.Spacing;
                    end = gd;
                    var sp = Track.Track.SurfaceAt(gd, spec.Lane);
                    sp.y += spec.Lift;
                    nodes.Add(sp);
                }
                gates.Add(new ConstellationGate(spec.Kind, nodes, 2.0f));
                gateEndDist.Add(end);
                gateMinApproach.Add(float.MaxValue);
                gateResolved.Add(false);

                Color dim = relight ? NodeRelight : NodeCool;
                var quads = new Transform[nodes.Count];
                var mats = new Material[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                {
                    quads[i] = MakeGlyphNode(dotSprite, nodes[i], dim, out mats[i]);
                    if (relight) quads[i].localScale = Vector3.one * 2.1f;
                }
                gateNodes.Add(quads);
                gateNodeMats.Add(mats);

                var lgo = new GameObject("GateThread");
                lgo.transform.SetParent(transform);
                var lr = lgo.AddComponent<LineRenderer>();
                lr.material = lineMat;
                lr.useWorldSpace = true;
                lr.widthMultiplier = relight ? 0.18f : 0.12f;
                lr.numCapVertices = 3;
                lr.numCornerVertices = 3;
                lr.textureMode = LineTextureMode.Stretch;
                lr.positionCount = nodes.Count;
                for (int i = 0; i < nodes.Count; i++) lr.SetPosition(i, nodes[i]);
                Color threadDim = (relight ? NodeRelight : NodeCool) * 0.5f;
                threadDim.a = 1f;
                lr.startColor = threadDim;
                lr.endColor = threadDim;
                gateLines.Add(lr);
            }
        }

        void UpdateGateVisuals()
        {
            Vector3 camPos = Camera != null && Camera.VisualCam != null ? Camera.VisualCam.position : Vector3.zero;
            for (int i = 0; i < gates.Count; i++)
            {
                var g = gates[i];
                var mats = gateNodeMats[i];
                var quads = gateNodes[i];
                Color dim = g.Type == ConstellationGate.Kind.Relight ? NodeRelight : NodeCool;
                for (int j = 0; j < mats.Length; j++)
                {
                    Color c;
                    if (j < g.HitCount)
                    {
                        c = NodeBright;
                    }
                    else
                    {
                        float pulse = 0.7f + 0.3f * Mathf.Sin(runClock * 3f + i * 0.7f + j * 0.4f);
                        c = dim * pulse;
                        c.a = 1f;
                    }
                    mats[j].color = c;
                    if (quads[j] != null)
                    {
                        Vector3 to = quads[j].position - camPos;
                        if (to.sqrMagnitude > 0.0001f) quads[j].rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
                    }
                }

                if (i < gateLines.Count && gateLines[i] != null)
                {
                    float lit = g.Complete ? 1f : Mathf.Clamp01(g.HitCount / Mathf.Max(1f, mats.Length));
                    Color baseC = g.Type == ConstellationGate.Kind.Relight ? NodeRelight : NodeCool;
                    Color lc = baseC * Mathf.Lerp(0.5f, 1.4f, lit);
                    lc.a = 1f;
                    gateLines[i].startColor = lc;
                    gateLines[i].endColor = lc;
                }
            }
        }

        void ApplyPressureDarkness()
        {
            float darkness = Mathf.Clamp01(Combo != null ? Combo.Pressure : 0f);
            WickDim = darkness;

            if (wickLight != null)
            {
                wickLight.intensity = Mathf.Lerp(5f, 1.8f, darkness);
                wickLight.color = Color.Lerp(LightWarm, LightCold, darkness);
            }
            if (trail != null)
            {
                trail.startWidth = Mathf.Lerp(0.55f, 0.16f, darkness);
                trail.time = Mathf.Lerp(0.7f, 0.32f, darkness);
                var sc = new Color(2.2f, 1.6f, 2.6f, Mathf.Lerp(0.9f, 0.28f, darkness));
                trail.startColor = sc;
            }
            if (wickMat != null)
            {
                Color wc = Color.Lerp(WickWarm, WickDark, darkness);
                wickMat.color = wc;
                wickMat.SetColor("_BaseColor", wc);
            }
            if (harmonySrc != null)
            {
                float flow = Combo != null ? Combo.FlowMeter : 0f;
                harmonySrc.volume = Mathf.Lerp(0f, 0.5f, flow) * (1f - 0.6f * darkness);
            }
        }

        void Burst(Vector3 pos, Color color)
        {
            var go = new GameObject("GateBurst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = 0.7f;
            main.startSpeed = 6f;
            main.startSize = 0.5f;
            main.startColor = color;
            main.stopAction = ParticleSystemStopAction.Destroy;
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 26) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.material = new Material(Shader.Find("Sprites/Default"));
            ps.Play();
        }

        void Update()
        {
            if (Running) Step(FlowInput.Read(Mathf.Max(0.0001f, Time.deltaTime)), Mathf.Max(0.0001f, Time.deltaTime));
        }

        public void Step(FlowInputFrame f, float dt)
        {
            if (!Running) return;

            Motor.Step(f, dt);
            Track.Advance(Motor.Distance);
            runClock += dt;

            if (Motor.LastLandingPerfect)
            {
                Camera.Punch();
                if (Sw.Sfx != null) Sw.Sfx.Confirm();
            }
            else if (Motor.LastLandingBad)
            {
                Combo.GraceLoss();
                if (Sw.Sfx != null) Sw.Sfx.Detune();
            }

            Vector3 cur = Motor.Position;
            if (wickVisual != null) wickVisual.position = cur;

            int aim = NextGateIndex();
            Camera.Bank = Track.Track.Bank(Motor.Distance);
            if (aim >= 0) Camera.SetLookTarget(gates[aim].NextNode, true);
            else Camera.SetLookTarget(Vector3.zero, false);
            Camera.Follow(Motor, dt);

            for (int i = 0; i < gates.Count; i++)
            {
                var g = gates[i];
                if (g.Complete) continue;

                float ap = Vector3.Distance(cur, g.NextNode);
                if (ap < gateMinApproach[i]) gateMinApproach[i] = ap;

                int beforePerfect = g.PerfectCount;
                if (g.Test(prevPos, cur))
                {
                    Combo.GateHit(g.PerfectCount > beforePerfect);
                    if (Sw.Sfx != null) Sw.Sfx.Note(Combo.ChainCount + g.HitCount);
                    if (g.Complete)
                    {
                        relitCount++;
                        Burst(cur, NodeBright);
                        if (Sw.Sfx != null) Sw.Sfx.Chord(Combo.Multiplier);
                        if (g.Type == ConstellationGate.Kind.Relight)
                        {
                            RelightFired = true;
                            Camera.Punch();
                            if (Sw.Sfx != null) Sw.Sfx.Chime();
                        }
                    }
                }

                if (!gateResolved[i] && !g.Complete && Motor.Distance > gateEndDist[i] + 4f)
                {
                    gateResolved[i] = true;
                    Combo.Miss();
                    if (Sw.Sfx != null) Sw.Sfx.Detune();
                }
            }

            UpdateGateVisuals();
            Combo.Tick(dt);
            ApplyPressureDarkness();

            if (Hud != null)
                Hud.Set(Motor.Speed, Combo.FlowMeter, Combo.ChainCount, Combo.StyleLabel,
                    Motor.Distance, relitCount, Combo.Pressure);

            prevPos = cur;

            if (Motor.Distance >= RunLength) Finish();
        }

        int NextGateIndex()
        {
            for (int i = 0; i < gates.Count; i++)
                if (!gates[i].Complete && Motor.Distance < gateEndDist[i] + 6f)
                    return i;
            return -1;
        }

        void Finish()
        {
            Running = false;
            Results = new RunResults
            {
                Distance = Motor.Distance,
                GatesRelit = relitCount,
                BestChain = Combo.BestChain,
                Starlight = relitCount * 10 + Combo.BestChain * 5,
            };
            PlayerProfileStore.ApplyRunResults(Results);
            if (Hud != null) Hud.ShowResults(Results);
        }
    }
}
