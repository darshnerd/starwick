using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public static class SaveData
    {
        public static int TotalStarsRelit;
        public static int RunsCompleted;
        public static int CompanionsSeen;
        public static int SecretsFound;

        const string KStars = "sw_stars";
        const string KRuns = "sw_runs";
        const string KComp = "sw_comp";
        const string KSecrets = "sw_secrets";

        static readonly Dictionary<string, int> mem = new Dictionary<string, int>();
        static bool InMemory => Application.isBatchMode;

        static int Get(string k) => InMemory ? (mem.TryGetValue(k, out var v) ? v : 0) : PlayerPrefs.GetInt(k, 0);
        static void Set(string k, int v) { if (InMemory) mem[k] = v; else PlayerPrefs.SetInt(k, v); }

        public static void Load()
        {
            TotalStarsRelit = Get(KStars);
            RunsCompleted = Get(KRuns);
            CompanionsSeen = Get(KComp);
            SecretsFound = Get(KSecrets);
        }

        public static void Save()
        {
            Set(KStars, TotalStarsRelit);
            Set(KRuns, RunsCompleted);
            Set(KComp, CompanionsSeen);
            Set(KSecrets, SecretsFound);
            if (!InMemory) PlayerPrefs.Save();
        }

        public static void RecordRelight()
        {
            TotalStarsRelit++;
            Save();
        }

        public static void RecordRun()
        {
            RunsCompleted++;
            Save();
        }

        public static void MarkCompanionSeen(int index)
        {
            if (index < 0 || index > 30) return;
            CompanionsSeen |= 1 << index;
            Save();
        }

        public static int CompanionsSeenCount()
        {
            int c = 0;
            int m = CompanionsSeen;
            while (m != 0)
            {
                c += m & 1;
                m >>= 1;
            }
            return c;
        }
    }
}
