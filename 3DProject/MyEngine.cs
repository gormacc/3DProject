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

        private MyVector3[] allLights = {new MyVector3(0.0f, 10.0f, -5.0f)};

        private MyMatrix transformationMatrix = new MyMatrix();
        private MyMatrix viewMatrix = new MyMatrix();
        private MyMatrix worldMatrix = new MyMatrix();
        private MyMatrix projectionMatrix = new MyMatrix();

        private bool IsGouraurdShading { get; set; } = false;
        private MyVector3 cameraPosition = new MyVector3();

        public MyEngine(WriteableBitmap bitmap)
        {
            this.bitmap = bitmap;
            renderWidth = bitmap.PixelWidth;
            renderHeight = bitmap.PixelHeight;
            renderLength = renderWidth * renderHeight;

            backBuffer = new byte[renderLength * 4];
            depthBuffer = new float[renderLength];
            lockBuffer = new object[renderLength];
            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }

        public void ClearBitmap()
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                SetColorOfPixel(Color.FromRgb(0, 0, 0), index);
            }

            for (var i = 0; i < depthBuffer.Length; i++)
            {
                depthBuffer[i] = float.MaxValue;
            }
        }

        public void ActualizeBitmap()
        {
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
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
            var coordinates = VectorCalculation.MyTransformCoordinate(vertex.Coordinates, transformationMatrix);
            var x = coordinates.X * renderWidth + renderWidth / 2.0f;
            var y = coordinates.Y * renderHeight + renderHeight / 2.0f;

            return new MyVertex
            {
                Coordinates = new MyVector3(x, y, coordinates.Z),
                Normal = VectorCalculation.MyTransformCoordinate(vertex.Normal, worldMatrix),
                WorldCoordinates = VectorCalculation.MyTransformCoordinate(vertex.Coordinates, worldMatrix)
            };
        }

        public void DrawPoint(MyVector3 point, Color color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
            {
                PutPixel((int) point.X, (int) point.Y, point.Z, color);
            }
        }

        private static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private static float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        private static MyVector3 InterpolateVector(MyVector3 leftVector, MyVector3 rightVector, float gradient)
        {
            var alfa = Clamp(gradient);
            MyVector3 vectorL = VectorCalculation.MulitplyVectorByScalar(leftVector, 1.0f - alfa);
            MyVector3 vectorR = VectorCalculation.MulitplyVectorByScalar(rightVector, alfa);

            return VectorCalculation.Addition(vectorL, vectorR);
        }

        private float CalculateShade(MyVector3 normal)
        {
            normal = VectorCalculation.Normalize(normal);
            var sumDotProduct = 0.0f;

            var kd = 1.0f;
            var ks = 0.0f;

            foreach (var light in allLights)
            {
                var camera = VectorCalculation.Normalize(cameraPosition);
                var lightDirection = VectorCalculation.Normalize(light);

                float NdotL = VectorCalculation.DotProduct(normal, lightDirection);
                float diff = kd * NdotL;
                sumDotProduct += Clamp(diff);

                MyVector3 H = VectorCalculation.Normalize(new MyVector3(lightDirection.X + -camera.X,
                    lightDirection.Y + -camera.Y, lightDirection.Z + -camera.Z));
                float NdotH = VectorCalculation.DotProduct(normal, H);
                float spec = ks * (float)(Math.Pow(Clamp(NdotH), 64.0f));

                sumDotProduct += Clamp(spec);
            }

            return Clamp(sumDotProduct);
        }

        public void PrepareFrame(Camera camera, MyVector3[] lights, MyMesh[] meshes)
        {
            allLights = lights;

            cameraPosition = camera.Position;

            viewMatrix = MatrixCalculation.MyLookAtLH(camera.Position, camera.Target, new MyVector3(0, 1, 0));

            projectionMatrix =
                MatrixCalculation.MyPerspectiveForRH(0.78f, (float) bitmap.PixelWidth / bitmap.PixelHeight, 0.01f,
                    1.0f);

            foreach (var mesh in meshes)
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

                    if (IsGouraurdShading)
                    {
                        DrawTriangleGouraurd(vertexA, vertexB, vertexC, mesh.MeshColor);
                    }
                    else
                    {
                        DrawTrianglePhong(vertexA, vertexB, vertexC, mesh.MeshColor);
                    }        
                });
            }
        }

        #region GouraurdShading

        public void DrawTriangleGouraurd(MyVertex v1, MyVertex v2, MyVertex v3, Color color)
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

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            var nl1 = CalculateShade(v1.Normal);
            var nl2 = CalculateShade(v2.Normal);
            var nl3 = CalculateShade(v3.Normal);

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
                    if (y < p2.Y)
                    {
                        var data = new GouraurdScanLineData(nl1, nl3, nl1, nl2, y);
                        ProcessGouraurdScanLine(data, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        var data = new GouraurdScanLineData(nl1, nl3, nl2, nl3, y);
                        ProcessGouraurdScanLine(data, p1, p3, p2, p3, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        var data = new GouraurdScanLineData(nl1, nl2, nl1, nl3, y);
                        ProcessGouraurdScanLine(data, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        var data = new GouraurdScanLineData(nl2, nl3, nl1, nl3, y);
                        ProcessGouraurdScanLine(data, p2, p3, p1, p3, color);
                    }
                }
            }
        }

        

        void ProcessGouraurdScanLine(GouraurdScanLineData data, MyVector3 pa, MyVector3 pb, MyVector3 pc, MyVector3 pd,
            Color color)
        {
            var gradient1 = pa.Y != pb.Y ? (data.CurrentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.CurrentY - pc.Y) / (pd.Y - pc.Y) : 1;

            var sx = (int)Interpolate(pa.X, pb.X, gradient1);
            var ex = (int)Interpolate(pc.X, pd.X, gradient2);

            var z1 = Interpolate(pa.Z, pb.Z, gradient1);
            var z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.Ndotla, data.Ndotlb, gradient1);
            var enl = Interpolate(data.Ndotlc, data.Ndotld, gradient2);

            for (var x = sx; x < ex; x++)
            {
                var gradient = (x - sx) / (float)(ex - sx);
                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);

                DrawPoint(new MyVector3(x, data.CurrentY, z),
                    Color.FromArgb(255, (byte)(color.R * ndotl), (byte)(color.G * ndotl), (byte)(color.B * ndotl)));
            }
        }

        #endregion

        #region PhongShading

        public void DrawTrianglePhong(MyVertex v1, MyVertex v2, MyVertex v3, Color color)
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

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

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
                    if (y < p2.Y)
                    {
                        var data = new PhongScanLineData(v1.Normal, v3.Normal, v1.Normal, v2.Normal, y);
                        ProcessPhongScanLine(data, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        var data = new PhongScanLineData(v1.Normal, v3.Normal, v2.Normal, v3.Normal, y);
                        ProcessPhongScanLine(data, p1, p3, p2, p3, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        var data = new PhongScanLineData(v1.Normal, v2.Normal, v1.Normal, v3.Normal, y);
                        ProcessPhongScanLine(data, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        var data = new PhongScanLineData(v2.Normal, v3.Normal, v1.Normal, v3.Normal, y);
                        ProcessPhongScanLine(data, p2, p3, p1, p3, color);
                    }
                }
            }
        }

        void ProcessPhongScanLine(PhongScanLineData data, MyVector3 pa, MyVector3 pb, MyVector3 pc, MyVector3 pd,
            Color color)
        {
            var gradient1 = pa.Y != pb.Y ? (data.CurrentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.CurrentY - pc.Y) / (pd.Y - pc.Y) : 1;

            var sx = (int)Interpolate(pa.X, pb.X, gradient1);
            var ex = (int)Interpolate(pc.X, pd.X, gradient2);

            var z1 = Interpolate(pa.Z, pb.Z, gradient1);
            var z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var sn = InterpolateVector(data.NormalA, data.NormalB, gradient1);
            var en = InterpolateVector(data.NormalC, data.NormalD, gradient2);

            for (var x = sx; x < ex; x++)
            {
                var gradient = (x - sx) / (float)(ex - sx);
                var z = Interpolate(z1, z2, gradient);
                var normal = InterpolateVector(sn, en, gradient);
                var shade = CalculateShade(normal);

                DrawPoint(new MyVector3(x, data.CurrentY, z),
                    Color.FromArgb(255, (byte)(color.R * shade), (byte)(color.G * shade), (byte)(color.B * shade)));
            }
        }



        #endregion


    }
}
