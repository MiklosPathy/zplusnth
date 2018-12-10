using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_nthCommon;

namespace TranceSyzer
{
    public class TraSy : Zplusnthbase
    {
        private readonly double[] Phase = new double[maxPolyPhony];

        public double Spread { get; set; } = 0;
        public int Count { get; set; } = 7;


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
                        currentsamplevalue += SuperSaw(Phase[channel], Count, Spread);
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel] > 2 * Math.PI) Channels[channel].State = ChannelState.Inactive;
                    }
                    if (Phase[channel] > 2 * Math.PI) Phase[channel] -= 2 * Math.PI;
                }
                currentsamplevalue = currentsamplevalue * short.MaxValue / CurrentPolyphony;
                buffer[sample + offset] = (short)currentsamplevalue;
            }
            return sampleCount;
        }

        private double SuperSaw(double phase, int count, double spread)
        {
            double result = 0;
            int start = 0 - (count / 2);

            for (int i = start; i < count; i++)
            {
                result += Z_nthCommon.Phase.Saw(phase + i * spread);
            }
            result = result / count;

            return result;
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
