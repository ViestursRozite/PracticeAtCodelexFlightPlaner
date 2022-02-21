using NUnit.Framework;
using static Attempt2.HelperFunctions;
using System.Net;
using Autofac.Extras.Moq;
using Moq;
using System.Net.Http;
using System.Collections.Specialized;
using Attempt2;
using System;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            //HttpListenerRequest req = new HttpListenerRequest();
        }

        [Test]
        [TestCase("codelex-admin:Password123", true)]
        [TestCase("codelex-admin:Password1234", false)]
        [TestCase("codelex-admin:Password12", false)]
        [TestCase("", false)]
        [TestCase("codelex-adminPassword123", false)]
        public void CheckPass_ShouldWork(string strToCheck, bool expected)
        {
            //Arrange
            bool actual;

            //Act
            actual = CheckPass(strToCheck);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("codelex-admin:Password123", "codelex-admin:Password123", true)]
        [TestCase("codelex-admin:Password1234", "codelex-admin:Password123", false)]
        [TestCase("codelex-admin:Password12", "codelex-admin:Password123", false)]
        [TestCase("", "codelex-admin:Password123", false)]
        [TestCase("codelex-adminPassword123", "codelex-admin:Password123", false)]
        public void CheckPass_ShouldWork(string strToCheck, string strWithCorrect, bool expected)
        {
            //Arrange
            bool actual;

            //Act
            actual = CheckPass(strToCheck, strWithCorrect);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("Authorization", "tfkafcodelex-admin:Password123", "Field1", "Val of Field 1", "Auth", "another value", "codelex-admin:Password123")]
        [TestCase("Field1", "Val of Field 1", "Authorization", "ovyhacodelex-admin:Password123",  "Auth", "another value", "codelex-admin:Password123")]
        [TestCase("Auth", "another value", "Authorization", "12345codelex-admin:Password123", "Field1", "Val of Field 1",  "codelex-admin:Password123")]
        [TestCase("Authorizatio", "qftivfcodelex-admin:Password123", "Field1", "Val of Field 1", "Auth", "another value", "bg==")]
        public void GetEncodedUserNPass_ShouldExtractInformation(string k1, string v1, 
            string k2, string v2,
            string k3, string v3,
            string expected)
        {
            //Arrange
            var kVPairs = new NameValueCollection();
            kVPairs.Add(k1, v1);
            kVPairs.Add(k2, v2);
            kVPairs.Add(k3, v3);

            //Act
            var actual = GetEncodedUserNPass(kVPairs);
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void JsonToFlightObjFields_ShouldFunctionWithPerfectInput()
        {
            //Arrange
            string[] expected = new string[] { "IE", "Killarney", "EIKY", "US", "Marion", "20OI", "Riverside Air Service", "2030 - 02 - 07 23:24", "2030 - 02 - 09 02:24"};
            string entityBody = "{\"from\":" +
                "{\"country\":\"IE\",\"city\":\"Killarney\",\"airport\":\"EIKY\"},\"to\":" +
                "{\"country\":\"US\",\"city\":\"Marion\",\"airport\":\"20OI\"}," +
                "\"carrier\":\"Riverside Air Service\"," +
                "\"departureTime\":\"2030 - 02 - 07 23:24\"," +
                "\"arrivalTime\":\"2030 - 02 - 09 02:24\"}";

            //Act
            var actual = JsonToFlightObjFields(entityBody);

            //Assert

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase()]
        public void FillFlightObject_ShouldFunctionWithPerfectInput()
        {
            //Arrange
            string[] arr = new string[] { "IE", "Killarney", "EIKY", "US", "Marion", "20OI", "Riverside Air Service", "2030 - 02 - 07 23:24", "2030 - 02 - 09 02:24" };
            Flight expected = new Flight();
            expected.From.Country = arr[0];
            expected.From.City = arr[1];
            expected.From.Airport = arr[2];
            expected.To.Country = arr[3];
            expected.To.City = arr[4];
            expected.To.Airport = arr[5];
            expected.Carrier = arr[6];
            expected.DepartureTime = Convert.ToDateTime(arr[7]);
            expected.ArrivalTime = Convert.ToDateTime(arr[8]);

            //Act
            var actual = FillFlightObject(arr);

            //Assert
            Assert.AreEqual(expected.From.Country, actual.From.Country);
            Assert.AreEqual(expected.From.City, actual.From.City);
            Assert.AreEqual(expected.From.Airport, actual.From.Airport);
            Assert.AreEqual(expected.To.Country, actual.To.Country);
            Assert.AreEqual(expected.To.City, actual.To.City);
            Assert.AreEqual(expected.To.Airport, actual.To.Airport);
            Assert.AreEqual(expected.Carrier, actual.Carrier);
            Assert.AreEqual(expected.DepartureTime, actual.DepartureTime);
            Assert.AreEqual(expected.ArrivalTime, actual.ArrivalTime);
        }

        [Test]
        public void JSObjToFlightObj_ShouldFunctionWithPerfectInput()
        {
            //Arrange
            string JSObj = "{\"from\":" +
                "{\"country\":\"IE\",\"city\":\"Killarney\",\"airport\":\"EIKY\"},\"to\":" +
                "{\"country\":\"US\",\"city\":\"Marion\",\"airport\":\"20OI\"}," +
                "\"carrier\":\"Riverside Air Service\"," +
                "\"departureTime\":\"2030 - 02 - 07 23:24\"," +
                "\"arrivalTime\":\"2030 - 02 - 09 02:24\"}";
            string[] arr = new string[] { "IE", "Killarney", "EIKY", "US", "Marion", "20OI", "Riverside Air Service", "2030 - 02 - 07 23:24", "2030 - 02 - 09 02:24" };
            Flight expected = new Flight();
            expected.From.Country = arr[0];
            expected.From.City = arr[1];
            expected.From.Airport = arr[2];
            expected.To.Country = arr[3];
            expected.To.City = arr[4];
            expected.To.Airport = arr[5];
            expected.Carrier = arr[6];
            expected.DepartureTime = Convert.ToDateTime(arr[7]);
            expected.ArrivalTime = Convert.ToDateTime(arr[8]);

            //Act
            var actual = JSObjToFlightObj(JSObj);

            //Assert

            Assert.AreEqual(expected.From.Country, actual.From.Country);
            Assert.AreEqual(expected.From.City, actual.From.City);
            Assert.AreEqual(expected.From.Airport, actual.From.Airport);
            Assert.AreEqual(expected.To.Country, actual.To.Country);
            Assert.AreEqual(expected.To.City, actual.To.City);
            Assert.AreEqual(expected.To.Airport, actual.To.Airport);
            Assert.AreEqual(expected.Carrier, actual.Carrier);
            Assert.AreEqual(expected.DepartureTime, actual.DepartureTime);
            Assert.AreEqual(expected.ArrivalTime, actual.ArrivalTime);
        }

        [Test]
        public void AddIdToResponseString_ShouldFunctionWithPerfectInput()
        {
            //Arrange
            long flightID = 6;
            string JSObj = "{\"from\":" +
                "{\"country\":\"IE\",\"city\":\"Killarney\",\"airport\":\"EIKY\"},\"to\":" +
                "{\"country\":\"US\",\"city\":\"Marion\",\"airport\":\"20OI\"}," +
                "\"carrier\":\"Riverside Air Service\"," +
                "\"departureTime\":\"2030 - 02 - 07 23:24\"," +
                "\"arrivalTime\":\"2030 - 02 - 09 02:24\"}";

            string expected = "{\"from\":" +
                "{\"country\":\"IE\",\"city\":\"Killarney\",\"airport\":\"EIKY\"},\"to\":" +
                "{\"country\":\"US\",\"city\":\"Marion\",\"airport\":\"20OI\"}," +
                "\"carrier\":\"Riverside Air Service\"," +
                "\"departureTime\":\"2030 - 02 - 07 23:24\"," +
                "\"arrivalTime\":\"2030 - 02 - 09 02:24\",\"id\":\"6\"}";


            //Act
            var actual = AddIdToResponseString(flightID, JSObj);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase()]
        public void FlightToResponseBody_ShouldFunctionWithPerfectInput()
        {
            //Arrange
            string[] arr = new string[] { "IE", "Killarney", "EIKY", "US", "Marion", "20OI", "Riverside Air Service", "2030 - 02 - 07 23:24", "2030 - 02 - 09 02:24" };
            Flight flight = new Flight();
            flight.From.Country = arr[0];
            flight.From.City = arr[1];
            flight.From.Airport = arr[2];
            flight.To.Country = arr[3];
            flight.To.City = arr[4];
            flight.To.Airport = arr[5];
            flight.Carrier = arr[6];
            flight.DepartureTime = Convert.ToDateTime(arr[7]);
            flight.ArrivalTime = Convert.ToDateTime(arr[8]);

            string expected = $"{{ \"arrivalTime\": \"{flight.ArrivalTime.ToString("yyyy-MM-dd HH:mm")}\"," +
                $" \"carrier\": \"{flight.Carrier}\"," +
                $" \"departureTime\": \"{flight.DepartureTime.ToString("yyyy-MM-dd HH:mm")}\", " +
                $"\"from\": {{ \"airport\": \"{flight.From.Airport}\", " +
                $"\"city\": \"{flight.From.City}\", " +
                $"\"country\": \"{flight.From.Country}\"}}, " +
                $"\"id\": \"{flight.Id}\", \"to\": {{ \"airport\": " +
                $"\"{flight.To.Airport}\", " +
                $"\"city\": \"{flight.To.City}\", " +
                $"\"country\": \"{flight.To.Country}\"}} }}";

            //Act
            var actual = FlightToResponseBody(flight);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        //[Test]
        //[TestCase()]
        //public void FillFlightObject()
        //{
        //    //Arrange
        //    //Act
        //    //Assert

        //    Assert.Pass();
        //}

        //[Test]
        //[TestCase()]
        //public void FillFlightObject()
        //{
        //    //Arrange
        //    //Act
        //    //Assert

        //    Assert.Pass();
        //}

        //[Test]
        //[TestCase()]
        //public void FillFlightObject()
        //{
        //    //Arrange
        //    //Act
        //    //Assert

        //    Assert.Pass();
        //}

        //[Test]
        //[TestCase()]
        //public void FillFlightObject()
        //{
        //    //Arrange
        //    //Act
        //    //Assert

        //    Assert.Pass();
        //}

        //[Test]
        //[TestCase()]
        //public void FillFlightObject()
        //{
        //    //Arrange
        //    //Act
        //    //Assert

        //    Assert.Pass();
        //}


        //[Test]
        //public void URLIsClearDatabase_ShouldDetectClearDatabase()
        //{
        //    //Arrange
        //    //Http
        //    //Act
        //    //URLIsClearDatabase();
        //    //Assert

        //    Assert.Pass();


        //}




    }
}