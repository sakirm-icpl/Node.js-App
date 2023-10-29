// ======================================
// <copyright file="APICompetenciesMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

namespace MyCourse.API.APIModel.Competency
{
    public class APICompetenciesMaster 
    {
        public int? Id { get; set; }
        public int? CategoryId { get; set; }
        public string? SubcategoryName { get; set; }
        public int? SubcategoryId { get; set; }
        public string? SubSubcategoryName { get; set; }
        public int? SubSubCategoryId { get; set; }
        public string CompetencyName { get; set; }
        public string Category { get; set; }
        public string CompetencyDescription { get; set; }
        public bool IsDeleted { get; set; }
    }


    #region Bulk Upload Competency Master
    public class APICompetencyMaster
    {
        public string CompetencyName { get; set; }
        public string CompetencyDescription { get; set; }
        public string Category { get; set; }
    }

    public class APICompetencyMasterImport
    {
        public string CompetencyName { get; set; }
        public string CompetencyDescription { get; set; }
        public string Category { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }


    }

    public class APICompetencyMasterImportColumns
    {
        public const string CompetencyName = "CompetencyName";
        public const string CompetencyDescription = "CompetencyDescription";
        public const string Category = "Category";
    }

    #endregion
}
