// Rinex3ParserTest
// RinexFilterObsParserTest.cs-2016-09-08

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rinex3Parser.Common;
using Rinex3Parser.Obs;


#endregion

namespace Rinex3ParserTest
{
    [TestClass]
    public class RinexFilterObsParserTest
    {
        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Temporal_Filter()
        {
            var start = new DateTime(2016, 3, 28, 0, 32, 0);
            var end = new DateTime(2016, 3, 28, 0, 35, 30);

            var obsFilter = new ObservationFilter(start, end, null, null);
            var parser = new RinexObsWithFilter("valid_mixed.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);

            Assert.IsTrue(parser.ObservationRecords.Count == 8);

            var firstObs = parser.ObservationRecords.First();
            var appxStart = firstObs.Key.ApproximateDateTime;
            Assert.AreEqual(start, appxStart);

            var lastObs = parser.ObservationRecords.Last();
            var appxEnd = lastObs.Key.ApproximateDateTime;
            Assert.AreEqual(end, appxEnd);
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Gps_Excluded_Only()
        {
            var obsFilter = new ObservationFilter(null, null, SatelliteSystem.Gps, null);

            var parser = new RinexObsWithFilter("valid_mixed.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);

            var headerMetadata = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(headerMetadata.ObsMetaData.ContainsKey(SatelliteSystem.Gps));

            var observedGpsSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Gps),
                "!observedGpsSatellites.ContainsKey(SatelliteSystem.Gps)");
            Assert.IsTrue(!observedGpsSatellites[SatelliteSystem.Gps].Any(),
                "!observedGpsSatellites[SatelliteSystem.Gps].Any()");

            var observedGpsSatellites2 = parser.GetSatellitesObservations<GpsSatellite>();
            Assert.IsTrue(observedGpsSatellites2.Count == 0, "observedGpsSatellites2.Count == 0");
        }


        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_glo.16o")]
        public void Check_Glo_Excluded_On_Pure_Glo_File()
        {
            var obsFilter = new ObservationFilter(null, null, SatelliteSystem.Glo, null);

            var parser = new RinexObsWithFilter("pure_glo.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Glo);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Glo);

            var headerMetadata = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(headerMetadata.ObsMetaData.ContainsKey(SatelliteSystem.Glo));

            var observedGpsSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Glo),
                "!observedGpsSatellites.ContainsKey(SatelliteSystem.Glo)");
            Assert.IsTrue(!observedGpsSatellites[SatelliteSystem.Glo].Any(),
                "!observedGpsSatellites[SatelliteSystem.Glo].Any()");

            var observedGpsSatellites2 = parser.GetSatellitesObservations<GloSatellite>();
            Assert.IsTrue(observedGpsSatellites2.Count == 0, "observedGpsSatellites2.Count == 0");
        }    

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\pure_glo.16o")]
        public void Check_Excluded_Glo_R01_R11_On_Pure_Glo_File()
        {
            var obsFilter = new ObservationFilter(null, null, null, new Dictionary<SatelliteSystem, int[]>
            {
                {SatelliteSystem.Glo, new[] {(int)GloSatellite.R01, (int)GloSatellite.R11}}
            });

            var parser = new RinexObsWithFilter("pure_glo.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Glo);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Glo);

            Assert.IsTrue(parser.ObservationRecords.Count == 19, "parser.ObservationRecords.Count == 19");

            var headerMetadata = parser.ObsHeader.ObsHeaderData;
            Assert.IsTrue(headerMetadata.ObsMetaData.ContainsKey(SatelliteSystem.Glo));

            var observedGpsSatellites = parser.GetObservedSatellites();
            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Glo),
                "!observedGpsSatellites.ContainsKey(SatelliteSystem.Glo)");
            CollectionAssert.IsNotSubsetOf(new[] {(int)GloSatellite.R01, (int)GloSatellite.R11},
                observedGpsSatellites[SatelliteSystem.Glo].ToArray(), "glo IsNotSubsetOf");
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Full_Filter()
        {
            var start = new DateTime(2016, 3, 28, 0, 32, 0);
            var end = new DateTime(2016, 3, 28, 0, 35, 30);


            var obsFilter = new ObservationFilter(start, end, SatelliteSystem.Qzss|SatelliteSystem.Sbas,
                new Dictionary<SatelliteSystem, int[]>
                {
                    {SatelliteSystem.Glo, new[] {(int)GloSatellite.R01, (int)GloSatellite.R11}},
                    {SatelliteSystem.Gps, new[] {(int)GpsSatellite.G05, (int)GpsSatellite.G15}},
                    {SatelliteSystem.Gal, new[] {(int)GalSatellite.E12}},
                    {SatelliteSystem.Bds, new[] {(int)BdsSatellite.C01, (int)BdsSatellite.C02, (int)BdsSatellite.C03}},
                });

            var parser = new RinexObsWithFilter("valid_mixed.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);

            Assert.IsTrue(parser.ObservationRecords.Count == 8, "Temporal test");

            Dictionary<SatelliteSystem, IEnumerable<int>> observedGpsSatellites = parser.GetObservedSatellites();

            Assert.IsTrue(
                !observedGpsSatellites[SatelliteSystem.Sbas].Any() && !observedGpsSatellites[SatelliteSystem.Qzss].Any(),
                "!observedGpsSatellites[SatelliteSystem.Sbas].Any() && !observedGpsSatellites[SatelliteSystem.Qzss].Any()");


            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Glo),
                "observedGpsSatellites.ContainsKey(SatelliteSystem.Glo)");
            CollectionAssert.IsNotSubsetOf(new[] {(int)GloSatellite.R01, (int)GloSatellite.R11},
                observedGpsSatellites[SatelliteSystem.Glo].ToArray(), "glo IsNotSubsetOf");


            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Gps),
                "observedGpsSatellites.ContainsKey(SatelliteSystem.Gps)");
            CollectionAssert.IsNotSubsetOf(new[] {(int)GpsSatellite.G05, (int)GpsSatellite.G15},
                observedGpsSatellites[SatelliteSystem.Gps].ToArray(), "gps IsNotSubsetOf");

            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Bds),
                "observedGpsSatellites.ContainsKey(SatelliteSystem.Bds)");
            CollectionAssert.IsNotSubsetOf(new[] {(int)BdsSatellite.C01, (int)BdsSatellite.C02, (int)BdsSatellite.C03},
                observedGpsSatellites[SatelliteSystem.Bds].ToArray(), "bds IsNotSubsetOf");

            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Gal),
                "!observedGpsSatellites.ContainsKey(SatelliteSystem.Glo)");
            CollectionAssert.IsNotSubsetOf(new[] {(int)GalSatellite.E12},
                observedGpsSatellites[SatelliteSystem.Gal].ToArray(), "gal IsNotSubsetOf");
        }

        [TestMethod, DeploymentItem(@"TestFiles\\Obs\\Full\\valid_mixed.16o")]
        public void Check_Invalid_Filter_No_Exception()
        {
            var start = new DateTime(2016, 3, 28, 0, 32, 0);
            var end = new DateTime(2016, 3, 28, 0, 35, 30);


            var obsFilter = new ObservationFilter(start, end, SatelliteSystem.Qzss|SatelliteSystem.Sbas,
                new Dictionary<SatelliteSystem, int[]>
                {
                    {SatelliteSystem.Gal, new[] {(int)GpsSatellite.G05, (int)GpsSatellite.G15}},
                });

            var parser = new RinexObsWithFilter("valid_mixed.16o", ParseType.StoreData, obsFilter);
            parser.Parse();

            Assert.IsTrue(parser.RinexType == RinexType.Obs);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.SatelliteSystem == SatelliteSystem.Mixed);
            Assert.IsTrue(parser.ObsHeader.ObsHeaderData.GnssTimeSystem == GnssTimeSystem.Gps);

            Assert.IsTrue(parser.ObservationRecords.Count == 8, "Temporal test");

            var observedGpsSatellites = parser.GetObservedSatellites();

            Assert.IsTrue(
                !observedGpsSatellites[SatelliteSystem.Sbas].Any() && !observedGpsSatellites[SatelliteSystem.Qzss].Any(),
                "!observedGpsSatellites[SatelliteSystem.Sbas].Any() && !observedGpsSatellites[SatelliteSystem.Qzss].Any()");

            Assert.IsTrue(observedGpsSatellites.ContainsKey(SatelliteSystem.Gal),
                "!observedGpsSatellites.ContainsKey(SatelliteSystem.Glo)");
            CollectionAssert.IsSubsetOf(new[] {(int)GalSatellite.E12},
                observedGpsSatellites[SatelliteSystem.Gal].ToArray(), "gal IsNotSubsetOf");
        }
    }
}