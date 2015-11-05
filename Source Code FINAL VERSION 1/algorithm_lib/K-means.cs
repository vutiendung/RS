using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rsglobal_lib;

namespace algorithm_lib
{
    public class K_means : Cluster
    {
        private Dictionary<int, int> clustered_Data = new Dictionary<int, int>();

        public K_means()
        {
        }

        public void run()
        {
            // Initialization
            double[][] v = new double[k][];
            v = CREATE_V_Init_01(cloneMatrix(x), k);
            k = v.Length;

            //Console.WriteLine("=========================================================");
            //Console.WriteLine("n      : " + x.Length);
            //Console.WriteLine("d      : " + x[0].Length);
            //Console.WriteLine("k      : " + k);
            //Console.WriteLine("epsilon: " + epsilon);
            //Console.WriteLine("---------------------------------------------------------");

            Repeat(v);
        }

        public void run(double[][] v)
        {
            Repeat(v);
        }

        private void Repeat(double[][] v)
        {


            int numIter = 0;
            bool changed = true;
            while (numIter < maxLoop && changed)
            {
                numIter++;
                Dictionary<int, int> re_clustered_Data = updateClustering(v, x);
                double[][] v_Before = cloneMatrix(v);
                recomputeV(v, x, re_clustered_Data);
                //changed = checkChange(clustered_Data, re_clustered_Data);
                changed = checkChangeV(v, v_Before);
                clustered_Data = re_clustered_Data;
            }

            v_Last = cloneMatrix(v);
        }

        private bool checkChangeV(double[][] v, double[][] v_Before)
        {
            if (Distance2Matrix(k, x[0].Length, v, v_Before) <= epsilon)
                return false;
            else
                return true;
        }

        private bool checkChange(Dictionary<int, int> clustered_Data, Dictionary<int, int> re_clustered_Data)
        {
            if (clustered_Data.Count == 0)
                return true;
            foreach (var item in re_clustered_Data)
            {
                if (item.Value != clustered_Data[item.Key])
                    return true;
            }
            return false;
        }

        public Dictionary<int, int> get_Clustered_Data()
        {
            return clustered_Data;
        }

    }
}
