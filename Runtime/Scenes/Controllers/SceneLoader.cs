using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// Shared "load/unload a SceneReference" wrapper around SceneManager — Boot, Frontend and
    /// Gameplay all need this, so it exists once here rather than three times.
    /// </summary>
    public static class SceneLoader {

        public static void Load(SceneReference scene, LoadSceneMode mode = LoadSceneMode.Additive) {
            if (scene == null || !scene.IsSet) {
                Debug.LogWarning("SceneLoader.Load called with no scene configured.");
                return;
            }

            if (SceneManager.GetSceneByName(scene.SceneName).isLoaded) { return; }

            SceneManager.LoadSceneAsync(scene.SceneName, mode);
        }

        public static void Unload(SceneReference scene) {
            if (scene == null || !scene.IsSet) { return; }

            Unload(SceneManager.GetSceneByName(scene.SceneName));
        }

        public static void Unload(Scene scene) {
            if (!scene.IsValid() || !scene.isLoaded) { return; }

            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
