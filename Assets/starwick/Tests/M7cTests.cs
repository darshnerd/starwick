using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M7cTests
    {
        [UnityTest]
        public IEnumerator Companion_seeds_the_realm()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.GreaterOrEqual(Roster.All.Length, 2, "need at least 2 companions");

            var a = Roster.All[0];
            var b = Roster.All[1];
            Assert.AreNotEqual(a.Seed, b.Seed, "companions share a seed");
            Assert.AreNotEqual(a.Name, b.Name, "companions share a name");

            Assert.AreEqual(Roster.Current.Seed, Sw.Realm.Seed, "realm not seeded by the carried companion");
            Assert.AreEqual(Roster.Current.Name, Sw.Companion.DisplayName, "companion identity not applied");

            float ha = 0f, hb = 0f;
            for (int k = 0; k < 8; k++)
            {
                ha += GroundRealm.HeightFor(a.Seed, k * 5.3f, k * 3.1f);
                hb += GroundRealm.HeightFor(b.Seed, k * 5.3f, k * 3.1f);
            }
            Assert.AreNotEqual(ha, hb, "terrain identical across companions");

            var sa = GroundRealm.SitesFor(a.Seed);
            var sb = GroundRealm.SitesFor(b.Seed);
            Assert.Greater(Vector3.Distance(sa[0], sb[0]), 1f, "site layout identical across companions");

            Assert.AreNotEqual(a.Ground, b.Ground, "ground palette identical across companions");
            Assert.AreNotEqual(a.Glow, b.Glow, "companion glow identical across companions");

            Debug.Log($"[swloop] m7c current={Sw.Companion.DisplayName} seed={Sw.Realm.Seed} roster={Roster.All.Length}");
        }
    }
}
