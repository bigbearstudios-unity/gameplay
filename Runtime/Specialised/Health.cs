using BBUnity;
using BBUnity.Statistics.Internal;

namespace BBUnity.Statistics.Specialised {
    public interface IHealth : IBaseStatistic {
        void OnHealthDecreased();
        void OnHealthIncrease();
        void OnHealthZero();
    }

    class Health : BaseStatistic {
        //BehaviourDelegate<IHealth> _delegate;
    }
}