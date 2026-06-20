using UnityEngine;

namespace Starwick
{
    public static class Sw
    {
        public static GameObject Root;
        public static Camera Cam;
        public static GroundRealm Realm;
        public static RealmDecor Decor;
        public static CosmosFx Cosmos;
        public static AudioSource Ambient;
        public static Companion Companion;
        public static AudioSource Motif;
        public static Constellation Constellation;
        public static DialogueSystem Dialogue;
        public static NarrationUI Narration;
        public static JournalUI Journal;
        public static ConstellariumUI Constellarium;
        public static TitleUI Title;
        public static PostFx PostFx;
        public static Director Director;
        public static AudioManager Audio;
        public static SfxManager Sfx;
        public static RunSession RunSession;
        public static bool Booted;
        public static bool AmbientStarted;
        public static bool MotifStarted;
    }
}
