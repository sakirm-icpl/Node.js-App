using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.API.Models
{
    [Table("FunctionsMaster", Schema = "Masters")]
    public class FunctionsMaster
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string FunctionGroupCode { get; set; }
        [Required]
        [MaxLength(50)]
        public string FunctionCode { get; set; }
        [Required]
        [MaxLength(200)]
        public string FunctionName { get; set; }
        [MaxLength(2000)]
        public string IconPath { get; set; }
        [Required]
        [MaxLength(50)]
        public string ModuleCode { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }

        [MaxLength(100)]
        public string ChangeFunctionName { get; set; }

    }
}
