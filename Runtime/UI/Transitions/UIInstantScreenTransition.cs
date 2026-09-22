using System;

namespace BBUnity.UI.Transitions {

    /// <summary>
    /// No animation — hides the previous screen and shows the new one on the same frame. The
    /// default transition for a UIScreenController that hasn't been given anything fancier.
    /// </summary>
    [Serializable]
    public class UIInstantScreenTransition : UIScreenTransition {

        public override void Play(UIScreen from, UIScreen to, Action onComplete) {
            if (from != null) { from.Hide(); }
            if (to != null) { to.Show(); }

            onComplete?.Invoke();
        }
    }
}
