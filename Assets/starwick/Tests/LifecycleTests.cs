using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class LifecycleTests
    {
        [UnityTest]
        public IEnumerator Run_session_reenters_without_leaking()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var sgo = new GameObject("Sess");
            var sess = sgo.AddComponent<RunSession>();
            Assert.IsFalse(sess.Active, "session should start idle");

            Assert.IsTrue(sess.Enter(7), "first enter failed");
            Assert.IsTrue(sess.Active, "session not active after enter");
            Assert.IsNotNull(sess.Dir, "no run director");
            Assert.IsFalse(sess.Enter(9), "duplicate enter should be blocked while a run is active");

            sess.Exit();
            yield return null;
            yield return null;
            Assert.IsFalse(sess.Active, "session should be idle after exit");

            Assert.IsTrue(sess.Enter(7), "re-enter failed");
            yield return null;
            var cams = sgo.GetComponentsInChildren<FlowCameraRig>();
            Assert.AreEqual(1, cams.Length, "re-entry leaked a flow camera");

            sess.Exit();
            yield return null;
            Debug.Log($"[swloop] lifecycle reentry cams={cams.Length}");
            Object.Destroy(sgo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Begin_and_build_are_idempotent()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var dgo = new GameObject("DoubleBegin");
            var dir = dgo.AddComponent<RunDirector>();
            dir.Begin(4242);
            int g1 = dir.GlyphLineCount;
            dir.Begin(4242);
            yield return null;
            Assert.AreEqual(g1, dir.GlyphLineCount, "double Begin changed the glyph count");
            var dcams = dgo.GetComponentsInChildren<FlowCameraRig>();
            Assert.AreEqual(1, dcams.Length, "double Begin leaked a camera");
            Object.Destroy(dgo);

            PlayerProfileStore.LoadFromJson("");
            PlayerProfileStore.Current.Starlight = 0;
            PlayerProfileStore.Current.HearthRestored = 0;
            PlayerProfileStore.Save();

            var hgo = new GameObject("DoubleBuild");
            var hv = hgo.AddComponent<HearthView>();
            hv.Build();
            hv.Build();
            yield return null;
            Assert.AreEqual(6, hv.Nodes.Length, "double Build changed node count");
            var rends = hgo.GetComponentsInChildren<MeshRenderer>();
            Assert.AreEqual(6, rends.Length, "double Build leaked node objects");

            Debug.Log($"[swloop] lifecycle idempotent glyph={g1} nodes={hv.Nodes.Length} rends={rends.Length}");
            Object.Destroy(hgo);
            yield return null;
        }
    }
}
