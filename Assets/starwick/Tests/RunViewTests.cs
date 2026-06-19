using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunViewTests
    {
        [UnityTest]
        public IEnumerator Run_is_visible()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var go = new GameObject("RunDir");
            var dir = go.AddComponent<RunDirector>();
            dir.Begin(7);
            for (int i = 0; i < 160; i++) dir.Step(new FlowInputFrame { Held = true }, 0.05f);
            Assert.IsTrue(dir.Running, "run ended before the shot");

            var cam = dir.Camera.Cam;
            var rt = new RenderTexture(960, 540, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            var px = tex.GetPixels();
            float brightest = 0f;
            for (int i = 0; i < px.Length; i++)
                brightest = Mathf.Max(brightest, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));

            var d = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(d);
            File.WriteAllBytes(Path.Combine(d, "run_view.png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
            Object.Destroy(go);

            Assert.Greater(brightest, 0.3f, "run scene did not render anything bright");
            Debug.Log($"[swloop] runview dist={dir.Motor.Distance:F0} brightest={brightest:F2}");
        }
    }
}
