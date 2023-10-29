// ======================================
// <copyright file="ModuleLevelPlanning.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.Collections.Generic;

namespace ILT.API.Model.AdministrativeFunctions
{
    public class ModuleLevelPlanning : BaseModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public List<ModuleLevelPlanningDetail> ModuleLevelPlanningDetails { get; set; }
    }
}
