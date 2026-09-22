using UnityEngine;

using System;
using System.Collections;

namespace BBUnity.UI {

    /// <summary>
    /// A show/hide/fade wrapper around a CanvasGroup — the generic building block for any UI
    /// screen that needs to fade in/out (a splash screen, a menu, a dialog, ...), not tied to any
    /// specific flow. Disables interactable/blocksRaycasts while fading or hidden so a
    /// mid-fade screen can't be clicked through. See UIScreenController for switching between a
    /// named set of these.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour {

        [SerializeField, Tooltip("Seconds a FadeIn()/FadeOut() takes.")]
        private float _fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show() {
            StopAllCoroutines();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            SetInteractable(true);
        }

        public void Hide() {
            StopAllCoroutines();
            SetInteractable(false);
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void FadeIn(Action onComplete = null) {
            gameObject.SetActive(true);
            Fade(_canvasGroup.alpha, 1f, () => {
                SetInteractable(true);
                onComplete?.Invoke();
            });
        }

        public void FadeOut(Action onComplete = null) {
            SetInteractable(false);
            Fade(_canvasGroup.alpha, 0f, () => {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void SetInteractable(bool interactable) {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
        }

        private void Fade(float from, float to, Action onComplete) {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(from, to, onComplete));
        }

        private IEnumerator FadeRoutine(float from, float to, Action onComplete) {
            float elapsed = 0f;

            while (elapsed < _fadeDuration) {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, _fadeDuration > 0f ? elapsed / _fadeDuration : 1f);
                yield return null;
            }

            _canvasGroup.alpha = to;
            onComplete?.Invoke();
        }
    }
}
