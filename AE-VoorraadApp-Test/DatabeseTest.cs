using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace AE_VoorraadApp_Test
{
    public class DatabaseTest
    {
        [SetUp]
        public void Setup()
        {

        }

        private class Location
        {
            [JsonPropertyName("Location")]
            public string Place { get; set; }
            public float Latitude { get; set; }
            public float Longitude { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        public class ProductStock
        {
            public int ID { get; set; }
            public int ProductId { get; set; }
            public int MachineId { get; set; }
            public int AvailableCount { get; set; }
            public int MinCount { get; set; }
            public int MaxCount { get; set; }
            public int RefillAdviceCount { get; set; }

        }

        private class _ProductStock
        {
            public int MachineId { get; set; }
            public List<ProductStock> ProductStock { get; set; }
        }

        [Test]
        public void ApiLocationsTest()
        {
            var content = ApiRequester("machines", "").Content;

            var actual = JsonSerializer.Deserialize<Location[]>(content);

            var actualList = actual.ToList();

            actualList = UniqueLocationsFilter(actualList);

            Assert.AreEqual(6, actualList.Count);
        }

        [Test]
        public void ApiUniqueFilterTest()
        {
            var content = ApiRequester("machines", "").Content;

            var actual = JsonSerializer.Deserialize<Location[]>(content);

            var actualList = actual.ToList();

            actualList = UniqueLocationsFilter(actualList);

            Assert.AreEqual(6, actualList.Count);
        }

        [Test]
        public void ApiTest()
        {
            var content = ApiRequester("machines/stock", "6483").Content;

            var actual = JsonSerializer.Deserialize<_ProductStock>(content) ?? throw new InvalidOperationException();

            Assert.AreEqual(6483, actual.MachineId);
        }

        private static List<Location> UniqueLocationsFilter(List<Location> locations)
        {
            var _locations = locations;

            foreach (var _location in _locations.ToList())
            {
                // var ido = _locations.IndexOf(_location);
                var locationDupCount = locations.FindAll(x => x.Place == _location.Place).Count;
                if (locationDupCount > 1)
                {
                    for (var i = 0; i < locationDupCount - 1; i++)
                    {
                        locations.Remove(_location);
                    }
                }
            }

            return locations;
        }

        private static IRestResponse ApiRequester(string header, string subheader)
        {
            var client = new RestClient($"https://api-staging-vendingweb.azurewebsites.net/api/external/{header}/")
            {
                Authenticator = new HttpBasicAuthenticator($"AppelEitjeTest", "appeleitje@!78")
            };

            var request = new RestRequest($"{subheader}", DataFormat.Json);

            return client.Get(request);
        }
    }
}