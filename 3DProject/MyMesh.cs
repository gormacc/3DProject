using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DProject
{
    public class MyMesh
    {
        public string Name { get; set; }
        public MyVector3[] Vertexes { get; }
        public MyVector3 Position { get; set; } = new MyVector3();
        public MyVector3 Rotation { get; set; } = new MyVector3();

        public MyMesh(string name, int vertexesCount)
        {
            Vertexes = new MyVector3[vertexesCount];
            Name = name;
        }
    }
}
