using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using PCL.Core.App.IoC;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Clock;
using PCL.Core.UI.Animation.UIAccessProvider;
using PCL.Core.UI.Animation.ValueProcessor;
using PCL.Core.Utils.Threading;

namespace PCL.Core.UI.Animation.Core;

[LifecycleService(LifecycleState.WindowCreating)]
public sealed class AnimationService : GeneralService
{
    #region Lifecycle

    private static LifecycleContext? _context;
    private static LifecycleContext Context => _context!;

    private AnimationService() : base("animation", "动画")
    {
        _context = ServiceContext;
    }
    
    public override void Start()
    {
        Context.Info($"正在启动动画服务，FPS={Fps}，Scale={Scale}");
        try
        {
            _Initialize();
            Context.Info("动画服务启动完成");
        }
        catch (Exception e)
        {
            Context.Error("启动动画服务时发生异常", e);
        }
    }

    public override void Stop()
    {
        Context.Info("正在停止动画服务");
        try
        {
            _Uninitialize();
            Context.Info("动画服务已停止");
        }
        catch (Exception e)
        {
            Context.Error("停止动画服务时发生异常", e);
        }
    }

    #endregion

    private static void _RegisterValueProcessors()
    {
        // 在这里注册所有的 ValueProcessor
        ValueProcessorManager.Register(new DoubleValueProcessor());
        ValueProcessorManager.Register(new MatrixValueProcessor());
        ValueProcessorManager.Register(new NColorValueProcessor());
        ValueProcessorManager.Register(new NRotateTransformValueProcessor());
        ValueProcessorManager.Register(new NScaleTransformValueProcessor());
        ValueProcessorManager.Register(new PointValueProcessor());
        ValueProcessorManager.Register(new ThicknessValueProcessor());
    }

    private static Channel<(IAnimation Animation, IAnimatable Target)> _animationChannel = null!;
    // private static Channel<IAnimationFrame> _frameChannel = null!;
    private static Channel<(IAnimationFrame Frame, IAnimation Source)> _frameChannel = null!;
    // private static ConcurrentDictionary<IAnimatable, IAnimationFrame> _frameDictionary = null!;
    private static ConcurrentDictionary<string, IAnimation> _namedAnimations = new();
    private static IClock _clock = null!;
    private static AsyncCountResetEvent _resetEvent = null!;
    private static int _taskCount;
    private static CancellationTokenSource _cts = null!;
    private static Task[] _animationTasks = null!;
    
    public static int Fps { get; set; } = 60;
    public static double Scale { get; set; } = 1d;

    public static IUIAccessProvider UIAccessProvider { get; private set; } = null!;
    
    private static void _Initialize()
    {
        // 初始化 Channel 与 Dictionary
        _animationChannel = Channel.CreateUnbounded<(IAnimation, IAnimatable)>();
        // _frameChannel = Channel.CreateUnbounded<IAnimationFrame>();
        _frameChannel = Channel.CreateUnbounded<(IAnimationFrame, IAnimation)>();
        
        // 根据核心数量来确定动画计算 Task 数量
        _taskCount = Environment.ProcessorCount;
        Context.Info($"以最多 {_taskCount} 个线程初始化动画计算 Task");

        // 初始化 CancellationTokenSource 与 ResetEvent
        _cts = new CancellationTokenSource();
        _resetEvent = new AsyncCountResetEvent();
        
        // 注册 ValueProcessor
        _RegisterValueProcessors();
        
        // 初始化 UI 线程访问提供器并启动赋值 Task
        UIAccessProvider = new WpfUIAccessProvider(Lifecycle.CurrentApplication.Dispatcher);
        var cancellationToken = _cts.Token;
        _ = UIAccessProvider.InvokeAsync(async () =>
        {
            try
            {
                while (await _frameChannel.Reader.WaitToReadAsync(cancellationToken))
                {
                    // 读取数据
                    while (_frameChannel.Reader.TryRead(out var item))
                    {
                        // 如果动画源已被标记取消，直接丢弃该帧，不进行处理
                        if (item.Source.Status == AnimationStatus.Canceled)
                            continue;

                        try
                        {
                            item.Frame.GetAction()();
                        }
                        catch (Exception ex)
                        {
                            Context.Error(
                                $"执行动画帧失败：动画类型={item.Source.GetType().Name}，名称={item.Source.Name}，状态={item.Source.Status}，当前帧={item.Source.CurrentFrame}",
                                ex);
                        }
                    }

                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                // 服务停止时取消 UI 消费任务是正常流程。
            }
            catch (Exception e)
            {
                Context.Error("动画 UI 消费任务异常退出", e);
            }
        });

        // 初始化 Clock 并注册 Tick 事件
        _clock = new WinMMClock(Fps);
        _clock.Tick += ClockOnTick;
        _clock.Start();
        
        // 运行动画计算 Task
        _animationTasks = new Task[_taskCount];
        for (var i = 0; i < _taskCount; i++)
            _animationTasks[i] = Task.Run(_AnimationComputeTaskAsync);
    }

    private static void _Uninitialize()
    {
        // 先停止产生 Tick，避免停止过程中继续访问 ResetEvent
        _clock.Tick -= ClockOnTick;
        _clock.Stop();

        // 取消并唤醒动画计算 Task
        _cts.Cancel();
        _animationChannel.Writer.TryComplete();
        _frameChannel.Writer.TryComplete();
        _resetEvent.Set(_taskCount);

        // 等待所有计算任务退出后再释放其依赖的资源
        Task.WaitAll(_animationTasks);
        _resetEvent.Dispose();
        _cts.Dispose();
        
        // 清理 Dictionary
        _namedAnimations.Clear();
    }

    private static void ClockOnTick(object? sender, long e)
    {
        // 通知所有等待的动画计算 Task 进行下一帧计算
        _resetEvent.Set(_taskCount);
    }

    private static async Task _AnimationComputeTaskAsync()
    {
        try
        {
            Context.Info($"动画计算任务启动，Task ID={Task.CurrentId}");

            // 本地动画列表，确保没有一直无法计算的动画
            var animationList = new List<(IAnimation Animation, IAnimatable Target)>(8);

            // 持续监听 Channel 中的动画
            while (!_cts.IsCancellationRequested)
            {
                // 读取所有可用的动画到本地列表
                while (_animationChannel.Reader.TryRead(out var animation))
                {
                    // 将动画添加到本地列表
                    animationList.Add(animation);
                }

                _cts.Token.ThrowIfCancellationRequested();
                
                // 如果没有动画，直接等下一帧
                if (animationList.Count == 0)
                {
                    await _resetEvent.WaitAsync();
                    continue;
                }
                
                _cts.Token.ThrowIfCancellationRequested();

                for (var i = animationList.Count - 1; i >= 0; i--)
                {
                    // TODO: 支持缓存动画计算结果 (由 AnimationData 支持)

                    // 从列表中获取动画
                    var animationEntry = animationList[i];

                    // 如果动画已经完成或被取消，则从列表中移除
                    if (animationEntry.Animation.Status is AnimationStatus.Canceled or AnimationStatus.Completed)
                    {
                        animationEntry.Animation.RaiseCompleted();

                        if (!string.IsNullOrEmpty(animationEntry.Animation.Name))
                        {
                            // 使用显式接口
                            ((ICollection<KeyValuePair<string, IAnimation>>)_namedAnimations)
                                .Remove(new KeyValuePair<string, IAnimation>(animationEntry.Animation.Name,
                                    animationEntry.Animation));
                        }

                        animationList.RemoveAt(i);
                        continue;
                    }

                    try
                    {
                        // 计算动画的下一帧
                        var frame = animationEntry.Animation.ComputeNextFrame(animationEntry.Target);
                        // 如果没有计算帧（当动画为 SequentialAnimationGroup 或 ParallelAnimationGroup 这种动画集合时），跳过
                        if (frame is null) continue;
                        // 将动画帧写入 Channel
                        if (!_frameChannel.Writer.TryWrite((frame, animationEntry.Animation)))
                        {
                            Context.Warn(
                                $"动画帧写入 UI Channel 失败：动画类型={animationEntry.Animation.GetType().Name}，名称={animationEntry.Animation.Name}，当前帧={animationEntry.Animation.CurrentFrame}");
                            continue;
                        }

                        // 增加当前帧计数
                        animationEntry.Animation.CurrentFrame++;
                    }
                    catch (Exception e)
                    {
                        Context.Error(
                            $"计算动画帧失败：动画类型={animationEntry.Animation.GetType().Name}，名称={animationEntry.Animation.Name}，状态={animationEntry.Animation.Status}，当前帧={animationEntry.Animation.CurrentFrame}，目标类型={animationEntry.Target.GetType().Name}",
                            e);

                        animationEntry.Animation.Cancel();
                    }
                }

                // 等待 Tick 事件的通知
                await _resetEvent.WaitAsync();
            }
        }
        catch (OperationCanceledException)
        {
            Context.Info($"动画计算任务结束，Task ID={Task.CurrentId}");
        }
        catch (ObjectDisposedException e) when (e.ObjectName == nameof(AsyncCountResetEvent))
        {
            // 停止服务时释放 ResetEvent 会唤醒等待任务并抛出此异常。
            Context.Info($"动画计算任务结束，Task ID={Task.CurrentId}");
        }
        catch (Exception e)
        {
            Context.Error($"动画计算任务异常退出，Task ID={Task.CurrentId}", e);
        }
    }

    private static void _HandleNamedAnimationConflict(IAnimation animation)
    {
        if (string.IsNullOrEmpty(animation.Name)) return;

        _namedAnimations.AddOrUpdate(
            animation.Name, 
            animation, // 如果不存在，直接添加
            (_, existingAnimation) => 
            {
                // 如果已存在同名动画，取消旧动画
                existingAnimation.Cancel();
                // 替换为新动画
                return animation;
            });
    }
    
    internal static Task PushAnimationAsync(IAnimation animation, IAnimatable target)
    {
        _HandleNamedAnimationConflict(animation);
        
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        
        animation.Completed += (_, _) => tcs.SetResult();
        
        _animationChannel.Writer.TryWrite((animation, target));
        return tcs.Task;
    }
    
    internal static void PushAnimationFireAndForget(IAnimation animation, IAnimatable target)
    {
        _HandleNamedAnimationConflict(animation);
        
        _animationChannel.Writer.TryWrite((animation, target));
    }
    
    public static void CancelAnimationByName(string name)
    {
        if (_namedAnimations.TryRemove(name, out var animation))
        {
            animation.Cancel();
            Context.Info($"已取消名为 '{name}' 的动画");
        }
    }
}