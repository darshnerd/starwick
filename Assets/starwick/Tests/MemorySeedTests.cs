using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class MemorySeedTests
    {
        [Test]
        public void Seed_codes_round_trip()
        {
            int[] seeds = { 1337, 7793, 4242, 9001, 0, 123456, 20240618 };
            for (int idx = 0; idx < Roster.All.Length; idx++)
                foreach (int seed in seeds)
                {
                    string code = MemorySeed.Encode(idx, seed);
                    StringAssert.Contains("-", code);
                    Assert.IsTrue(MemorySeed.TryDecode(code, out int i2, out int s2), $"decode failed for {code}");
                    Assert.AreEqual(idx, i2, $"companion mismatch for {code}");
                    Assert.AreEqual(seed, s2, $"seed mismatch for {code}");
                }

            Assert.IsFalse(MemorySeed.TryDecode("garbage", out _, out _), "garbage decoded");
            Assert.IsFalse(MemorySeed.TryDecode("ZZTAG-1-X", out _, out _), "unknown companion decoded");
        }

        [UnityTest]
        public IEnumerator Code_starts_the_same_run_and_results_make_a_postcard()
        {
            for (int i = 0; i < 10; i++) yield return null;

            string code = MemorySeed.Encode(1, 7793);
            var go = new GameObject("SeedLauncher");
            var launcher = go.AddComponent<RunLauncher>();

            Assert.IsTrue(launcher.EnterRunFromCode(code), "code did not start a run");
            Assert.AreEqual(7793, launcher.Dir.Track.Seed, "decoded seed did not reach the track");
            Assert.AreEqual(1, GameState.CompanionIndex, "companion not selected from code");

            int guard = 0;
            while (launcher.Dir.Running && guard++ < 3000) launcher.Dir.Step(new FlowInputFrame(), 0.05f);

            Assert.IsNotNull(launcher.Dir.PostcardImage, "no postcard captured at results");
            var px = launcher.Dir.PostcardImage.GetPixels();
            float maxc = 0f;
            for (int i = 0; i < px.Length; i += 37)
                maxc = Mathf.Max(maxc, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));
            Assert.Greater(maxc, 0.2f, "postcard is black");

            Debug.Log($"[swloop] seed code={code} trackSeed={launcher.Dir.Track.Seed} postcardMax={maxc:F2}");

            launcher.ExitRun();
            GameState.CompanionIndex = 0;
            Object.Destroy(go);
            yield return null;
        }
    }
}
