using System;

namespace Attempt2
{
    //structure
    //    "{
    //  "from": {
    //    "country": "Latvia",
    //    "city": "Riga",
    //    "airport": "RIX"
    //  },
    //  "to": {
    //    "country": "Sweden",
    //    "city": "Stockholm",
    //    "airport": "ARN"
    //  },
    //  "carrier": "Ryanair",
    //  "departureTime": "2019-01-01 00:00",
    //  "arrivalTime": "2019-01-02 00:00"
    //}"
    public class Flight
    {
        public Flight()
        {
            from = new from();
            to = new to();
            carrier = "";
            departureTime = DateTime.Now;
            arrivalTime = DateTime.Now;
            id = 0;
        }
        public from from { get; set; }
        public  to to { get; set; }
        public string carrier { get; set; }
        public  DateTime departureTime { get; set; }
        public DateTime arrivalTime { get; set; }
        public int id { get; set; }
    }

    public class to
    {
        //structure
        //  "to": {
        //    "country": "Sweden",
        //    "city": "Stockholm",
        //    "airport": "ARN"
        public to()
        {
            country = "";
            city = "";
            airport = "";
        }
        public string country { get; set; }
        public string city { get; set; }
        public string airport { get; set; }

    }

    public class from
    {
        //structure
        //        "from": {
        //    "country": "Latvia",
        //    "city": "Riga",
        //    "airport": "RIX"
        public from()
        {
            country = "";
            city = "";
            airport = "";
        }
        public string country { get; set; }
        public string city { get; set; }
        public string airport { get; set; }
    }
}
