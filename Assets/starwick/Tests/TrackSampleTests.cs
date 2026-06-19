using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class TrackSampleTests
    {
        [UnityTest]
        public IEnumerator Wick_world_follows_the_ribbon()
        {
            var t = new RunTrack(20240618);

            var f = t.Sample(100f);
            Assert.AreEqual(t.CenterX(100f), f.Center.x, 0.001f, "frame center.x off the centerline");
            Assert.AreEqual(1f, f.Right.magnitude, 0.01f, "right vector not unit length");

            Vector3 surf = t.SurfaceAt(100f, 2f);
            Assert.AreEqual(f.Center.x + f.Right.x * 2f, surf.x, 0.001f, "lane offset not applied along right");

            float sumCx = 0f;
            for (int k = 0; k < 10; k++) sumCx += Mathf.Abs(t.CenterX(k * 30f));
            Assert.Greater(sumCx, 1f, "track has no lateral curve");

            var mGo = new GameObject("M");
            var m = mGo.AddComponent<WickFlowMotor>();
            m.GroundAt = d => t.Height(d);
            m.WorldAt = (d, lane, h) => { var p = t.SurfaceAt(d, lane); p.y = h; return p; };
            for (int i = 0; i < 80; i++) m.Step(new FlowInputFrame { Held = true }, 0.05f);

            Vector3 wp = m.Position;
            Vector3 expected = t.SurfaceAt(m.Distance, m.LaneOffset);
            Assert.AreEqual(expected.x, wp.x, 0.01f, "wick world.x not on the ribbon");
            Assert.AreEqual(expected.z, wp.z, 0.01f, "wick world.z not on the ribbon");

            Object.Destroy(mGo);
            yield return null;
            Debug.Log($"[swloop] ff1 dist={m.Distance:F0} wickX={wp.x:F2} centerX={t.CenterX(m.Distance):F2}");
        }
    }
}
