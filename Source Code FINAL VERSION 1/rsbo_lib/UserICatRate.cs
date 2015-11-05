using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class UserICatRate
    {
        public string UserID { get; set; }
        public string ItemCategoryID { get; set; }
        public double MeansRate { get; set; }
    }
}
