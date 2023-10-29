using System;

namespace Competency.API.APIModel
{
    public class APIProgressStatusCountData
    {
        public int InprogressCount { get; set; }
        public int CompletedCount { get; set; }
        public int NotStartedCount { get; set; }
        public double InprogressCountPercentage { get; set; }
        public double CompletedCountPercentage { get; set; }
        public double NotStartedCountPercentage { get; set; }
        public DateTime LastRefreshedDate { get; set; }
    }
}
