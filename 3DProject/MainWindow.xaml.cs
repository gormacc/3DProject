using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using _3DProject.Vector;
using _3DProject._3DObject;

namespace _3DProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();  
        }

        private MyEngine engine;
        private MyMesh[] meshes;
        Camera mera = new Camera();
        private MyVector3[] lights;
        private int sign = 1;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var bmp = new WriteableBitmap(320, 240, 60, 80, PixelFormats.Bgra32, null);

            engine = new MyEngine(bmp);

            FrontBuffer.Source = bmp;

            meshes = ObjectLoader.LoadJSONFile("wholeScene8.babylon");

            InitializeMeshes(meshes);

            mera.Position = new MyVector3(0.0f, -5.0f, 20.0f);
            mera.Target = new MyVector3();

            lights = new[]
            {
                new MyVector3(20.0f, 0.0f, -10.0f),
                //new MyVector3(-20.0f, 10.0f, 0) 
            };


            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            engine.ClearBitmap();

            foreach (var mesh in meshes)
            {
                if (mesh.IsRotating)
                {
                    mesh.Rotation = new MyVector3(mesh.Rotation.X + 0.1f, mesh.Rotation.Y + 0.1f, mesh.Rotation.Z);

                    var position = mesh.Position;

                    if (position.Z >= 3 || position.Z <= -3)
                    {
                        sign = -sign;
                    }

                    var x = -sign * (float)Math.Sqrt(9 - position.Z * position.Z);
                    var z = position.Z + (sign * 0.02f);
                    mesh.Position = new MyVector3(x, position.Y, z);
                }
            }
            
            engine.PrepareFrame(mera, lights, meshes);
            engine.ActualizeBitmap();
        }


        private void InitializeMeshes(MyMesh[] meshes)
        {
            Random rand = new Random();

            foreach (var mesh in meshes)
            {
                switch (mesh.Name)
                {
                    case "Ground":
                    {
                        mesh.MeshColor = Color.FromRgb(255,255,255);
                        break;
                    }
                    case "Ground2":
                    {
                        mesh.MeshColor = Color.FromRgb(255, 255, 255);
                        break;
                    }
                    case "Snowflake":
                    {
                        mesh.MeshColor = Color.FromRgb(255, 255, 255);
                        mesh.Position = new MyVector3(3.0f, 1.5f, 0.0f);
                        mesh.IsRotating = true;
                        break;
                    }
                    case "Root":
                    {
                        mesh.MeshColor = Color.FromRgb(139, 69, 19);
                        break;
                    }
                    case "Tree":
                    {
                        mesh.MeshColor = Color.FromRgb(0, 100, 0);
                        break;
                    }
                    default:
                    {
                        mesh.MeshColor = Color.FromRgb((byte)(rand.Next() % 255), (byte)(rand.Next() % 255), (byte)(rand.Next() % 255));
                        break;
                    }
                        
                }
            }          
        }

    }
}
