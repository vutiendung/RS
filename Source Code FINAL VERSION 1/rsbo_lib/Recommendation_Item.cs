using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    public class Recommendation_Item
    {
        public int RecomID { get; set; }
        public int ScheduleID { get; set; }
        public string UserID { get; set; }
        public double Quantity { get; set; }
        public double Score { get; set; }
        public double Price { get; set; }
        public string RecommendType { get; set; }
        #region GUI - SONNT
        public string MetaItemID { get; set; }
        //public string UserName { get; set; }
        //public string MetaItemName { get; set; }
        #endregion
        public string ItemFamillyCode { get; set; }
    }
}
