// Rinex3ParserTest
// RinexObsParserTest.cs-2016-09-05

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rinex3Parser.Common;
using Rinex3Parser.Nav;
using Rinex3Parser.Obs;
// ReSharper disable UnusedParameter.Local


#endregion

namespace Rinex3ParserTest
{
    [TestClass]
    public class RinexObsParserTest
    {
        #region Methods

        [TestMethod, ExpectedException(typeof(RinexParserException))]
        public void Test_Rinex_Obs_Null_Exception_Raised()
        {
            var parser = new RinexObsParser(null, ParseType.RaiseEvents);
            parser.Parse();
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Test_Rinex_Successfull_Mixed_Parse()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.RaiseEvents);
            parser.Parse();
            Assert.IsTrue(true);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed2.16o")]
        public void Test_Rinex_Successfull_Mixed2_Parse()
        {
            var parser = new RinexObsParser("valid_mixed2.16o", ParseType.RaiseEvents);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed2.16o"),ExpectedException(typeof(ArgumentException))]
        public void Test_Invalid_Satellite_Constellation_Exception_Raised()
        {
            var parser = new RinexObsParser("valid_mixed2.16o", ParseType.RaiseEvents);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            parser.GetSatellitesObservations<Object>();//some dumb class
        }



        [TestMethod, ExpectedException(typeof(RinexParserException)),
         DeploymentItem(@"TestFiles\\Obs\\Full\\inconsitent_gps.16o")]
        public void Test_Inconsistent_Observations_Exception_Raised_()
        {
            var parser = new RinexObsParser("inconsitent_gps.16o", ParseType.RaiseEvents);
            parser.Parse();
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_gps.16o")]
        public void Check_Pure_Gps_Observations_File_With_StoreMode()
        {
            var parser = new RinexObsParser("pure_gps.16o", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Gps);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            Assert.IsTrue(parser.ObservationRecords.Count == 19);

            var metaData = parser.ObsHeader.ObsHeaderData.ObsMetaData;

            CheckObservations(metaData, SatelliteSystem.Gps, new[]
            {
                ObservationCode.C1C, ObservationCode.L1C, ObservationCode.D1C,
                ObservationCode.S1C, ObservationCode.C2S, ObservationCode.L2S,
                ObservationCode.D2S, ObservationCode.S2S, ObservationCode.C2W,
                ObservationCode.L2W, ObservationCode.D2W, ObservationCode.S2W,
                ObservationCode.C5Q, ObservationCode.L5Q, ObservationCode.D5Q,
                ObservationCode.S5Q
            });

            var sysPhaseShift = parser.ObsHeader.ObsHeaderData.PhaseShiftDict;
            Assert.IsTrue(sysPhaseShift.ContainsKey(SatelliteSystem.Gps));

            var temp = sysPhaseShift[SatelliteSystem.Gps].SingleOrDefault(t => t.ObservationCode == ObservationCode.L2S);
            Assert.IsTrue(temp != null);
            Assert.IsTrue(temp.ObservationCode == ObservationCode.L2S);
            Assert.IsTrue(Math.Abs(temp.CorrectionCycles - -0.25) <= Double.Epsilon);

            temp = sysPhaseShift[SatelliteSystem.Gps].SingleOrDefault(t => t.ObservationCode == ObservationCode.L2X);
            Assert.IsTrue(temp != null);
            Assert.IsTrue(temp.ObservationCode == ObservationCode.L2X);
            Assert.IsTrue(Math.Abs(temp.CorrectionCycles - -0.25) <= Double.Epsilon);


            var gpsObsData = parser.GetSatellitesObservations<GpsSatellite>();
            CollectionAssert.AllItemsAreNotNull(gpsObsData.Values as ICollection);
            CollectionAssert.AllItemsAreInstancesOfType(gpsObsData.Values.SelectMany(t => t).ToArray(),
                typeof(SatelliteObs<GpsSatellite>));

            var observedSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedSatellites.ContainsKey(SatelliteSystem.Gps));
            var galPrns = observedSatellites[SatelliteSystem.Gps];
            CollectionAssert.AreEqual(galPrns.SatellitePrn<GpsSatellite>().ToList(),
                new[]
                {
                    GpsSatellite.G05, GpsSatellite.G12, GpsSatellite.G14, GpsSatellite.G15, GpsSatellite.G18,
                    GpsSatellite.G20, GpsSatellite.G21, GpsSatellite.G24, GpsSatellite.G25, GpsSatellite.G29,
                    GpsSatellite.G31, GpsSatellite.G32
                });


            Assert.IsTrue(gpsObsData.Count == 19);
            foreach (var satData in gpsObsData.Values)
            {
                Assert.IsTrue(satData.Count == 12);
            }
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_gps.16o")]
        public void Check_Pure_Gps_Observations_File_With_RaiseMode()
        {
            var parser = new RinexObsParser("pure_gps.16o", ParseType.RaiseEvents);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Gps);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            Assert.IsTrue(parser.ObservationRecords.Count == 0); //raise mode, no internal storage
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_glo.16o")]
        public void Check_Pure_Glo_Observations_File_With_StoreMode()
        {
            var parser = new RinexObsParser("pure_glo.16o", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Glo);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Glo);
            Assert.IsTrue(parser.ObservationRecords.Count == 19);

            var metaData = parser.ObsHeader.ObsHeaderData.ObsMetaData;

            CheckObservations(metaData, SatelliteSystem.Glo, new[]
            {
                ObservationCode.C1C, ObservationCode.L1C, ObservationCode.D1C,
                ObservationCode.S1C, ObservationCode.C2P, ObservationCode.L2P,
                ObservationCode.D2P, ObservationCode.S2P
            });

            var phaseCorrections = parser.ObsHeader.ObsHeaderData.GloPhaseBiasCor;

            Assert.IsTrue(phaseCorrections.ContainsKey(ObservationCode.C1C));
            Assert.IsTrue(Math.Abs(phaseCorrections[ObservationCode.C1C] - -71.940) < Double.Epsilon);

            Assert.IsTrue(phaseCorrections.ContainsKey(ObservationCode.C1P));
            Assert.IsTrue(Math.Abs(phaseCorrections[ObservationCode.C1P] - -71.940) < Double.Epsilon);

            Assert.IsTrue(phaseCorrections.ContainsKey(ObservationCode.C2C));
            Assert.IsTrue(Math.Abs(phaseCorrections[ObservationCode.C2C] - -71.940) < Double.Epsilon);

            Assert.IsTrue(phaseCorrections.ContainsKey(ObservationCode.C2P));
            Assert.IsTrue(Math.Abs(phaseCorrections[ObservationCode.C2P] - -71.940) < Double.Epsilon);

            var slotFrqKeys = parser.ObsHeader.ObsHeaderData.GloSlotFreqDict.Keys;
            var slotFrqValues = parser.ObsHeader.ObsHeaderData.GloSlotFreqDict.Values;

            var expected = new List<GloSatellite>();
            var temp = Enum.GetValues(typeof(GloSatellite));
            for (var i = 0; i < 24; i++)
            {
                expected.Add((GloSatellite)temp.GetValue(i));
            }

            CollectionAssert.AreEqual(expected, slotFrqKeys);
            CollectionAssert.AreEqual(
                new[] {1, -4, 5, 6, 1, -4, 5, 6, -6, -7, 0, -1, -2, -7, 0, -1, 4, -3, 3, 2, 4, -3, 3, 2}, slotFrqValues);

            var gloSatData = parser.GetSatellitesObservations<GloSatellite>();
            CollectionAssert.AllItemsAreNotNull(gloSatData.Values as ICollection);
            CollectionAssert.AllItemsAreInstancesOfType(gloSatData.Values.SelectMany(t => t).ToArray(),
                typeof(SatelliteObs<GloSatellite>));


            var observedSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedSatellites.ContainsKey(SatelliteSystem.Glo));
            var galPrns = observedSatellites[SatelliteSystem.Glo];
            CollectionAssert.AreEqual(galPrns.SatellitePrn<GloSatellite>().ToList(),
                new[]
                {
                    GloSatellite.R01, GloSatellite.R08, GloSatellite.R10, GloSatellite.R11, GloSatellite.R13,
                    GloSatellite.R22, GloSatellite.R23, GloSatellite.R24
                });


            Assert.IsTrue(gloSatData.Count == 19);
            foreach (var satData in gloSatData.Values)
            {
                Assert.IsTrue(satData.Count == 8 || satData.Count == 7);
            }
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_bds.16o")]
        public void Check_Pure_Bds_Observations_File_With_StoreMode()
        {
            var parser = new RinexObsParser("pure_bds.16o", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Bds);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Bdt);
            Assert.IsTrue(parser.ObservationRecords.Count == 26);

            var metaData = parser.ObsHeader.ObsHeaderData.ObsMetaData;

            CheckObservations(metaData, SatelliteSystem.Bds, new[]
            {
                ObservationCode.C1I, ObservationCode.L1I, ObservationCode.D1I,
                ObservationCode.S1I, ObservationCode.C7I, ObservationCode.L7I,
                ObservationCode.D7I, ObservationCode.S7I
            });

            var bdsSatData = parser.GetSatellitesObservations<BdsSatellite>();

            CollectionAssert.AllItemsAreNotNull(bdsSatData.Values as ICollection);
            CollectionAssert.AllItemsAreInstancesOfType(bdsSatData.Values.SelectMany(t => t).ToArray(),
                typeof(SatelliteObs<BdsSatellite>));

            var observedSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedSatellites.ContainsKey(SatelliteSystem.Bds));
            var galPrns = observedSatellites[SatelliteSystem.Bds];
            CollectionAssert.AreEqual(galPrns.SatellitePrn<BdsSatellite>().ToList(),
                new[]
                {
                    BdsSatellite.C01, BdsSatellite.C02, BdsSatellite.C03, BdsSatellite.C05, BdsSatellite.C07,
                    BdsSatellite.C08, BdsSatellite.C10, BdsSatellite.C12,
                });

            Assert.IsTrue(bdsSatData.Count == 26);
            foreach (var satData in bdsSatData.Values)
            {
                Assert.IsTrue(satData.Count == 8);
            }
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_qzss.16o")]
        public void Check_Pure_Qzss_Observations_File_With_StoreMode()
        {
            var parser = new RinexObsParser("pure_qzss.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Qzss);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Qzs);
            Assert.IsTrue(parser.ObservationRecords.Count == 26);

            var metaData = parser.ObsHeader.ObsHeaderData.ObsMetaData;

            CheckObservations(metaData, SatelliteSystem.Qzss, new[]
            {
                ObservationCode.C1C, ObservationCode.L1C, ObservationCode.D1C,
                ObservationCode.S1C, ObservationCode.C2S, ObservationCode.L2S,
                ObservationCode.D2S, ObservationCode.S2S, ObservationCode.C5Q,
                ObservationCode.L5Q, ObservationCode.D5Q, ObservationCode.S5Q
            });

            var qzssSatData = parser.GetSatellitesObservations<QzssSatellite>();
            CollectionAssert.AllItemsAreNotNull(qzssSatData.Values as ICollection);
            CollectionAssert.AllItemsAreInstancesOfType(qzssSatData.Values.SelectMany(t => t).ToArray(),
                typeof(SatelliteObs<QzssSatellite>));


            var observedSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedSatellites.ContainsKey(SatelliteSystem.Qzss));
            var galPrns = observedSatellites[SatelliteSystem.Qzss];
            CollectionAssert.AreEqual(galPrns.SatellitePrn<QzssSatellite>().ToList(), new[] {QzssSatellite.J01});

            Assert.IsTrue(qzssSatData.Count == 26);
            foreach (var satData in qzssSatData.Values)
            {
                Assert.IsTrue(satData.Count == 1);
            }
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_gal.16o")]
        public void Check_Pure_Gal_Observations_File_With_StoreMode()
        {
            var parser = new RinexObsParser("pure_gal.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Gal);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gal);
            Assert.IsTrue(parser.ObservationRecords.Count == 26);

            var metaData = parser.ObsHeader.ObsHeaderData.ObsMetaData;

            CheckObservations(metaData, SatelliteSystem.Gal, new[]
            {
                ObservationCode.C1C, ObservationCode.L1C, ObservationCode.D1C,
                ObservationCode.S1C, ObservationCode.C5Q, ObservationCode.L5Q,
                ObservationCode.D5Q, ObservationCode.S5Q, ObservationCode.C7Q,
                ObservationCode.L7Q, ObservationCode.D7Q, ObservationCode.S7Q,
                ObservationCode.C8Q, ObservationCode.L8Q, ObservationCode.D8Q,
                ObservationCode.S8Q
            });

            var sysPhaseShift = parser.ObsHeader.ObsHeaderData.PhaseShiftDict;
            Assert.IsTrue(sysPhaseShift.ContainsKey(SatelliteSystem.Gal));

            var temp = sysPhaseShift[SatelliteSystem.Gal].SingleOrDefault(t => t.ObservationCode == ObservationCode.L8Q);
            Assert.IsTrue(temp != null);
            Assert.IsTrue(temp.ObservationCode == ObservationCode.L8Q);
            Assert.IsTrue(Math.Abs(temp.CorrectionCycles - -0.25) <= Double.Epsilon);

            var galSatData = parser.GetSatellitesObservations<GalSatellite>();
            CollectionAssert.AllItemsAreNotNull(galSatData.Values as ICollection);
            CollectionAssert.AllItemsAreInstancesOfType(galSatData.Values.SelectMany(t => t).ToArray(),
                typeof(SatelliteObs<GalSatellite>));

            var observedSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedSatellites.ContainsKey(SatelliteSystem.Gal));
            IEnumerable<int> galPrns = observedSatellites[SatelliteSystem.Gal];
            CollectionAssert.AreEqual(galPrns.SatellitePrn<GalSatellite>().ToList(),
                new[] {GalSatellite.E12, GalSatellite.E19, GalSatellite.E20});

            Assert.IsTrue(galSatData.Count == 26);
            foreach (var satData in galSatData.Values)
            {
                Assert.IsTrue(satData.Count == 3);
            }
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Gps_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
             G   16 C1C L1C D1C S1C C2S L2S D2S S2S C2W L2W D2W S2W C5Q  SYS / # / OBS TYPES
                    L5Q D5Q S5Q                                          SYS / # / OBS TYPES
             */

            var gpsSatellites = data.GetSatellitesObs<GpsSatellite>();
            var satellitesObs = gpsSatellites as SatelliteObs<GpsSatellite>[] ?? gpsSatellites.ToArray();
            var g05Sat = satellitesObs.SingleOrDefault(t => t.Prn == GpsSatellite.G05);
            Assert.IsTrue(g05Sat != null);
            Assert.IsTrue(g05Sat.Observations.Count == 12);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.C1C], null, null, 25159195.842);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.L1C], 0, 6, 132212421.188);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.D1C], null, null, -654.827);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.S1C], null, null, 41.550);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.C2S));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.C2S], null, null, 25159196.028);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.L2S));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.L2S], 0, 6, 103022649.726);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.D2S));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.D2S], null, null, -510.258);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.S2S));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.S2S], null, null, 38.250);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.C2W));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.C2W], null, null, 25159195.346);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.L2W));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.L2W], 0, 5, 103022636.717);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.D2W));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.D2W], null, null, -510.260);

            Assert.IsTrue(g05Sat.Observations.ContainsKey(ObservationCode.S2W));
            CheckObsValueImpl(g05Sat.Observations[ObservationCode.S2W], null, null, 34.100);

            Assert.IsTrue(!g05Sat.Observations.ContainsKey(ObservationCode.C5Q));
            Assert.IsTrue(!g05Sat.Observations.ContainsKey(ObservationCode.L5Q));
            Assert.IsTrue(!g05Sat.Observations.ContainsKey(ObservationCode.D5Q));
            Assert.IsTrue(!g05Sat.Observations.ContainsKey(ObservationCode.S5Q));

            var g18Sat = satellitesObs.SingleOrDefault(t => t.Prn == GpsSatellite.G18);
            Assert.IsTrue(g18Sat != null);
            Assert.IsTrue(g18Sat.Observations.Count == 8);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.C1C], null, null, 23727474.366);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.L1C], 0, 7, 124688687.832);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.D1C], null, null, 2223.839);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.S1C], null, null, 47.550);

            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.C2S));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.L2S));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.D2S));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.S2S));

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.C2W));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.C2W], null, null, 23727470.160);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.L2W));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.L2W], 0, 6, 97159988.897);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.D2W));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.D2W], null, null, 1732.862);

            Assert.IsTrue(g18Sat.Observations.ContainsKey(ObservationCode.S2W));
            CheckObsValueImpl(g18Sat.Observations[ObservationCode.S2W], null, null, 37.750);

            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.C5Q));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.L5Q));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.D5Q));
            Assert.IsTrue(!g18Sat.Observations.ContainsKey(ObservationCode.S5Q));


            var g24Sat = satellitesObs.SingleOrDefault(t => t.Prn == GpsSatellite.G24);
            Assert.IsTrue(g24Sat != null);
            Assert.IsTrue(g24Sat.Observations.Count == 16);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.C1C], null, null, 22634410.714);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.L1C], 0, 7, 118944638.470);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.D1C], null, null, -3215.095);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.S1C], null, null, 46.650);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.C2S));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.C2S], null, null, 22634410.707);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.L2S));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.L2S], 0, 7, 92684144.910);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.D2S));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.D2S], null, null, -2505.273);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.S2S));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.S2S], null, null, 45.400);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.C2W));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.C2W], null, null, 22634410.171);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.L2W));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.L2W], 0, 7, 92684152.908);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.D2W));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.D2W], null, null, -2505.273);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.S2W));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.S2W], null, null, 42.200);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.C5Q));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.C5Q], null, null, 22634412.052);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.L5Q));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.L5Q], 0, 8, 88822309.461);


            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.D5Q));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.D5Q], null, null, -2400.911);

            Assert.IsTrue(g24Sat.Observations.ContainsKey(ObservationCode.S5Q));
            CheckObsValueImpl(g24Sat.Observations[ObservationCode.S5Q], null, null, 50.750);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Glo_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
             R    8 C1C L1C D1C S1C C2P L2P D2P S2P                      SYS / # / OBS TYPES
            */

            var gloSatellites = data.GetSatellitesObs<GloSatellite>();
            var satellitesObs = gloSatellites as SatelliteObs<GloSatellite>[] ?? gloSatellites.ToArray();
            var r08Sat = satellitesObs.SingleOrDefault(t => t.Prn == GloSatellite.R08);
            Assert.IsTrue(r08Sat != null);
            Assert.IsTrue(r08Sat.Observations.Count == 8);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.C1C], null, null, 24081510.753);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.L1C], 0, 6, 128955390.499);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.D1C], null, null, 1200.094);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.S1C], null, null, 40.900);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.C2P));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.C2P], null, null, 24081515.178);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.L2P));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.L2P], 0, 6, 100298616.882);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.D2P));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.D2P], null, null, 933.405);

            Assert.IsTrue(r08Sat.Observations.ContainsKey(ObservationCode.S2P));
            CheckObsValueImpl(r08Sat.Observations[ObservationCode.S2P], null, null, 37.200);

            var r11Sat = satellitesObs.SingleOrDefault(t => t.Prn == GloSatellite.R11);
            Assert.IsTrue(r11Sat != null);
            Assert.IsTrue(r11Sat.Observations.Count == 4);

            Assert.IsTrue(r11Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(r11Sat.Observations[ObservationCode.C1C], null, null, 21100648.921);

            Assert.IsTrue(r11Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(r11Sat.Observations[ObservationCode.L1C], 0, 7, 112755504.278);

            Assert.IsTrue(r11Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(r11Sat.Observations[ObservationCode.D1C], null, null, -2627.600);

            Assert.IsTrue(r11Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(r11Sat.Observations[ObservationCode.S1C], null, null, 47.200);

            Assert.IsTrue(!r11Sat.Observations.ContainsKey(ObservationCode.C2P));
            Assert.IsTrue(!r11Sat.Observations.ContainsKey(ObservationCode.L2P));
            Assert.IsTrue(!r11Sat.Observations.ContainsKey(ObservationCode.D2P));
            Assert.IsTrue(!r11Sat.Observations.ContainsKey(ObservationCode.S2P));
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Gal_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
             E   16 C1C L1C D1C S1C C5Q L5Q D5Q S5Q C7Q L7Q D7Q S7Q C8Q  SYS / # / OBS TYPES
                    L8Q D8Q S8Q                                          SYS / # / OBS TYPES
            */

            var galSatellites = data.GetSatellitesObs<GalSatellite>();
            var satellitesObs = galSatellites as SatelliteObs<GalSatellite>[] ?? galSatellites.ToArray();
            var e19Sat = satellitesObs.SingleOrDefault(t => t.Prn == GalSatellite.E19);
            Assert.IsTrue(e19Sat != null);
            Assert.IsTrue(e19Sat.Observations.Count == 16);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.C1C], null, null, 27085724.583);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.L1C], 4, 7, 142336396.105);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.D1C], null, null, 418.319);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.S1C], null, null, 46.950);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.C5Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.C5Q], null, null, 27085728.160);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.L5Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.L5Q], 0, 7, 106290153.194);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.D5Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.D5Q], null, null, 312.298);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.S5Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.S5Q], null, null, 42.600);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.C7Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.C7Q], null, null, 27085723.735);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.L7Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.L7Q], 0, 7, 109062928.151);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.D7Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.D7Q], null, null, 320.513);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.S7Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.S7Q], null, null, 44.000);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.C8Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.C8Q], null, null, 27085724.975);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.L8Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.L8Q], 0, 6, 107676533.357);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.D8Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.D8Q], null, null, 316.562);

            Assert.IsTrue(e19Sat.Observations.ContainsKey(ObservationCode.S8Q));
            CheckObsValueImpl(e19Sat.Observations[ObservationCode.S8Q], null, null, 38.750);


            var e20Sat = satellitesObs.SingleOrDefault(t => t.Prn == GalSatellite.E20);
            Assert.IsTrue(e20Sat != null);
            Assert.IsTrue(e20Sat.Observations.Count == 4);

            Assert.IsTrue(e20Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(e20Sat.Observations[ObservationCode.C1C], null, null, 24318171.235);

            Assert.IsTrue(e20Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(e20Sat.Observations[ObservationCode.L1C], 4, 7, 127792790.125);

            Assert.IsTrue(e20Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(e20Sat.Observations[ObservationCode.D1C], null, null, 587.283);

            Assert.IsTrue(e20Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(e20Sat.Observations[ObservationCode.S1C], null, null, 42.900);

            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.C5Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.L5Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.D5Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.S5Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.C7Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.L7Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.D7Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.S7Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.C8Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.L8Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.D8Q));
            Assert.IsTrue(!e20Sat.Observations.ContainsKey(ObservationCode.S8Q));
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Bds_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
                C    8 C1I L1I D1I S1I C7I L7I D7I S7I                      SYS / # / OBS TYPES
            */

            var bdsSatellites = data.GetSatellitesObs<BdsSatellite>();
            var satellitesObs = bdsSatellites as SatelliteObs<BdsSatellite>[] ?? bdsSatellites.ToArray();
            var c01Sat = satellitesObs.SingleOrDefault(t => t.Prn == BdsSatellite.C01);
            Assert.IsTrue(c01Sat != null);
            Assert.IsTrue(c01Sat.Observations.Count == 8);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.C1I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.C1I], null, null, 36995948.288);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.L1I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.L1I], 0, 7, 192647700.365);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.D1I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.D1I], null, null, -7.255);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.S1I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.S1I], null, null, 44.100);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.C7I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.C7I], null, null, 36995937.189);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.L7I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.L7I], 0, 8, 148967424.283);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.D7I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.D7I], null, null, -5.750);

            Assert.IsTrue(c01Sat.Observations.ContainsKey(ObservationCode.S7I));
            CheckObsValueImpl(c01Sat.Observations[ObservationCode.S7I], null, null, 48.950);


            var c08Sat = satellitesObs.SingleOrDefault(t => t.Prn == BdsSatellite.C08);
            Assert.IsTrue(c08Sat != null);
            Assert.IsTrue(c08Sat.Observations.Count == 8);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.C1I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.C1I], null, null, 37417763.479);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.L1I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.L1I], 0, 7, 194844130.238);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.D1I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.D1I], null, null, -1110.066);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.S1I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.S1I], null, null, 42.750);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.C7I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.C7I], null, null, 37417755.393);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.L7I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.L7I], 0, 7, 150665818.831);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.D7I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.D7I], null, null, -858.293);

            Assert.IsTrue(c08Sat.Observations.ContainsKey(ObservationCode.S7I));
            CheckObsValueImpl(c08Sat.Observations[ObservationCode.S7I], null, null, 47.500);
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Qzss_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
               J   12 C1C L1C D1C S1C C2S L2S D2S S2S C5Q L5Q D5Q S5Q      SYS / # / OBS TYPES
            */

            var bdsSatellites = data.GetSatellitesObs<QzssSatellite>();
            var satellitesObs = bdsSatellites as SatelliteObs<QzssSatellite>[] ?? bdsSatellites.ToArray();
            var j01Sat = satellitesObs.SingleOrDefault(t => t.Prn == QzssSatellite.J01);
            Assert.IsTrue(j01Sat != null);
            Assert.IsTrue(j01Sat.Observations.Count == 12);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.C1C], null, null, 38065443.439);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.L1C], 0, 8, 200035315.791);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.D1C], null, null, 1935.075);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.S1C], null, null, 48.250);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.C2S));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.C2S], null, null, 38065440.550);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.L2S));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.L2S], 0, 7, 155871641.386);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.D2S));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.D2S], null, null, 1507.850);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.S2S));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.S2S], null, null, 46.050);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.C5Q));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.C5Q], null, null, 38065443.627);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.L5Q));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.L5Q], 0, 8, 149376997.208);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.D5Q));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.D5Q], null, null, 1445.028);

            Assert.IsTrue(j01Sat.Observations.ContainsKey(ObservationCode.S5Q));
            CheckObsValueImpl(j01Sat.Observations[ObservationCode.S5Q], null, null, 50.900);
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Sbas_Observation_Data_StoreMode()
        {
            var parser = new RinexObsParser("valid_mixed.16o", ParseType.StoreData);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);
            //> 2016 03 28 00 34 30.0000000  0 35  -- just random epoch
            var epoch =
                    parser.ObservationRecords.SingleOrDefault(
                        t => t.Key.ApproximateDateTime == new DateTime(2016, 3, 28, 0, 34, 30));
            var data = epoch.Value as ObservationDataRecord;
            Assert.IsTrue(data != null);

            /*
               S    4 C1C L1C D1C S1C                                      SYS / # / OBS TYPES
            */

            var sbasSatellites = data.GetSatellitesObs<SbasSatellite>();
            var satellitesObs = sbasSatellites as SatelliteObs<SbasSatellite>[] ?? sbasSatellites.ToArray();
            var s27Sat = satellitesObs.SingleOrDefault(t => t.Prn == SbasSatellite.S27);
            Assert.IsTrue(s27Sat != null);
            Assert.IsTrue(s27Sat.Observations.Count == 4);

            Assert.IsTrue(s27Sat.Observations.ContainsKey(ObservationCode.C1C));
            CheckObsValueImpl(s27Sat.Observations[ObservationCode.C1C], null, null, 38565600.351);

            Assert.IsTrue(s27Sat.Observations.ContainsKey(ObservationCode.L1C));
            CheckObsValueImpl(s27Sat.Observations[ObservationCode.L1C], 0, 7, 202663618.200);

            Assert.IsTrue(s27Sat.Observations.ContainsKey(ObservationCode.D1C));
            CheckObsValueImpl(s27Sat.Observations[ObservationCode.D1C], null, null, 7.815);

            Assert.IsTrue(s27Sat.Observations.ContainsKey(ObservationCode.S1C));
            CheckObsValueImpl(s27Sat.Observations[ObservationCode.S1C], null, null, 47.350);
        }



        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\ZIM300CHE_R_20182420828_01H_01S_MO.rnx")]
        public void Test_Rinex_Successfull_Mixed3_Parse()
        {
            var parser = new RinexObsParser("ZIM300CHE_R_20182420828_01H_01S_MO.rnx", ParseType.RaiseEvents);
            parser.Parse();
            Assert.IsTrue(true);
        }


        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\DAC200CHE_R_20182971100_01H_01S_MO.rnx")]
        public void Test_Rinex_Successfull_EpochRecord()
        {
            var parser = new RinexObsParser("DAC200CHE_R_20182971100_01H_01S_MO.rnx", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(true);
            Assert.AreEqual(parser.ObservationRecords.First().Key.Day, 24);
            Assert.AreEqual(parser.ObservationRecords.First().Key.EpochRecords, 26);
            Assert.AreEqual(parser.ObservationRecords.First().Key.Hour, 11);
            Assert.AreEqual(parser.ObservationRecords.First().Key.RcvClkOffset, 0.0);
            Assert.AreEqual(parser.ObservationRecords.Last().Key.EpochFlag, EpochFlag.Ok);
        }

        private static void CheckObsValueImpl(Observation observation, int? lockIndicator, int? sigStrength,
            double obsValue)
        {
            Assert.IsTrue(observation.Lli == lockIndicator);
            Assert.IsTrue(observation.SignalStrength == sigStrength);
            Assert.IsTrue(observation.ObsValue.HasValue &&
                          Math.Abs(observation.ObsValue.Value - obsValue) < Double.Epsilon);
        }


        private static void CheckObservations(IDictionary<SatelliteSystem, GnssObservation> metaData,
            SatelliteSystem satelliteSystem,
            ObservationCode[] expectedObs)
        {
            Assert.IsTrue(metaData.ContainsKey(satelliteSystem));
            var obs = metaData[satelliteSystem];
            CollectionAssert.AreEqual(expectedObs, obs.Observations);
        }


        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\TSK200JPN_R_20180010000_01D_30S_MO.rnx")]
        public void Test_Rinex_Missing_SystemTime_In_Obs_Time_In_Header_Successfull_Parse()
        {
            var parser = new RinexObsParser("TSK200JPN_R_20180010000_01D_30S_MO.rnx", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(true);
         }
        
        
        
        //OUL200FIN_R_20181640000_01D_30S_MO.rnx

        [TestMethod, DeploymentItem("TestFiles\\Obs\\Full\\OUL200FIN_R_20181640000_01D_30S_MO.rnx")]
        public void Test_Rinex_Successfull_Mixed4_Parse()
        {
            var parser = new RinexObsParser("OUL200FIN_R_20181640000_01D_30S_MO.rnx", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(true);
        }


        [TestMethod, DeploymentItem("TestFiles\\Obs\\Header\\interval_bug_fix.19o")]
        public void Test_Correct_Interval_Parse()
        {
            var parser = new RinexObsParser(@"interval_bug_fix.19o", ParseType.StoreData);
            parser.Parse();
            Assert.IsTrue(true);
        }
        #endregion
    }
}