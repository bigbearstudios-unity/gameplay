using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using BBUnity.Entities.Controllers.Combat;

namespace BBUnity.Entities.Controllers.EditorTools {

    /// <summary>
    /// Replaces the default "Element N" label on each Attacks list entry with its Id, and warns
    /// inline if its AttackCollider prefab isn't set — PerformAttack() no-ops (with its own
    /// runtime warning) without one, so it's worth catching at edit time too. DamageEntries is
    /// drawn via Unity's default list rendering — DamageEntryPropertyDrawer already gives each of
    /// its own entries a sensible label, and it's a plain (non-[SerializeReference]) list so the
    /// default "+"/"-" buttons work without the abstract-type problem SerializeReference lists have.
    ///
    /// AttackCollider is picked via a custom dropdown rather than Unity's default Object field —
    /// since AttackCollider is abstract (see AttackCollider2D), Unity's built-in Object Picker
    /// window is unreliable at finding prefabs whose only matching component is a concrete
    /// subclass of an abstract Component type (drag-and-drop still works; the picker's own search
    /// doesn't). Same fix as EnemySpawnNodeInspector's Enemy dropdown, and for the same reason.
    /// </summary>
    [CustomPropertyDrawer(typeof(Attack))]
    public class AttackPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty attackColliderProperty = property.FindPropertyRelative("_attackColliderPrefab");
            SerializedProperty damageEntriesProperty = property.FindPropertyRelative("_damageEntries");

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Id
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // AttackCollider prefab

            if (attackColliderProperty.objectReferenceValue == null) {
                height += MissingAttackColliderHelpBoxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Spawn position
            height += EditorGUI.GetPropertyHeight(damageEntriesProperty, true);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty idProperty = property.FindPropertyRelative("_id");
            SerializedProperty attackColliderProperty = property.FindPropertyRelative("_attackColliderPrefab");
            SerializedProperty spawnPositionProperty = property.FindPropertyRelative("_spawnPosition");
            SerializedProperty damageEntriesProperty = property.FindPropertyRelative("_damageEntries");

            float y = position.y;

            GUIContent rowLabel = new GUIContent(string.IsNullOrEmpty(idProperty.stringValue) ? "(Unnamed Attack)" : idProperty.stringValue);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), idProperty, rowLabel);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            DrawAttackColliderDropdown(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), attackColliderProperty);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (attackColliderProperty.objectReferenceValue == null) {
                Rect helpBoxRect = new Rect(position.x, y, position.width, MissingAttackColliderHelpBoxHeight);
                EditorGUI.HelpBox(helpBoxRect, "No AttackCollider prefab set — this attack will do nothing when performed.", MessageType.Warning);
                y += MissingAttackColliderHelpBoxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), spawnPositionProperty, new GUIContent("Spawn Position"));
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            float damageEntriesHeight = EditorGUI.GetPropertyHeight(damageEntriesProperty, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, damageEntriesHeight), damageEntriesProperty, new GUIContent("Damage Entries"), true);
        }

        private static float MissingAttackColliderHelpBoxHeight {
            get { return EditorGUIUtility.singleLineHeight * 1.5f; }
        }

        private void DrawAttackColliderDropdown(Rect rect, SerializedProperty attackColliderProperty) {
            AttackCollider current = attackColliderProperty.objectReferenceValue as AttackCollider;
            string label = current != null ? current.gameObject.name : "(none)";

            Rect fieldRect = EditorGUI.PrefixLabel(rect, new GUIContent("Attack Collider"));

            if (!EditorGUI.DropdownButton(fieldRect, new GUIContent(label), FocusType.Keyboard)) { return; }

            List<AttackCollider> available = FindAvailableAttackColliderPrefabs();
            GenericMenu menu = new GenericMenu();

            if (available.Count == 0) {
                menu.AddDisabledItem(new GUIContent("(no AttackCollider prefabs found in the project)"));
            }

            foreach (AttackCollider attackCollider in available) {
                menu.AddItem(new GUIContent(attackCollider.gameObject.name), attackCollider == current, () => {
                    attackColliderProperty.objectReferenceValue = attackCollider;
                    attackColliderProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private static List<AttackCollider> FindAvailableAttackColliderPrefabs() {
            var results = new List<AttackCollider>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                AttackCollider attackCollider = prefab != null ? prefab.GetComponent<AttackCollider>() : null;

                if (attackCollider != null) { results.Add(attackCollider); }
            }

            return results.OrderBy(attackCollider => attackCollider.gameObject.name).ToList();
        }
    }
}
