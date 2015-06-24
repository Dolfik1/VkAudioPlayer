using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;

namespace VkMusicPlayer.ViewModel
{
    class ViewModelBase
    {
        public String DisplayName { get; set; }

        public VkApi _vk
        {
            get
            {
                return App.vk;
            }
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
