using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunDiegeticTests
    {
        [UnityTest]
        public IEnumerator Trail_tracks_flow_and_sky_tracks_progress()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RunDiegetic");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(4242);

            dir.Step(new FlowInputFrame(), 0.05f);
            float calmTrail = dir.TrailWidth;
            float earlySky = dir.SkyLevel;

            int guard = 0;
            while (dir.Running && dir.Combo.ChainCount < 5 && guard++ < 900)
                dir.Step(new FlowInputFrame(), 0.05f);
            float flowingTrail = dir.TrailWidth;

            while (dir.Running && dir.Motor.Distance < 180f && guard++ < 3000)
                dir.Step(new FlowInputFrame(), 0.05f);
            float lateSky = dir.SkyLevel;

            Assert.Greater(flowingTrail, calmTrail + 0.05f, "trail did not thicken with flow");
            Assert.Greater(lateSky, earlySky + 0.05f, "sky brightness did not rise with run progress");

            Debug.Log($"[swloop] diegetic trail {calmTrail:F2}->{flowingTrail:F2} sky {earlySky:F2}->{lateSky:F2}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
