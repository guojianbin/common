﻿/*
 * Copyright 2006-2016 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;

#if NET40 || NET45 || NETSTANDARD2_0
using System.Threading.Tasks;
#endif

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Ensures that a block of code running on a background thread cleanly exits before a <see cref="CancellationTokenSource.Cancel()"/> call completes.
    /// </summary>
    /// <remarks>Do not use this if <see cref="CancellationTokenSource.Cancel()"/> is called from the same <see cref="SynchronizationContext"/> the guarded code is running under. This could lead to deadlocks.</remarks>
    /// <example>
    /// This class is best used in a using-block:
    /// <code>
    /// using (new CancellationGuard(cancellationToken))
    /// {
    ///     // Your code
    /// }
    /// </code>
    /// </example>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable is only implemented here to support using() blocks.")]
    public class CancellationGuard : IDisposable
    {
        private CancellationTokenRegistration _registration;

#if NET40 || NET45 || NETSTANDARD2_0
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
#else
        private readonly ManualResetEvent _event = new ManualResetEvent(initialState: false);
#endif

        /// <summary>
        /// Registers a callback for the <paramref name="cancellationToken"/> that blocks calls to <see cref="CancellationTokenSource.Cancel()"/> until <see cref="Dispose"/> has been called.
        /// </summary>
        /// <param name="cancellationToken">Used to signal cancellation requests.</param>
        public CancellationGuard(CancellationToken cancellationToken)
        {
            _registration = cancellationToken.Register(
#if NET40 || NET45 || NETSTANDARD2_0
                _tcs.Task.Wait
#else
                () => _event.WaitOne()
#endif
            );
        }

        /// <summary>
        /// Registers a callback for the <paramref name="cancellationToken"/> that blocks calls to <see cref="CancellationTokenSource.Cancel()"/> until <see cref="Dispose"/> has been called.
        /// </summary>
        /// <param name="cancellationToken">Used to signal cancellation requests.</param>
        /// <param name="timeout">A timespan after which the cancellation will be considered completed even if <see cref="Dispose"/> has not been called yet.</param>
        public CancellationGuard(CancellationToken cancellationToken, TimeSpan timeout)
        {
            _registration = cancellationToken.Register(
#if NET40 || NET45 || NETSTANDARD2_0
                () => _tcs.Task.Wait(timeout)
#else
                () =>
                {
                    _event.WaitOne(timeout, exitContext: true);
                    _event.Close();
                }
#endif
            );
        }

        /// <summary>
        /// Releases the block and allows <see cref="CancellationTokenSource.Cancel()"/> to complete.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable is only implemented here to support using() blocks.")]
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "IDisposable is only implemented here to support using() blocks.")]
        public void Dispose()
        {
#if NET40 || NET45 || NETSTANDARD2_0
            _tcs.SetResult(true);
#else
            _event.Set();
#endif
            _registration.Dispose();
        }
    }
}