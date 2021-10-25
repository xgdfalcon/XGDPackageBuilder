using System;
using System.Collections.Generic;
using System.IO;
using DiscUtils.Streams;

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Creates an RPM package for installation on ReaHat compatible systems.
    ///     TODO: Implement RPM
    /// </summary>
    public class Rpm : BasePkg
    {
        /// <summary>
        ///     Create the object for the RPM with all necessary information.
        /// </summary>
        /// <param name="dataFiles">The data files to include in the package.</param>
        /// <exception cref="NotImplementedException">This class is not yet implemented.</exception>
        public Rpm(List<PkgFile> dataFiles) : base(dataFiles)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="pkgFile"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Build(FileInfo pkgFile)
        {
            throw new NotImplementedException();
        }

        public override void Build(ref SparseStream pkgStream)
        {
            throw new NotImplementedException();
        }
    }
}