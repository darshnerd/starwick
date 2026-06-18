namespace Starwick
{
    public static class HearthState
    {
        public enum Node { Starwell, HollowGrove, LoomGate, MemoryArchive, ConstellariumChamber, RainikyoCore }

        static readonly string[] Names = { "Starwell", "Hollow Grove", "Loom Gate", "Memory Archive", "Constellarium Chamber", "Rainikyo Core" };
        static readonly int[] Costs = { 100, 250, 400, 300, 500, 1000 };

        public static int NodeCount => Names.Length;
        public static string NameOf(Node n) => Names[(int)n];
        public static int CostOf(Node n) => Costs[(int)n];

        public static bool IsRestored(Node n) =>
            PlayerProfileStore.Current != null && (PlayerProfileStore.Current.HearthRestored & (1 << (int)n)) != 0;

        public static bool CanRestore(Node n) =>
            PlayerProfileStore.Current != null && !IsRestored(n) && PlayerProfileStore.Current.Starlight >= CostOf(n);

        public static bool Restore(Node n)
        {
            if (!CanRestore(n)) return false;
            PlayerProfileStore.Current.Starlight -= CostOf(n);
            PlayerProfileStore.Current.HearthRestored |= 1 << (int)n;
            PlayerProfileStore.Save();
            return true;
        }

        public static int RestoredCount()
        {
            if (PlayerProfileStore.Current == null) return 0;
            int c = 0;
            int m = PlayerProfileStore.Current.HearthRestored;
            while (m != 0) { c += m & 1; m >>= 1; }
            return c;
        }

        public static bool RealmBiomesUnlocked => IsRestored(Node.LoomGate);
        public static bool SecondHollowSlot => IsRestored(Node.HollowGrove);
    }
}
