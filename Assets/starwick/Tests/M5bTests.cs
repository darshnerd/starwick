using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M5bTests
    {
        int picked;

        [UnityTest]
        public IEnumerator Choice_buttons_block_world_and_select()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Narration, "no NarrationUI");

            picked = 0;
            Sw.Narration.ShowChoices("The star waits. What do you offer it?",
                "Let it rest", "Send it onward", i => picked = i);
            yield return null;

            Assert.IsTrue(Sw.Narration.ChoiceActive, "choice not active after ShowChoices");
            Assert.IsTrue(InputService.UiBlocking, "choices should block world input");

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            yield return Capture(Path.Combine(dir, "m5_choice.png"));

            InputService.UseSynthetic = true;
            InputService.SyntheticPointerDown = false;
            InputService.SyntheticPointer = new Vector2(Screen.width * 0.7f, Screen.height * 0.41f);
            yield return null;
            yield return null;
            InputService.SyntheticPointerDown = true;
            yield return null;
            yield return null;
            InputService.SyntheticPointerDown = false;

            int rightPick = picked;

            picked = 0;
            Sw.Narration.ShowChoices("Again?", "Let it rest", "Send it onward", i => picked = i);
            yield return null;
            InputService.SyntheticPointer = new Vector2(Screen.width * 0.5f, Screen.height * 0.41f);
            InputService.SyntheticPointerDown = true;
            yield return null;
            yield return null;
            InputService.SyntheticPointerDown = false;
            int gapPick = picked;
            if (Sw.Narration.ChoiceActive) Sw.Narration.HideChoices();

            InputService.UseSynthetic = false;

            Assert.AreEqual(2, rightPick, "tap inside the right button should select choice 2");
            Assert.AreEqual(0, gapPick, "tap between the buttons should select nothing");

            Debug.Log($"[swloop] m5b rightPick={rightPick} gapPick={gapPick}");
        }

        IEnumerator Capture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rt = new RenderTexture(640, 360, 24);
            Sw.Cam.targetTexture = rt;
            yield return null;
            yield return null;
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 360, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 640, 360), 0, 0);
            tex.Apply();
            Sw.Cam.targetTexture = null;
            RenderTexture.active = null;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
        }
    }
}
