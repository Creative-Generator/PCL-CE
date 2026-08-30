using System;
using System.Windows;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Core;
using PCL.Core.Utils;

namespace PCL.Core.UI.Animation;

public readonly struct XAndYFromToAnimationFrame(
    IAnimatable target,
    double value,
    XAndYFromToAnimationFrameType type) : IAnimationFrame
{
    public IAnimatable Target { get; init; } = target;
    public double Value { get; init; } = value;
    public XAndYFromToAnimationFrameType Type { get; init; } = type;

    public Action GetAction()
    {
        var animatable = target;
        var d = value;

        // TODO: 石山 +1，但没办法，未来删了此处（旧动画兼容必须）
        return Type switch
        {
            XAndYFromToAnimationFrameType.X => () =>
                ControlHelper.OffsetX((FrameworkElement)((WpfAnimatable)animatable).Owner, d),
            XAndYFromToAnimationFrameType.Y => () =>
                ControlHelper.OffsetY((FrameworkElement)((WpfAnimatable)animatable).Owner, d),
            _ => throw new NotSupportedException("这怎么可能会发生？")
        };
    }
}

public enum XAndYFromToAnimationFrameType
{
    X,
    Y
}