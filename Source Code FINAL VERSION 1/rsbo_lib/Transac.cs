using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
     [Serializable]
    public class Transac
    {
         public string UserID { get; set; }
         public string ItemID { get; set; }
         public int Quantity { get; set; }
         public string MetaItemID { get; set; }

         public string ItemCategoryCode { get; set; }
         public int Times { get; set; }
         public double Rate { get; set; }
         public DateTime T_date { get; set; }
         public string U_CategoryID { get; set; }
    }
}
