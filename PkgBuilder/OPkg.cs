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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using DiscUtils;
using DiscUtils.Archives;
using DiscUtils.Streams;

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Class for building an opkg.
    /// </summary>
    public class OPkg : BasePkg
    {
        /// <summary>
        ///     Required header file for a ipk/opk.
        /// </summary>
        private const string PkgHeaderFilename = "debian-binary";

        /// <summary>
        ///     Filename for the data archive.
        /// </summary>
        private const string PkgDataFilename = "data.tar.gz";

        /// <summary>
        ///     Filename for the control archive.
        /// </summary>
        private const string PkgControlFilename = "control.tar.gz";

        /// <summary>
        ///     Content for the header file.
        /// </summary>
        private static readonly byte[] PkgHeaderFileContent = Encoding.ASCII.GetBytes("2.0");

        /// <summary>
        ///     Control files for the control archive.
        /// </summary>
        private readonly Dictionary<ControlFileEnum, PkgFile> _controlFiles;

        /// <summary>
        ///     Constructor for creating the Opkg object.
        /// </summary>
        /// <param name="dataFiles">Data files to include in the opkg.</param>
        /// <param name="controlFiles">Control files to include in the opkg.</param>
        public OPkg(List<PkgFile> dataFiles, Dictionary<ControlFileEnum, PkgFile> controlFiles) : base(dataFiles)
        {
            _controlFiles = controlFiles;
        }

        /// <summary>
        ///     Build the ipk file into a file.
        /// </summary>
        /// <param name="pkgFile">The file to create.</param>
        /// <exception cref="NullReferenceException">Error creating stream.</exception>
        public override void Build(FileInfo pkgFile)
        {
            var stream = SparseStream.FromStream(new MemoryStream(), Ownership.Dispose);
            if (stream is null) throw new NullReferenceException("Critical Error.");
            Build(ref stream);
            ToFile(pkgFile, stream);
        }

        /// <summary>
        ///     Builds the ipk file in memory using the supplied files.
        /// </summary>
        /// <param name="pkgStream">Stream to build the file within.</param>
        public override void Build(ref SparseStream pkgStream)
        {
            if (pkgStream is not { }) throw new NullReferenceException(nameof(pkgStream));
            var controlTar = new TarFileBuilder();
            foreach (var (name, pkgFile) in _controlFiles)
                controlTar.AddFile(name.ToFileName(), pkgFile.FileStream,
                    UnixFilePermissions.OwnerAll | UnixFilePermissions.GroupRead, 0, 0, DateTime.UtcNow);
            var controlTarStream = controlTar.Build();
            var dataTar = new TarFileBuilder();
            foreach (var pkgFile in DataFiles)
                controlTar.AddFile(pkgFile.PkgFilePath, pkgFile.FileStream,
                    UnixFilePermissions.OwnerAll | UnixFilePermissions.GroupRead, 0, 0, DateTime.UtcNow);
            var dataTarStream = dataTar.Build();

            var controlTgzStream = new MemoryStream();
            using (var zs = new GZipStream(controlTgzStream, CompressionMode.Compress, true))
            {
                controlTarStream.CopyTo(zs);
            }

            var dataTgzStream = new MemoryStream();
            using (var zs = new GZipStream(dataTgzStream, CompressionMode.Compress, true))
            {
                dataTarStream.CopyTo(zs);
            }

            var opkgTar = new TarFileBuilder();
            opkgTar.AddFile(PkgControlFilename, controlTgzStream, UnixFilePermissions.OwnerAll, 0, 0,
                DateTime.UtcNow);
            opkgTar.AddFile(PkgDataFilename, dataTgzStream, UnixFilePermissions.OwnerAll, 0, 0,
                DateTime.UtcNow);
            opkgTar.AddFile(PkgHeaderFilename, PkgHeaderFileContent, UnixFilePermissions.OwnerAll, 0, 0,
                DateTime.UtcNow);
            pkgStream = opkgTar.Build();
        }
    }
}