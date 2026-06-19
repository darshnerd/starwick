using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FlowFeelTests
    {
        static WickFlowMotor Make(System.Func<float, float> ground)
        {
            var go = new GameObject("FeelMotor");
            var m = go.AddComponent<WickFlowMotor>();
            m.GroundAt = ground;
            return m;
        }

        [UnityTest]
        public IEnumerator Slope_momentum_landings_and_pressure()
        {
            var flat = Make(d => 0f);
            var down = Make(d => -d * 0.3f);
            var up = Make(d => d * 0.3f);
            for (int i = 0; i < 40; i++)
            {
                flat.Step(new FlowInputFrame(), 0.05f);
                down.Step(new FlowInputFrame(), 0.05f);
                up.Step(new FlowInputFrame(), 0.05f);
            }
            Assert.Greater(down.Speed, flat.Speed + 0.5f, "downhill did not accelerate");
            Assert.Less(up.Speed, flat.Speed - 0.5f, "uphill did not slow");

            var hop = Make(d => 0f);
            for (int i = 0; i < 12; i++) hop.Step(new FlowInputFrame(), 0.05f);
            hop.Step(new FlowInputFrame { Released = true, HoldTime = 0.12f }, 0.05f);
            bool sawPerfect = false;
            for (int i = 0; i < 80; i++)
            {
                hop.Step(new FlowInputFrame(), 0.05f);
                if (hop.LastLandingPerfect) sawPerfect = true;
                if (hop.Grounded && i > 3) break;
            }
            Assert.IsTrue(sawPerfect, "a soft glide did not score a perfect landing");

            var slam = Make(d => 0f);
            for (int i = 0; i < 12; i++) slam.Step(new FlowInputFrame(), 0.05f);
            for (int i = 0; i < 8; i++) slam.Step(new FlowInputFrame { Held = true, HoldTime = i * 0.05f }, 0.05f);
            slam.Step(new FlowInputFrame { Released = true, HoldTime = 0.5f }, 0.05f);
            bool sawBad = false;
            for (int i = 0; i < 90; i++)
            {
                slam.Step(new FlowInputFrame { Held = true }, 0.05f);
                if (slam.LastLandingBad) sawBad = true;
                if (slam.Grounded && i > 3) break;
            }
            Assert.IsTrue(sawBad, "diving into the ground was not a bad landing");

            var combo = new ComboSystem();
            for (int i = 0; i < 4; i++) combo.GateHit(false);
            Assert.AreEqual(4, combo.ChainCount, "chain not built");
            combo.Miss();
            Assert.AreEqual(0, combo.ChainCount, "miss did not break the chain");
            Assert.Greater(combo.Pressure, 0f, "miss did not build pressure");
            float p0 = combo.Pressure;
            for (int i = 0; i < 30; i++) combo.Tick(0.1f);
            Assert.Less(combo.Pressure, p0, "pressure did not relax over time");

            Object.Destroy(flat.gameObject);
            Object.Destroy(down.gameObject);
            Object.Destroy(up.gameObject);
            Object.Destroy(hop.gameObject);
            Object.Destroy(slam.gameObject);
            yield return null;
            Debug.Log($"[swloop] feel down={down.Speed:F1} flat={flat.Speed:F1} up={up.Speed:F1} perfect={sawPerfect} bad={sawBad} pressure0={p0:F2}");
        }
    }
}
