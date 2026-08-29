using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Easings;

namespace PCL.Core.UI.Animation.Core;

public static class AnimationExtensions
{
    #region 附加属性

    public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
        "Target", typeof(DependencyObject), typeof(AnimationExtensions), new PropertyMetadata(default(DependencyObject)));

    public static void SetTarget(DependencyObject element, DependencyObject value)
    {
        if (element is not IAnimation)
            throw new InvalidOperationException("AnimationExtensions.Target 只能附加到 IAnimation 实例上。");
        
        element.SetValue(TargetProperty, value);
    }

    public static DependencyObject GetTarget(DependencyObject element)
    {
        return (DependencyObject)element.GetValue(TargetProperty);
    }

    public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.RegisterAttached(
        "TargetProperty", typeof(DependencyProperty), typeof(AnimationExtensions), new PropertyMetadata(default(DependencyProperty)));
    
    public static void SetTargetProperty(DependencyObject element, DependencyProperty value)
    {
        if (element is not IAnimation)
            throw new InvalidOperationException("AnimationExtensions.TargetProperty 只能附加到 IAnimation 实例上。");
        
        element.SetValue(TargetPropertyProperty, value);
    }

    public static DependencyProperty GetTargetProperty(DependencyObject element)
    {
        return (DependencyProperty)element.GetValue(TargetPropertyProperty);
    }

    #endregion

    public static void Animate(this DependencyObject target, TimeSpan? duration = null, TimeSpan? delay = null,
        IEasing? easing = null, AnimationValueType valueType = AnimationValueType.Relative, int iterationCount = 1,
        double? width = null,
        double? height = null,
        double? opacity = null,
        CornerRadius? radius = null,
        NTranslateTransform? translate = null,
        double? translateX = null,
        double? translateY = null,
        NRotateTransform? rotate = null,
        double? rotateAngle = null,
        NScaleTransform? scale = null,
        double? scaleX = null,
        double? scaleY = null,
        NSkewTransform? skew = null,
        double? skewX = null,
        double? skewY = null,
        Thickness? margin = null,
        double? marginLeft = null,
        double? marginTop = null,
        double? marginRight = null,
        double? marginBottom = null,
        Thickness? padding = null,
        double? paddingLeft = null,
        double? paddingTop = null,
        double? paddingRight = null,
        double? paddingBottom = null,
        NColor? background = null,
        NColor? foreground = null)
    {
        duration ??= TimeSpan.FromMilliseconds(100);
        delay ??= TimeSpan.Zero;
        easing ??= LinearEasing.Shared;

        var aniGroup = new ParallelAnimationGroup();

        // 这是全 CE 最石的代码（没有之一），你敢直视他 1 秒吗？

        if (width is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = width.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.WidthProperty);
            aniGroup.Children.Add(ani);
        }

        if (height is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = height.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.HeightProperty);
            aniGroup.Children.Add(ani);
        }

        if (opacity is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = opacity.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, UIElement.OpacityProperty);
            aniGroup.Children.Add(ani);
        }

        if (radius is not null)
        {
            var ani = new CornerRadiusFromToAnimation
            {
                To = radius.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Border.CornerRadiusProperty);
            aniGroup.Children.Add(ani);
        }

        if (translate is not null)
        {
            var ani = new NTranslateTransformFromToAnimation
            {
                To = translate.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, UIElement.RenderTransformProperty);
            aniGroup.Children.Add(ani);
        }

        if (translateX is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = translateX.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, TranslateTransform.XProperty);
            aniGroup.Children.Add(ani);
        }

        if (translateY is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = translateY.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, TranslateTransform.YProperty);
            aniGroup.Children.Add(ani);
        }

        if (rotate is not null)
        {
            var ani = new NRotateTransformFromToAnimation
            {
                To = rotate.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, UIElement.RenderTransformProperty);
            aniGroup.Children.Add(ani);
        }

        if (rotateAngle is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = rotateAngle.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, RotateTransform.AngleProperty);
            aniGroup.Children.Add(ani);
        }

        if (scale is not null)
        {
            var ani = new NScaleTransformFromToAnimation
            {
                To = scale.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, UIElement.RenderTransformProperty);
            aniGroup.Children.Add(ani);
        }

        if (scaleX is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = scaleX.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, ScaleTransform.ScaleXProperty);
            aniGroup.Children.Add(ani);
        }

        if (scaleY is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = scaleY.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, ScaleTransform.ScaleYProperty);
            aniGroup.Children.Add(ani);
        }

        if (skew is not null)
        {
            var ani = new NSkewTransformFromToAnimation
            {
                To = skew.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, UIElement.RenderTransformProperty);
            aniGroup.Children.Add(ani);
        }

        if (skewX is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = skewX.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, SkewTransform.AngleXProperty);
            aniGroup.Children.Add(ani);
        }

        if (skewY is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = skewY.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, SkewTransform.AngleYProperty);
            aniGroup.Children.Add(ani);
        }

        if (margin is not null)
        {
            var ani = new ThicknessFromToAnimation
            {
                To = margin.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.MarginProperty);
            aniGroup.Children.Add(ani);
        }

        if (marginLeft is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = marginLeft.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.MarginProperty);
            aniGroup.Children.Add(ani);
        }

        if (marginTop is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = marginTop.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.MarginProperty);
            aniGroup.Children.Add(ani);
        }

        if (marginRight is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = marginRight.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.MarginProperty);
            aniGroup.Children.Add(ani);
        }

        if (marginBottom is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = marginBottom.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, FrameworkElement.MarginProperty);
            aniGroup.Children.Add(ani);
        }

        if (padding is not null)
        {
            var ani = new ThicknessFromToAnimation
            {
                To = padding.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.PaddingProperty);
            aniGroup.Children.Add(ani);
        }

        if (paddingLeft is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = paddingLeft.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.PaddingProperty);
            aniGroup.Children.Add(ani);
        }

        if (paddingTop is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = paddingTop.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.PaddingProperty);
            aniGroup.Children.Add(ani);
        }

        if (paddingRight is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = paddingRight.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.PaddingProperty);
            aniGroup.Children.Add(ani);
        }

        if (paddingBottom is not null)
        {
            var ani = new DoubleFromToAnimation
            {
                To = paddingBottom.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.PaddingProperty);
            aniGroup.Children.Add(ani);
        }

        if (background is not null)
        {
            var ani = new NColorFromToAnimation
            {
                To = background.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.BackgroundProperty);
            aniGroup.Children.Add(ani);
        }

        if (foreground is not null)
        {
            var ani = new NColorFromToAnimation
            {
                To = foreground.Value,
                Duration = duration.Value,
                Delay = delay.Value,
                Easing = easing
            };
            SetTargetProperty(ani, Control.ForegroundProperty);
            aniGroup.Children.Add(ani);
        }

        aniGroup.RunFireAndForget(EmptyAnimatable.Instance);
    }
}