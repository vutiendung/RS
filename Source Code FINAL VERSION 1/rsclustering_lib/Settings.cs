using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsclustering_lib
{
    public class Settings
    {
        public int k { get; set; }
        public int maxLoop { get; set; }
        public double epsilon { get; set; }
        public double Alpha { get; set; }
        public double T { get; set; }
        public string cluster_type { get; set; }
        public string U_Timer { get; set; }
        //public string U_InitV { get; set; }
        public int U_M { get; set; }
        //public int loopUpdate { get; set; }
        public int loopInitV { get; set; }
    }
}
