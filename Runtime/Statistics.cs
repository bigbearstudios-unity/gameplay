using System.Collections.Generic;

using BBUnity.CharacterStatistics.Internal;

namespace BBUnity {

    public class Statistics {

        /// <summary>
        /// The internal collection for the statistics. This is populated
        /// upon creation
        /// </summary>
        Dictionary<int, BaseStatistic> _statistics = null;
    }
}