using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using rsbo_lib;
using System.Globalization;

namespace rsdao_lib
{
    public class Clustering_Base_DAO : Clustering_DAO
    {
        public Clustering_Base_DAO() : base() {}

        public DateTime getCurrentTime()
        {
            string strSelect = "SELECT GETDATE() as 'CurrentDate'";
            SqlDataReader dr = executeReader(strSelect);
            DateTime date = DateTime.Now;
            while (dr.Read())
            {
                date = dr.GetDateTime(dr.GetOrdinal("CurrentDate"));
            }
            dr.Close();
            return date;
        }

        public List<System_Setting> getListSystemSetting()
        {
            List<System_Setting> lst = new List<System_Setting>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            SqlDataReader reader  = executeReader("SELECT [Key],[DataType],[Description] FROM [RS].[SYSTEM_SETTING_TBL] ", parameters);
            while (reader.Read())
            {
                System_Setting cs = new System_Setting();
                cs.Key = reader.GetString(reader.GetOrdinal("Key"));
                cs.DataType = reader.GetString(reader.GetOrdinal("DataType"));
                cs.Description = reader.GetString(reader.GetOrdinal("Description"));
                lst.Add(cs);
            }
            reader.Close();
            return lst;
        }

        public List<System_Setting_Value> getListSystemSettingValue(string Key)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", Key);
            SqlDataReader reader = executeReader("SELECT [ValueID],[Value],[isDefault] FROM [RS].[SYSTEM_SETTING_VALUE_TBL] WHERE [Key] = @Key", parameters);
            List<System_Setting_Value> lst = new List<System_Setting_Value>();
            while (reader.Read())
            {
                System_Setting_Value csv = new System_Setting_Value();
                csv.ValueID = reader.GetInt32(reader.GetOrdinal("ValueID"));
                csv.Value = reader.GetString(reader.GetOrdinal("Value"));
                csv.isDedault = reader.GetBoolean(reader.GetOrdinal("isDefault"));
                csv.Key = Key;
                lst.Add(csv);
            }
            reader.Close();
            return lst;
        }

        public void addSystemSetting(System_Setting sst)
        {
            string strQuery = "INSERT INTO [RS].[SYSTEM_SETTING_TBL] ([Key] ,[DataType] ,[Description]) VALUES (@Key ,@DataType ,@Description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Key", sst.Key);
            parameters.Add("@DataType", sst.DataType);
            parameters.Add("@Description", sst.Description);
            executeNonQuery(strQuery, parameters);
        }

        public void addNewClusterSchedule(ClusterSchedule clusterSchedule)
        {
            string strQuery = "INSERT INTO [RS].[CLUSTER_SCHEDULE_TBL] ([ClusterType] ,[StartTime] ,[Log] ,[Algorithm],[LoginID]) VALUES (@ClusterType , GETDATE() , @Log , @Algorithm, @LoginID) ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterType", clusterSchedule.ClusterType);
            parameters.Add("@Algorithm", clusterSchedule.Algorithm);
            parameters.Add("@Log", clusterSchedule.Log);
            parameters.Add("@LoginID", clusterSchedule.LoginID);
            executeNonQuery(strQuery, parameters);
        }

        public void updateClusterSchedule(ClusterSchedule clusterSchedule)
        {
            string strQuery = "UPDATE [RS].[CLUSTER_SCHEDULE_TBL] SET [StopTime] = GETDATE(),[Log] = @Log WHERE [ClusterType] = @ClusterType and [ScheduleID] in (SELECT TOP 1 [ScheduleID] FROM [RS].[CLUSTER_SCHEDULE_TBL] ORDER BY [StartTime] DESC)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Log", clusterSchedule.Log);
            parameters.Add("@ClusterType", clusterSchedule.ClusterType);
            executeNonQuery(strQuery, parameters);
        }

        public ClusterSchedule getLastClusterSchedule()
        {
            string strSelect = "SELECT TOP 1 [ScheduleID] ,[ClusterType] ,[StartTime] ,[StopTime] ,[Log] , [Algorithm], [LoginID] FROM [RS].[CLUSTER_SCHEDULE_TBL] ORDER BY [StartTime] DESC";
            SqlDataReader dr = executeReader(strSelect);
            ClusterSchedule clusterSchedule = null;
            while (dr.Read())
            {
                clusterSchedule = new ClusterSchedule();
                clusterSchedule.ScheduleID = dr.GetInt32(dr.GetOrdinal("ScheduleID"));
                clusterSchedule.ClusterType = dr.GetString(dr.GetOrdinal("ClusterType"));
                clusterSchedule.StartTime = dr.GetDateTime(dr.GetOrdinal("StartTime"));
                clusterSchedule.StopTime = dr.GetDateTime(dr.GetOrdinal("StopTime"));
                clusterSchedule.Log = dr.GetString(dr.GetOrdinal("Log"));
                clusterSchedule.Algorithm = dr.GetString(dr.GetOrdinal("Algorithm"));
                clusterSchedule.LoginID = dr.GetString(dr.GetOrdinal("LoginID"));
            }
            dr.Close();
            return clusterSchedule;
        }

        // V2.0 ----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<User_SubCategories> getAllSubCategories()
        {
            //string strSelect = "SELECT [U_SubCategoryID] ,[SubCategoryName] ,[U_CategoryID] FROM [RS].[USER_SUBCATEGORY_TBL]";
            string strSelect = "SELECT DISTINCT [U_SubCategoryID],[U_CategoryID] FROM [RS].[USER_TBL]";
            SqlDataReader dr = executeReader(strSelect);
            List<User_SubCategories> list = new List<User_SubCategories>();
            while (dr.Read())
            {
                User_SubCategories obj = new User_SubCategories();
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_SubCategoryID"));
                //obj.SubCategoryName = dr.GetString(dr.GetOrdinal("SubCategoryName"));
                obj.U_CategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<MatrixItem> computeRattingMatrixItem(string U_SubCategoryID, double Alpha)
        {
            string strSelect = "SELECT x1.UserID, x2.MetaItemID, (@Alpha1*(x2.S1*1.0/x1.SU + x2.S1*1.0/x3.SG) + @Alpha2*(x2.F1*1.0/x1.FU + x2.F1*1.0/x3.FG))/2.0 As Rate FROM "+
                               "(SELECT [RS].[USER_TBL].UserID, SUM([Quantity]) as SU, count(*) as FU FROM [RS].[TRANSACTION_TBL], [RS].[USER_TBL] "+
                               "WHERE [RS].[TRANSACTION_TBL].[UserID] = [RS].[USER_TBL].[UserID] and [RS].[USER_TBL].U_SubCategoryID = @U_SubCategoryID " +
                               "GROUP BY [RS].[USER_TBL].UserID) X1, "+
                               "(SELECT [RS].[USER_TBL].UserID, [RS].[ITEM_TBL].[MetaItemID], SUM([Quantity]) as S1, count(*) as F1 FROM [RS].[TRANSACTION_TBL], [RS].[ITEM_TBL], [RS].[USER_TBL] "+
                               "WHERE [RS].[TRANSACTION_TBL].[UserID] = [RS].[USER_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].[USER_TBL].U_SubCategoryID = @U_SubCategoryID " +
                               "GROUP BY [RS].[USER_TBL].UserID, [RS].[ITEM_TBL].[MetaItemID]) X2,  "+
                               "(SELECT [RS].[ITEM_TBL].[MetaItemID], SUM([Quantity]) as SG, count(*) as FG FROM [RS].[TRANSACTION_TBL], [RS].[ITEM_TBL], [RS].[USER_TBL] "+
                               "WHERE [RS].[TRANSACTION_TBL].[UserID] = [RS].[USER_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].[USER_TBL].U_SubCategoryID = @U_SubCategoryID " +
                               "GROUP BY [RS].[ITEM_TBL].[MetaItemID]) X3 "+
                               "WHERE X1.UserID = X2.UserID AND X2.MetaItemID = X3.MetaItemID and  (@Alpha1*(x2.S1*1.0/x1.SU + x2.S1*1.0/x3.SG) + @Alpha2*(x2.F1*1.0/x1.FU + x2.F1*1.0/x3.FG))/2.0 >0";

            //string strSelect = "SELECT x2.[UserID], x2.MetaItemID, ((x2.QTY*1.0/x1.M*1.0) + (x2.FRE*1.0/x1.N*1.0))/2.0 as 'Rate' FROM (SELECT MAX([Quantity]) as M ,count(*) as N ,[RS].[USER_TBL].U_SubCategoryID ,[MetaItemID] FROM [RS].[TRANSACTION_TBL], [RS].[USER_TBL], [RS].ITEM_TBL WHERE [RS].[TRANSACTION_TBL].[UserID] = [RS].[USER_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].[USER_TBL].U_SubCategoryID = @U_SubCategoryID GROUP BY [MetaItemID], [RS].[USER_TBL].U_SubCategoryID ) x1 inner join (SELECT [RS].[TRANSACTION_TBL].[UserID] ,[MetaItemID] ,SUM([Quantity]) as QTY ,count(*) as FRE , [RS].[USER_TBL].U_SubCategoryID FROM [RS].[TRANSACTION_TBL], [RS].[USER_TBL], [RS].ITEM_TBL WHERE [RS].[TRANSACTION_TBL].[UserID] = [RS].[USER_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].[USER_TBL].U_SubCategoryID = @U_SubCategoryID GROUP BY [RS].[TRANSACTION_TBL].[UserID], [MetaItemID], [RS].[USER_TBL].U_SubCategoryID ) x2 on (x1.U_SubCategoryID = x2.U_SubCategoryID and x1.[MetaItemID] = x2.[MetaItemID]) WHERE ((x2.QTY*1.0/x1.M*1.0) + (x2.FRE*1.0/x1.N*1.0))/2.0 > 0 ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            string str1 =Alpha.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            string str2 = (1-Alpha).ToString(CultureInfo.CreateSpecificCulture("en-GB"));

            parameters.Add("@Alpha1",str1);
            parameters.Add("@Alpha2",str2);
            parameters.Add("@U_SubCategoryID", U_SubCategoryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<MatrixItem> list = new List<MatrixItem>();
            while (dr.Read())
            {
                double rate = 0;
                try
                {
                    rate = (dr[1] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[2].ToString());
                }
                catch (Exception ex) 
                {
                    throw ex;
                }

                MatrixItem obj = new MatrixItem();
                obj.Row = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Column = dr.GetString(dr.GetOrdinal("MetaItemID"));
                obj.Cell = rate;
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public void CLEAN_RATTING_MATRIX()
        {
            executeNonQuery("truncate table [RS].[RATING_MATRIX]");
        }

    }
}
