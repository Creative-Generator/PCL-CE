using System.Windows;

namespace PCL.Core.UI.Animation.ValueProcessor;

public class GridLengthValueProcessor : IValueProcessor<GridLength>
{
    public GridLength Filter(GridLength value) => value;

    public GridLength Add(GridLength value1, GridLength value2)
    {
        return new GridLength(value1.Value + value2.Value, value1.GridUnitType);
    }

    public GridLength Subtract(GridLength value1, GridLength value2)
    {
        return new GridLength(value1.Value - value2.Value, value1.GridUnitType);
    }

    public GridLength Scale(GridLength value, double factor)
    {
        return new GridLength(value.Value * factor, value.GridUnitType);
    }

    public GridLength DefaultValue() => new();

    public bool Equal(GridLength value1, GridLength value2) => value1 == value2;
}