using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class IsolatedShotsTests
    {
        const int ShotLayer = 29;
        float lastBrightest;

        [UnityTest]
        public IEnumerator Isolated_element_shots()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);

            var camGo = new GameObject("IsoCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.005f, 0.005f, 0.02f);
            cam.fieldOfView = 35f;
            cam.cullingMask = 1 << ShotLayer;

            yield return Shot(cam, Sw.Companion.transform, 2.2f, Path.Combine(dir, "iso_vesp.png"));
            float vespBright = lastBrightest;

            float wickBright = 0f;
            var wick = GameObject.Find("WickBody");
            if (wick != null)
            {
                yield return Shot(cam, wick.transform, 3.0f, Path.Combine(dir, "iso_wick.png"));
                wickBright = lastBrightest;
            }

            Object.Destroy(camGo);

            Debug.Log($"[swloop] iso vespBright={vespBright:F2} wickBright={wickBright:F2}");
            Assert.Greater(vespBright, 0.4f, "Vesp not visible in its isolated shot");
            if (wick != null) Assert.Greater(wickBright, 0.4f, "Wick not visible in its isolated shot");
        }

        static void SetLayer(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++)
                SetLayer(t.GetChild(i), layer);
        }

        IEnumerator Shot(Camera cam, Transform target, float dist, string path)
        {
            SetLayer(target, ShotLayer);
            cam.transform.position = target.position + new Vector3(0.35f, 0.25f, -1f).normalized * dist;
            cam.transform.LookAt(target.position);
            yield return null;

            var rt = new RenderTexture(480, 480, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(480, 480, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 480, 480), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            var px = tex.GetPixels();
            float b = 0f;
            for (int i = 0; i < px.Length; i++)
                b = Mathf.Max(b, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));
            lastBrightest = b;

            File.WriteAllBytes(path, tex.EncodeToPNG());
            SetLayer(target, 0);
            Object.Destroy(tex);
            rt.Release();
        }
    }
}
