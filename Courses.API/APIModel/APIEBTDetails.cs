using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIEBTDetails
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public string courseTitle { get; set; }
        public FromData[] FromData { get; set; }
        [MaxLength(20)]
        public string Status { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class airCraftType
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool checked1 { get; set; }
    }

    public class FromData
    {
        public string rank { get; set; }
        public string staffNo { get; set; }
        public string licenceNo { get; set; }
        public airCraftType[] airCraftType { get; set; }
        public string airField { get; set; }
        public string runWay { get; set; }
        public string airCraftReg { get; set; }
        public string lowVisibilityDate { get; set; }
        public bool simulationPass { get; set; }
        public bool simulationFail { get; set; }
        public string simulationDate { get; set; }
        public string checkingExaminerName { get; set; }
        public string checkingStaffNo { get; set; }
        public string checkingLicenceNo { get; set; }
        public bool checkingExamnerSign { get; set; }
        public bool checkingRnpCompleted { get; set; }
        public bool checkingRnpNA { get; set; }
        public string checkingRnpDate { get; set; }
        public string checkingRnpExaminerName { get; set; }
        public string checkingRnpStaffNo { get; set; }
        public string checkingRnpLicenceNo { get; set; }
        public bool checkingRnpExamnerSign { get; set; }
        public bool checkingSpotYes { get; set; }
        public bool checkingSpotNo { get; set; }
        public string checkingSpotDate { get; set; }
        public string checkingSpotExaminerName { get; set; }
        public string checkingSpotStaffNo { get; set; }
        public string checkingSpotLicenceNo { get; set; }
        public bool checkingSpotExamnerSign { get; set; }
        public bool loftSpotYes { get; set; }
        public bool loftSpotNo { get; set; }
        public string loftSpotDate { get; set; }
        public string loftSpotExaminerName { get; set; }
        public string loftSpotStaffNo { get; set; }
        public string loftSpotLicenceNo { get; set; }
        public bool loftSpotExamnerSign { get; set; }
        public bool loftYes { get; set; }
        public bool loftNo { get; set; }
        public string loftDate { get; set; }
        public string loftExaminerName { get; set; }
        public string loftStaffNo { get; set; }
        public string loftLicenceNo { get; set; }
        public bool loftExamnerSign { get; set; }
        public bool captainCompleted { get; set; }
        public string captainDate { get; set; }
        public string captainExaminerName { get; set; }
        public string captainStaffNo { get; set; }
        public string captainLicenceNo { get; set; }
        public bool captainExamnerSign { get; set; }
        public bool adminCmsUodated { get; set; }
        public string admincmsName { get; set; }
        public bool adminCmsSign { get; set; }
        public string adminCmsDate { get; set; }
        public bool adminCmsUpdatedVerified { get; set; }
        public bool adminReviewCompleted { get; set; }
        public bool adminTRN { get; set; }
        public string adminDate { get; set; }
    }
}
