using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Addictor
{
    public class SetAndRealSliderViewModel : INotifyPropertyChanged
    {
        private int real;
        private int target;
        public int BarSet { get { return target; } set { target = value; BarReal = value; NotifyPropertyChanged(); } }
        public int BarReal { get { return real; } set { real = value; NotifyPropertyChanged(); } }

        public double FreqMul;
        public double[] Phase;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
