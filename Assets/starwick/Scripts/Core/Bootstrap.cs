using UnityEngine;

namespace Starwick
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Sw.Booted) return;

            foreach (var existing in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
                Object.Destroy(existing.gameObject);
            foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include))
                Object.Destroy(listener);

            GameState.Reset();
            SaveData.Load();
            SaveData.MarkCompanionSeen(GameState.CompanionIndex);

            var root = new GameObject("Starwick");
            Object.DontDestroyOnLoad(root);
            Sw.Root = root;

            var realmGo = new GameObject("GroundRealm");
            realmGo.transform.SetParent(root.transform);
            Sw.Realm = realmGo.AddComponent<GroundRealm>();

            var camGo = new GameObject("CameraRig");
            camGo.transform.SetParent(root.transform);
            camGo.transform.position = new Vector3(0f, 8f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.02f, 0.05f);
            cam.farClipPlane = 2000f;
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraRig>();
            Sw.Cam = cam;

            var wickGo = new GameObject("WickBody");
            wickGo.transform.SetParent(camGo.transform, false);
            wickGo.transform.localPosition = new Vector3(0.35f, -1.15f, 2.5f);
            wickGo.AddComponent<WickBody>();

            var cosmosGo = new GameObject("Cosmos");
            cosmosGo.transform.SetParent(root.transform);
            Sw.Cosmos = cosmosGo.AddComponent<CosmosFx>();

            var postGo = new GameObject("PostFx");
            postGo.transform.SetParent(root.transform);
            Sw.PostFx = postGo.AddComponent<PostFx>();

            var audioGo = new GameObject("Ambient");
            audioGo.transform.SetParent(root.transform);
            var src = audioGo.AddComponent<AudioSource>();
            src.clip = ProcAudio.Drone();
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = 0.45f;
            src.Play();
            Sw.Ambient = src;
            Sw.AmbientStarted = true;

            var vespGo = new GameObject("Vesp");
            vespGo.transform.SetParent(root.transform);
            vespGo.transform.position = new Vector3(3f, 0f, -4f);
            Sw.Companion = vespGo.AddComponent<Companion>();
            var motifSrc = vespGo.AddComponent<AudioSource>();
            motifSrc.clip = ProcAudio.Motif(Roster.Current.Pitch);
            motifSrc.loop = true;
            motifSrc.spatialBlend = 0f;
            motifSrc.volume = 0.32f;
            motifSrc.Play();
            Sw.Motif = motifSrc;
            Sw.MotifStarted = true;

            var dlgGo = new GameObject("Dialogue");
            dlgGo.transform.SetParent(root.transform);
            Sw.Dialogue = dlgGo.AddComponent<DialogueSystem>();

            var conGo = new GameObject("Constellation");
            conGo.transform.SetParent(root.transform);
            var sites = Sw.Realm != null ? Sw.Realm.Sites : null;
            var site0 = (sites != null && sites.Length > 0) ? sites[0] : Vector3.zero;
            conGo.transform.position = site0 + Vector3.up * 7f;
            Sw.Constellation = conGo.AddComponent<Constellation>();
            Sw.Constellation.FaceCamera = true;
            Sw.Constellation.SiteIndex = 0;

            var decorGo = new GameObject("RealmDecor");
            decorGo.transform.SetParent(root.transform);
            decorGo.AddComponent<RealmDecor>();

            var uiGo = new GameObject("NarrationUI");
            uiGo.transform.SetParent(root.transform);
            Sw.Narration = uiGo.AddComponent<NarrationUI>();

            var journalGo = new GameObject("JournalUI");
            journalGo.transform.SetParent(root.transform);
            Sw.Journal = journalGo.AddComponent<JournalUI>();

            var skyGo = new GameObject("ConstellariumUI");
            skyGo.transform.SetParent(root.transform);
            Sw.Constellarium = skyGo.AddComponent<ConstellariumUI>();

            var titleGo = new GameObject("TitleUI");
            titleGo.transform.SetParent(root.transform);
            Sw.Title = titleGo.AddComponent<TitleUI>();

            var dirGo = new GameObject("Director");
            dirGo.transform.SetParent(root.transform);
            Sw.Director = dirGo.AddComponent<Director>();

            var audioMgrGo = new GameObject("AudioManager");
            audioMgrGo.transform.SetParent(root.transform);
            audioMgrGo.AddComponent<AudioManager>();

            var sfxGo = new GameObject("SfxManager");
            sfxGo.transform.SetParent(root.transform);
            sfxGo.AddComponent<SfxManager>();

            if (!Application.isBatchMode)
            {
                var coldGo = new GameObject("ColdOpen");
                coldGo.transform.SetParent(root.transform);
                coldGo.AddComponent<ColdOpen>();
            }

            root.AddComponent<SwTestHarness>();
            Sw.Booted = true;
        }
    }
}
