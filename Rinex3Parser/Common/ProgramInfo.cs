using System;


namespace Rinex3Parser.Common
{
    public class ProgramInfo
    {
        #region Properties

        public string Name { get; set; }
        public string AgencyInfo { get; set; }
        public DateTime FileCreationDateTime { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var dt = FileCreationDateTime.ToString("yyyyMMdd HHmmss ") +
                     FileCreationDateTime.Kind.ToString().ToUpper();
            return String.Format("{0,-20}{1,-20}{2,-20}", Name, AgencyInfo, dt);
        }

        #endregion
    }
}