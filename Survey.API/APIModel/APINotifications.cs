using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APINotifications
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public string RowGuid { get; set; }

        public int? QuizId { get; set; }
        public int? SurveyId { get; set; }
    }
}
