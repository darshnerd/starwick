using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M9bTests
    {
        [UnityTest]
        public IEnumerator Constellarium_shows_persistent_record()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Constellarium, "no Constellarium");

            if (SaveData.TotalStarsRelit < 6)
            {
                SaveData.TotalStarsRelit = 14;
                SaveData.Save();
            }

            Sw.Constellarium.SetOpen(true);
            yield return null;

            Assert.IsTrue(Sw.Constellarium.IsOpen, "constellarium did not open");
            Assert.IsTrue(InputService.UiBlocking, "open constellarium should block world input");
            Assert.AreEqual(Mathf.Min(72, SaveData.TotalStarsRelit), Sw.Constellarium.LitStars,
                "lit star count does not match all-time relit");

            var rt = new RenderTexture(960, 540, 24);
            Sw.Cam.targetTexture = rt;
            yield return null;
            yield return null;
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            tex.Apply();
            Sw.Cam.targetTexture = null;
            RenderTexture.active = null;
            var d = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(d);
            File.WriteAllBytes(Path.Combine(d, "m9_constellarium.png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();

            Sw.Constellarium.SetOpen(false);
            Assert.IsFalse(Sw.Constellarium.IsOpen, "constellarium did not close");

            Debug.Log($"[swloop] m9b lit={Sw.Constellarium.LitStars} total={SaveData.TotalStarsRelit}");
        }
    }
}
