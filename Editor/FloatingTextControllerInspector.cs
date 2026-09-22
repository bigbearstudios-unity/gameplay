using UnityEngine;
using UnityEditor;

using TMPro;

using BBUnity.Combat.FloatingText;

namespace BBUnity.Combat.FloatingText.EditorTools {

    /// <summary>
    /// Adds the "TextMeshPro isn't available" warning on top of the default inspector — checked
    /// via TMP_Settings.instance, the same signal Unity's own TextMeshPro tooling uses to decide
    /// whether TMP Essentials have been imported into the project.
    /// </summary>
    [CustomEditor(typeof(FloatingTextController), editorForChildClasses: true)]
    public class FloatingTextControllerInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            SerializedProperty rendererTypeProperty = serializedObject.FindProperty("_rendererType");
            var rendererType = (FloatingTextRendererType)rendererTypeProperty.enumValueIndex;

            if (rendererType == FloatingTextRendererType.TextMeshPro && TMP_Settings.instance == null) {
                EditorGUILayout.HelpBox(
                    "TextMeshPro is not available in this project — import TMP Essentials (Window > TextMeshPro > Import TMP Essential Resources), or switch Renderer Type to Unity.",
                    MessageType.Warning
                );
            }
        }
    }
}
