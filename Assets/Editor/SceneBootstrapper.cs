using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoreSandbox.Editor
{
    /// <summary>
    /// Auto-creates the Gore Sandbox scene when the project opens.
    /// Runs regardless of which scene is active. If GameManager is missing
    /// from the scene, it builds the full demo (floor, walls, player rig,
    /// hands, GameManager, GoreManager, spawn menu) and saves GoreSandbox.unity.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneBootstrapper
    {
        static bool hasRun;

        static SceneBootstrapper()
        {
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (hasRun) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (Application.isPlaying) return;

            hasRun = true;
            EditorApplication.update -= OnUpdate;

            // Only act if there's no GameManager already in the scene
            if (Object.FindObjectOfType<GameManager>(true) != null) return;

            SceneBuilder.CreateScene();
        }
    }
}
