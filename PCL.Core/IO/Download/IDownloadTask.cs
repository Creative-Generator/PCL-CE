using System;

namespace PCL.Core.IO.Download;

public interface IDownloadTask
{
    Guid Id { get; }
}