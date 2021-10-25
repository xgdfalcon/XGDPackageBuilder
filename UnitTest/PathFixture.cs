using System;
using System.IO;

namespace XGD.UnitTest
{
    /// <summary>
    ///     Fixture to allow for setting up paths relevant to the project file.
    ///     Example: public class PkgFileUnitTests : IClassFixture<PathFixture>
    /// </summary>
    public class PathFixture : IDisposable
    {
        /// <summary>
        ///     Constructor for fixture.
        /// </summary>
        public PathFixture()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
            Directory.SetCurrentDirectory(path);
        }

        /// <summary>
        ///     Dispose method.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}