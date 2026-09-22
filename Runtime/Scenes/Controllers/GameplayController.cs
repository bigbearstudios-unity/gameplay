using UnityEngine;

namespace BBUnity.Scenes.Controllers {

    /// <summary>
    /// The persistent "game session" tier — stays loaded for the whole play session, with Level
    /// scenes loading/unloading additively on top of it (see PlayerTransitionNode). Owns the
    /// Player, instantiated immediately (see SpawnPlayer) rather than waiting on the first Level,
    /// and the shared UI root everything else (main HUD, pooled enemy health bars, ...) hangs off
    /// — that's what fixes needing a Canvas per enemy: a Level-scene object reaches this via
    /// Current the same way an enemy's AttackCollider reaches its attacker's controller.
    ///
    /// Override OnReady() to wire up whatever else a concrete game needs once the Player exists —
    /// inventory, quest tracking, audio, ... this framework has no opinion on what those are.
    /// </summary>
    public abstract class GameplayController : MonoBehaviour {

        public static GameplayController Current { get; private set; }

        [SerializeField, Tooltip("Instantiated immediately when this scene loads — must carry a PlayerController component.")]
        private GameObject _playerPrefab;

        [SerializeField, Tooltip("Where the Player is instantiated. Falls back to this GameObject's own position if unassigned. Level scenes are responsible for repositioning the Player to a PlayerSpawnNode once loaded — this is only the very first, pre-Level placement.")]
        private Transform _playerSpawnAnchor;

        [SerializeField, Tooltip("The Canvas shared UI (main HUD, pooled enemy health bars, ...) should be parented under.")]
        private Canvas _sharedUIRoot;

        private PlayerController _player;

        public PlayerController Player {
            get { return _player; }
        }

        public Canvas SharedUIRoot {
            get { return _sharedUIRoot; }
        }

        protected virtual void Awake() {
            Current = this;

            SpawnPlayer();
            OnReady();
        }

        protected virtual void OnDestroy() {
            if (Current == this) { Current = null; }
        }

        /// <summary>
        /// Runs once, after SpawnPlayer() — extension point for a concrete game's own
        /// cross-cutting systems.
        /// </summary>
        protected virtual void OnReady() { }

        private void SpawnPlayer() {
            if (_playerPrefab == null) {
                Debug.LogWarning($"GameplayController on '{gameObject.name}' has no Player Prefab assigned.", this);
                return;
            }

            Vector3 position = _playerSpawnAnchor != null ? _playerSpawnAnchor.position : transform.position;
            GameObject instance = Instantiate(_playerPrefab, position, Quaternion.identity);

            _player = instance.GetComponent<PlayerController>();
            if (_player == null) {
                Debug.LogWarning($"GameplayController on '{gameObject.name}': Player Prefab has no PlayerController component.", this);
                return;
            }

            _player.LoadState();
        }
    }
}
