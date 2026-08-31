using System;

namespace PCL.Core.UI.Animation.Animatable;

public class CustomAnimatable<T> : IAnimatable
{
    public required Action<T> SetValueAction { get; init; }
    public required Func<T> GetValueAction { get; init; }
    
    public T GetValue() => GetValueAction();

    public void SetValue(T value) => SetValueAction(value);

    object? IAnimatable.GetValue() => GetValue();
    void IAnimatable.SetValue(object? value) => SetValue((T)value!);
    void IAnimatable.SetValue<TValue>(TValue value) => SetValue((T)(object)value!);
}
