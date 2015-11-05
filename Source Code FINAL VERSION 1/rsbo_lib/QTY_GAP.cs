using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class QTY_GAP
    {
        public string UserID { get; set; }
        public string MetaItemID { get; set; }
        public double QTYGAP { get; set; }
        public int Time { get; set; }
    }
}
