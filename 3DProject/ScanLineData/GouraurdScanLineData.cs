namespace _3DProject.ScanLineData
{
    public class GouraurdScanLineData
    {
        public int CurrentY = 0;
        public float Ndotla = 0.0f;
        public float Ndotlb = 0.0f;
        public float Ndotlc = 0.0f;
        public float Ndotld = 0.0f;

        public GouraurdScanLineData(float ndotla, float ndotlb, float ndotlc, float ndotld, int currentY)
        {
            CurrentY = currentY;
            Ndotla = ndotla;
            Ndotlb = ndotlb;
            Ndotlc = ndotlc;
            Ndotld = ndotld;
        }
    }
}
