using UnityEngine;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// Per-instance behaviour for one spawned floating text — drifts upward and fades out over
    /// its lifetime, then returns itself to the owning controller's pool. Renderer-agnostic: talks
    /// to whichever IFloatingTextRenderer adapter it was given, never the concrete text component.
    /// </summary>
    [AddComponentMenu("")]
    internal class FloatingTextInstance : MonoBehaviour {

        private IFloatingTextRenderer _renderer;
        private FloatingTextController _owner;

        private Color _baseColor;
        private float _moveSpeed;
        private float _lifetime;
        private float _fadeDuration;
        private float _age;

        public void Initialize(IFloatingTextRenderer renderer, FloatingTextController owner) {
            _renderer = renderer;
            _owner = owner;
        }

        public void Show(string text, Vector3 worldPosition, Color color, float size, float moveSpeed, float lifetime, float fadeDuration) {
            transform.position = worldPosition;

            _baseColor = color;
            _moveSpeed = moveSpeed;
            _lifetime = lifetime;
            _fadeDuration = fadeDuration;
            _age = 0f;

            _renderer.Text = text;
            _renderer.Size = size;
            _renderer.Color = _baseColor;

            gameObject.SetActive(true);
        }

        private void Update() {
            float deltaTime = Time.deltaTime;
            _age += deltaTime;

            transform.position += Vector3.up * (_moveSpeed * deltaTime);

            float fadeElapsed = _age - (_lifetime - _fadeDuration);
            if (fadeElapsed > 0f && _fadeDuration > 0f) {
                float alpha = 1f - Mathf.Clamp01(fadeElapsed / _fadeDuration);
                _renderer.Color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * alpha);
            }

            if (_age >= _lifetime) {
                _owner.ReturnToPool(this);
            }
        }
    }
}
