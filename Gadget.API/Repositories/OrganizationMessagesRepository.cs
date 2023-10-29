using Gadget.API.Data;
using Gadget.API.Helper;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace Gadget.API.Repositories
{
    public class OrganizationMessagesRepository : Repository<OrganizationMessages>, IOrganizationMessagesRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OrganizationMessagesRepository));
        private GadgetDbContext db;
        private INotification _notification;

        public OrganizationMessagesRepository(GadgetDbContext context, INotification notification) : base(context)
        {
            this.db = context;
            this._notification = notification;

        }

        public async Task<List<OrganizationMessages>> GetAllOrganizationMessages(int page, int pageSize, string search = null)
        {
             try
            {
                var Query = (from orgmsg in db.OrganizationMessages
                              where orgmsg.IsDeleted == false 
                              select new OrganizationMessages
                              {
                              MessageHeading = orgmsg.MessageHeading,
                             MessageDescription = orgmsg.MessageDescription,
                             ProfilePicture = orgmsg.ProfilePicture,
                               MessageFrom = orgmsg.MessageFrom,
                               ShowToAll = orgmsg.ShowToAll,
                              Status = orgmsg.Status,
                              Id=orgmsg.Id                                   
                              });

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(v => (Convert.ToString(v.MessageHeading).Contains(search)));
                }
                

                Query = Query.OrderByDescending(v => v.Id);
                if (page != -1)
                {
                    Query = Query.Skip((page - 1) * pageSize);
                }
                if (pageSize != -1)
                {
                    Query = Query.Take(pageSize);
                }
                return await Query.ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<bool> ExistOrganizationMessage(string Description)
        {
            int count = await this.db.OrganizationMessages.Where(p => p.MessageDescription.ToLower() == Description.ToLower().TrimStart() && p.IsDeleted == Record.NotDeleted).CountAsync();
            if (count > 0)
                return true;
            return false;

        }
        public async Task<int> CountOrganizationMessages(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.OrganizationMessages.Where(r => r.MessageHeading.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
            return await this.db.OrganizationMessages.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }


    }
}

