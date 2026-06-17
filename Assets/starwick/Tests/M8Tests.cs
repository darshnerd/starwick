using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M8Tests
    {
        [UnityTest]
        public IEnumerator Reseed_rebuilds_realm_for_new_companion()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Decor, "no realm decor");
            Assert.GreaterOrEqual(Roster.All.Length, 2, "need at least 2 companions");

            int seed0 = Sw.Realm.Seed;
            string name0 = Sw.Companion.DisplayName;
            Vector3 site0a = Sw.Realm.Sites[0];

            Sw.Decor.Reseed(1);
            yield return null;
            yield return null;

            Assert.AreEqual(Roster.All[1].Seed, Sw.Realm.Seed, "realm not reseeded to companion 1");
            Assert.AreEqual(Roster.All[1].Name, Sw.Companion.DisplayName, "companion not swapped");
            Assert.AreEqual(3, Sw.Decor.Sites.Length, "sites missing after reseed");
            Assert.Greater(Vector3.Distance(site0a, Sw.Realm.Sites[0]), 1f, "site layout did not change");

            yield return new WaitForSeconds(0.6f);
            var p = Sw.Cam.transform.position;
            float expected = Sw.Realm.Height(p.x, p.z) + 1.7f;
            Assert.Less(Mathf.Abs(p.y - expected), 1.2f, "player not grounded on the rebuilt realm");

            Sw.Decor.Reseed(0);
            yield return null;
            Assert.AreEqual(seed0, Sw.Realm.Seed, "did not reseed back to the start realm");
            Assert.AreEqual(name0, Sw.Companion.DisplayName, "companion not restored");

            Debug.Log($"[swloop] m8 seed0={seed0} name0={name0} back={Sw.Realm.Seed}");
        }
    }
}
