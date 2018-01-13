namespace _3DProject
{
    public class MyMatrix
    {
        private double[,] _values = new double[4,4];

        public MyMatrix()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _values[i, j] = 0.0;
                }
            }
        }

        public double this[int key, int key2]
        {
            get
            {
                if (key < 0 || key2 < 0 || key >= 4 || key2 >= 4)
                {
                    return 0.0;
                }
                return _values[key, key2];
            }

            set
            {
                if (key >= 0 && key2 >= 0 && key < 4 && key2 < 4)
                {
                    _values[key, key2] = value;
                }
            }
        }

    }
}
