using System;
using PCL.Core.IO.Download.Scheduler;

namespace PCL.Core.IO.Download;

public sealed class DownloadManager
{
    private static readonly Lazy<DownloadManager> Lazy = new(() => new DownloadManager());
    
    public static DownloadManager Current => Lazy.Value;

    public IDownloadScheduler Scheduler = new PipelineDownloadScheduler();
    
    private DownloadManager()
    {
        
    }
    
}