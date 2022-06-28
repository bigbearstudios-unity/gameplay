using BBUnity.Statistics.Internal;

namespace BBUnity.Statistics.Generics {

    /// <summary>
    /// A wrapper around an int based statistic 
    /// </summary>
    class IntStatisic : BaseStatistic {

        private int _value;
        private int _maximumValue;

        public int Value { 
            get { return _value; } 
            set { SetValue(value); }
        }

        public int MaximumValue { 
            get { return _maximumValue; }
            set { SetMaximumValue(value); }
        }
        
        public void SetValue(int newValue) {
            _value = newValue;
        }

        public int DecrementValue(int decrementBy) {
            _value -= decrementBy;

            return _value;
        }

        public int IncrementValue(int incrementBy) {
            _value += incrementBy;

            return _value;
        }

        public void SetMaximumValue(int newMaximumValue) {
            _maximumValue = newMaximumValue;
        }

        public int DecrementMaximumValue(int decrementBy) {
            _value -= decrementBy;

            return _value;
        }

        public int IncrementMaximumValue(int incrementBy) {
            _value += incrementBy;

            return _value;
        }
    }
}