using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M7aTests
    {
        [UnityTest]
        public IEnumerator Realm_progression_relights_every_site()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Decor, "no realm decor");

            int n = Sw.Realm.Sites.Length;
            Assert.GreaterOrEqual(n, 3, "expected at least 3 sites");

            Sw.Decor.ResetSites();
            yield return null;
            Assert.IsFalse(Sw.Decor.AllSitesLit, "all sites lit before play");

            int before = GameState.StarsRelit;

            for (int i = 0; i < n; i++)
            {
                float t = 0f;
                while (Sw.Constellation.Complete && t < 4f) { t += Time.deltaTime; yield return null; }

                Assert.AreEqual(i, Sw.Constellation.SiteIndex, "constellation not at the expected site");
                Sw.Constellation.TraceAll();
                yield return null;

                Assert.IsTrue(Sw.Decor.Sites[i].Lit, "site " + i + " not lit after trace");
                if (i < n - 1)
                {
                    Assert.IsTrue(Sw.Decor.Sites[i + 1].IsNext, "next-site beacon not raised");
                    Assert.IsFalse(Sw.Decor.AllSitesLit, "all-lit raised too early");
                    yield return new WaitForSeconds(2.6f);
                }
            }

            Assert.IsTrue(Sw.Decor.AllSitesLit, "realm not fully relit after every site");
            Assert.GreaterOrEqual(GameState.StarsRelit, before + n, "relit count too low");

            Debug.Log($"[swloop] m7a sites={n} allLit={Sw.Decor.AllSitesLit} relit={GameState.StarsRelit - before}");
        }
    }
}
