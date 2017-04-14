// Rinex3Parser
// NavMsgParserBase.cs-2016-08-15

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Rinex3Parser.Common;


#endregion

namespace Rinex3Parser.Nav
{
    public abstract class NavMessageContentParserBase
    {
    }


    public abstract class NavMessageContentParserBase<T> : NavMessageContentParserBase where T : NavMessage
    {
        #region Fields

        private readonly string[] _broadCastOrbits;
        private readonly int _numOfMsgLines;
        protected readonly StreamReader Sr;

        #endregion

        #region Constructor

        protected NavMessageContentParserBase(StreamReader sr, int numOfMsgLines)
        {
            Sr = sr;
            _numOfMsgLines = numOfMsgLines;
            _broadCastOrbits = new string[_numOfMsgLines];
        }

        #endregion

        #region Methods

        protected abstract T ParseContents(string[] broadCastOrbits);

        internal virtual IEnumerable<T> Parse()
        {
            var counter = 0;
            while(!Sr.EndOfStream)
            {
                var line = Sr.ReadLine();
                if (String.IsNullOrEmpty(line))
                    continue;
                _broadCastOrbits[counter%_numOfMsgLines] = line;
                counter++;
                if (counter%_numOfMsgLines == 0)
                {
                    yield return ParseContents(_broadCastOrbits);
                }
            }
        }

        protected internal T1 ParseImpl<T1>() where T1 : NavMessage
        {
            var counter = 0;
            while(!Sr.EndOfStream && counter != _numOfMsgLines)
            {
                var temp = Sr.ReadLine();
                _broadCastOrbits[counter] = temp;
                counter++;
            }
            return ParseContents(_broadCastOrbits) as T1;
        }


        protected void ParseSvEpochClk<T1>(string input, out T1 satnum, out DateTime dt,
                out double[] clkData)
        {
            var match = RinexRegex.NavSvEpochClk.Match(input);
            satnum = (T1)Enum.Parse(typeof(T1), match.Groups["satnum"].Value);
            var year = Int32.Parse(match.Groups["toc"].Value,CultureInfo.InvariantCulture);
            dt = new DateTime(year, Int32.Parse(match.Groups["month"].Value,CultureInfo.InvariantCulture),
                    Int32.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture), Int32.Parse(match.Groups["hour"].Value, CultureInfo.InvariantCulture),
                    Int32.Parse(match.Groups["min"].Value, CultureInfo.InvariantCulture), Int32.Parse(match.Groups["sec"].Value, CultureInfo.InvariantCulture));
            Debug.Assert(match.Groups["svclkdata"].Captures.Count == 3,
                    "match.Groups[\"svclkdata\"].Captures.Count == 3");
            input = Regex.Replace(match.Groups["svclkdata"].Captures[0].Value, "[dD]", "E");
            var clkBias = Double.Parse(input, CultureInfo.InvariantCulture);
            input = Regex.Replace(match.Groups["svclkdata"].Captures[1].Value, "[dD]", "E");
            var clkDrift = Double.Parse(input, CultureInfo.InvariantCulture);
            input = Regex.Replace(match.Groups["svclkdata"].Captures[2].Value, "[dD]", "E");
            var clkDriftRate = Double.Parse(input, CultureInfo.InvariantCulture);

            clkData = new[] {clkBias, clkDrift, clkDriftRate};
        }


        private static void ParseOrbitDataImpl(string input, int orbitIndex, double[] orbitParameters)
        {
            input = Regex.Replace(input, "[dD]", "E");
            var match = RinexRegex.NavOrbitParamtersRegex.Match(input);
            var counter = 0;
            foreach (Capture capture in match.Groups["orbitdata"].Captures)
            {
                double result;
                if (Double.TryParse(capture.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                {
                    orbitParameters[orbitIndex + counter] = result;
                }
                counter++;
            }
        }

        protected double[] ParseContentsImpl(string[] broadCastOrbits)
        {
            //_numOfMsgLines - 1  -- exclude header sv/epoch/svclk line
            // every orbit block have 4 values hence *4
            var orbitParameters = new double[(_numOfMsgLines - 1)*4];
            //bypass sv/epoch/sv clk row, so start with 1 orbit data
            for (var i = 1; i < broadCastOrbits.Length; i++)
            {
                ParseOrbitDataImpl(broadCastOrbits[i], 4*(i - 1), orbitParameters);
            }
            return orbitParameters;
        }

        #endregion
    }
}