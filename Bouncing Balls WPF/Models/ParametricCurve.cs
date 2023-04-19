using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.Models;

public delegate Vector ParametricFunction(double t);

public class ParametricCurve : Shape
{
    public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
        nameof(Min), typeof(double), typeof(ParametricCurve), typeMetadata: new (defaultValue: 0));

    public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
        nameof(Max), typeof(double), typeof(ParametricCurve), typeMetadata: new (defaultValue: 1));

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
        nameof(Step), typeof(double), typeof(ParametricCurve), typeMetadata: new (defaultValue: 0.01));

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }
    public ParametricFunction ParametricFunction { get; }

    protected override Geometry DefiningGeometry
    {
        get
        {
            var polyline = new PathFigure
            {
                StartPoint = new (ParametricFunction(Min).X, ParametricFunction(Min).Y),
                Segments = new () { new PolyLineSegment(PolylinePoints(Min), true) },
                IsClosed = false,
            };

            IEnumerable<Point> PolylinePoints(double start)
            {
                if (start <= Max)
                    yield return new (ParametricFunction(start).X, ParametricFunction(start).Y);
            }

            return new PathGeometry(new [] { polyline });
        }
    }

    public ParametricCurve (ParametricFunction function) => ParametricFunction = function;

    public bool IntersectsEllipse(Ellipse ellipse, out Vector reflectionNormal)
    {
        // If the ellipse boundary does not intersect or contain the curve, return false.
        var left = Canvas.GetLeft(ellipse); var right = left + ellipse.Width;
        var top = Canvas.GetTop(ellipse); var bottom = top + ellipse.Height;
        // Find t such that the curve is closest to the ellipse boundary.
        var tLeft = FindZero(left,  t => ParametricFunction(t).X, out var _);

        // Apply Newton's method to find the intersection of the curve and the ellipse.
        var xZero = FindZero(Canvas.GetLeft(ellipse), t => ParametricFunction(t).X, out var normalXSlope);
        var yZero = FindZero(Canvas.GetTop(ellipse),  t => ParametricFunction(t).Y, out var normalYSlope);


        // If the intersection is within the bounds of the curve, return true.

        reflectionNormal = new ();
        return false;
    }

    public double FindZero(double start, Func<double, double> func, out double normalSlope, double halfDelta = 0.005, byte steps = 4)
    {
        if (steps != 0)
            return FindZero(
                start: start - func(start) * 2 * halfDelta / (func(start + halfDelta) - func(start - halfDelta)),
                func: func, out normalSlope, halfDelta: halfDelta, steps: (byte) (steps - 1));
        
        normalSlope = 2 * halfDelta / (func(start + halfDelta) - func(start - halfDelta));
        return start;
    }
}