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
        /// Main entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int time_Seconds = 0;
            try
            {
                MessageBox.Show("Start.");
                DateTime start = DateTime.Now;

                //Start Recommend system
                new Predict().LoadRecommendation(ConstantValues.LOGIN_ID_AUTO);

                DateTime end = DateTime.Now;
                time_Seconds = Convert.ToInt32((end - start).TotalSeconds);
                //finish recommend
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
