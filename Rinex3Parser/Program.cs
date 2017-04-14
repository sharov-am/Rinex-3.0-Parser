// Rinex3Parser
// Program.cs-2016-08-19

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Rinex3Parser.Common;
using Rinex3Parser.Nav;
using Rinex3Parser.Obs;


#endregion

namespace Rinex3Parser
{
    class Program
    {
        #region Fields

        private static StreamWriter sw1;

        #endregion

        #region Methods

        static void Main()
        {
           
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //for (int i = 0; i < 10;i++)
            {
                ParseFiles(@"f:\GIT\Geodesy\Rinex3Parser\TestFiles\", @"f:\GIT\Geodesy\Rinex3Parser\Result");
                ParseSingleFile();
            }
            //ParseSingleFile();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            //ParseSingleFile();
            Console.WriteLine("done!!!");
            Console.ReadLine();
        }

        private static void ParseSingleFile()
        {
            var path = Path.Combine(@"f:\GIT\Geodesy\Rinex3Parser\TestFiles\","cibg0880.16o");
            var path2 = Path.Combine(Environment.CurrentDirectory, "rnxheader.txt");
            sw1 = new StreamWriter("123.321");
            var z = new Dictionary<SatelliteSystem, int[]>();
            var vals = Enum.GetValues(typeof(BdsSatellite));
            z.Add(SatelliteSystem.Bds, vals.Cast<int>().ToArray());
            var filter = new NavigationFilter(new DateTime(2016, 04, 29, 08, 0, 0), new DateTime(2016, 04, 29, 9, 0, 0),
                    z);

            //var rinexObsParser = new RinexObsParser(path, ParseType.StoreAndRaise);
            var rinexObsParser = new RinexObsWithFilter(path, ParseType.StoreAndRaise,
                    new ObservationFilter(null, null,
                            SatelliteSystem.Qzss|SatelliteSystem.Glo|SatelliteSystem.Gps|SatelliteSystem.Bds|
                            SatelliteSystem.Sbas, null));
            rinexObsParser.ObsDataEvents += ObsParserObsDataEvents;
            rinexObsParser.HeaderInfoEvent += ObsParserHeaderInfoEvent;
            rinexObsParser.Parse();
            sw1.Flush();
            sw1.Dispose();
        }

        static void RinexParserNewNavMessageEvent(object sender, NavMessage e)
        {
            sw1.Write(e.ToString());
        }

        static void RinexParserNavHeaderInfoEvent(object sender, RinexNavHeaderData rinexNavHeaderData)
        {
            sw1.WriteLine(rinexNavHeaderData.ToString());
        }

        static void ObsParserObsDataEvents(object sender, RinexObsData e)
        {
            sw1.WriteLine(e.ToString());
        }

        static void ObsParserHeaderInfoEvent(object sender, RinexObsHeaderData e)
        {
            sw1.WriteLine(e.ToString());
        }

        private static void ParseFiles([NotNull] string inputFolder, [NotNull] string output)
        {
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);
            foreach (var file in Directory.GetFiles(output))
            {
                File.Delete(file);
            }
            foreach (var inputFile in Directory.GetFiles(inputFolder))
            {
                RinexParser parser;
                try
                {
                    parser = RinexParser.ParserFactory(inputFile, ParseType.RaiseEvents);
                }
                catch(ArgumentException)
                {
                    continue;
                }
                var navParser = parser as RinexNavParser;
                var newNavFile = Path.Combine(output, Path.GetFileName(inputFile));
                RinexObsParser obsParser = null;
                using(var fs = File.Create(newNavFile))
                {
                    sw1 = new StreamWriter(fs);

                    if (navParser != null)
                    {
                        navParser.NewNavMessageEvent += RinexParserNewNavMessageEvent;
                        navParser.NavHeaderInfoEvent += RinexParserNavHeaderInfoEvent;
                    }
                    else
                    {
                        obsParser = parser as RinexObsParser;

                        if (obsParser != null)
                        {
                            obsParser.HeaderInfoEvent += ObsParserHeaderInfoEvent;
                            obsParser.ObsDataEvents += ObsParserObsDataEvents;
                        }
                    }

                    parser.Parse();
                    if (navParser != null)
                    {
                        navParser.NewNavMessageEvent -= RinexParserNewNavMessageEvent;
                        navParser.NavHeaderInfoEvent -= RinexParserNavHeaderInfoEvent;
                    }

                    if (obsParser != null)
                    {
                        obsParser.HeaderInfoEvent -= ObsParserHeaderInfoEvent;
                        obsParser.ObsDataEvents -= ObsParserObsDataEvents;
                    }
                    sw1.Flush();
                }
            }
        }

        #endregion
    }
}