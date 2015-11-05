using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsglobal_lib;

namespace algorithm_lib
{
    public class G_KMSSC : Cluster
    {
        private Dictionary<int, int> clustered_Data = new Dictionary<int, int>();
        private static double rho;

        public G_KMSSC()
        {

        }

        public void run()
        {
            // Initialization
            int n = x.Length;                               // n is number of user of group
            int d = x[0].Length;                            // d is number of Item of rating matrix
            rho = 2 / Math.Pow(sigma, 2);                   // rho
            double[] alpha = new double[d];
            double[] beta = new double[d];
            GKMSSC_buildAlphaBeta(x, d, alpha, beta);       // build Alpha and Beta based on (x)

            double[][] v = new double[k][];
            v = CREATE_V_Init_01(x, k);

            //for (int i = 0; i < 2; i++)
            //{
            //    int loop = 2;
            //    //Run Kmean to get new V
            //    K_means k_mean = new K_means();
            //    k_mean.addSetting(k, loop, x, epsilon, sigma, T, M);
            //    k_mean.run(v);
            //    v = k_mean.getV();

            //    //Run GKMSSC to get new V
            //    G_KMSSC GKMSSC = new G_KMSSC();
            //    GKMSSC.addSetting(k, loop, x, epsilon, sigma, T, M);
            //    GKMSSC.run(v);
            //    v = GKMSSC.getV();
            //}

            // Repeat step
            Repeat(n, d, alpha, beta, v);
        }

        public void run(double[][] v)
        {
            // Initialization
            int n = x.Length;                               // n is number of user of group
            int d = x[0].Length;                            // d is number of Item of rating matrix
            rho = 2 / Math.Pow(sigma, 2);                   // rho
            double[] alpha = new double[d];
            double[] beta = new double[d];
            GKMSSC_buildAlphaBeta(x, d, alpha, beta);              // build Alpha and Beta based on (x)

            Repeat(n, d, alpha, beta, v);
        }

        private void Repeat(int n, int d, double[] alpha, double[] beta, double[][] v)
        {
            int numIter = 0;
            double distanceV = Math.Pow(10, 6);
            double[][] v_before;
            double[][] v_bar;

            Console.WriteLine("=========================================================");
            Console.WriteLine("n      : " + n);
            Console.WriteLine("d      : " + d);
            Console.WriteLine("k      : " + k);
            Console.WriteLine("sigma  : " + sigma);
            Console.WriteLine("epsilon: " + epsilon);
            Console.WriteLine("T      : " + T);
            Console.WriteLine("---------------------------------------------------------");

            while (until(numIter, distanceV))
            {
                numIter++;
                v_before = cloneMatrix(v);
                v_bar = CREATE_V_Bar_Init_00(cloneMatrix(v));
                GKMSSC_Gauss_Subgradient_H(n, d, rho, v, v_bar);
                // Do Proj
                GKMSSC_Proj(n, d, alpha, beta, rho, v, v_bar);
                distanceV = Distance2Matrix(k, d, cloneMatrix(v), v_before);

                Console.WriteLine("KMSSC_Objective: " + GKMSSC_Objective(n,d,k,x,sigma,v));
            }

            // Clustering data (x) based on (v)
            GKMSSC_Generate_Clustered_Data(n, d, v);
            v_Last = cloneMatrix(v);
        }

        private Dictionary<int, int> GKMSSC_Generate_Clustered_Data(int n, int d, double[][] v)
        {
            clustered_Data = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
            {
                int group = 0;
                double min = -Math.Exp(-SquareDistanceEuclidean(d, v[0], x[i]) / Math.Pow(sigma, 2));
                for (int j = 1; j < k; j++)
                {
                    double distance = -Math.Exp(-SquareDistanceEuclidean(d, v[j], x[i]) / Math.Pow(sigma, 2));
                    if (distance < min)
                    {
                        group = j;
                        min = distance;
                    }
                }
                clustered_Data.Add(i, group);
            }
            return clustered_Data;
        }

        public Dictionary<int, int> get_Clustered_Data()
        {
            return clustered_Data;
        }

        /// <summary>
        /// Condition to stop Repeat Step
        /// </summary>
        private bool until(int numIter, double distanceV)
        {
            if (numIter >= maxLoop)
            {
                return false;
            }

            if (distanceV <= epsilon)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Build Upper and Lower bounds List
        /// </summary>
        private void GKMSSC_buildAlphaBeta(double[][] x,
            int d,
            double[] alpha,
            double[] beta)
        {
            double min;
            double max;
            for (int i = 0; i < d; i++)
            {
                min = x[0][i];
                max = x[0][i];
                for (int j = 1; j < x.Length; j++)
                {
                    if (x[j][i] < min) min = x[j][i];
                    if (x[j][i] > max) max = x[j][i];
                }
                alpha[i] = min;
                beta[i] = max;
            }
        }

        /// <summary>
        /// Compute W[l]
        /// </summary>
        private void GKMSSC_Gauss_Subgradient_H(int n, int d, double rho,
                                  double[][] v,
                                  double[][] v_bar)
        {
            double twoSquareSigma = 2.0 * Math.Pow(sigma, 2);
            for (int j = 0; j < n; j++)
            {
                // Find nearest v 
                int q = 0;
                double h = SquareDistanceEuclidean(d, v[0], x[j]);
                double max = 2.0 * Math.Exp(-h / twoSquareSigma);
                for (int i = 1; i < k; i++)
                {
                    h = SquareDistanceEuclidean(d, v[i], x[j]);
                    double value = 2.0 * Math.Exp(-h / twoSquareSigma);
                    if (value > max)
                    {
                        max = value;
                        q = i;
                    }
                }

                // Update v_bar
                double coef = (2.0 / Math.Pow(sigma, 2)) * Math.Exp(-SquareDistanceEuclidean(d, v[q], x[j]) / twoSquareSigma);
                for (int i = 0; i < k; i++)
                {
                    if (i == q)
                        for (int l = 0; l < d; l++) v_bar[i][l] = v_bar[i][l] + rho * v[i][l] - coef * (v[i][l] - x[j][l]) + T * v[i][l];
                    else
                        for (int l = 0; l < d; l++) v_bar[i][l] = v_bar[i][l] + T * v[i][l];
                }
            }
        }

        /// <summary>
        /// Compute V[l+1] by Proj for W[l]
        /// </summary>
        private void GKMSSC_Proj(int n, int d, double[] alpha, double[] beta, double rho, double[][] v, double[][] v_bar)
        {
            double local_coef = (n * rho + T);
            double temp;

            for (int i = 0; i < k; i++)
                for (int l = 0; l < d; l++)
                {
                    temp = v_bar[i][l] / local_coef;
                    if (temp < alpha[l])
                        v[i][l] = alpha[l];
                    else if (temp > beta[l])
                        v[i][l] = beta[l];
                    else
                        v[i][l] = temp;
                }
        }

        private double GKMSSC_Objective(int n, int d, int k, double[][] x,
                              double sigma,
                              double[][] v)
        {
            double result = 0;
            double min;
            double temp;
            double local_coef = (2.0 * sigma * sigma);

            for (int j = 0; j < n; j++)
            {
                min = -2.0 * Math.Exp(-SquareDistanceEuclidean(d, v[0], x[j]) / local_coef);
                for (int i = 1; i < k; i++)
                {
                    temp = -2.0 * Math.Exp(-SquareDistanceEuclidean(d, v[i], x[j]) / local_coef);
                    if (temp < min)
                        min = temp;
                }
                result = result + min;
            }

            return result;
        }
    }
}