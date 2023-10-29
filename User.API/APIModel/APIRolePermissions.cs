using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using User.API.Validation;

namespace User.API.APIModel
{
    public class APIRolePermissions
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [CSVInjection]
        public string RoleName { get; set; }
        [MaxLength(50)]
        [CSVInjection]
        public string RoleCode { get; set; }
        [CSVInjection]
        public string RoleDescription { get; set; }

        public bool IsImplicitRole { get; set; }

        public List<APIPermission> Permissions { get; set; }

    }
}
