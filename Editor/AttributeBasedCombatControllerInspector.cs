using UnityEditor;

namespace BBUnity.Entities.Controllers.EditorTools {

    // Inherits AttributeControllerInspector rather than duplicating it, so
    // AttributeBasedCombatController still gets the exact same "Add Attribute" list/button
    // AttributeController itself has (the auto-seeded Health entry included) — a separate
    // from-scratch editor here previously meant that list fell back to Unity's plain default
    // rendering ("Element 0" instead of the type name).
    //
    // Attacks and Reductions are both plain (non-[SerializeReference]) lists now — Attack and
    // ReductionEntry aren't polymorphic themselves, only fields inside them are, and those already
    // have their own registered drawers (AttackPropertyDrawer, ReductionEntryPropertyDrawer, and
    // the DamageValue/Reduction/AttributeTypeReference drawers they in turn rely on) — so Unity's
    // default list rendering (default "+"/"-", no abstract-type problem) is all that's needed here.
    [CustomEditor(typeof(AttributeBasedCombatController), editorForChildClasses: true)]
    public class AttributeBasedCombatControllerInspector : AttributeControllerInspector {

        private SerializedProperty _attacksProperty;
        private SerializedProperty _reductionsProperty;

        protected override void OnEnable() {
            base.OnEnable();

            _attacksProperty = serializedObject.FindProperty("_attacks");
            _reductionsProperty = serializedObject.FindProperty("_reductions");
        }

        protected override string[] GetExcludedProperties() {
            return new[] { "_entityAttributes", "_attacks", "_reductions", "m_Script" };
        }

        protected override void DrawAdditionalContent() {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attacksProperty, true);
            // Attacks -> DamageEntries -> DamageValue are all nested under this one property, so
            // a single recursive pass from the root catches (and repairs) aliasing anywhere in
            // that chain — see ManagedReferenceDeduplicator for why this is needed at all.
            ManagedReferenceDeduplicator.Deduplicate(_attacksProperty);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_reductionsProperty, true);
            ManagedReferenceDeduplicator.Deduplicate(_reductionsProperty);
        }
    }
}
