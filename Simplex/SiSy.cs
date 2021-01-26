using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Z_nthCommon;

namespace Simplex
{
    public class SiSy : Zplusnthbase
    {
        private double[] Phase = new double[maxPolyPhony];

        public Waveform CurrentOption { get; set; }
        private double _PWM = 1;
        public double PWM { get { return _PWM; } set { _PWM = value; if (PWM < PWM_Min) PWM = PWM_Min; if (PWM > PWM_Max) PWM = PWM_Max; NotifyPropertyChanged(); } }
        private double _PWM_Min = 1;
        public double PWM_Min { get { return _PWM_Min; } set { _PWM_Min = value; if (PWM_Min > PWM_Max) PWM_Min = PWM_Max; NotifyPropertyChanged(); if (PWM < PWM_Min) PWM = PWM_Min; } }
        private double _PWM_Max = 2;
        public double PWM_Max { get { return _PWM_Max; } set { _PWM_Max = value; if (PWM_Max < PWM_Min) PWM_Max = PWM_Min; NotifyPropertyChanged(); if (PWM > PWM_Max) PWM = PWM_Max; } }
        public double PWM_Freq { get; set; } = 0;
        private int PWM_Direction = 1;

        private Timer PWM_LFO = new Timer(1);

        public SiSy()
        {
            PWM_LFO.Elapsed += PWM_LFO_Elapsed;
            PWM_LFO.AutoReset = true;
            PWM_LFO.Start();
        }

        private void PWM_LFO_Elapsed(object sender, ElapsedEventArgs e)
        {
            double PWMTarget = PWM + PWM_Direction * PWM_Freq / 100;
            if (PWMTarget >= PWM_Max) { PWMTarget = PWM_Max; PWM_Direction = -1; }
            if (PWMTarget <= PWM_Min) { PWMTarget = PWM_Min; PWM_Direction = 1; }
            PWM = PWMTarget;
        }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Channels[channel].State == ChannelState.KeyOn) Phase[channel] = 0;
                    if (Channels[channel].State == ChannelState.KeyOn || Channels[channel].State == ChannelState.ReKeyOn) Channels[channel].State = ChannelState.Active;
                    if (Channels[channel].State == ChannelState.Active || Channels[channel].State == ChannelState.KeyOff)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + Bending);
                        Phase[channel] += commonsinpart;

                        Z_nthCommon.Phase.Waveformswitcher(CurrentOption, Phase[channel], ref currentsamplevalue, PWM);
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel] > 2 * Math.PI) Channels[channel].State = ChannelState.Inactive;
                    }
                    Phase[channel] = Z_nthCommon.Phase.Correction(Phase[channel]);
                }
                currentsamplevalue = currentsamplevalue * short.MaxValue / CurrentPolyphony;
                buffer[sample + offset] = (short)currentsamplevalue;
            }
            return sampleCount;
        }

        protected override void GetPreset(int presetnum)
        {
        }

        protected override void SavePresets()
        {
        }

        protected override void SetPreset(int presetnum)
        {
        }
    }
}
