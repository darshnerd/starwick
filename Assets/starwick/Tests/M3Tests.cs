using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M3Tests
    {
        [UnityTest]
        public IEnumerator Constellation_traces_and_relights()
        {
            for (int i = 0; i < 60; i++) yield return null;

            Assert.IsNotNull(Sw.Constellation, "No constellation spawned");
            Assert.Greater(Sw.Constellation.NodeCount, 0, "Constellation has no nodes");
            Assert.IsFalse(Sw.Constellation.Complete, "Constellation already complete before tracing");
            int before = GameState.StarsRelit;

            Sw.Constellation.TraceAll();
            yield return null;
            yield return null;

            Assert.IsTrue(Sw.Constellation.Complete, "Constellation did not complete after tracing");
            Assert.AreEqual(Sw.Constellation.NodeCount, Sw.Constellation.TracedCount, "Not all nodes traced");
            Assert.Greater(GameState.StarsRelit, before, "StarsRelit did not increment");

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
            File.WriteAllBytes(Path.Combine(dir, "m3_constellation.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m3 nodes={Sw.Constellation.NodeCount} traced={Sw.Constellation.TracedCount} relit={GameState.StarsRelit} brightest={brightest:F3}");
            Assert.Greater(brightest, 0.6f, "Relit constellation not rendering bright");
        }
    }
}
