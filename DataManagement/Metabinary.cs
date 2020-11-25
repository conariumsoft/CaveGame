using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataManagement
{
    public enum MetabinaryTagID : byte
    {
        END = 0,
        BYTE = 1,
        SHORT = 2,
        INT = 3,
        LONG = 4,
        FLOAT = 5,
        DOUBLE = 6,
        BYTE_ARRAY = 7,
        COMPLEX = 8,
        STRING = 9,
        BOOL = 10,


    }
    public abstract class MetabinaryTag
    {
        public abstract string Name { get; set; }
        public abstract int PayloadSize { get; }
        public abstract void Serialize(ref byte[] buffer, int index);
        public abstract object ValueProp { get; }

        public int StringNameLength => Encoding.ASCII.GetByteCount(Name);
    }

    public class ComplexTag : MetabinaryTag
    {
        public ComplexTag()
        {
            Payload = new List<MetabinaryTag>();
        }


        public override int PayloadSize => GetSubtagSize() + StringNameLength + 1 + 4 + 1;

        public override string Name { get; set; }
        public override object ValueProp => Payload;
        public List<MetabinaryTag> Payload;

        public int GetSubtagSize()
        {
            int size = 0;
            foreach (var tag in Payload)
            {
                size += tag.PayloadSize;
            }

            return size;
        }


        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.COMPLEX;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            index += StringNameLength + 1 + 4;
            foreach (var tag in Payload)
            {
                tag.Serialize(ref buffer, index);
                index += tag.PayloadSize;
            }
            buffer[index] = (byte)MetabinaryTagID.END;
            index++;
            Console.WriteLine("AAA" + index);
        }

        public void AddByte(string name, byte value) => Payload.Add(new ByteTag { Name = name, Value = value });
        public void AddShort(string name, short value) => Payload.Add(new ShortTag { Name = name, Value = value });
        public void AddInt(string name, int value) => Payload.Add(new IntTag { Name = name, Value = value });
        public void AddLong(string name, long value) => Payload.Add(new LongTag { Name = name, Value = value });
        public void AddFloat(string name, float value) => Payload.Add(new FloatTag { Name = name, Value = value });
        public void AddDouble(string name, float value) => Payload.Add(new DoubleTag { Name = name, Value = value });
        public void AddByteArray(string name, byte[] value) => Payload.Add(new ByteArrayTag { Name = name, Value = value });
        public void AddString(string name, string value) => Payload.Add(new StringTag { Name = name, Value = value });
        public object GetTag(string name)
        {
            return Payload.First(t => t.Name == name);
        }
        public object GetValue(string name)
        {
            return Payload.First(t => t.Name == name).ValueProp;
        }
        public T GetValue<T>(string name)
        {
            return (T)Payload.First(t => t.Name == name).ValueProp;
        }


        public void AddComplex(ComplexTag value) => Payload.Add(value);
    }
    public class ByteTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 1 + 4;
        public override string Name { get; set; }
        public byte Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.BYTE;
            int strLen = Encoding.ASCII.GetByteCount(Name);
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            buffer[index + StringNameLength + 1 + 4] = Value;
        }
    }
    public class ShortTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 2 + 4;
        public override string Name { get; set; }
        public short Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.SHORT;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromShort(ref buffer, index + 1 + 4 + StringNameLength, Value);
        }
    }
    public class IntTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 4 + 4;
        public override string Name { get; set; }
        public int Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.INT;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromInt(ref buffer, index + 1 + 4 + StringNameLength, Value);
        }
    }
    public class LongTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 8 + 4;
        public override string Name { get; set; }
        public long Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.LONG;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromLong(ref buffer, index + 1 + 4 + StringNameLength, Value);
        }
    }

    public class FloatTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 4 + 4;
        public override string Name { get; set; }
        public float Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.FLOAT;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromFloat(ref buffer, index + 1 + 4 + StringNameLength, Value);
        }
    }
    public class DoubleTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + 1 + 8 + 4;
        public override string Name { get; set; }
        public double Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.DOUBLE;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromDouble(ref buffer, index + 1 + 4 + StringNameLength, Value);
        }
    }
    public class ByteArrayTag : MetabinaryTag // TODO: Finish implementation
    {
        public override int PayloadSize => 33 + Value.Length;
        public override string Name { get; set; }
        public byte[] Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.BYTE_ARRAY;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1);
            TypeSerializer.FromInt(ref buffer, index + 33, Value.Length);
            Value.CopyTo(buffer, index + 34);
        }
    }
    public class StringTag : MetabinaryTag
    {
        public override int PayloadSize => StringNameLength + Encoding.ASCII.GetByteCount(Value) + 4 + 4 + 1;
        public override string Name { get; set; }
        public string Value { get; set; }
        public override object ValueProp => Value;
        public override void Serialize(ref byte[] buffer, int index)
        {
            buffer[index] = (byte)MetabinaryTagID.STRING;
            TypeSerializer.FromInt(ref buffer, index + 1, StringNameLength); // Name Length
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, index + 1 + 4, StringNameLength);
            TypeSerializer.FromInt(ref buffer, index + 1 + 4 + StringNameLength, Value.Length); // Value Length
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Value, index + 1 + 4 + StringNameLength + 4, Encoding.ASCII.GetByteCount(Value));
        }
    }

    public class Metabinary : ComplexTag
    {
        public byte[] Serialize()
        {
            int bufferSize = PayloadSize + 1 + 4 + StringNameLength;

            byte[] buffer = new byte[bufferSize];
            buffer[0] = (byte)MetabinaryTagID.COMPLEX;
            TypeSerializer.FromInt(ref buffer, 1, StringNameLength);
            TypeSerializer.FromString(ref buffer, Encoding.ASCII, Name, 5);

            int index = 5 + StringNameLength;
            for (int i = 0; i < Payload.Count; i++)// (var tag in Payload)
            {
                var tag = Payload[i];
                tag.Serialize(ref buffer, index);
                index += tag.PayloadSize;
            }

            buffer[index] = (byte)MetabinaryTagID.END;

            return buffer;
        }
        private static string GetNesting(int nesting)
        {
            string str = "";
            for (int i = 0; i < nesting; i++)
            {
                str += " ";
            }

            return str;
        }
        public static void DebugText(MetabinaryTag tag, int nesting = 0)
        {
            string tabstr = GetNesting(nesting);

            if (tag is ByteTag byteTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3:X2}", tabstr, "byte", byteTag.Name, byteTag.Value));
            if (tag is ShortTag shortTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3}", tabstr, "short", shortTag.Name, shortTag.Value));
            if (tag is IntTag intTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3}", tabstr, "int", intTag.Name, intTag.Value));
            if (tag is LongTag longTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3}", tabstr, "long", longTag.Name, longTag.Value));
            if (tag is FloatTag floatTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3}", tabstr, "float", floatTag.Name, floatTag.Value));
            if (tag is DoubleTag doubleTag)
                Console.WriteLine(String.Format("{0}{1} '{2}': {3}", tabstr, "double", doubleTag.Name, doubleTag.Value));
            if (tag is ByteArrayTag byteArrayTag)
                Console.WriteLine(String.Format("{0}{1} '{2}'(length:{3}): {4}", tabstr, "byte_array", byteArrayTag.Name, byteArrayTag.Value.Length, byteArrayTag.Value.DumpHex()));
            if (tag is StringTag stringTag)
                Console.WriteLine(String.Format("{0}{1} '{2}'(length:{3}): {4}", tabstr, "string", stringTag.Name, stringTag.Value.Length, stringTag.Value));
            if (tag is ComplexTag complexTag)
            {
                Console.WriteLine(String.Format("{0}{1} '{2}'", tabstr, "complex", complexTag.Name));
                nesting += 4;
                foreach (var child in complexTag.Payload)
                {
                    DebugText(child, nesting);
                }
                Console.WriteLine(String.Format("{0}{1}", tabstr, "end"));
            }
        }


        public static int InternalDeserialize(ref ComplexTag complex, byte[] buffer, int index)
        {
            for (int i = index; i < buffer.Length; i++)
            {
                Console.WriteLine(string.Format("pass {0} {1:X2} {2}", i, buffer[i], (MetabinaryTagID)buffer[i]));
                if (buffer[i] == (byte)MetabinaryTagID.END)
                {
                    Console.WriteLine(string.Format("ENDING AT {0} d: {1:X2}, b: {2:X2}, a: {3:X2}", i, buffer[i], buffer[i - 1], buffer[i + 1]));
                    return i + 1;
                }

                if (buffer[i] == (byte)MetabinaryTagID.BYTE)
                {
                    ByteTag tag = new ByteTag();
                    int tagNameLength = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 5, tagNameLength);
                    tag.Value = buffer[i + 5 + tagNameLength];
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.SHORT)
                {

                    ShortTag tag = new ShortTag();
                    int tagNameLength = TypeSerializer.ToInt(buffer, i + 1);
                    Console.WriteLine(tagNameLength);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 4 + 1, tagNameLength);
                    tag.Value = TypeSerializer.ToShort(buffer, i + 4 + 1 + tagNameLength);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.INT)
                {
                    IntTag tag = new IntTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    tag.Value = TypeSerializer.ToInt(buffer, i + 1 + 4 + nameLen);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.LONG)
                {
                    LongTag tag = new LongTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    tag.Value = TypeSerializer.ToLong(buffer, i + 1 + 4 + nameLen);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.FLOAT)
                {
                    FloatTag tag = new FloatTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    tag.Value = TypeSerializer.ToFloat(buffer, i + 1 + 4 + nameLen);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.DOUBLE)
                {
                    DoubleTag tag = new DoubleTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    tag.Value = TypeSerializer.ToDouble(buffer, i + 1 + 4 + nameLen);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.BYTE_ARRAY)
                {
                    ByteArrayTag tag = new ByteArrayTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    int count = TypeSerializer.ToInt(buffer, index + 1 + 4 + nameLen);
                    Array.Copy(buffer, index + 1 + 4 + 4 + nameLen, tag.Value, 0, count);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
                if (buffer[i] == (byte)MetabinaryTagID.COMPLEX)
                {
                    ComplexTag tag = new ComplexTag();
                    int nameLength = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLength);
                    InternalDeserialize(ref tag, buffer, i + 1 + 4 + nameLength);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;

                }
                if (buffer[i] == (byte)MetabinaryTagID.STRING)
                {
                    StringTag tag = new StringTag();
                    int nameLen = TypeSerializer.ToInt(buffer, i + 1);
                    tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4, nameLen);
                    int strlen = TypeSerializer.ToInt(buffer, i + 1 + 4 + nameLen);
                    tag.Value = TypeSerializer.ToString(Encoding.ASCII, buffer, i + 1 + 4 + nameLen + 4, strlen);
                    complex.Payload.Add(tag);
                    i += tag.PayloadSize - 1;
                }
            }
            return 0;
        }


        public static Metabinary Deserialize(byte[] buffer, int index = 0)
        {
            Metabinary file = new Metabinary();
            file.Payload = new List<MetabinaryTag>();
            var tag = (ComplexTag)file;
            var nameLength = TypeSerializer.ToInt(buffer, index + 1);
            tag.Name = TypeSerializer.ToString(Encoding.ASCII, buffer, index + 1 + 4, nameLength);
            InternalDeserialize(ref tag, buffer, index + nameLength + 4 + 1);
            return (Metabinary)tag;
        }
    }
}
