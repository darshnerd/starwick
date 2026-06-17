using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M6dTests
    {
        [UnityTest]
        public IEnumerator Adaptive_audio_and_reactive_cosmos()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Audio, "no AudioManager");
            Assert.IsNotNull(Sw.Cosmos, "no cosmos");

            Assert.Greater(Rms(ProcAudio.Pad()), 0.0001f, "pad stem is silent");
            Assert.Greater(Rms(ProcAudio.Tension()), 0.0001f, "tension stem is silent");

            if (Sw.Narration.ChoiceActive) Sw.Narration.HideChoices();
            if (Sw.Decor != null) Sw.Decor.ResetSites();
            Sw.Constellation.ResetForReplay();
            yield return null;

            float y = Sw.Cam.transform.position.y;
            Sw.Cam.transform.position = new Vector3(0f, y, 0f);
            yield return new WaitForSeconds(1.0f);
            float padFar = Sw.Audio.PadVol;
            float reactiveExplore = Sw.Cosmos.ReactiveLevel;

            var s0 = Sw.Realm.Sites[0];
            Sw.Cam.transform.position = new Vector3(s0.x, Sw.Cam.transform.position.y, s0.z);
            yield return new WaitForSeconds(1.0f);
            float padNear = Sw.Audio.PadVol;
            Assert.Greater(padNear, padFar + 0.1f, "pad did not swell near a site");

            Sw.Narration.ShowChoices("?", "a", "b", k => { });
            yield return new WaitForSeconds(0.8f);
            float tension = Sw.Audio.TensionVol;
            Sw.Narration.HideChoices();
            yield return null;
            Assert.Greater(tension, 0.2f, "tension layer did not rise during a choice");

            Sw.Constellation.ResetForReplay();
            yield return null;
            float energyBefore = Sw.Audio.Energy;
            Sw.Constellation.TraceAll();
            yield return null;
            yield return null;
            float energySpike = Sw.Audio.Energy;
            Assert.Greater(energySpike, energyBefore + 0.2f, "relight did not spike musical energy");

            yield return new WaitForSeconds(0.5f);
            float reactiveHigh = Sw.Cosmos.ReactiveLevel;
            Assert.Greater(reactiveHigh, reactiveExplore, "cosmos did not react to rising energy");

            Debug.Log($"[swloop] m6d padFar={padFar:F2} padNear={padNear:F2} tension={tension:F2} before={energyBefore:F2} spike={energySpike:F2} reactLo={reactiveExplore:F2} reactHi={reactiveHigh:F2}");
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
