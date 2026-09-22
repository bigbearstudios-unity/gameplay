using UnityEngine;

using System.Collections.Generic;

using TMPro;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// Scene-level controller for popping up floating world-space text (damage numbers, pickup
    /// callouts, ...) via either the engine's built-in TextMesh or TextMeshPro, picked per
    /// instance of this controller rather than per call. Renderer-agnostic pooling/movement/fade
    /// logic lives here; a subclass (see FloatingText2DController) only needs to fill in whatever
    /// is specific to its dimension — e.g. 2D sorting layer/order — via ConfigureInstanceRenderer.
    ///
    /// Deliberately self-contained rather than built on BBUnity.Pools.ObjectPool: instances are
    /// created procedurally (no prefab to author before this works), and pooled with a plain
    /// queue.
    /// </summary>
    public abstract class FloatingTextController : MonoBehaviour {

        [SerializeField, Tooltip("Which text component spawned instances use. See the inspector warning if TextMeshPro isn't set up in this project.")]
        private FloatingTextRendererType _rendererType = FloatingTextRendererType.Unity;

        [Header("Font")]

        [SerializeField, Tooltip("Used when Renderer Type is Unity.")]
        private Font _unityFont;

        [SerializeField, Tooltip("Used when Renderer Type is TextMeshPro. Falls back to TMP Settings' default font asset if left unassigned.")]
        private TMP_FontAsset _tmpFontAsset;

        [Header("Size")]

        [SerializeField, Tooltip("TextMesh's characterSize (world-space scale) — used when Renderer Type is Unity.")]
        private float _unityCharacterSize = 0.2f;

        [SerializeField, Tooltip("TextMeshPro's fontSize — used when Renderer Type is TextMeshPro.")]
        private float _tmpFontSize = 36f;

        [Header("Movement / Fade")]

        [SerializeField]
        private Color _defaultColor = Color.white;

        [SerializeField, Tooltip("World units per second this text drifts upward.")]
        private float _moveSpeed = 1f;

        [SerializeField, Tooltip("Total seconds a floating text instance exists for before returning to the pool.")]
        private float _lifetime = 1.5f;

        [SerializeField, Tooltip("How many of those seconds, at the end of its lifetime, it spends fading out.")]
        private float _fadeDuration = 0.5f;

        private readonly Queue<FloatingTextInstance> _pool = new Queue<FloatingTextInstance>();

        public void Show(string text, Vector3 worldPosition) {
            Show(text, worldPosition, _defaultColor);
        }

        public void Show(string text, Vector3 worldPosition, Color color) {
            FloatingTextInstance instance = GetOrCreateInstance();
            float size = _rendererType == FloatingTextRendererType.TextMeshPro ? _tmpFontSize : _unityCharacterSize;

            instance.Show(text, worldPosition, color, size, _moveSpeed, _lifetime, _fadeDuration);
        }

        internal void ReturnToPool(FloatingTextInstance instance) {
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }

        /// <summary>
        /// Extension point for whatever a subclass's dimension needs on a freshly-created
        /// instance before it's ever shown — e.g. FloatingText2DController assigning a sorting
        /// layer/order to the renderer's MeshRenderer so it draws correctly against sprites.
        /// </summary>
        protected virtual void ConfigureInstanceRenderer(GameObject instance) { }

        private FloatingTextInstance GetOrCreateInstance() {
            while (_pool.Count > 0) {
                FloatingTextInstance pooled = _pool.Dequeue();
                if (pooled != null) { return pooled; }
            }

            return CreateInstance();
        }

        private FloatingTextInstance CreateInstance() {
            GameObject instance = new GameObject("FloatingText");
            instance.SetActive(false);

            IFloatingTextRenderer renderer = _rendererType == FloatingTextRendererType.TextMeshPro
                ? CreateTextMeshProRenderer(instance)
                : CreateUnityTextRenderer(instance);

            ConfigureInstanceRenderer(instance);

            FloatingTextInstance floatingText = instance.AddComponent<FloatingTextInstance>();
            floatingText.Initialize(renderer, this);

            return floatingText;
        }

        private IFloatingTextRenderer CreateUnityTextRenderer(GameObject instance) {
            TextMesh textMesh = instance.AddComponent<TextMesh>();
            textMesh.font = _unityFont;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // TextMesh renders nothing without a material assigned to its MeshRenderer — this is
            // the classic gotcha with creating one procedurally instead of via the Editor's
            // "3D Object > Text" menu, which sets this up for you.
            if (_unityFont != null) {
                MeshRenderer meshRenderer = instance.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = _unityFont.material;
            } else {
                Debug.LogWarning($"FloatingTextController on '{gameObject.name}' has Renderer Type Unity but no Font assigned — spawned text will be invisible (TextMesh needs a font's material to render).", this);
            }

            return instance.AddComponent<UnityFloatingTextRenderer>();
        }

        private IFloatingTextRenderer CreateTextMeshProRenderer(GameObject instance) {
            TextMeshPro text = instance.AddComponent<TextMeshPro>();
            TMP_FontAsset font = _tmpFontAsset != null ? _tmpFontAsset : TMP_Settings.defaultFontAsset;

            if (font == null) {
                Debug.LogWarning($"FloatingTextController on '{gameObject.name}' has Renderer Type TextMeshPro but no Font Asset assigned, and TMP Settings has no default font asset either — spawned text will have no font.", this);
            }

            text.font = font;
            text.alignment = TextAlignmentOptions.Center;

            return instance.AddComponent<TextMeshProFloatingTextRenderer>();
        }
    }
}
