using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public static class ContainerExtensions
    {
        public static IEnumerable<T> DequeueAll<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }
        public static IEnumerable<T> DequeueAll<T>(this Queue<T> queue)
        {
            int max = queue.Count;
            for (int i = 0; i < max && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }
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
