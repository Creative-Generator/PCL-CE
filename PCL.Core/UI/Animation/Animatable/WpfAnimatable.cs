using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using PCL.Core.UI.Animation.ValueProcessor;

namespace PCL.Core.UI.Animation.Animatable;

public sealed class WpfAnimatable(DependencyObject owner, DependencyProperty? property) : IAnimatable
{
    public DependencyObject Owner { get; set; } = owner;
    public DependencyProperty? Property { get; set; } = property;

    public object? GetValue()
    {
        DependencyProperty? actualProperty;

        if (Property == FrameworkElement.WidthProperty)
        {
            actualProperty = FrameworkElement.ActualWidthProperty;
        }
        else if (Property == FrameworkElement.HeightProperty)
        {
            actualProperty = FrameworkElement.ActualHeightProperty;
        }
        else
        {
            actualProperty = Property;
        }

        ArgumentNullException.ThrowIfNull(actualProperty);
        
        var value  = Owner.GetValue(actualProperty);
        return value switch
        {
            SolidColorBrush brush => (NColor)brush,
            Color color => (NColor)color,
            TranslateTransform translateTransform => (NTranslateTransform)translateTransform,
            ScaleTransform scaleTransform => (NScaleTransform)scaleTransform,
            RotateTransform rotateTransform => (NRotateTransform)rotateTransform,
            SkewTransform skewTransform => (NSkewTransform)skewTransform,
            _ => value
        };
    }

    public void SetValue(object value)
    {
        value = ValueProcessorManager.Filter(value);
        ArgumentNullException.ThrowIfNull(Property);
        
        value = value switch
        {
            NColor color => Property.Name switch
            {
                "Color" => (Color)color,
                _ => (SolidColorBrush)color
            },
            NTranslateTransform tt => (TranslateTransform)tt,
            NScaleTransform st => (ScaleTransform)st,
            NRotateTransform rt => (RotateTransform)rt,
            NSkewTransform st => (SkewTransform)st,
            _ => value
        };

        Owner.SetValue(Property, value);
    }

    public void SetValue<T>(T value)
    {
        value = ValueProcessorManager.Filter(value);
        ArgumentNullException.ThrowIfNull(Property);
        _SetValueCore(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void _SetValueCore<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(Property);
        
        if (typeof(T) == typeof(NColor))
        {
            var color = Unsafe.As<T, NColor>(ref value);

            Owner.SetValue(
                Property,
                Property.Name == "Color"
                    ? (Color)color
                    : (SolidColorBrush)color);

            return;
        }

        if (typeof(T) == typeof(NScaleTransform))
        {
            var st = Unsafe.As<T, NScaleTransform>(ref value);
            Owner.SetValue(Property, (ScaleTransform)st);
            return;
        }

        if (typeof(T) == typeof(NTranslateTransform))
        {
            var tt = Unsafe.As<T, NTranslateTransform>(ref value);
            Owner.SetValue(Property, (TranslateTransform)tt);
            return;
        }

        if (typeof(T) == typeof(NRotateTransform))
        {
            var rt = Unsafe.As<T, NRotateTransform>(ref value);
            Owner.SetValue(Property, (RotateTransform)rt);
            return;
        }

        if (typeof(T) == typeof(NSkewTransform))
        {
            var st = Unsafe.As<T, NSkewTransform>(ref value);
            Owner.SetValue(Property, (SkewTransform)st);
            return;
        }

        Owner.SetValue(Property, value!);
    }

    public override string ToString()
    {
        return $"WpfAnimatable: {Owner.GetType().Name}.{Property?.Name}";
    }
}
