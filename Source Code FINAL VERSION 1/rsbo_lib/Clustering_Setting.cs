using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class Clustering_Setting
    {
        public string Key { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
        public List<Cluster_Setting_Value> Values {get;set;}
    }
}
