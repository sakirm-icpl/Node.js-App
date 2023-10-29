using Gadget.API.Helper;
using Gadget.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using log4net;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ValuesController));

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            try
            {
                return new string[] { "value1", "value2" };
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;   
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            try
            {
                return "value";
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPost("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete]
        public void Delete([FromQuery]string id)
        {
            int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
        }
    }
}
