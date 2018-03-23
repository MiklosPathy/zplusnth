using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Xml.Serialization;
using Z_nthCommon;

namespace PunkOrgan
{
    public class PunkOrganPreset
    {
        public PunkOrganPreset()
        {
            Polphony = 3;
            Drawbars = new int[] { 6, 8, 8, 8, 7, 6, 5, 4, 3 };
            Overdrive = 1;
            LeslieFreq = 3;
            LeslieRate = 1;
            EchoFreq = 10000;
            EchoRate = 50;
        }
        public int Polphony { get; set; }
        public int[] Drawbars { get; set; }
        public int Overdrive { get; set; }
        public int LeslieFreq { get; set; }
        public int LeslieRate { get; set; }
        public int EchoFreq { get; set; }
        public int EchoRate { get; set; }
    }

    public class DrawBar : INotifyPropertyChanged
    {
        private int volume;
        public int Volume { get { return volume; } set { volume = value; NotifyPropertyChanged(); } }
        public double FreqMul;
        public double[] Phase;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PunkHammond : Zplusnthbase
    {
        private static readonly int echobuffersize = 20000;

        public DrawBar[] Drawbars { get; set; }

        private int leslie_rate;
        public int Leslie_Rate { get { return leslie_rate; } set { leslie_rate = value; NotifyPropertyChanged(); } }
        private int leslie_freq;
        public int Leslie_Freq { get { return leslie_freq; } set { leslie_freq = value; NotifyPropertyChanged(); } }

        double Leslie_Phase = 0;

        private int echo_rate;
        public int Echo_Rate { get { return echo_rate; } set { echo_rate = value; NotifyPropertyChanged(); } }
        private int echo_freq;
        public int Echo_Freq { get { return echo_freq; } set { echo_freq = value; NotifyPropertyChanged(); } }

        private double[] echobuffer;
        int echophase = 0;

        private int phaser_mix_rate;
        public int Phaser_Mix_Rate { get { return phaser_mix_rate; } set { phaser_mix_rate = value; NotifyPropertyChanged(); } }
        private int phaser_feedback_rate;
        public int Phaser_Feedback_Rate { get { return phaser_mix_rate; } set { phaser_mix_rate = value; NotifyPropertyChanged(); } }
        private int phaser_freq;
        public int Phaser_Freq { get { return phaser_freq; } set { phaser_freq = value; NotifyPropertyChanged(); } }

        private double[] phaserbuffer;
        int phaserphase = 0;
        double phaserlfophase = 0;



        private PunkOrganPreset[] presets;

        public PunkHammond()
        {
            Drawbars = new DrawBar[10];
            //Thanks to: http://www.jessedeanefreeman.com/hammondstuff.html

            Drawbars[1] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 0.5 };
            Drawbars[2] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = Math.Pow(halfnotemultiplier, 7) };
            Drawbars[3] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 1 };
            Drawbars[4] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 2 };
            Drawbars[5] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 2 * Math.Pow(halfnotemultiplier, 7) };
            Drawbars[6] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 4 };
            Drawbars[7] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 4 * Math.Pow(halfnotemultiplier, 4) };
            Drawbars[8] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 4 * Math.Pow(halfnotemultiplier, 7) };
            Drawbars[9] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 0, FreqMul = 8 };

            Leslie_Freq = 0;
            Leslie_Rate = 0;

            echobuffer = new double[echobuffersize];
            Echo_Freq = 0;
            Echo_Rate = 0;

            phaserbuffer = new double[echobuffersize];

            CurrentPolyphony = 1;

            OverDrive = 10;
        }




        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double bending = Bending;

                Leslie_Phase += 2 * Math.PI / WaveFormat.SampleRate * Leslie_Freq;
                if (Leslie_Phase > 2 * Math.PI) Leslie_Phase -= 2 * Math.PI;
                double leslie = Math.Sin(Leslie_Phase) * Leslie_Rate;

                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Channels[channel].State == ChannelState.KeyOn)
                    {
                        for (int drawbar = 1; drawbar < 10; drawbar++)
                        {
                            Drawbars[drawbar].Phase[channel] = 0;
                        }
                    }
                    if (Channels[channel].State == ChannelState.KeyOn || Channels[channel].State == ChannelState.ReKeyOn) Channels[channel].State = ChannelState.Active;
                    if (Channels[channel].State == ChannelState.Active || Channels[channel].State == ChannelState.KeyOff)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Channels[channel].Freq + leslie + bending);
                        for (int drawbar = 1; drawbar < 10; drawbar++)
                        {
                            if (Drawbars[drawbar].Phase[channel] >= 0)
                            {
                                Drawbars[drawbar].Phase[channel] += commonsinpart * Drawbars[drawbar].FreqMul;
                            }
                            if (Channels[channel].State == ChannelState.KeyOff)
                                if (Drawbars[drawbar].Phase[channel] >= 2 * Math.PI) Drawbars[drawbar].Phase[channel] = -100;
                            if (Drawbars[drawbar].Phase[channel] >= 0)
                            {
                                if (Drawbars[drawbar].Phase[channel] >= 2 * Math.PI) Drawbars[drawbar].Phase[channel] -= 2 * Math.PI;
                                currentsamplevalue += (Math.Sin(Drawbars[drawbar].Phase[channel]) * Drawbars[drawbar].Volume);
                            }
                        }
                    }
                    if (Channels[channel].State == ChannelState.KeyOff)
                    {
                        int drawbar;
                        for (drawbar = 1; drawbar < 10; drawbar++)
                        {
                            if (Drawbars[drawbar].Phase[channel] >= 0) break;
                        }
                        if (drawbar == 10) Channels[channel].State = ChannelState.Inactive;
                    }

                }
                //The 90 divider is for the drawbars. There are 9 of them. They have 10 position here.
                currentsamplevalue = currentsamplevalue / 90 / CurrentPolyphony;

                //Over volume + limit = kinda overdrive and distortion
                #region Volume/Overdrive/Limit
                currentsamplevalue = currentsamplevalue * overDrive;
                if (currentsamplevalue > 1) currentsamplevalue = 1;
                if (currentsamplevalue < -1) currentsamplevalue = -1;
                #endregion Volume/Overdrive/Limit

                #region Phaser
                phaserlfophase += 2 * Math.PI / WaveFormat.SampleRate * Phaser_Freq;
                if (phaserlfophase > 2 * Math.PI) phaserlfophase -= 2 * Math.PI;
                int phaser = (int)Math.Round((Math.Sin(phaserlfophase) * WaveFormat.SampleRate / 40) + WaveFormat.SampleRate / 20); //0-50 ms delay
                currentsamplevalue = currentsamplevalue + phaserbuffer[limitechophase(phaserphase - phaser)] * Phaser_Mix_Rate / 100;
                phaserbuffer[phaserphase] = currentsamplevalue * Phaser_Feedback_Rate;
                phaserphase++;
                phaserphase = limitechophase(phaserphase);
                #endregion Phaser

                #region Echo
                currentsamplevalue = currentsamplevalue + echobuffer[limitechophase(echophase + Echo_Freq)] * Echo_Rate / 100;
                echobuffer[echophase] = currentsamplevalue;
                echophase++;
                echophase = limitechophase(echophase);
                #endregion Echo

                #region Out limiter
                currentsamplevalue = currentsamplevalue * short.MaxValue;
                if (currentsamplevalue > short.MaxValue) currentsamplevalue = short.MaxValue;
                if (currentsamplevalue < short.MinValue) currentsamplevalue = short.MinValue;
                #endregion Out limiter

                buffer[sample + offset] = (short)currentsamplevalue;
            }
            return sampleCount;
        }

        //Buffer looping
        private int limitechophase(int phase)
        {
            if (phase < 0) return echobuffersize + phase;
            if (phase >= echobuffersize) return phase - echobuffersize;
            return phase;
        }

        private int overDrive;
        public int OverDrive
        {
            get { return overDrive; }
            set
            {
                overDrive = value;
                if (overDrive == 0)
                {
                    th = short.MaxValue;
                    th2 = short.MaxValue;
                }
                else
                {
                    th = short.MaxValue / 3;
                    th2 = th * 2;
                }
                NotifyPropertyChanged();
            }
        }

        private double th; // threshold for symmetrical soft clipping
        private double th2;
        const double maxvalue2 = short.MaxValue * 2;
        const double maxvalue3 = short.MaxValue * 3;

        private double overdrive(double input)            //by Schetzen Formula
        {
            double absinput = Math.Abs(input);
            if (absinput < th) return 2 * input;
            int signinput = Math.Sign(input);
            if (absinput < th2)
            {
                return (maxvalue3 - Math.Pow((maxvalue2 - absinput * maxvalue3), 2)) / maxvalue3 * signinput;
            }
            return signinput * short.MaxValue;
        }

        private double distortion(double x, double gain, double mix)
        {
            // Distortion based on an exponential function
            // x - input
            // gain - amount of distortion, >0->
            // mix - mix of original and distorted sound, 1=only distorted
            if (x == 0) return 0;
            x = x / short.MaxValue;

            double y = x / Math.Abs(x) * (1 - Math.Pow(Math.E, gain * x * x / Math.Abs(x)));
            return y * short.MaxValue;


            //double q = x * gain / Math.Abs(x);
            //double z = Math.Sign(-q) * (1 - Math.Exp(Math.Sign(-q) * q));
            //double y = mix * z * Math.Abs(x) / Math.Abs(z) + (1 - mix) * x;
            //y = y * Math.Abs(x) / Math.Abs(y);


        }

        const string presetfilename = "PunkOrganPresets.xml";

        private void SetPreset(PunkOrganPreset preset)
        {
            CurrentPolyphony = preset.Polphony;
            for (int i = 0; i < 9; i++) { Drawbars[i + 1].Volume = preset.Drawbars[i]; }

            OverDrive = preset.Overdrive;

            Leslie_Freq = preset.LeslieFreq;
            Leslie_Rate = preset.LeslieRate;

            Echo_Freq = preset.EchoFreq;
            Echo_Rate = preset.EchoRate;

            NotifyPropertyChanged();
        }

        protected override void SetPreset(int presetnum)
        {
            SetPreset(presets[presetnum]);
        }

        public void LoadPresets()
        {
            presets = new PunkOrganPreset[25];
            for (int i = 0; i < 25; i++) { presets[i] = new PunkOrganPreset(); }

            if (File.Exists(presetfilename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PunkOrganPreset[]));
                FileStream fs = new FileStream(presetfilename, FileMode.Open);
                presets = (PunkOrganPreset[])serializer.Deserialize(fs);
            }

            SetPreset(presets[0]);
        }

        protected override void GetPreset(int presetnum)
        {
            presets[presetnum] = GetPreset();
        }

        private PunkOrganPreset GetPreset()
        {
            PunkOrganPreset preset = new PunkOrganPreset();

            preset.Polphony = CurrentPolyphony;
            for (int i = 0; i < 9; i++) { preset.Drawbars[i] = Drawbars[i + 1].Volume; }

            preset.Overdrive = OverDrive;

            preset.LeslieFreq = Leslie_Freq;
            preset.LeslieRate = Leslie_Rate;

            preset.EchoFreq = Echo_Freq;
            preset.EchoRate = Echo_Rate;

            return preset;
        }

        protected override void SavePresets()
        {
            GetPreset(0);

            XmlSerializer serializer = new XmlSerializer(typeof(PunkOrganPreset[]));
            TextWriter writer = new StreamWriter(presetfilename);
            serializer.Serialize(writer, presets);
            writer.Close();
        }
    }
}
