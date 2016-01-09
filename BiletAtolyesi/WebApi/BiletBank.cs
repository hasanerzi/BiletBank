using BiletAtolyesi.DAL;
using BiletBankCLClient.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BiletAtolyesi.WebApi
{
    public class BiletBank : ApiController
    {
        BILETBANKEntities dbcontext = new BILETBANKEntities();
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public HttpResponseMessage GetAirports()
        {
            Airports airport;
           try
            {
                airport = new Airports();

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

           var response = Request.CreateResponse(HttpStatusCode.Created, airport);

            return response;
        }

        // PUT api/<controller>/5
        public void Put(int id, string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}