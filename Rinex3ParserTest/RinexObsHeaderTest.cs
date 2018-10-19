// Rinex3ParserTest
// RinexObsHeaderTest.cs-2016-09-02

#region

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rinex3Parser.Common;
using Rinex3Parser.Obs;


#endregion

namespace Rinex3ParserTest
{
    //all data from http://mgex.igs.org/
    [TestClass]
    public class RinexObsHeaderTest
    {
        #region Methods

        [TestMethod, DeploymentItem("TestFiles\\Obs\\Header\\valid_header.rnx")]
        public void TestValidObsHeader()
        {
            var header = new RinexObsHeader();
            var headerLines = File.ReadAllLines("valid_header.rnx");
            foreach (var headerLine in headerLines)
            {
                header.ParseHeaderLine(headerLine);
            }

            var obsHeader = header.ObsHeaderData;

            Assert.AreEqual("3.02", obsHeader.Version);
            Assert.AreEqual(SatelliteSystem.Mixed, obsHeader.SatelliteSystem);
            Assert.AreEqual("CNES", obsHeader.HeaderProgramInfo.AgencyInfo);

            Assert.AreEqual("GR10 V3.22", obsHeader.HeaderProgramInfo.Name);
            Assert.AreEqual(new DateTime(2016, 03, 27, 23, 59, 44), obsHeader.HeaderProgramInfo.FileCreationDateTime);
            Assert.AreEqual("CIBG", obsHeader.MarkerName);

            Assert.AreEqual("GEODETIC", obsHeader.MarkerType);
            Assert.AreEqual("23101M005", obsHeader.MarkerNumber);

            Assert.AreEqual("REGINA", obsHeader.Observer);
            Assert.AreEqual("CNES", obsHeader.Agency);

            Assert.AreEqual("1700870", obsHeader.RcvInfo.Number);
            Assert.AreEqual("LEICA GR10", obsHeader.RcvInfo.RcvType);
            Assert.AreEqual("3.22/6.521", obsHeader.RcvInfo.Version);

            Assert.AreEqual("725039", obsHeader.AntInfo.Number);
            Assert.AreEqual("LEIAR25.R4      LEIT", obsHeader.AntInfo.Type);

            Assert.AreEqual(-1837002.6304, obsHeader.X);
            Assert.AreEqual(6065627.3599, obsHeader.Y);
            Assert.AreEqual(-716183.2716, obsHeader.Z);

            Assert.AreEqual(0.0, obsHeader.AntInfo.DeltaH);
            Assert.AreEqual(0.0, obsHeader.AntInfo.DeltaE);
            Assert.AreEqual(0.0, obsHeader.AntInfo.DeltaN);

            var gpsMetadata = obsHeader.ObsMetaData[SatelliteSystem.Gps];
            ObservationCode[] gpsObs =
                    TestUtils.FormObsCodes("C1C L1C D1C S1C C2S L2S D2S S2S C2W L2W D2W S2W C5Q L5Q D5Q S5Q");
            CollectionAssert.AreEqual(gpsObs, gpsMetadata.Observations);

            var gloMetadata = obsHeader.ObsMetaData[SatelliteSystem.Glo];
            var gloObs = TestUtils.FormObsCodes("C1C L1C D1C S1C C2P L2P D2P S2P ");
            CollectionAssert.AreEqual(gloObs, gloMetadata.Observations);

            var galMetadata = obsHeader.ObsMetaData[SatelliteSystem.Gal];
            var galObs = TestUtils.FormObsCodes("C1C L1C D1C S1C C5Q L5Q D5Q S5Q C7Q L7Q D7Q S7Q C8Q L8Q D8Q S8Q");
            CollectionAssert.AreEqual(galObs, galMetadata.Observations);

            var bdsMetadata = obsHeader.ObsMetaData[SatelliteSystem.Bds];
            var bdsObs = TestUtils.FormObsCodes("C1I L1I D1I S1I C7I L7I D7I S7I");
            CollectionAssert.AreEqual(bdsObs, bdsMetadata.Observations);


            var qzssMetadata = obsHeader.ObsMetaData[SatelliteSystem.Qzss];
            var qzssObs = TestUtils.FormObsCodes("C1C L1C D1C S1C C2S L2S D2S S2S C5Q L5Q D5Q S5Q");
            CollectionAssert.AreEqual(qzssObs, qzssMetadata.Observations);

            var sbasMetadata = obsHeader.ObsMetaData[SatelliteSystem.Sbas];
            var sbasObs = TestUtils.FormObsCodes("C1C L1C D1C S1C");
            CollectionAssert.AreEqual(sbasObs, sbasMetadata.Observations);

            Assert.AreEqual("DBHZ", obsHeader.SignalStrengthUnit);

            Assert.AreEqual(30.0, obsHeader.Interval);

            Assert.AreEqual(new DateTime(2016, 03, 28), obsHeader.FirstObs);
            Assert.AreEqual(new DateTime(2016, 03, 28, 23, 59, 30), obsHeader.LastObs);

            Assert.AreEqual(0, obsHeader.ApplyRcvClockOffset ? 1 : 0);


            Assert.IsTrue(obsHeader.PhaseShiftDict[SatelliteSystem.Gps].Count == 2);
            SatelliteSystem s = SatelliteSystem.Gps;
            Assert.AreEqual(ObservationCode.L2S, obsHeader.PhaseShiftDict[s][0].ObservationCode);
            Assert.AreEqual(ObservationCode.L2S, obsHeader.PhaseShiftDict[s][0].ObservationCode);
            Assert.AreEqual(-0.25, obsHeader.PhaseShiftDict[s][0].CorrectionCycles);
            Assert.AreEqual(ObservationCode.L2X, obsHeader.PhaseShiftDict[s][1].ObservationCode);
            Assert.AreEqual(-0.25, obsHeader.PhaseShiftDict[s][1].CorrectionCycles);

            s = SatelliteSystem.Glo;
            Assert.IsTrue(obsHeader.PhaseShiftDict[s].Count == 1);
            Assert.AreEqual(ObservationCode.L2P, obsHeader.PhaseShiftDict[s][0].ObservationCode);
            Assert.AreEqual(0.25, obsHeader.PhaseShiftDict[s][0].CorrectionCycles);

            s = SatelliteSystem.Gal;
            Assert.IsTrue(obsHeader.PhaseShiftDict[s].Count == 1);
            Assert.AreEqual(ObservationCode.L8Q, obsHeader.PhaseShiftDict[s][0].ObservationCode);
            Assert.AreEqual(-0.25, obsHeader.PhaseShiftDict[s][0].CorrectionCycles);

            var gloSatFrq = new Dictionary<GloSatellite, int>
            {
                {GloSatellite.R01, 1},
                {GloSatellite.R02, -4},
                {GloSatellite.R03, 5},
                {GloSatellite.R04, 6},
                {GloSatellite.R05, 1},
                {GloSatellite.R06, -4},
                {GloSatellite.R07, 5},
                {GloSatellite.R08, 6},
                {GloSatellite.R09, -6},
                {GloSatellite.R10, -7},
                {GloSatellite.R11, 0},
                {GloSatellite.R12, -1},
                {GloSatellite.R13, -2},
                {GloSatellite.R14, -7},
                {GloSatellite.R15, 0},
                {GloSatellite.R16, -1},
                {GloSatellite.R17, 4},
                {GloSatellite.R18, -3},
                {GloSatellite.R19, 3},
                {GloSatellite.R20, 2},
                {GloSatellite.R21, 4},
                {GloSatellite.R22, -3},
                {GloSatellite.R23, 3},
                {GloSatellite.R24, 2}
            };

            CollectionAssert.AreEqual(gloSatFrq.Values, obsHeader.GloSlotFreqDict.Values);
            CollectionAssert.AreEqual(gloSatFrq.Keys, obsHeader.GloSlotFreqDict.Keys);

            /*
                17    17  1851     3                                    LEAP SECONDS
             */

            Assert.AreEqual(-71.940, obsHeader.GloPhaseBiasCor[ObservationCode.C1C]);
            Assert.AreEqual(-71.940, obsHeader.GloPhaseBiasCor[ObservationCode.C1P]);
            Assert.AreEqual(-71.940, obsHeader.GloPhaseBiasCor[ObservationCode.C2C]);
            Assert.AreEqual(-71.940, obsHeader.GloPhaseBiasCor[ObservationCode.C2P]);

            Assert.AreEqual(17, obsHeader.LeapSeconds_.CurrentNumOfLeapSeconds);
            Assert.AreEqual(17, obsHeader.LeapSeconds_.FutureNumOfLeapSeconds);
            Assert.AreEqual(1851, obsHeader.LeapSeconds_.Week);
            Assert.AreEqual(3, obsHeader.LeapSeconds_.Day);
        }

        [TestMethod, DeploymentItem("TestFiles\\Obs\\Header\\head.14o")]
        public void Test_Rinex_Obs_Valid_Header_Parse()
        {
            var parser = new RinexObsParser("head.14o", ParseType.StoreData);
            parser.ParseHeader();
            Assert.IsTrue(true);
        }
        [TestMethod, ExpectedException(typeof(ArgumentException), "Invalid gnss system designation.")]
        public void Test_Rinex_Obs_Header_Version_Exception_Raised()
        {
            var temp = "     3.02           OBSERVATION DATA    Z: MIXED            RINEX VERSION / TYPE";
            RinexObsHeader header = new RinexObsHeader();
            header.ParseHeaderLine(temp);
        }


        [TestMethod, ExpectedException(typeof(ArgumentException), "Invalid rinex file version...")]
        public void Test_Rinex_Obs_Invalid_Header_Exception_Raised()
        {
            var temp = "                    OBSERVATION DATA    M: MIXED            RINEX VERSION / TYPE";
            RinexObsHeader header = new RinexObsHeader();
            header.ParseHeaderLine(temp);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException), "No marker name...")]
        public void Test_Rinex_Obs_With_Empty_MarkerName_Exception_Raised()
        {
            var temp = "                                                            MARKER NAME";
            RinexObsHeader header = new RinexObsHeader();
            header.ParseHeaderLine(temp);
        }


        [TestMethod, ExpectedException(typeof(FormatException))]
        public void Test_Rinex_Obs_Missing_ApproxPosition_Exception_Raised()
        {
            var temp = "                6065627.3599  -716183.2716                  APPROX POSITION XYZ";
            RinexObsHeader header = new RinexObsHeader();
            header.ParseHeaderLine(temp);
        }

        [TestMethod, ExpectedException(typeof(RinexParserException)),
         DeploymentItem("TestFiles\\Obs\\Header\\invalid_header_no_sysobs.rnx")]
        public void Test_Rinex_Obs_Missing_SysObs_Exception_Raised()
        {
            var parser = new RinexObsParser("invalid_header_no_sysobs.rnx", ParseType.RaiseEvents);
            parser.Parse();
        }

        [TestMethod, ExpectedException(typeof(RinexParserException)),
         DeploymentItem("TestFiles\\Obs\\Header\\invalid_header_no_markername.rnx")]
        public void Test_Rinex_Obs_Missing_MarkerName_Exception_Raised()
        {
            var parser = new RinexObsParser("invalid_header_no_markername.rnx", ParseType.RaiseEvents);
            parser.Parse();
        }

        [TestMethod, ExpectedException(typeof(RinexParserException)),
         DeploymentItem("TestFiles\\Obs\\Header\\invalid_header_no_rinexversion.rnx")]
        public void Test_Rinex_Obs_Missing_RinexInfo_Exception_Raised()
        {
            var parser = new RinexObsParser("invalid_header_no_rinexversion.rnx", ParseType.RaiseEvents);
            parser.Parse();
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Test_Rinex_Obs_Invalid_ObservedSignal_Raised()
        {
            var headerLine = "G   16 XXX L1C D1C S1C C2S L2S D2S S2S C2W L2W D2W S2W C5Q  SYS / # / OBS TYPES";
            var header = new RinexObsHeader();
            header.ParseHeaderLine(headerLine);
        }

        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\MATE00ITA_R_20181180000_01D_30S_MO.crx")]
        public void Test_Correct_Parse_Of_Rinex_Header_Of_Hatanala_File()
        {
            var parser = new RinexObsParser("MATE00ITA_R_20181180000_01D_30S_MO.crx", ParseType.RaiseEvents);
            parser.ParseHeader();
            var hd = parser.ObsHeader.ObsHeaderData;
            Assert.AreEqual(hd.X, 4641947.5008);
            Assert.AreEqual(hd.Y, 1393045.2215);
            Assert.AreEqual(hd.Z, 4133286.6560);
            Assert.AreEqual(hd.FirstObs, new DateTime(2018, 4, 28));
            Assert.AreEqual(hd.LastObs, new DateTime(2018, 4, 28, 23, 59, 30));
        }
        [TestMethod, DeploymentItem("TestFiles\\Obs\\Header\\glo_L3_codes_test.rnx")]
        public void Glo_L3_Obs_Codes_Read()
        {
            var parser = new RinexObsParser("glo_L3_codes_test.rnx", ParseType.RaiseEvents);
            parser.ParseHeader();
            var hd = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(hd.ObsMetaData[SatelliteSystem.Glo].Observations.Contains(ObservationCode.L3X),
                    "hd.ObsMetaData[SatelliteSystem.Gps].Observations.Contains(ObservationCode.L3X)");
        }
        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\MATE00ITA_R_20181180000_01D_30S_MO.crx"),
         DeploymentItem("TestFiles\\Obs\\Header\\head.14o")]
        public void Test_Correct_Recognition_Of_Hatanaka_File()
        {
            var parser = new RinexObsParser("MATE00ITA_R_20181180000_01D_30S_MO.crx", ParseType.RaiseEvents);
            parser.ParseHeader();
            var hd = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(hd.IsHatanaka, "hd.IsHatanaka");
            parser = new RinexObsParser("head.14o", ParseType.RaiseEvents);
            parser.ParseHeader();
            hd = parser.ObsHeader.ObsHeaderData;
            Assert.IsFalse(hd.IsHatanaka, "!hd.IsHatanaka");
        }
        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\ZIM300CHE_R_20182420828_01H_01S_MO.crx")]
        public void Test_Correct_Hatanaka_Header_Parse()
        {
            var parser = new RinexObsParser("ZIM300CHE_R_20182420828_01H_01S_MO.crx", ParseType.RaiseEvents);
            parser.ParseHeader();
            var hd = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(hd.IsHatanaka, "hd.IsHatanaka");
        }
        #endregion
    }
}