using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assessment.API.APIModel.Refactored
{
    public class QuestionAnswerAssessement
    {
        public int Marks { get; set; }
        public int QuestionID { get; set; }
        public int AnswerId { get; set; }
        public Boolean IsCorrectAnswer { get; set; }
        public string? OptionType { get; set; }

    }
}
