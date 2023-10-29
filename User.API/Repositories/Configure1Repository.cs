//======================================
// <copyright file="Configure1Repository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.APIModel;
using User.API.Data;
using User.API.Helper;
using User.API.Models;
using User.API.Repositories.Interfaces;
using System.Globalization;

namespace User.API.Repositories
{
    public class Configure1Repository : Repository<Configure1>, IConfigure1Repository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Configure1Repository));
        private IConfigurableParameterValuesRepository ConfigurableParameterValuesRepository;
        private UserDbContext _db;
        public Configure1Repository(UserDbContext context, IConfigurableParameterValuesRepository _ConfigurableParameterValuesRepository) : base(context)
        {
            this._db = context;
            ConfigurableParameterValuesRepository = _ConfigurableParameterValuesRepository;
    }
        public async Task<IEnumerable<TypeHeadDto>> GetConfiguration(string columnName, string search = null, int? page = null, int? pageSize = null,string? orgCode=null)
        {
            IQueryable<TypeHeadDto> Query = GetTypeHeadQuery(columnName, search);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            if (page != null && pageSize != null)
            {
                int skip = page.Value;
                int take = pageSize.Value;
                if (skip != -1)
                    Query = Query.Skip((skip - 1) * take);
                if (take != -1)
                    Query = Query.Take(take);
            }


            Query = Query.OrderBy(c => c.Name);

             var allowUserNameValidation = await ConfigurableParameterValuesRepository.GetConfigurationValueAsync("CAPS", orgCode);

           if(allowUserNameValidation == "Yes")
            {
                List<TypeHeadDto> Query2 = new List<TypeHeadDto>();
                foreach (var cell in Query)
                {
                    cell.Name = textInfo.ToTitleCase(cell.Name);
                    Query2.Add(cell);
                    //cell.Name = textInfo.ToTitleCase(cell.Name);
                }
                return Query2;
            }
            else
            {
                return Query;
            }
           
           
           
           
        }
        public async Task<IEnumerable<TypeHeadDto>> GetConfigurationGroup(string columnName, string search = null, int? page = null, int? pageSize = null)
        {
            IQueryable<TypeHeadDto> Query = GetTypeHeadQueryForGroup(columnName, search);

            if (page != null && pageSize != null)
            {
                int skip = page.Value;
                int take = pageSize.Value;
                if (skip != -1)
                    Query = Query.Skip((skip - 1) * take);
                if (take != -1)
                    Query = Query.Take(take);
            }
            Query = Query.OrderBy(c => c.Name);
            return await Query.ToListAsync();
        }
        public async Task<int> GetConfigurationCount(string columnName, string search = null)
        {
            IQueryable<TypeHeadDto> Query = GetTypeHeadQuery(columnName, search);
            return await Query.CountAsync();
        }

        public IQueryable<TypeHeadDto> GetTypeHeadQuery(string columnName, string search = null)
        {
            IQueryable<TypeHeadDto> Query = null;
            columnName = columnName.ToLower();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            switch (columnName)
            {
                case "area":
                    Query = this._db.Area.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "business":
                    Query = this._db.Business.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "group":
                    Query = this._db.Group.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "location":
                    Query = this._db.Location.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn1":
                    Query = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn2":
                    Query = this._db.Configure2.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn3":
                    Query = this._db.Configure3.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn4":
                    Query = this._db.Configure4.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn5":
                    Query = this._db.Configure5.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn6":
                    Query = this._db.Configure6.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn7":
                    Query = this._db.Configure7.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn8":
                    Query = this._db.Configure8.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn9":
                    Query = this._db.Configure9.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn10":
                    Query = this._db.Configure10.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn11":
                    Query = this._db.Configure11.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn12":
                    Query = this._db.Configure12.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn13":
                    Query = this._db.Configure13.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn14":
                    Query = this._db.Configure14.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn15":
                    Query = this._db.Configure15.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
            }
            return Query;
        }

        public IQueryable<TypeHeadDto> GetTypeHeadQueryForGroup(string columnName, string search = null)
        {
            IQueryable<TypeHeadDto> Query = null;
            columnName = columnName.ToLower();
            switch (columnName)
            {
                case "area":
                    Query = this._db.Area.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "business":
                    Query = this._db.Business.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "grouplist":
                    Query = this._db.Group.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "location":
                    Query = this._db.Location.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                
                case "configurationcolumn1":
                    Query = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn2":
                    Query = this._db.Configure2.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn3":
                    Query = this._db.Configure3.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn4":
                    Query = this._db.Configure4.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn5":
                    Query = this._db.Configure5.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn6":
                    Query = this._db.Configure6.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn7":
                    Query = this._db.Configure7.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn8":
                    Query = this._db.Configure8.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn9":
                    Query = this._db.Configure9.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn10":
                    Query = this._db.Configure10.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn11":
                    Query = this._db.Configure11.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn12":
                    Query = this._db.Configure12.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.Contains(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
            }
            return Query;
        }
        public async Task<IEnumerable<TypeHeadDto>> 
            GetConfigurationColumnDatails(string columnName, int Id)
        {
            IQueryable<TypeHeadDto> Query = GetConfigurationColumnDatailsById(columnName, Id);
            return await Query.ToListAsync();
        }


        public IQueryable<TypeHeadDto> GetConfigurationColumnDatailsById(string columnName, int id)
        {
            IQueryable<TypeHeadDto> Query = null;
            columnName = columnName.ToLower();
            switch (columnName)
            {
                case "area":
                    Query = this._db.Area.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == id)).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "business":
                    Query = this._db.Business.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == id)).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "group":
                    Query = this._db.Group.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "location":
                    Query = this._db.Location.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn1":
                    Query = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn2":
                    Query = this._db.Configure2.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn3":
                    Query = this._db.Configure3.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn4":
                    Query = this._db.Configure4.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn5":
                    Query = this._db.Configure5.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn6":
                    Query = this._db.Configure6.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn7":
                    Query = this._db.Configure7.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn8":
                    Query = this._db.Configure8.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn9":
                    Query = this._db.Configure9.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn10":
                    Query = this._db.Configure10.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn11":
                    Query = this._db.Configure11.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
                case "configurationcolumn12":
                    Query = this._db.Configure12.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
                    break;
            }
            return Query;
        }

        public async Task<string> UpdateConfigurationColumnDetails(string columnName, TypeHeadDto configurationColumn)
        {
            columnName = columnName.ToLower();

            switch (columnName)
            {
                case "area":
                    var Area = this._db.Area.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == configurationColumn.Id)).FirstOrDefault();
                    var DuplicateArea = this._db.Area.Where(c => (c.Id != configurationColumn.Id) && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateArea == null)
                    {
                        Area.Name = configurationColumn.Name;
                        this._db.Area.Update(Area);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "business":
                    var Business = this._db.Business.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == configurationColumn.Id)).FirstOrDefault();
                    var DuplicateBusiness = this._db.Business.Where(c => (c.Id != configurationColumn.Id) && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateBusiness == null)
                    {
                        Business.Name = configurationColumn.Name;
                        this._db.Business.Update(Business);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;

                case "group":
                    var Group = this._db.Group.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateGroup = this._db.Group.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateGroup == null)
                    {
                        Group.Name = configurationColumn.Name;
                        this._db.Group.Update(Group);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "location":
                    var Location = this._db.Location.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateLocation = this._db.Location.Where(c => c.Id == configurationColumn.Id && c.Id == configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateLocation == null)
                    {
                        Location.Name = configurationColumn.Name;
                        this._db.Location.Update(Location);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn1":
                    var configurationcolumn1 = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationColumn1 = this._db.Configure1.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationColumn1 == null)
                    {
                        configurationcolumn1.Name = configurationColumn.Name;
                        this._db.Configure1.Update(configurationcolumn1);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn2":
                    var configurationcolumn2 = this._db.Configure2.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn2 = this._db.Configure2.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn2 == null)
                    {
                        configurationcolumn2.Name = configurationColumn.Name;
                        this._db.Configure2.Update(configurationcolumn2);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn3":
                    var configurationcolumn3 = this._db.Configure3.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn3 = this._db.Configure3.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn3 == null)
                    {
                        configurationcolumn3.Name = configurationColumn.Name;
                        this._db.Configure3.Update(configurationcolumn3);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn4":
                    var configurationcolumn4 = this._db.Configure4.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn4 = this._db.Configure4.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn4 == null)
                    {
                        configurationcolumn4.Name = configurationColumn.Name;
                        this._db.Configure4.Update(configurationcolumn4);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn5":
                    var configurationcolumn5 = this._db.Configure5.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn5 = this._db.Configure5.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn5 == null)
                    {
                        configurationcolumn5.Name = configurationColumn.Name;
                        this._db.Configure5.Update(configurationcolumn5);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn6":
                    var configurationcolumn6 = this._db.Configure6.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var Duplicateconfigurationcolumn6 = this._db.Configure6.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (Duplicateconfigurationcolumn6 == null)
                    {
                        configurationcolumn6.Name = configurationColumn.Name;
                        this._db.Configure6.Update(configurationcolumn6);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn7":
                    var configurationcolumn7 = this._db.Configure7.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn7 = this._db.Configure7.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn7 == null)
                    {
                        configurationcolumn7.Name = configurationColumn.Name;
                        this._db.Configure7.Update(configurationcolumn7);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }

                    break;
                case "configurationcolumn8":
                    var configurationcolumn8 = this._db.Configure8.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn8 = this._db.Configure8.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn8 == null)
                    {
                        configurationcolumn8.Name = configurationColumn.Name;
                        this._db.Configure8.Update(configurationcolumn8);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn9":
                    var configurationcolumn9 = this._db.Configure9.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn9 = this._db.Configure9.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn9 == null)
                    {
                        configurationcolumn9.Name = configurationColumn.Name;
                        this._db.Configure9.Update(configurationcolumn9);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn10":
                    var configurationcolumn10 = this._db.Configure10.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn10 = this._db.Configure10.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn10 == null)
                    {
                        configurationcolumn10.Name = configurationColumn.Name;
                        this._db.Configure10.Update(configurationcolumn10);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn11":
                    var configurationcolumn11 = this._db.Configure11.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn11 = this._db.Configure11.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn11 == null)
                    {
                        configurationcolumn11.Name = configurationColumn.Name;
                        this._db.Configure11.Update(configurationcolumn11);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
                case "configurationcolumn12":
                    var configurationcolumn12 = this._db.Configure12.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configurationColumn.Id).FirstOrDefault();
                    var DuplicateConfigurationcolumn12 = this._db.Configure12.Where(c => c.Id != configurationColumn.Id && c.Name == configurationColumn.Name).FirstOrDefault();
                    if (DuplicateConfigurationcolumn12 == null)
                    {
                        configurationcolumn12.Name = configurationColumn.Name;
                        this._db.Configure12.Update(configurationcolumn12);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "Duplicate!" + configurationColumn.Name + " value already exist.";
                    }
                    break;
            }
            return "true";
        }

        public async Task<string> DeleteConfigurationColumnDetails(string columnName, int id)
        {
            UserMasterDetails userMasterDetails = new UserMasterDetails();
            columnName = columnName.ToLower();

            switch (columnName)
            {
                case "area":
                    var Area = this._db.Area.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == id)).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.AreaId == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Area.Remove(Area);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "business":
                    var Business = this._db.Business.Where(c => c.IsDeleted == Record.NotDeleted && (c.Id == id)).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.BusinessId == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Business.Remove(Business);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;

                case "group":
                    var Group = this._db.Group.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.GroupId == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Group.Remove(Group);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "location":
                    var Location = this._db.Location.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.LocationId == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Location.Remove(Location);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn1":
                    var configurationcolumn1 = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn1 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure1.Remove(configurationcolumn1);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn2":
                    var configurationcolumn2 = this._db.Configure2.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn2 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure2.Remove(configurationcolumn2);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn3":
                    var configurationcolumn3 = this._db.Configure3.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn3 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure3.Remove(configurationcolumn3);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn4":
                    var configurationcolumn4 = this._db.Configure4.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn4 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure4.Remove(configurationcolumn4);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn5":
                    var configurationcolumn5 = this._db.Configure5.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn5 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure5.Remove(configurationcolumn5);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn6":
                    var configurationcolumn6 = this._db.Configure6.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn6 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure6.Remove(configurationcolumn6);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn7":
                    var configurationcolumn7 = this._db.Configure7.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn7 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure7.Remove(configurationcolumn7);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }

                    break;
                case "configurationcolumn8":
                    var configurationcolumn8 = this._db.Configure8.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn8 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure8.Remove(configurationcolumn8);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn9":
                    var configurationcolumn9 = this._db.Configure9.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn9 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure9.Remove(configurationcolumn9);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn10":
                    var configurationcolumn10 = this._db.Configure10.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn10 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure10.Remove(configurationcolumn10);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn11":
                    var configurationcolumn11 = this._db.Configure11.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn11 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure11.Remove(configurationcolumn11);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
                case "configurationcolumn12":
                    var configurationcolumn12 = this._db.Configure12.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
                    userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn12 == id).FirstOrDefaultAsync();
                    if (userMasterDetails == null)
                    {
                        this._db.Configure12.Remove(configurationcolumn12);
                        await this._db.SaveChangesAsync();
                    }
                    else
                    {
                        return "DependancyExist";
                    }
                    break;
            }
            return "true";
        }


        public async Task<IEnumerable<string>> GetConfigurationNames()
        {
            var result = (from configure1 in this._db.Configure1
                          where (configure1.IsDeleted == 0)
                          select configure1.Name).ToListAsync();
            return await result;
        }
        public async Task<int> GetIdIfExist(string configurationName)
        {
            if (string.IsNullOrEmpty(configurationName))
                return 0;
            var result = await (from configure in this._db.Configure1.AsNoTracking()
                                where (configure.IsDeleted == 0 && string.Equals(configure.Name, configurationName, StringComparison.CurrentCultureIgnoreCase))
                                select configure.Id).SingleOrDefaultAsync();
            if (result != 0)
                return result;

            return 0;
        }
        public async Task<int> GetLastInsertedId()
        {
            var result = (from configure1 in this._db.Configure1
                          orderby configure1.Id descending
                          select configure1.Id);
            if (result != null)
                return await result.FirstAsync();
            return 0;
        }

        public async Task<string> GetConfigure1NameById(int? configureId)
        {
            var result = (from Configure1 in this._db.Configure1
                          where (Configure1.IsDeleted == Record.NotDeleted && Configure1.Id == configureId)
                          select Configure1.Name);
            return await result.AsNoTracking().SingleOrDefaultAsync();
        }
        public async Task<IEnumerable<Configure1>> GetAllConfiguration1(string search)
        {
            try
            {
                var result = (from configure1 in this._db.Configure1
                              where (configure1.Name.StartsWith(search) && configure1.IsDeleted == Record.NotDeleted)
                              select new Configure1
                              {
                                  Name = configure1.Name,
                                  Id = configure1.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<List<Configure1>> GetConfiguration1()
        {
            try
            {
                var result = (from configure1 in this._db.Configure1
                              where (configure1.IsDeleted == Record.NotDeleted)
                              select new Configure1
                              {
                                  Name = configure1.Name,
                                  Id = configure1.Id

                              });
                return await result.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return null;
        }

        public async Task<string> PostConfigurationColumnDetails(ApiConfiguration1 configuration1Details)
        {

            var configurationcolumn1 = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Name == configuration1Details.Name).FirstOrDefault();

            if (configurationcolumn1 == null)
            {
                Configure1 Configure1 = new Configure1();
                Configure1.Name = configuration1Details.Name;
                Configure1.NameEncrypted = Security.Encrypt(configuration1Details.Name);
                Configure1.CreatedDate = DateTime.UtcNow;

                this._db.Configure1.Add(Configure1);
                await this._db.SaveChangesAsync();
            }
            else
            {
                return "Value already exist.";
            }

            return "true";
        }

        public async Task<string> PutConfigurationColumnDetails(ApiConfiguration1 configuration1Details)
        {

            var configurationcolumn1 = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == configuration1Details.Id).FirstOrDefault();
            var DuplicateConfigurationColumn1 = this._db.Configure1.Where(c => c.Id != configuration1Details.Id && c.Name == configuration1Details.Name).FirstOrDefault();
            if (DuplicateConfigurationColumn1 == null)
            {
                configurationcolumn1.Name = configuration1Details.Name;
                configurationcolumn1.NameEncrypted = Security.Encrypt(configuration1Details.Name);


                this._db.Configure1.Update(configurationcolumn1);
                await this._db.SaveChangesAsync();
            }
            else
            {
                return "Duplicate!";
            }

            return "true";
        }

        public async Task<string> DeleteConfiguration1Details(int id)
        {
            UserMasterDetails userMasterDetails = new UserMasterDetails();

            var configurationcolumn1 = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && c.Id == id).FirstOrDefault();
            userMasterDetails = await this._db.UserMasterDetails.Where(c => c.ConfigurationColumn1 == id).FirstOrDefaultAsync();
            if (userMasterDetails == null)
            {
                this._db.Configure1.Remove(configurationcolumn1);
                await this._db.SaveChangesAsync();
            }
            else
            {
                return "DependancyExist";
            }

            return "true";
        }

        public async Task<IEnumerable<TypeHeadDto>> GetConfiguration(int? page = null, int? pageSize = null, string search = null)
        {
            IQueryable<TypeHeadDto> Query = null;
            Query = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });


            if (page != null && pageSize != null)
            {
                int skip = page.Value;
                int take = pageSize.Value;
                if (skip != -1)
                    Query = Query.Skip((skip - 1) * take);
                if (take != -1)
                    Query = Query.Take(take);
            }
            Query = Query.OrderBy(c => c.Name);
            return await Query.ToListAsync();
        }


        public async Task<int> GetConfiguration1Count(string search = null)
        {
            IQueryable<TypeHeadDto> Query = this._db.Configure1.Where(c => c.IsDeleted == Record.NotDeleted && (search == null || c.Name.StartsWith(search))).Select(c => new TypeHeadDto { Id = c.Id, Name = c.Name });
            return await Query.CountAsync();

        }


    }
}
