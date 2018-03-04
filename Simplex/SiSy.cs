using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Z_nthCommon;

namespace Simplex
{
    public class SiSy : Zplusnthbase
    {
        private double[] Phase = new double[maxPolyPhony];

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Notes[channel] > 0)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Notes[channel] + Bending);
                        Phase[channel] += commonsinpart;
                        currentsamplevalue += Math.Sin(Phase[channel]);
                    }
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
