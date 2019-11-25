﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.Content
{
    public sealed class ZipArchiveReader : IDataReader
    {
        private readonly ZipArchive _archive;

        private readonly string _archivePath;

        private readonly Mutex _exclusiveStreamAccessMutex;
        private readonly string _subPath;

        public ZipArchiveReader(string archivePath, string subPath = "")
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("Archive path not found.", archivePath);

            this._archivePath = archivePath;
            this._subPath = subPath;

            this._exclusiveStreamAccessMutex = new Mutex(false);

            this._archive = ZipFile.OpenRead(archivePath);
        }

        public IDataReader GetSubPath(string subPath)
        {
            return new ZipArchiveReader(this._archivePath, Path.Combine(subPath));
        }

        /// <inheritdoc />
        public string GetPathRepresentation(string relativeFilePath = null)
        {
            return
                $"{this._archivePath}[{Path.GetFileName(Path.Combine(this._subPath, relativeFilePath ?? string.Empty))}]";
        }

        /// <inheritdoc />
        public void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "",
            IProgress<string> progress = null)
        {
            var validEntries = this._archive.Entries
                .Where(e => e.Name.EndsWith($"{fileExtension}", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var entry in validEntries)
            {
                progress?.Report(string.Format(Strings.GameServices.ContentService.LoadingEntry, entry.Name));
                var entryStream = GetFileStream(entry.FullName);

                loadFileFunc.Invoke(entryStream, this);
            }
        }

        /// <inheritdoc />
        public bool FileExists(string filePath)
        {
            return this._archive.Entries.Any(entry =>
                string.Equals(Path.Combine(this._subPath, entry.FullName.Replace(@"\", "/")), filePath,
                    StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <inheritdoc />
        public Stream GetFileStream(string filePath)
        {
            ZipArchiveEntry fileEntry;

            if ((fileEntry = GetArchiveEntry(filePath)) != null)
            {
                this._exclusiveStreamAccessMutex.WaitOne();

                var memStream = new MemoryStream();
                using (var entryStream = fileEntry.Open())
                {
                    entryStream.CopyTo(memStream);
                }

                memStream.Position = 0;

                this._exclusiveStreamAccessMutex.ReleaseMutex();
                return memStream;
            }

            return null;
        }

        /// <inheritdoc />
        public byte[] GetFileBytes(string filePath)
        {
            // We know GetFileStream returns a MemoryStream, so we don't check
            using (var fileStream = GetFileStream(filePath) as MemoryStream)
            {
                if (fileStream != null)
                {
                    return fileStream.ToArray();
                }
            }

            return null;
        }

        /// <inheritdoc />
        public int GetFileBytes(string filePath, out byte[] fileBuffer)
        {
            fileBuffer = null;

            // We know GetFileStream returns a MemoryStream, so we don't check
            using (var fileStream = GetFileStream(filePath) as MemoryStream)
            {
                if (fileStream != null)
                {
                    fileBuffer = fileStream.GetBuffer();
                    return (int) fileStream.Length;
                }
            }

            return 0;
        }

        /// <inheritdoc />
        /// <remarks>For <see cref="ZipArchiveReader" />, use <see cref="GetFileStream(string)" /> instead.</remarks>
        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            return await Task.FromResult(GetFileStream(filePath));
        }

        /// <inheritdoc />
        /// <remarks>For <see cref="ZipArchiveReader" />, use <see cref="GetFileBytes(string)" /> instead.</remarks>
        public async Task<byte[]> GetFileBytesAsync(string filePath)
        {
            return await Task.FromResult(GetFileBytes(filePath));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this._archive?.Dispose();
        }

        private string GetUniformFileName(string filePath)
        {
            return filePath.Replace(@"\", "/").Replace("//", "/").Trim();
        }

        private ZipArchiveEntry GetArchiveEntry(string filePath)
        {
            var cleanFilePath = GetUniformFileName(Path.Combine(this._subPath, filePath));

            foreach (var zipEntry in this._archive.Entries)
            {
                var cleanZipEntry = GetUniformFileName(zipEntry.FullName);

                if (string.Equals(cleanFilePath, cleanZipEntry, StringComparison.OrdinalIgnoreCase))
                {
                    return zipEntry;
                }
            }

            return null;
        }
    }
}