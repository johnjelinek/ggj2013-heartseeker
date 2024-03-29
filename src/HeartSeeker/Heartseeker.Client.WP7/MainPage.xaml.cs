﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Heartseeker.Services;
using Microsoft.Phone.Controls.Maps.Platform;
using System.Device.Location;
using System.Windows.Media.Imaging;
using Microsoft.Phone.BackgroundAudio;
using System.IO;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Heartseeker
{
    public partial class MainPage : PhoneApplicationPage
    {
        private GeoCoordinateWatcher locationManager;
        private GeoCoordinate heartLocation;
        private double zoomLevel = 17.00;
        private MapLayer heartSeekerMapLayer;
        private LocationService locationService;
        private SoundService soundService;
        private double distanceToHeart;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            
            locationService = new LocationService();
            soundService = new SoundService();

            soundService.CopySoundBitesToIsolatedStorage();
            heartLocation = locationService.GetHeartLocation();

            locationManager = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            locationManager.MovementThreshold = 5.0;  // in meters
            locationManager.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(locationManager_StatusChanged);
            locationManager.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(locationManager_PositionChanged);
            heartSeekerMap.ZoomBarVisibility = Visibility.Visible; 

            heartSeekerMapLayer = new MapLayer();
            heartSeekerMap.Children.Add(heartSeekerMapLayer);


            heartSeekerMap.Mode = new AerialMode();

            locationManager.Start();
            SetupTimer();
        }

        string currentAudioTrack = "";

        private void PlaySound(string soundBite)
        {
            //if (soundBite.Equals(currentAudioTrack))
            //    return;

            currentAudioTrack = soundBite;
            // Play_wavFile(soundBite);
            BackgroundAudioPlayer player = BackgroundAudioPlayer.Instance;
            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                player.Pause();
            }
            player.Track = soundService.GetAudioTrack(soundBite);
            player.Stop();
            player.Play();
            player.PlayStateChanged += new EventHandler(player_PlayStateChanged);
        }

        void player_PlayStateChanged(object sender, EventArgs e)
        {
            //BackgroundAudioPlayer player = BackgroundAudioPlayer.Instance;
            //if (player.PlayerState == PlayState.Stopped)
            //{
            //    player.Play();
            //}
        }

        void Play_wavFile(string soundBite) 
        { 
            StreamResourceInfo _stream = 
                Application.GetResourceStream(new Uri("sounds/" + soundBite, UriKind.Relative)); 
            SoundEffect _soundeffect = SoundEffect.FromStream(_stream.Stream); 
            SoundEffectInstance soundInstance = _soundeffect.CreateInstance(); 
            FrameworkDispatcher.Update(); 
            soundInstance.Play(); 
        }


        // Called when players current position has changed
        void locationManager_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> newLocation)
        {

            Image pushpinImage = GetIcon("icon_pushpin.png", 0.75);
            try
            {
                distanceToHeart = newLocation.Position.Location.GetDistanceTo(heartLocation);
                GeoCoordinate myCurrentPosition = new GeoCoordinate(newLocation.Position.Location.Latitude, newLocation.Position.Location.Longitude);
                heartSeekerMapLayer.AddChild(pushpinImage, myCurrentPosition);
                heartSeekerMapLayer.UpdateLayout();
                locationService.ReportMyLocation(newLocation.Position.Location);

            }
            catch (Exception ex)
            {
                DistanceToHeart.Text = ex.Message;
            }
        }

        void locationManager_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            int i = 0;
            // throw new NotImplementedException();
            if (e.Status == GeoPositionStatus.Ready)
                heartSeekerMap.SetView(locationManager.Position.Location, zoomLevel);
        }

        private void ApplicationBarAerialMode_Click(object sender, EventArgs e)
        {
            heartSeekerMap.Mode = new AerialMode();
        }

        private void ApplicationBarRoadMode_Click(object sender, EventArgs e)
        {
            heartSeekerMap.Mode = new RoadMode();
        }

        // Show the user where the heart is
        private void ApplicationBarShowMe_Click(object sender, EventArgs e)
        {
            
            // heartSeekerMapLayer.Children.Clear();   // clear existing children

            GeoCoordinate heartLocation = locationService.GetHeartLocation();
            Image heartPushpinImage = GetIcon("icon_pushpin.png", 0.75);
            //Image heartImage = GetIcon("heartseeker.png", 0.25);

            // Add pins for my current position and where the heart is
            // My position is always centered in the map
            // heartSeekerMapLayer.AddChild(heartImage, heartLocation);
            heartSeekerMapLayer.AddChild(heartPushpinImage, heartLocation);
        }


        private Image GetIcon(string iconName, double opacity)
        {
            Image pushpinImage = new Image();
            pushpinImage.Source = new BitmapImage(new Uri(iconName, UriKind.Relative));
            pushpinImage.Opacity = opacity;
            pushpinImage.Stretch = System.Windows.Media.Stretch.None;
            return pushpinImage;
        }

        private void SetupTimer()
        {
            System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 1000); // 2000 Milliseconds
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (distanceToHeart < 30.0) // was 0.0002
            {
                this.DistanceToHeart.Text = "Whoop!! You Found the heart!!";
            }
            else
            {
                if (!double.IsNaN(distanceToHeart))
                    this.DistanceToHeart.Text = "You are " + distanceToHeart.ToString("0.00") + " meters away!";
            }

            if (distanceToHeart > 400) 
            {
                PlaySound("Heartbeat_Speed1.wav");
            }
            else if (distanceToHeart > 300)
            {
                PlaySound("Heartbeat_Speed2.wav");
            }
            else if (distanceToHeart > 200) 
            {
                PlaySound("Heartbeat_Speed3.wav");
            }
            else if (distanceToHeart > 100) 
            {
                PlaySound("Heartbeat_Speed4.wav");
            }
            else if (distanceToHeart > 30)
            {
                PlaySound("Heartbeat_Speed5.wav");
            }
            else
            {
                PlaySound("Heartbeat_Speed5.wav");
            }
        }

        private void ApplicationBarClearPins_Click(object sender, EventArgs e)
        {
            heartSeekerMapLayer.Children.Clear();
            GeoCoordinate heartLocation = locationService.GetHeartLocation();
            Image heartPushpinImage = GetIcon("icon_pushpin.png", 0.75);
            heartSeekerMapLayer.AddChild(heartPushpinImage, heartLocation);
            heartSeekerMapLayer.UpdateLayout();

        }


        private void ApplicationBarFindMe_Click(object sender, EventArgs e)
        {
            Image myImage = GetIcon("icon_pushpin.png", 0.75);

            heartSeekerMapLayer.AddChild(myImage, locationManager.Position.Location);
            heartSeekerMapLayer.UpdateLayout();

        }

        private void ApplicationBarPlaySound_Click(object sender, EventArgs e)
        {
            PlaySound("Heartbeat_Speed5.wav");
        }
    }
}