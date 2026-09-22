using System;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Renders an AttributeTypeReference as a dropdown button (reusing the same TypeDropdown and
    /// type scan as AttributeController's own "Add Attribute" button) instead of a raw type-name
    /// string field the developer would have to type correctly by hand.
    /// </summary>
    [CustomPropertyDrawer(typeof(BBUnity.Entities.Controllers.Combat.AttributeTypeReference))]
    public class AttributeTypeReferenceDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty typeNameProperty = property.FindPropertyRelative("_attributeTypeName");

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            Type currentType = string.IsNullOrEmpty(typeNameProperty.stringValue) ? null : Type.GetType(typeNameProperty.stringValue);
            string buttonLabel = currentType != null ? TypeDropdown.BuildLabel(currentType) : "(none)";

            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(buttonLabel), FocusType.Keyboard)) {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), AttributeControllerInspector.GetAvailableAttributeTypes(), type => {
                    typeNameProperty.stringValue = type.AssemblyQualifiedName;
                    typeNameProperty.serializedObject.ApplyModifiedProperties();
                }, "Attribute Type");

                dropdown.Show(fieldRect);
            }
        }
    }
}
