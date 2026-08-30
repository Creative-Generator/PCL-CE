using System;
using PCL.Core.UI.Animation.Core;

namespace PCL.Core.UI.Animation;

public struct ActionAnimationFrame(Action action) : IAnimationFrame
{
    public Action Action { get; set; } = action;

    public Action GetAction() => Action;
}