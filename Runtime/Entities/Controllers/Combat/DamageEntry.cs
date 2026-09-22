using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// One "damage this attribute, using this damage type" entry on an Attack — pick the target
    /// attribute, then a DamageValue (Static Damage / Static Range Damage / Attribute Damage /
    /// Attribute Percentage Damage), then whatever values that type needs. An Attack can carry any
    /// number of these, so a single attack can damage Health, Poise, or any future attribute, all
    /// from the same list rather than a fixed field per known attribute.
    /// </summary>
    [System.Serializable]
    public class DamageEntry {

        [SerializeField]
        private AttributeTypeReference _targetAttribute;

        [SerializeReference]
        private DamageValue _damageValue;

        public System.Type TargetAttributeType {
            get { return _targetAttribute?.ResolveType(); }
        }

        public int ResolveAmount(AttributeController attacker) {
            return _damageValue?.Resolve(attacker) ?? 0;
        }
    }
}
