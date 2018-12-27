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
        public MoogFilter Filter { get; } = new MoogFilter();
        public Echo Echo { get; } = new Echo();

        private readonly double[] Phase = new double[maxPolyPhony];

        public double Spread { get; set; } = 0;
        public int Count { get; set; } = 7;
        public double Mix { get; set; } = 1;

        public Waveform CurrentOption { get; set; }

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
                        currentsamplevalue += SuperSaw(Phase[channel]);
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel] > 2 * Math.PI) Channels[channel].State = ChannelState.Inactive;
                    }
                    if (Phase[channel] > 2 * Math.PI) Phase[channel] -= 2 * Math.PI;
                }
                currentsamplevalue = currentsamplevalue / CurrentPolyphony;
                Filter.Process(ref currentsamplevalue);
                Echo.Process(ref currentsamplevalue);
                buffer[sample + offset] = OutLimiter(ref currentsamplevalue);
            }
            return sampleCount;
        }

        private double SuperSaw(double phase)
        {
            double result = 0;
            double result2 = 0;

            for (int i = 1; i <= Count; i++)
            {
                Z_nthCommon.Phase.Waveformswitcher(CurrentOption, phase + i * Spread, ref result);
                Z_nthCommon.Phase.Waveformswitcher(CurrentOption, phase - i * Spread, ref result);
            }
            result = result / (Count * 2);

            Z_nthCommon.Phase.Waveformswitcher(CurrentOption, phase * Spread, ref result2);

            result = result * Mix + result2 * (1 - Mix);

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
