using System.Windows;
using PCL.Core.Utils;

namespace PCL.Core.UI.Animation.ValueProcessor;

public class CornerRadiusValueProcessor : IValueProcessor<CornerRadius>
{
    public CornerRadius Filter(CornerRadius value) => value;
    
    public CornerRadius Add(CornerRadius value1, CornerRadius value2) => value1 + value2;

    public CornerRadius Subtract(CornerRadius value1, CornerRadius value2) => value1 - value2;
    
    public CornerRadius Scale(CornerRadius value, double factor) => value * factor;
    
    public CornerRadius DefaultValue() => new();
    
    public bool Equal(CornerRadius value1, CornerRadius value2) => value1 == value2;
}