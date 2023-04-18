using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bouncing_Balls.ViewModels;
using Microsoft.VisualBasic.CompilerServices;

namespace Bouncing_Balls_WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private MainWindowViewModel? _dataContext;

    private Storyboard Storyboard { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        Loaded += delegate {
            _dataContext = (MainWindowViewModel)DataContext!;
            AddBall();

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
        };
    }

    public void AddBall()
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
            Duration = new (System.TimeSpan.FromSeconds(5))
        };

        Storyboard.SetTarget(animation, ball);
        Storyboard.SetTargetProperty(animation, new(Canvas.TopProperty));
        Storyboard.Children.Add(animation);
    }
}