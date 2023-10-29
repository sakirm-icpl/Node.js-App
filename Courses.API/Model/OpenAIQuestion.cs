using Assessment.API.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class OpenAIQuestion : CommonFields
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public string Metadata { get; set; }
        public string Industry { get; set; }

    }
}
