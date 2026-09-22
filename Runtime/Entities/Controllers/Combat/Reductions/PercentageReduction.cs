using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// Reduces incoming damage by a percentage read from the defender's chosen attribute — the
    /// attribute's raw current value is treated as a whole percentage (e.g. Resistance 25 means
    /// 25%), not a 0..1 fraction, since every attribute today is a plain int.
    /// </summary>
    [System.Serializable]
    public class PercentageReduction : Reduction {

        [SerializeField]
        private AttributeTypeReference _attribute;

        public override ReductionKind Kind {
            get { return ReductionKind.Percentage; }
        }

        public override float Resolve(AttributeController defender) {
            int? value = _attribute?.Resolve(defender)?.NumericValue;
            return value == null ? 0f : value.Value / 100f;
        }
    }
}
