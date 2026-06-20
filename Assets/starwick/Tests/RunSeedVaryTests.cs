using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunSeedVaryTests
    {
        [UnityTest]
        public IEnumerator Consecutive_runs_seed_differently()
        {
            for (int i = 0; i < 10; i++) yield return null;

            PlayerProfileStore.LoadFromJson("");
            int runs0 = PlayerProfileStore.Current.TotalRuns;
            int seed0 = ColdOpen.SeedForRun(runs0);

            var go = new GameObject("SeedRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(seed0);

            int g = 0;
            while (dir.Running && g++ < 3000) dir.Step(new FlowInputFrame(), 0.05f);

            int runs1 = PlayerProfileStore.Current.TotalRuns;
            Assert.Greater(runs1, runs0, "finishing a run did not increment the profile run count");

            int seed1 = ColdOpen.SeedForRun(runs1);
            Assert.AreNotEqual(seed0, seed1, "consecutive cold-open runs produce the same seed");

            Debug.Log($"[swloop] seedvary runs {runs0}->{runs1} seeds {seed0} vs {seed1}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
