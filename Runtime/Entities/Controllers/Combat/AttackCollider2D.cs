using UnityEngine;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// 2D implementation of AttackCollider — requires a Collider2D (trigger) and forwards
    /// OnTriggerEnter2D into the base's dimension-agnostic hit processing.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AttackCollider2D : AttackCollider {

        private void OnTriggerEnter2D(Collider2D col) {
            ProcessHit(col.gameObject);
        }
    }
}
