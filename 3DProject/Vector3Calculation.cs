using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _3DProject
{
    public static class Vector3Calculation
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
            double divisor = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            return new MyVector3(vector.X / divisor, vector.Y / divisor, vector.Z / divisor);
        }

        public static double DotProduct(MyVector3 leftVector , MyVector3 rightVector)
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


    }
}
