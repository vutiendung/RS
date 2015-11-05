using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace algorithm_lib
{
    public class K_means_DCA : Cluster
    {
        private Dictionary<int, int> clustered_Data = new Dictionary<int, int>();

        public void run()
        {
            int n = x.Length;                               // n is number of user of group
            int d = x[0].Length;                            // d is number of Item of rating matrix
            double[][] v = new double[k][];
            v = CREATE_V_Init_01(x, k);

            for (int i = 0; i < 3; i++)
            {
                int loop = 1;
                K_means k_mean = new K_means();
                k_mean.addSetting(k, loop, x, epsilon, sigma, T, M);
                k_mean.run(v);
                v = k_mean.getV();

                //K_means_DCA k_mean_DCA = new K_means_DCA();
                //k_mean_DCA.addSetting(k, loop, x, epsilon, sigma, T, M);
                //k_mean_DCA.run(v);
                //v = k_mean_DCA.getV();
            }

            Repeat(n, d, v);
        }

        public void run(double[][] v)
        {
            int n = x.Length;                               // n is number of user of group
            int d = x[0].Length;                            // d is number of Item of rating matrix
            Repeat(n, d, v);
        }

        private void Repeat(int n, int d, double[][] v)
        {
            //Console.WriteLine("=========================================================");
            //Console.WriteLine("n      : " + n);
            //Console.WriteLine("d      : " + d);
            //Console.WriteLine("k      : " + k);
            //Console.WriteLine("epsilon: " + epsilon);
            //Console.WriteLine("---------------------------------------------------------");

            double distanceV = Math.Pow(10, 6);
            double distanceO = Math.Pow(10, 6);
            int[] K = new int[n];
            int numIter = 0;
            bool change = true;
            double Optimal_Old;
            //KMEANSDCA_K_Calculated(n, d, v, K);

            while (until(numIter, distanceV, distanceO, change))
            {
                numIter++;
                double[][] v_before = cloneMatrix(v); 
                int[] K_OLD = cloneArray(K);

                Optimal_Old = Optimal_Calculated(n, d, v, K);
                KMEANSDCA_Subgradient(n, d, v_before, v, K);
                double Optimal_NEW = Optimal_Calculated(n, d, v, K);

                distanceV = Distance2Matrix(k, d, cloneMatrix(v), v_before);
                distanceO = Math.Abs(Optimal_NEW - Optimal_Old);
                change = checkChange(K, K_OLD);
                //Console.WriteLine("numIter: " + numIter);
            } 

            for (int i = 0; i < n; i++)
                clustered_Data.Add(i, K[i]);
            v_Last = cloneMatrix(v);
        }

        private bool checkChange(int[] K, int[] K_OLD)
        {
            for (int i = 0; i < K.Length; i++)
                if (K[i] != K_OLD[i])
                    return true;

            return false;
        }

        private bool until(int numIter, double distanceV, double distanceO, bool change)
        {
            if (numIter >= maxLoop)
                return false;
            if (distanceV <= epsilon)
                return false;
            if (distanceO <= epsilon)
                return false;
            //if (!change)
            //    return false;

            return true;
        }

        public void KMEANSDCA_Subgradient(int n, int d, double[][] v_old, double[][] v_new, int[] K)
        {
            double[][] delta_v = CREATE_V_Bar_Init_00(v_old);

            KMEANSDCA_K_Calculated(n, d, v_old, K);
            KMEANSDCA_DeltaV_Calculated(n, d, v_old, K, ref delta_v);

            // v calculated
            for (int i = 0; i < k; i++)
                for (int j = 0; j < d; j++)
                    v_new[i][j] = v_old[i][j] - (delta_v[i][j] / n);
        }

        public void KMEANSDCA_K_Calculated(int n, int d, double[][] v, int[] K)
        {
            for (int i = 0; i < n; i++)
            {
                int MinIndex = 0;
                double min = SquareDistanceEuclidean(d, x[i], v[0]);
                for (int j = 1; j < k; j++)
                {
                    double distance = SquareDistanceEuclidean(d, x[i], v[j]);
                    if (distance < min)
                    {
                        min = distance;
                        MinIndex = j;
                    }
                    K[i] = MinIndex;
                }
            }
        }

        public void KMEANSDCA_DeltaV_Calculated(int n, int d, double[][] v, int[] K, ref double[][] delta_v)
        {
            // Initialization
            for (int i = 0; i < k; i++)
                for (int j = 0; j < d; j++)
                    delta_v[i][j] = 0.0;

            // Estimation
            for (int i = 0; i < n; i++)
            {
                int index = K[i];
                for (int j = 0; j < d; j++)
                    delta_v[index][j] += v[index][j] - x[i][j];
            }
        }

        public double Optimal_Calculated(int n, int d, double[][] v, int[] K)
        {
            double result = 0.0;
            for (int i = 0; i < n; i++)
                result += SquareDistanceEuclidean(d, x[i], v[K[i]]);

            return result;
        }

        public Dictionary<int, int> get_Clustered_Data()
        {
            return clustered_Data;
        }

    }
}