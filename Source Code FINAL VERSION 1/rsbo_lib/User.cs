using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace rsbo_lib
{
    [Serializable]
    public class User
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string U_SubCategoryID { get; set; }
        //public bool Blocked { get; set; }
        public DateTime CreateDate { get; set; }
    } 
}
