// Rinex3Parser
// RinexRegex.cs-2016-08-24

#region

using System.Text.RegularExpressions;


#endregion

namespace Rinex3Parser.Common
{
    internal static class RinexRegex
    {
        #region Fields

        private static readonly RegexOptions CommonCompiledRegexOptions = RegexOptions.Compiled|RegexOptions.IgnoreCase|
                                                                          RegexOptions.Singleline;

        private static readonly RegexOptions CommonNonCompiledRegexOptions = RegexOptions.IgnoreCase|
                                                                             RegexOptions.Singleline;

        #endregion

        #region header regex

        internal static readonly Regex RinexVersionRegexp =
                new Regex(@"^(?<rnxver>.{9})\s{11}(?<filetype>(O|N|M){1}).{19}(?<satsystem>G|R|E|J|C|I|M)?.{19,20}$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex ProgramRunBy = new Regex(@"^(?<pgm>.{20})(?<runby>.{20})(?<date>.{20})$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex A60Regex = new Regex(@"^(.{60})$", CommonNonCompiledRegexOptions);
        internal static readonly Regex A20Regex = new Regex(@"^(.{20})\s+$", CommonCompiledRegexOptions);

        internal static readonly Regex ObserverAgencyRegex = new Regex(@"^(?<observer>.{20})(?<agency>.{40})$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex ReceiverRegex = new Regex(@"^(?<recnum>.{20})(?<type>.{20})(?<vers>.{20})$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex AntennaTypRegex = new Regex(@"^(?<antnum>.{20})(?<anttype>.{20}).+$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex PositionRegex =
                new Regex(@"^\s*(?<X>[+-]?\d{1,}\.\d{4})\s+(?<Y>[+-]?\d{1,}\.\d{4})\s+(?<Z>[+-]?\d{1,}\.\d{4})\s+$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex AntennaOffsetRegex =
                new Regex(@"^\s*(?<H>[+-]?\d{1,}\.\d{4})\s+(?<E>[+-]?\d{1,}\.\d{4})\s+(?<N>[+-]?\d{1,}\.\d{4})\s+$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex PhaseCenterRegex =
                new Regex(
                        @"^(?<satsystem>G|R|E|J|C|I|M|S)\s+(?<ObsCode>\w{3})\s+\s*(?<N>[+-]?\d{1,}\.\d{4})\s+(?<E>[+-]?\d{1,}\.\d{4})\s+(?<U>[+-]?\d{1,}\.\d{4})\s+$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex AntennaZeroDirAziRegex = new Regex(@"^\s*(?<AZeroDirection>\d{1,}\.\d{4})\s+$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex SysObsTypesRegex =
                new Regex(@"^(?<satsystem>G|R|E|J|C|I|M|S)?\s{2,}(?<numofobs>\d{1,3})?(?<obs>\s\w{3}){1,}",
                        CommonCompiledRegexOptions);

        internal static readonly Regex IntervalRegex = new Regex(@"^\s*(?<interval>\d{1,7}\.\d{3})\s+$",
                CommonNonCompiledRegexOptions); //F10.3

        internal static readonly Regex ObsTimeRegex =
                new Regex(
                        @"^\s*(?<year>\d{1,4})\s+(?<month>\d{1,2})\s+(?<day>\d{1,2})\s+(?<hour>\d{1,2})\s+(?<min>\d{1,2})\s+(?<preciseSec>\d{1,5}\.\d{7})\s+(?<timesystem>GPS|GLO|GAL|QZS|BDT|IRN)?\s+$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex RcvClkOffsetAplRegex = new Regex(@"^\s+(?<rcv_clk_offs_appl>\d)\s+$",
                CommonNonCompiledRegexOptions); //it seems to be boolean (1 -- yes, 0 -- no)

        internal static readonly Regex CorrectionInfoRegex =
                new Regex(@"^(?<satsystem>G|R|E|J|C|I|M|S)\s(?<pgm>\w{1,17})\s+(?<corrsource>.{1,40})\s*$",
                        CommonCompiledRegexOptions);

        internal static readonly Regex SysScaleFactorRegex =
                new Regex(
                        @"^(?<satsystem>G|R|E|J|C|I|M|S)?\s{1,}(?<Factor>\d{4})?\s{2}(?<obsnum>\d{2})?(?<obs>\s\w{3}){1,}\s*$",
                        CommonCompiledRegexOptions);

        internal static readonly Regex SysPhaseShiftRegex =
                new Regex(
                        @"^(?<satsystem>G|R|E|J|C|I|M|S)?\s(?<obscode>\w{3})?\s{0,}(?<corrcycles>[-]?\d{1,2}\.\d{5})?\s{0,3}(?<num>\d{1,2})?(?<sta>\s\w\d{2}){0,}\s*$",
                        CommonCompiledRegexOptions);

        internal static readonly Regex GloSlotFreqRegex =
                new Regex(@"^\s*(?<satnum>\d{1,3})?(?<satfreq>\s\w\d{2}\s{1,2}[-]?\d{1,2}){1,8}\s*$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex GloCodePhaBiasRegex =
                new Regex(@"^\s*(?<phcor>\s\w{3}\s{1,4}[-]?\d{1,4}\.\d{3}){4}\s*$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex LeapSecondRegex = new Regex(
                @"^\s*(?<numofls>\d{1,6})?\s{1,6}(?<fpls>\d{1,6})?\s{1,6}(?<weeknum>\d{1,6})?\s{1,6}(?<daynum>\d{1,6})?(?<satsystem>GPS|BDS|BDT)?\s*$",
                //BDS == BDT (strange rinex defintions)
                CommonNonCompiledRegexOptions);

        internal static readonly Regex NumberOfSatellites = new Regex(@"^\s*(?<numofsat>\d{1,6})\s*$",
                CommonNonCompiledRegexOptions);

        internal static readonly Regex NumberOfObsRegex =
                new Regex(@"^\s{3,6}(?<type>\w)?\s{0,1}(?<satnum>\d{1,2})?(?<obs>\s{1,6}\d{1,6}){1,9}\s*$",
                        CommonCompiledRegexOptions);

        internal static readonly Regex RinexV3FileNameAttrRegex = new Regex(
                @"^(?<stname>\w{4})\w{2}(?<cc>\w{3})_(?<source>[R|U|S])_(?<year>\d{4})(?<doy>\d{3})(?<hh>\d{2})(?<mm>\d{2})_(?<interval>\d{2})(?<period>\w)_(?:(?<freq>\d{2})(?<frequnits>\w)){0,1}_{0,1}(?<gnss>\w)(?<type>\w).(?<fileformat>(crx|rnx))(?:.(?<zip>(zip|gz|bz2))){0,1}$",
                CommonCompiledRegexOptions);
        #endregion

        #region obs data regex

        internal static readonly Regex ObsDataRegex =
                new Regex(@"(?<obsdata>[-]?\d{0,10}\.\d{3})(?<lli>\d)?(?<sigstrength>\d)?", CommonCompiledRegexOptions);

        //{14,16} to take into account last obs if LLI and signal strength is missing
        internal static readonly Regex ObsRecordRegex = new Regex(@"^.{3}(?<data>.{14,16}){1,}$",
                CommonCompiledRegexOptions);

        internal static readonly Regex ObsEpochRecordRegex =
                new Regex(
                        @"^>\s(?<year>\d{4})\s{1,2}(?<month>\d{1,2})\s{1,2}(?<day>\d{1,2})\s{1,2}(?<hour>\d{1,2})\s{1,2}(?<min>\d{1,2})\s{1,2}(?<preciseSec>\d{1,2}\.\d{7})\s\s(?<flag>\d)\s{0,2}(?<epochRecords>\d{1,3})\s{0,6}(?<rcvclkoffset>..\.\d{12})?\s*$",
                        CommonCompiledRegexOptions);


        internal static readonly Regex ObsEpochRecordReserveRegex =
                new Regex(
                        @"^>\s+(?<flag>\d)\s{0,2}(?<satNum>\d{1,3})$", CommonCompiledRegexOptions);


        internal static readonly Regex IonoCorrections =
                new Regex(
                        @"^(?<satsystem>GAL|GPS|QZS|BDS|IRN)(?<AB>\w)?\s{1,2}(?<ioncorr>.{12}){1,4}\s(?<timemark>\w)?\s(?<id>\d{1,2})?\s*$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex TimeSystemCorrection =
                new Regex(
                        @"^(?<corrtype>GAUT|GPUT|SBUT|GLUT|GPGA|GLGP|QZGP|QZUT|BDUT|IRUT|IRGP)\s(?<a0>.{17})(?<a1>.{16})(?<reftime>.{7})(?<refweek>.{5})\s{1,2}(?<sbas>\w{4,5})?\s{1,2}(?<utc>\d{1,2})?\s+$",
                        CommonNonCompiledRegexOptions);

        internal static readonly Regex NavOrbitParamtersRegex = new Regex(@"^\s{4}(?<orbitdata>.{19}){1,4}$",
                CommonCompiledRegexOptions);

        internal static readonly Regex NavSvEpochClk =
                new Regex(
                        @"^(?<satnum>\w{3})\s(?<toc>\d{4})\s{1,2}(?<month>\d{1,2})\s{1,2}(?<day>\d{1,2})\s{1,}(?<hour>\d{1,2})\s{1,2}(?<min>\d{1,2})\s{1,2}(?<sec>\d{1,2})(?<svclkdata>.{19}){3}$",
                        CommonCompiledRegexOptions);

        #endregion
    }
}