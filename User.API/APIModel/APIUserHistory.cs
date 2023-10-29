using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIUserHistory
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        [Required]
        public object Before { get; set; }
        [Required]
        public object After { get; set; }
        public DateTime Created { get; set; }
    }
}
