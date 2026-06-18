using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HearthTests
    {
        [UnityTest]
        public IEnumerator Hearth_restores_nodes_and_bonds_grow()
        {
            yield return null;

            PlayerProfileStore.LoadFromJson("");
            var p = PlayerProfileStore.Current;
            p.Starlight = 0;
            p.HearthRestored = 0;
            PlayerProfileStore.Save();

            Assert.IsFalse(HearthState.CanRestore(HearthState.Node.Starwell), "should not afford a node at 0 starlight");
            Assert.IsFalse(HearthState.Restore(HearthState.Node.Starwell), "restored without paying");

            p.Starlight = 300;
            Assert.IsTrue(HearthState.CanRestore(HearthState.Node.Starwell), "should afford Starwell with 300");
            Assert.IsTrue(HearthState.Restore(HearthState.Node.Starwell), "restore failed");
            Assert.IsTrue(HearthState.IsRestored(HearthState.Node.Starwell), "node not marked restored");
            Assert.AreEqual(300 - HearthState.CostOf(HearthState.Node.Starwell), PlayerProfileStore.Current.Starlight, "starlight not spent");
            Assert.AreEqual(1, HearthState.RestoredCount(), "restored count wrong");

            Assert.IsFalse(HearthState.SecondHollowSlot, "grove not restored yet");
            PlayerProfileStore.Current.Starlight = 300;
            HearthState.Restore(HearthState.Node.HollowGrove);
            Assert.IsTrue(HearthState.SecondHollowSlot, "grove restore did not unlock a second hollow slot");

            int lvl0 = HollowBond.Level(0);
            HollowBond.Add(0, 250);
            Assert.AreEqual(lvl0 + 2, HollowBond.Level(0), "bond level did not grow");

            PlayerProfileStore.Save();
            PlayerProfileStore.Load();
            Assert.IsTrue(HearthState.IsRestored(HearthState.Node.Starwell), "restore did not persist");

            Debug.Log($"[swloop] f7 restored={HearthState.RestoredCount()} bond0={HollowBond.Level(0)} starlight={PlayerProfileStore.Current.Starlight}");
        }
    }
}
