using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class ClusterInfo
    {
        public string ClusterID { get; set; }
        public int Count { get; set; }
    }
}
