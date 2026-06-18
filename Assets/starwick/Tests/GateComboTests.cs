using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class GateComboTests
    {
        [UnityTest]
        public IEnumerator Gate_path_intersection_and_combo_flow()
        {
            var nodes = new List<Vector3> { new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 20f) };
            var gate = new ConstellationGate(ConstellationGate.Kind.SingleNode, nodes, 1.2f);

            Assert.IsFalse(gate.Test(new Vector3(5f, 0f, 9f), new Vector3(5f, 0f, 11f)), "far segment should not hit");
            Assert.IsTrue(gate.Test(new Vector3(0f, 0f, 9f), new Vector3(0f, 0f, 11f)), "segment through node should hit");
            Assert.AreEqual(1, gate.HitCount, "hit not counted");
            Assert.AreEqual(1, gate.PerfectCount, "centre pass should be perfect");
            Assert.IsTrue(gate.Test(new Vector3(0f, 0f, 19f), new Vector3(0f, 0f, 21f)), "second node should hit");
            Assert.IsTrue(gate.Complete, "gate not complete after all nodes");

            var combo = new ComboSystem();
            for (int i = 0; i < 8; i++) combo.GateHit(i == 0);
            Assert.AreEqual(8, combo.ChainCount, "chain not counting");
            Assert.AreEqual(3, combo.Multiplier, "multiplier wrong at chain 8");
            Assert.AreEqual("Hollow Sync", combo.StyleLabel, "wrong style label at chain 8");
            Assert.Greater(combo.FlowMeter, 0f, "flow meter empty");

            for (float t = 0f; t < 2f; t += 0.1f) combo.Tick(0.1f);
            Assert.AreEqual(0, combo.ChainCount, "combo did not break after grace expired");
            Assert.AreEqual(8, combo.BestChain, "best chain not retained");

            yield return null;
            Debug.Log($"[swloop] f4 gateComplete={gate.Complete} perfect={gate.PerfectCount} best={combo.BestChain}");
        }
    }
}
