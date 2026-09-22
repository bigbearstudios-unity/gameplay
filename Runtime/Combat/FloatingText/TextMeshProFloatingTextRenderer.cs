using UnityEngine;
using TMPro;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// Adapts TMPro.TextMeshPro (the non-UGUI, world-space variant — not TextMeshProUGUI, which
    /// needs a Canvas) to IFloatingTextRenderer.
    /// </summary>
    [AddComponentMenu(""), RequireComponent(typeof(TextMeshPro))]
    internal class TextMeshProFloatingTextRenderer : MonoBehaviour, IFloatingTextRenderer {

        private TextMeshPro _text;

        private void Awake() {
            _text = GetComponent<TextMeshPro>();
        }

        public string Text {
            get { return _text.text; }
            set { _text.text = value; }
        }

        public Color Color {
            get { return _text.color; }
            set { _text.color = value; }
        }

        public float Size {
            get { return _text.fontSize; }
            set { _text.fontSize = value; }
        }
    }
}
