using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BiletBankCLClient
{
    public class Passenger
    {
        // <name>/<lastname>/<type>/<gender>/<birthdate>/<email>/<phone>
        // JOHN/DOE/ADT/M/10101990/JOHN@EXAMPLE.COM/05555555555

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PaxType { get; set; }
        public string Gender { get; set; } // M/F
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CitizenNo { get; set; }
        public string PassportNo { get; set; }

        public static Passenger Parse(string text)
        {
            try
            {
                string[] items = text.Split('/');
                Passenger passenger = new Passenger();
                if (items.Length >= 1)
                    passenger.FirstName = items[0];
                if (items.Length >= 2)
                    passenger.LastName = items[1];
                if (items.Length >= 3)
                    passenger.PaxType = items[2];
                if (items.Length >= 4)
                    passenger.Gender = items[3];
                if (items.Length >= 5)
                    passenger.BirthDate = DateTime.ParseExact(items[4], "ddMMyyyy", CultureInfo.GetCultureInfo("tr-TR"));
                if (items.Length >= 6)
                    passenger.Email = items[5];
                if (items.Length >= 7)
                    passenger.Phone = items[6];
                if (items.Length >= 8)
                    passenger.CitizenNo = items[7];
                if (items.Length >= 9)
                    passenger.PassportNo = items[8];
                return passenger;
            }
            catch
            {
                throw new ApplicationException("Illegal passenger record: \"" + text + "\"");
            }
        }

        public override string ToString()
        {
            string ret = String.Format("{0}/{1}/{2}/{3}/{4:ddMMyyyy}/{5}/{6}",
                FirstName, LastName, PaxType, Gender,
                BirthDate, Email, Phone);
            if (!String.IsNullOrEmpty(CitizenNo))
                ret = String.Format("{0}/{1}", ret, CitizenNo);
            if (!String.IsNullOrWhiteSpace(PassportNo))
                ret = String.Format("{0}/{1}", ret, PassportNo);
            return ret;
        }
    }
}
