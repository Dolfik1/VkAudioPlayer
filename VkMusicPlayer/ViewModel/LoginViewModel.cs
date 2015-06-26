using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VkMusicPlayer.Commands;
using VkMusicPlayer.View;
using VkNet.Enums.Filters;
using VkNet.Utils;

namespace VkMusicPlayer.ViewModel
{
    class LoginViewModel : ViewModelBase
    {
        public LoginViewModel()
        {
            RememberPassword = Properties.Settings.Default.RememberPassword;
            if (RememberPassword == true)
            {
                Login = Properties.Settings.Default.Login;
                Password = Properties.Settings.Default.Password;
                LoginExecute(App.Current.MainWindow);
            }
        }

        private void LoginExecute(Window window)
        {
            if (!(String.IsNullOrEmpty(Login) || String.IsNullOrEmpty(Password)))
            {
                if (RememberPassword)
                {
                    Properties.Settings.Default.Login = Login;
                    Properties.Settings.Default.Password = Password;
                    Properties.Settings.Default.RememberPassword = RememberPassword;
                    Properties.Settings.Default.Save();
                }

                Task.Run(() =>
                {

                    IsLogging = true;
                    _vk.Authorize(3541904, Login, Password, Settings.Audio);

                }).ContinueWith((x) =>
                {
                    IsLogging = false;

                    if (x.Exception == null)
                    {
                        window.Dispatcher.Invoke(() =>
                        {
                            MainWindow win = new MainWindow();
                            win.Show();
                            window.Close();
                        });
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(x.Exception.Message, "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                });
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Введите логин и пароль", "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        #region Bindings

        public string Login { get; set; }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        private bool _isLogging = false;
        public bool IsLogging
        {
            get
            {
                return _isLogging;
            }
            set
            {
                _isLogging = value;
                BusyVisibility = _isLogging == true ? Visibility.Visible : Visibility.Hidden;
                RaisePropertyChanged("IsLogging");
            }
        }

        private Visibility _busyVisibility;
        public Visibility BusyVisibility
        {
            get
            {
                return _busyVisibility;
            }
            set
            {
                _busyVisibility = value;
                RaisePropertyChanged("BusyVisibility");
            }
        }

        public bool RememberPassword { get; set; }

        #endregion

        #region Commands

        private DelegateCommand<Window> _loginCommand;
        public DelegateCommand<Window> LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new DelegateCommand<Window>(LoginExecute);
                }
                return _loginCommand;
            }
        }

        #endregion
    }
}
