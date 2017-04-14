// Rinex3ParserTest
// TestUtils.cs-2016-08-25

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rinex3Parser.Common;


namespace Rinex3ParserTest
{
    internal static class TestUtils
    {
        internal static ObservationCode[] FormObsCodes(string s)
        {
            return s.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => (ObservationCode)Enum.Parse(typeof(ObservationCode), t, true)).ToArray();
        }

        internal static StreamReader ConvertStringToReader(string str)
        {
            var mStream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return new StreamReader(mStream);
        }
    }
}