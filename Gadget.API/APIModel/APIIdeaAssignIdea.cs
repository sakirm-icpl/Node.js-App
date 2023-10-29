using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.APIModel
{
    public class APIIdeaAssignIdea
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Region { get; set; }
        public string Jurylevel { get; set; }
    }
    public class APIApplicationStatusFromAdmin
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
