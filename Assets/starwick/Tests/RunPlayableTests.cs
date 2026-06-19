using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunPlayableTests
    {
        [UnityTest]
        public IEnumerator Launcher_enters_a_live_run_with_hud()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("Launcher");
            var launcher = go.AddComponent<RunLauncher>();
            launcher.EnterRun(7);

            Assert.IsTrue(launcher.Active, "launcher did not activate");
            Assert.IsNotNull(launcher.Dir, "no run director");
            Assert.IsTrue(launcher.Dir.Running, "run not running after entering");
            Assert.IsNotNull(launcher.Hud, "no run HUD");
            Assert.AreEqual(launcher.Hud, launcher.Dir.Hud, "HUD not wired to the director");
            Assert.IsNotNull(launcher.Dir.Camera.Cam, "no flow camera");

            FlowInput.UseSynthetic = true;
            FlowInput.SyntheticHeld = true;
            FlowInput.SyntheticSwipe = Vector2.zero;

            bool sawLookAhead = false;
            for (int i = 0; i < 80; i++)
            {
                yield return new WaitForSeconds(0.04f);
                if (launcher.Dir != null && launcher.Dir.Camera.HasLookTarget) sawLookAhead = true;
            }

            Assert.Greater(launcher.Dir.Motor.Distance, 5f, "live run did not advance");
            Assert.Greater(launcher.Hud.LastDistance, 5f, "HUD not updated by the run");
            Assert.IsTrue(sawLookAhead, "camera never aimed ahead at a gate");

            float dist = launcher.Dir.Motor.Distance;
            int chain = launcher.Hud.LastChain;

            launcher.ExitRun();
            Assert.IsFalse(launcher.Active, "launcher did not exit");
            FlowInput.UseSynthetic = false;
            FlowInput.SyntheticHeld = false;

            Object.Destroy(go);
            yield return null;
            Debug.Log($"[swloop] fp dist={dist:F0} chain={chain} look={sawLookAhead}");
        }
    }
}
