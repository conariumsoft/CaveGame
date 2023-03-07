using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Extensions
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random rng, double probability = 0.5)
        {
            return rng.NextDouble() <= probability;
        }

        public static float NextFloat(this Random rng) => (float)rng.NextDouble();

        public static T OneOf<T>(this Random rng, params T[] things)
        {
            return things[rng.Next(things.Length)];
        }
    }
}
