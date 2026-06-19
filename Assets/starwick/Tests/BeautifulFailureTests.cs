using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class BeautifulFailureTests
    {
        [UnityTest]
        public IEnumerator Sustained_darkness_ends_as_a_memory_fragment()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("FragRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(4242);

            int guard = 0;
            while (dir.Running && guard++ < 400)
            {
                dir.Combo.Miss();
                dir.Step(new FlowInputFrame(), 0.05f);
            }

            Assert.IsFalse(dir.Running, "run never ended under sustained darkness");
            Assert.IsTrue(dir.EndedAsFragment, "run did not end as a memory fragment");
            Assert.IsNotNull(dir.Results, "no results");
            Assert.IsTrue(dir.Results.Fragment, "results not flagged as a fragment");
            Assert.Less(dir.Results.Distance, dir.RunLength, "fragment should end before the finale");
            Assert.Greater(dir.Results.Starlight, 0, "even a fragment should still earn a spark");

            Debug.Log($"[swloop] fragment ended={dir.EndedAsFragment} dist={dir.Results.Distance:F0} starlight={dir.Results.Starlight}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
