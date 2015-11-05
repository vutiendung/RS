using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class RSDataType
    {
        public string DataType { get; set; }
        public string Validation { get; set; }
        public string Guide { get; set; }
        public string DefaultValue { get; set; }
    }
}
