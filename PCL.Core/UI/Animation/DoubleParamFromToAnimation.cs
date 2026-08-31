using System.Threading;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Core;

namespace PCL.Core.UI.Animation;

// TODO: 旧动画兼容需要，未来必须移除
public class DoubleParamFromToAnimation : FromToAnimationBase<double>
{
    public required ParameterizedThreadStart Param { get; init; }
    
    public override IAnimationFrame? ComputeNextFrame(IAnimatable target)
    {
        // 应用缓动函数
        var easedProgress = Easing.Ease(CurrentFrame, TotalFrames);

        // 计算当前值
        CurrentValue = ValueType == AnimationValueType.Relative
            ? From + To * easedProgress
            : From + (To - From) * easedProgress;

        // TODO: 石山 +1，但没办法，未来删了此处（旧动画兼容必须）
        base.ComputeNextFrame(EmptyAnimatable.Instance);

        return new ActionAnimationFrame
        {
            Action = () => Param(CurrentValue)
        };
    }
}