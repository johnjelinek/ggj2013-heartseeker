using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace HeartSeeker.Services
{
    /// <summary>Player DTO</summary>
    [Route("/players", "POST")]
    [Route("/players/{Id}", "PUT")]
    public class Player
    {
        public long Id { get; set; }
        public GeoCoordinates GeoCoordinates { get; set; }

        public Player()
        {
            GeoCoordinates = new GeoCoordinates();
        }
    }

    /// <summary>PlayersNearMe DTO</summary>
    [Route("/players/{Id}/nearby", "GET")]
    public class PlayersNearMe : IReturn<List<Player>> { public long[] Id { get; set; } }

    /// <summary>ResetPlayers DTO</summary>
    [Route("/players/reset")]
    public class ResetPlayers { }

    /// <summary>GeoCoordinates</summary>
    public class GeoCoordinates
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class PlayersService : Service
    {
        public PlayerRepository Repository { get; set; } // Injected by IOC

        /// <summary>Reset the PlayerRepository</summary>
        public object Any(ResetPlayers request)
        {
            var players = Repository.GetAll();
            Repository.DeleteByIds(players.Select(x => x.Id).ToArray());
            return new { Result = "Game Reset" };
        }

        /// <summary>Get Players Near Me</summary>
        /// <returns>List of Players Near the Player Requested</returns>
        public object Get(PlayersNearMe request)
        {
            var player = Repository.GetByIds(request.Id).FirstOrDefault();
            if (player == null)
            {
                throw new ArgumentException("Player Id Does Not Exist");
            }

            return GetPlayersNearCoordinates(player.GeoCoordinates);
        }

        private List<Player> GetPlayersNearCoordinates(GeoCoordinates geoCoordinates)
        {
            // TODO: Add logic to Get Players
            return new List<Player>()
            {
                new Player { Id = 1 }
            };
        }

        public object Post(Player request)
        {
            return Repository.Store(request);
        }

        public object Put(Player request)
        {
            return Repository.Store(request);
        }
    }

    public class PlayerRepository
    {
        List<Player> players = new List<Player>();

        public List<Player> GetByIds(long[] ids)
        {
            return players.Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<Player> GetAll()
        {
            return players;
        }

        public Player Store(Player player)
        {
            var existing = players.FirstOrDefault(x => x.Id == player.Id);

            if (existing == null)
            {
                // add
                var newId = players.Count > 0 ? players.Max(x => x.Id) + 1 : 1;
                player.Id = newId;
                players.Add(player);
            }
            else
            {
                // update
                existing.GeoCoordinates = player.GeoCoordinates;
            }

            return player;
        }

        public void DeleteByIds(params long[] ids)
        {
            players.RemoveAll(x => ids.Contains(x.Id));
        }
    }
}