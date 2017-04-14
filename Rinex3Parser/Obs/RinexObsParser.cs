// Rinex3Parser
// RinexObsParser.cs-2016-08-10

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Obs
{
    public class RinexObsParser : RinexParser
    {
        #region Fields

        private readonly SortedList<ObsEpochRecord, ObservationRecord> _obsRecords =
                new SortedList<ObsEpochRecord, ObservationRecord>();

        protected readonly RinexObsHeader RinexObsHeader = new RinexObsHeader();

        #endregion

        #region Constructor

        public RinexObsParser(string filePath, ParseType parseType) : base(filePath, parseType)
        {
        }

        #endregion

        #region Properties

        public SortedList<ObsEpochRecord, ObservationRecord> ObservationRecords
        {
            get { return _obsRecords; }
        }

        #endregion

        #region Methods

        public event EventHandler<RinexObsHeaderData> HeaderInfoEvent;
        public event EventHandler<RinexObsData> ObsDataEvents;


        public SortedList<ObsEpochRecord, ICollection<SatelliteObs<T>>> GetSatellitesObservations<T>()
        {
            var q = typeof(T);
            if (q != typeof(GpsSatellite) && q != typeof(GloSatellite) &&
                q != typeof(GalSatellite) && q != typeof(BdsSatellite) &&
                q != typeof(QzssSatellite) && q != typeof(IrnssSatellite))
            {
                throw new ArgumentException(String.Format("Not supported type of satellites -- {0}.", q));
            }

            var temp = new SortedList<ObsEpochRecord, ICollection<SatelliteObs<T>>>();
            foreach (var observationRecord in _obsRecords)
            {
                var data = observationRecord.Value as ObservationDataRecord;
                if (data != null)
                {
                    var epoch = observationRecord.Key;
                    var satObsCollection = new List<SatelliteObs<T>>();
                    var temp2 = data.GetSatellitesObs<T>();
                    var satelliteObsArr = temp2 as SatelliteObs<T>[] ?? temp2.ToArray();
                    if (satelliteObsArr.Any())
                    {
                        satObsCollection.AddRange(satelliteObsArr);
                        temp.Add(epoch, satObsCollection);
                    }
                }
            }
            return temp;
        }


        public SortedList<ObsEpochRecord, ICollection<SatelliteObs>> GetSatellitesObservations(
                SatelliteSystem satelliteSystem)
        {
            var temp = new SortedList<ObsEpochRecord, ICollection<SatelliteObs>>();
            foreach (var observationRecord in _obsRecords)
            {
                var data = observationRecord.Value as ObservationDataRecord;
                if (data != null)
                {
                    var epoch = observationRecord.Key;
                    var satObsCollection = new List<SatelliteObs>();
                    satObsCollection.AddRange(data.GetSatellitesObs(satelliteSystem));
                    temp.Add(epoch, satObsCollection);
                }
            }
            return temp;
        }


        public IEnumerable<SatelliteSystem> GetObservedGnss()
        {
            return ObsHeader.ObsHeaderData.ObsMetaData.Keys;
        }


        public Dictionary<SatelliteSystem, IEnumerable<int>> GetObservedSatellites()
        {
            var result = new Dictionary<SatelliteSystem, IEnumerable<int>>();

            IEnumerable<ObservationDataRecord> temp = _obsRecords.Values.OfType<ObservationDataRecord>().ToList();

            foreach (var gnss in GetObservedGnss())
            {
                IEnumerable<int> temp2;
                // ReSharper disable once SwitchStatementMissingSomeCases
                //don't need mixed type
                switch(gnss)
                {
                    case SatelliteSystem.Gps:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<GpsSatellite>());
                        break;
                    case SatelliteSystem.Glo:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<GloSatellite>());
                        break;
                    case SatelliteSystem.Gal:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<GalSatellite>());
                        break;
                    case SatelliteSystem.Qzss:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<QzssSatellite>());
                        break;
                    case SatelliteSystem.Bds:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<BdsSatellite>());
                        break;
                    case SatelliteSystem.Irnss:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<IrnssSatellite>());
                        break;
                    case SatelliteSystem.Sbas:
                        temp2 = temp.SelectMany(t => t.GetSatellitesPrn2<SbasSatellite>());
                        break;
                    default:
                        throw new ArgumentException("Invalid observerd gnss system.");
                }
                temp2 = temp2.Distinct();
                result.Add(gnss, temp2);
            }

            return result;
        }



        public override RinexType RinexType
        {
            get { return RinexType.Obs;}
        }

        public RinexObsHeader ObsHeader
        {
            get { return RinexObsHeader; }
        }

        protected override void ParseHeader(StreamReader sr)
        {
            var cont = true;
            while(cont)
            {
                var line = sr.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    cont = RinexObsHeader.ParseHeaderLine(line);
                }
            }
            ValidateHeaderMetadata();
            var temp = HeaderInfoEvent;
            if (temp != null)
            {
                temp(this, RinexObsHeader.ObsHeaderData);
            }
        }

        protected virtual void ValidateHeaderMetadata()
        {
            var obsHeaderData = RinexObsHeader.ObsHeaderData;
            var noVersion = !obsHeaderData.HeaderLabels.Contains(RinexHeaderLabel.VERSION);
            var noMarkerName = !obsHeaderData.HeaderLabels.Contains(RinexHeaderLabel.MARKER_NAME);
            var noSysObs = !obsHeaderData.HeaderLabels.Contains(RinexHeaderLabel.SYS_OBS_TYPES);
            if (noVersion || noMarkerName || noSysObs)
            {
                throw new FormatException("Invalid rinex header format. Missing some mandatory information.");
            }
        }
       


        protected override void ParseContents(StreamReader sr)
        {
            var obsMetaData = RinexObsHeader.ObsHeaderData.ObsMetaData;
            while(!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    var epochRecord = CreateEpochRecord(line);
                    var counter = epochRecord.EpochRecords;
                    var observationRecord = GetObservationRecordType(epochRecord, obsMetaData);
                    while(!sr.EndOfStream && counter != 0)
                    {
                        line = sr.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            Debug.Assert(observationRecord != null, "observationRecord != null");
                            observationRecord.Parse(line);
                        }
                        counter--;
                    }

                    PerformOnData(observationRecord, epochRecord);
                }
            }
        }

        protected virtual void PerformOnData(ObservationRecord observationRecord, ObsEpochRecord epochRecord)
        {
            EventHandler<RinexObsData> temp;

            switch(ParseType)
            {
                case ParseType.RaiseEvents:
                    temp = ObsDataEvents;
                    if (temp != null)
                    {
                        Debug.Assert(observationRecord != null, "observationRecord != null");
                        temp(this, new RinexObsData(epochRecord, observationRecord));
                    }
                    break;
                case ParseType.StoreData:
                    _obsRecords.Add(epochRecord, observationRecord);
                    break;
                case ParseType.StoreAndRaise:
                    _obsRecords.Add(epochRecord, observationRecord);
                    temp = ObsDataEvents;
                    if (temp != null)
                    {
                        Debug.Assert(observationRecord != null, "observationRecord != null");
                        temp(this, new RinexObsData(epochRecord, observationRecord));
                    }
                    break;
            }
        }

        protected virtual ObservationRecord GetObservationRecordType(ObsEpochRecord epochRecord,
                Dictionary<SatelliteSystem, GnssObservation> obsMetaData)
        {
            ObservationRecord observationRecord;
            switch(epochRecord.EpochFlag)
            {
                case EpochFlag.Ok:
                case EpochFlag.PowerFailure:
                    observationRecord = new ObservationDataRecord(obsMetaData);
                    break;
                case EpochFlag.NewSite:
                    observationRecord = new NewSiteRecord();
                    break;
                case EpochFlag.Kinematic:
                case EpochFlag.Header:
                    observationRecord = new HeaderRecord();
                    break;
                case EpochFlag.Event:
                    observationRecord = new ExternalEventRecord();
                    break;
                case EpochFlag.CycleSlip:
                    observationRecord = new CycleSlipDataRecord(obsMetaData);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("EpochFlag");
            }
            return observationRecord;
        }

        protected ObsEpochRecord CreateEpochRecord([NotNull] string line)
        {
            var match = RinexRegex.ObsEpochRecordRegex.Match(line);
            var epochRecord = match.Success ? CreateEpochRecordImpl(match) : CreateReserveEpochRecord(line);
            return epochRecord;
        }

        private ObsEpochRecord CreateReserveEpochRecord([NotNull] string line)
        {
            var match = RinexRegex.ObsEpochRecordReserveRegex.Match(line);
            var obsRecord = _obsRecords.Last().Key;
            EpochFlag flag;
            int epochRecords;
            if (match.Groups["flag"].Success)
            {
                flag = (EpochFlag)(Int32.Parse(match.Groups["flag"].Value, CultureInfo.InvariantCulture) + 1);
            }
            else
            {
                throw new InvalidOperationException(String.Format("Invalid rinex 3.03 format. Line:{0}", line));
            }
            if (match.Groups["satNum"].Success)
            {
                epochRecords = Int32.Parse(match.Groups["epochRecords"].Value, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException(String.Format("Invalid rinex 3.03 format. Line:{0}", line));
            }
            return new ObsEpochRecord(obsRecord.Year, obsRecord.Month, obsRecord.Day, obsRecord.Hour, obsRecord.Min,
                obsRecord.PreciseSec, epochRecords, null, flag);
        }

        private static ObsEpochRecord CreateEpochRecordImpl(Match match)
        {
            //>(?<year>)(?<month>)(?<day>)(?<hour>)(?<min>)(?<preciseSec>)(?<flag>)(?<satNum>)(?<rcvclkoffset>)$",

            double? rcvClkOffset = null;
            var year = Int32.Parse(match.Groups["year"].Value);
            var month = Int32.Parse(match.Groups["month"].Value);
            var day = Int32.Parse(match.Groups["day"].Value);
            var hour = Int32.Parse(match.Groups["hour"].Value);
            var min = Int32.Parse(match.Groups["min"].Value);
            var seconds = Double.Parse(match.Groups["preciseSec"].Value, CultureInfo.InvariantCulture);
            var epochRecords = Int32.Parse(match.Groups["epochRecords"].Value);
            var flag = (EpochFlag)(Int32.Parse(match.Groups["flag"].Value) + 1);
            if (match.Groups["rcvclkoffset"].Success)
            {
                rcvClkOffset = Double.Parse(match.Groups["rcvclkoffset"].Value, CultureInfo.InvariantCulture);
            }

            return new ObsEpochRecord(year, month, day, hour, min, seconds, epochRecords, rcvClkOffset, flag);
        }

        #endregion
    }


    public class RinexObsData : EventArgs
    {
        #region Fields

        private readonly ObsEpochRecord _epochRecord;
        private readonly ObservationRecord _observationRecord;

        #endregion

        #region Constructor

        public RinexObsData(ObsEpochRecord epochRecord, ObservationRecord observationRecord)
        {
            _epochRecord = epochRecord;
            _observationRecord = observationRecord;
        }

        #endregion

        #region Properties

        public ObsEpochRecord EpochRecord
        {
            get { return _epochRecord; }
        }

        public ObservationRecord Record
        {
            get { return _observationRecord; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(EpochRecord.ToString());
            sb.AppendLine(Record.ToString());
            return sb.ToString();
        }

        #endregion
    }


    public class RinexObsWithFilter : RinexObsParser
    {
        #region Fields

        private readonly ObservationFilter _filter;

        #endregion

        #region Constructor

        public RinexObsWithFilter(string filePath, ParseType parseType, ObservationFilter filter)
            : base(filePath, parseType)
        {
            _filter = filter;
        }

        #endregion

        #region Methods

        protected override ObservationRecord GetObservationRecordType(ObsEpochRecord epochRecord,
            Dictionary<SatelliteSystem, GnssObservation> obsMetaData)
        {
            ObservationRecord observationRecord;
            if (epochRecord.EpochFlag == EpochFlag.Ok || epochRecord.EpochFlag == EpochFlag.PowerFailure)
            {
                observationRecord = new ObservationDataRecordWithFilter(obsMetaData, _filter);
            }
            else
            {
                observationRecord = base.GetObservationRecordType(epochRecord, obsMetaData);
            }
            return observationRecord;
        }

        protected override void PerformOnData(ObservationRecord observationRecord, ObsEpochRecord epochRecord)
        {
            if (observationRecord.HasData) base.PerformOnData(observationRecord, epochRecord);
        }

        protected override void ParseContents(StreamReader sr)
        {
            var obsMetaData = ObsHeader.ObsHeaderData.ObsMetaData;
            while(!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if(!String.IsNullOrEmpty(line))
                {
                    var epochRecord = CreateEpochRecord(line);
                    var counter = epochRecord.EpochRecords;
                    if (_filter.IsEpochFit(epochRecord))
                    {
                        var observationRecord = GetObservationRecordType(epochRecord, obsMetaData);
                        while(!sr.EndOfStream && counter != 0)
                        {
                            line = sr.ReadLine();
                            if (!String.IsNullOrEmpty(line))
                            {
                                Debug.Assert(observationRecord != null, "observationRecord != null");
                                observationRecord.Parse(line);
                            }
                            counter--;
                        }

                        PerformOnData(observationRecord, epochRecord);
                    }
                    else
                    {
                        //NOTE it could be done more effeciantly via seek where position is calcultad based on obs metadata
                        while(!sr.EndOfStream && counter != 0)
                        {
                            sr.ReadLine();
                            counter--;
                        }
                    }
                }
            }
        }

        #endregion
    }
}