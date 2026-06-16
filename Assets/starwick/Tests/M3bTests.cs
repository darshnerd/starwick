using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M3bTests
    {
        [UnityTest]
        public IEnumerator Movement_and_vesp_tap()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var start = Sw.Cam.transform.position;
            InputService.UseSynthetic = true;
            InputService.SyntheticMove = new Vector2(0f, 1f);
            yield return new WaitForSeconds(1.5f);
            InputService.SyntheticMove = Vector2.zero;

            float moved = Vector3.Distance(Sw.Cam.transform.position, start);

            int taps = Sw.Companion.TapCount;
            Sw.Companion.React();

            InputService.UseSynthetic = false;

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

            var d = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(d);
            File.WriteAllBytes(Path.Combine(d, "m3b_move.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m3b moved={moved:F2} taps={Sw.Companion.TapCount} brightest={brightest:F3}");

            Assert.Greater(moved, 1f, "Camera did not move with forward input");
            Assert.Greater(Sw.Companion.TapCount, taps, "Vesp did not react to tap");
            Assert.Greater(brightest, 0.5f, "Scene not rendering after movement");
        }
    }
}
