using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DProject
{
    public static class MatrixCalculation
    {
        public static MyMatrix Multiplication(MyMatrix leftMatrix, MyMatrix rightMatrix)
        {
            MyMatrix resultMatrix = new MyMatrix();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    double sum = 0.0;

                    for (int k = 0; k < 4; k++)
                    {
                        sum += leftMatrix[i, k] * rightMatrix[k, j];
                    }

                    resultMatrix[i, j] = sum;
                }
            }

            return resultMatrix;
        } 
    }
}
