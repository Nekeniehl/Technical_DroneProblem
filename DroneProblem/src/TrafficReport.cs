namespace Derivco
{
    #region Using

    using System;

    #endregion

    internal enum TrafficConditionEnum
    {
        HEAVY,

        LIGHT,

        MODERATE,
    }

    internal class TrafficReport
    {
        #region Properties

        internal string DroneId { get; set; }

        internal double Speed { get; set; }

        internal DateTime Time { get; set; }

        internal TrafficConditionEnum TrafficCondition => (TrafficConditionEnum) Enum
            .GetValues(typeof(TrafficConditionEnum))
            .GetValue(new Random().Next(0, 2));

        #endregion
    }
}