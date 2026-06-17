using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Starwick.Tests
{
    public class LoopV2Tests
    {
        [UnityTest]
        public IEnumerator Relight_sequence_capture()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput", "seq_relight");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);

            var fx = Sw.Constellation.GetComponentInChildren<StarRelightFx>(true);
            Assert.IsNotNull(fx, "No StarRelightFx under the constellation");
            fx.Play();

            var rt = new RenderTexture(640, 400, 24);
            for (int f = 0; f < 8; f++)
            {
                yield return new WaitForSeconds(0.3f);
                Sw.Cam.targetTexture = rt;
                yield return null;
                RenderTexture.active = rt;
                var tex = new Texture2D(640, 400, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 640, 400), 0, 0);
                tex.Apply();
                Sw.Cam.targetTexture = null;
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(dir, $"relight_{f:00}.png"), tex.EncodeToPNG());
                Object.Destroy(tex);
            }
            rt.Release();

            Debug.Log("[swloop] relight sequence: 8 frames captured");
        }

        [UnityTest]
        public IEnumerator Motion_vesp_follows()
        {
            for (int i = 0; i < 30; i++) yield return null;

            var camStart = Sw.Cam.transform.position;
            var vespStart = Sw.Companion.transform.position;
            InputService.UseSynthetic = true;
            InputService.SyntheticMove = new Vector2(0f, 1f);
            yield return new WaitForSeconds(1.2f);
            InputService.SyntheticMove = Vector2.zero;
            float camMoved = Vector3.Distance(Sw.Cam.transform.position, camStart);
            float vespMoved = Vector3.Distance(Sw.Companion.transform.position, vespStart);
            InputService.UseSynthetic = false;

            Debug.Log($"[swloop] motion camMoved={camMoved:F2} vespMoved={vespMoved:F2}");
            Assert.Greater(camMoved, 1f, "Camera did not move");
            Assert.Greater(vespMoved, 0.5f, "Vesp did not spring-follow the camera");
        }

        [UnityTest]
        public IEnumerator Audio_clips_have_signal()
        {
            for (int i = 0; i < 30; i++) yield return null;

            float drone = Rms(Sw.Ambient.clip);
            float motif = Rms(Sw.Motif.clip);
            Debug.Log($"[swloop] audio rms drone={drone:F4} motif={motif:F4}");
            Assert.Greater(drone, 0.01f, "Ambient drone is effectively silent");
            Assert.Greater(motif, 0.01f, "Vesp motif is effectively silent");
        }

        [UnityTest]
        public IEnumerator Perf_frame_ms()
        {
            for (int i = 0; i < 20; i++) yield return null;

            float sum = 0f;
            float max = 0f;
            const int n = 120;
            for (int i = 0; i < n; i++)
            {
                yield return null;
                float ms = Time.unscaledDeltaTime * 1000f;
                sum += ms;
                if (ms > max) max = ms;
            }
            float avg = sum / n;
            Debug.Log($"[swloop] perf avgMs={avg:F2} maxMs={max:F2}");
            Assert.Less(avg, 200f, "Average frame time pathologically high");
        }

        static float Rms(AudioClip clip)
        {
            if (clip == null) return 0f;
            var data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);
            double s = 0;
            for (int i = 0; i < data.Length; i++) s += data[i] * data[i];
            return Mathf.Sqrt((float)(s / Mathf.Max(1, data.Length)));
        }
    }
}
