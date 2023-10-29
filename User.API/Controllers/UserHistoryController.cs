using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.API.Helper.Log_API_Count;
using User.API.Repositories.Interfaces;

namespace User.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Produces("application/json")]
    [Route("api/v1/UserHistory")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    public class UserHistoryController : Controller
    {
        private IUserHistoryRepository _userHistory;
        public UserHistoryController(IUserHistoryRepository userHistory)
        {
            this._userHistory = userHistory;
        }


    }
}