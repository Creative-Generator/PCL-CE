using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Core.IO.Download.Target;

public interface IChunkTarget
{
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default);
    
    ValueTask CompleteAsync();
}