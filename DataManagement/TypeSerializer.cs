using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DataManagement
{
    // Endianness-agnostic utility class for converting datatypes into and out of byte arrays
    // Author: jms
    public static class TypeSerializer
    {
        public static char ReadChar(this byte[] data, int index) => ToChar(data, index);
        public static short ReadShort(this byte[] data, int index) => ToShort(data, index);
        /// <summary>
        /// Data Length = 2
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="val"></param>
        public static void WriteShort(this byte[] data, int index, short val) => FromShort(ref data, index, val);
        public static ushort ReadUShort(this byte[] data, int index) => ToUShort(data, index);
        /// <summary>
        /// Data Length = 2
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="val"></param>
        public static void WriteUShort(this byte[] data, int index, ushort val) => FromUShort(ref data, index, val);
        public static int ReadInt(this byte[] data, int index) => ToInt(data, index);
        /// <summary>
        /// Data Length = 4
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="val"></param>
        public static void WriteInt(this byte[] data, int index, int val) => FromInt(ref data, index, val);
        public static uint ReadUInt(this byte[] data, int index) => ToUInt(data, index);
        public static void WriteUInt(this byte[] data, int index, uint val) => FromUInt(ref data, index, val);
        public static long ReadLong(this byte[] data, int index) => ToLong(data, index);
        public static void WriteLong(this byte[] data, int index, long val) => FromLong(ref data, index, val);
        public static ulong ReadULong(this byte[] data, int index) => ToULong(data, index);
        public static void WriteULong(this byte[] data, int index, ulong val) => FromULong(ref data, index, val);
        public static float ReadFloat(this byte[] data, int index) => ToFloat(data, index);
        public static void WriteFloat(this byte[] data, int index, float value) => FromFloat(ref data, index, value);
        public static double ReadDouble(this byte[] data, int index) => ToDouble(data, index);
        public static void WriteDouble(this byte[] data, int index, double value) => FromDouble(ref data, index, value);
        public static string ReadString(this byte[] data, int index, int length, Encoding encoder) => ToString(encoder, data, index, length);

        public static string[] ReadStringArray(this byte[] data, int index, Encoding encoder, int stringlength, int count)
        {
            string[] list = new string[count];
            for (int i = 0; i < 10; i++)
            {
                list[i] = data.ReadString(index + (i * stringlength), stringlength, encoder);
            }
            return list;
        }

        public static void WriteStringArray(this byte[] data, int index, Encoding encoder, int strlength, int count, string[] list)
        {
            int idx = index;
            foreach (string val in list)
            {
                if (idx > count)
                    return;

                data.WriteString(index + (idx * strlength), val, encoder, strlength);
                idx++;
            }

        }

        public static void WriteString(this byte[] data, int index, string msg, Encoding encoder, int length) => FromString(ref data, encoder, msg, index, length);

        public static int ReadStringAuto(this byte[] data, int index, Encoding encoder, out string result)
        {
            int length = data.ReadInt(index);

            result = data.ReadString(index + 4, length, encoder);
            return length+4;
        }
        public static int WriteStringAuto(this byte[] data, int index, string msg, Encoding encoder)
        {
            byte[] stringbytes = encoder.GetBytes(msg);
            data.WriteInt(index, stringbytes.Length);
            data.WriteString(index+4, msg, encoder, stringbytes.Length);
            return stringbytes.Length+4;
        }

        public static byte[] FromChar(char input) => BitConverter.GetBytes(input);

        public static byte[] FromShort(short input) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
        public static byte[] FromUShort(ushort input)
        {
            var data = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }
        public static byte[] FromInt(int input) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
        public static byte[] FromUInt(uint input)
        {
            var data = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }
        public static byte[] FromLong(long input)
        {
            var data = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }
        public static byte[] FromULong(ulong input)
        {
            var data = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }
        public static byte[] FromFloat(float input)
        {
            byte[] data = BitConverter.GetBytes(input);
            //	if (BitConverter.IsLittleEndian)
            //	{
            //		Array.Reverse(data);
            //	}
            return data;
        }
        public static byte[] FromDouble(double input)
        {
            var data = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }
        public static byte[] FromString(Encoding encoder, string input, int blength = 32)
        {
            byte[] data = new byte[blength];
            byte[] str = encoder.GetBytes(input + char.MinValue);
            Array.Copy(str, 0, data, 0, Math.Min(blength, str.Length));
            return data;
        }
        public static void FromString(ref byte[] data, Encoding encoder, string value, int index, int blength = 32)
        {
            FromString(encoder, value, blength).CopyTo(data, index);
        }
        public static byte[] FromGuid(Guid input)
        {
            return input.ToByteArray();
        }

        public static char ToChar(byte[] input, int index = 0) => BitConverter.ToChar(input, index);

        public static short ToShort(byte[] input, int index = 0)=> IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input, index));
        
        public static ushort ToUShort(byte[] input, int index = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return BitConverter.ToUInt16(input, index);
        }
        
        public static int ToInt(byte[] input, int index = 0) => IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input, index));

        public static uint ToUInt(byte[] input, int index = 0)
        {

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return BitConverter.ToUInt32(input, index);
        }
        public static long ToLong(byte[] input, int index = 0) => IPAddress.NetworkToHostOrder(BitConverter.ToInt64(input, index));
        public static ulong ToULong(byte[] input, int index = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return BitConverter.ToUInt64(input, index);
        }

        public static void FromShort(ref byte[] data, int index, short value) => FromShort(value).CopyTo(data, index);
        public static void FromUShort(ref byte[] data, int index, ushort value) => FromUShort(value).CopyTo(data, index);
        public static void FromInt(ref byte[] data, int index, int value) => FromInt(value).CopyTo(data, index);
        public static void FromUInt(ref byte[] data, int index, uint value) => FromUInt(value).CopyTo(data, index);
        public static void FromLong(ref byte[] data, int index, long value) => FromLong(value).CopyTo(data, index);

        public static void FromULong(ref byte[] data, int index, ulong value) => FromULong(value).CopyTo(data, index);
        public static void FromFloat(ref byte[] data, int index, float value) => FromFloat(value).CopyTo(data, index);
        public static void FromDouble(ref byte[] data, int index, double value) => FromDouble(value).CopyTo(data, index);

        
        public static float ToFloat(byte[] input, int index = 0)
        {
            //byte[] temp = new byte[4];

            //	input.CopyTo(temp, index);

            //	if (BitConverter.IsLittleEndian)
            //{
            //	Array.Reverse(input);
            //}
            return BitConverter.ToSingle(input, index);

        }

        
        public static double ToDouble(byte[] input, int index = 0)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return BitConverter.ToDouble(input, index);
        }
        public static string ToString(Encoding encoder, byte[] input, int index, int blength = 32)
        {
            return encoder.GetString(input, index, blength).TrimEnd((char)0);
        }
        public static Guid ToGuid(byte[] input)
        {
            return new Guid(input);
        }


        [Obsolete]
        public static object RawDataToObject<T>(T type, params byte[] data) where T : Type
        {
            if (type == typeof(bool))
            {
                if (data[0] == 1)
                    return true;
                else
                    return false;
            }
            if (type.IsEnum) return Enum.ToObject(type, data[0]);
            if (type == typeof(byte)) return data[0];
            if (type == typeof(short)) return ToShort(data);
            if (type == typeof(int)) return ToInt(data);
            if (type == typeof(float)) return ToFloat(data);
            if (type == typeof(double)) return ToDouble(data);
            if (type == typeof(ushort)) return ToUShort(data);
            if (type == typeof(uint)) return ToUInt(data);
            if (type == typeof(Guid)) return ToGuid(data);
            throw new Exception("Type conversion not defined: " + type.ToString());
        }
        [Obsolete]
        public static byte[] ObjectToRawData(object obj, int length = 32)
        {

            Type t = obj.GetType();
            if (t == typeof(bool))
            {
                bool b = (bool)obj;
                if (b == true)
                    return new byte[] { 1 };
                else
                    return new byte[] { 0 };
            }
            if (t.IsEnum) return new byte[] { (byte)obj };
            if (t == typeof(byte)) return new byte[] { (byte)obj };
            if (t == typeof(short)) return FromShort((short)obj);
            if (t == typeof(int)) return FromInt((int)obj);
            if (t == typeof(float)) return FromFloat((float)obj);
            if (t == typeof(double)) return FromDouble((double)obj);
            if (t == typeof(ushort)) return FromUShort((ushort)obj);
            if (t == typeof(uint)) return FromUInt((uint)obj);
            if (t == typeof(Guid)) return FromGuid((Guid)obj);
            throw new Exception("Type conversion not defined: " + t.ToString());
        }
    }
}
