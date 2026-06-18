using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class ProfileTests
    {
        [UnityTest]
        public IEnumerator Profile_migrates_round_trips_and_records_runs()
        {
            yield return null;

            var old = new PlayerProfile { Version = 0, TotalRuns = 5, Starlight = 100 };
            PlayerProfileStore.LoadFromJson(JsonUtility.ToJson(old));
            Assert.AreEqual(PlayerProfileStore.CurrentVersion, PlayerProfileStore.Current.Version, "did not migrate version");
            Assert.AreEqual(5, PlayerProfileStore.Current.TotalRuns, "lost data during migration");

            PlayerProfileStore.Current.Starlight = 777;
            PlayerProfileStore.Current.TotalRuns = 12;
            PlayerProfileStore.Save();
            PlayerProfileStore.Current.Starlight = 0;
            PlayerProfileStore.Current.TotalRuns = 0;
            PlayerProfileStore.Load();
            Assert.AreEqual(777, PlayerProfileStore.Current.Starlight, "starlight not persisted");
            Assert.AreEqual(12, PlayerProfileStore.Current.TotalRuns, "runs not persisted");

            int runs0 = PlayerProfileStore.Current.TotalRuns;
            int stars0 = PlayerProfileStore.Current.TotalStarsRelit;
            int sl0 = PlayerProfileStore.Current.Starlight;
            PlayerProfileStore.ApplyRunResults(new RunResults { GatesRelit = 4, Starlight = 80, BestChain = 6, Distance = 300f });
            Assert.AreEqual(runs0 + 1, PlayerProfileStore.Current.TotalRuns, "run not recorded");
            Assert.AreEqual(stars0 + 4, PlayerProfileStore.Current.TotalStarsRelit, "stars not added");
            Assert.AreEqual(sl0 + 80, PlayerProfileStore.Current.Starlight, "starlight not added");

            Debug.Log($"[swloop] f6 v={PlayerProfileStore.Current.Version} runs={PlayerProfileStore.Current.TotalRuns} starlight={PlayerProfileStore.Current.Starlight}");
        }
    }
}
