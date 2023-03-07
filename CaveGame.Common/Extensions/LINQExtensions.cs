using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaveGame.Common.Extensions
{
    public interface IStatistic
    {
        decimal Comparator { get; set; }
    }


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

        public static TStat GetMean<TStat>(this TStat[] collection) where TStat : IStatistic
        {
            //return collection. Average();
            return default;
        }


    }
}
