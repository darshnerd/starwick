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

        public static void Load()
        {
            TotalStarsRelit = PlayerPrefs.GetInt(KStars, 0);
            RunsCompleted = PlayerPrefs.GetInt(KRuns, 0);
            CompanionsSeen = PlayerPrefs.GetInt(KComp, 0);
            SecretsFound = PlayerPrefs.GetInt(KSecrets, 0);
        }

        public static void Save()
        {
            PlayerPrefs.SetInt(KStars, TotalStarsRelit);
            PlayerPrefs.SetInt(KRuns, RunsCompleted);
            PlayerPrefs.SetInt(KComp, CompanionsSeen);
            PlayerPrefs.SetInt(KSecrets, SecretsFound);
            PlayerPrefs.Save();
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
