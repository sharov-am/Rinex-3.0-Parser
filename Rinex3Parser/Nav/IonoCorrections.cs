// Rinex3Parser
// IonoCorrections.cs-2016-08-09

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public enum IonoCorrectionsEnum
    {
        Alpha0 = 1,
        Alpha1,
        Alpha2,
        Alpha3,
        Beta0,
        Beta1,
        Beta2,
        Beta3,
        Ai0,
        Ai1,
        Ai2,
        Reserve
        
    }


    public class IonoCorrections
    {
        #region Fields

        private static readonly Dictionary<SatelliteSystem, string> IonoName = new Dictionary<SatelliteSystem, string>
        {
            {
                SatelliteSystem.Gal, "GAL"
            },
            {
                SatelliteSystem.Gps, "GPS"
            },
            {
                SatelliteSystem.Bds, "BDS"
            },
            {
                SatelliteSystem.Qzss, "QZS"
            },
            {
                SatelliteSystem.Irnss, "IRN"
            }
        };

        private readonly Dictionary<IonoCorrectionsEnum, double> _ionoCorrections =
                new Dictionary<IonoCorrectionsEnum, double>();

        private readonly int? _satId;
        private readonly char? _timeMark;
        private readonly SatelliteSystem _satelliteSystem;

        public IonoCorrections(SatelliteSystem satelliteSystem, IEnumerable<double> corrections, IonoCorrectionsEnum startIndex,
                char? timeMark, int? satId)
        {
            _satelliteSystem = satelliteSystem;
            _timeMark = timeMark;
            _satId = satId;
            var index = 0;
            foreach (var correction in corrections)
            {
                Debug.Assert(Enum.IsDefined(typeof(IonoCorrectionsEnum), startIndex + index),
                        "Enum.IsDefined(typeof(IonoCorrectionsEnum),startIndex + index)");
                _ionoCorrections.Add(startIndex + index, correction);
                index++;
            }
        }

        #endregion

        #region Properties

        public Dictionary<IonoCorrectionsEnum, double> Corrections
        {
            get { return _ionoCorrections; }
        }

        
        public char TimeMark
        {
            get { return _timeMark ?? ' '; }
        }

        internal SatelliteSystem SatelliteSystem
        {
            get { return _satelliteSystem; }
        }

        public int? SatId
        {
            get { return _satId; }
        }

        #endregion

        #region Methods


        public bool HasCorrection(IonoCorrectionsEnum corType)
        {
            return _ionoCorrections.ContainsKey(corType);
        }

        public IEnumerable<string> StringRepresentation()
        {
            var sb = new StringBuilder(60);
            var start = (int)IonoCorrectionsEnum.Alpha0;
            var end = (int)IonoCorrectionsEnum.Alpha3;
            if (_ionoCorrections.ContainsKey(IonoCorrectionsEnum.Alpha0))
            {
                if (_satelliteSystem == SatelliteSystem.Gal)
                    sb.AppendFormat("{0,-4} ", IonoName[SatelliteSystem]);
                else
                {
                    sb.AppendFormat("{0,4} ", IonoName[SatelliteSystem] + "A");
                }
                for (int i = start; i <= end; i++)
                {
                    var ic = (IonoCorrectionsEnum)i;
                    sb.AppendFormat("{0,12}",
                            _ionoCorrections.ContainsKey(ic)
                                    ? _ionoCorrections[ic].ToString("0.0000E+00", CultureInfo.InvariantCulture)
                                    : " ");
                }
                if (_satelliteSystem == SatelliteSystem.Bds)
                {
                    sb.AppendFormat(" {0} {1,2}", _timeMark?? ' ',_satId.HasValue? _satId.Value.ToString("00") : " ");
                }
                sb.Append(' ', 60 - sb.Length);
                yield return sb.ToString();
            }
            sb = new StringBuilder(60);
            if (_ionoCorrections.ContainsKey(IonoCorrectionsEnum.Beta0))
            {
                if (_satelliteSystem == SatelliteSystem.Gal)
                    sb.AppendFormat("{0,4} ", IonoName[SatelliteSystem]);
                else
                {
                    sb.AppendFormat("{0,4} ", IonoName[SatelliteSystem] + "B");
                }

                start = (int)IonoCorrectionsEnum.Beta0;
                end = (int)IonoCorrectionsEnum.Beta3;
                for (int i = start; i <= end; i++)
                {
                    var ic = (IonoCorrectionsEnum)i;
                    sb.AppendFormat("{0,12}",
                            _ionoCorrections.ContainsKey(ic)
                                    ? _ionoCorrections[ic].ToString("0.0000E+00", CultureInfo.InvariantCulture)
                                    : " ");
                }

                if (_satelliteSystem == SatelliteSystem.Bds)
                {
                    sb.AppendFormat(" {0} {1,2}", _timeMark ?? ' ',
                            _satId.HasValue ? _satId.Value.ToString("00") : " ");
                }

                sb.Append(' ', 60 - sb.Length);
                yield return sb.ToString();
            }
        }

        #endregion
    }
}