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

        public static MyMatrix MyLookAtLH(MyVector3 cameraPosition, MyVector3 cameraTarget, MyVector3 upVector)
        {
            MyVector3 zaxis =
                Vector3Calculation.Normalize(Vector3Calculation.Substitution(cameraTarget, cameraPosition));

            MyVector3 xaxis = Vector3Calculation.Normalize(Vector3Calculation.CrossProduct(upVector, zaxis));

            MyVector3 yaxis = Vector3Calculation.CrossProduct(zaxis, xaxis);

            return new MyMatrix
            {
                [0, 0] = xaxis.X,
                [0, 1] = yaxis.X,
                [0, 2] = zaxis.X,

                [1, 0] = xaxis.Y,
                [1, 1] = yaxis.Y,
                [1, 2] = zaxis.Y,

                [2, 0] = xaxis.Z,
                [2, 1] = yaxis.Z,
                [2, 2] = zaxis.Z,

                [3, 0] = -Vector3Calculation.DotProduct(xaxis, cameraPosition),
                [3, 1] = -Vector3Calculation.DotProduct(yaxis, cameraPosition),
                [3, 2] = -Vector3Calculation.DotProduct(zaxis, cameraPosition),
                [3, 3] = 1.0
            };

        }

        public static MyMatrix MyPerspectiveForRH(float fieldOfViewY, float aspectRatio, float znearPlane,
            float zfarPlane)
        {
            double val = fieldOfViewY / 2;
            double h = Math.Cos(val) / Math.Sin(val);

            double w = aspectRatio * h;

            return new MyMatrix
            {
                [0, 0] = w,

                [1, 1] = h,

                [2, 2] = zfarPlane / (znearPlane - zfarPlane),
                [2, 3] = -1,

                [3, 2] = znearPlane * zfarPlane / (znearPlane - zfarPlane)
            };
        }

        public static MyMatrix MyRotationYawPitchRoll(double yaw, double pitch, double roll)
        {
            double cosa = Math.Cos(pitch);
            double sina = Math.Sin(pitch);

            double cosb = Math.Cos(yaw);
            double sinb = Math.Sin(yaw);

            double cosy = Math.Cos(roll);
            double siny = Math.Sin(roll);

            MyMatrix rotX = new MyMatrix
            {
                [0, 0] = 1.0,

                [1, 1] = cosa,
                [1, 2] = -sina,

                [2, 1] = sina,
                [2 , 2] = cosa,

                [3, 3] = 1.0
            };

            MyMatrix rotY = new MyMatrix
            {
                [0, 0] = cosb,
                [0, 2] = sinb,

                [1, 1] = 1.0,

                [2, 0] = -sinb,
                [2, 2] = cosb,

                [3, 3] = 1.0
            };

            MyMatrix rotZ = new MyMatrix
            {
                [0, 0] = cosy,
                [0, 1] = -siny,

                [1, 0] = siny,
                [1, 1] = cosy,

                [2, 2] = 1.0,

                [3, 3] = 1.0
            };

            MyMatrix resultMatrix = Multiplication(rotZ, rotX);
            resultMatrix = Multiplication(resultMatrix, rotY);
            return resultMatrix;
        }

        public static MyMatrix MyTranslation(MyVector3 vector)
        {
            return new MyMatrix
            {
                [0, 0] = 1.0,

                [1, 1] = 1.0,
                
                [2, 2] = 1.0,

                [3, 0] = vector.X,
                [3, 1] = vector.Y,
                [3, 2] = vector.Z,
                [3, 3] = 1.0
            };
        }
    }
}
