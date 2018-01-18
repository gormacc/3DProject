namespace _3DProject.Vector
{
    public class MyVector3
    {
        public float X { get; set; } = 0.0f;

        public float Y { get; set; } = 0.0f;

        public float Z { get; set; } = 0.0f;

        public MyVector3()
        {
            
        }

        public MyVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
