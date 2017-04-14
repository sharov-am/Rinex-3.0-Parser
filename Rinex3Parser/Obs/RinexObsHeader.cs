// Rinex3Parser
// RinexObsHeader.cs-2016-08-04

#region

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Obs
{
    public class RinexObsHeader
    {
        #region Fields

        #region static fields

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
                RinexHeaderLabel.MARKER_NAME, RinexRegex.A60Regex
            },
            {
                RinexHeaderLabel.MARKER_NUMBER, RinexRegex.A20Regex
            },
            {
                RinexHeaderLabel.MARKER_TYPE, RinexRegex.A20Regex
            },
            {
                RinexHeaderLabel.OBSERVER, RinexRegex.ObserverAgencyRegex
            },
            {
                RinexHeaderLabel.RECEIVER, RinexRegex.ReceiverRegex
            },
            {
                RinexHeaderLabel.ANTENNA_TYPE, RinexRegex.AntennaTypRegex
            },
            {
                RinexHeaderLabel.POSITION, RinexRegex.PositionRegex
            },
            {
                RinexHeaderLabel.ANTENNA_OFFSETS, RinexRegex.AntennaOffsetRegex
            },
            {
                RinexHeaderLabel.ANTENNA_XYZ_OFFSETS, RinexRegex.PositionRegex
            },
            {
                RinexHeaderLabel.ANTENNA_PHASECENTER, RinexRegex.PhaseCenterRegex
            },
            {
                RinexHeaderLabel.ANTENNA_B_SIGHT_XYZ, RinexRegex.PositionRegex
            },
            {
                RinexHeaderLabel.ANTENNA_ZERODIR_AZI, RinexRegex.AntennaZeroDirAziRegex
            },
            {
                RinexHeaderLabel.ANTENNA_ZERODIR_XYZ, RinexRegex.PositionRegex
            },
            {
                RinexHeaderLabel.CENTER_OF_MASS_XYZ, RinexRegex.PositionRegex
            },
            {
                RinexHeaderLabel.SYS_OBS_TYPES, RinexRegex.SysObsTypesRegex
            },
            {
                RinexHeaderLabel.SIGNAL_STRENGTH_UNIT, RinexRegex.A20Regex
            },
            {
                RinexHeaderLabel.INTERVAL, RinexRegex.IntervalRegex
            },
            {
                RinexHeaderLabel.FIRST_OBS, RinexRegex.ObsTimeRegex
            },
            {
                RinexHeaderLabel.LAST_OBS, RinexRegex.ObsTimeRegex
            },
            {
                RinexHeaderLabel.RCV_CLOCK_OFFSET, RinexRegex.RcvClkOffsetAplRegex
            },
            {
                RinexHeaderLabel.SYS_DCBS_APPLIED, RinexRegex.CorrectionInfoRegex
            },
            {
                RinexHeaderLabel.SYS_PCVS_APPLIED, RinexRegex.CorrectionInfoRegex
            },
            {
                RinexHeaderLabel.SYS_SCALE_FACTOR, RinexRegex.SysScaleFactorRegex
            },
            {
                RinexHeaderLabel.SYS_PHASE_SHIFT, RinexRegex.SysPhaseShiftRegex
            },
            {
                RinexHeaderLabel.GLONASS_SLOT_FRQ, RinexRegex.GloSlotFreqRegex
            },
            {
                RinexHeaderLabel.GLONASS_COD_PHS_BIS, RinexRegex.GloCodePhaBiasRegex
            },
            {
                RinexHeaderLabel.GLONASS_COD_PHS_BIS2, RinexRegex.GloCodePhaBiasRegex
            },
            {
                RinexHeaderLabel.LEAP_SECONDS, RinexRegex.LeapSecondRegex
            },
            {
                RinexHeaderLabel.NUMBER_OF_SATELLITES, RinexRegex.NumberOfSatellites
            },
            {
                RinexHeaderLabel.NUMBER_OF_OBS, RinexRegex.NumberOfObsRegex
            },
        };

        #endregion

        public readonly RinexObsHeaderData ObsHeaderData = new RinexObsHeaderData();
        private GnssObservation _currentGnssSystem;
        private RinexObsHeaderData.PhaseShiftData _currentPhaseShiftData;

        private RinexObsHeaderData.ScaleFactorData _currentScaleFactorData;

        #endregion

        #region Methods

        internal bool ParseHeaderLine(string headerLine)
        {
            var header = headerLine.Substring(60).TrimEnd();
            var data = headerLine.Substring(0, 60);
            var matchResult = Match.Empty;

            ObsHeaderData.HeaderLabels.Add(header);

            if (HeaderRegexps.ContainsKey(header))
            {
                var regExp = HeaderRegexps[header];
                matchResult = regExp.Match(data);
            }

            SatelliteSystem satSystem;
            CaptureCollection captures;
            switch(header)
            {
                case RinexHeaderLabel.VERSION:
                    ObsHeaderData.Version = matchResult.Groups["rnxver"].Value.Trim();
                    if (String.IsNullOrEmpty(ObsHeaderData.Version))
                    {
                        throw new ArgumentException(@"Invalid rinex file version...");
                    }
                    if (matchResult.Groups["satsystem"].Success)
                    {
                        ObsHeaderData.SatelliteSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                    }
                    else
                    {
                        throw new ArgumentException(@"Invalid gnss system designation.");
                    }
                    return true;

                case RinexHeaderLabel.PROGRAM:
                    var programInfo = ObsHeaderData.HeaderProgramInfo;
                    programInfo.Name = matchResult.Groups["pgm"].Success
                            ? matchResult.Groups["pgm"].Value.Trim()
                            : String.Empty;
                    programInfo.AgencyInfo = matchResult.Groups["runby"].Success
                            ? matchResult.Groups["runby"].Value.Trim()
                            : String.Empty;

                    if (matchResult.Groups["date"].Success)
                    {
                        var dateVal = matchResult.Groups["date"].Value;
                        var temp = dateVal.Substring(0, 15);
                        var temp2 = DateTime.ParseExact(temp, "yyyyMMdd HHmmss", null);
                        var zone = dateVal.Substring(16).Trim();
                        programInfo.FileCreationDateTime = DateTime.SpecifyKind(temp2,
                                String.Equals(zone, "lcl", StringComparison.InvariantCultureIgnoreCase)
                                        ? DateTimeKind.Local
                                        : DateTimeKind.Utc);
                    }
                    return true;

                case RinexHeaderLabel.COMMENT:
                    ObsHeaderData.Comments.Add(matchResult.Groups[0].Value);
                    return true;

                case RinexHeaderLabel.MARKER_NAME:
                    if (matchResult.Groups[0].Success)
                    {
                        ObsHeaderData.MarkerName = matchResult.Groups[0].Value.Trim();
                        if (String.IsNullOrEmpty(ObsHeaderData.MarkerName))
                        {
                            throw new ArgumentException("Marker name is empty...");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("No marker name header label...");
                    }
                    return true;

                case RinexHeaderLabel.MARKER_NUMBER:
                    ObsHeaderData.MarkerNumber = matchResult.Groups[0].Value.Trim();
                    return true;

                case RinexHeaderLabel.MARKER_TYPE:
                    ObsHeaderData.MarkerType = matchResult.Groups[0].Value.Trim();
                    return true;

                case RinexHeaderLabel.OBSERVER:
                    ObsHeaderData.Observer = matchResult.Groups["observer"].Success
                            ? matchResult.Groups["observer"].Value.Trim()
                            : String.Empty;
                    ObsHeaderData.Agency = matchResult.Groups["agency"].Success
                            ? matchResult.Groups["agency"].Value.Trim()
                            : String.Empty;
                    return true;

                case RinexHeaderLabel.RECEIVER:
                    ObsHeaderData.RcvInfo.Number = matchResult.Groups["recnum"].Success
                            ? matchResult.Groups["recnum"].Value.Trim()
                            : String.Empty;
                    ObsHeaderData.RcvInfo.RcvType = matchResult.Groups["type"].Success
                            ? matchResult.Groups["type"].Value.Trim()
                            : String.Empty;
                    ObsHeaderData.RcvInfo.Version = matchResult.Groups["vers"].Success
                            ? matchResult.Groups["vers"].Value.Trim()
                            : String.Empty;
                    return true;

                case RinexHeaderLabel.ANTENNA_TYPE:
                    var antennaData = ObsHeaderData.AntInfo;
                    antennaData.Type = matchResult.Groups["anttype"].Value;
                    antennaData.Number = matchResult.Groups["antnum"].Success
                            ? matchResult.Groups["antnum"].Value.Trim()
                            : String.Empty;
                    return true;

                case RinexHeaderLabel.POSITION:
                    ObsHeaderData.X = Double.Parse(matchResult.Groups["X"].Value, CultureInfo.InvariantCulture);
                    ObsHeaderData.Y = Double.Parse(matchResult.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    ObsHeaderData.Z = Double.Parse(matchResult.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_OFFSETS:
                    antennaData = ObsHeaderData.AntInfo;
                    antennaData.DeltaH = Double.Parse(matchResult.Groups["H"].Value, CultureInfo.InvariantCulture);
                    antennaData.DeltaE = Double.Parse(matchResult.Groups["E"].Value, CultureInfo.InvariantCulture);
                    antennaData.DeltaN = Double.Parse(matchResult.Groups["N"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_XYZ_OFFSETS:
                    antennaData = ObsHeaderData.AntInfo;
                    antennaData.DeltaX = Double.Parse(matchResult.Groups["X"].Value, CultureInfo.InvariantCulture);
                    antennaData.DeltaY = Double.Parse(matchResult.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    antennaData.DeltaZ = Double.Parse(matchResult.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_PHASECENTER:
                    var antennaPhC = ObsHeaderData.AntInfo.AntennaPhaseCenter;
                    antennaPhC.SatelliteSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                    antennaPhC.ObservationCode = (ObservationCode)Enum.Parse(typeof(ObservationCode),
                            matchResult.Groups["ObsCode"].Value, true);
                    antennaPhC.DeltaN = Double.Parse(matchResult.Groups["N"].Value, CultureInfo.InvariantCulture);
                    antennaPhC.DeltaE = Double.Parse(matchResult.Groups["E"].Value, CultureInfo.InvariantCulture);
                    antennaPhC.DeltaU = Double.Parse(matchResult.Groups["U"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_B_SIGHT_XYZ:
                    antennaData = ObsHeaderData.AntInfo;
                    antennaData.BSightN = Double.Parse(matchResult.Groups["X"].Value, CultureInfo.InvariantCulture);
                    antennaData.BSightE = Double.Parse(matchResult.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    antennaData.BSightU = Double.Parse(matchResult.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_ZERODIR_AZI:
                    antennaData = ObsHeaderData.AntInfo;
                    antennaData.ZerodirAzi = Double.Parse(matchResult.Groups["AZeroDirection"].Value,
                            CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.ANTENNA_ZERODIR_XYZ:
                    antennaData = ObsHeaderData.AntInfo;
                    antennaData.ZeroDirN = Double.Parse(matchResult.Groups["X"].Value, CultureInfo.InvariantCulture);
                    antennaData.ZeroDirE = Double.Parse(matchResult.Groups["Y"].Value, CultureInfo.InvariantCulture);
                    antennaData.ZeroDirU = Double.Parse(matchResult.Groups["Z"].Value, CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.CENTER_OF_MASS_XYZ:
                    ObsHeaderData.CenterOfMassX = Double.Parse(matchResult.Groups["X"].Value,
                            CultureInfo.InvariantCulture);
                    ObsHeaderData.CenterOfMassY = Double.Parse(matchResult.Groups["Y"].Value,
                            CultureInfo.InvariantCulture);
                    ObsHeaderData.CenterOfMassZ = Double.Parse(matchResult.Groups["Z"].Value,
                            CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.SYS_OBS_TYPES:
                    if (matchResult.Groups["satsystem"].Success)
                    {
                        satSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                        _currentGnssSystem = ObsHeaderData.ObsMetaData[satSystem];
                    }
                    if (matchResult.Groups["obs"].Success)
                    {
                        Debug.Assert(_currentGnssSystem != null, "_currentGnssSystem != null");
                        //capture is '  L5Q' but enum parse calls trim intenally
                        foreach (Capture capture in matchResult.Groups["obs"].Captures)
                        {
                            var obsCode = (ObservationCode)Enum.Parse(typeof(ObservationCode), capture.Value, true);
                            _currentGnssSystem.AddObservation(obsCode);
                        }
                    }

                    return true;

                case RinexHeaderLabel.SIGNAL_STRENGTH_UNIT:
                    ObsHeaderData.SignalStrengthUnit = matchResult.Groups[0].Success
                            ? matchResult.Groups[0].Value.Trim()
                            : String.Empty;
                    return true;

                case RinexHeaderLabel.INTERVAL:
                    ObsHeaderData.Interval = Double.Parse(matchResult.Groups["interval"].Value,
                            CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.FIRST_OBS:
                case RinexHeaderLabel.LAST_OBS:
                    var year = Int32.Parse(matchResult.Groups["year"].Value, CultureInfo.InvariantCulture);
                    var month = Int32.Parse(matchResult.Groups["month"].Value, CultureInfo.InvariantCulture);
                    var day = Int32.Parse(matchResult.Groups["day"].Value, CultureInfo.InvariantCulture);
                    var hour = Int32.Parse(matchResult.Groups["hour"].Value, CultureInfo.InvariantCulture);
                    var min = Int32.Parse(matchResult.Groups["min"].Value, CultureInfo.InvariantCulture);
                    var sec = Double.Parse(matchResult.Groups["preciseSec"].Value, CultureInfo.InvariantCulture);
                    var timeSystem = Enum.Parse(typeof(GnssTimeSystem), matchResult.Groups["timesystem"].Value, true);
                    ObsHeaderData.GnssTimeSystem = (GnssTimeSystem)timeSystem;
                    switch(header)
                    {
                        case RinexHeaderLabel.FIRST_OBS:
                            ObsHeaderData.FirstObs = new DateTime(year, month, day, hour, min, (int)sec);
                            break;
                        default:
                            ObsHeaderData.LastObs = new DateTime(year, month, day, hour, min, (int)sec);
                            break;
                    }
                    return true;

                case RinexHeaderLabel.RCV_CLOCK_OFFSET:
                    ObsHeaderData.ApplyRcvClockOffset =
                            Int32.Parse(matchResult.Groups["rcv_clk_offs_appl"].Value, CultureInfo.InvariantCulture) ==
                            1;
                    return true;

                case RinexHeaderLabel.SYS_DCBS_APPLIED:
                    satSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                    if (!ObsHeaderData.DcbsCorrections.ContainsKey(satSystem))
                    {
                        var pgm = matchResult.Groups["pgm"].Value;
                        var corrsource = matchResult.Groups["corrsource"].Value;
                        ObsHeaderData.DcbsCorrections.Add(satSystem, new[] {pgm, corrsource});
                    }
                    else
                    {
                        throw new ArgumentException(
                                String.Format(
                                        "Invalid rinex header format. More than 1 row for DCBS corrections for {0}",
                                        satSystem));
                    }
                    return true;

                case RinexHeaderLabel.SYS_PCVS_APPLIED:
                    satSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                    if (!ObsHeaderData.PcvsCorrections.ContainsKey(satSystem))
                    {
                        var pgm = matchResult.Groups["pgm"].Value;
                        var corrsource = matchResult.Groups["corrsource"].Value;
                        ObsHeaderData.PcvsCorrections.Add(satSystem, new[] {pgm, corrsource});
                    }
                    else
                    {
                        throw new ArgumentException(
                                String.Format(
                                        "Invalid rinex header format. More than 1 row for DCBS corrections for {0}",
                                        satSystem));
                    }
                    return true;

                case RinexHeaderLabel.SYS_SCALE_FACTOR:
                    if (matchResult.Groups["satsystem"].Success)
                    {
                        //ignore number observation types involved, always save current sat. system in case
                        //number of observation types > 12
                        satSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();
                        var scaleFactorData = new RinexObsHeaderData.ScaleFactorData
                        {
                            ScaleFactor = Int32.Parse(matchResult.Groups["Factor"].Value,
                                    CultureInfo.InvariantCulture)
                        };

                        foreach (Capture code in matchResult.Groups["obs"].Captures)
                        {
                            var obsCode = (ObservationCode)Enum.Parse(typeof(ObservationCode), code.Value, true);
                            scaleFactorData.ObservationCodes.Add(obsCode);
                        }

                        //add to dictionary
                        ObsHeaderData.ScaleFactorsDict.Add(satSystem, scaleFactorData);
                        _currentScaleFactorData = scaleFactorData;
                    }
                    else
                    {
                        Debug.Assert(_currentScaleFactorData != null, "_currentScaleFactorData != null");
                        foreach (Capture code in matchResult.Groups["obs"].Captures)
                        {
                            var obsCode = (ObservationCode)Enum.Parse(typeof(ObservationCode), code.Value, true);
                            _currentScaleFactorData.ObservationCodes.Add(obsCode);
                        }
                    }
                    return true;


                case RinexHeaderLabel.SYS_PHASE_SHIFT:
                    if (matchResult.Groups["satsystem"].Success)
                    {
                        //ignore number observation types involved, always save current sat. system in case
                        //number of observation types > 12
                        satSystem = matchResult.Groups["satsystem"].Value[0].ConvertToEnum();

                        var phaseShiftData = new RinexObsHeaderData.PhaseShiftData
                        {
                            ObservationCode =
                                (ObservationCode)
                                        Enum.Parse(typeof(ObservationCode), matchResult.Groups["obscode"].Value, true),
                            CorrectionCycles =
                                Double.Parse(matchResult.Groups["corrcycles"].Value, CultureInfo.InvariantCulture)
                        };
                        if (matchResult.Groups["sta"].Success)
                        {
                            foreach (Capture satelliteNum in matchResult.Groups["sta"].Captures)
                            {
                                var temp = satelliteNum.Value.Trim().Substring(1);
                                var sNum = Int32.Parse(temp, CultureInfo.InvariantCulture);
                                phaseShiftData.Satellites.Add(sNum);
                            }
                        }

                        if (ObsHeaderData.PhaseShiftDict.ContainsKey(satSystem))
                        {
                            ObsHeaderData.PhaseShiftDict[satSystem].Add(phaseShiftData);
                        }
                        else
                        {
                            //add new dict item
                            ObsHeaderData.PhaseShiftDict.Add(satSystem,
                                    new List<RinexObsHeaderData.PhaseShiftData> {phaseShiftData});
                        }
                        _currentPhaseShiftData = phaseShiftData;
                    }
                    else
                    {
                        //continuation line
                        if (matchResult.Groups["sta"].Success)
                        {
                            Debug.Assert(_currentPhaseShiftData != null, "_currentPhaseShiftData != null");
                            foreach (Capture satelliteNum in matchResult.Groups["sta"].Captures)
                            {
                                var temp = satelliteNum.Value.Trim().Substring(1);
                                var sNum = Int32.Parse(temp, CultureInfo.InvariantCulture);
                                _currentPhaseShiftData.Satellites.Add(sNum);
                            }
                        }
                    }
                    return true;

                case RinexHeaderLabel.GLONASS_SLOT_FRQ:
                    //ignore num of satellites...
                    captures = matchResult.Groups["satfreq"].Captures;
                    foreach (Capture capture in captures)
                    {
                        var temp = capture.Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        //smth like ' R01  1'
                        var gsat = (GloSatellite)Enum.Parse(typeof(GloSatellite), temp[0], true);
                        var freq = Int32.Parse(temp[1], CultureInfo.InvariantCulture);
                        ObsHeaderData.GloSlotFreqDict.Add(gsat, freq);
                    }
                    return true;

                case RinexHeaderLabel.GLONASS_COD_PHS_BIS:
                case RinexHeaderLabel.GLONASS_COD_PHS_BIS2:
                    captures = matchResult.Groups["phcor"].Captures;
                    foreach (Capture phCor in captures)
                    {
                        var temp = phCor.Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries); //C1C  -71.940
                        var obsCode = (ObservationCode)Enum.Parse(typeof(ObservationCode), temp[0], true);
                        var corValue = Double.Parse(temp[1], CultureInfo.InvariantCulture);
                        ObsHeaderData.GloPhaseBiasCor.Add(obsCode, corValue);
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
                        var temp = matchResult.Groups["satsystem"].Value;
                        if (String.Equals(temp, "bds", StringComparison.InvariantCultureIgnoreCase) ||
                            String.Equals(temp, "bdt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ts = GnssTimeSystem.Bdt;
                        }
                    }
                    ObsHeaderData.LeapSeconds_ = new LeapSeconds
                    {
                        CurrentNumOfLeapSeconds = numOfLeapSeconds,
                        FutureNumOfLeapSeconds = futurePastLeapSeconds,
                        Week = weekNum,
                        Day = dayNum,
                        TimeSystem = ts
                    };

                    return true;

                case RinexHeaderLabel.NUMBER_OF_SATELLITES:
                    ObsHeaderData.NumberOfSatellites = Int32.Parse(matchResult.Groups["numofsat"].Value,
                            CultureInfo.InvariantCulture);
                    return true;

                case RinexHeaderLabel.NUMBER_OF_OBS: //PRN / # OF OBS
                    if (matchResult.Groups["type"].Success)
                    {
                        satSystem = matchResult.Groups["type"].Value[0].ConvertToEnum();
                        //ignore sbas here
                        if (!ObsHeaderData.ObsMetaData.ContainsKey(satSystem)) return true;
                        _currentGnssSystem = ObsHeaderData.ObsMetaData[satSystem];
                        var satPrnId = matchResult.Groups["type"].Value + matchResult.Groups["satnum"].Value;
                        int satPrn;
                        switch(satSystem)
                        {
                            case SatelliteSystem.Gps:
                                satPrn = (int)Enum.Parse(typeof(GpsSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Glo:
                                satPrn = (int)Enum.Parse(typeof(GloSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Gal:
                                satPrn = (int)Enum.Parse(typeof(GalSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Qzss:
                                satPrn = (int)Enum.Parse(typeof(QzssSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Bds:
                                satPrn = (int)Enum.Parse(typeof(BdsSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Irnss:
                                satPrn = (int)Enum.Parse(typeof(IrnssSatellite), satPrnId, true);
                                break;
                            case SatelliteSystem.Sbas:
                                satPrn = (int)Enum.Parse(typeof(SbasSatellite), satPrnId, true);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(@"Invalid satellite constellation");
                        }
                        var i = 0;
                        foreach (Capture capture in matchResult.Groups["obs"].Captures)
                        {
                            var numOfObs = Int32.Parse(capture.Value, CultureInfo.InvariantCulture);
                            _currentGnssSystem.AddNumberOfObservations(i, satPrn, numOfObs);
                            i++; //incr obs code
                        }
                    }
                    else
                    {
                        if (matchResult.Groups["obs"].Success)
                        {
                            Debug.Assert(_currentGnssSystem != null, "_currentGnssSystem != null");
                            var obsIndex = _currentGnssSystem.ObsIndex;
                            var i = ++obsIndex; //we have new line so increase number of current PRN
                            foreach (Capture capture in matchResult.Groups["obs"].Captures)
                            {
                                var numOfObs = Int32.Parse(capture.Value, CultureInfo.InvariantCulture);
                                _currentGnssSystem.AddNumberOfObservations(i, numOfObs);
                                i++; //incr obs code
                            }
                        }
                    }
                    return true;

                case RinexHeaderLabel.END_OF_HEADER:
                    return false;
            }

            return false;
        }

        #endregion

        //rinex fields format description -- http://gage14.upc.es/gLAB/HTML/LaunchHTML.html
        
    }


    public class RinexObsHeaderData : EventArgs
    {
        #region Fields

        public readonly AntennaInfo AntInfo = new AntennaInfo();
        public readonly List<string> Comments = new List<string>();

        public readonly Dictionary<SatelliteSystem, string[]> DcbsCorrections =
                new Dictionary<SatelliteSystem, string[]>();

        public readonly Dictionary<ObservationCode, double> GloPhaseBiasCor = new Dictionary<ObservationCode, double>();

        public readonly Dictionary<GloSatellite, int> GloSlotFreqDict = new Dictionary<GloSatellite, int>();


        //use to track header labels. Useful for header printing. 
        //OrderedSet preserve insert order.
        internal readonly OrderedSet<string> HeaderLabels = new OrderedSet<string>();
        public readonly ProgramInfo HeaderProgramInfo = new ProgramInfo();

        public readonly Dictionary<SatelliteSystem, GnssObservation> ObsMetaData =
                new Dictionary<SatelliteSystem, GnssObservation>();

        public readonly Dictionary<SatelliteSystem, string[]> PcvsCorrections =
                new Dictionary<SatelliteSystem, string[]>();

        public readonly Dictionary<SatelliteSystem, List<PhaseShiftData>> PhaseShiftDict =
                new Dictionary<SatelliteSystem, List<PhaseShiftData>>();

        public readonly ReceiverInfo RcvInfo = new ReceiverInfo();

        public readonly Dictionary<SatelliteSystem, ScaleFactorData> ScaleFactorsDict =
                new Dictionary<SatelliteSystem, ScaleFactorData>();

        #endregion

        #region Constructor

        public RinexObsHeaderData()
        {
            //NOTE no sbas here
            ObsMetaData.Add(SatelliteSystem.Gps, new GnssObservations<GpsSatellite>());
            ObsMetaData.Add(SatelliteSystem.Glo, new GnssObservations<GloSatellite>());
            ObsMetaData.Add(SatelliteSystem.Gal, new GnssObservations<GalSatellite>());
            ObsMetaData.Add(SatelliteSystem.Qzss, new GnssObservations<QzssSatellite>());
            ObsMetaData.Add(SatelliteSystem.Bds, new GnssObservations<BdsSatellite>());
            ObsMetaData.Add(SatelliteSystem.Irnss, new GnssObservations<IrnssSatellite>());
            ObsMetaData.Add(SatelliteSystem.Sbas, new GnssObservations<SbasSatellite>());
            GnssTimeSystem = GnssTimeSystem.Gps;
            MarkerType = "GEODETIC"; //by default
        }

        #endregion

        #region Properties

        public string Version { get; set; }
        public SatelliteSystem SatelliteSystem { get; set; }
        public string MarkerName { get; set; }
        public string MarkerNumber { get; set; }
        public string MarkerType { get; set; }
        public string Observer { get; set; }
        public string Agency { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double? CenterOfMassX { get; set; }
        public double? CenterOfMassY { get; set; }
        public double? CenterOfMassZ { get; set; }
        public string SignalStrengthUnit { get; set; }
        public double? Interval { get; set; }
        public DateTime FirstObs { get; set; }
        public DateTime LastObs { get; set; }
        public GnssTimeSystem GnssTimeSystem { get; set; }
        public bool ApplyRcvClockOffset { get; set; }
        public LeapSeconds LeapSeconds_ { get; set; }
        public int NumberOfSatellites { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var headerLabel in HeaderLabels)
            {
                //NOTE headers are following according to spec
                switch(headerLabel)
                {
                    case RinexHeaderLabel.VERSION:
                        var satSystem = Enum.GetName(typeof(SatelliteSystem), SatelliteSystem);
                        var satSymbol = 'M';
                        if (satSystem != null) satSymbol = satSystem[0];
                        sb.AppendLine(String.Format("{0,9}{1,11}{2,-20}{3,-20}{4}", Version, " ", "OBSERVATION DATA",
                                satSymbol, headerLabel));
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

                    case RinexHeaderLabel.MARKER_NAME:
                        sb.AppendLine(String.Format("{0,-60}{1}", MarkerName, headerLabel));
                        continue;

                    case RinexHeaderLabel.MARKER_NUMBER:
                        sb.AppendLine(String.Format("{0,-20}{1,-40}{2}", MarkerNumber, " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.MARKER_TYPE:
                        sb.AppendLine(String.Format("{0,-20}{1,-40}{2}", MarkerType, " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.OBSERVER:
                        sb.AppendLine(String.Format("{0,-20}{1,-40}{2}", Observer, Agency, headerLabel));
                        continue;

                    case RinexHeaderLabel.RECEIVER:
                        sb.AppendLine(String.Format("{0}{1}", RcvInfo, headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_TYPE:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.NumberAndType(), headerLabel));
                        continue;
                    case RinexHeaderLabel.POSITION:
                        sb.AppendLine(String.Format("{0,14}{1,14}{2,14}{3,18}{4,-60}",
                                X.ToString("0.0000", CultureInfo.InvariantCulture),
                                Y.ToString("0.0000", CultureInfo.InvariantCulture),
                                Z.ToString("0.0000", CultureInfo.InvariantCulture),
                                " ",
                                headerLabel));
                        continue;
                    case RinexHeaderLabel.ANTENNA_OFFSETS:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.DeltaHEN(), headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_XYZ_OFFSETS:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.DeltaXYZ(), headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_PHASECENTER:

                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.AntennaPhaseCenter, headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_B_SIGHT_XYZ:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.BSightXYZ(), headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_ZERODIR_AZI:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.ZeroDirAzi(), headerLabel));
                        continue;

                    case RinexHeaderLabel.ANTENNA_ZERODIR_XYZ:
                        sb.AppendLine(String.Format("{0}{1,-60}", AntInfo.ZeroDirXYZ(), headerLabel));
                        continue;

                    case RinexHeaderLabel.CENTER_OF_MASS_XYZ:
                        Debug.Assert(CenterOfMassX != null, "CenterOfMassX != null");
                        Debug.Assert(CenterOfMassY != null, "CenterOfMassY != null");
                        Debug.Assert(CenterOfMassZ != null, "CenterOfMassZ != null");
                        sb.AppendLine(String.Format("{0}{1,-60}", String.Format("{0,14}{1,14}{2,14}{3,18}",
                                CenterOfMassX.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                                CenterOfMassY.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                                CenterOfMassZ.Value.ToString("0.0000", CultureInfo.InvariantCulture), " "), headerLabel));
                        continue;

                    case RinexHeaderLabel.SYS_OBS_TYPES:
                        var observations = new List<StringBuilder>();
                        foreach (var gnssObservation in ObsMetaData)
                        {
                            switch(gnssObservation.Key)
                            {
                                case SatelliteSystem.Gps:
                                    var gps = gnssObservation.Value as GnssObservations<GpsSatellite>;
                                    var temp = gps.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Glo:
                                    var glo = gnssObservation.Value as GnssObservations<GloSatellite>;
                                    temp = glo.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Gal:
                                    var gal = gnssObservation.Value as GnssObservations<GalSatellite>;
                                    temp = gal.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Qzss:
                                    var qzss = gnssObservation.Value as GnssObservations<QzssSatellite>;
                                    temp = qzss.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Bds:
                                    var bds = gnssObservation.Value as GnssObservations<BdsSatellite>;
                                    temp = bds.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Irnss:
                                    var irnss = gnssObservation.Value as GnssObservations<IrnssSatellite>;
                                    temp = irnss.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;
                                case SatelliteSystem.Sbas:
                                    var sbas = gnssObservation.Value as GnssObservations<SbasSatellite>;
                                    temp = sbas.RnxPrintGnssObs();
                                    observations.AddRange(temp);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("SatelliteSystem");
                            }
                        }
                        foreach (var observation in observations)
                        {
                            Debug.Assert(observation.Length <= 60, "observation.Length <= 60");
                            observation.AppendFormat("{0}", headerLabel);
                            sb.AppendLine(observation.ToString());
                        }
                        continue;

                    case RinexHeaderLabel.SIGNAL_STRENGTH_UNIT:
                        sb.AppendLine(String.Format("{0,-20}{1,60}", SignalStrengthUnit, headerLabel));
                        continue;

                    case RinexHeaderLabel.INTERVAL:
                        Debug.Assert(Interval != null, "Interval != null");
                        sb.AppendLine(String.Format("{0,10}{1,50}{2,-60}",
                                Interval.Value.ToString("0.000", CultureInfo.InvariantCulture), " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.FIRST_OBS:
                        sb.AppendLine(String.Format("{0,6}{1,6}{2,6}{3,6}{4,6}{5,13}{6,8}{7,9}{8}", FirstObs.Year,
                                FirstObs.Month.ToString("00", CultureInfo.InvariantCulture),
                                FirstObs.Day.ToString("00", CultureInfo.InvariantCulture),
                                FirstObs.Hour.ToString("00", CultureInfo.InvariantCulture),
                                FirstObs.Minute.ToString("00", CultureInfo.InvariantCulture),
                                FirstObs.Second.ToString("0.0000000", CultureInfo.InvariantCulture),
                                GnssTimeSystem.ToString().ToUpper(), " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.LAST_OBS:
                        sb.AppendLine(String.Format("{0,6}{1,6}{2,6}{3,6}{4,6}{5,13}{6,8}{7,9}{8}", LastObs.Year,
                                LastObs.Month.ToString("00", CultureInfo.InvariantCulture),
                                LastObs.Day.ToString("00", CultureInfo.InvariantCulture),
                                LastObs.Hour.ToString("00", CultureInfo.InvariantCulture),
                                LastObs.Minute.ToString("00", CultureInfo.InvariantCulture),
                                LastObs.Second.ToString("0.0000000", CultureInfo.InvariantCulture),
                                GnssTimeSystem.ToString().ToUpper(), " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.RCV_CLOCK_OFFSET:
                        sb.AppendLine(String.Format("{0,6}{1,54}{2}", ApplyRcvClockOffset ? 1 : 0, " ", headerLabel));
                        continue;


                    case RinexHeaderLabel.SYS_DCBS_APPLIED:
                        foreach (var pair in DcbsCorrections)
                        {
                            var satSys = pair.Key.ToString()[0];
                            var pgm = pair.Value[0];
                            var sourceOfCorrections = pair.Value[1];
                            sb.AppendLine(String.Format("{0}{1}{2,-17}{3,41}{4}", satSys, " ", pgm, sourceOfCorrections,
                                    headerLabel));
                        }
                        continue;

                    case RinexHeaderLabel.SYS_PCVS_APPLIED:
                        foreach (var pair in PcvsCorrections)
                        {
                            var satSys = pair.Key.ToString()[0];
                            var pgm = pair.Value[0];
                            var sourceOfCorrections = pair.Value[1];
                            sb.AppendLine(String.Format("{0}{1}{2,-17}{3,41}{4}", satSys, " ", pgm, sourceOfCorrections,
                                    headerLabel));
                        }
                        continue;

                    case RinexHeaderLabel.SYS_SCALE_FACTOR:
                        foreach (var scaleFactorData in ScaleFactorsDict)
                        {
                            var satSys = scaleFactorData.Key.ToString()[0];
                            var temp = scaleFactorData.Value;
                            var scaleFactor = temp.ScaleFactor;
                            if (temp.ObservationCodes.Count == 0)
                            {
                                sb.AppendLine(String.Format("{0}{1}{2:0000}{1,54}{3}", satSys, " ", scaleFactor, headerLabel));
                            }
                            else
                            {
                                var obsCount = 0;
                                sb.AppendFormat("{0}{1}{2:0000}{1,2}{3}", satSys, " ", scaleFactor,
                                        temp.ObservationCodes.Count);

                                foreach (var observationCode in temp.ObservationCodes)
                                {
                                    obsCount++;
                                    if (obsCount%12 == 0)
                                    {
                                        sb.AppendLine(String.Format(" {0}  {1}", observationCode, headerLabel));
                                        //padding new line with 10 spaces according to spec.
                                        if (temp.ObservationCodes.Count > obsCount) sb.AppendFormat("{0,10}", " ");
                                        //Use continuation line(s) for more than 12 observation types.
                                    }
                                    else
                                    {
                                        sb.AppendFormat(" {0}", observationCode);
                                    }
                                }
                                if (obsCount%12 > 0)
                                {
                                    sb.Append(' ', 60 - (10 + (obsCount%12)*4));
                                    //60 -- start of header, 10 is 10X space, and obsCount * 4, where 4 is (1X,A3)
                                    sb.AppendLine(String.Format("{0}", headerLabel));
                                }
                            }
                        }
                        continue;

                    case RinexHeaderLabel.SYS_PHASE_SHIFT:

                        foreach (var phaseShiftDataKv in PhaseShiftDict)
                        {
                            var satSys = phaseShiftDataKv.Key;
                            var satSysKey = satSys.SatelliteCountryCode();

                            //iterate for every obs code
                            foreach (PhaseShiftData phaseShiftData in phaseShiftDataKv.Value)
                            {
                                var phaseShiftValue = phaseShiftData.CorrectionCycles.ToString("0.00000",
                                        CultureInfo.InvariantCulture);
                                var obsCode = phaseShiftData.ObservationCode;
                                if (phaseShiftData.AllSatellites)
                                {
                                    sb.AppendLine(String.Format("{0}{1}{2}{1}{3,8}{1,46}{4}", satSysKey, " ", obsCode,
                                            phaseShiftValue, headerLabel));
                                }
                                else
                                {
                                    //G L1C  0.00000   9 G07 G09 G12 G13 G15 G20 G21 G26 G31 G32  
                                    var satCount = 0;
                                    sb.AppendFormat("{0}{1}{2}{1}{3,8}{1,2}{4,2}", satSysKey, " ", obsCode,
                                            phaseShiftValue, phaseShiftData.Satellites.Count);
                                    //iterate for satellite prn
                                    foreach (var sat in phaseShiftData.Satellites)
                                    {
                                        satCount++;
                                        if (satCount%10 == 0)
                                        {
                                            var satPrn = ParserUtils.SatellitePrnString(satSys, sat);
                                            sb.AppendLine(String.Format(" {0}  {1}", satPrn, headerLabel));
                                            //padding new line with 10 spaces according to spec.
                                            //Use continuation line(s) for more than 10 satellites.
                                            if (phaseShiftData.Satellites.Count > satCount)
                                                sb.AppendFormat("{0,18}", " "); //continue within new line
                                        }
                                        else
                                        {
                                            var satPrn = ParserUtils.SatellitePrnString(satSys, sat);
                                            sb.AppendFormat(" {0}", satPrn);
                                        }
                                    }
                                    if (satCount%10 > 0)
                                    {
                                        sb.Append(' ', 60 - (18 + (satCount%10)*4));
                                        sb.AppendLine(String.Format("{0}", headerLabel));
                                    }
                                }
                            }
                        }
                        continue;

                    case RinexHeaderLabel.GLONASS_SLOT_FRQ:
                        sb.AppendFormat("{0,3}{1}", GloSlotFreqDict.Count, " ");
                        var gFreqCounter = 0;
                        foreach (var slotFreq in GloSlotFreqDict)
                        {
                            gFreqCounter++;
                            if (gFreqCounter%8 == 0)
                            {
                                sb.AppendLine(String.Format("{0} {1,2} {2}", slotFreq.Key, slotFreq.Value, headerLabel));
                                if (GloSlotFreqDict.Count > gFreqCounter) sb.AppendFormat("{0,4}", " ");
                            }
                            else
                            {
                                sb.AppendFormat("{0} {1,2} ", slotFreq.Key, slotFreq.Value);
                            }
                        }
                        if (gFreqCounter%8 > 0)
                        {
                            sb.Append(' ', 60 - (4 + (gFreqCounter%8)*7));
                            sb.AppendLine(String.Format("{0}", headerLabel));
                        }
                        continue;

                    case RinexHeaderLabel.GLONASS_COD_PHS_BIS:
                    case RinexHeaderLabel.GLONASS_COD_PHS_BIS2:
                        foreach (var phBiasCorrection in GloPhaseBiasCor)
                        {
                            sb.AppendFormat(" {0,-3} {1,8}", phBiasCorrection.Key,
                                    phBiasCorrection.Value.ToString("0.000", CultureInfo.InvariantCulture));
                        }
                        sb.Append(' ', 60 - 4*13);
                        sb.AppendLine(String.Format("{0}", headerLabel));
                        continue;

                    case RinexHeaderLabel.LEAP_SECONDS:
                        sb.AppendLine(String.Format("{0}{1,-33}{2}", LeapSeconds_, " ", headerLabel));
                        continue;

                    case RinexHeaderLabel.NUMBER_OF_SATELLITES:
                        sb.AppendLine(String.Format("{0,6}{1,54}{2}", NumberOfSatellites, " ", headerLabel));
                        continue;


                    case RinexHeaderLabel.NUMBER_OF_OBS: //PRN / # OF OBS
                        var observationsCount = new List<StringBuilder>();
                        foreach (var gnssObservation in ObsMetaData)
                        {
                            switch(gnssObservation.Key)
                            {
                                case SatelliteSystem.Gps:
                                    var gps = gnssObservation.Value as GnssObservations<GpsSatellite>;
                                    var temp = gps.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Glo:
                                    var glo = gnssObservation.Value as GnssObservations<GloSatellite>;
                                    temp = glo.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Gal:
                                    var gal = gnssObservation.Value as GnssObservations<GalSatellite>;
                                    temp = gal.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Qzss:
                                    var qzss = gnssObservation.Value as GnssObservations<QzssSatellite>;
                                    temp = qzss.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Bds:
                                    var bds = gnssObservation.Value as GnssObservations<BdsSatellite>;
                                    temp = bds.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Irnss:
                                    var irnss = gnssObservation.Value as GnssObservations<IrnssSatellite>;
                                    temp = irnss.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;
                                case SatelliteSystem.Sbas:
                                    var sbas = gnssObservation.Value as GnssObservations<SbasSatellite>;
                                    temp = sbas.RnxPrintGnssObsNumber();
                                    observationsCount.AddRange(temp);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("SatelliteSystem");
                            }
                        }
                        foreach (var observation in observationsCount)
                        {
                            Debug.Assert(observation.Length <= 60, "observation.Length <= 60");
                            observation.AppendFormat("{0}", headerLabel);
                            sb.AppendLine(observation.ToString());
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

        #region Extended data

        public class ReceiverInfo
        {
            #region Properties

            public string Number { get; set; }
            public string RcvType { get; set; }
            public string Version { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                return String.Format("{0,-20}{1,-20}{2,-20}", Number, RcvType, Version);
            }

            #endregion
        }


        public class AntennaInfo
        {
            #region Fields

            public readonly AntennaPhaseCenter AntennaPhaseCenter = new AntennaPhaseCenter();

            #endregion

            #region Properties

            public string Number { get; set; }
            public string Type { get; set; }
            public double DeltaH { get; set; }
            public double DeltaE { get; set; }
            public double DeltaN { get; set; }
            public double? DeltaX { get; set; }
            public double? DeltaY { get; set; }
            public double? DeltaZ { get; set; }
            public double? BSightN { get; set; }
            public double? BSightE { get; set; }
            public double? BSightU { get; set; }
            public double? ZerodirAzi { get; set; }
            public double? ZeroDirN { get; set; }
            public double? ZeroDirE { get; set; }
            public double? ZeroDirU { get; set; }

            #endregion

            #region Methods

            [Pure]
            public string NumberAndType()
            {
                return String.Format("{0,-20}{1,-20}{2,-20}", Number, Type, " ");
            }

            [Pure]
            public string DeltaXYZ()
            {
                Debug.Assert(DeltaX != null, "DeltaX != null");
                Debug.Assert(DeltaY != null, "DeltaY != null");
                Debug.Assert(DeltaZ != null, "DeltaZ != null");
                return String.Format("{0,14}{1,14}{2,14}{3,18}",
                        DeltaX.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaY.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaZ.Value.ToString("0.0000", CultureInfo.InvariantCulture), " ");
            }

            [Pure]
            // ReSharper disable once InconsistentNaming
            public string DeltaHEN()
            {
                return String.Format("{0,14}{1,14}{2,14}{3,18}", DeltaH.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaE.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaN.ToString("0.0000", CultureInfo.InvariantCulture), " ");
            }

            [Pure]
            public string BSightXYZ()
            {
                Debug.Assert(BSightN != null, "BSightN != null");
                Debug.Assert(BSightE != null, "BSightE != null");
                Debug.Assert(BSightU != null, "BSightU != null");
                return String.Format("{0,14}{1,14}{2,14}{3,18}",
                        BSightN.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        BSightE.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        BSightU.Value.ToString("0.0000", CultureInfo.InvariantCulture), " ");
            }

            [Pure]
            public string ZeroDirAzi()
            {
                Debug.Assert(ZerodirAzi != null, "ZerodirAzi != null");
                return String.Format("{0,14}{1,46}", ZerodirAzi.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        " ");
            }

            public string ZeroDirXYZ()
            {
                Debug.Assert(ZeroDirN != null, "ZeroDirN != null");
                Debug.Assert(ZeroDirE != null, "ZeroDirE != null");
                Debug.Assert(ZeroDirU != null, "ZeroDirU != null");
                return String.Format("{0,14}{1,14}{2,14}{3,18}",
                        ZeroDirN.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        ZeroDirE.Value.ToString("0.0000", CultureInfo.InvariantCulture),
                        ZeroDirU.Value.ToString("0.0000", CultureInfo.InvariantCulture), " ");
            }

            #endregion
        }


        public class AntennaPhaseCenter
        {
            #region Properties

            public ObservationCode ObservationCode { get; set; }
            public SatelliteSystem SatelliteSystem { get; set; }
            public double DeltaU { get; set; }
            public double DeltaE { get; set; }
            public double DeltaN { get; set; }

            #endregion

            #region Methods

            public override string ToString()
            {
                var s = SatelliteSystem.ToString().ToUpper()[0];
                var obsCode = ObservationCode.ToString().ToUpper();
                return String.Format("{0,-2}{1}{2,9}{3,14}{4,14}{5,18}", s, obsCode,
                        DeltaN.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaE.ToString("0.0000", CultureInfo.InvariantCulture),
                        DeltaU.ToString("0.0000", CultureInfo.InvariantCulture), " ");
            }

            #endregion
        }


        public class ScaleFactorData
        {
            #region Fields

            public readonly Collection<ObservationCode> ObservationCodes = new Collection<ObservationCode>();

            #endregion

            #region Properties

            public int ScaleFactor { get; set; }

            #endregion
        }


        public class PhaseShiftData
        {
            #region Fields

            public readonly Collection<int> Satellites = new Collection<int>();

            #endregion

            #region Properties

            public double CorrectionCycles { get; set; }
            public ObservationCode ObservationCode { get; set; }

            public bool AllSatellites
            {
                get { return Satellites.Count == 0; }
            }

            #endregion
        }


        public class GnssObservations<T> : GnssObservation
        {
            #region Constructor

            public GnssObservations()
            {
                var temp = Enum.GetValues(typeof(T));
                foreach (var enumItem in temp)
                {
                    SatObservations.Add((int)enumItem, new Dictionary<ObservationCode, int>());
                }
            }

            #endregion

            #region Methods

            internal IEnumerable<StringBuilder> RnxPrintGnssObs()
            {
                if (Observations.Count == 0)
                {
                    yield break;
                }
                var sb = new StringBuilder();
                //we need to get satellite system code, the convetion is that satellites 
                //is named by SatSysCode+PRN, so to get system code we just can read symbol of first
                // satellite in current constellation
                var temp = Enum.GetName(typeof(T), 1);
                Debug.Assert(!String.IsNullOrEmpty(temp) && temp.Length >= 1, "!String.IsNullOrEmpty(temp)");
                var systemCode = temp[0];
                sb.AppendFormat("{0}  {1,3}", systemCode, Observations.Count);
                var obsNumber = 0;
                foreach (var obs in Observations)
                {
                    obsNumber++;
                    if (obsNumber%13 == 0) //%13 is according to spec, no more then 13 obs on a line
                    {
                        sb.AppendFormat(" {0}", obs);
                        sb.Append(' ', 60 - sb.Length); //to get proper length
                        var str = sb.ToString();
                        yield return new StringBuilder(str);
                        sb = new StringBuilder();
                        sb.AppendFormat("{0,6}", " ");
                    }
                    else
                    {
                        sb.AppendFormat(" {0}", obs);
                    }
                }
                if (obsNumber%13 != 0)
                {
                    //align according to rnx format before return
                    sb.Append(' ', 60 - sb.Length);
                    yield return sb;
                }
            }

            internal IEnumerable<StringBuilder> RnxPrintGnssObsNumber()
            {
                foreach (int enumVal in Enum.GetValues(typeof(T)))
                {
                    foreach (var sb in RnxPrintGnssObsNumber(enumVal))
                    {
                        yield return sb;
                    }
                }
            }


            private IEnumerable<StringBuilder> RnxPrintGnssObsNumber(int satNum)
            {
                var obsData = SatObservations[satNum];
                if (obsData.Count == 0)
                {
                    yield break;
                }
                var sb = new StringBuilder();
                var name = Enum.GetName(typeof(T), satNum);
                sb.AppendFormat("{0,6}", name);
                var obsNumber = 0;
                foreach (var numOfObs in SatObservations[satNum].Select(t => t.Value))
                {
                    obsNumber++;
                    if (obsNumber%9 == 0) //%9 is according to spec, no more then 9 obs numbers on a line
                    {
                        sb.AppendFormat("{0,6}", numOfObs);
                        var str = sb.ToString();
                        yield return new StringBuilder(str);
                        sb = new StringBuilder();
                        sb.AppendFormat("{0,6}", " ");
                    }
                    else
                    {
                        sb.AppendFormat("{0,6}", numOfObs);
                    }
                }
                if (obsNumber%9 != 0)
                {
                    //align according to rnx format before return
                    sb.Append(' ', 60 - sb.Length);
                    yield return sb;
                }
            }

            #endregion
        }

        #endregion
    }


    public class GnssObservation
    {
        #region Fields

        protected readonly Collection<ObservationCode> _observations = new Collection<ObservationCode>();

        protected readonly Dictionary<int, Dictionary<ObservationCode, int>> SatObservations =
                new Dictionary<int, Dictionary<ObservationCode, int>>();

        #endregion

        #region Properties

        private int CurrentPrn { get; set; }
        internal int ObsIndex { get; private set; }

        internal ReadOnlyCollection<ObservationCode> Observations
        {
            get { return new ReadOnlyCollection<ObservationCode>(_observations); }
        }

        #endregion

        #region Methods

        internal void AddObservation(ObservationCode code)
        {
            _observations.Add(code);
        }

        internal void AddNumberOfObservations(int obsIndex, int obsNum)
        {
            //get observation code by index
            var obsCode = _observations[obsIndex];
            var obsData = SatObservations[CurrentPrn];
            obsData[obsCode] = obsNum;
        }

        internal void AddNumberOfObservations(int obsIndex, int prn, int numOfObs)
        {
            CurrentPrn = prn;
            ObsIndex = obsIndex;
            AddNumberOfObservations(obsIndex, numOfObs);
        }

        internal IEnumerable<int> GetSatObservations(int satNum)
        {
            var obsData = SatObservations[satNum];
            if (obsData.Count == 0)
            {
                yield break;
            }
            foreach (var od in obsData)
            {
                yield return od.Value;
            }
        }

        #endregion
    }
}