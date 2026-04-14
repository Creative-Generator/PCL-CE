using System;
using System.Collections.Generic;

namespace PCL.Core.IO.Download;

public sealed record DownloadRequestOptions
{
    public Version? HttpVersion { get; init; }
    public string? UserAgent { get; init; }
    public Uri? Referer { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}