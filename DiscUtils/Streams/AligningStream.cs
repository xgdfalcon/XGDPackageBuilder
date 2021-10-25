//
// Copyright (c) 2008-2012, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

namespace DiscUtils.Streams
{
    /// <summary>
    ///     Aligns I/O to a given block size.
    /// </summary>
    /// <remarks>Uses the read-modify-write pattern to align I/O.</remarks>
    public sealed class AligningStream : WrappingMappedStream<SparseStream>
    {
        private readonly byte[] _alignmentBuffer;
        private readonly int _blockSize;

        public AligningStream(SparseStream toWrap, Ownership ownership, int blockSize)
            : base(toWrap, ownership, null)
        {
            _blockSize = blockSize;
            _alignmentBuffer = new byte[blockSize];
        }

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var startOffset = (int) (Position % _blockSize);
            if (startOffset == 0 && (count % _blockSize == 0 || Position + count == Length))
            {
                // Aligned read - pass through to underlying stream.
                WrappedStream.Position = Position;
                var numRead = WrappedStream.Read(buffer, offset, count);
                Position += numRead;
                return numRead;
            }

            var startPos = MathUtilities.RoundDown(Position, _blockSize);
            var endPos = MathUtilities.RoundUp(Position + count, _blockSize);

            if (endPos - startPos > int.MaxValue) throw new IOException("Oversized read, after alignment");

            var tempBuffer = new byte[endPos - startPos];

            WrappedStream.Position = startPos;
            var read = WrappedStream.Read(tempBuffer, 0, tempBuffer.Length);
            var available = Math.Min(count, read - startOffset);

            Array.Copy(tempBuffer, startOffset, buffer, offset, available);

            Position += available;
            return available;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var effectiveOffset = offset;
            if (origin == SeekOrigin.Current)
                effectiveOffset += Position;
            else if (origin == SeekOrigin.End) effectiveOffset += Length;

            if (effectiveOffset < 0) throw new IOException("Attempt to move before beginning of stream");
            Position = effectiveOffset;
            return Position;
        }

        public override void Clear(int count)
        {
            DoOperation(
                (s, opOffset, opCount) => { s.Clear(opCount); },
                (buffer, offset, opOffset, opCount) => { Array.Clear(buffer, offset, opCount); },
                count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            DoOperation(
                (s, opOffset, opCount) => { s.Write(buffer, offset + opOffset, opCount); },
                (tempBuffer, tempOffset, opOffset, opCount) =>
                {
                    Array.Copy(buffer, offset + opOffset, tempBuffer, tempOffset, opCount);
                },
                count);
        }

        private void DoOperation(ModifyStream modifyStream, ModifyBuffer modifyBuffer, int count)
        {
            var startOffset = (int) (Position % _blockSize);
            if (startOffset == 0 && (count % _blockSize == 0 || Position + count == Length))
            {
                WrappedStream.Position = Position;
                modifyStream(WrappedStream, 0, count);
                Position += count;
                return;
            }

            var unalignedEnd = Position + count;
            var alignedPos = MathUtilities.RoundDown(Position, _blockSize);

            if (startOffset != 0)
            {
                WrappedStream.Position = alignedPos;
                WrappedStream.Read(_alignmentBuffer, 0, _blockSize);

                modifyBuffer(_alignmentBuffer, startOffset, 0, Math.Min(count, _blockSize - startOffset));

                WrappedStream.Position = alignedPos;
                WrappedStream.Write(_alignmentBuffer, 0, _blockSize);
            }

            alignedPos = MathUtilities.RoundUp(Position, _blockSize);
            if (alignedPos >= unalignedEnd)
            {
                Position = unalignedEnd;
                return;
            }

            var passthroughLength = (int) MathUtilities.RoundDown(Position + count - alignedPos, _blockSize);
            if (passthroughLength > 0)
            {
                WrappedStream.Position = alignedPos;
                modifyStream(WrappedStream, (int) (alignedPos - Position), passthroughLength);
            }

            alignedPos += passthroughLength;
            if (alignedPos >= unalignedEnd)
            {
                Position = unalignedEnd;
                return;
            }

            WrappedStream.Position = alignedPos;
            WrappedStream.Read(_alignmentBuffer, 0, _blockSize);

            modifyBuffer(_alignmentBuffer, 0, (int) (alignedPos - Position),
                (int) Math.Min(count - (alignedPos - Position), unalignedEnd - alignedPos));

            WrappedStream.Position = alignedPos;
            WrappedStream.Write(_alignmentBuffer, 0, _blockSize);

            Position = unalignedEnd;
        }

        private delegate void ModifyStream(SparseStream stream, int opOffset, int count);

        private delegate void ModifyBuffer(byte[] buffer, int offset, int opOffset, int count);
    }
}