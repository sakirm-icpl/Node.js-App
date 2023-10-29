using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Courses.API.Model;

namespace Course.API.Model
{

    public class ExternalTrainingRequestListandCount
    {
        public List<ExternalTrainingRequestAllUser> RequestList { get; set; }
        public int Count { get; set; }

    }

    public class ExternalTrainingRequestListandCountAllUser
    {
        public List<ExternalTrainingRequestAllUser> RequestList { get; set; }
        public int Count { get; set; }

    }
    public class ExternalTrainingRequestAllUser : ExternalTrainingRequest
    {
        public string UserName { get; set; }

        public int Days { get; set; }

        public double GST { get; set; }

        public double NetAmount { get; set; }

        public int UserId { get; set; }

    }

    public class ExternalTrainingRequestEdit : ExternalTrainingRequestAllUser
    {
        public double GST { get; set; }

        public double Converted { get; set; }

        public double Subsidy { get; set; }

        public double NetOutFlow { get; set; }

    }

    public class CurrencyConversion
    {
        public bool Success { get; set; }

        public double Result { get; set; }

    }


}
