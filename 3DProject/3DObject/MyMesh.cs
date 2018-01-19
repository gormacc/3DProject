using System.Windows.Media;
using _3DProject.Vector;

namespace _3DProject._3DObject
{
    public class MyMesh
    {
        public string Name { get; set; }
        public MyVertex[] Vertexes { get; }
        public MyFace[] Faces { get; set; }
        public MyVector3 Position { get; set; } = new MyVector3();
        public MyVector3 Rotation { get; set; } = new MyVector3();
        public Color MeshColor { get; set; } = new Color();
        public bool IsRotating { get; set; } = false;

        public MyMesh(string name, int vertexesCount, int facesCount)
        {
            Vertexes = new MyVertex[vertexesCount];
            Faces = new MyFace[facesCount];
            Name = name;
        }
    }
}
