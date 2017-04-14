// Rinex3Parser
// RinexNavParser.cs-2016-08-18

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public class RinexNavParser : RinexParser
    {
        #region Fields

        private readonly RinexNavHeader _rinexNavHeader = new RinexNavHeader();

        protected readonly List<NavMessage> _navMessages = new List<NavMessage>();

        #endregion

        #region Constructor

        public RinexNavParser(string filePath, ParseType parseType) : base(filePath, parseType)
        {
        }

        #endregion

        #region Properties

        public override RinexType RinexType
        {
            get { return RinexType.Nav; }
        }

        public RinexNavHeader NavHeader
        {
            get { return _rinexNavHeader; }
        }

        public IEnumerable<NavMessage> NavMessages
        {
            get { return new ReadOnlyCollection<NavMessage>(_navMessages); }
        }

        #endregion

        #region Methods

        public event EventHandler<RinexNavHeaderData> NavHeaderInfoEvent;
        public event EventHandler<NavMessage> NewNavMessageEvent;

        protected void OnNewMessage([NotNull] NavMessage e)
        {
            var handler = NewNavMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        public IEnumerable<T> GetMessagesInTimeRange<T>(DateTime from, DateTime to) where T : NavMessage
        {
            return _navMessages.Where(t => t.Time >= from && t.Time <= to).OfType<T>();
        }

        public IEnumerable<T> ApplyMessageFilter<T>([NotNull] Func<T, bool> filter) where T : NavMessage
        {
            if (filter == null) throw new ArgumentNullException("filter");
            return _navMessages.OfType<T>().Where(filter);
        }

        public IEnumerable<NavMessage> GetMessagesInTimeRange(DateTime from, DateTime to)
        {
            return _navMessages.Where(t => t.Time >= from && t.Time <= to);
        }


        public IEnumerable<T> GetMessagesOfType<T>()
        {
            return _navMessages.OfType<T>();
        }

        
        protected override void ParseHeader(StreamReader sr)
        {
            var cont = true;
            while(cont)
            {
                var line = sr.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    cont = _rinexNavHeader.ParseHeaderLine(line);
                }
            }
            var temp = NavHeaderInfoEvent;
            if (temp != null)
            {
                temp(this, _rinexNavHeader.NavHeaderData);
            }
        }

        protected override void ParseContents(StreamReader sr)
        {
            switch(_rinexNavHeader.NavHeaderData.SatelliteSystem)
            {
                case SatelliteSystem.Gps:
                    var gpsNavParser = new GpsNavMessageContentParser(sr);
                    PerformOnData(gpsNavParser);
                    break;
                case SatelliteSystem.Glo:
                    var gloNavParser = new GloNavMessageContentParser(sr);
                    PerformOnData(gloNavParser);
                    break;
                case SatelliteSystem.Gal:
                    var galNavParser = new GalNavMessageContentParser(sr);
                    PerformOnData(galNavParser);
                    break;
                case SatelliteSystem.Qzss:
                    var qzssNavParser = new QzssNavMessageContentParser(sr);
                    PerformOnData(qzssNavParser);
                    break;
                case SatelliteSystem.Bds:
                    var bdsNavParser = new BdsNavMessageContentParser(sr);
                    PerformOnData(bdsNavParser);
                    break;
                case SatelliteSystem.Irnss:
                    break;
                case SatelliteSystem.Sbas:
                    var sbasNavParser = new SbasNavMessageContentParser(sr);
                    PerformOnData(sbasNavParser);
                    break;
                case SatelliteSystem.Mixed:
                    var mixedParser = new MixedNavParser(sr);
                    PerformOnData(mixedParser);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("SatelliteSystem");
            }
        }

        protected virtual void PerformOnData<T>(NavMessageContentParserBase<T> parser) where T : NavMessage
        {
            switch(ParseType)
            {
                case ParseType.RaiseEvents:
                    foreach (var navMessage in parser.Parse())
                    {
                        OnNewMessage(navMessage);
                    }
                    break;
                case ParseType.StoreData:
                    foreach (var navMessage in parser.Parse())
                    {
                        _navMessages.Add(navMessage);
                    }
                    break;
                case ParseType.StoreAndRaise:
                    foreach (var navMessage in parser.Parse())
                    {
                        OnNewMessage(navMessage);
                        _navMessages.Add(navMessage);
                    }

                    break;
            }
        }

        #endregion
    }

    
    public class RinexNavWithFilterParser : RinexNavParser
    {
        #region Fields

        private readonly NavigationFilter _navFilter;

        #endregion

        #region Constructor

        public RinexNavWithFilterParser(string filePath, ParseType parseType, NavigationFilter navFilter)
                : base(filePath, parseType)
        {
            _navFilter = navFilter;
        }

        #endregion

        #region Methods

        protected override void PerformOnData<T>(NavMessageContentParserBase<T> parser)
        {
            switch(ParseType)
            {
                case ParseType.RaiseEvents:
                    foreach (var navMessage in parser.Parse())
                    {
                        if (_navFilter.ValidateNavMsg(navMessage)) OnNewMessage(navMessage);
                    }
                    break;
                case ParseType.StoreData:
                    foreach (var navMessage in parser.Parse())
                    {
                        if (_navFilter.ValidateNavMsg(navMessage)) _navMessages.Add(navMessage);
                    }
                    break;
                case ParseType.StoreAndRaise:
                    foreach (var navMessage in parser.Parse())
                    {
                        if (_navFilter.ValidateNavMsg(navMessage))
                        {
                            OnNewMessage(navMessage);
                            _navMessages.Add(navMessage);
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}