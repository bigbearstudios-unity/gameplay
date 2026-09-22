using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;

using BBUnity.Entities.Controllers.States;

namespace BBUnity.Entities.Controllers.EditorTools {

    [CustomEditor(typeof(StateController), editorForChildClasses: true)]
    public class StateControllerInspector : UnityEditor.Editor {

        private static List<Type> _cachedStateTypes;

        private ReorderableList _list;
        private SerializedProperty _statesProperty;
        private SerializedProperty _defaultStateProperty;

        private void OnEnable() {
            _statesProperty = serializedObject.FindProperty("_states");
            _defaultStateProperty = serializedObject.FindProperty("_defaultState");

            _list = new ReorderableList(serializedObject, _statesProperty, true, true, false, true);
            _list.drawHeaderCallback += DrawHeader;
            _list.drawElementCallback += DrawElement;
            _list.elementHeightCallback += GetElementHeight;
            _list.onRemoveCallback += RemoveElement;
        }

        private void OnDisable() {
            if (_list == null) { return; }

            _list.drawHeaderCallback -= DrawHeader;
            _list.drawElementCallback -= DrawElement;
            _list.elementHeightCallback -= GetElementHeight;
            _list.onRemoveCallback -= RemoveElement;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_states", "_defaultState", "m_Script");

            EditorGUILayout.Space();
            _list.DoLayoutList();
            DrawDefaultStateSelector();
            DrawAddStateButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(Rect rect) {
            GUI.Label(rect, "States");
        }

        private float GetElementHeight(int index) {
            return EditorGUIUtility.singleLineHeight + 4;
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused) {
            SerializedProperty element = _statesProperty.GetArrayElementAtIndex(index);
            EntityState instance = element.managedReferenceValue as EntityState;

            var typeRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
            string label = instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(missing state)";
            EditorGUI.LabelField(typeRect, label, EditorStyles.boldLabel);
        }

        // Single-selection dropdown listing every registered state, replacing what was previously
        // a per-row "Default" toggle. A dropdown makes the exclusivity (only one default state)
        // structural rather than something the user has to maintain by hand across rows.
        private void DrawDefaultStateSelector() {
            List<EntityState> instances = GetRegisteredStateInstances();
            string[] labels = new string[instances.Count + 1];
            labels[0] = "(None)";

            int currentIndex = 0;
            for (int i = 0; i < instances.Count; i++) {
                labels[i + 1] = TypeDropdown.BuildLabel(instances[i].GetType());
                if (ReferenceEquals(instances[i], _defaultStateProperty.managedReferenceValue)) {
                    currentIndex = i + 1;
                }
            }

            int newIndex = EditorGUILayout.Popup("Default State", currentIndex, labels);
            if (newIndex != currentIndex) {
                _defaultStateProperty.managedReferenceValue = newIndex == 0 ? null : instances[newIndex - 1];
            }
        }

        private List<EntityState> GetRegisteredStateInstances() {
            var instances = new List<EntityState>();

            for (int i = 0; i < _statesProperty.arraySize; i++) {
                EntityState instance = _statesProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityState;
                if (instance != null) { instances.Add(instance); }
            }

            return instances;
        }

        private void RemoveElement(ReorderableList list) {
            SerializedProperty element = _statesProperty.GetArrayElementAtIndex(list.index);
            object removedInstance = element.managedReferenceValue;

            if (ReferenceEquals(_defaultStateProperty.managedReferenceValue, removedInstance)) {
                _defaultStateProperty.managedReferenceValue = null;
            }

            _statesProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void DrawAddStateButton() {
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Add State"), GUI.skin.button);
            if (!GUI.Button(buttonRect, "Add State")) { return; }

            List<Type> availableTypes = GetAvailableStateTypes().Where(t => !ContainsStateOfType(t)).ToList();
            var dropdown = new TypeDropdown(new AdvancedDropdownState(), availableTypes, AddState, "State Type");
            dropdown.Show(buttonRect);
        }

        private void AddState(Type type) {
            // Mirrors the one-state-per-type constraint StateController.AddState already enforces
            // at runtime (keys are derived from the state's type, and registering two states under
            // the same key throws) — catching it here means a duplicate is rejected at edit time
            // rather than only surfacing as an exception when the scene is played.
            if (ContainsStateOfType(type)) {
                Debug.LogError($"'{target.name}' already has a state of type '{TypeDropdown.BuildLabel(type)}' — only one instance per state type is supported.", target);
                return;
            }

            var instance = Activator.CreateInstance(type) as EntityState;

            serializedObject.Update();
            _statesProperty.arraySize++;
            SerializedProperty newElement = _statesProperty.GetArrayElementAtIndex(_statesProperty.arraySize - 1);
            newElement.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
        }

        private bool ContainsStateOfType(Type type) {
            for (int i = 0; i < _statesProperty.arraySize; i++) {
                EntityState existing = _statesProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityState;
                if (existing != null && existing.GetType() == type) { return true; }
            }

            return false;
        }

        private static List<Type> GetAvailableStateTypes() {
            if (_cachedStateTypes == null) {
                _cachedStateTypes = TypeCache.GetTypesDerivedFrom<EntityState>()
                    .Where(t => !t.IsAbstract && !t.IsNested && !IsFromTestAssembly(t) && HasParameterlessConstructor(t))
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedStateTypes;
        }

        private static bool HasParameterlessConstructor(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        // Excludes test-double state classes (e.g. StateControllerTests' private nested stubs)
        // from the picker. Nested types are already caught by !t.IsNested above — this also
        // catches any future test double declared as a top-level type in a *.Tests assembly.
        private static bool IsFromTestAssembly(Type type) {
            return type.Assembly.GetName().Name.EndsWith(".Tests", StringComparison.Ordinal);
        }
    }
}
