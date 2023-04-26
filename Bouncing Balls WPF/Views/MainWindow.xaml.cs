using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Bouncing_Balls_WPF.Models;
using Bouncing_Balls_WPF.ViewModels;
using Path = System.Windows.Shapes.Path;

namespace Bouncing_Balls_WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _dataContext;
    private bool _isHit;

    private readonly StringBuilder _log = new();

    public MainWindow()
    {
        InitializeComponent();

        _dataContext = new();
        DataContext = _dataContext;

        Loaded += delegate {
            _dataContext.Ball = AddBall();
            _dataContext.Curve = AddCurve();
            //_dataContext.ParametricCurve = AddParametricCurve();
        };

        Canvas.SizeChanged += (_, args) =>
        {
            var w = args.NewSize.Width / args.PreviousSize.Width;
            var h = args.NewSize.Height / args.PreviousSize.Height;

            if (_dataContext.Curve is null) return;
            _dataContext.Curve.Width *= w;
            _dataContext.Curve.Height *= h;

            foreach (var obj in Canvas.Children) {
                if (obj is not UIElement item) continue;

                Canvas.SetLeft(item, Canvas.GetLeft(item) * w);
                Canvas.SetTop(item, Canvas.GetTop(item) * h);
            }

            _dataContext.Curve.Width *= w;
            _dataContext.Curve.Height *= h;
        };
        
    }

    private ParametricCurve AddParametricCurve()
    {
        const int size = 150;
        var parametricCurve = new ParametricCurve(t => new(Math.Cos(t), Math.Sin(t))) {
            MinT = 0,
            MaxT = 2 * Math.PI,
            IsClosed = true,
            Stroke = Brushes.OrangeRed,
            StrokeThickness = 1,
            Width = size,
            Height = size
        };

        Canvas.SetLeft(parametricCurve, 0.5 * Canvas.ActualWidth);
        Canvas.SetTop(parametricCurve, 0.5 * Canvas.ActualHeight);

        Canvas.Children.Add(parametricCurve);

        return parametricCurve;
    }

    private Path AddCurve()
    {
        var arcSegment = new ArcSegment
        {
            Point = new (Canvas.ActualWidth, 0),
            Size = new (Canvas.ActualWidth, 2500),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Counterclockwise
        };
        var path = new Path
        {
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            Data = new PathGeometry(new [] { new PathFigure(
                    new (0, 0),
                    new List<PathSegment> { arcSegment }, false) })
        };

        Canvas.Children.Add(path);
        return path;
    }

    private Ellipse AddBall()
    {
        var ball = new Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = Brushes.Red
        };

        Canvas.SetLeft(ball, 0.5 * Canvas.ActualWidth);
        Canvas.SetTop(ball, 0.5 * Canvas.ActualHeight);

        Canvas.Children.Add(ball);

        Timer timer = new(1.0 / 24);
        timer.Elapsed += (_, _) => Dispatcher.Invoke(() =>
        {
            var ballGeometry = ball.RenderedGeometry;
            ballGeometry.Transform = new TranslateTransform(Canvas.GetLeft(ball), Canvas.GetTop(ball));

            var curveGeometry = _dataContext.Curve?.RenderedGeometry;
            if (_dataContext.Curve != null) {
                var detail = curveGeometry?.StrokeContainsWithDetail(
                    new(Brushes.Black, _dataContext.Curve.StrokeThickness), ballGeometry);

                switch (detail)
                {
                    case > IntersectionDetail.Empty when !_isHit:
                        var points = curveGeometry?.GetIntersectionPoints(ball.RenderedGeometry)
                            .Where(p => curveGeometry.StrokeContainsWithDetail(new(Brushes.Black, 1.0),
                                new EllipseGeometry(new Point(Canvas.GetLeft(ball), Canvas.GetTop(ball)) + (Vector)p, 4.0, 4.0))
                                        > IntersectionDetail.Empty)
                            .ToArray();

                        if (points != null) Bounce(points, ball);
                        _isHit = true;
                        _log.AppendLine();
                        break;

                    case IntersectionDetail.Empty:
                        _isHit = false;
                        _dataContext.NormalForce = new (0, 0);
                        break;
                }
            }

            _dataContext.Velocity += _dataContext.Gravity + _dataContext.NormalForce;
            Canvas.SetLeft(ball, Canvas.GetLeft(ball) + _dataContext.Velocity.X);
            Canvas.SetTop(ball, Canvas.GetTop(ball) + _dataContext.Velocity.Y);
        });
        timer.Start();
        Closing += (_, _) =>
        {
            File.WriteAllText("log.txt", _log.ToString());
            timer.Stop();
        };

        return ball;
    }

    private void Bounce(Point[] points, Ellipse ball)
    {
#if DEBUG
        _log.AppendLine($"Points: {string.Join(" | ", points.Select(p => $"({p.X:f}, {p.Y:f})"))}");
        points.ToList().ForEach(p =>
        {
            Ellipse ellipse = new()
            {
                Width = 4,
                Height = 4,
                Fill = Brushes.LightCoral,
                Opacity = 0.6
            };

            Canvas.SetLeft(ellipse, Canvas.GetLeft(ball) + p.X);
            Canvas.SetTop(ellipse, Canvas.GetTop(ball) + p.Y);

            Canvas.Children.Add(ellipse);
        });
#endif

        // Try to compute normal force on each point instead.
        // Or try to compute velocity on each point instead, then sum the velocities.
        var xMean = points.Average(p => p.X);
        var yMean = points.Average(p => p.Y);
        var point = new Point(xMean, yMean);

        var ballCenter = ball.Center();
        var bounceDirection = ballCenter - point;
        var bounceDirectionLength = bounceDirection.Length;

        _dataContext.NormalForce = (_dataContext.Gravity * bounceDirection / bounceDirection.LengthSquared) * bounceDirection;
        _log.AppendLine($"Normal force: {_dataContext.NormalForce} [{_dataContext.NormalForce.Length}]");

        // Revise ball position first.
        //var ballRadius = 0.5 * ball.Width;
        //var ballRadiusVector = bounceDirection * (ballRadius / bounceDirectionLength);
        //_log.AppendLine($"{bounceDirection.Length:f2} | {ballRadiusVector.Length:f2}");
        //var ballPositionDifference = -bounceDirection + ballRadiusVector;
        //Canvas.SetLeft(ball, Canvas.GetLeft(ball) + ballPositionDifference.X);
        //Canvas.SetTop(ball, Canvas.GetTop(ball) + ballPositionDifference.Y);

        var velocityLengthRatio = _dataContext.Velocity.Length / bounceDirectionLength;
        var velocity = bounceDirection * velocityLengthRatio;
        _dataContext.Velocity = velocity;
    }

    private void CreateTrackerPoint(Point point)
    {
        Ellipse ellipse = new()
        {
            Width = 4,
            Height = 4,
            Fill = Brushes.LightCoral,
            Opacity = 0.6
        };

        Canvas.SetLeft(ellipse, point.X - 0.5 * ellipse.Width);
        Canvas.SetTop(ellipse, point.Y - 0.5 * ellipse.Height);

        Canvas.Children.Add(ellipse);
    }
}