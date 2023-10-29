namespace User.API.Models
{
    public class ExportFileLog
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string ServiceName { get; set; }
        public int Count { get; set; }
    }
}
