namespace _3DProject
{
    public class MyVector4
    {
        public float X { get; set; } = 0.0f;

        public float Y { get; set; } = 0.0f;

        public float Z { get; set; } = 0.0f;

        public float W { get; set; } = 0.0f;

        public MyVector4()
        {

        }

        public MyVector4(float x, float y, float z, float w = 1.0f)
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
            W = 1.0f;
        }
    }
}
