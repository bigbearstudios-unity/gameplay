using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// A percentage of the attacker's current value of a chosen attribute — e.g. Strength 100 at
    /// 50% deals 50 damage.
    /// </summary>
    [System.Serializable, System.ComponentModel.DisplayName("Attribute Percentage Damage")]
    public class AttributePercentageDamageValue : DamageValue {

        [SerializeField]
        private AttributeTypeReference _attribute;

        [SerializeField, Range(0f, 1f)]
        private float _percentage = 1f;

        public override int Resolve(AttributeController attacker) {
            int? value = _attribute?.Resolve(attacker)?.NumericValue;
            return value == null ? 0 : Mathf.RoundToInt(value.Value * _percentage);
        }
    }
}
