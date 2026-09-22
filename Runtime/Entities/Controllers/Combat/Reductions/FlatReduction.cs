using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// Subtracts the defender's raw current value of a chosen attribute — e.g. Defence 15
    /// reducing an incoming hit by 15.
    /// </summary>
    [System.Serializable]
    public class FlatReduction : Reduction {

        [SerializeField]
        private AttributeTypeReference _attribute;

        public override ReductionKind Kind {
            get { return ReductionKind.Flat; }
        }

        public override float Resolve(AttributeController defender) {
            return _attribute?.Resolve(defender)?.NumericValue ?? 0;
        }
    }
}
