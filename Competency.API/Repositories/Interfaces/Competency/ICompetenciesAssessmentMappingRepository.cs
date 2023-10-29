// ======================================
// <copyright file="ICompetencyLevelsRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Competency.API.APIModel.Competency;
using Competency.API.Model.Competency;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Competency.API.Repositories.Interfaces.Competency
{
    public interface ICompetenciesAssessmentMappingRepository : IRepository<AssessmentCompetenciesMapping>
    {

        Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMappingByCourse(int courseId);
        Task<bool> Exists(int AssessmentQuestionId, int comId, int? id = null);
        Task<IEnumerable<AssessmentCompetenciesMapping>> GetassessmentCompetency(int AssessmentQuestionId);
        void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId);
    }
}
