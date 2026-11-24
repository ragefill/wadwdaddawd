using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoreSandbox.Editor
{
    /// <summary>
    /// Auto-creates the Gore Sandbox scene when the project opens.
    /// Cleans up any previous [GS] objects and rebuilds floor, walls, player rig,
    /// hands, GameManager, GoreManager, spawn menu, and scattered breakable props.
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

            SceneBuilder.CreateScene();
        }
    }
}
