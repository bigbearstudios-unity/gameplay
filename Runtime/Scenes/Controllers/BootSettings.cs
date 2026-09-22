using UnityEngine;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// Must live at Assets/Resources/BootSettings.asset — the one thing BootLoaderController's static
    /// bootstrap (see BootLoaderController.EnsureLoaded) can read before any scene, including Boot
    /// itself, has loaded. Resources.Load works regardless of what's currently loaded; a
    /// serialized field on a BootLoaderController instance wouldn't be reachable yet at that point.
    ///
    /// Frontend/Gameplay are named here (not just left to BootLoaderController's own scene list)
    /// because EnsureLoaded() needs to know their identity specifically — both to recognise "I'm
    /// already entering one of the known tiers, don't do anything extra" and to know which scene
    /// to guarantee-load when that's not the case (see EnsureLoaded's doc comment).
    /// </summary>
    [CreateAssetMenu(fileName = "BootSettings", menuName = "BBUnity/Boot Settings")]
    public class BootSettings : ScriptableObject {

        [SerializeField]
        private SceneReference _bootScene;

        [SerializeField]
        private SceneReference _frontendScene;

        [SerializeField]
        private SceneReference _gameplayScene;

        public SceneReference BootScene {
            get { return _bootScene; }
        }

        public SceneReference FrontendScene {
            get { return _frontendScene; }
        }

        public SceneReference GameplayScene {
            get { return _gameplayScene; }
        }
    }
}
