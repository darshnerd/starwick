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

            var root = new GameObject("Starwick");
            Object.DontDestroyOnLoad(root);
            Sw.Root = root;

            var camGo = new GameObject("CameraRig");
            camGo.transform.SetParent(root.transform);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.02f, 0.05f);
            cam.farClipPlane = 2000f;
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraRig>();
            Sw.Cam = cam;

            var cosmosGo = new GameObject("Cosmos");
            cosmosGo.transform.SetParent(root.transform);
            Sw.Cosmos = cosmosGo.AddComponent<CosmosFx>();

            var postGo = new GameObject("PostFx");
            postGo.transform.SetParent(root.transform);
            postGo.AddComponent<PostFx>();

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
            motifSrc.clip = ProcAudio.Motif();
            motifSrc.loop = true;
            motifSrc.spatialBlend = 0f;
            motifSrc.volume = 0.32f;
            motifSrc.Play();
            Sw.Motif = motifSrc;
            Sw.MotifStarted = true;

            var conGo = new GameObject("Constellation");
            conGo.transform.SetParent(camGo.transform, false);
            conGo.transform.localPosition = new Vector3(0f, 0f, 14f);
            conGo.transform.localRotation = Quaternion.identity;
            Sw.Constellation = conGo.AddComponent<Constellation>();

            root.AddComponent<SwTestHarness>();
            Sw.Booted = true;
        }
    }
}
