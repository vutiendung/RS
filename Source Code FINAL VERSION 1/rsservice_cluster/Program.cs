using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using rsglobal_lib;
using rsclustering_lib;

namespace rsservice_cluster
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
                DateTime start = DateTime.Now;
                new ClusterUsers().startClusteringAuto();
                DateTime end = DateTime.Now;
                time_Seconds = Convert.ToInt32((end - start).TotalSeconds);
                MessageBox.Show("Finish FIRST - Time:" + time_Seconds);

            }
            catch (Exception ex)
            {
                Util.writeLog(ex.Message);
            }
            finally
            {
                MessageBox.Show("Finish - Time:" + time_Seconds);
                Application.Exit();
            }
        }
    }
}
