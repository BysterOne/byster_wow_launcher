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
        public string SessionId { get; set; }
        public RestService RestService { get; set; }

        private string username;
        private string password;
        private string referalCode;
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
        public string ReferalCode
        {
            get { return referalCode; }
            set
            {
                referalCode = value;
                OnPropertyChanged("ReferalCode");
            }
        }
        public void UpdateData()
        {
            (string _usernane, string _referalcode, int _bonuses) = RestService.GetUserInfo();
            if(string.IsNullOrEmpty(_usernane) &&
               string.IsNullOrEmpty(_referalcode))
            {
                return;
            }
            (Username, ReferalCode, BonusBalance) = (_usernane, _referalcode, _bonuses);
        }

        public UserInfoService(RestService service)
        {
            RestService = service;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
