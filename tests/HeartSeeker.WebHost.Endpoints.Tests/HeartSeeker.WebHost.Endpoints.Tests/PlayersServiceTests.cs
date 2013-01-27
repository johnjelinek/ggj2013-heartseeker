using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.WebHost.Endpoints;
using HeartSeeker.Services;
using Funq;
using ServiceStack.ServiceClient.Web;

namespace HeartSeeker.WebHost.Endpoints.Tests
{
    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("Players Tests", typeof(PlayersService).Assembly) { }

        public override void Configure(Container container)
        {
            container.Register(new PlayerRepository());
        }
    }

    [TestFixture]
    public class PlayersServiceTests
    {
        const string BaseUri = "http://localhost:82/";

        AppHost appHost;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            appHost = new AppHost();
            appHost.Init();
            appHost.Start(BaseUri);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            appHost.Dispose();
            appHost = null;
        }

        [Test]
        public void Run()
        {
            var restClient = new JsonServiceClient(BaseUri);
            var player = restClient.Post<Player>(String.Format("{0}/players", BaseUri), new Player { GeoCoordinates = { Latitude = 123, Longitude = 234 } });
            Assert.That(player.Id, Is.EqualTo(1));
            Assert.That(player.GeoCoordinates.Latitude, Is.EqualTo(123));
            Assert.That(player.GeoCoordinates.Longitude, Is.EqualTo(234));

            player.GeoCoordinates = new GeoCoordinates() { Latitude = 234, Longitude = 345 };
            player = restClient.Put<Player>(String.Format("{0}/players/{1}", BaseUri, player.Id), player);
            Assert.That(player.GeoCoordinates.Latitude, Is.EqualTo(234));
            Assert.That(player.GeoCoordinates.Longitude, Is.EqualTo(345));

            var nearby = restClient.Get<List<Player>>(String.Format("{0}/players/{1}/nearby", BaseUri, player.Id));
            Assert.IsNotNull(nearby);

            var reset = restClient.Get<ResetPlayers>(String.Format("{0}/players/reset", BaseUri));
            Assert.NotNull(reset);

            try
            {
                restClient.Get<List<Player>>(String.Format("{0}/players/{1}/nearby", BaseUri, player.Id));
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "ArgumentException");
            }
        }
    }
}
