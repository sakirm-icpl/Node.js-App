using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APIModeratorwiseSubjectDetailsModule:BaseReportModel
    {
        public string  search { get; set; }
    }
    public class ApiModeratorwiseSubjectDeailsReport
    {
        public string Moderator { get; set; }
        public string Subject { get; set; }
        public int SessionToBeCreated { get; set; }
        public int NumberOfPPTToBeCreated { get; set; }
        public int NumberOfPPTCreated { get; set; } 
        public int PPTPercentage { get; set; }
        public int NumberOfPDFToBeCreateed { get; set; }
        public int NumberOfPDFCreated { get; set; }
        public int PDFPercentage { get; set; }
        public int NumberOfYouTubeToBeCreated { get; set; }
        public int NumberOfYouTubeCreated { get; set; }
        public int YouTubePercentage { get; set; }
        public int TotalRecordCount { get; set; }


    }

}
