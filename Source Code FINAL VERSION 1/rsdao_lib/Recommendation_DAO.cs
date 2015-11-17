using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using rsbo_lib;

namespace rsdao_lib
{
    public class Recommendation_DAO : BASE_DAO
    {
        public Recommendation_DAO(): base() {}

        public List<Recommendation_Setting> getListRecommendationSetting()
        {
            List<Recommendation_Setting> lstRecommendationSetting = new List<Recommendation_Setting>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            SqlDataReader reader = executeReader("SELECT RS.RECOMMEND_SETTING_TBL.[Key],RS.RECOMMEND_SETTING_TBL.[DataType],RS.RECOMMEND_SETTING_TBL.[Description] FROM RS.RECOMMEND_SETTING_TBL,RS.DATATYPE_TBL WHERE RS.RECOMMEND_SETTING_TBL.DataType = RS.DATATYPE_TBL.DataType  ", parameters);
            while (reader.Read())
            {
                Recommendation_Setting cs = new Recommendation_Setting();
                cs.Key = reader.GetString(reader.GetOrdinal("Key"));
                cs.DataType = reader.GetString(reader.GetOrdinal("DataType"));
                cs.Description = reader.GetString(reader.GetOrdinal("Description"));
                lstRecommendationSetting.Add(cs);
            }
            reader.Close();
            return lstRecommendationSetting;
        }
        
        public List<Recommend_Setting_Value> getListRecommendationSettingValue(string Key)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", Key);
            SqlDataReader reader = executeReader("SELECT ValueID,Value,isDefault FROM RS.RECOMMEND_SETTING_VALUE_TBL WHERE [Key] = @Key ORDER BY isDefault DESC", parameters);
            List<Recommend_Setting_Value> lstRecommendSettingValue = new List<Recommend_Setting_Value>();
            while (reader.Read())
            {
                Recommend_Setting_Value rsv = new Recommend_Setting_Value();
                rsv.ValueID = reader.GetInt32(reader.GetOrdinal("ValueID"));
                rsv.Value = reader.GetString(reader.GetOrdinal("Value"));
                rsv.isDedault = reader.GetBoolean(reader.GetOrdinal("isDefault"));
                rsv.Key = Key;
                lstRecommendSettingValue.Add(rsv);
            }
            reader.Close();
            return lstRecommendSettingValue;
        }

        public void insertRecommendationSetting(Recommendation_Setting rs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", rs.Key);
            parameters.Add("@DataType", rs.DataType);
            parameters.Add("@Description", rs.Description);
            executeNonQuery("INSERT INTO [RS].[RECOMMEND_SETTING_TBL]([Key],[DataType],[Description]) VALUES(@Key ,@DataType,@Description)", parameters);
        }

        public void updateRecommendationSetting(Recommendation_Setting rs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", rs.Key);
            parameters.Add("@DataType", rs.DataType);
            parameters.Add("@Description", rs.Description);
            executeNonQuery("UPDATE [RS].[RECOMMEND_SETTING_TBL] SET [Key] = @Key ,[DataType] = @DataType ,[Description] = @Description WHERE [Key] = @Key", parameters);
        }

        public void updateRecommendationSettingDetail(Recommend_Setting_Value rsv)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", rsv.Key);
            parameters.Add("@Value", rsv.Value);
            parameters.Add("@isDefault", rsv.isDedault);
            parameters.Add("@ValueID", rsv.ValueID);
            executeNonQuery("UPDATE [RS].[RECOMMEND_SETTING_VALUE_TBL] SET [Key] = @Key ,[Value] = @Value ,[isDefault] = @isDefault WHERE ValueID = @ValueID", parameters);
        }

        public void insertRecommendationSettingDetail(Recommend_Setting_Value rsv)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", rsv.Key);
            parameters.Add("@Value", rsv.Value);
            parameters.Add("@isDefault", rsv.isDedault);
            executeNonQuery("INSERT INTO [RS].[RECOMMEND_SETTING_VALUE_TBL]([Key] ,[Value],[isDefault])VALUES(@Key,@Value,@isDefault)", parameters);
        }

        public void deleteRecommendationSettingDetail(int ValueID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ValueID", ValueID);
            executeNonQuery("DELETE [RS].[RECOMMEND_SETTING_VALUE_TBL] WHERE ValueID = @ValueID ", parameters);
        }

        public void deleteRecommendationSetting(string Key)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", Key);
            executeNonQuery("DELETE [RS].[RECOMMEND_SETTING_TBL] WHERE [Key] = @Key ", parameters);
        }

    }
}
