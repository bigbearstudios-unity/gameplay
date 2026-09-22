using System.Collections.Generic;

using UnityEngine;

using BBUnity.Pools;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// Lives on a spawned attack effect (a "slash") rather than on the attacker's body — this
    /// project's attack effects are themselves the hitbox, since different attacks need
    /// differently shaped/sized/timed ones rather than sharing one fixed collider on the
    /// attacker. Carries no damage numbers itself: _attackId just looks one up on the attacker's
    /// AttributeBasedCombatController.Attacks at the moment of impact (see
    /// AttributeBasedCombatController.CreateHit).
    ///
    /// Effects are pooled, so this can't hold a baked-in attacker reference —
    /// AttributeBasedCombatController.PerformAttack() calls SetSource()/SetAttackId() right after
    /// spawning one. IPoolBehaviour.OnSpawn is how it forgets who it already hit between reuses
    /// (see BBUnity.Pools.PoolBehaviour).
    ///
    /// Dimension-agnostic on purpose — no physics/collision detection lives here, since that's
    /// inherently 2D- or 3D-specific (Collider2D vs Collider, OnTriggerEnter2D vs
    /// OnTriggerEnter). See AttackCollider2D for the 2D implementation; a subclass just needs to
    /// call ProcessHit() from whatever collision callback its dimension uses.
    /// </summary>
    public abstract class AttackCollider : MonoBehaviour, IPoolBehaviour {

        // Always set at runtime via SetAttackId() (see PerformAttack) — not [SerializeField],
        // there's nothing to configure by hand in the Editor.
        private string _attackId;

        private GameObject _source;
        private readonly HashSet<AttackReceiver> _hitReceivers = new HashSet<AttackReceiver>();

        public void SetSource(GameObject source) {
            _source = source;
        }

        public void SetAttackId(string attackId) {
            _attackId = attackId;
        }

        /// <summary>
        /// Call this from whatever collision/trigger callback the concrete dimension uses, with
        /// whatever GameObject was hit.
        /// </summary>
        protected void ProcessHit(GameObject other) {
            if (_source == null) {
                Debug.LogWarning($"AttackCollider on '{gameObject.name}' has no source set — SetSource() must be called before it can register hits (see AttributeBasedCombatController.PerformAttack).", this);
                return;
            }

            if (other == _source) { return; }

            AttackReceiver receiver = other.GetComponentInParent<AttackReceiver>();
            if (receiver == null || _hitReceivers.Contains(receiver)) { return; }

            AttributeBasedCombatController attacker = _source.GetComponent<AttributeBasedCombatController>();
            if (attacker == null) {
                Debug.LogWarning($"AttackCollider on '{gameObject.name}' has a source ('{_source.name}') with no AttributeBasedCombatController — it can't deal damage.", this);
                return;
            }

            _hitReceivers.Add(receiver);
            receiver.ReceiveHit(attacker.CreateHit(_attackId));
        }

        public void OnCreate(PoolBehaviour behaviour) { }

        public void OnSpawn(PoolBehaviour behaviour) {
            _hitReceivers.Clear();
        }

        public void OnReturn(PoolBehaviour behaviour) {
            _source = null;
        }
    }
}
