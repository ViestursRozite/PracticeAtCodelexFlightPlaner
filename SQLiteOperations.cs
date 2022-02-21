using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Net;

namespace Attempt2
{
    class SQLiteOperations
    {
        //sqlFlights, f01Id, f02Carrier, f03DepartureTime, f04ArrivalTime, f05Country_from, f06City_from, f07Airport_from, f08Country_to, f09City_to, f10Airport_to,
        //sqlUnprocessed, fS1Id, fS2JS64, fS3Hash;

        // Stores Fligts obj seperated into component fields
        private static string sqlFlights = "flights";

        //Flight table column names:
        private static string f01Id = "id";//int
        private static string f02Carrier = "carrier";//string
        private static string f03DepartureTime = "departureTime";//string
        private static string f04ArrivalTime = "arrivalTime";//...
        private static string f05Country_from = "country_from";
        private static string f06City_from = "city_from";
        private static string f07Airport_from = "airport_from";
        private static string f08Country_to = "country_to";
        private static string f09City_to = "city_to";
        private static string f10Airport_to = "airport_to";//string

        //Stores incoming typescript obj as single string but in base64
        private static string sqlUnprocessed = "unprocessed";//used to check if obj in database

        //Unprocessed table column names:
        private static string fS1Id = "id";//SQL Foreign key Referencing f01Id
        private static string fS2JS64 = "javascript_but_base64";//typescript object stored as string encoded base 64, solves having to escape sql keywords 
        private static string fS3Hash = "hashCode";//INTEGER in SQL, used to look up encoded typescript objects faster
        //will fail to work if data left in sql, but server gets a restart



        public static void DeleteFlightsDotSqlite()//get rid of previous test run data
        {
            if (File.Exists("Flights.sqlite"))
            {
                File.Delete(Path.GetFullPath("Flights.sqlite"));
            }
        }

        public static void CreateTables(SQLiteConnection m_dbConnection)
        {
            string sqlFlightsCommand = $"CREATE TABLE {sqlFlights} " +
                $"(" +
                $"{f01Id} INTEGER NOT NULL PRIMARY KEY, " +
                $"{f02Carrier} TEXT NOT NULL, " +
                $"{f03DepartureTime} TEXT NOT NULL, " +
                $"{f04ArrivalTime} TEXT NOT NULL, " +
                $"{f05Country_from} TEXT NOT NULL, " +
                $"{f06City_from} TEXT NOT NULL, " +
                $"{f07Airport_from} TEXT NOT NULL, " +
                $"{f08Country_to} TEXT NOT NULL, " +
                $"{f09City_to} TEXT NOT NULL, " +
                $"{f10Airport_to} TEXT NOT NULL " +
                ");";

            string sqlUnprocessedCommand = $"CREATE TABLE {sqlUnprocessed} " +
                $"(" +
                $"{fS1Id} INTEGER NOT NULL, " +
                $"{fS2JS64} TEXT NOT NULL, " +
                $"{fS3Hash} INTEGER NOT NULL, " +
                $"PRIMARY KEY ({fS1Id} , {fS3Hash}), " +
                $"FOREIGN KEY ({fS1Id}) REFERENCES {sqlFlights}({f01Id})" +
                $");";

            SQLiteCommand tableFlights = new SQLiteCommand(sqlFlightsCommand, m_dbConnection);
            SQLiteCommand tableUnprocessed = new SQLiteCommand(sqlUnprocessedCommand, m_dbConnection);

            tableUnprocessed.ExecuteNonQuery();
            tableFlights.ExecuteNonQuery();
        }

        public static SQLiteConnection makeSQL()
        {
            //Create sql file and tables, leaves connection open
            //returns connection obj

            SQLiteConnection.CreateFile("Flights.sqlite");//make flights database
            SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=Flights.sqlite;Version=3;");//create connection obj
            m_dbConnection.Open();//use connection obj to connect

            return m_dbConnection;
        }

        public static int ClearDatabase(SQLiteConnection m_dbConnection)
        {
            string sqlDeleteAll = $"DELETE FROM {sqlFlights}; DELETE FROM {sqlUnprocessed}; ";

            SQLiteCommand deleteTables = new SQLiteCommand(sqlDeleteAll, m_dbConnection);

            int deleted = deleteTables.ExecuteNonQuery();//return num of rows effected

            return deleted;
        }

        public static string SearchMyDatabase(SQLiteConnection m_dbConnection, string[] fromToDate, out bool searchIsValid)
        {
            string result = "";
            int itemsFound = 0;
            string from = fromToDate[0];
            string to = fromToDate[1];

            if (from.Equals(to))//exit early if from and to airports are the same
            {
                searchIsValid = false;
                result = $"{{ \"items\": [], \"page\": 0, \"totalItems\": 0}}";
                return result;
            }

            string sqlSearchForAirportCommand = $"SELECT * FROM {sqlFlights} " +
                $"WHERE {f07Airport_from} LIKE '%{from}%' AND {f10Airport_to} LIKE '%{to}%' ;";

            SQLiteCommand searchDatabaseAirports = new SQLiteCommand(sqlSearchForAirportCommand, m_dbConnection);
            SQLiteDataReader sqLiteResponse = searchDatabaseAirports.ExecuteReader(); // response from sql 

            if (sqLiteResponse.HasRows)//if found something
            {
                while (sqLiteResponse.Read())//stop when no new line can be read 
                {
                    itemsFound++;//count rows
                }
            }

            result = $"{{ \"items\": [], \"page\": 0, \"totalItems\": {itemsFound}}}";
            searchIsValid = true;
            return result;
        }

        public static bool ObjIsInDatabaseSQL(SQLiteConnection m_dbConnection, string entityBody)
        {
            bool resultFound = false;
            //turn JS obj to base 64 string
            string encoded = HelperFunctions.Encode64(entityBody);
            //hash encoded for SQL
            int hashCode = encoded.GetHashCode();

            //look for hash in SQL
            string sqlSearchForHashCommand =
                $"SELECT * " +
                $"FROM {sqlUnprocessed} " +
                $"WHERE {fS3Hash} = {hashCode}; ";

            SQLiteCommand searchDatabaseHash = new SQLiteCommand(sqlSearchForHashCommand, m_dbConnection);//command
            SQLiteDataReader sqLiteResponse = searchDatabaseHash.ExecuteReader(); // response from sql 

            //unlikely but hash codes can be duplicates
            if (sqLiteResponse.HasRows)//sql returns something
            {
                //likely to contain only 1 row
                while (sqLiteResponse.Read() && !resultFound)//we can read a new line from response, and havent found matching string
                {
                    //compare returned and encoded
                    resultFound = encoded.Equals(sqLiteResponse.GetString(1));//1 = fS2JS64 Compare 2 json flights in base 64
                }
            }

            return resultFound;
        }

        public static bool AddFlightSQL(SQLiteConnection m_dbConnection, Flight flight, string entityBody)//Add usable Flight Obj and lokup table data
        {
            string entityBody64 = HelperFunctions.Encode64(entityBody);//turn entity body to base 64 string
            int hashCode = entityBody64.GetHashCode();//lets us query a number in SQL, instead of entityBody64

            SQLiteCommand sqlAddFlightCommand = new SQLiteCommand(m_dbConnection);
            SQLiteCommand sqlAddJSObjCommand = new SQLiteCommand(m_dbConnection);

            sqlAddFlightCommand.CommandText =
                $"INSERT INTO {sqlFlights} (" +
                $"" +//f01Id not specified as sql auto increments id
                $"{f02Carrier}, " +
                $"{f03DepartureTime}, " +
                $"{f04ArrivalTime}, " +
                $"{f05Country_from}, " +
                $"{f06City_from}, " +
                $"{f07Airport_from}, " +
                $"{f08Country_to}, " +
                $"{f09City_to}, " +
                $"{f10Airport_to}" +
                $")" +
            $"VALUES(" +
            $"@{f02Carrier}, @{f03DepartureTime}, @{f04ArrivalTime}, @{f05Country_from}, " + //str
            $"@{f06City_from}, @{f07Airport_from}, @{f08Country_to}, @{f09City_to}, @{f10Airport_to}); ";//str

            //Add fields to sql command, sollves field containing sql keyword
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f02Carrier}", flight.Carrier);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f03DepartureTime}", flight.DepartureTime.ToString("yyyy-MM-dd HH:mm"));
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f04ArrivalTime}", flight.ArrivalTime.ToString("yyyy-MM-dd HH:mm"));
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f05Country_from}", flight.From.Country);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f06City_from}", flight.From.City);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f07Airport_from}", flight.From.Airport);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f08Country_to}", flight.To.Country);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f09City_to}", flight.To.City);
            sqlAddFlightCommand.Parameters.AddWithValue($"@{f10Airport_to}", flight.To.Airport);
            
            //insert in flight table
            sqlAddFlightCommand.ExecuteNonQuery();

            //add to sql child table
            sqlAddJSObjCommand.CommandText = 
                $"INSERT INTO {sqlUnprocessed}({fS1Id}, {fS2JS64}, {fS3Hash}) " +//{fS1Id},
                $"VALUES(@{fS1Id}, @{fS2JS64}, @{fS3Hash});";//@{fS1Id},

            //get id assigned to flight we just added
            int id = SQLiteOperations.LastFlightAddedId(m_dbConnection);

            sqlAddJSObjCommand.Parameters.AddWithValue($"@{fS1Id}", id);
            sqlAddJSObjCommand.Parameters.AddWithValue($"@{fS2JS64}", entityBody64);
            sqlAddJSObjCommand.Parameters.AddWithValue($"@{fS3Hash}", hashCode);

            //ought to be 1
            int linesAdded = sqlAddJSObjCommand.ExecuteNonQuery();

            //return false if something went wrong
            return linesAdded == 1 ? true : false;
        }

        public static bool FlightWithThisIdExists(int id , SQLiteConnection m_dbConnection)
        {
            bool flightExists = false;
            string sqlFindIdCommand =
                $"SELECT {f01Id} " +
                $"FROM {sqlFlights} " +
                $"WHERE {f01Id} = {id}";

            SQLiteCommand findJsonString = new SQLiteCommand(sqlFindIdCommand, m_dbConnection); // command sent to sql
            SQLiteDataReader sqLiteResponse = findJsonString.ExecuteReader(); // response from sql 
            if (sqLiteResponse.HasRows) flightExists = true; //if sql response has rows, flight exists

            return flightExists;
        }

        public static Flight PullFlightFromDatabase(int id, SQLiteConnection m_dbConnection)
        {
            Flight flight = new Flight();
            string sqlPullFlightCommand =
                $"SELECT * " +
                $"FROM {sqlFlights} " +
                $"WHERE {f01Id} = {id}; ";//id is set to unique terefore will allways return max 1 row

            SQLiteCommand getFlight = new SQLiteCommand(sqlPullFlightCommand, m_dbConnection); // command sent to sql
            SQLiteDataReader sqLiteResponse = getFlight.ExecuteReader(); // response from sql 
            sqLiteResponse.Read();

            //columns in order
            /*id, carrier, departureTime, arrivalTime, country_from, city_from, airport_from, country_to, city_to, airport_to*/

            int of = -1;//column index offset

            var idConverson = sqLiteResponse.GetValue(1 + of);//1st column is int so cast int
            flight.Id = (Int64)idConverson;

            flight.Carrier = sqLiteResponse.GetString(2 + of);//All folowing columns are strings
            flight.DepartureTime = DateTime.Parse(sqLiteResponse.GetString(3 + of));// reverse of flight.DepartureTime.ToString("yyyy-MM-dd HH:mm")
            flight.ArrivalTime = DateTime.Parse(sqLiteResponse.GetString(4 + of));
            flight.From.Country = sqLiteResponse.GetString(5 + of);
            flight.From.City = sqLiteResponse.GetString(6 + of);
            flight.From.Airport = sqLiteResponse.GetString(7 + of);
            flight.To.Country = sqLiteResponse.GetString(8 + of);
            flight.To.City = sqLiteResponse.GetString(9 + of);
            flight.To.Airport = sqLiteResponse.GetString(10 + of);

            return flight;
        }

        internal static int LastFlightAddedId(SQLiteConnection connectionToSQL)
        {
            int result;
            result = -1;
            string sqlQuery =
                $"select last_insert_rowid();";

            SQLiteCommand getLastIdCommand = new SQLiteCommand(sqlQuery, connectionToSQL); 
            SQLiteDataReader sqLiteFromResponse = getLastIdCommand.ExecuteReader(); 

            if (sqLiteFromResponse.HasRows)
            {
                sqLiteFromResponse.Read();
                result = sqLiteFromResponse.GetInt32(0);// response from sql contains 1 row, with 1 cell
            }

            return result;
        }

        public static string FindAirport(HttpListenerRequest req, SQLiteConnection m_dbConnection)
        {
            string url = req.Url.ToString();
            string query = url.Substring(req.Url.ToString().LastIndexOf("search=") + 7).Trim('+');//extract search from url
            string result = "FindAirport() Failed";
            
            //sql columns
            /*[id, carrier, departureTime, arrivalTime, country_from, city_from, airport_from, country_to, city_to, airport_to]*/

            //query can be in any location column
            string sqlSearchAirportCommand =
                $"SELECT * FROM {sqlFlights} " +
                $"WHERE {f05Country_from} LIKE '%{query}%' OR " +
                $"{f06City_from} LIKE '%{query}%' OR " +
                $"{f07Airport_from} LIKE '%{query}%' OR " +
                $"{f08Country_to} LIKE '%{query}%' OR " +
                $"{f09City_to} LIKE '%{query}%' OR " +
                $"{f10Airport_to} LIKE '%{query}%'; ";

            SQLiteCommand searchDatabaseAirports = new SQLiteCommand(sqlSearchAirportCommand, m_dbConnection); //Command
            SQLiteDataReader sqLiteFromResponse = searchDatabaseAirports.ExecuteReader(); // response from sql 

            if (sqLiteFromResponse.HasRows)//found something
            {
                //returns first matching row
                sqLiteFromResponse.Read();

                string airport = "";
                string city = "";
                string country = "";

                //a match in ..._from columns
                if (sqLiteFromResponse.GetString(4).ToLower().Contains(query.ToLower()) || sqLiteFromResponse.GetString(5).ToLower().Contains(query.ToLower()) || sqLiteFromResponse.GetString(6).ToLower().Contains(query.ToLower()))//if in _from fields
                {
                    country = sqLiteFromResponse.GetString(4);
                    city = sqLiteFromResponse.GetString(5);
                    airport = sqLiteFromResponse.GetString(6);
                }
                //a match in ..._to columns
                else if (sqLiteFromResponse.GetString(7).ToLower().Contains(query.ToLower()) || sqLiteFromResponse.GetString(8).ToLower().Contains(query.ToLower()) || sqLiteFromResponse.GetString(9).ToLower().Contains(query.ToLower()))// if in _to fields
                {
                    country = sqLiteFromResponse.GetString(7);
                    city = sqLiteFromResponse.GetString(8);
                    airport = sqLiteFromResponse.GetString(9);
                }

                result = $"[{{\"airport\": \"{airport}\", " + 
                    $"\"city\": \"{city}\", " + 
                    $"\"country\": \"{country}\"}}]";
            }
            return result;
        }

    }
}
