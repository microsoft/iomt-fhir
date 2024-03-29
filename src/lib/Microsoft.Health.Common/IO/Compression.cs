// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.IO;
using System.IO.Compression;

namespace Microsoft.Health.Common.IO
{
    public static class Compression
    {
        public static string GzipContentType { get; } = "application/gzip";

        public static byte[] CompressWithGzip(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public static Stream DecompressWithGzip(Stream compressedStream)
        {
            var decompressedStream = new MemoryStream();

            using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzip.CopyTo(decompressedStream);
                decompressedStream.Position = 0;
                return decompressedStream;
            }
        }

        public static byte[] DecompressWithGzip(byte[] bytes)
        {
            using (var compressedStream = new MemoryStream(bytes))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(decompressedStream);
                    }

                    return decompressedStream.ToArray();
                }
            }
        }
    }
}