using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BiletAtolyesi;
namespace BiletBankCLClient
{
    public class BiletBankClient
    {
        #region Constants

        public static readonly string[] PaxTypes = new string[]
        {
            "ADT", // Adult    (ages between 12 and 65)
            "CHD", // Child    (ages between 2 and 12)
            "INF", // Infant   (ages between 0 and 2)
            "STD", // Student  (for students with valid student id card)
            "SRC", // Senior   (older than 65)
            "MIL"  // Military (for Turkish Military personels, id or document required)
        };

        public static readonly string[] FlightTypes = new string[]
        {
            "OW", // One-Way
            "RT", // Round-Trip
            "MP", // Multiple-Point (only allowed for international lines)
        };

        #endregion

        #region Fields

        /// <summary>
        /// Forward the output to this writer
        /// </summary>
        private TextWriter writer;

        /// <summary>
        /// The authentication header returned from the BiletBank system after the 
        /// Login method is stored in this field. If this field is null, either the 
        /// Login method is not called or returned an error. 
        /// </summary>
        private BiletAtolyesi.EBService.T_AuthenticationHeader authenticationHeader;

        /// <summary>
        /// Cached result of GetRoutingList method
        /// </summary>
        private BiletAtolyesi.EBService.Route[] routes;

        /// <summary>
        /// Instance of DAL.Airports to get CountryCode from AirportCode
        /// </summary>
        private DAL.Airports airports;

        #endregion

        #region Properties

        /// <summary>
        /// AirSearch method fills this field to use in Allocate method
        /// </summary>
        public BiletAtolyesi.EBService.T_FlightOption[] FlightOptions { get; set; }
        public BiletAtolyesi.EBService.T_FlightOption[] DepartureFlightOptions { get; set; }
        public BiletAtolyesi.EBService.T_FlightOption[] ReturnFlightOptions { get; set; }
        
        /// <summary>
        /// AirSearch method fills this field to use in Allocate method
        /// </summary>
        public BiletAtolyesi.EBService.T_RecommendationBox[] Recommendations { get; set; }

        /// <summary>
        /// Allocate and following methods fill and use this field
        /// see the notes at the end of the Allocate method for details
        /// </summary>
        public BiletAtolyesi.EBService.T_ShoppingFile ShoppingFile { get; set; }

        #endregion

        #region Constructor(s)

        /// <summary>
        ///  Default constructor
        /// </summary>
        public BiletBankClient(string dataFolder, TextWriter writer)
        {
            this.writer = writer;
            airports = new DAL.Airports();
        }

        #endregion

        #region BiletBank Web Service Flow

        #region Step-1 :: Login

        /// <summary>
        /// Logins to the BiletBank system using the provided credentials.
        /// isTest parameter, passed to the BiletBank Login method, is very important!
        /// 
        /// If isTest parameter is true, the system does not make real reservations 
        /// and bookings. Instead, test pnr code and ticket numbers will be generated. 
        /// 
        /// If isTest parameter is false, Informed that reservations/bookings will be 
        /// REAL and need to be CANCELLED as soon as possible either using the 
        /// RemoveProduct method (for reservations) or through our call-center. 
        /// 
        /// NOTE that, you should send your request to our call-center for ticket 
        /// cancellation in weekdays through 9:00 to 17:00 (Turkey time). Failure to 
        /// do so will cause chargebacks issued to you! 
        /// </summary>
        /// <param name="username">Test username provided by BiletBank</param>
        /// <param name="password">Test password provided by BiletBank</param>
        /// <param name="isTest">Whether reservations/bookings are real or not. default true</param>
        public void Login(string username, string password, bool isTest = true)
        {
            // we use the I_AuthenicationClient for Login and Logout methods
            BiletAtolyesi.EBService.I_AuthenticationClient client = new BiletAtolyesi.EBService.I_AuthenticationClient();

            BiletAtolyesi.EBService.T_LoginRequest loginRequest = CreateLoginRequest(username, password, isTest);
            BiletAtolyesi.EBService.T_LoginResult loginResult = null;

            try
            {
                loginResult = client.Login(loginRequest);
            }
            finally
            {
                client.Close(); // always close the client
            }

            // check whether there is any problem
            if (loginResult.HasError)
            {
                ThrowError(loginResult.ServiceError);
            }
            // login ok, save the header somewhere
            // Note: you can use a database, a cache server or web application's session to 
            //       store this authentication object. Beware that, you will need to call the
            //       Login method for each customer and save their header separately. 
            authenticationHeader = loginResult.AuthenticationHeader;
            // this authentication header will be used for all other methods such as AirSearch, HotelSearch etc. 
        }

        private BiletAtolyesi.EBService.T_LoginRequest CreateLoginRequest(string username, string password, bool isTest)
        {
            BiletAtolyesi.EBService.T_LoginRequest loginRequest = new BiletAtolyesi.EBService.T_LoginRequest();
            loginRequest.Form = new BiletAtolyesi.EBService.T_LoginForm();
            loginRequest.Form.Username = username;         // [required] BiletBank username
            loginRequest.Form.Password = password;         // [required] BiletBank password
            loginRequest.Form.IsTestMode = isTest;         // [optional] default false (real reservations/bookings)
            loginRequest.Form.ClientIP = "127.0.0.1";      // [optional] please send the IP of your client/customer
            loginRequest.Form.ClientName = "BB CL Client"; // [optional] a string defining your client. e.g. X Mobil Client, X Web Client etc.
            return loginRequest;
        }

        /// <summary>
        /// Logouts from the BiletBank and releases all resources on the server side. 
        /// Warning: Do not forget to close your sessions by calling this method! 
        /// </summary>
        public void Logout()
        {
            if (authenticationHeader == null)
                return; // not logged in, do nothing.

            // we use the I_AuthenicationClient for Login and Logout methods
            BiletAtolyesi.EBService.I_AuthenticationClient client = new BiletAtolyesi.EBService.I_AuthenticationClient();

            BiletAtolyesi.EBService.IO_LogOut_Request logoutRequest = new BiletAtolyesi.EBService.IO_LogOut_Request();
            logoutRequest.AuthenticationHeader = authenticationHeader;
            BiletAtolyesi.EBService.IO_LogOut_Result logoutResult = null;

            try
            {
                logoutResult = client.LogOut(logoutRequest);
            }
            finally
            {
                client.Close(); // always close the client
            }

            // check whether there is any problem
            if (logoutResult.HasError)
            {
                ThrowError(logoutResult.ServiceError);
            }

            // login ok, remove the header
            authenticationHeader = null;
        }

        #endregion

        #region Step-2 :: CheckRoutingList

        /// <summary>
        /// This method checks whether BiletBank provides a flight from 
        /// a departure point to an arrival point. 
        /// </summary>
        public bool CheckRoutingList(string departure, string arrival)
        {
            CheckAuthenticationHeader();

            if (departure == arrival) // sanity check 
                return false;
            // We do not call the GetRoutingList method if we called it before.
            // In that case, we get the result from the cache (routes field above)
            // Note: Since the routing list is subject to change, you 
            //       are advised to refresh the data in regular intervals. 
            //       You may maintain a timestamp to implement this. 
            if (routes == null)
            {
                // we need to check the routing list from the server
                BiletAtolyesi.EBService.I_ShoppingClient client = new BiletAtolyesi.EBService.I_ShoppingClient();

                BiletAtolyesi.EBService.IO_GetRoutingListRequest routingListRequest = new BiletAtolyesi.EBService.IO_GetRoutingListRequest();
                routingListRequest.AuthenticationHeader = authenticationHeader;
                BiletAtolyesi.EBService.IO_GetRoutingListResult routingListResult = null;

                try
                {
                    routingListResult = client.GetRoutingList(routingListRequest);
                }
                finally
                {
                    client.Close();
                }

                if (routingListResult.HasError)
                {
                    ThrowError(routingListResult.ServiceError);
                }
                // method ok, now check whether there exists a route from departure to arrival
                routes = routingListResult.RoutingList;
            }

            return CheckRoutingList(routes, departure, arrival);
        }

        /// <summary>
        /// This method checks whether a From-To pair exists in the given list.
        /// It performs a linear search to achive this. 
        /// </summary>
        /// <returns></returns>
        private bool CheckRoutingList(BiletAtolyesi.EBService.Route[] routes, string departure, string arrival)
        {
            BiletAtolyesi.EBService.Route route = routes.FirstOrDefault(x => x.From == departure && x.To == arrival);
            return route != null; // return true if there exists a route
        }

        Dictionary<string, HashSet<string>> cachedRoutes;

        /// <summary>
        /// This version of CheckRoutingList is more efficient since
        /// it stores from-to airports pairs in a dictionary, allowing
        /// amortized constant time access. 
        /// It first creates the dictionary if not exists, then checkes
        /// whether departure-arrival pair exists in the dictionary. 
        /// Subsequent calls to this method will be faster since 
        /// it does not create the dictionary again. 
        /// Note: You may use another data source such as a database 
        ///       or a cache server to put these results. 
        /// </summary>
        /// <returns></returns>
        private bool CheckRoutingList2(BiletAtolyesi.EBService.Route[] routes, string departure, string arrival, bool refreshCache)
        {
            if (cachedRoutes == null || refreshCache)
            {
                cachedRoutes = new Dictionary<string, HashSet<string>>();
                // construct the dictionary
                foreach (var route in routes)
                {
                    if (!cachedRoutes.ContainsKey(route.From))
                        cachedRoutes[route.From] = new HashSet<string>();
                    cachedRoutes[route.From].Add(route.To);
                }
            }
            return cachedRoutes.ContainsKey(departure) && cachedRoutes[departure].Contains(arrival);
        }

        private BiletAtolyesi.EBService.IO_GetRoutingListRequest CreateRoutingListRequest()
        {
            CheckAuthenticationHeader();

            BiletAtolyesi.EBService.IO_GetRoutingListRequest routingListRequest = new BiletAtolyesi.EBService.IO_GetRoutingListRequest();
            routingListRequest.AuthenticationHeader = authenticationHeader;
            return routingListRequest;
        }

        #endregion

        #region Step-3 :: AirSearch

        /// <summary>
        /// Calls the AirSearch method of the BiletBank service. 
        /// </summary>
        /// <param name="departure">Three letter airport or city code for departure point</param>
        /// <param name="arrival">Three letter airport or city code for arrival point</param>
        /// <param name="flightType">Two letter flight type code: OW, RT or MP</param>
        /// <param name="paxCounts">An array of 6 counts</param>
        /// <param name="departureIsCity">Specifies whether departure point is city</param>
        /// <param name="arrivalIsCity">Specifies whether arrival point is city</param>
        public void AirSearch(
            string departure, string arrival, 
            string flightType, int[] paxCounts,
            DateTime departureDate, DateTime? returnDate = null,
            bool departureIsCity = false, bool arrivalIsCity = false)
        {
            // first check whether there is a route from departure to arrival
            // since routing list contains items for only airport codes, we cannot 
            // use it if departure of arrival is city
            if (!departureIsCity && !arrivalIsCity)
            {
                if (!CheckRoutingList(departure, arrival))
                {
                    string message = String.Format("No route from {0} to {1}", departure, arrival);
                    throw new ApplicationException(message);
                }
            }
            // route check ok, continue to AirSearch method
            BiletAtolyesi.EBService.I_ShoppingClient client = new BiletAtolyesi.EBService.I_ShoppingClient();

            BiletAtolyesi.EBService.IO_AirSearchRequest searchRequest = CreateAirSearchRequest(
                departure, arrival, departureIsCity, arrivalIsCity,
                departureDate, returnDate, flightType, paxCounts);
            BiletAtolyesi.EBService.IO_AirSearchResult searchResult = null;

            try
            {
                searchResult = client.AirSearch(searchRequest);
            }
            finally
            {
                client.Close();
            }

            if (searchResult.HasError)
            {
                ThrowError(searchResult.ServiceError);
            }

            // Air search ok, display the results
            // Note: Since this is a simple command line application, we will 
            //       print the results in a simple format to the console screen. 
            //       If you are building a web application (most likely the case)
            //       you need to create HTML elements from this air search result. 
            //       You can login to our system at http://v4.biletbank.com with 
            //       your username and password to see our interface to give you 
            //       an idea on how to display the results in a web page. 

            // The air search result contains two important fields: FlightOptions and Recommendations
            // 
            // FlightOptions include results as incoming and outgoing list. Users are free to select
            // any flight from the former list and combine with any flight from the latter list. 
            // This field is used for Turkish domestic flights and for international charter flights. 
            // 
            // Recommendations include results as list of combinations. 
            // Recommendations: [Recommendation{1}, Recommendation{2}, ..., Recommendation{N}]
            // Recommendation{i}: DepartureFlights: <ListOfFlights>  (select one departure)
            //                    ReturnFlights: <ListOfFlights>     (select one return)
            //                    OtherFlights: <ListOfFlights>      (select one other leg) (for MP)
            //                    Fare (All combinations have the same price/fare)
            // ListOfFlights: [Flight{1}, Flight{2}, ..., Flight{N}]
            // Users are able to select any departure/return/other flight from a recommendation 
            // (the price is the same). However, they are not able to combine flights from one 
            // recommendation with another one. 
            // See the below DisplayFlightOptions and DisplayRecommendations methods to make things more clear

            // We set the public properties from the search result
            // Note: You actually do not need to store search results in your memory or cache. 
            //       BiletBank service already keeps these objects in storage for the allocate step.
            //       We only require flight/recommendation IDs (GUID) for allocate method. 
            //       However, since we do not want users to enter a long GUID in command line,
            //       we use integers (indexes of flights) to specify the allocated flight. 
            FlightOptions = searchResult.FlightOptions;
            DepartureFlightOptions = searchResult.FlightOptions.Where(
                x => x.OptionFlag == "OW" || x.OptionFlag == "OUT").ToArray();
            ReturnFlightOptions = searchResult.FlightOptions.Where(
                x => x.OptionFlag == "IN").ToArray();
            Recommendations = searchResult.Recommendations;
        }

        private BiletAtolyesi.EBService.IO_AirSearchRequest CreateAirSearchRequest(
            string departure, string arrival, bool departureIsCity, bool arrivalIsCity,
            DateTime departureDate, DateTime? returnDate,
            string flightType, int[] paxCounts)
        {
            CheckAuthenticationHeader();

            BiletAtolyesi.EBService.IO_AirSearchRequest searchRequest = new BiletAtolyesi.EBService.IO_AirSearchRequest();
            searchRequest.AuthenticationHeader = authenticationHeader;
            searchRequest.CreateNewShoppingFile = false; // if you would like to create a new file, set this to true
            searchRequest.Form = new BiletAtolyesi.EBService.T_AirSearchForm();
            // FlightType could be one of the following: OW : Oneway, RT : Roundtrip, MP : Multiple Points
            searchRequest.Form.FlightType = flightType;
            searchRequest.Form.Options = new BiletAtolyesi.EBService.T_AirSearch_Options();
            // FlightClass could be one of the following: "Economy", "Business", "Comfort", "First-Class"
            searchRequest.Form.Options.FlightClass = "Economy";
            // set this to true if you wish to fetch only non-stop flights
            searchRequest.Form.Options.IfDirectFlightsOnly = false;
            // always set this to true
            searchRequest.Form.Options.IfInternalNegotiatedFaresOnly = true;
            // set this to true if you wish to fetch only refundable flights 
            // (might be a good idea if you would like to create real tickets for test purposes)
            searchRequest.Form.Options.IfRefundablesOnly = false;
            // add the AITA codes of airlines you wish to fetch. 
            // for instance, if you only want the THY flights, use the following: new string[] { "TK" };
            // also, for THY and AtlasJet flights, use: new string[] { "TK", "KK" };
            // note that, you need to send an empty array, not null, if you do not have any preference,
            // in which case all airlines/carriers will be returned (recommended). 
            searchRequest.Form.Options.PreferedAirlines = new string[] { };
            // 
            List<BiletAtolyesi.EBService.T_AirSearch_PaxItem> paxItems = new List<BiletAtolyesi.EBService.T_AirSearch_PaxItem>();
            for (int i = 0; i < paxCounts.Length; ++i)
            {
                if (paxCounts[i] > 0)
                {
                    BiletAtolyesi.EBService.T_AirSearch_PaxItem paxItem = new BiletAtolyesi.EBService.T_AirSearch_PaxItem();
                    paxItem.PaxCode = PaxTypes[i];
                    paxItem.PaxCount = paxCounts[i];
                    paxItems.Add(paxItem);
                }
            }
            searchRequest.Form.PaxItems = paxItems.ToArray();
            if (flightType == "OW")
            {
                // we create a single outgoing segment for OW trips
                searchRequest.Form.Segments = new BiletAtolyesi.EBService.T_AirSearch_SegmentItem[1];
            }
            else if (flightType == "RT")
            {
                // for RT trips, we need to create to segments, 
                // one for the outbound, one for the incoming flight
                searchRequest.Form.Segments = new BiletAtolyesi.EBService.T_AirSearch_SegmentItem[2];
            }
            else
            {
                throw new NotImplementedException("MP search not implemented!");
            }

            // get departure and arrival country codes
            string departureCountry = airports.GetAirportCountryCode(departure);
            string arrivalCountry = airports.GetAirportCountryCode(arrival);

            // create the outgoing segment
            BiletAtolyesi.EBService.T_AirSearch_SegmentItem outgoingSegment = new BiletAtolyesi.EBService.T_AirSearch_SegmentItem();
            outgoingSegment.SequenceNo = 1;
            outgoingSegment.DepartureDay = departureDate.Date;
            // 
            outgoingSegment.Origin = new BiletAtolyesi.EBService.T_Airport();
            outgoingSegment.Origin.Code = departure;
            outgoingSegment.Origin.CountryCode = departureCountry;
            outgoingSegment.Origin.IsCity = departureIsCity;
            outgoingSegment.Origin.Name = ""; // send an empty string
            // 
            outgoingSegment.Destination = new BiletAtolyesi.EBService.T_Airport();
            outgoingSegment.Destination.Code = arrival;
            outgoingSegment.Destination.CountryCode = arrivalCountry;
            outgoingSegment.Destination.IsCity = arrivalIsCity;
            outgoingSegment.Destination.Name = ""; // send an empty string
            // 
            searchRequest.Form.Segments[0] = outgoingSegment;

            if (flightType == "RT" && returnDate != null)
            {
                // create the incoming segment (reverse of the outgoing segment)
                BiletAtolyesi.EBService.T_AirSearch_SegmentItem incomingSegment = new BiletAtolyesi.EBService.T_AirSearch_SegmentItem();
                incomingSegment.SequenceNo = 2;
                incomingSegment.DepartureDay = returnDate.Value.Date;
                // 
                incomingSegment.Origin = new BiletAtolyesi.EBService.T_Airport();
                incomingSegment.Origin.Code = arrival;
                incomingSegment.Origin.CountryCode = arrivalCountry;
                incomingSegment.Origin.IsCity = arrivalIsCity;
                incomingSegment.Origin.Name = ""; // send an empty string
                // 
                incomingSegment.Destination = new BiletAtolyesi.EBService.T_Airport();
                incomingSegment.Destination.Code = departure;
                incomingSegment.Destination.CountryCode = departureCountry;
                incomingSegment.Destination.IsCity = departureIsCity;
                incomingSegment.Destination.Name = ""; // send an empty string
                // 
                searchRequest.Form.Segments[1] = incomingSegment;
            }
            return searchRequest;
        }

        #endregion

        #region Step-3 :: Display

        #region Display Flight Options

        public void DisplayFlightOptions(BiletAtolyesi.EBService.T_FlightOption[] flightOptions)
        {
            if (flightOptions == null || flightOptions.Length == 0)
                return;
            // 
            BiletAtolyesi.EBService.T_FlightOption[] departures = flightOptions.Where(x => x.OptionFlag == "OW" || x.OptionFlag == "OUT").ToArray();
            BiletAtolyesi.EBService.T_FlightOption[] returns = flightOptions.Where(x => x.OptionFlag == "IN").ToArray();
            if (departures.Length == 0 || returns.Length == 0)
                return;
            // 
            writer.WriteLine("Departure Flights [{0}]:", departures.Length);
            //foreach (BiletAtolyesi.EBService.T_FlightOption flightOption in departures)
            for (int i = 0; i < departures.Length; ++i)
            {
                BiletAtolyesi.EBService.T_FlightOption flightOption = departures[i];
                DisplayFlightOption(flightOption, i + 1, writer);
            }
            writer.WriteLine("------------------------------------");
            if (returns.Length > 0)
            {
                writer.WriteLine();
                writer.WriteLine("Return Flights [{0}]:", returns.Length);
                //foreach (BiletAtolyesi.EBService.T_FlightOption flightOption in returns)
                for (int i = 0; i < returns.Length; ++i)
                {
                    BiletAtolyesi.EBService.T_FlightOption flightOption = returns[i];
                    DisplayFlightOption(flightOption, i + 1, writer);
                }
                writer.WriteLine("------------------------------------");
                writer.WriteLine();
            }
        }

        private void DisplayFlightOption(BiletAtolyesi.EBService.T_FlightOption flightOption,
            int index, TextWriter writer)
        {
            BiletAtolyesi.EBService.T_Segment first = flightOption.Segments.First();
            BiletAtolyesi.EBService.T_Segment last = flightOption.Segments.Last();
            string line = String.Format("{0}) {1} {2}-{3} {4:dd-MM-yyyy} {5}-{6} {7} {8} {9,7:0.00} {10}",
                index.ToString().PadLeft(3, ' '), first.MarketingAirline, first.OD_OriginCode,
                first.OD_DestinationCode, first.DepartureDay, first.DepartureTime, last.ArrivalTime,
                first.FlightNumber.PadRight(8, ' '), (first.BookingClass ?? "").PadRight(2, ' '), 
                flightOption.TotalFare, flightOption.Currency);
            writer.WriteLine(line);
        }

        private void DisplayFlightOptionDetails(BiletAtolyesi.EBService.T_FlightOption flightOption)
        {
            if (flightOption == null)
            {
                writer.WriteLine();
                return;
            }
            BiletAtolyesi.EBService.T_Segment[] segments = flightOption.Segments;
            BiletAtolyesi.EBService.T_SegmentAvailability[] segmentsAvails = flightOption.SegmentAvailabilities;
            for (int i = 0; i < segments.Length; ++i)
            {
                BiletAtolyesi.EBService.T_Segment segment = segments[i];
                BiletAtolyesi.EBService.T_SegmentAvailability segmentAvail = segmentsAvails.FirstOrDefault(x => x.SegmentSequenceNo == segment.SequenceNo);
                string index = (i + 1).ToString().PadLeft(3, ' ');
                string line = String.Format("{0}) {1} {2}-{3} {4:dd-MM-yyyy} {5}-{6} {7} {8} {9}",
                    index, segment.MarketingAirline, segment.OriginCode, segment.DestinationCode,
                    segment.DepartureDay, segment.DepartureTime, segment.ArrivalTime,
                    segment.FlightNumber.PadRight(8, ' '), (segment.BookingClass ?? "").PadRight(2, ' '), 
                    segmentAvail.AvailableSeats);
                writer.WriteLine(line);
            }
            writer.WriteLine();
        }

        public void DisplayFlightOptionDetails(int flightNo, bool isDeparture)
        {
            BiletAtolyesi.EBService.T_FlightOption flightOption = GetFlightOption(flightNo, isDeparture);
            DisplayFlightOptionDetails(flightOption);
        }

        #endregion

        #region Display Recommendations

        public void DisplayRecommendations(BiletAtolyesi.EBService.T_RecommendationBox[] recommendations)
        {
            if (recommendations == null || recommendations.Length == 0)
                return;
            // 
            writer.WriteLine("Recommendations [{0}]:", recommendations.Length);
            for (int i = 0; i < recommendations.Length; ++i)
            {
                string index = (i + 1).ToString().PadLeft(3, ' ');
                BiletAtolyesi.EBService.T_RecommendationBox recommendation = recommendations[i];
                writer.WriteLine("{0}) Recommendation: {1,7:0.00} {2}",
                    index, recommendation.FareInfo.GrandTotal, recommendation.FareInfo.Currency);
                // 
                writer.WriteLine("     Departure Flights:");
                DisplayFlights(recommendation.DepartureFlights);
                writer.WriteLine("     -------------------------------");
                // 
                writer.WriteLine("     Return Flights:");
                DisplayFlights(recommendation.ReturnFlights);
                writer.WriteLine("     -------------------------------");
                // 
                // TODO: print OtherFlights for MP (multiple points) case
            }
            writer.WriteLine();
        }

        private void DisplayFlights(BiletAtolyesi.EBService.A_Flight[] flights)
        {
            for (int i = 0; i < flights.Length; ++i)
            {
                string index = (i + 1).ToString().PadLeft(3, ' ');
                BiletAtolyesi.EBService.A_Flight flight = flights[i];
                BiletAtolyesi.EBService.A_FlightSegment first = flight.Segments.First();
                BiletAtolyesi.EBService.A_FlightSegment last = flight.Segments.Last();
                string line = String.Format("     {0} {1} {2}-{3} {4:dd-MM-yyyy} {5}-{6} {7} {8} {9}",
                    index, first.MarketingAirline, first.DepartureAirport, last.ArrivalAirport,
                    first.DepartureDate, first.DepartureTime, last.ArrivalTime, first.MarketingAirline,
                    first.FlightNumber.PadRight(8, ' '), first.BookingClass.PadRight(2, ' '));
                writer.WriteLine(line);
            }
        }

        private void DisplayFlightDetails(BiletAtolyesi.EBService.A_Flight flight)
        {
            if (flight == null)
            {
                writer.WriteLine();
                return;
            }
            BiletAtolyesi.EBService.A_FlightSegment[] segments = flight.Segments;
            for (int i = 0; i < segments.Length; ++i)
            {
                BiletAtolyesi.EBService.A_FlightSegment segment = segments[i];
                string index = (i + 1).ToString().PadLeft(3, ' ');
                string line = String.Format("{0}) {1} {2}-{3} {4:dd-MM-yyyy} {5}-{6} {7} {8} {9:00}",
                    index, segment.MarketingAirline, segment.DepartureAirport, segment.ArrivalAirport,
                    segment.DepartureDate, segment.DepartureTime, segment.ArrivalTime,
                    segment.FlightNumber.PadRight(8, ' '), segment.BookingClass.PadRight(2, ' '), 
                    segment.Availability);
                writer.WriteLine(line);
            }
            writer.WriteLine();
        }

        public void DisplayFlightDetails(int recNo, int flightNo, bool isDeparture)
        {
            BiletAtolyesi.EBService.A_Flight flight = GetFlight(recNo, flightNo, isDeparture);
            DisplayFlightDetails(flight);
        }

        #endregion

        public void DisplayAirSearchResults()
        {
            writer.WriteLine();
            if (FlightOptions != null)
            {
                writer.WriteLine("FLIGHT OPTIONS");
                DisplayFlightOptions(FlightOptions);
                writer.WriteLine();
            }
            if (Recommendations != null)
            {
                writer.WriteLine("RECOMMENDATIONS");
                DisplayRecommendations(Recommendations);
                writer.WriteLine();
            }
        }

        #endregion

        #region Step-4 :: Allocate

        /// <summary>
        /// Allocates the selected flight option (Turkish domestic or international low-cost)
        /// using the BiletBank service's Allocate method. 
        /// </summary>
        /// <param name="departureNo">The index of the selected departure flight in DepartureFlights array</param>
        /// <param name="returnNo">The index of the selected return flight in ReturnFlights array (-1 if there are no return flights)</param>
        /// <param name="serviceCharge">Extra service charge amount in TRY</param>
        public void AllocateFlightOption(int departureNo, int returnNo, decimal serviceCharge)
        {
            BiletAtolyesi.EBService.T_FlightOption departureFlight = GetFlightOption(departureNo, true);
            BiletAtolyesi.EBService.T_FlightOption returnFlight = GetFlightOption(returnNo, false);

            BiletAtolyesi.EBService.I_ShoppingClient client = new BiletAtolyesi.EBService.I_ShoppingClient();

            BiletAtolyesi.EBService.IO_AllocateRequest allocateRequest = CreateAllocateRequest(
                departureFlight, returnFlight, serviceCharge);
            BiletAtolyesi.EBService.IO_AllocateResult allocateResult = null;

            try
            {
                allocateResult = client.Allocate(allocateRequest);
            }
            finally
            {
                client.Close();
            }

            if (allocateResult.HasError)
            {
                ThrowError(allocateResult.ServiceError);
            }
            // allocate ok
        }

        private BiletAtolyesi.EBService.IO_AllocateRequest CreateAllocateRequest(
            BiletAtolyesi.EBService.T_FlightOption departureFlight, 
            BiletAtolyesi.EBService.T_FlightOption returnFlight,
            decimal serviceCharge)
        {
            CheckAuthenticationHeader();

            BiletAtolyesi.EBService.IO_AllocateRequest allocateRequest = new BiletAtolyesi.EBService.IO_AllocateRequest();
            allocateRequest.AuthenticationHeader = authenticationHeader;
            allocateRequest.Form = new BiletAtolyesi.EBService.IO_AllocateForm();
            allocateRequest.Form.Campaigns = null; // ignore
            if (returnFlight != null)
            {
                // two flight options to select (both departure and return)
                allocateRequest.Form.SelectedItems = new BiletAtolyesi.EBService.IO_AllocationItem[2];
            }
            else
            {
                // only one flight option to select (departure)
                allocateRequest.Form.SelectedItems = new BiletAtolyesi.EBService.IO_AllocationItem[1];
            }
            // departure flight selection
            // send the ProductId of the selected departure flight
            allocateRequest.Form.SelectedItems[0].ProductId = departureFlight.ProductId;
            if (serviceCharge == 0)
            {
                // set a non-zero value for each pax if you would like to add extra service charge
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee = null; // no extra-sc
            }
            else
            {
                // extra sc values should be between 0 and 25 TRY for Turkish domestic flights 
                //                       and between 0 and 75 TRY for all international low-cost flights
                // for round-trip flights, if the marketing airlines are the same one service charge is 
                // applied for each pax. On the other hand, if the marketing airlines are different, 
                // for instance, departure TK (Turkish Airlines0 and return PC (Pegasus), two separate 
                // service charge are applied and trips are booked separately; one ticket for TK flight 
                // and one ticket for PC flight. 
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee = new BiletAtolyesi.EBService.IO_ServiceFeeSelection();
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee.Amount = serviceCharge; // TRY
            }
            // SubOptions are only used for recommendation results (see the AllocateRecommendation method)
            allocateRequest.Form.SelectedItems[0].SubOptions = null;
            // 
            if (returnFlight != null)
            {
                // return flight selection
                // send the ProductId of the selected return flight
                allocateRequest.Form.SelectedItems[1].ProductId = returnFlight.ProductId;
                if (serviceCharge == 0)
                {
                    // set a non-zero value for each pax if you would like to add extra service charge
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee = null; // no extra-sc
                }
                else
                {
                    // extra sc values should be between 0 and 25 TRY for Turkish domestic flights 
                    //                       and between 0 and 75 TRY for all international low-cost flights
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee = new BiletAtolyesi.EBService.IO_ServiceFeeSelection();
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee.Amount = serviceCharge; // TRY
                }
                // SubOptions are only used for recommendation results (see the AllocateRecommendation method)
                allocateRequest.Form.SelectedItems[1].SubOptions = null;
            }
            // 
            return allocateRequest;
        }

        /// <summary>
        /// Allocates desired flights (departure and/or return) from the selected recommendation box 
        /// using the BiletBank service's Allocate method. Remember that recommendation boxes are used
        /// for international GDS content (Amadeus). 
        /// </summary>
        /// <param name="recNo">The index of the selected recommendation box in Recommendations array</param>
        /// <param name="departureNo">The index of the selected departure flight in the selected recommendation</param>
        /// <param name="returnNo">The index of the selected return flight in the selected recommendation (-1 if there are no return flights)</param>
        /// <param name="serviceCharge">Service charge amount in TRY</param>
        public void AllocateRecommendation(int recNo, int departureNo, int returnNo, decimal serviceCharge)
        {
            BiletAtolyesi.EBService.T_RecommendationBox recommendation = GetRecommendation(recNo);
            BiletAtolyesi.EBService.A_Flight departureFlight = GetFlight(recNo, departureNo, true);
            BiletAtolyesi.EBService.A_Flight returnFlight = GetFlight(recNo, returnNo, false);

            BiletAtolyesi.EBService.I_ShoppingClient client = new BiletAtolyesi.EBService.I_ShoppingClient();

            BiletAtolyesi.EBService.IO_AllocateRequest allocateRequest = CreateAllocateRequest(
                recommendation, departureFlight, returnFlight, serviceCharge);
            BiletAtolyesi.EBService.IO_AllocateResult allocateResult = null;

            try
            {
                allocateResult = client.Allocate(allocateRequest);
            }
            finally
            {
                client.Close();
            }

            if (allocateResult.HasError)
            {
                ThrowError(allocateResult.ServiceError);
            }
            // allocate ok, save the returned shopping file somewhere (in case we need it)
            // note that you don't actually need to store the shopping file anywhere
            // we are storing the result in this application since we need the GUID ids 
            // in the following methods. 
            ShoppingFile = allocateResult.ShoppingFile;
        }

        private BiletAtolyesi.EBService.IO_AllocateRequest CreateAllocateRequest(
            BiletAtolyesi.EBService.T_RecommendationBox recommendation, 
            BiletAtolyesi.EBService.A_Flight departureFlight, 
            BiletAtolyesi.EBService.A_Flight returnFlight,
            decimal serviceCharge)
        {
            CheckAuthenticationHeader();

            BiletAtolyesi.EBService.IO_AllocateRequest allocateRequest = new BiletAtolyesi.EBService.IO_AllocateRequest();
            allocateRequest.AuthenticationHeader = authenticationHeader;
            allocateRequest.Form = new BiletAtolyesi.EBService.IO_AllocateForm();
            allocateRequest.Form.Campaigns = null; // ignore
            if (returnFlight != null)
            {
                // only one flight option to select (departure)
                allocateRequest.Form.SelectedItems = new BiletAtolyesi.EBService.IO_AllocationItem[2];
            }
            else
            {
                // two flight options to select (both departure and return)
                allocateRequest.Form.SelectedItems = new BiletAtolyesi.EBService.IO_AllocationItem[1];
            }
            // departure flight selection
            // send the ProductId of the selected recommendation box
            allocateRequest.Form.SelectedItems[0].ProductId = recommendation.ProductId;
            // set a service charge for each product-item
            // note that unline flight options, recommendations boxes have flexible service charge policy
            // in other words, you can set any service charge between 0 and 999 for each pax. 
            if (serviceCharge == 0)
            {
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee = null;
            }
            else
            {
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee = new BiletAtolyesi.EBService.IO_ServiceFeeSelection();
                allocateRequest.Form.SelectedItems[0].SelectedServiceFee.Amount = serviceCharge; // TRY
            }
            // set which departure, return and other flights are selected
            // correctly set the flight id of the selected flight from the departure flight list
            allocateRequest.Form.SelectedItems[0].SubOptions = new Guid[] { departureFlight.FlightId };
            // 
            if (returnFlight != null)
            {
                // return flight selection
                // send the ProductId of the selected recommendation box
                allocateRequest.Form.SelectedItems[1].ProductId = recommendation.ProductId;
                // set a service charge for each product-item
                // note that unline flight options, recommendations boxes have flexible service charge policy
                // in other words, you can set any service charge between 0 and 999 for each pax.
                if (serviceCharge == 0)
                {
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee = null;
                }
                else
                {
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee = new BiletAtolyesi.EBService.IO_ServiceFeeSelection();
                    allocateRequest.Form.SelectedItems[1].SelectedServiceFee.Amount = serviceCharge; // TRY
                }
                // set which departure, return and other flights are selected
                // correctly set the flight id of the selected flight from the departure flight list
                allocateRequest.Form.SelectedItems[1].SubOptions = new Guid[] { returnFlight.FlightId };
            }
            // 
            return allocateRequest;
        }

        #endregion

        #region Step-5 :: UpdatePassengers

        /// <summary>
        /// Calls the UpdatePassengers method of BiletBank service. 
        /// </summary>
        /// <param name="passengers">Array of passsenger info</param>
        public void UpdatePassengers(Passenger[] passengers)
        {
            BiletAtolyesi.EBService.I_ShoppingClient client = new BiletAtolyesi.EBService.I_ShoppingClient();

            BiletAtolyesi.EBService.IO_UpdatePassengersRequest updatePassengersRequest = 
                CreateUpdatePassengersRequest(passengers);
            BiletAtolyesi.EBService.IO_UpdatePassengersResult updatePassengersResult = null;

            try
            {
                updatePassengersResult = client.UpdatePassengers(updatePassengersRequest);
            }
            finally
            {
                client.Close();
            }

            if (updatePassengersResult.HasError)
            {
                ThrowError(updatePassengersResult.ServiceError);
            }
            // update passengers ok, save the new shopping file
            // BiletBank service always returns the latest, up-to-date shopping file
            // with all products and stuff. 
            ShoppingFile = updatePassengersResult.ShoppingFile;
        }

        private BiletAtolyesi.EBService.IO_UpdatePassengersRequest CreateUpdatePassengersRequest(Passenger[] passengers)
        {
            CheckAuthenticationHeader();
            CheckShoppingFile();

            BiletAtolyesi.EBService.IO_UpdatePassengersRequest upRequest = new BiletAtolyesi.EBService.IO_UpdatePassengersRequest();
            upRequest.AuthenticationHeader = authenticationHeader;
            upRequest.Form = new BiletAtolyesi.EBService.IO_UpdatePassengersForm();
            // get product id's from the shopping file (returned by the Allocate method)
            upRequest.Form.ProductIds = ShoppingFile.AirBookings.Select(x => x.ProductId).ToArray();
            // set new passengers
            upRequest.Form.NewPassengers = new BiletAtolyesi.EBService.T_Passenger[passengers.Length];
            BiletAtolyesi.EBService.T_AirBookingItem[] bookingItems = ShoppingFile.AirBookings.First().BookingItems;
            for (int i = 0; i < passengers.Length; ++i)
            {
                upRequest.Form.NewPassengers[i] = ConvertPassenger(passengers[i], bookingItems[i]);
                upRequest.Form.NewPassengers[i].SequenceNo = i + 1;
            }
            // set the first passenger as contact [required] (others are false by default)
            if (upRequest.Form.NewPassengers.Length > 0)
            {
                upRequest.Form.NewPassengers[0].IfContact = true;
            }
            return upRequest;
        }

        private BiletAtolyesi.EBService.T_Passenger ConvertPassenger(Passenger passenger, BiletAtolyesi.EBService.T_AirBookingItem bookingItem)
        {
            BiletAtolyesi.EBService.T_Passenger p = new BiletAtolyesi.EBService.T_Passenger();
            p.FirstName = passenger.FirstName;   // First name of the passenger
            p.FirstName = passenger.LastName;    // Last name of the passenger
            p.Type = passenger.PaxType;          // Pax type: ADT, CHD, INF, STD, SRC or MIL
            p.Gender = passenger.Gender;         // Gender: "M" -> (M)ale or "F" -> (F)emale
            p.BirthDate = passenger.BirthDate.ToString("yyyy-MM-dd"); // Birthdate format: yyyy-mm-dd, i.e. 1990-07-10 -> 10th of July 1990
            p.Email = passenger.Email;           // Email address of the passenger
            p.Phone = passenger.Phone;           // Mobile phone of the passenger
            p.PassportNo = passenger.PassportNo; // Used for international flights
            p.CitizenNo = passenger.CitizenNo;   // Used for Turkish passengers (set "11111111111" if not applicable)
            // we need to set the pax reference id of this passenger to the TempTag of the T_Passenger
            // this is important, failing to set this will cause PaxReferenceException
            Guid paxReferenceId = bookingItem.PaxReference.PaxReferenceId;
            p.TempTag = paxReferenceId.ToString();
            return p;
        }

        #endregion

        #region Step-6 :: MakePayment



        #endregion

        #region Step-7 :: FinalizeShopping



        #endregion

        #endregion

        #region Auxilary Methods

        public BiletAtolyesi.EBService.T_FlightOption GetFlightOption(int flightNo, bool isDeparture)
        {
            if (flightNo < 1)
                return null;
            // 
            flightNo -= 1;
            if (isDeparture)
            {
                if (DepartureFlightOptions == null || flightNo > DepartureFlightOptions.Length)
                    return null;
                return DepartureFlightOptions[flightNo];
            }
            else
            {
                if (ReturnFlightOptions == null || flightNo > ReturnFlightOptions.Length)
                    return null;
                return ReturnFlightOptions[flightNo];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recNo"></param>
        /// <param name="flightNo"></param>
        /// <param name="isDeparture"></param>
        /// <returns></returns>
        public BiletAtolyesi.EBService.A_Flight GetFlight(int recNo, int flightNo, bool isDeparture)
        {
            if (recNo < 1 || flightNo < 1)
                return null;
            // 
            flightNo -= 1;
            BiletAtolyesi.EBService.T_RecommendationBox recommendation = GetRecommendation(recNo); ;
            BiletAtolyesi.EBService.A_Flight[] flights = null;
            if (isDeparture)
                flights = recommendation.DepartureFlights;
            else
                flights = recommendation.ReturnFlights;
            if (flights == null || flightNo > flights.Length)
                return null;
            return flights[flightNo];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recNo"></param>
        /// <returns></returns>
        public BiletAtolyesi.EBService.T_RecommendationBox GetRecommendation(int recNo)
        {
            if (recNo < 1)
                return null;
            // 
            recNo -= 1;
            if (Recommendations == null || recNo > Recommendations.Length)
                return null;
            BiletAtolyesi.EBService.T_RecommendationBox recommendation = Recommendations[recNo];
            return recommendation;
        }

        /// <summary>
        /// Determines whether the latest search is round-trip or one-way
        /// </summary>
        /// <returns>true if trip is RT, otherwise false</returns>
        public bool IsSearchRoundTrip()
        {
            if ((DepartureFlightOptions != null && DepartureFlightOptions.Length > 0) 
                && (ReturnFlightOptions == null || ReturnFlightOptions.Length == 0))
            {
                return true;
            }
            if (Recommendations != null && Recommendations.Length > 0)
            {
                BiletAtolyesi.EBService.T_RecommendationBox first = Recommendations[0];
                return first.ReturnFlights == null || first.ReturnFlights.Length == 0;
            }
            return false;
        }

        /// <summary>
        /// Checks the authenticationHeader field:
        /// if it is null, throws an error, otherwise does nothing
        /// </summary>
        private void CheckAuthenticationHeader()
        {
            if (authenticationHeader == null)
            {
                string message = "Not allowed! First call the Login method";
                throw new ApplicationException(message);
            }
        }

        private void CheckShoppingFile()
        {
            if (ShoppingFile == null)
            {
                string message = "Shopping file is null. You may perhaps forget to call allocate?";
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Checks the service error field and if it is not null, 
        /// throws an exception with the error messages returned 
        /// from the server. 
        /// </summary>
        /// <param name="error"></param>
        private void ThrowError(BiletAtolyesi.EBService.T_ServiceError error)
        {
            if (error == null)
                return;
            // In case you get an error from the BiletBank server,
            // please check the contents of this error object and
            // if you think the problem is due to BiletBank, send 
            // us and e-mail (destek@petour.com) with the SessionId. 
            // Note: It is really important that you send us the 
            //       SessionId since we access the logs using this id. 
            // SessionId is stored in authenticated header. 
            // If you get an error in Login method, you will not have 
            // a SessionId but for other methods you will have to report
            // the SessionId returned by the Login method. 
            string message = "";
            // get the SessionId if exists
            if (authenticationHeader != null)
            {
                message = String.Format("[SessionId: {0}]", authenticationHeader.SessionId);
            }
            message = String.Format("{0} Error: {1} {2} {3} {4} {5}",
                message, error.Name, error.ErrorMessage,
                error.DebugMessage, error.ErrorDefinitionId, error.ErrorInstanceId);
            throw new ApplicationException(message);
        }

        #endregion
    }
}
