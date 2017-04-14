// Rinex3Parser
// BdsNavMessage.cs-2016-08-16

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class BdsNavMessage : NavMessage
    {
        #region Constructor

        internal BdsNavMessage(BdsSatellite satellitePrn, DateTime dt, [NotNull] double[] clkData,
                [NotNull] double[] orbitParameters) : base(dt, clkData, orbitParameters)
        {
            SatellitePrn = satellitePrn;
        }

        #endregion

        #region Properties

        //sv, epoch, clk data
        public BdsSatellite SatellitePrn { get; private set; }

        public double SvClkBias
        {
            get { return ClkData[0]; }
        }

        public double SvClkDrift
        {
            get { return ClkData[1]; }
        }

        public double SvClkDriftRate
        {
            get { return ClkData[2]; }
        }

        public int Toc
        {
            get { return Time.Year; }
        }

        // orbit 1 data
        public double Aode
        {
            get { return OrbitParameters[0]; }
        }

        public double Crs
        {
            get { return OrbitParameters[1]; }
        }

        public double DeltaN
        {
            get { return OrbitParameters[2]; }
        }

        public double M0
        {
            get { return OrbitParameters[3]; }
        }

        // orbit 2 data
        public double Cuc
        {
            get { return OrbitParameters[4]; }
        }

        public double Ecc
        {
            get { return OrbitParameters[5]; }
        }

        public double Cus
        {
            get { return OrbitParameters[6]; }
        }

        public double SqrtA
        {
            get { return OrbitParameters[7]; }
        }

        // orbit 3 data
        public double Toe
        {
            get { return OrbitParameters[8]; }
        }

        public double Cic
        {
            get { return OrbitParameters[9]; }
        }

        public double Omega0
        {
            get { return OrbitParameters[10]; }
        }

        public double Cis
        {
            get { return OrbitParameters[11]; }
        }

        // orbit 4 data
        public double I0
        {
            get { return OrbitParameters[12]; }
        }

        public double Crc
        {
            get { return OrbitParameters[13]; }
        }

        public double Omega
        {
            get { return OrbitParameters[14]; }
        }

        public double OmegaDot
        {
            get { return OrbitParameters[15]; }
        }

        //orbit 5 data
        public double Idot
        {
            get { return OrbitParameters[16]; }
        }

        public double BdsWeek
        {
            get { return OrbitParameters[18]; }
        }


        //orbit 6 data
        public double Accuracy
        {
            get { return OrbitParameters[20]; }
        }

        public double SatH1
        {
            get { return OrbitParameters[21]; }
        }

        public double Tgd1
        {
            get { return OrbitParameters[22]; }
        }

        public double Tgd2
        {
            get { return OrbitParameters[23]; }
        }

        //orbit 7 data
        public double Ttom
        {
            get { return OrbitParameters[24]; }
        }

        public double Aodc
        {
            get { return OrbitParameters[25]; }
        }


        public override SatelliteSystem SatelliteSystem
        {
            get { return SatelliteSystem.Bds; }
        }

        internal override int SatPrn
        {
            get { return (int)SatellitePrn; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return PrintOrbitsValues(SatellitePrn.ToString());
        }

        #endregion
    }


    public class BdsNavMessageContentParser : NavMessageContentParserBase<BdsNavMessage>
    {
        #region Constructor

        public BdsNavMessageContentParser(StreamReader sr) : base(sr, 8)
        {
        }

        #endregion

        #region Methods

        protected override BdsNavMessage ParseContents([NotNull] string[] broadCastOrbits)
        {
            var svEpochClk = broadCastOrbits[0];
            BdsSatellite satellitePrn;
            DateTime dt;
            double[] clkData;
            ParseSvEpochClk(svEpochClk, out satellitePrn, out dt, out clkData);
            var orbitParameters = ParseContentsImpl(broadCastOrbits);
            return new BdsNavMessage(satellitePrn, dt, clkData, orbitParameters);
        }

        
        

        #endregion
    }
}