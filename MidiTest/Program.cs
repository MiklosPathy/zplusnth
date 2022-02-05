using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MidiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var strictMode = false;
            var mifi = new MidiFile(@"lamourtoujours.mid", strictMode);
            MidiEventsByTick = mifi.Events.SelectMany(x => x).OrderBy(x => x.AbsoluteTime).
                 GroupBy(x => x.AbsoluteTime, x => x).ToDictionary(x => x.Key, x => x.ToArray());

            var otherevents = mifi.Events.SelectMany(x => x).OrderBy(x => x.AbsoluteTime).Where(x => !(x is NoteOnEvent || x is TempoEvent)).ToList();
                 


            var MaxMidiTicks = MidiEventsByTick.Keys.Max();

            var timeSignature = mifi.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();

            int beatsPerBar = timeSignature == null ? 4 : timeSignature.Numerator;
            int ticksPerBar = timeSignature == null ? mifi.DeltaTicksPerQuarterNote * 4 : (timeSignature.Numerator * mifi.DeltaTicksPerQuarterNote * 4) / (1 << timeSignature.Denominator);
            int ticksPerBeat = ticksPerBar / beatsPerBar;

            //for (int n = 0; n < mf.Tracks; n++)
            //{
            //    foreach (var midiEvent in mf.Events[n])
            //    {
            //        if (!MidiEvent.IsNoteOff(midiEvent))
            //        {
            //            Console.WriteLine("{0} {1}", ToMBT(midiEvent.AbsoluteTime, mf.DeltaTicksPerQuarterNote, timeSignature), midiEvent);
            //        }
            //    }
            //}

            int sleeptime = 1;

            //Console.ReadKey();
            while (MidiTicks <= MaxMidiTicks)
            {
                Thread.Sleep(sleeptime);
                MidiEvent[] me = null;
                MidiEventsByTick.TryGetValue(MidiTicks, out me);
                MidiTicks++;
                if (me == null) continue;

                foreach (MidiEvent item in me)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    if (item is TempoEvent)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        TempoEvent te = (TempoEvent)item;

                        sleeptime = te.MicrosecondsPerQuarterNote / ticksPerBeat / 1000;
                    }
                    if (item is NoteOnEvent)
                    {
                        NoteOnEvent ne = (NoteOnEvent)item;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        if (ne.OffEvent == null)
                            Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine(item);
                }
            }
        }

        private static Dictionary<long, MidiEvent[]> MidiEventsByTick;
        private static int MidiTicks;

        private static string ToMBT(long eventTime, int ticksPerQuarterNote, TimeSignatureEvent timeSignature)
        {
            int beatsPerBar = timeSignature == null ? 4 : timeSignature.Numerator;
            int ticksPerBar = timeSignature == null ? ticksPerQuarterNote * 4 : (timeSignature.Numerator * ticksPerQuarterNote * 4) / (1 << timeSignature.Denominator);
            int ticksPerBeat = ticksPerBar / beatsPerBar;
            long bar = 1 + (eventTime / ticksPerBar);
            long beat = 1 + ((eventTime % ticksPerBar) / ticksPerBeat);
            long tick = eventTime % ticksPerBeat;
            return String.Format("{0}:{1}:{2}", bar, beat, tick);
        }
    }
}
