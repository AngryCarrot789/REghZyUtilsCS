using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REghZy.Streams {
    /// <summary>
    /// A data output stream that uses little-endianness when writing data. This is not
    /// what networking usually uses, but this way is typically faster
    /// </summary>
    public class DataOutputStreamLE : IDataOutput {
        private Stream stream;

        /// <summary>
        /// A temporary buffer used for writing
        /// </summary>
        private readonly byte[] buffer8 = new byte[8];

        public Stream Stream {
            get => this.stream;
            set => this.stream = value;
        }

        public DataOutputStreamLE(Stream stream) {
            this.stream = stream;
        }

        public void Flush() {
            this.stream.Flush();
        }

        public void Close() {
            this.stream.Close();
        }

        public void Write(byte[] src, int offset, int count) {
            this.stream.Write(src, offset, count);
        }

        public void Write(byte[] src, int offset = 0) {
            this.stream.Write(src, offset, src.Length);
        }

        public void WriteBoolean(bool val) {
            this.buffer8[0] = (byte) (val ? 1 : 0);
            this.stream.Write(this.buffer8, 0, 1);
        }

        public void WriteEnum8<TEnum>(TEnum value) where TEnum : unmanaged, Enum {
            unsafe {
                WriteByte(*(byte*) &value);
            }
        }

        public void WriteEnum16<TEnum>(TEnum value) where TEnum : unmanaged, Enum {
            unsafe {
                WriteUShort(*(ushort*) &value);
            }
        }

        public void WriteEnum32<TEnum>(TEnum value) where TEnum : unmanaged, Enum {
            unsafe {
                WriteUInt(*(uint*) &value);
            }
        }

        public void WriteEnum64<TEnum>(TEnum value) where TEnum : unmanaged, Enum {
            unsafe {
                WriteULong(*(ulong*) &value);
            }
        }

        public void WriteSByte(sbyte value) {
            this.buffer8[0] = (byte) (value & 255);
            this.stream.Write(this.buffer8, 0, 1);
        }

        public void WriteByte(byte value) {
            this.buffer8[0] = value;
            this.stream.Write(this.buffer8, 0, 1);
        }

        public void WriteShort(short value) {
            byte[] b = this.buffer8;
            b[0] = (byte) ((value >> 0) & 255);
            b[1] = (byte) ((value >> 8) & 255);
            this.stream.Write(b, 0, 2);
        }

        public void WriteUShort(ushort value) {
            uint i = value;
            byte[] b = this.buffer8;
            b[0] = (byte) ((i >> 0) & 255);
            b[1] = (byte) ((i >> 8) & 255);
            this.stream.Write(b, 0, 2);
        }

        public void WriteInt(int value) {
            uint i = (uint) value;
            byte[] b = this.buffer8;
            b[0] = (byte) ((i >> 0) & 255);
            b[1] = (byte) ((i >> 8) & 255);
            b[2] = (byte) ((i >> 16) & 255);
            b[3] = (byte) ((i >> 24) & 255);
            this.stream.Write(b, 0, 4);
        }

        public void WriteUInt(uint value) {
            byte[] b = this.buffer8;
            b[0] = (byte) ((value >> 0) & 255);
            b[1] = (byte) ((value >> 8) & 255);
            b[2] = (byte) ((value >> 16) & 255);
            b[3] = (byte) ((value >> 24) & 255);
            this.stream.Write(b, 0, 4);
        }

        public void WriteLong(long value) {
            unsafe {
                WritePtr((byte*) &value, 0, 8);
            }
        }

        public void WriteULong(ulong value) {
            unsafe {
                WritePtr((byte*) &value, 0, 8);
            }
        }

        public void WriteFloat(float value) {
            unsafe {
                uint bits = *(uint*) &value;
                byte[] b = this.buffer8;
                b[0] = (byte) ((bits >> 0) & 255);
                b[1] = (byte) ((bits >> 8) & 255);
                b[2] = (byte) ((bits >> 16) & 255);
                b[3] = (byte) ((bits >> 24) & 255);
                this.stream.Write(b, 0, 4);
            }
        }

        public void WriteDouble(double value) {
            unsafe {
                WritePtr((byte*) &value, 0, 8);
            }
        }

        public void WriteCharUTF16(char value) {
            byte[] b = this.buffer8;
            b[0] = (byte) ((value >> 0) & 255);
            b[1] = (byte) ((value >> 8) & 255);
            this.stream.Write(b, 0, 2);
        }

        public void WriteCharUTF8(char value) {
            this.buffer8[0] = (byte) (value & 255);
            this.stream.Write(this.buffer8, 0, 1);
        }

        public void WriteStringUTF16(string value) {
            int len = value.Length;
            if (len > 3) {
                unsafe {
                    fixed (char* ptr = value) {
                        WritePtrUTF16(ptr, 0, len);
                    }
                }
            }
            else {
                byte[] b = this.buffer8;
                switch (len) {
                    case 3: {
                        char c1 = value[0];
                        char c2 = value[1];
                        char c3 = value[2];
                        b[0] = (byte) ((c1 >> 0) & 255);
                        b[1] = (byte) ((c1 >> 8) & 255);
                        b[2] = (byte) ((c2 >> 0) & 255);
                        b[3] = (byte) ((c2 >> 8) & 255);
                        b[4] = (byte) ((c3 >> 0) & 255);
                        b[5] = (byte) ((c3 >> 8) & 255);
                        this.stream.Write(b, 0, 6);
                        break;
                    }
                    case 2: {
                        char c1 = value[0];
                        char c2 = value[1];
                        b[0] = (byte) ((c1 >> 0) & 255);
                        b[1] = (byte) ((c1 >> 8) & 255);
                        b[2] = (byte) ((c2 >> 0) & 255);
                        b[3] = (byte) ((c2 >> 8) & 255);
                        this.stream.Write(b, 0, 4);
                        break;
                    }
                    case 1: {
                        char c = value[0];
                        b[0] = (byte) ((c >> 0) & 255);
                        b[1] = (byte) ((c >> 8) & 255);
                        this.stream.Write(b, 0, 2);
                        break;
                    }
                }
            }
        }

        public void WriteStringUTF8(string value) {
            int len = value.Length;
            if (len > 3) {
                unsafe {
                    fixed (char* ptr = value) {
                        WritePtrUTF8(ptr, 0, len);
                    }
                }
            }
            else {
                byte[] b = this.buffer8;
                switch (len) {
                    case 3:
                        b[0] = (byte) (value[0] & 255);
                        b[1] = (byte) (value[1] & 255);
                        b[2] = (byte) (value[2] & 255);
                        this.stream.Write(b, 0, 3);
                        break;
                    case 2:
                        b[0] = (byte) (value[0] & 255);
                        b[1] = (byte) (value[1] & 255);
                        this.stream.Write(b, 0, 2);
                        break;
                    case 1:
                        b[0] = (byte) (value[0] & 255);
                        this.stream.Write(b, 0, 1);
                        break;
                }
            }
        }

        public void WriteCharsUTF16(char[] chars) {
            int len = chars.Length;
            if (len > 3) {
                unsafe {
                    fixed (char* ptr = chars) {
                        WritePtrUTF16(ptr, 0, len);
                    }
                }
            }
            else {
                byte[] b = this.buffer8;
                switch (len) {
                    case 3: {
                        char c1 = chars[0];
                        char c2 = chars[1];
                        char c3 = chars[2];
                        b[0] = (byte) ((c1 >> 0) & 255);
                        b[1] = (byte) ((c1 >> 8) & 255);
                        b[2] = (byte) ((c2 >> 0) & 255);
                        b[3] = (byte) ((c2 >> 8) & 255);
                        b[4] = (byte) ((c3 >> 0) & 255);
                        b[5] = (byte) ((c3 >> 8) & 255);
                        this.stream.Write(b, 0, 6);
                        break;
                    }
                    case 2: {
                        char c1 = chars[0];
                        char c2 = chars[1];
                        b[0] = (byte) ((c1 >> 0) & 255);
                        b[1] = (byte) ((c1 >> 8) & 255);
                        b[2] = (byte) ((c2 >> 0) & 255);
                        b[3] = (byte) ((c2 >> 8) & 255);
                        this.stream.Write(b, 0, 4);
                        break;
                    }
                    case 1: {
                        char c = chars[0];
                        b[0] = (byte) ((c >> 0) & 255);
                        b[1] = (byte) ((c >> 8) & 255);
                        this.stream.Write(b, 0, 2);
                        break;
                    }
                }
            }
        }

        public void WriteCharsUTF8(char[] chars) {
            int len = chars.Length;
            if (len > 3) {
                unsafe {
                    fixed (char* ptr = chars) {
                        WritePtrUTF8(ptr, 0, len);
                    }
                }
            }
            else {
                byte[] b = this.buffer8;
                switch (len) {
                    case 3:
                        b[0] = (byte) (chars[0] & 255);
                        b[1] = (byte) (chars[1] & 255);
                        b[2] = (byte) (chars[2] & 255);
                        this.stream.Write(b, 0, 3);
                        break;
                    case 2:
                        b[0] = (byte) (chars[0] & 255);
                        b[1] = (byte) (chars[1] & 255);
                        this.stream.Write(b, 0, 2);
                        break;
                    case 1:
                        b[0] = (byte) (chars[0] & 255);
                        this.stream.Write(b, 0, 1);
                        break;
                }
            }
        }

        public unsafe void WritePtrUTF16(char* src, int offset, int length) {
            byte[] b = this.buffer8;
            Stream s = this.stream;
            fixed (byte* bptr = this.buffer8) {
                while (length > 3) {
                    bptr[0] = (byte) ((src[offset + 0] >> 0) & 255);
                    bptr[1] = (byte) ((src[offset + 0] >> 8) & 255);
                    bptr[2] = (byte) ((src[offset + 1] >> 0) & 255);
                    bptr[3] = (byte) ((src[offset + 1] >> 8) & 255);
                    bptr[4] = (byte) ((src[offset + 2] >> 0) & 255);
                    bptr[5] = (byte) ((src[offset + 2] >> 8) & 255);
                    bptr[6] = (byte) ((src[offset + 3] >> 0) & 255);
                    bptr[7] = (byte) ((src[offset + 3] >> 8) & 255);
                    offset += 4;
                    length -= 4;
                    s.Write(b, 0, 8);
                }

                switch (length) {
                    case 3:
                        bptr[0] = (byte) ((src[offset + 0] >> 0) & 255);
                        bptr[1] = (byte) ((src[offset + 0] >> 8) & 255);
                        bptr[2] = (byte) ((src[offset + 1] >> 0) & 255);
                        bptr[3] = (byte) ((src[offset + 1] >> 8) & 255);
                        bptr[4] = (byte) ((src[offset + 2] >> 0) & 255);
                        bptr[5] = (byte) ((src[offset + 2] >> 8) & 255);
                        s.Write(b, 0, 6);
                        break;
                    case 2:
                        bptr[0] = (byte) ((src[offset + 0] >> 0) & 255);
                        bptr[1] = (byte) ((src[offset + 0] >> 8) & 255);
                        bptr[2] = (byte) ((src[offset + 1] >> 0) & 255);
                        bptr[3] = (byte) ((src[offset + 1] >> 8) & 255);
                        s.Write(b, 0, 4);
                        break;
                    case 1:
                        bptr[0] = (byte) ((src[offset] >> 0) & 255);
                        bptr[1] = (byte) ((src[offset] >> 8) & 255);
                        s.Write(b, 0, 2);
                        break;
                }
            }
        }

        public unsafe void WritePtrUTF8(char* src, int offset, int length) {
            byte[] b = this.buffer8;
            Stream s = this.stream;
            fixed (byte* bptr = b) {
                while (length > 7) {
                    bptr[0] = (byte) (src[offset + 0] & 255);
                    bptr[1] = (byte) (src[offset + 1] & 255);
                    bptr[2] = (byte) (src[offset + 2] & 255);
                    bptr[3] = (byte) (src[offset + 3] & 255);
                    bptr[4] = (byte) (src[offset + 4] & 255);
                    bptr[5] = (byte) (src[offset + 5] & 255);
                    bptr[6] = (byte) (src[offset + 6] & 255);
                    bptr[7] = (byte) (src[offset + 7] & 255);
                    offset += 8;
                    length -= 8;
                    s.Write(b, 0, 8);
                }

                if (length > 3) {
                    bptr[0] = (byte) (src[offset + 0] & 255);
                    bptr[1] = (byte) (src[offset + 1] & 255);
                    bptr[2] = (byte) (src[offset + 2] & 255);
                    bptr[3] = (byte) (src[offset + 3] & 255);
                    offset += 4;
                    length -= 4;
                    s.Write(b, 0, 4);
                }

                switch (length) {
                    case 3:
                        bptr[0] = (byte) (src[offset + 0] & 255);
                        bptr[1] = (byte) (src[offset + 1] & 255);
                        bptr[2] = (byte) (src[offset + 2] & 255);
                        s.Write(b, 0, 3);
                        break;
                    case 2:
                        bptr[0] = (byte) (src[offset + 0] & 255);
                        bptr[1] = (byte) (src[offset + 1] & 255);
                        s.Write(b, 0, 2);
                        break;
                    case 1:
                        bptr[0] = (byte) (src[offset] & 255);
                        s.Write(b, 0, 1);
                        break;
                }
            }
        }

        public void WritePtr(IntPtr src, int offset, int length) {
            byte[] b = this.buffer8;
            Stream s = this.stream;

            unsafe {
                fixed (byte* ptr = b) {
                    while (length > 7) {
                        *(ulong*) ptr = (ulong) Marshal.ReadInt64(src, offset);
                        offset += 8;
                        length -= 8;
                        s.Write(b, 0, 8);
                    }

                    if (length == 0) {
                        return;
                    }

                    if (length > 3) {
                        *(uint*) ptr = (uint) Marshal.ReadInt32(src);
                        offset += 4;
                        length -= 4;
                        s.Write(b, 0, 4);
                    }

                    switch (length) {
                        case 3:
                            b[0] = Marshal.ReadByte(src, offset);
                            b[1] = Marshal.ReadByte(src, offset + 1);
                            b[2] = Marshal.ReadByte(src, offset + 2);
                            s.Write(b, 0, 3);
                            break;
                        case 2:
                            b[0] = Marshal.ReadByte(src, offset);
                            b[1] = Marshal.ReadByte(src, offset + 1);
                            s.Write(b, 0, 2);
                            break;
                        case 1:
                            b[0] = Marshal.ReadByte(src, offset);
                            s.Write(b, 0, 1);
                            break;
                    }
                }
            }
        }

        public unsafe void WritePtr(byte* src, int offset, int length) {
            byte[] b = this.buffer8;
            Stream s = this.stream;
            fixed (byte* ptr = b) {
                while (length > 7) {
                    *(ulong*) ptr = *(ulong*) (src + offset);
                    offset += 8;
                    length -= 8;
                    s.Write(b, 0, 8);
                }

                if (length == 0) {
                    return;
                }

                if (length > 3) {
                    *(uint*) ptr = *(uint*) (src + offset);
                    offset += 4;
                    length -= 4;
                    s.Write(b, 0, 4);
                }

                switch (length) {
                    case 3:
                        ptr[0] = src[offset];
                        ptr[1] = src[offset + 1];
                        ptr[2] = src[offset + 2];
                        s.Write(b, 0, 3);
                        break;
                    case 2:
                        ptr[0] = src[offset];
                        ptr[1] = src[offset + 1];
                        s.Write(b, 0, 2);
                        break;
                    case 1:
                        ptr[0] = src[offset];
                        s.Write(b, 0, 1);
                        break;
                }
            }
        }

        /// <summary>
        /// Writes a primitive blittable value type
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void WritePrimitive<T>(T value) where T : unmanaged {
            unsafe {
                WritePtr((byte*) &value, 0, sizeof(T));
            }
        }
    }
}