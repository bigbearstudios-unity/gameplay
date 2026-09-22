using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// Lives on a hurtbox collider — typically a child of the entity, kept separate from the
    /// main movement collider so it can sit on its own physics layer (a GameObject's layer
    /// applies to every collider on it). Forwards a resolved hit up to the entity's own
    /// AttributeBasedCombatController, which is the only thing that knows how to mitigate/apply it —
    /// this class has no combat logic of its own.
    /// </summary>
    public class AttackReceiver : MonoBehaviour {

        [SerializeField, Tooltip("The AttributeBasedCombatController this hurtbox reports hits to. Found automatically (this GameObject or its children) if left unassigned.")]
        private AttributeBasedCombatController _attributeController;

        private void Awake() {
            if (_attributeController == null) {
                _attributeController = GetComponentInChildren<AttributeBasedCombatController>();
            }

            if (_attributeController == null) {
                Debug.LogWarning($"AttackReceiver on '{gameObject.name}' has no AttributeBasedCombatController assigned or found on itself/its children.", this);
            }
        }

        public void ReceiveHit(HitInfo hit) {
            _attributeController.ApplyHit(hit);
        }
    }
}
