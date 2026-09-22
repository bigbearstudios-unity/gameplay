using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// The first thing guaranteed to be running regardless of which scene the game is started
    /// from. EnsureLoaded (a RuntimeInitializeOnLoadMethod hook, so it runs before any scene's
    /// own Awake calls) always guarantees the Boot scene is loaded — and, if the scene actually
    /// being entered isn't part of the Boot/Frontend/Gameplay chain itself (e.g. a developer
    /// pressed Play with a Level/combat scene open directly in the Editor), also guarantees
    /// Gameplay's persistent systems (Player, shared UI, ...) exist, so that scene isn't missing
    /// everything it depends on. Frontend is deliberately skipped in that case — testing a Level
    /// shouldn't require clicking through a splash screen and main menu first every time.
    ///
    /// Once this controller itself wakes up (i.e. the Boot scene is actually running), it
    /// async-loads everything in ScenesToLoad additively — this is the normal path Frontend gets
    /// loaded through when the game starts properly from Boot.
    /// </summary>
    public class BootLoaderController : MonoBehaviour {

        private const string BootSettingsResourcePath = "BootSettings";

        [SerializeField, Tooltip("Scenes async-loaded (additively) as soon as this controller wakes up — Frontend belongs here.")]
        private List<SceneReference> _scenesToLoad = new List<SceneReference>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLoaded() {
            BootSettings settings = Resources.Load<BootSettings>(BootSettingsResourcePath);
            if (settings == null) {
                Debug.LogError($"BootLoaderController: no BootSettings asset found — create one at 'Resources/{BootSettingsResourcePath}.asset'.");
                return;
            }

            EnsureSceneLoadedImmediate(settings.BootScene, "Boot Scene");

            string activeSceneName = SceneManager.GetActiveScene().name;
            bool isKnownTierScene = IsScene(settings.BootScene, activeSceneName)
                || IsScene(settings.FrontendScene, activeSceneName)
                || IsScene(settings.GameplayScene, activeSceneName);

            if (!isKnownTierScene) {
                EnsureSceneLoadedImmediate(settings.GameplayScene, "Gameplay Scene");
            }
        }

        private static bool IsScene(SceneReference scene, string sceneName) {
            return scene != null && scene.IsSet && scene.SceneName == sceneName;
        }

        // Deliberately not SceneLoader.Load(): this runs before any scene exists yet, so it needs
        // the synchronous, "fully loaded (Awake included) before returning" guarantee LoadScene
        // gives — SceneLoader's async/skip-if-loaded behaviour is for everything after this point.
        private static void EnsureSceneLoadedImmediate(SceneReference scene, string label) {
            if (scene == null || !scene.IsSet) {
                Debug.LogError($"BootLoaderController: no {label} configured in BootSettings.");
                return;
            }

            if (SceneManager.GetSceneByName(scene.SceneName).isLoaded) { return; }

            SceneManager.LoadScene(scene.SceneName, LoadSceneMode.Additive);
        }

        private void Awake() {
            foreach (SceneReference scene in _scenesToLoad) {
                if (scene == null || !scene.IsSet) {
                    Debug.LogWarning($"BootLoaderController on '{gameObject.name}' has a Scenes To Load entry with no scene picked — it will be skipped.", this);
                    continue;
                }

                SceneLoader.Load(scene);
            }
        }
    }
}
