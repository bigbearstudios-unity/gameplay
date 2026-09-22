using UnityEngine;

using BBUnity.Entities.Controllers.Attributes;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// A designer-picked reference to an EntityAttribute type (e.g. HealthAttribute), stored as
    /// its AssemblyQualifiedName and resolved against a specific AttributeController at runtime
    /// via AttributeController.GetAttribute(Type). Used anywhere combat data needs to say "read
    /// this from a named attribute" — see DamageValue/Reduction. Edited via
    /// AttributeTypeReferenceDrawer, which reuses the same type picker as AttributeController's
    /// own "Add Attribute" dropdown.
    /// </summary>
    [System.Serializable]
    public class AttributeTypeReference {

        [SerializeField]
        private string _attributeTypeName;

        public EntityAttribute Resolve(AttributeController controller) {
            System.Type type = ResolveType();
            return type == null ? null : controller.GetAttribute(type);
        }

        /// <summary>
        /// The picked type itself, without resolving it against any particular controller — used
        /// where the type needs to travel from one controller to another (e.g. HitInfo, built by
        /// the attacker but applied by the defender).
        /// </summary>
        public System.Type ResolveType() {
            if (string.IsNullOrEmpty(_attributeTypeName)) { return null; }

            System.Type type = System.Type.GetType(_attributeTypeName);
            if (type == null) {
                Debug.LogWarning($"AttributeTypeReference could not resolve '{_attributeTypeName}' — the attribute type may have been renamed or removed since this was picked. Re-pick it in the Inspector.");
            }

            return type;
        }
    }
}
