using UnityEngine;

namespace Starwick
{
    public class ComboSystem
    {
        public int ChainCount { get; private set; }
        public int BestChain { get; private set; }
        public float FlowMeter { get; private set; }
        public float GraceTime = 1.2f;
        public string StyleLabel { get; private set; } = "";

        public int Multiplier => 1 + ChainCount / 4;

        float graceTimer;

        public void GateHit(bool perfect)
        {
            ChainCount++;
            if (ChainCount > BestChain) BestChain = ChainCount;
            FlowMeter = Mathf.Min(1f, FlowMeter + (perfect ? 0.18f : 0.1f));
            graceTimer = GraceTime;
            StyleLabel = Label(ChainCount);
        }

        public void Miss()
        {
            FlowMeter = Mathf.Max(0f, FlowMeter - 0.15f);
        }

        public void Break()
        {
            ChainCount = 0;
            StyleLabel = "";
        }

        public void Tick(float dt)
        {
            if (ChainCount > 0)
            {
                graceTimer -= dt;
                if (graceTimer <= 0f) Break();
            }
            if (ChainCount == 0)
                FlowMeter = Mathf.MoveTowards(FlowMeter, 0f, dt * 0.2f);
        }

        static string Label(int chain)
        {
            if (chain >= 20) return "Perfect Weave";
            if (chain >= 12) return "Starfall Chain";
            if (chain >= 8) return "Hollow Sync";
            if (chain >= 4) return "Bright Drift";
            if (chain >= 1) return "Soft Trace";
            return "";
        }
    }
}
