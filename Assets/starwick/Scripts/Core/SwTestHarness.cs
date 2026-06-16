using UnityEngine;

namespace Starwick
{
    public class SwTestHarness : MonoBehaviour
    {
        public bool Booted => Sw.Booted;
        public bool HasCamera => Sw.Cam != null;
        public bool AudioPlaying => Sw.Ambient != null && Sw.Ambient.isPlaying;
        public bool AudioStarted => Sw.AmbientStarted;
        public int Stars => Sw.Cosmos != null ? Sw.Cosmos.ActiveStars : 0;
        public bool HasCompanion => Sw.Companion != null;
        public bool MotifStarted => Sw.MotifStarted;
        public bool MotifPlaying => Sw.Motif != null && Sw.Motif.isPlaying;
        public int CompanionTaps => Sw.Companion != null ? Sw.Companion.TapCount : 0;
        public bool HasConstellation => Sw.Constellation != null;
        public bool ConstellationComplete => Sw.Constellation != null && Sw.Constellation.Complete;
        public int StarsRelit => GameState.StarsRelit;
        public float FrameMs { get; private set; }

        void Update()
        {
            FrameMs = Time.unscaledDeltaTime * 1000f;
        }
    }
}
