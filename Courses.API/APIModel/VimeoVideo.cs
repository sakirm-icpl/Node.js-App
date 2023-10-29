namespace Courses.API.APIModel
{
    public class VimeoVideo
    {
        public string name { get; set; }
        public string VideoLink { get; set; }
        public string metaData { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string language { get; set; }
        public float duration { get; set; }
        public bool ismodulecreate { get; set; }
        public bool isBuiltInAssesment { get; set; }
        public bool isMobileCompatible { get; set; }

    }
    public class VimeoConfiguration
    {
        public int Id { get; set; }
        public string Token { get; set; }
        
    }
    public class LCMSID
    {
        public int lcmsId { get; set; }
    }
    public class VimeoLink
    {
        public string Link { get; set; }
    }
}
