using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HearthReactTests
    {
        [UnityTest]
        public IEnumerator Hearth_reacts_after_a_run()
        {
            yield return null;

            PlayerProfileStore.LoadFromJson("");
            var p = PlayerProfileStore.Current;
            p.Starlight = 2000;
            p.HearthRestored = 0;
            PlayerProfileStore.Save();

            var go = new GameObject("HearthReact");
            var hv = go.AddComponent<HearthView>();
            hv.Build();

            Assert.GreaterOrEqual(hv.PulseNode, 0, "no affordable node was identified to pulse");
            Assert.IsTrue(HearthState.CanRestore(hv.PulseNodeId), "pulsing node is not actually affordable");

            var firstNode = hv.PulseNodeId;

            hv.React(new RunResults { Distance = 300f, GatesRelit = 3, BestChain = 5, Starlight = 80 });
            Assert.IsTrue(hv.Reacted, "hearth did not react to the run");
            Assert.Greater(hv.MoteCount, 0, "no starlight motes flowed into the hearth");

            bool restored = HearthState.Restore(firstNode);
            Assert.IsTrue(restored, "could not restore the affordable node");

            hv.React(new RunResults { Distance = 300f, GatesRelit = 1, BestChain = 1, Starlight = 20 });
            Assert.IsTrue(HearthState.IsRestored(firstNode), "restored node did not register");
            Assert.AreNotEqual(firstNode, hv.PulseNodeId, "pulse did not advance to the next affordable node");

            Debug.Log($"[swloop] hreact first={firstNode} motes={hv.MoteCount} nextPulse={hv.PulseNodeId} restored={HearthState.RestoredCount()}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
