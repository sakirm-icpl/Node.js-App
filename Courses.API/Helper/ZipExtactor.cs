using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Courses.API.Helper;
using log4net;

namespace Courses.API.Helper
{
    public class ZipExtactor
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ZipExtactor));
        public void UnzipFile(string extractPath, string zipFilePath)
        {
            try
            {
                if (extractPath.Length > 0)
                {
                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }
                    Directory.CreateDirectory(extractPath);
                }
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
        }

        public string GetRelativePath(string extractPath, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories)
                                     .Where(s => s.Contains(fileName));
                return files.FirstOrDefault();
            }
            else
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("Imsmanifest") || s.Contains("imsmanifest"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                    string startPageFromIMSManifest = (xmldoc.GetElementsByTagName("resource"))[0].Attributes["href"].Value;
                    if (startPageFromIMSManifest == "HTML5/player/APIWrapper.js")
                    {
                        return GetRelativePathDynamic(extractPath, fileName);
                    }
                        if (!string.IsNullOrEmpty(startPageFromIMSManifest))
                    {
                        return string.Format("{0}\\{1}", extractPath, startPageFromIMSManifest);
                    }
                    
                }
            }
            return string.Empty;

        }
        public string GetRelativePath_CMI5(string extractPath, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories)
                                     .Where(s => s.Contains(fileName));
                return files.FirstOrDefault();
            }
            else
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("cmi5") || s.Contains("cmi5"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                    var node = xmldoc.GetElementsByTagName("url")[0];
                    string startPageFromIMSManifest = node.ChildNodes[0].InnerText.ToString();//"res/index.html";//(xmldoc.GetElementsByTagName("url")[0].ChildNodes[0].InnerXml);
                    if (!string.IsNullOrEmpty(startPageFromIMSManifest))
                    {
                        return string.Format("{0}\\{1}", extractPath, startPageFromIMSManifest);
                    }

                }
            }
            return string.Empty;

        }
    
        public string GetRelativePathDynamic(string extractPath, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories)
                                     .Where(s => s.Contains(fileName));
                return files.FirstOrDefault();
            }
            else
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("Imsmanifest") || s.Contains("imsmanifest"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                    string path;
                    string startPageFromIMSManifest = (xmldoc.GetElementsByTagName("organizations"))[0].Attributes["default"].Value;
                    if (startPageFromIMSManifest == "tuto")
                    {
                        path = "HTML5/tutorial.html";
                        if (!string.IsNullOrEmpty(path))
                        {
                            return string.Format("{0}\\{1}", extractPath, path);
                        }
                    }
                    else if (startPageFromIMSManifest == "demo")
                    {
                        path = "HTML5/demo.html";
                        if (!string.IsNullOrEmpty(path))
                        {
                            return string.Format("{0}\\{1}", extractPath, path);
                        }
                    }
                    else if (startPageFromIMSManifest == "test")
                    {
                        path = "HTML5/test.html";
                        if (!string.IsNullOrEmpty(path))
                        {
                            return string.Format("{0}\\{1}", extractPath, path);
                        }
                    }
                    else if (startPageFromIMSManifest == "prac")
                    {
                        path = "HTML5/practice.html";
                        if (!string.IsNullOrEmpty(path))
                        {
                            return string.Format("{0}\\{1}", extractPath, path);
                        }
                    }
                    else
                    {
                        path = "HTML5/demo.html";
                        if (!string.IsNullOrEmpty(path))
                        {
                            return string.Format("{0}\\{1}", extractPath, path);
                        }
                    }

                }
            }
            return string.Empty;

        }

        public string GetLaunchData(string extractPath)
        {
            
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("Imsmanifest") || s.Contains("imsmanifest"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                     XmlNodeList datafromlmsList = (xmldoc.GetElementsByTagName("adlcp:datafromlms"));
                if (datafromlmsList.Count > 0)
                {
                    string datafromlms = (xmldoc.GetElementsByTagName("adlcp:datafromlms"))[0].InnerXml;

                    if (!string.IsNullOrEmpty(datafromlms))
                    {
                        return string.Format("{0}", datafromlms);
                    }
                }
                }
            
            return string.Empty;

        }

        public string GetxAPIData(string extractPath)
        {
            try
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("Tincan") || s.Contains("tincan"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                    XmlNodeList datafromlmsList = (xmldoc.GetElementsByTagName("tincan"));
                    if (datafromlmsList.Count > 0)
                    {
                        string activityId = (xmldoc.GetElementsByTagName("activity"))[0].Attributes["id"].Value;

                        if (!string.IsNullOrEmpty(activityId))
                        {
                            return string.Format("{0}", activityId);
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return string.Empty;
            }

        }
        public string Getcmi5ActivityId(string extractPath)
        {
            try
            {
                var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("cmi5") || s.Contains("cmi5"));

                if (files.Count() > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(files.First());
                    XmlNodeList datafromlmsList = (xmldoc.GetElementsByTagName("courseStructure"));
                    if (datafromlmsList.Count > 0)
                    {
                        string activityId = (xmldoc.GetElementsByTagName("course"))[0].Attributes["id"].Value;

                        if (!string.IsNullOrEmpty(activityId))
                        {
                            return string.Format("{0}", activityId);
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return string.Empty;
            }

        }

        public string GetxAPIDataPath(string extractPath)
        {

            var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Where(s => s.Contains("Tincan") || s.Contains("tincan"));

            if (files.Count() > 0)
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(files.First());
                XmlNodeList datafromlmsList = (xmldoc.GetElementsByTagName("tincan"));
                if (datafromlmsList.Count > 0)
                {
                    string startPage = (xmldoc.GetElementsByTagName("launch"))[0].InnerXml;                   

                    if (!string.IsNullOrEmpty(startPage))
                    {
                        return string.Format("{0}", startPage);
                    }
                }
            }

            return string.Empty;

        }
        private static string ModifiedLaunchCourseFileContent(string fileContent)
        {
            StringBuilder strBuilder = new StringBuilder(fileContent);
            if (fileContent.Contains("</head>"))
                strBuilder.Replace("</head>", getScript());
            else
                strBuilder.Replace("</HEAD>", getScript());
            return ModifiedOnUnloadMethod(strBuilder.ToString());

        }
        private static string ModifiedOnUnloadMethod(string fileContent)
        {
            StringBuilder strFileContent = new StringBuilder(fileContent);

            if (fileContent.Contains("<body"))
                strFileContent.Replace("<body", "<body  onUnLoad =refreshParentWindow();  ");
            else
                strFileContent.Replace("<BODY", "<BODY  onUnLoad =refreshParentWindow();  ");

            return strFileContent.ToString();
        }
        private static string getScript()
        {
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language=\"javascript\"> function refreshParentWindow() { if(window.opener!=null && !window.opener.closed) { window.opener.location.reload();} }   </script> </head> ");
            return strScript.ToString();
        }
    }
}
