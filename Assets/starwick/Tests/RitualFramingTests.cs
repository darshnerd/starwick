using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class RitualFramingTests
    {
        [UnityTest]
        public IEnumerator Ritual_frames_the_hearth_and_hud_yields()
        {
            for (int i = 0; i < 10; i++) yield return null;

            PlayerProfileStore.LoadFromJson("");
            PlayerProfileStore.Current.Starlight = 2000;
            PlayerProfileStore.Current.HearthRestored = 0;
            PlayerProfileStore.Save();

            var sgo = new GameObject("RitSess");
            var sess = sgo.AddComponent<RunSession>();
            sess.Enter(7793);

            int g = 0;
            while (sess.Dir.Running && g++ < 3000) sess.Dir.Step(new FlowInputFrame(), 0.05f);

            Assert.IsTrue(sess.Hud.Faded, "HUD did not yield to the ritual");
            Assert.IsNotNull(sess.Dir.Ritual, "no ritual after finish");

            var rit = sess.Dir.Ritual;
            rit.Tick(2f);
            rit.Tick(2f);

            Assert.IsNotNull(rit.RitualCam, "ritual built no camera to frame the hearth");
            Assert.IsNotNull(rit.Hearth, "ritual built no hearth");

            Vector3 toHearth = (rit.HearthCenter - rit.RitualCam.transform.position).normalized;
            float dot = Vector3.Dot(rit.RitualCam.transform.forward, toHearth);
            Assert.Greater(dot, 0.8f, "ritual camera is not framing the hearth");

            Transform n0 = rit.Hearth.Nodes != null && rit.Hearth.Nodes.Length > 0 ? rit.Hearth.Nodes[0] : null;
            int hearthLayer = n0 != null ? n0.gameObject.layer : -1;
            Assert.AreEqual(ResultRitual.RitualLayer, hearthLayer, "hearth is not on the ritual layer");

            var camGo = new GameObject("RitProbeCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.005f, 0.005f, 0.02f);
            cam.fieldOfView = 42f;
            cam.cullingMask = 1 << ResultRitual.RitualLayer;
            cam.transform.position = rit.HearthCenter + new Vector3(0.3f, 5f, -15f);
            cam.transform.LookAt(rit.HearthCenter);
            yield return null;

            var rt = new RenderTexture(480, 360, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(480, 360, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 480, 360), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            float maxc = 0f;
            var px = tex.GetPixels();
            for (int i = 0; i < px.Length; i++) maxc = Mathf.Max(maxc, Mathf.Max(px[i].r, Mathf.Max(px[i].g, px[i].b)));

            var capDir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(capDir);
            File.WriteAllBytes(Path.Combine(capDir, "ritual_hearth.png"), tex.EncodeToPNG());

            Debug.Log($"[swloop] ritualframe faded={sess.Hud.Faded} camDot={dot:F2} layer={hearthLayer} max={maxc:F2}");
            Assert.Greater(maxc, 0.2f, "ritual hearth rendered black");

            Object.Destroy(camGo);
            rt.Release();
            Object.Destroy(tex);
            sess.Exit();
            Object.Destroy(sgo);
            yield return null;
        }
    }
}
