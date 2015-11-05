using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsglobal_lib
{
    public class ScheduleTime
    {
        public int monthOfYear { get; set; }
        public int dayOfMonth { get; set; }
        public int dayOfWeek { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
    }
}
