using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Entities.Controllers.Combat {

    /// <summary>
    /// One named attack on an AttributeBasedCombatController's Attacks list — e.g. a boss with 5
    /// attacks has 5 of these, each fully self-contained: which AttackCollider prefab to spawn,
    /// where to spawn it, and a list of DamageEntries describing which attributes it damages and
    /// by how much. Triggered via AttributeBasedCombatController.PerformAttack(Id).
    /// </summary>
    [System.Serializable]
    public class Attack {

        [SerializeField, Tooltip("Looked up by AttributeBasedCombatController.PerformAttack(id).")]
        private string _id;

        [SerializeField, Tooltip("A prefab carrying an AttackCollider component — dragging in a GameObject without one is rejected by this field, since it's typed as the component rather than GameObject.")]
        private AttackCollider _attackColliderPrefab;

        [SerializeField, Tooltip("Where the AttackCollider is spawned — typically a child Transform on this entity, e.g. a weapon tip.")]
        private Transform _spawnPosition;

        [SerializeField]
        private List<DamageEntry> _damageEntries = new List<DamageEntry>();

        public string Id {
            get { return _id; }
        }

        public AttackCollider AttackColliderPrefab {
            get { return _attackColliderPrefab; }
        }

        public Transform SpawnPosition {
            get { return _spawnPosition; }
        }

        public IReadOnlyList<DamageEntry> DamageEntries {
            get { return _damageEntries; }
        }
    }
}
