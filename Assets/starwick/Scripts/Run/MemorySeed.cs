using System.Text;
using UnityEngine;

namespace Starwick
{
    public static class MemorySeed
    {
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const uint Version = 2u;
        static readonly string[] Flavor = { "LUMEN", "NOVA", "EMBER", "VEIL", "DRIFT", "HALO", "ECHO", "SPARK" };

        public static string Encode(int companionIndex, int seed)
        {
            int idx = Mathf.Clamp(companionIndex, 0, Roster.All.Length - 1);
            string tag = Tag(Roster.All[idx].Name);
            string s = Base36((uint)seed);
            uint h = ((uint)seed ^ (uint)(idx * 2654435761u));
            string f = Flavor[(int)(h % (uint)Flavor.Length)];
            string chk = Checksum(idx, (uint)seed);
            return tag + "-" + s + "-" + f + "-" + chk;
        }

        public static bool TryDecode(string code, out int companionIndex, out int seed)
        {
            companionIndex = 0;
            seed = 0;
            if (string.IsNullOrEmpty(code)) return false;
            var parts = code.Trim().ToUpperInvariant().Split('-');
            if (parts.Length < 3) return false;

            int idx = -1;
            for (int i = 0; i < Roster.All.Length; i++)
                if (Tag(Roster.All[i].Name) == parts[0]) { idx = i; break; }
            if (idx < 0) return false;

            if (!TryBase36(parts[1], out uint v)) return false;

            string chk = parts[parts.Length - 1];
            if (chk != Checksum(idx, v)) return false;

            companionIndex = idx;
            seed = (int)v;
            return true;
        }

        static string Checksum(int idx, uint seed)
        {
            uint h = 2166136261u;
            h = (h ^ Version) * 16777619u;
            h = (h ^ (uint)idx) * 16777619u;
            h = (h ^ seed) * 16777619u;
            uint c = h % 1296u;
            return new string(new[] { Digits[(int)(c / 36u)], Digits[(int)(c % 36u)] });
        }

        static string Tag(string name)
        {
            var sb = new StringBuilder();
            foreach (char ch in name.ToUpperInvariant())
                if (ch >= 'A' && ch <= 'Z') sb.Append(ch);
            return sb.Length > 0 ? sb.ToString() : "WICK";
        }

        static string Base36(uint v)
        {
            if (v == 0) return "0";
            var sb = new StringBuilder();
            while (v > 0)
            {
                sb.Insert(0, Digits[(int)(v % 36u)]);
                v /= 36u;
            }
            return sb.ToString();
        }

        static bool TryBase36(string s, out uint v)
        {
            v = 0;
            if (string.IsNullOrEmpty(s) || s.Length > 7) return false;
            foreach (char ch in s)
            {
                int d = Digits.IndexOf(ch);
                if (d < 0) return false;
                if (v > (uint.MaxValue - (uint)d) / 36u) return false;
                v = v * 36u + (uint)d;
            }
            return true;
        }
    }
}
