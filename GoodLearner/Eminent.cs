using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_nthCommon;

namespace GoodLearner
{
    class Eminent : Zplusnthbase
    {
        private int phaser_mix_rate;
        public int Phaser_Mix_Rate { get { return phaser_mix_rate; } set { phaser_mix_rate = value; NotifyPropertyChanged(); } }
        private int phaser_feedback_rate;
        public int Phaser_Feedback_Rate { get { return phaser_feedback_rate; } set { phaser_feedback_rate = value; NotifyPropertyChanged(); } }
        private int phaser_freq;
        public int Phaser_Freq { get { return phaser_freq; } set { phaser_freq = value; NotifyPropertyChanged(); } }
        private int phaser_delay;
        public int Phaser_Delay { get { return phaser_delay; } set { phaser_delay = value; NotifyPropertyChanged(); } }

        private double[] phaserbuffer;
        int phaserphase = 0;
        double phaserlfophase = 0;

        public Eminent()
        {
            phaserbuffer = new double[echobuffersize];
            Phaser_Delay = 50;
        }


        protected override void SavePresets()
        {
            throw new NotImplementedException();
        }

        protected override void GetPreset(int presetnum)
        {
            throw new NotImplementedException();
        }

        protected override void SetPreset(int presetnum)
        {
            throw new NotImplementedException();
        }

        private static readonly int echobuffersize = 20000;

        //Buffer looping
        private int limitechophase(int phase)
        {
            if (phase < 0) return echobuffersize + phase;
            if (phase >= echobuffersize) return phase - echobuffersize;
            return phase;
        }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            double currentsamplevalue = 0;

            #region Phaser
            phaserlfophase += 2 * Math.PI / WaveFormat.SampleRate * Phaser_Freq / 100;
            if (phaserlfophase > 2 * Math.PI) phaserlfophase -= 2 * Math.PI;
            int phaser = (int)Math.Round((Math.Sin(phaserlfophase) * WaveFormat.SampleRate / Phaser_Delay) + WaveFormat.SampleRate / Phaser_Delay / 2); //0-50 ms delay
                                                                                                                                                        //currentsamplevalue = currentsamplevalue + phaserbuffer[limitechophase(phaserphase - phaser)] * ((double)Phaser_Mix_Rate / 100);
                                                                                                                                                        //phaserbuffer[phaserphase] = phaserbuffer[phaserphase] + currentsamplevalue * (double)Phaser_Feedback_Rate / 100 / 2;
            phaserbuffer[phaserphase] = currentsamplevalue;
            currentsamplevalue = currentsamplevalue + phaserbuffer[limitechophase(phaserphase - phaser)] * ((double)Phaser_Mix_Rate / 100);
            phaserphase++;
            phaserphase = limitechophase(phaserphase);
            #endregion Phaser

            throw new NotImplementedException();
        }
    }
}
