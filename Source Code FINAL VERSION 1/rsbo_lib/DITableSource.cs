using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class DITableSource
    {
        public string TableID { get; set; }
        public string QueryID { get; set; }
        //public string DataID { get; set; }
        public string Type { get; set; }
    }
}
