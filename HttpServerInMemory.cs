using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Attempt2
{
    class HttpServerInMemory
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8080/";
        public static int requestCount = 0;//Tracks number of requests sent to server
        public static int flightID = 1;//Stores flight ID

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;//Always true
            Flight flight = new Flight();//Stores the flight object of this request            
            object[] myDatabase = HelpingFunc.ReturnANewDatabase();//[0] = database, [1] = lookup tables
            SQLiteConnection connectionToSQL = SQLiteOperations.StartSQL();//Create new database and store it`s connection obj

            while (runServer)// run my server forever
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                //Reformat stuff for ease of use
                string myResponse = "";//Body of the response
                string encodedPass = HelpingFunc.GetEncodedUserNPass(req);//Pull encoded pass and username
                string adminNameNPass = HelpingFunc.DecodeStuff(encodedPass);// get id and password as string decoded
                string entityBody = HelpingFunc.GiveBodyAsStringOrEmpty(req);//string with incoming message body

                // Write out the number of requests
                Console.WriteLine();
                Console.WriteLine("Request #:       {0}", ++requestCount);//Request count to console
                HelpingFunc.WriteInputToConsole(req);//see what we are receving in console
                Console.WriteLine("Incoming entity body : " + entityBody);//see what we are receving as incoming entity body

                //Build a propper response ########################################################################

                //1st layer if`s checks HTTP methods
                //  2nd layer if`s looks at url
                //      3rd layer if`s looks at specific fields

                if (HelpingFunc.MethodIsPOST(req))
                {
                    if (HelpingFunc.URLIsClearDatabase(req))//If clear the database.
                    {
                        SQLiteOperations.ClearDatabase(connectionToSQL); // clear database in SQL
                        myDatabase = HelpingFunc.ReturnANewDatabase();//clear the database
                        flightID = 1;//reset ID's to 1
                        resp.StatusCode = 200;//say: Ok
                    }
                    else if (HelpingFunc.URLContainsAPI(req))
                    {
                        if (req.HasEntityBody)//if search done by body data
                        {
                            if (entityBody.Contains("null"))
                            {
                                resp.StatusCode = 400;//say no to null fields in incoming Json obj
                            }
                            else//no evident problems
                            {
                                string[] incomingFields = HelpingFunc.ReturnIncomingSearch(entityBody);
                                string[] fromToDate = HelpingFunc.ReturnSearchInfo(incomingFields);//[from, to, date ] fields in search request
                                bool searchIsValid;//did we find our thing in the database
                                myResponse = HelpingFunc.SearchMyDatabase(myDatabase, fromToDate, out searchIsValid);//write to response body the number of flights found

                                if (searchIsValid)//found our thing, carry on
                                {

                                }
                                else//did not find anything
                                {
                                    resp.StatusCode = 400;//send failiure to find
                                }
                            }
                        }
                    }
                }
                else if (HelpingFunc.MethodIsGET(req))
                {
                    if (HelpingFunc.URLHasAdminAPI(req))
                    {
                        if (HelpingFunc.ThereIsID(req))//found id at the end of url
                        {
                            if (HelpingFunc.CheckPass(adminNameNPass))//if: Provided pasword correct
                            {
                                resp.StatusCode = 404;// nice, but flight does not exist
                            }
                            else if (!HelpingFunc.CheckPass(adminNameNPass))//if: Password was incorrect
                            {
                                resp.StatusCode = 401;//you are unauthorized
                            }
                        }
                    }
                    else if (HelpingFunc.URLContainsAPI(req))
                    {
                        if (!req.HasEntityBody)//if no entity body
                        {
                            if (HelpingFunc.URLIsFindAirport(req))
                            {
                                myResponse = HelpingFunc.FindAirport(myDatabase, req);//write response based on how many flights found
                                //response code is set to 200 by default
                            }
                            else
                            {
                                int id = HelpingFunc.GiveApiId(req);
                                if (((SortedDictionary<int, Flight>)myDatabase[0]).ContainsKey(id))// If flight with id exists
                                {
                                    flight = ((SortedDictionary<int, Flight>)myDatabase[0])[id];//pull flight with id from database
                                    myResponse = HelpingFunc.FlightToResponseBody(flight);//respond with the flight from database
                                }
                                else if (!((SortedDictionary<int, Flight>)myDatabase[0]).ContainsKey(id))// If flight with id does not exist
                                {
                                    resp.StatusCode = 404;
                                }
                            }
                        }
                    }
                }
                else if (HelpingFunc.MethodIsPUT(req))
                {
                    if (HelpingFunc.URLHasAdminAPI(req))
                    {
                        if (req.HasEntityBody)
                        {
                            if (entityBody.Contains("null"))//If add wierd request
                            {
                                resp.StatusCode = 400;//Bad request
                            }
                            else
                            {
                                if (entityBody.Contains("\"\""))//If add wierd request
                                {
                                    resp.StatusCode = 400;//Bad request
                                }
                                else//if not a wierd request
                                {
                                    if (((SortedSet<string>)myDatabase[2]).Contains(entityBody))//If requested addition is already in the database
                                    {
                                        resp.StatusCode = 409;//conflict in the request, this flight is already in the database
                                    }
                                    else//add the flight to database
                                    {
                                        flight = HelpingFunc.JSObjToFlightObj(entityBody);//Make Flight object

                                        if (flight.From.Airport.ToUpper().Trim().Equals(flight.To.Airport.ToUpper().Trim()))//if Same airport in Flight object
                                        {
                                            resp.StatusCode = 400;//Bad request, an airport cannot exist in 2 places at once
                                        }
                                        else if (flight.DepartureTime >= flight.ArrivalTime)//If wierd flight times
                                        {
                                            resp.StatusCode = 400;//Bad request, no wierd times
                                        }
                                        else
                                        {
                                            HelpingFunc.AddToDatabase(entityBody, flight, flightID, myDatabase);//Add incoming flight to database
                                            myResponse = HelpingFunc.AddIdToResponseString(flightID, entityBody);//prepare response .Json
                                            flightID++;//+1 flight added
                                            resp.StatusCode = 201; //sucsesfully added a flight
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                HelpingFunc.WriteOutputToConsole(resp);//see what is going to be the response in console
                Console.WriteLine("Response body:" + myResponse);
                //End of logic for response ########################################################################

                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(myResponse);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();//Send Away
            }
        }
    }
}
