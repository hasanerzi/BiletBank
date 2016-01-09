using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiletBankCLClient.DAL.Entity
{
    /// <summary>
    /// AirportCode;AirportName;CountryCode;CountryName;LocalizedCountryName;CityCode;CityName;LocalizedCityName;StateCode;StateName;Rating;IfActive
    /// AAA;Anaa;PF;French Polynesia;Fransız Polinezyası;AAA;Anaa;NULL;NULL;NULL;0;1
    /// </summary>
    public class Airport
    {
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string LocalizedCountryName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string LocalizedCityName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public int? Rating { get; set; }
        public bool IfActive { get; set; }
    }
}
