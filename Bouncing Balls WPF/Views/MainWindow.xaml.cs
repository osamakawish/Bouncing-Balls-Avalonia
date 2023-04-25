﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    private bool _isHit;

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
            var geo = ball.RenderedGeometry;
            geo.Transform = new TranslateTransform(Canvas.GetLeft(ball), Canvas.GetTop(ball));

            var detail = _dataContext.Curve?.RenderedGeometry.StrokeContainsWithDetail(
                new(Brushes.Black, _dataContext.Curve.StrokeThickness), geo);

            switch (detail)
            {
                case > IntersectionDetail.Empty when !_isHit:
                    var points = _dataContext.Curve?
                        .RenderedGeometry
                        .GetIntersectionPoints(ball.RenderedGeometry);

                    if (points != null) Bounce(points, ball);
                    _isHit = true;
                    break;

                case IntersectionDetail.Empty:
                    _isHit = false;
                    _dataContext.NormalForce = new (0, 0);
                    break;
            }

            _dataContext.Velocity += _dataContext.Gravity + _dataContext.NormalForce;
            Canvas.SetLeft(ball, Canvas.GetLeft(ball) + _dataContext.Velocity.X);
            Canvas.SetTop(ball, Canvas.GetTop(ball) + _dataContext.Velocity.Y);
        });
        timer.Start();
        Closing += (_, _) => timer.Stop();

        return ball;
    }

    private void Bounce(Point[] points, Ellipse ball)
    {
        // Try to compute normal force on each point instead.
        // Or try to compute velocity on each point instead, then sum the velocities.

        var xMean = points.Average(p => p.X);
        var yMean = points.Average(p => p.Y);
        var point = new Point(xMean, yMean);

        var ballCenter = ball.Center();
        var bounceDirection = ballCenter - point;
        var bounceDirectionLength = bounceDirection.Length;

        //_dataContext.NormalForce = (_dataContext.Gravity * bounceDirection / bounceDirection.LengthSquared) * bounceDirection;

        // Revise ball position first.
        //var ballRadius = 0.5 * ball.Width;
        //var ballRadiusVector = bounceDirection * (ballRadius / bounceDirectionLength);
        //var ballPositionDifference = -bounceDirection + ballRadiusVector;
        //Canvas.SetLeft(ball, Canvas.GetLeft(ball) + ballPositionDifference.X);
        //Canvas.SetTop(ball, Canvas.GetTop(ball) + ballPositionDifference.Y);

        var velocityLengthRatio = _dataContext.Velocity.Length / bounceDirectionLength;
        var velocity = bounceDirection * velocityLengthRatio;
        _dataContext.Velocity = velocity;
    }
}