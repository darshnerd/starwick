using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M9aTests
    {
        [UnityTest]
        public IEnumerator Persistence_round_trips_and_counts()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");

            SaveData.TotalStarsRelit = 4242;
            SaveData.RunsCompleted = 9;
            SaveData.Save();
            SaveData.TotalStarsRelit = 0;
            SaveData.RunsCompleted = 0;
            SaveData.Load();
            Assert.AreEqual(4242, SaveData.TotalStarsRelit, "stars did not persist");
            Assert.AreEqual(9, SaveData.RunsCompleted, "runs did not persist");

            int before = SaveData.TotalStarsRelit;
            SaveData.RecordRelight();
            Assert.AreEqual(before + 1, SaveData.TotalStarsRelit, "RecordRelight did not increment");
            SaveData.TotalStarsRelit = -1;
            SaveData.Load();
            Assert.AreEqual(before + 1, SaveData.TotalStarsRelit, "RecordRelight did not persist across reload");

            SaveData.MarkCompanionSeen(2);
            Assert.IsTrue((SaveData.CompanionsSeen & (1 << 2)) != 0, "companion not marked seen");
            Assert.GreaterOrEqual(SaveData.CompanionsSeenCount(), 1, "seen count wrong");

            int t0 = SaveData.TotalStarsRelit;
            Sw.Decor.ResetSites();
            yield return null;
            Sw.Constellation.TraceAll();
            yield return null;
            Assert.Greater(SaveData.TotalStarsRelit, t0, "in-game relight did not persist to SaveData");

            Debug.Log($"[swloop] m9a total={SaveData.TotalStarsRelit} runs={SaveData.RunsCompleted} comp={SaveData.CompanionsSeen}");
        }
    }
}
