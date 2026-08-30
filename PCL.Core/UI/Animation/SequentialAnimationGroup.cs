using System.Linq;
using System.Threading.Tasks;
using PCL.Core.Logging;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Core;

namespace PCL.Core.UI.Animation;

/// <summary>
/// 按顺序执行的动画集合。
/// </summary>
public sealed class SequentialAnimationGroup : AnimationGroup
{
    private TaskCompletionSource? _cancelTcs;

    public override async Task<IAnimation> RunAsync(IAnimatable target)
    {
        Status = AnimationStatus.Running;
        AnimationService.PushAnimationFireAndForget(this, target);
        
        LogWrapper.Debug("Animation", $"顺序动画组初始化：{Name}，有 {Children.Count} 个子动画");
        
        _cancelTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var childrenSnapshot = Children.ToList();

        lock (ChildrenCore)
        {
            if (Status == AnimationStatus.Canceled) return this;
        }

        try
        {
            foreach (var child in childrenSnapshot)
            {
                // 检查取消信号
                if (Status == AnimationStatus.Canceled) break;

                var childTarget = ResolveTarget(child, target);
                Task childWaiter;

                lock (ChildrenCore)
                {
                    var runChild = child.RunFireAndForget(childTarget);
                    ChildrenCore.Add(runChild);
                    
                    childWaiter = CreateChildAwaiter(runChild);
                }

                // 等待到当前子动画完成或组被取消
                await Task.WhenAny(childWaiter, _cancelTcs.Task);
                
                // 如果是取消触发的醒来，直接跳出循环
                if (Status == AnimationStatus.Canceled) break;
            }
        }
        finally
        {
            if (Status != AnimationStatus.Canceled)
            {
                Status = AnimationStatus.Completed;
            }
            _cancelTcs = null;
        }

        return this;
    }

    public override void Cancel()
    {
        base.Cancel();
        _cancelTcs?.TrySetResult();
    }

    public override IAnimation RunFireAndForget(IAnimatable target)
    {
        _ = RunAsync(target);
        return this;
    }
}