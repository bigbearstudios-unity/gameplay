using UnityEngine;

using System;
using System.Collections.Generic;

using BBUnity.StateMachines;
using BBUnity.EditorAttributes;
using BBUnity.Entities.Controllers;
using BBUnity.Entities.Controllers.States;

namespace BBUnity.Entities.Controllers {

    /// <summary>
    /// See <see cref="BBUnity.Entities.Controllers.InputController"/> for the rationale behind
    /// this execution order value and an important caveat about it not being inherited by
    /// concrete subclasses (e.g. PlayerStateController, EnemyStateController).
    /// </summary>
    [AddComponentMenu("State Controller (BBUnity)"), DefaultExecutionOrder(5)]
    public class StateController : EntityController {

        internal class InvalidAnimatorException : Exception {
            public InvalidAnimatorException() : base("An Animator component is required for the State Controller to function") { }
        }

        /// <summary>
        /// The animation event name auto-fired by this controller once the currently playing
        /// animator state finishes (see UpdateAnimationEndedEvent) — states no longer need a
        /// hand-placed "AnimationEnded" Animation Event at the last frame of every clip. Only
        /// mid-clip signals (e.g. AnimationAllowCancellation, AnimationAllowMovement) still need
        /// one, since those encode a designer-chosen timing window rather than "clip is done".
        /// </summary>
        public const string AnimationEndedEventName = "AnimationEnded";

        /// <summary>
        ///
        /// </summary>
        StateMachine _stateMachine = new StateMachine();

        // Resolved lazily (on first Play() call), not in Awake(): a state machine that never
        // calls PlayAnimation() (e.g. a non-visual entity) shouldn't be forced to have a sibling
        // Animator component just because it has a StateController.
        private Animator _animator;
        private Dictionary<string, bool> _animationEvents = new Dictionary<string, bool>();

        [SerializeField, ReadOnly]
        private string _currentState = "Not Set";

        // Editor-configured states (see StateControllerInspector). This is the preferred way to
        // register states — no subclass/RegisterStates() override required. _defaultState holds
        // a shared reference to whichever entry in _states is marked as the initial state; kept
        // as a reference rather than a list index so it stays correct across reordering.
        [SerializeReference]
        private List<EntityState> _states = new List<EntityState>();

        [SerializeReference]
        private EntityState _defaultState;

        // Registration (AddState) and initialisation (EntityState.Start()) are kept as separate
        // phases: every state a controller will ever have is registered first, and only once
        // RegisterStates() has fully returned do any of their Start() methods run. This means a
        // state's Start() can safely assume every sibling state on this controller already
        // exists, rather than only the ones registered before it.
        private readonly List<EntityState> _registeredStates = new List<EntityState>();
        private string _initialStateKey;

        /// <summary>
        /// Optional extension point for registering states in code, in addition to (or instead
        /// of) the editor-configured list above. Runs after the editor-configured states, so a
        /// state added here with isDefault: true takes precedence over the inspector's default.
        /// </summary>
        protected virtual void RegisterStates() { }

        protected void Start() {
            RegisterSerializedStates();
            RegisterStates();

            foreach (EntityState state in _registeredStates) {
                state.Start();
            }

            if (_initialStateKey != null) {
                _stateMachine.SetState(_initialStateKey, true);
            }
        }

        private void RegisterSerializedStates() {
            foreach (EntityState state in _states) {
                if (state == null) { continue; }

                AddState(state, isDefault: state == _defaultState);
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected void Update() {
            // Runs before the state machine's own Update() so a state reading
            // GetEventBoolean(AnimationEndedEventName) this frame sees this frame's playback
            // progress, not last frame's — otherwise a state entered this same frame (via a
            // transition below) could immediately read "ended" left over from the clip it just
            // replaced.
            UpdateAnimationEndedEvent();

            _stateMachine.Update();

            if (_stateMachine.CurrentState != null) {
                _currentState = _stateMachine.CurrentState.ReferenceKey;
            }
        }

        /// <summary>
        /// Derives AnimationEndedEventName from the Animator's own playback progress instead of
        /// relying on a hand-placed Animation Event at the last frame of every clip — that
        /// placement is easy to get slightly wrong, or forget entirely, per clip. No-ops for a
        /// StateController that has never called Play() (see the comment on _animator below).
        /// </summary>
        private void UpdateAnimationEndedEvent() {
            if (_animator == null) { return; }

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0)) {
                SetEventBooleanTrue(AnimationEndedEventName);
            }
        }

        /// <summary>
        /// Registers a state with this controller. The state's Start() is deferred until every
        /// state registered via RegisterStates() exists, and setState only takes effect (calling
        /// Enter() on the state machine) after all of those Start() calls have completed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <param name="setState"></param>
        public void AddState(string key, EntityState state, bool setState = false) {
            _stateMachine.AddState(key, state);
            state.SetStateController(this);
            _registeredStates.Add(state);

            if (setState) {
                _initialStateKey = key;
            }
        }

        /// <summary>
        /// Registers a state, deriving its key from its own type (see
        /// <see cref="EntityState.Key{T}"/>) instead of requiring a hand-written key constant.
        /// This is the preferred way to register a state — the string-keyed overload above still
        /// exists because the underlying StateMachine (bbunity-state-machines) is key-based, but
        /// callers should not need to think about keys directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state"></param>
        /// <param name="isDefault"></param>
        public void AddState<T>(T state, bool isDefault = false) where T : EntityState {
            AddState(EntityState.KeyFor(state.GetType()), state, isDefault);
        }

        /// <summary>
        /// Forces an immediate transition to the state registered for type T. Unlike
        /// EntityState.SetState&lt;T&gt;() (called from within a state's own Update() to decide its
        /// own next state), this is for external triggers — e.g. a hit reaction that needs to
        /// interrupt whatever the current state's own Update() would otherwise have decided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forceTransition"></param>
        public void SetState<T>(bool forceTransition = true) where T : EntityState {
            _stateMachine.SetState(EntityState.KeyFor(typeof(T)), forceTransition);
        }

        /// <summary>
        /// Transitions to the given state instance, resolving its key from its runtime type. For
        /// callers that only have a runtime EntityState reference rather than a compile-time
        /// type — e.g. a data-driven caller that resolves its target state from configuration —
        /// so SetState&lt;T&gt;() isn't callable. Defaults forceTransition to false (unlike
        /// SetState&lt;T&gt;()): a data-driven caller may re-evaluate and re-target the same state
        /// every frame, and that should be a no-op rather than repeatedly re-entering it.
        /// </summary>
        /// <param name="targetState"></param>
        /// <param name="forceTransition"></param>
        public void SetState(EntityState targetState, bool forceTransition = false) {
            _stateMachine.SetState(EntityState.KeyFor(targetState.GetType()), forceTransition);
        }

        /*
         * Animation Event Functionality
         *
         * States poll these via EntityState (e.g. BaseState in the game layer) to query
         * animation-driven flags and play clips — animation-driven transitions are a state
         * machine concept, so this lives here rather than on the shared EntityController base.
         */

        public void SetEventBooleanTrue(string eventName) {
            _animationEvents[eventName] = true;
        }

        public void SetEventBooleanFalse(string eventName) {
            _animationEvents[eventName] = false;
        }

        public void SetEventBoolean(string eventName, bool booleanValue) {
            _animationEvents[eventName] = booleanValue;
        }

        public bool GetEventBoolean(string eventName) {
            _animationEvents.TryGetValue(eventName, out bool value);

            return value;
        }

        /// <summary>
        /// Utility method for calling 'Play' on the animator instance
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="normalizedTime"></param>
        public void Play(string stateName, int layer = -1, float normalizedTime = 0.0f) {
            GetAnimator().Play(stateName, layer, normalizedTime);
        }

        private Animator GetAnimator() {
            if (_animator == null) {
                _animator = GetComponent<Animator>();
                if (_animator == null) { throw new InvalidAnimatorException(); }
            }

            return _animator;
        }
    }
}