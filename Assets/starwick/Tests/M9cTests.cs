using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M9cTests
    {
        [UnityTest]
        public IEnumerator Persistent_sky_and_returning_greeting()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Cosmos, "no cosmos");

            Assert.AreEqual(Mathf.Min(150, SaveData.TotalStarsRelit), Sw.Cosmos.BonusStars,
                "restored-star count does not track all-time total");
            Assert.Greater(Sw.Cosmos.BonusStars, 0, "no restored stars in a veteran sky");

            int runs = SaveData.RunsCompleted;
            SaveData.RunsCompleted = 0;
            var firstTime = StoryData.FirstConstellation();
            SaveData.RunsCompleted = 3;
            var returning = StoryData.FirstConstellation();
            SaveData.RunsCompleted = runs;

            Assert.Greater(returning.Count, firstTime.Count, "returning run has no extra greeting line");
            Assert.AreEqual("Vesp", returning[0].Speaker, "greeting is not from the companion");

            Debug.Log($"[swloop] m9c bonus={Sw.Cosmos.BonusStars} first={firstTime.Count} returning={returning.Count}");
        }
    }
}
