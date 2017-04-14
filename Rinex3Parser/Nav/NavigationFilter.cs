// Rinex3Parser
// NavigationFilter.cs-2016-08-18

#region

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public class NavigationFilter
    {
        #region Fields

        private readonly DateTime? _end;
        private readonly Dictionary<SatelliteSystem,int[]> _excludedSatPrn;
        private readonly DateTime? _start;

        #endregion

        #region Constructor

        public NavigationFilter(DateTime? start, DateTime? end) : this(start, end, new Dictionary<SatelliteSystem, int[]>())
        {
        }

        public NavigationFilter(DateTime? start, DateTime? end, [NotNull] Dictionary<SatelliteSystem, int[]> excludedSatPrn)
        {
            if (excludedSatPrn == null) throw new ArgumentNullException("excludedSatPrn");
            _start = start;
            _end = end;
            _excludedSatPrn = excludedSatPrn;
        }

        #endregion

        #region Properties

        public DateTime? End
        {
            get { return _end; }
        }

        public DateTime? Start
        {
            get { return _start; }
        }

        #endregion

        #region Methods

        internal bool ValidateNavMsg([NotNull] NavMessage navMsg)
        {
            var ok = navMsg.CheckInterval(_start ?? DateTime.MinValue, _end ?? DateTime.MaxValue);
            if (!ok) return false;
            if(_excludedSatPrn.ContainsKey(navMsg.SatelliteSystem))
            {
               return !_excludedSatPrn[navMsg.SatelliteSystem].Contains(navMsg.SatPrn);
            }
            return true;

        }

        #endregion
    }
}