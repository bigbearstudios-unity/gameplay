using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

using BBUnity.Scenes.Controllers;

namespace BBUnity.Scenes.Controllers.EditorTools {

    /// <summary>
    /// Renders a SceneReference as a dropdown of the scenes currently enabled in Build Settings —
    /// that's deliberate, not just "every scene in the project": a scene disabled or missing from
    /// Build Settings can't actually be loaded at runtime via SceneManager, so offering it here
    /// would just be a trap.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("_sceneName");

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            string current = string.IsNullOrEmpty(sceneNameProperty.stringValue) ? "(none)" : sceneNameProperty.stringValue;
            if (!EditorGUI.DropdownButton(fieldRect, new GUIContent(current), FocusType.Keyboard)) { return; }

            string[] sceneNames = GetBuildSettingsSceneNames();
            GenericMenu menu = new GenericMenu();

            if (sceneNames.Length == 0) {
                menu.AddDisabledItem(new GUIContent("(no scenes enabled in Build Settings)"));
            }

            foreach (string sceneName in sceneNames) {
                menu.AddItem(new GUIContent(sceneName), sceneName == sceneNameProperty.stringValue, () => {
                    sceneNameProperty.stringValue = sceneName;
                    sceneNameProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        internal static string[] GetBuildSettingsSceneNames() {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();
        }

        internal static string GetScenePath(string sceneName) {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (scene.enabled && Path.GetFileNameWithoutExtension(scene.path) == sceneName) {
                    return scene.path;
                }
            }

            return null;
        }
    }
}
