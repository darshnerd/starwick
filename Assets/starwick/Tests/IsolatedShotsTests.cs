using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class IsolatedShotsTests
    {
        [UnityTest]
        public IEnumerator Isolated_element_shots()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);

            var camGo = new GameObject("IsoCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.03f);
            cam.fieldOfView = 35f;

            yield return Shot(cam, Sw.Companion.transform, 3.2f, Path.Combine(dir, "iso_vesp.png"));

            var wick = GameObject.Find("WickBody");
            if (wick != null)
                yield return Shot(cam, wick.transform, 4.0f, Path.Combine(dir, "iso_wick.png"));

            Object.Destroy(camGo);
            Debug.Log("[swloop] isolated shots: vesp + wick captured");
        }

        IEnumerator Shot(Camera cam, Transform target, float dist, string path)
        {
            cam.transform.position = target.position + new Vector3(0.5f, 0.35f, -1f).normalized * dist;
            cam.transform.LookAt(target.position);

            var rt = new RenderTexture(480, 480, 24);
            cam.targetTexture = rt;
            yield return null;
            yield return null;
            RenderTexture.active = rt;
            var tex = new Texture2D(480, 480, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 480, 480), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
        }
    }
}
