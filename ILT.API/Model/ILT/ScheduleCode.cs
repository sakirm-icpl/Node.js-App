namespace ILT.API.Model.ILT
{
    public class ScheduleCode
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public bool? IsDeleted { get; set; }
        public int? UserId { get; set; }
    }
}
