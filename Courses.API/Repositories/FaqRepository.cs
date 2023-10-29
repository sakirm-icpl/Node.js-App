using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Repositories
{
    public class FaqRepository : Repository<Faq>, IFaqRepository
    {
        private CourseContext _db;
        ILCMSRepository _lcmsRepository;
        public FaqRepository(CourseContext context, ILCMSRepository lcmsRepository) : base(context)
        {
            this._db = context;
            this._lcmsRepository = lcmsRepository;
        }
        public async Task<ApiFaq> GetApiFaqByLcmsId(int lcmsId)
        {
            return await (from faq in this._db.Faq
                          join lcms in this._db.LCMS on faq.LcmsId equals lcms.Id
                          where faq.LcmsId == lcmsId && faq.IsDeleted == Record.NotDeleted
                          select new ApiFaq
                          {
                              Id = faq.Id,
                              Title = faq.Title,
                              Description = faq.Description,
                              Metadata = lcms.MetaData
                          }).FirstOrDefaultAsync();
        }
        public async Task<Faq> GetFaqByLcmsId(int lcmsId)
        {
            return await this._db.Faq.Where(f => f.LcmsId == lcmsId && f.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
        }

        public async Task<Message> PostFaq(ApiFaq apiFaq, int userId)
        {
            Faq Faq = Mapper.Map<Faq>(apiFaq);
            if (await Exist(Faq))
                return Message.Duplicate;
            LCMS lcms = new LCMS();
            lcms.Name = apiFaq.Title;
            lcms.MetaData = apiFaq.Metadata;
            lcms.ContentType = ContentType.Faq;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.CreatedBy = userId;
            lcms.ModifiedBy = userId;
            Faq.CreatedBy = lcms.CreatedBy;
            Faq.CreatedDate = lcms.CreatedDate;
            await _lcmsRepository.Add(lcms);
            Faq.LcmsId = lcms.Id;
            await this.Add(Faq);
            return Message.Success;
        }
        public async Task<bool> Exist(Faq faq)
        {
            return await _lcmsRepository.ExistByType(faq.Title, ContentType.Faq, faq.LcmsId);
        }
        public async Task<Message> PutFaq(ApiFaq apiFaq, int userId)
        {
            LCMS lcms = await _lcmsRepository.Get(apiFaq.LcmsId);
            Faq Faq = await this.GetFaqByLcmsId(apiFaq.LcmsId);
            if (lcms == null || Faq == null)
                return Message.NotFound;
            if (await Exist(Faq))
                return Message.Duplicate;

            lcms.Name = apiFaq.Title;
            lcms.MetaData = apiFaq.Metadata;
            lcms.ModifiedDate = DateTime.UtcNow;
            lcms.ModifiedBy = userId;
            Faq.Title = apiFaq.Title;
            Faq.Description = apiFaq.Description;
            Faq.ModifiedDate = lcms.ModifiedDate;
            Faq.ModifiedBy = lcms.ModifiedBy;
            await _lcmsRepository.Update(lcms);
            await this.Update(Faq);
            return Message.Ok;
        }
    }
}
