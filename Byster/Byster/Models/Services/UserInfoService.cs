using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Byster.Models.Services
{
    public class UserInfoService : INotifyPropertyChanged, IService
    {
        public RestService RestService { get; set; }

        private string username;
        private string password;
        private int bonusBalance;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged("Username");
            }
        }
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public int BonusBalance
        {
            get { return bonusBalance;}
            set
            {
                bonusBalance = value;
                OnPropertyChanged("BonusBalance");
            }
        }

        public void UpdateData()
        {

        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
