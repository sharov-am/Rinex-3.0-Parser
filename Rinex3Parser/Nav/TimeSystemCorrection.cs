// Rinex3Parser
// TimeSystemCorrection.cs-2016-08-09

using System;
using System.Globalization;
using System.Text;


namespace Rinex3Parser.Nav
{
    public enum CorrectionType
    {
        Gaut = 1,
        Gput,
        Sbut,
        Glut,
        Gpga,
        Glgp,
        Qzgp,
        Qzut,
        Bdut,
        Irut,
        Irgp
    }


    public struct TimeSystemCorrection
    {
        #region Fields

        private readonly double _a0;
        private readonly double _a1;
        private readonly CorrectionType _correctionType;
        private readonly int _referenceTime;
        private readonly int _referenceWeek;
        private readonly string _sbasType;
        private readonly int? _utcId;

        #endregion

        #region Constructor

        public TimeSystemCorrection(CorrectionType correctionType, double a0, double a1, int referenceTime,
                int referenceWeek, string sbasType, int? utcId)
        {
            _correctionType = correctionType;
            _a0 = a0;
            _a1 = a1;
            _referenceTime = referenceTime;
            _referenceWeek = referenceWeek;
            _sbasType = sbasType;
            _utcId = utcId;
        }

        #endregion

        #region Properties

        public CorrectionType Type
        {
            get { return _correctionType; }
        }

        public double A0
        {
            get { return _a0; }
        }

        public double A1
        {
            get { return _a1; }
        }

        public int ReferenceTime
        {
            get { return _referenceTime; }
        }

        public int ReferenceWeek
        {
            get { return _referenceWeek; }
        }

        public string SbasType
        {
            get { return _sbasType; }
        }

        public int? UtcId
        {
            get { return _utcId; }
        }

        
        public bool Equals(TimeSystemCorrection other)
        {
            return _a0.Equals(other._a0) && _a1.Equals(other._a1) && _correctionType == other._correctionType && _referenceTime == other._referenceTime && _referenceWeek == other._referenceWeek;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TimeSystemCorrection && Equals((TimeSystemCorrection)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _a0.GetHashCode();
                hashCode = (hashCode*397)^_a1.GetHashCode();
                hashCode = (hashCode*397)^(int)_correctionType;
                hashCode = (hashCode*397)^_referenceTime;
                hashCode = (hashCode*397)^_referenceWeek;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var temp = String.Format("{1}{0}{2,17}{3,16}{4,7}{5,5}{0}{6,5}{0}{7,2}{0}", " ",
                    _correctionType.ToString().ToUpper(),
                    A0.ToString("0.0000000000E+00", CultureInfo.InvariantCulture),
                    A1.ToString("0.000000000E+00", CultureInfo.InvariantCulture),
                    ReferenceTime, ReferenceWeek, SbasType, UtcId);
            var sb = new StringBuilder(temp);
            sb.Append(' ', 60 - sb.Length);
            return sb.ToString();
        }

        #endregion
    }
}