// Rinex3Parser
// ObservationRecord.cs-2016-08-04

#region

using System;
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
    public class ObservationDataRecord : ObservationRecord
    {
        #region Fields

        private readonly ILookup<SatelliteSystem, GnssObservation> _obsMetaData;
        private readonly List<SatelliteObs> _satObsrvations = new List<SatelliteObs>();

        #endregion

        #region Constructor

        internal ObservationDataRecord(Dictionary<SatelliteSystem, GnssObservation> obsMetaData)
        {
            _obsMetaData = obsMetaData.ToLookup(t => t.Key, t => t.Value);
        }

        #endregion

        #region Properties

        public override bool HasData
        {
            get { return _satObsrvations.Count > 0; }
        }

        #endregion

        #region Methods

        [Pure]
        protected virtual SatelliteObs<T> ParseImpl<T>([NotNull] ReadOnlyCollection<ObservationCode> obsCode,
            SatelliteSystem satelliteSystem, [NotNull] string prn)
        {
            if (prn[1] == ' ') prn = prn.Replace(' ', '0');
            var gpsPrn = (T)Enum.Parse(typeof(T), prn, true);
            return new SatelliteObs<T>(obsCode, gpsPrn);
        }

        public override void Parse(string line)
        {
            var prn = line.Substring(0, 3);
            var satelliteSystem = prn[0].ConvertToEnum();
            SatelliteObs satelliteObs;
            var temp = _obsMetaData[satelliteSystem].Single();
            ReadOnlyCollection<ObservationCode> obsCode = temp.Observations;
            switch(satelliteSystem)
            {
                case SatelliteSystem.Gps:
                    satelliteObs = ParseImpl<GpsSatellite>(obsCode, SatelliteSystem.Gps, prn);
                    break;
                case SatelliteSystem.Glo:
                    satelliteObs = ParseImpl<GloSatellite>(obsCode, SatelliteSystem.Glo, prn);
                    break;
                case SatelliteSystem.Gal:
                    satelliteObs = ParseImpl<GalSatellite>(obsCode, SatelliteSystem.Gal, prn);
                    break;
                case SatelliteSystem.Qzss:
                    satelliteObs = ParseImpl<QzssSatellite>(obsCode, SatelliteSystem.Qzss, prn);
                    break;
                case SatelliteSystem.Bds:
                    satelliteObs = ParseImpl<BdsSatellite>(obsCode, SatelliteSystem.Bds, prn);
                    break;
                case SatelliteSystem.Irnss:
                    satelliteObs = ParseImpl<IrnssSatellite>(obsCode, SatelliteSystem.Irnss, prn);
                    break;
                case SatelliteSystem.Sbas:
                    satelliteObs = ParseImpl<SbasSatellite>(obsCode, SatelliteSystem.Sbas, prn);
                    break;
                case SatelliteSystem.Mixed:
                    //ignore sbas and mixed    
                    return;
                default:
                    throw new ArgumentOutOfRangeException(@"satelliteSystem");
            }
            if (satelliteObs != null)
            {
                satelliteObs.Parse(line);
                _satObsrvations.Add(satelliteObs);
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var satObs in _satObsrvations)
            {
                //16 is observation record format m(F14.3, I1, I1), 14+1+1
                var sb = new StringBuilder(16*_satObsrvations.Count);
                var t = satObs.GetType();
                var prop = t.GetProperty("Prn");
                var prnName = prop.GetValue(satObs, new object[] {}).ToString();
                sb.Append(prnName);
                foreach (var obs in satObs.Observations.Values)
                {
                    if (obs == null) sb.AppendFormat("{0,16}", " ");
                    else sb.AppendFormat("{0,16}", obs);
                }
                stringBuilder.AppendLine(sb.ToString());
            }
            return stringBuilder.ToString();
        }

        internal IEnumerable<SatelliteObs> GetSatellitesObs(SatelliteSystem satelliteSystem)
        {
            foreach (var satObservation in _satObsrvations)
            {
                if (satObservation is SatelliteObs<GpsSatellite> && SatelliteSystem.Gps.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<GpsSatellite>;
                    continue;
                }

                if (satObservation is SatelliteObs<GloSatellite> && SatelliteSystem.Glo.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<GloSatellite>;
                    continue;
                }

                if (satObservation is SatelliteObs<BdsSatellite> && SatelliteSystem.Bds.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<BdsSatellite>;
                    continue;
                }

                if (satObservation is SatelliteObs<GalSatellite> && SatelliteSystem.Gal.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<GalSatellite>;
                    continue;
                }

                if (satObservation is SatelliteObs<QzssSatellite> && SatelliteSystem.Qzss.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<QzssSatellite>;
                    continue;
                }

                if (satObservation is SatelliteObs<IrnssSatellite> && SatelliteSystem.Irnss.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<IrnssSatellite>;
                }

                if (satObservation is SatelliteObs<SbasSatellite> && SatelliteSystem.Sbas.IsSet(satelliteSystem))
                {
                    yield return satObservation as SatelliteObs<SbasSatellite>;
                }
            }
        }

        internal IEnumerable<SatelliteObs<T>> GetSatellitesObs<T>()
        {
            return _satObsrvations.OfType<SatelliteObs<T>>().Select(satObservation => satObservation);
        }

        internal IEnumerable<T> GetSatellitesPrn<T>()
        {
            return _satObsrvations.OfType<SatelliteObs<T>>().Select(satObservation => satObservation.Prn);
        }

        internal IEnumerable<int> GetSatellitesPrn2<T>() where T : IConvertible
        {
            return
                    _satObsrvations.OfType<SatelliteObs<T>>()
                            .Select(satObservation => satObservation.Prn.ToInt32(CultureInfo.InvariantCulture));
        }

        #endregion
    }


    public class ObservationDataRecordWithFilter : ObservationDataRecord
    {
        #region Fields

        private readonly ObservationFilter _filter;

        #endregion

        #region Constructor

        internal ObservationDataRecordWithFilter(Dictionary<SatelliteSystem, GnssObservation> obsMetaData,
            ObservationFilter filter)
            : base(obsMetaData)
        {
            _filter = filter;
        }

        #endregion

        #region Methods

        protected override SatelliteObs<T> ParseImpl<T>(ReadOnlyCollection<ObservationCode> obsCode,
            SatelliteSystem satelliteSystem, string prn)
        {
            var intPrn = (int)Enum.Parse(typeof(T), prn, true);
            var satPrn = (T)Enum.Parse(typeof(T), prn, true);
            return _filter.IsGnssDataFit(satelliteSystem, intPrn) ? new SatelliteObs<T>(obsCode, satPrn) : null;
        }

        #endregion
    }


    public class CycleSlipDataRecord : ObservationRecord
    {
        #region Fields

        private readonly ILookup<SatelliteSystem, GnssObservation> _obsMetaData;
        private readonly List<SatelliteCycleSlip> _satCycleSlips = new List<SatelliteCycleSlip>();

        #endregion

        #region Constructor

        internal CycleSlipDataRecord(Dictionary<SatelliteSystem, GnssObservation> obsMetaData)
        {
            _obsMetaData = obsMetaData.ToLookup(t => t.Key, t => t.Value);
        }

        #endregion

        #region Properties

        public override bool HasData
        {
            get { return _satCycleSlips.Count > 0; }
        }

        #endregion

        #region Methods

        public override void Parse([NotNull] string line)
        {
            if (string.IsNullOrEmpty(line)) throw new ArgumentException("Value cannot be null or empty.", "line");
            var prn = line.Substring(0, 3);
            var satelliteSystem = prn[0].ConvertToEnum();
            SatelliteCycleSlip satelliteCycleSlip;
            var temp = _obsMetaData[satelliteSystem].Single();
            var obsCode = temp.Observations;
            switch(satelliteSystem)
            {
                case SatelliteSystem.Gps:
                    var gpsPrn = (GpsSatellite)Enum.Parse(typeof(GpsSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<GpsSatellite>(obsCode, gpsPrn);
                    break;
                case SatelliteSystem.Glo:
                    var gloPrn = (GloSatellite)Enum.Parse(typeof(GloSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<GloSatellite>(obsCode, gloPrn);
                    break;
                case SatelliteSystem.Gal:
                    var galPrn = (GalSatellite)Enum.Parse(typeof(GalSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<GalSatellite>(obsCode, galPrn);
                    break;
                case SatelliteSystem.Qzss:
                    var qzssPrn = (QzssSatellite)Enum.Parse(typeof(QzssSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<QzssSatellite>(obsCode, qzssPrn);
                    break;
                case SatelliteSystem.Bds:
                    var bdsPrn = (BdsSatellite)Enum.Parse(typeof(BdsSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<BdsSatellite>(obsCode, bdsPrn);
                    break;
                case SatelliteSystem.Irnss:
                    var inPrn = (IrnssSatellite)Enum.Parse(typeof(IrnssSatellite), prn, true);
                    satelliteCycleSlip = new SatelliteCycleSlip<IrnssSatellite>(obsCode, inPrn);
                    break;
                case SatelliteSystem.Sbas:
                case SatelliteSystem.Mixed:
                    //ignore sbas and mixed    
                    return;
                default:
                    throw new ArgumentOutOfRangeException(@"satelliteSystem");
            }
            satelliteCycleSlip.Parse(line);
            _satCycleSlips.Add(satelliteCycleSlip);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var satObs in _satCycleSlips)
            {
                //16 is observation record format m(F14.3, I1, I1), 14+1+1
                var sb = new StringBuilder(16*_satCycleSlips.Count);
                var t = satObs.GetType();
                var prop = t.GetProperty("Prn");
                var prnName = (string)prop.GetValue(satObs, new object[] {});
                sb.Append(prnName);
                foreach (var cycleSlip in satObs.CycleSlips.Values)
                {
                    sb.AppendFormat("{0,14}  ", cycleSlip);
                }
                stringBuilder.AppendLine(sb.ToString());
            }
            return stringBuilder.ToString();
        }

        #endregion
    }


    public class ExternalEventRecord : HeaderRecord
    {
    }


    public class NewSiteRecord : ObservationRecord
    {
        #region Fields

        private readonly RinexObsHeader _rinexObsHeader = new RinexObsHeader();

        #endregion

        #region Properties

        public RinexObsHeaderData NewSiteHeader
        {
            get { return _rinexObsHeader.ObsHeaderData; }
        }

        public override bool HasData
        {
            get { return true; }
        }

        #endregion

        #region Methods

        public override void Parse([NotNull] string record)
        {
            _rinexObsHeader.ParseHeaderLine(record);
        }

        public override string ToString()
        {
            return _rinexObsHeader.ObsHeaderData.ToString();
        }

        #endregion
    }


    public class HeaderRecord : ObservationRecord
    {
        #region Fields

        private readonly List<string> _epochRecords = new List<string>();

        #endregion

        #region Properties

        public List<string> EpochRecords
        {
            get { return _epochRecords; }
        }

        public override bool HasData
        {
            get { return _epochRecords.Count > 0; }
        }

        #endregion

        #region Methods

        public override void Parse([NotNull] string record)
        {
            _epochRecords.Add(record);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var epochRecord in _epochRecords)
            {
                sb.AppendLine(epochRecord);
            }
            return sb.ToString();
        }

        #endregion
    }


    public abstract class ObservationRecord
    {
        #region Properties

        public abstract bool HasData { get; }

        #endregion

        #region Methods

        public abstract void Parse(string line);

        #endregion
    }


    public class SatelliteObs<T> : SatelliteObs
    {
        #region Fields

        private readonly T _prn;

        #endregion

        #region Constructor

        internal SatelliteObs(ReadOnlyCollection<ObservationCode> observationCodes, T prn) : base(observationCodes)
        {
            ObservationCodes = observationCodes;
            _prn = prn;
        }

        #endregion

        #region Properties

        public T Prn
        {
            get { return _prn; }
        }

        #endregion
    }


    public class SatelliteObs
    {
        #region Fields

        private readonly Dictionary<ObservationCode, Observation> _observations =
                new Dictionary<ObservationCode, Observation>();

        protected ReadOnlyCollection<ObservationCode> ObservationCodes;

        #endregion

        #region Constructor

        public SatelliteObs(ReadOnlyCollection<ObservationCode> observationCodes)
        {
            ObservationCodes = observationCodes;
        }

        #endregion

        #region Properties

        public Dictionary<ObservationCode, Observation> Observations
        {
            get { return _observations; }
        }

        #endregion

        #region Methods

        internal void Parse([NotNull] string line)
        {
            var match = RinexRegex.ObsRecordRegex.Match(line);
            if (match.Groups["data"].Success)
            {
                var obsIndex = 0;
                var caps = match.Groups["data"].Captures;
                foreach (Capture capture in caps)
                {
                    //obs value -- [0:13], length = 14 (see spec.)
                    //lli -- [14], length = 1
                    //sig. strength -- [15], lenght = 1 
                    var value = capture.Value;
                    var obsValue = value.Substring(0, 14);
                    if (!obsValue.IsEmptyOrWhiteSpace())
                    {
                        var o = Double.Parse(obsValue, CultureInfo.InvariantCulture);

                        var lli = value.Length > 14 && Char.IsDigit(value, 14)
                                ? Int32.Parse(value.Substring(14, 1), CultureInfo.InvariantCulture)
                                : (int?)null;

                        var signalStrength = value.Length > 15 && Char.IsDigit(value, 15)
                                ? Int32.Parse(value.Substring(15, 1), CultureInfo.InvariantCulture)
                                : (int?)null;
                        var obs = new Observation(o, lli, signalStrength);
                        _observations.Add(ObservationCodes[obsIndex], obs);
                    }
                    else
                    {
                        _observations.Remove(ObservationCodes[obsIndex]);
                    }
                    obsIndex++;
                }
            }
        }

        #endregion
    }


    public class SatelliteCycleSlip<T> : SatelliteCycleSlip
    {
        #region Fields

        private readonly T _prn;

        #endregion

        #region Constructor

        public SatelliteCycleSlip(ReadOnlyCollection<ObservationCode> observationCodes, T prn) : base(observationCodes)
        {
            _prn = prn;
        }

        #endregion

        #region Properties

        public T Prn
        {
            get { return _prn; }
        }

        #endregion
    }


    public class SatelliteCycleSlip
    {
        #region Fields

        private readonly Dictionary<ObservationCode, int?> _cycleSlips =
                new Dictionary<ObservationCode, int?>();

        protected readonly ReadOnlyCollection<ObservationCode> ObservationCodes;

        #endregion

        #region Constructor

        public SatelliteCycleSlip(ReadOnlyCollection<ObservationCode> observationCodes)
        {
            ObservationCodes = observationCodes;
        }

        #endregion

        #region Properties

        public Dictionary<ObservationCode, int?> CycleSlips
        {
            get { return _cycleSlips; }
        }

        #endregion

        #region Methods

        internal void Parse(string line)
        {
            var match = RinexRegex.ObsRecordRegex.Match(line);
            var obsIndex = 0;
            foreach (Capture capture in match.Captures)
            {
                var value = capture.Value;
                if (!String.IsNullOrEmpty(value))
                {
                    match = RinexRegex.ObsDataRegex.Match(value);
                    var cycleSlipData = match.Groups["obsdata"].Value;
                    var o = String.IsNullOrEmpty(cycleSlipData)
                            ? (int?)null
                            : (int)Double.Parse(cycleSlipData, CultureInfo.InvariantCulture);

                    _cycleSlips.Add(ObservationCodes[obsIndex], o);
                }
                obsIndex++;
            }
        }

        #endregion
    }


    public class Observation
    {
        #region Fields

        private readonly double? _observation;

        #endregion

        #region Constructor

        public Observation(double? observation, int? lli, int? signalStrength)
        {
            Lli = lli;
            SignalStrength = signalStrength;
            _observation = observation;
        }

        #endregion

        #region Properties

        public bool LliBit0
        {
            get
            {
                if (Lli.HasValue) return (Lli.Value&1) != 0;
                return false;
            }
        }

        public bool LliBit1
        {
            get
            {
                if (Lli.HasValue) return (Lli.Value&2) != 0;
                return false;
            }
        }

        public bool LliBit2
        {
            get
            {
                if (Lli.HasValue) return (Lli.Value&4) != 0;
                return false;
            }
        }

        public int? Lli { get; private set; }

        public int? SignalStrength { get; private set; }

        public double? ObsValue
        {
            get { return _observation; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            Debug.Assert(_observation != null, "_observation != null");
            return String.Format("{0}{1}{2}", _observation.Value.ToString("0.000", CultureInfo.InvariantCulture),
                    Lli.HasValue ? Lli.Value.ToString() : " ",
                    SignalStrength.HasValue ? SignalStrength.Value.ToString() : " ");
        }

        #endregion
    }
}