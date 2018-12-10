using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_nthCommon;

namespace TheSID
{
    public class SID : Zplusnthbase
    {


        public double ModulatorMultiplier { get; set; }
        public bool Sync { get; set; }
        public bool Ring { get; set; }

        private double[] Phase = new double[maxPolyPhony];
        private double[] Phase2 = new double[maxPolyPhony];

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
                        double commonsinpart2 = 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq * ModulatorMultiplier + Bending);
                        Phase[channel] += commonsinpart;
                        Phase2[channel] += commonsinpart2;

                        currentsamplevalue += Math.Sin(Phase[channel]) * (Ring ? Math.Sin(Phase2[channel]) : 1);
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel] > 2 * Math.PI || Phase[channel] == 0) Channels[channel].State = ChannelState.Inactive;
                    }
                    if (Phase[channel] > 2 * Math.PI) { Phase[channel] -= 2 * Math.PI; }
                    if (Phase2[channel] > 2 * Math.PI)
                    {
                        Phase2[channel] -= 2 * Math.PI;
                        if (Sync) Phase[channel] = 0;
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        if (Phase[channel] == 0) Channels[channel].State = ChannelState.Inactive;
                    }

                }
                currentsamplevalue = currentsamplevalue * short.MaxValue / CurrentPolyphony;
                buffer[sample + offset] = (short)currentsamplevalue;
            }
            return sampleCount;
        }

        protected override void GetPreset(int presetnum)
        {
            throw new NotImplementedException();
        }

        protected override void SavePresets()
        {
        }

        protected override void SetPreset(int presetnum)
        {
            throw new NotImplementedException();
        }

        public void LoadPresets()
        {
            //presets = new PunkOrganPreset[25];
            //for (int i = 0; i < 25; i++) { presets[i] = new PunkOrganPreset(); }

            //if (File.Exists(presetfilename))
            //{
            //    XmlSerializer serializer = new XmlSerializer(typeof(PunkOrganPreset[]));
            //    FileStream fs = new FileStream(presetfilename, FileMode.Open);
            //    presets = (PunkOrganPreset[])serializer.Deserialize(fs);
            //}

            //SetPreset(presets[0]);
        }
    }
}
