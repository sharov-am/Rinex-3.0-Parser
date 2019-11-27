// Rinex3Parser
// FileNameAttributes.cs-2018-08-09

#region

using System;
using System.Text.RegularExpressions;

#endregion


namespace Rinex3Parser.Common
{
    //A 1 RINEX File name description (rinex 3.* spec)
    public class FileNameAttributes
    {
        #region Constructor

        private FileNameAttributes()
        {
        }

        #endregion

        #region Properties

        public string StationName { get; private set; }
        public string CCode { get; private set; }
        public RinexDataSource DataSource { get; private set; }
        public RinexStartTime StartTime { get; private set; }
        public TimeSpan FilePeriod { get; private set; }
        public RinexDataType DataType { get; private set; }
        public RinexDataFreq DataFreq { get; private set; }
        public RinexFileFormat FileFormat { get; set; }
        public Compression? Compress { get; set; }
        
        #endregion

        #region Methods

        public static FileNameAttributes ParseFileName(string filename)
        {
            var result = new FileNameAttributes();
            var match = RinexRegex.RinexV3FileNameAttrRegex.Match(filename);

            /*
             CAEN00FRA_R_20181640000_01D_30S_MO.crx.gz
             BRDC00IGS_R_20182130000_01D_MN.rnx.gz
             */

            //groups
            /*
                <stname>, <cc>,/m
                <source>,/m 
                <year>, <doy>, <hh>, <mm> /m
                <interval>, <period> /m
                <freq> ,<frequnits> /opt
                <gnss>,<type>/m
                <fileformat>/m
                <zip> /opt
            */

            if (!match.Groups["stname"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No station name");
            var stName = match.Groups["stname"].Value;
            result.StationName = stName;

            var cc = match.Groups["cc"].Value;
            result.CCode = cc;

            if (!match.Groups["source"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No source designation.");

            var rDataSource = match.Groups["source"].Value;
            result.DataSource = (RinexDataSource)Enum.Parse(typeof(RinexDataSource), rDataSource);


            if (!match.Groups["year"].Success || !match.Groups["doy"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No start time info.");
            var rst = new RinexStartTime();
            var year = match.Groups["year"].Value;
            int temp;
            if (Int32.TryParse(year, out temp))
            {
                rst.Year = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong year {0}", year));
            }

            var doy = match.Groups["doy"].Value;
            if (Int32.TryParse(doy, out temp))
            {
                rst.Doy = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong doy {0}", doy));
            }

            var h = match.Groups["hh"].Value;
            if (Int32.TryParse(h, out temp))
            {
                rst.Hours = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong hours {0}", h));
            }

            var m = match.Groups["mm"].Value;
            if (Int32.TryParse(m, out temp))
            {
                rst.Minutes = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong minutes {0}", m));
            }

            result.StartTime = rst;

            //<interval>, <period> /m
            if (!match.Groups["interval"].Success || !match.Groups["period"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file period info.");


            int interval;
            var inte = match.Groups["interval"].Value;
            if (!Int32.TryParse(inte, out interval))
            {
                throw new ArgumentException(
                        String.Format("Invalid rinex v3 file name format. Wrong interval {0}", inte));
            }


            var per = match.Groups["period"].Value;
            var period = (RinexFilePeriod)Enum.Parse(typeof(RinexFilePeriod), per);
            switch(period)
            {
                case RinexFilePeriod.M:
                    result.FilePeriod = TimeSpan.FromMinutes(interval);
                    break;
                case RinexFilePeriod.H:
                    result.FilePeriod = TimeSpan.FromHours(interval);
                    break;
                case RinexFilePeriod.D:
                    result.FilePeriod = TimeSpan.FromDays(interval);
                    break;
                case RinexFilePeriod.Y:
                    result.FilePeriod = TimeSpan.FromDays(365.25 * interval);
                    break;
                case RinexFilePeriod.U:
                    result.FilePeriod = TimeSpan.MinValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (match.Groups["freq"].Success && match.Groups["frequnits"].Success)
            {
                int freq = 0;

                if (!Int32.TryParse(match.Groups["freq"].Value, out freq))
                {
                    throw new ArgumentException(String.Format(
                            "Invalid rinex v3 file name format. Wrong freq. value {0}",
                            freq));
                }

                var f = match.Groups["frequnits"].Value;
                var fu = (RinexFreqUnits)Enum.Parse(typeof(RinexFreqUnits), f);
                if (fu == RinexFreqUnits.C)
                {
                    result.DataFreq = new RinexDataFreq {DataFreq = freq * 100 /*see sepec*/, Units = fu};
                }
                else
                {
                    result.DataFreq = new RinexDataFreq {DataFreq = freq, Units = fu};
                }
            }

            // <gnss>,<type>/m
            if (!match.Groups["gnss"].Success || !match.Groups["type"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file data type info.");

            var rnxDataType = new RinexDataType();

            var c = Convert.ToChar(match.Groups["gnss"].Value);
            rnxDataType.Gnss = Utils.CharToGnss(c);

            c = Convert.ToChar(match.Groups["type"].Value);
            rnxDataType.Type = Utils.CharToFileType(c);
            result.DataType = rnxDataType;

            //<fileformat>/m
            if (!match.Groups["fileformat"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file format(rnx,crx) specified.");

            result.FileFormat =
                    (RinexFileFormat)Enum.Parse(typeof(RinexFileFormat), match.Groups["fileformat"].Value, true);

            //<zip> /opt

            if (match.Groups["zip"].Success)
            {
                var z = match.Groups["zip"].Value;
                switch(z.ToLower())
                {
                    case "zip":
                        result.Compress = Compression.Zip;
                        break;
                    case "gz":
                        result.Compress = Compression.Gzip;
                        break;
                    case "bz2":
                        result.Compress = Compression.Bzip2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return result;
        }

        private static FileNameAttributes ParseFileName(Match match)
        {
            var result = new FileNameAttributes();

            /*
             CAEN00FRA_R_20181640000_01D_30S_MO.crx.gz
             BRDC00IGS_R_20182130000_01D_MN.rnx.gz
             */

            //groups
            /*
                <stname>, <cc>,/m
                <source>,/m 
                <year>, <doy>, <hh>, <mm> /m
                <interval>, <period> /m
                <freq> ,<frequnits> /opt
                <gnss>,<type>/m
                <fileformat>/m
                <zip> /opt
            */

            if (!match.Groups["stname"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No station name");
            var stName = match.Groups["stname"].Value;
            result.StationName = stName;

            var cc = match.Groups["cc"].Value;
            result.CCode = cc;

            if (!match.Groups["source"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No source designation.");

            var rDataSource = match.Groups["source"].Value;
            result.DataSource = (RinexDataSource)Enum.Parse(typeof(RinexDataSource), rDataSource);


            if (!match.Groups["year"].Success || !match.Groups["doy"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No start time info.");
            var rst = new RinexStartTime();
            var year = match.Groups["year"].Value;
            int temp;
            if (Int32.TryParse(year, out temp))
            {
                rst.Year = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong year {0}", year));
            }

            var doy = match.Groups["doy"].Value;
            if (Int32.TryParse(doy, out temp))
            {
                rst.Doy = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong doy {0}", doy));
            }

            var h = match.Groups["hh"].Value;
            if (Int32.TryParse(h, out temp))
            {
                rst.Hours = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong hours {0}", h));
            }

            var m = match.Groups["mm"].Value;
            if (Int32.TryParse(m, out temp))
            {
                rst.Minutes = temp;
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid rinex v3 file name format. Wrong minutes {0}", m));
            }

            result.StartTime = rst;

            //<interval>, <period> /m
            if (!match.Groups["interval"].Success || !match.Groups["period"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file period info.");


            int interval;
            var inte = match.Groups["interval"].Value;
            if (!Int32.TryParse(inte, out interval))
            {
                throw new ArgumentException(
                        String.Format("Invalid rinex v3 file name format. Wrong interval {0}", inte));
            }


            var per = match.Groups["period"].Value;
            var period = (RinexFilePeriod)Enum.Parse(typeof(RinexFilePeriod), per);
            switch(period)
            {
                case RinexFilePeriod.M:
                    result.FilePeriod = TimeSpan.FromMinutes(interval);
                    break;
                case RinexFilePeriod.H:
                    result.FilePeriod = TimeSpan.FromHours(interval);
                    break;
                case RinexFilePeriod.D:
                    result.FilePeriod = TimeSpan.FromDays(interval);
                    break;
                case RinexFilePeriod.Y:
                    result.FilePeriod = TimeSpan.FromDays(365.25 * interval);
                    break;
                case RinexFilePeriod.U:
                    result.FilePeriod = TimeSpan.MinValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (match.Groups["freq"].Success && match.Groups["frequnits"].Success)
            {
                int freq = 0;

                if (!Int32.TryParse(match.Groups["freq"].Value, out freq))
                {
                    throw new ArgumentException(String.Format(
                            "Invalid rinex v3 file name format. Wrong freq. value {0}",
                            freq));
                }

                var f = match.Groups["frequnits"].Value;
                var fu = (RinexFreqUnits)Enum.Parse(typeof(RinexFreqUnits), f);
                if (fu == RinexFreqUnits.C)
                {
                    result.DataFreq = new RinexDataFreq {DataFreq = freq * 100 /*see sepec*/, Units = fu};
                }
                else
                {
                    result.DataFreq = new RinexDataFreq {DataFreq = freq, Units = fu};
                }
            }

            // <gnss>,<type>/m
            if (!match.Groups["gnss"].Success || !match.Groups["type"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file data type info.");

            var rnxDataType = new RinexDataType();

            var c = Convert.ToChar(match.Groups["gnss"].Value);
            rnxDataType.Gnss = Utils.CharToGnss(c);

            c = Convert.ToChar(match.Groups["type"].Value);
            rnxDataType.Type = Utils.CharToFileType(c);
            result.DataType = rnxDataType;

            //<fileformat>/m
            if (!match.Groups["fileformat"].Success)
                throw new ArgumentException("Invalid rinex v3 format. No file format(rnx,crx) specified.");

            result.FileFormat =
                    (RinexFileFormat)Enum.Parse(typeof(RinexFileFormat), match.Groups["fileformat"].Value, true);

            //<zip> /opt

            if (match.Groups["zip"].Success)
            {
                var z = match.Groups["zip"].Value;
                switch(z.ToLower())
                {
                    case "zip":
                        result.Compress = Compression.Zip;
                        break;
                    case "gz":
                        result.Compress = Compression.Gzip;
                        break;
                    case "bz2":
                        result.Compress = Compression.Bzip2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return result;
        }


        public static bool TryParseFileName(string filename, out FileNameAttributes fna)
        {
            var match = RinexRegex.RinexV3FileNameAttrRegex.Match(filename);
            if (!match.Success)
            {
                fna = null;
                return false;
            }

            try
            {
            fna = ParseFileName(match);
            return true;
            }
            catch(Exception e)
            {
                throw new ArgumentException(String.Format("Error parsing fname {0}", filename), e);
            }
        }

        #endregion


        public class RinexStartTime
        {
            #region Properties

            public int Year { get; set; }
            public int Doy { get; set; }
            public int Hours { get; set; }
            public int Minutes { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                return String.Format("{0}{1}{2}{3}", Year, Doy, Hours, Minutes);
            }

            #endregion
        }


        public class RinexDataFreq
        {
            #region Properties

            public int DataFreq { get; set; }
            public RinexFreqUnits Units { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                switch(Units)
                {
                    case RinexFreqUnits.C:
                        return String.Format("{0:00}C", DataFreq);
                    case RinexFreqUnits.Z:
                        return String.Format("{0:00}Z", DataFreq);
                    case RinexFreqUnits.S:
                        return String.Format("{0:00}S", DataFreq);
                    case RinexFreqUnits.M:
                        return String.Format("{0:00}M", DataFreq);
                    case RinexFreqUnits.H:
                        return String.Format("{0:00}H", DataFreq);
                    case RinexFreqUnits.D:
                        return String.Format("{0:00}D", DataFreq);
                    case RinexFreqUnits.U:
                        return String.Format("{0:00}U", DataFreq);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            #endregion
        }


        public class RinexDataType
        {
            #region Properties

            public SatelliteSystem Gnss { get; set; }
            public FileType Type { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                var t = Enum.GetName(typeof(SatelliteSystem), Gnss);
                switch(Type)
                {
                    case FileType.Obs:
                        return String.Format("{0}O", t[0]);
                    case FileType.Nav:
                        return String.Format("{0}N", t[0]);
                    case FileType.Met:
                        return String.Format("{0}M", t[0]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            #endregion
        }




        #region enums

        public enum Compression
        {
            Gzip,
            Bzip2,
            Zip
        }


        public enum RinexDataSource
        {
            R = 1,
            S,
            U
        }


        public enum RinexFilePeriod
        {
            M = 1,
            H,
            D,
            Y,
            U
        }


        public enum RinexFreqUnits
        {
            C = 1, //100 Hz
            Z, //HZ
            S,
            M,
            H,
            D,
            U
        }



        #endregion


    }


    public enum FileType
    {
        Obs = 1,
        Nav,
        Met
    }


    public enum RinexFileFormat
    {
        Rnx = 1,
        Crx
    }
}