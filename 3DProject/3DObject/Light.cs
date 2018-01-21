using _3DProject.Vector;

namespace _3DProject._3DObject
{
    public class Light
    {
        public bool IsActive { get; set; } = false;

        public MyVector3 Position { get; set; } = new MyVector3();
    }
}
