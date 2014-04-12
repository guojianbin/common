﻿/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Runtime.Serialization;

namespace NanoByte.Common
{
    /// <summary>
    /// Like a <see cref="UnauthorizedAccessException"/> but with the additional hint that retrying the operation as an administrator would fix the problem.
    /// </summary>
    [Serializable]
    public class NotAdminException : UnauthorizedAccessException
    {
        /// <inheritdoc/>
        public NotAdminException()
        {}

        /// <inheritdoc/>
        public NotAdminException(string message, Exception inner) : base(message, inner)
        {}

        /// <inheritdoc/>
        public NotAdminException(string message) : base(message)
        {}

        /// <inheritdoc/>
        protected NotAdminException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}