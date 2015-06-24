using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using VkMusicPlayer.Commands;
using VkMusicPlayer.Model;
using VkMusicPlayer.Util;

namespace VkMusicPlayer.Styles.Controls
{
    /// <summary>
    /// Логика взаимодействия для VKStreamingPanel.xaml
    /// </summary>
    public partial class VKStreamingPanel : UserControl, INotifyPropertyChanged
    {

        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        public VKStreamingPanel()
        {
            InitializeComponent();
            DataContext = this;

            _timer1.Interval = 250;
            _timer1.Enabled = false;
            _timer1.Tick += timer1_Tick;
        }

        private BufferedWaveProvider _bufferedWaveProvider;
        private IWavePlayer _waveOut;
        private volatile StreamingPlaybackState _playbackState;
        private volatile bool _fullyDownloaded;
        private HttpWebRequest _webRequest;
        private VolumeWaveProvider16 _volumeProvider;
        private System.Windows.Forms.Timer _timer1 = new System.Windows.Forms.Timer();

        #region Bindings

        private int _volume = 100;
        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                if (_volumeProvider != null)
                    _volumeProvider.Volume = _volume / 100;
                RaisePropertyChanged("Volume");
            }
        }

        private int _playingProgress;

        public int PlayingProgress
        {
            get { return _playingProgress; }
            set
            {
                _playingProgress = value;
                RaisePropertyChanged("PlayingProgress");
            }
        }

        public MyAudio PlayingAudio
        {
            get { return (MyAudio)GetValue(PlayingAudioProperty); }
            set { SetValue(PlayingAudioProperty, value); }
        }

        public static readonly DependencyProperty PlayingAudioProperty =
            DependencyProperty.Register("PlayingAudio", typeof(MyAudio), typeof(VKStreamingPanel), new UIPropertyMetadata(null));

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

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(VKStreamingPanel), new FrameworkPropertyMetadata
            {
                BindsTwoWayByDefault = true,
            });


        public string CurrentArtist { get; set; }

        private string CurrentTitle { get; set; }

        #endregion
        
        /*

        delegate void ShowErrorDelegate(string message);

        private void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }
        */

        private void ShowError(string message)
        {
            System.Windows.Forms.MessageBox.Show(message, "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }


        private void StreamMp3(object state)
        {

            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //SettingsSection section = (SettingsSection)config.GetSection("system.net/settings");
            //section.HttpWebRequest.UseUnsafeHeaderParsing = true;
            //config.Save();


            this._fullyDownloaded = false;
            //string url = (string)state;
            Uri uri = null;
            //string artist = "", title = "";
            Dispatcher.Invoke(() => 
            {
                uri = PlayingAudio.Url;
                CurrentArtist = PlayingAudio.Artist;
                CurrentTitle = PlayingAudio.Title;
            });
            _webRequest = (HttpWebRequest)WebRequest.Create(uri);

            int metaInt = 0; // blocksize of mp3 data

            _webRequest.Headers.Clear();
            _webRequest.Headers.Add("GET", "/ HTTP/1.0");
            // needed to receive metadata informations
            _webRequest.Headers.Add("Icy-MetaData", "1");
            _webRequest.UserAgent = "WinampMPEG/5.09";

            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)_webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    ShowError(e.Message);
                }
                return;
            }
            byte[] buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            try
            {
                // read blocksize to find metadata block
                metaInt = Convert.ToInt32(resp.GetResponseHeader("icy-metaint"));

            }
            catch { }

            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (var responseStream = resp.GetResponseStream())
                {
                    var readFullyStream = new ReadFullyStream(responseStream);
                    do
                    {
                        if (_bufferedWaveProvider != null && _bufferedWaveProvider.BufferLength - _bufferedWaveProvider.BufferedBytes < _bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4)
                        {
                            //Debug.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame = null;
                            try
                            {

                                frame = Mp3Frame.LoadFromStream(readFullyStream, true);

                                //CurrentArtist = artist;
                                //CurrentTitle = title;


                            }
                            catch (EndOfStreamException)
                            {
                                this._fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                                decompressor = new AcmMp3FrameDecompressor(waveFormat);
                                this._bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                this._bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                                //this.bufferedWaveProvider.BufferedDuration = 250;
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                            _bufferedWaveProvider.AddSamples(buffer, 0, decompressed);


                        }

                    } while (_playbackState != StreamingPlaybackState.Stopped);
                    //Debug.WriteLine("Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    decompressor.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        private void PlayExecute()
        {
            if (_playbackState == StreamingPlaybackState.Stopped)
            {
                _playbackState = StreamingPlaybackState.Buffering;
                this._bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(new WaitCallback(StreamMp3), null);
                _timer1.Enabled = true;
            }
            else if (_playbackState == StreamingPlaybackState.Paused)
            {
                _playbackState = StreamingPlaybackState.Buffering;
            }
        }

        private void StopPlayback()
        {
            if (_playbackState != StreamingPlaybackState.Stopped)
            {
                if (!_fullyDownloaded)
                {
                    _webRequest.Abort();
                }
                this._playbackState = StreamingPlaybackState.Stopped;
                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }
                _timer1.Enabled = false;
                // n.b. streaming thread may not yet have exited
                Thread.Sleep(500);
                ShowBufferState(0);
            }
        }

        private void ShowBufferState(double totalSeconds)
        {
            //labelBuffered.Text = String.Format("{0:0.0}s", totalSeconds);
            //progressBarBuffer.Value = (int)(totalSeconds * 1000);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_playbackState != StreamingPlaybackState.Stopped)
            {
                if (this._waveOut == null && this._bufferedWaveProvider != null)
                {
                    //Debug.WriteLine("Creating WaveOut Device");
                    this._waveOut = CreateWaveOut();
                    _waveOut.PlaybackStopped += waveOut_PlaybackStopped;
                    this._volumeProvider = new VolumeWaveProvider16(_bufferedWaveProvider);
                    this._volumeProvider.Volume = Volume / 100;
                    _waveOut.Init(_volumeProvider);
                    //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                }
                else if (_bufferedWaveProvider != null)
                {
                    var bufferedSeconds = _bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    ShowBufferState(bufferedSeconds);
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && this._playbackState == StreamingPlaybackState.Playing && !this._fullyDownloaded)
                    {
                        this._playbackState = StreamingPlaybackState.Buffering;
                        _waveOut.Pause();
                        //Debug.WriteLine(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                    }
                    else if (bufferedSeconds > 4 && this._playbackState == StreamingPlaybackState.Buffering)
                    {
                        _waveOut.Play();
                        //Debug.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                        this._playbackState = StreamingPlaybackState.Playing;
                    }
                    else if (this._fullyDownloaded && bufferedSeconds == 0)
                    {
                        //Debug.WriteLine("Reached end of stream");
                        StopPlayback();
                    }
                }

            }
        }

        private IWavePlayer CreateWaveOut()
        {
            return new WaveOut();
            //return new DirectSoundOut();
        }

        private void MP3StreamingPanel_Disposing(object sender, EventArgs e)
        {
            StopPlayback();
        }

        private void PauseExecute()
        {
            if (_playbackState == StreamingPlaybackState.Playing || _playbackState == StreamingPlaybackState.Buffering)
            {
                _waveOut.Pause();
                //Debug.WriteLine(String.Format("User requested Pause, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                _playbackState = StreamingPlaybackState.Paused;
            }
        }

        private void StopExecute()
        {
            StopPlayback();
        }

        private void waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //Debug.WriteLine("Playback Stopped");
            if (e.Exception != null)
            {
                MessageBox.Show(String.Format("Playback Error {0}", e.Exception.Message));
            }
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
