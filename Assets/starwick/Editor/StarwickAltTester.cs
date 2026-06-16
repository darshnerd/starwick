#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
#if ALTTESTER
using AltTester.AltTesterUnitySDK.Editor;
using AltTester.AltTesterSDK.Driver;
#endif

namespace Starwick.EditorTools
{
    public static class StarwickAltTester
    {
        const string Define = "ALTTESTER";

        static NamedBuildTarget Current =>
            NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        [MenuItem("Starwick/AltTester/1. Enable Instrumentation Define")]
        public static void EnableDefine()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(Current);
            if (defines.Split(';').Contains(Define))
            {
                Debug.Log("[Starwick] ALTTESTER already enabled.");
                return;
            }
            defines = string.IsNullOrEmpty(defines) ? Define : defines + ";" + Define;
            PlayerSettings.SetScriptingDefineSymbols(Current, defines);
            Debug.Log("[Starwick] Enabled ALTTESTER. Unity will recompile — then run " +
                      "'Starwick/AltTester/2. Insert Into Active Scene'.");
        }

        [MenuItem("Starwick/AltTester/Disable Instrumentation Define")]
        public static void DisableDefine()
        {
            var kept = PlayerSettings.GetScriptingDefineSymbols(Current)
                .Split(';')
                .Where(d => !string.IsNullOrEmpty(d) && d != Define)
                .ToArray();
            PlayerSettings.SetScriptingDefineSymbols(Current, string.Join(";", kept));
            Debug.Log("[Starwick] Disabled ALTTESTER. (Remember to do this before shipping builds.)");
        }

#if ALTTESTER
        static AltInstrumentationSettings StarwickSettings() => new AltInstrumentationSettings
        {
            AltServerHost = "127.0.0.1",
            AltServerPort = 13000,
            AppName = "Starwick"
        };

        [MenuItem("Starwick/AltTester/2. Insert Into Active Scene")]
        public static void InsertIntoActiveScene()
        {
            AltBuilder.InsertAltTesterInTheActiveScene(StarwickSettings());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Starwick] AltTester inserted into the active scene (127.0.0.1:13000). " +
                      "Press Play, then run the Python driver in ~/starwick/altdriver/.");
        }
#endif
    }
}
#endif