using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M6cTests
    {
        float lastWarmth;

        [UnityTest]
        public IEnumerator Spatial_relight_evolution_and_cue()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Decor, "no realm decor manager");
            Assert.GreaterOrEqual(Sw.Decor.Sites.Length, 3, "expected at least 3 sites");

            var decor = Sw.Decor;
            decor.ResetSites();
            Sw.Constellation.ResetForReplay();
            for (int i = 0; i < 5; i++) yield return null;

            Assert.Less(decor.Sites[0].Warmth, 0.2f, "site not cool before relight");
            Assert.IsFalse(decor.Sites[1].IsNext, "next cue raised too early");

            yield return CaptureWarmth(decor.Sites[0].Center, "m6_site_dormant.png");
            float before = lastWarmth;

            Sw.Constellation.TraceAll();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(decor.Sites[0].Lit, "site 0 not lit after relight");
            Assert.Greater(decor.Sites[0].Warmth, 0.6f, "site 0 crystals did not warm");
            Assert.IsTrue(decor.Sites[1].IsNext, "next-site cue not raised after relight");

            yield return CaptureWarmth(decor.Sites[0].Center, "m6_site_lit.png");
            float after = lastWarmth;

            Debug.Log($"[swloop] m6c warmthBefore={before:F3} warmthAfter={after:F3} site0Lit={decor.Sites[0].Lit} nextCue={decor.Sites[1].IsNext}");
            Assert.Greater(after, before + 0.04f, "site did not visibly warm (local evolution)");
        }

        IEnumerator CaptureWarmth(Vector3 center, string fileName)
        {
            var camGo = new GameObject("WarmthShotCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.015f, 0.04f);
            cam.fieldOfView = 45f;
            cam.farClipPlane = 600f;
            cam.transform.position = center + new Vector3(3.5f, 2.5f, -8f);
            cam.transform.LookAt(center + Vector3.up * 2f);

            var rt = new RenderTexture(720, 405, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(720, 405, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 720, 405), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            var px = tex.GetPixels();
            float sumR = 0f, sumB = 0f;
            for (int i = 0; i < px.Length; i++) { sumR += px[i].r; sumB += px[i].b; }
            lastWarmth = (sumR - sumB) / px.Length;

            var d = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(d);
            File.WriteAllBytes(Path.Combine(d, fileName), tex.EncodeToPNG());

            Object.Destroy(camGo);
            Object.Destroy(tex);
            rt.Release();
            yield return null;
        }
    }
}
