using System;
using System.Windows;
using System.Windows.Media;

namespace PCL.Core.Utils;

public static class ControlHelper
{
    public static void OffsetX(FrameworkElement element, double offset)
    {
        if (element is Window window)
        {
            window.Left += offset;
        }
        else
        {
            element.Margin = GetOffsetMarginX(element, offset);
        }
    }

    public static void OffsetY(FrameworkElement element, double offset)
    {
        if (element is Window window)
        {
            window.Top += offset;
        }
        else
        {
            element.Margin = GetOffsetMarginY(element, offset);
        }
    }

    public static Thickness GetOffsetMarginX(FrameworkElement element, double offset)
    {
        return element.HorizontalAlignment switch
        {
            HorizontalAlignment.Left or HorizontalAlignment.Stretch => new Thickness(element.Margin.Left + offset,
                element.Margin.Top, element.Margin.Right, element.Margin.Bottom),
            HorizontalAlignment.Right => new Thickness(element.Margin.Left, element.Margin.Top,
                element.Margin.Right - offset, element.Margin.Bottom),
            _ => element.Margin
        };
    }

    public static Thickness GetOffsetMarginY(FrameworkElement element, double offset)
    {
        return element.VerticalAlignment switch
        {
            VerticalAlignment.Top => new Thickness(element.Margin.Left, element.Margin.Top + offset,
                element.Margin.Right, element.Margin.Bottom),
            VerticalAlignment.Bottom => new Thickness(element.Margin.Left, element.Margin.Top, element.Margin.Right,
                element.Margin.Bottom - offset),
            _ => element.Margin
        };
    }
    
    public static TTransform GetOrCreateTransform<TTransform>(DependencyObject target, bool setDefaultOrigin = false)
        where TTransform : Transform
    {
        if (target is not UIElement element)
            throw new ArgumentException("Transform 动画目标必须是 UIElement。", nameof(target));

        if (element.RenderTransform is not TTransform transform)
        {
            transform = typeof(TTransform) switch
            {
                var type when type == typeof(TranslateTransform) => (TTransform)(Transform)new TranslateTransform(),
                var type when type == typeof(RotateTransform) => (TTransform)(Transform)new RotateTransform(),
                var type when type == typeof(ScaleTransform) => (TTransform)(Transform)new ScaleTransform(),
                var type when type == typeof(SkewTransform) => (TTransform)(Transform)new SkewTransform(),
                _ => throw new NotSupportedException($"不支持的 Transform 类型：{typeof(TTransform).Name}")
            };
            element.RenderTransform = transform;
        }

        if (setDefaultOrigin && target is FrameworkElement frameworkElement)
            frameworkElement.RenderTransformOrigin = new Point(0.5, 0.5);

        return transform;
    }
}