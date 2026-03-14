using System.Buffers;
using Aitive.Framework.Cryptography.Hashing;
using Aitive.Framework.Cryptography.Hashing.Algorithms;

namespace Aitive.Framework.Data.Transfer;

public static class DataTransferHandlerExtensions
{
    extension(IDataTransferHandler handler)
    {
        /// <summary>
        /// Transfers <see cref="DataTransferFile"/> to a local path, with chunk-level and
        /// whole-file hash verification, resume support, and pooled memory.
        /// </summary>
        public async Task TransferToFileAsync(
            IHashProvider<Sha256Value> hashProvider,
            DataTransferFile file,
            string targetPath,
            DataTransferOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(file);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

            options ??= new DataTransferOptions();

            if (options.BlockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    "BlockSize must be positive."
                );
            }

            // Ensure the target directory exists.
            var directory = Path.GetDirectoryName(Path.GetFullPath(targetPath));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var startOffset = await ResolveStartOffsetAsync(
                file,
                hashProvider,
                targetPath,
                options.ForceRestart,
                cancellationToken
            );

            if (startOffset == file.Size)
            {
                // File already exists and verified.
                options.Progress?.Report(new DataTransferProgress(file.Size, file.Size, 0));
                return;
            }

            await TransferCoreAsync(
                handler,
                hashProvider,
                file,
                targetPath,
                startOffset,
                options,
                cancellationToken
            );

            await VerifyFinalHashAsync(hashProvider, file, targetPath, cancellationToken);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Resume resolution
    // ──────────────────────────────────────────────────────────────────

    private static async Task<long> ResolveStartOffsetAsync(
        DataTransferFile file,
        IHashProvider<Sha256Value> hashProvider,
        string targetPath,
        bool forceRestart,
        CancellationToken cancellationToken
    )
    {
        if (!File.Exists(targetPath))
        {
            return 0;
        }

        if (forceRestart)
        {
            File.Delete(targetPath);
            return 0;
        }

        var info = new FileInfo(targetPath);

        if (info.Length > file.Size)
        {
            // Larger than expected — corrupt, start over.
            File.Delete(targetPath);
            return 0;
        }

        if (info.Length == file.Size)
        {
            // Full-size file on disk — verify its hash.
            Sha256Value hash;
            await using (
                var stream = new FileStream(
                    targetPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81_920,
                    useAsync: true
                )
            )
            {
                hash = await hashProvider.Algorithm.ComputeAsync(stream, cancellationToken);
            }

            if (hash == file.Hash)
            {
                return file.Size; // Already complete.
            }

            // Right size, wrong hash — delete and restart.
            File.Delete(targetPath);
            return 0;
        }

        // Partial file — resume from current length.
        return info.Length;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Core transfer loop
    // ──────────────────────────────────────────────────────────────────

    private static async Task TransferCoreAsync(
        IDataTransferHandler handler,
        IHashProvider<Sha256Value> hashProvider,
        DataTransferFile file,
        string targetPath,
        long startOffset,
        DataTransferOptions options,
        CancellationToken cancellationToken
    )
    {
        var blockSize = options.BlockSize;
        var buffer = ArrayPool<byte>.Shared.Rent(blockSize);
        try
        {
            // Open or create; seek to resume position if resuming.
            var fileMode = startOffset > 0 ? FileMode.Open : FileMode.Create;
            await using var fs = new FileStream(
                targetPath,
                fileMode,
                FileAccess.Write,
                FileShare.None,
                bufferSize: blockSize,
                useAsync: true
            );

            if (startOffset > 0)
            {
                fs.Seek(startOffset, SeekOrigin.Begin);
            }

            var offset = startOffset;
            var chunksCompleted = 0;

            while (offset < file.Size)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var remaining = file.Size - offset;
                var readSize = (int)Math.Min(blockSize, remaining);
                var memory = buffer.AsMemory(0, readSize);

                var result = await handler.Read(memory, offset, cancellationToken);

                if (!result)
                {
                    throw new IOException($"Data transfer read failed at offset {offset}.");
                }

                if (result.Value.Length <= 0)
                {
                    throw new IOException(
                        $"Data transfer read returned no data at offset {offset}."
                    );
                }

                var bytesRead = (int)result.Value.Length;

                // Verify this chunk's integrity.
                VerifyChunkHash(hashProvider, buffer, bytesRead, result.Value.Hash, offset);

                await fs.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                offset += bytesRead;
                chunksCompleted++;

                options.Progress?.Report(
                    new DataTransferProgress(offset, file.Size, chunksCompleted)
                );
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Hash verification
    // ──────────────────────────────────────────────────────────────────

    private static void VerifyChunkHash(
        IHashProvider<Sha256Value> hashProvider,
        byte[] buffer,
        int length,
        Sha256Value expected,
        long offset
    )
    {
        using var ms = new MemoryStream(buffer, 0, length, writable: false);
        var actual = hashProvider.Algorithm.Compute(ms);

        if (actual != expected)
        {
            throw new InvalidDataException($"Chunk hash mismatch at offset {offset}.");
        }
    }

    private static async Task VerifyFinalHashAsync(
        IHashProvider<Sha256Value> hashProvider,
        DataTransferFile file,
        string targetPath,
        CancellationToken cancellationToken
    )
    {
        Sha256Value finalHash;
        await using (
            var stream = new FileStream(
                targetPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81_920,
                useAsync: true
            )
        )
        {
            finalHash = await hashProvider.Algorithm.ComputeAsync(stream, cancellationToken);
        }

        if (finalHash != file.Hash)
        {
            File.Delete(targetPath);
            throw new InvalidDataException(
                $"Final file hash verification failed for '{targetPath}'. "
                    + "The corrupted file has been deleted."
            );
        }
    }
}
