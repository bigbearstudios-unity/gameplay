using UnityEngine;

using BBUnity.UI;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// The transient splash/menu tier, in contrast to GameplayController which stays loaded for
    /// the whole session. Built on UIScreenController's named-screens/transitions machinery (see
    /// that class) for the splash/menu/settings flow itself — a StateController-driven state
    /// machine (SplashState -> MainMenuState -> ...) is a natural fit for deciding *when* to
    /// switch screens given the rest of this framework, but that's entirely up to the concrete
    /// game. All this adds on top is transitioning out of this scene once your own flow decides
    /// it's time: wire a "Start" button, or a state's Enter(), to call TransitionToGameplay().
    /// </summary>
    public class UIMainMenuController : UIScreenController {

        [SerializeField, Tooltip("Loaded additively when TransitionToGameplay() is called.")]
        private SceneReference _gameplayScene;

        [SerializeField, Tooltip("Whether this Frontend scene is unloaded once the transition to Gameplay happens. On by default to free menu-only assets; turn off if you want an instant \"Back to Main Menu\" without reloading it.")]
        private bool _unloadOnTransition = true;

        public void TransitionToGameplay() {
            if (_gameplayScene == null || !_gameplayScene.IsSet) {
                Debug.LogWarning($"UIMainMenuController on '{gameObject.name}' has no Gameplay Scene configured.", this);
                return;
            }

            SceneLoader.Load(_gameplayScene);

            if (_unloadOnTransition) {
                SceneLoader.Unload(gameObject.scene);
            }
        }
    }
}
