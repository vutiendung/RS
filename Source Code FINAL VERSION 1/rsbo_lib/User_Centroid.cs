using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class User_Centroid
    {
        public string ClusterID { get; set; }
        public string MetaItemID { get; set; }
        public double Value { get; set; }
    }
}
