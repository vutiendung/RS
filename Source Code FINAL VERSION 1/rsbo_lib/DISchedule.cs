using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsbo_lib
{
    [Serializable]
    public class DISchedule
    {
        public int ScheduleID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public string Log { get; set; }
        public string LoginID { get; set; } 
    }
}
