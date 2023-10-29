using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using ILT.API.Helper;
using log4net;

namespace ILT.API.Common
{
    public class BigBlueButtonUtilities
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BigBlueButtonUtilities));
        private IConfiguration _iConfig { get; set; }
        public BigBlueButtonUtilities(IConfiguration iConfig)
        {
            _iConfig = iConfig;
        }

        public static string GetDetailedException(Exception ex)
        {
            string exceptionText = string.Empty;
            try
            {
                exceptionText = "Error Message: " + ex.Message;
                exceptionText = exceptionText + "Stack Trace: " + ex.StackTrace;
            }
            catch (Exception e)
            { _logger.Error(Utilities.GetDetailedException(e));
            }
            return exceptionText;
        }

        public string GetCheckSum(string rawData)
        {
            // Create a SHA256
            var data = Encoding.ASCII.GetBytes(rawData);
            var hashData = new SHA1Managed().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash.ToLower();
        }

        public string GetCheckSumWithSecret(string rawData)
        {
            // Append secret to the provided URL.
            rawData = rawData + _iConfig.GetSection(BBBConstant.SHARED_SECRET).Value;
            var data = Encoding.ASCII.GetBytes(rawData);
            var hashData = new SHA1Managed().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash.ToLower();
        }

        public string GetCreateMeetingURL(BBBMeeting meeting, string userName)
        {
            string queryParameters = string.Empty;
            try
            {
                queryParameters = queryParameters + "allowStartStopRecording=" + HttpUtility.UrlEncode(meeting.AllowStartStopRecording.ToString().ToLower());
                queryParameters = queryParameters + "&autoStartRecording=" + HttpUtility.UrlEncode(meeting.AutoStartRecording.ToString().ToLower());
                queryParameters = queryParameters + "&duration=" + meeting.Duration;
                queryParameters = queryParameters + "&meetingID=" + HttpUtility.UrlEncode(meeting.MeetingID);
                queryParameters = queryParameters + "&name=" + HttpUtility.UrlEncode(meeting.MeetingName);
                queryParameters = queryParameters + "&record=" + HttpUtility.UrlEncode(meeting.Record.ToString().ToLower());
                queryParameters = queryParameters + "&attendeePW=" + HttpUtility.UrlEncode(BBBConstant.AttendeePassword);
                queryParameters = queryParameters + "&moderatorPW=" + HttpUtility.UrlEncode(BBBConstant.ModeratorPassword);
                return _iConfig.GetSection(BBBConstant.BBB_BASE_URL).Value + "/create?" + queryParameters + "&checksum=" + GetCheckSumWithSecret("create" + queryParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                return "";
            }
        }

        public string GetEndMeetingURL(BBBMeeting meeting)
        {
            string queryParameters = string.Empty;
            try
            {
                queryParameters = queryParameters + "meetingID=" + HttpUtility.UrlEncode(meeting.MeetingID);
                return _iConfig.GetSection(BBBConstant.BBB_BASE_URL).Value + "/end?" + queryParameters + "&checksum=" + GetCheckSumWithSecret("end" + queryParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return ""; 
            }
        }

        public string GetMeetingRunningURL(BBBMeeting meeting)
        {
            string queryParameters = string.Empty;
            try
            {
                queryParameters = queryParameters + "meetingID=" + HttpUtility.UrlEncode(meeting.MeetingID);
                return _iConfig.GetSection(BBBConstant.BBB_BASE_URL).Value + "/isMeetingRunning?" + queryParameters + "&checksum=" + GetCheckSumWithSecret("isMeetingRunning" + queryParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return ""; 
            }
        }

        public string GetJoinMeetingURL(BBBMeeting meeting, string attendeeName, bool isMeetingCreator)
        {
            string queryParameters = string.Empty;
            try
            {
                queryParameters = queryParameters + "meetingID=" + HttpUtility.UrlEncode(meeting.MeetingID);
                queryParameters = queryParameters + "&fullName=" + HttpUtility.UrlEncode(attendeeName);
                queryParameters = queryParameters + "&password=" + (isMeetingCreator == false ? HttpUtility.UrlEncode(BBBConstant.AttendeePassword) : HttpUtility.UrlEncode(BBBConstant.ModeratorPassword));
                string bbbMeetingBaseURL = _iConfig.GetSection(BBBConstant.BBB_BASE_URL).Value;
                return bbbMeetingBaseURL + "/join?" + queryParameters + "&checksum=" + GetCheckSumWithSecret("join" + queryParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return "";
            }
        }

        public string GetMeetingInfoURL(string meetingID)
        {
            string queryParameters = string.Empty;
            try
            {
                queryParameters = queryParameters + "meetingID=" + HttpUtility.UrlEncode(meetingID);
                queryParameters = queryParameters + "&password=" + HttpUtility.UrlEncode(BBBConstant.ModeratorPassword);
                string bbbMeetingBaseURL = _iConfig.GetSection(BBBConstant.BBB_BASE_URL).Value;
                return bbbMeetingBaseURL + "/getMeetingInfo?" + queryParameters + "&checksum=" + GetCheckSumWithSecret("getMeetingInfo" + queryParameters);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return "";
            }
        }

        public T FromXml<T>(string xml)
        {
            T returnedXmlClass = default(T);

            try
            {
                using (TextReader reader = new StringReader(xml))
                {
                    try
                    {
                        returnedXmlClass =
                            (T)new XmlSerializer(typeof(T)).Deserialize(reader);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        // String passed is not XML, simply return defaultXmlClass
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return returnedXmlClass;
        }
    }

}
