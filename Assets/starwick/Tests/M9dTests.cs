using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M9dTests
    {
        [UnityTest]
        public IEnumerator Ng_plus_unlocks_a_secret_companion()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");

            Assert.AreEqual(4, Roster.All.Length, "expected a 4th, secret companion");
            Assert.AreEqual(0, Roster.All[0].UnlockRuns, "starter companion should be unlocked");
            Assert.Greater(Roster.All[3].UnlockRuns, 0, "4th companion should be replay-gated");

            Assert.AreEqual(3, Roster.UnlockedCount(0), "secret companion leaked on the first run");
            Assert.AreEqual(3, Roster.UnlockedCount(2), "secret companion unlocked too early");
            Assert.AreEqual(4, Roster.UnlockedCount(Roster.All[3].UnlockRuns), "secret companion not unlocked at its run gate");

            Debug.Log($"[swloop] m9d roster={Roster.All.Length} unlocked0={Roster.UnlockedCount(0)} unlockedGate={Roster.UnlockedCount(Roster.All[3].UnlockRuns)}");
        }
    }
}
