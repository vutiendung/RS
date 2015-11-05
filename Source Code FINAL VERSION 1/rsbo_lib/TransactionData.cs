using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class TransactionData
    {
        public string UserID { get; set; }
        public string MetaItemID { get; set; }
        public string UserName { get; set; }
        public string MetaItemName { get; set; }
        public int quantity { get; set; }
        public DateTime t_date { get; set; }
    }


    [Serializable]
    public class AggTransactionData
    {
        public string UserID { get; set; }
        public string MetaItemID { get; set; }
        public int quantity { get; set; }
        public DateTime min_date { get; set; }
        public DateTime max_date { get; set; }
    }

}
