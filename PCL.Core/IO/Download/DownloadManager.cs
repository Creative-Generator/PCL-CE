using System;

namespace PCL.Core.IO.Download;

public sealed class DownloadManager
{
    private static readonly Lazy<DownloadManager> Lazy = new(() => new DownloadManager());
    
    public static DownloadManager Current => Lazy.Value;
    
    private DownloadManager()
    {
        
    }
    
}