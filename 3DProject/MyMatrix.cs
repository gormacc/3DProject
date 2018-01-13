namespace _3DProject
{
    public class MyMatrix
    {
        private float[,] _values = new float[4,4];

        public MyMatrix()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _values[i, j] = 0.0f;
                }
            }
        }

        public float this[int key, int key2]
        {
            get
            {
                if (key < 0 || key2 < 0 || key >= 4 || key2 >= 4)
                {
                    return 0.0f;
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
