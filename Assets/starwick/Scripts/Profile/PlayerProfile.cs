using System.Collections.Generic;

namespace Starwick
{
    [System.Serializable]
    public class PlayerProfile
    {
        public int Version = 1;
        public int TotalRuns;
        public int TotalStarsRelit;
        public int Starlight;
        public int Embers;
        public string CurrentHollow = "Vesp";
        public int HearthRestored;
        public int[] Bonds = new int[4];
        public List<string> MemoryArchive = new List<string>();
    }
}
