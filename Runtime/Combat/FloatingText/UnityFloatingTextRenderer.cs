using UnityEngine;

namespace BBUnity.Combat.FloatingText {

    /// <summary>
    /// Adapts the engine's built-in TextMesh to IFloatingTextRenderer. Size maps to characterSize
    /// (TextMesh's actual world-space scale control) rather than fontSize (which is texture
    /// rasterization resolution, not the visual size).
    /// </summary>
    [AddComponentMenu(""), RequireComponent(typeof(TextMesh))]
    internal class UnityFloatingTextRenderer : MonoBehaviour, IFloatingTextRenderer {

        private TextMesh _textMesh;

        private void Awake() {
            _textMesh = GetComponent<TextMesh>();
        }

        public string Text {
            get { return _textMesh.text; }
            set { _textMesh.text = value; }
        }

        public Color Color {
            get { return _textMesh.color; }
            set { _textMesh.color = value; }
        }

        public float Size {
            get { return _textMesh.characterSize; }
            set { _textMesh.characterSize = value; }
        }
    }
}
