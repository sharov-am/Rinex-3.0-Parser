// Rinex3ParserTest
// FileNameAttributesTests.cs-2018-08-09

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rinex3Parser.Common;
using Rinex3Parser.Obs;


namespace Rinex3ParserTest
{
    [TestClass]
    public class FileNameAttributesTests
    {
        [TestMethod]
        public void Check_File1_Correcntess()
        {
            var fname = "CAEN00FRA_R_20181640000_01D_30S_MO.crx.gz";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.StationName, "CAEN");
            Assert.AreEqual(result.CCode, "FRA");

            Assert.IsTrue(result.DataSource == FileNameAttributes.RinexDataSource.R,
                    "result.DataSource == FileNameAttributes.RinexDataSource.R");


            Assert.AreEqual(result.StartTime.Year, 2018);
            Assert.AreEqual(result.StartTime.Doy, 164);
            Assert.AreEqual(result.StartTime.Hours, 0);
            Assert.AreEqual(result.StartTime.Minutes, 0);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromDays(1));

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 30);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.S);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.IsTrue(result.DataType.Gnss == SatelliteSystem.Mixed,
                    "result.DataType.Gnss == SatelliteSystem.Mixed");
            Assert.IsTrue(result.DataType.Type == FileType.Obs, "result.DataType.Type == FileType.Obs");

            Assert.IsTrue(result.FileFormat == RinexFileFormat.Crx, "result.FileFormat == RinexFileFormat.Crx");

            Assert.IsTrue(result.Compress.HasValue && result.Compress == FileNameAttributes.Compression.Gzip,
                    "result.Compress.HasValue && result.Compress == FileNameAttributes.Compression.Gzip");
        }

        [TestMethod]
        public void Check_File_Compress_Extension_Correctness()
        {
            var fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNull(result.Compress, "result.Compress == null");

            fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx.zip";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.Compress, "result.Compress != null");
            Assert.IsTrue(result.Compress == FileNameAttributes.Compression.Zip,
                    "result.Compress == FileNameAttributes.Compression.Zip");


            fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx.bz2";
            result = FileNameAttributes.ParseFileName(fname);
            Assert.IsNotNull(result.Compress, "result.Compress != null");
            Assert.IsTrue(result.Compress == FileNameAttributes.Compression.Bzip2,
                    "result.Compress == FileNameAttributes.Compression.Bzip2");


            fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);
            Assert.IsNotNull(result.Compress, "result.Compress != null");
            Assert.IsTrue(result.Compress == FileNameAttributes.Compression.Gzip,
                    "result.Compress == FileNameAttributes.Compression.Gzip");
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_File_Compress_Extension_Throws()
        {
            var fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx.gzip";
            FileNameAttributes.ParseFileName(fname);
        }


        [TestMethod]
        public void Check_File2_Correctness()
        {
            var fname = "ALGO00CAN_R_20121601000_01D_01C_GO.rnx.gz";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.StationName, "ALGO");
            Assert.AreEqual(result.CCode, "CAN");

            Assert.IsTrue(result.DataSource == FileNameAttributes.RinexDataSource.R,
                    "result.DataSource == FileNameAttributes.RinexDataSource.R");


            Assert.AreEqual(result.StartTime.Year, 2012);
            Assert.AreEqual(result.StartTime.Doy, 160);
            Assert.AreEqual(result.StartTime.Hours, 10);
            Assert.AreEqual(result.StartTime.Minutes, 0);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromDays(1));

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 100);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.C);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.IsTrue(result.DataType.Gnss == SatelliteSystem.Gps, "result.DataType.Gnss == SatelliteSystem.Mixed");
            Assert.IsTrue(result.DataType.Type == FileType.Obs, "result.DataType.Type == FileType.Obs");

            Assert.IsTrue(result.FileFormat == RinexFileFormat.Rnx, "result.FileFormat == RinexFileFormat.Crx");

            Assert.IsTrue(result.Compress.HasValue && result.Compress == FileNameAttributes.Compression.Gzip,
                    "result.Compress.HasValue && result.Compress == FileNameAttributes.Compression.Gzip");
        }


        [TestMethod]
        public void Check_Only_DataFrequency_Correctness()
        {
            //C
            var fname = "ALGO00CAN_R_20121601000_01D_01C_GO.rnx.gz";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 100);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.C);

            fname = "ALGO00CAN_R_20121601000_01D_03C_GO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 300);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.C);


            //Z
            fname = "ALGO00CAN_R_20121601000_01D_05Z_RO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 5);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.Z);


            fname = "ALGO00CAN_R_20121601000_01D_30Z_RO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 30);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.Z);


            //S
            fname = "ALGO00CAN_R_20121601000_01D_01S_EO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 1);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.S);

            fname = "ALGO00CAN_R_20121601000_01D_31S_EO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 31);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.S);


            //M

            fname = "ALGO00CAN_R_20121601000_01D_05M_JO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 5);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.M);

            fname = "ALGO00CAN_R_20121601000_01D_45M_JO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 45);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.M);


            //H

            fname = "ALGO00CAN_R_20121601000_01D_01H_CO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 1);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.H);

            fname = "ALGO00CAN_R_20121601000_01D_10H_CO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 10);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.H);


            //D
            fname = "ALGO00CAN_R_20121601000_01D_01D_SO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 1);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.D);


            //D
            fname = "ALGO00CAN_R_20121601000_01D_18D_SO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.DataFreq, 18);
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.D);


            //U
            fname = "ALGO00CAN_R_20121601000_01D_00U_MO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataFreq, "result.DataFreq != null");
            Assert.AreEqual(result.DataFreq.Units, FileNameAttributes.RinexFreqUnits.U);
        }


        [TestMethod]
        public void Check_Only_FilePeriod_Correctness()
        {
            var fname = "ALGO00CAN_R_20121601000_15M_01S_GO.rnx.gz";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromMinutes(15));

            
            fname = "ALGO00CAN_R_20121601000_01H_05Z_MO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);
            
            Assert.AreEqual(result.FilePeriod, TimeSpan.FromHours(1));


            fname = "ALGO00CAN_R_20121601000_01D_30S_GO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromDays(1));


            fname = "ALGO00CAN_R_20121600000_30M_GN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromMinutes(30));


            fname = "ALGO00CAN_R_20121601000_11H_05Z_MO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromHours(11));


            fname = "ALGO00CAN_R_20121601000_03D_30S_GO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.AreEqual(result.FilePeriod, TimeSpan.FromDays(3));
            
        }



        [TestMethod]
        public void Check_Only_Data_Type_Format_Correctness()
        {
            var fname = "ALGO00CAN_R_20121601000_15M_01S_GO.rnx.gz";
            var result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType,"result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss,SatelliteSystem.Gps);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);


            fname = "ALGO00CAN_R_20121601000_15M_01S_RO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Glo);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);

            fname = "ALGO00CAN_R_20121601000_15M_01S_EO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Gal);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);


            fname = "ALGO00CAN_R_20121601000_15M_01S_JO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Qzss);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);


            fname = "ALGO00CAN_R_20121601000_15M_01S_CO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Bds);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);

            fname = "ALGO00CAN_R_20121601000_15M_01S_SO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Sbas);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);


            fname = "ALGO00CAN_R_20121601000_15M_01S_MO.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Mixed);
            Assert.AreEqual(result.DataType.Type, FileType.Obs);


           
            
            //nav
            fname = "ALGO00CAN_R_20121600000_01H_GN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Gps);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);


            fname = "ALGO00CAN_R_20121600000_01H_RN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Glo);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);

            fname = "ALGO00CAN_R_20121600000_01H_EN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Gal);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);


            fname = "ALGO00CAN_R_20121600000_01H_JN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Qzss);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);


            fname = "ALGO00CAN_R_20121600000_01H_CN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Bds);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);



            fname = "ALGO00CAN_R_20121600000_01H_SN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Sbas);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);


            fname = "ALGO00CAN_R_20121600000_01H_MN.rnx.gz";
            result = FileNameAttributes.ParseFileName(fname);

            Assert.IsNotNull(result.DataType, "result.DataType != null");
            Assert.AreEqual(result.DataType.Gnss, SatelliteSystem.Mixed);
            Assert.AreEqual(result.DataType.Type, FileType.Nav);


        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_Naming_Exception_Throws()
        {
            var fname = "AEN00FRA_R_20181640000_01D_30S_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_File_Data_Source_Throws()
        {
            var fname = "AEN00FRA_Q_20181640000_01D_30S_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_File_Start_Time_Source_Throws()
        {
            var fname = "AEN00FRA_Q_2018164000_01D_30S_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_File_Start_Time_Throws()
        {
            var fname = "AEN00FRA_Q_2018164_01D_30S_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_File_Interval_Throws()
        {
            var fname = "AEN00FRA_Q_2018164_01Q_30S_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Check_Invalid_File_Data_Freq_Throws()
        {
            var fname = "AEN00FRA_Q_2018164_01Q_30T_MO.rnx.gz";
            FileNameAttributes.ParseFileName(fname);
        }


        [TestMethod]
        public void Try_Parse_Not_RnxV3_File1()
        {
            var fname = "ista1180.18d.Z";
            FileNameAttributes fna;
            
            var result = FileNameAttributes.TryParseFileName(fname, out fna);
            
            Assert.IsFalse(result);
            Assert.IsTrue(fna == null);
        }

        [TestMethod]
        public void Try_Parse_Not_RnxV3_File2()
        {
            var fname = "abcd";
            FileNameAttributes fna;

            var result = FileNameAttributes.TryParseFileName(fname, out fna);

            Assert.IsFalse(result);
            Assert.IsTrue(fna == null);
        }

        [TestMethod]
        public void Try_Parse_Not_RnxV3_File3()
        {
            var fname = String.Empty;
            FileNameAttributes fna;

            var result = FileNameAttributes.TryParseFileName(fname, out fna);

            Assert.IsFalse(result);
            Assert.IsTrue(fna == null);
        }


        [TestMethod]
        public void Try_Parse_Invalid_RnxV3_File()
        {
            var fname = "AEN00FRA_Q_2018164_01Q_30T_MO.rnx.gz";
            FileNameAttributes fna;

            var result = FileNameAttributes.TryParseFileName(fname, out fna);

            Assert.IsFalse(result);
            Assert.IsTrue(fna == null);
        }


        [TestMethod]
        public void Try_Parse_Valid_RnxV3_File()
        {
            var fname = "CAEN00FRA_R_20181640000_01D_30S_MO.rnx";
            FileNameAttributes fna;

            var result = FileNameAttributes.TryParseFileName(fname, out fna);

            Assert.IsTrue(result);
            Assert.IsTrue(fna != null);
        }



    }
}