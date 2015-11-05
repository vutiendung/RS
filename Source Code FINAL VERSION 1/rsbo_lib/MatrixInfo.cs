using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class MatrixInfo
    {
        public Dictionary<int, int> Map_users { get; set; }
        public Dictionary<int, int> Map_items { get; set; }
        public double[][] Matrix { get; set; }
    }
}
