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

        private readonly double[,] Phase = new double[maxPolyPhony, 11];

        public double Spread { get; set; } = 0.5;
        public int Count { get; set; } = 3;
        public double Mix { get; set; } = 0.5;

        public Waveform CurrentOption { get; set; }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Channels[channel].State == ChannelState.KeyOn) Phase[channel, 0] = 0;
                    if (Channels[channel].State == ChannelState.KeyOn || Channels[channel].State == ChannelState.ReKeyOn) Channels[channel].State = ChannelState.Active;
                    if (Channels[channel].State == ChannelState.Active || Channels[channel].State == ChannelState.KeyOff)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + Bending);
                        Phase[channel, 0] += commonsinpart;
                        //Main voice
                        double mainvoice = 0;
                        Z_nthCommon.Phase.Waveformswitcher(CurrentOption, Phase[channel, 0], ref mainvoice);

                        //Detunes
                        //Detune improvements needed:
                        //Accurate cent calculation (log)
                        //2 first detune half , others 1, 2, 3... * +-spread distance
                        double detunes = 0;
                        for (int i = 1; i <= Count; i++)
                        {
                            Phase[channel, i * 2] += 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + Bending + i * Spread * 10);
                            Phase[channel, i * 2 - 1] += 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + Bending - i * Spread * 10);
                            Z_nthCommon.Phase.Waveformswitcher(CurrentOption, Phase[channel, i * 2], ref detunes);
                            Z_nthCommon.Phase.Waveformswitcher(CurrentOption, Phase[channel, i * 2 - 1], ref detunes);
                        }
                        detunes = detunes / (Count * 2);

                        currentsamplevalue += detunes * Mix + mainvoice * (1 - Mix);

                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel, 0] > 2 * Math.PI) Channels[channel].State = ChannelState.Inactive;
                    }
                    for (int i = 0; i < 11; i++)
                    {
                        if (Phase[channel, i] > 2 * Math.PI) Phase[channel, i] -= 2 * Math.PI;
                    }

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
