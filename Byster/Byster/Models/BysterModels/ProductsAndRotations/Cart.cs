using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Byster.Models.BysterModels
{
    public class Cart : INotifyPropertyChanged
    {
        public List<(int, int)> Products { get; set; }
        private int bonuses;
        public int Bonuses
        {
            get { return bonuses; }
            set
            {
                bonuses = value;
                OnPropertyChanged("Bonuses");
            }
        }

        private double sum;
        public double Sum
        {
            get { return sum; }
            set
            {
                sum = value;
                OnPropertyChanged("Sum");
            }
        }

        private int paymentSystemId;
        public int PaymentSystemId
        {
            get { return paymentSystemId; }
            set
            {
                paymentSystemId = value;
                OnPropertyChanged("PaymentSystemId");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
