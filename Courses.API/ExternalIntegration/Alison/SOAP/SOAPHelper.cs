using Alison.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using ExternalIntegration.MetaData;

namespace Alison.SOAP
{
    public static class SOAPHelper
    {

        public static string LoginSOAPRequest(RequestLogin requestLogin)
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {
                XmlDocument soapEnvelopeXml = new XmlDocument();
                var xmlStr = helper.Login_Template(requestLogin);

                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.loginAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetAvailableCoursesRequest(int startingOffset, int numberOfCourses)
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {
                XmlDocument soapEnvelopeXml = new XmlDocument();
                var xmlStr = helper.GetCourses_Template(startingOffset, numberOfCourses);

                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.availableCourseAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.UserAgent = "EnthrallTech";

                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetMyCoursesDetailed(string email)
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {

                int userID = GetUserIDByEmail(email);
                var xmlStr = helper.GetMyCourses_Template(userID);

                XmlDocument soapEnvelopeXml = new XmlDocument();
                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.getMyCoursesDetailedAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int GetUserIDByEmail(string email)
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {
                var xmlStr = helper.GetMyUserID_Template(email);

                XmlDocument soapEnvelopeXml = new XmlDocument();
                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.myUserIDAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }

                XDocument doc = XDocument.Parse(result);
                XElement root = doc.Root;
                XNamespace ns = root.GetDefaultNamespace();

                var retString = doc.Descendants(ns + "return").FirstOrDefault().Value;

                return Convert.ToInt32(retString);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetAllAlisonCategoriesRequest()
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {
                XmlDocument soapEnvelopeXml = new XmlDocument();
                var xmlStr = helper.GetCategory_Template();

                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.getAvailableCategoriesAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetCoursesByCategoryIDRequest(int ID)
        {
            XMLHelper helper = new XMLHelper();
            Boolean useSOAP12 = false;
            try
            {
                XmlDocument soapEnvelopeXml = new XmlDocument();
                var xmlStr = helper.GetCoursesByCategory_Template(ID);

                string parms = "";
                var s = String.Format(xmlStr, ConstantValues.getAvailableCategoriesAction, new Uri(ConstantValues.url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ConstantValues.url);
                webRequest.Headers.Add("SOAPAction", ConstantValues.soapAction ?? ConstantValues.url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Sends a custom sync SOAP request to given URL and receive a request
        /// </summary>
        /// <param name="url">The WebService endpoint URL</param>
        /// <param name="action">The WebService action name</param>
        /// <param name="parameters">A dictionary containing the parameters in a key-value fashion</param>
        /// <param name="soapAction">The SOAPAction value, as specified in the Web Service's WSDL (or NULL to use the url parameter)</param>
        /// <param name="useSOAP12">Set this to TRUE to use the SOAP v1.2 protocol, FALSE to use the SOAP v1.1 (default)</param>
        /// <returns>A string containing the raw Web Service response</returns>
        public static string SendSOAPRequest(string url, string action, Dictionary<string, string> parameters, string soapAction = null, bool useSOAP12 = false)
        {
            try
            {
                // Create the SOAP envelope
                XmlDocument soapEnvelopeXml = new XmlDocument();
                var xmlStr = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <SOAP-ENV:Envelope SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""
                            xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""
                            xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                            xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"">
                            <SOAP-ENV:Header>
		                            <credentials>
			                            <alisonOrgId>Eh73uDza9fYZV1Y3Tad7</alisonOrgId>
			                            <alisonOrgKey>FrFB8SbcMaE9q2UPWlJv</alisonOrgKey>
		                            </credentials>
	                            </SOAP-ENV:Header>
                            <SOAP-ENV:Body>
		                            <q1:login xmlns:q1=""urn:alisonwsdl"">
			                            <email xsi:type = ""xsd:string"">Pankaj.kshirsagar@enthralltech.com</email>
			                            <firstname xsi:type = ""xsd:string"">Pankaj</firstname>
			                            <lastname xsi:type = ""xsd:string"">Kshirsagar</lastname>
			                            <city xsi:type = ""xsd:string"">Pune</city>
			                            <country xsi:type = ""xsd:string"">IN</country>
			                            <external_id xsi:type = ""xsd:string"">PHK473</external_id>
		                            </q1:login>
                            </SOAP-ENV:Body>
                            </SOAP-ENV:Envelope>";

                string parms = ""; // string.Join(string.Empty, parameters.Select(kv => String.Format("<{0}>{1}</{0}>", kv.Key, kv.Value)).ToArray());
                var s = String.Format(xmlStr, action, new Uri(url).GetLeftPart(UriPartial.Authority) + "/", parms);
                soapEnvelopeXml.LoadXml(s);

                // Create the web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Headers.Add("SOAPAction", soapAction ?? url);
                webRequest.ContentType = (useSOAP12) ? "application/soap+xml;charset=\"utf-8\"" : "text/xml;charset=\"utf-8\"";
                webRequest.Accept = (useSOAP12) ? "application/soap+xml" : "text/xml";
                webRequest.Method = "POST";

                // Insert SOAP envelope
                using (Stream stream = webRequest.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                // Send request and retrieve result
                string result;
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        result = rd.ReadToEnd();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
