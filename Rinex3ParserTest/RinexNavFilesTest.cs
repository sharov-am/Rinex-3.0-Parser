// Rinex3ParserTest
// RinexNavFilesTest.cs-2016-09-14

#region

using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rinex3Parser.Common;
using Rinex3Parser.Nav;


#endregion

namespace Rinex3ParserTest
{
    [TestClass]
    public class RinexNavFilesTest
    {
        #region Fields

        private static readonly Func<string, double> DParse = t => Double.Parse(t, CultureInfo.InvariantCulture);

        #endregion

        #region Methods

        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\bds_nav.16c")]
        public void Check_Bds_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("bds_nav.16c", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Bds);
            Assert.IsTrue(metaData.IonoCorrections.Count == 1, "metaData.IonoCorrections.Count == 1");

            var bdsCorrections = metaData.IonoCorrections[0];

            var alpha0 = bdsCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse("2.1420E-08")) < Double.Epsilon);

            var alpha1 = bdsCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse("2.4587E-07")) < Double.Epsilon);

            var alpha2 = bdsCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("-2.4438E-06")) < Double.Epsilon);

            var alpha3 = bdsCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse("5.0068E-06")) < Double.Epsilon);

            var beta0 = bdsCorrections.Corrections[IonoCorrectionsEnum.Beta0];
            Assert.IsTrue(Math.Abs(beta0 - DParse("7.9872E+04")) < Double.Epsilon);

            var beta1 = bdsCorrections.Corrections[IonoCorrectionsEnum.Beta1];
            Assert.IsTrue(Math.Abs(beta1 - DParse("1.3107E+05")) < Double.Epsilon);

            var beta2 = bdsCorrections.Corrections[IonoCorrectionsEnum.Beta2];
            Assert.IsTrue(Math.Abs(beta2 - DParse("5.8982E+05")) < Double.Epsilon);

            var beta3 = bdsCorrections.Corrections[IonoCorrectionsEnum.Beta3];
            Assert.IsTrue(Math.Abs(beta3 - DParse("-7.8643E+05")) < Double.Epsilon);

            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 1, "metaData.TimeSystemCorrections.Count == 1");

            var timeSystemCorrection = metaData.TimeSystemCorrections[0];

            Assert.IsTrue(timeSystemCorrection.Type == CorrectionType.Bdut);

            Assert.IsTrue(!timeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = timeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("-7.4505805969E-09")) < Double.Epsilon);

            var a1 = timeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("2.042810365E-14")) < Double.Epsilon);

            var refTime = timeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 423654, "reftime is wrong");

            var week = timeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 534, "week is wrong");

            var messages = parser.GetMessagesOfType<BdsNavMessage>().ToList();
            Assert.IsTrue(messages.Count == 5, "parser.NavMessages.Count() == 5");

            var c14Sat = messages.SingleOrDefault(t => t.SatellitePrn == BdsSatellite.C14);
            Assert.IsTrue(c14Sat != null, "c14Sat != null");

            Assert.IsTrue(c14Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(c14Sat.Time == new DateTime(2016, 03, 31, 8, 00, 00));

            Assert.IsTrue(Math.Abs(c14Sat.SvClkBias - DParse("4.904676461592E-04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.SvClkDrift - DParse("1.383524406151E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.SvClkDriftRate - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Aode - DParse("1.400000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Crs - DParse("4.703125000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.DeltaN - DParse("4.251248510136E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.M0 - DParse("-1.573794198503E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Cuc - DParse("2.412125468254E-07")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Ecc - DParse("1.877332455479E-03 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Cus - DParse("7.031485438347E-06 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.SqrtA - DParse("5.282602144241E+03")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Toe - DParse("3.744000000000E+05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Cic - DParse("9.778887033463E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Omega0 - DParse("5.925811981283E-02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Cis - DParse("2.048909664154E-08")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.I0 - DParse("9.574968029909E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Crc - DParse("2.165000000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Omega - DParse("-2.466012640239E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.OmegaDot - DParse("-6.897787320529E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Idot - DParse("-8.250343659930E-11")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.BdsWeek - DParse(" 5.340000000000E+02")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Accuracy - DParse("2.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.SatH1 - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Tgd1 - DParse("1.190000000000E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Tgd2 - DParse("2.700000000000E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(c14Sat.Ttom - DParse("3.746700000000E+05 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(c14Sat.Aodc - DParse("0.000000000000E+00")) < Double.Epsilon);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\glo_nav.16g")]
        public void Check_Glo_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("glo_nav.16g", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Glo);
            Assert.IsTrue(metaData.IonoCorrections.Count == 0, "metaData.IonoCorrections.Count == 0");

            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 1, "metaData.TimeSystemCorrections.Count == 1");

            var timeSystemCorrection = metaData.TimeSystemCorrections[0];

            Assert.IsTrue(timeSystemCorrection.Type == CorrectionType.Glut);

            Assert.IsTrue(!timeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = timeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("-6.9849193096E-09")) < Double.Epsilon);

            var a1 = timeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("0.000000000E+00")) < Double.Epsilon);

            var refTime = timeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 0, "reftime is wrong");

            var week = timeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 0, "week is wrong");

            Assert.IsTrue(metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.FutureNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.FutureNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.Week == 1851, "metaData.LeapSeconds.Week == 1851");
            Assert.IsTrue(metaData.LeapSeconds.Day == 3, "metaData.LeapSeconds.Day == 3");

            var messages = parser.GetMessagesOfType<GloNavMessage>().ToList();
            Assert.IsTrue(messages.Count == 5, "parser.NavMessages.Count() == 5");

            var r03Sat = messages.SingleOrDefault(t => t.SatellitePrn == GloSatellite.R03);
            Assert.IsTrue(r03Sat != null, "r0314Sat != null");

            Assert.IsTrue(r03Sat.Toc == 2016, "r0314Sat.Toc == 2016");
            Assert.IsTrue(r03Sat.Time == new DateTime(2016, 01, 30, 23, 45, 00));

            Assert.IsTrue(Math.Abs(r03Sat.SvClkBias - DParse("6.122980266809E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.SvRelativeFreqBias - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.MessageFrameTime - DParse("6.030000000000E+05")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r03Sat.X - DParse("-7.220106933594E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Vx - DParse("-2.072010040283E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Ax - DParse("-9.313225746155E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Health - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r03Sat.Y - DParse(" -2.439217333984E+04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Vy - DParse("-1.823577880859E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Ay - DParse("-1.862645149231E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.FreqNumber - DParse(" 5.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r03Sat.Z - DParse("1.700746093750E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Vz - DParse("-3.559676170349E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.Az - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r03Sat.AgeOfOperInfo - DParse("0.000000000000E+00")) < Double.Epsilon);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\qzss_nav.16j")]
        public void Check_Qzss_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("qzss_nav.16j", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Qzss);
            Assert.IsTrue(metaData.IonoCorrections.Count == 1, "metaData.IonoCorrections.Count == 1");

            var qzssCorrections = metaData.IonoCorrections[0];

            var alpha0 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse("0.0000E+00")) < Double.Epsilon);

            var alpha1 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse("0.0000E+00")) < Double.Epsilon);

            var alpha2 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("0.0000E+00")) < Double.Epsilon);

            var alpha3 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse("0.0000E+00")) < Double.Epsilon);

            var beta0 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta0];
            Assert.IsTrue(Math.Abs(beta0 - DParse("0.0000E+00")) < Double.Epsilon);

            var beta1 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta1];
            Assert.IsTrue(Math.Abs(beta1 - DParse("0.0000E+00")) < Double.Epsilon);

            var beta2 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta2];
            Assert.IsTrue(Math.Abs(beta2 - DParse("0.0000E+00")) < Double.Epsilon);

            var beta3 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta3];
            Assert.IsTrue(Math.Abs(beta3 - DParse("0.0000E+00")) < Double.Epsilon);


            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 1, "metaData.TimeSystemCorrections.Count == 1");
            var timeSystemCorrection = metaData.TimeSystemCorrections[0];

            Assert.IsTrue(timeSystemCorrection.Type == CorrectionType.Qzut);

            Assert.IsTrue(!timeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = timeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("0.0000E+00")) < Double.Epsilon);

            var a1 = timeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("0.0000E+00")) < Double.Epsilon);

            var refTime = timeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 0, "reftime is wrong");

            var week = timeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 0, "week is wrong");

            Assert.IsTrue(metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17");


            var messages = parser.GetMessagesOfType<QzssNavMessage>().ToList();
            Assert.IsTrue(messages.Count == 5, "parser.NavMessages.Count() == 5");

            var time = new DateTime(2016, 05, 05, 22, 45, 04);
            var j01Sat = messages.SingleOrDefault(t => t.SatellitePrn == QzssSatellite.J01 && t.Time == time);
            Assert.IsTrue(j01Sat != null, "r0314Sat != null");

            Assert.IsTrue(j01Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(j01Sat.Time == time);

            Assert.IsTrue(Math.Abs(j01Sat.SvClkBias - DParse("-0.327751506120E-03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.SvClkDrift - DParse("0.341060513165E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.SvClkDriftRate - DParse("-0.277555756156E-16")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Iode - DParse("0.184000000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Crs - DParse("0.254937500000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.DeltaN - DParse("0.227723771323E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.M0 - DParse("-0.178054005153E+01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Cuc - DParse("0.785849988461E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Ecc - DParse("0.751390391961E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Cus - DParse("0.785849988461E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.SqrtA - DParse("0.649280592537E+04")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Toe - DParse("0.427504000000E+06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Cic - DParse("0.118836760521E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.OmegaA - DParse("-0.820196472864E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Cis - DParse("-0.152736902237E-05")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.I0 - DParse("0.709918807082E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Crc - DParse("-0.111125000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Omega - DParse("-0.157815543818E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.OmegaDot - DParse("-0.200508351978E-08")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Idot - DParse("0.758245869698E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.CodesOnL2 - DParse("0.200000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.GpsWeek - DParse("0.189500000000E+04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.L2Pflag - DParse("0.100000000000E+01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Accuracy - DParse("0.240000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Health - DParse("0.100000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Tgd - DParse("-0.512227416039E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.Iodc - DParse("0.184000000000E+03")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(j01Sat.Ttom - DParse(" 0.423936000000E+06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(j01Sat.FitIntervalFlag - DParse("0.400000000000E+01")) < Double.Epsilon);
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\gal_nav.16e")]
        public void Check_Gal_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("gal_nav.16e", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Gal);
            Assert.IsTrue(metaData.IonoCorrections.Count == 1, "metaData.IonoCorrections.Count == 1");

            var qzssCorrections = metaData.IonoCorrections[0];

            var alpha0 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse("7.3250E+01")) < Double.Epsilon);

            var alpha1 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse("-5.2734E-01")) < Double.Epsilon);

            var alpha2 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("8.9111E-03")) < Double.Epsilon);

            var alpha3 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse("0.0000E+00 ")) < Double.Epsilon);

            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 2, "metaData.TimeSystemCorrections.Count == 2");

            var gautTimeSystemCorrection = metaData.TimeSystemCorrections[0];
            Assert.IsTrue(gautTimeSystemCorrection.Type == CorrectionType.Gaut);
            Assert.IsTrue(!gautTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = gautTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("-3.7252902985E-09")) < Double.Epsilon);

            var a1 = gautTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("8.881784197E-16")) < Double.Epsilon);

            var refTime = gautTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 518400, "reftime is wrong");

            var week = gautTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1881, "week is wrong");

            var gpgaTimeSystemCorrection = metaData.TimeSystemCorrections[1];
            Assert.IsTrue(gpgaTimeSystemCorrection.Type == CorrectionType.Gpga);
            Assert.IsTrue(!gpgaTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            a0 = gpgaTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("1.7462298274E-10")) < Double.Epsilon);

            a1 = gpgaTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("4.884981308E-15")) < Double.Epsilon);

            refTime = gpgaTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 518400, "reftime is wrong");

            week = gpgaTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1881, "week is wrong");


            Assert.IsTrue(metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.FutureNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.FutureNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.Week == 1851,
                "metaData.LeapSeconds.Week == 1851");
            Assert.IsTrue(metaData.LeapSeconds.Day == 3,
                "metaData.LeapSeconds.Day == 3");


            var messages = parser.GetMessagesOfType<GalNavMessage>().ToList();
            Assert.IsTrue(messages.Count == 5, "parser.NavMessages.Count() == 5");

            var e19Sat = messages.SingleOrDefault(t => t.SatellitePrn == GalSatellite.E19);
            Assert.IsTrue(e19Sat != null, "r0314Sat != null");

            Assert.IsTrue(e19Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(e19Sat.Time == new DateTime(2016, 01, 30, 16, 00, 00));

            Assert.IsTrue(Math.Abs(e19Sat.SvClkBias - DParse("-1.268420601264E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.SvClkDrift - DParse("-1.591615728103E-12")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.SvClkDriftRate - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.IodNav - DParse(" 6.400000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Crs - DParse("4.078125000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.DeltaN - DParse("3.233348967677E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.M0 - DParse("6.370951810458E-01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.Cuc - DParse("1.909211277962E-06 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Ecc - DParse("2.753721782938E-04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Cus - DParse("9.970739483833E-06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.SqrtA - DParse("5.440621404648E+03")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.Toe - DParse("5.760000000000E+05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Cic - DParse("-1.490116119385E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Omega0 - DParse("1.453905174106E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Cis - DParse("3.725290298462E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.I0 - DParse(" 9.586963051149E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Crc - DParse("1.268750000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Omega - DParse("2.424610592752E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.OmegaDot - DParse("-5.536302037774E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.Idot - DParse(" 8.357490980188E-11")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.DataSourcesRaw - DParse("5.190000000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.GalWeek - DParse("1.881000000000E+03")) < Double.Epsilon);


            Assert.IsTrue(Math.Abs(e19Sat.Accuracy - DParse("5.520000000000E+0")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.Health - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.BgdE5A - DParse("-6.984919309616E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e19Sat.BgdE5B - DParse("-7.450580596924E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e19Sat.Ttom - DParse(" 5.767240000000E+05")) < Double.Epsilon);
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\gps_nav.16n")]
        public void Check_Gps_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("gps_nav.16n", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Gps);
            Assert.IsTrue(metaData.IonoCorrections.Count == 1, "metaData.IonoCorrections.Count == 1");

            var gpsCorrections = metaData.IonoCorrections[0];

            var alpha0 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse("1.2107E-08")) < Double.Epsilon);

            var alpha1 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse("-7.4506E-09")) < Double.Epsilon);

            var alpha2 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("-5.9605E-08")) < Double.Epsilon);

            var alpha3 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse("5.9605E-08")) < Double.Epsilon);

            var beta0 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta0];
            Assert.IsTrue(Math.Abs(beta0 - DParse("1.1469E+05")) < Double.Epsilon);

            var beta1 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta1];
            Assert.IsTrue(Math.Abs(beta1 - DParse("-1.3107E+05")) < Double.Epsilon);

            var beta2 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta2];
            Assert.IsTrue(Math.Abs(beta2 - DParse("-2.6214E+05")) < Double.Epsilon);

            var beta3 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta3];
            Assert.IsTrue(Math.Abs(beta3 - DParse(" 7.2090E+05")) < Double.Epsilon);

            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 1, "metaData.TimeSystemCorrections.Count == 1");

            var gautTimeSystemCorrection = metaData.TimeSystemCorrections[0];
            Assert.IsTrue(gautTimeSystemCorrection.Type == CorrectionType.Gput);
            Assert.IsTrue(!gautTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = gautTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("4.6566128731E-09")) < Double.Epsilon);

            var a1 = gautTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse("7.993605777E-15")) < Double.Epsilon);

            var refTime = gautTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 233472, "reftime is wrong");

            var week = gautTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1882, "week is wrong");

            Assert.IsTrue(metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.FutureNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.FutureNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.Week == 1851,
                "metaData.LeapSeconds.Week == 1851");
            Assert.IsTrue(metaData.LeapSeconds.Day == 3,
                "metaData.LeapSeconds.Day == 3");


            var messages = parser.GetMessagesOfType<GpsNavMessage>().ToList();
            Assert.IsTrue(messages.Count == 5, "parser.NavMessages.Count() == 5");

            var g03Sat = messages.SingleOrDefault(t => t.SatellitePrn == GpsSatellite.G03);
            Assert.IsTrue(g03Sat != null, "r0314Sat != null");

            Assert.IsTrue(g03Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(g03Sat.Time == new DateTime(2016, 01, 30, 23, 59, 44));

            Assert.IsTrue(Math.Abs(g03Sat.SvClkBias - DParse("-1.846300438046E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.SvClkDrift - DParse("-4.888534022029E-12")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.SvClkDriftRate - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Iode - DParse("2.300000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Crs - DParse("-8.968750000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.DeltaN - DParse("4.407326439980E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.M0 - DParse(" 5.156316281613E-01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Cuc - DParse("-4.190951585770E-07 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Ecc - DParse("2.960053971037E-04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Cus - DParse("9.540468454361E-06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.SqrtA - DParse("5.153778657913E+03")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Toe - DParse("6.047840000000E+05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Cic - DParse("5.587935447693E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Omega0 - DParse("1.009009558624E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Cis - DParse("3.166496753693E-08")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.I0 - DParse(" 9.592027190777E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Crc - DParse("1.950312500000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Omega - DParse("2.974211131900E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.OmegaDot - DParse("-7.959617264294E-09")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Idot - DParse(" 1.535778257043E-11")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.CodesOnL2 - DParse(" 1.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.GpsWeek - DParse("1.881000000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.L2Pflag - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Accuracy - DParse("2.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Health - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Tgd - DParse("1.862645149231E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g03Sat.Iodc - DParse("2.300000000000E+01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g03Sat.Ttom - DParse("5.976000000000E+05")) < Double.Epsilon);
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Nav\\mixed_nav.16m")]
        public void Check_Mixed_Nav_File_Parse_StoreMode()
        {
            var parser = new RinexNavParser("mixed_nav.16m", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Nav);

            var metaData = parser.NavHeader.NavHeaderData;
            Assert.IsTrue(metaData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(metaData.IonoCorrections.Count == 3, "metaData.IonoCorrections.Count == 3");

            var gpsCorrections = metaData.IonoCorrections[0]; //gps iono corrections

            var alpha0 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse(".1397E-07")) < Double.Epsilon);

            var alpha1 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse(".2235E-07")) < Double.Epsilon);

            var alpha2 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("-.1192E-06")) < Double.Epsilon);

            var alpha3 = gpsCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse("-.1192E-06")) < Double.Epsilon);

            var beta0 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta0];
            Assert.IsTrue(Math.Abs(beta0 - DParse(".1044E+06")) < Double.Epsilon);

            var beta1 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta1];
            Assert.IsTrue(Math.Abs(beta1 - DParse(".9830E+05")) < Double.Epsilon);

            var beta2 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta2];
            Assert.IsTrue(Math.Abs(beta2 - DParse("-.1966E+06")) < Double.Epsilon);

            var beta3 = gpsCorrections.Corrections[IonoCorrectionsEnum.Beta3];
            Assert.IsTrue(Math.Abs(beta3 - DParse("-.3932E+06")) < Double.Epsilon);

            var galCorrections = metaData.IonoCorrections[1];
            alpha0 = galCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse(".3750E+02")) < Double.Epsilon);

            alpha1 = galCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse(".1953E-01")) < Double.Epsilon);

            alpha2 = galCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse(".1657E-01")) < Double.Epsilon);

            alpha3 = galCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse(".0000E+00")) < Double.Epsilon);

            var qzssCorrections = metaData.IonoCorrections[2];

            alpha0 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha0];
            Assert.IsTrue(Math.Abs(alpha0 - DParse(".2980E-07")) < Double.Epsilon);

            alpha1 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha1];
            Assert.IsTrue(Math.Abs(alpha1 - DParse("-.2012E-06")) < Double.Epsilon);

            alpha2 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha2];
            Assert.IsTrue(Math.Abs(alpha2 - DParse("-.1192E-06")) < Double.Epsilon);

            alpha3 = qzssCorrections.Corrections[IonoCorrectionsEnum.Alpha3];
            Assert.IsTrue(Math.Abs(alpha3 - DParse(".1669E-05")) < Double.Epsilon);

            beta0 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta0];
            Assert.IsTrue(Math.Abs(beta0 - DParse(".1188E+06")) < Double.Epsilon);

            beta1 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta1];
            Assert.IsTrue(Math.Abs(beta1 - DParse("-.4588E+06")) < Double.Epsilon);

            beta2 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta2];
            Assert.IsTrue(Math.Abs(beta2 - DParse(".1507E+07")) < Double.Epsilon);

            beta3 = qzssCorrections.Corrections[IonoCorrectionsEnum.Beta3];
            Assert.IsTrue(Math.Abs(beta3 - DParse(".8323E+07")) < Double.Epsilon);


            Assert.IsTrue(metaData.TimeSystemCorrections.Count == 3, "metaData.TimeSystemCorrections.Count == 3");

            var gputTimeSystemCorrection = metaData.TimeSystemCorrections[0];
            Assert.IsTrue(gputTimeSystemCorrection.Type == CorrectionType.Gput);
            Assert.IsTrue(!gputTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            var a0 = gputTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse(".1862645149E-08")) < Double.Epsilon);

            var a1 = gputTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse(".621724894E-14")) < Double.Epsilon);

            var refTime = gputTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 61440, "reftime is wrong");

            var week = gputTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1895, "week is wrong");

            var gautTimeSystemCorrection = metaData.TimeSystemCorrections[1];
            Assert.IsTrue(gautTimeSystemCorrection.Type == CorrectionType.Gaut);
            Assert.IsTrue(!gautTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            a0 = gautTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse("-.1862645149E-08")) < Double.Epsilon);

            a1 = gautTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse(".266453526E-14")) < Double.Epsilon);

            refTime = gautTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 345600, "reftime is wrong");

            week = gautTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1894, "week is wrong");

            var qzutTimeSystemCorrection = metaData.TimeSystemCorrections[2];
            Assert.IsTrue(qzutTimeSystemCorrection.Type == CorrectionType.Qzut);
            Assert.IsTrue(!qzutTimeSystemCorrection.UtcId.HasValue, "!timeSystemCorrection.UtcId.HasValue");

            a0 = qzutTimeSystemCorrection.A0;
            Assert.IsTrue(Math.Abs(a0 - DParse(".2514570951E-07")) < Double.Epsilon);

            a1 = qzutTimeSystemCorrection.A1;
            Assert.IsTrue(Math.Abs(a1 - DParse(".266453526E-14")) < Double.Epsilon);

            refTime = qzutTimeSystemCorrection.ReferenceTime;
            Assert.IsTrue(refTime == 53248, "reftime is wrong");

            week = qzutTimeSystemCorrection.ReferenceWeek;
            Assert.IsTrue(week == 1895, "week is wrong");

            Assert.IsTrue(metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.CurrentNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.FutureNumOfLeapSeconds == 17,
                "metaData.LeapSeconds.FutureNumOfLeapSeconds == 17");
            Assert.IsTrue(metaData.LeapSeconds.Week == 1851,
                "metaData.LeapSeconds.Week == 1851");
            Assert.IsTrue(metaData.LeapSeconds.Day == 3,
                "metaData.LeapSeconds.Day == 3");


            var gpsNavMessages = parser.GetMessagesOfType<GpsNavMessage>().ToList();
            Assert.IsTrue(gpsNavMessages.Count == 1, "parser.NavMessages.Count() == 1");

            var g18Sat = gpsNavMessages.SingleOrDefault(t => t.SatellitePrn == GpsSatellite.G18);
            Assert.IsTrue(g18Sat != null, "r0314Sat != null");

            Assert.IsTrue(g18Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(g18Sat.Time == new DateTime(2016, 04, 29, 00, 00, 00));

            Assert.IsTrue(Math.Abs(g18Sat.SvClkBias - DParse(".509812962264E-03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.SvClkDrift - DParse(".420641299570E-11")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.SvClkDriftRate - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Iode - DParse(".820000000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Crs - DParse(".170625000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.DeltaN - DParse(".545022702383E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.M0 - DParse("-.161324860902E+01")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Cuc - DParse(".979751348495E-06 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Ecc - DParse(".170789742842E-01 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Cus - DParse(".687129795551E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.SqrtA - DParse(".515367198372E+04")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Toe - DParse(".432000000000E+06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Cic - DParse(".437721610069E-06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Omega0 - DParse("-.649963078691E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Cis - DParse(".242143869400E-06")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.I0 - DParse(" .925013005476E+00 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Crc - DParse(".226750000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Omega - DParse("-.189256608436E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.OmegaDot - DParse("-.834963351004E-08")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Idot - DParse(" .417874549009E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.CodesOnL2 - DParse(" .100000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.GpsWeek - DParse(".189400000000E+04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.L2Pflag - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Accuracy - DParse(".240000000000E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Health - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Tgd - DParse("-.121071934700E-07")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Iodc - DParse(".820000000000E+02")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(g18Sat.Ttom - DParse(".428556000000E+06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(g18Sat.Fiti - DParse(".400000000000E+01")) < Double.Epsilon);


            var gloNavMessages = parser.GetMessagesOfType<GloNavMessage>().ToList();
            Assert.IsTrue(gloNavMessages.Count == 1, "parser.NavMessages.Count() == 1");

            var r08Sat = gloNavMessages.SingleOrDefault(t => t.SatellitePrn == GloSatellite.R08);
            Assert.IsTrue(r08Sat != null, "r0314Sat != null");

            Assert.IsTrue(r08Sat.Toc == 2016, "r0314Sat.Toc == 2016");
            Assert.IsTrue(r08Sat.Time == new DateTime(2016, 04, 28, 23, 45, 00));

            Assert.IsTrue(Math.Abs(r08Sat.SvClkBias - DParse("-.271238386631E-04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.SvRelativeFreqBias - DParse("0.000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.MessageFrameTime - DParse(".863700000000E+05")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r08Sat.X - DParse("-.247477207031E+05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Vx - DParse(".516687393188E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Ax - DParse(".000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Health - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r08Sat.Y - DParse(" .505213964844E+04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Vy - DParse(".214462280273E-01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Ay - DParse(".000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.FreqNumber - DParse(" 6.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(r08Sat.Z - DParse("-.354245654297E+04")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Vz - DParse("-.352548027039E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.Az - DParse(".931322574615E-09")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(r08Sat.AgeOfOperInfo - DParse("0.000000000000E+00")) < Double.Epsilon);


            var galNavMessages = parser.GetMessagesOfType<GalNavMessage>().ToList();
            Assert.IsTrue(galNavMessages.Count == 1, "parser.NavMessages.Count() == 1");

            var e11Sat = galNavMessages.SingleOrDefault(t => t.SatellitePrn == GalSatellite.E11);
            Assert.IsTrue(e11Sat != null, "r0314Sat != null");

            Assert.IsTrue(e11Sat.Toc == 2016, "c14Sat.Toc == 2016");
            Assert.IsTrue(e11Sat.Time == new DateTime(2016, 04, 28, 23, 40, 00));

            Assert.IsTrue(Math.Abs(e11Sat.SvClkBias - DParse(".122322700918E-03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.SvClkDrift - DParse(".109707798401E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.SvClkDriftRate - DParse("0.000000000000E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.IodNav - DParse(" .780000000000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Crs - DParse("-.994062500000E+02")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.DeltaN - DParse(".313513059077E-08")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.M0 - DParse("-.723509579364E+00")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.Cuc - DParse("-.465661287308E-05 ")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Ecc - DParse(".401845434681E-03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Cus - DParse(".869110226631E-05")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.SqrtA - DParse(".544061901093E+04")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.Toe - DParse(".430800000000E+06")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Cic - DParse("-.316649675369E-07")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Omega0 - DParse("-.225263492898E+01")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Cis - DParse(".484287738800E-07")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.I0 - DParse(" .968899754952E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Crc - DParse(".159687500000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Omega - DParse("-.310683686823E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.OmegaDot - DParse("-.554773108527E-08")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.Idot - DParse("-.146434671020E-10")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.DataSourcesRaw - DParse(".513000000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.GalWeek - DParse(".189400000000E+04")) < Double.Epsilon);


            Assert.IsTrue(Math.Abs(e11Sat.Accuracy - DParse(".107000000000E+03")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.Health - DParse(".000000000000E+00")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.BgdE5A - DParse("-.158324837685E-07")) < Double.Epsilon);
            Assert.IsTrue(Math.Abs(e11Sat.BgdE5B - DParse("-.174622982740E-07")) < Double.Epsilon);

            Assert.IsTrue(Math.Abs(e11Sat.Ttom - DParse(".431485000000E+06")) < Double.Epsilon);
        }

        #endregion
    }
}