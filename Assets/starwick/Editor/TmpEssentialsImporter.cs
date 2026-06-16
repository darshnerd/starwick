#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Starwick.EditorTools
{
    [InitializeOnLoad]
    public static class TmpEssentialsImporter
    {
        const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        const string EssentialsPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        const string SessionKey = "Starwick.TMPImportAttempted";

        static TmpEssentialsImporter()
        {
            EditorApplication.delayCall += TryImport;
        }

        static void TryImport()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            if (File.Exists(TmpSettingsPath)) return;
            SessionState.SetBool(SessionKey, true);
            Debug.Log("[Starwick] Importing TMP Essential Resources...");
            AssetDatabase.ImportPackage(EssentialsPackage, false);
        }

        [MenuItem("Starwick/Setup/Import TMP Essentials")]
        public static void ImportMenu()
        {
            AssetDatabase.ImportPackage(EssentialsPackage, false);
        }
    }
}
#endif
