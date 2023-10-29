// ======================================
// <copyright file="APICompetenciesMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

namespace Competency.API.APIModel.Competency
{
    public class APICompetencyLevels
    {
        public int? Id { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? SubSubCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public string LevelName { get; set; }
        public string BriefDescriptionCompetencyLevel { get; set; }
        public string DetailedDescriptionOfLevel { get; set; }
        public string Category { get; set; }
        public string Competency { get; set; }
    }
    public class APICompetencyLevelsV2
    {
        public int? Id { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? SubSubCategoryId { get; set; }
        public int CompetencyId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubSubCategoryName { get; set; }
        public string LevelName { get; set; }
        public string BriefDescriptionCompetencyLevel { get; set; }
        public string DetailedDescriptionOfLevel { get; set; }
        public string Category { get; set; }
        public string Competency { get; set; }
    }


    #region bulk upload competency levels

    public class APICompetencyLevelImport
    {
        public string Category { get; set; }
        public string Competency { get; set; }
        public string CompetencyLevel { get; set; }
        public string LevelDescription { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }

    }

    public class APICompetencyLevelImportColumns
    {
        public const string Category = "Category";
        public const string Competency = "Competency";
        public const string CompetencyLevel = "CompetencyLevel";
        public const string LevelDescription = "LevelDescription";
    }

        #endregion
    }
