namespace _3DProject
{
    public class MyVector4
    {
        public double X { get; set; } = 0.0;

        public double Y { get; set; } = 0.0;

        public double Z { get; set; } = 0.0;

        public double W { get; set; } = 0.0;

        public MyVector4()
        {

        }

        public MyVector4(double x, double y, double z, double w = 1.0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public MyVector4(MyVector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
            W = 1.0;
        }
    }
}
