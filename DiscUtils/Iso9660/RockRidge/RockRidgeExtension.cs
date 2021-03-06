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

using System.Text;

namespace DiscUtils.Iso9660
{
    internal sealed class RockRidgeExtension : SuspExtension
    {
        public RockRidgeExtension(string identifier)
        {
            Identifier = identifier;
        }

        public override string Identifier { get; }

        public override SystemUseEntry Parse(string name, byte length, byte version, byte[] data, int offset, Encoding encoding)
        {
            switch (name)
            {
                case "PX":
                    return new PosixFileInfoSystemUseEntry(name, length, version, data, offset);

                case "NM":
                    return new PosixNameSystemUseEntry(name, length, version, data, offset);

                case "CL":
                    return new ChildLinkSystemUseEntry(name, length, version, data, offset);

                case "TF":
                    return new FileTimeSystemUseEntry(name, length, version, data, offset);

                default:
                    return new GenericSystemUseEntry(name, length, version, data, offset);
            }
        }
    }
}