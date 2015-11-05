using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class Item
    {
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string MetaItemID { get; set; }
        public double Rate { get; set; }
        public double LastDate_FirstDate { get; set; }
    }
}
