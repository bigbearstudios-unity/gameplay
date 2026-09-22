using UnityEngine;

using System;
using System.Collections.Generic;

using BBUnity.UI.Transitions;

namespace BBUnity.UI {

    /// <summary>
    /// One entry in a UIScreenController's screen list — a UIScreen looked up by a
    /// designer-chosen name rather than a hand-written field per screen.
    /// </summary>
    [Serializable]
    public class UIScreenEntry {

        [SerializeField]
        private string _name;

        [SerializeField]
        private UIScreen _screen;

        public string Name {
            get { return _name; }
        }

        public UIScreen Screen {
            get { return _screen; }
        }
    }

    /// <summary>
    /// Generic "named set of UIScreens, one active at a time" controller — the reusable building
    /// block behind both UIMainMenuController (splash/menu/settings) and, later, UIGameMenuController
    /// (in-game map/options). Concrete subclasses only need to add whatever domain-specific
    /// behaviour they need on top (e.g. UIMainMenuController's TransitionToGameplay()); switching
    /// between named screens, and how that switch animates, lives here once.
    /// </summary>
    public class UIScreenController : MonoBehaviour {

        [SerializeField]
        private List<UIScreenEntry> _screens = new List<UIScreenEntry>();

        [SerializeReference, Tooltip("Used by ShowScreen(name) overloads that don't specify a transition explicitly.")]
        private UIScreenTransition _defaultTransition = new UIInstantScreenTransition();

        private UIScreen _current;

        public UIScreen CurrentScreen {
            get { return _current; }
        }

        public UIScreen GetScreen(string name) {
            foreach (UIScreenEntry entry in _screens) {
                if (entry.Name == name) { return entry.Screen; }
            }

            Debug.LogWarning($"UIScreenController on '{gameObject.name}' has no screen named '{name}'.", this);
            return null;
        }

        public void ShowScreen(string name, Action onComplete = null) {
            ShowScreen(name, _defaultTransition, onComplete);
        }

        public void ShowScreen(string name, UIScreenTransition transition, Action onComplete = null) {
            UIScreen target = GetScreen(name);
            if (target == null) { return; }

            UIScreen previous = _current;
            _current = target;

            transition.Play(previous, target, onComplete);
        }
    }
}
