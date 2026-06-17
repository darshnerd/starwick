using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FP3Tests
    {
        [UnityTest]
        public IEnumerator Interaction_sfx_fire()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Sfx, "no SfxManager");

            Assert.Greater(Rms(ProcSfx.Tick()), 0.0001f, "tick sfx silent");
            Assert.Greater(Rms(ProcSfx.Shimmer()), 0.0001f, "shimmer sfx silent");
            Assert.Greater(Rms(ProcSfx.Chime()), 0.0001f, "chime sfx silent");
            Assert.Greater(Rms(ProcSfx.Confirm()), 0.0001f, "confirm sfx silent");

            Sw.Decor.ResetSites();
            yield return null;
            int before = Sw.Sfx.PlayCount;
            Sw.Constellation.TraceAll();
            yield return null;
            Assert.Greater(Sw.Sfx.PlayCount, before, "relight fired no sfx");

            int afterRelight = Sw.Sfx.PlayCount;
            Sw.Companion.React();
            yield return null;
            Assert.Greater(Sw.Sfx.PlayCount, afterRelight, "companion tap fired no sfx");

            Debug.Log($"[swloop] fp3 plays={Sw.Sfx.PlayCount}");
        }

        float Rms(AudioClip clip)
        {
            var data = new float[clip.samples];
            clip.GetData(data, 0);
            double sum = 0;
            for (int i = 0; i < data.Length; i++) sum += data[i] * data[i];
            return Mathf.Sqrt((float)(sum / data.Length));
        }
    }
}
