// ======================================
// <copyright file="MySuggestionDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Suggestion.API.Models
{
    public class MySuggestionDetail : CommonFields
    {
        public int Id { get; set; }
        public int SuggestionId { get; set; }
        [MaxLength(2000)]
        public string FilePath { get; set; }
        [MaxLength(3000)]
        public string ContentDescription { get; set; }
        public string FileType { get; set; }
        public MySuggestion MySuggestions { get; set; }
    }
}
