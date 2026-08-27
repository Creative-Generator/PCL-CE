namespace PCL.Core.UI.Animation.ValueProcessor;

public class NTranslateTransformValueProcessor : IValueProcessor<NTranslateTransform>
{
    public NTranslateTransform Filter(NTranslateTransform value) => value;
    
    public NTranslateTransform Add(NTranslateTransform value1, NTranslateTransform value2) => value1 + value2;

    public NTranslateTransform Subtract(NTranslateTransform value1, NTranslateTransform value2) => value1 - value2;
    
    public NTranslateTransform Scale(NTranslateTransform value, double factor) => value * (float)factor;
    
    public NTranslateTransform DefaultValue() => new();
    
    public bool Equal(NTranslateTransform value1, NTranslateTransform value2) => value1 == value2;
}