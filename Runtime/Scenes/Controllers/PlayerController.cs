using UnityEngine;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// Lives on the root of the Player prefab — the stable handle the rest of the game looks up
    /// via Current rather than needing a reference threaded through from GameplayController
    /// everywhere (a Level repositioning the Player at a PlayerSpawnNode, UI binding to it, ...).
    ///
    /// LoadState() is called once by GameplayController right after this Player is instantiated —
    /// it's an extension point, not an implementation: save-file format is entirely game-specific,
    /// so this framework has no opinion on it.
    /// </summary>
    public abstract class PlayerController : MonoBehaviour {

        public static PlayerController Current { get; private set; }

        protected virtual void Awake() {
            Current = this;
        }

        protected virtual void OnDestroy() {
            if (Current == this) { Current = null; }
        }

        public abstract void LoadState();
    }
}
