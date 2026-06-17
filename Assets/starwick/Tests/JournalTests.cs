using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class JournalTests
    {
        [UnityTest]
        public IEnumerator Journal_lists_fragments()
        {
            for (int i = 0; i < 60; i++) yield return null;

            GameState.AddFragment("A test memory - a small light remembered against the dark.");
            Assert.Greater(GameState.Fragments.Count, 0, "No fragment to show");

            Sw.Journal.SetOpen(true);
            Assert.IsTrue(Sw.Journal.IsOpen, "Journal did not open");
            yield return new WaitForSeconds(0.6f);

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
            int white = 0;
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                if (c.r > 0.7f && c.g > 0.7f && c.b > 0.7f) white++;
            }

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "m4_journal.png"), tex.EncodeToPNG());

            Object.Destroy(tex);
            rt.Release();

            Sw.Journal.SetOpen(false);

            Debug.Log($"[swloop] journal whitePixels={white} fragments={GameState.Fragments.Count}");
            Assert.Greater(white, 150, "Journal text not rendered");
        }
    }
}
