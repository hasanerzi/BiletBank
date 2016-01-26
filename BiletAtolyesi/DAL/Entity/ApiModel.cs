using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiletAtolyesi.DAL.Entity
{
    public class AirSearchModel {
        public BiletAtolyesi.EBService.T_FlightOption[] FlightOptions { get; set; }
        public BiletAtolyesi.EBService.T_FlightOption[] DepartureFlightOptions { get; set; }
        public BiletAtolyesi.EBService.T_FlightOption[] ReturnFlightOptions { get; set; }
        public BiletAtolyesi.EBService.T_RecommendationBox[] Recommendations { get; set; }
    }
}