using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VkMusicPlayer.Model;
using VkNet.Model.Attachments;

namespace VkMusicPlayer.ViewModel
{
    class PlayerWindowViewModel : ViewModelBase
    {
        IReadOnlyCollection<Audio> lst;
        Random rnd = new Random();
        public PlayerWindowViewModel()
        {
            lst = _vk.Audio.Get(0, null, null, 200, 0);
            CurrentAudio = new MyAudio(lst.First());
            Timer timer = new Timer(10000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentAudio = new MyAudio(lst.ElementAt(rnd.Next(0, lst.Count)));
            SearchValue = CurrentAudio.Title;
            Debug.Write("");
        }

        private string _searchValue;
        public string SearchValue
        {
            get
            {
                return _searchValue;
            }
            set
            {
                _searchValue = value;
                RaisePropertyChanged("SearchValue");
            }
        }

        public MyAudio CurrentAudio { get; set; }
    }
}
