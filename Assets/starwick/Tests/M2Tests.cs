using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M2Tests
    {
        [UnityTest]
        public IEnumerator Vesp_present_and_sings()
        {
            for (int i = 0; i < 90; i++) yield return null;

            Assert.IsNotNull(Sw.Companion, "Vesp companion not spawned");
            Assert.AreEqual("Vesp", Sw.Companion.DisplayName, "Companion is not Vesp");
            Assert.IsTrue(Sw.MotifStarted, "Vesp motif never started");
            Assert.IsNotNull(Sw.Motif, "No motif AudioSource");
            Assert.IsNotNull(Sw.Motif.clip, "Motif clip not synthesized");
            Assert.Greater(Sw.Motif.clip.samples, 0, "Motif clip empty");

            var rt = new RenderTexture(960, 600, 24);
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
            float brightest = 0f;
            for (int i = 0; i < px.Length; i++)
                brightest = Mathf.Max(brightest, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "m2_companion.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m2 companion={Sw.Companion.DisplayName} motifStarted={Sw.MotifStarted} brightest={brightest:F3}");
            Assert.Greater(brightest, 0.6f, "Scene not rendering bright elements");
        }
    }
}
