using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.Models;

/// <summary>
/// A parametric curve.
/// </summary>
public class ParametricCurve : Shape
{
#if DEBUG
    internal Rect TestRect { get; set; }
#endif

    public static readonly DependencyProperty MinTProperty = DependencyProperty.Register(
        nameof(MinT), typeof(double), typeof(ParametricCurve), new(0.0));

    public static readonly DependencyProperty MaxTProperty = DependencyProperty.Register(
        nameof(MaxT), typeof(double), typeof(ParametricCurve), new(1.0));

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
        nameof(Step), typeof(double), typeof(ParametricCurve), new(0.01));

    public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
        nameof(IsClosed), typeof(bool), typeof(ParametricCurve), new(false));

    public double MinT {
        get => (double)GetValue(MinTProperty);
        set => SetValue(MinTProperty, value);
    }

    public double MaxT {
        get => (double)GetValue(MaxTProperty);
        set => SetValue(MaxTProperty, value);
    }
    
    public Rect Bounds { get; private set; } = Rect.Empty;

    public double Step {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public bool IsClosed {
        get => (bool)GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    public ParametricFunction Parametric { get; set; }

    public ParametricFunction Derivative(double halfDelta = 0.005)
        => t => (Parametric(t + halfDelta) - Parametric(t - halfDelta)) / (2 * halfDelta);

    public Func<double, double> XFunc => t => Parametric(t).X;
    public Func<double, double> YFunc => t => Parametric(t).Y;

    // Include width and height in this calculation.
    protected override Geometry DefiningGeometry {
        get {
            Bounds = new(new(XFunc(MinT), YFunc(MinT)), new Vector(0, 0));

            var polyline = new PathFigure {
                StartPoint = new(Parametric(MinT).X, Parametric(MinT).Y),
                Segments = new() { new PolyLineSegment(PolylinePoints(MinT), true) },
                IsClosed = IsClosed
            };

            IEnumerable<Point> PolylinePoints(double start)
            {
                while (start <= MaxT) {
                    var x = XFunc(start); var y = YFunc(start);
                    if (x < Bounds.Left)
                        Bounds = new(new(x, Bounds.Top), new Point(Bounds.Right, Bounds.Bottom));
                    else if (x > Bounds.Right)
                        Bounds = new(new(Bounds.Left, Bounds.Top), new Point(x, Bounds.Bottom));
                    if (y < Bounds.Top)
                        Bounds = new(new(Bounds.Left, y), new Point(Bounds.Right, Bounds.Bottom));
                    else if (y > Bounds.Bottom)
                        Bounds = new(new(Bounds.Left, Bounds.Top), new Point(Bounds.Right, y));

                    yield return new(x, y);
                    start += Step;
                }
            }

            var xScale = Width / Bounds.Width;
            var yScale = Height / Bounds.Height;

            return new PathGeometry(new[] { polyline }) {
                
                Transform = new TransformGroup {
                    Children = {
                        new ScaleTransform(xScale, yScale, Bounds.Left, Bounds.Top),
                        new TranslateTransform(-Bounds.Left, -Bounds.Top)
                    }
                }
            };
        }
    }

    public ParametricCurve(ParametricFunction function) => Parametric = function;

    public bool IntersectsBall(Ellipse ball, out Vector reflectionNormal)
    {
        // If the ellipse boundary does not intersect or contain the curve, return false.
        var left = Canvas.GetLeft(ball);
        var right = left + ball.Width;
        var top = Canvas.GetTop(ball);
        var bottom = top + ball.Height;

        // Find t values such that the curve is closest to the ellipse boundaries.
        var mean = 0.5 * (MinT + MaxT);
        var tLeft = FindEquals(mean, left, XFunc);
        var tRight = FindEquals(mean, right, XFunc);
        var tTop = FindEquals(mean, top, YFunc);
        var tBottom = FindEquals(mean, bottom, YFunc);

#if DEBUG
        // Just draw a rectangle to visualize the ellipse boundaries to see if they intersect the curve.
        TestRect = new(new(XFunc(tLeft), YFunc(tTop)), new Point(XFunc(tRight), YFunc(tBottom)));
#endif


        // Apply Newton's method to find the intersection of the curve and the ellipse.
        var xZero = FindZero(Canvas.GetLeft(ball), XFunc, out var xRateReciprocal);
        var yZero = FindZero(Canvas.GetTop(ball), YFunc, out var yRateReciprocal);


        // If the intersection is within the bounds of the curve, return true.

        reflectionNormal = new();
        return false;
    }

    /// <summary>
    /// Returns a zero of the function <see cref="func"/>
    /// </summary>
    /// <param name="guess"></param>
    /// <param name="func"></param>
    /// <param name="rateReciprocal"></param>
    /// <param name="halfDelta"></param>
    /// <param name="steps">The number of steps to apply the iteration. 8 by default, as it is fairly good at achieving a close result.</param>
    /// <returns>A value closer to a zero of <see cref="func"/> than <see cref="guess"/>, assuming no discontinuities.</returns>
    public double FindZero(double guess, Func<double, double> func, out double rateReciprocal, double halfDelta = 0.005,
        byte steps = 8)
    {
        // May prefer a condition parameter.
        while (true) {
            rateReciprocal = 2 * halfDelta / (func(guess + halfDelta) - func(guess - halfDelta));

            if (steps == 0) return guess;
            guess -= func(guess) * rateReciprocal;
            steps -= 1;
        }
    }

    public double FindEquals(double guess, double target, Func<double, double> func, double halfDelta = 0.005,
        byte steps = 8)
        => FindZero(guess, t => func(t) - target, out _, halfDelta, steps);
}