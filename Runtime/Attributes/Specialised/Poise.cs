using UnityEngine;

using BBUnity.Gameplay.Attributes.Generic;
using UnityEditor;

namespace BBUnity.Gameplay.Attributes.Specialised {
    
    [System.Serializable]
    public class Poise : DynamicIntegerAttribute {

        /// <summary>
        /// The time it takes for the poise to regenerate. Once this time is hit
        /// the poise will be set to its maxium value instantly
        /// </summary>
        [SerializeField]
        private float _regenerationTime = 5.0f;
        
        /// <summary>
        /// The internal timer for the regeneration. Once triggered all
        /// poise values get reset to their default values
        /// </summary>
        [SerializeField, EditorAttributes.ReadOnly]
        private float _regenerationTimer = 0.0f;

        /// <summary>
        /// The threshold for when 'flitch' should be defined. This is reset upon
        /// 'flintching'
        /// </summary>
        [SerializeField]
        private int _flitchThreshold;
        private int _flitchCounter = 0;

        public bool Staggered { get { return _current <= _minimum; } }
        public bool Flinched { get { return _flitchCounter > _flitchThreshold; } }

        private void ResetRegenerationTimer() {
            _regenerationTimer = _regenerationTime;
        }

        private void IncreaseFlitchValue(int amount) {
            _flitchCounter += amount;
        }

        public void TakeDamage(int damage) {
            DecreaseBy(damage);
            IncreaseFlitchValue(damage);
            ResetRegenerationTimer();
        }

        public void ResetFlinchCounter() {
            _flitchCounter = 0;
        }

        public override void Update(float deltaTime) {
            if(_current < _maximum) {
                _regenerationTimer -= deltaTime;

                if(_regenerationTimer <= 0.0f) {
                    Reset();
                }
            }
        }

        public override void Reset() {
            base.Reset();

            ResetFlinchCounter();
            ResetRegenerationTimer();
        }
    }
}