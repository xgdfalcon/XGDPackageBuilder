using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscUtils.Streams;
using XGD.PkgBuilder;
using Xunit;

namespace XGD.UnitTest
{
    /// <summary>
    ///     Tests for <see cref="OPkg" />.
    /// </summary>
    public class OPkgUnitTests : IClassFixture<PathFixture>
    {
        /// <summary>
        ///     Test text file.
        /// </summary>
        private static readonly List<PkgFile> DataFiles = new List<PkgFile>()
        {
            new PkgFile("/tmp/test/test1.txt", new FileInfo("files/test1.txt")),
            new PkgFile("/tmp/test/test2.txt", new FileInfo("files/test2.txt")),
            new PkgFile("/tmp/test/test3.txt", new FileInfo("files/test3.txt"))
        };

        /// <summary>
        ///     Test content for the text file.
        /// </summary>
        private static readonly Dictionary<ControlFileEnum, PkgFile> ControlFiles =
            new Dictionary<ControlFileEnum, PkgFile>();
        
        /// <summary>
        ///     Constructor to setup the tests.
        /// </summary>
        public OPkgUnitTests()
        {
            ControlFiles.Add(ControlFileEnum.PostInst, new PkgFile(ControlFileEnum.PostInst.ToFileName(),
                SparseStream.FromStream(new MemoryStream(Encoding.ASCII.GetBytes("postInst")),
                    Ownership.Dispose)));
            ControlFiles.Add(ControlFileEnum.PreInst, new PkgFile(ControlFileEnum.PreInst.ToFileName(),
                SparseStream.FromStream(new MemoryStream(Encoding.ASCII.GetBytes("preInst")),
                    Ownership.Dispose)));
            ControlFiles.Add(ControlFileEnum.PostRm, new PkgFile(ControlFileEnum.PostRm.ToFileName(),
                SparseStream.FromStream(new MemoryStream(Encoding.ASCII.GetBytes("postRm")),
                    Ownership.Dispose)));
            ControlFiles.Add(ControlFileEnum.PreRm, new PkgFile(ControlFileEnum.PreRm.ToFileName(),
                SparseStream.FromStream(new MemoryStream(Encoding.ASCII.GetBytes("preRm")),
                    Ownership.Dispose)));
        }

        /// <summary>
        ///     Test creating a <see cref="OPkg" /> containing a stream.
        /// </summary>
        [Fact]
        public void CreateOPkgStream()
        {
            var pkg = new OPkg(DataFiles, ControlFiles);
            var stream = SparseStream.FromStream(new MemoryStream(), Ownership.Dispose);
            var length = stream.Length;
            pkg.Build(ref stream);
            Assert.True(length != stream.Length);
        }

        /// <summary>
        ///     Test creating a <see cref="OPkg" /> containing a physical file.
        /// </summary>
        [Fact]
        public void TestCreatePkgFileObject()
        {
            var pkg = new OPkg(DataFiles, ControlFiles);
            if (File.Exists("files/file.ipk")) File.Delete("files/file.ipk");
            var outFile = new FileInfo("files/file.ipk");
            pkg.Build(outFile);
            var result = File.ReadAllBytes(outFile.FullName);
            Assert.True(result.Length > 0);
        }
    }
}