using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunFeelTests
    {
        static float Rms(AudioClip c)
        {
            var d = new float[c.samples * c.channels];
            c.GetData(d, 0);
            double s = 0;
            for (int i = 0; i < d.Length; i++) s += d[i] * d[i];
            return Mathf.Sqrt((float)(s / Mathf.Max(1, d.Length)));
        }

        [UnityTest]
        public IEnumerator Run_is_musical_and_pressure_darkens()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var note = ProcSfx.Note(4);
            var chord = ProcSfx.Chord(2);
            var det = ProcSfx.Detune();
            Assert.Greater(note.samples, 0, "note clip empty");
            Assert.Greater(Rms(note), 0.001f, "note clip silent");
            Assert.Greater(Rms(chord), 0.001f, "chord clip silent");
            Assert.Greater(Rms(det), 0.001f, "detune clip silent");

            var goA = new GameObject("RunFeelA");
            var dirA = goA.AddComponent<RunDirector>();
            dirA.Begin(4242);
            int plays0 = Sw.Sfx != null ? Sw.Sfx.PlayCount : 0;

            float maxHarmony = 0f;
            int guard = 0;
            while (dirA.Running && guard++ < 900)
            {
                dirA.Step(new FlowInputFrame(), 0.05f);
                if (dirA.HarmonyLevel > maxHarmony) maxHarmony = dirA.HarmonyLevel;
            }
            int plays1 = Sw.Sfx != null ? Sw.Sfx.PlayCount : 0;

            Assert.Greater(maxHarmony, 0f, "harmony layer never swelled with flow");
            if (Sw.Sfx != null) Assert.Greater(plays1, plays0, "no musical notes/chords played during the run");
            Object.Destroy(goA);

            var goB = new GameObject("RunFeelB");
            var dirB = goB.AddComponent<RunDirector>();
            dirB.Begin(4242);
            float litBright = dirB.WickLightIntensity;
            for (int i = 0; i < 6; i++) dirB.Combo.Miss();
            dirB.Step(new FlowInputFrame(), 0.05f);

            Assert.Greater(dirB.WickDim, 0.4f, "pressure did not darken the wick");
            Assert.Less(dirB.WickLightIntensity, litBright - 1f, "wick light did not dim under pressure");

            Debug.Log($"[swloop] runfeel maxHarm={maxHarmony:F2} plays={plays1 - plays0} wickDim={dirB.WickDim:F2} lit={litBright:F1}->{dirB.WickLightIntensity:F1}");

            Object.Destroy(goB);
            yield return null;
        }
    }
}
