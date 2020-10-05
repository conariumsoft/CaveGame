using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;


namespace CaveGame.Core.Network
{
	// Serializes datatypes regardless of endianness
	public static class TypeSerializer
	{

		#region Runtime Testing

		public static T RoundTrip<T>(T original)
		{
			byte[] bytedata = ObjectToRawData(original);
			return (T)RawDataToObject(typeof(T), bytedata);
		}

		[Conditional("DEBUG")]
		public static void TestTypeRoundTrip()
		{
			TestInt();
			TestShort();
			TestString();
			TestFloat();
		}

		[Conditional("DEBUG")]
		private static void TestInt()
		{
			int original_int = 420;
			int reconstr_int = RoundTrip(original_int);
			if (original_int == reconstr_int)
				Console.WriteLine("SerializerRoundTripTest int32 = PASS");
			else
				Console.WriteLine("SerializerRoundTripTest int32 = FAIL {0} {1}", original_int, reconstr_int);
		}
		[Conditional("DEBUG")]
		private static void TestShort()
		{
			short original = 69;
			short reconstr = RoundTrip(original);
			if (original == reconstr)
				Console.WriteLine("SerializerRoundTripTest short16 = PASS");
			else
				Console.WriteLine("SerializerRoundTripTest short16 = FAIL {0} {1}", original, reconstr);
		}
		[Conditional("DEBUG")]
		private static void TestString()
		{
			string original = "ur mom LOL";
			string reconstr = RoundTrip(original);
			if (original == reconstr)
				Console.WriteLine("SerializerRoundTripTest string(32) = PASS");
			else
				Console.WriteLine("SerializerRoundTripTest string(32) = FAIL {0} {1}", original, reconstr);
		}
		[Conditional("DEBUG")]
		private static void TestFloat()
		{
			float original = 69;
			float reconstr = RoundTrip(original);
			if (original == reconstr)
				Console.WriteLine("SerializerRoundTripTest float = PASS");
			else
				Console.WriteLine("SerializerRoundTripTest float = FAIL {0} {1}", original, reconstr);
		}
		#endregion

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
			if (type == typeof(string)) return ToString(data);
			if (type == typeof(Guid)) return ToGuid(data);
			throw new Exception("Type conversion not defined: " + type.ToString());
		}
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
			if (t == typeof(string)) return FromString((string)obj, length);
			if (t == typeof(Guid)) return FromGuid((Guid)obj);
			throw new Exception("Type conversion not defined: " + t.ToString());
		}

		public static byte[] FromShort(short input)
		{
			return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
		}
		public static byte[] FromUShort(ushort input)
		{
			var data = BitConverter.GetBytes(input);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(data);
			}
			return data;
		}
		public static byte[] FromInt(int input)
		{
			return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(input));
		}
		public static byte[] FromUInt(uint input)
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
		public static byte[] FromString(string input, int blength = 32)
		{
			byte[] data = new byte[blength];
			byte[] str = Encoding.ASCII.GetBytes(input + char.MinValue);
			Array.Copy(str, 0, data, 0, Math.Min(blength, str.Length));
			return data;
		}
		public static byte[] FromGuid(Guid input)
		{
			return input.ToByteArray();
		}

		public static short ToShort(byte[] input)
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input, 0));
		}
		public static ushort ToUShort(byte[] input)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(input);
			}
			return BitConverter.ToUInt16(input, 0);
		}
		public static int ToInt(byte[] input, int index = 0)
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input, index));
		}

		public static void FromInt(ref byte[] data, int index, int value)
		{
			FromInt(value).CopyTo(data, index);
		}
		public static void FromFloat(ref byte[] data, int index, float value)
		{
			FromFloat(value).CopyTo(data, index);
		}


		public static uint ToUInt(byte[] input)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(input);
			}
			return BitConverter.ToUInt32(input, 0);
		}

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

		public static double ToDouble(byte[] input)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(input);
			}
			return BitConverter.ToDouble(input, 0);
		}
		public static string ToString(byte[] input)
		{
			return Encoding.ASCII.GetString(input).TrimEnd((char)0);
		}
		public static Guid ToGuid(byte[] input)
		{
			return new Guid(input);
		}
	}
}
