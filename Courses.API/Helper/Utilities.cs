using System;
using System.Runtime.InteropServices;
using Courses.API.Helper;
using log4net;


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
            catch (Exception Ex )
            {   }
            return detailedException;
        }

        public static void ReleaseObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

}

