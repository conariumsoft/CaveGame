using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public static class MathematicsExtensions
    {
        public static float Lerp(this float a, float b, float alpha)=> a + (b - a) * alpha;

        // positive modulus
        public static int Mod(this int num, int remainder) => (num % remainder + remainder) % remainder;

        public static int Max(this int num, int max) => Math.Max(num, max);
        public static float Max(this float num, float max) => Math.Max(num, max);
        public static double Max(this double num, double max) => Math.Max(num, max);
    }
}
