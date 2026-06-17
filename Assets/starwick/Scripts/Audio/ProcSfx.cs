using UnityEngine;

namespace Starwick
{
    public static class ProcSfx
    {
        public static AudioClip Tick(int sampleRate = 44100)
        {
            int n = (int)(0.07f * sampleRate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 42f);
                data[i] = Mathf.Sin(2f * Mathf.PI * 1400f * t) * env * 0.3f;
            }
            var clip = AudioClip.Create("SfxTick", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Shimmer(int sampleRate = 44100)
        {
            int n = (int)(0.22f * sampleRate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float k = t / 0.22f;
                float freq = Mathf.Lerp(620f, 1380f, k);
                float env = Mathf.Sin(k * Mathf.PI) * 0.28f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }
            var clip = AudioClip.Create("SfxShimmer", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Chime(int sampleRate = 44100)
        {
            int n = (int)(0.85f * sampleRate);
            var data = new float[n];
            float f = 523.25f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 3.2f) * Mathf.Clamp01(t * 200f);
                float s = Mathf.Sin(2f * Mathf.PI * f * t)
                        + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * f * 3.01f * t) * 0.25f;
                data[i] = s * env * 0.16f;
            }
            var clip = AudioClip.Create("SfxChime", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Confirm(int sampleRate = 44100)
        {
            int n = (int)(0.45f * sampleRate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 5.5f) * Mathf.Clamp01(t * 120f);
                float s = Mathf.Sin(2f * Mathf.PI * 196f * t) + Mathf.Sin(2f * Mathf.PI * 294f * t) * 0.6f;
                data[i] = s * env * 0.22f;
            }
            var clip = AudioClip.Create("SfxConfirm", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
