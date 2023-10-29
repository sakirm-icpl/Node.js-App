using Evaluation.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class ProcessConfiguration : CommonFields
    {
        public int Id { get; set; }
        public int ProcessManagementId { get; set; }

    }
}
