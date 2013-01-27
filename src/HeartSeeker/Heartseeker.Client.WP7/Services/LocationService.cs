using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;
using System.Collections.Generic;

namespace Heartseeker.Services
{
    public class LocationService
    {
        public LocationService() { }

        public GeoCoordinate GetHeartLocation()
        {
            GeoCoordinate heart = new GeoCoordinate();
            heart.Latitude = 33.0511;
            heart.Longitude = -96.6752;
            return heart;
        }

        public double GetDistanceFromHeart(GeoCoordinate myLocation, GeoCoordinate heartLocation)
        {
            double myLongitude = Math.Abs(myLocation.Longitude);
            double myLatitude = Math.Abs(myLocation.Latitude);

            double heartLongitude = Math.Abs(heartLocation.Longitude);
            double heartLatitude = Math.Abs(heartLocation.Latitude);

            double longDiff = 0;
            double latDiff = 0;

            if (heartLongitude < myLongitude)
                longDiff = myLongitude - heartLongitude;
            else
                longDiff = heartLongitude - myLongitude;

            if (heartLatitude < myLatitude)
                latDiff = myLatitude - heartLatitude;
            else
                latDiff = heartLatitude - myLatitude;

            double distance = Math.Sqrt(longDiff * longDiff + latDiff * latDiff);
            return distance;

        }
    }
}
