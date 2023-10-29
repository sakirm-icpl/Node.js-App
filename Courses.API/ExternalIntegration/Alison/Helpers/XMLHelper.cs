using System;
using System.Collections.Generic;
using System.Text;
using ExternalIntegration.MetaData;

namespace Alison.Helpers
{
    public class XMLHelper
    {
        private string GetRequestTemplate()
        {
            try
            {
                string xmlTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <SOAP-ENV:Envelope SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""
                            xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""
                            xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                            xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"">
                            <SOAP-ENV:Header>
		                            <credentials>
			                            <alisonOrgId>[alisonOrgId]</alisonOrgId>
			                            <alisonOrgKey>[alisonOrgKey]</alisonOrgKey>
		                            </credentials>
	                            </SOAP-ENV:Header>
                            <SOAP-ENV:Body>
		                            [REQ_VAL]
                            </SOAP-ENV:Body>
                            </SOAP-ENV:Envelope>";

                xmlTemplate = xmlTemplate.Replace("[alisonOrgId]", ConstantValues.alisonOrgId);
                xmlTemplate = xmlTemplate.Replace("[alisonOrgKey]", ConstantValues.alisonOrgKey);

                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        public string Login_Template(RequestLogin login)
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();

                string LoginTemplate = @"<q1:login xmlns:q1=""urn:alisonwsdl"">
			                            <email xsi:type = ""xsd:string"">[EMAIL]</email>
			                            <firstname xsi:type = ""xsd:string"">[FNAME]</firstname>
			                            <lastname xsi:type = ""xsd:string"">[LNAME]</lastname>
			                            <city xsi:type = ""xsd:string"">[CITY]</city>
			                            <country xsi:type = ""xsd:string"">[COUNTRY]</country>
			                            <external_id xsi:type = ""xsd:string"">[EXTID]</external_id>
		                            </q1:login>";
                
                LoginTemplate = LoginTemplate.Replace("[EMAIL]", login.email);
                LoginTemplate = LoginTemplate.Replace("[FNAME]", login.firstname);
                LoginTemplate = LoginTemplate.Replace("[LNAME]", login.lastname);
                LoginTemplate = LoginTemplate.Replace("[CITY]", login.city);
                LoginTemplate = LoginTemplate.Replace("[COUNTRY]", login.country);
                LoginTemplate = LoginTemplate.Replace("[EXTID]", login.external_id);

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", LoginTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCourses_Template(int startingOffset, int numberOfCourses)
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();

                string CourseTemplate = @"<q1:getAvailableCourses xmlns:q1=""urn:alisonwsdl"">
	                                        <offset xsi:type=""xsd:int"">[STARTINGOFFSET]</offset>
	                                        <count xsi:type=""xsd:int"">[NUMBEROFCOURSES]</count>
                                        </q1:getAvailableCourses>";

                CourseTemplate = CourseTemplate.Replace("[STARTINGOFFSET]", startingOffset.ToString());
                CourseTemplate = CourseTemplate.Replace("[NUMBEROFCOURSES]", numberOfCourses.ToString());

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", CourseTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetMyCourses_Template(int alisonUserID)
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();
                string CourseTemplate = @"<q1:getMyCoursesDetailed xmlns:q1=""urn:alisonwsdl"">
	                                        <userid xsi:type=""xsd:int"">[USERID]</userid>
                                        </q1:getMyCoursesDetailed>";

                CourseTemplate = CourseTemplate.Replace("[USERID]", alisonUserID.ToString());

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", CourseTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetMyUserID_Template(string emailId)
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();
                string CourseTemplate = @"<q1:getUserId xmlns:q1=""urn:alisonwsdl"">
	                                        <email xsi:type=""xsd:string"">[emailId]</email>
                                          </q1:getUserId>";

                CourseTemplate = CourseTemplate.Replace("[emailId]", emailId);

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", CourseTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCategory_Template()
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();
                string CategoryTemplate = @"<q1:getAvailableCategories xmlns:q1=""urn:alisonwsdl"">
                                        </q1:getAvailableCategories>";

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", CategoryTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCoursesByCategory_Template(int ID)
        {
            try
            {
                string xmlTemplate = GetRequestTemplate();
                string CategoryTemplate = @"<q1:getCoursesByCategory xmlns:q1=""urn:alisonwsdl"">
                                            <categoryid xsi:type=""xsd:int"">[CATEGORYID]</categoryid>
                                            </q1:getCoursesByCategory>";

                CategoryTemplate = CategoryTemplate.Replace("[CATEGORYID]", ID.ToString());

                xmlTemplate = xmlTemplate.Replace("[REQ_VAL]", CategoryTemplate);
                return xmlTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
