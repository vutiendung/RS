using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using rsbo_lib;

namespace rsdao_lib
{
    public class Clustering_DAO : BASE_DAO
    {
        public Clustering_DAO(): base(){}

        public List<Clustering_Setting> getListClusteringSetting()
        {
            List<Clustering_Setting> lstClusteringSetting = new List<Clustering_Setting>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            SqlDataReader reader = executeReader("SELECT RS.CLUSTER_SETTING_TBL.[Keys],RS.CLUSTER_SETTING_TBL.[DataType],RS.CLUSTER_SETTING_TBL.[Description] FROM RS.CLUSTER_SETTING_TBL,RS.CLUSTER_SETTING_VALUE_TBL WHERE RS.CLUSTER_SETTING_TBL.Keys = RS.CLUSTER_SETTING_VALUE_TBL.Keys", parameters);
            while (reader.Read())
            {
                Clustering_Setting cs = new Clustering_Setting();
                cs.Key = reader.GetString(reader.GetOrdinal("Keys"));
                cs.DataType = reader.GetString(reader.GetOrdinal("DataType"));
                cs.Description = reader.GetString(reader.GetOrdinal("Description"));
                lstClusteringSetting.Add(cs);
            }
            reader.Close();
            return lstClusteringSetting;
        }

        public List<Clustering_Setting> getListClusteringSetting(string ClusterType)
        {
            List<Clustering_Setting> lstClusteringSetting = new List<Clustering_Setting>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterType", ClusterType + "%");
            SqlDataReader reader = executeReader("SELECT RS.CLUSTER_SETTING_TBL.[Keys],RS.CLUSTER_SETTING_TBL.[DataType],RS.CLUSTER_SETTING_TBL.[Description] FROM RS.CLUSTER_SETTING_TBL,RS.CLUSTER_SETTING_VALUE_TBL WHERE RS.CLUSTER_SETTING_TBL.Keys = RS.CLUSTER_SETTING_VALUE_TBL.Keys and RS.CLUSTER_SETTING_TBL.[Keys] like @ClusterType", parameters);
            while (reader.Read())
            {
                Clustering_Setting cs = new Clustering_Setting();
                cs.Key = reader.GetString(reader.GetOrdinal("Keys"));
                cs.DataType = reader.GetString(reader.GetOrdinal("DataType"));
                cs.Description = reader.GetString(reader.GetOrdinal("Description"));
                lstClusteringSetting.Add(cs);
            }
            reader.Close();
            return lstClusteringSetting;
        }

        public List<Cluster_Setting_Value> getListClusteringSettingValue(string Key)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", Key);
            SqlDataReader reader = executeReader("SELECT ValueID,Value,isDefault FROM RS.CLUSTER_SETTING_VALUE_TBL WHERE [Keys] = @Key ORDER BY isDefault DESC", parameters);
            List<Cluster_Setting_Value> lstClusteringSettingValue = new List<Cluster_Setting_Value>();
            while (reader.Read())
            {
                Cluster_Setting_Value csv = new Cluster_Setting_Value();
                csv.ValueID = reader.GetInt32(reader.GetOrdinal("ValueID"));
                csv.Value = reader.GetString(reader.GetOrdinal("Value"));
                csv.isDedault = reader.GetBoolean(reader.GetOrdinal("isDefault"));
                csv.Key = Key;
                lstClusteringSettingValue.Add(csv);
            }
            reader.Close();
            return lstClusteringSettingValue;
        }

        public List<RSDataType> getListDataTypes()
        {
            SqlDataReader reader = executeReader("select [DataType],[Validation],[Guide],[DefaultValue] from [RS].[DATATYPE_TBL]", null);
            List<RSDataType> lstDataTypes = new List<RSDataType>();
            while (reader.Read())
            {
                RSDataType type = new RSDataType();
                type.DataType = reader.GetString(reader.GetOrdinal("DataType"));
                type.Validation = reader.GetString(reader.GetOrdinal("Validation"));
                type.Guide = reader.GetString(reader.GetOrdinal("Guide"));
                type.DefaultValue = reader.GetString(reader.GetOrdinal("DefaultValue")); ;
                lstDataTypes.Add(type);
            }
            reader.Close();
            return lstDataTypes;
        }

        public void insertClusteringSetting(Clustering_Setting cs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", cs.Key);
            parameters.Add("@DataType", cs.DataType);
            parameters.Add("@Description", cs.Description);
            executeNonQuery("INSERT INTO [RS].[CLUSTER_SETTING_TBL]([Key],[DataType],[Description]) VALUES(@Key ,@DataType,@Description)", parameters);
        }

        public void updateClusteringSetting(Clustering_Setting cs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", cs.Key);
            parameters.Add("@DataType", cs.DataType);
            parameters.Add("@Description", cs.Description);
            executeNonQuery("UPDATE [RS].[CLUSTER_SETTING_TBL] SET [Key] = @Key ,[DataType] = @DataType ,[Description] = @Description WHERE [Key] = @Key", parameters);
        }

        public void updateClusteringSettingDetail(Cluster_Setting_Value csv)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", csv.Key);
            parameters.Add("@Value", csv.Value);
            parameters.Add("@isDefault", csv.isDedault);
            parameters.Add("@ValueID", csv.ValueID);
            executeNonQuery("UPDATE [RS].[CLUSTER_SETTING_VALUE_TBL] SET [Key] = @Key ,[Value] = @Value ,[isDefault] = @isDefault WHERE ValueID = @ValueID", parameters);
        }

        public void insertClusteringSettingDetail(Cluster_Setting_Value csv)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", csv.Key);
            parameters.Add("@Value", csv.Value);
            parameters.Add("@isDefault", csv.isDedault);
            executeNonQuery("INSERT INTO [RS].[CLUSTER_SETTING_VALUE_TBL]([Key] ,[Value],[isDefault])VALUES(@Key,@Value,@isDefault)", parameters);
        }

        public void deleteClusteringSettingDetail(int ValueID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ValueID", ValueID);
            executeNonQuery("DELETE [RS].[CLUSTER_SETTING_VALUE_TBL] WHERE ValueID = @ValueID ", parameters);
        }

        public void deleteClusteringSetting(string Key)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", Key);
            executeNonQuery("DELETE [RS].[CLUSTER_SETTING_TBL] WHERE [Key] = @Key ", parameters);
        }

    }
}