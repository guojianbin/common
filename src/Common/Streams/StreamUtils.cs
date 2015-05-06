/*
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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;

namespace NanoByte.Common.Streams
{
    /// <summary>
    /// Provides <see cref="Stream"/>-related helper methods.
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// Copies the content of one stream to another.
        /// </summary>
        /// <param name="source">The source stream to copy from.</param>
        /// <param name="destination">The destination stream to copy to.</param>
        /// <param name="bufferSize">The size of the buffer to use for copying in bytes.</param>
        /// <param name="cancellationToken">Used to signal when the user wishes to cancel the task execution.</param>
        /// <remarks>Will try to <see cref="Stream.Seek"/> to the start of <paramref name="source"/>.</remarks>
        public static void CopyTo([NotNull] this Stream source, [NotNull] Stream destination, long bufferSize = 4096, CancellationToken cancellationToken = default(CancellationToken))
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            #endregion

            var buffer = new byte[bufferSize];
            int read;

            if (source.CanSeek) source.Position = 0;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                read = source.Read(buffer, 0, buffer.Length);
                destination.Write(buffer, 0, read);
            } while (read != 0);

            if (destination.CanSeek) destination.Position = 0;
        }

        /// <summary>
        /// Writes the entire content of a stream to a file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path of the file to write.</param>
        public static void WriteTo([NotNull] this Stream stream, [NotNull, Localizable(false)] string path)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.Create(path))
                stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Compares two streams for bit-wise equality.
        /// </summary>
        /// <remarks>Will try to <see cref="Stream.Seek"/> to the start of both streams.</remarks>
        public static bool ContentEquals([NotNull] this Stream stream1, [NotNull] Stream stream2)
        {
            #region Sanity checks
            if (stream1 == null) throw new ArgumentNullException("stream1");
            if (stream2 == null) throw new ArgumentNullException("stream2");
            #endregion

            if (stream1.CanSeek) stream1.Position = 0;
            if (stream2.CanSeek) stream2.Position = 0;

            while (true)
            {
                int byte1 = stream1.ReadByte();
                int byte2 = stream2.ReadByte();
                if (byte1 != byte2) return false;
                else if (byte1 == -1) return true;
            }
        }

        /// <summary>
        /// Creates a new <see cref="MemoryStream"/> and fills it with UTF-8 encoded string data.
        /// </summary>
        /// <param name="data">The data to fill the stream with.</param>
        /// <returns>A filled stream with the position set to zero.</returns>
        [NotNull]
        public static MemoryStream ToStream([NotNull] this string data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            var stream = new MemoryStream(byteArray);
            return stream;
        }

        /// <summary>
        /// Reads the entire content of a stream as string data (will seek from zero to end).
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding of the string; leave <see langword="null"/> to default to <see cref="UTF8Encoding"/>.</param>
        /// <returns>A entire content of the stream.</returns>
        [NotNull]
        public static string ReadToString([NotNull] this Stream stream, [CanBeNull] Encoding encoding = null)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            if (stream.CanSeek) stream.Position = 0;
            var reader = new StreamReader(stream, encoding ?? new UTF8Encoding(false));
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Reads the entire content of a stream to a byte array (will seek from zero to end).
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A entire content of the stream.</returns>
        [NotNull]
        public static byte[] ReadToArray([NotNull] this Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Returns an embedded resource/file as a stream.
        /// </summary>
        /// <param name="type">A type that is stored in the same namespace as the embedded resource.</param>
        /// <param name="name">The file name of the embedded resource.</param>
        /// <exception cref="ArgumentException">The specified embedded resource does not exist.</exception>
        [NotNull]
        public static Stream GetEmbeddedStream([NotNull] this Type type, [NotNull, Localizable(false)] string name)
        {
            #region Sanity checks
            if (type == null) throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            var assembly = Assembly.GetAssembly(type);
            var stream = assembly.GetManifestResourceStream(type, name);
            if (stream == null) throw new ArgumentException(string.Format("Embedded resource '{0}' not found.", name), "name");
            return stream;
        }

        /// <summary>
        /// Returns an embedded UTF-8 encoded resource/file as a string.
        /// </summary>
        /// <param name="type">A type that is stored in the same namespace as the embedded resource.</param>
        /// <param name="name">The file name of the embedded resource.</param>
        /// <exception cref="ArgumentException">The specified embedded resource does not exist.</exception>
        [NotNull]
        public static string GetEmbeddedString([NotNull] this Type type, [NotNull, Localizable(false)] string name)
        {
            #region Sanity checks
            if (type == null) throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            using (var stream = type.GetEmbeddedStream(name))
                return stream.ReadToString();
        }

        /// <summary>
        /// Writes an embedded file to a real on-disk file.
        /// </summary>
        /// <param name="type">A type that is stored in the same namespace as the embedded resource.</param>
        /// <param name="name">The file name of the embedded resource.</param>
        /// <param name="path">The path of the file to write.</param>
        /// <exception cref="ArgumentException">The specified embedded resource does not exist.</exception>
        public static void WriteEmbeddedFile([NotNull] this Type type, [NotNull, Localizable(false)] string name, [NotNull, Localizable(false)] string path)
        {
            #region Sanity checks
            if (type == null) throw new ArgumentNullException("type");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var stream = type.GetEmbeddedStream(name))
                stream.WriteTo(path);
        }
    }
}