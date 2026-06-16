using UnityEngine;

namespace Starwick
{
    public static class ProcAudio
    {
        public static AudioClip Drone(int seconds = 6, int sampleRate = 44100)
        {
            int n = seconds * sampleRate;
            var data = new float[n];
            float[] freqs = { 55f, 82.41f, 110f, 164.81f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                float s = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    s += Mathf.Sin(2f * Mathf.PI * freqs[f] * t) / freqs.Length;
                float lfo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                float fade = Mathf.Clamp01(Mathf.Min(t, seconds - t) * 2f);
                data[i] = s * lfo * fade * 0.5f;
            }
            var clip = AudioClip.Create("StarwickDrone", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip Motif(int seconds = 8, int sampleRate = 44100)
        {
            int n = seconds * sampleRate;
            var data = new float[n];
            float[] notes = { 523.25f, 659.25f, 783.99f, 659.25f, 587.33f, 783.99f, 1046.5f, 783.99f };
            float noteLen = (float)seconds / notes.Length;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp((int)(t / noteLen), 0, notes.Length - 1);
                float local = t - idx * noteLen;
                float env = Mathf.Exp(-local * 2.2f) * Mathf.Clamp01(local * 30f);
                float f = notes[idx];
                float s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.2f;
                data[i] = s * env * 0.2f;
            }
            var clip = AudioClip.Create("VespMotif", n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
