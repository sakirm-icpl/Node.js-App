using Gadget.API.Models;
using System.Collections.Generic;

public class DigitalAdoptionReview : CommonFields
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ReviewerId { get; set; }
    public int DescriptionId { get; set; }
    public int RoleId { get; set; }
    public int InvolvementLevel { get; set; }
    public int DigitalAwareness { get; set; }
    public int UseCaseKnowledge { get; set; }
    public string Remarks { get; set; }
    public bool IsActive { get; set; }
    public string code { get; set; }
}
public class UseCase : CommonFields
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}

public class DigitalRole : CommonFields
{
    public string Code { get; set; }
    public string Description { get; set; }
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

public class ImportHeaders
{
    public const string EmployeeCode = "EmployeeCode";
    public const string UseCase = "UseCase";
    public const string Role = "Role";
    public const string InvolvementLevel= "InvolvementLevel";
    public const string DigitalAwareness = "DigitalAwareness";
    public const string UseCaseKnowledge = "UseCaseKnowledge";
    public const string Remark = "Remark";
}

public class DigitalAdoptionReviewData
{
    public  string EmployeeCode { get; set; }
    public  string UseCase { get; set; }
    public string Role { get; set; }
    public  string InvolvementLevel { get; set; }
    public  string DigitalAwareness { get; set; }
    public  string UseCaseKnowledge { get; set; }
    public  string Remark  { get; set; }
    public string IsInserted { get; set; }
    public string IsUpdated { get; set; }
    public string notInsertedCode { get; set; }
    public string ErrMessage { get; set; }
}




