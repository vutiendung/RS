using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class RATE
    {
        public string ClusterID { get; set; }
        public string UserID{ get; set; }
        public string ItemFamillyCode{ get; set; }
        public double SUM_RATE { get; set; }

    }

    public class U_I_Q
    {
        public string UserID { get; set; }
        public string MetaItemID{ get; set; }
        public double QTY{ get; set; }
    }

    public class DIST
    {
        public string ClusterID { get; set; }
        public string MetaItemSource { get; set; }
        public string MetaItemDestination { get; set; }
        public double Distance { get; set; }
    }

    public class CONF
    {
        public string ClusterID { get; set; }
        public string MetaItemSource { get; set; }
        public string MetaItemDestination { get; set; }
        public double Confident { get; set; }
    }

}
