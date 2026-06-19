using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunHollowTests
    {
        [UnityTest]
        public IEnumerator Hollow_rides_and_responds_to_combo_and_pressure()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RunHollowDir");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            Assert.IsNotNull(dir.Hollow, "no run-mode Hollow was created");

            dir.Step(new FlowInputFrame(), 0.05f);
            float calmOrbit = dir.Hollow.OrbitSpeed;

            int guard = 0;
            while (dir.Running && dir.Combo.ChainCount < 5 && guard++ < 900)
                dir.Step(new FlowInputFrame(), 0.05f);
            float chainedOrbit = dir.Hollow.OrbitSpeed;

            Assert.Greater(chainedOrbit, calmOrbit + 5f, "Hollow orbit did not speed up with the combo");

            float curlCalm = dir.Hollow.CurlAmount;
            for (int i = 0; i < 6; i++) dir.Combo.Miss();
            dir.Step(new FlowInputFrame(), 0.05f);
            Assert.Greater(dir.Hollow.CurlAmount, curlCalm + 0.3f, "Hollow did not curl in under pressure");

            Debug.Log($"[swloop] hollow calm={calmOrbit:F0} chained={chainedOrbit:F0} curl={dir.Hollow.CurlAmount:F2}");

            Object.Destroy(go);
            yield return null;
        }
    }
}
