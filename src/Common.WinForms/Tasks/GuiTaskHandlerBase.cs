﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Net;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Common base class for WinForms <see cref="ITaskHandler"/> implementations.
    /// </summary>
    public abstract class GuiTaskHandlerBase : TaskHandlerBase
    {
        protected GuiTaskHandlerBase()
        {
            if (WindowsUtils.IsWindowsNT)
                CredentialProvider = new CachedCredentialProvider(new WindowsDialogCredentialProvider(this));
        }

        /// <summary>
        /// Stores log messages formatted in RTF for visualization.
        /// </summary>
        protected readonly RtfBuilder LogRtf = new RtfBuilder();

        /// <summary>
        /// Records <see cref="Log"/> messages in an internal log based on their <see cref="LogSeverity"/> and the current <see cref="Verbosity"/> level.
        /// </summary>
        /// <param name="severity">The type/severity of the entry.</param>
        /// <param name="message">The message text of the entry.</param>
        protected override void LogHandler(LogSeverity severity, string message)
        {
            switch (severity)
            {
                case LogSeverity.Debug:
                    if (Verbosity >= Verbosity.Debug) LogRtf.AppendPar(message, RtfColor.Blue);
                    break;
                case LogSeverity.Info:
                    if (Verbosity >= Verbosity.Verbose) LogRtf.AppendPar(message, RtfColor.Green);
                    break;
                case LogSeverity.Warn:
                    LogRtf.AppendPar(message, RtfColor.Orange);
                    break;
                case LogSeverity.Error:
                    LogRtf.AppendPar(message, RtfColor.Red);
                    break;
            }
        }

        /// <inheritdoc/>
        public override ICredentialProvider CredentialProvider { get; }

        /// <inheritdoc/>
        protected override bool Ask(string question, MsgSeverity severity)
        {
            Log.Debug("Question: " + question);
            switch (ThreadUtils.RunSta(() => Msg.YesNoCancel(null, question, severity)))
            {
                case DialogResult.Yes:
                    Log.Debug("Answer: Yes");
                    return true;
                case DialogResult.No:
                    Log.Debug("Answer: No");
                    return false;
                case DialogResult.Cancel:
                default:
                    Log.Debug("Answer: Cancel");
                    throw new OperationCanceledException();
            }
        }

        /// <inheritdoc/>
        public override void Output(string title, string message)
        {
            #region Sanity checks
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (message == null) throw new ArgumentNullException(nameof(message));
            #endregion

            ThreadUtils.RunSta(() => OutputBox.Show(null, title, message.TrimEnd(Environment.NewLine.ToCharArray())));
        }

        /// <inheritdoc/>
        public override void Output<T>(string title, IEnumerable<T> data)
        {
            #region Sanity checks
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (data == null) throw new ArgumentNullException(nameof(data));
            #endregion

            ThreadUtils.RunSta(() => OutputGridBox.Show(null, title, data));
        }

        public override void Error(Exception exception)
        {
            #region Sanity checks
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            #endregion

            ThreadUtils.RunSta(() => ErrorBox.Show(null, exception, LogRtf));
        }
    }
}