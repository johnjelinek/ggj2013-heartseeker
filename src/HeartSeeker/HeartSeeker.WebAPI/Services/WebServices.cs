using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using HaversineFormula;

namespace HeartSeeker.Services
{
    /// <summary>Player DTO</summary>
    [Route("/players", "POST")]
    [Route("/players/{Id}", "PUT")]
    public class Player
    {
        public long Id { get; set; }
        public Position Position { get; set; }

        public Player(Position position)
        {
            Position = position;
        }
    }

    /// <summary>PlayersNearMe DTO</summary>
    [Route("/players/{Id}/nearby", "GET")]
    public class PlayersNearMe : IReturn<List<Player>> { public long[] Id { get; set; } }

    /// <summary>ResetPlayers DTO</summary>
    [Route("/players/reset")]
    public class ResetPlayers { }

    [Route("/players/nearby", "GET")]
    public class PlayersNearby : IReturn<List<Player>> { public Position Position { get; set; } }

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

        /// <summary>Get Players near coordinates</summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Any(PlayersNearby request)
        {
            if (request.Position == null)
            {
                throw new ArgumentException("Position", "Position is not acceptable");
            }

            return request.Position.GetNearbyPlayers(Repository);
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

            return player.Position.GetNearbyPlayers(Repository);
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
                existing.Position = player.Position;
            }

            return player;
        }

        public void DeleteByIds(params long[] ids)
        {
            players.RemoveAll(x => ids.Contains(x.Id));
        }
    }

    public static class PositionExtensions
    {
        public static List<Player> GetNearbyPlayers(this Position position, PlayerRepository players)
        {
            const double MAXDISTANCE = 0.2; // 200 Meters
            var haversine = new Haversine();
            var nearby = players.GetAll().Where(x => haversine.Distance(position, x.Position, DistanceType.Kilometers) < MAXDISTANCE);
            return nearby.ToList();
        }
    }
}