﻿using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;

namespace NanoByte.Common.Net
{
    /// <summary>
    /// Common base class for <see cref="ICredentialProvider"/> implementations.
    /// </summary>
    public abstract class CredentialProviderBase : MarshalNoTimeout, ICredentialProvider
    {
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new credential provider.
        /// </summary>
        /// <param name="handler">Used to determine whether and how to ask the user for input.</param>
        protected CredentialProviderBase([NotNull] ITaskHandler handler) => _handler = handler;

        /// <inheritdoc/>
        public bool Interactive => _handler.Verbosity > Verbosity.Batch;

        /// <inheritdoc/>
        [CanBeNull]
        public abstract NetworkCredential GetCredential(Uri uri, string authType);

        private readonly HashSet<Uri> _invalidList = new HashSet<Uri>();

        /// <inheritdoc/>
        public void ReportInvalid(Uri uri)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            #endregion

            lock (_invalidList)
                _invalidList.Add(uri);
        }

        /// <summary>
        /// Checks whether <paramref name="uri"/> was previously reported as invalid and resets the flag.
        /// </summary>
        protected bool WasReportedInvalid([NotNull] Uri uri)
        {
            lock (_invalidList)
                return _invalidList.Remove(uri);
        }
    }
}
