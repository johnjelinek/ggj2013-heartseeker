﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.BackgroundAudio;

namespace HeartSeekerAudioPlaybackAgent.Services
{
    public class AudioService
    {

        public AudioTrack GetAudioTrack(string soundBite)
        {
            AudioTrack audioTrack = new AudioTrack(new Uri(soundBite, UriKind.Relative), "", "", "", null);
            return audioTrack;
        }

    }
}
