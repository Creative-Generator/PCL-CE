using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PCL.Core.App.IoC;

namespace PCL.Core.Minecraft.Friends;

/// <summary>
/// 好友系统，用于获取玩家的好友列表、好友请求等信息。
/// 注：多线程安全。
/// </summary>
public sealed class FriendsService : GeneralService
{
    #region Lifecycle
    
    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;
    
    private FriendsService() : base("friends", "好友") { _context = ServiceContext; }
    
    
    public override void Start()
    {
        _Initialize();
    }

    public override void Stop()
    {
        _Uninitialize();
    }

    #endregion
    
    private static readonly ConcurrentDictionary<string, FriendsContext> Contexts = [];
    
    /// <summary>
    /// 用于设置好友系统轮询获取好友详情的延迟。默认为 5 分钟。
    /// 注：Mojang 好友 API 的访问频率限制为每 10 秒一次。
    /// </summary>
    public static TimeSpan Delay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 用于控制好友系统是否开启轮询。
    /// </summary>
    public static bool PollingEnabled
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            foreach (var ctx in Contexts.Values)
            {
                if (value)
                    ResumeLoop(ctx);
                else
                    StopLoop(ctx);
            }
            
        }
    } = true;

    #region 初始化 / 反初始化

    private static void _Initialize()
    {
    }

    private static void _Uninitialize()
    {
        foreach (var ctx in Contexts.Values)
        {
            StopLoop(ctx);
            ctx.Dispose();
        }
        Contexts.Clear();
    }

    #endregion

    #region 轮询

    /// <summary>
    /// 轮询主任务。
    /// </summary>
    /// <param name="ctx">好友上下文。</param>
    private static async Task LoopAsync(FriendsContext ctx)
    {
        while (!ctx.LoopCts.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Delay, ctx.DelayCts.Token);
            }
            catch (TaskCanceledException)
            {
                // 被打断
            }
            
            // 刷新好友列表
            await ctx.RefreshFriendsListAsync();
            
            // 在时间到或被打断后重新创建 cts
            ctx.RefreshDelayCts();
        }
    }

    /// <summary>
    /// 恢复某玩家轮询。
    /// </summary>
    /// <param name="ctx">好友上下文。</param>
    private static void ResumeLoop(FriendsContext ctx)
    {
        ctx.RefreshLoopCts();
        _ = LoopAsync(ctx);
    }

    /// <summary>
    /// 停止某玩家轮询。
    /// </summary>
    /// <param name="ctx">好友上下文。</param>
    private static void StopLoop(FriendsContext ctx)
    {
        ctx.StopLoopCts();
        ctx.CancelDelay();
    }

    #endregion
    
    public void CreateContext(string uuid)
    {
        if (Contexts.ContainsKey(uuid)) return;
        var ctx = new FriendsContext();
        Contexts[uuid] = ctx;
        if (PollingEnabled) ResumeLoop(ctx);
    }
    
    public FriendsContext GetContext(string uuid)
    {
        return Contexts.TryGetValue(uuid, out var ctx)
            ? ctx
            : throw new InvalidOperationException($"好友上下文 {uuid} 不存在。");
    }
    
    public FriendsContext DestroyContext(string uuid)
    {
        if (!Contexts.TryRemove(uuid, out var ctx))
            throw new InvalidOperationException($"好友上下文 {uuid} 不存在。");
        ctx.Dispose();
        return ctx;
    }
}