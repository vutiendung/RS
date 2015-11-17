using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsbo_lib;
using rsdao_lib;
using algorithm_lib;
using rsglobal_lib;
using System.Data;
using System.Diagnostics;


namespace rsclustering_lib
{
    public class ClusterUsers : Base
    {

        #region GUI - SNT

        public int[] getStaticData()
        {
            int[] result = new int[4];
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                result = dao.getStaticsData();
                dao.commitTransaction();
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
            return result;
        }

        public bool checkClusterTableIsInProcess()
        {
            bool result = true;
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                result = dao.checkLockedTable("USER_CLUSTER_TBL");
                dao.commitTransaction();
            }
            catch (Exception)
            {
                dao.rollbackTransaction();
                result = true;
            }
            return result;
        }

        #endregion

        #region Update Cluster

        public int updateCluster_AllTransac(string LoginID)
        {
            Clustering_Users_DAO dao = null;
            ClusterSchedule clusterSchedule = new ClusterSchedule();
            clusterSchedule.ClusterType = ConstantValues.SCHE_CLUSTER_USER;
            clusterSchedule.Log = String.Empty;
            clusterSchedule.LoginID = LoginID;

            int processType = checkProcess();
            if (processType == 0)
            {
                Settings st = getSettings();
                try
                {
                    dao = new Clustering_Users_DAO();
                    dao.beginTransaction();
                    dao.CLEAN_RATTING_MATRIX();

                    // Add to Schedule
                    clusterSchedule.Algorithm = Cluster.CLUSTER_KMEAN;//st.cluster_type;
                    dao.addNewClusterSchedule(clusterSchedule);
                    string log = "Update Cluster\n";

                    List<User_SubCategories> list_User_SubCategories = dao.getAllSubCategories();
                    bool existTransac = false;

                    List<string> list_User_Categories = get_User_Categories(list_User_SubCategories);
                    List<List<User_SubCategories>> list_SMALL_User_SubCategories = new List<List<User_SubCategories>>();
                    foreach (var item in list_User_Categories)
                        list_SMALL_User_SubCategories.Add(new List<User_SubCategories>());

                    #region Cluster For LargeGroup

                    int numUser = 0;
                    Dictionary<string, int> allItem = new Dictionary<string, int>();
                    foreach (var item in list_User_SubCategories)
                    {
                        string categoryName = item.U_SubCategoryID + "_";

                        // Convert Data to matrix
                        Dictionary<string, int> dic_users = new Dictionary<string, int>();
                        Dictionary<string, int> dic_items = new Dictionary<string, int>();

                        // Compute ratting matrix
                        List<MatrixItem> lstRMI = dao.computeRattingMatrixItem(item.U_SubCategoryID, st.Alpha);
                        double[][] x = Util.toMatrix_MatrixItem(lstRMI, out dic_users, out dic_items);

                        if (dic_users.Count > st.U_M / 4)
                        {
                            numUser += dic_users.Count;
                            foreach (var it in dic_items)
                                if (!allItem.ContainsKey(it.Key))
                                    allItem.Add(it.Key, it.Value);

                            List<Partion> listPartion = getPartion_ByU_SubCategoryID(dao, item.U_SubCategoryID);
                            LoadUpdateCluster(dao, st, ref existTransac, listPartion, item.U_SubCategoryID, categoryName, dic_users, dic_items, x);
                        }
                        else
                            add_SMALL_User_SubCategories(list_User_Categories, ref list_SMALL_User_SubCategories, item);
                    }

                    #endregion

                    #region Cluster For SmallGroup

                    dao.delete_USER_CLUSTER_TBL_2("_MERGE_");
                    dao.delete_PARTION_TBL_2("_MERGE_");
                    dao.delete_USER_CENTROID_TBL_2("_MERGE_");

                    for (int i = 0; i < list_User_Categories.Count; i++)
                    {
                        string categoryName = list_User_Categories[i] + "_MERGE_";

                        // Convert Data to matrix
                        Dictionary<string, int> dic_users = new Dictionary<string, int>();
                        Dictionary<string, int> dic_items = new Dictionary<string, int>();

                        // Compute ratting matrix
                        List<MatrixItem> lstRMI = new List<MatrixItem>();
                        foreach (User_SubCategories item in list_SMALL_User_SubCategories[i])
                            lstRMI.AddRange(dao.computeRattingMatrixItem(item.U_SubCategoryID, st.Alpha));

                        double[][] x = Util.toMatrix_MatrixItem(lstRMI, out dic_users, out dic_items);

                        numUser += dic_users.Count;
                        foreach (var it in dic_items)
                            if (!allItem.ContainsKey(it.Key))
                                allItem.Add(it.Key, it.Value);

                        List<Partion> listPartion = getPartion_ByU_CategoryID_ForMergeGreoup(dao, categoryName);
                        LoadUpdateCluster(dao, st, ref existTransac, listPartion, list_User_Categories[i], categoryName, dic_users, dic_items, x);
                    }

                    #endregion

                    // Remove Excess USER_CLUSTER_TBL
                    dao.removeExcessCluster();

                    // Update USE_TBL
                    if (existTransac)
                        updateTransac_CheckPoint();

                    log += "\t Number of Client Categories: " + list_User_SubCategories.Count;
                    log += "\t Number of Client           : " + numUser;
                    log += "\t Number of Item             : " + allItem.Count;
                    log += "\n Parameter: \n";
                    log += "\t Algorithm:" + st.cluster_type;
                    log += "\t Epsilon  :" + st.epsilon;
                    log += "\t Max Loop :" + st.maxLoop;

                    // Update Success Schedule
                    clusterSchedule.Log = log;
                    dao.updateClusterSchedule(clusterSchedule);
                    dao.commitTransaction();
                }
                catch (Exception ex)
                {
                    dao.rollbackTransaction();
                    throw ex;
                }
            }
            return processType;
        }

        private void LoadUpdateCluster(Clustering_Users_DAO dao,
            Settings st,
            ref bool existTransac,
            List<Partion> listPartion,
            string U_SubCategoryID,
            string categoryName,
            Dictionary<string, int> dic_users,
            Dictionary<string, int> dic_items,
            double[][] x)
        {
            if (x.Length > 0)
            {
                existTransac = true;
                List<string> listClusterID = getListClusterID(listPartion);

                // Get existed V
                double[][] v = new double[listClusterID.Count][];
                Cluster cl = new Cluster();
                cl.addSetting(v.Length, st.maxLoop, x, 0.0001, st.Alpha, st.T, st.U_M);
                Dictionary<int, int> clustered_Data;

                if (listPartion.Count > 0)
                {
                    v = get_V_ByNewItems_ReComputeAll(x, dic_items, dic_users, listPartion, listClusterID);
                    clustered_Data = cl.Clustering_Merge(st.cluster_type, v);
                }
                else
                    clustered_Data = cl.Clustering_Merge(st.cluster_type);

                v = cl.getV();

                // Mapping UserID to clustered Data
                Dictionary<string, int> mapped_Data = map_ID_to_Index(dic_users, clustered_Data);

                #region C1 - Ratting Matrix

                // Add Ratting Matrix
                for (int i = 0; i < x.Length; i++)
                    for (int j = 0; j < x[0].Length; j++)
                        if (x[i][j] > 0)
                        {
                            try
                            {
                                MatrixItem matrixItem = new MatrixItem();
                                matrixItem.ClusterID = categoryName + (mapped_Data[Util.FindKeyByValue(dic_users, i)] + 1);
                                matrixItem.Row = Util.FindKeyByValue(dic_users, i);
                                matrixItem.Column = Util.FindKeyByValue(dic_items, j);
                                matrixItem.Cell = x[i][j];
                                dao.setRattingMatrix(matrixItem);
                            }
                            catch (Exception) { }
                        }

                #endregion

                // Remove old USER_CLUSTER_TBL
                dao.delete_USER_CLUSTER_TBL(categoryName);
                // Add to USER_CLUSTER_TBL
                for (int i = 0; i < v.Length; i++)
                    dao.addCluster(categoryName + (i + 1), U_SubCategoryID);

                // Remove old PARTION_TBL
                dao.delete_PARTION_TBL(categoryName);
                // Add to PARTION_TBL
                foreach (var map in mapped_Data)
                {
                    Partion detail = new Partion();
                    detail.UserID = map.Key;
                    detail.ClusterID = categoryName + (map.Value + 1);
                    dao.addPARTION_TBL(detail);
                }

                // Remove old USER_CENTROID_TBL
                dao.delete_USER_CENTROID_TBL(categoryName);
                // Add to USER_CENTROID_TBL
                for (int i = 0; i < v.Length; i++)
                    for (int j = 0; j < v[0].Length; j++)
                        if (!Double.NaN.Equals(v[i][j]))
                        {
                            try
                            {
                                User_Centroid centroid = new User_Centroid();
                                centroid.Value = v[i][j];
                                centroid.ClusterID = categoryName + (i + 1);
                                centroid.MetaItemID = Util.FindKeyByValue(dic_items, j);
                                dao.add_User_Centroid(centroid);
                            }
                            catch (Exception) { }
                        }

            }
        }

        private List<Partion> getPartion_ByU_CategoryID_ForMergeGreoup(Clustering_Users_DAO dao, string categoryName)
        {
            List<Partion> list = dao.getPartion_ByU_CategoryID_ForMergeGreoup(categoryName);
            foreach (var item in list)
                item.UserCentroid = dao.getUserCentroid(item.ClusterID);
            return list;
        }

        private List<Partion> getPartion_ByU_SubCategoryID(Clustering_Users_DAO dao, string U_SubCategoryID)
        {
            List<Partion> list = dao.getPartion_ByU_SubCategoryID(U_SubCategoryID);
            foreach (var item in list)
                item.UserCentroid = dao.getUserCentroid(item.ClusterID);
            return list;
        }

        private double[][] get_V_ByNewItems_ReComputeAll(double[][] x,
            Dictionary<string, int> dic_items,
            Dictionary<string, int> dic_users,
            List<Partion> listPartion,
            List<string> listClusterID)
        {
            double[][] v = new double[listClusterID.Count][];

            for (int i = 0; i < listClusterID.Count; i++)
                v[i] = new double[dic_items.Count];

            for (int i = 0; i < listClusterID.Count; i++)
                foreach (var item in dic_items)
                {
                    double count = 0.0;
                    for (int j = 0; j < listPartion.Count; j++)
                    {
                        if (dic_users.ContainsKey(listPartion[j].UserID))
                        {
                            if (listPartion[j].ClusterID.Equals(listClusterID[i])
                                && x[dic_users[listPartion[j].UserID]][item.Value] > 0)
                            {
                                count++;
                                v[i][item.Value] += x[dic_users[listPartion[j].UserID]][item.Value];
                            }
                        }
                    }

                    if (count > 0)
                        v[i][item.Value] /= count;
                }

            return v;
        }

        private static List<string> getListClusterID(List<Partion> listPartion)
        {
            List<string> listClusterID = new List<string>();
            foreach (var item in listPartion)
            {
                bool exist = false;
                foreach (var id in listClusterID)
                    if (id.Equals(item.ClusterID))
                    {
                        exist = true;
                        break;
                    }

                if (!exist)
                    listClusterID.Add(item.ClusterID);
            }

            return listClusterID;
        }

        #endregion

        #region Cluster

        public void startClusteringAuto()
        {
            string LoginID = ConstantValues.LOGIN_ID_AUTO;
            startClusteringManual(LoginID);
        }

        public int startClusteringManual(string LoginID)
        {
            return clusteringAllTransac(LoginID);
        }

        public int clusteringAllTransac(string LoginID)
        {
            Clustering_Users_DAO dao = null;
            ClusterSchedule clusterSchedule = new ClusterSchedule();
            clusterSchedule.ClusterType = ConstantValues.SCHE_CLUSTER_USER;
            clusterSchedule.Log = String.Empty;
            clusterSchedule.LoginID = LoginID;

            int processType = checkProcess();
            if (processType == 0)
            {
                Settings st = getSettings();
                try
                {
                    dao = new Clustering_Users_DAO();
                    dao.beginTransaction();

                    // Add to Schedule
                    clusterSchedule.Algorithm = st.cluster_type;
                    string log = "Clustering Client\n";
                    clusterSchedule.Log = log;
                    dao.addNewClusterSchedule(clusterSchedule);

                    // Clean data
                    dao.CLEAN_USER_CENTROID_TBL();
                    dao.CLEAN_PARTION_TBL();
                    dao.CLEAN_USER_CLUSTER_TBL();
                    dao.CLEAN_RATTING_MATRIX();

                    List<User_SubCategories> list_User_SubCategories = dao.getAllSubCategories();
                    bool existTransac = false;

                    List<string> list_User_Categories = get_User_Categories(list_User_SubCategories);
                    List<List<User_SubCategories>> list_SMALL_User_SubCategories = new List<List<User_SubCategories>>();
                    foreach (var item in list_User_Categories)
                        list_SMALL_User_SubCategories.Add(new List<User_SubCategories>());

                    #region Cluster For LargeGroup

                    int numUser = 0;
                    Dictionary<string, int> allItem = new Dictionary<string, int>();
                    foreach (User_SubCategories item in list_User_SubCategories)
                    {
                        string categoryName = item.U_SubCategoryID + "_";

                        // Convert Data to matrix
                        Dictionary<string, int> dic_users = new Dictionary<string, int>();
                        Dictionary<string, int> dic_items = new Dictionary<string, int>();


                        // Compute ratting matrix
                        List<MatrixItem> lstRMI = dao.computeRattingMatrixItem(item.U_SubCategoryID, st.Alpha);
                        double[][] x = Util.toMatrix_MatrixItem(lstRMI, out dic_users, out dic_items);

                        if (dic_users.Count > st.U_M / 2)
                        {
                            numUser += dic_users.Count;
                            foreach (var it in dic_items)
                                if (!allItem.ContainsKey(it.Key))
                                    allItem.Add(it.Key, it.Value);

                            LoadCluster(dao, st, ref existTransac, item.U_SubCategoryID, categoryName, dic_users, dic_items, x);
                        }
                        else
                            add_SMALL_User_SubCategories(list_User_Categories, ref list_SMALL_User_SubCategories, item);
                    }

                    #endregion

                    #region Cluster For SmallGroup

                    dao.delete_USER_CLUSTER_TBL_2("_MERGE_");
                    dao.delete_PARTION_TBL_2("_MERGE_");
                    dao.delete_USER_CENTROID_TBL_2("_MERGE_");

                    for (int i = 0; i < list_User_Categories.Count; i++)
                    {
                        string categoryName = list_User_Categories[i] + "_MERGE_";

                        // Convert Data to matrix
                        Dictionary<string, int> dic_users = new Dictionary<string, int>();
                        Dictionary<string, int> dic_items = new Dictionary<string, int>();

                        // Compute ratting matrix
                        List<MatrixItem> lstRMI = new List<MatrixItem>();
                        foreach (User_SubCategories item in list_SMALL_User_SubCategories[i])
                            lstRMI.AddRange(dao.computeRattingMatrixItem(item.U_SubCategoryID, st.Alpha));

                        double[][] x = Util.toMatrix_MatrixItem(lstRMI, out dic_users, out dic_items);

                        numUser += dic_users.Count;
                        foreach (var it in dic_items)
                            if (!allItem.ContainsKey(it.Key))
                                allItem.Add(it.Key, it.Value);

                        LoadCluster(dao, st, ref existTransac, list_User_Categories[i], categoryName, dic_users, dic_items, x);
                    }

                    #endregion

                    // Remove Excess USER_CLUSTER_TBL
                    dao.removeExcessCluster();

                    // Update USE_TBL
                    if (existTransac)
                        updateTransac_CheckPoint();

                    log += "\t Number of Client Categories: " + list_User_SubCategories.Count;
                    log += "\t Number of Client           : " + numUser;
                    log += "\t Number of Item             : " + allItem.Count;
                    log += "\n Parameter: \n";
                    log += "\t Algorithm:" + st.cluster_type;
                    log += "\t Epsilon  :" + st.epsilon;
                    log += "\t Max Loop :" + st.maxLoop;

                    // Update Success Schedule
                    clusterSchedule.Log = log;
                    dao.updateClusterSchedule(clusterSchedule);
                    dao.commitTransaction();
                }
                catch (Exception ex)
                {
                    dao.rollbackTransaction();
                    throw ex;
                }
            }
            return processType;
        }

        private void add_SMALL_User_SubCategories(List<string> list_User_Categories,
            ref List<List<User_SubCategories>> list_SMALL_User_SubCategories,
            User_SubCategories subCat)
        {
            for (int i = 0; i < list_User_Categories.Count; i++)
                if (subCat.U_CategoryID.Equals(list_User_Categories[i]))
                    list_SMALL_User_SubCategories[i].Add(subCat);
        }

        private List<string> get_User_Categories(List<User_SubCategories> list_User_SubCategories)
        {
            List<string> list_User_Categories = new List<string>();

            foreach (User_SubCategories subCat in list_User_SubCategories)
            {
                bool exit = false;
                for (int i = 0; i < list_User_Categories.Count; i++)
                    if (subCat.U_CategoryID.Equals(list_User_Categories[i]))
                    {
                        exit = true;
                        break;
                    }

                if (!exit)
                    list_User_Categories.Add(subCat.U_CategoryID);
            }

            return list_User_Categories;
        }

        private void LoadCluster(Clustering_Users_DAO dao,
            Settings st,
            ref bool existTransac,
            string U_SubCategoryID,
            string categoryName,
            Dictionary<string, int> dic_users,
            Dictionary<string, int> dic_items,
            double[][] x)
        {
            if (x.Length > 0)
            {
                existTransac = true;

                // Clustering Data
                Cluster cluster = new Cluster();
                Dictionary<int, int> clustered_Data = new Dictionary<int, int>();

                cluster.addSetting(st.k, st.maxLoop, x, st.epsilon, st.Alpha, st.T, st.U_M);
                clustered_Data = cluster.Clustering(st.cluster_type);

                //if (clustered_Data.Count > 0)
                //{
                // Mapping UserID to clustered Data
                Dictionary<string, int> mapped_Data = map_ID_to_Index(dic_users, clustered_Data);

                // Get Destination V
                double[][] v = cluster.getV();

                // Add new data

                #region C1 - Ratting Matrix

                // Add Ratting Matrix
                for (int i = 0; i < x.Length; i++)
                    for (int j = 0; j < x[0].Length; j++)
                        if (x[i][j] > 0)
                        {
                            try
                            {
                                MatrixItem matrixItem = new MatrixItem();
                                matrixItem.ClusterID = categoryName + (mapped_Data[Util.FindKeyByValue(dic_users, i)] + 1);
                                matrixItem.Row = Util.FindKeyByValue(dic_users, i);
                                matrixItem.Column = Util.FindKeyByValue(dic_items, j);
                                matrixItem.Cell = x[i][j];
                                dao.setRattingMatrix(matrixItem);
                            }
                            catch (Exception) { }
                        }

                #endregion

                // Add to USER_CLUSTER_TBL
                for (int i = 0; i < v.Length; i++)
                    dao.addCluster(categoryName + (i + 1), U_SubCategoryID);

                // Add to PARTION_TBL
                foreach (var map in mapped_Data)
                {
                    Partion detail = new Partion();
                    detail.UserID = map.Key;
                    detail.ClusterID = categoryName + (map.Value + 1);
                    dao.addPARTION_TBL(detail);
                }

                // Add to USER_CENTROID_TBL
                for (int i = 0; i < v.Length; i++)
                    for (int j = 0; j < v[0].Length; j++)
                        if (!Double.NaN.Equals(v[i][j]))
                        {
                            try
                            {
                                User_Centroid centroid = new User_Centroid();
                                centroid.Value = v[i][j];
                                centroid.ClusterID = categoryName + (i + 1);
                                centroid.MetaItemID = Util.FindKeyByValue(dic_items, j);
                                dao.add_User_Centroid(centroid);
                            }
                            catch (Exception) { }
                        }
            }
            //}
        }

        private static Dictionary<string, int> map_ID_to_Index(Dictionary<string, int> dic_users,
            Dictionary<int, int> clustered_Data)
        {
            Dictionary<string, int> clustered_Data_Maped = new Dictionary<string, int>();
            if (null != clustered_Data & null != dic_users)
                foreach (var cd_item in clustered_Data)
                    clustered_Data_Maped.Add(Util.FindKeyByValue(dic_users, cd_item.Key), cd_item.Value);

            return clustered_Data_Maped;
        }

        #endregion

        public List<ClusteredReport> getClusteredReport()
        {
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                List<User_SubCategories> list_User_Categories = dao.getAllSubCategories();
                List<ClusteredReport> list = new List<ClusteredReport>();
                foreach (var item in list_User_Categories)
                {
                    ClusteredReport cr = new ClusteredReport();
                    cr.U_SubCategoryID = item.U_SubCategoryID;
                    cr.lstCluster = dao.getClusterInfo_BySubCategory(item.U_SubCategoryID);
                    list.Add(cr);
                }
                dao.commitTransaction();
                return list;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

        public List<ClusterSchedule> getListClusterSchedule_DESC()
        {
            Clustering_Users_DAO dao = null;
            try
            {
                dao = new Clustering_Users_DAO();
                dao.beginTransaction();
                List<ClusterSchedule> list = dao.getListClusterSchedule_DESC();
                dao.commitTransaction();
                return list;
            }
            catch (Exception ex)
            {
                dao.rollbackTransaction();
                throw ex;
            }
        }

    }
}