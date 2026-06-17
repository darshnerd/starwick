using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FP4Tests
    {
        [UnityTest]
        public IEnumerator Haptics_fire_on_relight_and_tap()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Sfx, "no SfxManager");

            Sw.Decor.ResetSites();
            yield return null;

            int before = Haptics.PulseCount;
            Sw.Constellation.TraceAll();
            yield return null;
            Assert.Greater(Haptics.PulseCount, before, "no haptics on relight");

            int afterRelight = Haptics.PulseCount;
            Sw.Companion.React();
            yield return null;
            Assert.Greater(Haptics.PulseCount, afterRelight, "no haptics on companion tap");

            Debug.Log($"[swloop] fp4 pulses={Haptics.PulseCount}");
        }
    }
}
