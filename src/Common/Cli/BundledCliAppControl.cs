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

using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;

namespace NanoByte.Common.Cli
{
    /// <summary>
    /// Provides an interface to a bundled external command-line application controlled via arguments and stdin and monitored via stdout and stderr.
    /// </summary>
    public abstract class BundledCliAppControl : CliAppControl
    {
        /// <summary>
        /// Returns the directory containing the bundled version of an application.
        /// </summary>
        /// <param name="name">The directory name to search for.</param>
        /// <remarks>
        /// If a sub-directory named like <paramref name="name"/> is found in the installation directory this is used.
        /// Otherwise we try to locate the directory within the "bundled" directory (parallel to "src").
        /// Finally try the working directory.
        /// </remarks>
        [PublicAPI, NotNull]
        public static string GetBundledDirectory(string name)
        {
            string path = Path.Combine(Locations.InstallBase, name); // Subdir of installation directory
            if (Directory.Exists(path)) return path;
            path = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? "", name); // Subdir of library installation diretory
            if (Directory.Exists(path)) return path;
            path = FileUtils.PathCombine(Locations.InstallBase, "..", "..", "..", "bundled", name); // Parallel directory during development
            if (Directory.Exists(path)) return path;
            path = FileUtils.PathCombine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? "", "..", "..", "..", "bundled", name); // Parallel directory during developmen
            if (Directory.Exists(path)) return path;
            path = Path.Combine(Directory.GetCurrentDirectory(), name); // Subdir of working directory
            if (Directory.Exists(path)) return path;
            return Locations.InstallBase; // Installation directory
        }

        /// <summary>
        /// The name of the directory containing the bundled version of this application.
        /// </summary>
        protected abstract string AppDirName { get; }

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(params string[] arguments)
        {
            var startInfo = base.GetStartInfo(arguments);

            // Try to use bundled version of the application when running on Windows
            var appDirectory = GetBundledDirectory(AppDirName);
            string exePath = Path.Combine(appDirectory, AppBinary + ".exe");
            if (WindowsUtils.IsWindows && File.Exists(exePath))
            {
                startInfo.FileName = exePath;
                startInfo.EnvironmentVariables["PATH"] = appDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];
            }

            return startInfo;
        }
    }
}
