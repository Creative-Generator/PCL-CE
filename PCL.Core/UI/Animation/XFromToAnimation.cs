using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Core;
using PCL.Core.UI.Animation.ValueProcessor;

namespace PCL.Core.UI.Animation;

public class XFromToAnimation : FromToAnimationBase<double>
{
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
        
        return new XAndYFromToAnimationFrame
        {
            Target = target,
            Value = ValueType == AnimationValueType.Relative
                ? CurrentValue
                : ValueProcessorManager.Subtract(CurrentValue, From),
            Type = XAndYFromToAnimationFrameType.X
        };
    }
}