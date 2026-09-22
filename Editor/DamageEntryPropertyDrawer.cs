using UnityEngine;
using UnityEditor;

using BBUnity.Entities.Controllers.Combat;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Lays out a DamageEntry as two rows — Target Attribute (via AttributeTypeReferenceDrawer)
    /// then Damage (via DamageValuePropertyDrawer) — both of which already have their own
    /// registered drawers, so this just positions them.
    /// </summary>
    [CustomPropertyDrawer(typeof(DamageEntry))]
    public class DamageEntryPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty targetAttributeProperty = property.FindPropertyRelative("_targetAttribute");
            SerializedProperty damageValueProperty = property.FindPropertyRelative("_damageValue");

            float height = EditorGUI.GetPropertyHeight(targetAttributeProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(damageValueProperty, true);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty targetAttributeProperty = property.FindPropertyRelative("_targetAttribute");
            SerializedProperty damageValueProperty = property.FindPropertyRelative("_damageValue");

            float y = position.y;

            float targetHeight = EditorGUI.GetPropertyHeight(targetAttributeProperty, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, targetHeight), targetAttributeProperty, new GUIContent("Target Attribute"));
            y += targetHeight + EditorGUIUtility.standardVerticalSpacing;

            float damageHeight = EditorGUI.GetPropertyHeight(damageValueProperty, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, damageHeight), damageValueProperty, new GUIContent("Damage"), true);
        }
    }
}
