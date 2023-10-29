using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIRoleAuthority
    {
        public int Id { get; set; }
        [Required]
        public int RoleId { get; set; }
        [Required]
        public int PermissionId { get; set; }
        public bool IsAccess { get; set; }
        public string RoleName { get; set; }
        public string PermissionName { get; set; }

    }
}
