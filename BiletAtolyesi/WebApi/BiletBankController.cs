
using BiletAtolyesi.DAL.Entity;
using BiletBankCLClient;
using BiletBankCLClient.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace BiletAtolyesi.WebApi
{
    public class BiletBankController : ApiController
    {
        BiletBankClient client = new BiletBankClient(HttpContext.Current.Server.MapPath("~\\App_Data\\Airports.csv"));
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
            Airports airports;
           try
            {
                airports = client.airports;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

           var response = Request.CreateResponse(HttpStatusCode.Created, airports);

            return response;
        }
        
        public HttpResponseMessage GetAir(string test)
        {
         
            var response = Request.CreateResponse(HttpStatusCode.Created, "test");

            return response;
        }
         [HttpGet]
        public HttpResponseMessage GetAir(string departure, string arrival, string flightType, string pax , DateTime departureDate, DateTime returnDate)
        {
             // bool departureIsCity = false, bool arrivalIsCity = false
            AirSearchModel result;
            pax = "ADT/1;CHD/0;INF/0;STD/0;SRC/0;MIL/0";
            try
            {
              result = new AirSearchModel();

                var paxlist = pax.Split(';');
                var paxCounts = new int[6];
                for (var i = 0; i < paxlist.Length; i++)
                {
                    var temp= paxlist[i].Split('/');
                    paxCounts[i] = Convert.ToInt32(temp[1]);
                }

                client.AirSearch(departure, arrival, flightType, paxCounts, departureDate, returnDate);
                result.DepartureFlightOptions = client.DepartureFlightOptions;
                result.FlightOptions = client.FlightOptions;
                result.ReturnFlightOptions = client.ReturnFlightOptions;
                result.Recommendations = client.Recommendations;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            var response = Request.CreateResponse(HttpStatusCode.Created, result);

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