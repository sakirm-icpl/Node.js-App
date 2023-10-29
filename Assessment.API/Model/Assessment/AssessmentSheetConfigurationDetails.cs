using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Models
{
    [Table("AssessmentSheetConfigurationDetails", Schema = "Course")]

    public class AssessmentSheetConfigurationDetails : CommonFields
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public int AssessmentSheetConfigID { get; set; }
        public int? SequenceNumber { get; set; }
    }
}
