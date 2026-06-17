using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public static class ProcTex
    {
        static readonly Dictionary<int, Texture2D> dotCache = new Dictionary<int, Texture2D>();
        static readonly Dictionary<long, Texture2D> nebulaCache = new Dictionary<long, Texture2D>();

        public static Texture2D SoftDot(int size = 64)
        {
            if (dotCache.TryGetValue(size, out var cached) && cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float r = size * 0.5f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - r) / r;
                    float dy = (y + 0.5f - r) / r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * a;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            dotCache[size] = tex;
            return tex;
        }

        public static Texture2D Nebula(int size = 128, int seed = 4242)
        {
            long key = ((long)size << 32) ^ (uint)seed;
            if (nebulaCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float r = size * 0.5f;
            float off = (seed % 997) * 0.137f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float n = 0f, amp = 0.6f, freq = 2.5f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(off + nx * freq, off + ny * freq);
                        freq *= 2f;
                        amp *= 0.5f;
                    }
                    float dx = (x + 0.5f - r) / r;
                    float dy = (y + 0.5f - r) / r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float edge = Mathf.Clamp01(1f - d);
                    float a = Mathf.Clamp01(n - 0.22f) * edge;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            nebulaCache[key] = tex;
            return tex;
        }
    }
}
