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
using Microsoft.Phone.BackgroundAudio;
using System.IO.IsolatedStorage;
using System.Windows.Resources;

namespace Heartseeker.Services
{
    public class SoundService
    {

        public AudioTrack GetAudioTrack(string soundBite)
        {
            AudioTrack audioTrack = new AudioTrack(new Uri(soundBite, UriKind.Relative), "", "", "", new Uri("heartseeker.png", UriKind.Relative));
            return audioTrack;
        }

        public void CopySoundBitesToIsolatedStorage()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = new string[] 
                { 
                    "Heartbeat_Speed1.wav", 
                    "Heartbeat_Speed2.wav", 
                    "Heartbeat_Speed3.wav", 
                    "Heartbeat_Speed4.wav", 
                    "Heartbeat_Speed5.wav"
                };

                foreach (var _fileName in files)
                {
                    if (!storage.FileExists(_fileName))
                    {
                        string _filePath = "sounds/" + _fileName;
                        StreamResourceInfo resource = Application.GetResourceStream(new Uri(_filePath, UriKind.Relative));

                        using (IsolatedStorageFileStream file = storage.CreateFile(_fileName))
                        {
                            int chunkSize = 4096;
                            byte[] bytes = new byte[chunkSize];
                            int byteCount;

                            while ((byteCount = resource.Stream.Read(bytes, 0, chunkSize)) > 0)
                            {
                                file.Write(bytes, 0, byteCount);
                            }
                        }
                    }
                }
            }
        }

    }
}
