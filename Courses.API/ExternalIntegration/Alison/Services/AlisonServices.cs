using Alison.SOAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ExternalIntegration.MetaData;
using ExternalIntegration.Services.Interfaces;

namespace ExternalIntegration.Services
{
    public class AlisonServices : IAlisonServices
    {
        //public readonly EndpointAddress endpointAddress;
        //public readonly BasicHttpsBinding basicHttpsBinding;
        //public readonly string alisonOrgIdName = "alisonOrgId";
        //public readonly string alisonOrgKeyName = "alisonOrgKey";
        //public readonly string alisonOrgIdValue = "Eh73uDza9fYZV1Y3Tad7";
        //public readonly string alisonOrgKeyValue = "FrFB8SbcMaE9q2UPWlJv";

        public AlisonServices()
        {

            //HttpRequestMessageProperty reqProps = new HttpRequestMessageProperty();
            //reqProps.Headers.Add(alisonOrgIdName, alisonOrgIdValue);
            //reqProps.Headers.Add(alisonOrgKeyName, alisonOrgKeyValue);

            //System.ServiceModel.Channels.AddressHeader[] addressHeaders = new System.ServiceModel.Channels.AddressHeader[2];
            //addressHeaders[0] = System.ServiceModel.Channels.AddressHeader.CreateAddressHeader(alisonOrgIdName, "http://schemas.com", alisonOrgIdValue);
            //addressHeaders[1] = System.ServiceModel.Channels.AddressHeader.CreateAddressHeader(alisonOrgKeyName, "http://schemas.com", alisonOrgKeyValue);
            
            //Uri myUri = new Uri(serviceUrl, UriKind.Absolute);

            //endpointAddress = new EndpointAddress(myUri, addressHeaders);

            //basicHttpsBinding = new BasicHttpsBinding();

            //basicHttpsBinding.OpenTimeout = TimeSpan.MaxValue;
            //basicHttpsBinding.CloseTimeout = TimeSpan.MaxValue;
            //basicHttpsBinding.ReceiveTimeout = TimeSpan.MaxValue;
            //basicHttpsBinding.SendTimeout = TimeSpan.MaxValue;

        }
        public string GetLoginAsync(RequestLogin requestLogin)
        {
            try
            {
                var parms = new Dictionary<string, string>();
                string result = SOAPHelper.LoginSOAPRequest(requestLogin);

                XDocument doc = XDocument.Parse(result);
                XElement root = doc.Root;
                XNamespace ns = root.GetDefaultNamespace();

                var retString = doc.Descendants(ns + "return").FirstOrDefault().Value;
                retString = "https://alison.com/login/external.php?" + retString;
                return retString;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<APIAlisonCoursesDetails> GetCourseDetails(string result)
        {
            List<APIAlisonCoursesDetails> aPICourses = new List<APIAlisonCoursesDetails>();
            try
            {
                XDocument doc = XDocument.Parse(result);
                XElement root = doc.Root;
                XNamespace ns = root.GetDefaultNamespace();

                var retNew = doc.Descendants(ns + "return").Elements(ns + "item").ToList();

                foreach (XElement mainEllement in retNew)
                {
                    var elementList = mainEllement.Elements();
                    APIAlisonCoursesDetails coursesDetails = new APIAlisonCoursesDetails();
                    foreach (XElement childEllement in elementList)
                    {
                        switch (childEllement.Name.LocalName.ToLower())
                        {
                            case "id":
                                if(childEllement.Value !="")
                                    coursesDetails.id = Convert.ToInt32(childEllement.Value);
                                break;
                            case "category":
                                if (childEllement.Value != "")
                                    coursesDetails.category = Convert.ToInt32(childEllement.Value);
                                break;
                            case "categoryname":
                                coursesDetails.categoryname = childEllement.Value;
                                break;
                            case "release_date":
                                coursesDetails.release_date = childEllement.Value;
                                break;
                            case "fullname":
                                coursesDetails.fullname = childEllement.Value;
                                break;
                            case "headline":
                                coursesDetails.headline = childEllement.Value;
                                break;
                            case "image":
                                coursesDetails.image = childEllement.Value;
                                break;
                            case "coursename":
                                coursesDetails.coursename = childEllement.Value;
                                break;
                            case "courselink":
                                {
                                    coursesDetails.courselink = childEllement.Value;
                                    coursesDetails.shortName = childEllement.Value.Replace("https://alison.com/login/external.php?idcourse=", "");
                                    break;
                                }
                            case "coursestate":
                                coursesDetails.coursestate = childEllement.Value;
                                break;
                            case "coursevalue":
                                try
                                {
                                    string cv = childEllement.Value;
                                    coursesDetails.coursevalue = childEllement.Value;
                                    if (cv.ToUpper().Contains("QUIZZES"))
                                        coursesDetails.courseStatus = "Completed";
                                    else if (cv == "" || cv == "0")
                                        coursesDetails.courseStatus = "Not Started";
                                    else
                                        coursesDetails.courseStatus = "In Progress";
                                }
                                catch (Exception)
                                {
                                    coursesDetails.coursevalue = "0";
                                    coursesDetails.courseStatus = "Not Started";
                                }
                                break;
                            case "firstaccess":
                                coursesDetails.firstaccess = childEllement.Value;
                                break;
                            case "lastaccess":
                                coursesDetails.lastaccess = childEllement.Value;
                                break;
                            case "totaltimespent":
                                coursesDetails.totaltimespent = childEllement.Value;
                                break;
                            case "scores":
                                coursesDetails.scores = childEllement.Value;
                                break;
                            case "fullname_en":
                                coursesDetails.fullname_en = childEllement.Value;
                                break;
                            case "fullname_ar":
                                coursesDetails.fullname_ar = childEllement.Value;
                                break;
                            case "fullname_fr":
                                coursesDetails.fullname_fr = childEllement.Value;
                                break;
                            case "summary_en":
                                coursesDetails.summary_en = childEllement.Value;
                                break;
                            case "summary_ar":
                                coursesDetails.summary_ar = childEllement.Value;
                                break;
                            case "summary_fr":
                                coursesDetails.summary_en = childEllement.Value;
                                break;
                        }
                        Console.WriteLine(childEllement);
                    }
                    aPICourses.Add(coursesDetails);
                }
                return aPICourses;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<APIAlisonCoursesDetails> GetAvailableCourses(int offset, int count)
        {
            try
            {
                string result = SOAPHelper.GetAvailableCoursesRequest(offset, count);
                return GetCourseDetails(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<APIAlisonCoursesDetails>> GetMyCoursesDetailed(string emailID)
        {
            try
            {
                string result = SOAPHelper.GetMyCoursesDetailed(emailID);

                List<APIAlisonCoursesDetails> aPICourses = new List<APIAlisonCoursesDetails>();

                XDocument doc = XDocument.Parse(result);
                XElement root = doc.Root;
                XNamespace ns = root.GetDefaultNamespace();

                var retNew = doc.Descendants(ns + "return").Elements(ns + "item").ToList();

                foreach (XElement mainEllement in retNew)
                {
                    var elementList = mainEllement.Elements();
                    APIAlisonCoursesDetails coursesDetails = new APIAlisonCoursesDetails();
                    foreach (XElement childEllement in elementList)
                    {
                        switch (childEllement.Name.LocalName.ToLower())
                        {
                            case "id":
                                {
                                    if (childEllement.Value != string.Empty)
                                        coursesDetails.id = Convert.ToInt32(childEllement.Value);
                                    break;
                                }
                            case "category":
                                {
                                    if (childEllement.Value != string.Empty)
                                        coursesDetails.category = Convert.ToInt32(childEllement.Value);
                                    break;
                                }
                            case "categoryname":
                                coursesDetails.categoryname = childEllement.Value;
                                break;
                            case "release_date":
                                coursesDetails.release_date = childEllement.Value;
                                break;
                            case "fullname":
                                coursesDetails.fullname = childEllement.Value;
                                break;
                            case "headline":
                                coursesDetails.headline = childEllement.Value;
                                break;
                            case "image":
                                coursesDetails.image = childEllement.Value;
                                break;
                            case "coursename":
                                coursesDetails.coursename = childEllement.Value;
                                break;
                            case "courselink":
                                {
                                    coursesDetails.courselink = childEllement.Value;
                                    coursesDetails.shortName = childEllement.Value.Replace("https://alison.com/login/external.php?idcourse=", "");
                                    break;
                                }
                            case "coursestate":
                                coursesDetails.coursestate = childEllement.Value;
                                break;
                            case "coursevalue":
                                try
                                {
                                    string cv = childEllement.Value;
                                    coursesDetails.coursevalue = childEllement.Value;
                                }
                                catch (Exception)
                                {
                                    coursesDetails.coursevalue = "0";
                                }
                                break;
                            case "course_status":
                                coursesDetails.courseStatus= childEllement.Value;
                                break;
                            case "firstaccess":
                                if (childEllement.Value != "")
                                {
                                    coursesDetails.firstaccess = UnixTimeStampToDateTime(Convert.ToDouble(childEllement.Value)).ToString();
                                }
                                break;
                            case "lastaccess":
                                if (childEllement.Value != "")
                                {
                                    coursesDetails.lastaccess = UnixTimeStampToDateTime(Convert.ToDouble(childEllement.Value)).ToString();
                                }
                                break;
                            case "enrollment_date":
                                if (childEllement.Value != "")
                                {
                                    coursesDetails.enrollment_date = UnixTimeStampToDateTime(Convert.ToDouble(childEllement.Value)).ToString();
                                }
                                break;
                            case "totaltimespent":
                                coursesDetails.totaltimespent = childEllement.Value;
                                break;
                            case "scores":
                                coursesDetails.scores = childEllement.Value;
                                break;
                            case "fullname_en":
                                coursesDetails.fullname_en = childEllement.Value;
                                break;
                            case "fullname_ar":
                                coursesDetails.fullname_ar = childEllement.Value;
                                break;
                            case "fullname_fr":
                                coursesDetails.fullname_fr = childEllement.Value;
                                break;
                            case "summary_en":
                                coursesDetails.summary_en = childEllement.Value;
                                break;
                            case "summary_ar":
                                coursesDetails.summary_ar = childEllement.Value;
                                break;
                            case "summary_fr":
                                coursesDetails.summary_en = childEllement.Value;
                                break;
                        }
                        Console.WriteLine(childEllement);
                    }
                    aPICourses.Add(coursesDetails);
                }
                return aPICourses;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<List<APIAlisonCourses>> GetMyCourses(string emailID)
        //{
        //    try
        //    {
        //        List<APIAlisonCourses> aPICourses = new List<APIAlisonCourses>();
        //        var client = await GetInstanceAsync();
        //        if (client.InnerChannel.State != System.ServiceModel.CommunicationState.Faulted)
        //        {

        //            int UserID = await client.getUserIdAsync(emailID);

        //            if (UserID > 0)
        //            {
        //                courses[] courses = await client.getMyCoursesAsync(UserID);
        //                if (courses != null)
        //                {
        //                    for (int i = 0; i <= courses.Length - 1; i++)
        //                    {
        //                        aPICourses.Add(new APIAlisonCourses
        //                        {
        //                            courselink = courses[i].courselink,
        //                            coursename = courses[i].coursename,
        //                        });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception("User not found in external system.");
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Fault execption.");
        //        }
        //        return aPICourses;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public Task<string> GetLogin(RequestLogin requestLogin)
        {
            throw new NotImplementedException();
        }

        public Task<List<APIAlisonCourses>> GetMyCourses(string emailID)
        {
            throw new NotImplementedException();
        }

        public List<APIAlisonCategory> GetAllAlisonCategories()
        {
            List<APIAlisonCategory> aPICategory = new List<APIAlisonCategory>();
            try
            {
                string result = SOAPHelper.GetAllAlisonCategoriesRequest();
                XDocument doc = XDocument.Parse(result);
                XElement root = doc.Root;
                XNamespace ns = root.GetDefaultNamespace();

                var retNew = doc.Descendants(ns + "return").Elements(ns + "item").ToList();

                foreach (XElement mainEllement in retNew)
                {
                    var elementList = mainEllement.Elements();
                    APIAlisonCategory category = new APIAlisonCategory();
                    foreach (XElement childEllement in elementList)
                    {
                        switch (childEllement.Name.LocalName.ToLower())
                        {
                            case "id":
                                category.ID = Convert.ToInt32(childEllement.Value);
                                break;
                            case "name":
                                category.CategoryName = childEllement.Value;
                                break;
                        }
                    }
                    aPICategory.Add(category);
                }
                return aPICategory;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<APIAlisonCoursesDetails> GetCoursesByCategoryID(int ID)
        {
            try
            {
                string result = SOAPHelper.GetCoursesByCategoryIDRequest(ID);
                return GetCourseDetails(result);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
