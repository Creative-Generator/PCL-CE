namespace PCL.Core.UI.Animation.ValueProcessor;

public class NSkewTransformValueProcessor : IValueProcessor<NSkewTransform>
{
    public NSkewTransform Filter(NSkewTransform value) => value;
    
    public NSkewTransform Add(NSkewTransform value1, NSkewTransform value2) => value1 + value2;

    public NSkewTransform Subtract(NSkewTransform value1, NSkewTransform value2) => value1 - value2;
    
    public NSkewTransform Scale(NSkewTransform value, double factor) => value * (float)factor;
    
    public NSkewTransform DefaultValue() => new();
    
    public bool Equal(NSkewTransform value1, NSkewTransform value2) => value1 == value2;
}