using System;

namespace ILT.API.APIModel
{
    public class ApiConfigurableParameters
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Attribute { get; set; }
        public string Value { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ApiGetSelectedLanguage
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool isDeleted { get; set; }
    }
}
