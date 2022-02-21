using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Attempt2
{
    public static class HelperFunctions
    {
        public static string Encode64(string message)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode64(string encodedMessage)
        {
            byte[] data = Convert.FromBase64String(encodedMessage);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static bool CheckPass(string userAndPass)//check if provided id and pass correct
        {
            return userAndPass.Equals("codelex-admin:Password123");
        }

        public static bool CheckPass(string userAndPass, string expected)//check if provided id and pass correct
        {
            return userAndPass.Equals(expected);
        }

        public static string GetEncodedUserNPass(NameValueCollection kVPairs)//extract username and password from request
        {
            string[] allHeaders = kVPairs.AllKeys;
            string encodedPass = "bg==";

            foreach (var header in allHeaders)
            {
                if (header.Equals("Authorization"))
                {
                    encodedPass = kVPairs[header].Substring(5);
                }
            }
            return encodedPass;
        }

        public static string GiveBodyAsStringOrEmpty(Stream body, Encoding encoding)//return data in json object as string
        {
            StreamReader reader = new StreamReader(body, encoding);
            string s = reader.ReadToEnd();
            return s;
        }

        public static void HttpToConsole(HttpListenerResponse resp)//Show the incoming request
        {
            Console.WriteLine();
            Console.WriteLine("Planned Response: ");
            Console.WriteLine("Response code:           " + resp.StatusCode);
            Console.WriteLine("OutputStream:            " + resp.OutputStream);
            Console.WriteLine("ContentLength64:         " + resp.ContentLength64);
            Console.WriteLine("Headers:                 " + resp.Headers);
            Console.WriteLine("ProtocolVersion:         " + resp.ProtocolVersion);
        }

        public static void HttpToConsole(HttpListenerRequest req)//Show the constructed response
        {
            Console.WriteLine("Incoming:");
            Console.WriteLine("Requested url:       " + req.Url.ToString());
            Console.WriteLine("Method called:       " + req.HttpMethod);
            Console.WriteLine("Entity body:         " + req.HasEntityBody);
            Console.WriteLine("Input stream:        " + req.InputStream);
            Console.WriteLine("Headers:             " + req.Headers);
            Console.WriteLine("QueryString Keys:    " + req.QueryString.AllKeys);
        }


        public static string[] JsonToFlightObjFields(string body)
        {
            string sepertor = "\"";
            char[] splitChars = new char[] { sepertor[0] };
            string[] allPeaces = body.Split(splitChars);
            string[] a = allPeaces;
            //                   from.country/city/airport, to.country/city/airport, carrier, departure_Time, arrival_Time
            string[] flightObjFields = {a[5], a[9], a[13], a[19], a[23], a[27], a[31], a[35], a[39] };

            return flightObjFields;
        }

        public static Flight FillFlightObject(string[] dataAsArr)//takes array of Flight fields and makes Flight object
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
            string[] a = JsonToFlightObjFields(messageBody);
            return FillFlightObject(a);
        }

        public static string AddIdToResponseString(long flightID, string messageBody)//add id field to incoming Json string
        {
            messageBody = messageBody.Remove(messageBody.Length - 1, 1); //take off the last } to add id
            messageBody = messageBody + $",\"id\":\"{flightID}\"}}";//id field to the end of object
            return messageBody;
        }

        internal static bool ThereIsID(HttpListenerRequest req)//Detects if id is at the end of url
        {
            int id = GiveAdminApiId(req);
            return id != -1;
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

        public static string FlightToResponseBody(Flight flight)//convert Flight to Json string
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

        public static string[] Return3FlightFieldsFromToAndDate(string[] incomingFields)//each index is a string of either fieldname or field value from original obj receaved
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
    }
}
