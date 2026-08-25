namespace PCL.Core.IO.Download;

public record DownloadServiceOptions
{
    /// <summary>
    /// 并发数。
    /// </summary>
    public int MaxConcurrency { get; set; } = 64;
    /// <summary>
    /// 缓冲区大小。
    /// </summary>
    public int BufferSize { get; set; } = 80 * 1024;
    /// <summary>
    /// 块下载回调的报告阈值。
    /// </summary>
    public int ChunkReportThreshold { get; set; } = 64 * 1024;
    /// <summary>
    /// 限制全局每秒下载最大千字节数。默认为 <c>null</c> ，即不限制。
    /// </summary>
    public int? KilobytesPerSecond { get; set; } = null;
}