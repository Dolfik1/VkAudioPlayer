using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VkMusicPlayer.Commands;
using VkMusicPlayer.Model;
using VkNet.Enums.Filters;
using VkNet.Model.Attachments;

namespace VkMusicPlayer.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            //_vk.Authorize(3541904, "", "", Settings.Audio);
            ReplaceAudiosList(_vk.Audio.Get(0));
        }

        private void ReplaceAudiosList(IEnumerable<Audio> audios)
        {
            AudiosList.Clear();
            foreach (Audio audio in audios)
            {
                AudiosList.Add(new MyAudio(audio));
            }
        }

        private void SearchExecute()
        {
            int x = 0;
            ReplaceAudiosList(_vk.Audio.Search(SearchTextQuery, out x, false, VkNet.Enums.AudioSort.Popularity, false, 30, 0));
        }

        #region Bindings

        private MyAudio _currentAudio;
        public MyAudio CurrentAudio
        {
            get
            {
                return _currentAudio;
            }
            set
            {
                _currentAudio = value;
                RaisePropertyChanged("CurrentAudio");
            }
        }

        private bool _isAppBusy = false;
        public bool IsAppBusy
        {
            get
            {
                return _isAppBusy;
            }
            set
            {
                _isAppBusy = value;
            }
        }

        private ObservableCollection<MyAudio> _audiosList = new ObservableCollection<MyAudio>();
        public ObservableCollection<MyAudio> AudiosList
        {
            get
            {
                return _audiosList;
            }
            set
            {
                _audiosList = value;
                RaisePropertyChanged("AudiosList");
            }
        }

        public string SearchTextQuery { get; set; }

        private int _selectedAudioIdx = 0;
        public int SelectedAudioIdx
        {
            get
            {
                return _selectedAudioIdx;
            }
            set
            {
                _selectedAudioIdx = value;
                if (_selectedAudioIdx >= 0 && _selectedAudioIdx - 1 < AudiosList.Count)
                {
                    CurrentAudio = AudiosList[_selectedAudioIdx];
                    //StringAudioIdx = CurrentAudio.Url.ToString();
                }
                StringAudioIdx = _selectedAudioIdx.ToString();
                RaisePropertyChanged("SelectedAudioIdx");
            }
        }


        public string StringAudioIdx { get; set; }

        public int SearchTypeIndex { get; set; }

        #endregion

        #region Commands

        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new DelegateCommand(SearchExecute);
                return _searchCommand;
            }
        }

        #endregion

    }
}
