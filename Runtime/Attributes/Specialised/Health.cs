using BBUnity.Gameplay.Attributes.Generic;

namespace BBUnity.Gameplay.Attributes.Specialised {
    
    [System.Serializable]
    public class Health : DynamicIntegerAttribute {

        public bool Dead { get { return _current <= 0; } }
    }
}