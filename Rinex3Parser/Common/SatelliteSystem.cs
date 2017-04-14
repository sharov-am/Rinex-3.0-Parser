// Rinex3Parser
// SatelliteSystem.cs-2016-07-08

#region

using System;


#endregion

namespace Rinex3Parser.Common
{
    [Flags]
    public enum SatelliteSystem
    {
        Gps = 1,
        Glo = 2,
        Gal = 4,
        Qzss = 8,
        Bds = 16,
        Irnss = 32,
        Sbas = 64,
        Mixed = 128
    }


    //https://en.wikipedia.org/wiki/List_of_GLONASS_satellites
    public enum GloSatellite
    {
        R01 = 1,
        R02,
        R03,
        R04,
        R05,
        R06,
        R07,
        R08,
        R09,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,
        R16,
        R17,
        R18,
        R19,
        R20,
        R21,
        R22,
        R23,
        R24,
        R25,
        R26,
        R27,
        R28,
        R29,
        R30,
        R31,
        R32,
        R33,
        R34,
        R35,
        R36,
        R37,
        R38,
        R39,
    }


    //https://en.wikipedia.org/wiki/List_of_GPS_satellites
    public enum GpsSatellite
    {
        G01 = 1,
        G02,
        G03,
        G04,
        G05,
        G06,
        G07,
        G08,
        G09,
        G10,
        G11,
        G12,
        G13,
        G14,
        G15,
        G16,
        G17,
        G18,
        G19,
        G20,
        G21,
        G22,
        G23,
        G24,
        G25,
        G26,
        G27,
        G28,
        G29,
        G30,
        G31,
        G32,
        G33,
    }


    //https://en.wikipedia.org/wiki/List_of_BeiDou_satellites#cite_note-gpsworld201501-3
    public enum BdsSatellite
    {
        C01 = 1,
        C02,
        C03,
        C04,
        C05,
        C06,
        C07,
        C08,
        C09,
        C10,
        C11,
        C12,
        C13,
        C14,
        C15,
        C30,
        C31,
        C32,
        C33,
        C34,
    }


    //https://en.wikipedia.org/wiki/List_of_Galileo_satellites
    public enum GalSatellite
    {
        E01 = 1,
        E02,
        E03,
        E04,
        E05,
        E06,
        E07,
        E08,
        E09,
        E10,
        E11,
        E12,
        E13,
        E14,
        E15,
        E16,
        E17,
        E18,
        E19,
        E20,
        E21,
        E22,
        E23,
        E24,
        E25,
        E26,
        E27,
        E28,
        E29,
        E30,
    }


    //https://en.wikipedia.org/wiki/Quasi-Zenith_Satellite_System#QZSS_timekeeping_and_remote_synchronization
    public enum QzssSatellite
    {
        J01 = 1,
        J02,
        J03,
        J04
    }


    //https://en.wikipedia.org/wiki/Quasi-Zenith_Satellite_System#QZSS_timekeeping_and_remote_synchronization
    public enum IrnssSatellite
    {
        I01 = 1,
        I02,
        I03,
        I04,
        I05,
        I06,
        I07
    }


    //http://www.nstb.tc.faa.gov/RTData_WAASSatelliteData.htm
    //http://www.navipedia.net/index.php/WAAS_Space_Segment
    public enum SbasSatellite
    {
        S01 = 1,
        S02,
        S03,
        S04,
        S05,
        S06,
        S07,
        S08,
        S09,
        S10,
        S11,
        S12,
        S13,
        S14,
        S15,
        S16,
        S17,
        S18,
        S19,
        S20,
        S21,
        S22,
        S23,
        S24,
        S25,
        S26,
        S27,
        S28,
        S29,
        S30,
        S31,
        S32,
        S33,
        S34,
        S35,
        S36,
        S37,
        S38,
    }



}