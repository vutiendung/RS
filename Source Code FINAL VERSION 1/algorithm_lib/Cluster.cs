using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using rsbo_lib;
using rsglobal_lib;

namespace algorithm_lib
{
    public class Cluster
    {
        private static readonly Random getrandom = new Random();
        public double[][] v_Last;

        // Cluster Algorithms
        public static string CLUSTER_KMEAN = "KMeans";
        public static string CLUSTER_KMEAN_DCA = "KmeansDCA";
        public static string CLUSTER_GKMSSC = "GKMSSC";

        // Setting Value
        public int k;
        public int maxLoop;
        public double[][] x;
        public double epsilon;
        public double sigma;
        public double T;
        public int M;

        public double[][] getV()
        {
            return v_Last;
        }

        public void addSetting(int k,
            int maxLoop,
            double[][] x,
            double epsilon,
            double sigma,
            double T,
            int M)
        {
            this.k = k;
            this.maxLoop = maxLoop;
            this.x = x;
            this.epsilon = epsilon;
            this.sigma = sigma;
            this.T = T;
            this.M = M;
        }

        public static void setDefaultSettings(int k, int maxLoop, double epsilon, double sigma, double T, string cluster_type)
        {
            k = 3;
            maxLoop = 3;
            epsilon = Math.Pow(10, -6);
            sigma = 4.4;
            T = 0.001;
            cluster_type = CLUSTER_KMEAN;
        }

        private static double[][] Normalized(double[][] rawData)
        {
            double[][] result = cloneMatrix(rawData);

            for (int j = 0; j < result[0].Length; ++j) // each col
            {
                double colSum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                double mean = colSum / result.Length;
                double sum = 0.0;
                for (int i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                double sd = sum / result.Length;
                for (int i = 0; i < result.Length; ++i)
                    if (sd != 0)
                        result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }

        public Dictionary<int, int> Clustering(string type)
        {
            Dictionary<int, int> clusteredData = new Dictionary<int, int>();
            if (null != x && x.Length > 0)
            {
                
                    k = Convert.ToInt32(Math.Sqrt(Convert.ToDouble(x.Length) / 2.0));
                    if (k < 1) k = 1;
                    
                //cuong sua here
                    if (k == 1)
                        clusteredData = byKmeans();
                    else

                    if (k != 1)
                    {
                        if (type.Equals(CLUSTER_KMEAN))
                            clusteredData = byKmeans();
                        else if (type.Equals(CLUSTER_GKMSSC))
                            clusteredData = byGKMSSC();
                        else if (type.Equals(CLUSTER_KMEAN_DCA))
                            clusteredData = byKmeansDCA();

                        clusteredData = MergeGroup(clusteredData);
                        recomputeV(v_Last, x, clusteredData);

                        //reOrder(ref v_Last, clusteredData);
                        k = v_Last.Length;
                    }
                    //else
                    //{
                    //    for (int i = 0; i < x.Length; i++)
                    //        clusteredData.Add(i, 0);
                    //        recomputeV(v_Last, x, clusteredData);
                    //}
                
            }
            return clusteredData;
        }

        public Dictionary<int, int> Clustering(string type, double[][] v)
        {
            Dictionary<int, int> clusteredData = new Dictionary<int, int>();
            if (null != x && x.Length > 0 && v.Length > 0)
            {
                k = v.Length;

                if (type.Equals(CLUSTER_KMEAN))
                    clusteredData = byKmeans(v);
                else if (type.Equals(CLUSTER_GKMSSC))
                    clusteredData = byGKMSSC(v);
                else if (type.Equals(CLUSTER_KMEAN_DCA))
                    clusteredData = byKmeansDCA();
            }
            return clusteredData;
        }

        public Dictionary<int, int> Clustering_Merge(string type, double[][] v)
        {
            Dictionary<int, int> clusteredData = Clustering(type, v);

            if (clusteredData.Count > 0)
            {
                clusteredData = MergeGroup(clusteredData);
                recomputeV(v_Last, x, clusteredData);
                k = v_Last.Length;
            }

            return clusteredData;
        }

        public Dictionary<int, int> Clustering_Merge(string type)
        {
            Dictionary<int, int> clusteredData = Clustering(type);

            if (clusteredData.Count > 0)
            {
                clusteredData = MergeGroup(clusteredData);
                recomputeV(v_Last, x, clusteredData);
                k = v_Last.Length;
            }

            return clusteredData;
        }

        private void reOrder(ref double[][] v_Last, Dictionary<int, int> clusteredData)
        {
            List<int> clData = new List<int>();
            foreach (var item in clusteredData)
                clData.Add(item.Value);

            int minus = 0;
            List<double[]> newCentroid = new List<double[]>();
            if (!Double.NaN.Equals(v_Last[0][0]))
                newCentroid.Add(v_Last[0]);
            else
                minus++;

            for (int i = 1; i < v_Last.Length; i++)
                if (!Double.NaN.Equals(v_Last[i][0]))
                {
                    if (Double.NaN.Equals(v_Last[i - 1][0]))
                        ChangeCentroidIndex(clData, i, minus); // Change Centroid Index 
                    else if (minus > 0)
                        ChangeCentroidIndex(clData, i, minus); // Change Centroid Index 

                    newCentroid.Add(v_Last[i]);
                }
                else if (Double.NaN.Equals(v_Last[i - 1][0]))
                    minus++;

            Dictionary<int, int> newData = new Dictionary<int, int>();
            for (int i = 0; i < clData.Count; i++)
                newData.Add(i, clData[i]);

            clusteredData = newData;
            v_Last = newCentroid.ToArray();
        }

        private void ChangeCentroidIndex(List<int> clData, int index, int minus)
        {
            for (int i = 0; i < clData.Count; i++)
                if (clData[i] == index)
                    clData[i] -= minus;
        }

        private Dictionary<int, int> MergeGroup(Dictionary<int, int> clusteredData)
        {
            Dictionary<int, int> newData = new Dictionary<int, int>();
            List<int> clData = new List<int>();

            int[] lstCountElement = new int[v_Last.Length];
            List<int> lstDesGroup = new List<int>();
            List<int> lstSmallGroup = new List<int>();

            foreach (var item in clusteredData)
            {
                lstCountElement[item.Value]++;
                clData.Add(item.Value);
            }

            for (int i = 0; i < v_Last.Length; i++)
                if (lstCountElement[i] > M / 2)
                    lstDesGroup.Add(i);
                else
                    lstSmallGroup.Add(i);

            if (lstDesGroup.Count > 0 & lstSmallGroup.Count > 0)
            {
                for (int i = 0; i < clData.Count; i++)
                {
                    bool isSmallGroup = false;
                    for (int j = 0; j < lstSmallGroup.Count; j++)
                        if (clData[i] == lstSmallGroup[j])
                        {
                            isSmallGroup = true;
                            break;
                        }

                    if (isSmallGroup)
                    {
                        int newGroup = lstDesGroup[0];
                        double min = Distance(v_Last[0].Length, x[i], v_Last[lstDesGroup[0]]);
                        for (int j = 0; j < lstDesGroup.Count; j++)
                        {
                            double distance = Distance(v_Last[0].Length, x[i], v_Last[lstDesGroup[j]]);
                            if (distance < min)
                            {
                                newGroup = lstDesGroup[j];
                                min = distance;
                            }
                        }

                        clData[i] = newGroup;
                    }
                }
            }
            else
                return clusteredData;

            for (int i = 0; i < clData.Count; i++)
                newData.Add(i, clData[i]);


            return newData;
        }

        private Dictionary<int, int> byGKMSSC()
        {
            G_KMSSC GKMSSC = new G_KMSSC();
            GKMSSC.addSetting(k, maxLoop, Normalized(x), epsilon, sigma, T, M);
            GKMSSC.run();
            v_Last = GKMSSC.getV();
            return GKMSSC.get_Clustered_Data();
        }

        private Dictionary<int, int> byGKMSSC(double[][] v)
        {
            G_KMSSC GKMSSC = new G_KMSSC();
            GKMSSC.addSetting(k, maxLoop, x, epsilon, sigma, T, M);
            GKMSSC.run(v);
            v_Last = GKMSSC.getV();
            return GKMSSC.get_Clustered_Data();
        }

        private Dictionary<int, int> byKmeans()
        {
            K_means k_mean = new K_means();
            //k_mean.addSetting(k, maxLoop, Normalized(x), epsilon, sigma, T, M);
            k_mean.addSetting(k, maxLoop, x, epsilon, sigma, T, M);
            k_mean.run();
            v_Last = k_mean.getV();
            return k_mean.get_Clustered_Data();
        }

        private Dictionary<int, int> byKmeans(double[][] v)
        {
            K_means k_mean = new K_means();
            //k_mean.addSetting(k, maxLoop, Normalized(x), epsilon, sigma, T, M);
            k_mean.addSetting(k, maxLoop, x, epsilon, sigma, T, M);
            k_mean.run(v);
            v_Last = k_mean.getV();
            return k_mean.get_Clustered_Data();
        }

        private Dictionary<int, int> byKmeansDCA()
        {
            K_means_DCA k_mean_DCA = new K_means_DCA();
            k_mean_DCA.addSetting(k, maxLoop, x, epsilon, sigma, T, M);
            k_mean_DCA.run();
            v_Last = k_mean_DCA.getV();
            return k_mean_DCA.get_Clustered_Data();
        }

        private Dictionary<int, int> byKmeansDCA(double[][] v)
        {
            K_means_DCA k_mean_DCA = new K_means_DCA();
            //k_mean.addSetting(k, maxLoop, Normalized(x), epsilon, sigma, T, M);
            k_mean_DCA.addSetting(k, maxLoop, x, epsilon, sigma, T, M);
            k_mean_DCA.run(v);
            v_Last = k_mean_DCA.getV();
            return k_mean_DCA.get_Clustered_Data();
        }

        #region Create Initialization Vector

        /// <summary>
        /// Generate V[l=0] by radom x[rd]
        /// </summary>
        /// get V init by geting the random points
        /// 
        public static double[][] CREATE_V_Init_01(double[][] x, int k)
        {
            double[][] v = new double[k][];
            for (int i = 0; i < k; i++)
            {
                v[i] = new double[x[0].Length];
            }

            for (int i = 0; i < k; i++)
            {
                int rd = getrandom.Next(0, x.Length);
                int count = 0;
                while (checkDuplicate(x, rd, v, count, i))
                {
                    rd = getrandom.Next(0, x.Length);
                    count++;
                }
                for(int j=0; j<x[0].Length; j++)
                v[i][j] = x[rd][j];

            }

            v = removeBlankVector(v);
            return v;
        }


        public static double[][] CREATE_V_Init_02(double[][] x, int k)
        {
            double[][] v = new double[k][];
            for (int i = 0; i < k; i++)
            {
                v[i] = new double[x[0].Length];
            }

            for (int i = 0; i < k; i++)
            for(int j=0; j<x[0].Length; j++)
                v[i][j] = (double) getrandom.Next(0, 1000000)/1000000.0;
            return v;
        }

        private static bool checkDuplicate(double[][] x, int rd, double[][] v, int count, int existV)
        {
            if (count > x.Length)
                return false;

            for (int i = 0; i < existV; i++)
            {
                bool duplicate = true;
                for (int j = 0; j < v[0].Length; j++)
                {
                    if (v[i][j] != x[rd][j])
                    {
                        duplicate = false;
                        break;
                    }
                }
                if (duplicate)
                    return true;
            }
            return false;
        }

        private static double[][] removeBlankVector(double[][] v)
        {
            List<double[]> lst = new List<double[]>();
            foreach (var item in v)
            {
                for (int i = 0; i < v[0].Length; i++)
                {
                    if (item[i] != 0)
                    {
                        lst.Add(item);
                        break;
                    }
                }
            }
           
            return lst.ToArray();
        }

        #endregion

        public static Dictionary<int, int> updateClustering(double[][] v, double[][] x)
        {
            Dictionary<int, int> re_clustering_Data = new Dictionary<int, int>();

            for (int i = 0; i < x.Length; i++)
            {
                int group = 0;
                double min = Distance(x[0].Length, x[i], v[0]);
                for (int j = 1; j < v.Length; j++)
                {
                    double distance = Distance(x[0].Length, x[i], v[j]);
                    if (distance < min)
                    {
                        group = j;
                        min = distance;
                    }
                }
                re_clustering_Data.Add(i, group);
            }

            //int[] Size = new int[v.Length];
            //for (int i = 0; i < v.Length; i++) Size[i] = 0;

            //for (int i = 0; i < x.Length; i++)
            //    Size[re_clustering_Data[i]]++;

            return re_clustering_Data;
        }

        public static void recomputeV(double[][] v, double[][] x, Dictionary<int, int> cdata)
        {
            int[] groupCount = new int[v.Length];
            for (int i = 0; i < x.Length; ++i)
            {
                int cluster = cdata[i];
                ++groupCount[cluster];
            }

            for (int i = 0; i < v.Length; i++)
                for (int j = 0; j < v[0].Length; j++)
                    v[i][j] = 0.0;

            for (int i = 0; i < x.Length; ++i)
            {
                int cluster = cdata[i];
                for (int j = 0; j < x[i].Length; ++j)
                    v[cluster][j] += x[i][j];
            }

            //Cuong sua here
            for(int i=0; i<v.Length; i++)
            if(groupCount[i] !=0)
                for (int j = 0; j < v[i].Length; ++j)
                    v[i][j] = (double) v[i][j]/groupCount[i];

            //Delete the empty group
            //for(int i=0; i<v.Length; i++)
            //    if (groupCount[i] == 0)
            //    { 
            //        k
            //    }
        }

        /// <summary>
        /// Clone Matrix
        /// </summary>
        /// <param name="v">Matrix input</param>
        /// <returns></returns>
        public static double[][] cloneMatrix(double[][] raw)
        {
            double[][] result = new double[raw.Length][];
            for (int i = 0; i < raw.Length; ++i)
            {
                result[i] = new double[raw[i].Length];
                Array.Copy(raw[i], result[i], raw[i].Length);
            }

            return result;
        }

        public static int[] cloneArray(int[] raw)
        {
            int[] result = new int[raw.Length];
            Array.Copy(raw, result, raw.Length);

            return result;
        }

        /// <summary>
        /// Compute Diatance Between Two Matrix
        /// </summary>
        public static double Distance2Matrix(int k,
            int d,
            double[][] v,
            double[][] v_before)
        {
            double distance = 0.0;
            for (int i = 0; i < k; i++)
                for (int j = 0; j < d; j++)
                {
                    distance += (v[i][j] - v_before[i][j]) * (v[i][j] - v_before[i][j]);
                }
            return Math.Sqrt(distance);
        }

        /// <summary>
        /// Square Distance Between Two Vector
        /// </summary>
        public static double Distance(int d, double[] p1, double[] p2)
        {
            double distance = 0.0;
            for (int i = 0; i < d; i++)
            {
                distance += Math.Pow((p1[i] - p2[i]), 2);
            }
            return Math.Sqrt(distance);
        }

        public static double SquareDistanceEuclidean(int d, double[] p1, double[] p2)
        {
            double distance = 0.0;
            for (int i = 0; i < d; i++)
            {
                distance += Math.Pow((p1[i] - p2[i]), 2);
            }
            return distance;
        }

        public static double[][] CREATE_V_Bar_Init_00(double[][] v)
        {
            double[][] v_bar = new double[v.Length][];
            for (int i = 0; i < v.Length; i++)
            {
                v_bar[i] = new double[v[0].Length];
            }
            return v_bar;
        }

    }
}