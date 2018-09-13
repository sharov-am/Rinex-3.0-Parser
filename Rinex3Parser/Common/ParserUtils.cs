// Rinex3Parser
// ParserUtils.cs-2016-07-15

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


#endregion

namespace Rinex3Parser.Common
{
    public static class ParserUtils
    {
        public static SatelliteSystem ConvertToEnum(this char system)
        {
            var s = Char.ToLowerInvariant(system);
            switch(s)
            {
                case 'g':
                    return SatelliteSystem.Gps;
                case 'r':
                    return SatelliteSystem.Glo;
                case 'e':
                    return SatelliteSystem.Gal;
                case 'j':
                    return SatelliteSystem.Qzss;
                case 'c':
                    return SatelliteSystem.Bds;
                case 'i':
                    return SatelliteSystem.Irnss;
                case 's':
                    return SatelliteSystem.Sbas;
                case 'm':
                    return SatelliteSystem.Mixed;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("No satellite system {0}", s));
            }
        }


        public static string SatelliteCountryCode(this SatelliteSystem system)
        {
            switch(system)
            {
                case SatelliteSystem.Gps:
                    return "G";
                case SatelliteSystem.Glo:
                    return "R";
                case SatelliteSystem.Gal:
                    return "E";
                case SatelliteSystem.Qzss:
                    return "J";
                case SatelliteSystem.Bds:
                    return "C";
                case SatelliteSystem.Irnss:
                    return "I";
                case SatelliteSystem.Sbas:
                    return "S";

                default:
                    throw new ArgumentOutOfRangeException("system", system, null);
            }
        }


        public static string SatellitePrnString(SatelliteSystem system, int satNum)
        {
            switch(system)
            {
                case SatelliteSystem.Gps:
                    return Enum.GetName(typeof(GpsSatellite), satNum);
                case SatelliteSystem.Glo:
                    return Enum.GetName(typeof(GloSatellite), satNum);
                case SatelliteSystem.Gal:
                    return Enum.GetName(typeof(GalSatellite), satNum);
                case SatelliteSystem.Qzss:
                    return Enum.GetName(typeof(QzssSatellite), satNum);
                case SatelliteSystem.Bds:
                    return Enum.GetName(typeof(BdsSatellite), satNum);
                case SatelliteSystem.Irnss:
                    return Enum.GetName(typeof(IrnssSatellite), satNum);
                case SatelliteSystem.Sbas:
                    return Enum.GetName(typeof(SbasSatellite), satNum);
                default:
                    throw new ArgumentOutOfRangeException("system", system, null);
            }
        }


       
        
        public static IEnumerable<T> SatellitePrn<T>(this IEnumerable<int> satPrn)
        {
            var temp = Enum.GetValues(typeof(T));
            return satPrn.Select(i1 => (T)temp.GetValue(i1 - 1));
        }




        public static void GenerateObsSignals()
        {
            var bands = new Dictionary<SatelliteSystem, int[]>
            {
                {SatelliteSystem.Gps, new[] {1, 2, 5}},
                    {SatelliteSystem.Glo, new[] {1, 2, 3}},
                {SatelliteSystem.Gal, new[] {1, 5, 6, 7, 8}},
                {SatelliteSystem.Bds, new[] {2, 6, 7}},
                {SatelliteSystem.Irnss, new[] {5, 9}},
                {SatelliteSystem.Qzss, new[] {1, 2, 5, 6}},
                {SatelliteSystem.Sbas, new[] {1, 5}}
            };
            var attributes = new Dictionary<SatelliteSystem, ObsAttribute[]>
            {
                {
                    SatelliteSystem.Gps, new[]
                    {
                                    ObsAttribute.P, ObsAttribute.C, ObsAttribute.D, ObsAttribute.Y, ObsAttribute.M,
                                    ObsAttribute.N,
                                    ObsAttribute.I, ObsAttribute.Q, ObsAttribute.S, ObsAttribute.L, ObsAttribute.X,
                                    ObsAttribute.W
                    }
                },
                    {SatelliteSystem.Glo, new[] {
                            ObsAttribute.P, ObsAttribute.C, ObsAttribute.X, ObsAttribute.I, ObsAttribute.Q}},
                {
                    SatelliteSystem.Gal, new[]
                    {
                                    ObsAttribute.A, ObsAttribute.B, ObsAttribute.C, ObsAttribute.I, ObsAttribute.Q,
                                    ObsAttribute.X,
                        ObsAttribute.Z,
                    }
                },
                {SatelliteSystem.Bds, new[] {ObsAttribute.I, ObsAttribute.Q, ObsAttribute.X}},
                {SatelliteSystem.Irnss, new[] {ObsAttribute.A, ObsAttribute.B, ObsAttribute.C, ObsAttribute.X}},
                    {SatelliteSystem.Qzss,
                            new[]
                {
                                    ObsAttribute.C, ObsAttribute.I, ObsAttribute.Q, ObsAttribute.S, ObsAttribute.L,
                                    ObsAttribute.X, ObsAttribute.Z
                            }
                },
                    {SatelliteSystem.Sbas, new[] {ObsAttribute.C, ObsAttribute.I, ObsAttribute.Q, ObsAttribute.X}}
            };
            using(var file = File.Create(Path.Combine(Environment.CurrentDirectory, "codes")))
            {
                List<string> z = new List<string>();
                using(var sw = new StreamWriter(file))
                {
                foreach (var oType in Enum.GetNames(typeof(ObsType)))
                {
                    var result = from band in bands
                        join attr in attributes on band.Key equals attr.Key
                        select new {bands = band.Value, attr = attr.Value};

                    var temp = from r in result
                        from rb in r.bands
                        from ra in r.attr
                        select String.Format("{0}{1}{2}", oType, rb, ra);

                    z.AddRange(temp);
                }
                    foreach (var obs in z.Distinct(StringComparer.Create(CultureInfo.InvariantCulture, true))
                            .OrderBy(t => t, StringComparer.InvariantCultureIgnoreCase).ToList())
                {
                    sw.WriteLine(obs + ",");
                    }
                }
            }
        }
    }
}