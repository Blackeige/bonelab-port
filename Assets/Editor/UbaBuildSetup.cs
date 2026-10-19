using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Configuration automatique du projet pour Unity Build Automation (aucun editeur Unity n'est necessaire).
/// Dans le tableau de bord : Advanced settings > Pre-export method name = UbaBuildSetup.PreExport
/// </summary>
public static class UbaBuildSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    public static void PreExport()
    {
        try
        {
            ApplyPlayerSettings();
            EnsureScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[LaboVR] PreExport termine.");
        }
        catch (Exception e)
        {
            Debug.LogError("[LaboVR] PreExport a echoue : " + e);
            throw;
        }
    }

    private static void ApplyPlayerSettings()
    {
        PlayerSettings.companyName = "LaboVR";
        PlayerSettings.productName = "Labo VR";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.labovr.prototype");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.Android.forceInternetPermission = false;
    }

    /// <summary>Verifie que la scene existe et se charge, sinon la recree ; la declare dans Build Settings.</summary>
    private static void EnsureScene()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string fullPath = Path.Combine(projectRoot, ScenePath);
        bool usable = false;

        if (File.Exists(fullPath))
        {
            try
            {
                UnityEngine.SceneManagement.Scene opened = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                usable = opened.IsValid();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LaboVR] Scene illisible, recreation : " + e.Message);
            }
        }

        if (!usable)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            UnityEngine.SceneManagement.Scene created = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(created, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[LaboVR] Scene creee : " + ScenePath);
        }

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(ScenePath, true) };
    }
}
