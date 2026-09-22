using UnityEngine;
using UnityEditor;

using BBUnity.Entities.Controllers.Combat;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Lays out a ReductionEntry as two rows — Target Attribute (via AttributeTypeReferenceDrawer)
    /// then Reduction (via ReductionPropertyDrawer) — both of which already have their own
    /// registered drawers, so this just positions them.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReductionEntry))]
    public class ReductionEntryPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty targetAttributeProperty = property.FindPropertyRelative("_targetAttribute");
            SerializedProperty reductionProperty = property.FindPropertyRelative("_reduction");

            float height = EditorGUI.GetPropertyHeight(targetAttributeProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(reductionProperty, true);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty targetAttributeProperty = property.FindPropertyRelative("_targetAttribute");
            SerializedProperty reductionProperty = property.FindPropertyRelative("_reduction");

            float y = position.y;

            float targetHeight = EditorGUI.GetPropertyHeight(targetAttributeProperty, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, targetHeight), targetAttributeProperty, new GUIContent("Target Attribute"));
            y += targetHeight + EditorGUIUtility.standardVerticalSpacing;

            float reductionHeight = EditorGUI.GetPropertyHeight(reductionProperty, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, reductionHeight), reductionProperty, new GUIContent("Reduction"), true);
        }
    }
}
