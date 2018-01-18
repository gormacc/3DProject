using System;
using _3DProject.Matrix;

namespace _3DProject.Vector
{
    public static class VectorCalculation
    {
        public static MyVector3 Substitution(MyVector3 leftVector, MyVector3 rightVector)
        {
            return new MyVector3(
                leftVector.X - rightVector.X,
                leftVector.Y - rightVector.Y,
                leftVector.Z - rightVector.Z
                );
        }

        public static MyVector3 Normalize(MyVector3 vector)
        {
            var divisor = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            return new MyVector3(vector.X / divisor, vector.Y / divisor, vector.Z / divisor);
        }

        public static float DotProduct(MyVector3 leftVector , MyVector3 rightVector)
        {
            return (leftVector.X * rightVector.X) + (leftVector.Y * rightVector.Y) + (leftVector.Z * rightVector.Z);
        }

        public static MyVector3 CrossProduct(MyVector3 leftVector, MyVector3 rightVector)
        {
            return new MyVector3(
                leftVector.Y * rightVector.Z - leftVector.Z * rightVector.Y,
                leftVector.Z * rightVector.X - leftVector.X * rightVector.Z,
                leftVector.X * rightVector.Y - leftVector.Y * rightVector.X
            );
        }

        public static MyVector4 MultiplyVectorByMatrix(MyVector4 vector, MyMatrix matrix) 
        {
            var retVector = new MyVector4();

            var sum = 0.0f;

            sum += matrix[0, 0] * vector.X;
            sum += matrix[1, 0] * vector.Y;
            sum += matrix[2, 0] * vector.Z;
            sum += matrix[3, 0] * vector.W;

            retVector.X = sum;

            sum = 0.0f;

            sum += matrix[0, 1] * vector.X;
            sum += matrix[1, 1] * vector.Y;
            sum += matrix[2, 1] * vector.Z;
            sum += matrix[3, 1] * vector.W;

            retVector.Y = sum;

            sum = 0.0f;

            sum += matrix[0, 2] * vector.X;
            sum += matrix[1, 2] * vector.Y;
            sum += matrix[2, 2] * vector.Z;
            sum += matrix[3, 2] * vector.W;

            retVector.Z = sum;

            sum = 0.0f;

            sum += matrix[0, 3] * vector.X;
            sum += matrix[1, 3] * vector.Y;
            sum += matrix[2, 3] * vector.Z;
            sum += matrix[3, 3] * vector.W;

            retVector.W = sum;

            return retVector;
        }

        public static MyVector3 MyTransformCoordinate(MyVector3 vec, MyMatrix matrix)
        {
            var vector4 = MultiplyVectorByMatrix(new MyVector4(vec), matrix);

            return new MyVector3(vector4.X / vector4.W, vector4.Y / vector4.W, vector4.Z / vector4.W);
        }


    }
}
