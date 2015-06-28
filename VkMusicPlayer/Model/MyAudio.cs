using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VkMusicPlayer.Helpers;
using VkNet.Enums;
using VkNet.Model.Attachments;

namespace VkMusicPlayer.Model
{
    public class MyAudio : DependencyObject
    {
        public MyAudio(Audio audio)
        {
            AlbumId = audio.AlbumId;
            Artist = audio.Artist;
            Duration = audio.Duration;
            DurationString = StringHelper.SecondsToTimeString(audio.Duration);
            Genre = audio.Genre;
            Id = audio.Id;
            LyricsId = audio.LyricsId;
            OwnerId = audio.OwnerId;
            Title = audio.Title;
            Url = audio.Url;
            IsPlaying = false;
            
        }

        public MyAudio()
        {
            IsPlaying = false;
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(MyAudio));

        public bool IsPlaying
        {
            get
            {
                return (bool)GetValue(IsPlayingProperty);
            }
            set
            {
                SetValue(IsPlayingProperty, value);
            }
        }

        public static readonly DependencyProperty DurationStringProperty =
            DependencyProperty.Register("DurationString", typeof(string), typeof(MyAudio));

        public string DurationString
        {
            get
            {
                return (string)GetValue(DurationStringProperty);
            }
            set
            {
                SetValue(DurationStringProperty, value);
            }
        }
        
        public int Duration { get; set; }

        public long? AlbumId { get; set; }

        public string Artist { get; set; }

        public AudioGenre? Genre { get; set; }

        public long Id { get; set; }

        public long? LyricsId { get; set; }

        public long? OwnerId { get; set; }

        public string Title { get; set; }

        public Uri Url { get; set; }
    }
}
