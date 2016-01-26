
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

           var response = Request.CreateResponse(HttpStatusCode.Created, airports.airports.Take(100));

            return response;
        }

        public HttpResponseMessage SearchAir(string departure, string arrival, string flightType, string paxes, DateTime departureDate, DateTime returnDate, bool departureIsCity = false, bool arrivalIsCity = false)
        {
            AirSearchModel result;
            try
            {
            string[] paxItems = paxes.Split(';');
            int[] paxCounts = new int[BiletBankClient.PaxTypes.Length];
            foreach (string paxItem in paxItems)
            {
                string[] pair = paxItem.Split('/');
                string paxType = pair[0];
                int count = 1;
                if (pair.Length == 2)
                    Int32.TryParse(pair[1], out count);
                int indexOf = Array.IndexOf(BiletBankClient.PaxTypes, paxType);
                if (indexOf >= 0 && indexOf < paxCounts.Length)
                    paxCounts[indexOf] = count;
            }
              result = new AirSearchModel();

                client.AirSearch(departure, arrival, flightType, paxCounts, departureDate, returnDate, departureIsCity, arrivalIsCity);
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