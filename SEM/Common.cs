using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompressionMethods
{
    struct CodeNode
    {
        public uint code;
        public byte lenght;
        //public CodeNode() : this(0, 0) { }
        public CodeNode(uint _huffCode, byte _length)
        {
            this.code = _huffCode;
            this.lenght = _length;
        }
        public override string ToString()
        {
            string code = "";
            for (int i = lenght - 1; i >= 0; i--)
                code += (this.code >> i & 1);
            return code;
        }
    }
    internal class Common
    {
        public static ushort MAGIC_BYTES = 12345;
    }
    public class ByteArrayComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] first, byte[] second)
        {
            if (first == null || second == null)
                return first == second;
            if (ReferenceEquals(first, second))
                return true;
            if (first.Length != second.Length)
                return false;
            return first.SequenceEqual(second);
        }
        public override int GetHashCode(byte[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException("One of the comparing arrays is empty");
            return obj.Length;
        }
    }
    public class OutBytes : IEnumerable<byte[]>
    {
        public IEnumerator<byte[]> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public interface ICodec
    {
        byte[] encode(byte[] bytes);
        byte[] decode(byte[] bytes);
    }
}