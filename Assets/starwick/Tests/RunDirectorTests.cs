using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunDirectorTests
    {
        [UnityTest]
        public IEnumerator Full_run_reaches_results_with_relight()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RunDir");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(424242);
            Assert.IsTrue(dir.Running, "run did not begin");

            int guard = 0;
            while (dir.Running && guard++ < 3000) dir.Step(new FlowInputFrame(), 0.05f);

            Assert.IsFalse(dir.Running, "run never finished");
            Assert.IsNotNull(dir.Results, "no results produced");
            Assert.Greater(dir.Results.Distance, 200f, "did not travel far enough");
            Assert.GreaterOrEqual(dir.Results.GatesRelit, 3, "too few gates relit");
            Assert.IsTrue(dir.RelightFired, "relight setpiece never fired");
            Assert.Greater(dir.Results.Starlight, 0, "no reward earned");

            Object.Destroy(go);
            yield return null;
            Debug.Log($"[swloop] f5 dist={dir.Results.Distance:F0} gates={dir.Results.GatesRelit} best={dir.Results.BestChain} starlight={dir.Results.Starlight} relight={dir.RelightFired}");
        }
    }
}
