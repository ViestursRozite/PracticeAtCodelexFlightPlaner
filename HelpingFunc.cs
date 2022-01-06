using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Attempt2
{
    public static class HelpingFunc
    {
        public static string Encode64(string message) //encode string to base64
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string DecodeStuff(string encodedMessage)//decodes base 64 encoded string id and pass
        {
            byte[] data = Convert.FromBase64String(encodedMessage);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static bool CheckPass(string userAndPass)//check if provided id and pass correct
        {
            if (userAndPass.Equals("codelex-admin:Password123")) return true;
            else return false;
        }

        public static string GetEncodedUserNPass(HttpListenerRequest req)//extract username and password from request
        {
            string[] allHeaders = req.Headers.AllKeys;
            string encodedPass = "bg==";

            foreach (var header in allHeaders)
            {
                if (header.Equals("Authorization"))
                {
                    encodedPass = req.Headers[header].Substring(5);
                }
            }
            return encodedPass;
        }

        public static string GiveBodyAsStringOrEmpty(HttpListenerRequest request)//return data in json object as string
        {
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            string s = reader.ReadToEnd();
            return s;
        }

        public static bool AddFlightObjectToDatabase(int fligtID, Flight flight, SortedDictionary<int, Flight> position0)//add Flight object to database[0]
        {
            position0.Add(fligtID, flight);
            flight.Id = fligtID;
            Console.WriteLine($"Message added, ID[{fligtID}] to Database[0] proceeding...");
            return true;
        }

        public static bool URLIsClearDatabase(HttpListenerRequest req)
        {
            if ((req.Url.ToString().Equals("http://localhost:8080/testing-api/clear"))) return true;
            else return false;
        }


        public static bool URLHasFlights(HttpListenerRequest req)
        {
            if ((req.Url.ToString().Contains(@"/flights/"))) return true;
            else return false;
        }

        public static bool URLHasCear(HttpListenerRequest req)
        {
            string requestURL = req.Url.ToString();
            if (requestURL.Contains(@"/clear")) return true;
            else return false;
        }

        public static bool URLHasTestingApi(HttpListenerRequest req)
        {
            if ((req.Url.ToString().Contains($"/testing-api/"))) return true;
            else return false;
        }

        public static void WriteOutputToConsole(HttpListenerResponse resp)//Show the incoming request
        {
            Console.WriteLine();
            Console.WriteLine("Planned Response: ");
            Console.WriteLine("Response code:           " + resp.StatusCode);
            Console.WriteLine("OutputStream:            " + resp.OutputStream);
            Console.WriteLine("ContentLength64:         " + resp.ContentLength64);
            Console.WriteLine("Headers:                 " + resp.Headers);
            Console.WriteLine("ProtocolVersion:         " + resp.ProtocolVersion);
        }

        public static void WriteInputToConsole(HttpListenerRequest req)//Show the constructed response
        {
            Console.WriteLine("Incoming:");
            Console.WriteLine("Requested url:       " + req.Url.ToString());
            Console.WriteLine("Method called:       " + req.HttpMethod);
            Console.WriteLine("Entity body:         " + req.HasEntityBody);
            Console.WriteLine("Input stream:        " + req.InputStream);
            Console.WriteLine("Headers:             " + req.Headers);
            Console.WriteLine("QueryString Keys:    " + req.QueryString.AllKeys);
        }

        public static object[] ReturnANewDatabase()//makes database
        {
            //myDatabase[0] = flights stored in SortedDictionary, where key = flight id, val = flight class
            return new object[] { new SortedDictionary<int, Flight>(), 
                //myDatabase[1] = lookup tables
                //<Key = FieldNameInFlightObj, Val = <Key = ContentOfThisField, Val = List<ID of matching flights>>>
                new SortedDictionary<string, SortedDictionary<string, List<int>>>() ,//Posibility to add lookup table
                //sortedSet takes the raw Flight as string .Json, this is also used to determine if this flight has been seen before
                new SortedSet<string>()};
        }

        public static string[] ProcessJsonToArrOfUsefulPlaces(string body)
        {
            string sepertor = "\"";
            char[] splitChars = new char[] { sepertor[0] };
            string[] allPeaces = body.Split(splitChars);
            string[] a = allPeaces;
            string[] usefulPeaces = {a[5], //from country
                a[9], //from city
                a[13], //from airport
                a[19], //to country
                a[23], //to city
                a[27], //to airport
                a[31], //carrier
                a[35], //departure time
                a[39] };//arrival time

            return usefulPeaces;
        }

        public static Flight FillFlightObject(string[] dataAsArr)//takes array of useful places and makes Flight object
        {
            Flight flight = new Flight();
            flight.From.Country = dataAsArr[0];
            flight.From.City = dataAsArr[1];
            flight.From.Airport = dataAsArr[2];
            flight.To.Country = dataAsArr[3];
            flight.To.City = dataAsArr[4];
            flight.To.Airport = dataAsArr[5];
            flight.Carrier = dataAsArr[6];
            flight.DepartureTime = Convert.ToDateTime(dataAsArr[7]);
            flight.ArrivalTime = Convert.ToDateTime(dataAsArr[8]);

            return flight;
        }

        public static Flight JSObjToFlightObj(string messageBody)//combines ProcessJsonToArrOfUsefulPlaces and FillFlightObject, then returns Flight object
        {
            string[] a = ProcessJsonToArrOfUsefulPlaces(messageBody);
            return FillFlightObject(a);
        }

        public static string AddIdToResponseString(long flightID, string messageBody)//add id field to incoming Json string
        {
            messageBody = messageBody.Remove(messageBody.Length - 1, 1); //take off the last } to add id
            messageBody = messageBody + $",\"id\":\"{flightID}\"}}";//slap id field to the back of the response object
            return messageBody;
        }

        internal static bool MethodIsPOST(HttpListenerRequest req)
        {
            if (req.HttpMethod.Equals("POST")) return true;
            else return false;
        }

        internal static bool MethodIsGET(HttpListenerRequest req)
        {
            if (req.HttpMethod.Equals("GET")) return true;
            else return false;
        }

        internal static bool URLHasAdminAPI(HttpListenerRequest req)
        {
            if (req.Url.ToString().Contains("/admin-api/")) return true;
            else return false;
        }

        internal static bool ThereIsID(HttpListenerRequest req)//Detects if id is at the end of url
        {
            int id = GiveAdminApiId(req);
            if (id == -1) return false;
            else return true;
        }

        public static int GiveAdminApiId(HttpListenerRequest req)
        {
            string idSTR = req.Url.ToString().Substring("http://localhost:8080/admin-api/flights/".Length);
            int id = -1;
            int.TryParse(idSTR, out id);
            return id;
        }

        public static int GiveApiId(HttpListenerRequest req)
        {
            string idSTR = req.Url.ToString().Substring("http://localhost:8080/api/flights/".Length);
            int id = -1;
            int.TryParse(idSTR, out id);
            return id;
        }

        internal static bool MethodIsPUT(HttpListenerRequest req)
        {
            if (req.HttpMethod.Equals("PUT")) return true;
            else return false;
        }

        internal static void AddJsonToDatabase(string entityBody, SortedSet<string> database2)//add to database[2]
        {
            database2.Add(entityBody);
        }

        internal static void AddToDatabase(string entityBody, Flight flight, int flightID, object[] myDatabase)
        {
            //add incoming .json, to be able to reject duplicates in database[2]
            HelpingFunc.AddJsonToDatabase(entityBody, (SortedSet<string>)myDatabase[2]);
            //add incoming .json as Flight object to database[0]
            HelpingFunc.AddFlightObjectToDatabase(flightID, flight, (SortedDictionary<int, Flight>)myDatabase[0]);
        }

        internal static bool URLContainsAPI(HttpListenerRequest req)
        {
            if (req.Url.ToString().Contains("/api/")) return true;
            else return false;
        }

        internal static string FlightToResponseBody(Flight flight)//convert Flight to Json string
        {
            return $"{{ \"arrivalTime\": \"{flight.ArrivalTime.ToString("yyyy-MM-dd HH:mm")}\"," +
                $" \"carrier\": \"{flight.Carrier}\"," +
                $" \"departureTime\": \"{flight.DepartureTime.ToString("yyyy-MM-dd HH:mm")}\", " +
                $"\"from\": {{ \"airport\": \"{flight.From.Airport}\", " +
                $"\"city\": \"{flight.From.City}\", " +
                $"\"country\": \"{flight.From.Country}\"}}, " +
                $"\"id\": \"{flight.Id}\", \"to\": {{ \"airport\": " +
                $"\"{flight.To.Airport}\", " +
                $"\"city\": \"{flight.To.City}\", " +
                $"\"country\": \"{flight.To.Country}\"}} }}";
        }

        internal static string[] ReturnIncomingSearch(string entityBody)//return aray of words that make up Flight object fields and values
        {
            string[] smD = entityBody.Replace("}", "")
                .Replace("{", "")
                .Replace("\"", "@")
                .Replace(",", "")
                .Replace(":", "")
                .Replace("@@", "@")
                .Trim('@')
                .Trim(' ')
                .Split('@');
            return smD;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)//Return a sub array starting at an index and of specified length
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string[] ReturnSearchInfo(string[] incomingFields)//each index is a string of either fieldname or field value from original obj receaved
        {
            int fieldsCount = incomingFields.Length;

            //devide incoming data into sub categories for ease of use
            string[] fromArr = incomingFields.SubArray(0, Array.IndexOf(incomingFields, "to"));//from category and it's fields
            int dif = Array.IndexOf(incomingFields, "departureDate") - Array.IndexOf(incomingFields, "to");//length property for .SubArray
            string[] toArr = incomingFields.SubArray(Array.IndexOf(incomingFields, "to"), dif);//to category and it's fields
            string[] departureArr = incomingFields.SubArray(Array.IndexOf(incomingFields, "departureDate"), 2);//departure field

            string[] result = new string[] { fromArr[1], toArr[1], departureArr[1] };//[0] = field name, [1] = field value
            return result;
        }

        public static string SearchMyDatabase(object[] myDatabase, string[] fromToDate, out bool searchIsValid)
        {
            SortedDictionary<int, Flight> flightObjects = (SortedDictionary<int, Flight>)myDatabase[0];
            int itemsFound = 0;
            List<Flight> matchingFlights = new List<Flight>();//flights that match
            string from = fromToDate[0];
            string to = fromToDate[1];

            foreach (var KVP in flightObjects)
            {
                var key = KVP.Key;//id
                var flight = KVP.Value;

                if (
                    ((flight.From.Airport.Equals(from)) ||//this flight contains from search query
                    (flight.To.Airport.Equals(from)))
                    &&
                    ((flight.From.Airport.Equals(to)) ||//this flight contains to search query
                    (flight.To.Airport.Equals(to)))
                   )
                {
                    itemsFound++;
                    matchingFlights.Add(flight);
                }
            }

            string result = $"{{ \"items\": [], \"page\": 0, \"totalItems\": {itemsFound}}}";
            if (fromToDate[0].Equals(fromToDate[1]))
            {
                searchIsValid = false;
                return result;
            }
            searchIsValid = true;
            return result;
        }

        internal static bool URLIsFindAirport(HttpListenerRequest req)
        {
            if (req.Url.ToString().Contains("/airports?"))
            {
                return true;
            }
            else return false;
        }

        internal static string FindAirport(object[] myDatabase, HttpListenerRequest req)
        {
            string url = req.Url.ToString();
            string query = url.Substring(req.Url.ToString().LastIndexOf("search=") + 7).Trim('+');

            SortedSet<string> jsonFlightObjects = (SortedSet<string>)myDatabase[2];
            SortedDictionary<int, Flight> flightObjects = (SortedDictionary<int, Flight>)myDatabase[0];

            foreach (var KVP in flightObjects)
            {
                Flight flight = KVP.Value;
                if (
                    (flight.From.Airport.ToLower().Contains(query.ToLower())) ||
                    (flight.From.City.ToLower().Contains(query.ToLower())) ||
                    (flight.From.Country.ToLower().Contains(query.ToLower()))
                    )
                {
                    return $"[{{\"airport\": \"{flight.From.Airport}\", \"city\": \"{flight.From.City}\", \"country\": \"{flight.From.Country}\"}}]";
                }
                else if (
                    (flight.To.Airport.ToLower().Contains(query.ToLower())) ||
                    (flight.To.City.ToLower().Contains(query.ToLower())) ||
                    (flight.To.Country.ToLower().Contains(query.ToLower()))
                   )
                {
                    return $"[{{\"airport\": \"{flight.To.Airport}\", \"city\": \"{flight.To.City}\", \"country\": \"{flight.To.Country}\"}}]";
                }
            }
            return "failed at FindAirport()";
        }
    
    }
}
