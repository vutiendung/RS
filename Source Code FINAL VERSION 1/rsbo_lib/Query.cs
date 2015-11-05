using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class Query
    {
        public string QueryID { get; set; }
        public string DescTable { get; set; }
        public int OrderNo { get; set; }
        public string String { get; set; }
        public string Description { get; set; }
        public string DataID { get; set; }
        public List<DIParameter> parameters = new List<DIParameter>();
        public List<DIMapping> mappings = new List<DIMapping>();

    }
}
