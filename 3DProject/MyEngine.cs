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
        private WriteableBitmap _bitmap;

        public MyEngine(WriteableBitmap bitmap)
        {
            _bitmap = bitmap;

            _backBuffer = new byte[bitmap.PixelWidth * bitmap.PixelHeight * bitmap.Format.BitsPerPixel/8];
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
        }

        public void Present()
        {
            _bitmap.WritePixels(new Int32Rect(0,0,_bitmap.PixelWidth, _bitmap.PixelHeight),
                _backBuffer, _bitmap.PixelWidth * _bitmap.Format.BitsPerPixel/8, 0);

            _bitmap.Lock();
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        public void PutPixel(int x, int y, Color color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * _bitmap.PixelWidth) * 4;

            _backBuffer[index] = color.B;
            _backBuffer[index + 1] = color.G;
            _backBuffer[index + 2] = color.R;
            _backBuffer[index + 3] = color.A;
        }

        public MyVector2 Project(MyVector3 coord, MyMatrix transMat)
        {
            // transforming the coordinates

            MyVector4 vec = Vector3Calculation.MultiplyByMatrix(new MyVector4(coord), transMat);

            MyVector2 point = new MyVector2(vec.X, vec.Y); 
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X * _bitmap.PixelWidth + _bitmap.PixelWidth / 2.0f;
            var y = -point.Y * _bitmap.PixelHeight + _bitmap.PixelHeight / 2.0f;
            return (new MyVector2(x, y));
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(MyVector2 point)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < _bitmap.PixelWidth && point.Y < _bitmap.PixelHeight)
            {
                // Drawing a yellow point
                PutPixel((int)point.X, (int)point.Y, Color.FromRgb(255, 0, 0));
            }
        }

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

                foreach (var vertex in mesh.Vertexes)
                {
                    // First, we project the 3D coordinates into the 2D space
                    var point = Project(vertex, transformatioMatrix);
                    // Then we can draw on screen
                    DrawPoint(point);
                }
            }
        }
    }
}
