using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RhythmTests
    {
        [UnityTest]
        public IEnumerator Rhythm_plan_banking_and_ramps()
        {
            var plan = RunRhythmPlan.Build(7, 300f);
            Assert.GreaterOrEqual(plan.Count, 8, "rhythm plan too sparse");

            int relight = 0, arc = 0, thread = 0, split = 0;
            foreach (var g in plan)
            {
                if (g.Kind == ConstellationGate.Kind.Relight) relight++;
                if (g.Kind == ConstellationGate.Kind.Arc) arc++;
                if (g.Kind == ConstellationGate.Kind.Thread) thread++;
                if (g.Kind == ConstellationGate.Kind.SplitChoice) split++;
            }
            Assert.AreEqual(1, relight, "expected exactly one relight finale");
            Assert.Greater(arc, 0, "no airtime arc gates");
            Assert.Greater(split, 0, "no split-choice gate");
            Assert.Greater(thread, 3, "not enough trace gates");

            var finale = plan.Find(g => g.Kind == ConstellationGate.Kind.Relight);
            Assert.Greater(finale.Distance, 300f * 0.8f, "relight finale not near the end");

            var ramps = RunRhythmPlan.Ramps(7, 300f);
            Assert.Greater(ramps.Count, 0, "no ramps authored");

            float maxBank = 0f;
            for (int d = 0; d < 300; d += 3) maxBank = Mathf.Max(maxBank, Mathf.Abs(RunTrack.BankFor(7, d)));
            Assert.Greater(maxBank, 0.5f, "track never banks into a turn");

            var t = new RunTrack(7);
            float baseH = t.Height(ramps[0].Distance);
            t.AddRamp(ramps[0].Distance, ramps[0].Amplitude, ramps[0].Width);
            Assert.Greater(t.Height(ramps[0].Distance), baseH + 1f, "ramp did not raise the surface");

            yield return null;
            Debug.Log($"[swloop] rhythm gates={plan.Count} arc={arc} thread={thread} split={split} finale={finale.Distance:F0} ramps={ramps.Count} bank={maxBank:F2}");
        }
    }
}
