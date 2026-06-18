using UnityEngine;

namespace Starwick
{
    public static class Haptics
    {
        public static bool Enabled = true;
        public static int PulseCount { get; private set; }

        public static void Light() => Pulse(false);
        public static void Medium() => Pulse(true);
        public static void Heavy() => Pulse(true);

        static void Pulse(bool buzz)
        {
            PulseCount++;
            if (!buzz || !Enabled) return;
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
