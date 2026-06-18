using UnityEngine;
using UnityEngine.InputSystem;

namespace Starwick
{
    public struct FlowInputFrame
    {
        public bool Held;
        public bool Pressed;
        public bool Released;
        public Vector2 Swipe;
        public float HoldTime;
    }

    public static class FlowInput
    {
        public static bool UseSynthetic;
        public static bool SyntheticHeld;
        public static Vector2 SyntheticSwipe;

        static bool prevHeld;
        static float holdTime;

        public static FlowInputFrame Read(float dt)
        {
            bool held;
            Vector2 swipe;
            if (UseSynthetic)
            {
                held = SyntheticHeld;
                swipe = SyntheticSwipe;
            }
            else
            {
                var p = Pointer.current;
                held = p != null && p.press.isPressed;
                swipe = (held && p != null) ? p.delta.ReadValue() : Vector2.zero;
            }

            var f = new FlowInputFrame();
            f.Held = held;
            f.Pressed = held && !prevHeld;
            f.Released = !held && prevHeld;
            if (f.Pressed) holdTime = 0f;
            if (held) holdTime += dt;
            f.HoldTime = holdTime;
            f.Swipe = swipe;
            if (f.Released) holdTime = 0f;
            prevHeld = held;
            return f;
        }
    }
}
