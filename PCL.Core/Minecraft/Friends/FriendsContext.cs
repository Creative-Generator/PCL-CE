using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Core.Minecraft.Friends;

public sealed class FriendsContext
{
    private bool _disposed;
    
    private CancellationTokenSource _delayCts = new();
    private CancellationTokenSource _loopCts = new();

    internal CancellationTokenSource DelayCts => _delayCts;
    internal CancellationTokenSource LoopCts => _loopCts;

    internal async Task RefreshFriendsListAsync()
    {
        // TODO
    }

    /// <summary>
    /// 中断当前延时，使轮询循环立刻进入下一次迭代。
    /// </summary>
    internal void CancelDelay()
    {
        if (_disposed) return;
        try { _delayCts.Cancel(); }
        catch (ObjectDisposedException) { }
    }
    
    internal void RefreshDelayCts()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var old = Interlocked.Exchange(ref _delayCts, new CancellationTokenSource());
        try { old.Cancel(); } catch (ObjectDisposedException) { }
        try { old.Dispose(); } catch (ObjectDisposedException) { }
    }
    
    internal void RefreshLoopCts()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var old = Interlocked.Exchange(ref _loopCts, new CancellationTokenSource());
        try { old.Cancel(); } catch (ObjectDisposedException) { }
        try { old.Dispose(); } catch (ObjectDisposedException) { }
    }
    
    internal void StopLoopCts()
    {
        if (_disposed) return;
        try { _loopCts.Cancel(); } catch (ObjectDisposedException) { }
    }
    
    /// <summary>
    /// 销毁上下文及其所有资源。仅应由 <see cref="FriendsService"/> 调用。
    /// </summary>
    internal void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        CancelAndDispose(_loopCts);
        CancelAndDispose(_delayCts);
        return;

        static void CancelAndDispose(CancellationTokenSource cts)
        {
            try { cts.Cancel(); } catch (ObjectDisposedException) { }
            try { cts.Dispose(); } catch (ObjectDisposedException) { }
        }
    }
}