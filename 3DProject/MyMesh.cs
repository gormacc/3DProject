using System;
using System.Windows.Media;
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
        public MyFace[] Faces { get; set; }
        public MyVector3 Position { get; set; } = new MyVector3();
        public MyVector3 Rotation { get; set; } = new MyVector3();
        public Color[]  Colors { get; set; }

        public MyMesh(string name, int vertexesCount, int facesCount)
        {
            Vertexes = new MyVector3[vertexesCount];
            Faces = new MyFace[facesCount];
            Name = name;

            InitializeColors(facesCount);
        }

        private void InitializeColors(int colorCount)
        {
            Colors = new Color[colorCount];

            Random rand = new Random();

            for(int i = 0; i < colorCount; i++)
            {
                byte r = (byte)(rand.Next() % 255);
                byte g = (byte)(rand.Next() % 255);
                byte b = (byte)(rand.Next() % 255);

                Colors[i] = Color.FromRgb(r, g, b);
            }
        }
    }
}
