namespace _3DProject.Matrix
{
    public class MyMatrix
    {
        private float[,] values = new float[4,4];

        public MyMatrix()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    values[i, j] = 0.0f;
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
                return values[key, key2];
            }

            set
            {
                if (key >= 0 && key2 >= 0 && key < 4 && key2 < 4)
                {
                    values[key, key2] = value;
                }
            }
        }

    }
}
