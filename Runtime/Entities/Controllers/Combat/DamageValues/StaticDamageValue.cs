using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    [System.Serializable, System.ComponentModel.DisplayName("Static Damage")]
    public class StaticDamageValue : DamageValue {

        [SerializeField]
        private int _amount;

        public override int Resolve(AttributeController attacker) {
            return _amount;
        }
    }
}
