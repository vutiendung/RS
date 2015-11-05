using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rsglobal_lib
{
    public class ConstantValues
    {
        // Schedule Cluster Type
        public static string SCHE_CLUSTER_USER = "USE";
        public static string SCHE_CLUSTER_ITEM = "ITE";

        // LOGIN_ID_DEFAULT
        public static string LOGIN_ID_AUTO = "0";
        public static string LOGIN_ID_USER_DEFAULT = "1";

        // TIMER 
        public static char[] DELIMITER = { '-', '|',':'};

        public static string TIMER_DAILY = "DAILY";
        public static string TIMER_WEEKLY = "WEEKLY";
        public static string TIMER_MONTHLY = "MONTHLY";
        public static string TIMER_YEARLY = "YEARLY";
        public static string TIMER_WEEK_MONDAY = "MONDAY";
        public static string TIMER_WEEK_TUESDAY = "TUESDAY";
        public static string TIMER_WEEK_WEDNESDAY = "WEDNESDAY";
        public static string TIMER_WEEK_THUSDAY = "THUSDAY";
        public static string TIMER_WEEK_FRIDAY = "FRIDAY";
        public static string TIMER_WEEK_SATURDAY = "SATURDAY";
        public static string TIMER_WEEK_SUNDAY = "SUNDAY";

        // Cluster Settings
        public static string CST_User = "U_";
        public static string CST_U_K = "U_k";
        public static string CST_U_MAX_LOOP = "U_maxLoop";
        public static string CST_U_EPSILON = "U_epsilon";
        public static string CST_U_ALPHA = "U_Alpha";
        public static string CST_U_T = "U_T";
        public static string CST_U_CLUSTER = "U_Cluster";
        public static string CST_U_TIMER = "U_Timer";
        public static string CST_U_INIT_V = "U_InitV";
        public static string CST_U_INIT_V_RANDOM = "random";
        public static string CST_U_LOOP_UPDATE = "U_loopUpdate";
        public static string CST_U_M = "U_M";

        // Recommendation Setting
        public static string REC_TIMER = "REC_Timer";
        public static string nb_R1 = "nbR1";
        public static string nb_R2 = "nbR2";
        public static string nb_R3 = "nbR3";
        public static string nb_R4 = "nbR4";
        public static string param_R4 = "paramR4";

        // System Settings
        public static string SST_TRANSAC_CP = "Transac_cp";
        public static string SST_TRANSAC_CP_TYPE = "datetime";
        public static string SST_TRANSAC_CP_DES = "Time to define new transac";

        // Recommendation type
        public static string RC_TYPE_LRS01 = "LRS01";
        public static string RC_TYPE_LRS02 = "LRS02";
        public static string RC_TYPE_LRS03 = "LRS03";
        public static string RC_TYPE_LRS04 = "LRS04";

        // User Type
        public static string USER_TRADITIONAL = "Traditional User";
        public static string USER_NEW = "New User";

        // Recommendation type

        
    }
}
