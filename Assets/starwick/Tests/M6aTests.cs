using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M6aTests
    {
        [UnityTest]
        public IEnumerator Walkable_ground()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Realm, "no GroundRealm");

            float a1 = GroundRealm.HeightFor(1337, 12f, -8f);
            float a2 = GroundRealm.HeightFor(1337, 12f, -8f);
            Assert.AreEqual(a1, a2, 0.0001f, "height not deterministic for a seed");

            float sa = 0f, sb = 0f;
            for (int k = 0; k < 6; k++)
            {
                sa += GroundRealm.HeightFor(1337, k * 7.3f, k * 4.1f);
                sb += GroundRealm.HeightFor(99, k * 7.3f, k * 4.1f);
            }
            Assert.AreNotEqual(sa, sb, "different seeds produced identical terrain");

            var p = Sw.Cam.transform.position;
            float expectedP = Sw.Realm.Height(p.x, p.z) + 1.7f;
            Assert.Less(Mathf.Abs(p.y - expectedP), 0.5f, "player not grounded at spawn");

            InputService.UseSynthetic = true;
            InputService.SyntheticMove = new Vector2(0f, 1f);
            yield return new WaitForSeconds(1.5f);
            InputService.SyntheticMove = Vector2.zero;
            InputService.UseSynthetic = false;

            var q = Sw.Cam.transform.position;
            float movedXZ = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(q.x, q.z));
            float expectedQ = Sw.Realm.Height(q.x, q.z) + 1.7f;
            Assert.Greater(movedXZ, 1f, "did not walk forward across the ground");
            Assert.Less(Mathf.Abs(q.y - expectedQ), 0.7f, "player left the ground while walking");

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

            var px = tex.GetPixels();
            float brightest = 0f;
            for (int i = 0; i < px.Length; i++)
                brightest = Mathf.Max(brightest, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));

            var d = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(d);
            File.WriteAllBytes(Path.Combine(d, "m6_ground.png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();

            Debug.Log($"[swloop] m6a movedXZ={movedXZ:F2} y={q.y:F2} expected={expectedQ:F2} brightest={brightest:F3}");
            Assert.Greater(brightest, 0.3f, "ground scene not rendering");
        }
    }
}
