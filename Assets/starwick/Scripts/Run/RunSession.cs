using UnityEngine;

namespace Starwick
{
    public class RunSession : MonoBehaviour
    {
        RunLauncher launcher;

        public bool Active => launcher != null && launcher.Active;
        public RunDirector Dir => launcher != null ? launcher.Dir : null;
        public RunHUD Hud => launcher != null ? launcher.Hud : null;

        RunLauncher EnsureLauncher()
        {
            if (launcher == null)
            {
                var go = new GameObject("RunLauncher");
                go.transform.SetParent(transform, false);
                launcher = go.AddComponent<RunLauncher>();
            }
            return launcher;
        }

        public bool Enter(int seed)
        {
            if (Active) return false;
            EnsureLauncher().EnterRun(seed);
            return true;
        }

        public bool EnterFromCode(string code)
        {
            if (Active) return false;
            return EnsureLauncher().EnterRunFromCode(code);
        }

        public void Exit()
        {
            if (launcher != null) launcher.ExitRun();
        }
    }
}
