using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BiletAtolyesi.DAL;

namespace BiletBankCLClient.DAL
{
    public class Airports
    {
        /// <summary>
        /// Map from <airport-code/city-code> to <airport>
        /// </summary>
        public List<Entity.Airport> airports;

        public Airports(string fileName)
        {
            LoadAirports(fileName);
        }

        /// <summary>
        /// Loads airport information from the given CSV file: <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">Path of the CSV file</param>
        public void LoadAirports(string fileName)
        {
            airports = new List<Entity.Airport>();

            //var dbairport=dbcontext.Location.Select(x => x);

            //foreach (var item in dbairport)
            //{
            //    var airport = new Entity.Airport();
            //    airport.AirportCode = item.AirportCode;
            //    airport.AirportName = item.AirportName;
            //    airport.CityCode = item.CityCode;
            //    airport.CityName = item.CityName;
            //    airport.CountryCode = item.CountryCode;
            //    airport.CountryName = item.CountryName;
            //    airport.IfActive = Convert.ToBoolean(item.IfActive);
            //    airport.LocalizedCityName = item.LocalizedCityName;
            //    airport.LocalizedCountryName = item.LocalizedCountryName;
            //    airport.Rating = item.Rating;
            //    airport.StateCode = item.StateCode;
            //    airport.StateName = item.StateName;
            //    airports[airport.AirportCode] = airport;
            //}
            using (var reader = new StreamReader(File.OpenRead(fileName), Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    // AirportCode;AirportName;CountryCode;CountryName;LocalizedCountryName;CityCode;CityName;LocalizedCityName;StateCode;StateName;Rating;IfActive
                    // ADB;Adnan Menderes;TR;Turkey;Türkiye;IZM;Izmir;İzmir;NULL;NULL;88420;1
                    string line = reader.ReadLine();
                    string[] values = line.Split(';');
                    if (values.Length < 12)
                        continue;
                    Entity.Airport airport = new Entity.Airport();
                    airport.AirportCode = ToString(values[0]);
                    airport.AirportName = ToString(values[1]);
                    airport.CountryCode = ToString(values[2]);
                    airport.CountryName = ToString(values[3]);
                    airport.LocalizedCountryName = ToString(values[4]);
                    airport.CityCode = ToString(values[5]);
                    airport.CityName = ToString(values[6]);
                    airport.LocalizedCityName = ToString(values[7]);
                    airport.StateCode = ToString(values[8]);
                    airport.StateName = ToString(values[9]);
                    airport.Rating = ToInteger(values[10]);
                    airport.IfActive = ToBoolean(values[11]);
                    if (!airport.IfActive || String.IsNullOrWhiteSpace(airport.AirportCode))
                        continue;
                    airports.Add(airport);
                }
            }
        }

        public Entity.Airport GetAirport(string code)
        {
            var airport = airports.Where(x => x.AirportCode == code).FirstOrDefault();
            return airport;           
        }

        public string GetAirportCountryCode(string code)
        {
            Entity.Airport airport = GetAirport(code);
            if (airport == null)
                return null;
            return airport.CountryCode;
        }

        #region Helper Methods

        private string ToString(string str)
        {
            if (str == null || str == "NULL")
                return null;
            return str.Trim().ToUpper();
        }

        private int? ToInteger(string str)
        {
            try
            {
                return Int32.Parse(str.Trim());
            }
            catch
            {
                return null;
            }
        }

        private bool ToBoolean(string str)
        {
            if (str == "1")
                return true;
            else
                return false;
        }

        #endregion
    }
}
