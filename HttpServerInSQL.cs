using System;
using System.Data.SQLite;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Attempt2
{
    class HttpServerInSQL
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8080/";
        public static int requestCount = 0;//Tracks number of requests sent to server shown in console

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;//Always true

            //Creates new database and stores it`s connection obj
            //Connection remains open while server runs, there is no close()
            SQLiteConnection connectionToSQL = SQLiteOperations.StartSQL();

            //Stores the flight object of this request
            Flight flight = new Flight();            

            while (runServer)
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

                //Build a response 

                //outer ifs check method
                //  followed by url 
                //      and specific fields
                if (HelpingFunc.MethodIsPOST(req))
                {
                    if (HelpingFunc.URLHasTestingApi(req))//→  /testing-api/
                    {
                        if (HelpingFunc.URLHasCear(req))//→  /clear
                        {
                            SQLiteOperations.ClearDatabase(connectionToSQL);//Delete command to sql
                            resp.StatusCode = 200;//say ok
                        }
                    }
                    else if (HelpingFunc.URLContainsAPI(req))//→  /api/
                    {
                        if (HelpingFunc.URLHasFlights(req))//a search is beeing performed→  /flights/
                        {
                            if (req.HasEntityBody)//search has actual query
                            {
                                if (entityBody.Contains("null"))//reject incomplete 
                                {
                                    resp.StatusCode = 400;
                                }
                                else
                                {
                                    //Assuming a JS flight obect is entityBody
                                    string[] incomingFields = HelpingFunc.ReturnIncomingSearch(entityBody);//array of all its fields

                                    //fromToDate = [flight.From, flight.to, flight.DepartureTime]
                                    string[] fromToDate = HelpingFunc.ReturnSearchInfo(incomingFields);

                                    //searchIsValid = true, if .from and .to do not match
                                    bool searchIsValid;

                                    //write to response body the number of flights found
                                    myResponse = SQLiteOperations.SearchMyDatabase(connectionToSQL, fromToDate, out searchIsValid);

                                    if (searchIsValid)
                                    {
                                        resp.StatusCode = 200;
                                    }
                                    else if (!searchIsValid)
                                    {
                                        resp.StatusCode = 400;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (HelpingFunc.MethodIsGET(req))
                {
                    if (HelpingFunc.URLHasAdminAPI(req))//→   /admin-api/
                    {
                        if (HelpingFunc.ThereIsID(req))//found id at the end of url
                        {
                            if (HelpingFunc.CheckPass(adminNameNPass))//if: Provided pasword correct
                            {
                                // no test cases present
                                resp.StatusCode = 404;//doesnt exist
                            }
                            else if (!HelpingFunc.CheckPass(adminNameNPass))//if: Password was incorrect
                            {
                                resp.StatusCode = 401;//you are unauthorized
                            }
                        }
                    }
                    else if (HelpingFunc.URLContainsAPI(req))//→  /api/
                    {
                        //for api a search by data in url
                        if (!req.HasEntityBody)//should not have an obect attached
                        {
                            //a specific airport is beeing asked for
                            if (HelpingFunc.URLIsFindAirport(req))//→ /airports?   
                            {
                                //write response based on how many flights found
                                myResponse = SQLiteOperations.FindAirport(req, connectionToSQL);
                                resp.StatusCode = 200;
                            }
                            //asked for specific flight
                            else if (HelpingFunc.URLHasFlights(req))//contains→   /flights/ 
                            {
                                // /flights/ ought to be followed by flight id
                                //id = -1 if could not parse from url
                                int id = HelpingFunc.GiveApiId(req);

                                //query sql to see if flight with this id exists
                                bool flightExists = SQLiteOperations.FlightWithThisIdExists(id, connectionToSQL);
                                
                                if (flightExists)
                                {
                                    //pull flight
                                    flight = SQLiteOperations.PullFlightFromDatabase(id, connectionToSQL);
                                    //convert to JS obj
                                    myResponse = HelpingFunc.FlightToResponseBody(flight);
                                    resp.StatusCode = 200;
                                }
                                else if (!flightExists)//flight with id does not exist
                                {
                                    resp.StatusCode = 404;
                                }
                            }
                        }
                    }
                }
                else if (HelpingFunc.MethodIsPUT(req))
                {
                    if (HelpingFunc.URLHasAdminAPI(req))//→  /admin-api/
                    {
                        if (req.HasEntityBody)//an object is likely beeing added
                        {
                            if (entityBody.Contains("null"))//If obj incomplete
                            {
                                resp.StatusCode = 400;//Bad request
                            }
                            else if (entityBody.Contains("\"\""))//If a field is empty string
                            {
                                resp.StatusCode = 400;//Bad request
                            }
                            else//JS obj probably contains correctly formated flight
                            {
                                if (SQLiteOperations.ObjIsInDatabaseSQL(connectionToSQL, entityBody))//If requested addition is already in the database
                                {
                                    resp.StatusCode = 409;//conflict in the request, this flight is already in the database
                                }
                                else//this is not in the database
                                {
                                    //Make Flight object
                                    flight = HelpingFunc.JSObjToFlightObj(entityBody);
                                    
                                    //check if same airport
                                    if (flight.From.Airport.ToUpper().Trim().Equals(flight.To.Airport.ToUpper().Trim()))
                                    {
                                        resp.StatusCode = 400;//Bad request
                                    }
                                    //check if timetravel
                                    else if (flight.DepartureTime >= flight.ArrivalTime)
                                    {
                                        resp.StatusCode = 400;//Bad request
                                    }
                                    //add fight to database
                                    else
                                    {
                                        //pull last id from sql
                                        int lastId = SQLiteOperations.LastFlightAddedId(connectionToSQL);
                                        //.Id was empty till now
                                        flight.Id = lastId + 1;
                                        //Add incoming flight to database and lookup table
                                        bool flightIsAdded = SQLiteOperations.AddFlightSQL(connectionToSQL, flight, entityBody);
                                        //prepare response 
                                        if (flightIsAdded)
                                        {
                                            int flightID = SQLiteOperations.LastFlightAddedId(connectionToSQL);
                                            myResponse = HelpingFunc.AddIdToResponseString(flightID, entityBody);
                                            resp.StatusCode = 201; //sucsesfully added a flight
                                        }
                                        else
                                        {
                                            resp.StatusCode = 500; //server err
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //see what is going to be the response in console
                HelpingFunc.WriteOutputToConsole(resp);
                Console.WriteLine("Response body:" + myResponse);

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
