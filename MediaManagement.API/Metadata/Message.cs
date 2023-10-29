namespace MediaManagement.API.Metadata
{
    public enum Message
    {
        InvalidModel,
        Ok,
        SameData,
        NotFound,
        DependencyExist,
        Success,
        Duplicate,
    }
    public class FileContentType
    {
        public static string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    public class FileType
    {
        public const string Pdf = "Pdf";
        public const string png = "png";
        public const string mp4 = "mp4";
        public const string Video = "video";
        public const string Audio = "audio";
        public const string Image = "image";
        public const string Thumbnail = "Thumbnail";
        public const string AppZip = "application/zip";
        public const string AppXZipFile = "application/x-zip";
        public const string AppXZip = "application/x-zip-compressed";
        public const string Document = "document";
        public const string Zip = "zip";
        public const string Scorm = "scorm";
        public const string Youtube = "youtube";
        public static string Objective = "objective";
        public static string Subjective = "subjective";
        public static readonly string[] Doc = { "msword", "officedocument", "ms-word", "ms-excel", "ms-powerpoint", "pdf" , "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/msword","application/rtf"
        };
    }
}
