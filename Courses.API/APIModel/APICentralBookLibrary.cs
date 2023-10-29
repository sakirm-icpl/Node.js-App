using Courses.API.Model;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APICentralBookLibrary 
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string CustomerCode { get; set; }
        [Range(0, int.MaxValue)]
        public string BookId { get; set; }
        [MaxLength(100)]
        public string BookName { get; set; }
        [MaxLength(100)]
        public string Author { get; set; }
        [MaxLength(50)]
        public string Language { get; set; }
        [MaxLength(50)]
        public string Category { get; set; }
        [Range(0, int.MaxValue)]
        public int CategoryId { get; set; }
        public bool AccessibleToAllUser { get; set; }
        [MaxLength(200)]
        public string KeywordForSearch { get; set; }
        [Range(0, int.MaxValue)]
        public string AccessibilityRuleId { get; set; }
        [MaxLength(50)]
        public string ConfigurationColumn { get; set; }
        [MaxLength(50)]
        public string ConfigurationValue { get; set; }
        public string BookFile { get; set; }
        public string BookImage { get; set; }


        public CentralBookLibrary MapAPIToCentralBookLibrary(APICentralBookLibrary aPICentralBookLibrary)
        {
            CentralBookLibrary centralBookLibrary = new CentralBookLibrary();
            centralBookLibrary.Id = aPICentralBookLibrary.Id;
            centralBookLibrary.CategoryId = aPICentralBookLibrary.CategoryId;
            centralBookLibrary.BookId = aPICentralBookLibrary.BookId;
            centralBookLibrary.BookName = aPICentralBookLibrary.BookName;
            centralBookLibrary.Author = aPICentralBookLibrary.Author;
            centralBookLibrary.Language = aPICentralBookLibrary.Language;
            centralBookLibrary.AccessibleToAllUser = aPICentralBookLibrary.AccessibleToAllUser;
            centralBookLibrary.KeywordForSearch = aPICentralBookLibrary.KeywordForSearch;
            centralBookLibrary.AccessibilityRuleId = aPICentralBookLibrary.AccessibilityRuleId;
            centralBookLibrary.ConfigurationColumn = aPICentralBookLibrary.ConfigurationColumn;
            centralBookLibrary.ConfigurationValue = aPICentralBookLibrary.ConfigurationValue;
            centralBookLibrary.BookFile = aPICentralBookLibrary.BookFile;
            centralBookLibrary.BookImage = aPICentralBookLibrary.BookImage;
            return centralBookLibrary;
        }

        public APICentralBookLibrary MapCentralBookLibraryToAPI(CentralBookLibrary centralBookLibrary)
        {
            APICentralBookLibrary aPICentralBookLibrary = new APICentralBookLibrary();
            aPICentralBookLibrary.Id = centralBookLibrary.Id;
            aPICentralBookLibrary.CategoryId = centralBookLibrary.CategoryId;
            aPICentralBookLibrary.BookId = centralBookLibrary.BookId;
            aPICentralBookLibrary.BookName = centralBookLibrary.BookName;
            aPICentralBookLibrary.Author = centralBookLibrary.Author;
            aPICentralBookLibrary.Language = centralBookLibrary.Language;
            aPICentralBookLibrary.AccessibleToAllUser = centralBookLibrary.AccessibleToAllUser;
            aPICentralBookLibrary.KeywordForSearch = centralBookLibrary.KeywordForSearch;
            aPICentralBookLibrary.AccessibilityRuleId = centralBookLibrary.AccessibilityRuleId;
            aPICentralBookLibrary.ConfigurationColumn = centralBookLibrary.ConfigurationColumn;
            aPICentralBookLibrary.ConfigurationValue = centralBookLibrary.ConfigurationValue;
            aPICentralBookLibrary.BookFile = centralBookLibrary.BookFile;
            aPICentralBookLibrary.BookImage = centralBookLibrary.BookImage;
            return aPICentralBookLibrary;
        }
    }
    public class Picture
    {
        public string Base64String { get; set; }
        public string CustomerCode { get; set; }
        public string FileType { get; set; }
    }

    public class GetCentralBookLibrary
    {
        public int Id { get; set; }
        public string Category { get; set; }
    }
}
