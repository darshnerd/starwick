using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class NarrationUITests
    {
        [UnityTest]
        public IEnumerator Narration_renders_on_screen()
        {
            for (int i = 0; i < 60; i++) yield return null;

            Sw.Dialogue.Play(StoryData.FirstConstellation());
            Assert.IsTrue(Sw.Dialogue.Active, "Dialogue not active");

            yield return new WaitForSeconds(1.5f);

            var rt = new RenderTexture(1280, 720, 24);
            Sw.Cam.targetTexture = rt;
            yield return null;
            yield return null;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            Sw.Cam.targetTexture = null;
            RenderTexture.active = null;

            var px = tex.GetPixels();
            int w = tex.width;
            int h = tex.height;
            int whiteLower = 0;
            for (int y = 0; y < h / 3; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = px[y * w + x];
                    float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                    if (lum > 0.6f) whiteLower++;
                }

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "m4_narration.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m4b whiteLowerPixels={whiteLower} line='{(Sw.Dialogue.Current != null ? Sw.Dialogue.Current.Text : "")}'");
            Assert.Greater(whiteLower, 100, "No narration text rendered in lower region");
        }
    }
}
