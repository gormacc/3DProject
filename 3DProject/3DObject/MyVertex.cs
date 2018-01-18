using _3DProject.Vector;

namespace _3DProject._3DObject
{
    public class MyVertex
    {
        public MyVector3 Normal { get; set; } = new MyVector3();

        public MyVector3 Coordinates { get; set; } = new MyVector3();

        public MyVector3 WorldCoordinates { get; set; } = new MyVector3();
    }
}
