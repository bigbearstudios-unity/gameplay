namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// The type listed in an Attack's damage-type picker and stored in its [SerializeReference]
    /// fields (HealthDamage/PoiseDamage) — mirrors EntityState/EntityAttribute's "add a concrete
    /// type from a dropdown" pattern, but for a single polymorphic field rather than a list (see
    /// DamageValuePropertyDrawer). Resolved against the *attacker's* AttributeController at the
    /// moment a hit lands (see AttackCollider), before the defender's Reductions are applied.
    /// </summary>
    [System.Serializable]
    public abstract class DamageValue {

        public abstract int Resolve(AttributeController attacker);
    }
}
