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
using DiscUtils.Streams;

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Base class for use in creating classes for creating packages.
    /// </summary>
    public abstract class BasePkg
    {
        /// <summary>
        ///     Data files to include in the package.
        /// </summary>
        protected readonly List<PkgFile> DataFiles;

        /// <summary>
        ///     Base constructor for packages.
        /// </summary>
        /// <param name="dataFiles">Data files to include in the package.</param>
        protected BasePkg(List<PkgFile> dataFiles)
        {
            DataFiles = dataFiles;
        }

        /// <summary>
        ///     Abstract method overriden to build the package as a file.
        /// </summary>
        /// <param name="pkgFile"></param>
        public abstract void Build(FileInfo pkgFile);

        /// <summary>
        ///     Abstract method overridden to build the package as a stream.
        /// </summary>
        /// <param name="pkgStream">Stream to build the package in.</param>
        public abstract void Build(ref SparseStream pkgStream);

        /// <summary>
        ///     Static method for writing a stream to a specified file.
        /// </summary>
        /// <param name="fileObject">File to write to.</param>
        /// <param name="fileStream">Stream to write.</param>
        protected static void ToFile(FileInfo fileObject, SparseStream fileStream)
        {
            if (fileObject.Exists) fileObject.Delete();
            var output = fileObject.OpenWrite();
            var writer = new BinaryWriter(output);
            var reader = new BinaryReader(fileStream);
            writer.Write(reader.ReadBytes((int)fileStream.Length));
            writer.Close();
            reader.Close();
        }
    }
}