using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public static class ContainerExtensions
    {
        public static List<T> ToList<T>(this T[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            List<T> ret = new List<T>(width * height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    ret.Add(array[i, j]);
                }
            }
            return ret;
        }
    }
}
