﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace OpenMcdf
{
    /// <summary>
    /// Unzip
    /// </summary>
    public static class CompoundFileUnzipper
    {
        public static void Unzip(string sourceCompoundFileName, string destinationDirectoryName)
        {

        }

        public static void Unzip(Stream sourceCompoundFileStream, string destinationDirectoryName, IByteArrayPool byteArrayPool = null)
        {
            byteArrayPool ??= new ByteArrayPool();

            Stream stream;
            if (sourceCompoundFileStream.CanSeek is false)
            {
                stream = new ForwardSeekStream(sourceCompoundFileStream, byteArrayPool);
            }
            else
            {
                stream = sourceCompoundFileStream;
            }

            var compoundFile = new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.Default);

            Unzip(compoundFile,compoundFile.RootStorage, destinationDirectoryName, byteArrayPool);
        }

        private static void Unzip(CompoundFile compoundFile, CFStorage cfStorage, string destinationDirectoryName, IByteArrayPool byteArrayPool)
        {
            Directory.CreateDirectory(destinationDirectoryName);

            cfStorage.VisitEntries(cfItem =>
            {
                var name = FileNameHelper.FixFileName(cfItem.Name);

                if (cfItem is CFStorage storage)
                {
                    var folder = Path.Combine(destinationDirectoryName, name);
                    Unzip(compoundFile, storage, folder, byteArrayPool);
                }
                else if (cfItem is CFStream stream)
                {
                    var file = Path.Combine(destinationDirectoryName, name);
                    using var fileStream = new FileStream(file,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);

                    compoundFile.CopyTo(stream, fileStream);
                }

            }, false);
        }
    }
}
