using Gadget.API.Models;
using System;
using System.Collections.Generic;

public class EmployeeSuggestions : CommonFields
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Suggestion { get; set; }
    public int Files { get; set; }
    public int Category { get; set; }
    public string AdditionalDescription { get; set; }
    public bool IsActive { get; set; }
}

public class FilesAndLikesCount
{
    public int Count { get; set; }

    public int SuggestionId { get; set; }

}
public class LikesList
{
    public int Id { get; set; }

    public string remark { get; set; }
    public string UserName { get; set; }

}
public class AttachedFilesListandCount
{
    public List<GetAttachedFiles> AttachedFileListandCount { get; set; }
    public int Count { get; set; }
}
public class GetAttachedFiles
{
    public int id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
}
public class EmployeeSuggestionFileV2
{
    public int Id { get; set; }
    public APIEmployeeSuggestionMerge[] aPIEmployeeSuggestionMerge { get; set; }
}

public class APIEmployeeSuggestionMerge
{
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public string FileName { get; set; }
}

public class EmployeeSuggestionFile
{
    public int Id { get; set; }
    public int CreatedBy { get; set; }
    public int ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public int SuggestionId { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public string FileName { get; set; }
}

public class EmployeeSuggestionLike : CommonFields
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public int SuggestionId { get; set; }
    public string Remarks { get; set; }
    public bool? Status { get; set; }
    public int ReviewerId { get; set; }
}
public class getfilecount
{
    public int SuggestionId { get; set; }
    public int cnt { get; set; }
}

public class SuggestionCategory : CommonFields
{
    public string Code { get; set; }
    public string SuggestionsCategory { get; set; }
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

public class AwardList : CommonFields
{
    public string Title { get; set; }
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public string FilePath { get; set; }

}
public class EmployeeAwards : CommonFields
{
    public int Id { get; set; }
    public int AwardId { get; set; }
    public int EmployeeId { get; set; }
    public string Month { get; set; }
    public int Year { get; set; }
    public string Remarks { get; set; }
    public bool IsActive { get; set; }
}
public class EmployeeAwardsGet : EmployeeAwards
{
    public string UserId { get; set; }
    public string ProjectName { get; set; }
    public string EmployeeName { get; set; }
    public string FilePath { get; set; }
    public string AwardName { get; set; }
    public string Code { get; set; }
    public string Location { get; set; }
    public string Area { get; set; }
    public string ProfilePicture { get; set; }
    public string Cluster { get; set; }
    public string Gender { get; set; }
    public int? ClusterId { get; set; }
    public int? ProjectId { get; set; }
}

public class EmployeeGroupDigitalAdoptionReview
{
    public int EmployeeId { get; set; }
    public int Total { get; set; }
    public int average { get; set; }
    public string EmployeeName { get; set; }
    public int UseCase { get; set; }
    public int Role { get; set; }
    public string UseCaseName { get; set; }
    public string RoleName { get; set; }
    public string UserId { get; set; }
    public int IlAverage { get; set; }
    public int DaAverage { get; set; }
    public int UcAverage { get; set; }
    public string ProfilePicture { get; set; }
    public string Gender { get; set; }
}

public class UserDigitalAdoptionReview
{
    public int IlAverage { get; set; }
    public int DaAverage { get; set; }
    public int UcAverage { get; set; }
}