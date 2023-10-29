using System;

namespace PollManagement.API.Helper
{
    public class Utilities
    {
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
            catch (Exception)
            { }
            return detailedException;
        }
    }
}
