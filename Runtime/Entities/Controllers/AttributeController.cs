
using UnityEngine;

using System.Collections.Generic;

using BBUnity.Entities.Controllers.Attributes;

namespace BBUnity.Entities.Controllers {

    /// <summary>
    /// Controller for all attributes. Attributes are added at edit time via the "Add Attribute"
    /// button in the inspector (see AttributeControllerInspector) rather than declared as
    /// individual [SerializeField] fields on a subclass — each entry is an EntityAttribute
    /// boxing one of the BBUnity.Gameplay.Attributes.Specialised types (Health, Mana, Poise, ...).
    ///
    /// See <see cref="BBUnity.Entities.Controllers.InputController"/> for the rationale behind
    /// this execution order value and an important caveat about it not being inherited by
    /// concrete subclasses (e.g. AttributeBasedCombatController).
    /// </summary>
    [DefaultExecutionOrder(10)]
    public class AttributeController : EntityController {

        [SerializeReference]
        private List<EntityAttribute> _entityAttributes = new List<EntityAttribute>();

        protected virtual void Start() {
            foreach (EntityAttribute attribute in _entityAttributes) {
                attribute.Start();
            }
        }

        protected virtual void Update() {
            float deltaTime = Time.deltaTime;

            foreach (EntityAttribute attribute in _entityAttributes) {
                attribute.Tick(deltaTime);
            }
        }

        /// <summary>
        /// Looks up the registered attribute of type T (e.g. HealthAttribute) — used by
        /// subclasses to expose a strongly-typed accessor (see BaseAttributeController.Health)
        /// backed by this controller's list instead of a dedicated [SerializeField].
        /// </summary>
        protected T GetAttribute<T>() where T : EntityAttribute {
            foreach (EntityAttribute attribute in _entityAttributes) {
                if (attribute is T typed) { return typed; }
            }

            return null;
        }

        /// <summary>
        /// Non-generic counterpart to GetAttribute&lt;T&gt;() — used by combat data (DamageValue/
        /// Reduction) that only has a System.Type picked at edit time (see AttributeTypeReference),
        /// not a compile-time type parameter.
        /// </summary>
        public EntityAttribute GetAttribute(System.Type attributeType) {
            foreach (EntityAttribute attribute in _entityAttributes) {
                if (attributeType.IsInstanceOfType(attribute)) { return attribute; }
            }

            return null;
        }

        /// <summary>
        /// Registers an attribute at runtime — used by AttributeBasedCombatController.Reset() to
        /// auto-seed a mandatory Health entry, mirroring PlayerUnityInputController.Reset()'s
        /// auto-seeded movement mapping.
        /// </summary>
        protected void AddAttribute(EntityAttribute attribute) {
            _entityAttributes.Add(attribute);
        }
    }
}