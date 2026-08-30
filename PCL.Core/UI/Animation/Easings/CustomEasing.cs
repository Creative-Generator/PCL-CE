using System;

namespace PCL.Core.UI.Animation.Easings;

public class CustomEasing : Easing
{
    public Func<double, double> EasingFunction { get; set; }

    protected override double EaseCore(double progress) => EasingFunction(progress);
}