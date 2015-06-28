using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using VkMusicPlayer.Commands;
using VkMusicPlayer.Model;
using VkMusicPlayer.Services.Media.Core;
using VkMusicPlayer.Util;

namespace VkMusicPlayer.Styles.Controls
{
    /// <summary>
    /// Логика взаимодействия для VKStreamingPanel.xaml
    /// </summary>
    public partial class VKStreamingPanel : UserControl, INotifyPropertyChanged
    {

        public enum PlaybackState
        {
            Stopped,
            Playing,
            Paused
        }

        private DispatcherTimer _positionTimer;

        public VKStreamingPanel()
        {
            InitializeComponent();
            DataContext = this;
            Volume = 100;

            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(950);
            _positionTimer.Tick += PositionTimerTick;

            if (IsPlaying)
                _positionTimer.Start();
        }

        private MediaPlayerBase _mediaPlayer;

        private MediaPlayerBase MediaPlayer
        {
            get
            {
                if (_mediaPlayer == null)
                {
                    _mediaPlayer = (MediaPlayerBase)new NaudioMediaPlayer();

                    _mediaPlayer.Initialize();
                    _mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
                    _mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
                    _mediaPlayer.MediaOpened += MediaPlayerOnMediaOpened;
                    _mediaPlayer.Volume = Volume;
                }

                return _mediaPlayer;
            }
        }

        private volatile PlaybackState _state;
        public PlaybackState State
        {

            get { return _state; }
            set
            {
                if (_state == value)
                    return;

                _state = value;

                if (_state == PlaybackState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();
            }
        }

        private void PositionTimerTick(object sender, object e)
        {
            try
            {
                //possible fix for not switching tracks issue
                UpdateProgress(MediaPlayer.Position.TotalSeconds);

                if (MediaPlayer.Position > MediaPlayer.Duration)
                    MediaPlayerOnMediaEnded(MediaPlayer, EventArgs.Empty);

                PlayingTime = MediaPlayer.Position.TotalHours > 1 ? MediaPlayer.Position.ToString(@"hh\:mm\:ss") : MediaPlayer.Position.ToString(@"mm\:ss");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #region Bindings

        private int _volume;
        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                if (MediaPlayer != null)
                    MediaPlayer.Volume = _volume / 100.0f;
                RaisePropertyChanged("Volume");
            }
        }

        private int _playingProgress;

        public int PlayingProgress
        {
            get
            {
                return _playingProgress;
            }
            set
            {
                if (MediaPlayer == null)
                    return;

                if (MediaPlayer.Position.TotalSeconds == value)
                    return;

                MediaPlayer.Position = TimeSpan.FromSeconds(value);
                _playingProgress = value;
                PlayingTime = MediaPlayer.Position.TotalHours > 1 ? MediaPlayer.Position.ToString(@"hh\:mm\:ss") : MediaPlayer.Position.ToString(@"mm\:ss");
                RaisePropertyChanged("PlayingProgress");
            }
        }

        private int _playingAudioDuration;
        public int PlayingAudioDuration
        {
            get
            {
                return _playingAudioDuration;
            }
            set
            {
                _playingAudioDuration = value;
                RaisePropertyChanged("PlayingAudioDuration");
            }
        }

        public int PlayingAudioIdx
        {
            get { return (int)GetValue(PlayingAudioIdxProperty); }
            set { SetValue(PlayingAudioIdxProperty, value); }
        }

        public static readonly DependencyProperty PlayingAudioIdxProperty =
            DependencyProperty.Register("PlayingAudioIdx", typeof(int), typeof(VKStreamingPanel),
                new FrameworkPropertyMetadata(-1,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnPlayingAudioIdxChanged)));

        public ObservableCollection<MyAudio> Playlist
        {
            get { return (ObservableCollection<MyAudio>)GetValue(PlaylistProperty); }
            set { SetValue(PlaylistProperty, value); }
        }

        public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register("Playlist", typeof(ObservableCollection<MyAudio>), typeof(VKStreamingPanel), null);


        private static void OnPlayingAudioIdxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newval = (int)e.NewValue;
            int oldval = (int)e.OldValue;
            VKStreamingPanel panel = (d as VKStreamingPanel);
            if (panel.Playlist != null && newval + 1 < panel.Playlist.Count && newval >= 0)
            {
                if (oldval + 1 < panel.Playlist.Count && oldval >= 0)
                    panel.Dispatcher.Invoke(() => panel.Playlist[oldval].IsPlaying = false);

                MyAudio audio = panel.Playlist[newval];
                panel.CurrentArtist = audio.Artist;
                panel.CurrentTitle = audio.Title;
                panel.PlayingProgress = 0;
                panel.PlayingTime = "00:00";
                panel.PlayingAudioDuration = audio.Duration;
                if (panel.IsPlaying)
                    panel.PlayExecute();
            }
        }

        public string Test
        {
            get { return (string)GetValue(TestProperty); }
            set { SetValue(TestProperty, value); }
        }

        public static readonly DependencyProperty TestProperty =
            DependencyProperty.Register("Test", typeof(string), typeof(VKStreamingPanel), new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
            });

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        private string _playingTime;
        public string PlayingTime
        {
            get
            {
                return _playingTime;
            }
            set
            {
                _playingTime = value;
                RaisePropertyChanged("PlayingTime");
            }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(VKStreamingPanel), new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
            });

        private string _currentArtist = "Artist";
        public string CurrentArtist
        {
            get
            {
                return _currentArtist;
            }
            set
            {
                _currentArtist = value;
                RaisePropertyChanged("CurrentArtist");
            }
        }

        private string _currentTitle = "Title";
        public string CurrentTitle
        {
            get
            {
                return _currentTitle;
            }
            set
            {
                _currentTitle = value;
                RaisePropertyChanged("CurrentTitle");
            }
        }

        #endregion

        private MyAudio PlayingAudio
        {
            get
            {
                return Playlist[PlayingAudioIdx];
            }
            set
            {
                Playlist[PlayingAudioIdx] = value;
            }
        }

        private void ShowError(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        private void ShowMessage(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "Информация", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void PlayExecute()
        {
            if (PlayingAudio != null)
            {
                if (State != PlaybackState.Paused)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Source = PlayingAudio.Url;
                }
                PlayingAudio.IsPlaying = true;
                State = PlaybackState.Playing;
                MediaPlayer.Play();
            }
            else
            {
                ShowMessage("Выберите аудиозапись для проигрывания");
            }

        }

        private void PauseExecute()
        {
            State = PlaybackState.Paused;
            MediaPlayer.Pause();
        }

        private void StopExecute()
        {
            State = PlaybackState.Stopped;
            MediaPlayer.Stop();
        }

        private void MediaPlayerOnMediaOpened(object sender, EventArgs e)
        {
            State = PlaybackState.Playing;
            Dispatcher.Invoke(() =>
            {
                Playlist[PlayingAudioIdx].IsPlaying = true;
            });
        }

        private void MediaPlayerOnMediaFailed(object sender, Exception e)
        {
            if (PlayingAudio != null)
                Debug.WriteLine("Media failed " + PlayingAudio.Id + " " + PlayingAudio.Artist + " - " + PlayingAudio.Title + ". " + e);

            if (e is InvalidWmpVersionException)
            {
                ShowError(e.ToString());
                return;
            }

            if (e is COMException)
            {
                var com = (COMException)e;
                if ((uint)com.ErrorCode == 0xC00D0035) //not found or connection problem
                {
                    ShowError(e.ToString());
                    return;
                }
            }
        }

        private void MediaPlayerOnMediaEnded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayingAudio.IsPlaying = false;
                PlayingAudioIdx++;
                PlayingAudio.IsPlaying = true;
            });
            //SwitchNext();
        }

        private void UpdateProgress(double value)
        {
            _playingProgress = (int)value;
            RaisePropertyChanged("PlayingProgress");
        }

        private void ImageButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(Test);
            IsPlaying = !IsPlaying;
            if (IsPlaying)
                PlayExecute();
            else
                PauseExecute();
        }

        #region INotifyPropertyChanged Members

        protected void RaisePropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}
