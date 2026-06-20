using System.IO;
using UnityEngine;

namespace Starwick
{
    public static class PlayerProfileStore
    {
        public const int CurrentVersion = 1;
        public static PlayerProfile Current { get; private set; }

        static bool InMemory => Application.isBatchMode;
        static string memJson;
        static string FilePath => Path.Combine(Application.persistentDataPath, "starwick_profile.json");

        public static void Load()
        {
            string json = InMemory ? memJson : (File.Exists(FilePath) ? File.ReadAllText(FilePath) : null);
            LoadFromJson(json);
        }

        public static void LoadFromJson(string json)
        {
            Current = string.IsNullOrEmpty(json)
                ? new PlayerProfile()
                : (JsonUtility.FromJson<PlayerProfile>(json) ?? new PlayerProfile());
            Migrate(Current);
        }

        public static void Save()
        {
            if (Current == null) Current = new PlayerProfile();
            string json = JsonUtility.ToJson(Current);
            if (InMemory) memJson = json;
            else File.WriteAllText(FilePath, json);
        }

        public static void ApplyRunResults(RunResults r)
        {
            if (Current == null) Load();
            Current.TotalRuns++;
            Current.TotalStarsRelit += r.GatesRelit;
            Current.Starlight += r.Starlight;

            int idx = r.HollowIndex;
            if (idx < 0 || idx >= Roster.All.Length) idx = Roster.IndexOf(Current.CurrentHollow);
            if (idx >= 0 && idx < Roster.All.Length)
            {
                Current.CurrentHollow = Roster.All[idx].Name;
                if (Current.Bonds != null && idx < Current.Bonds.Length)
                    Current.Bonds[idx] += r.GatesRelit * 10 + r.BestChain * 5;
            }

            Save();
        }

        public static void AddMemory(string memory)
        {
            if (Current == null) Load();
            if (Current.MemoryArchive == null) Current.MemoryArchive = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(memory) && !Current.MemoryArchive.Contains(memory))
            {
                Current.MemoryArchive.Add(memory);
                Save();
            }
        }

        static void Migrate(PlayerProfile p)
        {
            if (p.Version < 1) p.Version = 1;
            p.Version = CurrentVersion;
        }
    }
}
