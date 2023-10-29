// ======================================
// <copyright file="ICompetencyLevelsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Assessment.API.Model.Competency;
using Assessment.API.Repositories.Interfaces;

namespace Assessment.API.Repositories.Interfaces.Competency
{
    public interface ICompetenciesAssessmentMappingRepository : IRepository<AssessmentCompetenciesMapping>
    {

        

        Task<bool> Exists(int AssessmentQuestionId, int comId, int? id = null);
       
    }
}
