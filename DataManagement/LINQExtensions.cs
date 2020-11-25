using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public static class LINQExtensions
    {
        // LINQ Extensions, borrowed from Jonathan Skeet
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            foreach (var element in source)
            {
                action(element);
            }
        }
    }
}
