using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsbo_lib;
using rsdao_lib;

namespace rssetting
{
    public class RecommendationSetting : ClusterSetting
    {
        public List<Recommendation_Setting> getListRecommendationSetting()
        {
            Recommendation_DAO dao = null;
            try
            {
                dao = new Recommendation_DAO();
                dao.beginTransaction();
                List<Recommendation_Setting> lstRecommendationSetting = dao.getListRecommendationSetting();
                foreach (Recommendation_Setting rs in lstRecommendationSetting)
                {
                    rs.Values = dao.getListRecommendationSettingValue(rs.Key);
                }
                dao.commitTransaction();
                return lstRecommendationSetting;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public void updateRecommendationSetting(Recommendation_Setting rs)
        {
            Recommendation_DAO dao = null;
            try
            {
                dao = new Recommendation_DAO();
                dao.beginTransaction();
                dao.updateRecommendationSetting(rs);
                List<Recommend_Setting_Value> lstAvailable = dao.getListRecommendationSettingValue(rs.Key);
                foreach (Recommend_Setting_Value avai in lstAvailable)
                {
                    bool isDeleted = true;
                    foreach (Recommend_Setting_Value val in rs.Values)
                    {
                        if (val.ValueID == avai.ValueID)
                        {
                            isDeleted = false;
                            break;
                        }
                    }
                    if (isDeleted)
                        dao.deleteRecommendationSettingDetail(avai.ValueID);
                }

                foreach (Recommend_Setting_Value val in rs.Values)
                {
                    if (val.ValueID != -1)
                    {
                        dao.updateRecommendationSettingDetail(val);
                    }
                    else
                    {
                        dao.insertRecommendationSettingDetail(val);
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

        public void insertRecommendationSetting(Recommendation_Setting rs)
        {
            Recommendation_DAO dao = null;
            try
            {
                dao = new Recommendation_DAO();
                dao.beginTransaction();
                dao.insertRecommendationSetting(rs);
                foreach (Recommend_Setting_Value val in rs.Values)
                {
                    dao.insertRecommendationSettingDetail(val);
                }
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public void deleteRecommendationSetting(Recommendation_Setting rs)
        {
            Recommendation_DAO dao = null;
            try
            {
                dao = new Recommendation_DAO();
                dao.beginTransaction();
                foreach (Recommend_Setting_Value val in rs.Values)
                {
                    dao.deleteRecommendationSettingDetail(val.ValueID);
                }
                dao.deleteRecommendationSetting(rs.Key);
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
