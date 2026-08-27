using System;
using System.Windows;

namespace PCL.Core.Utils;

public static class OperatorExtensions
{
    extension(CornerRadius)
    {
        public static CornerRadius operator +(CornerRadius a, CornerRadius b) => new CornerRadius(
            a.TopLeft + b.TopLeft,
            a.TopRight + b.TopRight,
            a.BottomRight + b.BottomRight,
            a.BottomLeft + b.BottomLeft);

        public static CornerRadius operator -(CornerRadius a, CornerRadius b) => new CornerRadius(
            a.TopLeft - b.TopLeft,
            a.TopRight - b.TopRight,
            a.BottomRight - b.BottomRight,
            a.BottomLeft - b.BottomLeft);

        public static CornerRadius operator *(CornerRadius a, double b) => new CornerRadius(
            a.TopLeft * b,
            a.TopRight * b,
            a.BottomRight * b,
            a.BottomLeft * b);

        public static CornerRadius operator /(CornerRadius a, double b) =>
            b == 0
                ? throw new DivideByZeroException("除数不能为零。")
                : new CornerRadius(
                    a.TopLeft / b,
                    a.TopRight / b,
                    a.BottomRight / b,
                    a.BottomLeft / b);
    }
}