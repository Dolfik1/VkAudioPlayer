using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkMusicPlayer.Helpers
{
    public class StringHelper
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string SecondsToTimeString(int secs)
        {
            string s = "";
            int minutes, seconds;

            if ((secs / 60) >= 60)
            {
                int hours = Math.Abs((secs / 60) / 60); //Часов
                minutes = Math.Abs((secs - ((hours * 60) * 60)) / 60);
                seconds = secs - (((hours * 60) * 60) + (minutes * 60));
                s = hours + (minutes >= 10 ? ":" : ":0") + minutes + (seconds >= 10 ? ":" : ":0") + seconds;
            }
            else
            {

                minutes = Math.Abs(secs / 60);
                seconds = secs - (minutes * 60);
                s = minutes + (seconds >= 10 ? ":" : ":0") + seconds;
            }

            return s;
        }
    }
}
