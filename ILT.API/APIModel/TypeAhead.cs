namespace ILT.API.APIModel
{
    public class TypeAhead
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Type { get; set; }
    }
    public class ILTCourseTypeAhead
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int CourseFee { get; set; }
        public string Currency { get; set; }
    }
}
