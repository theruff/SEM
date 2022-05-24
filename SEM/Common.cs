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
    public interface ICodec
    {
        byte[] encode(byte[] bytes);
        byte[] decode(byte[] bytes);
    }
}