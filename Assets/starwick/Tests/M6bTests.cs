using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class M6bTests
    {
        [UnityTest]
        public IEnumerator Realm_layout_and_world_constellation()
        {
            for (int i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(Sw.Booted, "not booted");
            Assert.IsNotNull(Sw.Realm, "no realm");

            var sites = Sw.Realm.Sites;
            Assert.GreaterOrEqual(sites.Length, 3, "expected at least 3 star-sites");
            Assert.Greater(Vector3.Distance(sites[0], sites[1]), 8f, "sites overlap");

            Assert.IsFalse(Sw.Constellation.transform.IsChildOf(Sw.Cam.transform),
                "constellation still parented to the camera");
            var cp = Sw.Constellation.transform.position;
            float toSite = Vector2.Distance(new Vector2(cp.x, cp.z), new Vector2(sites[0].x, sites[0].z));
            Assert.Less(toSite, 2f, "constellation not placed at a site");

            yield return ShotRealm(Path.Combine(
                Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput"), "m6_realm.png"));

            Sw.Constellation.ResetForReplay();
            yield return null;
            int before = GameState.StarsRelit;
            Sw.Constellation.TraceAll();
            yield return null;
            Assert.IsTrue(Sw.Constellation.Complete, "world-placed constellation did not relight");
            Assert.Greater(GameState.StarsRelit, before, "StarsRelit not incremented");

            Debug.Log($"[swloop] m6b sites={sites.Length} toSite={toSite:F2} relit={GameState.StarsRelit}");
        }

        IEnumerator ShotRealm(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var con = Sw.Constellation.transform;

            var camGo = new GameObject("RealmShotCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.015f, 0.04f);
            cam.fieldOfView = 52f;
            cam.farClipPlane = 600f;
            cam.transform.position = con.position + new Vector3(7f, 1.5f, -17f);
            cam.transform.LookAt(con.position);

            Vector3 to = cam.transform.position - con.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f) con.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);

            var rt = new RenderTexture(960, 540, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(camGo);
            Object.Destroy(tex);
            rt.Release();
            yield return null;
        }
    }
}
