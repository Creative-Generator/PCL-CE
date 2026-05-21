using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Core.IO.Download.Target;

public sealed class NullChunkTarget : IChunkTarget
{
    private bool _isCompleted;
    
    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default)
    {
        if (_isCompleted) throw new InvalidOperationException("目标已经完成。");
        
        return ValueTask.CompletedTask;
    }

    public ValueTask CompleteAsync()
    {
        _isCompleted = true;
        return ValueTask.CompletedTask;
    }
}