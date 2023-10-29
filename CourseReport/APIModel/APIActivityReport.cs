using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseReport.API.APIModel
{
    public class APIActivityReport
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string UserStatus { get; set; }
        public string FirstLoginDate { get; set; }
        public string FirstContentAccess { get; set; }
        public string LastContentAccess { get; set; }
        public string CourseAssigned { get; set; }
        public string CourseCompleted { get; set; }
        public string CourseInprogress { get; set; }
        public string Completion { get; set; }
        public string LastActiveDate { get; set; }
        public string DeviceMostActive { get; set; }
        public string TotalSignIns { get; set; }
        public string WebSignIns { get; set; }
        public string AppSignIns { get; set; }
        public string TotalTimeSpentInMinutes { get; set; }
        public string WebTimeSpentInMinutes { get; set; }
        public string AppTimeSpentInMinutes { get; set; }
        public string TotalNoofPageviews { get; set; }
        public string TotalPointEarned { get; set; }
        public string CertificateEarned { get; set; }
        public string DateOfJoining { get; set; }
        public string Business { get; set; }
        public string Group { get; set; }
        public string Area { get; set; }
        public string Location { get; set; }
        public string Configurationcolumn1 { get; set; }
        public string Configurationcolumn2 { get; set; }
        public string Configurationcolumn3 { get; set; }
        public string Configurationcolumn4 { get; set; }
        public string Configurationcolumn5 { get; set; }
        public string Configurationcolumn6 { get; set; }
        public string Configurationcolumn7 { get; set; }
        public string Configurationcolumn8 { get; set; }
        public string Configurationcolumn9 { get; set; }
        public string Configurationcolumn10 { get; set; }
        public string Configurationcolumn11 { get; set; }
        public string Configurationcolumn12 { get; set; }
        public string Configurationcolumn13 { get; set; }
        public string Configurationcolumn14 { get; set; }
        public string Configurationcolumn15 { get; set; }
    }

    public class APIUserSetting
    {
        public int Id { get; set; }
        public string ConfiguredColumnName { get; set; }
        public string ChangedColumnName { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsShowInReport { get; set; }
        public bool IsShowInAnalyticalDashboard { get; set; }
        public string ConfiguredColumnModel { get; set; }
        public string FieldType { get; set; }
        public bool IsBuddyConfiguredOn { get; set; }
    }
}
