// Rinex3Parser
// MixedNavParser.cs-2016-08-16

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public class MixedNavParser : NavMessageContentParserBase<NavMessage>
    {
        #region Fields

        private readonly Dictionary<SatelliteSystem, NavMessageContentParserBase> _parserMap =
                new Dictionary<SatelliteSystem, NavMessageContentParserBase>();

        #endregion

        #region Constructor

        public MixedNavParser(StreamReader sr) : base(sr, 0)
        {
            _parserMap.Add(SatelliteSystem.Gps, new GpsNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Glo, new GloNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Bds, new BdsNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Gal, new GalNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Qzss, new QzssNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Irnss, new IrnssNavMessageContentParser(sr));
            _parserMap.Add(SatelliteSystem.Sbas, new SbasNavMessageContentParser(sr));
        }

        #endregion

        #region Methods

        protected override NavMessage ParseContents(string[] broadCastOrbits)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<NavMessage> Parse()
        {
            while(!Sr.EndOfStream)
            {
                var ch = Sr.Peek();
                char s = Convert.ToChar(ch);
                var satelliteSystem = s.ConvertToEnum();

                switch(satelliteSystem)
                {
                    case SatelliteSystem.Gps:
                        var gpsParser = _parserMap[satelliteSystem] as GpsNavMessageContentParser;
                        Debug.Assert(gpsParser != null, "temp != null");
                        yield return gpsParser.ParseImpl<GpsNavMessage>();
                        break;
                    case SatelliteSystem.Glo:
                        var gloParser = _parserMap[satelliteSystem] as GloNavMessageContentParser;
                        Debug.Assert(gloParser != null, "temp != null");
                        yield return gloParser.ParseImpl<GloNavMessage>();
                        break;
                    case SatelliteSystem.Gal:
                        var galParser = _parserMap[satelliteSystem] as GalNavMessageContentParser;
                        Debug.Assert(galParser != null, "temp != null");
                        yield return galParser.ParseImpl<GalNavMessage>();
                        break;
                    case SatelliteSystem.Qzss:
                        var qzssParser = _parserMap[satelliteSystem] as QzssNavMessageContentParser;
                        Debug.Assert(qzssParser != null, "temp != null");
                        yield return qzssParser.ParseImpl<QzssNavMessage>();
                        break;
                    case SatelliteSystem.Bds:
                        var bdsParser = _parserMap[satelliteSystem] as BdsNavMessageContentParser;
                        Debug.Assert(bdsParser != null, "temp != null");
                        yield return bdsParser.ParseImpl<BdsNavMessage>();
                        break;
                    case SatelliteSystem.Irnss:
                        var irnssParser = _parserMap[satelliteSystem] as IrnssNavMessageContentParser;
                        Debug.Assert(irnssParser != null, "temp != null");
                        yield return irnssParser.ParseImpl<IrnssNavMessage>();
                        break;
                    case SatelliteSystem.Sbas:
                        var sbasParser = _parserMap[satelliteSystem] as SbasNavMessageContentParser;
                        Debug.Assert(sbasParser != null, "temp != null");
                        yield return sbasParser.ParseImpl<SbasNavMessage>();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}