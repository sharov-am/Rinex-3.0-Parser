// Rinex3Parser
// RinexHeader.cs-2016-08-18

#region

using System;


#endregion

namespace Rinex3Parser.Common
{
    public abstract class RinexHeader
    {
        #region Properties

        public abstract RinexFileType RinexFileType { get; }
        public RinexHeaderData RinexHeaderData { get; private set; }

        #endregion

        #region Methods

        public abstract bool ParseHeaderLine(string line);

        #endregion
    }


    public abstract class RinexHeaderData : EventArgs
    {
        #region Properties

        public SatelliteSystem SatelliteSystem { get; set; }

        #endregion
    }
}