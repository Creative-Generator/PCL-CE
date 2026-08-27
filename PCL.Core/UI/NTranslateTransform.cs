using System;
using System.Numerics;
using System.Windows.Media;
using PCL.Core.UI.Animation.Core;

namespace PCL.Core.UI;

public struct NTranslateTransform : 
    IEquatable<NTranslateTransform>,
    IAdditionOperators<NTranslateTransform, NTranslateTransform, NTranslateTransform>,
    ISubtractionOperators<NTranslateTransform, NTranslateTransform, NTranslateTransform>,
    IMultiplyOperators<NTranslateTransform, float, NTranslateTransform>,
    IDivisionOperators<NTranslateTransform, float, NTranslateTransform>
{
    private Vector2 _translate;
    
    public float X
    {
        get => _translate.X;
        set => _translate.X = value;
    }
    
    public float Y
    {
        get => _translate.Y;
        set => _translate.Y = value;
    }

    #region 构造函数

    public NTranslateTransform()
    {
        _translate = new Vector2(0, 0);
    }
    
    public NTranslateTransform(float x, float y)
    {
        _translate = new Vector2(x, y);
    }
    
    public NTranslateTransform(TranslateTransform translateTransform)
    {
        var uiAccessProvider = AnimationService.UIAccessProvider;
        if (uiAccessProvider.CheckAccess())
        {
            _translate = GetVector(translateTransform);
        }
        else
        {
            Vector2 localTranslate = default;
            uiAccessProvider.Invoke(() => localTranslate = GetVector(translateTransform));
            _translate = localTranslate;
        }
        
        Vector2 GetVector(TranslateTransform tt)
        {
            return new Vector2((float)tt.X, (float)tt.Y);
        }
    }

    #endregion

    #region 运算符重载

    public static NTranslateTransform operator +(NTranslateTransform a, NTranslateTransform b) => new(a.X + b.X, a.Y + b.Y);
    public static NTranslateTransform operator -(NTranslateTransform a, NTranslateTransform b) => new(a.X - b.X, a.Y - b.Y);
    public static NTranslateTransform operator *(NTranslateTransform a, float scalar) => new(a.X * scalar, a.Y * scalar);
    public static NTranslateTransform operator /(NTranslateTransform a, float scalar) => 
        scalar == 0 ? throw new DivideByZeroException("除数不能为零。") : new NTranslateTransform(a.X / scalar, a.Y / scalar);
    public static bool operator ==(NTranslateTransform a, NTranslateTransform b) => a._translate == b._translate;
    public static bool operator !=(NTranslateTransform a, NTranslateTransform b) => a._translate != b._translate;
    
    #endregion

    #region IEquatable

    public bool Equals(NTranslateTransform other)
    {
        return _translate.Equals(other._translate);
    }

    public override bool Equals(object? obj)
    {
        if (obj is NTranslateTransform color)
            return Equals(color);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    #endregion

    #region 隐式转换

    public static implicit operator TranslateTransform(NTranslateTransform tt) =>
        new(tt.X, tt.Y);

    public static implicit operator NTranslateTransform(TranslateTransform tt) => new(tt);

    #endregion
}