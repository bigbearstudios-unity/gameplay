using BBUnity;
using BBUnity.CharacterStatistics.Internal;

namespace BBUnity.CharacterStatistics.StaticStatistics {
    public interface IHealth : IBaseStatistic {
        void OnHealthDecreased();
        void OnHealthIncrease();
        void OnHealthZero();
    }

    class Health : BaseStatistic {
        private uint _currentHealth;
        private uint _maximumHealth;

        //BehaviourDelegate<IHealth> _delegate;
    }
}