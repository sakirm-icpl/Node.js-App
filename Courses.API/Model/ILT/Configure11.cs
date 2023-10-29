using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.ILT
{
    [Table("Configure11", Schema = "User")]
    public class Configure11
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }
    }

    [Table("Configure2", Schema = "User")]
    public class Configure2
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }
        [MaxLength(200)]
        public string NameEncrypted { get; set; }
    }
}
