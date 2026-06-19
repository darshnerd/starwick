using UnityEngine;

namespace Starwick
{
    public class RunLauncher : MonoBehaviour
    {
        public RunDirector Dir { get; private set; }
        public RunHUD Hud { get; private set; }
        public bool Active { get; private set; }

        bool camWasEnabled;

        public void EnterRun(int seed)
        {
            if (Active) return;

            if (!Application.isBatchMode && Sw.Cam != null)
            {
                camWasEnabled = Sw.Cam.enabled;
                Sw.Cam.enabled = false;
            }

            var dGo = new GameObject("RunDirector");
            dGo.transform.SetParent(transform);
            Dir = dGo.AddComponent<RunDirector>();
            Dir.Begin(seed);

            var hGo = new GameObject("RunHUD");
            hGo.transform.SetParent(transform);
            Hud = hGo.AddComponent<RunHUD>();
            Dir.Hud = Hud;

            FlowInput.UseSynthetic = false;
            Active = true;
        }

        public void ExitRun()
        {
            if (!Active) return;
            if (Dir != null) Destroy(Dir.gameObject);
            if (Hud != null) Destroy(Hud.gameObject);
            if (!Application.isBatchMode && Sw.Cam != null) Sw.Cam.enabled = camWasEnabled;
            Active = false;
        }
    }
}
