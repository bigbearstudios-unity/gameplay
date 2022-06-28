using System.Collections.Generic;

namespace BBUnity.Statistics.Internal {

    public class BaseStatisticsController {
        /// <summary>
        /// The internal collection for the statistics. This is populated
        /// upon creation via Reflection
        /// </summary>
        Dictionary<int, BaseStatistic> _statistics = null;
    }
}