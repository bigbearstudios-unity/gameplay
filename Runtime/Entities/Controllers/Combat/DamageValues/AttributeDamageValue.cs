using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// The attacker's raw current value of a chosen attribute — e.g. "Strength" dealing damage
    /// equal to its current value.
    /// </summary>
    [System.Serializable, System.ComponentModel.DisplayName("Attribute Damage")]
    public class AttributeDamageValue : DamageValue {

        [SerializeField]
        private AttributeTypeReference _attribute;

        public override int Resolve(AttributeController attacker) {
            return _attribute?.Resolve(attacker)?.NumericValue ?? 0;
        }
    }
}
