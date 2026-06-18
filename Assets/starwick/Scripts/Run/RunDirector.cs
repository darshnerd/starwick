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

        readonly List<ConstellationGate> gates = new List<ConstellationGate>();
        Vector3 prevPos;
        int relitCount;

        public void Begin(int seed)
        {
            var tGo = new GameObject("RunTrack");
            tGo.transform.SetParent(transform);
            tGo.SetActive(false);
            Track = tGo.AddComponent<RunTrackGenerator>();
            Track.Seed = seed;
            tGo.SetActive(true);

            var mGo = new GameObject("Wick");
            mGo.transform.SetParent(transform);
            Motor = mGo.AddComponent<WickFlowMotor>();
            Motor.GroundAt = d => Track.Track.Height(d);
            Motor.Height = Track.Track.Height(0f);

            var cGo = new GameObject("FlowCam");
            cGo.transform.SetParent(transform);
            Camera = cGo.AddComponent<FlowCameraRig>();
            Camera.Target = null;

            Combo = new ComboSystem();
            gates.Clear();
            relitCount = 0;
            PlaceGates();

            prevPos = Motor.Position;
            Results = null;
            RelightFired = false;
            Running = true;
        }

        void PlaceGates()
        {
            for (float d = 60f; d <= RunLength - 30f; d += 50f)
            {
                bool relight = d >= RunLength - 80f && d < RunLength - 30f;
                var kind = relight ? ConstellationGate.Kind.Relight : ConstellationGate.Kind.Thread;
                var nodes = new List<Vector3>();
                for (int i = 0; i < 3; i++)
                {
                    float gd = d + i * 3f;
                    nodes.Add(new Vector3(0f, Track.Track.Height(gd) + 1.0f, gd));
                }
                gates.Add(new ConstellationGate(kind, nodes, 2.0f));
            }
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
            Camera.Follow(Motor, dt);

            Vector3 cur = Motor.Position;
            for (int i = 0; i < gates.Count; i++)
            {
                var g = gates[i];
                if (g.Complete) continue;
                int beforePerfect = g.PerfectCount;
                if (g.Test(prevPos, cur))
                {
                    Combo.GateHit(g.PerfectCount > beforePerfect);
                    if (Sw.Sfx != null) Sw.Sfx.Shimmer(Combo.ChainCount);
                    if (g.Complete)
                    {
                        relitCount++;
                        if (g.Type == ConstellationGate.Kind.Relight)
                        {
                            RelightFired = true;
                            Camera.Punch();
                            if (Sw.Sfx != null) Sw.Sfx.Chime();
                        }
                    }
                }
            }
            Combo.Tick(dt);
            prevPos = cur;

            if (Motor.Distance >= RunLength) Finish();
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
        }
    }
}
