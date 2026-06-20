using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RouteIdentityTests
    {
        [UnityTest]
        public IEnumerator Centered_run_takes_the_safe_route()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RouteSafe");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            Assert.AreEqual(4, dir.BranchOptions, "expected four distinct route options");

            int g = 0;
            while (dir.Running && dir.Motor.Distance < 220f && g++ < 3000)
                dir.Step(new FlowInputFrame(), 0.05f);

            Assert.AreEqual(0, dir.ChosenRoute, "centered run did not take the safe route");
            Assert.Greater(dir.RouteBonus, 0, "route did not apply its reward");

            Debug.Log($"[swloop] route-safe options={dir.BranchOptions} chosen={dir.ChosenRoute} bonus={dir.RouteBonus}");
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Steering_into_memory_route_records_a_memory()
        {
            for (int i = 0; i < 10; i++) yield return null;

            int before = GameState.Fragments.Count;
            var go = new GameObject("RouteMem");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            var swipeLeft = new FlowInputFrame { Swipe = new Vector2(-2f, 0f) };
            int g = 0;
            while (dir.Running && dir.Motor.Distance < 220f && g++ < 3000)
                dir.Step(swipeLeft, 0.05f);

            Assert.AreEqual(1, dir.ChosenRoute, "steering left did not take the memory route");
            Assert.Greater(GameState.Fragments.Count, before, "memory route recorded no fragment");

            Debug.Log($"[swloop] route-mem chosen={dir.ChosenRoute} frags {before}->{GameState.Fragments.Count}");
            Object.Destroy(go);
            yield return null;
        }
    }
}
