using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// One "mitigate incoming damage to this attribute, using this reduction" entry on an
    /// AttributeBasedCombatController's Reductions list — pick the target attribute, then a
    /// Reduction (Flat / Percentage). Two entries can legitimately target the same attribute
    /// (e.g. a Flat and a Percentage reduction both applying to Health), and
    /// AttributeBasedCombatController.MitigateDamage() combines whichever entries match the
    /// attribute actually being damaged.
    /// </summary>
    [System.Serializable]
    public class ReductionEntry {

        [SerializeField]
        private AttributeTypeReference _targetAttribute;

        [SerializeReference]
        private Reduction _reduction;

        public System.Type TargetAttributeType {
            get { return _targetAttribute?.ResolveType(); }
        }

        public Reduction Reduction {
            get { return _reduction; }
        }
    }
}
