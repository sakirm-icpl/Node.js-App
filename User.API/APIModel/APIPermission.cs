using System.ComponentModel.DataAnnotations;


namespace User.API.APIModel
{
    public class APIPermission
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public int RoleAuthorityId { get; set; }

        public bool IsAccess { get; set; }

        public string Group { get; set; }

        public string Module { get; set; }

        public string description { get; set; }

        public int sequence { get; set; }

    }
}
