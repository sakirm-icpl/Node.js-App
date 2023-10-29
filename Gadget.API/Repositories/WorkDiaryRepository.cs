using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class WorkDiaryRepository : Repository<WorkDiary>, IWorkDiaryRepository
    {
        private GadgetDbContext context;
        public WorkDiaryRepository(GadgetDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
