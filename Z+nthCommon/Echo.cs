using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Z_nthCommon
{
    public class Echo : INotifyPropertyChanged
    {
        private static readonly int echobuffersize = 20000;

        private int echo_rate;
        public int Rate { get { return echo_rate; } set { echo_rate = value; NotifyPropertyChanged(); } }
        private int echo_freq;
        public int Freq { get { return echo_freq; } set { echo_freq = value; NotifyPropertyChanged(); } }

        private double[] echobuffer;
        int echophase = 0;

        public Echo()
        {
            echobuffer = new double[echobuffersize];
            Freq = 0;
            Rate = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Buffer looping
        private int Limitechophase(int phase)
        {
            if (phase < 0) return echobuffersize + phase;
            if (phase >= echobuffersize) return phase - echobuffersize;
            return phase;
        }

        public void Process(ref double currentsample)
        {
            currentsample = currentsample + echobuffer[Limitechophase(echophase + Freq)] * Rate / 100;
            echobuffer[echophase] = currentsample;
            echophase++;
            echophase = Limitechophase(echophase);
        }
    }
}
