using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HearthLivingTests
    {
        [UnityTest]
        public IEnumerator Restoring_a_node_grows_a_structure()
        {
            yield return null;

            PlayerProfileStore.LoadFromJson("");
            var p = PlayerProfileStore.Current;
            p.Starlight = 2000;
            p.HearthRestored = 0;
            PlayerProfileStore.Save();

            var go = new GameObject("LivingHearth");
            var hv = go.AddComponent<HearthView>();
            hv.Build();

            Assert.AreEqual(0, hv.StructureCount, "dormant hearth should have no structures");

            bool ok = HearthState.Restore(HearthState.Node.Starwell);
            Assert.IsTrue(ok, "could not restore Starwell");

            hv.React(new RunResults { Distance = 300f, GatesRelit = 2, BestChain = 3, Starlight = 40 });

            Assert.AreEqual(1, hv.StructureCount, "restoring a node did not grow a structure");

            Debug.Log($"[swloop] living structures={hv.StructureCount} restored={HearthState.RestoredCount()}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
