using UnityEngine;

using System;
using System.Collections.Generic;

using BBUnity.Entities.Controllers.Attributes;
using BBUnity.Entities.Controllers.Attributes.Specialised;
using BBUnity.Entities.Controllers.Combat;
using BBUnity.Gameplay.Attributes.Specialised;

namespace BBUnity.Entities.Controllers {

    /// <summary>
    /// A fully-fledged combat controller built on top of AttributeController/EffectController —
    /// those stay low-level, reusable on their own; this is the "just works" option for anything
    /// that fights. Still an AttributeController underneath (see StateController-facing code
    /// like BaseState.AttributeController), so attribute access works exactly the same way.
    ///
    /// Auto-seeds a Health attribute (see Reset()) so every combat entity has one without needing
    /// to click "Add Attribute" for it; Poise, Mana etc. are still added per-entity like any other
    /// attribute.
    ///
    /// Owns both halves of the hit pipeline as data, generic over which attribute is involved —
    /// neither Attacks nor Reductions are hardcoded to Health/Poise, they're lists of "pick an
    /// attribute, then how much" entries (see DamageEntry/ReductionEntry), so a single Attack can
    /// damage any number of attributes, and any attribute can have its own mitigation:
    /// - Attacks: each one fully self-contained (its own AttackCollider prefab, spawn position,
    ///   and DamageEntries) — see PerformAttack().
    /// - Reductions: what this entity mitigates, per attribute, when it takes a hit — see
    ///   ApplyHit().
    /// </summary>
    public class AttributeBasedCombatController : AttributeController {

        [SerializeField]
        private List<Attack> _attacks = new List<Attack>();

        [SerializeField]
        private List<ReductionEntry> _reductions = new List<ReductionEntry>();

        [SerializeField, Tooltip("However much a hit to a given attribute is mitigated, at least this much always gets through — unless the hit dealt none of that attribute's damage to begin with.")]
        private int _minimumDamage = 1;

        // Set by e.g. a hyper-armour attack (GroundedChargedAttackState) — poise damage is
        // ignored entirely while true; every other attribute's damage still applies.
        private bool _isPoiseImmune = false;

        private EffectController _effectController;

        /// <summary>
        /// Fired once accumulated poise damage crosses the flinch threshold. The game layer
        /// (e.g. PlayerStateController) listens for this to force a reaction state — this class
        /// deliberately knows nothing about states itself.
        /// </summary>
        public event System.Action OnFlinched;

        public Health Health {
            get { return GetAttribute<HealthAttribute>()?.Value; }
        }

        public Poise Poise {
            get { return GetAttribute<PoiseAttribute>()?.Value; }
        }

        private void Reset() {
            if (GetAttribute<HealthAttribute>() == null) {
                AddAttribute(new HealthAttribute());
            }
        }

        public void SetPoiseImmune(bool immune) {
            _isPoiseImmune = immune;
        }

        /// <summary>
        /// Spawns the named Attack's AttackCollider prefab (pooled, via EffectController — added
        /// silently to this GameObject if it doesn't already have one; see GetOrAddEffectController)
        /// at its configured spawn position, and stamps it with this entity as the source and the
        /// attack's id, so it can resolve this same Attack's damage again at the moment it lands
        /// a hit (see CreateHit). Logs a warning and no-ops on any missing setup — a missing
        /// AttackCollider prefab, spawn position, or unregistered id.
        /// </summary>
        public GameObject PerformAttack(string id) {
            Attack attack = _attacks.Find(a => a.Id == id);
            if (attack == null) {
                Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}' has no Attack registered with id '{id}'.", this);
                return null;
            }

            if (attack.AttackColliderPrefab == null) {
                Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}': Attack '{id}' has no AttackCollider prefab assigned.", this);
                return null;
            }

            if (attack.SpawnPosition == null) {
                Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}': Attack '{id}' has no spawn position assigned.", this);
                return null;
            }

            GameObject instance = GetOrAddEffectController().Spawn(id, attack.AttackColliderPrefab.gameObject, attack.SpawnPosition.position);

            AttackCollider attackCollider = instance.GetComponent<AttackCollider>();
            attackCollider.SetAttackId(id);
            attackCollider.SetSource(gameObject);

            return instance;
        }

        /// <summary>
        /// Resolves the named Attack's DamageEntries against this (attacking) controller's own
        /// attributes into a hit's raw, pre-mitigation numbers — called by AttackCollider at the
        /// moment of impact, not when PerformAttack() spawns it, so stat-scaled damage reflects
        /// the attacker's current state rather than a value snapshotted earlier.
        /// </summary>
        public HitInfo CreateHit(string attackId) {
            Attack attack = _attacks.Find(a => a.Id == attackId);

            if (attack == null) {
                Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}' has no Attack registered with id '{attackId}'.", this);
                return new HitInfo(gameObject, Array.Empty<AttributeDamage>());
            }

            var damages = new List<AttributeDamage>(attack.DamageEntries.Count);
            foreach (DamageEntry entry in attack.DamageEntries) {
                Type attributeType = entry.TargetAttributeType;
                if (attributeType == null) {
                    Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}': Attack '{attackId}' has a Damage Entry with no Target Attribute set — it will be skipped.", this);
                    continue;
                }

                damages.Add(new AttributeDamage(attributeType, entry.ResolveAmount(this)));
            }

            return new HitInfo(gameObject, damages);
        }

        /// <summary>
        /// Applies this (defending) controller's own Reductions to each of an incoming hit's
        /// per-attribute amounts, then reduces that attribute. Poise damage is skipped entirely
        /// while poise-immune; a successful poise break still fires OnFlinched exactly as before.
        /// </summary>
        public void ApplyHit(HitInfo hit) {
            foreach (AttributeDamage damage in hit.Damages) {
                ApplyDamage(damage.AttributeType, damage.Amount);
            }
        }

        private void ApplyDamage(Type attributeType, int rawAmount) {
            if (attributeType == null) { return; }
            if (attributeType == typeof(PoiseAttribute) && _isPoiseImmune) { return; }

            EntityAttribute targetAttribute = GetAttribute(attributeType);
            if (targetAttribute == null) {
                Debug.LogWarning($"AttributeBasedCombatController on '{gameObject.name}' took damage targeting {attributeType.Name}, but has no such attribute — it was ignored.", this);
                return;
            }

            int mitigated = MitigateDamage(rawAmount, GetReductionsFor(attributeType));
            if (mitigated <= 0) { return; }

            targetAttribute.TakeDamage(mitigated);

            if (targetAttribute is PoiseAttribute poiseAttribute && poiseAttribute.Value.Flinched) {
                OnFlinched?.Invoke();
                poiseAttribute.Value.ResetFlinchCounter();
            }
        }

        private List<Reduction> GetReductionsFor(Type attributeType) {
            var matching = new List<Reduction>();

            foreach (ReductionEntry entry in _reductions) {
                if (entry.Reduction != null && entry.TargetAttributeType == attributeType) {
                    matching.Add(entry.Reduction);
                }
            }

            return matching;
        }

        /// <summary>
        /// Default combination for a list of Reductions targeting one attribute:
        /// "additive-then-flat" — every Percentage reduction is summed into one combined
        /// percentage and applied once (so stacking two 25% resistances gives 50% off, not 25%
        /// off twice, compounding), then every Flat reduction is subtracted from what's left.
        /// Overridable by a subclass that wants a different combination rule.
        /// </summary>
        protected virtual int MitigateDamage(int rawDamage, List<Reduction> reductions) {
            if (rawDamage <= 0) { return 0; }

            float totalPercentage = 0f;
            float totalFlat = 0f;

            foreach (Reduction reduction in reductions) {
                float amount = reduction.Resolve(this);

                if (reduction.Kind == ReductionKind.Percentage) {
                    totalPercentage += amount;
                } else {
                    totalFlat += amount;
                }
            }

            float afterPercentage = rawDamage * (1f - Mathf.Clamp01(totalPercentage));
            int mitigated = Mathf.RoundToInt(afterPercentage - totalFlat);

            return Mathf.Max(_minimumDamage, mitigated);
        }

        // Silent by design (see PerformAttack) — a fully-fledged combat controller shouldn't
        // require its user to remember a second, unrelated component just to spawn attacks.
        // We'll likely want this configurable later (e.g. warn instead of auto-add).
        private EffectController GetOrAddEffectController() {
            if (_effectController != null) { return _effectController; }

            _effectController = GetComponent<EffectController>();
            if (_effectController == null) {
                _effectController = gameObject.AddComponent<EffectController>();
            }

            return _effectController;
        }
    }
}
