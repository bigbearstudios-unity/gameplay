using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using BBUnity.Entities.Controllers.Combat;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// "Pick a concrete type, then edit its fields inline" drawer for a single [SerializeReference]
    /// Reduction field (ReductionEntry.Reduction) — mirrors DamageValuePropertyDrawer exactly, one
    /// per polymorphic slot rather than a list.
    /// </summary>
    [CustomPropertyDrawer(typeof(Reduction), useForChildren: true)]
    public class ReductionPropertyDrawer : PropertyDrawer {

        private static List<Type> _cachedTypes;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;

            foreach (SerializedProperty child in EnumerateChildren(property)) {
                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawTypeSelector(headerRect, property, label);

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;
            foreach (SerializedProperty child in EnumerateChildren(property)) {
                float height = EditorGUI.GetPropertyHeight(child, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawTypeSelector(Rect rect, SerializedProperty property, GUIContent label) {
            object instance = property.managedReferenceValue;
            string currentLabel = instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(none)";

            Rect fieldRect = EditorGUI.PrefixLabel(rect, label);

            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(currentLabel), FocusType.Keyboard)) {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), GetAvailableTypes(), type => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                }, "Reduction Type");

                dropdown.Show(fieldRect);
            }
        }

        private static IEnumerable<SerializedProperty> EnumerateChildren(SerializedProperty property) {
            if (property.managedReferenceValue == null) { yield break; }

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }

        private static List<Type> GetAvailableTypes() {
            if (_cachedTypes == null) {
                _cachedTypes = TypeCache.GetTypesDerivedFrom<Reduction>()
                    .Where(t => !t.IsAbstract && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedTypes;
        }
    }
}
