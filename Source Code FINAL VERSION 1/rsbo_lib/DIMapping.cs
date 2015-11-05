using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class DIMapping
    {
        public int MappingID { get; set; }
        public string DesColumnID { get; set; }
        public string SrcColumnName { get; set; }
        public string QueryID { get; set; }
    }
}
