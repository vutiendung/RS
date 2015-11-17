using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using rsbo_lib;

namespace rsdao_lib
{
    public class Clustering_Users_DAO : Clustering_Base_DAO
    {
        public Clustering_Users_DAO(): base(){}

        public void addPARTION_TBL(Partion item)
        {
            string strSelect = "INSERT INTO [RS].[PARTION_TBL] ([ClusterID] ,[UserID]) VALUES (@ClusterID ,@UserID) ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", item.ClusterID);
            parameters.Add("@UserID", item.UserID);
            executeNonQuery(strSelect, parameters);
        }

        public void addCluster(string id, string U_SubCategoryID)
        {
            string strSelect = "INSERT INTO [RS].[USER_CLUSTER_TBL] ([ClusterID] ,[U_SubCategoryID]) VALUES (@ClusterID ,@U_SubCategoryID)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", id);
            parameters.Add("@U_SubCategoryID", U_SubCategoryID);
            executeNonQuery(strSelect, parameters);
        }

        public void CLEAN_USER_CLUSTER_TBL()
        {
            string strSelect = "DELETE  [RS].[USER_CLUSTER_TBL]";
            executeNonQuery(strSelect);
        }
        public void CLEAN_USER_CENTROID_TBL()
        {
            string strSelect = "DELETE  [RS].[USER_CENTROID_TBL]";
            executeNonQuery(strSelect);
        }
        public void CLEAN_PARTION_TBL()
        {
            string strSelect = "DELETE  [RS].[PARTION_TBL]";
            executeNonQuery(strSelect);
        }

        public void CLEAN_ALL_INPUT_DATA()
        {
            string strSelect = "DELETE  [RS].[USER_TBL]";
            executeNonQuery(strSelect);
            
            strSelect = "DELETE [RS].[ITEM_TBL]";
            executeNonQuery(strSelect);

            strSelect = "DELETE [RS].[TRANSACTION_TBL]";
            executeNonQuery(strSelect);
        }

        /// <summary>
        /// Return HOUR
        /// </summary>
        /// <returns></returns>
        public int getLastRunning_HOUR(string LoginID)
        {
            string strSelect = " SELECT DATEDIFF(HOUR,(Select top 1 [StartTime] FROM [RS].CLUSTER_SCHEDULE_TBL WHERE [LoginID] = @LoginID ORDER BY [StartTime] DESC),GETDATE()) as 'Time'";
            SqlDataReader dr = executeReader(strSelect);
            int time = 0;
            while (dr.Read())
            {
                string t = dr.GetOrdinal("Time").ToString().Trim();
                time = Convert.ToInt32(t);
            }
            dr.Close();
            return time;
        }

        public int getLastRunning_MINUTE(string LoginID)
        {
            string strSelect = " SELECT DATEDIFF(MINUTE,(Select top 1 [StartTime] FROM [RS].CLUSTER_SCHEDULE_TBL WHERE [LoginID] = @LoginID ORDER BY [StartTime] DESC),GETDATE()) as 'Time'";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@LoginID", LoginID);
            SqlDataReader dr = executeReader(strSelect, parameters);
            int time = 0;
            while (dr.Read())
            {
                string t = dr.GetOrdinal("Time").ToString().Trim();
                time = Convert.ToInt32(t);
            }
            dr.Close();
            return time;
        }

        public void updateTransac_CheckPoint(string key)
        {
            string strQuery = "UPDATE [RS].[SYSTEM_SETTING_VALUE_TBL] SET [Value] = convert(varchar(MAX), GETDATE(), 100) WHERE ValueID in ( Select ValueID FROM [RS].[SYSTEM_SETTING_VALUE_TBL], [RS].[SYSTEM_SETTING_TBL] Where [SYSTEM_SETTING_VALUE_TBL].[Key] = [SYSTEM_SETTING_TBL].[Key] and [SYSTEM_SETTING_VALUE_TBL].[Key] = @Transac_cp and [SYSTEM_SETTING_TBL].[Datatype] ='datetime' and [isDefault]=1 ) ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Transac_cp", key);
            executeNonQuery(strQuery, parameters);
        }

        public void addTransac_CheckPoint(string key)
        {
            string strQuery = "INSERT INTO [RS].[SYSTEM_SETTING_VALUE_TBL] ([Key] ,[Value] ,[isDefault]) VALUES (@Transac_cp ,convert(varchar(MAX), GETDATE(), 100) ,1)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Transac_cp", key);
            executeNonQuery(strQuery, parameters);
        }

        //-----------------------------------------------------------------------------------------------

        #region NT

        public int[] getStaticsData()
        {
            int[] counter = new int[4];
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string strSelect = "select COUNT(DISTINCT UserID) as NumOfUsers from RS.TRANSACTION_TBL";
            SqlDataReader dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                counter[0] = dr.GetInt32(dr.GetOrdinal("NumOfUsers"));
            }
            dr.Close();
            strSelect = "select COUNT(DISTINCT i.MetaItemID) as NumOfMetaItems from RS.TRANSACTION_TBL as t INNER JOIN [RS].[ITEM_TBL] i ON t.ItemID = i.ItemID";
            dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                counter[1] = dr.GetInt32(dr.GetOrdinal("NumOfMetaItems"));
            }
            dr.Close();

            strSelect = "select COUNT(DISTINCT i.ItemID) as NumOfItems from RS.TRANSACTION_TBL as t INNER JOIN [RS].[ITEM_TBL] i ON t.ItemID = i.ItemID";
            dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                counter[2] = dr.GetInt32(dr.GetOrdinal("NumOfItems"));
            }
            dr.Close();

            strSelect = "select COUNT(DISTINCT u.U_SubCategoryID) as NumOfGroups from RS.TRANSACTION_TBL t INNER JOIN [RS].[USER_TBL] u ON t.UserID = u.UserID";
            dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                counter[3] = dr.GetInt32(dr.GetOrdinal("NumOfGroups"));
            }
            dr.Close();


            return counter;
        }
        //public List<ClusterInfo> getClusterInfo_BySubCategory(string U_SubCategoryID)
        //{
        //    string strSelect = "SELECT [ClusterID] ,count(*) as 'Count' FROM [RS].[PARTION_TBL], [RS].[USER_TBL] WHERE [PARTION_TBL].[UserID] = [USER_TBL].[UserID] and [USER_TBL].U_SubCategoryID = @U_SubCategoryID group by [ClusterID] order by [ClusterID] ASC";
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@U_SubCategoryID", U_SubCategoryID);
        //    SqlDataReader dr = executeReader(strSelect, parameters);

        //    List<ClusterInfo> list = new List<ClusterInfo>();
        //    while (dr.Read())
        //    {
        //        ClusterInfo obj = new ClusterInfo();
        //        obj.ClusterID = dr.GetString(dr.GetOrdinal("ClusterID"));
        //        obj.Count = dr.GetInt32(dr.GetOrdinal("Count"));
        //        list.Add(obj);
        //    }
        //    dr.Close();
        //    return list;
        //}
        //public List<User_SubCategories> getAllSubCategories()
        //{
        //    string strSelect = "SELECT [U_SubCategoryID],[SubCategoryName] FROM [RS].[USER_SUBCATEGORY_TBL]";
        //    SqlDataReader dr = executeReader(strSelect);
        //    List<User_SubCategories> list = new List<User_SubCategories>();
        //    while (dr.Read())
        //    {
        //        User_SubCategories obj = new User_SubCategories();
        //        obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_SubCategoryID"));
        //        obj.SubCategoryName = dr.GetString(dr.GetOrdinal("SubCategoryName"));
        //        list.Add(obj);
        //    }
        //    dr.Close();
        //    return list;
        //}

        #endregion

        public List<ClusterInfo> getClusterInfo_BySubCategory(string U_SubCategoryID)
        {
            string strSelect = "SELECT [ClusterID] ,count(*) as 'Count' FROM [RS].[PARTION_TBL], [RS].[USER_TBL] WHERE [PARTION_TBL].[UserID] = [USER_TBL].[UserID] and [USER_TBL].U_SubCategoryID = @U_SubCategoryID group by [ClusterID] order by [ClusterID] ASC";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_SubCategoryID", U_SubCategoryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<ClusterInfo> list = new List<ClusterInfo>();
            while (dr.Read())
            {
                ClusterInfo obj = new  ClusterInfo();
                obj.ClusterID = dr.GetString(dr.GetOrdinal("ClusterID"));
                obj.Count = dr.GetInt32(dr.GetOrdinal("Count"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<ClusterSchedule> getListClusterSchedule_DESC()
        {
            string strSelect = "SELECT [ScheduleID] ,[ClusterType] ,[StartTime] ,[StopTime] ,[Log] ,[Algorithm] ,[LoginID] FROM [RS].[CLUSTER_SCHEDULE_TBL] order by [StartTime] ASC";
            SqlDataReader dr = executeReader(strSelect);

            List<ClusterSchedule> list = new List<ClusterSchedule>();
            while (dr.Read())
            {
                ClusterSchedule obj = new ClusterSchedule();
                obj.ScheduleID = dr.GetInt32(dr.GetOrdinal("ScheduleID"));
                obj.ClusterType = dr.GetString(dr.GetOrdinal("ClusterType"));
                obj.StartTime = dr.GetDateTime(dr.GetOrdinal("StartTime"));
                obj.StopTime = (dr["StopTime"] == System.DBNull.Value) ? (DateTime?)null : dr.GetDateTime(dr.GetOrdinal("StopTime"));
                obj.Log = dr.GetString(dr.GetOrdinal("Log"));
                obj.Algorithm = dr.GetString(dr.GetOrdinal("Algorithm"));
                obj.LoginID = dr.GetString(dr.GetOrdinal("LoginID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public void setRattingMatrix(MatrixItem matrixItem)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", matrixItem.ClusterID);
            parameters.Add("@UserID", matrixItem.Row);
            parameters.Add("@MetaItemID", matrixItem.Column);
            parameters.Add("@Value", matrixItem.Cell);
            executeNonQuery("INSERT INTO [RS].[RATING_MATRIX] ([ClusterID] ,[UserID] ,[MetaItemID] ,[Value]) VALUES (@ClusterID ,@UserID ,@MetaItemID ,@Value)", parameters);
        }

        public void add_User_Centroid(User_Centroid centroid)
        {
            string strSelect = "INSERT INTO  [RS].[USER_CENTROID_TBL] ([ClusterID] ,[MetaItemID] ,[Value]) VALUES (@ClusterID ,@MetaItemID ,@Value) ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", centroid.ClusterID);
            parameters.Add("@MetaItemID", centroid.MetaItemID);
            parameters.Add("@Value", centroid.Value);

            executeNonQuery(strSelect, parameters);
        }

        public void removeExcessCluster()
        {
            string strSelect = "DELETE FROM [RS].[USER_CLUSTER_TBL] WHERE [RS].[USER_CLUSTER_TBL].ClusterID NOT IN ( SELECT [RS].[USER_CLUSTER_TBL].ClusterID FROM [RS].[USER_CLUSTER_TBL] join [RS].[PARTION_TBL] on [RS].[USER_CLUSTER_TBL].ClusterID = [RS].PARTION_TBL.ClusterID ) ";
            executeNonQuery(strSelect);
        }

        public List<Partion> getPartion_ByU_SubCategoryID(string U_SubCategoryID)
        {
            string strSelect = "SELECT [RS].[PARTION_TBL].[ClusterID] ,[UserID] FROM [RS].[PARTION_TBL],[RS].[USER_CLUSTER_TBL] WHERE [RS].[PARTION_TBL].[ClusterID] = [RS].[USER_CLUSTER_TBL].[ClusterID] and [RS].[USER_CLUSTER_TBL].[U_SubCategoryID] = @U_SubCategoryID ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_SubCategoryID", U_SubCategoryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Partion> list = new List<Partion>();
            while (dr.Read())
            {
                Partion obj = new Partion();
                obj.ClusterID = dr.GetString(dr.GetOrdinal("ClusterID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Partion> getPartion_ByU_CategoryID_ForMergeGreoup(string U_Category)
        {
            string strSelect = "SELECT [ClusterID] ,[UserID] FROM [RS].[PARTION_TBL] WHERE [ClusterID] LIKE @U_Category";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", U_Category + "%");
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Partion> list = new List<Partion>();
            while (dr.Read())
            {
                Partion obj = new Partion();
                obj.ClusterID = dr.GetString(dr.GetOrdinal("ClusterID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<User_Centroid> getUserCentroid(string ClusterID)
        {
            string strSelect = "SELECT [ClusterID] ,[MetaItemID] ,[Value] FROM [RS].[USER_CENTROID_TBL] WHERE [ClusterID]=@ClusterID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", ClusterID);
            SqlDataReader dr = executeReader(strSelect, parameters);
            List<User_Centroid> list = new List<User_Centroid>();
            while (dr.Read())
            {
                User_Centroid obj = new User_Centroid();
                obj.ClusterID = ClusterID;
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("MetaItemID"));
                obj.Value = dr.GetDouble(dr.GetOrdinal("Value"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public void delete_USER_CLUSTER_TBL(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[USER_CLUSTER_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }

        public void delete_PARTION_TBL(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[PARTION_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }

        public void delete_USER_CENTROID_TBL(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[USER_CENTROID_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }


        public void delete_USER_CLUSTER_TBL_2(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", "%" + categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[USER_CLUSTER_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }

        public void delete_PARTION_TBL_2(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", "%" + categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[PARTION_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }

        public void delete_USER_CENTROID_TBL_2(string categoryName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_Category", "%" + categoryName + "%");
            executeNonQuery("DELETE FROM [RS].[USER_CENTROID_TBL] WHERE [ClusterID] LIKE @U_Category ", parameters);
        }
    }
}