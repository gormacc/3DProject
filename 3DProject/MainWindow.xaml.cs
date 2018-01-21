﻿using System;
using System.Collections.Generic;
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
        private List<MyMesh> meshes;
        Camera camera = new Camera();
        private Light[] lights;
        private int sign = 1;
        private bool snowflakeCamera;
        private bool snowflakeTargetedCamera;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var bmp = new WriteableBitmap(320, 240, 60, 80, PixelFormats.Bgra32, null);
            engine = new MyEngine(bmp);
            FrontBuffer.Source = bmp;

            meshes = ObjectLoader.LoadJSONFile("wholeScene.babylon");
            //AddGroundMesh();
            InitializeMeshes(meshes);

            SetDefaultCamera(new object(), null);
            SetDefaultLightningAndShading();
            SetLights();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            FpsCounter();

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
                    var z = position.Z + (sign * 0.03f);
                    mesh.Position = new MyVector3(x, position.Y, z);

                    if (snowflakeCamera)
                    {
                        var cameraPosition = camera.Position;
                        var xC = -sign * (float)Math.Sqrt(4 - position.Z * position.Z);
                        var zC = position.Z + (sign * 0.02f);
                        camera.Position = new MyVector3(xC, cameraPosition.Y, zC);
                    }

                    if (snowflakeTargetedCamera)
                    {
                        camera.Target = mesh.Position;
                    }
                    
                }
            }
            
            engine.PrepareFrame(camera, lights, meshes);
            engine.ActualizeBitmap();
        }

        private DateTime previousDate = DateTime.Now;

        private void FpsCounter()
        {
            // Fps
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - previousDate).TotalMilliseconds;
            previousDate = now;

            FpsTextBlock.Text = $"{currentFps:0.00} fps";
        }

        #region Initialization


        private void SetLights()
        {
            lights = new[]
            {
                new Light()
                {
                    Position = new MyVector3(10.0f, 3.0f, 0.0f),
                    IsActive = false
                },

                new Light()
                {
                    Position = new MyVector3(0.0f, 8.0f, -10.0f),
                    IsActive = false
                },

                new Light()
                {
                    Position = new MyVector3(0.0f, 8.0f, 0.0f),
                    IsActive = false
                },
            };

            LightOneCheckBox.IsChecked = true;
        }

        private void SetDefaultLightningAndShading()
        {
            SetBlinnLightningRadioButton.IsChecked = true;
            SetGouraudShadingRadioButton.IsChecked = true;
        }

        private void InitializeMeshes(List<MyMesh> meshes)
        {
            Random rand = new Random();

            foreach (var mesh in meshes)
            {
                switch (mesh.Name)
                {
                    case "Ground":
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


        private void AddGroundMesh()
        {
            MyMesh ground = new MyMesh("Ground", 4, 2);

            MyVertex vertexA = new MyVertex()
            {
                Coordinates = new MyVector3(-5, -1, -10)
            };

            MyVertex vertexB = new MyVertex()
            {
                Coordinates = new MyVector3(5, -1, -10)
            };

            MyVertex vertexC = new MyVertex()
            {
                Coordinates = new MyVector3(5, -1, 10)
            };

            MyVertex vertexD = new MyVertex()
            {
                Coordinates = new MyVector3(-5, 0, 10)
            };

            vertexA.Normal = VectorCalculation.CrossProduct(
                VectorCalculation.Substitution(vertexD.Coordinates, vertexA.Coordinates),
                VectorCalculation.Substitution(vertexB.Coordinates, vertexA.Coordinates));

            vertexD.Normal = VectorCalculation.CrossProduct(
                VectorCalculation.Substitution(vertexC.Coordinates, vertexD.Coordinates),
                VectorCalculation.Substitution(vertexA.Coordinates, vertexD.Coordinates));

            vertexC.Normal = VectorCalculation.CrossProduct(
                VectorCalculation.Substitution(vertexB.Coordinates, vertexC.Coordinates),
                VectorCalculation.Substitution(vertexD.Coordinates, vertexC.Coordinates));

            vertexB.Normal = VectorCalculation.CrossProduct(
                VectorCalculation.Substitution(vertexA.Coordinates, vertexB.Coordinates),
                VectorCalculation.Substitution(vertexC.Coordinates, vertexB.Coordinates));

            MyFace faceA = new MyFace
            {
                A = 0,
                B = 1,
                C = 3
            };

            MyFace faceB = new MyFace
            {
                A = 2,
                B = 1,
                C = 3
            };

            ground.Vertexes[0] = vertexA;
            ground.Vertexes[1] = vertexB;
            ground.Vertexes[2] = vertexC;
            ground.Vertexes[3] = vertexD;

            ground.Faces[0] = faceA;
            ground.Faces[1] = faceB;

            meshes.Add(ground);

        }

        #endregion


        #region GUI

        private void SetDefaultCamera(object sender, RoutedEventArgs e)
        {
            snowflakeCamera = false;
            snowflakeTargetedCamera = false;

            camera.Position = new MyVector3(0.0f, -1.0f, 12.0f);
            camera.Target = new MyVector3(0, 2, 0);
        }

        private void SetSnowflakeTargetedCamera(object sender, RoutedEventArgs e)
        {
            snowflakeCamera = false;
            snowflakeTargetedCamera = true;
            camera.Position = new MyVector3(0.0f, -1.0f, 12.0f);
        }

        private void SetSnowflakePositionedCamera(object sender, RoutedEventArgs e)
        {
            snowflakeCamera = true;
            snowflakeTargetedCamera = false;
            camera.Target = new MyVector3(0, 2, 0);
        }

        private void SetBlinnLightningModel(object sender, RoutedEventArgs e)
        {
            engine.IsBlinnLightning = true;
        }

        private void SetPhongLightningModel(object sender, RoutedEventArgs e)
        {
            engine.IsBlinnLightning = false;
        }

        private void SetGouraudShading(object sender, RoutedEventArgs e)
        {
            engine.IsGouraurdShading = true;
        }

        private void SetPhongShading(object sender, RoutedEventArgs e)
        {
            engine.IsGouraurdShading = false;
        }


        private void SetLightOneOn(object sender, RoutedEventArgs e)
        {
            lights[0].IsActive = true;
        }

        private void SetLightOneOff(object sender, RoutedEventArgs e)
        {
            lights[0].IsActive = false;
        }

        private void SetLightTwoOn(object sender, RoutedEventArgs e)
        {
            lights[1].IsActive = true;
        }

        private void SetLightTwoOff(object sender, RoutedEventArgs e)
        {
            lights[1].IsActive = false;
        }

        private void SetLightThreeOn(object sender, RoutedEventArgs e)
        {
            lights[2].IsActive = true;
        }

        private void SetLightThreeOff(object sender, RoutedEventArgs e)
        {
            lights[2].IsActive = false;
        }

        #endregion

    }
}
