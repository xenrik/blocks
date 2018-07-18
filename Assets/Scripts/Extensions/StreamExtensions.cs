using System.IO;
using System;

public static class StreamExtensions {
    public static void WriteFloat(this Stream stream, float f) {
        byte[] bytes = BitConverter.GetBytes(f);
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void WriteInt(this Stream stream, int i) {
        byte[] bytes = BitConverter.GetBytes(i);
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void WriteLong(this Stream stream, long l) {
        byte[] bytes = BitConverter.GetBytes(l);
        stream.Write(bytes, 0, bytes.Length);
    }

    public static float ReadFloat(this Stream stream) {
        byte[] buffer = new byte[sizeof(float)];
        int read = stream.Read(buffer, 0, sizeof(float));
        if (read != sizeof(float)) {
            throw new IOException("End of stream encountered");
        }

        return BitConverter.ToSingle(buffer, 0);
    }

    public static int ReadInt(this Stream stream) {
        byte[] buffer = new byte[sizeof(int)];
        int read = stream.Read(buffer, 0, sizeof(int));
        if (read != sizeof(int)) {
            throw new IOException("End of stream encountered");
        }

        return BitConverter.ToInt32(buffer, 0);
    }

    public static long ReadLong(this Stream stream) {
        byte[] buffer = new byte[sizeof(long)];
        int read = stream.Read(buffer, 0, sizeof(long));
        if (read != sizeof(long)) {
            throw new IOException("End of stream encountered");
        }

        return BitConverter.ToInt64(buffer, 0);
    }
}
