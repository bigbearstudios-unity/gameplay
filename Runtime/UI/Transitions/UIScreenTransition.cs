using System;

namespace BBUnity.UI.Transitions {

    /// <summary>
    /// A pluggable strategy for switching a UIScreenController from one UIScreen to another —
    /// picked per controller (or per call) via [SerializeReference], the same pattern
    /// StateController uses for its own list of EntityState. Concrete transitions decide how
    /// (or whether) to animate the handover; UIScreenController itself only knows how to ask for
    /// one to run.
    /// </summary>
    [Serializable]
    public abstract class UIScreenTransition {

        /// <summary>
        /// Switches from <paramref name="from"/> (may be null — nothing was showing yet) to
        /// <paramref name="to"/> (may be null — hiding everything), calling
        /// <paramref name="onComplete"/> once the switch has fully finished.
        /// </summary>
        public abstract void Play(UIScreen from, UIScreen to, Action onComplete);
    }
}
