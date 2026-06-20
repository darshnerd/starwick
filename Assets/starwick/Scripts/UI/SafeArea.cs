using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public static class SafeArea
    {
        public static RectTransform Apply(Canvas canvas)
        {
            if (canvas == null) return null;
            var root = canvas.transform;

            var kids = new List<Transform>();
            for (int i = 0; i < root.childCount; i++) kids.Add(root.GetChild(i));

            var go = new GameObject("SafeArea");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(root, false);

            float w = Screen.width;
            float h = Screen.height;
            Vector2 mn = Vector2.zero;
            Vector2 mx = Vector2.one;
            if (w > 0f && h > 0f)
            {
                var r = Screen.safeArea;
                mn = new Vector2(r.xMin / w, r.yMin / h);
                mx = new Vector2(r.xMax / w, r.yMax / h);
            }
            rt.anchorMin = mn;
            rt.anchorMax = mx;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            foreach (var k in kids) k.SetParent(rt, false);
            return rt;
        }
    }
}
