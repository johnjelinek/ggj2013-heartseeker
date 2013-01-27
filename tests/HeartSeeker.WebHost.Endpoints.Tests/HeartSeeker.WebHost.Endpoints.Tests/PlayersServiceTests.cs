using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.WebHost.Endpoints;
using HeartSeeker.Services;
using Funq;
using ServiceStack.ServiceClient.Web;
using HaversineFormula;

namespace HeartSeeker.WebHost.Endpoints.Tests
{
    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("Players Tests", typeof(PlayersService).Assembly) { }

        public override void Configure(Container container)
        {
            container.Register(new PlayersService(new Position(33.054123, -96.679988), new PlayerRepository()));
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
            var player = restClient.Post<Player>(String.Format("{0}/players", BaseUri), new Player(new Position(33.051281, -96.676662)));
            Assert.That(player.Id, Is.EqualTo(1));
            Assert.That(player.Position.Latitude, Is.EqualTo(33.051281));
            Assert.That(player.Position.Longitude, Is.EqualTo(-96.676662));

            player.Position = new Position(33.051524, -96.677306);
            player = restClient.Put<Player>(String.Format("{0}/players/{1}", BaseUri, player.Id), player);
            Assert.That(player.Position.Latitude, Is.EqualTo(33.051524));
            Assert.That(player.Position.Longitude, Is.EqualTo(-96.677306));

            var newPlayer = restClient.Post<Player>(String.Format("{0}/players", BaseUri), new Player(new Position(33.051281, -96.676662)));
            Assert.That(newPlayer.Id, Is.EqualTo(2));
            Assert.That(newPlayer.Position.Latitude, Is.EqualTo(33.051281));
            Assert.That(newPlayer.Position.Longitude, Is.EqualTo(-96.676662));

            var nearby = restClient.Get<List<Player>>(String.Format("{0}/players/{1}/nearby", BaseUri, player.Id));
            Assert.That(nearby.Count, Is.EqualTo(2));

            nearby = restClient.Get<List<Player>>(String.Format("{0}/players/nearby/{1}/{2}", BaseUri, 33.050130, -96.676641));
            Assert.That(nearby.Count, Is.EqualTo(2));

            var heartbeat = restClient.Get<int>(String.Format("{0}/heartbeat/{1}/{2}", BaseUri, player.Position.Latitude, player.Position.Longitude));
            Assert.NotNull(heartbeat);

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
