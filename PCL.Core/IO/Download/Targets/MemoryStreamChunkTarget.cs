using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Core.IO.Download.Targets;

public sealed class MemoryStreamChunkTarget(MemoryStream memoryStream) : IChunkTarget
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private bool _isCompleted;
    
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default)
    {
        if (_isCompleted) throw new InvalidOperationException("目标已经完成。");

        // 异步等待获取锁
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            memoryStream.Seek(offset, SeekOrigin.Begin);
            await memoryStream.WriteAsync(buffer, ct).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public ValueTask CompleteAsync()
    {
        _isCompleted = true;
        _semaphore.Release();
        return ValueTask.CompletedTask;
    }
}