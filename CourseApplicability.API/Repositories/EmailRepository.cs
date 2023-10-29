using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using CourseApplicability.API.Repositories.Interfaces;
using CourseApplicability.API.Services;
using CourseApplicability.API.APIModel;
using CourseApplicability.API.Helper;

namespace CourseApplicability.API.Repositories
{
    public class EmailRepository : IEmail
    {
        ICustomerConnectionStringRepository _customerConnection;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;
      //  private readonly INotification _notification;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailRepository));

        public EmailRepository(IConfiguration configuration, IIdentityService identityService, ICustomerConnectionStringRepository customerConnection)
        {
            this._customerConnection = customerConnection;
            _configuration = configuration;
            _identityService = identityService;
            //this._notification = notification;
            //INotification notification,
        }

        public async Task<int> SendCourseRequestApprovalMailToUsers(List<APINodalRequestList> aPINodalRequestList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/CourseRequestApprovalMailToUsers";
            string Body = JsonConvert.SerializeObject(aPINodalRequestList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }

        public async Task<int> SendSelfCourseRequestMail(APISelfCourseRequestEmail aPISelfCourseRequestEmail)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/SelfCourseRequestMailToUser";
            string Body = JsonConvert.SerializeObject(aPISelfCourseRequestEmail);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }

        public async Task<int> SendCourseRequestRejectedMailToUsers(List<APINodalRequestList> aPINodalRequestList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/CourseRequestRejectionMailToUsers";
            string Body = JsonConvert.SerializeObject(aPINodalRequestList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }

        public async Task<int> UserSignUpMailToNodalOfficers(List<APINodalUserDetailsEmail> aPINodalUserDetailsList)
        {
            string Url = _configuration[Configuration.NotificationApi];
            Url += "/UserSignUpMailToNodalOfficers";
            string Body = JsonConvert.SerializeObject(aPINodalUserDetailsList);

            HttpResponseMessage response = ApiHelper.CallPostAPI(Url, Body).Result;
            return 1;
        }
    }
}
