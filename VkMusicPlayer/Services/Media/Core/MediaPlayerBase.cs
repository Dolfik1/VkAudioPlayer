using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkMusicPlayer.Services.Media.Core
{
    public enum MediaEngine
    {
        /// <summary>
        /// Windows Media Player engine
        /// </summary>
        Wmp,
        /// <summary>
        /// NAudio engine
        /// </summary>
        NAudio
    }

    public abstract class MediaPlayerBase : IDisposable
    {
        //fields
        public abstract TimeSpan Position { get; set; }
        public abstract TimeSpan Duration { get; }
        public abstract Uri Source { get; set; }
        public abstract double Volume { get; set; }

        //events
        public EventHandler MediaOpened;
        public EventHandler MediaEnded;
        public EventHandler<Exception> MediaFailed;

        //methods
        public abstract void Initialize();
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();

        public abstract void Dispose();
    }
}
