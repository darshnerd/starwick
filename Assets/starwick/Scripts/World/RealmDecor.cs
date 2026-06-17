using System.Collections;
using UnityEngine;

namespace Starwick
{
    public class RealmDecor : MonoBehaviour
    {
        public RealmSite[] Sites { get; private set; }
        public Vector3 ObjectiveCenter { get; private set; }
        public bool AllSitesLit { get; private set; }

        int relitCount;

        void Start()
        {
            if (Sw.Realm == null) return;
            Sw.Decor = this;

            BuildSites();
            ResetSites();
            if (Sw.Constellation != null) Sw.Constellation.OnRelight += OnRelight;
        }

        void BuildSites()
        {
            var pts = Sw.Realm.Sites;
            Sites = new RealmSite[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                var go = new GameObject("Site_" + i);
                go.transform.SetParent(transform, false);
                var site = go.AddComponent<RealmSite>();
                site.Build(pts[i], i);
                Sites[i] = site;
                site.MarkDormant();
            }
        }

        public void Reseed(int index)
        {
            StopAllCoroutines();
            GameState.CompanionIndex = index;
            SaveData.MarkCompanionSeen(index);
            if (index >= 0 && index < Roster.All.Length && Roster.All[index].UnlockRuns > 0)
            {
                SaveData.SecretsFound = 1;
                SaveData.Save();
            }
            if (Sw.Realm != null) Sw.Realm.RebuildFor(Roster.Current.Seed);

            if (Sites != null)
                for (int i = 0; i < Sites.Length; i++)
                    if (Sites[i] != null) Destroy(Sites[i].gameObject);

            BuildSites();

            if (Sw.Companion != null) Sw.Companion.Retint();
            if (Sw.Motif != null)
            {
                Sw.Motif.clip = ProcAudio.Motif(Roster.Current.Pitch);
                Sw.Motif.Play();
            }

            ResetSites();
        }

        void OnRelight()
        {
            if (Sites == null || Sites.Length == 0) return;
            int idx = Sw.Constellation != null ? Mathf.Clamp(Sw.Constellation.SiteIndex, 0, Sites.Length - 1) : 0;
            Sites[idx].Light();
            relitCount++;

            int next = NextUnlit(idx);
            if (next < 0)
            {
                AllSitesLit = true;
                ObjectiveCenter = Sites[idx].Center;
                return;
            }

            Sites[next].MarkNext();
            ObjectiveCenter = Sites[next].Center;
            StartCoroutine(RelocateAfterFx(next));
        }

        IEnumerator RelocateAfterFx(int site)
        {
            yield return new WaitForSeconds(2.4f);
            if (Sw.Constellation == null || Sites == null || site < 0 || site >= Sites.Length) yield break;
            Sw.Constellation.SiteIndex = site;
            Sw.Constellation.transform.position = Sites[site].Center + Vector3.up * 7f;
            Sw.Constellation.ResetForReplay();
        }

        int NextUnlit(int from)
        {
            for (int step = 1; step <= Sites.Length; step++)
            {
                int j = (from + step) % Sites.Length;
                if (!Sites[j].Lit) return j;
            }
            return -1;
        }

        public void ResetSites()
        {
            StopAllCoroutines();
            relitCount = 0;
            AllSitesLit = false;

            if (Sites == null || Sites.Length == 0) return;
            for (int i = 0; i < Sites.Length; i++) Sites[i].ResetState();

            if (Sw.Constellation != null)
            {
                Sw.Constellation.SiteIndex = 0;
                Sw.Constellation.transform.position = Sites[0].Center + Vector3.up * 7f;
                Sw.Constellation.ResetForReplay();
            }
            Sites[0].MarkNext();
            ObjectiveCenter = Sites[0].Center;
        }
    }
}
