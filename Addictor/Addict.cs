using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Z_nthCommon;

namespace Addictor
{

    public class Addict : Zplusnthbase
    {
        const int MaxHarmonics = 20;
        public SetAndRealSliderViewModel[] Harmonics { get; set; }
        private double filter;
        public double Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                for (int i = 0; i < MaxHarmonics; i++)
                {
                    if (i >= filter) Harmonics[i].BarReal = 0; else Harmonics[i].BarReal = Harmonics[i].BarSet;
                }
            }
        }

        public Addict()
        {
            Harmonics = new SetAndRealSliderViewModel[MaxHarmonics];
            for (int i = 0; i < MaxHarmonics; i++)
            {
                Harmonics[i] = new SetAndRealSliderViewModel();
                Harmonics[i].FreqMul = i + 1;
                Harmonics[i].Phase = new double[maxPolyPhony];
                Harmonics[i].BarSet = 100;
            }

            Filter = 100;

        }

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double bending = Bending;
                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Channels[channel].State == ChannelState.KeyOn)
                    {
                        for (int harmonics = 0; harmonics < MaxHarmonics; harmonics++)
                        {
                            Harmonics[harmonics].Phase[channel] = 0;
                        }
                    }
                    if (Channels[channel].State == ChannelState.KeyOn || Channels[channel].State == ChannelState.ReKeyOn) Channels[channel].State = ChannelState.Active;
                    if (Channels[channel].State == ChannelState.Active || Channels[channel].State == ChannelState.KeyOff)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + bending);
                        for (int harmonics = 0; harmonics < MaxHarmonics; harmonics++)
                        {
                            if (Harmonics[harmonics].Phase[channel] >= 0)
                            {
                                Harmonics[harmonics].Phase[channel] += commonsinpart * Harmonics[harmonics].FreqMul;
                            }
                            if (Channels[channel].State == ChannelState.KeyOff)
                                if (Harmonics[harmonics].Phase[channel] >= 2 * Math.PI) Harmonics[harmonics].Phase[channel] = -100;
                            if (Harmonics[harmonics].Phase[channel] >= 0)
                            {
                                if (Harmonics[harmonics].Phase[channel] >= 2 * Math.PI) Harmonics[harmonics].Phase[channel] -= 2 * Math.PI;
                                currentsamplevalue += (Math.Sin(Harmonics[harmonics].Phase[channel]) * Harmonics[harmonics].BarReal);
                            }
                        }
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        int harmonics;
                        for (harmonics = 0; harmonics < MaxHarmonics; harmonics++)
                        {
                            if (Harmonics[harmonics].Phase[channel] >= 0) break;
                        }
                        if (harmonics == MaxHarmonics) Channels[channel].State = ChannelState.Inactive;
                    }
                }
                currentsamplevalue = currentsamplevalue / (100 * MaxHarmonics) * short.MaxValue / CurrentPolyphony;
                buffer[sample + offset] = (short)currentsamplevalue;
            }
            return sampleCount;
        }

        protected override void SavePresets()
        {
        }

        protected override void GetPreset(int presetnum)
        {
        }

        protected override void SetPreset(int presetnum)
        {
        }


    }
}