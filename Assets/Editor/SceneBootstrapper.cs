using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoreSandbox.Editor
{
    /// <summary>
    /// Auto-creates the Gore Sandbox scene when opening a fresh Unity project
    /// (SampleScene). If you already have your own scene, this does nothing.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneBootstrapper
    {
        static SceneBootstrapper()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        static void EnsureScene()
        {
            if (Object.FindObjectOfType<GameManager>() != null) return;
            if (SceneManager.GetActiveScene().name != "SampleScene") return;

            SceneBuilder.CreateScene();
        }
    }
}
