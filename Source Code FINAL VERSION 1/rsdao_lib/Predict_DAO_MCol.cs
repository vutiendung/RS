using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsbo_lib;
using System.Data.SqlClient;
using System.Data;
using rsglobal_lib;
using System.Globalization;

namespace rsdao_lib
{
    public class Predict_DAO_MCol : Predict_DAO
    {
        public Predict_DAO_MCol() : base() { }

        #region get recommendations R1 - C6 - MC

        public List<Recommendation_Meta_Item> GetRecommendations(string sql)
        {

            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                Recommendation_Meta_Item item = new Recommendation_Meta_Item();
                item.UserName = dr.GetString(dr.GetOrdinal("UserName"));
                item.UserID = dr.GetString(dr.GetOrdinal("UserID"));
                item.MetaItemID = dr.GetString(dr.GetOrdinal("ItemID"));
                item.MetaItemName = dr.GetString(dr.GetOrdinal("MetaItemName"));
                item.Quantity = dr.GetDouble(dr.GetOrdinal("Quantity"));

                list.Add(item);
            }
            dr.Close();
            return list;
        }

        public List<ClusterInfo> GetListCluster()
        {
            List<ClusterInfo> list = new List<ClusterInfo>();
            string sql = "SELECT clusterID FROM [RS].[USER_CLUSTER_TBL]";
            SqlDataReader dr = executeReader(sql);
            ClusterInfo cluster = new ClusterInfo();
            while (dr.Read())
            {
                cluster.ClusterID = dr.GetString(dr.GetOrdinal("ClusterID"));
                list.Add(cluster);
            }
            dr.Close();
            return list;
        }

        public List<Recommendation_Meta_Item> GetRecommendC6_ForTraditionalUser(string u, int k)
        {
            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            SqlDataReader sqlDataReader = this.executeReader("SELECT TOP " + (object)k + " A.MetaItemID, A.Q * RS.CLUSTER_SCORE_C6.Q as SCORE FROM (SELECT ClusterID, MetaItemID, Q FROM RS.ALL_TRANSACTION WHERE UserID = '" + u + "') AS A, RS.CLUSTER_SCORE_C6 WHERE RS.CLUSTER_SCORE_C6.MetaItemID = A.MetaItemID AND RS.CLUSTER_SCORE_C6.Clusterid = A.ClusterID ORDER BY SCORE DESC");
            while (sqlDataReader.Read())
                list.Add(new Recommendation_Meta_Item()
                {
                    UserID = u,
                    RecommendType = ConstantValues.RC_TYPE_LRS01,
                    MetaItemID = sqlDataReader.GetString(sqlDataReader.GetOrdinal("MetaItemID")),
                    Quantity = 0.0,
                    Score = Convert.ToDouble(sqlDataReader[1])
                });
            sqlDataReader.Close();
            return list;
        }

        public List<Recommendation_Meta_Item> GetRecommendC6_ForSubCategory(string SubCate, int k)
        {
            string sql;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@k", k.ToString());
            parameters.Add("@SubCate", SubCate);

            // Recommendation_Meta_Item list = new Recommendation_Meta_Item();
            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            Recommendation_Meta_Item item;


            //-----------------------------------------------
            sql = "SELECT TOP " + k + " [RS].SCORE_GLOBAL_C6.MetaItemID, SCORE FROM [RS].SCORE_GLOBAL_C6 WHERE [RS].SCORE_GLOBAL_C6.MetaItemID " +
                  "IN(SELECT DISTINCT MetaItemID FROM [RS].ALL_TRANSACTION WHERE UserID IN(SELECT UserID From RS.USER_TBL WHERE U_SubCategoryID = @SubCate)) ORDER BY SCORE DESC";

            SqlDataReader dr = executeReader(sql, parameters);
            bool Check = dr.HasRows;

            while (dr.Read())
            {
                item = new Recommendation_Meta_Item();
                item.UserID = SubCate;//Luu tam thoi
                item.RecommendType = ConstantValues.RC_TYPE_LRS01;// "Best Selling";
                item.MetaItemID = dr[0].ToString();
                item.Quantity = 0;
                item.Score = Convert.ToDouble(dr[1]);
                list.Add(item);
            }
            dr.Close();

            if (!Check)
            {
                sql = "SELECT TOP " + k + " [RS].[CLUSTER_SCORE_C6].MetaItemID, Q " +
                      "FROM [RS].[CLUSTER_SCORE_C6] " +
                      "WHERE [RS].[CLUSTER_SCORE_C6].MetaItemID IN(SELECT DISTINCT MetaItemID FROM [RS].ALL_TRANSACTION WHERE UserID IN( " +
                      "SELECT UserID From RS.USER_TBL WHERE U_SubCategoryID = @SubCate)) ORDER BY Q DESC";

                SqlDataReader dr2 = executeReader(sql, parameters);
                while (dr2.Read())
                {
                    item = new Recommendation_Meta_Item();
                    item.UserID = SubCate;//Luu tam thoi
                    item.RecommendType = ConstantValues.RC_TYPE_LRS01;//"Best Selling";
                    item.MetaItemID = dr2[0].ToString();
                    // item.MetaItemName = dr2[1].ToString();
                    item.Quantity = 0;
                    item.Score = Convert.ToDouble(dr2[1]);
                    list.Add(item);
                }
                dr2.Close();
            }
            return list;
        }

        public List<string> getListSubCategory_ForNewUsers()
        {
            string item;
            List<string> list = new List<string>();
            string sql = "Select DISTINCT U_SubCategoryID from RS.USER_TBL Where UserID Not in (Select DISTINCT UserID From RS.Transaction_tbl)";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                item = dr.GetString(dr.GetOrdinal("U_SubCategoryID"));
                list.Add(item);
            }
            dr.Close();
            return list;
        }

        public Int32 GetQualtityTraditionalUser(Recommendation_Meta_Item RC)
        {

            int QTY = 0;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", RC.UserID);
            parameters.Add("@MetaItemID", RC.MetaItemID);

            string sql = "SELECT AVG([QAV]*[WEIGHT]) AS QTY " +
                         "FROM  [RS].[ALL_TRANSACTION],  [RS].[WEIGHT_MATRIX] " +
                         "WHERE MetaItemDestination =@MetaItemID AND [MetaItemSource]  = [MetaItemID] AND [WEIGHT_MATRIX].ClusterID = [ALL_TRANSACTION].ClusterID AND [UserID] = @UserID";

            SqlDataReader dr = executeReader(sql, parameters);


            while (dr.Read())
            {
                if (!dr.IsDBNull(0))
                {
                    double WEI = Convert.ToDouble(dr[0], CultureInfo.CreateSpecificCulture("en-GB"));
                    QTY = (int)WEI;

                }
            }
            dr.Close();
            //----------------------------------
            if (QTY == 0)
            {
                sql = "SELECT AVG([QAV]) AS QTY " +
                         "FROM  [RS].[ALL_TRANSACTION] " +
                         "WHERE [MetaItemID]  = @MetaItemID AND [UserID] = @UserID";

                SqlDataReader dr1 = executeReader(sql, parameters);

                while (dr1.Read())
                {
                    if (!dr1.IsDBNull(0))
                    {
                        double WEI = Convert.ToDouble(dr1[0], CultureInfo.CreateSpecificCulture("en-GB"));
                        QTY = (int)WEI;
                    }
                }
                dr1.Close();
            }
            //--------------------------------
            return QTY;
        }

        public double GetQualtityNewUser(Recommendation_Meta_Item RC)
        {
            double QAV = 2;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@UserID", RC.UserID);
            parameters.Add("@MetaItemID", RC.MetaItemID);

            string sql = "SELECT QAV FROM [RS].[QTY_PAR_SUBCATEGORY] WHERE [MetaItemID] = @MetaItemID AND [U_SUBCATEGORYID] = (SELECT U_SUBCATEGORYID FROM  [RS].USER_TBL WHERE USERID = @UserID)";
            SqlDataReader dr = executeReader(sql, parameters);

            while (dr.Read())
            {
                QAV = Convert.ToDouble(dr[0]);
                if (QAV < 0)
                    QAV = 1;
            }
            dr.Close();
            return QAV;
        }

        public List<Recommendation_Meta_Item> GetAllQuantity(List<Recommendation_Meta_Item> list)
        {
            List<U_I_Q> lstSubCate = new List<U_I_Q>();
            List<U_I_Q> lstcate = new List<U_I_Q>();
            U_I_Q item;

            string sql = "SELECT U_SubCategoryID, MetaItemID, QAV FROM [RS].[QTY_PAR_SUBCATEGORY]";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                item = new U_I_Q();
                item.UserID = dr[0].ToString();
                item.MetaItemID = dr[1].ToString();
                item.QTY = Convert.ToDouble(dr[2]);
                lstSubCate.Add(item);
            }
            dr.Close();

            dr = executeReader("SELECT U_CategoryID, MetaItemID, QAV FROM [RS].[QTY_PAR_CATEGORY]");
            while (dr.Read())
            {
                item = new U_I_Q();
                item.UserID = dr[0].ToString();
                item.MetaItemID = dr[1].ToString();
                item.QTY = Convert.ToDouble(dr[2]);
                lstcate.Add(item);
            }
            dr.Close();

            foreach (Recommendation_Meta_Item R in list)
            {
                dr = executeReader("Select UserID, U_SubcategoryID From [RS].[User_TBL] Where U_SubCategoryID = '" + R.UserID + "'");
                dr.Read(); string Ucate = dr[1].ToString();
                dr.Close();
                bool Check = false;
                for (int j = 0; j < lstSubCate.Count; j++)
                {
                    if (lstSubCate[j].UserID.Equals(Ucate))
                    {
                        R.Quantity = lstSubCate[j].QTY;
                        Check = true;
                        break;
                    }
                }
                //if Sub rong
                if (!Check)
                {
                    dr = executeReader("Select UserID, U_categoryID From [RS].[User_TBL] Where UserID = '" + R.UserID + "'");
                    dr.Read(); string USub = dr[1].ToString();
                    dr.Close();

                    for (int j = 0; j < lstcate.Count; j++)
                    {
                        if (lstcate[j].UserID.Equals(USub))
                        {
                            R.Quantity = lstcate[j].QTY;
                            break;
                        }
                    }
                }
            }
            return list;
        }

        #endregion

        #region get recommendations R3 - C8 - MC

        public void RemoveDuplicateR3R1()
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@R1", ConstantValues.RC_TYPE_LRS01);
            parameters.Add("@R2", ConstantValues.RC_TYPE_LRS03);
            executeNonQuery("DELETE [RS].[RECOMMENDATION_TBL] WHERE RecomID IN (SELECT DISTINCT a.RecomID FROM (SELECT RecomID,MetaItemID,UserID,ItemFamillyCode FROM [RS].[RECOMMENDATION_TBL] Where RecommendType = @R2) a LEFT JOIN (select RecomID,MetaItemID,UserID,ItemFamillyCode FROM [RS].[RECOMMENDATION_TBL] Where RecommendType = @R1) b ON a.MetaItemID = b.MetaItemID AND a.UserID = b.UserID AND a.ItemFamillyCode = b.ItemFamillyCode WHERE b.RecomID IS NOT NULL)", parameters);
        }
        public List<Recommendation_Meta_Item> GetRecommendationR3()
        {
            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();

            return list;
        }


        public List<User> GetListTraditionalUsers()
        {
            List<User> list = new List<User>();
            User U = null;

            string sql = "Select DISTINCT UserID, U_SUBCATEGORYID from [RS].User_TBL";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                U = new User();
                U.UserID = dr[0].ToString();
                U.UserName = dr[1].ToString();
                U.U_SubCategoryID = dr[2].ToString();
                list.Add(U);
            }
            return list;
        }

        //-----------------------------------------------------------------------------------------
        //This function gets a list of all clusters
        //-----------------------------------------------------------------------------------------
        public List<string> GetListClusterID()
        {
            List<string> list = new List<string>();

            SqlDataReader dr = executeReader("SELECT ClusterID FROM [RS].[USER_CLUSTER_TBL]");
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }

        public List<CONF> GetCONF_OfCluster(string cluster)
        {
            List<CONF> list = new List<CONF>();
            CONF item;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);

            SqlDataReader dr = executeReader("SELECT MetaItemSource, MetaItemDestination, Confident FROM [RS].CONFIDENT_MATRIX WHERE CLusterID = @cluster", parameters);
            while (dr.Read())
            {
                item = new CONF();
                item.ClusterID = cluster;
                item.MetaItemSource = dr[0].ToString();
                item.MetaItemDestination = dr[1].ToString();
                item.Confident = Convert.ToDouble(dr[2]);
                list.Add(item);
            }
            dr.Close();
            return list;
        }

        public List<DIST> GetDIST_OfCluster(string cluster)
        {
            List<DIST> list = new List<DIST>();
            DIST item;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            SqlDataReader dr = executeReader("SELECT MetaItemSource, MetaItemDestination, Distance FROM [RS].DISTANCE_MATRIX WHERE CLusterID = @cluster", parameters);
            while (dr.Read())
            {
                item = new DIST();
                item.ClusterID = cluster;
                item.MetaItemSource = dr[0].ToString();
                item.MetaItemDestination = dr[1].ToString();
                item.Distance = Convert.ToDouble(dr[2]);
                list.Add(item);
            }
            dr.Close();
            return list;
        }

        public List<string> GetListMetaItemPurchased_OfCluster(string cluster)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            SqlDataReader dr = executeReader("SELECT DISTINCT MetaItemID FROM [RS].ALL_TRANSACTION WHERE CLusterID = @cluster", parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }


        public List<string> GetListUser_OfCluster(string cluster)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            SqlDataReader dr = executeReader("SELECT DISTINCT UserID FROM [RS].[PARTION_TBL] WHERE ClusterID = @cluster", parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }


        public List<string> GetListMetaItem_OfUser(string user)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@user", user);
            SqlDataReader dr = executeReader("SELECT DISTINCT MetaItemID FROM [RS].[ALL_TRANSACTION] WHERE UserID = @user", parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }

        public List<string> GetListMetaItemNotPurchased_OfUser(string user, string cluster)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            parameters.Add("@user", user);
            SqlDataReader dr = executeReader("SELECT DISTINCT MetaItemID FROM [RS].ALL_TRANSACTION WHERE CLusterID = @cluster AND UserID != @user", parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }

        public List<U_I_Q> GetQuantityTraditionalUsers_More(string cluster)
        {
            List<U_I_Q> list = new List<U_I_Q>();
            U_I_Q item;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            string sql = "SELECT MetaItemID, QAV from [RS].[QTY_PAR_CLUSTER] WHERE ClusterID = @cluster";
            SqlDataReader dr = executeReader(sql, parameters);
            while (dr.Read())
            {
                item = new U_I_Q();
                item.MetaItemID = dr[0].ToString();
                item.QTY = Convert.ToDouble(dr[1]);
                if (item.QTY < 0) item.QTY = 1;

                list.Add(item);
            }
            dr.Close();
            return list;
        }



        #endregion

        #region Get recommendations R4 - MC

        public Dictionary<string, double> GetListRateU(string cluster)
        {
            Dictionary<string, double> list = new Dictionary<string, double>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            string sql = "Select UserID, SUM_RATE FROM [RS].SUM_RATE_U WHERE CLusterID = @cluster";
            SqlDataReader dr = executeReader(sql, parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString(), Convert.ToDouble(dr[1]));
            }
            dr.Close();
            return list;
        }

        public Dictionary<string, double> GetListRateF(string cluster)
        {
            Dictionary<string, double> list = new Dictionary<string, double>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@cluster", cluster);
            string sql = "Select ItemFamillyCode, SUM_RATE FROM [RS].SUM_RATE_F WHERE CLusterID = @cluster";
            SqlDataReader dr = executeReader(sql, parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString(), Convert.ToDouble(dr[1]));
            }
            dr.Close();
            return list;
        }

        public Dictionary<string, double> GetListRateUF(string user)
        {
            Dictionary<string, double> list = new Dictionary<string, double>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@user", user);
            string sql = "Select ItemFamillyCode, SUM_RATE FROM [RS].SUM_RATE_UF WHERE UserID = @user";
            SqlDataReader dr = executeReader(sql, parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString(), Convert.ToDouble(dr[1]));
            }
            dr.Close();
            return list;
        }

        public Dictionary<string, string> GetListNewItem()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            string sql = "SELECT MetaItemID, ItemfamillyCode FROM [RS].NEW_META_ITEM";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                if (dr[0].ToString() != "")
                    list.Add(dr[0].ToString(), dr[1].ToString());
            }
            dr.Close();
            return list;
        }

        public int getQTYR4(string user, string FamillyCode, int Type)
        {
            int QTY = 1;
            string sql;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@user", user);
            parameters.Add("@famillycode", FamillyCode);


            if (Type == 0)
                sql = "SELECT  QAV FROM [RS].QTY_PAR_FF WHERE U_SUBCATEGORYID = @user AND ItemFamillyCode = @famillycode";
            else
                sql = "SELECT QAV FROM [RS].QTY_PAR_UF WHERE UserID = @user AND ItemFamillyCode = @famillycode";

            SqlDataReader dr = executeReader(sql, parameters);

            while (dr.Read())
            {
                if (dr[0].ToString() != "")
                    QTY = Convert.ToInt32(dr[0]);
                else
                    QTY = 1;
            }
            dr.Close();
            return QTY;
        }

        public string GetSubCategory_ForUser(string user)
        {
            string subcategory = "";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@user", user);
            SqlDataReader dr = executeReader("SELECT U_SUBCATEGORYID FROM RS.USER_TBL WHERE UserID = @user", parameters);
            while (dr.Read())
                subcategory = dr[0].ToString();
            dr.Close();
            return subcategory;
        }

        public Recommendation_Meta_Item getCandidateForUserSubCategory_R4(string u_subcategory)
        {
            Recommendation_Meta_Item item = new Recommendation_Meta_Item();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@u_subcategory", u_subcategory);

            string sql = "SELECT TOP 1 U_SUBCATEGORYID, ITEMFAMILLYCODE, RAV as SCORE FROM [RS].SCORE_GLOBAL_C9 where U_SUBCATEGORYID = @u_subcategory " +
                         " AND ITEMFAMILLYCODE IN(SELECT DISTINCT ITEMFAMILLYCODE FROM [RS].NEW_META_ITEM) OrDER BY SCORE DESC";
            SqlDataReader dr = executeReader(sql, parameters);

            while (dr.Read())
            {
                item.UserID = dr[0].ToString();//luu U_SubcategoryID
                item.MetaItemID = dr[1].ToString();//luu famillycode
                item.Score = Convert.ToDouble(dr[2]);
            }
            dr.Close();
            return item;
        }

        public Dictionary<string, string> getUser_SubCategory_FORNEWUSERS()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            SqlDataReader dr = executeReader("SELECT U_SUBCATEGORYID, USERID FROM [RS].NEW_USERS");
            while (dr.Read())
                if (!dr.IsDBNull(1)) list.Add(dr[1].ToString(), dr[0].ToString());
            dr.Close();

            return list;
        }

        public List<string> getListSubCategoryUser()
        {
            List<string> list = new List<string>();
            SqlDataReader dr = executeReader("Select DISTINCT U_SUBCATEGORYID FROM [RS].[New_Users]");
            while (dr.Read())
                list.Add(dr[0].ToString());
            dr.Close();
            return list;
        }

        public List<string> getListNewUser_OfSub(string SubCategory)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@subcategory", SubCategory);
            SqlDataReader dr = executeReader("Select DISTINCT UseriD FROM [RS].[NEW_Users] WHERE U_SUBCATEGORYID = @subcategory", parameters);
            while (dr.Read())
                list.Add(dr[0].ToString());
            dr.Close();
            return list;
        }

        public List<string> GetListMetaItem_Of(string FamillyID)
        {
            List<string> list = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@famillyID", FamillyID);
            SqlDataReader dr = executeReader("Select DISTINCT MetaItemID from [RS].[NEW_META_ITEM] WHERE [ItemFamillyCode] = @famillyID", parameters);
            while (dr.Read())
            {
                list.Add(dr[0].ToString());
            }
            dr.Close();
            return list;
        }

        #endregion

        #region public - MC

        public List<Recommendation_Setting> GetRecommendationSetting()
        {
            List<Recommendation_Setting> list = new List<Recommendation_Setting>();
            Recommendation_Setting item = null;
            string str = "SELECT [Key], [DataType],[Description] FROM [RS].[RECOMMEND_SETTING_TBL]";
            SqlDataReader dr = executeReader(str);
            while (dr.Read())
            {
                item = new Recommendation_Setting();
                item.Key = dr[0].ToString();
                item.DataType = dr[1].ToString();
                item.Description = dr[2].ToString();
                item.Values = null;
                list.Add(item);
            }
            dr.Close();
            //-------------------------------
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("@Key1", list[i].Key);
                    Recommend_Setting_Value it = null;
                    list[i].Values = new List<Recommend_Setting_Value>();
                    str = "SELECT [Key], [Value], [isDefault] FROM [RS].[RECOMMEND_SETTING_VALUE_TBL] WHERE [Key] = @Key1";
                    SqlDataReader dr1 = executeReader(str, parameters);
                    while (dr1.Read())
                    {
                        it = new Recommend_Setting_Value();
                        it.Key = dr1[0].ToString();
                        it.Value = dr1[1].ToString();
                        it.isDedault = Convert.ToBoolean(dr1[2]);
                        list[i].Values.Add(it);
                    }
                    dr1.Close();
                }
            }

            return list;
        }

        public void InsertList(List<Recommendation_Meta_Item> list, RecommdationSchedule schedule)
        {
            try
            {
                int i = 0;
                foreach (Recommendation_Meta_Item RCItem in list)
                {
                    string str = i.ToString();

                    string[] MetaID = RCItem.MetaItemID.Split('_');
                    RCItem.MetaItemID = MetaID[0];
                    RCItem.ItemFamillyCode = MetaID[1];
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("@F1", schedule.ScheduleID);
                    parameters.Add("@F2", RCItem.Quantity.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                    parameters.Add("@F3", RCItem.Score.ToString(CultureInfo.CreateSpecificCulture("en-GB")));

                    command.Parameters.Clear();
                    foreach (KeyValuePair<string, object> pair in parameters)
                    {
                        command.Parameters.AddWithValue(pair.Key, pair.Value);
                    }

                    command.CommandText = "INSERT INTO [RS].[RECOMMENDATION_TBL] ([ScheduleID] ,[MetaItemID] ,[UserID] ,[Quantity] ,[Score] ,[RecommendType],[ItemFamillyCode])  VALUES (" +
                    "@F1, '" + RCItem.MetaItemID + "', '" + RCItem.UserID + "',@F2, @F3,'" + RCItem.RecommendType + "','" + RCItem.ItemFamillyCode + "')";
                    int nbRow = command.ExecuteNonQuery();

                    i++;
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public void InsertListNewUser(List<Recommendation_Meta_Item> list, RecommdationSchedule schedule)
        {
            try
            {
                int i = 0;
                foreach (Recommendation_Meta_Item item in list)
                {

                    string str = i.ToString();
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    string[] MetaID = item.MetaItemID.Split('_');
                    item.MetaItemID = MetaID[0];
                    item.ItemFamillyCode = MetaID[1];

                    parameters.Add("@F1", schedule.ScheduleID);
                    parameters.Add("@F2", item.Quantity.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                    parameters.Add("@F3", item.Score.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                    command.Parameters.Clear();
                    foreach (KeyValuePair<string, object> pair in parameters)
                    {
                        command.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    command.CommandText = "INSERT INTO [RS].[RECOMMENDATION_NEWUSERS_TBL]([ScheduleID],[MetaItemID],[U_SubCategoryID],[Quantity],[Score],[RecommendType],[ItemFamillyCode]) " +
                                          "VALUES (@F1, '" + item.MetaItemID + "','" + item.UserID + "',@F2, @F3, '" + item.RecommendType + "','" + item.ItemFamillyCode + "')";
                    int nbRow = command.ExecuteNonQuery();
                    i++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        #endregion


        public double GetPrice(string MetaItemID, double QTY)
        {
            double Price = 0;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@MetaItem", MetaItemID);
            parameters.Add("@QTY", QTY.ToString());

            string str = "SELECT TOP 1 [Unit Price] FROM [TOULEMB_V3].[dbo].[TOULEMBAL$Sales Price] where [Item No_] = @MetaItem AND [Minimum Quantity] <= @QTY ORDER BY [Unit Price] ASC";
            SqlDataReader dr = executeReader(str, parameters);
            while (dr.Read())
                Price = Convert.ToDouble(dr[0]);
            dr.Close();

            if (Price == 0)
            {
                str = "SELECT [Unit Price] FROM  [TOULEMB_V3].[dbo].[TOULEMBAL$Sales Price] WHERE [Unit Price] = (SELECT Max([Unit Price]) FROM [TOULEMB_V3].[dbo].[TOULEMBAL$Sales Price] WHERE [Item No_] = @MetaItem)";
                SqlDataReader dr2 = executeReader(str, parameters);
                while (dr2.Read())
                    Price = Convert.ToDouble(dr2[0]);
                dr2.Close();
            }
            return Price;

        }
        public List<string> GetAllSubCate()
        {
            List<string> lst = new List<string>();
            SqlDataReader dr = executeReader("Select Distinct U_SubcategoryID From [RS].User_TBL");
            while (dr.Read())
                lst.Add(dr[0].ToString());
            dr.Close();

            return lst;
        }

        public void GetPrice()
        {
            try
            {
                string str = "DELETE FROM [RS].[PRICE_RECOM_TBL]";
                executeNonQuery(str);

                str = "DELETE FROM [RS].[PRICE_RECOM_NEWUSERS_TBL]";
                executeNonQuery(str);

                str = "INSERT INTO [RS].[PRICE_RECOM_TBL]([RecomID],[ItemID],[Price]) SELECT RecomID, ItemID, Price FROM RS.GET_PRICE_FINAL";
                executeNonQuery(str);

                str = "INSERT INTO [RS].[PRICE_RECOM_NEWUSERS_TBL]([RecomID],[ItemID],[Price]) SELECT RecomID, ItemID, Price FROM RS.GET_PRICE_NEWUSERS_FINAL";
                executeNonQuery(str);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Dictionary<string, double> getUCA()
        {
            Dictionary<string, double> lst = new Dictionary<string, double>();
            string str = "SELECT UserID, CA_EUROS FROM [RS].[User_TBL] WHERE CA_EUROS != 0";
            SqlDataReader dr = executeReader(str);
            double MEANS = 0.0;

            while (dr.Read())
            {
                lst.Add(dr[0].ToString(), Convert.ToDouble(dr[1]));
                MEANS += Convert.ToDouble(dr[1]);

            }
            if (lst.Count != 0)
            {
                double M = MEANS / lst.Count;
                lst.Add("MEANS_VALUE", M);

            }
            dr.Close();

            return lst;
        }


        public List<ADDGAP> GetListOfNewGAP()
        {
            List<ADDGAP> lst = new List<ADDGAP>();
            string sql = "Select DISTINCT UserID, MetaItemID, A, B, QTY_GAP from [RS].[QUERY_QTY_GAP]";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                ADDGAP item = new ADDGAP();
                item.UserID = dr[0].ToString();
                item.MetaItemID = dr[1].ToString();
                item.QTY_RECOM = Convert.ToInt32(dr[3]);
                item.QTY_REAL = Convert.ToInt32(dr[2]);
                item.GAP_ADDITION = Convert.ToDouble(dr[4]);
                lst.Add(item);
            }
            dr.Close();
            return lst;
        }


        //This function update the GAP of Quantity
        //Checked: all correct.

        public void UpdateGAP(List<ADDGAP> list)
        {
            string sql = "";
            foreach (ADDGAP item in list)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@UserID", item.UserID);
                parameters.Add("@MetaItemID", item.MetaItemID);
                parameters.Add("@QTY_RECOM", item.QTY_RECOM.ToString());
                parameters.Add("@QTY_REAL", item.QTY_REAL.ToString());
                parameters.Add("@GAPADD", item.GAP_ADDITION.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                List<double> GAP = new List<double>();
                List<int> TIMES = new List<int>();
                sql = "SELECT UserID, MetaItemID, QTYGap, Times FROM [RS].[QTYGAP] WHERE UserID = @UserID AND MetaItemID = @MetaItemID";
                SqlDataReader dr = executeReader(sql, parameters);
                int d = 0;
                while (dr.Read())
                {
                    double g = Convert.ToDouble(dr[2], CultureInfo.CreateSpecificCulture("en-GB"));
                    if (g != 0)
                    {
                        d++;
                        GAP.Add(Convert.ToDouble(dr[2], CultureInfo.CreateSpecificCulture("en-GB")));
                        TIMES.Add(Convert.ToInt32(dr[3], CultureInfo.CreateSpecificCulture("en-GB")));
                    }
                }
                dr.Close();
                if (d > 0)
                    for (int i = 0; i < d; i++)
                    {

                        string s = "UPDATE [RS].[QTYGAP] SET QTYGap = " + ((GAP[i] * TIMES[i] + item.GAP_ADDITION) / (TIMES[i] + 1)).ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ", Times = " + (TIMES[i] + 1) + " WHERE UserID = @UserID AND MetaItemID = @MetaItemID";
                        executeNonQuery(s, parameters);
                    }
                if (d == 0)
                {
                    dr.Close();
                    executeNonQuery("INSERT INTO [RS].[QTYGAP](UserID, MetaItemID, QTYGap, Times) VALUES(@UserID, @MetaItemID, " + item.GAP_ADDITION.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ", 1)", parameters);
                }
            }//for     
        }


        public List<QTY_GAP> GetGAP()
        {
            List<QTY_GAP> lst = new List<QTY_GAP>();
            string sql = "Select UserID, MetaItemID, QTYGap, Times from [RS].[QTYGAP]";
            SqlDataReader dr = executeReader(sql);
            while (dr.Read())
            {
                QTY_GAP item = new QTY_GAP();
                item.UserID = dr[0].ToString();
                item.MetaItemID = dr[1].ToString();
                item.QTYGAP = Convert.ToDouble(dr[2]);
                if (item.QTYGAP > 2) item.QTYGAP = 2.0;//this line can be deleted 
                item.Time = Convert.ToInt32(dr[3]);

                lst.Add(item);
            }
            dr.Close();
            return lst;
        }
    }
}