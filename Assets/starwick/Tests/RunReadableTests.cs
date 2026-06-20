using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RunReadableTests
    {
        [UnityTest]
        public IEnumerator Run_frame_is_readable_and_not_overexposed()
        {
            for (int i = 0; i < 10; i++) yield return null;

            var sgo = new GameObject("ReadRun");
            var sess = sgo.AddComponent<RunSession>();
            sess.Enter(7793);
            var dir = sess.Dir;

            int g = 0;
            while (dir.Running && dir.Motor.Distance < 130f && g++ < 2000)
                dir.Step(new FlowInputFrame(), 0.05f);

            float aimGlow = dir.GateGlow(0);
            for (int i = 1; i < dir.GateCount; i++) aimGlow = Mathf.Max(aimGlow, dir.GateGlow(i));
            Assert.Greater(aimGlow, 0.4f, "gate glyphs are not bright enough to read");

            Assert.IsNotNull(dir.Hud, "run has no HUD");
            Assert.Greater(dir.Hud.ThreadFill, 0.05f, "diegetic thread meter did not fill with progress");
            Assert.Less(dir.Hud.ThreadFill, 0.99f, "thread meter full too early");

            yield return null;
            yield return null;

            var cam = dir.Camera.Cam;
            var rt = new RenderTexture(480, 270, 24);
            cam.targetTexture = rt;
            cam.Render();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(480, 270, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 480, 270), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            var px = tex.GetPixels();
            float maxc = 0f;
            int white = 0;
            for (int i = 0; i < px.Length; i++)
            {
                float m = Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b));
                maxc = Mathf.Max(maxc, m);
                if (px[i].r > 0.95f && px[i].g > 0.95f && px[i].b > 0.95f) white++;
            }
            float whiteFrac = white / (float)px.Length;

            var capDir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(capDir);
            File.WriteAllBytes(Path.Combine(capDir, "run_readable.png"), tex.EncodeToPNG());

            Assert.Greater(maxc, 0.3f, "run frame rendered too dark to read");
            Assert.Less(whiteFrac, 0.5f, "run frame is blown out / Wick overexposed");

            Debug.Log($"[swloop] runreadable aimGlow={aimGlow:F2} thread={dir.Hud.ThreadFill:F2} maxc={maxc:F2} whiteFrac={whiteFrac:F3}");

            rt.Release();
            Object.Destroy(tex);
            sess.Exit();
            Object.Destroy(sgo);
            yield return null;
        }
    }
}
