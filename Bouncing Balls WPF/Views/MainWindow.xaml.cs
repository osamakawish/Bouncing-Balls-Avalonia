using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Bouncing_Balls.ViewModels;

namespace Bouncing_Balls_WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private MainWindowViewModel? _dataContext;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += delegate {
            _dataContext = (MainWindowViewModel)DataContext!;
            AddBall();
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

        Canvas.SetLeft(ball, 0.5 * Canvas.ActualWidth); Canvas.SetTop(ball, 0.333 * Canvas.ActualHeight);
        Canvas.Children.Add(ball);

        Canvas.SizeChanged += (_, args) =>
        {
            var w = args.NewSize.Width  / args.PreviousSize.Width ;
            var h = args.NewSize.Height / args.PreviousSize.Height;
            
            Canvas.SetLeft(ball, Canvas.GetLeft(ball) * w);
            Canvas.SetTop (ball, Canvas.GetTop (ball) * h);
        };
    }
}