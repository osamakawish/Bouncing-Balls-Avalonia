using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bouncing_Balls_WPF.Models;
using Bouncing_Balls_WPF.ViewModels;

namespace Bouncing_Balls_WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _dataContext;

    private Storyboard Storyboard { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        _dataContext = new();
        DataContext = _dataContext;

        Loaded += delegate {
            _dataContext.Ball = AddBall();
            _dataContext.Curve = AddCurve();
            _dataContext.ParametricCurve = AddParametricCurve();

            Storyboard.Begin();
        };

        Canvas.SizeChanged += (_, args) =>
        {
            var w = args.NewSize.Width / args.PreviousSize.Width;
            var h = args.NewSize.Height / args.PreviousSize.Height;

            foreach (var obj in Canvas.Children)
            {
                if (obj is not UIElement item) continue;

                Canvas.SetLeft(item, Canvas.GetLeft(item) * w);
                Canvas.SetTop(item, Canvas.GetTop(item) * h);
            }

            if (_dataContext.Curve == null) return;
            _dataContext.Curve.Width *= w;
            _dataContext.Curve.Height *= h;
        };
    }

    private ParametricCurve AddParametricCurve()
    {
        var parametricCurve = new ParametricCurve(t => new(12 * ( Math.Cos(t)), 12 * ( Math.Sin(t)))) {
            MinT = 0,
            MaxT = 2 * Math.PI,
            IsClosed = true,
            Stroke = Brushes.OrangeRed,
            StrokeThickness = 1
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

        var animation = new DoubleAnimation
        {
            From = 0.5 * Canvas.ActualHeight,
            To = 0.8 * Canvas.ActualHeight,
            Duration = new (TimeSpan.FromSeconds(5))
        };

        Storyboard.SetTarget(animation, ball);
        Storyboard.SetTargetProperty(animation, new(Canvas.TopProperty));
        Storyboard.Children.Add(animation);

        return ball;
    }
}