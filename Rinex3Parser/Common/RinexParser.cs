#region

using System;
using System.IO;
using JetBrains.Annotations;
using Rinex3Parser.Nav;
using Rinex3Parser.Obs;


#endregion

namespace Rinex3Parser.Common
{
    public abstract class RinexParser
    {
        private readonly string _filePath;
        protected readonly ParseType ParseType;

        protected RinexParser(string filePath, ParseType parseType)
        {
            _filePath = filePath;
            ParseType = parseType;
        }


        public void Parse()
        {
            try
            {
                using(Stream file = File.OpenRead(_filePath))
                {
                    var sr = new StreamReader(file);
                    ParseHeader(sr);
                    ParseContents(sr);
                }
            }
            catch(Exception e)
            {
                throw new RinexParserException(e.Message, e);
            }
        }

        public void ParseHeader()
        {
            try
            {
                using(Stream file = File.OpenRead(_filePath))
                {
                    var sr = new StreamReader(file);
                    ParseHeader(sr);
                }
            }
            catch(Exception e)
            {
                throw new RinexParserException(e.Message, e);
            }
        }

        public static RinexParser ParserFactory(string filePath, ParseType parseType)
        {
            using(Stream file = File.OpenRead(filePath))
            {
                var sr = new StreamReader(file);
                var firstLine = sr.ReadLine();
                if (!String.IsNullOrEmpty(firstLine))
                {
                    firstLine = firstLine.Substring(0, 60);
                    var match = RinexRegex.RinexVersionRegexp.Match(firstLine);
                    var val = match.Groups["filetype"].Value;
                    if (!String.IsNullOrEmpty(val))
                    {
                        val = val.ToUpperInvariant();
                        switch(val)
                        {
                            case "O":
                                return new RinexObsParser(filePath, parseType);
                            case "N":
                                return new RinexNavParser(filePath, parseType);
                        }
                    }
                }
                throw new ArgumentException("Invalid file...");
            }
        }

        public abstract RinexType RinexType { get; }
        protected abstract void ParseHeader([NotNull] StreamReader sr);
        protected abstract void ParseContents([NotNull] StreamReader sr);
    }
}