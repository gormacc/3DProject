using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using _3DProject.Matrix;
using _3DProject.Vector;
using _3DProject._3DObject;

namespace _3DProject
{
    public class MyEngine
    {
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private object[] lockBuffer;
        private WriteableBitmap bitmap;
        private readonly int renderWidth;
        private readonly int renderHeight;
        private readonly int renderLength;

        private MyVector3 lightPosition = new MyVector3(0.0f, 10.0f, -5.0f);

        private MyMatrix transformationMatrix = new MyMatrix();
        private MyMatrix viewMatrix = new MyMatrix();
        private MyMatrix worldMatrix = new MyMatrix();
        private MyMatrix projectionMatrix = new MyMatrix();


        public MyEngine(WriteableBitmap bitmap)
        {
            this.bitmap = bitmap;
            renderWidth = bitmap.PixelWidth;
            renderHeight = bitmap.PixelHeight;
            renderLength = renderWidth * renderHeight;

            backBuffer = new byte[renderLength * 4];
            depthBuffer = new float[renderLength];
            lockBuffer = new object[renderLength];
            for (int i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }

        public void ClearBitmap()
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                SetColorOfPixel(Color.FromRgb(0,0,0), index);
            }

            for(int i = 0; i < depthBuffer.Length; i++)
            {
                depthBuffer[i] = float.MaxValue;
            }
        }

        public void ActualizeBitmap()
        {
            bitmap.WritePixels(new Int32Rect(0,0,bitmap.PixelWidth, bitmap.PixelHeight),
                backBuffer, bitmap.PixelWidth * 4, 0);

            bitmap.Lock();
            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();
        }

        public void PutPixel(int x, int y, float z, Color color)
        {

            var index = (x + y * renderWidth);
            var backBufferIndex = index * 4;

            lock (lockBuffer[index])
            {
                if (depthBuffer[index] < z) return;

                depthBuffer[index] = z;

                SetColorOfPixel(color, backBufferIndex);
            }
        }

        private void SetColorOfPixel(Color color, int index)
        {
            backBuffer[index] = color.B;
            backBuffer[index + 1] = color.G;
            backBuffer[index + 2] = color.R;
            backBuffer[index + 3] = color.A;
        }

        public MyVertex PrepareVertex(MyVertex vertex)
        {
            MyVector3 coordinates = VectorCalculation.MyTransformCoordinate(vertex.Coordinates, transformationMatrix);
            MyVector3 point3DWorld = VectorCalculation.MyTransformCoordinate(vertex.Coordinates, worldMatrix);
            MyVector3 normal3DWorld = VectorCalculation.MyTransformCoordinate(vertex.Normal, worldMatrix);

            var x = coordinates.X * renderWidth + renderWidth / 2.0f;
            var y = coordinates.Y * renderHeight + renderHeight / 2.0f;

            return new MyVertex
            {
              Coordinates = new MyVector3(x, y, coordinates.Z),
              Normal = normal3DWorld,
              WorldCoordinates = point3DWorld
            };
        }

        public void DrawPoint(MyVector3 point, Color color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
            {
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        void ProcessScanLine(ScanLineData data, MyVector3 pa, MyVector3 pb, MyVector3 pc, MyVector3 pd, Color color)
        {
            var gradient1 = pa.Y != pb.Y ? (data.CurrentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.CurrentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.Ndotla, data.Ndotlb, gradient1);
            var enl = Interpolate(data.Ndotlc, data.Ndotld, gradient2);

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);
                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);

                DrawPoint(new MyVector3(x, data.CurrentY, z), Color.FromArgb(255, (byte)(color.R * ndotl), (byte)(color.G * ndotl), (byte)(color.B * ndotl)));
            }
        }

        private float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        public void DrawTriangle(MyVertex v1, MyVertex v2, MyVertex v3, Color color)
        {
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            MyVector3 p1 = v1.Coordinates;
            MyVector3 p2 = v2.Coordinates;
            MyVector3 p3 = v3.Coordinates;

            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal);

            var data = new ScanLineData { };


            float dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < p2.Y)
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl2;
                        ProcessScanLine(data, v1.Coordinates, v3.Coordinates, v1.Coordinates, v2.Coordinates, color);
                    }
                    else
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl2;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v1.Coordinates, v3.Coordinates, v2.Coordinates, v3.Coordinates, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < p2.Y)
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl2;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v1.Coordinates, v2.Coordinates, v1.Coordinates, v3.Coordinates, color);
                    }
                    else
                    {
                        data.Ndotla = nl2;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v2.Coordinates, v3.Coordinates, v1.Coordinates, v3.Coordinates, color);
                    }
                }
            }
        }

        float ComputeNDotL(MyVector3 vertex, MyVector3 normal)
        {
            var lightDirection = VectorCalculation.Substitution(lightPosition, vertex);
            normal = VectorCalculation.Normalize(normal);
            lightDirection = VectorCalculation.Normalize(lightDirection); 
            return Math.Max(0, VectorCalculation.DotProduct(normal, lightDirection));
        }


        public void PrepareFrame(Camera camera, params MyMesh[] meshes)
        {
            viewMatrix = MatrixCalculation.MyLookAtLH(camera.Position, camera.Target, new MyVector3(0, 1, 0));

            projectionMatrix =
                MatrixCalculation.MyPerspectiveForRH(0.78f, (float) bitmap.PixelWidth / bitmap.PixelHeight, 0.01f,
                    1.0f);

            Parallel.ForEach(meshes, mesh =>
            {
                worldMatrix =
                    MatrixCalculation.MyRotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);

                worldMatrix =
                    MatrixCalculation.Multiplication(worldMatrix, MatrixCalculation.MyTranslation(mesh.Position));

                transformationMatrix = MatrixCalculation.Multiplication(worldMatrix, viewMatrix);
                transformationMatrix = MatrixCalculation.Multiplication(transformationMatrix, projectionMatrix);

                Parallel.ForEach(mesh.Faces, face =>
                {
                    var vertexA = mesh.Vertexes[face.A];
                    var vertexB = mesh.Vertexes[face.B];
                    var vertexC = mesh.Vertexes[face.C];

                    vertexA = PrepareVertex(vertexA);
                    vertexB = PrepareVertex(vertexB);
                    vertexC = PrepareVertex(vertexC);

                    DrawTriangle(vertexA, vertexB, vertexC, mesh.MeshColor);
                });
            });
        }
    }
}
