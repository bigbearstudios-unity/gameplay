namespace BBUnity.Gameplay.Attributes.Base {
    public interface IAttribute {

    }

    public class Attribute : IAttribute {
        
        /// <summary>
        /// Resets the values of the attribute
        /// </summary>
        public virtual void Reset() {

        }
    }
}