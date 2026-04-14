using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace PCL.Core.IO.Download.Targets;

public sealed class FileChunkTarget(string filePath) : IChunkTarget
{
    private readonly SafeFileHandle _handle = File.OpenHandle(filePath, FileMode.Create, FileAccess.Write);
    private bool _isCompleted;

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default)
    {
        if (_isCompleted) throw new InvalidOperationException("目标已经完成。");
        
        return RandomAccess.WriteAsync(_handle, buffer, offset, ct);
    }

    public ValueTask CompleteAsync()
    {
        RandomAccess.FlushToDisk(_handle);
        
        _isCompleted = true;
        _handle.Dispose();
        return ValueTask.CompletedTask;
    }
}