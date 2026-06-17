using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FP1Tests
    {
        [UnityTest]
        public IEnumerator Relight_juice_punch_and_pop()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Decor, "no realm decor");

            var rig = Sw.Cam.GetComponent<CameraRig>();
            Assert.IsNotNull(rig, "no CameraRig on camera");

            Sw.Decor.ResetSites();
            for (int i = 0; i < 4; i++) yield return null;

            Assert.Less(rig.Shake, 0.1f, "shake not at rest before relight");
            Assert.Less(Mathf.Abs(Sw.Decor.Sites[0].Pop - 1f), 0.03f, "pop not at rest before relight");

            Sw.Constellation.TraceAll();
            yield return null;

            float shakePeak = rig.Shake;
            float popPeak = Sw.Decor.Sites[0].Pop;
            Assert.Greater(shakePeak, 0.3f, "camera did not punch on relight");
            Assert.Greater(popPeak, 1.1f, "crystals did not pop on relight");

            yield return new WaitForSeconds(1.2f);
            Assert.Less(rig.Shake, 0.1f, "screen punch did not settle");
            Assert.Less(Mathf.Abs(Sw.Decor.Sites[0].Pop - 1f), 0.07f, "crystal pop did not settle");

            Debug.Log($"[swloop] fp1 shakePeak={shakePeak:F2} popPeak={popPeak:F2}");
        }
    }
}
