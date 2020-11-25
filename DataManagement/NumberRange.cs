using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public interface IRange<T>
    {
        bool Inclusive { get; }
        T UpperLimit { get; }
        T LowerLimit { get; }
        T NormalizeFrom(IRange<T> from, T number);

    }
    static class RangeHelper
    {
        public static string ToString<T>(IRange<T> range)
        {
            if (range.Inclusive)
                return String.Format("Range<{0}>[{1}, {2}]", typeof(T).GetType().Name, range.LowerLimit, range.UpperLimit);
            else
                return String.Format("Range<{0}>({1}, {2})", typeof(T).GetType().Name, range.LowerLimit, range.UpperLimit);
        }
    }

    public struct IntegerRange : IRange<int>
    {
        public static IntegerRange PercentInterval = new IntegerRange(0, 100);

        public bool Inclusive { get; private set; }
        public int LowerLimit { get; private set; }
        public int UpperLimit { get; private set; }
        public int Range => Maximum - Minimum;
        private int Minimum => Inclusive ? LowerLimit : LowerLimit + 1;
        private int Maximum => Inclusive ? UpperLimit : UpperLimit - 1;
        public IntegerRange(int lower, int upper, bool inclusive = false)
        {
            LowerLimit = lower;
            UpperLimit = upper;
            Inclusive = inclusive;
        }
        public int NormalizeFrom(IntegerRange from, int number) => (((number - from.Minimum) * Range) / from.Range) + Minimum;
        int IRange<int>.NormalizeFrom(IRange<int> from, int number) => NormalizeFrom((IntegerRange)from, number);
        public override string ToString() => RangeHelper.ToString(this);
    }

    public struct FloatRange : IRange<float>
    {
        public static FloatRange I_Unit = new FloatRange(0, 1);
        public static FloatRange I_Percentile = new FloatRange(0, 100);
        public static FloatRange I_Negative1_Positive1 = new FloatRange(-1, 1);
        public static FloatRange I_NegativeHalf_PositiveHalf = new FloatRange(-0.5f, 0.5f);

        public bool Inclusive { get; private set; }
        public float LowerLimit { get; private set; }
        public float UpperLimit { get; private set; }
        public float Range => Maximum - Minimum;
        private float Minimum => Inclusive ? LowerLimit : LowerLimit + 1;
        private float Maximum => Inclusive ? UpperLimit : UpperLimit - 1;
        public FloatRange(float lower, float upper, bool inclusive = false)
        {
            LowerLimit = lower;
            UpperLimit = upper;
            Inclusive = inclusive;
        }
        public float NormalizeFrom(FloatRange from, float number) => (((number - from.Minimum) * Range) / from.Range) + Minimum;
        float IRange<float>.NormalizeFrom(IRange<float> from, float number) => NormalizeFrom((FloatRange)from, number);
        public override string ToString() => RangeHelper.ToString(this);
    }
    public struct DoubleRange : IRange<double>
    {
        public static DoubleRange UnitInterval = new DoubleRange(0, 1);
        public bool Inclusive { get; private set; }
        public double LowerLimit { get; private set; }
        public double UpperLimit { get; private set; }
        public double Range => Maximum - Minimum;
        private double Minimum => Inclusive ? LowerLimit : LowerLimit + 1;
        private double Maximum => Inclusive ? UpperLimit : UpperLimit - 1;
        public DoubleRange(double lower, double upper, bool inclusive = false)
        {
            LowerLimit = lower;
            UpperLimit = upper;
            Inclusive = inclusive;
        }
        public double NormalizeFrom(DoubleRange from, double number) => (((number - from.Minimum) * Range) / from.Range) + Minimum;
        double IRange<double>.NormalizeFrom(IRange<double> from, double number) => NormalizeFrom((DoubleRange)from, number);
        public override string ToString() => RangeHelper.ToString(this);
    }
}
