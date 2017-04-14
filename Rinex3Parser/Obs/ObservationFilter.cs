// Rinex3Parser
// ObservationFilter.cs-2016-08-04

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Obs
{
    public class ObservationFilter
    {
        private readonly DateTime? _start;
        private readonly DateTime? _end;
        private readonly SatelliteSystem? _excludedGnsssSystems;
        private readonly Dictionary<SatelliteSystem, int[]> _excludedSatellites;

        public ObservationFilter(DateTime? start, DateTime? end, SatelliteSystem? excludedGnsssSystems,
                Dictionary<SatelliteSystem, int[]> excludedSatellites)
        {
            _start = start;
            _end = end;
            _excludedGnsssSystems = excludedGnsssSystems;
            _excludedSatellites = excludedSatellites;
        }

        public bool IsEpochFit(ObsEpochRecord epochRecord)
        {
            return epochRecord.CheckInterval(_start ?? DateTime.MinValue, _end ?? DateTime.MaxValue);
        }

        public bool IsGnssDataFit(SatelliteSystem satelliteSystem, int prn)
        {
            //check whole gnss system
            if (_excludedGnsssSystems.HasValue)
            {
                if (_excludedGnsssSystems.Value.IsSet(satelliteSystem)) return false;
            }
            //check separate satellites
            if (_excludedSatellites != null)
            {
                if (!_excludedSatellites.ContainsKey(satelliteSystem)) return true;
                var prns = _excludedSatellites[satelliteSystem];
                return !prns.Contains(prn);
            }
            return true;
        }
    }
}