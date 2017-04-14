// Rinex3Parser
// SbasNavMessage.cs-2016-08-16

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    //NOTE Not tested...
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SbasNavMessage : NavMessage
    {
        #region Constructor

        public SbasNavMessage(SbasSatellite satellitePrn, DateTime dt, double[] clkData, double[] orbitParameters)
                : base(dt, clkData, orbitParameters)
        {
            SatellitePrn = satellitePrn;
        }

        #endregion

        #region Properties

        public double SvClkBias
        {
            get { return ClkData[0]; }
        }

        public double SvRelativeFreqBias
        {
            get { return ClkData[1]; }
        }

        public double Ttom
        {
            get { return ClkData[2]; }
        }

        public int Toc
        {
            get { return Time.Year; }
        }

        //orbit 1 data
        public double X
        {
            get { return OrbitParameters[0]; }
        }

        public double Vx
        {
            get { return OrbitParameters[1]; }
        }

        public double Ax
        {
            get { return OrbitParameters[2]; }
        }

        public double Health
        {
            get { return OrbitParameters[3]; }
        }

        //orbit 2 data
        public double Y
        {
            get { return OrbitParameters[4]; }
        }

        public double VY
        {
            get { return OrbitParameters[5]; }
        }

        public double Ay
        {
            get { return OrbitParameters[6]; }
        }

        public double AccuracyCode
        {
            get { return OrbitParameters[7]; }
        }

        //orbit 3 data
        public double Z
        {
            get { return OrbitParameters[8]; }
        }

        public double Vz
        {
            get { return OrbitParameters[9]; }
        }

        public double Az
        {
            get { return OrbitParameters[10]; }
        }

        public double AgeOfOperInfo
        {
            get { return OrbitParameters[11]; }
        }

        public SbasSatellite SatellitePrn { get; set; }

        public override SatelliteSystem SatelliteSystem
        {
            get { return SatelliteSystem.Sbas; }
        }

        internal override int SatPrn
        {
            get { return (int)SatellitePrn; }
        }

        #endregion

        #region Methods

        protected override string PrintOrbitsValues(string satellitePrn)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("{0} {1,-19}{2,19}{3,19}{4,19}", satellitePrn,
                    Time.ToString("yyyy MM dd HH mm ss"),
                    ClkData[0].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture),
                    ClkData[1].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture),
                    ClkData[2].ToString(ORBIT_VALUES_FORMAT, CultureInfo.InvariantCulture)));
            //orbit data 1
            FormatOrbitData(sb, OrbitParameters[0], OrbitParameters[1], OrbitParameters[2], OrbitParameters[3]);
            //orbit data 2
            FormatOrbitData(sb, OrbitParameters[4], OrbitParameters[5], OrbitParameters[6], OrbitParameters[7]);
            //orbit data 3
            FormatOrbitData(sb, OrbitParameters[8], OrbitParameters[9], OrbitParameters[10], OrbitParameters[11]);

            return sb.ToString();
        }

        public override string ToString()
        {
            return PrintOrbitsValues(SatellitePrn.ToString());
        }

        #endregion
    }


    public class SbasNavMessageContentParser : NavMessageContentParserBase<SbasNavMessage>
    {
        #region Constructor

        public SbasNavMessageContentParser(StreamReader sr)
                : base(sr, 4)
        {
        }

        #endregion

        #region Methods

        protected override SbasNavMessage ParseContents(string[] broadCastOrbits)
        {
            var svEpochClk = broadCastOrbits[0];
            SbasSatellite satellitePrn;
            DateTime dt;
            double[] clkData;
            ParseSvEpochClk(svEpochClk, out satellitePrn, out dt, out clkData);
            var orbitParameters = ParseContentsImpl(broadCastOrbits);
            return new SbasNavMessage(satellitePrn, dt, clkData, orbitParameters);
        }

        #endregion
    }
}