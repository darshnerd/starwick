using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HardeningBTests
    {
        [UnityTest]
        public IEnumerator Bond_credits_the_companion_from_the_code()
        {
            for (int i = 0; i < 10; i++) yield return null;

            PlayerProfileStore.LoadFromJson("");
            PlayerProfileStore.Current.HearthRestored = 0;
            PlayerProfileStore.Save();
            GameState.CompanionIndex = 0;

            string code = MemorySeed.Encode(2, 4242);
            var sgo = new GameObject("RewardSess");
            var sess = sgo.AddComponent<RunSession>();
            Assert.IsTrue(sess.EnterFromCode(code), "code did not start a run");
            Assert.AreEqual(2, GameState.CompanionIndex, "companion not set from code");

            int guard = 0;
            while (sess.Dir.Running && guard++ < 3000) sess.Dir.Step(new FlowInputFrame(), 0.05f);

            var p = PlayerProfileStore.Current;
            Assert.Greater(p.Bonds[2], 0, "the run's companion earned no bond");
            Assert.AreEqual(0, p.Bonds[0], "a different companion was wrongly credited");
            Assert.AreEqual("Mara", p.CurrentHollow, "current hollow not synced to the run");

            Debug.Log($"[swloop] reward bond2={p.Bonds[2]} bond0={p.Bonds[0]} hollow={p.CurrentHollow}");

            sess.Exit();
            GameState.CompanionIndex = 0;
            Object.Destroy(sgo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Fragment_writes_a_real_memory()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("FragMem");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(5151);

            int guard = 0;
            while (dir.Running && guard++ < 400)
            {
                dir.Combo.Miss();
                dir.Step(new FlowInputFrame(), 0.05f);
            }

            Assert.IsTrue(dir.EndedAsFragment, "did not end as a fragment");

            bool found = false;
            foreach (var f in GameState.Fragments)
                if (f.Contains("slipped into the dark")) { found = true; break; }
            Assert.IsTrue(found, "fragment run stored no memory");

            Debug.Log($"[swloop] fragmem found={found} total={GameState.Fragments.Count}");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Postcard_does_not_leak_textures()
        {
            yield return null;

            var cgo = new GameObject("PostcardCam");
            var cam = cgo.AddComponent<Camera>();

            var t1 = Postcard.Capture(cam, 64, 36);
            Assert.IsNotNull(t1, "first capture was null");
            var t2 = Postcard.Capture(cam, 64, 36);
            Assert.IsNotNull(t2, "second capture was null");

            yield return null;

            Assert.IsTrue(t1 == null, "previous postcard texture was not destroyed (leak)");
            Assert.IsTrue(Postcard.Last == t2, "Last not pointing at the newest postcard");

            Debug.Log("[swloop] postcard leak-check ok");

            Object.Destroy(cgo);
            yield return null;
        }
    }
}
