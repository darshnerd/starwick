using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M0Tests
    {
        [UnityTest]
        public IEnumerator Boots_with_cosmos_audio_and_camera()
        {
            for (int i = 0; i < 60; i++) yield return null;

            Assert.IsTrue(Sw.Booted, "Starwick did not boot");
            Assert.IsNotNull(Sw.Cam, "No camera");
            Assert.IsTrue(Sw.AmbientStarted, "Ambient audio never started");
            Assert.IsNotNull(Sw.Ambient, "No ambient AudioSource");
            Assert.IsNotNull(Sw.Ambient.clip, "Ambient clip not synthesized");
            Assert.Greater(Sw.Ambient.clip.samples, 0, "Ambient clip empty");
            Assert.GreaterOrEqual(Sw.Cosmos != null ? Sw.Cosmos.ActiveStars : 0, 300, "Too few stars");

            if (!Sw.Ambient.isPlaying)
                Debug.Log("[swloop] note: AudioSource.isPlaying false (expected in batch with no audio device)");

            var rt = new RenderTexture(720, 480, 24);
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
            double lum = 0;
            for (int i = 0; i < px.Length; i++)
                lum += 0.2126 * px[i].r + 0.7152 * px[i].g + 0.0722 * px[i].b;
            lum /= Mathf.Max(1, px.Length);

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "m0_cosmos.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] stars={Sw.Cosmos.ActiveStars} luminance={lum:F5}");
            Assert.Greater(lum, 0.0015, "Screen is effectively black (cosmos not rendering)");
        }
    }
}
