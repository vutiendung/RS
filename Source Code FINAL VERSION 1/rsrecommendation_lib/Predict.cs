using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsglobal_lib;
using rsdao_lib;
using rsbo_lib;
using rsclustering_lib;
using dataintergration_lib;

namespace rsrecommendation_lib
{
    public class Predict
    {
        #region Recommendation Schedule

        public void LoadRecommendation(string LoginID)
        {
            Predict_DAO_MC dao = null;

            RecommdationSchedule schedule = new RecommdationSchedule();
            schedule.Log = "Recommendation";
            schedule.LoginID = LoginID;
            Predict_MC predictMC = new Predict_MC();


            try
            {
                dao = new Predict_DAO_MC();
                dao.beginTransaction();
                RecommdationSchedule currentSchedule = dao.addRecommdationSchedule(schedule);

                // Settings st = getSettings();
                Settings r = predictMC.GetRecommendationSeting(dao);
                Dictionary<string, double> UCA = predictMC.getUCA(dao);

                //Data service 
                IntergrationManager manager = new IntergrationManager(); manager.execute();

                //Learn the quantity GAP
                List<ADDGAP> lstADDGAP = predictMC.GetListOfNewGAP(dao);       //get list of new GAP
                List<QTY_GAP> lstGAP = predictMC.UpdateQTYGap(dao, lstADDGAP);   //Update new GAP and return the GAP Matrix

                //Clustering service 
                new ClusterUsers().startClusteringAuto();

                /*
                 * 
                 * 
                 * this paragraph need to rewrite
                 * 
                 * */
                //C4                
                ComputeConfident(dao);
                // C5 
                ComputeDIST(dao);

                //=====================================================================================

                //dao.CLEAN_RECOMMENDATION();

                //// R1              
                DateTime startC6 = DateTime.Now;
                predictMC.R1(dao, currentSchedule, r.nbR1, UCA, lstGAP);
                DateTime endC6 = DateTime.Now;
                int time_Seconds_CLC6 = Convert.ToInt32((endC6 - startC6).TotalSeconds);

                ////R2               
                DateTime startC7 = DateTime.Now;
                setRCPurchasedItems(dao, currentSchedule, r.nbR2, lstGAP);
                DateTime endC7 = DateTime.Now;
                int time_Seconds_CLC7 = Convert.ToInt32((endC7 - startC7).TotalSeconds);

                //// R3               
                DateTime startC8 = DateTime.Now;
                predictMC.R3(dao, currentSchedule, r.nbR3, UCA, lstGAP);
                DateTime endC8 = DateTime.Now;
                int time_Seconds_CLC8 = Convert.ToInt32((endC8 - startC8).TotalSeconds);

                //// R4               
                DateTime startC9 = DateTime.Now;
                predictMC.R4(dao, currentSchedule, r.paramR4, UCA, lstGAP);
                DateTime endC9 = DateTime.Now;
                int time_Seconds_CLC9 = Convert.ToInt32((endC9 - startC9).TotalSeconds);

                ////Get R1R4 for new users                 
                DateTime startR1R4NewUsers = DateTime.Now;
                predictMC.R1R4_FOR_NEW_USERS(dao, currentSchedule, UCA);
                DateTime endR1R4NewUsers = DateTime.Now;
                int time_second_R1R4NewUsers = Convert.ToInt32((endR1R4NewUsers - startR1R4NewUsers).TotalSeconds);

                //GetPrice             
                dao.GetPrice();

                Console.WriteLine("FINISH");

                // build statistic log
                List<string[]> lstStatic = dao.getStaticsData();
                foreach (string[] statics in lstStatic)
                {
                    schedule.Log += statics[0] + statics[1] + ". ";
                }

                schedule.Log += "Time to find LRS01 : " + time_Seconds_CLC6.ToString() + ". ";
                schedule.Log += "Time to find LRS02 : " + time_Seconds_CLC7.ToString() + ". ";
                schedule.Log += "Time to find LRS03 : " + time_Seconds_CLC8.ToString() + ". ";
                schedule.Log += "Time to find LRS04 : " + time_Seconds_CLC9.ToString() + ". ";
                schedule.Log += "Time to find recommendations for new clients : " + time_second_R1R4NewUsers.ToString() + ". ";
                schedule.Log += "System successfully stopped at " + DateTime.Now.ToString();

                dao.updateRecommdationSchedule(schedule);
                dao.commitTransaction();

            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        #endregion

        #region C2 - Clustering User

        public void ClusteringUser()
        {
            new rsclustering_lib.ClusterUsers().startClusteringManual(ConstantValues.LOGIN_ID_USER_DEFAULT);
        }

        #endregion

        #region C3 - Update Cluster

        public void UpdateClusterUser()
        {
            new rsclustering_lib.ClusterUsers().updateCluster_AllTransac(ConstantValues.LOGIN_ID_USER_DEFAULT);
        }

        #endregion

        #region C4 - Build CONF & WEIG Matrix

        //---------------------------------------------------------------------------------------
        //This function computes the Confident Matrix and Weight Matrix, forllowing the formulars
        //Created by Vuong Minh Tuan
        //Corrected by MC. NGUYEN 21.10.2014.
        //---------------------------------------------------------------------------------------

        public void ComputeConfident(Predict_DAO_MC dao)
        {

            dao.CLEAN_CONF_Matrix();
            dao.CLEAN_WEIG_Matrix();

            List<string> list_ClusterID = dao.getAllClusterID();
            foreach (var item in list_ClusterID)
            {
                List<Transac> u_transacs = dao.getTransac_ForMatrix_ByClusterID_V2(item);
                if (u_transacs.Count > 0)
                {
                    // Get QuantityMatrix - MC OK
                    Dictionary<string, int> QM_dic_users = new Dictionary<string, int>();
                    Dictionary<string, int> QM_dic_items = new Dictionary<string, int>();
                    double[][] QuantityMatrix = Util.getQuantityMatrix_FromTransac_ByMetaItemID(u_transacs, out QM_dic_users, out QM_dic_items);

                    if (QuantityMatrix[0].Length > 1)
                    {
                        // Compute Support One Item Matrix - MC OK
                        double[] S = new double[QuantityMatrix[0].Length];
                        for (int j = 0; j < QuantityMatrix[0].Length; j++)
                        {
                            double count = 0.0;
                            for (int i = 0; i < QuantityMatrix.Length; i++)
                                if (QuantityMatrix[i][j] > 0)
                                    count++;
                            S[j] = count;
                        }

                        // Compute Support Matrix - MC OK
                        double[][] SupportMatrix = new double[QuantityMatrix[0].Length][];
                        for (int i = 0; i < QuantityMatrix[0].Length; i++)
                            SupportMatrix[i] = new double[QuantityMatrix[0].Length];

                        for (int i = 0; i < QuantityMatrix.Length; i++)
                        {
                            for (int j = 0; j < QuantityMatrix[0].Length; j++)
                                for (int j2 = 0; j2 < QuantityMatrix[0].Length; j2++)
                                    if (QuantityMatrix[i][j] > 0 & QuantityMatrix[i][j2] > 0 & j != j2)
                                        SupportMatrix[j][j2]++;
                        }

                        // Compute CONF Matrix - MC OK
                        double[][] CONF = new double[QuantityMatrix[0].Length][];
                        for (int i = 0; i < QuantityMatrix[0].Length; i++)
                            CONF[i] = new double[QuantityMatrix[0].Length];

                        for (int i = 0; i < QuantityMatrix[0].Length; i++)
                        {
                            for (int j = 0; j < QuantityMatrix[0].Length; j++)
                                if (i == j)
                                    CONF[i][j] = 1.0;
                                else if (S[i] > 0)
                                    CONF[i][j] = SupportMatrix[i][j] / S[i];
                        }

                        // Compute WEIG Matrix - MC OK
                        double[][] WEIG = new double[QuantityMatrix[0].Length][];
                        for (int i = 0; i < QuantityMatrix[0].Length; i++)
                            WEIG[i] = new double[QuantityMatrix[0].Length];

                        for (int i = 0; i < QuantityMatrix[0].Length; i++)
                        {
                            for (int j = 0; j < QuantityMatrix[0].Length; j++)
                                if (i != j)
                                    WEIG[i][j] = computeWEIG(i, j, SupportMatrix, QuantityMatrix);
                        }

                        // Save CONF Matrix - MC - OK
                        for (int i = 0; i < CONF.Length; i++)
                            for (int j = 0; j < CONF[0].Length; j++)
                                if (CONF[i][j] > 0)
                                {
                                    MatrixItem matrixItem = new MatrixItem();
                                    matrixItem.Row = Util.FindKeyByValue(QM_dic_items, i);
                                    matrixItem.Column = Util.FindKeyByValue(QM_dic_items, j);
                                    matrixItem.Cell = CONF[i][j];
                                    matrixItem.ClusterID = item;
                                    dao.setCONF_Matrix(matrixItem);
                                }

                        // Save WEIG Matrix
                        for (int i = 0; i < WEIG.Length; i++)
                            for (int j = 0; j < WEIG[0].Length; j++)
                                if (WEIG[i][j] > 0)
                                {
                                    MatrixItem matrixItem = new MatrixItem();
                                    matrixItem.Row = Util.FindKeyByValue(QM_dic_items, i);
                                    matrixItem.Column = Util.FindKeyByValue(QM_dic_items, j);
                                    matrixItem.Cell = WEIG[i][j];
                                    matrixItem.ClusterID = item;
                                    dao.setWEIG_Matrix(matrixItem);
                                }
                    }
                }
            }
        }

        //---------------------------------------------------------------------------------------
        //This function computes WEIG[i][j], with i, j are known
        //Created by Vuong Minh Tuan
        //Corrected all errors by MC. NGUYEN 21.10.2014
        //---------------------------------------------------------------------------------------
        private double computeWEIG(int i,
            int j,
            double[][] SupportMatrix,
            double[][] QuantityMatrix)
        {
            double rs = 0.0; int count = 0;
            if (SupportMatrix[i][j] == 0)
                rs = 0.0;
            else
            {
                for (int a = 0; a < QuantityMatrix.Length; a++)
                    if (QuantityMatrix[a][i] != 0 && QuantityMatrix[a][j] != 0)
                    {
                        rs += (QuantityMatrix[a][j] / QuantityMatrix[a][i]);
                        count++;
                    }
                if (count != 0) rs /= count;
            }
            return rs;
        }

        #endregion

        #region C5 - Build DIST Matrix


        //---------------------------------------------------------------------------------------
        //This function computes the DISTANCE matrix and inserts it into the database
        //This is the distance between the two columns of the Rating Matrix
        //Created by Vuong Minh Tuan
        //Corrected all errors by MC. NGUYEN 21.10.2014.
        //---------------------------------------------------------------------------------------

        public void ComputeDIST(Predict_DAO_MC dao)
        {

            dao.CLEAN_DIST_Matrix();

            List<string> list_ClusterID = dao.getAllClusterID();
            foreach (var item in list_ClusterID)
            {
                Dictionary<string, int> RM_dic_users = new Dictionary<string, int>();
                Dictionary<string, int> RM_dic_items = new Dictionary<string, int>();
                // Compute ratting matrix
                List<MatrixItem> lstRMI = dao.getRattingMatrixItem_ByClusterID(item);
                double[][] RattingMatrix = Util.toMatrix_MatrixItem(lstRMI, out RM_dic_users, out RM_dic_items);

                if (RattingMatrix.Length > 0 & RM_dic_items.Count > 1)
                {
                    double[][] DIST = new double[RattingMatrix[0].Length][];
                    for (int i = 0; i < RattingMatrix[0].Length; i++)
                        DIST[i] = new double[RattingMatrix[0].Length];

                    // Compute DIST Matrix
                    for (int i1 = 0; i1 < RattingMatrix[0].Length - 1; i1++)
                        for (int i2 = i1 + 1; i2 < RattingMatrix[0].Length; i2++)
                        {
                            double value = 0.0;
                            for (int t = 0; t < RattingMatrix.Length; t++)
                                value += Math.Pow(RattingMatrix[t][i1] - RattingMatrix[t][i2], 2);
                            value = Math.Sqrt(value);
                            DIST[i1][i2] = value;
                            DIST[i2][i1] = value;
                        }

                    if (DIST.Length > 0)
                    {
                        double max = DIST[0][0];
                        // Find Max
                        for (int i = 0; i < DIST.Length; i++)
                        {
                            double localMax = DIST[i].Max();
                            if (localMax > max)
                                max = localMax;
                        }

                        // Standardization
                        if (max != 0)
                        {
                            for (int i = 0; i < DIST.Length; i++)
                                for (int j = 0; j < DIST[0].Length; j++)
                                    DIST[i][j] /= max;
                        }

                        // Save to Database
                        for (int i = 0; i < DIST.Length; i++)
                            for (int j = 0; j < DIST[0].Length; j++)
                                if (DIST[i][j] > 0)
                                {
                                    MatrixItem matrixItem = new MatrixItem();
                                    matrixItem.Row = Util.FindKeyByValue(RM_dic_items, i);
                                    matrixItem.Column = Util.FindKeyByValue(RM_dic_items, j);
                                    matrixItem.Cell = DIST[i][j];
                                    matrixItem.ClusterID = item;
                                    dao.setDIST_Matrix(matrixItem);
                                }
                    }
                }
            }
        }

        #endregion

        #region C7 - Traditional user - SONNT - Implement new algorithm

        public void setRCPurchasedItems(Predict_DAO dao, RecommdationSchedule schedule, int nbR2, List<QTY_GAP> lst)
        {
            //Content is cleared by M.C. Nguyen 19.9.2015
        }

        public List<Recommendation_Meta_Item> Normaliser(List<Recommendation_Meta_Item> list)
        {
            double Max = 0;
            for (int i = 0; i < list.Count; i++)
                if (list[i].Score > Max) Max = list[i].Score;
            if (Max != 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Score /= Max;
                    if (list[i].Score < 0.001)
                        list[i].Score = 0.001;
                    else
                        list[i].Score = Math.Round(list[i].Score, 3);
                }
            }

            return list;
        }

        #endregion

        //--------------------------------------------------------------

        #region get schedules , get Results of Recommendation - Sonnt
        public List<RecommdationSchedule> getListSchedules()
        {
            Predict_DAO dao = null;
            List<RecommdationSchedule> listSchedules = null;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                listSchedules = dao.getSchedules();
                dao.commitTransaction();
            }
            catch (Exception)
            {
                dao.rollbackTransaction();
            }
            return listSchedules;
        }

        public List<string[]> getStaticData()
        {
            Predict_DAO dao = null;
            List<string[]> listResult;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                listResult = dao.getStaticsData();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return listResult;
        }
        public List<string> getRecommendTypes()
        {
            Predict_DAO dao = null;
            List<string> listSchedules = null;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                listSchedules = dao.getRecommendTypes();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return listSchedules;
        }

        public List<Recommendation_Item> searchRecommendations(string UserID, string UserName, string RecommendType, bool TraditionalUser)
        {
            Predict_DAO dao = null;
            List<Recommendation_Item> listSchedules = null;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                listSchedules = dao.searchRecommendations(UserID, UserName, RecommendType, TraditionalUser);
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return listSchedules;
        }
        public bool checkRecommendationTableIsInProcess()
        {
            bool result = true;
            Predict_DAO dao = null;
            // List<Recommendation_Item> listSchedules = null;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                result = dao.checkLockedTable("RECOMMENDATION_TBL");
                dao.commitTransaction();
            }
            catch (Exception)
            {
                dao.rollbackTransaction();
                return true;
            }
            return result;
        }


        #endregion

        public DateTime getCurrentTime()
        {
            Predict_DAO dao = null;
            try
            {
                dao = new Predict_DAO();
                dao.beginTransaction();
                DateTime date = dao.getCurrentTime();
                dao.commitTransaction();
                return date;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public string getTimer()
        {
            Settings st = getSettings();
            return st.REC_Timer;
        }

        private Settings getSettings()
        {
            Settings st = new Settings();
            List<Recommendation_Setting> listCs = getListRCSetting();

            if (null != listCs && listCs.Count > 0)
                foreach (var item in listCs)
                    if (item.Key.Equals(ConstantValues.REC_TIMER))
                        foreach (var cs_detail in item.Values)
                            if (cs_detail.isDedault.Equals(true))
                            {
                                st.REC_Timer = cs_detail.Value.Trim();
                                break;
                            }

            return st;
        }

        private List<Recommendation_Setting> getListRCSetting()
        {
            Recommendation_DAO dao = null;
            try
            {
                dao = new Recommendation_DAO();
                dao.beginTransaction();
                List<Recommendation_Setting> lstRCSetting = dao.getListRecommendationSetting();
                foreach (Recommendation_Setting cs in lstRCSetting)
                {
                    cs.Values = dao.getListRecommendationSettingValue(cs.Key);
                }
                dao.commitTransaction();
                return lstRCSetting;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }



    }
}