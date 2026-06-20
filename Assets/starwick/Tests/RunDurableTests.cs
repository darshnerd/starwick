using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunDurableTests
    {
        [UnityTest]
        public IEnumerator Results_carry_route_seed_and_memory_persists()
        {
            for (int i = 0; i < 10; i++) yield return null;

            PlayerProfileStore.LoadFromJson("");

            var go = new GameObject("DurRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(4242);

            int gg = 0;
            while (dir.Running && gg++ < 3000) dir.Step(new FlowInputFrame(), 0.05f);

            var r = dir.Results;
            Assert.IsNotNull(r, "no results");
            Assert.AreEqual(4242, r.Seed, "results did not record the run seed");
            Assert.AreEqual(0, r.RouteId, "centered run should record the safe route");
            Assert.Greater(r.RouteBonus, 0, "results did not record the route bonus");

            PlayerProfileStore.AddMemory("persist-test-xyz");
            string json = JsonUtility.ToJson(PlayerProfileStore.Current);
            PlayerProfileStore.LoadFromJson(json);
            Assert.IsTrue(PlayerProfileStore.Current.MemoryArchive.Contains("persist-test-xyz"),
                "memory did not persist through profile json");

            Debug.Log($"[swloop] durable seed={r.Seed} route={r.RouteId} bonus={r.RouteBonus} perfect={r.PerfectCount} mem={PlayerProfileStore.Current.MemoryArchive.Count}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
