using UnityEngine;

namespace Starwick
{
    public static class ProcSfx
    {
        static readonly int[] Pentatonic = { 0, 2, 4, 7, 9 };

        static float DegreeFreq(int degree)
        {
            int oct = degree / Pentatonic.Length;
            int idx = ((degree % Pentatonic.Length) + Pentatonic.Length) % Pentatonic.Length;
            int semis = Pentatonic[idx] + 12 * oct;
            return 392f * Mathf.Pow(2f, semis / 12f);
        }

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

        public static AudioClip Note(int degree, int sampleRate = 44100)
        {
            int n = (int)(0.5f * sampleRate);
            var data = new float[n];
            float f = DegreeFreq(degree);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 5f) * Mathf.Clamp01(t * 160f);
                float s = Mathf.Sin(2f * Mathf.PI * f * t)
                        + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.4f
                        + Mathf.Sin(2f * Mathf.PI * f * 3.01f * t) * 0.16f;
                data[i] = s * env * 0.2f;
            }
            var clip = AudioClip.Create("SfxNote", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Chord(int tier, int sampleRate = 44100)
        {
            int n = (int)(0.95f * sampleRate);
            var data = new float[n];
            float root = 261.63f * Mathf.Pow(2f, Mathf.Clamp(tier - 1, 0, 3) / 12f);
            float[] voices = { root, root * 1.25f, root * 1.5f, root * 2f };
            int active = Mathf.Clamp(2 + tier, 2, 4);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Clamp01(t * 6f) * Mathf.Exp(-t * 2.4f);
                float s = 0f;
                for (int v = 0; v < active; v++) s += Mathf.Sin(2f * Mathf.PI * voices[v] * t);
                data[i] = (s / active) * env * 0.26f;
            }
            var clip = AudioClip.Create("SfxChord", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Detune(int sampleRate = 44100)
        {
            int n = (int)(0.45f * sampleRate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float k = t / 0.45f;
                float fall = Mathf.Lerp(1f, 0.78f, k);
                float a = 196f * fall;
                float b = 188f * fall;
                float env = Mathf.Exp(-t * 4.5f) * Mathf.Clamp01(t * 100f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * a * t) + Mathf.Sin(2f * Mathf.PI * b * t)) * env * 0.18f;
            }
            var clip = AudioClip.Create("SfxDetune", n, 1, sampleRate, false);
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
