using _3DProject.Vector;

namespace _3DProject.ScanLineData
{
    public class PhongScanLineData
    {
        public int CurrentY = 0;
        public MyVector3 NormalA = new MyVector3();
        public MyVector3 NormalB = new MyVector3();
        public MyVector3 NormalC = new MyVector3();
        public MyVector3 NormalD = new MyVector3();

        public PhongScanLineData(MyVector3 normalA, MyVector3 normalB, MyVector3 normalC, MyVector3 normalD, int currentY)
        {
            NormalA = normalA;
            NormalB = normalB;
            NormalC = normalC;
            NormalD = normalD;
            CurrentY = currentY;
        }
    }
}
