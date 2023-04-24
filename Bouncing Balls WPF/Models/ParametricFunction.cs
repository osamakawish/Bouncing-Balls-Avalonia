using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.Models;

public delegate Vector ParametricFunction(double t);

public static class ParametricFunctionExt
{
    public static ParametricFunction Derivative(this ParametricFunction parametric, double halfDelta = 0.005)
        => t => (parametric(t + halfDelta) - parametric(t - halfDelta)) / (2 * halfDelta);

    public static ParametricFunction NthDerivative(this ParametricFunction parametric, byte n = 1,
        double halfDelta = 0.005)
        => n switch {
            0 => parametric,
            1 => parametric.Derivative(halfDelta),
            _ => parametric.Derivative(halfDelta).NthDerivative((byte)(n - 1), halfDelta)
        };

    /// <summary>
    /// A parametric function which stores the normal/perpendicular vector (in the counter-clockwise direction) at every point on the curve.
    /// </summary>
    /// <param name="parametric"></param>
    /// <param name="halfDelta"></param>
    public static ParametricFunction Normal(this ParametricFunction parametric, double halfDelta = 0.05)
    {
        var derivative = parametric.Derivative(halfDelta);
        return t => new(-derivative(t).Y, derivative(t).X);
    }

    public static double FindCriticalPoint(this ParametricFunction parametric, byte steps = 8)
        => throw new NotImplementedException();

    public static double FindZero(this ParametricFunction parametric, byte steps = 8)
        => throw new NotImplementedException();
}

public static class GeometryExt
{
    public static Point[] GetIntersectionPoints(this Geometry g1, Geometry g2)
    {
        Geometry og1 = g1.GetWidenedPathGeometry(new(Brushes.Black, 1.0));
        Geometry og2 = g2.GetWidenedPathGeometry(new(Brushes.Black, 1.0));

        var cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);

        var pg = cg.GetFlattenedPathGeometry();
        var result = new Point[pg.Figures.Count];

        for (var i = 0; i < pg.Figures.Count; i++) {
            var fig = new PathGeometry(new[] { pg.Figures[i] }).Bounds;
            result[i] = new(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
        }

        return result;
    }

    public static Point Center(this Ellipse ellipse)
        => new(ellipse.Width / 2.0, ellipse.Height / 2.0);
}