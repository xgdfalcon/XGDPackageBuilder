using System.IO;
using System.Text;
using DiscUtils.Streams;
using XGD.PkgBuilder;
using Xunit;
using Xunit.Sdk;

namespace XGD.UnitTest
{
    /// <summary>
    ///     Tests for <see cref="PkgFile" />.
    /// </summary>
    public class PkgFileUnitTests : IClassFixture<PathFixture>
    {
        /// <summary>
        ///     Test text file.
        /// </summary>
        private const string TestFile = "files/testfile.txt";

        /// <summary>
        ///     Test content for the text file.
        /// </summary>
        private const string TextContent = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        ///     Constructor to setup the tests.
        /// </summary>
        public PkgFileUnitTests()
        {
            if (File.Exists(TestFile)) File.Delete(TestFile);
            File.WriteAllText(TestFile, TextContent);
        }

        /// <summary>
        ///     Test creating a <see cref="PkgFile" /> containing a stream.
        /// </summary>
        [Fact]
        public void TestCreatePkgFileStream()
        {
            var stream = SparseStream.FromStream(new MemoryStream(Encoding.ASCII.GetBytes(TextContent)),
                Ownership.Dispose);
            var item = new PkgFile(TestFile, stream);
            var reader = new StreamReader(item.FileStream ?? throw new NullException("fatal error"));
            var result = reader.ReadToEnd();
            Assert.True(result is TextContent);
        }

        /// <summary>
        ///     Test creating a <see cref="PkgFile" /> containing a physical file.
        /// </summary>
        [Fact]
        public void TestCreatePkgFileObject()
        {
            var item = new PkgFile(TestFile, new FileInfo(TestFile));
            var text = File.ReadAllText(item.FileObject?.FullName ?? string.Empty);
            Assert.True(text is TextContent);
        }
    }
}