﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Z_nthCommon;

namespace Simplex
{
    public enum Waveform { Sine, Saw, Square, Triangle };


    public class SiSy : Zplusnthbase
    {
        private double[] Phase = new double[maxPolyPhony];

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
                        double correctedphase = Z_nthCommon.Phase.Correction(Phase[channel]);
                        switch (CurrentOption)
                        {
                            case Waveform.Sine:
                                currentsamplevalue += Math.Sin(correctedphase);
                                break;
                            case Waveform.Saw:
                                currentsamplevalue += Z_nthCommon.Phase.Saw(correctedphase);
                                break;
                            case Waveform.Square:
                                currentsamplevalue += Z_nthCommon.Phase.Square(correctedphase);
                                break;
                            case Waveform.Triangle:
                                currentsamplevalue += Z_nthCommon.Phase.Triangle(correctedphase);
                                break;
                            default:
                                break;
                        }
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
