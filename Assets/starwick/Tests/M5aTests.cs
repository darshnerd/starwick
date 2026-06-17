using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M5aTests
    {
        [UnityTest]
        public IEnumerator World_evolves_and_endings_branch()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var rest = StoryData.Ending(1);
            var send = StoryData.Ending(2);
            Assert.Greater(rest.Count, 0, "Rest ending empty");
            Assert.Greater(send.Count, 0, "Send ending empty");
            StringAssert.Contains("slow", rest[0].Text);
            StringAssert.Contains("leaps", send[0].Text);

            Sw.PostFx.SetMoodWarm(1f);
            yield return new WaitForSeconds(2.5f);
            Assert.Greater(Sw.PostFx.Warm, 0.8f, "World did not warm");
            yield return Capture("m5_warm.png");

            int frags = GameState.Fragments.Count;
            Sw.Dialogue.Play(StoryData.EndingSend());
            int guard = 0;
            while (Sw.Dialogue.Active && guard++ < 40)
            {
                Sw.Dialogue.Advance();
                yield return null;
            }
            Assert.Greater(GameState.Fragments.Count, frags, "Ending recorded no memory");

            Sw.PostFx.SetMoodWarm(0.1f);
            yield return new WaitForSeconds(2.5f);
            Assert.Less(Sw.PostFx.Warm, 0.3f, "World did not cool");
            yield return Capture("m5_cool.png");

            Sw.Constellation.ResetForReplay();
            Assert.IsFalse(Sw.Constellation.Complete, "Constellation not reset for replay");
            Assert.AreEqual(0, Sw.Constellation.TracedCount, "Traced count not reset");

            Debug.Log($"[swloop] m5 warm/cool ok, ending recorded, constellation reset complete={Sw.Constellation.Complete}");
        }

        IEnumerator Capture(string name)
        {
            var rt = new RenderTexture(640, 400, 24);
            Sw.Cam.targetTexture = rt;
            yield return null;
            yield return null;
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 400, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 640, 400), 0, 0);
            tex.Apply();
            Sw.Cam.targetTexture = null;
            RenderTexture.active = null;
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
        }
    }
}
