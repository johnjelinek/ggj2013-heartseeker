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
    [Route("/players/nearby/{Latitude}/{Longitude}", "GET")]
    public class PlayersNearby : IReturn<List<Player>>
    {
        public long[] Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>ResetPlayers DTO</summary>
    [Route("/players/reset")]
    public class ResetPlayers { }

    [Route("/heartbeat/{Latitude}/{Longitude}")]
    public class GetHeartBeat : IReturn<HeartBeatResponse>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class HeartBeatResponse
    {
        public int HeartBeatDistance { get; set; }
        public double HeartBeatDistanceInMeters { get; set; }
    }

    public class PlayersService : Service
    {
        public PlayerRepository Repository { get; set; } // Injected by IOC
        public Position Heart { get; set; }

        public PlayersService()
        {

        }

        public PlayersService(Position heart, PlayerRepository repository)
        {
            if (heart == null)
            {
                throw new ArgumentException("Heart", "Heart Position is Missing");
            }

            if (repository == null)
            {
                throw new ArgumentException("Repository", "Player Repository is Missing");
            }

            Heart = heart;
            Repository = repository;
        }

        /// <summary>Reset the PlayerRepository</summary>
        public object Any(ResetPlayers request)
        {
            var players = Repository.GetAll();
            Repository.DeleteByIds(players.Select(x => x.Id).ToArray());
            var random = new Random();
            // min + randomizer * (max - min) for lat and long
            Heart = new Position(33.048044 + random.NextDouble() * (33.055526 - 33.048044), -96.681275 + random.NextDouble() * (-96.672156 - -96.681275));
            return new { Result = "Game Reset" };
        }

        /// <summary>Get Players Near Coordinates</summary>
        /// <returns>List of Players Near the Coordinates provided</returns>
        public object Get(PlayersNearby request)
        {
            if (request.Id != null)
            {
                var player = Repository.GetByIds(request.Id).FirstOrDefault();
                if (player == null)
                {
                    throw new ArgumentException("Player Id", "Player Id Does Not Exist");
                }
                return player.Position.GetNearbyPlayers(Repository);
            }
            else
            {
                // check for coordinates
                if (request.Latitude == 0.0)
                {
                    throw new ArgumentException("Latitude", "Latitude is not acceptable");
                }
                if (request.Longitude == 0.0)
                {
                    throw new ArgumentException("Longitude", "Longitude is not acceptable");
                }

                var position = new Position(request.Latitude, request.Longitude);
                return position.GetNearbyPlayers(Repository);
            }
        }

        public object Post(Player request)
        {
            return Repository.Store(request);
        }

        public object Put(Player request)
        {
            return Repository.Store(request);
        }

        /// <summary>Get the HeartBeat</summary>
        public object Any(GetHeartBeat request)
        {
            // check for coordinates
            if (request.Latitude == 0.0)
            {
                throw new ArgumentException("Latitude", "Latitude is not acceptable");
            }
            if (request.Longitude == 0.0)
            {
                throw new ArgumentException("Longitude", "Longitude is not acceptable");
            }

            // return number between 0 (lowest) - 5 (furthest)
            var haversine = new Haversine();
            var heartbeatDistance = new HeartBeatResponse();
            heartbeatDistance.HeartBeatDistanceInMeters = haversine.Distance(new Position(request.Latitude, request.Longitude), Heart, DistanceType.Kilometers) * 1000;

            if (heartbeatDistance.HeartBeatDistanceInMeters >= 200)
            {
                heartbeatDistance.HeartBeatDistance = 5;
            }
            else if (heartbeatDistance.HeartBeatDistanceInMeters > 150)
            {
                heartbeatDistance.HeartBeatDistance = 4;
            }
            else if (heartbeatDistance.HeartBeatDistanceInMeters > 100)
            {
                heartbeatDistance.HeartBeatDistance = 3;
            }
            else if (heartbeatDistance.HeartBeatDistanceInMeters > 70)
            {
                heartbeatDistance.HeartBeatDistance = 2;
            }
            else if (heartbeatDistance.HeartBeatDistanceInMeters > 30)
            {
                heartbeatDistance.HeartBeatDistance = 1;
            }
            else if (heartbeatDistance.HeartBeatDistanceInMeters <= 30)
            {
                heartbeatDistance.HeartBeatDistance = 0;
            }
            else
            {
                // falls through
                heartbeatDistance.HeartBeatDistance = 5;
            }

            return heartbeatDistance;
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