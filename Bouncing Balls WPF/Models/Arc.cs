using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.Models;

file static class PointExt
{
    public static Point ToCirclePoint(double angle) => new(Math.Cos(angle), -Math.Sin(angle));
}

public class Arc : Shape
{
    public static readonly DependencyProperty StartAngleProperty
        = DependencyProperty.Register($"{nameof(StartAngle)}", typeof(double), typeof(Arc));

    public static readonly DependencyProperty EndAngleProperty
        = DependencyProperty.Register($"{nameof(EndAngle)}", typeof(double), typeof(Arc));

    public double StartAngle {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double EndAngle {
        get => (double)GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }
    
    public Point EllipseRadii {
        get => new(Width / 2, Height / 2);
        set => (Width, Height) = (value.X * 2, value.Y * 2);
    }

    public Point Center {
        get => new(Canvas.GetLeft(this) + Width / 2, Canvas.GetTop(this) + Height / 2);
        set { Canvas.SetLeft(this, value.X - Width / 2); Canvas.SetTop(this, value.Y - Height / 2); }
    }

    public static Point ToCirclePoint(double angle) => new(Math.Cos(angle), -Math.Sin(angle));

    public static Geometry GetGeometry(Point center, Size ellipseRadii, double startAngle, double endAngle)
    {
        Point EllipsePoint(double angle)
        {
            var circlePoint = ToCirclePoint(angle) + new Vector(1, 1);
            return new(ellipseRadii.Width * circlePoint.X + center.X, ellipseRadii.Height * circlePoint.Y + center.Y);
        }

        var arcSegment = new ArcSegment(
            point: EllipsePoint(endAngle),
            size: ellipseRadii, 0,
            isLargeArc: Math.Abs(startAngle - endAngle) >= Math.PI,
            sweepDirection: endAngle > startAngle
                ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
            isStroked: true);

        var pathFigure = new PathFigure
        {
            StartPoint = EllipsePoint(startAngle),
            Segments = new() { arcSegment }
        };

        return new PathGeometry(new[] { pathFigure });
    }

    protected override Geometry DefiningGeometry {
        get {
            var ellipseRadii = new Point(Width / 2, Height / 2);
            Point EllipsePoint(double angle)
            {
                var circlePoint = ToCirclePoint(angle) + new Vector(1, 1);
                return new(ellipseRadii.X * circlePoint.X, ellipseRadii.Y * circlePoint.Y);
            }

            var arcSegment = new ArcSegment(
                point: EllipsePoint(EndAngle),
                size: new (ellipseRadii.X, ellipseRadii.Y), 0,
                isLargeArc: Math.Abs(StartAngle - EndAngle) >= Math.PI,
                sweepDirection: EndAngle > StartAngle
                    ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                isStroked: true);
            
            var pathFigure = new PathFigure
            {
                StartPoint = EllipsePoint(StartAngle),
                Segments = new() { arcSegment }
            };

            return new PathGeometry(new[] { pathFigure });
        }
    }

    /// <summary>
    /// Creates a semicircle.
    /// </summary>
    public Arc()
        => (StartAngle, EndAngle) = (0, Math.PI);

    /// <summary>
    /// Creates an arc given the provided angle range. By default, this angle range is from 0 to pi, creating a semicircle.
    /// </summary>
    /// <param name="startAngle"></param>
    /// <param name="endAngle"></param>
    public Arc(double startAngle=0, double endAngle=Math.PI)
        => (StartAngle, EndAngle) = (startAngle, endAngle);
}