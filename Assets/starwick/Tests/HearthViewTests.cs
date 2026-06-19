using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HearthViewTests
    {
        [UnityTest]
        public IEnumerator Hearth_view_shows_restored_nodes_brighter()
        {
            yield return null;

            PlayerProfileStore.LoadFromJson("");
            var p = PlayerProfileStore.Current;
            p.Starlight = 2000;
            p.HearthRestored = 0;
            PlayerProfileStore.Save();

            HearthState.Restore(HearthState.Node.Starwell);

            var go = new GameObject("HearthView");
            var hv = go.AddComponent<HearthView>();
            hv.Build();

            Assert.IsNotNull(hv.Nodes, "no nodes built");
            Assert.AreEqual(6, hv.Nodes.Length, "expected six hearth nodes");
            Assert.Greater(hv.Brightness(0), hv.Brightness(5) + 0.5f, "restored Starwell not brighter than dormant core");

            Object.Destroy(go);
            yield return null;
            Debug.Log($"[swloop] hview b0={hv.Brightness(0):F2} b5={hv.Brightness(5):F2} restored={HearthState.RestoredCount()}");
        }
    }
}
