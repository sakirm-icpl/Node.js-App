namespace ILT.API.Model.ThirdPartyIntegration
{
    public class AlisonCourse
    {
        public Data[] data { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }

    }
    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public string language { get; set; }
        public publishers[] publishers { get; set; }
        public Categories[] categories { get; set; }
        public Translations[] translations { get; set; }
        public string image { get; set; }
        public string url { get; set; }
        public string primary_cip_code { get; set; }
        public string secondary_cip_code { get; set; }
        public string first_access { get; set; }
        public string last_access { get; set;}
        public string enrollment_date { get; set;}
        public string total_time_spent { get; set; }
        public string course_state { get; set; }
        public string course_status { get; set;}
        public string? course_value { get; set;}
        public string scores { get; set;}
        public string published_at { get; set;}
        public string updated_at { get; set;}
        public string created_at { get; set;}
    }
    public class publishers
    {
        public string Name { get; set; }
        public string slug { get; set; }
        public string location { get; set; }
        public int? country_id { get; set; }
    }
    public class Categories
    {
        public int id { get; set; }
        public string code { get; set; }
        public TranslationsCategory[] translations { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
    }
    public class TranslationsCategory
    {
        public string Name { get; set; }
        public string locale { get; set; }

    }
    public class Translations
    {
        public string Name { get; set; }
        public string headline { get; set; }
        public string summary { get; set; }
        public string locale { get; set; }
    }
    public class Links
    {
        public string first { get; set; }
        public string? last { get; set; }
        public string? prev { get; set; }
        public string? next { get; set; }
    }
    public class Meta
    {
        public int? current_page { get; set; }
        public int? from { get; set; }
        public int? last_page { get; set; }
        public LinksMeta[] links { get; set; }
        public string path { get; set; }
        public int? per_page { get; set; }
        public int? to { get; set; }
        public int? total { get; set; }
    }
    public class LinksMeta
    {
        public string? url { get; set; }
        public string? label { get; set; }
        public bool active { get; set; }
    }
    public class AlisonCategory
    {
        public Categories[] data { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
    }
}
