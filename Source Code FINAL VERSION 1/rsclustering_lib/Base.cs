using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsdao_lib;
using rsbo_lib;
using rsglobal_lib;
using algorithm_lib;

namespace rsclustering_lib
{
    public class Base
    {
        public int checkProcess()
        {
            int processType = 0;
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                ClusterSchedule cs = dao.getLastClusterSchedule();
                if (null != cs)
                {
                    if (null == cs.StopTime)
                    {
                        processType = 1;
                    }
                }
                dao.commitTransaction();
                return processType;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public Settings getSettings()
        {
            Settings st = new Settings();
            try
            {
                List<Clustering_Setting> listCs = getListClusterSetting();

                if (null != listCs && listCs.Count > 0)
                    foreach (var item in listCs)
                    {
                        if (item.Key.Equals(ConstantValues.CST_U_K))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.k = Convert.ToInt32(cs_detail.Value.Trim());
                                    break;
                                }
                        }
                        //else if (item.Key.Equals(ConstantValues.CST_U_INIT_V))
                        //{
                        //    foreach (var cs_detail in item.Values)
                        //        if (cs_detail.isDedault.Equals(true))
                        //        {
                        //            st.U_InitV = cs_detail.Value.Trim();
                        //            break;
                        //        }
                        //}
                        else if (item.Key.Equals(ConstantValues.CST_U_MAX_LOOP))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.maxLoop = Convert.ToInt32(cs_detail.Value.Trim());
                                    break;
                                }
                        }
                        else if (item.Key.Equals(ConstantValues.CST_U_TIMER))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.U_Timer = cs_detail.Value.Trim();
                                    break;
                                }
                        }
                        else if (item.Key.Equals(ConstantValues.CST_U_CLUSTER))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.cluster_type = cs_detail.Value.Trim();
                                    break;
                                }
                        }
                        //else if (item.Key.Equals(ConstantValues.CST_U_LOOP_UPDATE))
                        //{
                        //    foreach (var cs_detail in item.Values)
                        //        if (cs_detail.isDedault.Equals(true))
                        //        {
                        //            st.loopUpdate = Convert.ToInt32(cs_detail.Value.Trim());
                        //            break;
                        //        }
                        //}
                        else if (item.Key.Equals(ConstantValues.CST_U_T))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    try
                                    {
                                        st.T = Double.Parse(cs_detail.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        st.T = Double.Parse(cs_detail.Value.Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    break;
                                }
                        }
                        else if (item.Key.Equals(ConstantValues.CST_U_EPSILON))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    try
                                    {
                                        st.epsilon = Double.Parse(cs_detail.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        st.epsilon = Double.Parse(cs_detail.Value.Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    break;
                                }
                        }
                        else if (item.Key.Equals(ConstantValues.CST_U_ALPHA))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    try
                                    {
                                        st.Alpha = Double.Parse(cs_detail.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        st.Alpha = Double.Parse(cs_detail.Value.Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    break;
                                }
                        }
                        else if (item.Key.Equals(ConstantValues.CST_U_M))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.U_M = Convert.ToInt32(cs_detail.Value.Trim());
                                    break;
                                }
                        }
                    }

            }
            catch (Exception)
            {
                Cluster.setDefaultSettings(st.k, st.maxLoop, st.epsilon, st.Alpha, st.T, st.cluster_type);
            }
            return st;
        }

        public List<Clustering_Setting> getListClusterSetting()
        {
            Clustering_Base_DAO dao = null;
            try
            {
                dao = new Clustering_Base_DAO();
                dao.beginTransaction();
                List<Clustering_Setting> lstClusteringSetting = dao.getListClusteringSetting();
                foreach (Clustering_Setting cs in lstClusteringSetting)
                {
                    cs.Values = dao.getListClusteringSettingValue(cs.Key);
                }
                dao.commitTransaction();
                return lstClusteringSetting;
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
            return st.U_Timer;
        }

        public DateTime getCurrentTime()
        {
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
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

        public List<System_Setting> getListSystemSetting()
        {
            Clustering_Base_DAO dao = null;
            try
            {
                dao = new Clustering_Base_DAO();
                dao.beginTransaction();
                List<System_Setting> lst = dao.getListSystemSetting();
                foreach (System_Setting cs in lst)
                {
                    cs.Values = dao.getListSystemSettingValue(cs.Key);
                }
                dao.commitTransaction();
                return lst;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public static void updateTransac_CheckPoint()
        {
            Clustering_Users_DAO dao = null;
            System_Setting transac_check_point = getTransac_CheckPoint();
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                if (ConstantValues.SST_TRANSAC_CP.Equals(transac_check_point.Key))
                {
                    if (transac_check_point.Values.Count > 0)
                    {
                        dao.updateTransac_CheckPoint(ConstantValues.SST_TRANSAC_CP);
                    }
                    else
                    {
                        dao.addTransac_CheckPoint(ConstantValues.SST_TRANSAC_CP);
                    }
                }
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public static System_Setting getTransac_CheckPoint()
        {
            Clustering_Users_DAO dao = null;
            System_Setting transac_check_point = new System_Setting();
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                List<System_Setting> listSystemSetting = dao.getListSystemSetting();
                foreach (var item in listSystemSetting)
                {
                    if (item.Key.Equals(ConstantValues.SST_TRANSAC_CP))
                    {
                        item.Values = dao.getListSystemSettingValue(ConstantValues.SST_TRANSAC_CP);
                        return item;
                    }
                }

                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return transac_check_point;
        }

        public static void addNewTransacCheckpointKey()
        {
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                // Add Key ConstantVariables.SST_TRANSAC_CHECK_POINT
                System_Setting transac_checkpoint = new System_Setting();
                transac_checkpoint.Key = ConstantValues.SST_TRANSAC_CP;
                transac_checkpoint.DataType = ConstantValues.SST_TRANSAC_CP_TYPE;
                transac_checkpoint.Description = ConstantValues.SST_TRANSAC_CP_DES;
                dao.addSystemSetting(transac_checkpoint);

                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

    }
}
