using System;

// 第4章缓冲区：只保存尚未处理的数据，扩容时保留有效区间。
public sealed class ByteArray
{
    public byte[] bytes;
    public int readIdx;
    public int writeIdx;
    public int length { get { return writeIdx - readIdx; } }
    public int remain { get { return bytes.Length - writeIdx; } }
    public ByteArray(int size = 1024) { bytes = new byte[Math.Max(1, size)]; }
    public ByteArray(byte[] data) { bytes = (byte[])data.Clone(); writeIdx = data.Length; }
    public void MoveBytes()
    {
        int count = length;
        Buffer.BlockCopy(bytes, readIdx, bytes, 0, count);
        readIdx = 0; writeIdx = count;
    }
    public void CheckAndMoveBytes() { if (remain < 8) MoveBytes(); }
    public void ReSize(int size)
    {
        if (size < length) throw new ArgumentOutOfRangeException("size");
        byte[] next = new byte[Math.Max(1, size)];
        int count = length;
        Buffer.BlockCopy(bytes, readIdx, next, 0, count);
        bytes = next; readIdx = 0; writeIdx = count;
    }
}
