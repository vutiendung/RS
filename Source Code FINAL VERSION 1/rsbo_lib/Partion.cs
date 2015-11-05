using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class Partion
    {
        public string ClusterID { get; set; }
        public string UserID { get; set; }
        public List<User_Centroid> UserCentroid { get; set; }
    }
}
