// Rinex3Parser
// Utils.cs-2018-08-09

using System;


namespace Rinex3Parser.Common
{
    public static class Utils
    {
        public static SatelliteSystem CharToGnss(char ch)
        {
            switch(Char.ToUpper(ch))
            {
                case 'G':
                    return SatelliteSystem.Gps;
                case 'R':
                    return SatelliteSystem.Glo;
                case 'E':
                    return SatelliteSystem.Gal;
                case 'J':
                    return SatelliteSystem.Qzss;
                case 'C':
                    return SatelliteSystem.Bds;
                case 'I':
                    return SatelliteSystem.Irnss;
                case 'S':
                    return SatelliteSystem.Sbas;
                case 'M':
                    return SatelliteSystem.Mixed;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static FileType CharToFileType(char ch)
        {
            switch(Char.ToUpper(ch))
            {
                case 'O':
                    return FileType.Obs;
                case 'N':
                    return FileType.Nav;
                case 'M':
                    return FileType.Met;


                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}