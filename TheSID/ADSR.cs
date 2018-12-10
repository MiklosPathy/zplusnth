using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSID
{
    public class ADSR
    {
        public int A { get; set; }
        public int D { get; set; }
        public int S { get; set; }
        public int R { get; set; }
        public bool Gate { get; set; }

        public static readonly int[] A_to_mS = { 2, 8, 16, 24, 38, 56, 68, 80, 100, 250, 500, 800, 1000, 3000, 5000, 8000 };
        public static readonly int[] DR_to_mS = { 6, 24, 48, 72, 114, 168, 204, 240, 300, 750, 1500, 2400, 3000, 9000, 15000, 24000 };

        public double S_in_double {get{ return S / 15; } }
    }
}
