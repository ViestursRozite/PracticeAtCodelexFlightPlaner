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
            From = new From();
            To = new To();
            Carrier = "";
            DepartureTime = DateTime.Now;
            ArrivalTime = DateTime.Now;
            Id = 0;
        }
        public From From { get; set; }
        public  To To { get; set; }
        public string Carrier { get; set; }
        public  DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int Id { get; set; }
    }

    public class To
    {
        //structure
        //  "to": {
        //    "country": "Sweden",
        //    "city": "Stockholm",
        //    "airport": "ARN"
        public To()
        {
            Country = "";
            City = "";
            Airport = "";
        }
        public string Country { get; set; }
        public string City { get; set; }
        public string Airport { get; set; }

    }

    public class From
    {
        //structure
        //        "from": {
        //    "country": "Latvia",
        //    "city": "Riga",
        //    "airport": "RIX"
        public From()
        {
            Country = "";
            City = "";
            Airport = "";
        }
        public string Country { get; set; }
        public string City { get; set; }
        public string Airport { get; set; }
    }
}