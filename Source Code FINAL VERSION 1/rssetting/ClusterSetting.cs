using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsbo_lib;
using rsdao_lib;

namespace rssetting
{
    public class ClusterSetting
    {
        public List<Clustering_Setting> getListClusterSetting()
        {
            Clustering_DAO dao = null;
            try
            {
                dao = new Clustering_DAO();
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

        public void updateClusteringSetting(Clustering_Setting cs)
        {
            Clustering_DAO dao = null;
            try
            {
                dao = new Clustering_DAO();
                dao.beginTransaction();
                dao.updateClusteringSetting(cs);
                List<Cluster_Setting_Value> lstAvailable = dao.getListClusteringSettingValue(cs.Key);
                foreach (Cluster_Setting_Value avai in lstAvailable)
                {
                    bool isDeleted = true;
                    foreach (Cluster_Setting_Value val in cs.Values)
                    {
                        if (val.ValueID == avai.ValueID)
                        {
                            isDeleted = false;
                            break;
                        }
                    }
                    if (isDeleted)
                        dao.deleteClusteringSettingDetail(avai.ValueID);
                }

                foreach (Cluster_Setting_Value val in cs.Values)
                {
                    if (val.ValueID != -1)
                    {
                        dao.updateClusteringSettingDetail(val);
                    }
                    else
                    {
                        dao.insertClusteringSettingDetail(val);
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

        public void insertClusteringSetting(Clustering_Setting cs)
        {
            Clustering_DAO dao = null;
            try
            {
                dao = new Clustering_DAO();
                dao.beginTransaction();
                dao.insertClusteringSetting(cs);
                foreach (Cluster_Setting_Value val in cs.Values)
                {
                    dao.insertClusteringSettingDetail(val);
                }
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public void deleteClusteringSetting(Clustering_Setting cs)
        {
            Clustering_DAO dao = null;
            try
            {
                dao = new Clustering_DAO();
                dao.beginTransaction();
                foreach (Cluster_Setting_Value val in cs.Values)
                {
                    dao.deleteClusteringSettingDetail(val.ValueID);
                }
                dao.deleteClusteringSetting(cs.Key);
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public List<RSDataType> getListDataTypes()
        {
            Clustering_DAO dao = null;
            try
            {
                dao = new Clustering_DAO();
                dao.beginTransaction();
                List<RSDataType> lstDataTypes = dao.getListDataTypes();
                dao.commitTransaction();
                return lstDataTypes;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        //@TODO: add validate at server level
        private void validateData()
        {

        }

    }
}
