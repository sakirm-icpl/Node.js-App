using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ILT.API.Repositories
{
    public class ApplicabilityGroupTemplateRepository : Repository<ApplicabilityGroupTemplate>, IApplicabilityGroupTemplate
    {
        private CourseContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
        public ApplicabilityGroupTemplateRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }

       
        public async Task<int> Count(string search = null, string filter = null)
        {
            var Query = (_db.ApplicabilityGroupTemplate.Where(b => b.IsDeleted == false && (search == null || b.ApplicabilityGroupName.Contains(search))).GroupBy(a => a.Id).Select(result => new
            {
                ID = result.Key
            }));
          
            return await Query.CountAsync();
        }
        public async Task<List<object>> GetAllGroupTemplate(int page, int pageSize, string search = null, string filter = null)
        {
            var Query = from applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate
                        join accessibiltyRule in _db.AccessibilityRule on applicabilityGroupTemplate.Id equals accessibiltyRule.Id      //change accessibiltyRule.Id 
                        into c
                        from accessibiltyRule in c.DefaultIfEmpty()
                        where applicabilityGroupTemplate.IsDeleted == false && (search == null || applicabilityGroupTemplate.ApplicabilityGroupName.Contains(search))
                        select new
                        {
                            Id = applicabilityGroupTemplate.Id,
                            ApplicabilityGroupName = applicabilityGroupTemplate.ApplicabilityGroupName,
                            ApplicabilityGroupDescription = applicabilityGroupTemplate.ApplicabilityGroupDescription,
                            HasDependancy = (accessibiltyRule == null ? 0 : 1)
                        };

            Query = Query.OrderByDescending(a => a.Id);
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.ToListAsync<object>();
        }
        public async Task<APIApplicabilityGroupTemplate> GetApplicabilityGroupTemplateDetails(int TemplateId, string orgnizationCode)
        {
            APIApplicabilityGroupTemplate ApiRule = await GetApplicabilityGroupTemplate(TemplateId);

            if (ApiRule != null)
            {
                string UserUrl = _configuration[APIHelper.UserAPI];
                string NameById = "GetNameById";
                foreach (var applicabilityRule in ApiRule.ApplicabilityGroupRule)
                {
                    string ColumnName = applicabilityRule.ApplicabilityRule;
                    int Value = Convert.ToInt32(applicabilityRule.ParameterValue);

                    HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Title _Title = JsonConvert.DeserializeObject<Title>(result);
                        applicabilityRule.ParameterValue = _Title?.Name;
                    }
                }
            }
            return ApiRule;
        }
        public async Task<List<ApplicabilityGroupRules>> Post(APIApplicabilityGroupTemplate apiApplicabilityGroupTemplate, int userId)
        {
            List<ApplicabilityGroupRules> Duplicates = new List<ApplicabilityGroupRules>();
            ApplicabilityGroupRules[] AndApplicabilityRules = apiApplicabilityGroupTemplate.ApplicabilityGroupRule.Where(a => a.Condition.ToLower().Equals("and")).ToArray();
            ApplicabilityGroupRules[] OrApplicabilityRules = apiApplicabilityGroupTemplate.ApplicabilityGroupRule.Where(a => a.Condition.ToLower().Equals("or") || a.Condition.ToLower().Equals("null")).ToArray();
            if (AndApplicabilityRules.Count() > 0)
            {
                ApplicabilityGroupTemplate applicabilityGroupTemplate = new ApplicabilityGroupTemplate
                {
                    ConditionForRules = "and",
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    Id = apiApplicabilityGroupTemplate.Id,
                    ApplicabilityGroupName = apiApplicabilityGroupTemplate.ApplicabilityGroupName,
                    ApplicabilityGroupDescription = apiApplicabilityGroupTemplate.ApplicabilityGroupDescription,
                    IsDeleted = false,
                    IsActive = true
                };

                foreach (ApplicabilityGroupRules applicability in AndApplicabilityRules)
                {
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn1"))
                        applicabilityGroupTemplate.ConfigurationColumn1 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn2"))
                        applicabilityGroupTemplate.ConfigurationColumn2 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn3"))
                        applicabilityGroupTemplate.ConfigurationColumn3 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn4"))
                        applicabilityGroupTemplate.ConfigurationColumn4 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn5"))
                        applicabilityGroupTemplate.ConfigurationColumn5 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn6"))
                        applicabilityGroupTemplate.ConfigurationColumn6 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn7"))
                        applicabilityGroupTemplate.ConfigurationColumn7 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn8"))
                        applicabilityGroupTemplate.ConfigurationColumn8 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn9"))
                        applicabilityGroupTemplate.ConfigurationColumn9 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn10"))
                        applicabilityGroupTemplate.ConfigurationColumn10 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn11"))
                        applicabilityGroupTemplate.ConfigurationColumn11 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("configurationcolumn12"))
                        applicabilityGroupTemplate.ConfigurationColumn12 = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("area"))
                        applicabilityGroupTemplate.Area = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("business"))
                        applicabilityGroupTemplate.Business = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("emailid"))
                        applicabilityGroupTemplate.EmailID = applicability.ParameterValue;
                    if (applicability.ApplicabilityRule.ToLower().Equals("location"))
                        applicabilityGroupTemplate.Location = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("group"))
                        applicabilityGroupTemplate.Group = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("userid"))
                        applicabilityGroupTemplate.UserID = Convert.ToInt32(applicability.ParameterValue);
                    if (applicability.ApplicabilityRule.ToLower().Equals("mobilenumber"))
                        applicabilityGroupTemplate.MobileNumber = applicability.ParameterValue;
                }
                if (await RuleExist(applicabilityGroupTemplate))
                {
                    Duplicates.Add(AndApplicabilityRules[0]);
                }
                else if (applicabilityGroupTemplate.Id == null)
                {
                    await this.Add(applicabilityGroupTemplate);
                }
                else
                {
                    await this.Update(applicabilityGroupTemplate);
                }
            }
            if (OrApplicabilityRules.Count() > 0)
            {
                foreach (ApplicabilityGroupRules applicability in OrApplicabilityRules)
                {
                    ApplicabilityGroupTemplate applicabilityGroupTemplate = new ApplicabilityGroupTemplate();
                    if (!applicability.Condition.Equals("null"))
                        applicabilityGroupTemplate.ConditionForRules = "or";
                    applicabilityGroupTemplate.CreatedDate = DateTime.UtcNow;
                    bool RecordExist = false;
                    string columnName = applicability.ApplicabilityRule.ToLower();
                    var Query = _db.ApplicabilityGroupTemplate.Where(a => a.ApplicabilityGroupName == apiApplicabilityGroupTemplate.ApplicabilityGroupName && a.IsDeleted == false);
                    applicabilityGroupTemplate.ApplicabilityGroupDescription = apiApplicabilityGroupTemplate.ApplicabilityGroupDescription;
                    switch (columnName)
                    {
                        case "configurationcolumn1":
                            applicabilityGroupTemplate.ConfigurationColumn1 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn1 == applicabilityGroupTemplate.ConfigurationColumn1);
                            break;
                        case "configurationcolumn2":
                            applicabilityGroupTemplate.ConfigurationColumn2 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn2 == applicabilityGroupTemplate.ConfigurationColumn2);
                            break;
                        case "configurationcolumn3":
                            applicabilityGroupTemplate.ConfigurationColumn3 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn3 == applicabilityGroupTemplate.ConfigurationColumn3);
                            break;
                        case "configurationcolumn4":
                            applicabilityGroupTemplate.ConfigurationColumn4 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn4 == applicabilityGroupTemplate.ConfigurationColumn4);
                            break;
                        case "configurationcolumn5":
                            applicabilityGroupTemplate.ConfigurationColumn5 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn5 == applicabilityGroupTemplate.ConfigurationColumn5);
                            break;
                        case "configurationcolumn6":
                            applicabilityGroupTemplate.ConfigurationColumn6 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn6 == applicabilityGroupTemplate.ConfigurationColumn6);
                            break;
                        case "configurationcolumn7":
                            applicabilityGroupTemplate.ConfigurationColumn7 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn7 == applicabilityGroupTemplate.ConfigurationColumn7);
                            break;
                        case "configurationcolumn8":
                            applicabilityGroupTemplate.ConfigurationColumn8 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn8 == applicabilityGroupTemplate.ConfigurationColumn8);
                            break;
                        case "configurationcolumn9":
                            applicabilityGroupTemplate.ConfigurationColumn9 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn9 == applicabilityGroupTemplate.ConfigurationColumn9);
                            break;
                        case "configurationcolumn10":
                            applicabilityGroupTemplate.ConfigurationColumn10 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn10 == applicabilityGroupTemplate.ConfigurationColumn10);
                            break;
                        case "configurationcolumn11":
                            applicabilityGroupTemplate.ConfigurationColumn11 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn11 == applicabilityGroupTemplate.ConfigurationColumn11);
                            break;
                        case "configurationcolumn12":
                            applicabilityGroupTemplate.ConfigurationColumn12 = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.ConfigurationColumn12 == applicabilityGroupTemplate.ConfigurationColumn12);
                            break;
                        case "area":
                            applicabilityGroupTemplate.Area = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.Area == applicabilityGroupTemplate.Area);
                            break;
                        case "business":
                            applicabilityGroupTemplate.Business = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.Business == applicabilityGroupTemplate.Business);
                            break;
                        case "emailid":
                            applicabilityGroupTemplate.EmailID = applicability.ParameterValue;
                            Query = Query.Where(x => x.EmailID == applicabilityGroupTemplate.EmailID);
                            break;
                        case "location":
                            applicabilityGroupTemplate.Location = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.Location == applicabilityGroupTemplate.Location);
                            break;
                        case "group":
                            applicabilityGroupTemplate.Group = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.Group == applicabilityGroupTemplate.Group && x.IsDeleted == Record.NotDeleted);
                            break;
                        case "userid":
                            applicabilityGroupTemplate.UserID = Convert.ToInt32(applicability.ParameterValue);
                            Query = Query.Where(x => x.UserID == applicabilityGroupTemplate.UserID && x.IsDeleted == Record.NotDeleted);
                            break;
                        case "mobilenumber":
                            applicabilityGroupTemplate.MobileNumber = applicability.ParameterValue;
                            Query = Query.Where(x => x.MobileNumber == applicabilityGroupTemplate.MobileNumber && x.IsDeleted == Record.NotDeleted);
                            break;
                    }
                    RecordExist = Query.Count() > 0 ? true : false;
                    if (!RecordExist)
                    {
                        if (applicabilityGroupTemplate.Id == null)
                        {
                            await this.Add(applicabilityGroupTemplate);
                        }
                        else
                        {
                            await this.Update(applicabilityGroupTemplate);
                        }
                    }
                    else
                    {
                        Duplicates.Add(applicability);
                    }
                }
            }
            if (Duplicates.Count > 0)
                return Duplicates;
            return null;
        }
        public async Task<bool> RuleExist(ApplicabilityGroupTemplate applicabilityGroupTemplate)
        {
            IQueryable<ApplicabilityGroupTemplate> Query = this._db.ApplicabilityGroupTemplate.Where(x=>x.IsDeleted==false);

            if (applicabilityGroupTemplate.Area != null)
                Query = Query.Where(a => a.Area == applicabilityGroupTemplate.Area);
            if (applicabilityGroupTemplate.Business != null)
                Query = Query.Where(a => a.Business == applicabilityGroupTemplate.Business);
            if (applicabilityGroupTemplate.ConfigurationColumn1 != null)
                Query = Query.Where(a => a.ConfigurationColumn1 == applicabilityGroupTemplate.ConfigurationColumn1);
            if (applicabilityGroupTemplate.ConfigurationColumn2 != null)
                Query.Where(a => a.ConfigurationColumn2 == applicabilityGroupTemplate.ConfigurationColumn2);
            if (applicabilityGroupTemplate.ConfigurationColumn3 != null)
                Query = Query.Where(a => a.ConfigurationColumn3 == applicabilityGroupTemplate.ConfigurationColumn3);
            if (applicabilityGroupTemplate.ConfigurationColumn4 != null)
                Query = Query.Where(a => a.ConfigurationColumn4 == applicabilityGroupTemplate.ConfigurationColumn4);
            if (applicabilityGroupTemplate.ConfigurationColumn5 != null)
                Query = Query.Where(a => a.ConfigurationColumn5 == applicabilityGroupTemplate.ConfigurationColumn5);
            if (applicabilityGroupTemplate.ConfigurationColumn6 != null)
                Query = Query.Where(a => a.ConfigurationColumn6 == applicabilityGroupTemplate.ConfigurationColumn6);
            if (applicabilityGroupTemplate.ConfigurationColumn7 != null)
                Query = Query.Where(a => a.ConfigurationColumn7 == applicabilityGroupTemplate.ConfigurationColumn7);
            if (applicabilityGroupTemplate.ConfigurationColumn8 != null)
                Query = Query.Where(a => a.ConfigurationColumn8 == applicabilityGroupTemplate.ConfigurationColumn8);
            if (applicabilityGroupTemplate.ConfigurationColumn9 != null)
                Query = Query.Where(a => a.ConfigurationColumn9 == applicabilityGroupTemplate.ConfigurationColumn9);
            if (applicabilityGroupTemplate.ConfigurationColumn10 != null)
                Query = Query.Where(a => a.ConfigurationColumn10 == applicabilityGroupTemplate.ConfigurationColumn10);
            if (applicabilityGroupTemplate.ConfigurationColumn11 != null)
                Query = Query.Where(a => a.ConfigurationColumn11 == applicabilityGroupTemplate.ConfigurationColumn11);
            if (applicabilityGroupTemplate.ConfigurationColumn12 != null)
                Query = Query.Where(a => a.ConfigurationColumn12 == applicabilityGroupTemplate.ConfigurationColumn12);
            if (applicabilityGroupTemplate.MobileNumber != null)
                Query = Query.Where(a => a.MobileNumber == applicabilityGroupTemplate.MobileNumber);
            if (applicabilityGroupTemplate.EmailID != null)
                Query = Query.Where(a => a.EmailID == applicabilityGroupTemplate.EmailID);
            if (applicabilityGroupTemplate.Location != null)
                Query = Query.Where(a => a.Location == applicabilityGroupTemplate.Location);
            if (applicabilityGroupTemplate.Group != null)
                Query = Query.Where(a => a.Group == applicabilityGroupTemplate.Group);
            if (applicabilityGroupTemplate.UserID != null)
                Query = Query.Where(a => a.Group == applicabilityGroupTemplate.UserID);

            int Count = await Query.CountAsync();
            if (Count > 0)
                return true;
            else
                return false;
        }
        public async Task<APIApplicabilityGroupTemplate> GetApplicabilityGroupTemplate(int TemplateId)
        {
            var AccessRule = await (from applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate
                                    where applicabilityGroupTemplate.Id == TemplateId && applicabilityGroupTemplate.IsDeleted == false
                                    select new
                                    {
                                        applicabilityGroupTemplate.UserID,
                                        applicabilityGroupTemplate.Business,
                                        applicabilityGroupTemplate.Group,
                                        applicabilityGroupTemplate.Area,
                                        applicabilityGroupTemplate.Location,
                                        applicabilityGroupTemplate.ConfigurationColumn1,
                                        applicabilityGroupTemplate.ConfigurationColumn2,
                                        applicabilityGroupTemplate.ConfigurationColumn3,
                                        applicabilityGroupTemplate.ConfigurationColumn4,
                                        applicabilityGroupTemplate.ConfigurationColumn5,
                                        applicabilityGroupTemplate.ConfigurationColumn6,
                                        applicabilityGroupTemplate.ConfigurationColumn7,
                                        applicabilityGroupTemplate.ConfigurationColumn8,
                                        applicabilityGroupTemplate.ConfigurationColumn9,
                                        applicabilityGroupTemplate.ConfigurationColumn10,
                                        applicabilityGroupTemplate.ConfigurationColumn11,
                                        applicabilityGroupTemplate.ConfigurationColumn12,
                                        applicabilityGroupTemplate.ConditionForRules,
                                        applicabilityGroupTemplate.Id,
                                        applicabilityGroupTemplate.ApplicabilityGroupName,
                                        applicabilityGroupTemplate.ApplicabilityGroupDescription
                                    }).SingleOrDefaultAsync();

            string Condition = AccessRule.ConditionForRules;
            PropertyInfo[] properties = AccessRule.GetType().GetProperties();
            List<Rules> Rules = new List<Rules>();
            string ApplicabilityGroupName = string.Empty;
            string ApplicabilityGroupDescription = string.Empty;
            int Id = 0;
            foreach (PropertyInfo rule in properties)
            {
                if (rule.Name.ToLower().Equals("applicabilitygroupname"))
                    ApplicabilityGroupName = rule.GetValue(AccessRule).ToString();
                if (rule.Name.ToLower().Equals("applicabilitygroupdescription") && rule.GetValue(AccessRule, null) != null)
                    ApplicabilityGroupDescription = rule.GetValue(AccessRule).ToString();

                if (rule.Name.ToLower().Equals("id"))
                    Id = Int32.Parse(rule.GetValue(AccessRule).ToString());

                if (rule.GetValue(AccessRule, null) != null &&
                    !rule.Name.Equals("ConditionForRules") &&
                    !rule.Name.ToLower().Equals("applicabilitygroupname") &&
                    !rule.Name.ToLower().Equals("applicabilitygroupdescription") &&
                    !rule.Name.ToLower().Equals("id"))
                {
                    Rules Rule = new Rules
                    {
                        AccessibilityParameter = rule.Name,
                        AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                        Condition = Condition
                    };
                    Rules.Add(Rule);
                }
            }
            APIApplicabilityGroupTemplate ApiRule = new APIApplicabilityGroupTemplate
            {
                Id = Id,
                ApplicabilityGroupName = ApplicabilityGroupName,
                ApplicabilityGroupDescription = ApplicabilityGroupDescription
            };
            int i = 0;
            ApiRule.ApplicabilityGroupRule = new ApplicabilityGroupRules[Rules.Count];
            foreach (var rule in Rules)
            {
                ApplicabilityGroupRules applicabilityGroupRule = new ApplicabilityGroupRules
                {
                    ApplicabilityRule = rule.AccessibilityParameter,
                    ParameterValue = rule.AccessibilityValue,
                    Condition = rule.Condition
                };
                ApiRule.ApplicabilityGroupRule[i] = applicabilityGroupRule;
                i++;
            }
            return ApiRule;
        }


        public async Task<APIScheduleVisibilityTemplate> GetVisibilityTeamTemplate(int TemplateId,int scheduleId)
        {
            try
            {
                var AccessRule = await (from applicabilityGroupTemplate in _db.UserTeamsMapping
                                        where applicabilityGroupTemplate.UserTeamId == TemplateId && applicabilityGroupTemplate.IsDeleted == false
                                        select new
                                        {
                                            applicabilityGroupTemplate.UserID,
                                            applicabilityGroupTemplate.Business,
                                            applicabilityGroupTemplate.Group,
                                            applicabilityGroupTemplate.Area,
                                            applicabilityGroupTemplate.Location,
                                            applicabilityGroupTemplate.ConfigurationColumn1,
                                            applicabilityGroupTemplate.ConfigurationColumn2,
                                            applicabilityGroupTemplate.ConfigurationColumn3,
                                            applicabilityGroupTemplate.ConfigurationColumn4,
                                            applicabilityGroupTemplate.ConfigurationColumn5,
                                            applicabilityGroupTemplate.ConfigurationColumn6,
                                            applicabilityGroupTemplate.ConfigurationColumn7,
                                            applicabilityGroupTemplate.ConfigurationColumn8,
                                            applicabilityGroupTemplate.ConfigurationColumn9,
                                            applicabilityGroupTemplate.ConfigurationColumn10,
                                            applicabilityGroupTemplate.ConfigurationColumn11,
                                            applicabilityGroupTemplate.ConfigurationColumn12,
                                            applicabilityGroupTemplate.ConditionForRules,
                                            applicabilityGroupTemplate.Id,
                                            //applicabilityGroupTemplate.ApplicabilityGroupName,
                                            //applicabilityGroupTemplate.ApplicabilityGroupDescription
                                        }).SingleOrDefaultAsync();

                if (AccessRule != null)
                {
                    string Condition = AccessRule.ConditionForRules;
                    PropertyInfo[] properties = AccessRule.GetType().GetProperties();
                    List<Rules> Rules = new List<Rules>();
                    string ApplicabilityGroupName = string.Empty;
                    string ApplicabilityGroupDescription = string.Empty;
                    int Id = 0;
                    foreach (PropertyInfo rule in properties)
                    {
                        if (rule.Name.ToLower().Equals("applicabilitygroupname"))
                            ApplicabilityGroupName = rule.GetValue(AccessRule).ToString();
                        if (rule.Name.ToLower().Equals("applicabilitygroupdescription") && rule.GetValue(AccessRule, null) != null)
                            ApplicabilityGroupDescription = rule.GetValue(AccessRule).ToString();

                        if (rule.Name.ToLower().Equals("id"))
                            Id = Int32.Parse(rule.GetValue(AccessRule).ToString());

                        if (rule.GetValue(AccessRule, null) != null &&
                            !rule.Name.Equals("ConditionForRules") &&
                            !rule.Name.ToLower().Equals("applicabilitygroupname") &&
                            !rule.Name.ToLower().Equals("applicabilitygroupdescription") &&
                            !rule.Name.ToLower().Equals("id"))
                        {
                            Rules Rule = new Rules
                            {
                                AccessibilityParameter = rule.Name,
                                AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                                Condition = Condition
                            };
                            Rules.Add(Rule);
                        }
                    }
                    APIScheduleVisibilityTemplate ApiRule = new APIScheduleVisibilityTemplate
                    {
                        Id = Id,
                        ApplicabilityGroupName = ApplicabilityGroupName,
                        ApplicabilityGroupDescription = ApplicabilityGroupDescription
                    };
                    int i = 0;
                    ApiRule.ScheduleVisibilityRules = new ScheduleVisibilityRules[Rules.Count];
                    foreach (var rule in Rules)
                    {
                        ScheduleVisibilityRules applicabilityGroupRule = new ScheduleVisibilityRules
                        {
                            ApplicabilityRule = rule.AccessibilityParameter,
                            ParameterValue = rule.AccessibilityValue,
                            Condition = rule.Condition
                        };
                        ApiRule.ScheduleVisibilityRules[i] = applicabilityGroupRule;
                        i++;
                    }
                    return ApiRule;
                }
            }
            catch (Exception ex)
            { }
            return null;
        }
        public async Task<List<object>> GetAllGroupTemplate()
        {
            var Query = from applicabilityGroupTemplate in _db.ApplicabilityGroupTemplate
                        where applicabilityGroupTemplate.IsDeleted == false
                        select new
                        {
                            Id = applicabilityGroupTemplate.Id,
                            ApplicabilityGroupName = applicabilityGroupTemplate.ApplicabilityGroupName,
                            ApplicabilityGroupDescription = applicabilityGroupTemplate.ApplicabilityGroupDescription
                        };

            Query = Query.OrderBy(a => a.ApplicabilityGroupName);
            return await Query.ToListAsync<object>();
        }

        public async Task<APIApplicabilityGroupTemplate> GetApplicabilityUserTeam(int TemplateId)
        {
            var AccessRule = await (from applicabilityGroupTemplate in _db.ScheduleVisibilityRule
                                    where applicabilityGroupTemplate.UserTeamId == TemplateId && applicabilityGroupTemplate.IsDeleted == false
                                    select new
                                    {
                                        applicabilityGroupTemplate.UserID,
                                        applicabilityGroupTemplate.Business,
                                        applicabilityGroupTemplate.Group,
                                        applicabilityGroupTemplate.Area,
                                        applicabilityGroupTemplate.Location,
                                        applicabilityGroupTemplate.ConfigurationColumn1,
                                        applicabilityGroupTemplate.ConfigurationColumn2,
                                        applicabilityGroupTemplate.ConfigurationColumn3,
                                        applicabilityGroupTemplate.ConfigurationColumn4,
                                        applicabilityGroupTemplate.ConfigurationColumn5,
                                        applicabilityGroupTemplate.ConfigurationColumn6,
                                        applicabilityGroupTemplate.ConfigurationColumn7,
                                        applicabilityGroupTemplate.ConfigurationColumn8,
                                        applicabilityGroupTemplate.ConfigurationColumn9,
                                        applicabilityGroupTemplate.ConfigurationColumn10,
                                        applicabilityGroupTemplate.ConfigurationColumn11,
                                        applicabilityGroupTemplate.ConfigurationColumn12,
                                        applicabilityGroupTemplate.ConditionForRules,
                                        applicabilityGroupTemplate.Id,
                                        applicabilityGroupTemplate.UserTeamId
                                       // applicabilityGroupTemplate.ApplicabilityGroupDescription
                                    }).SingleOrDefaultAsync();

            string Condition = AccessRule.ConditionForRules;
            PropertyInfo[] properties = AccessRule.GetType().GetProperties();
            List<Rules> Rules = new List<Rules>();
            string ApplicabilityGroupName = string.Empty;
            string ApplicabilityGroupDescription = string.Empty;
            int Id = 0;
            foreach (PropertyInfo rule in properties)
            {
                if (rule.Name.ToLower().Equals("applicabilityuserteam"))
                    ApplicabilityGroupName = rule.GetValue(AccessRule).ToString();
                if (rule.Name.ToLower().Equals("applicabilityuserteamescription") && rule.GetValue(AccessRule, null) != null)
                    ApplicabilityGroupDescription = rule.GetValue(AccessRule).ToString();

                if (rule.Name.ToLower().Equals("id"))
                    Id = Int32.Parse(rule.GetValue(AccessRule).ToString());

                if (rule.GetValue(AccessRule, null) != null &&
                    !rule.Name.Equals("ConditionForRules") &&
                    !rule.Name.ToLower().Equals("applicabilitygroupname") &&
                    !rule.Name.ToLower().Equals("applicabilitygroupdescription") &&
                    !rule.Name.ToLower().Equals("id"))
                {
                    Rules Rule = new Rules
                    {
                        AccessibilityParameter = rule.Name,
                        AccessibilityValue = rule.GetValue(AccessRule).ToString(),
                        Condition = Condition
                    };
                    Rules.Add(Rule);
                }
            }
            APIApplicabilityGroupTemplate ApiRule = new APIApplicabilityGroupTemplate
            {
                Id = Id,
                ApplicabilityGroupName = ApplicabilityGroupName,
                ApplicabilityGroupDescription = ApplicabilityGroupDescription
            };
            int i = 0;
            ApiRule.ApplicabilityGroupRule = new ApplicabilityGroupRules[Rules.Count];
            foreach (var rule in Rules)
            {
                ApplicabilityGroupRules applicabilityGroupRule = new ApplicabilityGroupRules
                {
                    ApplicabilityRule = rule.AccessibilityParameter,
                    ParameterValue = rule.AccessibilityValue,
                    Condition = rule.Condition
                };
                ApiRule.ApplicabilityGroupRule[i] = applicabilityGroupRule;
                i++;
            }
            return ApiRule;
        }
    }
}
