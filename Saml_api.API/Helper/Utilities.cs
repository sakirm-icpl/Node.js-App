using log4net;
using System;

namespace Saml.API.Helper
{
    public class Utilities
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Utilities));
        public static string GetDetailedException(Exception ex)
        {

            string detailedException = string.Empty;
            try
            {
                detailedException = Environment.NewLine + Environment.NewLine;
                detailedException = detailedException + "Message :- " + ex.Message + Environment.NewLine;
                detailedException = detailedException + "StackTrace :- " + ex.StackTrace + Environment.NewLine;
                if (ex.InnerException != null)
                    detailedException = detailedException + "InnerExceptionMessage :- " + ex.InnerException.Message + Environment.NewLine;

            }
            catch (Exception Ex)
            {
                
            }
            return detailedException;
        }
    }
}
