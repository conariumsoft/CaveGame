﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement
{
    public static class ByteExtensions
    {


        public static void Set(ref this byte a, int pos, bool value)
        {
            if (value)
                a = (byte)(a | (1 << pos));
            else
                a = (byte)(a & ~(1 << pos));
        }

        public static bool Get(this byte a, int pos)
        {
            return ((a & (1 << pos)) != 0);
        }

        public static byte ReadLowerNibble(this byte src) => (byte)(src & 0xF); // = 0000 0010
        public static byte ReadUpperNibble(this byte src) => (byte)(src >> 4 & 0xF);

        public static byte WriteLowerNibble(this byte src, byte nibble) => (byte)(src & 0xF0 + nibble);
        public static byte WriteUpperNibble(this byte src, byte nibble) => (byte)(src & 0x0F + (nibble << 4));

        public static byte ProtectedSubtract(this byte num, byte sub)
        {

            return (byte)Math.Max(num-sub, 0);
        }

        public static string DumpHex(this byte[] data, int index = 0) => DumpHex(data, index, data.Length);

        public static string DumpHex(this byte[] data, int index, int length)
        {
            StringBuilder bob = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                bob.Append(String.Format("{0:X2}", data[i + index]));
            }
            return bob.ToString();
        }
    }
}