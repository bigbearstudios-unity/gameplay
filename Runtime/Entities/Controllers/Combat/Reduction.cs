namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// Which bucket a Reduction's Resolve() value belongs to when AttributeBasedCombatController
    /// combines a whole list of them — see AttributeBasedCombatController.MitigateDamage() for the
    /// default "additive-then-flat" combination (all Percentage values summed and applied once,
    /// then all Flat values subtracted from the remainder).
    /// </summary>
    public enum ReductionKind {
        Flat,
        Percentage
    }

    /// <summary>
    /// The type picked for a ReductionEntry.Reduction — unlike EntityAttribute/EntityState,
    /// duplicates of the same concrete type are allowed across an AttributeBasedCombatController's
    /// Reductions list (e.g. two ReductionEntry entries, both FlatReduction, targeting the same
    /// attribute from two different resistance attributes), so there's no one-per-type constraint
    /// here.
    /// </summary>
    [System.Serializable]
    public abstract class Reduction {

        public abstract ReductionKind Kind { get; }

        /// <summary>
        /// Flat: the raw amount to subtract. Percentage: a 0..1 fraction of incoming damage to
        /// remove.
        /// </summary>
        public abstract float Resolve(AttributeController defender);
    }
}
