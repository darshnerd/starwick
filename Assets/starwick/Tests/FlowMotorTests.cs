using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FlowMotorTests
    {
        [UnityTest]
        public IEnumerator Flow_motor_runs_charges_lifts_drifts()
        {
            var go = new GameObject("FlowMotorTest");
            var m = go.AddComponent<WickFlowMotor>();
            m.GroundAt = d => 0f;

            for (int i = 0; i < 5; i++) yield return null;
            Assert.AreEqual(0f, m.Distance, 0.0001f, "motor self-advanced while externally driven (double-step)");

            for (int i = 0; i < 12; i++) m.Step(new FlowInputFrame(), 0.05f);
            Assert.IsTrue(m.Grounded, "motor should settle grounded at rest");

            float d0 = m.Distance;
            for (int i = 0; i < 20; i++) m.Step(new FlowInputFrame(), 0.05f);
            Assert.Greater(m.Distance - d0, 5f, "did not auto-run forward");

            for (int i = 0; i < 10; i++)
                m.Step(new FlowInputFrame { Held = true, HoldTime = i * 0.05f }, 0.05f);
            float speedCharged = m.Speed;

            m.Step(new FlowInputFrame { Released = true, HoldTime = 0.5f }, 0.05f);
            for (int i = 0; i < 4; i++) m.Step(new FlowInputFrame(), 0.05f);
            Assert.Greater(m.Height, 0.1f, "release did not lift off the ground");
            Assert.IsFalse(m.Grounded, "should be airborne after a charged lift");
            Assert.Greater(speedCharged, 12f, "holding did not build speed");

            float lane0 = m.LaneOffset;
            for (int i = 0; i < 6; i++) m.Step(new FlowInputFrame { Swipe = new Vector2(50f, 0f) }, 0.05f);
            Assert.Greater(m.LaneOffset, lane0 + 0.5f, "swipe did not drift the lane");

            Object.Destroy(go);
            yield return null;
            Debug.Log($"[swloop] f1 dist={m.Distance:F1} speedCharged={speedCharged:F1} lane={m.LaneOffset:F2}");
        }
    }
}
