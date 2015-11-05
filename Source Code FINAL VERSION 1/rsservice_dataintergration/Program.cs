using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using dataintergration_lib;
using rsglobal_lib;

namespace rsservice_dataintergration
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int time_Seconds = 0;
            try
            {
               // MessageBox.Show("Start.");
                DateTime start = DateTime.Now;
                IntergrationManager manager = new IntergrationManager();
                manager.execute();
                DateTime end = DateTime.Now;
                time_Seconds = Convert.ToInt32((end - start).TotalSeconds);
            }
            catch (Exception ex)
            {
                Util.writeLog(ex.Message);
            }
            finally
            {
                //MessageBox.Show("Finish - Time:" + time_Seconds);
                Application.Exit();
            }
        }
    }
}
