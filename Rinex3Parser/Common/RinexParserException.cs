// Rinex3Parser
// RinexParserException.cs-2016-09-02

using System;
using System.Runtime.Serialization;


namespace Rinex3Parser.Common
{
    [Serializable]
    public class RinexParserException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RinexParserException()
        {
        }

        public RinexParserException(string message) : base(message)
        {
        }

        public RinexParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RinexParserException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
        {
        }
    }
}