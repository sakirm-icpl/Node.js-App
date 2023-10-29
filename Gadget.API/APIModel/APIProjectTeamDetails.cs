using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.APIModel
{
    public class APIProjectTeamDetails
    {
       
        public string UserId { get; set; }
        public int? TeamMember1 { get; set; }
        public int? TeamMember2 { get; set; }
        public int? TeamMember3 { get; set; }
        public int? TeamMember4 { get; set; }
    }
    public class APIGetProjectTeam
    {
        public string ApplicationCode { get; set; }
        public string CountByUser { get; set; }
        
    }
    public class APIProjectFileforUpload
    {
        public string ApplicationCode { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
    }
    public class APISaveProjectApplication
    {
        public string ApplicationCode { get; set; }
        public string category { get; set; }
        public string timePeriod { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }
        public string afterFilePath { get; set; }
        public string beforeFilePath { get; set; }
        public string scopandplan { get; set; }
        public bool status { get; set; }
        public string kaizenCategory { get; set; }
        public string refineClassification { get; set; }
        
    }
    public class APIGetProjectAppDetails
    {
        public int Id { get; set; }
        public string ApplicationCode { get; set; }
        public string category { get; set; }
        public string timePeriod { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Answer4 { get; set; }
        public string afterFilePath { get; set; }
        public string afterFileName { get; set; }
        public string beforeFileName { get; set; }
        public string beforeFilePath { get; set; }
        public string scopandplan { get; set; }
        public string kaizenCategory { get; set; }
        public string refineClassification { get; set; }
        public bool status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string JuryStatus { get; set; }
        public int UserId { get; set; }

    }
    public class APIGetUserProjectReport
    {
        public int Id { get; set; }
        public string ApplicationCode { get; set; }
        public string StageOfJourney { get; set; }
        public string Status { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime ActualDate { get; set; }
        public string Type { get; set; }
    }
    public class APIGetFormDetails
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
   
}
