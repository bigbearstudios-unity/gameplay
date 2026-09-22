using System;

namespace BBUnity.UI.Transitions {

    /// <summary>
    /// Fades the previous screen out, then fades the new one in — the previous screen is fully
    /// gone before the new one starts appearing, rather than the two cross-fading simultaneously.
    /// </summary>
    [Serializable]
    public class UIFadeScreenTransition : UIScreenTransition {

        public override void Play(UIScreen from, UIScreen to, Action onComplete) {
            if (from != null) {
                from.FadeOut(() => ShowTo(to, onComplete));
            } else {
                ShowTo(to, onComplete);
            }
        }

        private void ShowTo(UIScreen to, Action onComplete) {
            if (to != null) {
                to.FadeIn(onComplete);
            } else {
                onComplete?.Invoke();
            }
        }
    }
}
