using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APISurveyQuestionMergeredForMultiple
    {
        public int? Id { get; set; }
        public string Section { get; set; }
        public string Question { get; set; }
        public int SurveyId { get; set; }
        public bool AllowSkipAswering { get; set; }
        public bool Status { get; set; }
        public APISurveyOption[] aPISurveyOption { get; set; }
        public string OptionType { get; set; }
        public int AverageRespondTime { get; set; }

        public bool IsMultipleChoice { get; set; }

    }
}
