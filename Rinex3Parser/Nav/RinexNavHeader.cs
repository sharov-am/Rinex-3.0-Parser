// Rinex3Parser
// RinexNavHeader.cs-2016-08-16

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public class RinexNavHeader
    {
        #region Fields

        private static readonly Dictionary<string, Regex> HeaderRegexps = new Dictionary<string, Regex>
        {
            {
                RinexHeaderLabel.VERSION, RinexRegex.RinexVersionRegexp
            },
            {
                RinexHeaderLabel.PROGRAM, RinexRegex.ProgramRunBy
            },
            {
                RinexHeaderLabel.COMMENT, RinexRegex.A60Regex
            },
            {
                RinexHeaderLabel.IONOSPHERIC_CORRECTIONS, RinexRegex.IonoCorrections
            },
            {
                RinexHeaderLabel.TIME_SYSTEM_CORRECTIONS, RinexRegex.TimeSystemCorrection
            },
            {
                RinexHeaderLabel.LEAP_SECONDS, RinexRegex.LeapSecondRegex
            },
        };

        public readonly RinexNavHeaderData NavHeaderData = new RinexNavHeaderData();

        #endregion

        #region Methods

        public bool ParseHeaderLine(string headerLine)
        {
            var header = headerLine.Substring(60).TrimEnd();
            var data = headerLine.Substring(0, 60);
            var matchResult = Match.Empty;

            NavHeaderData.HeaderLabels.Add(header);

            if (HeaderRegexps.ContainsKey(header))
            {
                var regExp = HeaderRegexps[header];
                matchResult = regExp.Match(data);
            }

            string temp;
            switch(header)
            {
                case RinexHeaderLabel.VERSION:
                    NavHeaderData.Version = matchResult.Groups["rnxver"].Value;
                    NavHeaderData.SatelliteSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                    return true;

                case RinexHeaderLabel.PROGRAM:
                    var programInfo = NavHeaderData.HeaderProgramInfo;
                    programInfo.Name = matchResult.Groups["pgm"].Success
                            ? matchResult.Groups["pgm"].Value.Trim()
                            : String.Empty;
                    programInfo.AgencyInfo = matchResult.Groups["runby"].Success
                            ? matchResult.Groups["runby"].Value.Trim()
                            : String.Empty;

                    if (matchResult.Groups["date"].Success)
                    {
                        var dateVal = matchResult.Groups["date"].Value;
                        temp = dateVal.Substring(0, 15);
                        DateTime temp2;
                        try
                        {
                            temp2 = DateTime.ParseExact(temp, "yyyyMMdd HHmmss", null);
                        }
                        catch(FormatException)
                        {
                            temp2 = DateTime.Now;
                        }
                        var zone = dateVal.Substring(16).Trim();
                        programInfo.FileCreationDateTime = DateTime.SpecifyKind(temp2,
                                String.Equals(zone, "lcl", StringComparison.InvariantCultureIgnoreCase)
                                        ? DateTimeKind.Local
                                        : DateTimeKind.Utc);
                    }
                    return true;

                case RinexHeaderLabel.LEAP_SECONDS:
                    var numOfLeapSeconds = matchResult.Groups["numofls"].Success
                            ? Int32.Parse(matchResult.Groups["numofls"].Value, CultureInfo.InvariantCulture)
                            : (int?)null;
                    var futurePastLeapSeconds = matchResult.Groups["fpls"].Success
                            ? Int32.Parse(matchResult.Groups["fpls"].Value,
                                    CultureInfo.InvariantCulture)
                            : (int?)null;
                    var weekNum = matchResult.Groups["weeknum"].Success
                            ? Int32.Parse(matchResult.Groups["weeknum"].Value, CultureInfo.InvariantCulture)
                            : (int?)null;
                    var dayNum = matchResult.Groups["daynum"].Success
                            ? Int32.Parse(matchResult.Groups["daynum"].Value, CultureInfo.InvariantCulture)
                            : (int?)null;
                    var ts = GnssTimeSystem.Gps;
                    if (matchResult.Groups["satsystem"].Success)
                    {
                        temp = matchResult.Groups["satsystem"].Value;
                        if (String.Equals(temp, "bds", StringComparison.InvariantCultureIgnoreCase) ||
                            String.Equals(temp, "bdt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ts = GnssTimeSystem.Bdt;
                        }
                    }
                    NavHeaderData.LeapSeconds = new LeapSeconds
                    {
                        CurrentNumOfLeapSeconds = numOfLeapSeconds,
                        FutureNumOfLeapSeconds = futurePastLeapSeconds,
                        Week = weekNum,
                        Day = dayNum,
                        TimeSystem = ts
                    };

                    return true;

                //"^(?:\w{3})(?<AB>\w)?\s{1,2}(?<ioncorr>.{12}){1,4}\s(?<timemark>\w)?\s(?<id>\d{1,2})?\s*$"
                case RinexHeaderLabel.IONOSPHERIC_CORRECTIONS:
                    NavHeaderData.ParseIonoCorrections(matchResult);
                    return true;

                //^(?<corrtype>GAUT|GPUT|SBUT|GLUT|GPGA|GLGP|QZGP|QZUT|BDUT|IRUT|IRGP)\s
                //(?<a0>.{17})(?<a1>.{16})(?<reftime>.{7})(?<refweek>.{5})\s{1,2}(?<sbas>\w{4,5})?\s{1,2}(?<utc>\d{1,2})?\s+$
                case RinexHeaderLabel.TIME_SYSTEM_CORRECTIONS:
                    NavHeaderData.ParseTimeSystemCorrection(matchResult);
                    return true;

                case RinexHeaderLabel.COMMENT:
                    NavHeaderData.Comments.Add(matchResult.Groups[0].Value);
                    return true;

                case RinexHeaderLabel.END_OF_HEADER:
                    return false;
            }

            return false;
        }

        #endregion
    }


    public class RinexNavHeaderData : EventArgs
    {
        #region Fields

        private readonly List<TimeSystemCorrection> _timeSystemCorrections = new List<TimeSystemCorrection>(2);

        public readonly List<string> Comments = new List<string>();

        //use to track header labels. Useful for header printing. 
        //OrderedSet preserve insert order.
        internal readonly OrderedSet<string> HeaderLabels = new OrderedSet<string>();
        public readonly ProgramInfo HeaderProgramInfo = new ProgramInfo();
        private List<IonoCorrections> _ionoCorrections = new List<IonoCorrections>(4);

        #endregion

        #region Properties

        protected List<IonoCorrections> IonoCorrections
        {
            get { return _ionoCorrections; }
            set { _ionoCorrections = value; }
        }

        public int IonoCorrectionsCount
        {
            get { return _ionoCorrections.Count; }
        }


        public IEnumerable<IonoCorrections> GetCorrections(SatelliteSystem satSys)
        {
            return _ionoCorrections.Where(t => t.SatelliteSystem == satSys);
        }

        public IEnumerable<IonoCorrections> GetCorrections(SatelliteSystem satSys, IonoCorrectionsEnum corType)
        {
            return _ionoCorrections.Where(t => t.SatelliteSystem == satSys && t.HasCorrection(corType));
        }

        public IEnumerable<IonoCorrections> GetAlphaCorrections(SatelliteSystem satSys)
        {
            return GetCorrections(satSys, IonoCorrectionsEnum.Alpha0);
        }

        public IEnumerable<IonoCorrections> GetBetaCorrections(SatelliteSystem satSys)
        {
            return GetCorrections(satSys, IonoCorrectionsEnum.Beta0);
        }

        public IEnumerable<IonoCorrections> GetCorrections(SatelliteSystem satSys, IonoCorrectionsEnum corType,
                char timeMark, int satId)
        {
            return IonoCorrections.Where(t => t.SatId.HasValue && t.SatId.Value == satId &&
                                              t.SatelliteSystem == satSys && t.HasCorrection(corType) &&
                                              Char.ToLowerInvariant(t.TimeMark).Equals(Char.ToLowerInvariant(timeMark)));
        }

        public IEnumerable<IonoCorrections> GetCorrections(SatelliteSystem satSys, IonoCorrectionsEnum corType,
                int timeMark, int satId)
        {
            return GetCorrections(satSys, corType, CharToHourMap.HourToChar[timeMark], satId);
        }


        public List<TimeSystemCorrection> TimeSystemCorrections
        {
            get { return _timeSystemCorrections; }
        }

        public LeapSeconds LeapSeconds { get; internal set; }
        public string Version { get; internal set; }
        public SatelliteSystem SatelliteSystem { get; internal set; }

        #endregion

        #region Methods

        internal void ParseIonoCorrections([NotNull] Match matchResult)
        {
            var s = SatelliteSystem;
            if (SatelliteSystem == SatelliteSystem.Mixed)
            {
                var temp = matchResult.Groups["satsystem"].Value.ToUpperInvariant();
                switch(temp)
                {
                    case "GPS":
                        s = SatelliteSystem.Gps;
                        break;

                    case "GAL":
                        s = SatelliteSystem.Gal;
                        break;

                    case "BDS":
                        s = SatelliteSystem.Bds;
                        break;

                    case "QZS":
                        s = SatelliteSystem.Qzss;
                        break;

                    case "IRN":
                        s = SatelliteSystem.Irnss;
                        break;
                }
            }

            ParseIonoCorrectionsImpl(matchResult, s);
        }


        private void ParseIonoCorrectionsImpl([NotNull] Match matchResult, SatelliteSystem satelliteSystem)
        {
            var corrections = new List<double>(4);
            foreach (Capture capture in matchResult.Groups["ioncorr"].Captures)
            {
                var temp = capture.Value.Replace('D', 'E');

                try
                {
                    var corVal = Double.Parse(temp, CultureInfo.InvariantCulture);
                    corrections.Add(corVal);
                }
                // Handle this case
                //GAL    2.3500D+01 -3.5156D-02  1.4008D-02                   IONOSPHERIC CORR
                // missing 4th value
                catch(FormatException)
                {
                    corrections.Add(0.0);
                }
            }

            char ab;
            IonoCorrections ionoCor = null;
            switch(satelliteSystem)
            {
                case SatelliteSystem.Gps:
                case SatelliteSystem.Irnss:
                case SatelliteSystem.Qzss:
                    ab = Char.ToLower(matchResult.Groups["AB"].Value[0]);
                    if (ab == 'a')
                    {
                        ionoCor = new IonoCorrections(satelliteSystem, corrections, IonoCorrectionsEnum.Alpha0, ' ',
                                null);
                    }
                    else if (ab == 'b')
                    {
                        ionoCor = new IonoCorrections(satelliteSystem, corrections, IonoCorrectionsEnum.Beta0, ' ',
                                null);
                    }

                    break;
                case SatelliteSystem.Gal:
                    ionoCor = new IonoCorrections(satelliteSystem, corrections, IonoCorrectionsEnum.Ai0, ' ', null);

                    break;

                case SatelliteSystem.Bds:
                    var timeMark = matchResult.Groups["timemark"].Success
                            ? (char?)matchResult.Groups["timemark"].Value[0]
                            : null;
                    var satId = matchResult.Groups["id"].Success
                            ? (int?)Int32.Parse(matchResult.Groups["id"].Value, CultureInfo.InvariantCulture)
                            : null;
                    ab = Char.ToLowerInvariant(matchResult.Groups["AB"].Value[0]);
                    if (ab == 'a')
                    {
                        ionoCor = new IonoCorrections(satelliteSystem, corrections, IonoCorrectionsEnum.Alpha0,
                                timeMark, satId);
                    }
                    else if (ab == 'b')
                    {
                        ionoCor = new IonoCorrections(satelliteSystem, corrections, IonoCorrectionsEnum.Beta0, timeMark,
                                satId);
                    }

                    break;


                default:
                    throw new ArgumentOutOfRangeException("satelliteSystem");
            }

            Debug.Assert(ionoCor != null, "ionoCor != null");
            IonoCorrections.Add(ionoCor);
        }

        internal void ParseTimeSystemCorrection([NotNull] Match matchResult)
        {
            var temp = matchResult.Groups["corrtype"].Value;
            var corrType = (CorrectionType)Enum.Parse(typeof(CorrectionType), temp, true);
            temp = matchResult.Groups["a0"].Value;
            temp = temp.Replace('D', 'E');
            var a0 = Double.Parse(temp, CultureInfo.InvariantCulture);
            temp = matchResult.Groups["a1"].Value;
            temp = temp.Replace('D', 'E');
            var a1 = Double.Parse(temp, CultureInfo.InvariantCulture);
            temp = matchResult.Groups["reftime"].Value;
            var reftime = Int32.Parse(temp, CultureInfo.InvariantCulture);
            temp = matchResult.Groups["refweek"].Value;
            var refweek = Int32.Parse(temp, CultureInfo.InvariantCulture);
            var sbas = String.Empty;
            if (matchResult.Groups["sbas"].Success) sbas = matchResult.Groups["sbas"].Value;
            int? utc = null;
            if (matchResult.Groups["utc"].Success)
                utc = Int32.Parse(matchResult.Groups["utc"].Value, CultureInfo.InvariantCulture);
            _timeSystemCorrections.Add(new TimeSystemCorrection(corrType, a0, a1, reftime, refweek,
                    sbas, utc));
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var headerLabel in HeaderLabels)
            {
                //NOTE headers are following according to spec
                switch(headerLabel)
                {
                    case RinexHeaderLabel.VERSION:
                        string satSystem;
                        switch(SatelliteSystem)
                        {
                            case SatelliteSystem.Gps:
                                satSystem = "G: GPS";
                                break;
                            case SatelliteSystem.Glo:
                                satSystem = "R: GLONASS";
                                break;
                            case SatelliteSystem.Gal:
                                satSystem = "E: Galileo";
                                break;
                            case SatelliteSystem.Qzss:
                                satSystem = "J: QZSS";
                                break;
                            case SatelliteSystem.Bds:
                                satSystem = "C: BDS";
                                break;
                            case SatelliteSystem.Irnss:
                                satSystem = "I: IRNSS";
                                break;
                            case SatelliteSystem.Mixed:
                                satSystem = "M: MIXED";
                                break;

                            default:
                                throw new ArgumentOutOfRangeException("SatelliteSystem");
                        }

                        sb.AppendLine(String.Format("{0,9}{1,11}{2,-20}{3,-20}{4}", Version, " ", "N: GNSS NAV DATA",
                                satSystem, headerLabel));
                        break;

                    case RinexHeaderLabel.PROGRAM:
                        sb.AppendLine(String.Format("{0}{1}", HeaderProgramInfo, headerLabel));
                        continue;

                    case RinexHeaderLabel.COMMENT:
                        foreach (var comment in Comments)
                        {
                            sb.AppendLine(String.Format("{0,-60}{1}", comment, headerLabel));
                        }

                        continue;

                    case RinexHeaderLabel.LEAP_SECONDS:
                        sb.AppendLine(String.Format("{0}{1,-33}{2}", LeapSeconds, " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.IONOSPHERIC_CORRECTIONS:
                        foreach (var ionoCor in IonoCorrections)
                        {
                            foreach (var ionoCorrection in ionoCor.StringRepresentation())
                            {
                                sb.AppendLine(String.Format("{0}{1}", ionoCorrection, headerLabel));
                            }
                        }

                        continue;

                    case RinexHeaderLabel.TIME_SYSTEM_CORRECTIONS:
                        foreach (var timeSystemCorrection in TimeSystemCorrections)
                        {
                            sb.AppendLine(String.Format("{0}{1}", timeSystemCorrection, headerLabel));
                        }

                        continue;

                    case RinexHeaderLabel.END_OF_HEADER:
                        sb.AppendFormat("{0,60}", " ");
                        sb.AppendLine(headerLabel);
                        continue;
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}