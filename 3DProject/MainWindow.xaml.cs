﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        MyMesh mesh = new MyMesh("Cube", 8);
        Camera mera = new Camera();

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgra32, null);

            engine = new MyEngine(bmp);

            // Our XAML Image control
            FrontBuffer.Source = bmp;

            mesh.Vertexes[0] = new MyVector3(-1, 1, 1);
            mesh.Vertexes[1] = new MyVector3(1, 1, 1);
            mesh.Vertexes[2] = new MyVector3(-1, -1, 1);
            mesh.Vertexes[3] = new MyVector3(-1, -1, -1);
            mesh.Vertexes[4] = new MyVector3(-1, 1, -1);
            mesh.Vertexes[5] = new MyVector3(1, 1, -1);
            mesh.Vertexes[6] = new MyVector3(1, -1, 1);
            mesh.Vertexes[7] = new MyVector3(1, -1, -1);

            mera.Position = new MyVector3(0, 0, 10.0f);
            mera.Target = new MyVector3();

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            engine.Clear(0, 0, 0, 255);

            // rotating slightly the cube during each frame rendered
            mesh.Rotation = new MyVector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);

            // Doing the various matrix operations
            engine.Render(mera, mesh);
            // Flushing the back buffer into the front buffer
            engine.Present();
        }
    }
}
