using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class FlowCameraTests
    {
        [UnityTest]
        public IEnumerator Flow_camera_shake_is_visual_only()
        {
            var rGo = new GameObject("FlowRig");
            var rig = rGo.AddComponent<FlowCameraRig>();
            var mGo = new GameObject("FlowMotor");
            var m = mGo.AddComponent<WickFlowMotor>();
            m.GroundAt = d => 0f;

            for (int i = 0; i < 20; i++) { m.Step(new FlowInputFrame(), 0.05f); rig.Follow(m, 0.05f); }
            Vector3 stableBefore = rig.StableForward;

            rig.Punch();
            for (int i = 0; i < 3; i++) { m.Step(new FlowInputFrame(), 0.05f); rig.Follow(m, 0.05f); }
            Vector3 stableAfter = rig.StableForward;
            float visualShake = Quaternion.Angle(Quaternion.identity, rig.VisualCam.localRotation);

            Assert.Less(Vector3.Angle(stableBefore, stableAfter), 6f, "shake contaminated the stable aim");
            Assert.Greater(visualShake, 0.1f, "visual camera did not receive shake");

            Object.Destroy(rGo);
            Object.Destroy(mGo);
            yield return null;

            Debug.Log($"[swloop] f2 stableDelta={Vector3.Angle(stableBefore, stableAfter):F2} visualShake={visualShake:F2}");
        }
    }
}
