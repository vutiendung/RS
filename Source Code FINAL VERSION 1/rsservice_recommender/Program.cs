using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using rsrecommendation_lib;
using rsglobal_lib;
using dataintergration_lib;

namespace rsservice_recommender
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
                // 12 Month - 1399s
                // MessageBox.Show("Start.");
                DateTime start = DateTime.Now;
                new Predict().LoadRecommendation(ConstantValues.LOGIN_ID_AUTO);
                DateTime end = DateTime.Now;
                time_Seconds = Convert.ToInt32((end - start).TotalSeconds);
            }
            catch (Exception ex)
            {
                Util.writeLog(ex.Message);
            }
            finally
            {
                // MessageBox.Show("Finish - Time:" + time_Seconds);
                Application.Exit();
            }
        }
    }
}
