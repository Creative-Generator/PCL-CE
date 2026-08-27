using System;
using System.Numerics;
using System.Windows.Media;
using PCL.Core.UI.Animation.Core;

namespace PCL.Core.UI;

public struct NSkewTransform : 
    IEquatable<NSkewTransform>,
    IAdditionOperators<NSkewTransform, NSkewTransform, NSkewTransform>,
    ISubtractionOperators<NSkewTransform, NSkewTransform, NSkewTransform>,
    IMultiplyOperators<NSkewTransform, float, NSkewTransform>,
    IDivisionOperators<NSkewTransform, float, NSkewTransform>
{
    private Vector4 _skew;
    
    public float AngleX
    {
        get => _skew.X;
        set => _skew.X = value;
    }
    
    public float AngleY
    {
        get => _skew.Y;
        set => _skew.Y = value;
    }

    public float CenterX
    {
        get => _skew.Z;
        set => _skew.Z = value;
    }
    
    public float CenterY
    {
        get => _skew.W;
        set => _skew.W = value;
    }

    #region 构造函数

    public NSkewTransform()
    {
        _skew = new Vector4(0, 0, 0, 0);
    }
    
    public NSkewTransform(float angleX, float angleY, float centerX = 0f, float centerY = 0f)
    {
        _skew = new Vector4(angleX, angleY, centerX, centerY);
    }
    
    public NSkewTransform(SkewTransform skewTransform)
    {
        var uiAccessProvider = AnimationService.UIAccessProvider;
        if (uiAccessProvider.CheckAccess())
        {
            _skew = GetVector(skewTransform);
        }
        else
        {
            Vector4 localSkew = default;
            uiAccessProvider.Invoke(() => localSkew = GetVector(skewTransform));
            _skew = localSkew;
        }
        
        Vector4 GetVector(SkewTransform st)
        {
            return new Vector4((float)st.AngleX, (float)st.AngleY, (float)st.CenterX, (float)st.CenterY);
        }
    }

    #endregion

    #region 运算符重载

    public static NSkewTransform operator +(NSkewTransform a, NSkewTransform b) => new(a.AngleX + b.AngleX,
        a.AngleY + b.AngleY, a.CenterX + b.CenterX, a.CenterY + b.CenterY);

    public static NSkewTransform operator -(NSkewTransform a, NSkewTransform b) => new(a.AngleX - b.AngleX,
        a.AngleY - b.AngleY, a.CenterX - b.CenterX, a.CenterY - b.CenterY);

    public static NSkewTransform operator *(NSkewTransform a, float scalar) => new(a.AngleX * scalar, a.AngleY * scalar,
        a.CenterX * scalar, a.CenterY * scalar);

    public static NSkewTransform operator /(NSkewTransform a, float scalar) =>
        scalar == 0
            ? throw new DivideByZeroException("除数不能为零。")
            : new NSkewTransform(a.AngleX / scalar, a.AngleY / scalar, a.CenterX / scalar, a.CenterY / scalar);
    
    public static bool operator ==(NSkewTransform a, NSkewTransform b) => a._skew == b._skew;
    
    public static bool operator !=(NSkewTransform a, NSkewTransform b) => !(a == b);

    #endregion

    #region IEquatable

    public bool Equals(NSkewTransform other)
    {
        return _skew.Equals(other._skew);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is NSkewTransform other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(AngleX, AngleY, CenterX, CenterY);
    }

    #endregion

    #region 隐式转换

    public static implicit operator SkewTransform(NSkewTransform skew) => new(skew.AngleX, skew.AngleY, skew.CenterX, skew.CenterY);
    public static implicit operator NSkewTransform(SkewTransform skew) => new(skew);
    
    #endregion
}