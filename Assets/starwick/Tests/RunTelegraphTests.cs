using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunTelegraphTests
    {
        [UnityTest]
        public IEnumerator Gates_telegraph_hum_and_ramps()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("TeleRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            Assert.Greater(dir.RampMarkers, 0, "no ramp silhouettes placed");

            float maxHum = 0f;
            int guard = 0;
            while (dir.Running && guard++ < 3000)
            {
                dir.Step(new FlowInputFrame(), 0.05f);
                if (dir.ApproachHum > maxHum) maxHum = dir.ApproachHum;
            }

            Assert.Greater(maxHum, 0.02f, "approach hum never swelled near a gate");

            Debug.Log($"[swloop] telegraph ramps={dir.RampMarkers} maxHum={maxHum:F2} perfect={dir.PerfectBlooms}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
