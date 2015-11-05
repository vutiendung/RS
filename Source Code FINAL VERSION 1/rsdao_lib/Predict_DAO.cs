using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsbo_lib;
using System.Data.SqlClient;
using rsglobal_lib;

namespace rsdao_lib
{
    public class Predict_DAO : Clustering_Base_DAO
    {
        public Predict_DAO()
            : base()
        {
        }

        public List<User> getNewUser_UnBlocked()
        {
            //string strSelect = "SELECT [USER_TBL].[UserID] ,[UserName] ,[U_SubCategoryID] FROM [RS].[USER_TBL] WHERE [Blocked] = 0 and [USER_TBL].[UserID] NOT IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] )";
            string strSelect = "SELECT [USER_TBL].[UserID] ,[UserName] ,[U_SubCategoryID] FROM [RS].[USER_TBL] WHERE [USER_TBL].[UserID] NOT IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] )";
            SqlDataReader dr = executeReader(strSelect);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_SubCategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<User> getTraditionalUser_UnBlocked()
        {
            //string strSelect = "SELECT DISTINCT [USER_TBL].[UserID] ,[UserName] ,[U_CategoryID] FROM [RS].[USER_TBL] WHERE [USER_TBL].[UserID] IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] ) and [Blocked] = 0";
            string strSelect = "SELECT DISTINCT [USER_TBL].[UserID] ,[UserName] ,[U_CategoryID] FROM [RS].[USER_TBL] WHERE [USER_TBL].[UserID] IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] ) ";
            SqlDataReader dr = executeReader(strSelect);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Item> getItems_ByItemCategoryCodeOfItem(string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            string strSelect = "SELECT [ItemID] ,[ItemName] ,[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) ";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> list = new List<Item>();
            while (dr.Read())
            {
                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.ItemName = dr.GetString(dr.GetOrdinal("ItemName"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("ItemCategoryCode"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Item> getNewItems()
        {
            string strSelect = "SELECT [ItemID] ,[ItemName] ,[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] NOT IN( SELECT [ItemID] FROM [RS].[TRANSACTION_TBL] ) ";
            SqlDataReader dr = executeReader(strSelect);

            List<Item> list = new List<Item>();
            while (dr.Read())
            {
                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.ItemName = dr.GetString(dr.GetOrdinal("ItemName"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("ItemCategoryCode"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Item> getItems_ByItemCategoryCode_ExistTransac(string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            string strSelect = "SELECT [ItemID] ,[ItemName] ,[ItemCategoryCode],[ProductGroupCode]FROM [RS].[ITEM_TBL] WHERE [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) and [ItemID] IN ( SELECT DISTINCT [ItemID] FROM [RS].[TRANSACTION_TBL])";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> list = new List<Item>();
            while (dr.Read())
            {
                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.ItemName = dr.GetString(dr.GetOrdinal("ItemName"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("ItemCategoryCode"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<User> getUnBlockUsers()
        {
            //tring strSelect = "SELECT [UserID] ,[UserName] ,[U_CategoryID] ,[Blocked] ,[CreateDate] FROM [RS].[USER_TBL] Where [Blocked] = 0";
            string strSelect = "SELECT [UserID] ,[UserName] ,[U_CategoryID] ,[CreateDate] FROM [RS].[USER_TBL]";
            SqlDataReader dr = executeReader(strSelect);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                obj.CreateDate = dr.GetDateTime(dr.GetOrdinal("CreateDate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public void addUserICatRate(UserICatRate obj)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", obj.UserID);
            parameters.Add("@ItemCategoryID", obj.ItemCategoryID);
            parameters.Add("@MeansRate", obj.MeansRate);
            executeNonQuery("INSERT INTO [RS].[TEMP_USER_ICAT_RATE] ([UserID] ,[ItemCategoryID] ,[MeansRate]) VALUES (@UserID ,@ItemCategoryID ,@MeansRate)", parameters);
        }

        public List<UserICatRate> getUserICatRate_ByUserID(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [UserID] ,[ItemCategoryID] ,[MeansRate] FROM [RS].[TEMP_USER_ICAT_RATE] WHERE [UserID] = @UserID ";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<UserICatRate> list = new List<UserICatRate>();
            while (dr.Read())
            {
                UserICatRate obj = new UserICatRate();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.ItemCategoryID = dr.GetString(dr.GetOrdinal("ItemCategoryID"));
                obj.MeansRate = dr.GetDouble(dr.GetOrdinal("MeansRate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public void truncateUserICatRate()
        {
            executeNonQuery("truncate table [RS].[TEMP_USER_ICAT_RATE]");
        }

        public void truncateRCNewItem()
        {
            executeNonQuery("truncate table [RS].[TEMP_RC_NEWITEM_TBL]");
        }

        public void truncateRCBestSelling()
        {
            executeNonQuery("truncate table [RS].[TEMP_RC_BESTSELLING_TBL]");
        }

        public void truncateRCPurchased()
        {
            executeNonQuery("truncate table [RS].[TEMP_RC_PURCHASED_TBL]");
        }

        public void truncateRCNotPurchased()
        {
            executeNonQuery("truncate table [RS].[TEMP_RC_NOT_PURCHASED_TBL]");
        }

        public void truncateRecommendation()
        {
            executeNonQuery("truncate table [RS].[RECOMMENDATION_TBL]");
        }

        public void addRCNewItem(RCItem obj)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", obj.UserID);
            parameters.Add("@ItemID", obj.ItemID);
            parameters.Add("@Rate", obj.Rate);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_NEWITEM_TBL] ([UserID] ,[ItemID] ,[Rate]) VALUES (@UserID ,@ItemID ,@Rate)", parameters);
        }

        public void addRCBestSelling(RCItem obj)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", obj.UserID);
            parameters.Add("@ItemID", obj.ItemID);
            parameters.Add("@Rate", obj.Rate);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_BESTSELLING_TBL] ([UserID] ,[ItemID] ,[Rate]) VALUES (@UserID ,@ItemID ,@Rate)", parameters);
        }

        public void addRCPurchased(RCItem obj)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", obj.UserID);
            parameters.Add("@ItemID", obj.ItemID);
            parameters.Add("@Rate", obj.Rate);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_PURCHASED_TBL] ([UserID] ,[ItemID] ,[Rate]) VALUES (@UserID ,@ItemID ,@Rate)", parameters);
        }

        public void addRCNotPurchased(RCItem obj)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", obj.UserID);
            parameters.Add("@ItemID", obj.ItemID);
            parameters.Add("@Rate", obj.Rate);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_NOT_PURCHASED_TBL] ([UserID] ,[ItemID] ,[Rate]) VALUES (@UserID ,@ItemID ,@Rate)", parameters);
        }

        public List<RCItem> getRCBestSellingItems(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [UserID] ,[ItemID] ,[Rate] FROM [RS].[TEMP_RC_BESTSELLING_TBL] WHERE [UserID] = @UserID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<RCItem> list = new List<RCItem>();
            while (dr.Read())
            {
                RCItem obj = new RCItem();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Rate = dr.GetDouble(dr.GetOrdinal("Rate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<RCItem> getRCPurchasedItems(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [UserID] ,[ItemID] ,[Rate] FROM [RS].[TEMP_RC_PURCHASED_TBL] WHERE [UserID] = @UserID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<RCItem> list = new List<RCItem>();
            while (dr.Read())
            {
                RCItem obj = new RCItem();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Rate = dr.GetDouble(dr.GetOrdinal("Rate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<RCItem> getRCNotPurchasedItems(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [UserID] ,[ItemID] ,[Rate] FROM [RS].[TEMP_RC_NOT_PURCHASED_TBL] WHERE [UserID] = @UserID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<RCItem> list = new List<RCItem>();
            while (dr.Read())
            {
                RCItem obj = new RCItem();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Rate = dr.GetDouble(dr.GetOrdinal("Rate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<RCItem> getRCNewItems(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [UserID] ,[ItemID] ,[Rate] FROM [RS].[TEMP_RC_NEWITEM_TBL] WHERE [UserID] = @UserID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<RCItem> list = new List<RCItem>();
            while (dr.Read())
            {
                RCItem obj = new RCItem();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Rate = dr.GetDouble(dr.GetOrdinal("Rate"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        //public void addRecommendationItem(Recommendation_Item RCItem) //SONNT has modified
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@ScheduleID", RCItem.ScheduleID);
        //    parameters.Add("@MetaItemID", RCItem.MetaItemID);
        //    parameters.Add("@UserID", RCItem.UserID);
        //    parameters.Add("@Quantity", RCItem.Quantity);
        //    parameters.Add("@Score", RCItem.Score);
        //    parameters.Add("@RecommendType", RCItem.RecommendType);
        //    parameters.Add("@Price", RCItem.Price);
        //    //executeNonQuery("INSERT INTO [RS].[RECOMMENDATION_TBL] ([ScheduleID] ,[MetaItemID] ,[UserID] ,[Quantity] ,[Score] ,[RecommendType],[Price],[MetaItemName],[UserName])  VALUES (@ScheduleID ,@MetaItemID ,@UserID ,@Quantity ,@Score ,@RecommendType,@Price, (SELECT i.[MetaItemName] from [RS].[META_ITEM_TBL] i WHERE i.[MetaItemID] = @MetaItemID),(SELECT u.[UserName] from [RS].[USER_TBL] u WHERE u.[UserID] = @UserID))", parameters);
        //    executeNonQuery("INSERT INTO [RS].[RECOMMENDATION_TBL] ([ScheduleID] ,[MetaItemID] ,[UserID] ,[Quantity] ,[Score] ,[RecommendType],[Price])  VALUES (@ScheduleID ,@MetaItemID ,@UserID ,@Quantity ,@Score ,@RecommendType,@Price)", parameters);
        //}

        public void addRecommendationItem(Recommendation_Meta_Item RCItem) //SONNT has modified
        {
               string[] MetaID = RCItem.MetaItemID.Split('_');
               
                RCItem.MetaItemID = MetaID[0];
                RCItem.ItemFamillyCode = MetaID[1];
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@ScheduleID", RCItem.ScheduleID);
                parameters.Add("@MetaItemID", RCItem.MetaItemID);
                parameters.Add("@UserID", RCItem.UserID);
                parameters.Add("@Quantity", RCItem.Quantity);
                parameters.Add("@Score", RCItem.Score);
                parameters.Add("@RecommendType", RCItem.RecommendType);
                parameters.Add("@ItemFamillyCode", RCItem.ItemFamillyCode);
                executeNonQuery("INSERT INTO [RS].[RECOMMENDATION_TBL] ([ScheduleID] ,[MetaItemID] ,[UserID] ,[Quantity] ,[Score] ,[RecommendType],[ItemFamillyCode])  VALUES (@ScheduleID ,@MetaItemID ,@UserID ,@Quantity ,@Score ,@RecommendType,@ItemFamillyCode)", parameters);
            
           
        }

        public List<User> getUser_SameClusterID_InTransac(string UserID, string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            parameters.Add("@UserID", UserID);
            string strSelect = "select DISTINCT u.[UserID],u.[UserName],u.[U_CategoryID] from ( SELECT [USER_TBL].[UserID],[USER_TBL].[UserName],[USER_TBL].[U_CategoryID] FROM [RS].[USER_TBL], [RS].[PARTION_TBL] WHERE [USER_TBL].[UserID] = [PARTION_TBL].[UserID] and [ClusterID] IN (SELECT [ClusterID] FROM [RS].[PARTION_TBL] WHERE [UserID]= @UserID)) u inner join [RS].[TRANSACTION_TBL] t on u.[UserID] = t.[UserID] Where t.[ItemID] = @ItemID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<User> getUser_SameCategory_InTransac(string UserID, string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            parameters.Add("@UserID", UserID);
            string strSelect = "select DISTINCT u.[UserID],u.[UserName],u.[U_CategoryID] from ( SELECT [USER_TBL].[UserID],[USER_TBL].[UserName],[USER_TBL].[U_CategoryID] FROM [RS].[USER_TBL] WHERE [U_CategoryID] IN (SELECT [U_CategoryID] FROM [RS].[PARTION_TBL] WHERE [UserID]= @UserID)) u inner join [RS].[TRANSACTION_TBL] t on u.[UserID] = t.[UserID] Where t.[ItemID] = @ItemID ";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                obj.U_SubCategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<User> getUser_SameItem_InTransac(string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            string strSelect = "select DISTINCT  [UserID] from [rs].TRANSACTION_TBL where  ItemID = @ItemID;";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<User> list = new List<User>();
            while (dr.Read())
            {
                User obj = new User();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                //obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                //obj.CategoryID = dr.GetString(dr.GetOrdinal("U_CategoryID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<Item> getItem_SameCategory_InTransac(string UserID, string ItemID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            parameters.Add("@UserID", UserID);
            string strSelect = "SELECT [ITEM_TBL].[ItemID] ,[ItemName] ,[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL], [RS].[TRANSACTION_TBL] WHERE [ITEM_TBL].[ItemID] = [TRANSACTION_TBL].[ItemID] and [TRANSACTION_TBL].[UserID] = @UserID and [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) ";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> list = new List<Item>();
            while (dr.Read())
            {
                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.ItemName = dr.GetString(dr.GetOrdinal("ItemName"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("ItemCategoryCode"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public double getQuanlityMeans_ByItemID_UserID(string UserID, string ItemID)
        {
            string strSelect = "SELECT [TRANSACTION_TBL].[UserID] ,[TRANSACTION_TBL].[ItemID] ,  avg([Quantity]) as 'QuantityMeans' FROM [RS].[TRANSACTION_TBL] WHERE [TRANSACTION_TBL].[UserID] = @UserID and [TRANSACTION_TBL].[ItemID] = @ItemID group by [TRANSACTION_TBL].[UserID],[TRANSACTION_TBL].[ItemID] ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double quantity = 0;
            while (dr.Read())
            {
                try
                {
                    quantity = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return quantity;
        }

        public double getQuanlityMeans_SameClusterID_UserID(string UserID, string ItemID)
        {
            string strSelect = "select avg(ts.QuantityMeans) as 'QuantityMeans' FROM ( select DISTINCT u.[UserID] , avg([Quantity]) as 'QuantityMeans' from ( SELECT [USER_TBL].[UserID] FROM [RS].[USER_TBL], [RS].[PARTION_TBL] WHERE [USER_TBL].[UserID] = [PARTION_TBL].[UserID] and [ClusterID] IN (SELECT [ClusterID] FROM [RS].[PARTION_TBL] WHERE [UserID]= @UserID) ) u inner join [RS].[TRANSACTION_TBL] t on u.[UserID] = t.[UserID] Where t.[ItemID] = @ItemID group by u.[UserID],t.[ItemID] ) ts ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double quantity = 0;
            while (dr.Read())
            {
                try
                {
                    quantity = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return quantity;
        }

        public double getQuanlityMeans_SameUCategory_UserID(string UserID, string ItemID)
        {
            string strSelect = "select avg(ts.QuantityMeans) 'Quantity_M' FROM ( select DISTINCT u.[UserID] , avg([Quantity])  'QuantityMeans' from ( SELECT [USER_TBL].[UserID] FROM [RS].[USER_TBL] WHERE [U_CategoryID] IN (SELECT [U_CategoryID] FROM [RS].[USER_TBL] WHERE [UserID]= @UserID) ) u inner join [RS].[TRANSACTION_TBL] t on u.[UserID] = t.[UserID] Where t.[ItemID] = @ItemID group by u.[UserID],t.[ItemID] ) ts ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double quantity = 0;
            while (dr.Read())
            {
                try
                {
                    quantity = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return quantity;
        }

        public double getQuanlityMeans_SameItem(string ItemID)
        {
            string strSelect = "select avg(ts.QuantityMeans) as 'QuantityMeans' FROM ( select DISTINCT [UserID] , avg([Quantity]) as 'QuantityMeans' from [RS].[TRANSACTION_TBL] Where [ItemID] = @ItemID group by [UserID], [ItemID] ) ts ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double quantity = 0;
            while (dr.Read())
            {
                try
                {
                    quantity = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return quantity;
        }

        public double getQuanlityMean_SameICategory_InTransac(string UserID, string ItemID)
        {
            string strSelect = "select avg(ts.QuantityMeans) as 'QuantityMeans' FROM ( SELECT avg([Quantity]) as 'QuantityMeans' FROM [RS].[ITEM_TBL], [RS].[TRANSACTION_TBL] WHERE [ITEM_TBL].[ItemID] = [TRANSACTION_TBL].[ItemID] and [TRANSACTION_TBL].[UserID] = @UserID and [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) group by [TRANSACTION_TBL].[ItemID] ,[TRANSACTION_TBL].[UserID] ) ts ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double quantity = 0;
            while (dr.Read())
            {
                try
                {
                    quantity = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return quantity;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public List<Item> getRatingMatrixMeans_ByClusterID_RCNotPurchased(string UserID)
        {
            string strSelect = "SELECT rt.ItemID, avg(rt.Rate) 'RateMeans' FROM ( SELECT t.[UserID], t.ItemID, (SUM(Quantity) + COUNT(*))/2.0 as 'Rate' FROM (SELECT [USER_TBL].[UserID] FROM [RS].[USER_TBL], [RS].[PARTION_TBL] WHERE [USER_TBL].[UserID] = [PARTION_TBL].[UserID] and [ClusterID] IN (SELECT [ClusterID] FROM [RS].[PARTION_TBL] WHERE [UserID]= @UserID)) u inner join [RS].[TRANSACTION_TBL] t on u.[UserID] = t.[UserID] GROUP BY t.[UserID], t.ItemID HAVING t.ItemID NOT IN (SELECT ItemID FROM [RS].[TRANSACTION_TBL] WHERE [UserID]= @UserID) ) rt group by rt.ItemID ORDER BY RateMeans DESC";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> lst = new List<Item>();
            while (dr.Read())
            {
                double rate = 0;
                try
                {
                    rate = (dr[1] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[1].ToString());
                }
                catch (Exception) { }

                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.Rate = rate;
                lst.Add(obj);
            }

            dr.Close();
            return lst;
        }

        public List<Item> getQuantityMatrix_ByUser(string UserID)
        {
            string strSelect = "SELECT ItemID, SUM(Quantity) as 'Quantity'  FROM [RS].[TRANSACTION_TBL] WHERE [UserID]= @UserID GROUP BY [UserID], ItemID ORDER BY Quantity DESC";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> lst = new List<Item>();
            while (dr.Read())
            {
                double rate = 0;
                try
                {
                    rate = (dr[1] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[1].ToString());
                }
                catch (Exception) { }

                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.Rate = rate;
                lst.Add(obj);
            }

            dr.Close();
            return lst;
        }

        public List<Item> getQuantityMatrix_ByUcategory(string U_CategoryID)
        {
            string strSelect = "SELECT rt.ItemID, avg(rt.Quantity) 'QuantityMeans' FROM ( SELECT [TRANSACTION_TBL].[UserID],ItemID, SUM(Quantity) as 'Quantity' FROM [RS].[TRANSACTION_TBL],[RS].[USER_TBL] WHERE [TRANSACTION_TBL].[UserID] = [USER_TBL].[UserID] and [USER_TBL].[U_CategoryID] = @U_CategoryID GROUP BY [TRANSACTION_TBL].[UserID], ItemID ) rt group by rt.ItemID ORDER BY QuantityMeans DESC ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@U_CategoryID", U_CategoryID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> lst = new List<Item>();
            while (dr.Read())
            {
                double rate = 0;
                try
                {
                    rate = (dr[1] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[1].ToString());
                }
                catch (Exception) { }

                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.Rate = rate;
                lst.Add(obj);
            }

            dr.Close();
            return lst;
        }

        public List<Item> getDemandOfUser(string UserID)
        {
            string strSelect = "SELECT ts1.ItemID , ts1.CurrDate_LastDate - (ts2.LastQuantity * ts1.meansQuanlity) as 'Demand' , ts1.LastDate_FirstDate FROM (SELECT [UserID] , ItemID , SUM(Quantity) as 'TotalQuantity' , (DATEDIFF(day, MIN(Date), MAX(Date)) + 1.0) as 'LastDate_FirstDate' , (DATEDIFF(day, MAX(Date), GETDATE()) + 1.0) as 'CurrDate_LastDate' , SUM(Quantity) / (DATEDIFF(day, MIN(Date), MAX(Date)) + 1.0) as 'meansQuanlity' FROM [RS].[TRANSACTION_TBL] WHERE [UserID]= @UserID GROUP BY [UserID], ItemID ) ts1, (SELECT t2.[UserID],t2.[ItemID],t2.Quantity as 'LastQuantity' FROM (SELECT [UserID] , [ItemID] , MAX([Date]) as 'LastDate' FROM [RS].[TRANSACTION_TBL] GROUP BY [UserID],[ItemID] ) t1,[RS].[TRANSACTION_TBL] t2 WHERE t1.[UserID] = t2.[UserID] and t1.[ItemID] = t2.[ItemID] and t1.LastDate = t2.[Date] ) ts2 WHERE ts1.UserID = ts2.UserID and ts1.ItemID = ts2.ItemID ORDER BY Demand DESC";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Item> lst = new List<Item>();
            while (dr.Read())
            {
                double rate = 0;
                double LastDate_FirstDate = 1;
                try
                {
                    rate = (dr[1] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[1].ToString());
                }
                catch (Exception) { }
                try
                {
                    LastDate_FirstDate = (dr[2] == System.DBNull.Value) ? 1.0 : Convert.ToDouble(dr[2].ToString());
                }
                catch (Exception) { }

                Item obj = new Item();
                obj.ItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                obj.Rate = rate;
                obj.LastDate_FirstDate = LastDate_FirstDate;
                lst.Add(obj);
            }

            dr.Close();
            return lst;
        }

        public double getDemainMeans_ByItemCategoryCodeOfItem(string UserID, string ItemID)
        {
            string strSelect = "SELECT AVG(Demand) as 'DemandMeans' FROM ( SELECT ts1.ItemID , ts1.CurrDate_LastDate - (ts2.LastQuantity * ts1.meansQuanlity) as 'Demand' , ts1.LastDate_FirstDate FROM ( SELECT [UserID] , ItemID , SUM(Quantity) as 'TotalQuantity' , (DATEDIFF(day, MIN(Date), MAX(Date)) + 1.0) as 'LastDate_FirstDate' , (DATEDIFF(day, MAX(Date), GETDATE()) + 1.0) as 'CurrDate_LastDate' , SUM(Quantity) / (DATEDIFF(day, MIN(Date), MAX(Date)) + 1.0) as 'meansQuanlity' FROM [RS].[TRANSACTION_TBL] WHERE [UserID]= @UserID and [ItemID] IN ( SELECT [ItemID] FROM [RS].[ITEM_TBL] WHERE [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) ) GROUP BY [UserID], [ItemID] Having (DATEDIFF(day, MIN(Date), MAX(Date))) > 0.0 ) ts1, ( SELECT t2.[UserID],t2.[ItemID],t2.Quantity as 'LastQuantity' FROM (SELECT [UserID] , [ItemID] , MAX([Date]) as 'LastDate' FROM [RS].[TRANSACTION_TBL] GROUP BY [UserID],[ItemID] ) t1,[RS].[TRANSACTION_TBL] t2 WHERE t1.[UserID] = t2.[UserID] and t1.[ItemID] = t2.[ItemID] and t1.LastDate = t2.[Date] and t2.[ItemID] IN ( SELECT [ItemID] FROM [RS].[ITEM_TBL] WHERE [ItemCategoryCode] IN ( SELECT [ItemCategoryCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] = @ItemID ) ) ) ts2 WHERE ts1.UserID = ts2.UserID and ts1.ItemID = ts2.ItemID ) dm ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            parameters.Add("@ItemID", ItemID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            double demandMeans = 0;
            while (dr.Read())
            {
                try
                {
                    demandMeans = (dr[0] == System.DBNull.Value) ? 0.0 : Convert.ToDouble(dr[0].ToString());
                }
                catch (Exception) { }
            }
            dr.Close();
            return demandMeans;
        }

        public void setRCNewItem_FormTempTable(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_NEWITEM_TBL] SELECT TOP 1 a.[UserID], a.[ItemID], a.[MeansRate] FROM ( SELECT r.[UserID], i.[ItemID], r.[MeansRate] FROM (SELECT [UserID] ,[ItemCategoryID] ,[MeansRate] FROM [RS].[TEMP_USER_ICAT_RATE] WHERE [UserID] = @UserID ) r inner join (SELECT [ItemID],[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] NOT IN( SELECT [ItemID] FROM [RS].[TRANSACTION_TBL])) i ON (r.[ItemCategoryID] = i.[ItemCategoryCode]) UNION SELECT r.[UserID], i.[ItemID], r.[MeansRate] FROM (SELECT [UserID] ,[ItemCategoryID] ,[MeansRate] FROM [RS].[TEMP_USER_ICAT_RATE] WHERE [UserID] = @UserID ) r inner join (SELECT [ItemID],[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] NOT IN( SELECT [ItemID] FROM [RS].[TRANSACTION_TBL])) i ON (r.[ItemCategoryID] = i.[ProductGroupCode]) ) a ORDER BY a.[MeansRate] DESC ", parameters);
        }

        public void setRCNewItem_FormTempTable_ALLTranditionalUser()
        {
            executeNonQuery_LongTimeOut("INSERT INTO [RS].[TEMP_RC_NEWITEM_TBL] SELECT xx.[UserID], xx.[ItemID], xx.[MeansRate] FROM ( SELECT a.[UserID], a.[ItemID], a.[MeansRate] ,RowID = ROW_NUMBER() OVER (PARTITION BY a.[UserID] ORDER BY a.[MeansRate] DESC) FROM ( SELECT r.[UserID], i.[ItemID], r.[MeansRate] FROM (SELECT [UserID] ,[ItemCategoryID] ,[MeansRate] FROM [RS].[TEMP_USER_ICAT_RATE] WHERE [UserID] IN (SELECT [USER_TBL].[UserID] FROM [RS].[USER_TBL] WHERE [USER_TBL].[UserID] IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] ) and [Blocked] = 0)) r inner join (SELECT [ItemID],[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] NOT IN( SELECT [ItemID] FROM [RS].[TRANSACTION_TBL])) i ON (r.[ItemCategoryID] = i.[ItemCategoryCode]) UNION SELECT r.[UserID], i.[ItemID], r.[MeansRate] FROM (SELECT [UserID] ,[ItemCategoryID] ,[MeansRate] FROM [RS].[TEMP_USER_ICAT_RATE] WHERE [UserID] IN (SELECT [USER_TBL].[UserID] FROM [RS].[USER_TBL] WHERE [USER_TBL].[UserID] IN ( SELECT [UserID] FROM [RS].[TRANSACTION_TBL] ) and [Blocked] = 0)) r inner join (SELECT [ItemID],[ItemCategoryCode],[ProductGroupCode] FROM [RS].[ITEM_TBL] WHERE [ItemID] NOT IN( SELECT [ItemID] FROM [RS].[TRANSACTION_TBL])) i ON (r.[ItemCategoryID] = i.[ProductGroupCode]) ) a )xx WHERE xx.RowID = 1 ");
        }

        public void setRCNotPurchasedItems(string UserID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", UserID);
            executeNonQuery("INSERT INTO [RS].[TEMP_RC_NOT_PURCHASED_TBL] SELECT DISTINCT Top 3 @UserID, rt.[ItemID], Avg(rt.rate) 'RateMeans' FROM (SELECT t.[UserID], t.[ItemID], ( Sum(quantity) + Count(*) ) / 2.0 AS 'Rate' FROM (SELECT [user_tbl].[userid] FROM [RS].[user_tbl], [RS].[user_cluster_detail] WHERE [user_tbl].[userid] = [user_cluster_detail].[userid] AND [clusterid] IN (SELECT [clusterid] FROM [RS].[user_cluster_detail] WHERE [userid] = @UserID)) u INNER JOIN [RS].[transaction_tbl] t ON u.[userid] = t.[userid] GROUP BY t.[userid], t.itemid HAVING t.itemid NOT IN (SELECT itemid FROM [RS].[transaction_tbl] WHERE [userid] = @UserID)) rt GROUP BY rt.[ItemID] ORDER BY ratemeans DESC ", parameters);
        }

        // V2.0 ----------------------------------------------------------------------------------------------------------------------------------------------------

        #region GUI-Sonnt
        public List<RecommdationSchedule> getSchedules()
        {
            string strSelect = "SELECT [ScheduleID],[StartTime],[StopTime],[Log] FROM [RS].[RECOMMENDATION_SCHEDULE_TBL] ORDER BY [StartTime] DESC";
            SqlDataReader dr = executeReader(strSelect);

            List<RecommdationSchedule> list = new List<RecommdationSchedule>();
            while (dr.Read())
            {
                RecommdationSchedule obj = new RecommdationSchedule();
                obj.ScheduleID = dr.GetInt32(dr.GetOrdinal("ScheduleID"));
                obj.Log = dr.GetString(dr.GetOrdinal("Log"));
                obj.StartTime = dr.GetDateTime(dr.GetOrdinal("StartTime"));
                obj.StopTime = (dr["StopTime"] == System.DBNull.Value) ? (DateTime?)null : dr.GetDateTime(dr.GetOrdinal("StopTime"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<string[]> getStaticsData()
        {
            List<string[]> list = new List<string[]>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string strSelect = "SELECT count(DISTINCT [UserID]) as NumOfUsers FROM [RS].[RECOMMENDATION_TBL]";
            SqlDataReader dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                string[] item = new string[2];
                item[0] = "Number of CLIENT who have recommendations:";
                item[1] = dr.GetInt32(dr.GetOrdinal("NumOfUsers")).ToString();
                list.Add(item);
            }
            dr.Close();

            strSelect = @"SELECT '\tBest Sellings:' RecommendType ,COUNT([UserID]) NumOfUsers FROM (SELECT DISTINCT [UserID] FROM [RS].[RECOMMENDATION_TBL] WHERE RecommendType = 'LRS01') R1 union SELECT '\tPurchased Items:' RecommendType ,COUNT([UserID]) NumOfUsers FROM (SELECT DISTINCT [UserID] FROM [RS].[RECOMMENDATION_TBL] WHERE RecommendType = 'LRS02') R2 union SELECT '\tNot Purchased Items:' RecommendType ,COUNT([UserID]) NumOfUsers FROM (SELECT DISTINCT [UserID] FROM [RS].[RECOMMENDATION_TBL] WHERE RecommendType = 'LRS03') R3 union SELECT '\tNew Items:' RecommendType ,COUNT([UserID]) NumOfUsers FROM (SELECT DISTINCT [UserID] FROM [RS].[RECOMMENDATION_TBL] WHERE RecommendType = 'LRS04') R4";
            dr = executeReader(strSelect, parameters);
            while (dr.Read())
            {
                string[] item = new string[2];
                item[0] = dr.GetString(dr.GetOrdinal("RecommendType"));
                item[1] = dr.GetInt32(dr.GetOrdinal("NumOfUsers")).ToString();
                list.Add(item);
            }
            dr.Close();

            /////

            strSelect = "SELECT count(DISTINCT [MetaItemID]) as NumOfMetaItem FROM [RS].[RECOMMENDATION_TBL]";
            dr = executeReader(strSelect, parameters);

            if (dr.Read())
            {
                string[] item = new string[2];
                item[0] = "Number of recommended META PRODUCT:";
                item[1] = dr.GetInt32(dr.GetOrdinal("NumOfMetaItem")).ToString();
                list.Add(item);
            }
            dr.Close();


            strSelect = "SELECT count(DISTINCT u.U_SubCategoryID) as NumOfGroups FROM [RS].[RECOMMENDATION_TBL] r INNER JOIN [RS].[USER_TBL] u ON r.UserID = u.UserID";
            dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                string[] item = new string[2];
                item[0] = "Number of CLIENT GROUPs:";
                item[1] = dr.GetInt32(dr.GetOrdinal("NumOfGroups")).ToString();
                list.Add(item);
            }
            dr.Close();

            strSelect = "SELECT count(DISTINCT u.ClusterID) as NumOfClusters FROM [RS].[RECOMMENDATION_TBL] r INNER JOIN [RS].[PARTION_TBL] u ON r.UserID = u.UserID";
            dr = executeReader(strSelect, parameters);
            if (dr.Read())
            {
                string[] item = new string[2];
                item[0] = "Number of CLIENT CLUSTERs:";
                item[1] = dr.GetInt32(dr.GetOrdinal("NumOfClusters")).ToString();
                list.Add(item);
            }
            dr.Close();

            return list;
        }

        public List<string> getRecommendTypes()
        {
            string strSelect = "SELECT DISTINCT [RecommendType] From [RS].[RECOMMENDATION_TBL] ORDER BY [RecommendType]";
            SqlDataReader dr = executeReader(strSelect);

            List<string> list = new List<string>();
            list.Add("All");
            while (dr.Read())
            {
                string type = dr.GetString(dr.GetOrdinal("RecommendType"));
                list.Add(type);
            }
            dr.Close();
            return list;
        }

        public List<Recommendation_Item> searchRecommendations(string UserID, string UserName, string RecommendType, bool TraditionalUser)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string strSelect;

            if(TraditionalUser)
                strSelect = "SELECT [RecomID], [ScheduleID], [MetaItemID], [UserID], [Quantity], [Score], [RecommendType] FROM [RS].[RECOMMENDATION_TBL] WHERE 1=1";
            else
                strSelect = "SELECT [RecomID], [ScheduleID], [MetaItemID], [U_SubCategoryID] as UserID, [Quantity], [Score], [RecommendType] FROM [RS].[RECOMMENDATION_NEWUSERS_TBL] WHERE 1=1";

            if (TraditionalUser)
            {
                if (UserID.Length > 0)
                {
                    parameters.Add("@UserID", UserID);
                    strSelect += " AND [UserID] like '%'+@UserID+'%'";
                }

                if (UserName.Length > 0)
                {
                    parameters.Add("@UserName", UserName);
                    strSelect += " AND [UserID] IN (SELECT UserID FROM [RS].[User_TBL] where [UserName] like '%'+ @UserName+'%')";
                }
            }
            else
            {
                if (UserID.Length > 0)
                {
                    parameters.Add("@UserID", UserID);
                    strSelect += " AND [U_SubCategoryID] like '%'+@UserID+'%'";
                }
            }

            if (RecommendType != "All")
            {
                parameters.Add("@RecommendType", RecommendType);
                strSelect += " AND [RecommendType] = @RecommendType";
            }
            
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Recommendation_Item> list = new List<Recommendation_Item>();
            int i = 0;
            while (dr.Read())
            {
                i++;
                Recommendation_Item obj = new Recommendation_Item();
                obj.RecomID = i;//dr.GetInt32(dr.GetOrdinal("RecomID"));
                obj.ScheduleID = dr.GetInt32(dr.GetOrdinal("ScheduleID"));
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("MetaItemID"));
                //obj.Price = dr.GetDouble(dr.GetOrdinal("Price"));
                obj.Quantity = dr.GetDouble(dr.GetOrdinal("Quantity"));
                obj.Score = dr.GetDouble(dr.GetOrdinal("Score"));
                obj.RecommendType = dr.GetString(dr.GetOrdinal("RecommendType"));
                //obj.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                //obj.MetaItemName = dr.GetString(dr.GetOrdinal("MetaItemName"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        #endregion

        #region SONNT-C7

        public void removeDuplicateC6andC7()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@R1", ConstantValues.RC_TYPE_LRS01);
            parameters.Add("@R2", ConstantValues.RC_TYPE_LRS02);
            executeNonQuery("DELETE [RS].[RECOMMENDATION_TBL] WHERE RecomID IN (SELECT DISTINCT a.RecomID FROM (SELECT RecomID,MetaItemID,UserID,ItemFamillyCode FROM [RS].[RECOMMENDATION_TBL] Where RecommendType = @R2) a LEFT JOIN (select RecomID,MetaItemID,UserID,ItemFamillyCode FROM [RS].[RECOMMENDATION_TBL] Where RecommendType = @R1) b ON a.MetaItemID = b.MetaItemID AND a.UserID = b.UserID AND a.ItemFamillyCode = b.ItemFamillyCode WHERE b.RecomID IS NOT NULL)", parameters);
        }

        /// <summary>
        /// Get list names of Cluster IDs
        /// </summary>
        /// <returns> List<string> of cluster IDs</String> </returns>

        public List<string> getListClusterIDs()
        {
            string strSelect = "SELECT [ClusterID] FROM [RS].[USER_CLUSTER_TBL]";
            SqlDataReader dr = executeReader(strSelect);

            List<string> list = new List<string>();
            while (dr.Read())
            {
                string obj = string.Empty;
                obj = dr.GetString(dr.GetOrdinal("ClusterID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<TransactionData> getTransactionByClusterID(string clusterID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clusterID", clusterID);
            string strSelect = "SELECT u.[UserID], m.[MetaItemID], SUM(t.[Quantity]) as Quantity ,t.[Date] FROM [RS].[TRANSACTION_TBL] t INNER JOIN RS.ITem_TBL m ON t.ItemID = m.ItemID INNER JOIN [RS].[USER_TBL] u ON t.UserID = u.UserID INNER JOIN [RS].[PARTION_TBL] c ON c.UserID = u.UserID WHERE c.ClusterID = @clusterID GROUP BY u.[UserID], m.[MetaItemID], t.[Date] ORDER BY [UserID],[Date] ASC";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<TransactionData> list = new List<TransactionData>();
            while (dr.Read())
            {
                TransactionData obj = new TransactionData();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("MetaItemID"));
                obj.quantity = dr.GetInt32(dr.GetOrdinal("Quantity"));
                obj.t_date = dr.GetDateTime(dr.GetOrdinal("Date"));
                
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<MatrixItem> getWeighMatrixByClusterID(string clusterID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clusterID", clusterID);
            string strSelect = "SELECT [MetaItemSource],[MetaItemDestination],[WEIGHT] FROM [RS].[WEIGHT_MATRIX] WHERE [ClusterID] =@clusterID";
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<MatrixItem> list = new List<MatrixItem>();
            while (dr.Read())
            {
                MatrixItem obj = new MatrixItem();
                obj.Row = dr.GetString(dr.GetOrdinal("MetaItemSource"));
                obj.Column = dr.GetString(dr.GetOrdinal("MetaItemDestination"));
                obj.Cell = dr.GetDouble(dr.GetOrdinal("WEIGHT"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }


        #endregion

        public void CLEAN_CONF_Matrix()
        {
            executeNonQuery("truncate table [RS].[CONFIDENT_MATRIX]");
        }

        public void CLEAN_WEIG_Matrix()
        {
            executeNonQuery("truncate table [RS].[WEIGHT_MATRIX]");
        }

        public void CLEAN_DIST_Matrix()
        {
            executeNonQuery("truncate table [RS].[DISTANCE_MATRIX]");
        }

        public void setCONF_Matrix(MatrixItem matrixItem)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", matrixItem.ClusterID);
            parameters.Add("@MetaItemSource", matrixItem.Row);
            parameters.Add("@MetaItemDestination", matrixItem.Column);
            parameters.Add("@Confident", matrixItem.Cell);
            executeNonQuery("INSERT INTO [RS].[CONFIDENT_MATRIX] ([ClusterID] ,[MetaItemSource] ,[MetaItemDestination] ,[Confident]) VALUES (@ClusterID ,@MetaItemSource ,@MetaItemDestination ,@Confident)", parameters);
        }

        public void setWEIG_Matrix(MatrixItem matrixItem)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", matrixItem.ClusterID);
            parameters.Add("@MetaItemSource", matrixItem.Row);
            parameters.Add("@MetaItemDestination", matrixItem.Column);
            parameters.Add("@WEIGHT", matrixItem.Cell);
            executeNonQuery("INSERT INTO [RS].[WEIGHT_MATRIX] ([ClusterID] ,[MetaItemSource] ,[MetaItemDestination] ,[WEIGHT]) VALUES (@ClusterID ,@MetaItemSource ,@MetaItemDestination ,@WEIGHT)", parameters);
        }

        public void setDIST_Matrix(MatrixItem matrixItem)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", matrixItem.ClusterID);
            parameters.Add("@MetaItemSource", matrixItem.Row);
            parameters.Add("@MetaItemDestination", matrixItem.Column);
            parameters.Add("@Distance", matrixItem.Cell);
            executeNonQuery("INSERT INTO [RS].[DISTANCE_MATRIX] ([ClusterID] ,[MetaItemSource] ,[MetaItemDestination] ,[Distance]) VALUES (@ClusterID ,@MetaItemSource ,@MetaItemDestination ,@Distance)", parameters);
        }

        public List<Transac> getTransac_ForMatrix_ByClusterID_V2(string ClusterID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", ClusterID);

            //string strSelect = "SELECT [RS].[TRANSACTION_TBL].[UserID] ,SUM([Quantity]) as 'Quantity' ,[MetaItemID] FROM [RS].[TRANSACTION_TBL], [RS].PARTION_TBL , [RS].ITEM_TBL WHERE [RS].PARTION_TBL.[UserID] = [RS].[TRANSACTION_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].PARTION_TBL.ClusterID = @ClusterID GROUP BY [RS].[TRANSACTION_TBL].[UserID], [MetaItemID] ";

            string strSelect = "SELECT [RS].[TRANSACTION_TBL].[UserID] ,AVG([Quantity]) as QAVG ,[MetaItemID] FROM [RS].[TRANSACTION_TBL], [RS].PARTION_TBL , [RS].ITEM_TBL WHERE [RS].PARTION_TBL.[UserID] = [RS].[TRANSACTION_TBL].[UserID] and [RS].[TRANSACTION_TBL].ItemID = [RS].ITEM_TBL.ItemID and [RS].PARTION_TBL.ClusterID = @ClusterID GROUP BY [RS].[TRANSACTION_TBL].[UserID], [MetaItemID] ";


            SqlDataReader dr = executeReader(strSelect, parameters);

            List<Transac> list = new List<Transac>();
            while (dr.Read())
            {
                Transac obj = new Transac();
                obj.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Quantity = dr.GetInt32(dr.GetOrdinal("QAVG"));
                obj.MetaItemID = dr.GetString(dr.GetOrdinal("MetaItemID"));
                list.Add(obj);
            }
            dr.Close();
            return list;
        }

        public List<string> getAllClusterID()
        {
            string strSelect = "SELECT [ClusterID] FROM [RS].[USER_CLUSTER_TBL]";
            SqlDataReader dr = executeReader(strSelect);
            List<string> lst = new List<string>();
            while (dr.Read())
            {
                lst.Add(dr.GetString(dr.GetOrdinal("ClusterID")));
            }

            dr.Close();
            return lst;
        }

        //------------------------------------------------------------------------------------------------------
        //This function is considered carefully by MC. NGUYEN, on 21.10.2014, I think it do not have any error.
        //------------------------------------------------------------------------------------------------------
        public List<MatrixItem> getRattingMatrixItem_ByClusterID(string ClusterID)
        {
            string strSelect = "SELECT [UserID], [MetaItemID], [Value] FROM [RS].[RATING_MATRIX] WHERE ClusterID = @ClusterID";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@ClusterID", ClusterID);
            SqlDataReader dr = executeReader(strSelect, parameters);

            List<MatrixItem> lst = new List<MatrixItem>();
            while (dr.Read())
            {
                MatrixItem obj = new MatrixItem();
                obj.Row = dr.GetString(dr.GetOrdinal("UserID"));
                obj.Column = dr.GetString(dr.GetOrdinal("MetaItemID"));
                obj.Cell = dr.GetDouble(dr.GetOrdinal("Value"));
                lst.Add(obj);
            }
            dr.Close();
            return lst;
        }

        //public void addRecommdationSchedule(RecommdationSchedule schedule)
        //{
        //    string strQuery = "INSERT INTO [RS].[RECOMMENDATION_SCHEDULE_TBL] ([StartTime] ,[Log] ,[LoginID]) VALUES ( GETDATE(), @Log , @LoginID)";
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@Log", schedule.Log);
        //    parameters.Add("@LoginID", schedule.LoginID);
        //    executeNonQuery(strQuery, parameters);
        //}
        public RecommdationSchedule addRecommdationSchedule(RecommdationSchedule schedule) //SONNT: has modified
        {
/////////////////////
            // error here. not found RECOMMENDATION_SCHEDULE_TBL. so. we need to repair RECOMMENDATION_SCHEDULE_TBL table or add in db
            ///////////////
            string strQuery = "INSERT INTO [RS].[RECOMMENDATION_SCHEDULE_TBL] ([StartTime] ,[Log] ,[LoginID]) VALUES ( GETDATE(), @Log , @LoginID)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Log", schedule.Log);
            parameters.Add("@LoginID", schedule.LoginID);
            executeNonQuery(strQuery, parameters);
            //query the schedule also
            strQuery = "SELECT TOP 1 [ScheduleID],[StartTime],[StopTime],[Log],[LoginID] FROM [RS].[RECOMMENDATION_SCHEDULE_TBL] ORDER BY [StartTime] DESC";
            SqlDataReader dr = executeReader(strQuery);
            if (dr.Read())
            {
                schedule.ScheduleID = dr.GetInt32(dr.GetOrdinal("ScheduleID"));
            }
            dr.Close();
            return schedule;
        }
        public void updateRecommdationSchedule(RecommdationSchedule schedule)
        {
            string strQuery = "UPDATE [RS].RECOMMENDATION_SCHEDULE_TBL SET [StopTime] = GETDATE(),[Log] = @Log WHERE [ScheduleID] in (SELECT TOP 1 [ScheduleID] FROM [RS].RECOMMENDATION_SCHEDULE_TBL ORDER BY [StartTime] DESC) ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Log", schedule.Log);
            executeNonQuery(strQuery, parameters);
        }

        public void CLEAN_RECOMMENDATION()
        {
            executeNonQuery("truncate table [RS].[PRICE_RECOM_TBL]");
            executeNonQuery("truncate table [RS].[PRICE_RECOM_NEWUSERS_TBL]");
            executeNonQuery("truncate table [RS].[RECOMMENDATION_TBL]");
            executeNonQuery("truncate table [RS].[RECOMMENDATION_NEWUSERS_TBL]");
        }

    }
}