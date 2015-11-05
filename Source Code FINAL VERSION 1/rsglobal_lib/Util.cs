using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using rsbo_lib;

namespace rsglobal_lib
{
    public class Util
    {
        /// <summary>
        /// FindKeyByValue into Dictionary object
        /// </summary>
        public static TKey FindKeyByValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TValue value)
        {
            if (dictionary != null)
            {
                foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                    if (value.Equals(pair.Value)) return pair.Key;
            }
            throw new Exception("The value is not found in the dictionary");
        }
        //public static int FindKeyByValue(IDictionary<int, int> dictionary, int value)
        //{
        //    if (dictionary != null)
        //    {
        //        foreach (KeyValuePair<int, int> pair in dictionary)
        //            if (value.Equals(pair.Value)) return pair.Key;
        //    }
        //    return 0;
        //}

        public static ScheduleTime convertTimer(string U_Timer)
        {
            ScheduleTime st = new ScheduleTime();
            string[] timer = U_Timer.Split(ConstantValues.DELIMITER);

            if (timer.Length > 1)
            {
                string t = timer[0];
                if (t.Equals(ConstantValues.TIMER_DAILY))
                {
                    try
                    {
                        st.hour = Convert.ToInt32(timer[1]);
                        st.minute = Convert.ToInt32(timer[2]);
                    }
                    catch (Exception) { }
                }
                else if (t.Equals(ConstantValues.TIMER_WEEKLY))
                {
                    st.dayOfWeek = 1;
                    try
                    {
                        string dayOfWeek = timer[1];
                        if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_MONDAY))
                        {
                            st.dayOfWeek = 1;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_TUESDAY))
                        {
                            st.dayOfWeek = 2;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_WEDNESDAY))
                        {
                            st.dayOfWeek = 3;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_THUSDAY))
                        {
                            st.dayOfWeek = 4;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_FRIDAY))
                        {
                            st.dayOfWeek = 5;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_SATURDAY))
                        {
                            st.dayOfWeek = 6;
                        }
                        else if (dayOfWeek.Equals(ConstantValues.TIMER_WEEK_SUNDAY))
                        {
                            st.dayOfWeek = 7;
                        }

                        st.hour = Convert.ToInt32(timer[2]);
                        st.minute = Convert.ToInt32(timer[3]);
                    }
                    catch (Exception) { }
                }
                else if (t.Equals(ConstantValues.TIMER_MONTHLY))
                {
                    st.dayOfMonth = 1;
                    try
                    {
                        st.dayOfMonth = Convert.ToInt32(timer[1]);
                        st.hour = Convert.ToInt32(timer[2]);
                        st.minute = Convert.ToInt32(timer[3]);
                    }
                    catch (Exception) { }
                }
                else if (t.Equals(ConstantValues.TIMER_YEARLY))
                {
                    st.monthOfYear = 1;
                    st.dayOfMonth = 1;
                    try
                    {
                        st.monthOfYear = Convert.ToInt32(timer[1]);
                        st.dayOfMonth = Convert.ToInt32(timer[2]);
                        st.hour = Convert.ToInt32(timer[3]);
                        st.minute = Convert.ToInt32(timer[4]);
                    }
                    catch (Exception) { }
                }
            }
            return st;
        }

        public static DateTime getNextRunningTime(ScheduleTime st, DateTime currentTime)
        {
            DateTime nextTimes = currentTime;
            if (st.monthOfYear > 0)
            {
                // Get next Time for Yearly task
                nextTimes = currentTime.Date.AddDays(-currentTime.Day + 1);
                nextTimes = nextTimes.Date.AddMonths(-currentTime.Month + 1);
                nextTimes = nextTimes.Date.AddMonths(st.monthOfYear - 1);
                nextTimes = nextTimes.AddDays(Convert.ToDouble(st.dayOfMonth) - 1);
                nextTimes = nextTimes.AddHours(Convert.ToDouble(st.hour));
                nextTimes = nextTimes.AddMinutes(Convert.ToDouble(st.minute));
                if (nextTimes < currentTime)
                {
                    nextTimes = nextTimes.AddYears(1);
                }
            }
            else if (st.dayOfMonth > 0)
            {
                // Get next Time for Monthly task
                nextTimes = currentTime.Date.AddDays(-currentTime.Day);
                nextTimes = nextTimes.AddDays(Convert.ToDouble(st.dayOfMonth));
                nextTimes = nextTimes.AddHours(Convert.ToDouble(st.hour));
                nextTimes = nextTimes.AddMinutes(Convert.ToDouble(st.minute));
                if (nextTimes < currentTime)
                {
                    nextTimes = nextTimes.AddMonths(1);
                }
            }
            else if (st.dayOfWeek > 0)
            {
                // Get next Time for Weekly task
                nextTimes = currentTime.Date;
                while (nextTimes.Date.DayOfWeek != DayOfWeek.Monday)
                    nextTimes = nextTimes.AddDays(-1);
                nextTimes = nextTimes.AddDays(Convert.ToDouble(st.dayOfWeek - 1));
                nextTimes = nextTimes.AddHours(Convert.ToDouble(st.hour));
                nextTimes = nextTimes.AddMinutes(Convert.ToDouble(st.minute));
                if (nextTimes < currentTime)
                {
                    nextTimes = nextTimes.AddDays(7);
                }
            }
            else if (st.dayOfWeek == 0
                && st.dayOfMonth == 0)
            {
                // Get next Time for Daily task
                nextTimes = currentTime.Date;
                nextTimes = nextTimes.AddHours(Convert.ToDouble(st.hour));
                nextTimes = nextTimes.AddMinutes(Convert.ToDouble(st.minute));
                if (nextTimes < currentTime)
                {
                    nextTimes = nextTimes.AddDays(1);
                }
            }
            return nextTimes;
        }

        public static double[][] toMatrix_MatrixItem(List<MatrixItem> lstMI,
            out Dictionary<string, int> dic_rows,
            out Dictionary<string, int> dic_columns)
        {
            double[][] rt;
            dic_rows = new Dictionary<string, int>();
            dic_columns = new Dictionary<string, int>();

            // Build Rows and Columns for rating matrix
            int rowNo = 0;
            int columnNo = 0;

            for (int i = 0; i < lstMI.Count; i++)
            {
                string row = lstMI[i].Row;
                string column = lstMI[i].Column;
                if (!dic_rows.ContainsKey(row))
                {
                    dic_rows.Add(row, rowNo);
                    rowNo++;
                }
                if (!dic_columns.ContainsKey(column))
                {
                    dic_columns.Add(column, columnNo);
                    columnNo++;
                }
            }

            // Add data for each item of matrix
            rt = new double[rowNo][];
            for (int i = 0; i < rowNo; i++)
                rt[i] = new double[columnNo];

            for (int i = 0; i < lstMI.Count; i++)
            {
                string userID = lstMI[i].Row;
                string CodeMetaProd = lstMI[i].Column;
                double rate = lstMI[i].Cell;

                int matrixRow = dic_rows[userID];
                int matrixCol = dic_columns[CodeMetaProd];

                rt[matrixRow][matrixCol] = rate;
            }

            return rt;
        }

        /// <summary>
        /// Build Rating Matrix form Transac data
        /// </summary>
        /// <param name="lstTransac">Input data</param>
        /// <param name="dic_users"> Key: userID | Value: Order into matrix </param>
        /// <param name="dic_items"> Key: itemID | Value: Order into matrix </param>
        /// <returns></returns>
        public static double[][] getRatingMatrix_FromTransac(List<Transac> lstTransac,
            out Dictionary<string, int> dic_users,
            out Dictionary<string, int> dic_items)
        {
            // Compute rate of matrix first
            Util.computeRate_RatingMatrix(lstTransac);

            double[][] rt;
            dic_users = new Dictionary<string, int>();
            dic_items = new Dictionary<string, int>();

            // Build Rows and Columns for rating matrix
            int userNo = 0;
            int itemNo = 0;

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string itemID = lstTransac[i].ItemID;
                if (!dic_users.ContainsKey(userID))
                {
                    dic_users.Add(userID, userNo);
                    userNo++;
                }
                if (!dic_items.ContainsKey(itemID))
                {
                    dic_items.Add(itemID, itemNo);
                    itemNo++;
                }
            }

            // Add data for each item of matrix
            rt = new double[userNo][];
            for (int i = 0; i < userNo; i++)
            {
                rt[i] = new double[itemNo];
            }

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string itemID = lstTransac[i].ItemID;
                double rate = lstTransac[i].Rate;

                int matrixRow = dic_users[userID];
                int matrixCol = dic_items[itemID];

                rt[matrixRow][matrixCol] = rate;
            }

            return rt;
        }

        public static double[][] toQuantityMatrix_FromTransac(List<Transac> lstTransac,
            out Dictionary<string, int> dic_users,
            out Dictionary<string, int> dic_items)
        {
            // Compute rate of matrix first
            double[][] rt;
            dic_users = new Dictionary<string, int>();
            dic_items = new Dictionary<string, int>();

            // Build Rows and Columns for rating matrix
            int userNo = 0;
            int itemNo = 0;

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string itemID = lstTransac[i].ItemID;
                if (!dic_users.ContainsKey(userID))
                {
                    dic_users.Add(userID, userNo);
                    userNo++;
                }
                if (!dic_items.ContainsKey(itemID))
                {
                    dic_items.Add(itemID, itemNo);
                    itemNo++;
                }
            }

            // Add data for each item of matrix
            rt = new double[userNo][];
            for (int i = 0; i < userNo; i++)
            {
                rt[i] = new double[itemNo];
            }

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string itemID = lstTransac[i].ItemID;
                int matrixRow = dic_users[userID];
                int matrixCol = dic_items[itemID];
                rt[matrixRow][matrixCol] = lstTransac[i].Quantity;
            }

            return rt;
        }

        /// <summary>
        /// Define Rate value
        /// </summary>
        /// <param name="u_transacs"></param>
        public static void computeRate_RatingMatrix(List<Transac> u_transacs)
        {
            foreach (var item in u_transacs)
            {
                item.Rate = (item.Times + item.Quantity) / 2;
            }
        }
        
        #region ComputeAccuracy

        public static double computeAccuracy(Dictionary<int, int> trainingData, Dictionary<int, int> testData)
        {
            double accuracy = 0;
            try
            {
                List<List<int>> data = group_trainingData_by_type(trainingData);
                List<int> list_test_type = get_test_type(testData);

                // Define result matrix
                int[][] result_matrix = new int[data.Count][];
                for (int i = 0; i < data.Count; i++)
                {
                    result_matrix[i] = set_sesult_matrix_row(testData, data[i], list_test_type);
                }

                // Define array W, H to mark assigned item with value is 1
                int[] H = new int[result_matrix.Length];
                int[] W = new int[result_matrix[0].Length];

                // Assign test_type for each training
                for (int k = 0; k < result_matrix.Length; k++)
                {
                    int Max = -1; int index1 = 0; int index2 = 0;
                    for (int i = 0; i < result_matrix.Length; i++)
                    {
                        for (int j = 0; j < result_matrix[0].Length; j++)
                        {
                            if (result_matrix[i][j] > Max
                                && H[i] != 1 && W[j] != 1)
                            {
                                Max = result_matrix[i][j];
                                index1 = i; index2 = j;
                            }
                        }
                    }
                    accuracy += Max; H[index1] = W[index2] = 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ((accuracy * 100) / testData.Count);
        }

        private static List<int> get_test_type(Dictionary<int, int> testData)
        {
            List<int> list_test_type = new List<int>();
            foreach (var item in testData)
            {
                bool exist = false;
                foreach (var type in list_test_type)
                {
                    if (item.Value == type)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    list_test_type.Add(item.Value);
            }
            return list_test_type;
        }

        private static List<List<int>> group_trainingData_by_type(Dictionary<int, int> trainingData)
        {
            List<List<int>> data = new List<List<int>>();
            List<int> training_type = new List<int>();

            foreach (var trainingData_item in trainingData)
            {
                bool exist = false;
                for (int i = 0; i < training_type.Count; i++)
                {
                    if (trainingData_item.Value == training_type[i])
                    {
                        exist = true;
                        data[i].Add(trainingData_item.Key);
                        break;
                    }
                }
                if (!exist)
                {
                    training_type.Add(trainingData_item.Value);
                    data.Add(new List<int>());
                    data[training_type.Count - 1].Add(trainingData_item.Key);
                }
            }
            return data;
        }

        private static int[] set_sesult_matrix_row(Dictionary<int, int> testData
            , List<int> data
            , List<int> list_test_type)
        {
            int[] row = new int[list_test_type.Count];
            List<List<int>> testData_by_type = new List<List<int>>();
            foreach (var item in list_test_type)
            {
                testData_by_type.Add(new List<int>());
            }
            // Add to testData_by_type
            foreach (var item in data)
            {
                for (int i = 0; i < list_test_type.Count; i++)
                {
                    if (testData[item] == list_test_type[i])
                    {
                        testData_by_type[i].Add(item);
                    }
                }
            }
            // Set row value
            for (int i = 0; i < list_test_type.Count; i++)
            {
                row[i] = testData_by_type[i].Count;
            }
            return row;
        }

        #endregion

        public static double[][] getQuantityMatrix_FromTransac_ByMetaItemID(List<Transac> lstTransac, 
            out Dictionary<string, int> dic_users, 
            out Dictionary<string, int> dic_items)
        {
            // Compute rate of matrix first
            double[][] rt;
            dic_users = new Dictionary<string, int>();
            dic_items = new Dictionary<string, int>();

            // Build Rows and Columns for rating matrix
            int userNo = 0;
            int itemNo = 0;

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string MetaItemID = lstTransac[i].MetaItemID;
                if (!dic_users.ContainsKey(userID))
                {
                    dic_users.Add(userID, userNo);
                    userNo++;
                }
                if (!dic_items.ContainsKey(MetaItemID))
                {
                    dic_items.Add(MetaItemID, itemNo);
                    itemNo++;
                }
            }

            // Add data for each item of matrix
            rt = new double[userNo][];
            for (int i = 0; i < userNo; i++)
                rt[i] = new double[itemNo];

            for (int i = 0; i < lstTransac.Count; i++)
            {
                string userID = lstTransac[i].UserID;
                string CodeMetaProd = lstTransac[i].MetaItemID;
                int matrixRow = dic_users[userID];
                int matrixCol = dic_items[CodeMetaProd];
                rt[matrixRow][matrixCol] = lstTransac[i].Quantity;
            }

            return rt;
        }

        public static void writeTextFile(string content, string location)
        {
            List<string> rs = new List<string>();
            rs.Add(content);
            System.IO.File.WriteAllLines(location, rs.ToArray());
        }

        public static void writeLog(string txt)
        {
            string filePath = @"Log.txt";
            System.IO.StreamWriter ss = new System.IO.StreamWriter(filePath, true);

            ss.WriteLine("\n\n" + DateTime.Now.ToString() + ": " + txt);
            ss.Close();
        }

    }
}
