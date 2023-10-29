using System;
using TimeZoneConverter;
namespace Courses.API.Helper
{
    public static class TimeZoneHelper
    {       

            public static DateTime getLocaltimeFromUniversal(DateTime utcDateTime, string timeZone)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime,
               //TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(timeZone)));
            TimeZoneInfo.FindSystemTimeZoneById(timeZone));
        }
            public static DateTime ConvertLocalToUTCwithTimeZone(DateTime localDateTime, string timezone)
            {
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(localDateTime,
                 // TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(timezone)));
            TimeZoneInfo.FindSystemTimeZoneById(timezone));
        }
       
    }
    }

