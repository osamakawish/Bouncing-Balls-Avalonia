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
#if DEBUG
    internal Rect TestRect { get; set; }
#endif

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
    public Func<double, double> XFunc => t => ParametricFunction(t).X;
    public Func<double, double> YFunc => t => ParametricFunction(t).Y;

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

    public bool IntersectsBall(Ellipse ball, out Vector reflectionNormal)
    {
        // If the ellipse boundary does not intersect or contain the curve, return false.
        var left = Canvas.GetLeft(ball); var right = left + ball.Width;
        var top = Canvas.GetTop(ball); var bottom = top + ball.Height;
        
        // Find t values such that the curve is closest to the ellipse boundaries.
        var mean = 0.5 * (Min + Max);
        var tLeft   = FindEquals(mean, left  , XFunc);
        var tRight  = FindEquals(mean, right , XFunc);
        var tTop    = FindEquals(mean, top   , YFunc);
        var tBottom = FindEquals(mean, bottom, YFunc);

#if DEBUG
        // Just draw a rectangle to visualize the ellipse boundaries to see if they intersect the curve.
        TestRect = new(new (XFunc(tLeft), YFunc(tTop)), new Point(XFunc(tRight), YFunc(tBottom)));
#endif
        


        // Apply Newton's method to find the intersection of the curve and the ellipse.
        var xZero = FindZero(Canvas.GetLeft(ball), XFunc, out var xRateReciprocal);
        var yZero = FindZero(Canvas.GetTop(ball),  YFunc, out var yRateReciprocal);


        // If the intersection is within the bounds of the curve, return true.

        reflectionNormal = new ();
        return false;
    }

    /// <summary>
    /// Returns a zero of the function <see cref="func"/>
    /// </summary>
    /// <param name="guess"></param>
    /// <param name="func"></param>
    /// <param name="rateReciprocal"></param>
    /// <param name="halfDelta"></param>
    /// <param name="steps"></param>
    /// <returns>A value closer to a zero of <see cref="func"/> than <see cref="guess"/>, assuming no discontinuities.</returns>
    public double FindZero(double guess, Func<double, double> func, out double rateReciprocal, double halfDelta = 0.005, byte steps = 4)
    {
        rateReciprocal = 2 * halfDelta / (func(guess + halfDelta) - func(guess - halfDelta));
        if (steps != 0)
            return FindZero(
                guess: guess - func(guess) * rateReciprocal,
                func: func, out rateReciprocal, halfDelta: halfDelta, steps: (byte) (steps - 1));
        return guess;
    }

    public double FindEquals(double guess, double target, Func<double, double> func, double halfDelta = 0.005,
        byte steps = 4) 
        => FindZero(guess, t => func(t) - target, out var _, halfDelta, steps);
}