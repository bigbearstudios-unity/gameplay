using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// One raw, pre-mitigation damage amount targeting a specific attribute type — the type
    /// travels with the amount since it's resolved by the attacker but applied by the defender,
    /// two different AttributeControllers.
    /// </summary>
    public readonly struct AttributeDamage {

        public readonly System.Type AttributeType;
        public readonly int Amount;

        public AttributeDamage(System.Type attributeType, int amount) {
            AttributeType = attributeType;
            Amount = amount;
        }
    }

    /// <summary>
    /// A hit's raw, pre-mitigation numbers — resolved by the attacker's
    /// AttributeBasedCombatController (see Attack.DamageEntries) before being handed to the
    /// defender's AttackReceiver. The defender's own AttributeBasedCombatController.ApplyHit()
    /// applies its Reductions to these before touching any attribute — neither side needs to know
    /// how the other is configured, or which attributes the other even has.
    /// </summary>
    public readonly struct HitInfo {

        public readonly GameObject Source;
        public readonly IReadOnlyList<AttributeDamage> Damages;

        public HitInfo(GameObject source, IReadOnlyList<AttributeDamage> damages) {
            Source = source;
            Damages = damages;
        }
    }
}
