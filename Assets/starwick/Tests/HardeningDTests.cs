using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HardeningDTests
    {
        [UnityTest]
        public IEnumerator Split_offers_branches_and_locks_one()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("BranchRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            Assert.GreaterOrEqual(dir.BranchOptions, 2, "the split did not offer multiple branches");
            Assert.AreEqual(-1, dir.ChosenBranch, "a branch was locked before the run even started");

            int guard = 0;
            while (dir.Running && dir.Motor.Distance < 220f && guard++ < 3000)
                dir.Step(new FlowInputFrame(), 0.05f);

            Assert.GreaterOrEqual(dir.ChosenBranch, 0, "no branch locked in after passing the split");

            Debug.Log($"[swloop] branch options={dir.BranchOptions} chosen={dir.ChosenBranch} dist={dir.Motor.Distance:F0}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
