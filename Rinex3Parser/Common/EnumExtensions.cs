// Rinex3Parser
// EnumUtils.cs-2016-08-02

#region



#endregion

namespace Rinex3Parser.Common
{
    public static class EnumExtensions
    {
        public static bool IsSet(this SatelliteSystem satSystems, SatelliteSystem singleSatSystem)
        {
            return (singleSatSystem&satSystems) == singleSatSystem;
        }
    }
}