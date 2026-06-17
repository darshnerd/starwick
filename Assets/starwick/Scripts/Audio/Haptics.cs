using UnityEngine;

namespace Starwick
{
    public static class Haptics
    {
        public static int PulseCount { get; private set; }

        public static void Light() => Pulse();
        public static void Medium() => Pulse();
        public static void Heavy() => Pulse();

        static void Pulse()
        {
            PulseCount++;
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
