using UnityEngine;

namespace Starwick
{
    public struct CompanionData
    {
        public string Name;
        public int Seed;
        public Color Glow;
        public Color Ground;
        public Color Ambient;
        public float Pitch;
        public int UnlockRuns;
    }

    public static class Roster
    {
        public static readonly CompanionData[] All =
        {
            new CompanionData
            {
                Name = "Vesp", Seed = 1337, Pitch = 1f, UnlockRuns = 0,
                Glow = new Color(1.7f, 0.75f, 2.3f, 1f),
                Ground = new Color(0.06f, 0.07f, 0.13f),
                Ambient = new Color(0.07f, 0.08f, 0.16f),
            },
            new CompanionData
            {
                Name = "Corvel", Seed = 7793, Pitch = 1.18f, UnlockRuns = 0,
                Glow = new Color(0.5f, 1.9f, 1.4f, 1f),
                Ground = new Color(0.05f, 0.11f, 0.09f),
                Ambient = new Color(0.06f, 0.13f, 0.10f),
            },
            new CompanionData
            {
                Name = "Mara", Seed = 4242, Pitch = 0.84f, UnlockRuns = 0,
                Glow = new Color(2.2f, 1.3f, 0.7f, 1f),
                Ground = new Color(0.12f, 0.08f, 0.06f),
                Ambient = new Color(0.15f, 0.10f, 0.07f),
            },
            new CompanionData
            {
                Name = "Ash", Seed = 9001, Pitch = 0.66f, UnlockRuns = 3,
                Glow = new Color(2.6f, 0.9f, 0.35f, 1f),
                Ground = new Color(0.11f, 0.06f, 0.06f),
                Ambient = new Color(0.13f, 0.07f, 0.06f),
            },
        };

        public static CompanionData Current => All[Mathf.Clamp(GameState.CompanionIndex, 0, All.Length - 1)];

        public static int UnlockedCount(int runs)
        {
            int c = 0;
            for (int i = 0; i < All.Length; i++)
                if (All[i].UnlockRuns <= runs) c++;
            return c < 1 ? 1 : c;
        }
    }
}
