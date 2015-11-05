using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class RCItem
    {
        public string UserID { get; set; }
        public string ItemID { get; set; }
        public double Rate { get; set; }
        public double Quanlity { get; set; }
    }
}
