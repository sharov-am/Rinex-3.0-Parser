using System;
using System.Globalization;


namespace Rinex3Parser.Common
{
    public class LeapSeconds
    {
        #region Properties

        public int? CurrentNumOfLeapSeconds { get; set; }
        public int? FutureNumOfLeapSeconds { get; set; }
        public int? Week { get; set; }
        public int? Day { get; set; }
        public GnssTimeSystem TimeSystem { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return String.Format("{0,6}{1,6}{2,6}{3,6}{4}", CurrentNumOfLeapSeconds, FutureNumOfLeapSeconds, Week,
                    Day, TimeSystem == GnssTimeSystem.Gps ? "   " : GnssTimeSystem.Bdt.ToString().ToUpperInvariant());
        }

        #endregion
    }
}