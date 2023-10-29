// ======================================
// <copyright file="APIInterestingArticles.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Publication.API.Models;
using Publication.API.Validation;
using System;

namespace Publication.API.APIModel
{
    public class APIInterestingArticles //: CommonFields
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        [CSVInjection]
        public string Category { get; set; }
        [CSVInjection]
        public string Article { get; set; }
        [CSVInjection]
        public string ArticleDescription { get; set; }
        public bool ShowToAll { get; set; }
        public DateTime ValidityDate { get; set; }
        public bool Status { get; set; }


        public InterestingArticles MapAPIToInterestingArticles(APIInterestingArticles aPIInterestingArticles)
        {
            InterestingArticles interestingArticles = new InterestingArticles
            {
                Id = aPIInterestingArticles.Id,
                Date = aPIInterestingArticles.Date,
                CategoryId = aPIInterestingArticles.CategoryId,
                Category = aPIInterestingArticles.Category,
                ShowToAll = aPIInterestingArticles.ShowToAll,
                Article = aPIInterestingArticles.Article,
                ArticleDescription = aPIInterestingArticles.ArticleDescription,
                ValidityDate = aPIInterestingArticles.ValidityDate,
                //interestingArticles.CreatedBy = aPIInterestingArticles.CreatedBy;
                //interestingArticles.ModifiedBy = aPIInterestingArticles.CreatedBy;
                Status = aPIInterestingArticles.Status
            };
            return interestingArticles;
        }
        public APIInterestingArticles MapInterestingArticlesToAPI(InterestingArticles interestingArticles)
        {
            APIInterestingArticles aPIInterestingArticles = new APIInterestingArticles
            {
                Id = interestingArticles.Id,
                Date = interestingArticles.Date,
                CategoryId = interestingArticles.CategoryId,
                Category = interestingArticles.Category,
                ShowToAll = interestingArticles.ShowToAll,
                Article = interestingArticles.Article,
                ArticleDescription = interestingArticles.ArticleDescription,
                ValidityDate = interestingArticles.ValidityDate,
                Status = interestingArticles.Status
            };
            return aPIInterestingArticles;
        }
    }
    public class APIInterestingArticleCategory //: CommonFields
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public bool ShowToAll { get; set; }
    }
}
