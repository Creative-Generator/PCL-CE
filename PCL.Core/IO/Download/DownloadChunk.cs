using System;
using System.Threading;
using PCL.Core.IO.Download.Targets;

namespace PCL.Core.IO.Download;

public sealed record DownloadChunk
{
    public required Guid Id { get; init; }
    public required Uri Url { get; init; }
    public required long? Offset { get; init; }
    public required long? Length { get; init; }
    public required IChunkTarget Target { get; init; }
    public Action<ChunkEvent> ChunkCallback { get; init; }
    public DownloadRequestOptions Options { get; init; }
    public CancellationToken CancellationToken { get; init; }
}