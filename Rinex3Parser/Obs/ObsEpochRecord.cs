// Rinex3Parser
// ObsEpochRecord.cs-2016-09-16

#region

using System;
using System.Globalization;


#endregion

namespace Rinex3Parser.Obs
{
    public enum EpochFlag
    {
        Ok = 1, //	OK
        PowerFailure = 2, //	power failure between previous and current epoch
        Kinematic = 3, //	start moving antenna
        NewSite = 4, //	new site occupation (end of kinem. data) 
        Header = 5, //	header information follows
        Event = 6, //	external event
        CycleSlip = 7, //cycle slip records follow
        None = 0
    }


    public struct ObsEpochRecord : IComparable<ObsEpochRecord>
    {
        #region Fields

        private readonly DateTime _approximateDateTime;

        private readonly EpochFlag _epochFlag;
        private readonly int _epochRecords;
        private readonly double _preciseSec;
        private readonly double? _rcvClkOffset;

        #endregion

        #region Constructor

        public ObsEpochRecord(int year, int month, int day, int hour, int min, double preciseSec, int epochRecords,
            double? rcvClkOffset, EpochFlag epochFlag)
        {
            _preciseSec = preciseSec;
            _epochRecords = epochRecords;
            _rcvClkOffset = rcvClkOffset;
            _epochFlag = epochFlag;
            _approximateDateTime = new DateTime(year, month, day, hour, min, (int)Math.Truncate(preciseSec));
        }

        #endregion

        #region Properties

        public int Year
        {
            get { return _approximateDateTime.Year; }
        }

        public int Month
        {
            get { return _approximateDateTime.Month; }
        }

        public int Day
        {
            get { return _approximateDateTime.Day; }
        }

        public int Hour
        {
            get { return _approximateDateTime.Hour; }
        }

        public int Min
        {
            get { return _approximateDateTime.Minute; }
        }

        public double PreciseSec
        {
            get { return _preciseSec; }
        }

        public int EpochRecords
        {
            get { return _epochRecords; }
        }

        public double? RcvClkOffset
        {
            get { return _rcvClkOffset; }
        }

        public EpochFlag EpochFlag
        {
            get { return _epochFlag; }
        }

        public DateTime ApproximateDateTime
        {
            get { return _approximateDateTime; }
        }

        #endregion

        #region Methods

        internal bool CheckInterval(DateTime start, DateTime end)
        {
            return _approximateDateTime >= start && _approximateDateTime <= end;
        }


        //> 2016 01 31 23 50 30.0000000  0 30
        public override string ToString()
        {
            return _rcvClkOffset.HasValue
                ? String.Format("> {0:0000} {1:00} {2:00} {3:00} {4:00}{5,11}  {6}{7,3}      {8,15}", ApproximateDateTime.Year, ApproximateDateTime.Month, ApproximateDateTime.Day, ApproximateDateTime.Hour, ApproximateDateTime.Minute,
                    _preciseSec.ToString("0.0000000", CultureInfo.InvariantCulture), (int)_epochFlag - 1,
                    _epochRecords,
                    _rcvClkOffset.Value.ToString("0.000000000000", CultureInfo.InvariantCulture))
                : String.Format("> {0:0000} {1:00} {2:00} {3:00} {4:00}{5,11}  {6}{7,3}", ApproximateDateTime.Year, ApproximateDateTime.Month, ApproximateDateTime.Day, ApproximateDateTime.Hour, ApproximateDateTime.Minute,
                    _preciseSec.ToString("0.0000000", CultureInfo.InvariantCulture), (int)_epochFlag - 1,
                    _epochRecords);
        }

        public bool Equals(ObsEpochRecord other)
        {
            return _approximateDateTime.Equals(other._approximateDateTime) && _epochFlag == other._epochFlag && _preciseSec.Equals(other._preciseSec);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ObsEpochRecord && Equals((ObsEpochRecord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _approximateDateTime.GetHashCode();
                hashCode = (hashCode*397)^(int)_epochFlag;
                hashCode = (hashCode*397)^_preciseSec.GetHashCode();
                return hashCode;
            }
        }

        #endregion

        public int CompareTo(ObsEpochRecord other)
        {
            return ApproximateDateTime.Equals(other.ApproximateDateTime)
                ? PreciseSec.CompareTo(other.PreciseSec)
                : ApproximateDateTime.CompareTo(other.ApproximateDateTime);
        }
    }
}