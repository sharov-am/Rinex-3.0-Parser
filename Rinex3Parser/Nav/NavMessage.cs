// Rinex3Parser
// NavMessage.cs-2016-08-18

#region

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public abstract class NavMessage : EventArgs
    {
        #region Fields

        internal const string ORBIT_VALUES_FORMAT = "0.000000000000E+00";
        protected readonly double[] ClkData;
        protected  readonly double[] OrbitParameters;

        #endregion

        #region Constructor

        protected NavMessage(DateTime dt, double[] clkData, double[] orbitParameters)
        {
            ClkData = clkData;
            OrbitParameters = orbitParameters;
            Time = dt;
        }
        
        #endregion

        #region Properties

        // ReSharper disable once UnusedMember.Global
        public abstract SatelliteSystem SatelliteSystem { get; }
        internal abstract int SatPrn { get; }

        public DateTime Time { get; private set; }

        #endregion

        #region Methods

        public void SetTime(DateTime dt)
        {
            Time = dt;
        }
                 
        public T ConvertTo<T>() where T:NavMessage
        {
            return this as T;
        }


        internal bool CheckInterval(DateTime start, DateTime end)
        {
            return Time >= start && Time <= end;
        }


        protected static void FormatOrbitData([NotNull] StringBuilder sb, [NotNull] params double[] values)
        {
            var first = true;
            foreach (var value in values)
            {
                sb.AppendFormat(first ? "    {0,19}" : "{0,19}",
                        value.ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture));
                first = false;
            }
            sb.AppendLine();
        }


        protected virtual string PrintOrbitsValues(string satellitePrn)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0} {1,-19:yyyy MM dd HH mm ss}{2,19}{3,19}{4,19}", satellitePrn, Time,
                    ClkData[0].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture),
                    ClkData[1].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture),
                    ClkData[2].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture)));
            //orbit data 1
            FormatOrbitData(sb, OrbitParameters[0], OrbitParameters[1], OrbitParameters[2], OrbitParameters[3]);
            //orbit data 2
            FormatOrbitData(sb, OrbitParameters[4], OrbitParameters[5], OrbitParameters[6], OrbitParameters[7]);
            //orbit data 3
            FormatOrbitData(sb, OrbitParameters[8], OrbitParameters[9], OrbitParameters[10], OrbitParameters[11]);
            //orbit data 4
            FormatOrbitData(sb, OrbitParameters[12], OrbitParameters[13], OrbitParameters[14], OrbitParameters[15]);
            //orbit data 5
            FormatOrbitData(sb, OrbitParameters[16], OrbitParameters[17], OrbitParameters[18], OrbitParameters[19]);
            //orbit data 6
            FormatOrbitData(sb, OrbitParameters[20], OrbitParameters[21], OrbitParameters[22], OrbitParameters[23]);
            //orbit data 7
            FormatOrbitData(sb, OrbitParameters[24], OrbitParameters[25]);

            return sb.ToString();
        }

        #endregion
    }
}