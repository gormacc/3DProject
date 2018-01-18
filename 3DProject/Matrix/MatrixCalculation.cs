using System;
using _3DProject.Vector;

namespace _3DProject.Matrix
{
    public static class MatrixCalculation
    {
        public static MyMatrix Multiplication(MyMatrix leftMatrix, MyMatrix rightMatrix)
        {
            var resultMatrix = new MyMatrix();

            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    var sum = 0.0f;

                    for (var k = 0; k < 4; k++)
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
            var zaxis =
                VectorCalculation.Normalize(VectorCalculation.Substitution(cameraTarget, cameraPosition));

            var xaxis = VectorCalculation.Normalize(VectorCalculation.CrossProduct(upVector, zaxis));

            var yaxis = VectorCalculation.CrossProduct(zaxis, xaxis);

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

                [3, 0] = -VectorCalculation.DotProduct(xaxis, cameraPosition),
                [3, 1] = -VectorCalculation.DotProduct(yaxis, cameraPosition),
                [3, 2] = -VectorCalculation.DotProduct(zaxis, cameraPosition),
                [3, 3] = 1.0f
            };

        }

        public static MyMatrix MyPerspectiveForRH(float fieldOfViewY, float aspectRatio, float znearPlane,
            float zfarPlane)
        {
            var val = fieldOfViewY / 2;
            var h = (float)(Math.Cos(val) / Math.Sin(val));

            var w = h / aspectRatio;

            return new MyMatrix
            {
                [0, 0] = w,

                [1, 1] = h,

                [2, 2] = zfarPlane / (znearPlane - zfarPlane),
                [2, 3] = -1,

                [3, 2] = znearPlane * zfarPlane / (znearPlane - zfarPlane)
            };
        }

        public static MyMatrix MyRotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            var cosa = (float)Math.Cos(pitch);
            var sina = (float)Math.Sin(pitch);

            var cosb = (float)Math.Cos(yaw);
            var sinb = (float)Math.Sin(yaw);

            var cosy = (float)Math.Cos(roll);
            var siny = (float)Math.Sin(roll);

            var rotX = new MyMatrix
            {
                [0, 0] = 1.0f,

                [1, 1] = cosa,
                [1, 2] = -sina,

                [2, 1] = sina,
                [2, 2] = cosa,

                [3, 3] = 1.0f
            };

            var rotY = new MyMatrix
            {
                [0, 0] = cosb,
                [0, 2] = sinb,

                [1, 1] = 1.0f,

                [2, 0] = -sinb,
                [2, 2] = cosb,

                [3, 3] = 1.0f
            };

            var rotZ = new MyMatrix
            {
                [0, 0] = cosy,
                [0, 1] = -siny,

                [1, 0] = siny,
                [1, 1] = cosy,

                [2, 2] = 1.0f,

                [3, 3] = 1.0f
            };

            var resultMatrix = Multiplication(rotZ, rotX);
            resultMatrix = Multiplication(resultMatrix, rotY);
            return resultMatrix;
        }

        public static MyMatrix MyTranslation(MyVector3 vector)
        {
            return new MyMatrix
            {
                [0, 0] = 1.0f,

                [1, 1] = 1.0f,
                
                [2, 2] = 1.0f,

                [3, 0] = vector.X,
                [3, 1] = vector.Y,
                [3, 2] = vector.Z,
                [3, 3] = 1.0f
            };
        }
    }
}
