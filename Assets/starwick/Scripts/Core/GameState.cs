using System.Collections.Generic;

namespace Starwick
{
    public static class GameState
    {
        public static int StarsRelit;
        public static int ConstellationsComplete;
        public static int Choice;
        public static int CompanionIndex;
        public static readonly List<string> Fragments = new List<string>();

        public static void AddFragment(string fragment)
        {
            if (!string.IsNullOrEmpty(fragment) && !Fragments.Contains(fragment))
                Fragments.Add(fragment);
        }

        public static void Reset()
        {
            StarsRelit = 0;
            ConstellationsComplete = 0;
            Choice = 0;
            Fragments.Clear();
        }
    }
}
