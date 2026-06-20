using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class HardeningCTests
    {
        [Test]
        public void Track_frame_banks_on_turns()
        {
            var track = new RunTrack(7793);
            float maxBank = 0f, bankedAt = 0f;
            for (float d = 4f; d < 300f; d += 1f)
            {
                float b = Mathf.Abs(RunTrack.BankFor(7793, d));
                if (b > maxBank) { maxBank = b; bankedAt = d; }
            }
            Assert.Greater(maxBank, 0.3f, "track has no banking at all");

            var fr = track.Sample(bankedAt);
            Vector3 tan = track.Tangent(bankedAt);
            Vector3 flatRight = Vector3.Cross(Vector3.up, tan).normalized;
            float rightDiff = (fr.Right - flatRight).magnitude;
            float upTilt = (fr.Up - Vector3.up).magnitude;

            Assert.Greater(rightDiff, 0.001f, "frame right is not banked relative to a flat cross");
            Assert.Greater(upTilt, 0.001f, "frame up is not tilted by the bank");

            Debug.Log($"[swloop] bankframe maxBank={maxBank:F2} rightDiff={rightDiff:F3} upTilt={upTilt:F3}");
        }

        [Test]
        public void Bad_seed_codes_are_rejected()
        {
            string code = MemorySeed.Encode(1, 7793);
            Assert.IsTrue(MemorySeed.TryDecode(code, out int i, out int s), "valid code rejected");
            Assert.AreEqual(1, i);
            Assert.AreEqual(7793, s);

            char last = code[code.Length - 1];
            char other = last == 'A' ? 'B' : 'A';
            string corrupt = code.Substring(0, code.Length - 1) + other;
            Assert.IsFalse(MemorySeed.TryDecode(corrupt, out _, out _), "corrupted checksum was accepted");

            Assert.IsFalse(MemorySeed.TryDecode("VESP-ZZZZZZZZ-NOVA-AA", out _, out _), "overflowing seed was accepted");
            Debug.Log($"[swloop] seedguard code={code}");
        }

        [UnityTest]
        public IEnumerator Camera_follows_behind_along_the_track()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("CamRun");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7793);

            int guard = 0;
            while (dir.Running && dir.Motor.Distance < 120f && guard++ < 3000)
                dir.Step(new FlowInputFrame(), 0.05f);

            Vector3 motorPos = dir.Motor.Position;
            Vector3 camPos = dir.Camera.transform.position;
            Vector3 tan = dir.Track.Track.Tangent(dir.Motor.Distance);

            Vector3 camToWick = motorPos - camPos; camToWick.y = 0f;
            Vector3 flatTan = new Vector3(tan.x, 0f, tan.z);
            float dot = Vector3.Dot(camToWick.normalized, flatTan.normalized);
            Assert.Greater(dot, 0.5f, "camera is not behind the wick along the track tangent");

            Debug.Log($"[swloop] camfollow dot={dot:F2} dist={dir.Motor.Distance:F0}");
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Holding_still_does_not_open_the_gate()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var cgo = new GameObject("ColdStill");
            var cold = cgo.AddComponent<ColdOpen>();
            cold.AutoLaunch = false;
            yield return null;

            InputService.UseSynthetic = true;
            InputService.SyntheticPointerDown = true;
            InputService.SyntheticPointer = new Vector2(500f, 500f);

            for (int i = 0; i < 25; i++) yield return null;

            Assert.Less(cold.ThreadLength, 0.5f, "thread filled from holding still - pull is not travel-driven");

            Debug.Log($"[swloop] stillpull thread={cold.ThreadLength:F2} state={cold.State}");

            InputService.UseSynthetic = false;
            InputService.SyntheticPointerDown = false;
            Object.Destroy(cgo);
            yield return null;
        }
    }
}
