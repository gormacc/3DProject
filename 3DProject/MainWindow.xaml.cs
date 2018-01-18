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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap(320, 240, 60, 80, PixelFormats.Bgra32, null);

            engine = new MyEngine(bmp);

            FrontBuffer.Source = bmp;

            meshes = ObjectLoader.LoadJSONFile("monkey.babylon");

            foreach (var mesh in meshes)
            {
                mesh.MeshColor = Color.FromRgb(0, 255, 0);
            }

            mera.Position = new MyVector3(0, 0, 10.0f);
            mera.Target = new MyVector3();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            engine.ClearBitmap();

            foreach (var mesh in meshes)
            {
                mesh.Rotation = new MyVector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
            }
            
            engine.PrepareFrame(mera, meshes);
            engine.ActualizeBitmap();
        }
    }
}
