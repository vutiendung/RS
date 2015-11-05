using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class System_Setting_Value
    {
        public int ValueID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool isDedault { get; set; }
    }
}
