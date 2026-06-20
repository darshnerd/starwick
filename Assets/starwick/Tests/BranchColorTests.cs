using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class BranchColorTests
    {
        static float Dist(Color a, Color b)
        {
            return Mathf.Sqrt((a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b));
        }

        static float DirDot(Color a, Color b)
        {
            Vector3 va = new Vector3(a.r, a.g, a.b).normalized;
            Vector3 vb = new Vector3(b.r, b.g, b.b).normalized;
            return Vector3.Dot(va, vb);
        }

        [UnityTest]
        public IEnumerator Route_branches_read_as_distinct_colors()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("BranchRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            Color safe = dir.RouteColor(0);
            Color memory = dir.RouteColor(1);
            Color eclipse = dir.RouteColor(2);
            Color bond = dir.RouteColor(3);

            Assert.Greater(Dist(safe, memory), 0.3f, "safe vs memory too similar");
            Assert.Greater(Dist(safe, eclipse), 0.3f, "safe vs eclipse too similar");
            Assert.Greater(Dist(memory, eclipse), 0.3f, "memory vs eclipse too similar");
            Assert.Greater(Dist(eclipse, bond), 0.3f, "eclipse vs bond too similar");

            Assert.Greater(safe.b, safe.r, "safe route should read blue");
            Assert.Greater(safe.b, safe.g, "safe route should read blue");
            Assert.Greater(eclipse.r, eclipse.b, "eclipse route should read red");
            Assert.Greater(eclipse.r, eclipse.g, "eclipse route should read red");
            Assert.Greater(memory.b, memory.g, "memory route should read violet");
            Assert.Greater(memory.r, memory.g, "memory route should read violet");

            Color glow = Roster.Current.Glow;
            Assert.Greater(DirDot(bond, glow), 0.9f, "bond route should carry the Hollow color");

            Debug.Log($"[swloop] branchcolors safe={safe} memory={memory} eclipse={eclipse} bond={bond} glow={glow}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
