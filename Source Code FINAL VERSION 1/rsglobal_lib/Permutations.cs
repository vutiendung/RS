//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace rsglobal_lib
//{
//    class Permutations
//    {
//        private int elementLevel = -1;
//        private int numberOfElements;
//        private int[] permutationValue;
//        private int[] inputSet;
//        public List<int[]> outputSet = new List<int[]>();

//        public Permutations(int[] inputSet)
//        {
//            this.inputSet = inputSet;
//            permutationValue = new int[inputSet.Length];
//            numberOfElements = inputSet.Length;
//            CalcPermutation(0);
//        }

//        public void CalcPermutation(int k)
//        {
//            elementLevel++;
//            permutationValue[k] = elementLevel;

//            if (elementLevel == numberOfElements)
//            {
//                outputSet.Add(OutputPermutation(permutationValue));
//            }
//            else
//            {
//                for (int i = 0; i < numberOfElements; i++)
//                {
//                    if (permutationValue[i] == 0)
//                    {
//                        CalcPermutation(i);
//                    }
//                }
//            }
//            elementLevel--;
//            permutationValue[k] = 0;
//        }

//        private int[] OutputPermutation(int[] value)
//        {
//            List<int> output_item = new List<int>();
//            foreach (int i in value)
//            {
//                output_item.Add(inputSet[i - 1]);
//            }
//            return output_item.ToArray();
//        }
//    }
//}
