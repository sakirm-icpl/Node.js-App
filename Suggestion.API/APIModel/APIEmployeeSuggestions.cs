// ======================================
// <copyright file="APIAnnouncements.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.Models;
using System.Collections.Generic;

namespace Suggestion.API.APIModel
{
    public class APIEmployeeSuggestions : CommonFields
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Suggestion { get; set; }
        public int Files { get; set; }
        public int Category { get; set; }
        public string AdditionalDescription { get; set; }
        public bool IsActive { get; set; }
    }

    public class APIEmployeeSuggestionsGet : APIEmployeeSuggestions
    {
        public string EmployeeName { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string SuggestionsCategory { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int UserId { get; set; }
        public string AreaName { get; set; }
        public string Business { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
    }
    public class APIEmployeeSuggestionsListandCount
    {
        public List<APIEmployeeSuggestionsGet> EmployeeSuggestionListandCount { get; set; }
        public int Count { get; set; }
    }

    public class APIEmployeeSuggestionAndDigitalAdoptionReview
    {

        public List<EmployeeGroupDigitalAdoptionReview> employeeGroupDigitalAdoptionReviews { get; set; }

        public List<APIEmployeeSuggestionsGet> EmployeeSuggestionListandCount { get; set; }

    }

    public class APISuggestionCategory : CommonFields
    {
        public string Code { get; set; }
        public string SuggestionsCategory { get; set; }
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
    public class APISuggestionCategoriesListandCount
    {
        public List<APISuggestionCategory> SuggestionCategoryListandCount { get; set; }
        public int Count { get; set; }
    }

    public class APIAwardList : CommonFields
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string FilePath { get; set; }

    }
    public class APIAwardListsListandCount
    {
        public List<APIAwardList> AwardListListandCount { get; set; }
        public int Count { get; set; }
    }

    public class APIEmployeeAwards : CommonFields
    {
        public int Id { get; set; }
        public int AwardId { get; set; }
        public int EmployeeId { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }

    public class APIEmployeeAwardsListandCount
    {
        public List<EmployeeAwardsGet> EmployeeAwardListandCount { get; set; }
        public int Count { get; set; }
    }
}
