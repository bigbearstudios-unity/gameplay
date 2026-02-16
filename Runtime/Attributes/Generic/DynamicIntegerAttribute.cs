using UnityEngine;

using BBUnity.Gameplay.Attributes.Base;

namespace BBUnity.Gameplay.Attributes.Generic {
    public class DynamicIntegerAttribute : DynamicAttribute {

        public delegate void OnChange(int current, int minimum, int maximum);

        public enum OnResetBehaviours {
            ResetToMaximum = 0,
            ResetToMinimum = 100,
            ResetToZero = 200
        }

        [SerializeField]
        protected int _maximum = 0;

        [SerializeField]
        protected int _minimum = 0;

        [SerializeField]
        private OnResetBehaviours _onResetBehaviour;

        [SerializeField, EditorAttributes.ReadOnly]
        protected int _current = 0;

        public int Minimum { get { return _minimum; } }
        public int Maximum { get { return _maximum; } }
        public int Current { get { return _current; } }
        public bool IsZero { get { return _current == 0; } }

        public OnChange onChange;

        public override void Reset() {
            if(_onResetBehaviour == OnResetBehaviours.ResetToMaximum) {
                SetCurrent(_maximum); return;
            }

            if(_onResetBehaviour == OnResetBehaviours.ResetToZero) {
                SetCurrent(0); return;
            }

            if(_onResetBehaviour == OnResetBehaviours.ResetToMinimum) {
                SetCurrent(_minimum); return;
            }
        }

        public virtual void IncreaseBy(int amount) {
            _current += amount;

            onChange?.Invoke(_current, _minimum, _maximum);
        }

        public virtual void DecreaseBy(int amount) {
            _current -= amount;

            onChange?.Invoke(_current, _minimum, _maximum);
        }

        public virtual void SetCurrent(int value) {
            _current = value;

            onChange?.Invoke(_current, _minimum, _maximum);
        }
    }
}