//
// Copyright (c) 2008-2011, Kenneth Bell
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
using System.Collections.Generic;
using System.IO;

namespace DiscUtils.Streams
{
    /// <summary>
    ///     A wrapper stream that enables you to take a snapshot, pushing changes into a side buffer.
    /// </summary>
    /// <remarks>
    ///     Once a snapshot is taken, you can discard subsequent changes or merge them back
    ///     into the wrapped stream.
    /// </remarks>
    public sealed class SnapshotStream : SparseStream
    {
        private readonly Ownership _baseStreamOwnership;
        private Stream _baseStream;

        /// <summary>
        ///     Records which byte ranges in diffStream hold changes.
        /// </summary>
        /// <remarks>
        ///     Can't use _diffStream's own tracking because that's based on it's
        ///     internal block size, not on the _actual_ bytes stored.
        /// </remarks>
        private List<StreamExtent> _diffExtents;

        /// <summary>
        ///     Captures changes to the base stream (when enabled).
        /// </summary>
        private SparseMemoryStream _diffStream;

        /// <summary>
        ///     Indicates that no writes should be permitted.
        /// </summary>
        private bool _frozen;

        /// <summary>
        ///     The saved stream position (if the diffStream is active).
        /// </summary>
        private long _savedPosition;

        /// <summary>
        ///     Initializes a new instance of the SnapshotStream class.
        /// </summary>
        /// <param name="baseStream">The stream to wrap.</param>
        /// <param name="owns">Indicates if this stream should control the lifetime of baseStream.</param>
        public SnapshotStream(Stream baseStream, Ownership owns)
        {
            _baseStream = baseStream;
            _baseStreamOwnership = owns;
            _diffExtents = new List<StreamExtent>();
        }

        /// <summary>
        ///     Gets an indication as to whether the stream can be read.
        /// </summary>
        public override bool CanRead => _baseStream.CanRead;

        /// <summary>
        ///     Gets an indication as to whether the stream position can be changed.
        /// </summary>
        public override bool CanSeek => _baseStream.CanSeek;

        /// <summary>
        ///     Gets an indication as to whether the stream can be written to.
        /// </summary>
        /// <remarks>
        ///     This property is orthogonal to Freezing/Thawing, it's
        ///     perfectly possible for a stream to be frozen and this method
        ///     return <c>true</c>.
        /// </remarks>
        public override bool CanWrite => _diffStream != null ? true : _baseStream.CanWrite;

        /// <summary>
        ///     Returns an enumeration over the parts of the stream that contain real data.
        /// </summary>
        public override IEnumerable<StreamExtent> Extents
        {
            get
            {
                var sparseBase = _baseStream as SparseStream;
                if (sparseBase == null) return new[] {new StreamExtent(0, Length)};
                return StreamExtent.Union(sparseBase.Extents, _diffExtents);
            }
        }

        /// <summary>
        ///     Gets the length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (_diffStream != null) return _diffStream.Length;
                return _baseStream.Length;
            }
        }

        /// <summary>
        ///     Gets and sets the current stream position.
        /// </summary>
        public override long Position { get; set; }

        /// <summary>
        ///     Prevents any write operations to the stream.
        /// </summary>
        /// <remarks>Useful to prevent changes whilst inspecting the stream.</remarks>
        public void Freeze()
        {
            _frozen = true;
        }

        /// <summary>
        ///     Re-permits write operations to the stream.
        /// </summary>
        public void Thaw()
        {
            _frozen = false;
        }

        /// <summary>
        ///     Takes a snapshot of the current stream contents.
        /// </summary>
        public void Snapshot()
        {
            if (_diffStream != null) throw new InvalidOperationException("Already have a snapshot");

            _savedPosition = Position;

            _diffExtents = new List<StreamExtent>();
            _diffStream = new SparseMemoryStream();
            _diffStream.SetLength(_baseStream.Length);
        }

        /// <summary>
        ///     Reverts to a previous snapshot, discarding any changes made to the stream.
        /// </summary>
        public void RevertToSnapshot()
        {
            if (_diffStream == null) throw new InvalidOperationException("No snapshot");

            _diffStream = null;
            _diffExtents = null;

            Position = _savedPosition;
        }

        /// <summary>
        ///     Discards the snapshot any changes made after the snapshot was taken are kept.
        /// </summary>
        public void ForgetSnapshot()
        {
            if (_diffStream == null) throw new InvalidOperationException("No snapshot");

            var buffer = new byte[8192];

            foreach (var extent in _diffExtents)
            {
                _diffStream.Position = extent.Start;
                _baseStream.Position = extent.Start;

                var totalRead = 0;
                while (totalRead < extent.Length)
                {
                    var toRead = (int) Math.Min(extent.Length - totalRead, buffer.Length);

                    var read = _diffStream.Read(buffer, 0, toRead);
                    _baseStream.Write(buffer, 0, read);

                    totalRead += read;
                }
            }

            _diffStream = null;
            _diffExtents = null;
        }

        /// <summary>
        ///     Flushes the stream.
        /// </summary>
        public override void Flush()
        {
            CheckFrozen();

            _baseStream.Flush();
        }

        /// <summary>
        ///     Reads data from the stream.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="offset">The buffer offset to start from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int numRead;

            if (_diffStream == null)
            {
                _baseStream.Position = Position;
                numRead = _baseStream.Read(buffer, offset, count);
            }
            else
            {
                if (Position > _diffStream.Length) throw new IOException("Attempt to read beyond end of file");

                var toRead = (int) Math.Min(count, _diffStream.Length - Position);

                // If the read is within the base stream's range, then touch it first to get the
                // (potentially) stale data.
                if (Position < _baseStream.Length)
                {
                    var baseToRead = (int) Math.Min(toRead, _baseStream.Length - Position);
                    _baseStream.Position = Position;

                    var totalBaseRead = 0;
                    while (totalBaseRead < baseToRead)
                        totalBaseRead += _baseStream.Read(buffer, offset + totalBaseRead, baseToRead - totalBaseRead);
                }

                // Now overlay any data from the overlay stream (if any)
                var overlayExtents = StreamExtent.Intersect(_diffExtents,
                    new StreamExtent(Position, toRead));
                foreach (var extent in overlayExtents)
                {
                    _diffStream.Position = extent.Start;
                    var overlayNumRead = 0;
                    while (overlayNumRead < extent.Length)
                        overlayNumRead += _diffStream.Read(
                            buffer,
                            (int) (offset + (extent.Start - Position) + overlayNumRead),
                            (int) (extent.Length - overlayNumRead));
                }

                numRead = toRead;
            }

            Position += numRead;

            return numRead;
        }

        /// <summary>
        ///     Moves the stream position.
        /// </summary>
        /// <param name="offset">The origin-relative location.</param>
        /// <param name="origin">The base location.</param>
        /// <returns>The new absolute stream position.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckFrozen();

            var effectiveOffset = offset;
            if (origin == SeekOrigin.Current)
                effectiveOffset += Position;
            else if (origin == SeekOrigin.End) effectiveOffset += Length;

            if (effectiveOffset < 0) throw new IOException("Attempt to move before beginning of disk");
            Position = effectiveOffset;
            return Position;
        }

        /// <summary>
        ///     Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length.</param>
        public override void SetLength(long value)
        {
            CheckFrozen();

            if (_diffStream != null)
                _diffStream.SetLength(value);
            else
                _baseStream.SetLength(value);
        }

        /// <summary>
        ///     Writes data to the stream at the current location.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The first byte to write from buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckFrozen();

            if (_diffStream != null)
            {
                _diffStream.Position = Position;
                _diffStream.Write(buffer, offset, count);

                // Beware of Linq's delayed model - force execution now by placing into a list.
                // Without this, large execution chains can build up (v. slow) and potential for stack overflow.
                _diffExtents =
                    new List<StreamExtent>(StreamExtent.Union(_diffExtents, new StreamExtent(Position, count)));

                Position += count;
            }
            else
            {
                _baseStream.Position = Position;
                _baseStream.Write(buffer, offset, count);
                Position += count;
            }
        }

        /// <summary>
        ///     Disposes of this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from Dispose(), else <c>false</c>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_baseStreamOwnership == Ownership.Dispose && _baseStream != null) _baseStream.Dispose();

                _baseStream = null;

                if (_diffStream != null) _diffStream.Dispose();

                _diffStream = null;
            }

            base.Dispose(disposing);
        }

        private void CheckFrozen()
        {
            if (_frozen) throw new InvalidOperationException("The stream is frozen");
        }
    }
}