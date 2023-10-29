// ======================================
// <copyright file="APIAnnouncements.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Gadget.API.Models;
using System.Collections.Generic;

namespace Gadget.API.APIModel
{
    public class APIDigitalAdoptionReview : CommonFields
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
        public string code { get; set; }

    }

    public class APIDigitalAdoptionReviewsListandCount
    {
        public List<APIDigitalAdoptionReviewGet> DigitalAdoptionReviewListandCount { get; set; }
        public int Count { get; set; }


    }

    public class APIDigitalAdoptionReviewGet : APIDigitalAdoptionReview
    {
        public string EmployeeName { get; set; }
        public string ReviewerName { get; set; }
        public string UseCaseDescription { get; set; }
        public string UserRoleDescription { get; set; }
    }
    public class APIUseCase : CommonFields
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }


    }

    public class APIUseCasesListandCount
    {
        public List<APIUseCase> UseCaseListandCount { get; set; }
        public int Count { get; set; }


    }

    public class APIDigitalRole : CommonFields
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public bool IsActive { get; set; }

    }

    public class APIDigitalRolesListandCount
    {
        public List<APIDigitalRole> DigitalRoleListandCount { get; set; }
        public int Count { get; set; }


    }

    public class APIFilter
    {
        public string Top5 { get; set; }
        public int? Cluster { get; set; }
        public int? Project { get; set; }
        public int? UseCase { get; set; }
        public int?[] Role { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? Category { get; set; }
        public int? RoleId { get; set; }
    }
    public class APIDigitalAdoptionReviewDashBoard
    {
        public List<EmployeeGroupDigitalAdoptionReview> IlTop5List { get; set; }
        public List<EmployeeGroupDigitalAdoptionReview> DaTop5List { get; set; }
        public List<EmployeeGroupDigitalAdoptionReview> UcTop5List { get; set; }
    }

    public class APIDARImport
    {
        public string Path { get; set; }
    }

}
