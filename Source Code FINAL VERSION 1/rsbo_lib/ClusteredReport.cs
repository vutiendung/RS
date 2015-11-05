using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class ClusteredReport
    {
        public string U_SubCategoryID { get; set; }
        public List<ClusterInfo> lstCluster { get; set; }
    }
}
