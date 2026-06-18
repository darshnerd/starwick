namespace Starwick
{
    public static class HollowBond
    {
        public const int PointsPerLevel = 100;

        public static void Add(int companionIndex, int amount)
        {
            var p = PlayerProfileStore.Current;
            if (p == null || p.Bonds == null) return;
            if (companionIndex < 0 || companionIndex >= p.Bonds.Length) return;
            p.Bonds[companionIndex] += amount;
            PlayerProfileStore.Save();
        }

        public static int Level(int companionIndex)
        {
            var p = PlayerProfileStore.Current;
            if (p == null || p.Bonds == null) return 0;
            if (companionIndex < 0 || companionIndex >= p.Bonds.Length) return 0;
            return p.Bonds[companionIndex] / PointsPerLevel;
        }
    }
}
