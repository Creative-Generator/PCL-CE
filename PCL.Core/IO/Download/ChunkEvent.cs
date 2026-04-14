using System;
using System.Net;

namespace PCL.Core.IO.Download;

public sealed record ChunkEvent
{
    public Guid ChunkId { get; init; }
    public ChunkEventType Type { get; init; }
    public HttpStatusCode StatusCode { get; set; }
    public Exception? Error { get; set; }
    public long BytesDownloaded { get; set; }
    public bool SupportsRange { get; set; }
    public long? ContentLength { get; set; }
}