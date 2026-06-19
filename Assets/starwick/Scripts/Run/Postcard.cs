using System.IO;
using UnityEngine;

namespace Starwick
{
    public static class Postcard
    {
        public static Texture2D Last;

        public static Texture2D Capture(Camera cam, int w, int h)
        {
            if (cam == null) return null;
            var rt = new RenderTexture(w, h, 24);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            rt.Release();
            Object.Destroy(rt);

            Last = tex;
            return tex;
        }

        public static void Save(Texture2D tex, string fileName)
        {
            if (tex == null) return;
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "CaptureOutput");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, fileName), tex.EncodeToPNG());
        }
    }
}
