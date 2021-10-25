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

namespace XGD.PkgBuilder
{
    /// <summary>
    ///     Enumeration of all the file types in the "control" archive in the package.
    /// </summary>
    public enum ControlFileEnum
    {
        /// <summary>
        ///     Package pre-installation maintainer script.
        /// </summary>
        PreInst,

        /// <summary>
        ///     Package post-installation maintainer script.
        /// </summary>
        PostInst,

        /// <summary>
        ///     Package pre-removal maintainer script.
        /// </summary>
        PreRm,

        /// <summary>
        ///     Package post-removal maintainer script.
        /// </summary>
        PostRm
    }

    /// <summary>
    ///     Extensions to <see cref="ControlFileEnum" />.
    /// </summary>
    public static class ControlFileEnumExtensions
    {
        /// <summary>
        ///     Extension to <see cref="ControlFileEnum" /> to print the string version.
        ///     This method uses less memory than "ToString".
        /// </summary>
        /// <param name="input">Enumeration item.</param>
        /// <returns>String filename</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the enum doesn't match.</exception>
        public static string ToFileName(this ControlFileEnum input)
        {
            return input switch
            {
                ControlFileEnum.PostInst => "postinst",
                ControlFileEnum.PreInst => "preinst",
                ControlFileEnum.PreRm => "prerm",
                ControlFileEnum.PostRm => "postrm",
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
        }
    }
}