using UnityEngine;
using UnityEngine.UI;

using BBUnity.Gameplay.Attributes.Generic;

namespace BBUnity.Entities.Controllers.Attributes {

    /// <summary>
    /// The type listed in AttributeController's "Add Attribute" dropdown and stored in its
    /// [SerializeReference] list — mirrors EntityState/StateController. A concrete attribute
    /// (e.g. HealthAttribute in the Specialised folder) never reimplements attribute logic
    /// itself: it boxes one instance of the corresponding BBUnity.Gameplay.Attributes.Specialised
    /// type via EntityAttribute&lt;TAttribute&gt; below and forwards to it.
    /// </summary>
    [System.Serializable]
    public abstract class EntityAttribute {

        // Generic numeric read used by combat data (DamageValue/Reduction picking "an attribute"
        // by type) without needing to know the concrete attribute type — null for a wrapper whose
        // boxed type doesn't expose one. See EntityAttribute&lt;TAttribute&gt; below.
        public virtual int? NumericValue {
            get { return null; }
        }

        // Generic damage application, used by AttributeBasedCombatController.ApplyHit() without
        // needing to know the concrete attribute type — no-ops by default for a wrapper whose
        // boxed type doesn't support it. See EntityAttribute&lt;TAttribute&gt; below.
        public virtual void TakeDamage(int amount) { }

        public virtual void Start() { }

        public virtual void Tick(float deltaTime) { }
    }

    /// <summary>
    /// Boxes a single DynamicIntegerAttribute instance (Health, Mana, Poise, ...) instead of
    /// duplicating its logic, and wires up an optional Slider: its max value is set from the
    /// attribute on Start, and its value is kept in sync via the attribute's onChange event.
    /// </summary>
    public abstract class EntityAttribute<TAttribute> : EntityAttribute where TAttribute : DynamicIntegerAttribute, new() {

        [SerializeField]
        protected TAttribute _attribute = new TAttribute();

        [SerializeField, Tooltip("Optional — kept in sync with this attribute's value once Start() runs.")]
        private Slider _slider;

        public TAttribute Value {
            get { return _attribute; }
        }

        public override int? NumericValue {
            get { return _attribute.Current; }
        }

        public override void TakeDamage(int amount) {
            _attribute.TakeDamage(amount);
        }

        public override void Start() {
            _attribute.Reset();

            if (_slider != null) {
                _slider.maxValue = _attribute.Maximum;
                _slider.value = _attribute.Current;
                _attribute.onChange += HandleAttributeChange;
            }
        }

        public override void Tick(float deltaTime) {
            _attribute.Update(deltaTime);
        }

        private void HandleAttributeChange(int current, int minimum, int maximum) {
            _slider.value = current;
        }
    }
}
