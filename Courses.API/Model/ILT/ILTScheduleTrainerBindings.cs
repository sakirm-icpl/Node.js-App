namespace Courses.API.Model.ILT
{
    public class ILTScheduleTrainerBindings : BaseModel
    {
        public int ID { get; set; }
        public int ScheduleID { get; set; }
        public int? TrainerID { get; set; }
        public string TrainerName { get; set; }
        public string TrainerType { get; set; }
        public string AcademyTrainerName { get; set; }
        public string ScheduleType { get; set; }
    }
}
