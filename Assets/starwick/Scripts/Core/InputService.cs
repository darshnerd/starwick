using UnityEngine;
using UnityEngine.InputSystem;

namespace Starwick
{
    public static class InputService
    {
        public static bool UseSynthetic;
        public static bool SyntheticPointerDown;
        public static Vector2 SyntheticPointer;
        public static Vector2 SyntheticMove;
        public static Vector2 SyntheticLook;

        public static bool PointerDown
        {
            get
            {
                if (UseSynthetic) return SyntheticPointerDown;
                var p = Pointer.current;
                return p != null && p.press.isPressed;
            }
        }

        public static Vector2 PointerPosition
        {
            get
            {
                if (UseSynthetic) return SyntheticPointer;
                var p = Pointer.current;
                return p != null ? p.position.ReadValue() : Vector2.zero;
            }
        }

        public static Vector2 MoveAxis
        {
            get
            {
                if (UseSynthetic) return SyntheticMove;
                var k = Keyboard.current;
                if (k == null) return Vector2.zero;
                float x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
                float y = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
                return new Vector2(x, y);
            }
        }

        public static Vector2 LookDelta
        {
            get
            {
                if (UseSynthetic) return SyntheticLook;
                var m = Mouse.current;
                if (m != null && m.rightButton.isPressed) return m.delta.ReadValue();
                return Vector2.zero;
            }
        }

        public static bool UiBlocking =>
            (Sw.Dialogue != null && Sw.Dialogue.Active) ||
            (Sw.Journal != null && Sw.Journal.IsOpen) ||
            (Sw.Constellarium != null && Sw.Constellarium.IsOpen) ||
            (Sw.Narration != null && Sw.Narration.ChoiceActive);
    }
}
