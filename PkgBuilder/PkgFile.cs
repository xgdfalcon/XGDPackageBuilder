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

using System.IO;
using DiscUtils.Streams;

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Containment class for describing a file to include in a package.
    /// </summary>
    public class PkgFile
    {
        /// <summary>
        ///     File object referencing the file to include.
        /// </summary>
        public readonly FileInfo? FileObject;

        /// <summary>
        ///     Stream containing the file content.
        /// </summary>
        public readonly SparseStream? FileStream;

        /// <summary>
        ///     Path (including filename) to put the file in the package.
        /// </summary>
        public readonly string PkgFilePath;

        /// <summary>
        ///     Construct the object from a stream.
        /// </summary>
        /// <param name="pkgFilePath">Path (including filename) to put the file in the package.</param>
        /// <param name="fileStream">Stream containing the file content.</param>
        public PkgFile(string pkgFilePath, SparseStream fileStream)
        {
            PkgFilePath = pkgFilePath;
            FileObject = null;
            FileStream = fileStream;
        }

        /// <summary>
        ///     Construct the object from a physical file.
        /// </summary>
        /// <param name="pkgFilePath">File object referencing the file to include.</param>
        /// <param name="fileObject">File object referencing the file to include.</param>
        public PkgFile(string pkgFilePath, FileInfo fileObject)
        {
            PkgFilePath = pkgFilePath;
            FileObject = fileObject;
            FileStream = null;
        }

        /// <summary>
        ///     Reports if the file is a stream type if the <see cref="PkgFile.FileStream" /> is null.
        /// </summary>
        public bool IsStream => FileStream is not null;
    }
}