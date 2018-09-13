// Rinex3Parser
// RinexHeaderLabel.cs-2016-08-09
namespace Rinex3Parser.Common
{
    public static class RinexHeaderLabel
    {
        internal const string VERSION = "RINEX VERSION / TYPE";
        internal const string PROGRAM = "PGM / RUN BY / DATE";
        internal const string COMMENT = "COMMENT";
        internal const string MARKER_NAME = "MARKER NAME";
        internal const string MARKER_NUMBER = "MARKER NUMBER";
        internal const string MARKER_TYPE = "MARKER TYPE";
        internal const string OBSERVER = "OBSERVER / AGENCY";
        internal const string RECEIVER = "REC # / TYPE / VERS";
        internal const string ANTENNA_TYPE = "ANT # / TYPE";
        internal const string POSITION = "APPROX POSITION XYZ";
        internal const string ANTENNA_OFFSETS = "ANTENNA: DELTA H/E/N";
        internal const string ANTENNA_XYZ_OFFSETS = "ANTENNA: DELTA X/Y/Z";
        internal const string ANTENNA_PHASECENTER = "ANTENNA: PHASECENTER";
        internal const string ANTENNA_B_SIGHT_XYZ = "ANTENNA: B.SIGHT XYZ";
        internal const string ANTENNA_ZERODIR_AZI = "ANTENNA: ZERODIR AZI";
        internal const string ANTENNA_ZERODIR_XYZ = "ANTENNA: ZERODIR XYZ";
        internal const string CENTER_OF_MASS_XYZ = "CENTER OF MASS: XYZ";
        internal const string SYS_OBS_TYPES = "SYS / # / OBS TYPES";
        internal const string SIGNAL_STRENGTH_UNIT = "SIGNAL STRENGTH UNIT";
        internal const string INTERVAL = "INTERVAL";
        internal const string FIRST_OBS = "TIME OF FIRST OBS";
        internal const string LAST_OBS = "TIME OF LAST OBS";
        internal const string RCV_CLOCK_OFFSET = "RCV CLOCK OFFS APPL";
        internal const string SYS_DCBS_APPLIED = "SYS / DCBS APPLIED";
        internal const string SYS_PCVS_APPLIED = "SYS / PCVS APPLIED";
        internal const string SYS_SCALE_FACTOR = "SYS / SCALE FACTOR";
        internal const string SYS_PHASE_SHIFT = "SYS / PHASE SHIFT";
        internal const string GLONASS_SLOT_FRQ = "GLONASS SLOT / FRQ #";
        internal const string GLONASS_COD_PHS_BIS = "GLONASS COD/PHS/BIS";
        internal const string GLONASS_COD_PHS_BIS2 = "GLONASS COD/PHS/BIS#";
        internal const string LEAP_SECONDS = "LEAP SECONDS";
        internal const string NUMBER_OF_SATELLITES = "# OF SATELLITES";
        internal const string NUMBER_OF_OBS = "PRN / # OF OBS";
        internal const string IONOSPHERIC_CORRECTIONS = "IONOSPHERIC CORR";
        internal const string TIME_SYSTEM_CORRECTIONS = "TIME SYSTEM CORR";
        internal const string END_OF_HEADER = "END OF HEADER";
        internal const string CRINEX_VERSION = "CRINEX VERS   / TYPE";
        internal const string CRINEX_PROGRAM = "CRINEX PROG / DATE";
    }
}