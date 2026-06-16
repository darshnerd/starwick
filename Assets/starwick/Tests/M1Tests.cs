using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M1Tests
    {
        [UnityTest]
        public IEnumerator Cosmos_has_nebula_and_bright_stars()
        {
            for (int i = 0; i < 60; i++) yield return null;

            Assert.GreaterOrEqual(Sw.Cosmos != null ? Sw.Cosmos.ActiveStars : 0, 300, "Too few stars");
            Assert.GreaterOrEqual(Sw.Cosmos != null ? Sw.Cosmos.ActiveNebula : 0, 8, "No parallax nebula layer");

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
            double lum = 0;
            float brightest = 0f;
            for (int i = 0; i < px.Length; i++)
            {
                lum += 0.2126 * px[i].r + 0.7152 * px[i].g + 0.0722 * px[i].b;
                brightest = Mathf.Max(brightest, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));
            }
            lum /= Mathf.Max(1, px.Length);

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "m1_cosmos.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m1 stars={Sw.Cosmos.ActiveStars} nebula={Sw.Cosmos.ActiveNebula} lum={lum:F5} brightest={brightest:F3}");
            Assert.Greater(brightest, 0.6f, "No bright star pixels (stars not glowing)");
        }
    }
}
