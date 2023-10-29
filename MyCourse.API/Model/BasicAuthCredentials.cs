using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace MyCourse.API.Model
{
    public class BasicAuthCredentials
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApiToken { get; set; }
    }
}
