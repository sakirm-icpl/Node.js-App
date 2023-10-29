// ======================================
// <copyright file="APIMySuggestion.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;

namespace Suggestion.API.APIModel
{
    public class APIMySuggestion //: CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string SuggestionBrief { get; set; }
        public string ContextualAreaofBusiness { get; set; }
        public string DetailedDescription { get; set; }
        public string Status { get; set; }
        public string ContentDescription { get; set; }
    }
    public class APIMySuggestionDetail //: CommonFields
    {
        public int? Id { get; set; }
        public int SuggestionId { get; set; }
        public string FilePath { get; set; }
        public string ContentDescription { get; set; }
        public string FileType { get; set; }
    }
    public class APIMySuggestionMerge //: CommonFields
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string SuggestionBrief { get; set; }
        public string ContextualAreaofBusiness { get; set; }
        public string DetailedDescription { get; set; }
        public string Status { get; set; }
        public APIMySuggestionDetail[] aPIMySuggestionDetail { get; set; }
    }

    public class SearchDetails
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? SuggestionId { get; set; }
        public int? CourseId { get; set; }
        public int? UserId { get; set; }
        public string Status { get; set; }
        public string ColumnName { get; set; }
        public string SearchText { get; set; }
    }
}
