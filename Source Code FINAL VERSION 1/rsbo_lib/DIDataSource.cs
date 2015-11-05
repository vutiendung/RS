using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class DIDataSource
    {
        public string DataID { get; set; }
        public string ConnectionString { get; set; }
        public string Type { get; set; }
        public List<Query> queries =new List<Query>();
    }
}
