using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RitualTests
    {
        [UnityTest]
        public IEnumerator Result_ritual_sequences_seed_then_starlight_to_hearth()
        {
            for (int i = 0; i < 10; i++) yield return null;

            PlayerProfileStore.LoadFromJson("");
            PlayerProfileStore.Current.Starlight = 2000;
            PlayerProfileStore.Current.HearthRestored = 0;
            PlayerProfileStore.Save();

            var go = new GameObject("RitualRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            int g = 0;
            while (dir.Running && g++ < 3000) dir.Step(new FlowInputFrame(), 0.05f);

            Assert.IsFalse(dir.Running, "run did not finish");
            Assert.IsNotNull(dir.Ritual, "no result ritual created on finish");
            Assert.AreEqual(ResultRitual.Stage.Postcard, dir.Ritual.State, "ritual did not begin at the postcard freeze");
            StringAssert.Contains("-", dir.Ritual.SeedCode);

            dir.Ritual.Tick(2f);
            Assert.AreEqual(ResultRitual.Stage.Seed, dir.Ritual.State, "ritual did not reveal the seed");

            dir.Ritual.Tick(2f);
            Assert.AreEqual(ResultRitual.Stage.Starlight, dir.Ritual.State, "ritual did not reach the starlight stage");
            Assert.IsNotNull(dir.Ritual.Hearth, "starlight stage did not build the hearth");
            Assert.IsTrue(dir.Ritual.Hearth.Reacted, "starlight did not flow into the hearth");

            dir.Ritual.Tick(2f);
            Assert.AreEqual(ResultRitual.Stage.Hearth, dir.Ritual.State, "ritual did not settle on the hearth");

            Debug.Log($"[swloop] ritual seed={dir.Ritual.SeedCode} state={dir.Ritual.State} motes={dir.Ritual.Hearth.MoteCount}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
