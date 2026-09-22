using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;

using BBUnity.Entities.Controllers.Attributes;

namespace BBUnity.Entities.Controllers.EditorTools {

    [CustomEditor(typeof(AttributeController), editorForChildClasses: true)]
    public class AttributeControllerInspector : UnityEditor.Editor {

        private static List<Type> _cachedAttributeTypes;

        private ReorderableList _list;
        private SerializedProperty _attributesProperty;

        // protected virtual, not private: Unity's Editor OnEnable/OnDisable lookup does not call
        // every same-named method up an Editor subclass chain the way MonoBehaviour's does — a
        // subclass (see AttributeBasedCombatControllerInspector) with its own private OnEnable
        // would silently shadow this one, leaving _list null. A subclass must override and call
        // base.OnEnable()/base.OnDisable() explicitly.
        protected virtual void OnEnable() {
            _attributesProperty = serializedObject.FindProperty("_entityAttributes");

            _list = new ReorderableList(serializedObject, _attributesProperty, true, true, false, true);
            _list.drawHeaderCallback += DrawHeader;
            _list.drawElementCallback += DrawElement;
            _list.elementHeightCallback += GetElementHeight;
        }

        protected virtual void OnDisable() {
            if (_list == null) { return; }

            _list.drawHeaderCallback -= DrawHeader;
            _list.drawElementCallback -= DrawElement;
            _list.elementHeightCallback -= GetElementHeight;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, GetExcludedProperties());

            EditorGUILayout.Space();
            _list.DoLayoutList();
            DrawAddAttributeButton();

            DrawAdditionalContent();

            serializedObject.ApplyModifiedProperties();
        }

        // Overridden by AttributeBasedCombatControllerInspector to also exclude its own fields
        // (Attacks, Reductions) from the default property drawing below, so they aren't drawn
        // twice — once here, once by DrawAdditionalContent().
        protected virtual string[] GetExcludedProperties() {
            return new[] { "_entityAttributes", "m_Script" };
        }

        // Extension point for a subclass's own inspector content (see
        // AttributeBasedCombatControllerInspector) — kept inside the same Update()/ApplyModifiedProperties()
        // bracket as everything else here rather than requiring a second one.
        protected virtual void DrawAdditionalContent() { }

        private void DrawHeader(Rect rect) {
            GUI.Label(rect, "Attributes");
        }

        // Unlike StateControllerInspector's states (pure logic, no data), attributes carry
        // editable data (the boxed Health/Mana/Poise fields, plus the Slider) — so every visible
        // field of the concrete type is drawn here, fully editable, exactly as it would be if it
        // were still a direct [SerializeField] on the component.
        private void DrawElement(Rect rect, int index, bool active, bool focused) {
            SerializedProperty element = _attributesProperty.GetArrayElementAtIndex(index);
            float y = rect.y + EditorGUIUtility.standardVerticalSpacing;

            EntityAttribute instance = element.managedReferenceValue as EntityAttribute;
            string label = instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(missing attribute)";
            EditorGUI.LabelField(new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty iterator = element.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                enterChildren = false;

                float height = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), iterator, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private float GetElementHeight(int index) {
            SerializedProperty element = _attributesProperty.GetArrayElementAtIndex(index);
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

            SerializedProperty iterator = element.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                enterChildren = false;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        private void DrawAddAttributeButton() {
            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent("Add Attribute"), GUI.skin.button);
            if (!GUI.Button(buttonRect, "Add Attribute")) { return; }

            List<Type> availableTypes = GetAvailableAttributeTypes().Where(t => !ContainsAttributeOfType(t)).ToList();
            var dropdown = new TypeDropdown(new AdvancedDropdownState(), availableTypes, AddAttribute, "Attribute Type");
            dropdown.Show(buttonRect);
        }

        private void AddAttribute(Type type) {
            // Mirrors the one-attribute-per-type constraint below — a second Health entry would
            // just shadow the first one via GetAttribute<T>() without this check.
            if (ContainsAttributeOfType(type)) {
                Debug.LogError($"'{target.name}' already has an attribute of type '{TypeDropdown.BuildLabel(type)}' — only one instance per attribute type is supported.", target);
                return;
            }

            var instance = Activator.CreateInstance(type) as EntityAttribute;

            serializedObject.Update();
            _attributesProperty.arraySize++;
            SerializedProperty newElement = _attributesProperty.GetArrayElementAtIndex(_attributesProperty.arraySize - 1);
            newElement.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
        }

        private bool ContainsAttributeOfType(Type type) {
            for (int i = 0; i < _attributesProperty.arraySize; i++) {
                EntityAttribute existing = _attributesProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityAttribute;
                if (existing != null && existing.GetType() == type) { return true; }
            }

            return false;
        }

        // Internal rather than private: AttributeTypeReferenceDrawer reuses this same scan for
        // its "pick an attribute type" dropdown (DamageValue/Reduction), rather than repeating
        // the TypeCache query.
        internal static List<Type> GetAvailableAttributeTypes() {
            if (_cachedAttributeTypes == null) {
                _cachedAttributeTypes = TypeCache.GetTypesDerivedFrom<EntityAttribute>()
                    .Where(t => !t.IsAbstract && !t.IsNested && HasParameterlessConstructor(t))
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedAttributeTypes;
        }

        private static bool HasParameterlessConstructor(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
