using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using BBUnity.Entities.Controllers.Input;

[CustomEditor(typeof(PlayerUnityInputController))]
public class PlayerUnityInputControllerEditor : Editor {

    private PlayerUnityInputController Controller {
        get { return (PlayerUnityInputController)target; }
    }

    private ReorderableList _mappingsList;
    private SerializedProperty _mappingsProperty;

    private void OnEnable() {
        _mappingsProperty = serializedObject.FindProperty("_actionMappings");

        // displayAddButton is false — new mappings are only added via the "Actions not yet
        // mapped" +Button/+Axis buttons below, which also stamp the action name. A bare "+"
        // here would insert an element with no action name (and, since UnityActionMapping is
        // abstract, nothing valid to instantiate) rather than a usable mapping.
        _mappingsList = new ReorderableList(serializedObject, _mappingsProperty, true, true, false, true);
        _mappingsList.drawHeaderCallback += DrawMappingsHeader;
        _mappingsList.drawElementCallback += DrawElement;
        _mappingsList.elementHeightCallback += GetElementHeight;
    }

    private void OnDisable() {
        if (_mappingsList == null) { return; }

        _mappingsList.drawHeaderCallback -= DrawMappingsHeader;
        _mappingsList.drawElementCallback -= DrawElement;
        _mappingsList.elementHeightCallback -= GetElementHeight;
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        SerializedProperty playerInputProperty = serializedObject.FindProperty("_playerInput");

        EditorGUILayout.PropertyField(playerInputProperty);

        EditorGUILayout.Space();
        _mappingsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        DrawSetupWarnings(playerInputProperty, _mappingsProperty);
        DrawPopulateFromInputActions(playerInputProperty, _mappingsProperty);
    }

    private void DrawMappingsHeader(Rect rect) {
        GUI.Label(rect, "Action Mappings");
    }

    // Draws every visible serialized field of the mapping's concrete type (Action, and
    // whatever else that subclass declares, e.g. Buffer Threshold on a button mapping) —
    // Action is disabled since it's only ever meant to be set via the "Actions not yet
    // mapped" buttons, not typed in by hand.
    private void DrawElement(Rect rect, int index, bool active, bool focused) {
        SerializedProperty element = _mappingsProperty.GetArrayElementAtIndex(index);
        float y = rect.y + EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty iterator = element.Copy();
        SerializedProperty end = iterator.GetEndProperty();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
            enterChildren = false;

            float height = EditorGUI.GetPropertyHeight(iterator, true);
            Rect fieldRect = new Rect(rect.x, y, rect.width, height);

            using (new EditorGUI.DisabledScope(iterator.name == "_action")) {
                EditorGUI.PropertyField(fieldRect, iterator, true);
            }

            y += height + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private float GetElementHeight(int index) {
        SerializedProperty element = _mappingsProperty.GetArrayElementAtIndex(index);
        float height = EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty iterator = element.Copy();
        SerializedProperty end = iterator.GetEndProperty();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
            enterChildren = false;
            height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    /// <summary>
    /// Surfaces the setup problems this controller would otherwise only reveal by throwing
    /// at runtime: a missing Player Input, a Player Input with no actions asset, no mappings
    /// configured at all, or a mapping whose action name doesn't exist in the assigned asset.
    /// </summary>
    private void DrawSetupWarnings(SerializedProperty playerInputProperty, SerializedProperty mappingsProperty) {
        var playerInput = playerInputProperty.objectReferenceValue as UnityEngine.InputSystem.PlayerInput;

        if (playerInput == null) {
            EditorGUILayout.HelpBox("No Player Input component assigned — this controller will throw on Awake().", MessageType.Error);
            return;
        }

        if (playerInput.actions == null) {
            EditorGUILayout.HelpBox("The assigned Player Input has no Input Actions asset set.", MessageType.Error);
            return;
        }

        if (mappingsProperty.arraySize == 0) {
            EditorGUILayout.HelpBox("No action mappings configured.", MessageType.Warning);
            return;
        }

        List<string> problemMappings = new List<string>();
        for (int i = 0; i < mappingsProperty.arraySize; i++) {
            SerializedProperty actionNameProperty = mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_action");
            string actionName = actionNameProperty != null ? actionNameProperty.stringValue : null;

            if (string.IsNullOrEmpty(actionName)) {
                problemMappings.Add("(unnamed mapping)");
            } else if (playerInput.actions.FindAction(actionName) == null) {
                problemMappings.Add(actionName);
            }
        }

        if (problemMappings.Count > 0) {
            EditorGUILayout.HelpBox(
                "These mappings don't match any action in the assigned Input Actions asset: " + string.Join(", ", problemMappings),
                MessageType.Warning
            );
        }
    }

    /// <summary>
    /// Lists actions present in the assigned Player Input's actions asset that don't have a
    /// mapping yet, with a one-click way to add one — you still choose whether it should be a
    /// button or an axis mapping.
    /// </summary>
    private void DrawPopulateFromInputActions(SerializedProperty playerInputProperty, SerializedProperty mappingsProperty) {
        var playerInput = playerInputProperty.objectReferenceValue as UnityEngine.InputSystem.PlayerInput;
        if (playerInput == null || playerInput.actions == null) { return; }

        HashSet<string> mappedActionNames = new HashSet<string>();
        for (int i = 0; i < mappingsProperty.arraySize; i++) {
            SerializedProperty actionNameProperty = mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_action");
            if (actionNameProperty != null && !string.IsNullOrEmpty(actionNameProperty.stringValue)) {
                mappedActionNames.Add(actionNameProperty.stringValue);
            }
        }

        List<string> unmappedActionNames = playerInput.actions
            .Select(action => action.name)
            .Distinct()
            .Where(name => !mappedActionNames.Contains(name))
            .ToList();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions not yet mapped", EditorStyles.boldLabel);

        if (unmappedActionNames.Count == 0) {
            EditorGUILayout.LabelField("All actions mapped");
            return;
        }

        foreach (string actionName in unmappedActionNames) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(actionName);

            if (GUILayout.Button("+ Button", GUILayout.Width(70))) {
                AddMapping(actionName, isButton: true);
            }

            if (GUILayout.Button("+ Axis", GUILayout.Width(70))) {
                AddMapping(actionName, isButton: false);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void AddMapping(string actionName, bool isButton) {
        Undo.RecordObject(Controller, "Add Action Mapping");

        if (isButton) {
            Controller._Editor_AddButtonMapping();
        } else {
            Controller._Editor_AddAxisMapping();
        }

        serializedObject.Update();

        SerializedProperty newMapping = _mappingsProperty.GetArrayElementAtIndex(_mappingsProperty.arraySize - 1);
        SerializedProperty actionNameProperty = newMapping.FindPropertyRelative("_action");
        if (actionNameProperty != null) {
            actionNameProperty.stringValue = actionName;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
