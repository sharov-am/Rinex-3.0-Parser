// Rinex3Parser
// Helper.cs-2016-08-18

using System.Linq;


namespace Rinex3Parser.Common
{
    internal static class Helper
    {
        public static bool IsEmptyOrWhiteSpace(this string value)
        {
            return value.All(char.IsWhiteSpace);
        }
    }
}