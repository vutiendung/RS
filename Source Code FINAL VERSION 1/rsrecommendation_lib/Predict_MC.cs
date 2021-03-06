﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsdao_lib;
using rsbo_lib;
using rsglobal_lib;
using System.Globalization;


namespace rsrecommendation_lib
{
    public class Predict_MC
    {
        #region Call Algorithms

        public void R1(Predict_DAO_MCol dao, RecommdationSchedule schedule, int nbR1, Dictionary<string, double> UCA, List<QTY_GAP> list)
        {
            //Get recommendations for traditional users
            List<Recommendation_Meta_Item> lrc_ForUserTra = getListRecommendation_R1_TraditionalUsers(dao, nbR1);
            lrc_ForUserTra = GetQuantityTraditionalUsers(dao, lrc_ForUserTra, UCA);
            Normaliser(lrc_ForUserTra);
            AddGAPtoListRecommends(list, lrc_ForUserTra);
            dao.InsertList(lrc_ForUserTra, schedule);
        }
        public void R1(Predict_DAO_MCol dao, RecommdationSchedule schedule)
        {
            List<Recommendation_Meta_Item> traditionalUsers1 = this.getListRecommendation_R1_TraditionalUsers(dao);
            List<Recommendation_Meta_Item> traditionalUsers2 = this.GetQuantityTraditionalUsers(dao, traditionalUsers1);
            this.Normaliser(traditionalUsers2);
            dao.InsertList(traditionalUsers2, schedule);
        }
        public void R3(Predict_DAO_MCol dao, RecommdationSchedule schedule)
        {
            List<Recommendation_Meta_Item> recommendationR3 = this.GetRecommendationR3(dao);
            List<Recommendation_Meta_Item> traditionalUsersR3 = this.GetQuantityTraditionalUsersR3(dao, recommendationR3);
            this.Normaliser(traditionalUsersR3);
            dao.InsertList(traditionalUsersR3, schedule);
        }

        public void R4(Predict_DAO_MCol dao, RecommdationSchedule schedule)
        {
            List<Recommendation_Meta_Item> recommendationR4 = this.GetRecommendationR4(dao, 2.0);
            this.Normaliser(recommendationR4);
            dao.InsertList(recommendationR4, schedule);
        }
        public List<Recommendation_Meta_Item> GetRecommendationR4(Predict_DAO_MCol a, double param)
        {
            List<Recommendation_Meta_Item> list1 = new List<Recommendation_Meta_Item>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            Dictionary<string, double> dictionary1 = new Dictionary<string, double>();
            Dictionary<string, double> dictionary2 = new Dictionary<string, double>();
            Dictionary<string, double> dictionary3 = new Dictionary<string, double>();
            Dictionary<string, double> dictionary4 = new Dictionary<string, double>();
            Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
            List<string> listClusterId = a.GetListClusterID();
            Dictionary<string, string> listNewItem = a.GetListNewItem();
            foreach (string cluster in listClusterId)
            {
                try
                {
                    Dictionary<string, double> listRateU = a.GetListRateU(cluster);
                    Dictionary<string, double> listRateF = a.GetListRateF(cluster);
                    foreach (KeyValuePair<string, double> keyValuePair1 in listRateU)
                    {
                        Dictionary<string, double> listRateUf = a.GetListRateUF(keyValuePair1.Key);
                        double num1 = 0.0;
                        string FamillyCode = "Not found";
                        List<string> list5 = new List<string>();
                        List<double> list6 = new List<double>();
                        foreach (KeyValuePair<string, double> keyValuePair2 in listRateF)
                        {
                            double num2;
                            if (listRateUf.ContainsKey(keyValuePair2.Key))
                            {
                                double num3 = keyValuePair2.Value;
                                double num4 = keyValuePair1.Value;
                                double num5 = keyValuePair2.Value;
                                num2 = param * num5 * (1.0 / num4 - 1.0 / num3);
                            }
                            else
                                num2 = 0.0;
                            if (num2 > num1)
                            {
                                num1 = num2;
                                FamillyCode = keyValuePair2.Key;
                            }
                        }
                        if (num1 != 0.0)
                        {
                            double num2 = (double)a.getQTYR4(keyValuePair1.Key, FamillyCode, 1);
                            foreach (KeyValuePair<string, string> keyValuePair2 in listNewItem)
                            {
                                if (keyValuePair2.Value.Equals(FamillyCode))
                                    list1.Add(new Recommendation_Meta_Item()
                                    {
                                        UserID = keyValuePair1.Key,
                                        RecommendType = ConstantValues.RC_TYPE_LRS04,
                                        MetaItemID = keyValuePair2.Key,
                                        Quantity = num2,
                                        Score = num1
                                    });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return list1;
        }
        public void R3(Predict_DAO_MCol dao, RecommdationSchedule schedule, int nbR3, Dictionary<string, double> UCA, List<QTY_GAP> lst)
        {
            List<Recommendation_Meta_Item> list = GetRecommendationR3(dao, nbR3);
            list = GetQuantityTraditionalUsersR3(dao, list, UCA);
            dao.RemoveDuplicateR3R1();
            Normaliser(list);
            AddGAPtoListRecommends(lst, list);
            dao.InsertList(list, schedule);
        }

        public void R4(Predict_DAO_MCol dao, RecommdationSchedule schedule, double paramR4, Dictionary<string, double> UCA, List<QTY_GAP> lst)
        {
            List<Recommendation_Meta_Item> list = GetRecommendationR4(dao, paramR4, UCA);
            Normaliser(list);
            AddGAPtoListRecommends(lst, list);
            dao.InsertList(list, schedule);
        }

        public List<Recommendation_Meta_Item> GetQuantityTraditionalUsersR3(Predict_DAO_MCol a, List<Recommendation_Meta_Item> list)
        {
            for (int index = 0; index < list.Count; ++index)
            {
                double qualtityTraditionalUser = a.GetQualtityTraditionalUser(list[index]);
                if (qualtityTraditionalUser != 0.0)
                    list[index].Quantity = (double)((int)(list[index].Quantity + qualtityTraditionalUser) / 2);
            }
            return list;
        }
        //have formular here
        public List<Recommendation_Meta_Item> GetRecommendationR3(Predict_DAO_MCol a)
        {
            List<Recommendation_Meta_Item> list1 = new List<Recommendation_Meta_Item>();
            Recommendation_Meta_Item recommendationMetaItem1 = new Recommendation_Meta_Item();
            List<CONF> list2 = new List<CONF>();
            List<DIST> list3 = new List<DIST>();
            List<string> list4 = new List<string>();
            List<string> list5 = new List<string>();
            List<string> list6 = new List<string>();
            List<U_I_Q> list7 = new List<U_I_Q>();
            foreach (string cluster in a.GetListClusterID())
            {
                List<CONF> confOfCluster = a.GetCONF_OfCluster(cluster);
                List<DIST> distOfCluster = a.GetDIST_OfCluster(cluster);
                List<string> listUserOfCluster = a.GetListUser_OfCluster(cluster);
                List<U_I_Q> traditionalUsersMore = a.GetQuantityTraditionalUsers_More(cluster);
                foreach (string user in listUserOfCluster)
                {
                    List<string> notPurchasedOfUser = a.GetListMetaItemNotPurchased_OfUser(user, cluster);
                    List<string> listMetaItemOfUser = a.GetListMetaItem_OfUser(user);
                    string str = "";
                    double num1 = 0.0;
                    for (int index1 = 0; index1 < notPurchasedOfUser.Count; ++index1)
                    {
                        double num2 = 0.0;
                        double num3 = 0.0;
                        double num4 = 1.0;
                        for (int index2 = 0; index2 < listMetaItemOfUser.Count; ++index2)
                        {
                            for (int index3 = 0; index3 < confOfCluster.Count; ++index3)
                            {
                                if (confOfCluster[index3].MetaItemSource.Equals(listMetaItemOfUser[index2]) && confOfCluster[index3].MetaItemDestination.Equals(notPurchasedOfUser[index1]))
                                {
                                    num3 = confOfCluster[index3].Confident;
                                    break;
                                }
                            }
                            for (int index3 = 0; index3 < distOfCluster.Count; ++index3)
                            {
                                if (distOfCluster[index3].MetaItemDestination.Equals(listMetaItemOfUser[index2]) && distOfCluster[index3].MetaItemDestination.Equals(notPurchasedOfUser[index1]))
                                {
                                    num4 = distOfCluster[index3].Distance;
                                    break;
                                }
                            }
                            if (num4 != 0.0)
                                num2 += num3 / num4;
                        }
                        if (num1 < num2 / (double)listMetaItemOfUser.Count)
                        {
                            num1 = num2;
                            str = notPurchasedOfUser[index1];
                        }
                    }
                    if (num1 != 0.0)
                    {
                        Recommendation_Meta_Item recommendationMetaItem2 = new Recommendation_Meta_Item();
                        recommendationMetaItem2.UserID = user;
                        recommendationMetaItem2.MetaItemID = str;
                        recommendationMetaItem2.RecommendType = ConstantValues.RC_TYPE_LRS03;
                        recommendationMetaItem2.Quantity = 0.0;
                        for (int index = 0; index < traditionalUsersMore.Count; ++index)
                        {
                            if (traditionalUsersMore[index].MetaItemID.Equals(str))
                            {
                                recommendationMetaItem2.Quantity = traditionalUsersMore[index].QTY;
                                break;
                            }
                        }
                        recommendationMetaItem2.Score = num1;
                        list1.Add(recommendationMetaItem2);
                    }
                }
            }
            return list1;
        }
        public List<Recommendation_Meta_Item> GetQuantityTraditionalUsers(Predict_DAO_MCol a, List<Recommendation_Meta_Item> list)
        {
            for (int index = 0; index < list.Count; ++index)
            {
                double qualtityTraditionalUser = a.GetQualtityTraditionalUser(list[index]);
                list[index].Quantity = qualtityTraditionalUser == 0.0 ? 1.0 : (double)(int)qualtityTraditionalUser;
            }
            return list;
        }
        public void R1R4_FOR_NEW_USERS(Predict_DAO_MCol dao, RecommdationSchedule schedule, Dictionary<string, double> UCA)
        {
            List<Recommendation_Meta_Item> lrc_ForNewUsers = GetListR1R4_ForNewUsers(dao, UCA);
            // lrc_ForNewUsers = GetQuantityNewUsers(dao, lrc_ForNewUsers);
            Normaliser(lrc_ForNewUsers);
            dao.InsertListNewUser(lrc_ForNewUsers, schedule);
        }

        #endregion


        #region R1 + R4 for New users MC
        public List<Recommendation_Meta_Item> GetListR1R4_ForNewUsers(Predict_DAO_MCol dao, Dictionary<string, double> UCA)
        {
            double M = 1;
            if (UCA.ContainsKey("MEANS_VALUE"))
                M = UCA["MEANS_VALUE"];

            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            //Content is cleared by M.C. Nguyen 19.9.2015
            return list;

        }
        #endregion


        #region Recommendations R1 - C6 - MC

        public List<Recommendation_Meta_Item> GetRecommendations(string sql)
        {
            Predict_DAO_MCol a = new Predict_DAO_MCol();
            try
            {
                a.beginTransaction();
                List<Recommendation_Meta_Item> list = a.GetRecommendations(sql);
                a.commitTransaction();
                return list;
            }
            catch (Exception ex)
            {
                a.rollbackTransaction();
                throw ex;
            }
        }

        public List<ClusterInfo> GetListCluster()
        {
            Predict_DAO_MCol a = new Predict_DAO_MCol();
            List<ClusterInfo> list = new List<ClusterInfo>();

            try
            {
                a.beginTransaction();
                list = a.GetListCluster();
                a.commitTransaction();
                return list;
            }
            catch (Exception ex)
            {
                a.rollbackTransaction();
                throw ex;
            }
        }

        //public List<Recommendation_Meta_Item> GetListRecommendC6_ForTraditionalUser()
        //{         
        //    Predict_DAO_MC a = new Predict_DAO_MC();
        //    List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
        //    List<Recommendation_Meta_Item> RC = new List<Recommendation_Meta_Item>();
        //    try
        //    {
        //        a.beginTransaction();
        //        List<User> list_users_tra = a.getTraditionalUser_UnBlocked();

        //        foreach (User u in list_users_tra)
        //        {

        //            RC = a.GetRecommendC6_ForTraditionalUser(u, 1);
        //            list.AddRange(RC);

        //        }
        //        a.commitTransaction();
        //        return list;
        //    }
        //    catch (Exception ex)
        //    {
        //        a.rollbackTransaction();
        //        throw ex;
        //    }
        //}

        //----------------------------------------------------------------------------------------------
        //This function calls GetRecommendC6_ForTraditionalUser from Predict_DAO_MC to give R1
        //Created by MC. NGUYEN
        //Corrected by MC. NGUYEN, 22.10.2014
        //Changed: 24.10.2014: the formule to calculate SCORE for each Meta Produit (based on Count(*)).
        //----------------------------------------------------------------------------------------------
        public List<Recommendation_Meta_Item> getListRecommendation_R1_TraditionalUsers(Predict_DAO_MCol a)
        {
            List<Recommendation_Meta_Item> list1 = new List<Recommendation_Meta_Item>();
            List<Recommendation_Meta_Item> list2 = new List<Recommendation_Meta_Item>();
            List<string> list3 = new List<string>();
            List<string> list4 = new List<string>();
            foreach (string cluster in a.GetListClusterID())
            {
                foreach (string u in a.GetListUser_OfCluster(cluster))
                {
                    List<Recommendation_Meta_Item> forTraditionalUser = a.GetRecommendC6_ForTraditionalUser(u, 2);
                    list1.AddRange((IEnumerable<Recommendation_Meta_Item>)forTraditionalUser);
                }
            }
            return list1;
        }

        public List<Recommendation_Meta_Item> getListRecommendation_R1_TraditionalUsers(Predict_DAO_MCol a, int nbR1)
        {
            //Predict_DAO_MC a = new Predict_DAO_MC();
            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            List<Recommendation_Meta_Item> RC = new List<Recommendation_Meta_Item>();
            List<string> lstCluster = new List<string>();
            List<string> User_OfCluster = new List<string>();
            //Settings r = GetRecommendationSeting(a);
            lstCluster = a.GetListClusterID();
            //Check number of recommendations
            if (nbR1 <= 0) nbR1 = 1;

            //int i = 0;

            foreach (string cluster in lstCluster)
            {


                User_OfCluster = a.GetListUser_OfCluster(cluster);

                foreach (string u in User_OfCluster)
                {

                    RC = a.GetRecommendC6_ForTraditionalUser(u, nbR1);//2
                    list.AddRange(RC);

                }
            }
            return list;
        }


        //---------------------------------------------------------------------------------------------
        //This function call GetQualtityTraditionalUser to compute the quantity for each recommendations in the list
        //---------------------------------------------------------------------------------------------
        public List<Recommendation_Meta_Item> GetQuantityTraditionalUsers(Predict_DAO_MCol a, List<Recommendation_Meta_Item> list, Dictionary<string, double> UCA)
        {
            Int32 QTY;
            double M = 1;
            if (UCA.ContainsKey("MEANS_VALUE"))
                M = UCA["MEANS_VALUE"];

            for (int i = 0; i < list.Count; i++)
            {
                QTY = a.GetQualtityTraditionalUser(list[i]);
                if (QTY > 0)
                    list[i].Quantity = QTY;
                else
                    list[i].Quantity = 1;

                if (UCA.ContainsKey(list[i].UserID))
                {
                    list[i].Quantity = (int)list[i].Quantity * (UCA[list[i].UserID] / M);
                }
            }

            return list;

        }

        public List<Recommendation_Meta_Item> GetQuantityNewUsers(Predict_DAO_MCol a, List<Recommendation_Meta_Item> list)
        {
            //Predict_DAO_MC a = new Predict_DAO_MC();
            //List<U_I_Q> Cate_Item_QTY = new List<U_I_Q>();
            //try
            //{
            //    a.beginTransaction();
            list = a.GetAllQuantity(list);
            //    a.commitTransaction();

            return list;
            //}
            //catch (Exception ex)
            //{
            //    a.rollbackTransaction();
            //    throw ex;
            //}
        }

        //public List<Recommendation_Meta_Item> GetListRecommendC6_ForNewUser(Predict_DAO_MC a)
        //{
        //    //Predict_DAO_MC a = new Predict_DAO_MC();
        //    List<Recommendation_Meta_Item> list_On_SubCate = new List<Recommendation_Meta_Item>();
        //    List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
        //    List<Recommendation_Meta_Item> RC = new List<Recommendation_Meta_Item>();
        //    Recommendation_Meta_Item item = new Recommendation_Meta_Item();
        //    //try
        //    //{
        //    //a.beginTransaction();
        //    List<User> list_users_new = a.getNewUser_UnBlocked();
        //    List<string> lstSubCategoryID_new_user = a.getListSubCategory_ForNewUsers();

        //    foreach (string SubCate in lstSubCategoryID_new_user)
        //    {
        //        RC = a.GetRecommendC6_ForSubCategory(SubCate, 1);
        //        if (RC.Count > 0)
        //            list_On_SubCate.AddRange(RC);
        //    }

        //    foreach (User u in list_users_new)
        //    {
        //        foreach (Recommendation_Meta_Item R in list_On_SubCate)
        //            if (u.U_SubCategoryID.Equals(R.UserID))
        //            {
        //                item = new Recommendation_Meta_Item();
        //                item.UserID = u.UserID;
        //                item.RecommendType = ConstantValues.RC_TYPE_LRS01;
        //                //item.UserName = u.UserName;
        //                item.MetaItemID = R.MetaItemID;
        //                //item.MetaItemName = R.MetaItemName;
        //                item.Quantity = 0;
        //                item.Score = R.Score;
        //                list.Add(item);
        //            }
        //    }
        //    //a.commitTransaction();
        //    return list;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    a.rollbackTransaction();
        //    //    throw ex;
        //    //}
        //}

        #endregion

        #region get recommendations R3 - C8 - MC

        //----------------------------------------------------------------------------------------------
        //This function gives R3 AND QUANTITY (with a number of recommendations for each user as input)
        //----------------------------------------------------------------------------------------------

        public List<Recommendation_Meta_Item> GetRecommendationR3(Predict_DAO_MCol a, int nbR3)
        {
            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            //Content is cleared by M.C. Nguyen 19.9.2015
            return list;
        }

        public List<Recommendation_Meta_Item> GetQuantityTraditionalUsersR3(Predict_DAO_MCol a, List<Recommendation_Meta_Item> list, Dictionary<string, double> UCA)
        {

            double QTY;
            double M = 1;
            if (UCA.ContainsKey("MEANS_VALUE"))
                M = UCA["MEANS_VALUE"];

            for (int i = 0; i < list.Count; i++)
            {
                QTY = a.GetQualtityTraditionalUser(list[i]);
                if (QTY != 0)
                    list[i].Quantity = (int)(list[i].Quantity + QTY) / 2;
                //----------------------------------------
                if (UCA.ContainsKey(list[i].UserID))
                {
                    list[i].Quantity = (int)list[i].Quantity * (UCA[list[i].UserID] / M);
                }
            }



            //--------------------
            //a.commitTransaction();
            return list;
            //}
            //catch (Exception ex)
            //{
            //    a.rollbackTransaction();
            //    throw ex;
            //}
        }


        public void RemoveDuplicateR3R1()
        {
            Predict_DAO_MCol a = new Predict_DAO_MCol();
            try
            {
                a.RemoveDuplicateR3R1();
            }
            catch (Exception)
            {

                throw;
            }


        }

        #endregion

        #region get recommendations R4 - C9 - MC

        //----------------------------------------------------------------------------------------------
        //This function gives R4 AND QUANTITY (with a param as input)
        //Created by MC. NGUYEN
        //Corrected by MC. NGUYEN, 22.10.2014.
        //----------------------------------------------------------------------------------------------
        public List<Recommendation_Meta_Item> GetRecommendationR4(Predict_DAO_MCol a, double paramR4, Dictionary<string, double> UCA)
        {

            double M = 1;
            if (UCA.ContainsKey("MEANS_VALUE"))
                M = UCA["MEANS_VALUE"];

            List<Recommendation_Meta_Item> list = new List<Recommendation_Meta_Item>();
            //Content is cleared by M.C. Nguyen 19.9.2015
            return list;

        }

        #endregion

        #region public - MC

        //THis function get a price of MetaItemID that correspondance with the quantity
        //----------------------------------------------------------------------------------
        public double GetPrice(string MetaItemID, int QTY)
        {
            double Price = 0;
            Predict_DAO_MCol a = new Predict_DAO_MCol();
            try
            {
                Price = a.GetPrice(MetaItemID, QTY);
            }
            catch (Exception)
            {

                throw;
            }

            return Price;

        }


        public Dictionary<string, double> getUCA(Predict_DAO_MCol dao)
        {
            Dictionary<string, double> lstUCA = new Dictionary<string, double>();
            try
            {
                lstUCA = dao.getUCA();
            }
            catch (Exception)
            {

                lstUCA = null;
            }
            return lstUCA;

        }




        public List<Recommendation_Meta_Item> AddGAPtoListRecommends(List<QTY_GAP> lstGAP, List<Recommendation_Meta_Item> list)
        {
            for (int i = 0; i < list.Count; i++)
                list[i].Quantity = (double)AddGapToQTY(lstGAP, list[i].UserID.ToString(), list[i].MetaItemID.ToString(), (int)list[i].Quantity);
            return list;
        }



        public List<Recommendation_Meta_Item> Normaliser(List<Recommendation_Meta_Item> list)
        {
            double Max = 0.0;
            for (int i = 0; i < list.Count; i++)
                if (list[i].Score > Max) Max = list[i].Score;
            if (Max != 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Score /= Max;
                    if (list[i].Score < 0.001)
                        list[i].Score = 0.001;
                    else
                        list[i].Score = Math.Round(list[i].Score, 3);
                }
            }

            return list;
        }



        public Settings GetRecommendationSeting(Predict_DAO_MCol a)
        {
            Settings st = new Settings();

            try
            {
                List<Recommendation_Setting> listCs = a.GetRecommendationSetting();

                if (null != listCs && listCs.Count > 0)
                    foreach (var item in listCs)
                    {
                        if (item.Key.Equals(ConstantValues.nb_R1))
                        {
                            foreach (var cs_detail in item.Values)
                                if (cs_detail.isDedault.Equals(true))
                                {
                                    st.nbR1 = Convert.ToInt32(cs_detail.Value.Trim());
                                    break;
                                }
                        }
                        else
                            if (item.Key.Equals(ConstantValues.nb_R2))
                            {
                                foreach (var cs_detail in item.Values)
                                    if (cs_detail.isDedault.Equals(true))
                                    {
                                        st.nbR2 = Convert.ToInt32(cs_detail.Value.Trim());
                                        break;
                                    }
                            }
                            else if (item.Key.Equals(ConstantValues.nb_R3))
                            {
                                foreach (var cs_detail in item.Values)
                                    if (cs_detail.isDedault.Equals(true))
                                    {
                                        st.nbR3 = Convert.ToInt32(cs_detail.Value.Trim());
                                        break;
                                    }
                            }
                            else if (item.Key.Equals(ConstantValues.param_R4))
                            {
                                foreach (var cs_detail in item.Values)
                                    if (cs_detail.isDedault.Equals(true))
                                    {
                                        st.paramR4 = Convert.ToDouble(cs_detail.Value.Trim(), CultureInfo.CreateSpecificCulture("en-GB"));
                                        break;
                                    }
                            }
                    }

            }
            catch (Exception ex)
            {
                a.rollbackTransaction();
                throw ex;
            }
            return st;
        }


        #endregion

        public List<ADDGAP> GetListOfNewGAP(Predict_DAO_MCol a)
        {
            List<ADDGAP> lst = new List<ADDGAP>();
            try
            {
                lst = a.GetListOfNewGAP();
            }
            catch (Exception)
            {
                throw;
            }

            return lst;
        }



        public List<QTY_GAP> UpdateQTYGap(Predict_DAO_MCol a, List<ADDGAP> list)
        {
            List<QTY_GAP> lst = new List<QTY_GAP>();
            try
            {

                a.UpdateGAP(list);
                lst = a.GetGAP();
            }
            catch (Exception)
            {

                throw;
            }
            return lst;
        }


        //This function return the final quantity for each pair <User, MetaItem> by using GAP matrix
        int AddGapToQTY(List<QTY_GAP> lst, string UserID, string MetaItemID, int QTY)
        {
            int G = QTY;
            double T = 0;
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].UserID == UserID && lst[i].MetaItemID == MetaItemID)
                {
                    T = lst[i].QTYGAP; break;
                }
            if (T != 0)
                G = (int)(QTY + QTY * (1 + T)) / 2;
            return G;
        }


    }
}