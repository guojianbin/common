﻿/*
 * Copyright 2006-2015 Bastian Eicher
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
using System.Collections.Generic;

namespace NanoByte.Common
{
    /// <summary>
    /// Compares <see cref="INamed{T}"/> objects based on their <see cref="INamed{T}.Name"/> in a case-insensitive way.
    /// </summary>
    public sealed class NamedComparer<T> : IComparer<T>, IEqualityComparer<T> where T : INamed<T>
    {
        /// <summary>A singleton instance of the comparer.</summary>
        public static readonly NamedComparer<T> Instance = new NamedComparer<T>();

        private NamedComparer()
        {}

        public int Compare(T x, T y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);

        public bool Equals(T x, T y) => StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);

        public int GetHashCode(T obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }
}
