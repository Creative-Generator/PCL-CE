using System;
using System.Buffers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using PCL.Core.App.IoC;
using PCL.Core.IO.Net;

namespace PCL.Core.IO.Download;

[LifecycleService(LifecycleState.Loaded)]
public sealed class DownloadService : GeneralService
{
    #region Lifecycle
    
    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;
    public DownloadService() : base("download", "下载服务") { _context = ServiceContext; }
    
    public override void Start() => _Initialize();
    public override void Stop() => _Uninitialize();

    #endregion

    private static Channel<DownloadChunk> _chunkChannel = null!;
    private static SemaphoreSlim _connectionSemaphore = null!;
    private static TokenBucketRateLimiter? _rateLimiter;

    /// <summary>
    /// DownloadService 的设置项。
    /// </summary>
    public static DownloadServiceOptions Options { get; set; } = new DownloadServiceOptions();

    private void _Initialize()
    {
        // 初始化 Channel
        _chunkChannel = Channel.CreateUnbounded<DownloadChunk>();
        
        // 初始化控制并发的信号量
        _connectionSemaphore = new SemaphoreSlim(Options.MaxConcurrency, Options.MaxConcurrency);
        
        if (Options.KilobytesPerSecond > 0)
        {
            var kilobytesPerSecond = Options.KilobytesPerSecond.Value;

            _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = kilobytesPerSecond,
                TokensPerPeriod = kilobytesPerSecond,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = int.MaxValue
            });
        }
        
        // 运行 Worker
        Task.Run(WorkerAsync);
    }
    
    private void _Uninitialize()
    {
        // 释放信号量
        _connectionSemaphore.Dispose();
    }
    
    // TODO: 如果可行，使用 System.Threading.Tasks.Dataflow 来替代 Channel 和 SemaphoreSlim 的组合，以简化代码
    // 注：如果换成 TPL Dataflow，有可能导致可读性下降（尤其是对于没接触过 TPL Dataflow 的人来说）
    
    private async Task WorkerAsync()
    {
        await foreach (var chunk in _chunkChannel.Reader.ReadAllAsync())
        {
            // 等待信号量，控制并发数量
            await _connectionSemaphore.WaitAsync();
            
            // 处理下载任务
            _ = ProcessChunkAsync(chunk);
        }
    }

    private async Task ProcessChunkAsync(DownloadChunk chunk)
    {
        // 获取请求
        var request = CreateRequest(chunk);
        var downloaded = 0L;

        try
        {
            // 发送请求
            using var response = await NetworkService.GetClient(/*"cache"*/) // 注：等待其他 PR 提供的缓存支持
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, chunk.CancellationToken);

            // 从请求获取信息
            var supportsRange = response.StatusCode == HttpStatusCode.PartialContent;
            var contentLength = response.Content.Headers.ContentLength;

            response.EnsureSuccessStatusCode();

            // 开始下载，发送带有探测的回调
            chunk.ChunkCallback.Invoke(new ChunkEvent
            {
                ChunkId = chunk.Id,
                Type = ChunkEventType.Started,
                StatusCode = response.StatusCode,
                SupportsRange = supportsRange,
                ContentLength = contentLength
            });

            // 将请求转成流
            await using var stream = await response.Content.ReadAsStreamAsync(chunk.CancellationToken);

            // 获取缓冲区
            var buffer = ArrayPool<byte>.Shared.Rent(Options.BufferSize);

            // 单块的限速设置
            TokenBucketRateLimiter? chunkRateLimiter = null;
            if (chunk.KilobytesPerSecond > 0)
            {
                var kilobytesPerSecond = chunk.KilobytesPerSecond.Value;

                chunkRateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    TokenLimit = kilobytesPerSecond,
                    TokensPerPeriod = kilobytesPerSecond,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                    AutoReplenishment = true,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = int.MaxValue
                });
            }
            
            try
            {
                var lastReported = 0L;

                while (true)
                {
                    // 从流读取
                    var read = await stream.ReadAsync(buffer, chunk.CancellationToken);
                    if (read <= 0) break;

                    var tokens = (read + 1024 - 1) / 1024;  // 1KB
                    
                    // 全局限速
                    if (_rateLimiter is not null)
                    {
                        using var g = await _rateLimiter.AcquireAsync(tokens, chunk.CancellationToken);
                    }
                    // 块限速
                    if (chunkRateLimiter is not null)
                    {
                        using var c = await chunkRateLimiter.AcquireAsync(tokens, chunk.CancellationToken);
                    }

                    // 向目标写入下载到的内容
                    await chunk.Target.WriteAsync(
                        buffer.AsMemory(0, read),
                        (chunk.Offset ?? 0) + downloaded,
                        chunk.CancellationToken);

                    downloaded += read;

                    // 判断是否达到阈值
                    if (downloaded - lastReported >= Options.ChunkReportThreshold)
                    {
                        lastReported = downloaded;

                        // 发送实时回调
                        chunk.ChunkCallback.Invoke(new ChunkEvent
                        {
                            ChunkId = chunk.Id,
                            Type = ChunkEventType.Progress,
                            BytesDownloaded = downloaded
                        });
                    }
                }

                // 补尾
                if (downloaded != lastReported)
                {
                    chunk.ChunkCallback.Invoke(new ChunkEvent
                    {
                        ChunkId = chunk.Id,
                        Type = ChunkEventType.Progress,
                        BytesDownloaded = downloaded
                    });
                }

                // 下载完成
                chunk.ChunkCallback.Invoke(new ChunkEvent
                {
                    ChunkId = chunk.Id,
                    Type = ChunkEventType.Completed,
                    BytesDownloaded = downloaded
                });
            }
            finally
            {
                // 归还缓冲区
                ArrayPool<byte>.Shared.Return(buffer);
                // 释放限制器
                if (chunkRateLimiter is not null)
                {
                    await chunkRateLimiter.DisposeAsync();
                }  
            }
        }
        catch (OperationCanceledException)
        {
            // 暂停
            chunk.ChunkCallback.Invoke(new ChunkEvent
            {
                ChunkId = chunk.Id,
                Type = ChunkEventType.Paused,
                BytesDownloaded = downloaded
            });
        }
        catch (Exception e)
        {
            // 失败
            chunk.ChunkCallback.Invoke(new ChunkEvent
            {
                ChunkId = chunk.Id,
                Type = ChunkEventType.Failed,
                Error = e,
                BytesDownloaded = downloaded
            });
        }
        finally
        {
            // 释放限制并发的信号量
            _connectionSemaphore.Release();
        }
    }

    private static HttpRequestMessage CreateRequest(DownloadChunk chunk)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, chunk.Url);
        var options = chunk.Options;

        request.Version = options.HttpVersion ?? HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

        if (options.UserAgent is not null)
        {
            request.Headers.UserAgent.ParseAdd(options.UserAgent);
        }

        if (options.Referer is not null)
        {
            request.Headers.Referrer = options.Referer;
        }

        if (options.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        if (chunk.Offset is not null)
        {
            var start = chunk.Offset.Value;
            long? end = null;

            if (chunk.Length is not null)
            {
                end = start + chunk.Length - 1;
            }

            request.Headers.Range = new RangeHeaderValue(start, end);
        }

        return request;
    }
    
    internal static void PushChunk(DownloadChunk chunk)
    {
        _chunkChannel.Writer.TryWrite(chunk);
    }
}