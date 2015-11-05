using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class ADDGAP
    {
        public string UserID { get; set; }
        public string MetaItemID { get; set; }
        public int QTY_RECOM { get; set; }
        public int QTY_REAL { get; set; }
        public double GAP_ADDITION { get; set; }
    }
}
