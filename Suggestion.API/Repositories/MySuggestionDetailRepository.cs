// ======================================
// <copyright file="MySuggestionDetailRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Suggestion.API.Data;
using Suggestion.API.Models;
using Suggestion.API.Repositories.Interfaces;

namespace Suggestion.API.Repositories
{
    public class MySuggestionDetailRepository : Repository<MySuggestionDetail>, IMySuggestionDetailRepository
    {
        private GadgetDbContext db;
        public MySuggestionDetailRepository(GadgetDbContext context) : base(context)
        {
            this.db = context;
        }
    }
}
