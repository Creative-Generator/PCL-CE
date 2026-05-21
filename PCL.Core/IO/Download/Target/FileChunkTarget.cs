using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace PCL.Core.IO.Download.Target;

public sealed class FileChunkTarget(string filePath) : IChunkTarget
{
    private readonly SafeFileHandle _handle = File.OpenHandle(filePath, FileMode.Create, FileAccess.Write,
        options: FileOptions.RandomAccess | FileOptions.Asynchronous);
    private bool _isCompleted;

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default)
    {
        if (_isCompleted) throw new InvalidOperationException("目标已经完成。");

        return RandomAccess.WriteAsync(_handle, buffer, offset, ct);
    }

    public async ValueTask CompleteAsync()
    {
        // ReSharper disable once AccessToDisposedClosure
        await Task.Run(() => RandomAccess.FlushToDisk(_handle)).ConfigureAwait(false);

        _isCompleted = true;
        _handle.Dispose();
    }
}