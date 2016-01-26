﻿
using BiletAtolyesi.DAL.Entity;
using BiletBankCLClient;
using BiletBankCLClient.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BiletAtolyesi.WebApi
{
    public class BiletBankController : ApiController
    {

        BiletBankClient client = new BiletBankClient(@"C:\Data");
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
                airport = new Airports(@"C:\Data");

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

           var response = Request.CreateResponse(HttpStatusCode.Created, airport);

            return response;
        }

        public HttpResponseMessage SearchAir(string departure,string arrival,string flightType,int[] paxCounts,DateTime departureDate,DateTime returnDate,bool departureIsCity,bool arrivalIsCity)
        {
            AirSearchModel result;
            try
            {
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