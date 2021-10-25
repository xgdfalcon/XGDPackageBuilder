// <remarks>
// Copyright (c) 2021, XGDFalcon® LLC
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
// </remarks>

using System.Collections.Generic;
using System.IO;
using DiscUtils.Iso9660;

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Class for creating an ISO9660 file containing a set of files.
    /// </summary>
    public class Iso9660
    {
        /// <summary>
        ///     List of data files to include in the ISO.
        /// </summary>
        private readonly List<PkgFile> _dataFiles;

        /// <summary>
        ///     Constructor for the <see cref="Iso9660" /> object.
        /// </summary>
        /// <param name="dataFiles">Datafiles to include in the ISO.</param>
        protected Iso9660(List<PkgFile> dataFiles)
        {
            _dataFiles = dataFiles;
        }

        /// <summary>
        ///     Build the ISO file.
        /// </summary>
        /// <param name="file">The <see cref="FileInfo" /> object of the resulting ISO file.</param>
        /// <param name="volumeId">The volume ID to set on the ISO9660 volume.</param>
        public void Build(FileInfo file, string volumeId)
        {
            var builder = new CDBuilder { VolumeIdentifier = volumeId };
            foreach (var item in _dataFiles)
            {
                if (item.IsStream)
                {
                    builder.AddFile(item.PkgFilePath, item.FileStream);
                    continue;
                }

                builder.AddFile(item.PkgFilePath, item.FileObject?.FullName);
            }

            builder.Build(file.FullName);
        }
    }
}