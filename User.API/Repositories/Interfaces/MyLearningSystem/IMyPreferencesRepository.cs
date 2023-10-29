//======================================
// <copyright file="IMyPreferencesRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using System.Collections.Generic;
using System.Threading.Tasks;
using User.API.APIModel.MyLearningSystem;
using User.API.Models.MyLearningSystem;

namespace User.API.Repositories.Interfaces.MyLearningSystem
{
    public interface IMyPreferencesRepository : IRepository<MyPreferences>
    {
        Task<bool> Exist(int userId);
        Task<APIMyPreferences> GetMyPreferenceByToken(int userid);
        Task<bool> IsLanguageExists(string Language);
        Task<List<MyPreferenceConfiguration>> GetLandingPages();
        Task<List<MyPreferenceConfiguration>> UpdateLandingPageConfiguration(List<MyPreferenceConfiguration> myPreferenceConfigurations);
    }

}
