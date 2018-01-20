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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var bmp = new WriteableBitmap(320, 240, 60, 80, PixelFormats.Bgra32, null);

            engine = new MyEngine(bmp);

            FrontBuffer.Source = bmp;

            meshes = ObjectLoader.LoadJSONFile("monkey.babylon");

            foreach (var mesh in meshes)
            {
                mesh.MeshColor = Color.FromRgb(0, 255, 255);
            }

            mera.Position = new MyVector3(0, 0, 10.0f);
            mera.Target = new MyVector3();

            lights = new[]
            {
                new MyVector3(0.0f, 0.0f, -10.0f),
                //new MyVector3(0.0f, -10.0f, -5.0f) 
            };


            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            engine.ClearBitmap();

            foreach (var mesh in meshes)
            {
                mesh.Rotation = new MyVector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
            }
            
            engine.PrepareFrame(mera, lights, meshes);
            engine.ActualizeBitmap();
        }
    }
}
