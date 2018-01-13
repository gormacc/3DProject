using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3DProject
{
    public class MyEngine
    {
        private byte[] _backBuffer;
        private readonly float[] depthBuffer;
        private WriteableBitmap _bitmap;
        private readonly int renderWidth;
        private readonly int renderHeight;

        public MyEngine(WriteableBitmap bitmap)
        {
            _bitmap = bitmap;
            renderWidth = (int)bitmap.Width;
            renderHeight = (int)bitmap.Height;


            _backBuffer = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            depthBuffer = new float[bitmap.PixelWidth * bitmap.PixelHeight];
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < _backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                _backBuffer[index] = b;
                _backBuffer[index + 1] = g;
                _backBuffer[index + 2] = r;
                _backBuffer[index + 3] = a;
            }

            for(int i = 0; i < depthBuffer.Length; i++)
            {
                depthBuffer[i] = float.MaxValue;
            }
        }

        public void Present()
        {
            _bitmap.WritePixels(new Int32Rect(0,0,_bitmap.PixelWidth, _bitmap.PixelHeight),
                _backBuffer, _bitmap.PixelWidth * _bitmap.Format.BitsPerPixel/8, 0);

            _bitmap.Lock();
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        public void PutPixel(int x, int y, float z, Color color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * _bitmap.PixelWidth);
            var index4 = index * 4;

            if (depthBuffer[index] < z) return;

            depthBuffer[index] = z;

            _backBuffer[index4] = color.B;
            _backBuffer[index4 + 1] = color.G;
            _backBuffer[index4 + 2] = color.R;
            _backBuffer[index4 + 3] = color.A;
        }

        public MyVector3 Project(MyVector3 coord, MyMatrix transMat)
        {
            // transforming the coordinates

            MyVector3 vec = Vector3Calculation.MyTransformCoordinate(coord, transMat);

            MyVector2 point = new MyVector2(vec.X, vec.Y); 
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X * _bitmap.PixelWidth + _bitmap.PixelWidth / 2.0f;
            var y = -point.Y * _bitmap.PixelHeight + _bitmap.PixelHeight / 2.0f;
            return (new MyVector3(x, y, vec.Z));
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(MyVector3 point, Color color)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < _bitmap.PixelWidth && point.Y < _bitmap.PixelHeight)
            {
                // Drawing a yellow point
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        void ProcessScanLine(int y, MyVector3 pa, MyVector3 pb, MyVector3 pc, MyVector3 pd, Color color)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                DrawPoint(new MyVector3(x, y, z), color);
            }
        }

        private float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        private float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        public void DrawTriangle(MyVector3 p1, MyVector3 p2, MyVector3 p3, Color color)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            // computing lines' directions
            float dP1P2, dP1P3;

            // Computing slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(y, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p1, p3, p2, p3, color);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        ProcessScanLine(y, p2, p3, p1, p3, color);
                    }
                }
            }
        }

        //public void DrawBline(MyVector2 point0, MyVector2 point1)
        //{
        //    int x0 = (int)point0.X;
        //    int y0 = (int)point0.Y;
        //    int x1 = (int)point1.X;
        //    int y1 = (int)point1.Y;

        //    var dx = Math.Abs(x1 - x0);
        //    var dy = Math.Abs(y1 - y0);
        //    var sx = (x0 < x1) ? 1 : -1;
        //    var sy = (y0 < y1) ? 1 : -1;
        //    var err = dx - dy;

        //    while (true)
        //    {
        //        DrawPoint(new MyVector3(x0, y0), );

        //        if ((x0 == x1) && (y0 == y1)) break;
        //        var e2 = 2 * err;
        //        if (e2 > -dy) { err -= dy; x0 += sx; }
        //        if (e2 < dx) { err += dx; y0 += sy; }
        //    }
        //}

        public void Render(Camera camera, params MyMesh[] meshes)
        {
            MyMatrix viewMatrix = MatrixCalculation.MyLookAtLH(camera.Position, camera.Target, new MyVector3(0, 1, 0));

            MyMatrix projectionMatrix =
                MatrixCalculation.MyPerspectiveForRH(0.78f, (float) _bitmap.PixelWidth / _bitmap.PixelHeight, 0.01f,
                    1.0f);

            foreach (var mesh in meshes)
            {
                MyMatrix worldMatrix =
                    MatrixCalculation.MyRotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);

                worldMatrix =
                    MatrixCalculation.Multiplication(worldMatrix, MatrixCalculation.MyTranslation(mesh.Position));

                MyMatrix transformatioMatrix = MatrixCalculation.Multiplication(worldMatrix, viewMatrix);
                transformatioMatrix = MatrixCalculation.Multiplication(transformatioMatrix, projectionMatrix);

                int faceIndex = 0;

                foreach (var face in mesh.Faces)
                {
                    var vertexA = mesh.Vertexes[face.A];
                    var vertexB = mesh.Vertexes[face.B];
                    var vertexC = mesh.Vertexes[face.C];

                    var pixelA = Project(vertexA, transformatioMatrix);
                    var pixelB = Project(vertexB, transformatioMatrix);
                    var pixelC = Project(vertexC, transformatioMatrix);

                    var val = faceIndex * 11;
                    byte valByte = (byte)val;

                    var color = Color.FromArgb(255, valByte,valByte , valByte);
                    DrawTriangle(pixelA, pixelB, pixelC, color);
                    faceIndex++;
                }
            }
        }
    }
}
